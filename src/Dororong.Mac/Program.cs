using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using Dororong.Portable;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Dororong.Portable.Desktop;
using Dororong.Mac.Native;

namespace Dororong.Mac;

internal static class Program
{
    internal static bool MotionPreview { get; private set; }
    internal static bool DesktopPreview { get; private set; }
    internal static bool DesktopMode { get; private set; }
    [STAThread] public static int Main(string[] args)
    {
        MotionPreview = args.Contains("--motion-preview");
        DesktopPreview = args.Contains("--desktop-preview");
        DesktopMode = DesktopPreview || LaunchesPet(args,MacLoginService.IsBundledApp());
        if(DesktopMode&&!OperatingSystem.IsMacOS())
        {Console.WriteLine("--desktop-preview requires macOS; use --motion-preview on Windows.");return 2;}
        if (!args.Contains("--preview") && !MotionPreview && !DesktopMode)
        {
            Console.WriteLine("macOS parity probe only, not the complete pet. Use --preview, --motion-preview, or --desktop-preview (macOS only). See docs/specs/2026-10-15-macos-parity.md.");
            return 2;
        }
        SingleInstanceLease? lease=null;
        try
        {
            if(DesktopMode)
            {
                lease=SingleInstanceLease.TryAcquire(SingleInstanceLease.UserName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
                if(lease is null){Console.WriteLine("도로롱이 이미 실행 중입니다.");return 0;}
            }
            return AppBuilder.Configure<ProbeApp>().UsePlatformDetect().StartWithClassicDesktopLifetime(args);
        }
        catch(Exception ex){Console.Error.WriteLine("시작 실패: "+ex.GetType().Name);return 1;}
        finally{lease?.Dispose();}
    }
    internal static bool LaunchesPet(string[] args,bool bundled)=>bundled&&args.Length==0;
}

public sealed class ProbeApp : Application
{
    public override void Initialize() => Styles.Add(new FluentTheme());
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if(Program.DesktopMode)
            {
                // No MainWindow: the pet and status item own the normal session.
                // Closing a popup or temporarily hiding the pet must not exit it.
                desktop.ShutdownMode=ShutdownMode.OnExplicitShutdown;
                DesktopSession? session=null;
                desktop.Startup+=(_,_)=>
                {
                    session=new DesktopSession(Program.DesktopPreview,()=>desktop.Shutdown());
                    session.Start();
                };
                desktop.Exit+=(_,_)=>session?.Dispose();
                desktop.ShutdownRequested+=(_,_)=>session?.Dispose();
            }
            else desktop.MainWindow=Program.MotionPreview ? new MotionPreviewWindow() : new ProbeWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}

// Intentional host/renderer probe. No fake/disabled parity menus or legacy walk substitute.
internal sealed class ProbeWindow : Window
{
    private readonly LocomotionBank bank;
    private readonly Image image;
    private readonly WriteableBitmap sitFrame = new(new PixelSize(96, 96), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
    private readonly WriteableBitmap standing = new(new PixelSize(96, 96), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Unpremul);
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly Dororong.Portable.Rendering.LocomotionPresentation locomotion = new();
    private double amount;
    private bool sitting;
    private TimeSpan last;

    internal ProbeWindow()
    {
        Title = "Dororong — Mac renderer probe (not a release)";
        Width = Height = 144;
        CanResize = false; ShowInTaskbar = false; ShowActivated = false; Topmost = true;
        SystemDecorations = SystemDecorations.None;
        TransparencyLevelHint = [WindowTransparencyLevel.Transparent];
        Background = Brushes.Transparent;
        using (var stream = Resource("Dororong.locomotion")) bank = LocomotionBank.Load(stream);
        using (var stream = Resource("Dororong.canonical"))
        using (var decoded = WriteableBitmap.Decode(stream))
        {
            if (decoded.PixelSize != new PixelSize(96, 96)) throw new InvalidDataException("Canonical geometry changed.");
            var bytes = new byte[LocomotionBank.FrameBytes];
            // Normalize decoder format and alpha before applying the exact cleanup.
            using var destination = standing.Lock();
            decoded.CopyPixels(destination, AlphaFormat.Unpremul);
            for (var y = 0; y < 96; y++) Marshal.Copy(destination.Address + y * destination.RowBytes, bytes, y * 384, 384);
            var cleaned = CanonicalPixels.Clean(bytes);
            for (var y = 0; y < 96; y++) Marshal.Copy(cleaned, y * 384, destination.Address + y * destination.RowBytes, 384);
        }
        image = new Image { Source = standing, Width = 96, Height = 96, Stretch = Stretch.None };
        RenderOptions.SetBitmapInterpolationMode(image, BitmapInterpolationMode.HighQuality);
        Content = image;
        var toggle = new MenuItem { Header = "앉기/일어서기 — 렌더링 검사" };
        toggle.Click += (_, _) => sitting = !sitting;
        var exit = new MenuItem { Header = "검사 종료" }; exit.Click += (_, _) => Close();
        image.ContextMenu = new ContextMenu { ItemsSource = new[] { toggle, exit } };
        image.PointerPressed += (_, e) => { if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e); };
        timer.Tick += Tick;
        Opened += (_, _) => { last = clock.Elapsed; timer.Start(); };
        Closed += (_, _) => { timer.Stop(); timer.Tick -= Tick; image.Source = null; standing.Dispose(); sitFrame.Dispose(); };
    }

    private void Tick(object? sender, EventArgs args)
    {
        var now = clock.Elapsed; var dt = Math.Clamp((now - last).TotalSeconds, 0, .05); last = now;
        locomotion.Advance(dt*1000,false,0,false,sitting);
        Present(locomotion.Sit,locomotion.UsesRisePose);
    }

    internal void Present(double progress, bool rising = false)
    {
        amount = progress;
        if (amount == 0) { image.Source = standing; return; }
        var bytes = bank.Sit(amount, rising: rising).ToArray();
        using (var buffer = sitFrame.Lock())
            for (var y = 0; y < 96; y++) Marshal.Copy(bytes, y * 384, buffer.Address + y * buffer.RowBytes, 384);
        image.Source = sitFrame;
        image.InvalidateVisual();
    }

    private static Stream Resource(string name) => typeof(ProbeWindow).Assembly.GetManifestResourceStream(name)
        ?? throw new InvalidDataException($"Missing embedded asset: {name}");
}

using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Mac.Native;
using Dororong.Mac.Product;
using Dororong.Portable.Desktop;
using Dororong.Portable.Rendering;
using Dororong.Mac.Rendering;

namespace Dororong.Mac;

// Normal sessions have no control/inspection window, even for one startup frame.
// Explicit --desktop-preview retains diagnostics; fatal errors have a small exit UI.
internal sealed class DesktopSession : IDisposable
{
    private QuartzDesktop? nativeInstance;
    private QuartzDesktop native=>nativeInstance??=new();
    private readonly SceneCache scenes=new(background:true);
    private readonly Window petWindow=Overlay(408);
    private readonly Window jewelWindow=Overlay(32);
    private readonly R104Surface petImage=new(){Width=408,Height=408};
    private long displayedRevision=-1;
    private readonly WindowUpdateState petUpdates=new(),jewelUpdates=new();
    private double displayedDensity;
    private SizeSettings? sizeSettings;
    private int savedSize;
    private TimeSpan sizeChangedAt;
    private readonly Bitmap jewel;
    private readonly TextBlock status=new(){Text="시작 중",TextWrapping=TextWrapping.Wrap,Margin=new Thickness(12)};
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(16)};
    private readonly Stopwatch clock=Stopwatch.StartNew();
    private R104DesktopController? controller;
    private IReadOnlyList<DesktopMonitor> monitors=[];
    private nint petHandle,jewelHandle;
    private TimeSpan last,lastMonitorRead;
    private bool disposed,contextOpen;
    private readonly PetMenuHost menu=new();
    private readonly StatusMenuHost statusMenu=new();
    private StatusItem? statusItem;
    private readonly LoginStartup startup=new(new MacLoginService());
    private readonly string dataDirectory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Library","Application Support","JOEWRKS","Dororong");
    private BehaviorSettings? settings;
    private string? lastError;
    private readonly Window? inspection;
    internal bool HasInspectionWindow=>inspection is not null;
    internal bool HasVisibleOverlay=>petWindow.IsVisible||jewelWindow.IsVisible;
    private Window? errorWindow;
    private readonly Action quit;
    private readonly bool boundaryDiagnostics;
    private BoundaryDiagnostic? boundaryLog;
    internal DesktopSession(bool diagnostics,Action exitApplication,bool boundaryDiagnostics=false)
    {
        this.boundaryDiagnostics=boundaryDiagnostics;
        quit=()=>{if(disposed)return;Dispose();exitApplication();};
        if(diagnostics)
        {
        inspection=new Window{Title="Dororong — macOS desktop integration preview (not a release)",Width=480,Height=300};
        var controls=new StackPanel();controls.Children.Add(new TextBlock{Text="개발 검사판 · 실제 Mac 수락 전 / 화면 바닥 걷기·창에 걸치기",Margin=new Thickness(12),TextWrapping=TextWrapping.Wrap});
        void Button(string text,Action action){var button=new Button{Content=text,Margin=new Thickness(12,2)};button.Click+=(_,_)=>action();controls.Children.Add(button);}
        Button("간식 주기 / 취소",ToggleSnack);Button("앉기 / 일어서기",()=>controller?.ToggleSit());Button("자동 눕기 켜기 / 끄기",()=>SetSleep(!(controller?.AutomaticSleep??true)));Button("메뉴 막대 메뉴 열기",()=>ShowMenu(true));Button("발판에서 내려오기",()=>controller?.Drop());Button("검사 종료",quit);
        controls.Children.Add(status);inspection.Content=controls;
        inspection.Closed+=(_,_)=>{if(!disposed)quit();};
        inspection.Deactivated+=(_,_)=>menu.Close();
        }
        petWindow.Content=petImage;
        RenderOptions.SetBitmapInterpolationMode(petImage,BitmapInterpolationMode.None);
        using(var stream=typeof(DesktopSession).Assembly.GetManifestResourceStream("Dororong.jewel")!)jewel=new Bitmap(stream);
        jewelWindow.Content=new Image{Source=jewel,Width=32,Height=32,IsHitTestVisible=false};
        void MenuState(){contextOpen=menu.IsOpen||statusMenu.IsOpen;if(controller is not null)controller.MenuOpen=contextOpen;}
        menu.OpenChanged+=_=>MenuState();statusMenu.OpenChanged+=open=>{statusItem?.Highlight(open);MenuState();};
        statusMenu.Failed+=e=>ReportError("메뉴 배치",e);
        petWindow.Deactivated+=(_,_)=>menu.Close();
        petImage.PointerPressed+=(_,e)=>
        {
            if(e.GetCurrentPoint(petImage).Properties.IsRightButtonPressed){ShowMenu(false);e.Handled=true;return;}
            if(statusMenu.IsOpen){statusMenu.Close();e.Handled=true;return;}
            if(controller is null||!e.GetCurrentPoint(petImage).Properties.IsLeftButtonPressed)return;
            var pointer=native.Pointer();if(pointer.IsAvailable)controller.TryBeginInteraction(pointer.Position);
        };
        timer.Tick+=Tick;
    }
    public void Dispose()
    {
            if(disposed)return;
            if(sizeChangedAt!=TimeSpan.Zero){try{sizeSettings?.Save(savedSize);}catch(Exception e){ReportError("크기 저장",e);}}
            disposed=true;timer.Stop();timer.Tick-=Tick;boundaryLog?.Dispose();menu.Dispose();statusMenu.Dispose();statusItem?.Dispose();statusItem=null;controller?.Dispose();
            jewelWindow.Content=null;petWindow.Close();jewelWindow.Close();petImage.Dispose();jewel.Dispose();
            inspection?.Close();errorWindow?.Close();
    }
    internal void Start()
    {
        inspection?.Show();
        try
        {
            settings=new BehaviorSettings(Path.Combine(dataDirectory,"settings.json"));
            var sleepEnabled=settings.AutomaticSleepEnabled;
            sizeSettings=new(Path.Combine(dataDirectory,"size.json"));savedSize=sizeSettings.Read();
            monitors=native.Monitors();scenes.Read(clock.Elapsed,native.Capture,monitors);
            var main=monitors[0];var frame=DesktopFrames.Place(MotionRenderer.Standing(),false);
            var contact=DesktopFrames.Measure(frame).Contact;
            var x=main.Bounds.X+main.Bounds.Width/2-72;var top=main.Bounds.Bottom;
            // Start at the physical screen bottom, not Dock's variable top.
            // ConfigureWindow keeps the pet in front of Dock independently.
            controller=new(new(x,top-contact.SoleY),new RandomSource(),savedSize);
            controller.Faulted+=e=>{timer.Stop();ReportError("동작 처리",e);};
            controller.SetAutomaticSleep(sleepEnabled);
            petWindow.Show();jewelWindow.Show();jewelWindow.Hide();
            petHandle=Handle(petWindow);jewelHandle=Handle(jewelWindow);
            QuartzDesktop.ConfigureWindow(petHandle,false);QuartzDesktop.ConfigureWindow(jewelHandle,true);
            native.Move(petHandle,controller.Position-new PointD(96,96));last=clock.Elapsed;timer.Start();
            if(boundaryDiagnostics)boundaryLog=new(Path.Combine(dataDirectory,"Logs",$"boundary-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Environment.ProcessId}.jsonl"));
            using var head=typeof(DesktopSession).Assembly.GetManifestResourceStream("Dororong.head")!;using var bytes=new MemoryStream();head.CopyTo(bytes);
            try{statusItem=new StatusItem(bytes.ToArray(),()=>Dispatcher.UIThread.Post(()=>{if(!disposed)ShowMenu(true);}),e=>ReportError("메뉴 막대",e));}
            catch(Exception e){ReportError("메뉴 막대",e);}
        }
        catch(Exception e){timer.Stop();petWindow.Hide();jewelWindow.Hide();ReportError("초기화",e);}
    }
    private void ShowMenu(bool tray)
    {
        if(controller is null)return;
        try
        {
            PetMenuPanel Create()=>new(tray,controller.Snack!=SnackPhase.None,controller.AutomaticSleep,ToggleSnack,()=>controller.ToggleSit(),SetSleep,OpenLogs,quit,startup.Read,startup.Set,controller.SizePercent,controller.RequestSize);
            if(!tray){statusMenu.Close();menu.Open(petImage,Create);return;}
            menu.Close();var pointer=native.Pointer();if(!pointer.IsAvailable)return;
            statusMenu.Open(Create,w=>
            {
                var point=pointer.Position;var screen=monitors.FirstOrDefault(m=>point.X>=m.Bounds.X&&point.X<m.Bounds.Right&&point.Y>=m.Bounds.Y&&point.Y<m.Bounds.Bottom)??monitors[0];
                var rect=screen.Bounds;var at=StatusMenuHost.Clamp(new(point.X,point.Y),w.Bounds.Size,new(rect.X,rect.Y,rect.Width,rect.Height));
                var handle=Handle(w);QuartzDesktop.ConfigureWindow(handle,false);native.Move(handle,new(at.X,at.Y));
            },()=>
            {
                // Explicit user click only: make the accessory app eligible for
                // keyboard focus while its menu is open, never during idle motion.
                Cocoa.Bool(Cocoa.Get(Cocoa.Class("NSApplication"),"sharedApplication"),Cocoa.Sel("activateIgnoringOtherApps:"),true);
            });
        }
        catch(Exception e){ReportError("메뉴 열기",e);}
    }
    private bool SetSleep(bool enabled)
    {
        if(controller is null||settings is null)return false;
        try{settings.SetAutomaticSleepEnabled(enabled);controller.SetAutomaticSleep(enabled);lastError=null;return true;}
        catch(Exception e) when(e is IOException or UnauthorizedAccessException){ReportError("설정 저장",e);return false;}
    }
    private void OpenLogs()
    {
        try{var folder=Path.Combine(dataDirectory,"Logs");Directory.CreateDirectory(folder);Process.Start(new ProcessStartInfo("/usr/bin/open"){ArgumentList={folder},UseShellExecute=false});}
        catch(Exception e){ReportError("로그 폴더 열기",e);}
    }
    private void ReportError(string operation,Exception error)
    {
        lastError=$"{operation} 실패 ({error.GetType().Name}). 폴더 권한을 확인하거나 종료 후 다시 실행해 주세요.";status.Text=lastError;
        // No window titles, cursor coordinates, text input or exception messages.
        try{var folder=Path.Combine(dataDirectory,"Logs");Directory.CreateDirectory(folder);var path=Path.Combine(folder,"diagnostic.log");if(File.Exists(path)&&new FileInfo(path).Length>262144)File.Move(path,path+".previous",true);File.AppendAllText(path,$"{DateTimeOffset.UtcNow:O} {operation}: {error.GetType().Name}\n");}catch{/* Visible error remains; never fail the exit path. */}
        if(inspection is not null||disposed)return;
        if(errorWindow is not null){errorWindow.Activate();return;}
        // Only failures show this recovery surface; never leave an invisible,
        // unresponsive app when the status item or native initialization fails.
        var text=new TextBlock{Text=lastError,TextWrapping=TextWrapping.Wrap,Margin=new Thickness(12)};
        var logs=new Button{Content="로그 폴더 열기",Margin=new Thickness(12,2)};logs.Click+=(_,_)=>OpenLogs();
        var exit=new Button{Content="종료",Margin=new Thickness(12,2)};exit.Click+=(_,_)=>quit();
        errorWindow=new Window{Title="도로롱 — 실행 문제",Width=380,Height=190,Content=new StackPanel{Children={text,logs,exit}}};
        errorWindow.Closed+=(_,_)=>{errorWindow=null;if(!disposed)quit();};errorWindow.Show();
    }
    private void ToggleSnack()
    {if(controller is null)return;if(controller.Snack==SnackPhase.None)controller.BeginSnack();else controller.CancelSnack();}
    private void Tick(object? sender,EventArgs args)
    {
        if(disposed||controller is null)return;
        try
        {
            var now=clock.Elapsed;var delta=now-last;last=now;
            if(now-lastMonitorRead>=TimeSpan.FromMilliseconds(80))
            {lastMonitorRead=now;try{
                var updated=native.Monitors();
                if(!monitors.SequenceEqual(updated)){petUpdates.InvalidatePosition();jewelUpdates.InvalidatePosition();}
                monitors=updated;
            }catch{/* retain last valid floors */}}
            var scene=scenes.Read(now,native.Capture,monitors);var pointer=native.Pointer();
            controller.Density=petWindow.RenderScaling;
            controller.Advance(delta,pointer,native.PrimaryDown(),scene);
            if(displayedRevision!=controller.FrameRevision||displayedDensity!=controller.Density){
                if(displayedDensity!=controller.Density)petUpdates.InvalidatePosition();
                petImage.Upload(controller.Pixels,controller.Scale,controller.Density);
                displayedRevision=controller.FrameRevision;displayedDensity=controller.Density;
            }
            petImage.Origin=new(96-56*controller.Scale,96-56*controller.Scale+controller.ReleaseOffset);
            native.MoveIfChanged(petHandle,controller.Position-new PointD(96,96),petUpdates);
            boundaryLog?.Sample(now,()=>new {
                placement=native.PlacementEvidence(petHandle),requestedQuartz=controller.Position-new PointD(96,96),
                controller.Position,geometry=controller.Geometry is {} g?new{g.Contact,g.Bounds}:null,
                controller.Capturing,controller.Scale,controller.Density,
                pointer=pointer.IsAvailable?(PointD?)pointer.Position:null,
                sceneHealth=scene.Health.ToString(),captureStatus=native.CaptureStatus,
                currentMonitors=monitors,sceneMonitors=scene.Scene.Monitors,
                sceneAgeSeconds=(now-scene.Scene.CapturedAt).TotalSeconds,
                windowCount=scene.Scene.Windows.Count,
                windows=scene.Scene.Windows.Take(512).Select(w=>new{w.Bounds,w.ZOrder,w.CanSupport,w.CanOcclude,w.Taskbar}).ToArray(),
                platform=controller.Platform
            });
            if(savedSize!=controller.SizePercent){savedSize=controller.SizePercent;sizeChangedAt=now;}
            if(sizeChangedAt!=TimeSpan.Zero&&now-sizeChangedAt>=TimeSpan.FromMilliseconds(350)){
                sizeSettings?.Save(savedSize);sizeChangedAt=TimeSpan.Zero;
            }
            var clickThrough=!contextOpen&&!controller.Capturing&&(!pointer.IsAvailable||!controller.Hit(pointer.Position));
            if(petUpdates.ClickThroughChanged(clickThrough))QuartzDesktop.ClickThrough(petHandle,clickThrough);
            var snack=controller.Snack==SnackPhase.Chasing&&pointer.IsAvailable;
            if(snack)
            {
                if(!jewelWindow.IsVisible){jewelWindow.Show();QuartzDesktop.ConfigureWindow(jewelHandle,true);jewelUpdates.InvalidatePosition();}
                native.MoveIfChanged(jewelHandle,pointer.Position-new PointD(16,16),jewelUpdates);
            }
            else if(jewelWindow.IsVisible)jewelWindow.Hide();
            // Never globally hide or replace the system cursor. The click-through
            // jewel overlay cannot leave other applications with a missing cursor.
            if(inspection is not null)status.Text=lastError??$"창 정보: {scene.Health} · 이동: {controller.Perch} · 매달리기: {controller.Perch}";
        }
        catch(Exception e)
        {timer.Stop();menu.Close();statusMenu.Close();controller.CancelSnack();jewelWindow.Hide();ReportError("동작 처리",e);try{QuartzDesktop.ClickThrough(petHandle,true);}catch{/* Recovery dialog still owns a usable exit. */}}
    }
    private static nint Handle(Window window)
    {
        var handle=window.TryGetPlatformHandle();
        return handle is {HandleDescriptor:"NSWindow"}&&handle.Handle!=0?handle.Handle:throw new PlatformNotSupportedException("Expected NSWindow.");
    }
    private static Window Overlay(double size)=>new(){Width=size,Height=size,CanResize=false,ShowInTaskbar=false,ShowActivated=false,
        Topmost=true,SystemDecorations=SystemDecorations.None,Background=Brushes.Transparent,TransparencyLevelHint=[WindowTransparencyLevel.Transparent]};
    private sealed class RandomSource:IRandomSource{public double NextUnit()=>Random.Shared.NextDouble();}
}

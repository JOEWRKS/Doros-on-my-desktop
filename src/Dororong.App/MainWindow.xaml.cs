using System.Windows;
using System.Windows.Interop;
using Dororong.App.Controls;
using Dororong.App.Interop;
using Dororong.App.Product;
using Dororong.App.Runtime;

namespace Dororong.App;

public partial class MainWindow : Window
{
    private readonly OneShotOperation _loadedOperation = new();
    private readonly OneShotOperation _fatalOperation = new();
    private readonly Action<DiagnosticEvent, Exception?> _reportDiagnostic;
    private WindowActivationGuard? _activationGuard;
    private PetLoop? _loop;
    private RuntimePerformanceCapture? _performanceCapture;
    private StartupRegistration? _startup;
    private readonly BehaviorPreferences _behaviorPreferences = new();
    private readonly PetSizePreferences _sizePreferences = new();
    internal int SizePercent => (int)Math.Round(Presenter.HighResolutionScale * 100);
    internal bool UsesHighResolutionArt => Presenter.HighResolutionGrabEnabled;
    internal bool CanResize => UsesHighResolutionArt && _loop?.CanResize == true;
    internal bool SetSizePercent(int percent)
    {
        if (percent == SizePercent) return true;
        var started = System.Diagnostics.Stopwatch.GetTimestamp();
        if (_loop?.TryResize(() => Presenter.SetArtSize(percent)) != true) return false;
        _performanceCapture?.Input($"size:{percent}", System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        return true;
    }
    internal void SaveSize()
    {
        try { _sizePreferences.Save(SizePercent); }
        catch (Exception ex)
        {
            _reportDiagnostic(DiagnosticEvent.TrayCommandFailure, ex);
            MessageBox.Show(this, ex.Message, "크기 설정", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
    internal PetSizeMenuItem CreateSizeMenuItem() => new(() => SizePercent, () => CanResize, SetSizePercent, SaveSize);
    // Explicit developer test surface only. Normal desktop/no-activate policy
    // is unchanged. This makes native input inspection possible without
    // inventing window handles or pretending a synthetic event is a mouse.
    private readonly bool _interactionTestWindow=Environment.GetCommandLineArgs().Contains("--interaction-test-window");
    internal bool AutomaticSleepEnabled { get; private set; } = true;

    public MainWindow() : this((_, _) => { })
    {
    }

    internal MainWindow(Action<DiagnosticEvent, Exception?> reportDiagnostic)
    {
        _reportDiagnostic = reportDiagnostic ?? throw new ArgumentNullException(nameof(reportDiagnostic));
        InitializeComponent();
        if(_interactionTestWindow){
            ShowInTaskbar=true;ShowActivated=true;Title="Dororong interaction test";
            // Windows.Graphics.Capture cannot reliably capture this layered
            // tool window. Only this explicit input-test host is opaque; the
            // production transparent host and source sampler stay unchanged.
            AllowsTransparency=false;Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(41,45,52));
        }
        // Explicit local test path only. No preference/startup/installer mutation.
        var highResolution=Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--high-resolution-grab=",StringComparison.Ordinal));
        if(highResolution is not null&&double.TryParse(highResolution.Split('=')[1],System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,out var artScale)&&artScale>=1&&artScale<=1.5)
        {
            Presenter.HighResolutionGrabEnabled=true;Presenter.HighResolutionScale=artScale;
            Presenter.Width=Presenter.Height=144*artScale;
            ((System.Windows.Controls.Canvas)Presenter.Content).Width=144*artScale;
            ((System.Windows.Controls.Canvas)Presenter.Content).Height=144*artScale;
        }
        try { if (_sizePreferences.Percent is { } size) Presenter.SetArtSize(size); }
        catch (Exception ex) { _reportDiagnostic(DiagnosticEvent.TrayCommandFailure, ex); }
        var menu = ((System.Windows.Controls.Canvas)Presenter.FindName("BodyGroup")).ContextMenu;
        var sizeItem = CreateSizeMenuItem();
        if (UsesHighResolutionArt) menu.Items.Insert(3, sizeItem);
        menu.Opened += (_, _) => sizeItem.Refresh();
        menu.Closed += (_, _) => sizeItem.Finish();
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        Closed += OnClosed;
        Presenter.DirectInteractionPressed += OnDirectInteractionPressed;
        Presenter.LostMouseCapture += OnDirectInteractionCanceled;
        Presenter.ExitRequested += OnExitRequested;
        Presenter.SitRequested += OnSitRequested;
        Presenter.SnackRequested += OnSnackRequested;
        Presenter.StartupToggleRequested += OnStartupToggleRequested;
        Presenter.AutomaticSleepToggleRequested += OnAutomaticSleepToggleRequested;
        Presenter.ProductMenuOpened += OnProductMenuOpened;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        try
        {
            if (_activationGuard?.IsAttached == true)
            {
                return;
            }

            var detachError = DetachActivationGuard();
            if (detachError is not null)
            {
                throw detachError;
            }

            var handle = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(handle)
                ?? throw new InvalidOperationException("The WPF window source is unavailable.");

            if(!_interactionTestWindow)WindowStyleManager.ApplyNoActivateToolWindow(handle);
            _activationGuard = new WindowActivationGuard(source);
        }
        catch (Exception exception)
        {
            HandleFatal(exception, DiagnosticEvent.StartupFailure);
        }
    }

    private Exception? DetachActivationGuard()
    {
        var guard = _activationGuard;
        _activationGuard = null;
        return CleanupSequence.Run(() => guard?.Dispose());
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        try
        {
            _loadedOperation.TryRun(() =>
            {
                if (_loop is not null)
                {
                    throw new InvalidOperationException("The pet loop has already been created.");
                }

                var loop = new PetLoop(this, Presenter, new DesktopInput());
                _loop = loop;
                try { AutomaticSleepEnabled = _behaviorPreferences.AutomaticSleepEnabled; }
                catch (Exception ex) { ReportSleepPreferenceFailure(ex); }
                loop.SetAutomaticSleepEnabled(AutomaticSleepEnabled);
                loop.Faulted += OnLoopFaulted;
                loop.Start();
                var capture=Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--performance-capture=",StringComparison.Ordinal));
                var landingCapture=Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--landing-frame-capture=",StringComparison.Ordinal));
                if(capture is not null)_performanceCapture=new(capture[(capture.IndexOf('=')+1)..],Presenter,_interactionTestWindow,()=>loop.DiagnosticState,loop,
                    landingFrameDirectory:landingCapture?[(landingCapture.IndexOf('=')+1)..],
                    frameTrigger:Environment.GetCommandLineArgs().Contains("--frame-capture-trigger=release")
                        ?FrameCaptureTrigger.FirstFall:FrameCaptureTrigger.StrongLanding);
            });
        }
        catch (Exception exception)
        {
            HandleFatal(exception, DiagnosticEvent.StartupFailure);
        }
    }

    private void OnDirectInteractionPressed(object? sender, DirectInteractionPressEventArgs e)
    {
        _performanceCapture?.Input(e.Target.ToString());
        _loop?.NotifyDirectInteractionPressed(e);
    }

    private void OnDirectInteractionCanceled(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _loop?.NotifyDirectInteractionCanceled();
    }

    private void OnSitRequested(object? sender, EventArgs e) => _loop?.NotifySitRequested();
    internal bool IsSnackActive => _loop?.IsSnackActive == true;
    internal void ToggleSnack(){_performanceCapture?.Input("snack-toggle");_loop?.ToggleSnack();}
    internal bool StartupEnabled => (_startup ??= new()).Enabled;
    internal void ToggleStartup()
    {
        try { (_startup ??= new()).SetEnabled(!StartupEnabled); }
        catch(Exception ex)
        {
            _reportDiagnostic(DiagnosticEvent.TrayCommandFailure,ex);
            MessageBox.Show(this,ex.Message,"자동 실행 설정",MessageBoxButton.OK,MessageBoxImage.Warning);
        }
    }
    private void OnSnackRequested(object? sender, EventArgs e) => ToggleSnack();
    private void OnStartupToggleRequested(object? sender, EventArgs e) => ToggleStartup();
    internal void ToggleAutomaticSleep()
    {
        try
        {
            var enabled = !AutomaticSleepEnabled;
            _behaviorPreferences.SetAutomaticSleepEnabled(enabled);
            AutomaticSleepEnabled = enabled;
            if (!enabled) Presenter.PrepareAutomaticSleepWake();
            _loop?.SetAutomaticSleepEnabled(enabled);
        }
        catch (Exception ex) { ReportSleepPreferenceFailure(ex); }
    }
    private void ReportSleepPreferenceFailure(Exception ex)
    {
        _reportDiagnostic(DiagnosticEvent.TrayCommandFailure, ex);
        MessageBox.Show(this, ex.Message, "자동 눕기 설정", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    private void OnAutomaticSleepToggleRequested(object? sender, EventArgs e)
    {
        ToggleAutomaticSleep();
        OnProductMenuOpened(sender, e);
    }
    private void OnProductMenuOpened(object? sender, EventArgs e)
    {
        try { Presenter.UpdateProductMenu(IsSnackActive, StartupEnabled, AutomaticSleepEnabled); }
        catch(Exception ex) { _reportDiagnostic(DiagnosticEvent.TrayCommandFailure,ex); Presenter.UpdateProductMenu(IsSnackActive,false,AutomaticSleepEnabled); }
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnLoopFaulted(object? sender, Exception exception)
    {
        HandleFatal(exception, DiagnosticEvent.LoopFailure);
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _performanceCapture?.Dispose();
        // Explicit diagnostic export is bounded; preserve it on user exit too.
        _performanceCapture?.CaptureExport.GetAwaiter().GetResult();
        _performanceCapture?.FrameExport.GetAwaiter().GetResult();_performanceCapture=null;
        var cleanupException = CleanupWindowResources();
        if (cleanupException is not null)
        {
            HandleFatal(cleanupException, DiagnosticEvent.CleanupFailure);
        }
    }

    private void HandleFatal(Exception exception, DiagnosticEvent diagnosticEvent)
    {
        _fatalOperation.TryRun(() =>
        {
            try
            {
                _reportDiagnostic(diagnosticEvent, exception);
            }
            catch
            {
                // Diagnostics cannot suppress the existing fatal boundary.
            }

            FatalBoundary.Run(
                    exception,
                    CleanupWindowResources,
                    fatalException =>
                        MessageBox.Show(
                            this,
                            $"Dororong encountered an unexpected error and must close.\n\n{fatalException.Message}",
                            "Dororong",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error),
                    () => Application.Current.Shutdown(1));
        });
    }

    private Exception? CleanupWindowResources()
    {
        var loop = _loop;
        _loop = null;
        var guard = _activationGuard;
        _activationGuard = null;

        return CleanupSequence.Run(
            () => SourceInitialized -= OnSourceInitialized,
            () => Loaded -= OnLoaded,
            () => Closed -= OnClosed,
            () => Presenter.DirectInteractionPressed -= OnDirectInteractionPressed,
            () => Presenter.LostMouseCapture -= OnDirectInteractionCanceled,
            () => Presenter.ExitRequested -= OnExitRequested,
            () => Presenter.SitRequested -= OnSitRequested,
            () => Presenter.SnackRequested -= OnSnackRequested,
            () => Presenter.StartupToggleRequested -= OnStartupToggleRequested,
            () => Presenter.AutomaticSleepToggleRequested -= OnAutomaticSleepToggleRequested,
            () => Presenter.ProductMenuOpened -= OnProductMenuOpened,
            () =>
            {
                if (loop is not null)
                {
                    loop.Faulted -= OnLoopFaulted;
                }
            },
            () => loop?.Dispose(),
            () => guard?.Dispose());
    }
}

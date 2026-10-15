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
    private StartupRegistration? _startup;
    private readonly BehaviorPreferences _behaviorPreferences = new();
    internal bool AutomaticSleepEnabled { get; private set; } = true;

    public MainWindow() : this((_, _) => { })
    {
    }

    internal MainWindow(Action<DiagnosticEvent, Exception?> reportDiagnostic)
    {
        _reportDiagnostic = reportDiagnostic ?? throw new ArgumentNullException(nameof(reportDiagnostic));
        InitializeComponent();
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

            WindowStyleManager.ApplyNoActivateToolWindow(handle);
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
            });
        }
        catch (Exception exception)
        {
            HandleFatal(exception, DiagnosticEvent.StartupFailure);
        }
    }

    private void OnDirectInteractionPressed(object? sender, DirectInteractionPressEventArgs e)
    {
        _loop?.NotifyDirectInteractionPressed(e);
    }

    private void OnDirectInteractionCanceled(object sender, System.Windows.Input.MouseEventArgs e)
    {
        _loop?.NotifyDirectInteractionCanceled();
    }

    private void OnSitRequested(object? sender, EventArgs e) => _loop?.NotifySitRequested();
    internal bool IsSnackActive => _loop?.IsSnackActive == true;
    internal void ToggleSnack() => _loop?.ToggleSnack();
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

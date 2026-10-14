using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Dororong.App.Product;
using Forms = System.Windows.Forms;

namespace Dororong.SingleInstanceProbe;

internal static class StyledMenuProbe
{
    internal static void CheckScenario(string scenario)
    {
        using var notify = new Forms.NotifyIcon();
        using var source = new Forms.ContextMenuStrip();
        var snack = false; var startup = false; var logs = 0; var exits = 0;
        using var service = new TrayService((System.Drawing.Icon)System.Drawing.SystemIcons.Application.Clone(),
            notify, source, () => logs++, () => exits++, false);
        service.AddPetCommands(() => snack = !snack, () => snack, () => startup = !startup, () => startup);
        var owner = new Window { Width = 144, Height = 144, ShowActivated = false, ShowInTaskbar = false };
        owner.Show();
        Dororong.App.Interop.WindowStyleManager.ApplyNoActivateToolWindow(new WindowInteropHelper(owner).Handle);
        service.EnableStyledMenu(owner);
        var legacyOpenings = 0;
        source.Opening += (_, _) => legacyOpenings++;
        Check(notify.ContextMenuStrip is null, "Styled tray must detach the legacy popup route.");
        void Left() => typeof(Forms.NotifyIcon).GetMethod("OnMouseUp", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(notify, [new Forms.MouseEventArgs(Forms.MouseButtons.Left, 1, 0, 0, 0)]);
        void Right()
        {
            var message = Forms.Message.Create(IntPtr.Zero, 0x400 + 1024, IntPtr.Zero, new IntPtr(0x0205));
            typeof(Forms.NotifyIcon).GetMethod("WndProc", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(notify, [message]);
        }
        if (scenario == "styled-right")
        {
            Right(); Pump();
            Check(service.CurrentStyledMenu is { IsOpen: true } && !source.Visible,
                "Right release alone did not open the styled popup.");
            var rightMenu = service.CurrentStyledMenu;
            PumpFor(350);
            Check(rightMenu is { IsOpen: true } && ReferenceEquals(rightMenu, service.CurrentStyledMenu),
                "Right-click popup immediately disappeared or was recreated after native release.");
        }
        else Left();
        if (scenario == "styled-dispose-pending")
        {
            service.Dispose(); Pump();
            Check(service.CurrentStyledMenu is null, "Disposed tray opened queued popup.");
            return;
        }
        Left(); Left(); Pump();
        var menu = service.CurrentStyledMenu ?? throw new Exception("Styled tray did not open.");
        Check(menu.IsOpen && !source.Visible, "Both native and styled surfaces must not open.");
        Check(legacyOpenings == 0, "Styled release entered the legacy Forms popup state machine.");
        Check(ReferenceEquals(owner, menu.PlacementTarget), "Tray menu lost its real WPF owner.");
        Check(PresentationSource.FromVisual(menu) is HwndSource, "Not a native WPF popup.");
        Left(); Pump();
        Check(ReferenceEquals(menu, service.CurrentStyledMenu), "Repeated release replaced popup.");
        Check(menu.ActualWidth >= 288 && menu.ActualWidth < 320, "Unexpected menu width.");

        if (scenario == "styled-repeat-escape")
        {
            var first = menu.Items.OfType<MenuItem>().First();
            Keyboard.Focus(first);
            first.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(menu), 0, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent });
            Pump(); Check(!menu.IsOpen, "WPF keyboard Escape did not close popup.");
            Left(); Pump();
            Check(service.CurrentStyledMenu is { IsOpen: true } next && !ReferenceEquals(menu, next), "Cannot reopen.");
        }
        else if (scenario == "styled-keyboard")
        {
            var first = menu.Items.OfType<MenuItem>().First();
            Keyboard.Focus(first);
            first.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(menu), 0, Key.Enter) { RoutedEvent = Keyboard.KeyDownEvent });
            Pump();
            Check(snack && !menu.IsOpen, $"Enter must invoke snack once and dismiss. snack={snack}, open={menu.IsOpen}, role={first.Role}, focus={first.IsKeyboardFocused}, highlighted={first.IsHighlighted}");
        }
        else if (scenario == "styled-dispose-open")
        {
            service.Dispose(); Pump(); Check(!menu.IsOpen, "Disposal left popup open.");
        }
        else if (scenario == "styled-outside")
        {
            var window = new Window { Width = 160, Height = 100, Left = 700, Top = 400, ShowInTaskbar = false };
            try
            {
                window.Show(); window.Activate(); Pump();
                var handle = new WindowInteropHelper(window).Handle;
                SetForegroundWindow(handle);
                // Activation may already have dismissed it. Only inject the
                // isolated-desktop focus loss if the popup is still open.
                if (menu.IsOpen)
                {
                    var popup = (HwndSource)PresentationSource.FromVisual(menu);
                    SendMessage(popup.Handle, 0x001C, IntPtr.Zero, IntPtr.Zero); // WM_ACTIVATEAPP
                }
                Pump(); Check(!menu.IsOpen, "Outside activation did not dismiss popup.");
            }
            finally { window.Close(); }
        }
        else if (scenario == "styled-character")
        {
            menu.IsOpen = false;
            var presenter = new Dororong.App.Controls.DororongPresenter();
            var body = (FrameworkElement)presenter.FindName("BodyGroup");
            var character = (PetContextMenu)body.ContextMenu;
            var count = 0;
            presenter.SnackRequested += (_, _) => count++;
            for (var state = 0; state < 4; state++)
            {
                presenter.UpdateProductMenu((state & 1) != 0, (state & 2) != 0);
                character.IsOpen = true; Pump();
                var items = character.Items.OfType<MenuItem>().ToArray();
                Check(items.Length == 4 && items.Any(i => Equals(i.Tag, "sit")) &&
                    !items.Any(i => Equals(i.Tag, "logs")), "Character menu scope changed.");
                Check(items[2].IsChecked == ((state & 2) != 0), "Character startup state wrong.");
                Save(character, $"character-state-{state}.png");
                character.IsOpen = false;
            }
            ((MenuItem)character.Items[0]).RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Check(count == 1, "Character snack command did not execute exactly once.");
        }
        else if (scenario == "styled-state-commands")
        {
            for (var state = 0; state < 4; state++)
            {
                snack = (state & 1) != 0; startup = (state & 2) != 0;
                menu.IsOpen = false; Left(); Pump(); menu = service.CurrentStyledMenu!;
                var items = menu.Items.OfType<MenuItem>().ToArray();
                Check(items.Length == 4 && !items.Any(item => Equals(item.Tag, "sit")), "Tray scope changed.");
                Check((string)items[0].Header == (snack ? "간식 취소" : "간식 주기"), "Stale snack state.");
                Check(items[1].IsChecked == startup, "Stale startup state.");
                Save(menu, $"tray-state-{state}.png");
            }
            var commands = menu.Items.OfType<MenuItem>().ToArray();
            foreach (var item in commands) item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            Check(!snack && !startup && logs == 1 && exits == 1, "Callbacks did not execute exactly once.");
        }
        else if (scenario != "styled-right") throw new Exception("Unknown styled scenario.");
    }

    private static void Save(FrameworkElement visual, string name)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "DororongDesktopPet.sln"))) root = root.Parent;
        var directory = Path.Combine(root!.FullName, "artifacts/ui-menu-head-icon-20261014/native-proof");
        Directory.CreateDirectory(directory);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(visual.ActualWidth),
            (int)Math.Ceiling(visual.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(Path.Combine(directory, name)); encoder.Save(file);
    }

    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static void PumpFor(int milliseconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(milliseconds) };
        timer.Tick += (_, _) => frame.Continue = false;
        timer.Start();
        try { Dispatcher.PushFrame(frame); }
        finally { timer.Stop(); }
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr handle);
    [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
}

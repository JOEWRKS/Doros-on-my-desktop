using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Dororong.App.Interop;
using FormsMenu = System.Windows.Forms.ContextMenuStrip;
using FormsItem = System.Windows.Forms.ToolStripMenuItem;

namespace Dororong.App.Product;

/// <summary>Keep NotifyIcon's left/right routes and callbacks, but show one WPF menu.</summary>
internal sealed class StyledTrayMenuBridge : IDisposable
{
    private readonly FormsMenu source;
    private readonly Dispatcher dispatcher;
    private readonly Window owner;
    private readonly Action refresh;
    private DispatcherOperation? pending;
    private PetContextMenu? current;
    private bool disposed;

    internal StyledTrayMenuBridge(FormsMenu source, Window owner, Action refresh)
    {
        this.source = source;
        this.owner = owner;
        this.refresh = refresh;
        dispatcher = owner.Dispatcher;
        dispatcher.VerifyAccess();
    }

    internal PetContextMenu? Current => current;

    internal void Open()
    {
        dispatcher.VerifyAccess();
        if (disposed || dispatcher.HasShutdownStarted ||
            pending is { Status: DispatcherOperationStatus.Pending } || current is { IsOpen: true }) return;

        // Finish the native release before opening, without entering a Forms popup.
        pending = dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            pending = null;
            if (disposed || source.IsDisposed || dispatcher.HasShutdownStarted) return;
            refresh();
            var menu = CreateMenu();
            menu.PlacementTarget = owner;
            current = menu;
            menu.Opened += OnOpened;
            menu.Closed += (_, _) =>
            {
                menu.Opened -= OnOpened;
                if (ReferenceEquals(current, menu)) current = null;
            };
            // Activate the actual owner BEFORE popup capture. Activating the
            // non-activating popup after Opened can invalidate its menu mode.
            var handle = new WindowInteropHelper(owner).Handle;
            if (handle != IntPtr.Zero) NativeMethods.SetForegroundWindow(handle);
            menu.IsOpen = true;
        }));
    }

    internal PetContextMenu CreateMenu()
    {
        var menu = new PetContextMenu { ShowVersion = true };
        foreach (var sourceItem in source.Items.OfType<FormsItem>())
        {
            if (Equals(sourceItem.Tag, "version"))
            {
                if (SystemParameters.HighContrast)
                    menu.Items.Add(new MenuItem { Header = sourceItem.Text, IsEnabled = false });
                continue;
            }
            if (Equals(sourceItem.Tag, "sleep") && owner is MainWindow pet && pet.UsesHighResolutionArt)
            {
                var size = pet.CreateSizeMenuItem();
                menu.Items.Add(size);
                menu.Closed += (_, _) => size.Finish();
            }
            if (Equals(sourceItem.Tag, "logs")) menu.Items.Add(new Separator());
            var item = new MenuItem
            {
                Header = sourceItem.Text, Tag = sourceItem.Tag, IsEnabled = sourceItem.Enabled,
                IsCheckable = Equals(sourceItem.Tag, "startup") || Equals(sourceItem.Tag, "sleep"), IsChecked = sourceItem.Checked
            };
            item.Click += (_, _) =>
            {
                if (disposed || sourceItem.IsDisposed) return;
                menu.IsOpen = false;
                sourceItem.PerformClick();
                if (disposed || sourceItem.IsDisposed) return;
                item.IsChecked = sourceItem.Checked;
                item.Header = sourceItem.Text;
            };
            menu.Items.Add(item);
        }
        return menu;
    }

    private void OnOpened(object sender, RoutedEventArgs e)
    {
        if (disposed || sender is not PetContextMenu { IsOpen: true } menu) return;
        menu.Focus();
        if (menu.Items.OfType<MenuItem>().FirstOrDefault(item => item.IsEnabled) is { } first)
            Keyboard.Focus(first);
    }

    public void Dispose()
    {
        if (disposed) return;
        dispatcher.VerifyAccess();
        disposed = true;
        pending?.Abort();
        pending = null;
        if (current is { } menu)
        {
            menu.Opened -= OnOpened;
            menu.IsOpen = false;
            current = null;
        }
    }
}

using Dororong.App.Runtime;
using Dororong.App.Interop;
using DrawingIcon = System.Drawing.Icon;
using FormsContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using FormsNotifyIcon = System.Windows.Forms.NotifyIcon;
using FormsToolStripMenuItem = System.Windows.Forms.ToolStripMenuItem;

namespace Dororong.App.Product;

internal sealed class TrayService : IDisposable
{
    private readonly TrayServiceResources resources;
    private FormsToolStripMenuItem? versionItem;
    private FormsToolStripMenuItem? openLogsItem;
    private FormsToolStripMenuItem? exitItem;
    private EventHandler? openLogsHandler;
    private EventHandler? exitHandler;
    private StyledTrayMenuBridge? styledMenu;
    private Action? refreshPetCommands;
    private int disposed;

    internal TrayService(Action openLogs, Action exit, bool visible = true) : this(
        AcquireDefaultResources(
            openLogs,
            exit,
            LoadIcon,
            () => new FormsTrayNotifyIcon(new FormsNotifyIcon()),
            () => new FormsContextMenuStrip()),
        openLogs,
        exit,
        visible)
    {
    }

    internal TrayService(
        DrawingIcon icon,
        FormsNotifyIcon notifyIcon,
        FormsContextMenuStrip menu,
        Action openLogs,
        Action exit,
        bool visible) : this(
            new TrayServiceResources(icon, new FormsTrayNotifyIcon(notifyIcon), menu),
            openLogs,
            exit,
            visible)
    {
    }

    private TrayService(
        TrayServiceResources resources,
        Action openLogs,
        Action exit,
        bool visible)
    {
        this.resources = resources ?? throw new ArgumentNullException(nameof(resources));

        try
        {
            ArgumentNullException.ThrowIfNull(openLogs);
            ArgumentNullException.ThrowIfNull(exit);
            versionItem = new FormsToolStripMenuItem($"{ProductIdentity.DisplayName} {ProductIdentity.Version}")
            {
                Enabled = false, Tag = "version"
            };
            openLogsItem = new FormsToolStripMenuItem("로그 폴더 열기") { Tag = "logs" };
            exitItem = new FormsToolStripMenuItem("종료") { Tag = "exit" };
            openLogsHandler = (_, _) => openLogs();
            exitHandler = (_, _) => exit();
            openLogsItem.Click += openLogsHandler;
            exitItem.Click += exitHandler;
            resources.Menu.Items.AddRange([versionItem, openLogsItem, exitItem]);
            resources.NotifyIcon.Icon = resources.Icon;
            resources.NotifyIcon.Text = ProductIdentity.DisplayName;
            resources.NotifyIcon.ContextMenuStrip = resources.Menu;
            resources.NotifyIcon.Visible = visible;
        }
        catch (Exception initializationException)
        {
            var cleanupException = CleanupSequence.Run(
                DetachOpenLogsHandler,
                DetachExitHandler,
                () => versionItem?.Dispose(),
                () => openLogsItem?.Dispose(),
                () => exitItem?.Dispose(),
                resources.Dispose);
            if (cleanupException is not null)
            {
                throw new AggregateException(initializationException, cleanupException).Flatten();
            }

            throw;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        var cleanupException = CleanupSequence.Run(
            () => styledMenu?.Dispose(),
            DetachOpenLogsHandler,
            DetachExitHandler,
            resources.Dispose);
        if (cleanupException is not null)
        {
            throw cleanupException;
        }
    }

    internal void AddPetCommands(Action snack, Func<bool> snackActive, Action startup, Func<bool> startupEnabled)
    {
        var snackItem=new FormsToolStripMenuItem("간식 주기") { Tag = "snack" };
        var startupItem=new FormsToolStripMenuItem("Windows 로그인 시 실행") { Tag = "startup" };
        snackItem.Click+=(_,_)=>snack();
        startupItem.Click+=(_,_)=> { startup(); Refresh(); };
        void Refresh()
        {
            snackItem.Text=snackActive()?"간식 취소":"간식 주기";
            try { startupItem.Checked=startupEnabled(); } catch { startupItem.Checked=false; }
        }
        resources.Menu.Opening+=(_,_)=>Refresh();
        refreshPetCommands = Refresh;
        resources.Menu.Items.Insert(1,snackItem);
        resources.Menu.Items.Insert(2,startupItem);
    }

    internal void EnableStyledMenu(System.Windows.Window owner)
    {
        ObjectDisposedException.ThrowIf(disposed != 0, this);
        if (resources.NotifyIcon is not FormsTrayNotifyIcon native)
            throw new InvalidOperationException("Styled menus require the native tray adapter.");
        styledMenu ??= new StyledTrayMenuBridge(resources.Menu, owner, () => refreshPetCommands?.Invoke());
        native.SetStyledMenu(styledMenu.Open);
    }

    internal PetContextMenu? CurrentStyledMenu => styledMenu?.Current;

    private static DrawingIcon LoadIcon()
    {
        const string resourceName = "Dororong.App.Assets.dororong.ico";
        using var stream = typeof(TrayService).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The product icon resource '{resourceName}' is unavailable.");
        using var loaded = new DrawingIcon(stream);
        return new DrawingIcon(loaded, System.Windows.Forms.SystemInformation.SmallIconSize);
    }

    private static TrayServiceResources AcquireDefaultResources(
        Action openLogs,
        Action exit,
        Func<DrawingIcon> createIcon,
        Func<ITrayNotifyIcon> createNotifyIcon,
        Func<FormsContextMenuStrip> createMenu)
    {
        ArgumentNullException.ThrowIfNull(openLogs);
        ArgumentNullException.ThrowIfNull(exit);
        return TrayServiceResources.Acquire(createIcon, createNotifyIcon, createMenu);
    }

    private void DetachOpenLogsHandler()
    {
        if (openLogsItem is not null && openLogsHandler is not null)
        {
            openLogsItem.Click -= openLogsHandler;
        }
    }

    private void DetachExitHandler()
    {
        if (exitItem is not null && exitHandler is not null)
        {
            exitItem.Click -= exitHandler;
        }
    }
}

internal interface ITrayNotifyIcon : IDisposable
{
    DrawingIcon? Icon { set; }
    string Text { set; }
    FormsContextMenuStrip? ContextMenuStrip { set; }
    bool Visible { get; set; }
}

internal sealed class FormsTrayNotifyIcon : ITrayNotifyIcon
{
    private readonly FormsNotifyIcon notifyIcon;
    private Action? openStyledMenu;
    private int disposed;

    internal FormsTrayNotifyIcon(FormsNotifyIcon notifyIcon)
    {
        this.notifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
        notifyIcon.MouseUp += OnMouseUp;
    }

    public DrawingIcon? Icon
    {
        set => notifyIcon.Icon = value;
    }

    public string Text
    {
        set => notifyIcon.Text = value;
    }

    public FormsContextMenuStrip? ContextMenuStrip
    {
        set => notifyIcon.ContextMenuStrip = value;
    }

    public bool Visible
    {
        get => notifyIcon.Visible;
        set => notifyIcon.Visible = value;
    }

    private void OnMouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (disposed != 0) return;
        if (openStyledMenu is { } open)
        {
            if (e.Button is System.Windows.Forms.MouseButtons.Left or System.Windows.Forms.MouseButtons.Right) open();
            return;
        }
        // Right click keeps NotifyIcon's native menu route. MouseUp also covers
        // the second release of a double click without creating a second popup.
        if (disposed != 0 || e.Button != System.Windows.Forms.MouseButtons.Left ||
            notifyIcon.ContextMenuStrip is not { IsDisposed: false, Visible: false } menu)
        {
            return;
        }

        menu.Show(System.Windows.Forms.Cursor.Position,
            System.Windows.Forms.ToolStripDropDownDirection.AboveLeft);
        if (menu.Visible)
        {
            // Standalone tray popups need foreground ownership for keyboard
            // navigation and dismissal when focus moves to another application.
            NativeMethods.SetForegroundWindow(menu.Handle);
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        notifyIcon.MouseUp -= OnMouseUp;
        openStyledMenu = null;
        notifyIcon.Dispose();
    }

    internal void SetStyledMenu(Action open)
    {
        openStyledMenu = open ?? throw new ArgumentNullException(nameof(open));
        // Do not enter WinForms' popup/foreground state machine just to cancel it.
        // Both native releases now have exactly one WPF opening route.
        notifyIcon.ContextMenuStrip = null;
    }
}

internal sealed class TrayServiceResources : IDisposable
{
    internal DrawingIcon Icon { get; }
    internal ITrayNotifyIcon NotifyIcon { get; }
    internal FormsContextMenuStrip Menu { get; }

    internal TrayServiceResources(
        DrawingIcon icon,
        ITrayNotifyIcon notifyIcon,
        FormsContextMenuStrip menu)
    {
        Icon = icon ?? throw new ArgumentNullException(nameof(icon));
        NotifyIcon = notifyIcon ?? throw new ArgumentNullException(nameof(notifyIcon));
        Menu = menu ?? throw new ArgumentNullException(nameof(menu));
    }

    internal static TrayServiceResources Acquire(
        Func<DrawingIcon> createIcon,
        Func<ITrayNotifyIcon> createNotifyIcon,
        Func<FormsContextMenuStrip> createMenu)
    {
        ArgumentNullException.ThrowIfNull(createIcon);
        ArgumentNullException.ThrowIfNull(createNotifyIcon);
        ArgumentNullException.ThrowIfNull(createMenu);

        DrawingIcon? icon = null;
        ITrayNotifyIcon? notifyIcon = null;
        FormsContextMenuStrip? menu = null;
        try
        {
            icon = createIcon() ?? throw new InvalidOperationException("The icon factory returned null.");
            notifyIcon = createNotifyIcon()
                ?? throw new InvalidOperationException("The notify-icon factory returned null.");
            menu = createMenu() ?? throw new InvalidOperationException("The tray-menu factory returned null.");
            return new TrayServiceResources(icon, notifyIcon, menu);
        }
        catch (Exception acquisitionException)
        {
            var cleanupException = CleanupSequence.Run(
                () => menu?.Dispose(),
                () => notifyIcon?.Dispose(),
                () => icon?.Dispose());
            if (cleanupException is not null)
            {
                throw new AggregateException(acquisitionException, cleanupException).Flatten();
            }

            throw;
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0)
        {
            return;
        }

        var cleanupException = CleanupSequence.Run(
            () => NotifyIcon.Visible = false,
            () => NotifyIcon.ContextMenuStrip = null,
            NotifyIcon.Dispose,
            Menu.Dispose,
            Icon.Dispose);
        if (cleanupException is not null)
        {
            throw cleanupException;
        }
    }

    private int disposed;
}

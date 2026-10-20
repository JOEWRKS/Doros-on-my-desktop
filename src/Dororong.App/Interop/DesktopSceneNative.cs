using Dororong.App.Runtime;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace Dororong.App.Interop;

internal sealed record NativeWindow(long Handle, uint ProcessId, RectD Bounds, bool Visible = true,
    bool Minimized = false, bool Cloaked = false, bool Excluded = false, bool CanSupport = true,
    bool Taskbar = false, bool HorizontalTaskbar = false, string ClassName = "")
{
    internal bool CanOcclude { get; init; } = true;
    internal bool IsTransientMenu { get; init; }
}
internal sealed record NativeTaskbarMetadata(long MonitorId, uint Edge, long AutoHideHandle);
internal sealed record NativeDesktop(IReadOnlyList<DesktopMonitor> Monitors, IReadOnlyList<NativeWindow> Windows)
{
    internal IReadOnlyList<NativeTaskbarMetadata> Taskbars { get; init; } = Array.Empty<NativeTaskbarMetadata>();
    internal RectD? SystemTaskbarBounds { get; init; }
}
internal interface IDesktopMetadataReader { NativeDesktop? Read(); }
internal sealed class DesktopMetadataException(string operation, int code) : Exception
{
    internal string Diagnostic => $"{operation}:{code}";
}
internal sealed class DesktopMetadataReader : IDesktopMetadataReader
{
    private readonly nint petHandle;
    private readonly Func<uint, string> readProcessImagePath;
    // Discovery hints only: never retain bounds, visibility or a previous z-order.
    private Dictionary<long,uint> knownTaskbars = new();
    private readonly HashSet<nint> captureHandles=new(), orderedVisited=new();
    private Dictionary<long,NativeWindow> previousWindows=new(), windowScratch=new();
    // Keep the original constructor first for the existing reflection-based metadata probe.
    public DesktopMetadataReader(nint petHandle) : this(petHandle, ReadProcessImagePath) { }
    internal DesktopMetadataReader(nint petHandle, Func<uint, string> readProcessImagePath)
    {
        this.petHandle = petHandle;
        this.readProcessImagePath = readProcessImagePath;
    }
    internal static IReadOnlyList<DesktopMonitor> ReadMonitorBounds()
    {
        var previous=Api.SetThreadDpiAwarenessContext(new nint(-4));
        if(previous==0) throw Failure("DpiContext");
        try {return ReadMonitors().AsReadOnly();}
        finally {if(Api.SetThreadDpiAwarenessContext(previous)==0) throw Failure("RestoreDpiContext");}
    }

    private static List<DesktopMonitor> ReadMonitors()
    {
        var monitors = new List<DesktopMonitor>();
        var monitorError = false;
        Api.MonitorCallback callback = (nint monitor, nint dc, ref Api.Rect rectangle, nint data) =>
        {
            var info = new Api.MonitorInfo { Size = Marshal.SizeOf<Api.MonitorInfo>() };
            if (!Api.GetMonitorInfo(monitor,ref info)) { monitorError=true; return false; }
            monitors.Add(new(monitor.ToInt64(),info.Monitor.ToRect())); return monitors.Count < 128;
        };
        if (!Api.EnumDisplayMonitors(0,0,callback,0) || monitorError || monitors.Count == 0)
            throw Failure("MonitorEnumeration");
        return monitors;
    }
    // Class names are shell heuristics, not a universal Windows contract. Unknown classes
    // stay ordinary windows. Executable identity excludes other pets, never all WPF apps.
    public NativeDesktop? Read()
    {
        var previous = Api.SetThreadDpiAwarenessContext(new nint(-4)); // PER_MONITOR_AWARE_V2
        if (previous == 0) throw Failure("DpiContext");
        try
        {
            var monitors = ReadMonitors();
            var handles = captureHandles;handles.Clear();
            if (!Api.EnumWindows((handle,_) => { handles.Add(handle); return handles.Count < 4096; },0))
                throw Failure("WindowEnumeration");
            // Explicit bounded z-order traversal; EnumWindows alone is not used as an ordering contract.
            var windows = ReadOrderedWindows(handles, EnumerateZOrder(), DiscoverTaskbars());
            var taskbars = new List<NativeTaskbarMetadata>();
            foreach (var monitor in monitors)
            for (uint edge=0; edge<4; edge++)
            {
                var data=Api.AppbarData.Create(); data.Edge=edge; data.Rectangle=Api.Rect.From(monitor.Bounds);
                // NULL means no appbar OR error, so it never proves current hidden state.
                var handle=Api.SHAppBarMessage(0xB,ref data);
                taskbars.Add(new(monitor.Id,edge,unchecked((long)handle.ToUInt64())));
            }
            var systemBar=Api.AppbarData.Create();
            RectD? systemBounds=Api.SHAppBarMessage(5,ref systemBar) != UIntPtr.Zero ? systemBar.Rectangle.ToRect() : null;
            return new(monitors.AsReadOnly(),windows.AsReadOnly()) { Taskbars=taskbars.AsReadOnly(),SystemTaskbarBounds=systemBounds };
        }
        finally
        {
            if (Api.SetThreadDpiAwarenessContext(previous)==0) throw Failure("RestoreDpiContext");
        }
    }

    private static IEnumerable<nint> EnumerateZOrder()
    {
        for (var handle=Api.GetTopWindow(0); handle != 0; handle=Api.GetWindow(handle,2))
            yield return handle;
    }

    private static IReadOnlyCollection<nint> DiscoverTaskbars()
    {
        // Class-specific lookup is independent of the two full-desktop walks,
        // including when this reader starts while the shell HWND is omitted.
        var found=new HashSet<nint>();
        foreach(var className in new[]{"Shell_TrayWnd","Shell_SecondaryTrayWnd"})
        {
            nint previous=0;
            while(true)
            {
                var handle=Api.FindWindowEx(0,previous,className,null);
                if(handle==0) break;
                if(!found.Add(handle)||found.Count>128) throw Failure("TaskbarDiscovery");
                previous=handle;
            }
        }
        return found;
    }

    internal List<NativeWindow> ReadOrderedWindows(HashSet<nint> handles, IEnumerable<nint> orderedHandles,
        IReadOnlyCollection<nint>? discoveredTaskbars = null)
    {
        // EnumWindows already supplied a bounded count. Allocate its snapshot
        // once instead of growing/copying the backing array at every size tier.
        var windows = new List<NativeWindow>(Math.Min(handles.Count,4096)); var visited = orderedVisited;visited.Clear();
        foreach (var handle in orderedHandles)
        {
            if (!visited.Add(handle) || visited.Count > 4096) throw Failure("ZOrderTraversal");
            var enumerated=handles.Remove(handle);
            // A shell taskbar can be absent from EnumWindows during application
            // transitions while still present in the current desktop z-order.
            // Validate that live HWND instead of publishing a false disappearance.
            // Other newly appearing windows keep the existing snapshot policy.
            if (!enumerated && !IsTaskbarHandle(handle)) continue;
            var window=ReadWindow(handle);
            if (window is not null && (enumerated || window.Taskbar)) windows.Add(window);
        }
        var candidates=new Dictionary<long,uint>(knownTaskbars);
        foreach(var handle in discoveredTaskbars??Array.Empty<nint>())
            if(!candidates.ContainsKey(handle.ToInt64()) && IsTaskbarHandle(handle) &&
                Api.GetWindowThreadProcessId(handle,out var pid)!=0 && pid!=0)
                candidates.Add(handle.ToInt64(),pid);
        var inactiveTaskbars=new List<NativeWindow>();
        foreach (var (value,pid) in candidates)
        {
            var alreadyCaptured=false;
            foreach(var captured in windows)if(captured.Handle==value){alreadyCaptured=true;break;}
            if (alreadyCaptured) continue;
            var handle=(nint)value;
            var taskbar=ReadWindow(handle);
            if (taskbar is null || !taskbar.Taskbar || taskbar.ProcessId!=pid) continue;
            // Search/game activation can omit a still-live shell HWND from BOTH
            // top-level walks. Query it directly, and reconstruct only its current
            // native sibling slot. Never equate enumeration absence with hiding.
            if (!taskbar.Visible || taskbar.Minimized || taskbar.Cloaked)
                inactiveTaskbars.Add(taskbar); // Not native-order anchors.
            else
                windows.Insert(FindTaskbarSlot(handle,windows),taskbar);
            handles.Remove(handle);
        }
        windows.AddRange(inactiveTaskbars); // Cannot support or occlude.
        if (handles.Any(Api.IsWindow)) throw Failure("ZOrderChanged");
        var nextTaskbars=windows.Where(w=>w.Taskbar).ToDictionary(w=>w.Handle,w=>w.ProcessId);
        if (nextTaskbars.Count>128) throw Failure("TaskbarLimit");
        knownTaskbars=nextTaskbars;
        // Retain only the last successful enumeration, never departed HWNDs.
        // All native fields are still queried on every pass before reuse.
        windowScratch.Clear();foreach(var window in windows)windowScratch[window.Handle]=window;
        (previousWindows,windowScratch)=(windowScratch,previousWindows);
        windowScratch.Clear();
        return windows;
    }

    private static int FindTaskbarSlot(nint handle,List<NativeWindow> windows)
    {
        // Child-window sibling order is not comparable with the desktop list.
        if (Api.GetAncestor(handle,2)!=handle) throw Failure("TaskbarOrderUnavailable");
        var above=FindAnchor(3);
        var below=FindAnchor(2);
        // Both anchors must describe the same unambiguous gap. A raced/unknown
        // order is an unavailable capture, not a successful taskbar disappearance.
        if ((above is null && below is null) ||
            (above??-1)+1!=(below??windows.Count)) throw Failure("TaskbarOrderUnavailable");
        return below??windows.Count;

        int? FindAnchor(uint direction)
        {
            var visited=new HashSet<nint>{handle};
            for (var h=Api.GetWindow(handle,direction);h!=0;h=Api.GetWindow(h,direction))
            {
                if (!visited.Add(h) || visited.Count>4096) throw Failure("TaskbarOrderUnavailable");
                var index=windows.FindIndex(w=>w.Handle==h.ToInt64());
                if (index<0) continue;
                if (!Api.IsWindow(h) || Api.GetWindowThreadProcessId(h,out var pid)==0 ||
                    pid!=windows[index].ProcessId) throw Failure("TaskbarOrderUnavailable");
                return index;
            }
            return null;
        }
    }

    private static bool IsTaskbarHandle(nint handle)
    {
        var name=GetClassNameBuffer();
        return Api.GetClassName(handle,name,name.Capacity)>0 &&
            name.ToString() is "Shell_TrayWnd" or "Shell_SecondaryTrayWnd";
    }

    internal NativeWindow? ReadWindow(nint handle)
    {
        if (!Api.IsWindow(handle)) return null;
        var thread=Api.GetWindowThreadProcessId(handle,out var pid);
        if (thread==0 || pid==0) return DestroyedOrThrow(handle,"OwnerPid");
        var name=GetClassNameBuffer();
        if (Api.GetClassName(handle,name,name.Capacity)==0) return DestroyedOrThrow(handle,"Class");
        previousWindows.TryGetValue(handle.ToInt64(),out var old);
        var className=old is not null&&name.Equals(old.ClassName.AsSpan())?old.ClassName:name.ToString();
        var excluded=handle==petHandle || className is "Progman" or "WorkerW" or "Shell_DesktopWnd";
        var visible=Api.IsWindowVisible(handle); var minimized=Api.IsIconic(handle);
        var processFileName=!excluded && visible && !minimized ? ReadProcessFileName(pid) : null;
        excluded |= string.Equals(processFileName,"Dororong.App.exe",StringComparison.OrdinalIgnoreCase) ||
            string.Equals(processFileName,"Dororong.exe",StringComparison.OrdinalIgnoreCase);
        var gamingOverlay=IsNvidiaGamingOverlay(handle,className,processFileName) ||
            IsDiscordGamingOverlay(handle,className,processFileName);
        var transientMenu = className == "#32768" ||
            (!excluded && visible && !minimized && className == "Chrome_WidgetWin_1" && IsChromiumMenuStyle(className,
                NativeMethods.GetWindowLongPtr(handle,-16).ToInt64(),
                NativeMethods.GetWindowLongPtr(handle,NativeMethods.GwlExStyle).ToInt64()));
        var taskbar=className is "Shell_TrayWnd" or "Shell_SecondaryTrayWnd";
        var bounds=new RectD(); var cloaked=false;
        if (!excluded && visible && !minimized)
        {
            var hr=Api.DwmCloaked(handle,14,out var cloak, sizeof(int));
            if (hr<0) return DestroyedOrThrow(handle,"DwmCloaked",hr);
            cloaked=cloak!=0;
            if (!cloaked)
            {
                hr=Api.DwmFrame(handle,9,out var frame,Marshal.SizeOf<Api.Rect>());
                if (hr<0) return DestroyedOrThrow(handle,"DwmFrame",hr);
                bounds=frame.ToRect();
                // Shell appbar geometry must describe the currently exposed HWND, not its reserved position.
                if (taskbar)
                {
                    if (!Api.GetWindowRect(handle,out var rectangle)) return DestroyedOrThrow(handle,"TaskbarRect");
                    bounds=rectangle.ToRect();
                }
            }
        }
        if (!Api.IsWindow(handle)) return null;
        if (Api.GetWindowThreadProcessId(handle,out var finalPid)==0 || finalPid!=pid)
            throw Failure("OwnerChanged");
        var support=!gamingOverlay&&!transientMenu&&className!="tooltips_class32";
        var horizontal=taskbar&&bounds.Width>bounds.Height;
        if(old is not null&&old.ProcessId==pid&&old.Bounds==bounds&&old.Visible==visible&&old.Minimized==minimized&&
            old.Cloaked==cloaked&&old.Excluded==excluded&&old.CanSupport==support&&old.Taskbar==taskbar&&
            old.HorizontalTaskbar==horizontal&&old.ClassName==className&&old.CanOcclude==!gamingOverlay&&old.IsTransientMenu==transientMenu)
            return old;
        return new(handle.ToInt64(),pid,bounds,visible,minimized,cloaked,excluded,support,taskbar,horizontal,className)
            { CanOcclude=!gamingOverlay, IsTransientMenu=transientMenu };
    }

    internal static bool IsChromiumMenuStyle(string className,long style,long extendedStyle)
    {
        // Observed Figma tray menu: Chrome_WidgetWin_1, 0x96000000/0x80088.
        // Do not exempt normal Chromium/Electron windows just by class or process.
        const long popup = 0x80000000L;
        const long childOrFrame = 0x40000000L | 0x00C00000L | 0x00040000L;
        const long layeredTopmostTool = 0x00080000L | 0x00000008L | 0x00000080L;
        const long appWindow = 0x00040000L;
        return className == "Chrome_WidgetWin_1" && (style & popup) != 0 &&
            (style & childOrFrame) == 0 && (extendedStyle & layeredTopmostTool) == layeredTopmostTool &&
            (extendedStyle & appWindow) == 0;
    }
    private static bool IsNvidiaGamingOverlay(nint handle,string className,string? processFileName)
    {
        if (className != "CEF-OSC-WIDGET" ||
            !string.Equals(processFileName,"NVIDIA Overlay.exe",StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            // The observed gaming HUD is layered and click-through. Same-process
            // opaque/interactive panels and other CEF apps remain physical windows.
            const long layeredAndTransparent=0x00080000L | 0x00000020L;
            return (NativeMethods.GetWindowLongPtr(handle,NativeMethods.GwlExStyle).ToInt64() & layeredAndTransparent)
                == layeredAndTransparent;
        }
        catch (Win32Exception)
        {
            // Optional overlay classification cannot be confirmed; retain ordinary
            // window behavior and still require the PID/DWM checks in ReadWindow.
            return false;
        }
    }
    private static bool IsDiscordGamingOverlay(nint handle,string className,string? processFileName)
    {
        if (className != "Chrome_WidgetWin_1" ||
            !string.Equals(processFileName,"Discord.exe",StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            return IsDiscordOverlayStyle(className,processFileName,
                NativeMethods.GetWindowLongPtr(handle,-16).ToInt64(),
                NativeMethods.GetWindowLongPtr(handle,NativeMethods.GwlExStyle).ToInt64());
        }
        catch (Win32Exception)
        {
            // Unconfirmed metadata keeps normal physical-window behavior.
            return false;
        }
    }

    internal static bool IsDiscordOverlayStyle(string className,string? processFileName,long style,long extendedStyle)
    {
        // Captured game HUD: Chrome_WidgetWin_1, Discord.exe, 0x14030000/0x800A8.
        // Never ignore Discord's normal window or an interactive/opaque panel.
        const long layeredClickThroughTopmostTool=0x00080000L|0x00000020L|0x00000008L|0x00000080L;
        const long childPopupOrFrame=0x40000000L|0x80000000L|0x00C00000L|0x00040000L;
        const long appWindow=0x00040000L;
        return className == "Chrome_WidgetWin_1" &&
            string.Equals(processFileName,"Discord.exe",StringComparison.OrdinalIgnoreCase) &&
            (style & childPopupOrFrame)==0 &&
            (extendedStyle & layeredClickThroughTopmostTool)==layeredClickThroughTopmostTool &&
            (extendedStyle & appWindow)==0;
    }

    private string? ReadProcessFileName(uint pid)
    {
        try
        {
            return Path.GetFileName(readProcessImagePath(pid));
        }
        catch (Win32Exception error) when (error.NativeErrorCode == 5) // ERROR_ACCESS_DENIED
        {
            // Executable identity is optional exclusion metadata. A protected process
            // remains an ordinary HWND; its PID and DWM geometry are still required.
            return null;
        }
        catch (Win32Exception error)
        {
            throw new DesktopMetadataException("ProcessIdentity",error.NativeErrorCode);
        }
    }
    // Scene capture revisits visible HWNDs every80ms. Keep only the native
    // output buffer per worker, never the returned identity: PID reuse and
    // changed process metadata must still be observed on every capture.
    [ThreadStatic] private static StringBuilder? processPathBuffer;
    [ThreadStatic] private static StringBuilder? classNameBuffer;
    internal static StringBuilder GetClassNameBuffer(){
        var buffer=classNameBuffer??=new StringBuilder(256);
        buffer.Clear();return buffer;
    }
    private static string ReadProcessImagePath(uint pid)
    {
        var process=Api.OpenProcess(0x1000,false,pid);
        if (process==0) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            var path=processPathBuffer??=new StringBuilder(32768);
            path.Clear();uint size=(uint)path.Capacity;
            if (!Api.QueryFullProcessImageName(process,0,path,ref size)) throw new Win32Exception(Marshal.GetLastWin32Error());
            return path.ToString();
        }
        finally { Api.CloseHandle(process); }
    }
    private static NativeWindow? DestroyedOrThrow(nint handle,string operation,int? code=null)
    {
        var error=code ?? Marshal.GetLastWin32Error();
        if (!Api.IsWindow(handle)) return null;
        throw new DesktopMetadataException(operation,error);
    }
    private static DesktopMetadataException Failure(string operation) => new(operation,Marshal.GetLastWin32Error());
    internal static class Api
    {
        [StructLayout(LayoutKind.Sequential)] internal struct Rect
        {
            internal int Left,Top,Right,Bottom;
            internal readonly RectD ToRect()=>new(Left,Top,Right-Left,Bottom-Top);
            internal static Rect From(RectD r)=>new(){Left=(int)r.X,Top=(int)r.Y,Right=(int)r.Right,Bottom=(int)r.Bottom};
        }
        [StructLayout(LayoutKind.Sequential)] internal struct MonitorInfo { internal int Size; internal Rect Monitor,Work; internal uint Flags; }
        [StructLayout(LayoutKind.Sequential)] internal struct AppbarData
        {
            internal uint Size; internal nint Handle; internal uint Callback,Edge; internal Rect Rectangle; internal nint Param;
            internal static AppbarData Create()=>new(){Size=(uint)Marshal.SizeOf<AppbarData>()};
        }
        internal delegate bool WindowCallback(nint handle,nint data);
        internal delegate bool MonitorCallback(nint handle,nint dc,ref Rect rectangle,nint data);
        [DllImport("user32.dll",SetLastError=true)] internal static extern nint SetThreadDpiAwarenessContext(nint context);
        [DllImport("user32.dll")] internal static extern nint GetThreadDpiAwarenessContext();
        [DllImport("user32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool EnumDisplayMonitors(nint dc,nint clip,MonitorCallback callback,nint data);
        [DllImport("user32.dll",EntryPoint="GetMonitorInfoW",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool GetMonitorInfo(nint monitor,ref MonitorInfo info);
        [DllImport("user32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool EnumWindows(WindowCallback callback,nint data);
        [DllImport("user32.dll")] internal static extern nint GetTopWindow(nint handle);
        [DllImport("user32.dll")] internal static extern nint GetWindow(nint handle,uint command);
        [DllImport("user32.dll")] internal static extern nint GetAncestor(nint handle,uint flags);
        [DllImport("user32.dll",EntryPoint="FindWindowExW",CharSet=CharSet.Unicode)] internal static extern nint FindWindowEx(nint parent,nint after,string className,string? title);
        [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindow(nint handle);
        [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool IsWindowVisible(nint handle);
        [DllImport("user32.dll")] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool IsIconic(nint handle);
        [DllImport("user32.dll",SetLastError=true)] internal static extern uint GetWindowThreadProcessId(nint handle,out uint pid);
        [DllImport("user32.dll",EntryPoint="GetClassNameW",CharSet=CharSet.Unicode,SetLastError=true)] internal static extern int GetClassName(nint handle,StringBuilder name,int size);
        [DllImport("user32.dll",SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool GetWindowRect(nint handle,out Rect rectangle);
        [DllImport("dwmapi.dll",EntryPoint="DwmGetWindowAttribute")] internal static extern int DwmFrame(nint handle,uint attribute,out Rect value,int size);
        [DllImport("dwmapi.dll",EntryPoint="DwmGetWindowAttribute")] internal static extern int DwmCloaked(nint handle,uint attribute,out int value,int size);
        [DllImport("shell32.dll")] internal static extern UIntPtr SHAppBarMessage(uint message,ref AppbarData data);
        [DllImport("kernel32.dll",SetLastError=true)] internal static extern nint OpenProcess(uint access,[MarshalAs(UnmanagedType.Bool)]bool inherit,uint pid);
        [DllImport("kernel32.dll",EntryPoint="QueryFullProcessImageNameW",CharSet=CharSet.Unicode,SetLastError=true)] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool QueryFullProcessImageName(nint process,uint flags,StringBuilder path,ref uint size);
        [DllImport("kernel32.dll")] [return:MarshalAs(UnmanagedType.Bool)] internal static extern bool CloseHandle(nint handle);
    }
}
internal sealed class DesktopSceneNative(IDesktopMetadataReader reader) : IDesktopSceneNative
{
    private Dictionary<long, (uint Pid, long Generation, TimeSpan? VisibleSince)> identities = new();
    private Dictionary<long, (uint Pid, long Generation, TimeSpan? VisibleSince)> identityScratch = new();
    private long generation;
    private long revision;
    private IReadOnlyList<DesktopWindow> previousWindows=Array.Empty<DesktopWindow>();
    internal DesktopSceneNative(nint petHandle) : this(new DesktopMetadataReader(petHandle)) { }
    public DesktopScene? Capture(TimeSpan now)
    {
        var metadata = reader.Read();
        if (metadata is null) return null;
        // This metadata source has one capture in flight. Neither dictionary
        // is published to readers; swap only after successful enumeration so
        // failures/absence/HWND reuse preserve the existing identity policy.
        var next = identityScratch;next.Clear();
        var windows = new List<DesktopWindow>(metadata.Windows.Count);
        foreach (var window in metadata.Windows)
        {
            if (window.Handle == 0) continue;
            if (!identities.TryGetValue(window.Handle, out var identity) || identity.Pid != window.ProcessId)
                identity = (window.ProcessId, ++generation, null);
            var bounds = window.Bounds;
            // Hover previews, tooltips and their separate decorative shadow
            // HWNDs can cross an existing surface edge. Classify SysShadow
            // independently of whether its tooltip is also enumerated.
            // They are neither perches nor holes in perches. Menus retain their
            // identity so geometry can preserve taskbars across app-owned menus.
            var hoverOverlay = window.ClassName is "TaskListThumbnailWnd" or "TaskListOverlayWnd" or "tooltips_class32" or "SysShadow";
            var support = window.CanSupport && !hoverOverlay;
            if (window.Taskbar)
            {
                bounds = TaskbarBounds(bounds,metadata.Monitors);
                var thickness = window.HorizontalTaskbar ? bounds.Height : bounds.Width;
                if (!window.Visible || window.Minimized || window.Cloaked || thickness <= 2)
                    identity.VisibleSince = null;
                else
                    identity.VisibleSince ??= now;
                support &= window.HorizontalTaskbar && identity.VisibleSince is { } since && now-since >= TimeSpan.FromMilliseconds(80);
            }
            next[window.Handle] = identity;
            var key=new SurfaceKey(window.Handle,window.ProcessId,identity.Generation);
            var occlude=window.CanOcclude&&!hoverOverlay;
            var menu=window.IsTransientMenu||window.ClassName=="#32768";
            var old=windows.Count<previousWindows.Count?previousWindows[windows.Count]:null;
            if(old is not null&&old.Key==key&&old.Bounds==bounds&&old.ZOrder==windows.Count&&old.Visible==window.Visible&&
                old.Minimized==window.Minimized&&old.Cloaked==window.Cloaked&&old.Excluded==window.Excluded&&
                old.CanSupport==support&&old.CanOcclude==occlude&&old.Taskbar==window.Taskbar&&
                old.HorizontalTaskbar==window.HorizontalTaskbar&&old.IsTransientMenu==menu)windows.Add(old);
            else windows.Add(new(key,bounds,windows.Count,window.Visible,window.Minimized,window.Cloaked,window.Excluded,
                support,occlude,window.Taskbar,window.HorizontalTaskbar){IsTransientMenu=menu});
        }
        identityScratch=identities;identities=next; // Only successful absence retires an observed identity.
        previousWindows=windows;
        return new(++revision,now,Array.AsReadOnly(metadata.Monitors.ToArray()),windows.AsReadOnly());
    }
    private static RectD TaskbarBounds(RectD bounds,IReadOnlyList<DesktopMonitor> monitors){
        // Keep captured LINQ locals out of the per-window loop (ordinary
        // windows also allocated its closure). Preserve first-largest ties.
        var best=default(RectD);var area=0d;
        foreach(var monitor in monitors){var clip=Intersect(bounds,monitor.Bounds);
            if(clip.Width>0&&clip.Height>0&&clip.Width*clip.Height>area){best=clip;area=clip.Width*clip.Height;}}
        return best;
    }
    private static RectD Intersect(RectD a, RectD b) => new(Math.Max(a.X,b.X),Math.Max(a.Y,b.Y),
        Math.Max(0,Math.Min(a.Right,b.Right)-Math.Max(a.X,b.X)),Math.Max(0,Math.Min(a.Bottom,b.Bottom)-Math.Max(a.Y,b.Y)));
}

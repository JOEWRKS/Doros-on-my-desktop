using System.Runtime.InteropServices;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;

namespace Dororong.Mac.Native;

// Metadata and cursor position only: no event tap, key state, screen image,
// AX actions, other-window movement, titles, or permission-request side effects.
internal sealed class QuartzDesktop
{
    private const string CG="/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";
    private const string CF="/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
    private const string ObjC="/usr/lib/libobjc.A.dylib";
    private readonly MacSceneMapper mapper=new();
    private string captureStatus="NotRead";
    internal string CaptureStatus=>Volatile.Read(ref captureStatus);
    private DesktopScene? CaptureFailed(string reason){Volatile.Write(ref captureStatus,reason);return null;}
    internal QuartzDesktop(){if(!OperatingSystem.IsMacOS())throw new PlatformNotSupportedException("Quartz requires macOS.");}
    internal PointerSample Pointer()
    {
        var e=CGEventCreate(0);if(e==0)return PointerSample.Unavailable;
        try{var p=CGEventGetLocation(e);return double.IsFinite(p.X)&&double.IsFinite(p.Y)?new(true,new(p.X,p.Y)):PointerSample.Unavailable;}
        finally{CFRelease(e);}
    }
    internal bool PrimaryDown()=>CGEventSourceButtonState(1,0);
    internal double PrimaryHeight=>CGDisplayBounds(CGMainDisplayID()).Height;
    internal IReadOnlyList<DesktopMonitor> Monitors()
    {
        var ids=new uint[64];
        if(CGGetActiveDisplayList((uint)ids.Length,ids,out var count)!=0||count==0||count>ids.Length)throw new InvalidOperationException("DisplayUnavailable");
        return ids.Take((int)count).Select(id=>{var r=CGDisplayBounds(id);return new DesktopMonitor(id,new(r.X,r.Y,r.Width,r.Height));}).ToArray();
    }
    internal DesktopScene? Capture(TimeSpan now)
    {
        using var pool=new Pool();
        IReadOnlyList<DesktopMonitor> displays;
        try{displays=Monitors();}catch{Volatile.Write(ref captureStatus,"DisplayUnavailable");throw;}
        var list=CGWindowListCopyWindowInfo(1|16,0); // on-screen, excluding desktop elements
        if(list==0)return CaptureFailed("WindowListUnavailable");
        using var keys=new Keys("kCGWindowNumber","kCGWindowOwnerPID","kCGWindowBounds","kCGWindowLayer","kCGWindowAlpha");
        try
        {
            var count=CFArrayGetCount(list);if(count<0||count>10000)return CaptureFailed("InvalidWindowCount");
            var rows=new List<MacWindowRow>();var dockPids=new Dictionary<uint,bool>();
            for(nint i=0;i<count;i++)
            {
                var entry=CFArrayGetValueAtIndex(list,i);
                if(!Number(entry,keys[0],out var id)||!Number(entry,keys[1],out var pid)||!Number(entry,keys[3],out var layer))return CaptureFailed("MissingWindowIdentityOrLayer");
                if(pid<=0||pid>uint.MaxValue||layer<int.MinValue||layer>int.MaxValue)return CaptureFailed("InvalidWindowIdentityOrLayer");
                if(pid==Environment.ProcessId)continue;
                var bound=CFDictionaryGetValue(entry,keys[2]);
                if(bound==0||!CGRectMakeWithDictionaryRepresentation(bound,out var rect))return CaptureFailed("InvalidWindowBounds");
                var alphaRef=CFDictionaryGetValue(entry,keys[4]);double alpha=1;
                if(alphaRef!=0&&!CFNumberGetDouble(alphaRef,6,out alpha))return CaptureFailed("InvalidWindowAlpha");
                if(!dockPids.TryGetValue((uint)pid,out var dock))
                {
                    var app=SendPid(objc_getClass("NSRunningApplication"),Sel("runningApplicationWithProcessIdentifier:"),(int)pid);
                    var bundle=app==0?0:Send(app,Sel("bundleIdentifier"));
                    var utf8=bundle==0?0:Send(bundle,Sel("UTF8String"));
                    dock=utf8!=0&&Marshal.PtrToStringUTF8(utf8)=="com.apple.dock";dockPids[(uint)pid]=dock;
                }
                rows.Add(new(id,(uint)pid,new(rect.X,rect.Y,rect.Width,rect.Height),(int)layer,alpha,true,dock));
            }
            var scene=mapper.Build(now,displays,rows,(uint)Environment.ProcessId,CGWindowLevelForKey(7),CGWindowLevelForKey(11));
            Volatile.Write(ref captureStatus,"Success");return scene;
        }
        catch(Exception e){Volatile.Write(ref captureStatus,e.GetType().Name);throw;}
        finally{CFRelease(list);}
    }
    // Opt-in boundary diagnosis only. Never substitute requested placement for
    // actual AppKit readback, and never query titles, images or accessibility.
    internal object PlacementEvidence(nint window)
    {
        static CGRect Rect(nint receiver,string selector)
        {
            if(RuntimeInformation.ProcessArchitecture==Architecture.X64){SendRectStret(out var r,receiver,Sel(selector));return r;}
            return SendRect(receiver,Sel(selector));
        }
        var frame=Rect(window,"frame");var height=PrimaryHeight;
        var screens=Send(objc_getClass("NSScreen"),Sel("screens"));
        var first=Send(screens,Sel("firstObject"));
        var cocoaPrimary=first==0?default:Rect(first,"frame");
        return new {
            primaryHeight=height, cocoaPrimary=new RectD(cocoaPrimary.X,cocoaPrimary.Y,cocoaPrimary.Width,cocoaPrimary.Height),
            actualQuartz=new RectD(frame.X,height-frame.Y-frame.Height,frame.Width,frame.Height),
            styleMask=(long)Send(window,Sel("styleMask"))
        };
    }
    internal static nint Sel(string name)=>sel_registerName(name);
    internal static void ConfigureWindow(nint window,bool clickThrough)
    {
        SendBool(window,Sel("setIgnoresMouseEvents:"),clickThrough);
        SendBool(window,Sel("setHidesOnDeactivate:"),false);
        SendInt(window,Sel("setLevel:"),CGWindowLevelForKey(7)+1);
        SendInt(window,Sel("setCollectionBehavior:"),(1<<0)|(1<<8)); // all Spaces + full-screen auxiliary
    }
    internal static void ClickThrough(nint window,bool ignore)=>SendBool(window,Sel("setIgnoresMouseEvents:"),ignore);
    internal void Move(nint window,PointD topLeft)
    {
        var p=MacSceneMapper.ToCocoaTopLeft(topLeft,PrimaryHeight);
        SendPoint(window,Sel("setFrameTopLeftPoint:"),new(p.X,p.Y));
    }
    internal void MoveIfChanged(nint window,PointD topLeft,WindowUpdateState state)
    {
        // Compare Cocoa coordinates too: a primary-display height change must
        // still move the window even when Quartz topLeft did not change.
        var p=MacSceneMapper.ToCocoaTopLeft(topLeft,PrimaryHeight);
        if(state.PositionChanged(p))SendPoint(window,Sel("setFrameTopLeftPoint:"),new(p.X,p.Y));
    }
    private static bool Number(nint dict,nint key,out long value)
    {var number=CFDictionaryGetValue(dict,key);value=0;return number!=0&&CFNumberGetInt(number,4,out value);}
    [StructLayout(LayoutKind.Sequential)] internal readonly record struct CGPoint(double X,double Y);
    [StructLayout(LayoutKind.Sequential)] private readonly record struct CGRect(double X,double Y,double Width,double Height);
    private sealed class Keys:IDisposable
    {
        private readonly nint[] values;
        internal Keys(params string[] names)=>values=names.Select(n=>CFStringCreateWithCString(0,n,0x08000100)).ToArray();
        internal nint this[int i]=>values[i];
        public void Dispose(){foreach(var value in values)if(value!=0)CFRelease(value);}
    }
    private sealed class Pool:IDisposable
    {
        private readonly nint value=Send(Send(objc_getClass("NSAutoreleasePool"),Sel("alloc")),Sel("init"));
        public void Dispose(){if(value!=0)Send(value,Sel("drain"));}
    }
    [DllImport(CG)] private static extern nint CGEventCreate(nint source);
    [DllImport(CG)] private static extern CGPoint CGEventGetLocation(nint e);
    [DllImport(CG)] [return:MarshalAs(UnmanagedType.I1)] private static extern bool CGEventSourceButtonState(int state,uint button);
    [DllImport(CG)] private static extern int CGGetActiveDisplayList(uint max,[Out]uint[] ids,out uint count);
    [DllImport(CG)] private static extern CGRect CGDisplayBounds(uint id);
    [DllImport(CG)] private static extern uint CGMainDisplayID();
    [DllImport(CG)] private static extern nint CGWindowListCopyWindowInfo(uint options,uint relative);
    [DllImport(CG)] [return:MarshalAs(UnmanagedType.I1)] private static extern bool CGRectMakeWithDictionaryRepresentation(nint dictionary,out CGRect rectangle);
    [DllImport(CG)] private static extern int CGWindowLevelForKey(int key);
    [DllImport(CF)] private static extern void CFRelease(nint value);
    [DllImport(CF)] private static extern nint CFArrayGetCount(nint array);
    [DllImport(CF)] private static extern nint CFArrayGetValueAtIndex(nint array,nint index);
    [DllImport(CF)] private static extern nint CFDictionaryGetValue(nint dictionary,nint key);
    [DllImport(CF,EntryPoint="CFNumberGetValue")] [return:MarshalAs(UnmanagedType.I1)] private static extern bool CFNumberGetInt(nint number,int type,out long value);
    [DllImport(CF,EntryPoint="CFNumberGetValue")] [return:MarshalAs(UnmanagedType.I1)] private static extern bool CFNumberGetDouble(nint number,int type,out double value);
    [DllImport(CF)] private static extern nint CFStringCreateWithCString(nint allocator,[MarshalAs(UnmanagedType.LPUTF8Str)]string text,uint encoding);
    [DllImport(ObjC)] private static extern nint objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)]string name);
    [DllImport(ObjC)] private static extern nint sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)]string name);
    [DllImport(ObjC,EntryPoint="objc_msgSend")] private static extern nint Send(nint receiver,nint selector);
    [DllImport(ObjC,EntryPoint="objc_msgSend")] private static extern nint SendPid(nint receiver,nint selector,int pid);
    [DllImport(ObjC,EntryPoint="objc_msgSend")] private static extern void SendBool(nint receiver,nint selector,[MarshalAs(UnmanagedType.I1)]bool value);
    [DllImport(ObjC,EntryPoint="objc_msgSend")] private static extern void SendInt(nint receiver,nint selector,nint value);
    [DllImport(ObjC,EntryPoint="objc_msgSend")] private static extern void SendPoint(nint receiver,nint selector,CGPoint point);
    [DllImport(ObjC,EntryPoint="objc_msgSend")] private static extern CGRect SendRect(nint receiver,nint selector);
    [DllImport(ObjC,EntryPoint="objc_msgSend_stret")] private static extern void SendRectStret(out CGRect result,nint receiver,nint selector);
}

using System.Runtime.InteropServices;

namespace Dororong.Mac.Native;

// Only the two owned, borderless overlays may put transparent frame padding
// above a screen. The controller still constrains the visible pet. Menus and
// every other Avalonia/NSWindow instance keep their original constraint policy.
internal sealed class OverlayFramePolicy(OverlayFramePolicy.IRuntime runtime)
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly record struct Frame(double X,double Y,double Width,double Height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate Frame Constraint(nint self,nint command,Frame proposed,nint screen);
    // Root for the process lifetime: registered Objective-C classes outlive any
    // individual overlay. No captured window, render buffer, allocation or throw.
    internal static readonly Constraint Callback=KeepProposedFrame;
    internal static long Calls {get;private set;}
    internal static Frame KeepProposedFrame(nint self,nint command,Frame proposed,nint screen){Calls++;return proposed;}
    internal interface IRuntime
    {
        nint GetClass(nint window);
        string Name(nint type);
        nint CreateSubclass(nint original,Constraint callback);
        void SetClass(nint window,nint type);
    }
    private readonly Dictionary<nint,nint> types=[]; // at most AvnWindow + AvnPanel
    internal void Apply(nint window)
    {
        if(window==0)throw new ArgumentException("Overlay window is missing.");
        var original=runtime.GetClass(window);
        if(types.ContainsValue(original))return;
        if(runtime.Name(original) is not ("AvnWindow" or "AvnPanel"))
            throw new InvalidOperationException("Unsupported overlay window class.");
        if(!types.TryGetValue(original,out var type)){
            if(types.Count>=2)throw new InvalidOperationException("Unexpected overlay class count.");
            type=runtime.CreateSubclass(original,Callback);
            if(type==0)throw new InvalidOperationException("Overlay constraint registration failed.");
            types.Add(original,type);
        }
        runtime.SetClass(window,type);
    }
    private static readonly OverlayFramePolicy native=new(new NativeRuntime());
    internal static void Configure(nint window)
    {
        Cocoa.RequireMainThread();
        // The released tester is arm64; do not silently assume Intel CGRect ABI.
        if(RuntimeInformation.ProcessArchitecture!=Architecture.Arm64)
            throw new PlatformNotSupportedException("Overlay frame policy requires Apple Silicon.");
        native.Apply(window);
    }
    private sealed class NativeRuntime:IRuntime
    {
        public nint GetClass(nint window)=>object_getClass(window);
        public string Name(nint type)=>Marshal.PtrToStringUTF8(class_getName(type))??"";
        public nint CreateSubclass(nint original,Constraint callback)
        {
            var selector=Cocoa.Sel("constrainFrameRect:toScreen:");
            var method=class_getInstanceMethod(original,selector);
            if(method==0)throw new InvalidOperationException("Native frame constraint method missing.");
            var encoding=Marshal.PtrToStringUTF8(method_getTypeEncoding(method));
            if(encoding is null)throw new InvalidOperationException("Native frame constraint signature missing.");
            var type=Cocoa.objc_allocateClassPair(original,"DororongFreeFrame_"+Name(original)+"_V1",0);
            if(type==0)throw new InvalidOperationException("Overlay subclass allocation failed.");
            if(!Cocoa.class_addMethod(type,selector,Marshal.GetFunctionPointerForDelegate(callback),encoding)){
                objc_disposeClassPair(type);
                throw new InvalidOperationException("Overlay constraint method registration failed.");
            }
            // Inherits Avalonia layout/event/lifetime behavior. No global swizzle,
            // no ivar addition, no change to the original class's method table.
            Cocoa.objc_registerClassPair(type);return type;
        }
        public void SetClass(nint window,nint type)=>object_setClass(window,type);
        [DllImport(Cocoa.ObjC)]private static extern nint object_getClass(nint obj);
        [DllImport(Cocoa.ObjC)]private static extern nint object_setClass(nint obj,nint type);
        [DllImport(Cocoa.ObjC)]private static extern nint class_getName(nint type);
        [DllImport(Cocoa.ObjC)]private static extern nint class_getInstanceMethod(nint type,nint selector);
        [DllImport(Cocoa.ObjC)]private static extern nint method_getTypeEncoding(nint method);
        [DllImport(Cocoa.ObjC)]private static extern void objc_disposeClassPair(nint type);
    }
}

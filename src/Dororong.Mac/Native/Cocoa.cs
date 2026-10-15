using System.Runtime.InteropServices;
namespace Dororong.Mac.Native;

// Pointer/NSInteger/BOOL/CGSize signatures are explicit: no varargs or CGRect
// returns, whose ABI differs between Intel and Apple Silicon.
internal static class Cocoa
{
    internal const string ObjC="/usr/lib/libobjc.A.dylib";
    private static readonly Lazy<nint> foundation=new(()=>NativeLibrary.Load("/System/Library/Frameworks/Foundation.framework/Foundation"));
    internal static nint Sel(string name)=>sel_registerName(name);
    internal static nint Class(string name){_=foundation.Value;return objc_getClass(name);}
    internal static nint Get(nint obj,string name)=>Send(obj,Sel(name));
    internal static nint String(string text)=>StringSend(Class("NSString"),Sel("stringWithUTF8String:"),text);
    internal static string? Text(nint value)=>value==0?null:Marshal.PtrToStringUTF8(Get(value,"UTF8String"));
    internal static void RequireMainThread(){if(!OperatingSystem.IsMacOS())throw new PlatformNotSupportedException();if(Get(Class("NSThread"),"isMainThread")==0)throw new InvalidOperationException("AppKit requires the main thread.");}
    [StructLayout(LayoutKind.Sequential)]internal readonly record struct Size(double Width,double Height);
    [DllImport(ObjC)]private static extern nint objc_getClass([MarshalAs(UnmanagedType.LPUTF8Str)]string name);
    [DllImport(ObjC)]private static extern nint sel_registerName([MarshalAs(UnmanagedType.LPUTF8Str)]string name);
    [DllImport(ObjC,EntryPoint="objc_msgSend")]internal static extern nint Send(nint receiver,nint selector);
    [DllImport(ObjC,EntryPoint="objc_msgSend")]internal static extern nint Ptr(nint receiver,nint selector,nint value);
    [DllImport(ObjC,EntryPoint="objc_msgSend")]internal static extern nint Double(nint receiver,nint selector,double value);
    [DllImport(ObjC,EntryPoint="objc_msgSend")]internal static extern void SetSize(nint receiver,nint selector,Size value);
    [DllImport(ObjC,EntryPoint="objc_msgSend")]internal static extern void Bool(nint receiver,nint selector,[MarshalAs(UnmanagedType.I1)]bool value);
    [DllImport(ObjC,EntryPoint="objc_msgSend")]private static extern nint StringSend(nint receiver,nint selector,[MarshalAs(UnmanagedType.LPUTF8Str)]string text);
    [DllImport(ObjC,EntryPoint="objc_msgSend")]internal static extern nint Bytes(nint receiver,nint selector,byte[] bytes,nuint length);
    [DllImport(ObjC,EntryPoint="objc_msgSend")][return:MarshalAs(UnmanagedType.I1)]internal static extern bool Error(nint receiver,nint selector,out nint error);
    [DllImport(ObjC)]internal static extern nint objc_allocateClassPair(nint superclass,[MarshalAs(UnmanagedType.LPUTF8Str)]string name,nuint extra);
    [DllImport(ObjC)][return:MarshalAs(UnmanagedType.I1)]internal static extern bool class_addMethod(nint cls,nint selector,nint imp,[MarshalAs(UnmanagedType.LPUTF8Str)]string types);
    [DllImport(ObjC)]internal static extern void objc_registerClassPair(nint cls);
}

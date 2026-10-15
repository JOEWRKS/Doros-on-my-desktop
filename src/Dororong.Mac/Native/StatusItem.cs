using System.Runtime.InteropServices;
namespace Dororong.Mac.Native;

internal sealed class StatusItem:IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]private delegate void ActionImp(nint self,nint selector,nint sender);
    private static readonly ActionImp thunk=Dispatch;
    private static readonly Dictionary<nint,StatusItem> live=[]; // UI thread only; roots callbacks.
    private static nint targetClass;
    private nint bar,item,button,target,image;
    private readonly Action clicked;private readonly Action<Exception> failure;
    internal StatusItem(byte[] png,Action clicked,Action<Exception> failure)
    {
        Cocoa.RequireMainThread();this.clicked=clicked;this.failure=failure;
        try
        {
            if(targetClass==0)
            {
                targetClass=Cocoa.objc_allocateClassPair(Cocoa.Class("NSObject"),"DororongStatusActionV1",0);
                if(targetClass==0||!Cocoa.class_addMethod(targetClass,Cocoa.Sel("doroClick:"),Marshal.GetFunctionPointerForDelegate(thunk),"v@:@"))throw new InvalidOperationException("Status target registration failed.");
                Cocoa.objc_registerClassPair(targetClass);
            }
            target=Cocoa.Get(Cocoa.Get(targetClass,"alloc"),"init");live.Add(target,this);
            bar=Cocoa.Get(Cocoa.Class("NSStatusBar"),"systemStatusBar");item=Cocoa.Double(bar,Cocoa.Sel("statusItemWithLength:"),28);
            if(item==0)throw new InvalidOperationException("Status item unavailable.");Cocoa.Get(item,"retain");button=Cocoa.Get(item,"button");
            if(button==0)throw new InvalidOperationException("Status button unavailable.");
            var data=Cocoa.Bytes(Cocoa.Class("NSData"),Cocoa.Sel("dataWithBytes:length:"),png,(nuint)png.Length);
            image=Cocoa.Ptr(Cocoa.Get(Cocoa.Class("NSImage"),"alloc"),Cocoa.Sel("initWithData:"),data);
            if(image==0)throw new InvalidOperationException("Head icon decode failed.");
            Cocoa.SetSize(image,Cocoa.Sel("setSize:"),new(22,22));Cocoa.Bool(image,Cocoa.Sel("setTemplate:"),false);
            Cocoa.Ptr(button,Cocoa.Sel("setImage:"),image);Cocoa.Ptr(button,Cocoa.Sel("setToolTip:"),Cocoa.String("도로롱이 내 컴에?!"));
            Cocoa.Ptr(button,Cocoa.Sel("setAccessibilityLabel:"),Cocoa.String("도로롱 메뉴"));
            Cocoa.Ptr(button,Cocoa.Sel("setTarget:"),target);Cocoa.Ptr(button,Cocoa.Sel("setAction:"),Cocoa.Sel("doroClick:"));
            Cocoa.Ptr(button,Cocoa.Sel("sendActionOn:"),(1<<2)|(1<<4)); // leftMouseUp | rightMouseUp, once per click.
        }
        catch{Dispose();throw;}
    }
    private static void Dispatch(nint self,nint selector,nint sender)
    {
        if(!live.TryGetValue(self,out var owner))return;
        try{owner.clicked();}catch(Exception ex){try{owner.failure(ex);}catch{/* never unwind into Objective-C */}}
    }
    internal void Highlight(bool value){if(button!=0)Cocoa.Bool(button,Cocoa.Sel("highlight:"),value);}
    public void Dispose()
    {
        if(button!=0){Cocoa.Ptr(button,Cocoa.Sel("setTarget:"),0);Cocoa.Ptr(button,Cocoa.Sel("setAction:"),0);button=0;}
        if(item!=0){Cocoa.Ptr(bar,Cocoa.Sel("removeStatusItem:"),item);Cocoa.Get(item,"release");item=0;}
        if(target!=0){live.Remove(target);Cocoa.Get(target,"release");target=0;}
        if(image!=0){Cocoa.Get(image,"release");image=0;}
    }
}

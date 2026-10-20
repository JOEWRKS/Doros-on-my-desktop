using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Interop;
using Dororong.Core.Behavior;

namespace Dororong.App.Controls;

// A non-activating click-through cursor companion. Never replace OS cursor
// schemes, suppress other apps' input, or leave a system cursor changed on crash.
internal sealed class SnackCursorWindow : Window
{
    private bool closed;
    private readonly Image image=new() { Stretch=Stretch.Uniform };
    internal SnackCursorWindow()
    {
        Width=32;Height=32;WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;
        AllowsTransparency=true;Background=Brushes.Transparent;Topmost=true;
        ShowActivated=false;ShowInTaskbar=false;IsHitTestVisible=false;
        Content=image;
        SourceInitialized+=(_,_)=>
        {
            var h=new WindowInteropHelper(this).Handle;
            WindowStyleManager.ApplyNoActivateToolWindow(h);
            var style=NativeMethods.GetWindowLongPtr(h,NativeMethods.GwlExStyle).ToInt64();
            NativeMethods.SetWindowLongPtr(h,NativeMethods.GwlExStyle,new IntPtr(style|0x20));
        };
        Closed+=(_,_)=>closed=true;
    }
    internal void Update(PointerSample pointer,bool visible)
    {
        if(closed)return;
        if(!visible||!pointer.IsAvailable) { if(IsVisible)Hide();return; }
        if(!IsVisible){image.Source??=SnackImage.Source;Show();}
        var h=new WindowInteropHelper(this).Handle;
        if(pointer.ScreenPixelPosition is { } pixels)
        {
            var dpi=VisualTreeHelper.GetDpi(this);
            NativeMethods.SetWindowPosChecked(h,new IntPtr(-1),(int)Math.Round(pixels.X-16*dpi.DpiScaleX),
                (int)Math.Round(pixels.Y-16*dpi.DpiScaleY),0,0,NativeMethods.SwpNoSize|NativeMethods.SwpNoActivate);
        }
        else { Left=pointer.Position.X-16;Top=pointer.Position.Y-16; }
    }
}

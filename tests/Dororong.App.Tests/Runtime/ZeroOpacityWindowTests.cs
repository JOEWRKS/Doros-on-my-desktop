using System.Runtime.InteropServices;
using Dororong.App.Interop;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public sealed class ZeroOpacityWindowTests
{
    [Theory]
    [InlineData(0, 2, false)] [InlineData(0, 3, false)]
    [InlineData(1, 2, true)] [InlineData(128, 2, true)] [InlineData(255, 2, true)]
    [InlineData(0, 1, true)]
    public void Only_confirmed_global_zero_alpha_is_nonphysical(byte alpha,uint flags,bool physical)
    {
        using var window=new NvidiaOverlayTests.OffscreenWindow("DoroOpacityProbe",0x80080);
        Assert.True(SetLayeredWindowAttributes(window.Handle,0,alpha,flags));
        var reader=new DesktopMetadataReader(0,_=>@"C:\OtherApp.exe");
        var result=reader.ReadWindow(window.Handle)!;
        Assert.Equal(physical,result.CanSupport);
        Assert.Equal(physical,result.CanOcclude);
        Assert.False(result.Excluded); // Identity remains tracked through opacity changes.
    }

    [Fact]
    public void Unknown_layering_attributes_are_not_assumed_transparent()
    {
        using var window=new NvidiaOverlayTests.OffscreenWindow("DoroUnknownAlphaProbe",0x80080);
        var result=new DesktopMetadataReader(0,_=>@"C:\OtherApp.exe").ReadWindow(window.Handle)!;
        Assert.True(result.CanSupport);Assert.True(result.CanOcclude);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Invisible_overlay_z_order_cannot_remove_window_or_taskbar_but_opaque_window_can(bool taskbar)
    {
        using var window=new NvidiaOverlayTests.OffscreenWindow("CEF-OSC-WIDGET",0x8080080);
        Assert.True(SetLayeredWindowAttributes(window.Handle,0,0,2));
        var reader=new DesktopMetadataReader(0,_=>@"C:\NVIDIA Overlay.exe");
        var metadata=new Desktop(reader,window.Handle,taskbar);
        var native=new DesktopSceneNative(metadata);native.Capture(TimeSpan.Zero);
        var motion=new PlatformMotion();var contact=new FootContact(20,100,100,10);
        var p=new PointD(100,taskbar?940:300);
        for(var i=1;i<=30;i++){
            metadata.Front=i%2==0;
            var scene=native.Capture(TimeSpan.FromMilliseconds(i*80))!;
            var pose=motion.Advance(new(TimeSpan.FromMilliseconds(80),p,p,contact,PlatformGeometry.Build(scene,90),false,true));
            Assert.Equal(PlatformPhase.Supported,pose.Phase);Assert.Equal(p,pose.Position);
        }
        Assert.True(SetLayeredWindowAttributes(window.Handle,0,255,2));metadata.Front=true;
        var opaque=native.Capture(TimeSpan.FromSeconds(3))!;
        var fallen=motion.Advance(new(TimeSpan.FromMilliseconds(80),p,p,contact,PlatformGeometry.Build(opaque,90),false,true));
        Assert.Equal(PlatformPhase.Falling,fallen.Phase);Assert.True(fallen.Position.Y>p.Y);
    }

    private sealed class Desktop(DesktopMetadataReader reader,nint overlay,bool taskbar):IDesktopMetadataReader
    {
        internal bool Front;
        public NativeDesktop Read(){
            var layer=reader.ReadWindow(overlay)! with {Bounds=new(0,0,1920,1080)};
            var owner=new NativeWindow(123,321,new(0,taskbar?1040:400,1920,taskbar?40:300),
                Taskbar:taskbar,HorizontalTaskbar:taskbar,ClassName:taskbar?"Shell_TrayWnd":"OrdinaryWindow");
            return new([new(1,new(0,0,1920,1080))],Front?[layer,owner]:[owner,layer]);
        }
    }

    [DllImport("user32.dll")]
    [return:MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetLayeredWindowAttributes(nint window,uint key,byte alpha,uint flags);
}

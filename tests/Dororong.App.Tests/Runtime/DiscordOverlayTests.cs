using System.ComponentModel;
using Dororong.App.Interop;
using Dororong.App.Runtime;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public class DiscordOverlayTests
{
    [Theory]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14030000, 0x800A8, true)]
    [InlineData("Chrome_WidgetWin_1", "DISCORD.EXE", 0x14030000, 0x80800A8, true)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14CF0000, 0x40000, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14030000, 0x80088, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14030000, 0xA8, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14030000, 0x800A0, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14030000, 0x80028, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14030000, 0xC00A8, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14C30000, 0x800A8, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x14070000, 0x800A8, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x54030000, 0x800A8, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe", 0x94030000, 0x800A8, false)]
    [InlineData("Chrome_WidgetWin_1", "Figma.exe", 0x14030000, 0x800A8, false)]
    [InlineData("Chrome_WidgetWin_1", "Discord.exe.backup", 0x14030000, 0x800A8, false)]
    [InlineData("Chrome_WidgetWin_1", null, 0x14030000, 0x800A8, false)]
    [InlineData("OtherClass", "Discord.exe", 0x14030000, 0x800A8, false)]
    public void Classification_requires_confirmed_overlay_not_ordinary_discord(
        string className,string? executable,long style,long extendedStyle,bool expected)
        => Assert.Equal(expected,DesktopMetadataReader.IsDiscordOverlayStyle(className,executable,style,extendedStyle));

    [Theory]
    [InlineData("Discord.exe", 0x800A8u, true)]
    [InlineData("Discord.exe", 0x80088u, false)]
    [InlineData("Discord.exe", 0x40000u, false)]
    [InlineData("Other.exe", 0x800A8u, false)]
    [InlineData(null, 0x800A8u, false)]
    public void Native_reader_preserves_normal_window_support_and_occlusion(
        string? executable,uint extendedStyle,bool overlay)
    {
        using var window=new NvidiaOverlayTests.OffscreenWindow("Chrome_WidgetWin_1",extendedStyle,0x14030000);
        var reader=new DesktopMetadataReader(0,_=>executable is null
            ? throw new Win32Exception(5) : @"C:\Apps\"+executable);
        var metadata=new CapturedDesktop(reader,window.Handle);
        var source=new DesktopSceneSource(new DesktopSceneNative(metadata));
        source.Read(TimeSpan.Zero);
        var read=source.Read(TimeSpan.FromMilliseconds(80));
        Assert.Equal(SceneReadHealth.Fresh,read.Health);
        Assert.Null(source.LastFailure);
        var scene=read.Scene!;
        var classified=scene.Windows[0];
        Assert.False(classified.Excluded);
        Assert.False(classified.IsTransientMenu);
        Assert.Equal(!overlay,classified.CanSupport);
        Assert.Equal(!overlay,classified.CanOcclude);
        var surfaces=PlatformGeometry.Build(scene,63);
        var primary=Assert.Single(surfaces.Where(s=>s.Kind==PlatformKind.Taskbar&&s.MonitorId==1));
        Assert.Equal(0,primary.Left);
        Assert.Equal(overlay?1920:1,primary.Right);
        var secondary=Assert.Single(surfaces.Where(s=>s.Kind==PlatformKind.Taskbar&&s.MonitorId==2));
        Assert.Equal(overlay?1920:1921,secondary.Left);
        Assert.Equal(3840,secondary.Right);
        Assert.Equal(!overlay,PlatformGeometry.Build(scene,20).Any(s=>s.Kind==PlatformKind.Window&&s.Key.Handle==window.Handle.ToInt64()));
    }

    [Fact]
    public void Repeated_captured_restore_maximize_keeps_taskbar_contact()
    {
        using var window=new NvidiaOverlayTests.OffscreenWindow("Chrome_WidgetWin_1",0x800A8,0x14030000);
        var reader=new DesktopMetadataReader(0,_=>@"C:\Apps\Discord.exe");
        var metadata=new CapturedDesktop(reader,window.Handle){OverlayPresent=false};
        var source=new DesktopSceneSource(new DesktopSceneNative(metadata));
        source.Read(TimeSpan.Zero);
        var motion=new PlatformMotion();
        var position=new PointD(400,929);
        var foot=new FootContact(20,45,111,48);
        for(var ms=80;ms<=40000;ms+=80)
        {
            // Four captured overlapping intervals; window mode changes precede
            // the HUD resize. Preserve stable bars and their relative z-order.
            metadata.OverlayPresent=ms>=1014&&ms<5416||ms>=11775&&ms<16727||
                ms>=23073&&ms<27571||ms>=32290&&ms<36409;
            metadata.Restored=ms>=521&&ms<5321||ms>=11775&&ms<16633||
                ms>=22982&&ms<27464||ms>=32197&&ms<36409;
            var read=source.Read(TimeSpan.FromMilliseconds(ms));
            Assert.Equal(SceneReadHealth.Fresh,read.Health);
            var pose=motion.Advance(new(TimeSpan.FromMilliseconds(80),position,position,foot,
                PlatformGeometry.Build(read.Scene!,63),false,true));
            Assert.Equal(PlatformPhase.Supported,pose.Phase);
            Assert.Equal(929,pose.Position.Y);
            Assert.Equal(1040,pose.Position.Y+foot.SoleY);
            Assert.Equal(65712,pose.Support!.Value.Handle);
            position=pose.Position;
        }
    }

    private sealed class CapturedDesktop(DesktopMetadataReader reader,nint handle):IDesktopMetadataReader
    {
        internal bool OverlayPresent=true;
        internal bool Restored=true;
        public NativeDesktop Read()
        {
            var windows=new List<NativeWindow>();
            if(OverlayPresent)
                windows.Add(reader.ReadWindow(handle)! with{Bounds=new(1,43,1920,1017)});
            windows.Add(new(65712,10,new(0,1040,1920,40),Taskbar:true,HorizontalTaskbar:true,ClassName:"Shell_TrayWnd"));
            windows.Add(new(65882,10,new(1920,1040,1920,40),Taskbar:true,HorizontalTaskbar:true,ClassName:"Shell_SecondaryTrayWnd"));
            windows.Add(new(64031512,20,Restored?new(0,12,1922,1049):new(0,0,1920,1040),ClassName:"UnityWndClass"));
            return new([new(1,new(0,0,1920,1080)),new(2,new(1920,0,1920,1080))],windows);
        }
    }
}

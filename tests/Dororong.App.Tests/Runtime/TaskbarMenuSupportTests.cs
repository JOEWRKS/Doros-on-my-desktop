using Dororong.App.Interop;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public sealed class TaskbarMenuSupportTests
{
    private static readonly NativeWindow Bar = new(1,10,new(0,1040,1920,40),Taskbar:true,HorizontalTaskbar:true,ClassName:"Shell_TrayWnd");
    private static readonly NativeWindow Menu = new(2,10,new(1600,600,320,450),CanSupport:false,ClassName:"#32768");
    private sealed class Metadata : IDesktopMetadataReader
    {
        internal NativeWindow[] Windows=[Bar];
        public NativeDesktop? Read()=>new([new(1,new(0,0,1920,1080))],Windows);
    }
    private static PerchSurface[] Perches(DesktopScene scene)=>PlatformGeometry.Build(scene,0)
        .Select(s=>new PerchSurface(s,scene.Windows.FirstOrDefault(w=>w.Key==s.Key)?.ZOrder??int.MaxValue)).ToArray();

    [Theory]
    [InlineData("Chrome_WidgetWin_1",0x96000000L,0x80088L,true)]
    [InlineData("Chrome_WidgetWin_1",0x14C70000L,0x40100L,false)]
    [InlineData("Chrome_WidgetWin_1",0x16CF0000L,0x100L,false)]
    [InlineData("Chrome_WidgetWin_1",0x96C00000L,0x80088L,false)]
    [InlineData("Chrome_WidgetWin_1",0x96040000L,0x80088L,false)]
    [InlineData("Chrome_WidgetWin_1",0xD6000000L,0x80088L,false)]
    [InlineData("Chrome_WidgetWin_1",0x96000000L,0x80008L,false)]
    [InlineData("Chrome_WidgetWin_1",0x96000000L,0x80080L,false)]
    [InlineData("Chrome_WidgetWin_1",0x96000000L,0x88L,false)]
    [InlineData("Chrome_WidgetWin_1",0x96000000L,0xC0088L,false)]
    [InlineData("OtherWindow",0x96000000L,0x80088L,false)]
    public void Chromium_menu_classification_requires_observed_popup_style(string className,long style,long extendedStyle,bool expected)
    {
        Assert.Equal(expected,DesktopMetadataReader.IsChromiumMenuStyle(className,style,extendedStyle));
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Captured_figma_menu_geometry_preserves_standing_and_hanging_support(bool hanging)
    {
        var menu=new NativeWindow(15339574,321096,new(1427,840,302,212),CanSupport:false,
            ClassName:"Chrome_WidgetWin_1") { IsTransientMenu=true };
        var reader=new Metadata();var native=new DesktopSceneNative(reader);
        native.Capture(TimeSpan.Zero);
        var scene=native.Capture(TimeSpan.FromMilliseconds(80))!;
        var foot=new FootContact(20,45,111,48);var position=new PointD(1500,929);
        var motion=new PlatformMotion();var perch=new EdgePerch();var contact=new PerchContact(20,45,70,10);
        if(hanging) Assert.True(perch.TryBegin(new(1500,975),contact,Perches(scene),scene.Monitors,true));
        for(var step=2;step<=14;step++)
        {
            reader.Windows=step%2==0?[menu,Bar]:[Bar];
            scene=native.Capture(TimeSpan.FromMilliseconds(step*80))!;
            var surfaces=PlatformGeometry.Build(scene,63);
            Assert.DoesNotContain(surfaces,s=>s.Key.Handle==menu.Handle);
            if(hanging)
            {
                var pose=perch.Advance(TimeSpan.FromMilliseconds(160),new(1500,970),contact,Perches(scene),scene.Monitors,true);
                Assert.Equal(EdgePerchPhase.Attached,pose.Phase);
                Assert.Equal(new PointD(1500,970),pose.Position);
            }
            else
            {
                var pose=motion.Advance(new(TimeSpan.FromMilliseconds(80),position,position,foot,surfaces,false,true));
                Assert.Equal(PlatformPhase.Supported,pose.Phase);Assert.Equal(position,pose.Position);
            }
        }
        reader.Windows=[menu];scene=native.Capture(TimeSpan.FromMilliseconds(1200))!;
        Assert.DoesNotContain(PlatformGeometry.Build(scene,63),s=>s.Kind==PlatformKind.Taskbar);
    }

    [Theory]
    [InlineData(10u)] [InlineData(20u)]
    public void Repeated_menu_open_close_preserves_right_side_support(uint menuPid)
    {
        var reader=new Metadata();var native=new DesktopSceneNative(reader);native.Capture(TimeSpan.Zero);
        var motion=new PlatformMotion();var foot=new FootContact(20,45,111,48);var position=new PointD(1700,929);
        for(var step=1;step<=12;step++)
        {
            reader.Windows=step%2==0?[Menu with {ProcessId=menuPid},Bar]:[Bar];
            var scene=native.Capture(TimeSpan.FromMilliseconds(step*80))!;
            var pose=motion.Advance(new(TimeSpan.FromMilliseconds(80),position,position,foot,PlatformGeometry.Build(scene,63),false,true));
            Assert.Equal(PlatformPhase.Supported,pose.Phase);
            Assert.Equal(position,pose.Position);
            Assert.Equal(1,pose.Support!.Value.Handle);
        }
    }

    [Theory]
    [InlineData(false,10u)] [InlineData(true,10u)]
    [InlineData(false,20u)] [InlineData(true,20u)]
    public void Menu_keeps_entering_and_attached_grip_but_not_a_missing_taskbar(bool attached,uint menuPid)
    {
        var reader=new Metadata();var native=new DesktopSceneNative(reader);native.Capture(TimeSpan.Zero);
        var scene=native.Capture(TimeSpan.FromMilliseconds(80))!;
        var perch=new EdgePerch();var contact=new PerchContact(20,45,70,10);var position=new PointD(1700,975);
        Assert.True(perch.TryBegin(position,contact,Perches(scene),scene.Monitors,true));
        if(attached)position=perch.Advance(TimeSpan.FromMilliseconds(160),position,contact,Perches(scene),scene.Monitors,true).Position;
        reader.Windows=[Menu with {ProcessId=menuPid},Bar];scene=native.Capture(TimeSpan.FromMilliseconds(160))!;
        var pose=perch.Advance(TimeSpan.FromMilliseconds(160),position,contact,Perches(scene),scene.Monitors,true);
        Assert.Equal(EdgePerchPhase.Attached,pose.Phase);Assert.Equal(new PointD(1700,970),pose.Position);
        reader.Windows=[Menu];scene=native.Capture(TimeSpan.FromMilliseconds(240))!;
        Assert.Equal(EdgePerchPhase.None,perch.Advance(TimeSpan.FromMilliseconds(16),pose.Position,contact,Perches(scene),scene.Monitors,true).Phase);
    }

    [Theory]
    [InlineData("OrdinaryWindow",10,true)] // Same process alone is not enough.
    [InlineData("OrdinaryWindow",20,true)] // Other apps' real windows still cut the bar.
    [InlineData("#32768",10,false)] // Shell menu still occludes ordinary windows.
    [InlineData("#32768",20,false)]
    public void Real_windows_and_menus_over_ordinary_surfaces_still_occlude(string className,uint pid,bool taskbar)
    {
        var reader=new Metadata();var native=new DesktopSceneNative(reader);
        var surface=Bar with { Taskbar=taskbar,HorizontalTaskbar=taskbar,ClassName=taskbar?"Shell_TrayWnd":"CabinetWClass" };
        reader.Windows=[surface];native.Capture(TimeSpan.Zero);
        var scene=native.Capture(TimeSpan.FromMilliseconds(80))!;
        var motion=new PlatformMotion();var foot=new FootContact(20,45,111,48);var position=new PointD(1700,929);
        Assert.Equal(PlatformPhase.Supported,motion.Advance(new(TimeSpan.FromMilliseconds(16),position,position,foot,PlatformGeometry.Build(scene,63),false,true)).Phase);
        reader.Windows=[Menu with {ClassName=className,ProcessId=pid},surface];scene=native.Capture(TimeSpan.FromMilliseconds(160))!;
        Assert.Equal(PlatformPhase.Falling,motion.Advance(new(TimeSpan.FromMilliseconds(16),position,position,foot,PlatformGeometry.Build(scene,63),false,true)).Phase);
        Assert.DoesNotContain(PlatformGeometry.Build(scene,63),s=>s.Key.Handle==2);
    }
}

using System.Reflection;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Mac.Rendering;
using Dororong.Portable.Desktop;

namespace Dororong.Mac.Tests;

public sealed class DesktopBoundaryRegressionTests
{
    [Theory]
    [InlineData(100, 1)]
    [InlineData(150, 2)]
    public void HeadDragCanReachTopOfInternalScreen(int size, double density)
    {
        using var c = new R104DesktopController(new(400,300),new R(),size);
        c.Density=density;c.SetAutomaticSleep(false);
        var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1470,956))],[]),SceneHealth.Fresh);
        var errors=new List<Exception>();c.Faulted+=errors.Add;
        for(var i=0;i<90;i++)c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
        var p=(Dororong.App.Controls.DororongPresenter)typeof(R104DesktopController).GetField("presenter",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(c)!;
        var local=p.HighResolutionFaceToPresenter(new(37.5,28.25));
        var cursor=c.Position+new PointD(local.X,local.Y);
        Assert.True(c.TryBeginInteraction(cursor));
        c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor),true,scene);
        for(var i=1;i<=120;i++)c.Advance(TimeSpan.FromMilliseconds(16),new(true,new(cursor.X,Math.Max(0,cursor.Y-i*16))),true,scene);
        Assert.Empty(errors);Assert.True(c.Capturing);
        Assert.InRange(c.Position.Y+c.Geometry!.Value.Bounds.Y,-1,20);
    }
    public static IEnumerable<object[]> PerchLayouts()=>
        from size in new[]{100,125,150} from density in new[]{1d,2d}
        from origin in new[]{new PointD(0,0),new(-1920,0),new(1470,-1080)}
        select new object[]{origin.X,origin.Y,size,density};
    [Theory][MemberData(nameof(PerchLayouts))]
    public void ExternalOnlyTopologyCanAcquireOrdinaryWindow(double x,double y,int size,double density)
    {
        var mapper=new MacSceneMapper();
        var monitors=new DesktopMonitor[]{new(2,new(x,y,1920,1080))};
        SceneSnapshot Scene(double? top=null)=>new(mapper.Build(TimeSpan.Zero,monitors,
            top is {} t?[new(123,456,new(x+100,t,1600,400),0,1,true,false)]:[],999,20,24),SceneHealth.Fresh);
        using var c=new R104DesktopController(new(400,300),new R(),size);c.SetAutomaticSleep(false);c.Density=density;
        // Start with the internal panel, then replace it with the external layout.
        c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,
            new(new(0,TimeSpan.Zero,[new(1,new(0,0,1470,956))],[]),SceneHealth.Fresh));
        var errors=new List<Exception>();c.Faulted+=errors.Add;var scene=Scene();
        for(var i=0;i<90;i++)c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
        var p=(Dororong.App.Controls.DororongPresenter)typeof(R104DesktopController).GetField("presenter",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(c)!;
        var local=p.HighResolutionFaceToPresenter(new(37.5,28.25));var cursor=c.Position+new PointD(local.X,local.Y);
        Assert.True(c.TryBeginInteraction(cursor));c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor),true,scene);
        cursor+=new PointD(x+800-c.Position.X,y+350-c.Position.Y);
        for(var i=0;i<60;i++)c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor),true,scene);
        var contact=p.MeasureEdgePerchContact(FacingDirection.Right)!.Value;
        scene=Scene(c.Position.Y+contact.GripY-10);
        for(var i=0;i<10;i++)c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor),true,scene);
        c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor),false,scene);
        for(var i=0;i<20;i++)c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor),false,scene);
        Assert.Empty(errors);Assert.Equal(EdgePerchPhase.Attached,c.Perch);
        var attached=c.Position;var oldTop=scene.Scene.Windows[0].Bounds.Y;
        scene=new(mapper.Build(TimeSpan.Zero,monitors,[new(123,456,new(x+120,oldTop+35,1600,400),0,1,true,false)],999,20,24),SceneHealth.Fresh);
        for(var i=0;i<8;i++)c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
        Assert.Equal(EdgePerchPhase.Attached,c.Perch);
        Assert.InRange(Math.Abs(c.Position.X-attached.X-20),0,.001);Assert.InRange(Math.Abs(c.Position.Y-attached.Y-35),0,.001);
        scene=Scene();for(var i=0;i<8;i++)c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
        Assert.Equal(EdgePerchPhase.None,c.Perch);Assert.Empty(errors);
    }
    private sealed class R:IRandomSource{public double NextUnit()=>.5;}
    public static IEnumerable<object[]> Transitions()
    {
        foreach(var size in new[]{100,125,150})
        foreach(var density in new[]{1d,2d})
        foreach(var width in new[]{1280,1920,2560})
        foreach(var layout in new[]{"left","right","above","below","primary","clamshell"})
            yield return [size,density,width,layout];
    }
    [Theory][MemberData(nameof(Transitions))]
    public void ControllerUsesNewTopologyOnTheFirstTransitionFrame(int size,double density,int width,string layout)
    {
        using var c=new R104DesktopController(new(400,300),new R(),size);
        c.Density=density;c.SetAutomaticSleep(false);var errors=new List<Exception>();c.Faulted+=errors.Add;
        var original=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1470,956))],[]),SceneHealth.Fresh);
        c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,original);
        var rect=layout switch{
            "left"=>new RectD(-width,0,width,1080),"right"=>new(1470,0,width,1080),
            "above"=>new(0,-1080,width,1080),"below"=>new(0,956,width,1080),
            _=>new RectD(0,0,width,1080)};
        // Old panel is removed, or the coordinate origin/resolution is changed.
        var next=new SceneSnapshot(new(2,TimeSpan.FromMilliseconds(16),[new(2,rect)],[]),SceneHealth.FloorFallback);
        c.Density=density==1?2:1;
        c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,next);
        Assert.Empty(errors);
        using var state=System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(c.Platform));
        var monitor=state.RootElement.GetProperty("monitor");
        Assert.Equal(rect.X,monitor.GetProperty("X").GetDouble());Assert.Equal(rect.Y,monitor.GetProperty("Y").GetDouble());
        Assert.Equal(rect.Width,monitor.GetProperty("Width").GetDouble());
        var b=c.Geometry!.Value.Bounds;
        Assert.InRange(c.Position.X+b.X,rect.X-1,rect.Right+1);
        Assert.InRange(c.Position.Y+b.Y,rect.Y-1,rect.Bottom+1);
    }
}

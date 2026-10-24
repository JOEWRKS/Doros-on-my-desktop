using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(false, 80)] [InlineData(false, 499)] [InlineData(false, 500)]
    [InlineData(false, 5000)] [InlineData(false, 60000)]
    [InlineData(true, 80)] [InlineData(true, 499)] [InlineData(true, 500)]
    [InlineData(true, 5000)] [InlineData(true, 60000)]
    public void Stationary_owner_survives_metadata_outage_without_replaying_landing(bool taskbar, int outageMs)
    {
        using var h=new Harness();
        if(taskbar) h.Native.Scene=h.Native.Scene with { Windows=h.Native.Scene.Windows.Select(w=>
            w with {Taskbar=true,HorizontalTaskbar=true}).ToArray() };
        h.Start();h.Tick();var pose=h.LastPose;
        Assert.NotNull(pose?.Support);
        h.Native.Fail=true;h.Tick(outageMs);
        Assert.Equal(new PointD(100,100),h.Position);
        Assert.Equal(pose?.Support,h.LastPose?.Support);
        Assert.Equal(pose?.Phase,h.LastPose?.Phase);
        h.Native.Fail=false;h.Tick(80);
        Assert.Equal(new PointD(100,100),h.Position);
        Assert.Equal(pose?.Phase,h.LastPose?.Phase);
    }

    [Theory]
    [InlineData("closed")] [InlineData("minimized")] [InlineData("hidden")]
    [InlineData("cloaked")] [InlineData("replaced")] [InlineData("occluded")]
    public void Fresh_evidence_after_outage_still_removes_invalid_owner(string change)
    {
        using var h=new Harness();h.Start();h.Tick();h.Native.Fail=true;h.Tick(1000);
        Assert.Equal(new PointD(100,100),h.Position);
        var w=h.Native.Scene.Windows[0];
        h.Native.Scene=h.Native.Scene with {Windows=change switch {
            "closed"=>[], "minimized"=>[w with {Minimized=true}],
            "hidden"=>[w with {Visible=false}], "cloaked"=>[w with {Cloaked=true}],
            "replaced"=>[w with {Key=new(1,2,2),Bounds=w.Bounds with {Y=300}}],
            _=>[w with {ZOrder=1},w with {Key=new(2,2,2),ZOrder=0,Bounds=new(0,0,800,600)}]
        }};
        h.Native.Fail=false;h.Tick(80);
        Assert.Equal(PlatformPhase.Falling,h.LastPose?.Phase);
        Assert.True(h.Position.Y>100);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Perch_survives_expiry_but_confirmed_owner_or_monitor_loss_detaches(bool removeMonitor)
    {
        DesktopMonitor[] monitors=[new(1,new(0,0,800,600))];
        using var h=new Harness(fallback:()=>monitors,perchEnabled:true);
        h.Native.Scene=Scene(y:194);
        var runtime=h.Platforms;var p=new PointD(100,100);
        runtime.BeginFrame(TimeSpan.Zero,p,PointerSample.Unavailable,false);
        Assert.True(runtime.TryBeginPerch(p,FacingDirection.Right,true,true));
        Assert.True(runtime.TryAdvancePerch(TimeSpan.FromMilliseconds(200),p,out p));
        Assert.Equal(EdgePerchPhase.Attached,runtime.PerchPhase);
        h.Native.Fail=true;
        runtime.BeginFrame(TimeSpan.FromSeconds(2),p,PointerSample.Unavailable,false);
        Assert.True(runtime.TryAdvancePerch(TimeSpan.FromMilliseconds(16),p,out var held));
        Assert.Equal(p,held);Assert.Equal(EdgePerchPhase.Attached,runtime.PerchPhase);
        if(removeMonitor) monitors=[new(2,new(-800,0,800,600))];
        else {h.Native.Fail=false;h.Native.Scene=Scene(windows:false);}
        runtime.BeginFrame(TimeSpan.FromSeconds(3),p,PointerSample.Unavailable,false);
        Assert.False(runtime.TryAdvancePerch(TimeSpan.FromMilliseconds(16),p,out _));
        Assert.Equal(EdgePerchPhase.None,runtime.PerchPhase);
    }

    [Fact]
    public void Expired_supported_scene_still_clamps_to_independently_confirmed_surviving_monitor()
    {
        DesktopMonitor[] monitors=[new(1,new(0,0,800,600))];
        using var h=new Harness(fallback:()=>monitors);h.Start();h.Tick();h.Native.Fail=true;
        monitors=[new(2,new(-800,0,600,500))];h.Tick(1000);
        Assert.Equal(-310,h.Position.X);
        for(var i=0;i<60;i++)h.Tick(80);
        Assert.Equal(new PointD(-310,400),h.Position);
    }
}

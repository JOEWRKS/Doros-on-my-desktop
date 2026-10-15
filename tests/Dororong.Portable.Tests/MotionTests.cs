using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Portable.Rendering;

namespace Dororong.Portable.Tests;

public sealed class MotionTests
{
    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonFiniteMotionRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(()=>MotionRenderer.Walk(value,0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>MotionRenderer.Walk(1,value));
        Assert.Throws<ArgumentOutOfRangeException>(()=>MotionRenderer.Standing(breathing:value));
        Assert.Throws<ArgumentOutOfRangeException>(()=>new TrackingRenderer().Render(new(0,0,value)));
        Assert.Throws<ArgumentOutOfRangeException>(()=>SnackRenderer.JumpFrame(value));
    }
    [Fact]
    public void ChewingAndSwayMoveWithoutMutatingCapturedFrames()
    {
        var renderer=new SnackRenderer();
        SnackPose Pose(double age)=>new(SnackPhase.Eating,FacingDirection.Right,0,0,age,0);
        var first=renderer.Render(Pose(.5));var saved=first.Pixels.ToArray();
        var next=renderer.Render(Pose(.7));Assert.False(saved.SequenceEqual(next.Pixels.ToArray()));
        Assert.Equal(saved,first.Pixels.ToArray());
        Assert.Equal(0,SnackRenderer.HappyRoll(0));Assert.Equal(0,SnackRenderer.HappyRoll(SnackSession.HappySeconds));
        Assert.True(SnackRenderer.HappyRoll(.46)<0);Assert.True(SnackRenderer.HappyRoll(1.10)>0);
    }
    [Fact]
    public void TrackingSmoothlyEntersExitsAndYieldsToSnack()
    {
        var tracking=new UprightTracking();tracking.Advance(.016,new(150,0),false);
        var first=tracking.Pose;Assert.InRange(tracking.Blend,0.001,.999);
        for(var i=0;i<50;i++)tracking.Advance(.016,new(150,0),false);
        Assert.True(Math.Abs(tracking.Pose.Roll)>Math.Abs(first.Roll));
        var before=tracking.Pose;tracking.Advance(.016,null,false);
        Assert.False(tracking.IsTracking);Assert.NotEqual(default,tracking.Pose);
        Assert.True(Math.Abs(tracking.Pose.Roll)<Math.Abs(before.Roll));
        for(var i=0;i<120;i++)tracking.Advance(.016,null,false);
        Assert.Equal(0,tracking.Blend);
        tracking.Advance(.1,new(100,0),false);tracking.Advance(.016,new(100,0),false,blocked:true);
        Assert.Equal(0,tracking.Blend);Assert.Equal(default,tracking.Pose);
    }
    [Fact]
    public void WalkingWrapAndNativeFramesAreStable()
    {
        var first=MotionRenderer.Walk(1,-2.5);var loop=MotionRenderer.Walk(1,7.5);
        Assert.Same(first,loop);Assert.Equal(96,first.PixelWidth);Assert.Equal(96,first.PixelHeight);
        Assert.Same(MotionRenderer.Standing(),MotionRenderer.Walk(0,100));
        var bytes=first.Pixels.Span;
        for(var i=0;i<bytes.Length;i+=4)for(var c=0;c<3;c++)Assert.True(bytes[i+c]<=bytes[i+3]);
    }
    [Fact]
    public void SnackInvalidRecoveryAndCancellationAreSafe()
    {
        var renderer=new SnackRenderer();var pose=new SnackPose(SnackPhase.Chasing,FacingDirection.Right,0,0,0,0);
        Assert.Throws<ArgumentOutOfRangeException>(()=>renderer.Render(pose,-1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>renderer.Render(pose with{RunAmount=2}));
        Assert.Equal(MotionRenderer.Standing().Pixels.ToArray(),renderer.Render(default).Pixels.ToArray());
        var session=new SnackSession();session.Begin(FacingDirection.Right);
        session.Advance(TimeSpan.FromSeconds(2),new(true,new(500,20)),new(10,20),true);
        Assert.False(session.Active);
    }
}

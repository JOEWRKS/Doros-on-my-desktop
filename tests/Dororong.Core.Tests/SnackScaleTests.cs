using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.Core.Tests;

public sealed class SnackScaleTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Ground_speed_scales_both_ways_without_changing_cadence(double scale){
        foreach(var direction in new[]{-1,1}){
            var baseline=new SnackSession();baseline.Begin(FacingDirection.Right);
            var scaled=new SnackSession();scaled.Begin(FacingDirection.Right);
            var travel=0d;
            for(var i=0;i<50;i++){
                var pointer=new PointerSample(true,new PointD(direction*10000,50));
                var a=baseline.Advance(TimeSpan.FromMilliseconds(20),pointer,new(0,50),true);
                var b=scaled.Advance(TimeSpan.FromMilliseconds(20),pointer,new(0,50),true,scale);
                travel+=b.DeltaX;
                Assert.Equal(a.DeltaX*scale,b.DeltaX,10);
                Assert.Equal(a.Distance,b.Distance,10);Assert.Equal(a.RunAmount,b.RunAmount);
                Assert.Equal(a.Facing,b.Facing);
            }
            Assert.Equal(direction*185*scale,travel,9);
        }
    }

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Final_approach_is_capped_before_scaling_can_overshoot(double scale){
        foreach(var direction in new[]{-1,1}){
            var session=new SnackSession();session.Begin(FacingDirection.Right);var x=0d;
            for(var i=0;i<40;i++){
                var pose=session.Advance(TimeSpan.FromMilliseconds(20),new(true,new(direction*10,50)),new(x,50),true,scale);
                x+=pose.DeltaX;Assert.InRange(direction*x,0,2.00000001);
            }
            Assert.Equal(SnackPhase.Eating,session.Current.Phase);
        }
    }

    [Theory]
    [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Airborne_trajectory_is_not_scaled_by_ground_speed(double scale){
        var a=new SnackSession();a.Begin(FacingDirection.Right);var b=new SnackSession();b.Begin(FacingDirection.Right);
        for(var i=0;i<40;i++){
            var pa=a.Advance(TimeSpan.FromMilliseconds(20),new(true,new(40,-50)),new(0,50),true);
            var pb=b.Advance(TimeSpan.FromMilliseconds(20),new(true,new(40,-50)),new(0,50),true,scale);
            Assert.Equal(pa,pb);
        }
    }

    [Fact]
    public void Live_scale_change_preserves_distance_history_and_run_ramp(){
        var a=new SnackSession();a.Begin(FacingDirection.Right);var b=new SnackSession();b.Begin(FacingDirection.Right);
        for(var i=0;i<75;i++){
            var scale=i<25?1:i<50?1.5:1.25;
            var pa=a.Advance(TimeSpan.FromMilliseconds(20),new(true,new(10000,50)),new(0,50),true);
            var pb=b.Advance(TimeSpan.FromMilliseconds(20),new(true,new(10000,50)),new(0,50),true,scale);
            Assert.Equal(pa.Distance,pb.Distance,9);Assert.Equal(pa.RunAmount,pb.RunAmount);
            Assert.Equal(pa.DeltaX*scale,pb.DeltaX,10);
        }
    }
}

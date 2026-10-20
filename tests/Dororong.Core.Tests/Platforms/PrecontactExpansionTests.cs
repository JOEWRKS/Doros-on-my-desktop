using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.Core.Tests.Platforms;

public class PrecontactExpansionTests
{
    private static readonly FootContact Contact=new(40,80,110,20);
    private static readonly PlatformSurface Floor=new(new(0,0,1),1,PlatformKind.Floor,0,1000,1200,new(0,1200));
    private static PlatformMotionInput Input(PointD position,double ms,IReadOnlyList<PlatformSurface>? surfaces=null)=>
        new(TimeSpan.FromMilliseconds(ms),position,position,Contact,surfaces??[Floor],false,true)
        {Monitors=[new(1,new(0,0,1000,1000))],AnticipateExtremeLanding=true};

    [Theory]
    [InlineData(1)] [InlineData(4)] [InlineData(8)] [InlineData(16)]
    public void Expansion_uses_the_same_35ms_curve_in_air_and_does_not_restart_at_contact(double step){
        var motion=new PlatformMotion();var position=new PointD(100,0);var samples=0;
        for(var i=0;i<2000;i++){
            var pose=motion.Advance(Input(position,step));position=pose.Position;
            if(pose.Phase==PlatformPhase.Falling&&pose.IsExtremeLanding){
                samples++;var remaining=(Floor.Top-Contact.SoleY-position.Y)/1200;
                Assert.InRange(remaining,0,.035);
                Assert.Equal(1-Math.Pow(remaining/.035,3),pose.LegSpread,7);
                Assert.Equal(.82*.70*pose.LegSpread,pose.Squash,9);
                Assert.Null(pose.Support);Assert.True(position.Y+Contact.SoleY<Floor.Top);
            }
            if(pose.Phase==PlatformPhase.Landing){
                Assert.True(samples>0);Assert.Equal(1,pose.LegSpread);Assert.Equal(.82*.70,pose.Squash);
                var next=motion.Advance(Input(position,8));
                var t=8d/30;Assert.Equal(1-.06*t*t*(3-2*t),next.LegSpread,8);
                return;
            }
        }
        Assert.Fail("No landing");
    }

    [Fact]
    public void Removed_surface_cancels_forecast_without_creating_support(){
        var motion=new PlatformMotion();var position=new PointD(100,0);
        for(var i=0;i<500;i++){
            var pose=motion.Advance(Input(position,8));position=pose.Position;
            if(!pose.IsExtremeLanding)continue;
            var removed=motion.Advance(Input(position,8,[]));
            Assert.Equal(PlatformPhase.Falling,removed.Phase);Assert.Null(removed.Support);
            Assert.False(removed.IsExtremeLanding);Assert.Equal(0,removed.LegSpread);return;
        }
        Assert.Fail("No forecast");
    }

    [Fact]
    public void Short_fall_does_not_use_strong_landing_anticipation(){
        var motion=new PlatformMotion();var position=new PointD(100,1000);
        for(var i=0;i<100;i++){
            var pose=motion.Advance(Input(position,8));position=pose.Position;
            Assert.False(pose.IsExtremeLanding);Assert.Equal(0,pose.LegSpread);
            if(pose.Phase==PlatformPhase.Landing)return;
        }
        Assert.Fail("No landing");
    }
}

using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.Core.Tests.Platforms;

public class ImpactRemainderTests
{
    [Theory]
    [InlineData(8, 4)]
    [InlineData(16, 4)]
    [InlineData(32, 12)]
    public void Strong_contact_preserves_full_approved_spread_sequence_without_skipping_its_start(double tickMs,double crossingMs)
    {
        var motion=new PlatformMotion();var position=new PointD(100,0);
        var contact=new FootContact(40,80,110,20);
        // Reach terminal velocity in open space. The next collision time is
        // exact (constant 1200 DIP/s), independent of the subdivision loop.
        for(var i=0;i<4;i++) position=motion.Advance(new(TimeSpan.FromMilliseconds(250),position,position,contact,[],false,true)).Position;
        var top=position.Y+contact.SoleY+1.2*crossingMs;
        var floor=new PlatformSurface(new(0,0,1),1,PlatformKind.Floor,0,1000,top,new(0,top));
        var pose=motion.Advance(new(TimeSpan.FromMilliseconds(tickMs),position,position,contact,[floor],false,true)
            {Monitors=[new(1,new(0,0,1000,100))]});
        Assert.True(pose.IsExtremeLanding);
        Assert.Equal(PlatformPhase.Landing,pose.Phase);
        // r83 advanced this first frame by the remainder. The user rejected
        // the skipped start as visibly faster unfolding. Keep it as a guard.
        Assert.Equal(0,pose.LegSpread);
        Assert.Equal(0,pose.Squash);
        Assert.Equal(top-contact.SoleY,pose.Position.Y,8);
        var next=motion.Advance(new(TimeSpan.FromMilliseconds(8),pose.Position,pose.Position,contact,[floor],false,true)
            {Monitors=[new(1,new(0,0,1000,100))]});
        Assert.Equal(1-Math.Pow(1-8d/35,3),next.LegSpread,8);
    }
}

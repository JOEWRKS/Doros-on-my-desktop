using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.Core.Tests;

public class SnackWithdrawalTests
{
    private static readonly PointD Mouth=new(50,50);
    private static SnackSession New(){var s=new SnackSession();s.Begin(FacingDirection.Left);return s;}
    private static SnackPose Step(SnackSession s,double seconds,PointD? pointer=null,PointD? mouth=null)
        =>s.Advance(TimeSpan.FromSeconds(seconds),new(true,pointer??Mouth),mouth??Mouth,true);
    private static void Hold(SnackSession s,double seconds){var ticks=TimeSpan.FromSeconds(seconds).Ticks;while(ticks>0){var dt=Math.Min(TimeSpan.TicksPerMillisecond*10,ticks);s.Advance(TimeSpan.FromTicks(dt),new(true,Mouth),Mouth,true);ticks-=dt;}}

    [Theory][InlineData(.01)][InlineData(.02)][InlineData(.025)][InlineData(.05)]
    public void OfferIsTotalHalfSecondWithoutAdditionalOldStillDelay(double dt)
    {
        var s=New();var count=(int)Math.Round(.5/dt);
        for(var i=0;i<count-1;i++)Assert.Equal(SnackPhase.Chasing,Step(s,dt).Phase);
        Assert.Equal(SnackPhase.Eating,Step(s,dt).Phase);
        Assert.False(s.Current.CarriesJewel);
    }
    [Theory][InlineData(.08)][InlineData(.2)][InlineData(.49)]
    public void RecognizedOfferWithdrawnBeforeDeadlineReactsOnceThenResumes(double held)
    {
        var s=New();Hold(s,held);
        var p=Step(s,.01,new(500,50));Assert.Equal(SnackPhase.Angry,p.Phase);
        Assert.True(p.CarriesJewel);Assert.Equal(0,p.DeltaX);Assert.Equal(0,p.Lift);
        for(var i=0;i<89;i++){
            p=Step(s,.01,new(i%2==0?400:600,50));Assert.Equal(SnackPhase.Angry,p.Phase);
            Assert.Equal(0,p.DeltaX);Assert.Equal(FacingDirection.Left,p.Facing);
        }
        Step(s,.011,new(500,50));Assert.Equal(SnackPhase.Chasing,s.Current.Phase);
        Assert.True(Step(s,.02,new(500,50)).DeltaX>0);
        Hold(s,.51);Assert.Equal(SnackPhase.Eating,s.Current.Phase);
    }
    [Fact] public void CommittedMealCannotBeStolen(){var s=New();Hold(s,.5);Assert.Equal(SnackPhase.Eating,Step(s,.01,new(500,50)).Phase);}
    [Fact] public void FlyByDoesNotArmAnger(){var s=New();Step(s,.016);Assert.Equal(SnackPhase.Chasing,Step(s,.016,new(500,50)).Phase);}
    [Fact] public void OrdinaryChaseAndCrossingFaceDoNotReact(){var s=New();for(var i=0;i<100;i++)Assert.Equal(SnackPhase.Chasing,Step(s,.016,new(i%2==0?40:600,50)).Phase);}
    [Fact] public void SlowWithdrawalStillReacts(){var s=New();Hold(s,.1);for(var x=50.3;x<69;x+=.3)Step(s,.01,new(x,50));Assert.Equal(SnackPhase.Angry,Step(s,.01,new(69.1,50)).Phase);}
    [Fact] public void StationaryCursorAndMovingMouthDoNotReact(){var s=New();Hold(s,.2);Assert.Equal(SnackPhase.Chasing,Step(s,.01,Mouth,new(150,50)).Phase);}
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void CancellationInputLossAndResumeClearReaction(int mode)
    {
        var s=New();Hold(s,.2);Step(s,.01,new(500,50));
        if(mode==0)s.Cancel();else s.Advance(TimeSpan.FromSeconds(mode==1?.01:2),mode==1?PointerSample.Unavailable:new(true,Mouth),Mouth,true);
        Assert.False(s.Active);s.Begin(FacingDirection.Right);Step(s,.01,new(500,50));Assert.Equal(SnackPhase.Chasing,s.Current.Phase);
    }
    [Fact] public void ReactionHasNeutralEndpointsAndBoundedRoll(){Assert.Equal(0,SnackAngerMotion.Roll(0));Assert.Equal(0,SnackAngerMotion.Roll(.9));for(var i=0;i<90;i++)Assert.InRange(Math.Abs(SnackAngerMotion.Roll(i/100d)),0,3*Math.PI/180);}

    [Fact] public void ThirdConsecutiveWithdrawalEscalatesAndMealResetsStreak()
    {
        var s=New();
        for(var attempt=1;attempt<=5;attempt++){
            Hold(s,.2);var p=Step(s,.01,new(500,50));
            Assert.Equal(SnackPhase.Angry,p.Phase);Assert.Equal(attempt,p.WithdrawalStreak);
            for(var i=0;i<=Math.Ceiling(SnackAngerMotion.Duration(attempt)*100);i++)Step(s,.01,new(500,50));
            Assert.Equal(SnackPhase.Chasing,s.Current.Phase);
            Assert.Equal(attempt,s.Current.WithdrawalStreak);
        }
        Hold(s,.51);Assert.Equal(SnackPhase.Eating,s.Current.Phase);Assert.Equal(0,s.Current.WithdrawalStreak);
        s.Begin(FacingDirection.Left);Hold(s,.2);Assert.Equal(1,Step(s,.01,new(500,50)).WithdrawalStreak);
    }

    [Fact] public void FlyByAndMotionDuringAngerDoNotIncrementAndCancelClearsStreak()
    {
        var s=New();Step(s,.01);Assert.Equal(0,Step(s,.01,new(500,50)).WithdrawalStreak);
        Hold(s,.2);Assert.Equal(1,Step(s,.01,new(500,50)).WithdrawalStreak);
        for(var i=0;i<50;i++)Assert.Equal(1,Step(s,.01,new(i%2==0?50:500,50)).WithdrawalStreak);
        s.Cancel();Assert.Equal(0,s.Current.WithdrawalStreak);
        s.Begin(FacingDirection.Left);Hold(s,.2);Assert.Equal(1,Step(s,.01,new(500,50)).WithdrawalStreak);
    }
}

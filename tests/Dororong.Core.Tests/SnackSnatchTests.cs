using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.Core.Tests;

public class SnackSnatchTests
{
    private static readonly PointD Mouth=new(400,400);
    private static SnackSession Sixth(double dt=.01,double scale=1){
        var s=new SnackSession();s.Begin(FacingDirection.Left);
        for(var attempt=1;attempt<=6;attempt++){
            for(var i=0;i<(int)Math.Ceiling(.2/dt);i++)s.Advance(TimeSpan.FromSeconds(dt),new(true,Mouth),Mouth,true,scale);
            s.Advance(TimeSpan.FromSeconds(dt),new(true,new(600,200)),Mouth,true,scale);
            Assert.Equal(attempt,s.Current.WithdrawalStreak);
            if(attempt==6)break;
            Assert.Equal(SnackPhase.Angry,s.Current.Phase);
            for(var i=0;i<300&&s.Current.Phase==SnackPhase.Angry;i++)s.Advance(TimeSpan.FromSeconds(dt),new(true,new(600,200)),Mouth,true,scale);
            Assert.Equal(SnackPhase.Chasing,s.Current.Phase);
        }
        Assert.Equal(SnackPhase.Snatching,s.Current.Phase);return s;
    }
    [Theory][InlineData(.01,1)][InlineData(.016,1.25)][InlineData(.05,1.5)]
    public void SixthJumpsToLiveJewelAndConsumesOnlyAfterActualContact(double dt,double scale){
        var s=Sixth(dt,scale);var target=new PointerSample(true,new(610,180));
        Assert.True(s.Current.CarriesJewel);Assert.False(s.ConfirmSnatch(target.Position,target));
        SnackPose p=default;
        for(var i=0;i<(int)Math.Ceiling(.24/dt);i++){
            p=s.Advance(TimeSpan.FromSeconds(dt),target,Mouth,true,scale);
            Assert.Equal(SnackPhase.Snatching,p.Phase);Assert.True(p.CarriesJewel);
            Assert.False(s.ConfirmSnatch(Mouth,target));
        }
        Assert.NotNull(p.SnatchMouth);Assert.InRange(Math.Abs(p.SnatchMouth.Value.X-target.Position.X),0,1e-7);
        Assert.InRange(Math.Abs(p.SnatchMouth.Value.Y-target.Position.Y),0,1e-7);
        Assert.True(s.ConfirmSnatch(target.Position,target));Assert.False(s.ConfirmSnatch(target.Position,target));
        Assert.Equal(SnackPhase.Eating,s.Current.Phase);Assert.False(s.Current.CarriesJewel);
        Assert.Equal(0,s.Current.WithdrawalStreak);Assert.Null(s.Current.SnatchMouth);
    }
    [Fact] public void UnreachableCursorTimesOutWithoutEating(){
        var s=Sixth();for(var i=0;i<66;i++)s.Advance(TimeSpan.FromSeconds(.01),new(true,new(600,200)),Mouth,true);
        Assert.Equal(SnackPhase.Chasing,s.Current.Phase);Assert.Equal(5,s.Current.WithdrawalStreak);Assert.True(s.Current.CarriesJewel);
    }
    [Fact] public void ChuckleRequiresBothConfirmedSixthBiteAndSupport(){
        var s=Sixth();var pointer=new PointerSample(true,Mouth);
        for(var i=0;i<5;i++)s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,false);
        Assert.True(s.ConfirmSnatch(Mouth,pointer));
        for(var i=0;i<250;i++){
            s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,false);
            Assert.Equal(SnackPhase.Eating,s.Current.Phase);
        }
        s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,true);
        Assert.Equal(SnackPhase.Chuckling,s.Current.Phase);Assert.Equal(0,s.Current.Age);
        Assert.False(s.Current.CarriesJewel);Assert.Null(s.Current.SnatchMouth);
        s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,false);
        Assert.Equal(SnackPhase.Eating,s.Current.Phase);
        s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,true);
        for(var i=0;i<181;i++)s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,true);
        Assert.False(s.Active);
        s.Begin(FacingDirection.Left);
        for(var i=0;i<230;i++)s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,true);
        Assert.Equal(SnackPhase.Happy,s.Current.Phase);
    }
    [Fact] public void LandingEarlyStillFinishesEatingBeforeChuckle(){
        var s=Sixth();var pointer=new PointerSample(true,Mouth);
        for(var i=0;i<5;i++)s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,false);
        Assert.True(s.ConfirmSnatch(Mouth,pointer));
        for(var i=0;i<169;i++)s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,true);
        Assert.Equal(SnackPhase.Eating,s.Current.Phase);
        for(var i=0;i<2;i++)s.Advance(TimeSpan.FromSeconds(.01),pointer,Mouth,true);
        Assert.Equal(SnackPhase.Chuckling,s.Current.Phase);
        s.Cancel();Assert.False(s.Active);
    }
    [Theory][InlineData(0)][InlineData(1)][InlineData(2)]
    public void SnatchCancelsOnExplicitCancelLostPointerOrResume(int mode){
        var s=Sixth();if(mode==0)s.Cancel();else s.Advance(TimeSpan.FromSeconds(mode==1?.01:2),mode==1?PointerSample.Unavailable:new(true,Mouth),Mouth,true);
        Assert.False(s.Active);Assert.Null(s.Current.SnatchMouth);Assert.Equal(0,s.Current.WithdrawalStreak);
    }
}

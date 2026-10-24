using Dororong.Core.Behavior;

namespace Dororong.Core.Tests;

public class SnackStompTests
{
    [Theory][InlineData(2,4,1.54)][InlineData(3,3,1.22)][InlineData(4,4,1.54)][InlineData(5,5,1.86)]
    public void EscalationHasRequestedStampCountAndExactNeutralEnd(int streak,int count,double duration)
    {
        Assert.Equal(duration,SnackAngerMotion.Duration(streak),9);
        var starts=0;var wasUp=false;
        for(var i=0;i<=Math.Ceiling(duration*1000)+1;i++){
            var age=i/1000d;var lift=SnackAngerMotion.PawLift(age,streak);
            Assert.InRange(lift,0,streak>=3?4.8:2.4);Assert.Equal(0,SnackAngerMotion.Roll(age,streak));
            Assert.InRange(SnackAngerMotion.PawReach(age,streak),0,streak>=3?16:4);
            Assert.InRange(SnackAngerMotion.Crouch(age,streak),0,.340000001);
            if(lift>1e-8&&!wasUp)starts++;wasUp=lift>1e-8;
            Assert.Equal(0,SnackAngerMotion.PawLift(age,1));
            Assert.Equal(0,SnackAngerMotion.PawReach(age,1));
            Assert.Equal(0,SnackAngerMotion.Crouch(age,1));
            Assert.Equal(0,SnackAngerMotion.Roll(age,2));
        }
        Assert.Equal(count,starts);
        foreach(var age in new[]{0d,duration+1e-8,duration+.1,double.NaN,double.PositiveInfinity}){
            Assert.Equal(0,SnackAngerMotion.PawLift(age,streak));
            Assert.Equal(0,SnackAngerMotion.PawReach(age,streak));Assert.Equal(0,SnackAngerMotion.Crouch(age,streak));
        }
        for(var hit=0;hit<count;hit++){
            var offset=.32*hit;
            Assert.Equal(streak>=3?4.8:2.4,SnackAngerMotion.PawLift(.19+offset,streak),8);
            Assert.Equal(0,SnackAngerMotion.PawLift(.28+offset,streak),8);
            Assert.Equal(.34,SnackAngerMotion.Crouch(.28+offset,streak),8);
            Assert.Equal(streak>=3?16:4,SnackAngerMotion.PawReach(.28+offset,streak),8);
        }
    }

    [Fact] public void SecondHasOneContinuousMotionWithoutMidpointRecoveryOrTimeStretch()
    {
        static double Smooth(double x){x=Math.Clamp(x,0,1);return x*x*(3-2*x);}
        static double Beat(double age,double start,double peak,double end)=>age<=start||age>=end?0:
            age<=peak?Smooth((age-start)/(peak-start)):1-Smooth((age-peak)/(end-peak));
        for(var frame=0;frame<=1540;frame++){
            var age=frame/1000d;
            var lift=2.4*(Beat(age,.04,.19,.28)+Beat(age,.36,.51,.60)+Beat(age,.68,.83,.92)+Beat(age,1,1.15,1.24));
            var impact=Beat(age,.19,.28,.36)+Beat(age,.51,.60,.68)+Beat(age,.83,.92,1)+Beat(age,1.15,1.24,1.34);
            var envelope=Smooth((age-.02)/.16)*(1-Smooth((age-1.30)/.24));
            Assert.InRange(Math.Abs(lift-SnackAngerMotion.PawLift(age,2)),0,1e-12);
            Assert.InRange(Math.Abs(3.2*envelope+.8*impact-SnackAngerMotion.PawReach(age,2)),0,1e-12);
            Assert.InRange(Math.Abs(.12*envelope+.22*impact-SnackAngerMotion.Crouch(age,2)),0,1e-12);
            if(age>=.18&&age<=1.30){
                Assert.True(SnackAngerMotion.PawReach(age,2)>=3.2-1e-12);
                Assert.True(SnackAngerMotion.Crouch(age,2)>=.12-1e-12);
            }
        }
    }
}

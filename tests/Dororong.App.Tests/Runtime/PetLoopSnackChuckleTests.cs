using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory][InlineData(false,1)][InlineData(false,1.5)][InlineData(true,1)][InlineData(true,1.5)]
    public void SixthChucklePlaysOnceOnFloorOrWindowOnlyAfterLanding(bool window,double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,snackPresentation:true,highResolutionScale:scale);
        h.Native.Scene=Scene(x:0,y:360,windows:window);h.Start();for(var i=0;i<240;i++)h.Tick();h.Loop.ToggleSnack();
        for(var attempt=1;attempt<=6;attempt++){
            for(var i=0;i<18;i++){h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth());h.Tick();}
            h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth()+new PointD(45,attempt==6?-130:0));h.Tick();
            if(attempt<6)for(var i=0;i<160&&h.Snack.Phase==SnackPhase.Angry;i++)h.Tick();
        }
        var caught=false;var chuckled=false;var ticks=0;PointD? planted=null;
        for(var frame=0;frame<400;frame++){
            h.Tick();caught|=h.Snack.Phase==SnackPhase.Eating;
            if(h.Snack.Phase!=SnackPhase.Chuckling)continue;
            Assert.True(caught);chuckled=true;ticks++;
            Assert.Equal(PlatformPhase.Supported,h.LastPose?.Phase);
            Assert.False(h.Platforms.SuspendsAutonomousMotion);
            planted??=h.Position;
            Assert.InRange(Math.Abs(h.Position.Y-planted.Value.Y),0,.01);
            Assert.InRange(Math.Abs(h.Position.X-planted.Value.X),0,.01);
            Assert.False(h.Snack.CarriesJewel);
            if(h.Snack.Age is >.3 and <1.4)Assert.Equal(1,h.Presenter!.HighResolutionFace.ChuckleCover,6);
        }
        Assert.True(chuckled);Assert.InRange(ticks*.016,1.78,1.83);
        Assert.Equal(SnackPhase.None,h.Snack.Phase);Assert.Null(h.Fault);
        Assert.InRange(h.Position.Y+h.Presenter!.MeasurePlatformContact()!.Value.SoleY,(window?360:600)-1,(window?360:600)+1);
    });
}

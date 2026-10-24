using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Idle_activities_hold_actual_platform_and_give_way_to_input(double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,snackPresentation:true,
            huntingPresentation:true,doroQuestion:true,idleActivities:true);
        h.Start();var sole=h.Presenter!.MeasurePlatformContact()!.Value.SoleY;
        var floor=100+sole;h.Native.Scene=Scene(y:floor);h.Tick(80);
        void Planted(){Assert.InRange(h.Position.Y+h.Presenter.MeasurePlatformContact()!.Value.SoleY,floor-1.1,floor+1.1);}
        // Headless host time/input, not physical mouse actions.
        for(var i=0;i<460;i++)h.Tick(100);
        Assert.Equal(IdleActivityKind.Groom,h.Core.IdleActivity.Kind);
        for(var i=0;i<240;i++){h.Tick();Planted();}
        Assert.Equal(IdleActivityKind.None,h.Core.IdleActivity.Kind);
        h.Loop.ToggleSnack();
        var sawBeg=false;
        for(var i=0;i<450;i++){
            h.Pointer=new(true,h.Position+h.Presenter.GetSnackMouth());h.Tick();Planted();
            if(h.Core.IdleActivity.Kind==IdleActivityKind.Beg){sawBeg=true;break;}
        }
        Assert.True(sawBeg);
        for(var i=0;i<150;i++){h.Tick();Planted();}
        h.Pointer=PointerSample.Unavailable;
        h.Loop.SetAutomaticSleepEnabled(false);h.Loop.SetAutomaticSleepEnabled(true);
        for(var i=0;i<1840&&h.Core.State!=PetState.Sleep;i++)h.Tick(100);
        Assert.Equal(PetState.Sleep,h.Core.State);
        for(var i=0;i<45;i++)h.Tick();
        h.Loop.SetAutomaticSleepEnabled(false);
        var sawStretch=false;
        for(var i=0;i<185;i++){h.Tick();Planted();sawStretch|=h.Core.IdleActivity.Kind==IdleActivityKind.Stretch;}
        Assert.True(sawStretch);
        Assert.Equal(IdleActivityKind.None,h.Core.IdleActivity.Kind);
        h.Pointer=new(true,h.Position+new PointD(400,0));
        h.Loop.ToggleSnack();h.Tick();Assert.True(h.Snack.Active);Assert.Equal(IdleActivityKind.None,h.Core.IdleActivity.Kind);
    });
}

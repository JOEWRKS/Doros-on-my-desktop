using Dororong.App.Controls;
using Dororong.Core.Geometry;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Seated_near_pointer_taps_repeatedly_and_departure_returns_to_sit(double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true,snackPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        h.Loop.NotifySitRequested();for(var i=0;i<70;i++)h.Tick(20);
        var p=h.Presenter!;var floor=h.Position.Y+p.MeasurePlatformContact()!.Value.SoleY;
        var moving=0;var starts=0;var was=false;
        for(var i=0;i<200;i++){
            h.Pointer=new(true,h.Position+new PointD(30*scale,85*scale));h.Tick(20);
            var active=p.HighResolutionFace.SeatedPawAmount>0;
            if(active){moving++;if(!was)starts++;}was=active;
            Assert.Equal(321,p.HighResolutionFace.MotionFrame);
            Assert.InRange(Math.Abs(floor-h.Position.Y-p.MeasurePlatformContact()!.Value.SoleY),0,.01);
        }
        Assert.True(moving>40);Assert.InRange(starts,2,3);
        h.Pointer=PointerSample.Unavailable;for(var i=0;i<60;i++)h.Tick(20);
        Assert.Equal(0,p.HighResolutionFace.SeatedPawAmount);Assert.Equal(321,p.HighResolutionFace.MotionFrame);
        Assert.Null(h.Fault);
    });
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Seated_click_shakes_without_hop_then_returns_to_same_support(double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true,snackPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        h.Loop.NotifySitRequested();for(var i=0;i<70;i++)h.Tick(20);
        var held=h.Position;var presenter=h.Presenter!;
        var floor=h.Position.Y+presenter.MeasurePlatformContact()!.Value.SoleY;
        var image=presenter.HighResolutionImage!;
        var local=presenter.HighResolutionFaceToPresenter(new(35,35));
        var press=presenter.CreateDirectPress(image,presenter.TranslatePoint(local,image));Assert.NotNull(press);
        h.Pointer=new(true,h.Position+press!.WindowLocalPosition);h.Down=true;
        h.Loop.NotifyDirectInteractionPressed(press);h.Tick(20);h.Down=false;
        var shakes=0;var trace=new List<string>();var maxDelta=0d;var missing=0;
        for(var i=0;i<60;i++){
            h.Tick(20);
            if(Math.Abs(presenter.HighResolutionFace.SeatedShake)>.01)shakes++;
            maxDelta=Math.Max(maxDelta,Math.Abs(floor-(h.Position.Y+presenter.MeasurePlatformContact()!.Value.SoleY)));
            if(presenter.HighResolutionFace.MotionFrame!=321)missing++;
            if(i<15)trace.Add($"{i}: state={h.Core.State},direct={h.Direct.Target},platform={h.LastPose?.Phase},y={h.Position.Y},motion={presenter.HighResolutionFace.MotionFrame},shake={presenter.HighResolutionFace.SeatedShake}");
        }
        Assert.True(shakes>10,$"Only {shakes} shake frames: "+string.Join(";",trace));
        Assert.True(missing==0,$"Missing seated {missing}: "+string.Join(";",trace));
        Assert.True(maxDelta<.01,$"World sole delta {maxDelta}: "+string.Join(";",trace));
        Assert.Equal(0,presenter.HighResolutionFace.SeatedShake);
        Assert.Null(h.Fault);
    });
}

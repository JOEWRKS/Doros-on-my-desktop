using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(1,false)] [InlineData(1.1,false)] [InlineData(1.25,false)]
    [InlineData(1.35,false)] [InlineData(1.5,false)] [InlineData(1.5,true)]
    public void Crouched_cheek_release_has_one_body_timeline(double scale,bool leave)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        for(var i=0;i<60;i++){h.Pointer=new(true,h.Position+new PointD(72*scale,81*scale));h.Tick(20);}
        var p=h.Presenter!;var captured=p.HighResolutionFace.BodyFrame;
        Assert.InRange(captured,42,99);
        var local=p.HighResolutionFaceToPresenter(new(16,58));
        h.Pointer=new(true,h.Position+new PointD(local.X,local.Y));h.Down=true;
        var image=p.HighResolutionImage!;
        h.Loop.NotifyDirectInteractionPressed(p.CreateDirectPress(image,p.TranslatePoint(local,image))!);h.Tick();
        var axis=h.Direct.CheekPull!.Capture.OutwardUnit;
        h.Pointer=new(true,h.Pointer.Position+new PointD(axis.X*12,axis.Y*12));
        for(var i=0;i<8;i++)h.Tick();h.Down=false;
        for(var i=0;i<150&&h.Direct.Target!=DirectInteractionTarget.None;i++)h.Tick();
        Assert.Equal(DirectInteractionTarget.None,h.Direct.Target);
        Assert.Equal(captured,p.HighResolutionFace.BodyFrame);
        var lastAmount=1d;
        for(var i=0;i<30;i++){
            h.Pointer=new(true,h.Position+(leave?new PointD(900,900):new PointD(72*scale,81*scale)));h.Tick();
            var frame=p.HighResolutionFace.BodyFrame;
            var amount=frame<0?0:HuntPose.At(frame/60d).Amount;
            if(!leave)Assert.Equal(1,amount,8);
            else Assert.True(amount<=lastAmount+1e-9,$"Recovery reversed at tick {i}: {lastAmount} -> {amount}");
            lastAmount=amount;
        }
        if(leave)Assert.Equal(0,lastAmount);
    });
    [Fact]
    public void Unrouted_press_release_resumes_preparation_without_stuck_capture()=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:1.5,huntingPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        for(var i=0;i<40;i++){h.Pointer=new(true,h.Position+new PointD(108,121.5));h.Tick(20);}
        var before=h.Presenter!.HighResolutionFace;
        h.Down=true;for(var i=0;i<20;i++){h.Tick();Assert.Equal(before,h.Presenter.HighResolutionFace);}
        h.Down=false;h.Tick(20);
        Assert.NotEqual(before,h.Presenter.HighResolutionFace);
        Assert.Equal(DirectInteractionTarget.None,h.Direct.Target);Assert.Equal(0,h.Captures);
    });
    [Theory]
    [InlineData(1,0)] [InlineData(1,1)] [InlineData(1.1,3)]
    [InlineData(1.25,0)] [InlineData(1.25,1)] [InlineData(1.35,3)]
    [InlineData(1.5,0)] [InlineData(1.5,1)] [InlineData(1.5,3)]
    public void Crouched_cheek_keeps_pose_across_poll_and_routed_press_order(double scale,int polls) => Controls.CheekProductTests.Sta(() =>
    {
        using var h = new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();
        for(var i=0;i<100;i++)h.Tick(20);
        for(var i=0;i<60;i++){h.Pointer=new(true,h.Position+new PointD(72*scale,81*scale));h.Tick(20);}
        var p=h.Presenter!;
        var body=p.HighResolutionFace.BodyFrame;
        Assert.True(body>=0,$"Expected crouch, got {body}");
        var local=p.HighResolutionFaceToPresenter(new(16,58));
        h.Pointer=new(true,h.Position+new PointD(local.X,local.Y));
        h.Down=true;
        for(var i=0;i<polls;i++)
        {
            // Physical button seen by polling before WPF routes MouseDown.
            h.Tick(16);
            Assert.Equal(body,p.HighResolutionFace.BodyFrame);
            Assert.Equal(DirectInteractionTarget.None,h.Direct.Target);
        }
        {
            var image=p.HighResolutionImage!;
            var press=p.CreateDirectPress(image,p.TranslatePoint(local,image));
            Assert.NotNull(press);
            Assert.Equal(DirectInteractionTarget.RightCheek,press!.Target);
            h.Loop.NotifyDirectInteractionPressed(press);h.Tick(16);
            Assert.Equal(body,p.HighResolutionFace.BodyFrame);
            Assert.NotNull(h.Direct.CheekPull);
            var axis=h.Direct.CheekPull!.Capture.OutwardUnit;
            h.Pointer=new(true,h.Pointer.Position+new PointD(axis.X*12,axis.Y*12));
            for(var i=0;i<8;i++){h.Tick(16);Assert.Equal(body,p.HighResolutionFace.BodyFrame);}
            Assert.True(h.Direct.CheekPull!.PullDips>0);
            h.Down=false;h.Tick(16);
            Assert.Equal(body,p.HighResolutionFace.BodyFrame);
            for(var i=0;i<100;i++)h.Tick(16);
            Assert.Equal(DirectInteractionTarget.None,h.Direct.Target);
        }
    });
}

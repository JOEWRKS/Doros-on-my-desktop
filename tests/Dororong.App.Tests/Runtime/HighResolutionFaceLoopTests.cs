using System.Windows;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void High_resolution_hover_and_cheek_use_real_loop_and_keep_support(double scale)=>Controls.CheekProductTests.Sta(()=>
    {
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true);h.Start();
        var p=h.Presenter!;var floor=h.Position.Y+p.MeasurePlatformContact()!.Value.SoleY;h.Native.Scene=Scene(y:floor);h.Tick(80);
        foreach(var pointerY in new[]{55d,105})foreach(var side in new[]{-1,1}){
            h.Pointer=new(true,h.Position+new PointD((72+side*160)*scale,pointerY*scale));
            for(var i=0;i<8;i++){h.Tick(20);Assert.NotNull(p.HighResolutionImage);Assert.Equal(256*scale,p.HighResolutionImage!.Width,6);}
            var image=p.HighResolutionImage!;var at=p.HighResolutionFaceToPresenter(new(16,58));
            var press=p.CreateDirectPress(image,p.TranslatePoint(at,image));Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.RightCheek,press!.Target);
            var anchor=h.Position+press.WindowLocalPosition;h.Down=true;h.Pointer=new(true,anchor);h.Loop.NotifyDirectInteractionPressed(press);h.Tick();
            Assert.Equal(DirectInteractionTarget.RightCheek,h.Direct.Target);
            var x=h.Position.X;var unit=press.CheekCapture!.OutwardUnit;
            h.Pointer=new(true,anchor+new PointD(unit.X*20,unit.Y*20));h.Tick(20);
            Assert.Equal(20,h.Direct.CheekPull!.PullDips,5);Assert.Equal(x,h.Position.X,5);
            h.Down=false;h.Tick();Assert.Equal(DirectInteractionPhase.CheekRelease,h.Direct.Phase);
            for(var i=0;i<40;i++){
                h.Tick(20);Assert.NotNull(p.HighResolutionImage);
                var lift=h.Direct.Phase==DirectInteractionPhase.CheekRelease?CheekSpringMotion.Lift(h.Direct.ReleaseProgress*CheekSpringMotion.DurationMilliseconds,20):0;
                var error=h.Position.Y+p.MeasurePlatformContact()!.Value.SoleY-floor+lift;
                Assert.True(Math.Abs(error)<=1.1,$"tick={i}; error={error}; position={h.Position}; contact={p.MeasurePlatformContact()}; pose={h.LastPose}; direct={h.Direct}");
            }
            Assert.Equal(DirectInteractionTarget.None,h.Direct.Target);Assert.Equal(h.Captures,h.Releases);
        }
    });
}

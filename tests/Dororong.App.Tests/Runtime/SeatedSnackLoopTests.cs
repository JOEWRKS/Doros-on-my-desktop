using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void Seated_tracking_through_product_loop_keeps_body_and_changes_facing(double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true,snackPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        h.Loop.NotifySitRequested();for(var i=0;i<70;i++)h.Tick(20);
        foreach(var x in new[]{-80d,250d}){
            for(var i=0;i<40;i++){
                h.Pointer=new(true,h.Position+new PointD(x*scale,70*scale));h.Tick(20);
                Assert.Equal(HighResolutionLocomotion.Sit(1),h.Presenter!.HighResolutionFace.MotionFrame);
                Assert.Equal(-1,h.Presenter.HighResolutionFace.BodyFrame);
            }
            Assert.Equal(x<0?FacingDirection.Left:FacingDirection.Right,h.Core.Facing);
            Assert.True(Math.Abs(h.Presenter!.HighResolutionFace.EyeX)>.1);
        }
        Assert.Null(h.Fault);
    });

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Snack_translation_scales_but_authored_gait_distance_keeps_1x_cadence(double scale)=>Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,snackPresentation:true);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        h.Pointer=new(true,new(10000,500));h.Loop.ToggleSnack();var start=h.Position.X;
        for(var i=0;i<50;i++){
            h.Tick(20);
            Assert.InRange(h.Position.X-start,185*scale*(i+1)*.02-.01,185*scale*(i+1)*.02+.01);
            Assert.InRange(h.Snack.Distance,185*(i+1)*.02-.001,185*(i+1)*.02+.001);
            if(i is 0 or 1 or 3 or 5 or 8 or 16 or 25 or 34 or 44 or 49)SaveSnackScale(h.Presenter!,scale,$"run-{i:D2}");
        }
        Assert.Equal(SnackPhase.Chasing,h.Snack.Phase);
        Assert.InRange(h.Position.X-start,185*scale-.01,185*scale+.01);
        // Offer at the rendered mouth: do not multiply the final approach
        // delta after the session has already capped it to the cursor gap.
        for(var i=0;i<100&&h.Snack.Phase!=SnackPhase.Eating;i++){
            h.Pointer=new(true,h.Position+h.Presenter!.GetSnackMouth());h.Tick(20);
            if(i is 0 or 4 or 10 or 20)SaveSnackScale(h.Presenter!,scale,$"stop-{i:D2}");
        }
        Assert.Equal(SnackPhase.Eating,h.Snack.Phase);SaveSnackScale(h.Presenter!,scale,"eating");
        Assert.Null(h.Fault);
    });

    private static void SaveSnackScale(DororongPresenter p,double scale,string label){
        var output=Environment.GetEnvironmentVariable("DORORONG_R70_OUTPUT");if(output is null)return;
        System.IO.Directory.CreateDirectory(output);
        using var file=System.IO.File.Create(System.IO.Path.Combine(output,$"{scale:F2}-{label}.png"));
        var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create((System.Windows.Media.Imaging.BitmapSource)p.HighResolutionImage!.Source));encoder.Save(file);
    }
}

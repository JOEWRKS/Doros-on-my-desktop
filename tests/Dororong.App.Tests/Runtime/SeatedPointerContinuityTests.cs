using Dororong.Core.Geometry;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory][InlineData(1)][InlineData(1.25)][InlineData(1.5)]
    public void Seated_pointer_reach_survives_temporary_scene_query_failure(double scale)
        => Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true,snackPresentation:true);
        h.Native.Scene=Scene();h.Start();for(var i=0;i<100;i++)h.Tick(20);
        h.Loop.NotifySitRequested();for(var i=0;i<70;i++)h.Tick(20);
        Assert.Equal(321,h.Presenter!.HighResolutionFace.MotionFrame);
        var origin=h.Position;var trace=new List<string>();var failures=0;var held=0;
        var floor=origin.Y+h.Presenter.MeasurePlatformContact()!.Value.SoleY;
        var output=Environment.GetEnvironmentVariable("DORORONG_SEATED_CONTINUITY_OUTPUT");
        for(var i=0;i<90;i++){
            h.Pointer=new(true,origin+new PointD(30*scale,85*scale));
            h.Native.Fail=i is >=16 and <22 or >=40 and <48;
            h.Tick(20);
            var face=h.Presenter.HighResolutionFace;
            trace.Add($"{i}: failure={h.Native.Fail},platform={h.LastPose?.Phase},blocked={h.Platforms.SuspendsAutonomousMotion},motion={face.MotionFrame},body={face.BodyFrame},paw={face.SeatedPawAmount:R}");
            if(face.MotionFrame!=321||face.BodyFrame!=-1)failures++;
            if(h.Platforms.SuspendsAutonomousMotion){
                held++;
                Assert.False(h.Platforms.BlocksLocomotionPresentation(true));
                Assert.True(h.Platforms.BlocksLocomotionPresentation(false));
            }
            Assert.Equal(Dororong.Core.Platforms.PlatformPhase.Supported,h.LastPose?.Phase);
            Assert.InRange(Math.Abs(floor-h.Position.Y-h.Presenter.MeasurePlatformContact()!.Value.SoleY),0,.01);
            if(output is not null){
                System.IO.Directory.CreateDirectory(output);
                var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
                encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create((System.Windows.Media.Imaging.BitmapSource)h.Presenter.HighResolutionImage!.Source));
                using var stream=System.IO.File.Create(System.IO.Path.Combine(output,$"outage-{scale:F2}-{i:D3}.png"));encoder.Save(stream);
            }
        }
        if(output is not null)System.IO.File.WriteAllLines(System.IO.Path.Combine(output,$"outage-{scale}.txt"),trace);
        Assert.True(failures==0,$"Standing frames={failures}: "+string.Join(";",trace.Where(x=>!x.Contains("motion=321"))));
        Assert.True(held>5);
    });
    [Fact]
    public void Seated_query_expiry_keeps_window_support_but_confirmed_removal_falls()
        => Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:1,huntingPresentation:true,snackPresentation:true);
        h.Native.Scene=Scene(y:350);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        h.Loop.NotifySitRequested();for(var i=0;i<70;i++)h.Tick(20);
        Assert.Equal((nint)1,h.LastPose!.Value.Support!.Value.Handle);
        var origin=h.Position;
        h.Native.Fail=true;
        for(var i=0;i<50;i++){
            h.Pointer=new(true,origin+new PointD(30,85));h.Tick(20);
            Assert.Equal(321,h.Presenter!.HighResolutionFace.MotionFrame);
            Assert.Equal(Dororong.Core.Platforms.PlatformPhase.Supported,h.LastPose?.Phase);
            Assert.InRange(Math.Abs(h.Position.X-origin.X)+Math.Abs(h.Position.Y-origin.Y),0,.01);
        }
        // Actual loss of support must retain the existing fall/landing priority.
        h.Native.Fail=false;
        h.Native.Scene=Scene(windows:false);var falls=0;
        for(var i=0;i<6;i++){
            h.Tick(20);
            if(h.LastPose?.Phase==Dororong.Core.Platforms.PlatformPhase.Falling){
                falls++;Assert.True(h.Platforms.BlocksLocomotionPresentation(true));
                Assert.Equal(-1,h.Presenter!.HighResolutionFace.MotionFrame);
            }
        }
        Assert.True(falls>0);
    });
    [Theory]
    [InlineData(1, 0)] [InlineData(1, 1)]
    [InlineData(1.25, .8)] [InlineData(1.5, 1)]
    public void Seated_pointer_crossings_keep_seated_torso_with_native_position_rounding(double scale,double readback)
        => Controls.CheekProductTests.Sta(()=>{
        using var h=new Harness(realPresenter:true,highResolutionScale:scale,huntingPresentation:true,
            snackPresentation:true,readbackStep:readback);
        h.Native.Scene=Scene(windows:false);h.Start();for(var i=0;i<100;i++)h.Tick(20);
        h.Loop.NotifySitRequested();for(var i=0;i<70;i++)h.Tick(20);
        var origin=h.Position;
        var failures=new List<string>();var trace=new List<string>();var moving=0;
        for(var i=0;i<360;i++){
            // Screen-space pointer, independent of animated HWND/sole corrections.
            var x=i<120?30:i<240?115:30;
            var y=85+35*Math.Sin(i*Math.PI/60);
            h.Pointer=new(true,origin+new PointD(x*scale,y*scale));h.Tick(20);
            var face=h.Presenter!.HighResolutionFace;
            var row=$"{i}: state={h.Core.State},facing={h.Core.Facing},platform={h.LastPose?.Phase},position={h.Position},motion={face.MotionFrame},body={face.BodyFrame},paw={face.SeatedPawAmount:R}";
            trace.Add(row);
            if(face.SeatedPawAmount>0)moving++;
            if(face.MotionFrame!=321||face.BodyFrame!=-1)failures.Add(row);
        }
        var output=Environment.GetEnvironmentVariable("DORORONG_SEATED_CONTINUITY_OUTPUT");
        if(output is not null){System.IO.Directory.CreateDirectory(output);System.IO.File.WriteAllLines(System.IO.Path.Combine(output,$"trace-{scale}-{readback}.txt"),trace);}
        Assert.True(failures.Count==0,$"Standing frames={failures.Count}: "+string.Join(";",failures.Take(15)));
        Assert.True(moving>40);
    });
}

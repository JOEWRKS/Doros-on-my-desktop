using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class SeatedSleepTrackingTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Seated_gaze_tracks_and_recovers_without_replacing_the_seated_body(double scale)=>CheekProductTests.Sta(()=>{
        var p=Create(scale);var pet=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
        p.SetSittingRequested(true);
        void Tick(PointD? target){
            p.UpdateHuntingWithPounce(pet,DirectInteractionSnapshot.None,target is {} t?new(true,pet.Position+new PointD(t.X*scale,t.Y*scale)):PointerSample.Unavailable,TimeSpan.FromMilliseconds(20),false);
            p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(20));
        }
        for(var i=0;i<60;i++)Tick(null);
        Assert.Equal(HighResolutionLocomotion.Sit(1),p.HighResolutionFace.MotionFrame);
        Save(p,scale,"sit-00-neutral");
        var eyes=new List<(double X,double Y)>();
        var targets=new[]{new PointD(12,77),new PointD(220,77),new PointD(72,10),new PointD(72,130)};
        for(var d=0;d<targets.Length;d++){
            // Near-pointer dwell must not arm a pounce while seated.
            for(var i=0;i<85;i++){
                Tick(targets[d]);
                Assert.Equal(HighResolutionLocomotion.Sit(1),p.HighResolutionFace.MotionFrame);
                Assert.Equal(-1,p.HighResolutionFace.BodyFrame);
                Assert.False(p.Pounce.IsEngaged);
                if(i is 0 or 8 or 40 or 84)Save(p,scale,$"sit-{d+1}-{i:D2}");
            }
            if(p.HighResolutionFace.Closed)for(var i=0;i<20;i++)Tick(targets[d]);
            eyes.Add((p.HighResolutionFace.EyeX,p.HighResolutionFace.EyeY));
        }
        Assert.True(Math.Abs(eyes[0].X-eyes[1].X)>.4,$"Horizontal tracking: {eyes[0]} -> {eyes[1]}");
        Assert.True(Math.Abs(eyes[2].Y-eyes[3].Y)>.3,$"Vertical tracking: {eyes[2]} -> {eyes[3]}");
        for(var i=0;i<70;i++){Tick(null);if(i is 0 or 8 or 30 or 69)Save(p,scale,$"sit-return-{i:D2}");}
        Assert.InRange(Math.Abs(p.HighResolutionFace.EyeX)+Math.Abs(p.HighResolutionFace.EyeY),0,.01);
        Assert.Equal(HighResolutionLocomotion.Sit(1),p.HighResolutionFace.MotionFrame);
    });

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Settled_sleep_keeps_eyes_closed_across_phase_wraps_and_wakes_normally(double scale)=>CheekProductTests.Sta(()=>{
        var p=Create(scale);var pet=new PetSnapshot(PetState.Sleep,new(100,100),FacingDirection.Right,0,false,null);
        void Render(double phase){pet=pet with{Phase=phase};p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(20));}
        Render(0);Assert.False(p.HighResolutionFace.Closed);Save(p,scale,"sleep-00-entry");
        Render(.03);Assert.True(p.HighResolutionFace.Closed);
        Render(.135);Assert.True(p.HighResolutionFace.Closed);
        var n=0;
        for(var cycle=0;cycle<3;cycle++)foreach(var phase in new[]{.95,.995,0,.005,.02,.025,.135}){
            Render(phase);Assert.True(p.HighResolutionFace.Closed,$"Cycle {cycle}, phase {phase}");
            Assert.Equal(HighResolutionLocomotion.Sleep(1),p.HighResolutionFace.MotionFrame);
            Assert.Equal(0,p.HighResolutionFace.EyeX);Assert.Equal(0,p.HighResolutionFace.EyeY);
            if(cycle==0)Save(p,scale,$"sleep-{++n:D2}-{phase:F3}");
        }
        pet=pet with{State=PetState.Idle};
        for(var i=0;i<15;i++){Render(0);if(i is 0 or 4 or 14)Save(p,scale,$"wake-{i:D2}");}
        Assert.False(p.HighResolutionFace.Closed);
        Assert.Equal(-1,p.HighResolutionFace.MotionFrame);
    });

    private static DororongPresenter Create(double scale){
        var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale,Width=144*scale,Height=144*scale};
        ((Canvas)p.Content).Width=p.Width;((Canvas)p.Content).Height=p.Height;return p;
    }
    private static void Save(DororongPresenter p,double scale,string name){
        var output=Environment.GetEnvironmentVariable("DORORONG_R69_OUTPUT");if(output is null)return;
        Directory.CreateDirectory(output);
        using var stream=File.Create(Path.Combine(output,$"{scale:F2}-{name}.png"));
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create((BitmapSource)p.HighResolutionImage!.Source));encoder.Save(stream);
    }
}

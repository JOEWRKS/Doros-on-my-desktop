using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.App.Interaction;

namespace Dororong.App.Tests.Controls;

public class SnackRenderTests
{
    [Fact] public void Happy_sways_slowly_without_accent_holds_then_diminishes_to_rest()
    {
        double Degrees(double age)=>SnackRenderer.HappyRoll(age)*180/Math.PI;
        Assert.Equal(-8,Degrees(.46),8);Assert.Equal(8,Degrees(1.10),8);
        Assert.Equal(-4,Degrees(1.64),8);
        Assert.Equal(0,Degrees(0));Assert.Equal(0,Degrees(SnackSession.HappySeconds));
        Assert.True(Degrees(.48)>Degrees(.46));Assert.True(Degrees(1.12)<Degrees(1.10));
        for(var t=0d;t<SnackSession.HappySeconds;t+=.001)
            Assert.InRange(Math.Abs((Degrees(t+.001)-Degrees(t))/.001),0,38);
        foreach(var t in new[]{0,.06,.46,1.10,1.64,2.10,SnackSession.HappySeconds})
        {
            Assert.InRange(Math.Abs((Degrees(t+.00001)-Degrees(t))/.00001),0,.03);
            Assert.InRange(Math.Abs((Degrees(t)-Degrees(t-.00001))/.00001),0,.03);
        }
    }

    [Fact] public void Happy_reuses_accepted_closed_eye_rotation_keeps_feet_and_has_exact_endpoints() => CheekProductTests.Sta(()=>
    {
        var renderer=new SnackRenderer();var accepted=new HuntRenderer();
        var original=PremultipliedFrame.From(UprightRumpSource.Standing(true)).Pixels;
        foreach(var age in Enumerable.Range(0,136).Select(i=>i*.016))
        {
            var image=renderer.Render(new(SnackPhase.Happy,FacingDirection.Left,0,0,age,0),default);
            var actual=PremultipliedFrame.From(image).Pixels;
            var gaze=new HuntGazePose(0,0,SnackRenderer.HappyRoll(age));
            Assert.Equal(PremultipliedFrame.From(accepted.Render(156,gaze,closed:true)).Pixels,actual);
            Assert.Equal(HuntRenderer.HeadTransform(156,gaze),renderer.HeadTransform);
            Assert.True(image.IsFrozen);
            // No movement or resampling of any planted sole/hindfoot.
            Assert.Equal(original.AsSpan(80*384).ToArray(),actual.AsSpan(80*384).ToArray());
        }
        foreach(var age in new[]{0d,SnackSession.HappySeconds})
            Assert.Equal(original,PremultipliedFrame.From(renderer.Render(new(SnackPhase.Happy,FacingDirection.Left,0,0,age,0),default)).Pixels);
    });

    [Fact] public void Landing_does_not_replay_the_hunting_forepaw_reach() => CheekProductTests.Sta(()=>
    {
        var renderer=new SnackRenderer();
        var standing=PremultipliedFrame.From(UprightRumpSource.Standing(false)).Pixels;
        for(var i=0;i<=20;i++)
        {
            var age=SnackSession.JumpPrepareSeconds+SnackSession.JumpFlightSeconds+i*.01;
            Assert.Equal(156,SnackRenderer.JumpFrame(age));
            var actual=PremultipliedFrame.From(renderer.Render(new(SnackPhase.Chasing,FacingDirection.Left,0,0,1,0){JumpAge=age},default)).Pixels;
            // Entire fore/hind foot region stays in the approved planted pose.
            for(var y=75;y<96;y++)for(var x=0;x<96;x++)for(var c=0;c<4;c++)
                Assert.Equal(standing[(y*96+x)*4+c],actual[(y*96+x)*4+c]);
        }
    });
    [Fact] public void Closed_chewing_uses_source_mouth_and_returns_to_exact_original() => CheekProductTests.Sta(()=>
    {
        var source=UprightRumpSource.Standing(true);var original=PremultipliedFrame.From(source).Pixels;
        Assert.Same(source,SnackMouthFrame.Chew(source,.48));
        Assert.Same(source,SnackMouthFrame.Chew(source,1.55));
        var renderer=new SnackRenderer();
        foreach(var age in new[]{.60,.72,.94,1.21,1.58})
        {
            var expected=PremultipliedFrame.From(SnackMouthFrame.Chew(source,age)).Pixels;
            var actual=PremultipliedFrame.From(renderer.Render(new(SnackPhase.Eating,FacingDirection.Left,0,0,age,0),default)).Pixels;
            Assert.Equal(expected,actual); // No separately drawn replacement lips/blush.
            for(var y=0;y<96;y++)for(var x=0;x<96;x++)
                if(x<24||x>34||y<56||y>64)
                    for(var c=0;c<4;c++)Assert.Equal(original[(y*96+x)*4+c],actual[(y*96+x)*4+c]);
        }
    });
    [Fact] public void Eating_frames_change_mouth_without_changing_alpha_silhouette() => CheekProductTests.Sta(()=>
    {
        var r=new SnackRenderer();
        var a=r.Render(new(SnackPhase.Eating,FacingDirection.Left,0,0,.1,0),default);
        var b=r.Render(new(SnackPhase.Eating,FacingDirection.Left,0,0,.25,0),default);
        Assert.Equal(96,a.PixelWidth);Assert.Equal(96,a.PixelHeight);
        var aa=new byte[96*96*4];var bb=new byte[aa.Length];a.CopyPixels(aa,384,0);b.CopyPixels(bb,384,0);
        Assert.NotEqual(aa,bb);
        for(var i=3;i<aa.Length;i+=4)Assert.Equal(aa[i],bb[i]);
        Assert.True(a.IsFrozen);Assert.True(b.IsFrozen);
    });
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_snack_preserves_every_original_pixel_outside_local_mouth(bool eating) => CheekProductTests.Sta(()=>
    {
        var r=new SnackRenderer();var a=r.Render(new(eating?SnackPhase.Eating:SnackPhase.Chasing,FacingDirection.Left,0,0,.6,0),default);
        var expected=PremultipliedFrame.From(UprightRumpSource.Standing(eating)).Pixels;
        var actual=PremultipliedFrame.From(a).Pixels;
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)
        {
            if(x>=19&&x<=39&&y>=55&&y<=75)continue;
            var at=(y*96+x)*4;
            for(var c=0;c<4;c++)Assert.Equal(expected[at+c],actual[at+c]);
        }
    });
    [Fact] public void Run_has_asymmetric_fore_hind_beats_and_a_suspended_phase() => CheekProductTests.Sta(()=>
    {
        var suspension=false;var unique=new HashSet<string>();
        for(var i=0;i<48;i++)
        {
            var phase=i/48d;
            suspension|=Enumerable.Range(0,4).All(n=>SnackRunFrames.Foot(n,phase).Y<-.01);
            var frame=SnackRunFrames.Render(phase*SnackRunFrames.Stride,1);
            unique.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(PremultipliedFrame.From(frame).Pixels)));
            Assert.Equal(96,frame.PixelWidth);
        }
        Assert.True(suspension);Assert.True(unique.Count>=40);
        Assert.NotEqual(SnackRunFrames.Foot(0,.15),SnackRunFrames.Foot(2,.15));
        Assert.NotEqual(SnackRunFrames.Foot(0,.15),SnackRunFrames.Foot(1,.15));
    });
    [Theory]
    [InlineData(SnackPhase.Eating)] [InlineData(SnackPhase.Happy)] [InlineData(SnackPhase.Settling)]
    public void Snack_reaction_blocks_hunting_even_if_the_presenter_is_polled_directly(SnackPhase phase) => CheekProductTests.Sta(()=>
    {
        var p=new DororongPresenter();var snapshot=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Left,0,false,null);
        p.SetSnack(new(phase,FacingDirection.Left,0,0,.6,0),new(true,new(230,181)));
        for(var i=0;i<120;i++)
        {
            Assert.False(p.UpdateHuntingWithPounce(snapshot,DirectInteractionSnapshot.None,new(true,new(230,181)),TimeSpan.FromMilliseconds(16),false));
            p.RenderDesktop(snapshot,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
            Assert.Equal(PouncePhase.Watch,p.Pounce.Phase);
        }
    });
    [Fact] public void Hunt_departure_cancels_behavior_but_recovers_actual_crouch_over_multiple_frames() => CheekProductTests.Sta(()=>
    {
        var p=new DororongPresenter();var snapshot=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Left,0,false,null);
        void Step(PointerSample pointer){p.UpdateHuntingWithPounce(snapshot,DirectInteractionSnapshot.None,pointer,TimeSpan.FromMilliseconds(16),false);p.RenderDesktop(snapshot,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));}
        for(var i=0;i<60;i++)Step(new(true,new(230,181)));
        var before=HuntPose.At(p.HuntingVisualFrame/60d).Amount;
        Assert.True(before>.9);Step(PointerSample.Unavailable);
        Assert.Equal(156,p.HuntingFrame);Assert.Equal(PouncePhase.Watch,p.Pounce.Phase);
        var after=HuntPose.At(p.HuntingVisualFrame/60d).Amount;
        Assert.InRange(after,.8,before);
        for(var i=0;i<20;i++)Step(PointerSample.Unavailable);
        Assert.Equal(156,p.HuntingVisualFrame);
    });
    [Fact] public void Facing_mirrors_mouth_and_retirement_restores_normal_source() => CheekProductTests.Sta(()=>
    {
        var p=new DororongPresenter();p.Measure(new Size(144,144));p.Arrange(new Rect(0,0,144,144));
        var snapshot=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Left,0,false,null);
        p.SetSnack(new(SnackPhase.Chasing,FacingDirection.Left,0,0,.1,0),new(true,new(100,184)));
        p.RenderDesktop(snapshot,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));var left=p.GetSnackMouth();
        p.SetSnack(new(SnackPhase.Chasing,FacingDirection.Right,0,0,.1,0),new(true,new(300,184)));
        p.RenderDesktop(snapshot with {Facing=FacingDirection.Right},DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
        var right=p.GetSnackMouth();Assert.True(right.X>left.X+30);
        var image=(Image)p.FindName("DororongImage");var snack=image.Source;
        p.SetSnack(default,PointerSample.Unavailable);p.RenderDesktop(snapshot,DirectInteractionSnapshot.None);
        Assert.NotSame(snack,image.Source);
    });
}

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public sealed class CrossStateSourceTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Neutral_states_share_every_authored_pixel(bool closed)=>CheekProductTests.Sta(()=>
    {
        _=new DororongPresenter();var source=UprightRumpSource.Standing(closed);
        Assert.Same(source,LocomotionFrames.Sit(0,closed));
        Assert.Same(source,LocomotionFrames.Walk(0,7.3,closed));
        foreach(var frame in new[]{0,15,156})Assert.Same(source,new HuntRenderer().Render(frame,default,closed));
    });

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Breathing_keeps_head_and_feet_exact_at_every_sample(bool closed)=>CheekProductTests.Sta(()=>
    {
        _=new DororongPresenter();var source=Pixels(UprightRumpSource.Standing(closed));
        var mask=HuntFrames.Instance.Head(closed?1:0).ToArray();var changed=0;
        for(var n=0;n<=32;n++)
        {
            var frame=Pixels(BodyBreathingFrame.Standing(closed,n/32d));
            for(var i=0;i<frame.Length;i+=4)
            {
                if(mask[i+3]>0 || i>=74*384)Assert.Equal(source.AsSpan(i,4).ToArray(),frame.AsSpan(i,4).ToArray());
                if(!source.AsSpan(i,4).SequenceEqual(frame.AsSpan(i,4)))changed++;
            }
        }
        Assert.True(changed>100,"Breathing should move the torso, not disable all motion");
    });

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void All_walking_phases_keep_the_unfiltered_head_at_integer_bob(bool closed)=>CheekProductTests.Sta(()=>
    {
        _=new DororongPresenter();var source=Pixels(UprightRumpSource.Standing(closed));
        for(var n=0;n<32;n++)
        {
            var frame=Pixels(LocomotionFrames.Walk(1,n*10d/32,closed));
            var bob=(int)Math.Round(-.55*(1-Math.Cos(n/32d*4*Math.PI)));
            for(var y=20;y<55;y++)for(var x=18;x<62;x++)for(var c=0;c<4;c++)
                Assert.Equal(source[(y*96+x)*4+c],frame[((y+bob)*96+x)*4+c]);
        }
    });

    [Fact]
    public void Actual_idle_breathing_never_scales_the_whole_image()=>CheekProductTests.Sta(()=>
    {
        var presenter=new DororongPresenter();
        foreach(var phase in new[]{0,.19,.4,.7,.99})
        {
            presenter.RenderDesktop(new(PetState.Idle,new(100,100),FacingDirection.Left,phase,false,null),DirectInteractionSnapshot.None,TimeSpan.Zero);
            var scale=(ScaleTransform)presenter.FindName("ImageBreathingScaleTransform");
            Assert.Equal(1,scale.ScaleX);Assert.Equal(1,scale.ScaleY);
            var source=Pixels(UprightRumpSource.Standing(false));var actual=Pixels((BitmapSource)((Image)presenter.FindName("DororongImage")).Source);
            Assert.Equal(source.AsSpan(0,49*384).ToArray(),actual.AsSpan(0,49*384).ToArray());
        }
    });

    [Theory]
    [InlineData(0)] [InlineData(16)] [InlineData(28)] [InlineData(42)]
    [InlineData(90)] [InlineData(132)] [InlineData(155)] [InlineData(156)]
    public void Snack_and_ordinary_hunting_use_identical_sampling(int frame)=>CheekProductTests.Sta(()=>
    {
        _=new DororongPresenter();var renderer=new HuntRenderer();
        Assert.Equal(Pixels(renderer.Render(frame,default)),Pixels(renderer.Render(frame,default,crisp:true)));
    });
    private static byte[] Pixels(BitmapSource image)=>PremultipliedFrame.From(image).Pixels;
}

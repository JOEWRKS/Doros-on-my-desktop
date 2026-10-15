using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;
using Dororong.Portable.Rendering;

namespace Dororong.Portable.Tests;

public sealed class InteractionTests
{
    private sealed class Still:IRandomSource{public double NextUnit()=>.99;}
    private static readonly SceneSnapshot scene=new(new(1,TimeSpan.Zero,[new DesktopMonitor(1,new(0,0,1400,900))],[]),SceneHealth.Fresh);
    private static readonly TimeSpan dt=TimeSpan.FromMilliseconds(16);
    private static DesktopPetController Pet(){var p=new DesktopPetController(new(300,791),new Still());p.Advance(dt,new(true,new(1300,0)),false,scene);return p;}
    [Fact]
    public void HeadPresentationBothFacingsRemainInsideTransparentViewport()
    {
        using var sheet=new SkiaSharp.SKBitmap(144*6,144*2);using var canvas=new SkiaSharp.SKCanvas(sheet);canvas.Clear(new SkiaSharp.SKColor(235,233,227));
        foreach(var flip in new[]{false,true})
        {
            var head=new HeadDragPresentation();var pointer=new PointD(flip?84:60,50);var facing=flip?FacingDirection.Left:FacingDirection.Right;
            head.Begin(pointer,default,DesktopFrames.Place(MotionRenderer.Standing(),flip),facing);
            var column=0;
            for(var i=0;i<36;i++)
            {
                var offset=i<5?new PointD(20,0):new PointD(90+Math.Sin(i*.3)*50,-20);
                head.Advance(dt,pointer+offset,true,i<26);
                if(!head.Active||i is not (0 or 4 or 10 or 20 or 26 or 34))continue;
                var frame=head.Render();var pixels=frame.Pixels.Span;
                for(var x=0;x<144;x++){Assert.Equal(0,pixels[x*4+3]);Assert.Equal(0,pixels[(143*144+x)*4+3]);}
                using var image=frame.ToBitmap();canvas.DrawBitmap(image,column++*144,flip?144:0);
            }
        }
        using var result=SkiaSharp.SKImage.FromBitmap(sheet);using var png=result.Encode(SkiaSharp.SKEncodedImageFormat.Png,100);using var output=System.IO.File.Create(System.IO.Path.Combine(AppContext.BaseDirectory,"head-drag-sequence.png"));png.SaveTo(output);
    }
    [Fact]
    public void HuntLeavesPreparationImmediatelyAndDoesNotLaunchOnOldDwell()
    {
        var h=new HuntingPresentation();var source=MotionRenderer.Standing();
        for(var i=0;i<70;i++){h.Advance(.016,new(100,80),FacingDirection.Right,false);h.Render(source,FacingDirection.Right,false,0);}
        Assert.True(h.Preparing);h.Advance(.016,new(900,0),FacingDirection.Right,false);Assert.False(h.Preparing);Assert.False(h.Pounce.IsEngaged);
        for(var i=0;i<50;i++)h.Advance(.016,new(100,80),FacingDirection.Right,false);
        Assert.False(h.Pounce.IsJump);
    }
    [Fact]
    public void PounceOwnsFlightThenCooldownAndSnackCancelsIt()
    {
        var pet=Pet();var launched=false;var tracked=false;
        for(var i=0;i<160;i++){pet.Advance(dt,new(true,pet.Position+new PointD(115,75)),false,scene);launched|=pet.Pounce.IsJump;tracked|=pet.Pounce.Phase==PouncePhase.Track;}
        Assert.True(launched);Assert.True(tracked);pet.BeginSnack();Assert.False(pet.Pounce.IsEngaged);Assert.Equal(SnackPhase.Chasing,pet.Snack);
    }
    [Fact]
    public void SleepUsesCoreDelayAndSwitchOffWakesWithoutHunting()
    {
        var pet=Pet();for(var i=0;i<1850;i++)pet.Advance(TimeSpan.FromMilliseconds(50),new(true,new(1300,0)),false,scene);
        Assert.Equal(PetState.Sleep,pet.State);Assert.True(pet.AutomaticSleep);
        pet.ToggleSleep();Assert.False(pet.AutomaticSleep);Assert.Equal(PetState.Idle,pet.State);
        for(var i=0;i<15;i++)pet.Advance(dt,new(true,new(1300,0)),false,scene);
        Assert.False(pet.Pounce.IsEngaged);
    }
    [Fact]
    public void SittingPinsBodyAndSuppressesHuntUntilRise()
    {
        var pet=Pet();pet.ToggleSit();var x=pet.Position.X;
        for(var i=0;i<100;i++)pet.Advance(dt,new(true,pet.Position+new PointD(110,80)),false,scene);
        Assert.Equal(x,pet.Position.X);Assert.False(pet.Pounce.IsEngaged);Assert.False(pet.Preparing);
        pet.ToggleSit();for(var i=0;i<50;i++)pet.Advance(dt,new(true,new(1300,0)),false,scene);Assert.False(pet.Sitting);
    }
    [Fact]
    public void HeadPullKeepsOffsetAcrossPartialAndFullExtensionAndRecovers()
    {
        var pet=Pet();var press=pet.Position+new PointD(60,50);Assert.True(pet.TryBeginHeadDrag(press));var origin=pet.Position;
        pet.Advance(dt,new(true,press+new PointD(20,-10)),true,scene);Assert.InRange(pet.HeadStrength,.1,.5);Assert.Equal(origin+new PointD(20,-10),pet.Position);
        pet.Advance(dt,new(true,press+new PointD(120,-40)),true,scene);Assert.True(pet.HeadHeld);Assert.Equal(origin+new PointD(120,-40),pet.Position);
        for(var i=0;i<8;i++)pet.Advance(dt,new(true,press+new PointD(120+i*5,-40)),true,scene);
        Assert.NotEqual(0,pet.HeadAngle);var last=pet.Position;
        pet.Advance(dt,new(true,new(1000,0)),false,scene);Assert.Equal(last.X,pet.Position.X);Assert.False(pet.Dragging);
        for(var i=0;i<16;i++)pet.Advance(dt,new(true,new(1300,0)),false,scene);Assert.Equal(0,pet.HeadStrength);
        Assert.False(pet.TryBeginHeadDrag(pet.Position+new PointD(70,108))); // feet are not head-grab targets
    }
    [Fact]
    public void UnavailablePointerOrResumeCannotLeaveHeadCaptureActive()
    {
        var pet=Pet();var press=pet.Position+new PointD(60,50);Assert.True(pet.TryBeginHeadDrag(press));
        pet.Advance(dt,new(true,press+new PointD(110,0)),true,scene);Assert.True(pet.HeadHeld);
        pet.Advance(TimeSpan.FromSeconds(2),PointerSample.Unavailable,false,scene);Assert.False(pet.Dragging);Assert.False(pet.HeadHeld);
    }
}

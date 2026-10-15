using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;
using Dororong.Portable.Rendering;

namespace Dororong.Portable.Tests;

public sealed class DirectTests
{
    private sealed class Still:IRandomSource{public double NextUnit()=>.99;}
    private static readonly TimeSpan step=TimeSpan.FromMilliseconds(16);
    private static readonly DesktopMonitor[] monitors=[new(1,new(0,0,1400,900))];
    private static SceneSnapshot Scene(bool window=false)=>new(new MacSceneMapper().Build(TimeSpan.Zero,monitors,
        window?[new MacWindowRow(1,7,new(100,300,800,400),0,1,true,false)]:[],99,20,101),SceneHealth.Fresh);
    private static DesktopPetController Pet(){var p=new DesktopPetController(new(300,791),new Still());p.Advance(step,new(true,new(1300,0)),false,Scene());return p;}
    private static PointD Find(DesktopPetController p,DirectInteractionTarget target)
    {
        for(var y=0;y<144;y++)for(var x=0;x<144;x++){var at=p.Position+new PointD(x+.5,y+.5);if(p.Classify(at)==target)return at;}
        throw new Exception("No visible target: "+target);
    }
    private static DesktopPetController Perched()
    {
        var p=new DesktopPetController(new(150,216),new Still());var pointer=new PointerSample(true,p.Position+new PointD(72,50));p.BeginDrag(pointer.Position);
        p.Advance(step,pointer,true,Scene(true));p.Advance(step,pointer,false,Scene(true));
        for(var i=0;i<16;i++)p.Advance(step,pointer,false,Scene(true));Assert.Equal(EdgePerchPhase.Attached,p.Perch);return p;
    }
    [Fact]
    public void ExtremeDropSpreadsAndRecoversAndCanBeInterruptedByHead()
    {
        var p=new DesktopPetController(new(300,-50),new Still());var far=new PointerSample(true,new(1300,0));var extreme=false;var spread=false;
        for(var i=0;i<300;i++)
        {
            p.Advance(step,far,false,Scene());extreme|=p.Platform.IsExtremeLanding;spread|=p.Platform.LegSpread>.95;
        }
        Assert.True(extreme);Assert.True(spread);Assert.Equal(PlatformPhase.Supported,p.Platform.Phase);
        p.BeginDrag(p.Position+new PointD(60,40));p.Advance(step,new(true,new(360,0)),true,Scene());p.Advance(step,new(true,new(1300,0)),false,Scene());
        for(var i=0;i<140&&!p.Platform.IsExtremeLanding;i++)p.Advance(step,far,false,Scene());Assert.True(p.Platform.IsExtremeLanding);
        var at=Find(p,DirectInteractionTarget.Body);Assert.True(p.TryBeginInteraction(at));p.Advance(step,new(true,at),true,Scene());Assert.False(p.Platform.IsExtremeLanding);Assert.True(p.Capturing);
    }
    [Fact]
    public void PendingHeadPressKeepsSoleAndCancelsOnMenu()
    {
        var p=Pet();var at=Find(p,DirectInteractionTarget.Body);var sole=DesktopFrames.Measure(p.Frame).Contact.SoleY;var origin=p.Position;
        Assert.True(p.TryBeginInteraction(at));for(var i=0;i<5;i++){p.Advance(step,new(true,at),true,Scene());Assert.Equal(sole,DesktopFrames.Measure(p.Frame).Contact.SoleY);Assert.Equal(origin,p.Position);}
        p.Advance(step,new(true,at),false,Scene());Assert.True(p.ClickReacting);p.MenuOpen=true;p.Advance(step,new(true,at),false,Scene());Assert.False(p.ClickReacting);
    }
    [Fact]
    public void BodyTapHopsWithoutCarryAndSleepClickWakes()
    {
        var p=Pet();var at=Find(p,DirectInteractionTarget.ClickOnly);var origin=p.Position;
        Assert.True(p.TryBeginInteraction(at));p.Advance(step,new(true,at),true,Scene());p.Advance(step,new(true,at),false,Scene());Assert.True(p.ClickReacting);
        for(var i=0;i<17;i++)p.Advance(step,new(true,new(1300,0)),false,Scene());
        Assert.Equal(origin.X,p.Position.X);Assert.True(p.Position.Y<origin.Y);
        for(var i=0;i<30;i++)p.Advance(step,new(true,new(1300,0)),false,Scene());Assert.False(p.ClickReacting);Assert.Equal(origin.Y,p.Position.Y,6);
        for(var i=0;i<1850;i++)p.Advance(TimeSpan.FromMilliseconds(50),new(true,new(1300,0)),false,Scene());Assert.Equal(PetState.Sleep,p.State);
        at=Find(p,DirectInteractionTarget.ClickOnly);Assert.True(p.TryBeginInteraction(at));Assert.NotEqual(PetState.Sleep,p.State);p.Advance(step,new(true,at),false,Scene());Assert.True(p.ClickReacting);
        p.BeginSnack();Assert.False(p.ClickReacting);
    }
    [Fact]
    public void CheekPullHoldsPositionThenHopsAfter65msAndStaysAtNewLanding()
    {
        var p=Pet();var press=Find(p,DirectInteractionTarget.RightCheek);var origin=p.Position;
        Assert.False(p.TryBeginHeadDrag(press));Assert.True(p.TryBeginInteraction(press));
        for(var i=0;i<15;i++)p.Advance(step,new(true,press+new PointD(-40,0)),true,Scene());
        Assert.Equal(20,p.CheekPull);Assert.Equal(origin,p.Position);Assert.False(p.Preparing);
        p.Advance(step,new(true,new(1200,0)),false,Scene());Assert.Equal(origin,p.Position);
        for(var i=0;i<4;i++)p.Advance(step,new(true,new(1300,0)),false,Scene());Assert.Equal(origin,p.Position);
        p.Advance(step,new(true,new(1300,0)),false,Scene());Assert.True(p.Position.X>origin.X);Assert.True(p.Position.Y<origin.Y);
        for(var i=0;i<24;i++)p.Advance(step,new(true,new(1300,0)),false,Scene());
        Assert.False(p.CheekActive);Assert.Equal(origin.X+8,p.Position.X,6);Assert.Equal(origin.Y,p.Position.Y,6);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void PerchedPawTapAndPullStayOnTheirSupport(bool right)
    {
        var p=Perched();var origin=p.Position;var press=Find(p,right?DirectInteractionTarget.PerchRightPaw:DirectInteractionTarget.PerchLeftPaw);
        Assert.True(p.TryBeginInteraction(press));p.Advance(step,new(true,press),true,Scene(true));p.Advance(step,new(true,press),false,Scene(true));
        p.Advance(step,new(true,press),false,Scene(true));Assert.NotEqual(0,p.Paw!.Angle);Assert.Equal(right,p.Paw.Right);Assert.Equal(origin,p.Position);
        for(var i=0;i<40;i++)p.Advance(step,new(true,press),false,Scene(true));Assert.Null(p.Paw);
        Assert.True(p.TryBeginInteraction(press));p.Advance(step,new(true,press+new PointD(0,60)),true,Scene(true));
        Assert.Equal(18,p.Paw!.Offset.Y);Assert.Equal(origin,p.Position);Assert.Equal(EdgePerchPhase.Attached,p.Perch);
        p.Advance(step,new(true,press),true,Scene());Assert.Null(p.Paw);Assert.Equal(EdgePerchPhase.None,p.Perch);
    }
    [Fact]
    public void PerchedCheekDoesNotUnhookOrHop()
    {
        var p=Perched();var origin=p.Position;var press=Find(p,DirectInteractionTarget.RightCheek);Assert.True(p.TryBeginInteraction(press));
        for(var i=0;i<8;i++)p.Advance(step,new(true,press+new PointD(-30,0)),true,Scene(true));
        for(var i=0;i<32;i++){p.Advance(step,new(true,press),false,Scene(true));Assert.Equal(origin,p.Position);Assert.Equal(EdgePerchPhase.Attached,p.Perch);}
    }
    [Fact]
    public void SleepHeadMovesWithDescriptorAndFeetNeverCarry()
    {
        var p=Pet();for(var i=0;i<1850;i++)p.Advance(TimeSpan.FromMilliseconds(50),new(true,new(1300,0)),false,Scene());
        Assert.Equal(PetState.Sleep,p.State);var head=Find(p,DirectInteractionTarget.Body);var foot=Find(p,DirectInteractionTarget.ClickOnly);
        Assert.False(p.TryBeginHeadDrag(foot));Assert.True(p.TryBeginHeadDrag(head));
    }
    [Fact]
    public void HuntPoseCheekCapturesVisibleFrameAndBlocksHuntingUntilRelease()
    {
        var p=Pet();for(var i=0;i<60;i++)p.Advance(step,new(true,p.Position+new PointD(105,65)),false,Scene());
        var press=Find(p,DirectInteractionTarget.RightCheek);Assert.True(p.Preparing);Assert.True(p.TryBeginInteraction(press));
        p.Advance(step,new(true,press+new PointD(-30,0)),true,Scene());Assert.True(p.CheekActive);Assert.False(p.Preparing);Assert.False(p.Pounce.IsEngaged);
        p.BeginSnack();Assert.False(p.CheekActive);Assert.Equal(SnackPhase.Chasing,p.Snack);
    }
    [Fact]
    public void TransparentGutterDoesNotMoveLogicalOriginOrCaptureEmptyArea()
    {
        var p=Pet();var frame=p.ViewportFrame;Assert.Equal(336,frame.PixelWidth);
        for(var y=0;y<144;y++)Assert.Equal(p.Frame.Pixels.Slice(y*576,576).ToArray(),frame.Pixels.Slice(((y+96)*336+96)*4,576).ToArray());
        Assert.False(p.Hit(p.Position-new PointD(90,90)));
        var head=new HeadDragPresentation();var origin=new PointD(60,35);head.Begin(origin,default,DesktopFrames.Place(MotionRenderer.Standing(),false),FacingDirection.Right);
        for(var i=0;i<25;i++){head.Advance(step,origin+new PointD(100+i*20,-10),true,true);head.Render();}
        Assert.NotNull(head.ViewportFrame);Assert.Equal(336,head.ViewportFrame!.PixelWidth);Assert.True(Math.Abs(head.Angle)>0);
        var bytes=head.ViewportFrame.Pixels.Span;for(var x=0;x<336;x++){Assert.Equal(0,bytes[x*4+3]);Assert.Equal(0,bytes[(335*336+x)*4+3]);}
    }
    [Fact]
    public void LostPointerAndLongResumeCancelLocalCaptures()
    {
        var p=Pet();Assert.True(p.TryBeginInteraction(Find(p,DirectInteractionTarget.RightCheek)));
        p.Advance(TimeSpan.FromSeconds(2),new(false,default),true,Scene());Assert.False(p.Capturing);Assert.False(p.CheekActive);
    }
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void MirroredAndSeatedCheekUsesOutwardAxisAndNeverDrags(bool seated)
    {
        var cheek=new CheekPresentation();var source=MotionRenderer.Standing();var press=new PointD(100,100);
        cheek.Begin(press,source,new(-1,0,0,1,120,24),new(1,0),false,seated);
        cheek.Advance(16,press+new PointD(40,0),true);Assert.Equal(20,cheek.Pull);Assert.True(cheek.Flipped);Assert.Equal(0,cheek.DeltaX);
        cheek.Advance(16,press+new PointD(-40,0),true);Assert.Equal(seated?20:-10,cheek.Pull);Assert.Equal(!seated,cheek.Flipped);
        cheek.Advance(16,press,false);for(var i=0;i<5;i++)cheek.Advance(16,press,false);
        Assert.Equal(seated,cheek.DeltaX>0);
    }
    [Fact]
    public void SeatedCaptureAndZeroPullDoNotChangePixelsOrPosition()
    {
        var pet=Pet();pet.ToggleSit();for(var i=0;i<70;i++)pet.Advance(step,new(true,new(1300,0)),false,Scene());
        var before=pet.Frame.CopyPixels();var position=pet.Position;var press=Find(pet,DirectInteractionTarget.RightCheek);
        Assert.True(pet.TryBeginInteraction(press));pet.Advance(step,new(true,press),true,Scene());
        Assert.Equal(before,pet.Frame.CopyPixels());Assert.Equal(position,pet.Position);Assert.True(pet.Sitting);
    }
}

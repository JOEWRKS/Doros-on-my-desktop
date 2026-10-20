using System.Windows;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Controls;

public class FirstImpactFrameTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Contact_then_first_spread_preserves_approved_onset_at_each_scale(double scale)=>CheekProductTests.Sta(()=>
    {
        var presenter=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=scale};
        var pet=new PetSnapshot(PetState.Idle,new(100,0),FacingDirection.Right,0,false,null);
        presenter.Render(pet,DirectInteractionSnapshot.None);
        presenter.Measure(new(144*scale,144*scale));presenter.Arrange(new Rect(0,0,144*scale,144*scale));presenter.UpdateLayout();
        var before=presenter.MeasurePlatformGeometry()!.Value;
        var motion=new PlatformMotion();var position=pet.Position;var contact=before.Contact;
        for(var i=0;i<4;i++)position=motion.Advance(new(TimeSpan.FromMilliseconds(250),position,position,contact,[],false,true)).Position;
        var top=position.Y+contact.SoleY+4.8;
        var floor=new PlatformSurface(new(0,0,1),1,PlatformKind.Floor,0,1000,top,new(0,top));
        var pose=motion.Advance(new(TimeSpan.FromMilliseconds(16),position,position,contact,[floor],false,true)
            {Monitors=[new(1,new(0,0,1000,100))]});
        Assert.True(pose.IsExtremeLanding);Assert.Equal(0,pose.LegSpread);
        presenter.ApplyPlatformPose(pose,before.Contact.SoleY);
        Assert.Equal(0,presenter.LastRenderedSpread);
        pose=motion.Advance(new(TimeSpan.FromMilliseconds(8),pose.Position,pose.Position,contact,[floor],false,true)
            {Monitors=[new(1,new(0,0,1000,100))]});
        Assert.InRange(pose.LegSpread,.54,.55);
        presenter.BeginHighResolutionPlatformFrame();
        presenter.Render(pet,DirectInteractionSnapshot.None);
        presenter.ApplyPlatformPose(pose,before.Contact.SoleY);
        Assert.Equal(pose.LegSpread,presenter.LastRenderedSpread);
        var after=presenter.MeasurePlatformGeometry()!.Value;
        Assert.True(after.Bounds.Width>before.Bounds.Width);
        Assert.True(after.Bounds.Height<before.Bounds.Height);
        Assert.InRange(Math.Abs(after.Contact.SoleY-before.Contact.SoleY),0,2);
    });
}

using System.Windows.Controls;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public sealed class ArticulatedSittingTests
{
    [Fact]
    public void Seating_keeps_a_settling_interval_instead_of_finishing_with_the_old_morph()
    {
        var motion = new LocomotionPresentation();
        motion.Advance(650, false, 0, false, true);
        Assert.InRange(motion.Sit, .5, .95);
        Assert.True(motion.HoldWalking);
        motion.Advance(250, false, 0, false, true);
        Assert.InRange(motion.Sit, .8, .99);
        motion.Advance(150, false, 0, false, true);
        Assert.Equal(1, motion.Sit);
    }

    [Fact]
    public void Fully_seated_rise_uses_the_accepted_part_geometry_in_reverse() => EdgePerchPresentationTests.Sta(() =>
    {
        var p = new DororongPresenter();
        var pet = new PetSnapshot(PetState.Idle, new(100,100), FacingDirection.Right, 0, false, null);
        p.SetSittingRequested(true);
        p.Render(pet, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(1050));
        p.SetSittingRequested(false);
        p.Render(pet, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(325));
        var expected = new byte[96*96*4]; var actual = new byte[expected.Length];
        LocomotionFrames.Sit(.5).CopyPixels(expected, 384, 0);
        ((System.Windows.Media.Imaging.BitmapSource)((Image)p.FindName("DororongImage")).Source).CopyPixels(actual, 384, 0);
        Assert.Equal(expected, actual);
    });

    [Theory]
    [InlineData(320, false)]
    [InlineData(1050, true)]
    public void Reversing_a_partial_movement_preserves_the_visible_pose(int seatedTime, bool duringRise) => EdgePerchPresentationTests.Sta(() =>
    {
        var p = new DororongPresenter();
        var pet = new PetSnapshot(PetState.Idle, new(100,100), FacingDirection.Right, 0, false, null);
        p.SetSittingRequested(true);
        p.Render(pet, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(seatedTime));
        if(duringRise)
        {
            p.SetSittingRequested(false);
            p.Render(pet, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(200));
        }
        var image = (Image)p.FindName("DororongImage");
        var before = image.Source;
        p.SetSittingRequested(duringRise);
        p.Render(pet, DirectInteractionSnapshot.None, TimeSpan.Zero);
        Assert.Same(before, image.Source);
    });
}

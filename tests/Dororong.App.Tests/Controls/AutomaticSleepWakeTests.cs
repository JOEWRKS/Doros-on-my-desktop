using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public class AutomaticSleepWakeTests
{
    [Fact]
    public void Option_wake_reuses_tuck_and_crouch_then_retires_without_repeating() => CheekProductTests.Sta(() =>
    {
        var presenter = new DororongPresenter();
        var image = (Image)presenter.FindName("DororongImage");
        var sleep = new PetSnapshot(PetState.Sleep, new(100,100), FacingDirection.Right, .2, false, null);
        presenter.Render(sleep, DirectInteractionSnapshot.None);
        presenter.PrepareAutomaticSleepWake();
        var idle = sleep with { State = PetState.Idle, Phase = 0 };
        presenter.Render(idle, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(90));
        Assert.EndsWith("dororong-sleep-tuck-squint.png", Assert.IsType<BitmapImage>(image.Source).UriSource.ToString());
        presenter.Render(idle, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(90));
        Assert.EndsWith("dororong-sleep-crouch-squint.png", Assert.IsType<BitmapImage>(image.Source).UriSource.ToString());
        presenter.Render(idle, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(16));
        Assert.False(image.Source is BitmapImage frame && frame.UriSource.ToString().Contains("sleep-"));
        presenter.Render(idle with { Phase = .01 }, DirectInteractionSnapshot.None);
        Assert.False(image.Source is BitmapImage repeat && repeat.UriSource.ToString().Contains("sleep-"));
    });
}

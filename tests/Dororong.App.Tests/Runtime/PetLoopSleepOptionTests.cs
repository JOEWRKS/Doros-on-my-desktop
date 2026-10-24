using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Fact]
    public void Sleep_choice_applies_before_first_tick_and_wakes_existing_sleep()
    {
        using var h = new Harness(platformEnabled: false);
        h.Loop.SetAutomaticSleepEnabled(false);
        h.Start();
        for (var i = 0; i < 2400; i++) { h.Tick(80); Assert.NotEqual(PetState.Sleep, h.Core.State); }
        h.Loop.SetAutomaticSleepEnabled(true);
        h.Tick(80);
        Assert.NotEqual(PetState.Sleep, h.Core.State);
        for (var i = 0; i < 2400; i++) h.Tick(80);
        Assert.Equal(PetState.Sleep, h.Core.State);
        var position = h.Position;
        h.Loop.SetAutomaticSleepEnabled(false);
        h.Tick();
        Assert.Equal(PetState.Idle, h.Core.State);
        Assert.Equal(position, h.Position);
    }

    [Fact]
    public void Changing_sleep_choice_does_not_cancel_snack_or_sitting_request() => Controls.CheekProductTests.Sta(() =>
    {
        using var h = new Harness(realPresenter: true, snackPresentation: true);
        h.Start();
        h.Loop.SetAutomaticSleepEnabled(false);
        h.Loop.NotifySitRequested();
        for (var i = 0; i < 100; i++) h.Tick();
        var seated = h.Presenter!.MeasurePlatformContact();
        h.Loop.SetAutomaticSleepEnabled(true);
        h.Tick();
        Assert.Equal(seated, h.Presenter.MeasurePlatformContact());
        h.Pointer = new(true, new(700, 150));
        h.Loop.ToggleSnack();
        Assert.True(h.Loop.IsSnackActive);
        h.Loop.SetAutomaticSleepEnabled(false);
        h.Tick();
        Assert.True(h.Loop.IsSnackActive);
        Assert.Null(h.Fault);
    });
}

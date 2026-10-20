using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Runtime;

public partial class PetLoopPlatformTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Live_size_preserves_world_sole_and_pose(bool seated) => Controls.CheekProductTests.Sta(() =>
    {
        using var h = new Harness(realPresenter: true, highResolutionScale: 1.5, snackPresentation: true);
        h.Start();
        for (var i = 0; i < 160; i++) h.Tick();
        if (seated) { h.Loop.NotifySitRequested(); for (var i = 0; i < 100; i++) h.Tick(); }
        foreach (var percent in new[] { 100, 110, 125, 140, 150, 100, 150 })
        {
            Assert.True(h.Loop.CanResize);
            var before = h.Presenter!.MeasurePlatformContact()!.Value;
            var anchor = h.Position + new PointD((before.Left + before.Right) / 2, before.SoleY);
            var state = h.Core.State;
            Assert.True(h.Loop.TryResize(() => h.Presenter.SetArtSize(percent)));
            var after = h.Presenter.MeasurePlatformContact()!.Value;
            var actual = h.Position + new PointD((after.Left + after.Right) / 2, after.SoleY);
            Assert.InRange(Math.Abs(actual.X - anchor.X), 0, .01);
            Assert.InRange(Math.Abs(actual.Y - anchor.Y), 0, .01);
            Assert.Equal(state, h.Core.State);
            Assert.Equal(144 * percent / 100d, h.Presenter.Width, 6);
            h.Tick();
            var tickSole = h.Position.Y + h.Presenter.MeasurePlatformContact()!.Value.SoleY;
            Assert.InRange(Math.Abs(tickSole - anchor.Y), 0, 1);
        }
    });

    [Fact]
    public void Size_is_rejected_during_fall_press_and_snack()
    {
        using var h = new Harness();
        h.Native.Scene = Scene(windows: false);
        h.Start(); h.Tick(); Assert.False(h.Loop.CanResize);
        for (var i = 0; i < 150; i++) h.Tick();
        Assert.True(h.Loop.CanResize);
        h.Press(0); Assert.False(h.Loop.TryResize(() => throw new Exception("must not run")));
        h.Tick(); Assert.False(h.Loop.CanResize);
        h.Down = false; for (var i = 0; i < 200; i++) h.Tick();
        h.Loop.ToggleSnack(); Assert.False(h.Loop.CanResize);
    }
}

using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Tests.TestSupport;

namespace Dororong.Core.Tests.Behavior;

public sealed class PetBrainSleepTests
{
    [Fact]
    public void Disabled_sleep_does_not_block_time_progression_and_reenable_starts_fresh_delay()
    {
        var brain = new PetBrain(BehaviorTuning.Default with
        {
            MaxDelta = TimeSpan.FromSeconds(1), IdleMin = TimeSpan.FromSeconds(1),
            IdleMax = TimeSpan.FromSeconds(1), IdleToWalkProbability = 0,
            SleepDelay = TimeSpan.FromSeconds(2)
        }, new SeededRandomSource(), new(100, 100));
        Assert.True(brain.AutomaticSleepEnabled);
        brain.SetAutomaticSleepEnabled(false);
        for (var i = 0; i < 600; i++)
            Assert.Equal(PetState.Idle, brain.Update(PetTestInput.At(1)).State);
        brain.SetAutomaticSleepEnabled(true);
        Assert.Equal(PetState.Idle, brain.Update(PetTestInput.At(1)).State);
        Assert.Equal(PetState.Sleep, brain.Update(PetTestInput.At(1)).State);
    }

    [Theory]
    [InlineData(0)] [InlineData(20)]
    public void Disabling_during_entry_or_settled_sleep_wakes_without_moving(int ticks)
    {
        var brain = PetTestInput.CreateSleepingBrain();
        for (var i = 0; i < ticks; i++) brain.Update(PetTestInput.At(.1));
        var before = brain.Current;
        brain.SetAutomaticSleepEnabled(false);
        Assert.Equal(PetState.Idle, brain.Current.State);
        Assert.Equal(before.Position, brain.Current.Position);
        Assert.Equal(before.Facing, brain.Current.Facing);
        for (var i = 0; i < 20; i++) Assert.NotEqual(PetState.Sleep, brain.Update(PetTestInput.At(.1)).State);
    }

    [Fact]
    public void Changing_option_preserves_active_body_press_and_walk()
    {
        var brain = PetTestInput.CreateBrainAt(new(100, 100));
        brain.Update(PetTestInput.At(1));
        Assert.Equal(PetState.Walk, brain.Current.State);
        var before = brain.Current;
        brain.SetAutomaticSleepEnabled(false);
        Assert.Equal(before, brain.Current);
        brain.Update(PetTestInput.At(.1, pointer: new(160,150), primaryDown:true, bodyPressPosition:new(160,150)));
        before = brain.Current;
        Assert.True(before.IsDirectInteractionPending);
        brain.SetAutomaticSleepEnabled(true);
        Assert.Equal(before, brain.Current);
    }

    [Fact]
    public void Sleep_timing_matches_whole_and_split_frames_when_the_threshold_precedes_the_idle_boundary()
    {
        var wholeFrame = CreateSleepTimingBrain(
            idleDuration: TimeSpan.FromSeconds(0.75),
            sleepDelay: TimeSpan.FromSeconds(0.5));
        var splitFrames = CreateSleepTimingBrain(
            idleDuration: TimeSpan.FromSeconds(0.75),
            sleepDelay: TimeSpan.FromSeconds(0.5));

        var whole = wholeFrame.Update(PetTestInput.At(1.0));
        splitFrames.Update(PetTestInput.At(0.5));
        var split = splitFrames.Update(PetTestInput.At(0.5));

        Assert.Equal(PetState.Sleep, whole.State);
        Assert.Equal(whole, split);
    }

    [Fact]
    public void Sleep_wins_when_the_inactivity_and_idle_boundaries_are_equal()
    {
        var brain = CreateSleepTimingBrain(
            idleDuration: TimeSpan.FromSeconds(0.5),
            sleepDelay: TimeSpan.FromSeconds(0.5));

        var actual = brain.Update(PetTestInput.At(0.5));

        Assert.Equal(PetState.Sleep, actual.State);
    }

    [Fact]
    public void Pending_direct_interaction_defers_sleep_until_the_interaction_resolves()
    {
        var brain = CreateSleepTimingBrain(
            idleDuration: TimeSpan.FromSeconds(2),
            sleepDelay: TimeSpan.FromSeconds(0.2));
        brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(160, 150), primaryDown: true,
            bodyPressPosition: new PointD(160, 150)));

        brain.Update(PetTestInput.At(0.1, pointer: new PointD(160, 150), primaryDown: true));
        var actual = brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(160, 150), primaryDown: true));

        Assert.Equal(PetState.Idle, actual.State);
        Assert.True(actual.IsDirectInteractionPending);
    }

    [Fact]
    public void Inactivity_enters_sleep_only_after_the_sleep_delay()
    {
        var brain = PetTestInput.CreateSleepBrain();

        for (var index = 0; index < 9; index++)
        {
            brain.Update(PetTestInput.At(0.1));
        }

        Assert.Equal(PetState.Idle, brain.Current.State);

        brain.Update(PetTestInput.At(0.1));

        Assert.Equal(PetState.Sleep, brain.Current.State);
    }

    [Fact]
    public void Sleep_phase_advances_one_quarter_after_one_second()
    {
        var brain = PetTestInput.CreateSleepingBrain();

        for (var tick = 0; tick < 10; tick++)
        {
            brain.Update(PetTestInput.At(0.1));
        }

        Assert.Equal(PetState.Sleep, brain.Current.State);
        Assert.Equal(0.25, brain.Current.Phase, precision: 10);
    }

    [Fact]
    public void Click_without_drag_wakes_sleep_into_click_reaction()
    {
        var brain = PetTestInput.CreateSleepingBrain();
        Assert.Equal(PetState.Sleep, brain.Current.State);

        brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(160, 150), primaryDown: true,
            bodyPressPosition: new PointD(160, 150)));

        var actual = brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(160, 150), primaryDown: false));

        Assert.Equal(PetState.ClickReaction, actual.State);
    }

    [Theory]
    [InlineData(false, PetState.Curious)]
    [InlineData(true, PetState.Startled)]
    public void New_pointer_approach_wakes_sleep_into_the_matching_reaction(
        bool fast,
        PetState expected)
    {
        var brain = PetTestInput.CreateSleepingBrain();
        Assert.Equal(PetState.Sleep, brain.Current.State);

        brain.Update(PetTestInput.At(0.1, pointer: new PointD(460, 150)));

        var actual = brain.Update(PetTestInput.At(
            fast ? 0.1 : 1.0,
            pointer: fast ? new PointD(260, 150) : new PointD(300, 150)));

        Assert.Equal(expected, actual.State);
    }

    [Fact]
    public void Crossing_the_drag_threshold_wakes_sleep_into_dragged()
    {
        var brain = PetTestInput.CreateSleepingBrain();
        Assert.Equal(PetState.Sleep, brain.Current.State);

        brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(160, 150), primaryDown: true,
            bodyPressPosition: new PointD(160, 150)));

        var actual = brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(165, 150), primaryDown: true));

        Assert.Equal(PetState.Dragged, actual.State);
        Assert.Equal(new PointD(60, 50), actual.GrabOffset);
    }

    [Fact]
    public void Drag_release_does_not_create_a_new_near_entry_on_the_next_stationary_tick()
    {
        var brain = PetTestInput.CreateSleepingBrain();
        Assert.Equal(PetState.Sleep, brain.Current.State);
        brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(160, 150), primaryDown: true,
            bodyPressPosition: new PointD(160, 150)));
        brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(164, 150), primaryDown: true));
        var released = brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(164, 150), primaryDown: false));

        var stationary = brain.Update(PetTestInput.At(
            0.1, pointer: new PointD(164, 150), primaryDown: false));

        Assert.Equal(PetState.Idle, released.State);
        Assert.Equal(PetState.Idle, stationary.State);
    }

    [Theory]
    [InlineData(WakeRoute.SlowApproach)]
    [InlineData(WakeRoute.FastApproach)]
    [InlineData(WakeRoute.Click)]
    [InlineData(WakeRoute.Drag)]
    public void Every_wake_route_resets_stale_inactivity(WakeRoute route)
    {
        var brain = PetTestInput.CreateSleepingBrain(TimeSpan.FromSeconds(5));
        Assert.Equal(PetState.Sleep, brain.Current.State);

        WakeAndFinishReaction(brain, route);

        Assert.Equal(PetState.Idle, brain.Current.State);
    }

    [Fact]
    public void Reentering_the_near_zone_during_cooldown_resets_inactivity()
    {
        var brain = PetTestInput.CreateSleepBrain();
        brain.Update(PetTestInput.At(0.1, pointer: new PointD(460, 150)));
        brain.Update(PetTestInput.At(0.1, pointer: new PointD(300, 150)));

        for (var index = 0; index < 11; index++)
        {
            brain.Update(PetTestInput.At(0.1, pointer: new PointD(460, 150)));
        }

        brain.Update(PetTestInput.At(0.1, pointer: new PointD(300, 150)));
        for (var index = 0; index < 3; index++)
        {
            brain.Update(PetTestInput.At(0.1, pointer: new PointD(300, 150)));
        }

        Assert.Equal(PetState.Idle, brain.Current.State);
    }

    [Fact]
    public void A_stationary_near_cursor_does_not_repeatedly_reset_inactivity()
    {
        var brain = PetTestInput.CreateSleepBrain();
        brain.Update(PetTestInput.At(0.1, pointer: new PointD(460, 150)));
        brain.Update(PetTestInput.At(0.1, pointer: new PointD(300, 150)));

        for (var index = 0; index < 16; index++)
        {
            brain.Update(PetTestInput.At(0.1, pointer: new PointD(300, 150)));
        }

        Assert.Equal(PetState.Sleep, brain.Current.State);
    }

    private static void WakeAndFinishReaction(PetBrain brain, WakeRoute route)
    {
        switch (route)
        {
            case WakeRoute.SlowApproach:
                brain.Update(PetTestInput.At(0.1, pointer: new PointD(460, 150)));
                brain.Update(PetTestInput.At(1.0, pointer: new PointD(300, 150)));
                UpdateRepeatedly(brain, 15, pointer: new PointD(300, 150));
                break;
            case WakeRoute.FastApproach:
                brain.Update(PetTestInput.At(0.1, pointer: new PointD(460, 150)));
                brain.Update(PetTestInput.At(0.1, pointer: new PointD(260, 150)));
                UpdateRepeatedly(brain, 7, pointer: new PointD(260, 150));
                break;
            case WakeRoute.Click:
                brain.Update(PetTestInput.At(
                    0.1, pointer: new PointD(160, 150), primaryDown: true,
                    bodyPressPosition: new PointD(160, 150)));
                brain.Update(PetTestInput.At(
                    0.1, pointer: new PointD(160, 150), primaryDown: false));
                UpdateRepeatedly(brain, 4, pointer: new PointD(160, 150));
                break;
            case WakeRoute.Drag:
                brain.Update(PetTestInput.At(
                    0.1, pointer: new PointD(160, 150), primaryDown: true,
                    bodyPressPosition: new PointD(160, 150)));
                brain.Update(PetTestInput.At(
                    0.1, pointer: new PointD(165, 150), primaryDown: true));
                brain.Update(PetTestInput.At(
                    0.1, pointer: new PointD(165, 150), primaryDown: false));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(route));
        }
    }

    private static void UpdateRepeatedly(PetBrain brain, int count, PointD pointer)
    {
        for (var index = 0; index < count; index++)
        {
            brain.Update(PetTestInput.At(0.1, pointer: pointer));
        }
    }

    private static PetBrain CreateSleepTimingBrain(TimeSpan idleDuration, TimeSpan sleepDelay) =>
        new(
            BehaviorTuning.Default with
            {
                MaxDelta = TimeSpan.FromSeconds(1),
                IdleMin = idleDuration,
                IdleMax = idleDuration,
                WalkMin = TimeSpan.FromSeconds(2),
                WalkMax = TimeSpan.FromSeconds(2),
                IdleToWalkProbability = 1,
                SleepDelay = sleepDelay
            },
            new SequenceRandomSource(0, 0),
            new PointD(100, 100));

    public enum WakeRoute
    {
        SlowApproach,
        FastApproach,
        Click,
        Drag
    }
}

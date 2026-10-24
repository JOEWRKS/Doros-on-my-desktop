namespace Dororong.Core.Behavior;

public sealed partial class PetBrain
{
    private IdleActivityKind _idleActivity, _pendingIdleActivity;
    private TimeSpan _groomRemaining = TimeSpan.FromSeconds(45);
    private int _completedMeals;

    // Called only after the existing eat/celebration/settle sequence completes.
    // Alternating meals gives an occasional reaction without consuming the RNG
    // that controls the accepted walk directions/durations.
    public void NotifyMealCompleted()
    {
        if (!_tuning.IdleActivitiesEnabled) return;
        _completedMeals++;
        if ((_completedMeals & 1) == 1) _pendingIdleActivity = IdleActivityKind.Beg;
    }

    private void QueueWakeStretch()
    {
        if (_tuning.IdleActivitiesEnabled) _pendingIdleActivity = IdleActivityKind.Stretch;
    }

    private void CancelIdleActivity(bool clearPending = true)
    {
        if (_idleActivity != IdleActivityKind.None) { _idleActivity = IdleActivityKind.None; StartIdle(); }
        if (clearPending) _pendingIdleActivity = IdleActivityKind.None;
    }

    private bool TryStartIdleActivity()
    {
        if (!_tuning.IdleActivitiesEnabled || _idleActivity != IdleActivityKind.None ||
            _state != PetState.Idle || _pressPosition.HasValue || _askingDoro || _doroWander) return false;
        var kind = _pendingIdleActivity;
        if (kind == IdleActivityKind.None && _groomRemaining <= TimeSpan.Zero) kind = IdleActivityKind.Groom;
        if (kind == IdleActivityKind.None) return false;
        _pendingIdleActivity = IdleActivityKind.None;
        _idleActivity = kind;
        _stateElapsed = TimeSpan.Zero;
        _stateDuration = TimeSpan.FromSeconds(new IdleActivity(kind, 0).Duration);
        if (kind == IdleActivityKind.Groom) _groomRemaining = TimeSpan.FromSeconds(90);
        return true;
    }
}

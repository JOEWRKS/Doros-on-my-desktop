namespace Dororong.Core.Behavior;

public sealed partial class PetBrain
{
    private static readonly TimeSpan DoroQuestionDuration=TimeSpan.FromSeconds(3);
    private bool _doroWander,_askingDoro;
    private TimeSpan _nextDoroAt;
    public void NotifyActivity()=>ResetInactivity();

    private bool CanStartDoroWander()=>!_doroWander && !_askingDoro && _tuning.DoroQuestionDelay is not null &&
        !(AutomaticSleepEnabled && _inactivity >= _tuning.SleepDelay) &&
        !_pressPosition.HasValue && _state is PetState.Idle or PetState.Walk;

    private bool TryStartDoroWander()
    {
        if(!CanStartDoroWander() || _inactivity < _nextDoroAt)return false;
        // Interval starts at the wander trigger, not after the walk/question ends.
        // Never replay a backlog when another action has deferred the trigger.
        _nextDoroAt=_inactivity+_tuning.DoroQuestionDelay!.Value;
        CancelIdleActivity();
        _doroWander=true;
        // Reuse the existing authored gait, speed, walk duration and surface physics.
        StartWalk();return true;
    }
}

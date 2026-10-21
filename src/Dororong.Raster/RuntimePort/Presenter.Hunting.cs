// Mac-only extraction of accepted r104 (3813312). Windows source is unchanged.
using System.Windows;
using System.Windows.Media;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
namespace Dororong.App.Controls;
internal partial class DororongPresenter {
    private bool UpdateHuntingCore(PetSnapshot snapshot, DirectInteractionSnapshot direct,
        PointerSample pointer, TimeSpan elapsed, bool pressed, bool pounceEnabled)
    {
        // Physical button polling can precede routed MouseDown. Preserve the
        // displayed preparation pose until that event classifies/captures it.
        // Do not turn a still-unclassified cheek press into an upright reset.
        // No timer accumulates while waiting; release without an event resumes.
        _huntAwaitingPress = HighResolutionGrabEnabled && pressed && !_snackPose.Active &&
            !_sittingRequested && !_locomotionBlocked && !(_hrPerchPhase==EdgePerchPhase.Attached) &&
            _hrPerchPhase==Dororong.Core.Platforms.EdgePerchPhase.None &&
            snapshot.State is PetState.Idle or PetState.Walk &&
            direct.Target == DirectInteractionTarget.None && direct.HeadLanding is null &&
            _huntSession.IsActive && !Pounce.IsJump && _hrFace.BodyFrame >= 0;
        if (_huntAwaitingPress) { _huntDelta = 0; return true; }
        // The captured crouch owns the torso throughout cheek pull/recoil.
        // Pause its clock rather than resetting it and starting another lower
        // animation while the presenter is recovering the captured face.
        if (HighResolutionGrabEnabled && direct.CheekPull?.Capture.HighResolution is { Face.BodyFrame: >= 0 } &&
            _huntSession.IsActive && !_snackPose.Active && !_sittingRequested && !_locomotionBlocked)
        {
            _huntDelta=0;
            _pounce.Reset();
            return true;
        }
        // Sitting owns the authored torso, not the gaze. Keep preparation and
        // pouncing disabled through the stand-up blend while allowing attention.
        var postureBlocked = _sittingRequested || (HighResolutionGrabEnabled && _locomotion.Sit > 0);
        var blocked = _snackPose.Active || pressed || (_sittingRequested && !HighResolutionGrabEnabled) || _locomotionBlocked || (_hrPerchPhase==EdgePerchPhase.Attached) || _hrPerchPhase!=Dororong.Core.Platforms.EdgePerchPhase.None ||
            snapshot.State is not (PetState.Idle or PetState.Walk or PetState.Sleep) || snapshot.IsDirectInteractionPending ||
            direct.Target != DirectInteractionTarget.None || direct.HeadLanding is not null;
        _huntDelta = Math.Clamp(elapsed.TotalSeconds, 0, .1);
        _huntPointer = pointer.IsAvailable && double.IsFinite(pointer.Position.X) && double.IsFinite(pointer.Position.Y)
            ? pointer.Position - snapshot.Position : null;
        if(HighResolutionGrabEnabled&&_huntPointer is { } scaledPointer)
            _huntPointer=new(scaledPointer.X/HighResolutionScale,scaledPointer.Y/HighResolutionScale);
        // Authored 96px image begins at presenter (24,24). Use its stable
        // registration, not animated alpha bounds, for proximity and gaze.
        var radiusScale = _huntNear ? 1.2 : 1;
        _huntNear = !blocked && !postureBlocked && _huntPointer is { } p &&
            Math.Pow((p.X-72)/(116.25*radiusScale),2)+Math.Pow((p.Y-81)/(75*radiusScale),2)<1;
        // The outer ellipse only watches upright; it never builds launch dwell.
        _huntTracking = !blocked && _huntPointer is { } watch &&
            Math.Pow((watch.X-72)/310,2)+Math.Pow((watch.Y-81)/200,2)<1;
        var oldPounce = Pounce;
        if (pounceEnabled)
        {
            _pounce.Advance(_huntDelta, _huntNear, _huntPointer?.X - 72 ?? double.NaN, snapshot.Facing, blocked || postureBlocked);
            if (!oldPounce.IsJump && Pounce.IsJump) _pounceLaunchAge = Math.Clamp(_huntSession.FrameAgeSeconds, .7, 1.65);
        }
        // Departure cancels preparation on this tick, not at the next sway peak.
        // In-flight/landing and the post-landing cooldown remain owned by Pounce.
        if (!_huntNear && _huntSession.IsActive && !blocked && !Pounce.IsEngaged)
            _huntRecoveryAge = SnackRenderer.RecoveryAge(HuntPose.At(_huntVisualFrame/60d).Amount);
        if(blocked || postureBlocked || _huntNear || Pounce.IsEngaged) _huntRecoveryAge=2.6;
        else _huntRecoveryAge=Math.Min(2.6,_huntRecoveryAge+_huntDelta*.55/.24);
        if (Pounce.IsEngaged || !_huntNear) _huntSession.Reset();
        else _huntSession.Advance(_huntDelta,_huntNear,blocked);
        if (blocked) {_huntGaze.Reset();_huntBlendProgress=0;}
        else if (!_huntTracking && !(Pounce.Phase == PouncePhase.Track && _huntPointer is not null))
        {
            // Visual recovery must continue after behavior/dwell is cancelled.
            // Advance here even without a render so stale gaze cannot leak into
            // the next entry; ApplyHunting consumes this same pose with zero dt.
            _huntGaze.Advance(_huntDelta,null,false,false);
            _huntDelta=0;
        }
        if (!blocked)
        {
            if (_huntTracking || _huntSession.IsActive || Pounce.IsEngaged)
                _huntBlendProgress=Math.Min(1,_huntBlendProgress+Math.Clamp(elapsed.TotalSeconds,0,.1)/.12);
            else if (_huntGaze.IsAtRest)
                _huntBlendProgress=Math.Max(0,_huntBlendProgress-Math.Clamp(elapsed.TotalSeconds,0,.1)/.16);
        }
        return _huntTracking || _huntSession.IsActive || Pounce.IsEngaged;
    }
}

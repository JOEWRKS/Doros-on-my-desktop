using System.Windows;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Controls;

public partial class DororongPresenter
{
    private readonly SeatedInteraction _seatedInteraction=new();
    private void AdvanceSeatedInteraction(PetSnapshot pet,DirectInteractionSnapshot direct,TimeSpan elapsed)
    {
        Point? pointer=null;
        if(_huntPointer is { } p){var x=p.X-24;if(_activeFacing==FacingDirection.Left)x=96-x;pointer=new(x,p.Y-24);}
        _seatedInteraction.Advance(elapsed.TotalSeconds,
            _sittingRequested&&_locomotion.Sit>=.999&&!_locomotionBlocked&&!_edgePerch.IsAttached&&
            !_snackPose.Active&&!pet.IsDirectInteractionPending&&direct.Target==DirectInteractionTarget.None&&
            direct.HeadLanding is null&&pet.State is PetState.Idle or PetState.Walk or PetState.ClickReaction,
            pet.State==PetState.ClickReaction,pointer);
    }
    private void ApplySeatedInteraction(PetSnapshot pet)
    {
        if(!_sittingRequested||_locomotionBlocked||_edgePerch.IsAttached||_snackPose.Active)return;
        // A click in the sit transition stays on the current seated pose too.
        if(pet.State==PetState.ClickReaction)HighResolutionMotion(HighResolutionLocomotion.Sit(_locomotion.Sit));
        _hrFace=SeatedInteraction.Pose(_hrFace,_seatedInteraction.Kind,_seatedInteraction.Age,_seatedInteraction.AimAngle);
    }
}

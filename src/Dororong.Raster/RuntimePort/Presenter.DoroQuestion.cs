using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Platforms;
namespace Dororong.App.Controls;
internal partial class DororongPresenter {
 private bool _showDoroQuestion;
 internal (bool Visible,bool TailRight,double Opacity) Question {get;private set;}
 private bool CanShowDoroQuestion(PetSnapshot pet,DirectInteractionSnapshot direct)=>
  pet.DoroQuestionPhase is >=0 and <=1 && pet.State==PetState.Idle && !pet.IsDirectInteractionPending &&
  direct.Target==DirectInteractionTarget.None && direct.HeadLanding is null && !_snackPose.Active &&
  !_sittingRequested && !_locomotionBlocked && !_huntTracking && !_huntAwaitingPress && _hrPerchPhase==EdgePerchPhase.None;
 private void UpdateDoroBubble(PetSnapshot pet){
  if(!_showDoroQuestion){Question=default;return;}
  var head=HighResolutionFaceToPresenter(new(29,45));
  Question=(true,head.X>72*HighResolutionScale,DoroQuestionMotion.BubbleOpacity(pet.DoroQuestionPhase!.Value));
 }
}

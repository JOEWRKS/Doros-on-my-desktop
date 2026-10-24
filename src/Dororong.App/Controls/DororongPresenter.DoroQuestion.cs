using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Platforms;

namespace Dororong.App.Controls;

public partial class DororongPresenter
{
    private DoroQuestionBubble? _doroBubble;
    private readonly ScaleTransform _doroBubbleScale=new();
    private bool _showDoroQuestion;
    internal FrameworkElement? DoroBubble=>_doroBubble;
    private bool CanShowDoroQuestion(PetSnapshot pet,DirectInteractionSnapshot direct)=>
        HighResolutionGrabEnabled && pet.DoroQuestionPhase is >=0 and <=1 &&
        pet.State==PetState.Idle && !pet.IsDirectInteractionPending &&
        direct.Target==DirectInteractionTarget.None && direct.HeadLanding is null &&
        !_snackPose.Active && !_sittingRequested && !_locomotionBlocked &&
        !_huntTracking && !_huntAwaitingPress && _hrPerchPhase==EdgePerchPhase.None;

    private void UpdateDoroBubble(PetSnapshot pet){
        if(!_showDoroQuestion){if(_doroBubble is not null)_doroBubble.Visibility=Visibility.Collapsed;return;}
        if(_doroBubble is null){
            _doroBubble=new(){RenderTransform=_doroBubbleScale};
            Panel.SetZIndex(_doroBubble,100);((Canvas)Content).Children.Add(_doroBubble);
        }
        var scale=HighResolutionScale;
        var head=HighResolutionFaceToPresenter(new(29,45));
        var headRight=head.X>72*scale;
        _doroBubbleScale.ScaleX=_doroBubbleScale.ScaleY=scale;
        _doroBubble.TailRight=headRight;
        Canvas.SetLeft(_doroBubble,(headRight?2:78)*scale);Canvas.SetTop(_doroBubble,12*scale);
        _doroBubble.Opacity=DoroQuestionMotion.BubbleOpacity(pet.DoroQuestionPhase!.Value);
        _doroBubble.Visibility=Visibility.Visible;
    }
}

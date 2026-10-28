// Port of accepted deb5cae presentation; Windows source remains unchanged.
using System.Windows;
using System.Windows.Media;
using Dororong.Core.Behavior;

namespace Dororong.App.Controls;

internal partial class DororongPresenter
{
    private void ApplyIdleActivity(PetSnapshot pet)
    {
        if (pet.IdleActivity.Kind==IdleActivityKind.None || _snackPose.Active || _sittingRequested ||
            _locomotionBlocked || (_hrPerchPhase==Dororong.Core.Platforms.EdgePerchPhase.Attached) || pet.IsDirectInteractionPending || pet.State!=PetState.Idle) return;
        var age=pet.IdleActivity.Age;
        static double S(double t)=>HighResolutionGrabRaster.Smooth(t);
        if(pet.IdleActivity.Kind==IdleActivityKind.Stretch){
            // Keep the accepted180ms sleep-to-upright blend before stretching.
            if(age<.2)return;
            var amount=S((age-.2)/.55)*(1-S((age-1.35)/.55));
            var shake=Math.Sin((age-1.95)*Math.PI*12)*S((age-1.95)/.12)*(1-S((age-2.5)/.3));
            var head=Matrix.Identity;head.RotateAt(-3*amount+1.5*shake,43,66);head.Translate(-3*amount,4*amount);
            _hrFace=HighResolutionFacePose.Neutral with{Head=head,Stretch=amount,StretchShake=shake,Closed=amount>.2};
        }else if(pet.IdleActivity.Kind==IdleActivityKind.Beg){
            var lift=S((age-.45)/.35)*(1-S((age-1.8)/.5));
            var gazeX=0d;var gazeY=0d;
            if(_huntPointer is { } pointer){
                var x=pointer.X-24; if(pet.Facing==FacingDirection.Left)x=96-x;
                gazeX=Math.Clamp((x-31)/55,-1,1)*.65*lift;
                gazeY=Math.Clamp((pointer.Y-24-52)/55,-1,1)*.45*lift;
            }
            _hrFace=HighResolutionFacePose.Neutral with{StompLift=2.4*lift,StompReach=.7*lift,
                EyeX=gazeX,EyeY=gazeY,MouthAge=age>=.1&&age<.7?.48+(age-.1)/.6*1.07:-1};
        }else if(pet.IdleActivity.Kind==IdleActivityKind.Groom){
            var sit=S(age/.4)*(1-S((age-3.35)/.45));
            HighResolutionMotion(sit<=1e-9?-1:HighResolutionLocomotion.Sit(sit));
            var cover=S((age-.4)/.4)*(1-S((age-2.9)/.45));
            var swipe=0d;
            // Three connected wipes, with one raise and one lower only.
            for(var i=0;i<3;i++){
                var t=(age-(.9+i*.65))/.55;
                if(t>0&&t<1)swipe=Math.Pow(Math.Sin(Math.PI*t),2);
            }
            var head=_hrFace.Head;head.RotateAt(-2*swipe,43,66);
            _hrFace=_hrFace with{Head=head,ChuckleCover=cover,GroomSweep=swipe*cover,Closed=cover>.5,GroomingPaw=true};
        }
    }
}

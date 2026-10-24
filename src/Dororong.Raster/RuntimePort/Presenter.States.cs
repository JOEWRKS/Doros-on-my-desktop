// Mac-only extraction of accepted r104 (3813312). Windows source is unchanged.
using System.Windows;
using System.Windows.Media;
using Dororong.Core.Behavior;

namespace Dororong.App.Controls;

internal partial class DororongPresenter
{
    private HighResolutionFacePose RecoverCapturedPose(HighResolutionFacePose captured,double u){
        if(u<=0)return captured;if(u>=1)return HighResolutionFacePose.Neutral;
        var frame=captured.MotionFrame;var body=captured.BodyFrame;
        if(frame is >=1 and <=256){var level=(frame-1)/32+1;var phase=(frame-1)%32;frame=HighResolutionLocomotion.Walk(level/8d*(1-u),phase*10d/32);}
        else if(frame is >=257 and <=321)frame=HighResolutionLocomotion.Sit((frame-257)/64d*(1-u));
        else if(frame is >=322 and <=705){var level=(frame-322)/48+1;var phase=(frame-322)%48;frame=HighResolutionLocomotion.Run(level/8d*(1-u),phase*66d/48);}
        else if(frame is >=706 and <=770)frame=HighResolutionLocomotion.Sleep((frame-706)/64d*(1-u));
        if(body>=0){var start=SnackRenderer.RecoveryAge(HuntPose.At(body/60d).Amount);body=(int)Math.Round((start+(2.6-start)*u)*60);}
        var a=captured.Head;var head=new Matrix(a.M11+(1-a.M11)*u,a.M12*(1-u),a.M21*(1-u),a.M22+(1-a.M22)*u,a.OffsetX*(1-u),a.OffsetY*(1-u));
        captured=captured with {ChuckleCover=captured.ChuckleCover*(1-u),ChuckleBob=captured.ChuckleBob*(1-u)};
        return captured with {Head=head,MotionFrame=frame,BodyFrame=body,EyeX=captured.EyeX*(1-u),EyeY=captured.EyeY*(1-u),Breathing=captured.Breathing*(1-u),StompLift=captured.StompLift*(1-u),StompReach=captured.StompReach*(1-u),StompCrouch=captured.StompCrouch*(1-u),EffectFrame=frame is >=706 and <=770?HighResolutionLocomotion.SleepEffect(frame):-1};
    }
    private void HighResolutionMotion(int frame,bool closed=false,int effect=-1){
        var matrix=Matrix.Identity;
        if(frame>=0){var offset=_hrRaster!.MotionOffset(frame);matrix.Translate(offset.X,offset.Y);}
        _hrFace=HighResolutionFacePose.Neutral with {Head=matrix,MotionFrame=frame,Closed=closed,EffectFrame=effect};
    }

    private void ApplyHighResolutionState(PetSnapshot pet,TimeSpan elapsed){
        var phase=Math.Clamp(pet.Phase,0,1);
        if(pet.State==PetState.Sleep){
            if(_lastRenderedState!=PetState.Sleep)_sleepEntryComplete=false;
            if(phase>=.135)_sleepEntryComplete=true;
            _hrSleepAmount=_sleepEntryComplete?1:HighResolutionGrabRaster.Smooth((phase-.045)/.09);
            var frame=HighResolutionLocomotion.Sleep(_hrSleepAmount);
            // Phase loops every four seconds; entry completion survives that
            // wrap. Only leaving Sleep may start the eye-opening wake blend.
            HighResolutionMotion(frame,_sleepEntryComplete||phase>=.0225,HighResolutionLocomotion.SleepEffect(frame));
            _hrFace=_hrFace with {Breathing=_sleepEntryComplete?GetBreathingAmount((phase-.135+1)%1):0};
        }else{
            if(_lastRenderedState==PetState.Sleep){_hrWakeFrom=_hrSleepAmount;_hrWakeAge=0;_hrSleepAmount=0;}
            if(_hrWakeAge<1){
                // Existing automatic/attention wake is 180ms; body-click wake
                // is 90ms. Use the same clock, with source-parts interpolation.
                var duration=pet.State==PetState.ClickReaction?.09:.18;
                var amount=_hrWakeFrom*(1-HighResolutionGrabRaster.Smooth(_hrWakeAge));
                _hrWakeAge=Math.Min(1,_hrWakeAge+Math.Max(0,elapsed.TotalSeconds)/duration);
                var frame=HighResolutionLocomotion.Sleep(amount);
                HighResolutionMotion(frame,_hrWakeAge<.5,HighResolutionLocomotion.SleepEffect(frame));
            }else if(pet.State==PetState.ClickReaction){
                HighResolutionMotion(-1);
                var sample=BodyClickTransformSampler.SampleConfirmedClick(phase);
                var origin=_hrForward.Transform(new Point(96*.428987,96*.916667));
                _hrForward.ScaleAt(sample.ScaleX,sample.ScaleY,origin.X,origin.Y);
                _hrForward.Translate(0,sample.TranslationY);
            }else if(pet.State==PetState.Curious){
                _hrForward.RotateAt(pet.Facing==FacingDirection.Right?7:-7,48,48);
            }else if(pet.State==PetState.Startled&&phase<.5){
                var bounce=Math.Sin(phase*Math.PI);
                _hrForward.ScaleAt(1+.18*bounce,1-.14*bounce,48,48);
            }else if(pet.State==PetState.Idle&&_hrFace.MotionFrame<0&&_hrFace.BodyFrame<0&&!_huntTracking){
                _hrFace=_hrFace with {Breathing=GetBreathingAmount(phase)};
            }
        }
        if(!_snackPose.Active)return;
        var pose=_snackPose;_hrFacing=pet.Facing;_hrForward=GrabTransform(0,0,HighResolutionScale);
        if(pose.Phase==SnackPhase.Angry){
            _hrFace=HighResolutionGrabRaster.SnackReaction(pose.Age,pose.WithdrawalStreak);
        }else if(pose.Phase==SnackPhase.Chuckling){
            _hrFace=HighResolutionGrabRaster.SnackChuckle(pose.Age);
        }else if(pose.Phase==SnackPhase.Happy){
            var head=HuntRenderer.HeadTransform(156,new(0,0,SnackRenderer.HappyRoll(pose.Age)));
            _hrFace=HighResolutionFacePose.Neutral with {Head=head,Closed=true};
        }else{
            var frame=pose.JumpAge>=0?SnackRenderer.JumpFrame(pose.JumpAge):156;
            if(pose.Phase==SnackPhase.Chasing&&pose.Age<.24&&_snackRecoveryAge<2.59)
                frame=(int)Math.Round(Math.Min(2.6,_snackRecoveryAge+pose.Age*.55/.24)*60);
            if(frame<156)_hrFace=HighResolutionFacePose.Neutral with {Head=HuntRenderer.HeadTransform(frame,default),BodyFrame=frame};
            else if(pose.Phase==SnackPhase.Chasing&&pose.RunAmount>0&&pose.JumpAge<0)
                HighResolutionMotion(HighResolutionLocomotion.Run(pose.RunAmount,pose.Distance));
            else HighResolutionMotion(-1,pose.Phase==SnackPhase.Eating||pose.Phase==SnackPhase.Settling&&pose.Age<.16);
            if(pose.Phase==SnackPhase.Eating)_hrFace=_hrFace with {MouthAge=Math.Round(pose.Age*60)/60};
            else if(pose.Phase==SnackPhase.Snatching&&pose.Age>=SnackSession.SnatchPrepareSeconds)
                _hrFace=_hrFace with {MouthAge=.35};
            else if(pose.Phase==SnackPhase.Chasing)_hrFace=_hrFace with {DroolAge=Math.Round(pose.Age*60)/60};
        }
        _snackHead=_hrFace.Head;
    }
}

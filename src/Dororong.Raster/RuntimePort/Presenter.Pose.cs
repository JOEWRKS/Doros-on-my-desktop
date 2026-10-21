// Mac-only extraction of accepted r104 (3813312). Windows source is unchanged.
using System.Windows;
using System.Windows.Media;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
namespace Dororong.App.Controls;
internal partial class DororongPresenter {
    private bool RenderHighResolutionGrab(PetSnapshot pet,DirectInteractionSnapshot direct,TimeSpan elapsed)
    {
        var head=direct.Target==DirectInteractionTarget.Body;
        var cheek=direct.CheekPull?.Capture.HighResolution is not null;
        var idle=direct.Target is DirectInteractionTarget.None or DirectInteractionTarget.ClickOnly||direct.PawPull is not null;
        var scale=HighResolutionScale;
        if(!double.IsFinite(scale)||scale<1||scale>1.5)throw new InvalidOperationException("High resolution scale must be 1–1.5.");
        // A disabled body drag retains its actual displayed pose until release.
        if(direct.Target==DirectInteractionTarget.ClickOnly&&_hasFrame)return true;
        if(!_sizeChanged&&_huntAwaitingPress&&!_snackPose.Active&&!_sittingRequested&&direct.Target==DirectInteractionTarget.None&&_hasFrame)return true;
        _sizeChanged=false;
        _hrPreviousCenterY=head&&direct.Phase==DirectInteractionPhase.BodyDragSettle&&MeasurePlatformGeometry() is {} previous
            ?previous.Bounds.Y+previous.Bounds.Height/2:null;
        _releaseOffset=0;
        _hrDirect=head;_directOwnsPresentation=head||cheek;
        _hrPerchPaw=direct.PawPull;
        if(head){
            _hrPerchPhase=EdgePerchPhase.None;
            _hrFace=HighResolutionFacePose.Neutral;_hrCheekCapture=null;_hrRecoil=default;_hrReturnAge=1;
            if(_hrCapturePending||_hrPress!=direct.PressOrigin){
                if(!_hrCapturePending){_hrEntryProgress=0;_hrCaptureForward=GrabTransform(0,1,scale);}
                _hrCapturePending=false;_hrPress=direct.PressOrigin;_hrLag.Reset();_hrSettling=false;_hrAngle=0;_hrProgress=_hrEntryProgress;
                if(!_hrFacingCaptured){_hrFacing=direct.PressFacing??_activeFacing;_hrPin=new((24+_hrGrab.X)*scale,(24+_hrGrab.Y)*scale);}}
            var settling=direct.Phase==DirectInteractionPhase.BodyDragSettle;
            if(settling&&!_hrSettling){_hrReleaseStart=_hrProgress;_hrReleaseAngle=_hrAngle;}
            _hrSettling=settling;
            _hrProgress=settling?Math.Max(0,(_hrReleaseStart*7-direct.ReleaseProgress*(_hrReleaseStart*7+1))/7):
                direct.Phase==DirectInteractionPhase.BodyDragHold||direct.StartsHanging?1:_hrEntryProgress+(1-_hrEntryProgress)*Math.Clamp(direct.Strength,0,1);
            var remaining=settling?1-HighResolutionGrabRaster.Smooth(direct.ReleaseProgress):1;
            var angle=settling?_hrReleaseAngle*remaining:Math.Clamp(double.IsFinite(direct.HeadSwingDegrees)?direct.HeadSwingDegrees:0,-88,88);
            _hrForward=GrabTransform(angle,remaining,scale);
            // Symmetric cap against actual host viewport; never shrink the artwork.
            if(!HighResolutionFits(Math.Abs(angle),remaining,scale)||!HighResolutionFits(-Math.Abs(angle),remaining,scale)){
                var lo=0d;var hi=1d;for(var i=0;i<12;i++){var f=(lo+hi)/2;if(HighResolutionFits(angle*f,remaining,scale)&&HighResolutionFits(-angle*f,remaining,scale))lo=f;else hi=f;}angle*=lo;}
            _hrAngle=angle;_hrForward=GrabTransform(angle,remaining,scale);
            if(!settling&&_hrEntryProgress==0&&!direct.StartsHanging){
                var hasEntryPose=_hrEntryFace.MotionFrame>=0||_hrEntryFace.BodyFrame>=0||_hrEntryFace.Head!=Matrix.Identity;
                if(direct.Phase==DirectInteractionPhase.BodyPending){
                    _hrFace=_hrEntryFace;_hrForward=_hrCaptureForward;_hrProgress=0;
                }else if(hasEntryPose){
                    const double normalization=.35;
                    var u=HighResolutionGrabRaster.Smooth(direct.Strength/normalization);
                    _hrFace=RecoverCapturedPose(_hrEntryFace,u);
                    _hrProgress=Math.Clamp((direct.Strength-normalization)/(1-normalization),0,1);
                    // The displayed head transform is part of the press anchor.
                    // Counter its shrinking offset while the body normalizes.
                    var at=_hrFace.Head.Transform(new Point(_hrGrab.X,_hrGrab.Y));
                    var pin=_hrForward.Transform(at);
                    _hrForward.Translate(_hrPin.X/scale-24-pin.X,_hrPin.Y/scale-24-pin.Y);
                }
            }
            if(_hrBlendCapture&&!settling&&direct.Phase!=DirectInteractionPhase.BodyDragHold&&!direct.StartsHanging){
                // A regrab starts at the displayed (possibly tilted/squashed)
                // pose. Both matrices pin the same grabbed point, so blending
                // the affine coefficients cannot make that point jump.
                var t=HighResolutionGrabRaster.Smooth(direct.Strength);var a=_hrCaptureForward;var b=_hrForward;
                _hrForward=new(a.M11+(b.M11-a.M11)*t,a.M12+(b.M12-a.M12)*t,a.M21+(b.M21-a.M21)*t,a.M22+(b.M22-a.M22)*t,a.OffsetX+(b.OffsetX-a.OffsetX)*t,a.OffsetY+(b.OffsetY-a.OffsetY)*t);
            }
            _hrLag.Advance(direct,elapsed.TotalMilliseconds);
            (_hrNear,_hrFar)=_hrLag.LocalOffsets(_hrFacing,angle);
        }else if(cheek){
            var snapshot=direct.CheekPull!;var capture=snapshot.Capture.HighResolution!;
            if(!ReferenceEquals(_hrCheekCapture,snapshot.Capture)){_hrFaceFollow.Reset();_hrCheekReleasing=false;_hrCheekCapture=snapshot.Capture;}
            var release=direct.Phase==DirectInteractionPhase.CheekRelease&&!_hrCheekReleasing;
            if(release){_hrFaceFollow.BeginRelease(snapshot.SpringRelease);_hrCheekReleasing=true;}
            _hrFaceFollow.Step(snapshot.PullDips,release?0:elapsed.TotalMilliseconds);
            _hrForward=capture.World;_hrProgress=capture.Progress;_hrNear=_hrFar=default;
            _hrFace=capture.Face with {Pull=snapshot.PullDips,EyeFollow=_hrFaceFollow.Eye,HairFollow=_hrFaceFollow.Hair};
            _hrRecoil=new(-Math.Sign(snapshot.Capture.OutwardUnit.X)*snapshot.RecoilOffset,-snapshot.RecoilLift);
        }else if(direct.PawPull is not null){
            SetHighResolutionPerchFace();
        }else{
            var ordinaryAllowed=!_locomotionBlocked&&pet.State is PetState.Idle or PetState.Walk&&!pet.IsDirectInteractionPending&&direct.HeadLanding is null&&!_snackPose.Active;
            _ordinaryBlinkMilliseconds=ordinaryAllowed?(_ordinaryBlinkMilliseconds+Math.Max(0,elapsed.TotalMilliseconds))%5000:0;
            var eyesClosed=_ordinaryBlinkMilliseconds is >=2000 and <2360;
            var traveled=_locomotionPosition is {} last?Math.Abs(pet.Position.X-last.X):0;
            _locomotionPosition=pet.Position;
            if(ordinaryAllowed||!_sittingRequested)_locomotion.Advance(Math.Max(0,elapsed.TotalMilliseconds),pet.State==PetState.Walk&&!_huntSession.IsActive,traveled,!ordinaryAllowed,_sittingRequested);
            if(!ordinaryAllowed&&!_sittingRequested)_locomotionStarted=false;
            else if(Math.Round(_locomotion.Sit*64)>0||Math.Round(_locomotion.Walk*8)>0)_locomotionStarted=true;
            if(_hrCheekCapture?.HighResolution is { } completed){_hrReturnFrom=completed.Face;_hrReturnAge=0;}
            _hrCheekCapture=null;
            _hrRecoil=default;
            // Captured facing belongs to the drag/fall/landing, not the next
            // idle tracking session. A cancelled press must release it too.
            if(ordinaryAllowed)_hrFacingCaptured=false;
            if(!_hrFacingCaptured)_hrFacing=pet.Facing;
            _hrPress=null;_hrSettling=false;_hrProgress=0;_hrAngle=0;_hrLag.Reset();_hrNear=_hrFar=default;
            _hrForward=GrabTransform(0,0,scale);
            var flip=_hrFacing==FacingDirection.Left;
            if(_huntingLastFlip is {} previousFlip&&previousFlip!=flip)_huntGaze.Reset();
            _huntingLastFlip=flip;
            PointD? target=_huntPointer is { } pointer?new((pointer.X-(flip?88:56))*2,(pointer.Y-77)*2):null;
            var gaze=_huntGaze.Advance(_huntDelta,target,flip,_huntTracking);
            _huntDelta=0;
            var age=_huntSession.IsActive?_huntSession.FrameAgeSeconds:_huntRecoveryAge;
            if(Pounce.Phase==PouncePhase.Flight){var u=Math.Clamp(Pounce.Age/.34,0,1);age=_pounceLaunchAge+(2.05-_pounceLaunchAge)*HighResolutionGrabRaster.Smooth(u);}
            else if(Pounce.Phase==PouncePhase.Landing)age=2.05+.55*Math.Clamp((Pounce.Age-.34)/.28,0,1);
            var frame=(int)Math.Clamp(Math.Round(age*60),0,156);
            // Departure recovery reads the last displayed preparation pose.
            // The legacy renderer used to publish this; its early return must
            // not leave the high-resolution path stuck at the neutral frame.
            _huntFrame=_huntVisualFrame=frame;
            _hrReturnAge=Math.Min(1,_hrReturnAge+elapsed.TotalSeconds/.24);
            var returnMix=HighResolutionGrabRaster.Smooth(_hrReturnAge);
            // Hunting owns the body clock, including departure recovery. Only
            // face/gaze blend here: a second body-to-upright blend would fight
            // the resumed crouch and visibly stand up then dip down again.
            var mix=1-Math.Pow(1-_huntBlendProgress,3);
            gaze=new(gaze.EyeX*mix,gaze.EyeY*mix,Math.Clamp(gaze.Roll*mix,-Math.PI/9,14*Math.PI/180));
            _hrFace=new(HuntRenderer.HeadTransform(frame,gaze),gaze.EyeX,gaze.EyeY,0,0,0,HuntPose.At(frame/60d).Amount>0?frame:-1);
            if(ordinaryAllowed&&_hrFace.BodyFrame<0&&(_locomotion.Sit>0||(!_huntTracking&&!_huntSession.IsActive))){
                var motion=_locomotion.Sit>0?HighResolutionLocomotion.Sit(_locomotion.Sit):HighResolutionLocomotion.Walk(_locomotion.Walk,_locomotion.Distance);
                var matrix=_hrFace.Head;
                if(motion>=0){var offset=_hrRaster.MotionOffset(motion);matrix.Translate(offset.X,offset.Y);}
                _hrFace=_hrFace with {Head=matrix,MotionFrame=motion,Closed=eyesClosed,EyeX=eyesClosed?0:_hrFace.EyeX,EyeY=eyesClosed?0:_hrFace.EyeY};
            }
            if(_hrReturnAge<1){
                var a=_hrReturnFrom.Head;var b=_hrFace.Head;var t=returnMix;
                _hrFace=_hrFace with {Head=new(a.M11+(b.M11-a.M11)*t,a.M12+(b.M12-a.M12)*t,a.M21+(b.M21-a.M21)*t,a.M22+(b.M22-a.M22)*t,a.OffsetX+(b.OffsetX-a.OffsetX)*t,a.OffsetY+(b.OffsetY-a.OffsetY)*t),
                    EyeX=_hrReturnFrom.EyeX+(_hrFace.EyeX-_hrReturnFrom.EyeX)*t,EyeY=_hrReturnFrom.EyeY+(_hrFace.EyeY-_hrReturnFrom.EyeY)*t};
            }
            ApplyHighResolutionState(pet,elapsed);
        }
        _hrFlutterMilliseconds=direct.IsPerchReady?(_hrFlutterMilliseconds+Math.Clamp(elapsed.TotalMilliseconds,0,250))%250:0;
        if(direct.IsPerchReady&&head)_hrFace=_hrFace with {FlutterPhase=_hrFlutterMilliseconds/250};
        _activeFacing=_hrFacing;_lastRenderedState=pet.State;_lastRenderedWindowPosition=pet.Position;
        _hrBase=_hrForward;DrawHighResolution(0);return true;
    }
    private Matrix GrabTransform(double angle,double remaining,double scale)
    {
        var m=Matrix.Identity;m.ScaleAt(_hrFacing==FacingDirection.Left?-1:1,1,48,48);m.RotateAt(angle,48,48);
        var p=m.Transform(new Point(_hrGrab.X,_hrGrab.Y));
        m.Translate((_hrPin.X/scale-24-p.X)*remaining,(_hrPin.Y/scale-24-p.Y)*remaining);return m;
    }
}

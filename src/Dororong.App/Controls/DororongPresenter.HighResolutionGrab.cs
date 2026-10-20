using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Controls;

public partial class DororongPresenter
{
    internal bool HighResolutionGrabEnabled { get; set; }
    internal double HighResolutionScale { get; set; } = 1;
    private HighResolutionGrabRaster? _hrRaster;
    private AlphaHitTestImage? _hrImage;
    private readonly HeadSurroundingPresentation _hrLag = new();
    private PointD _hrGrab=new(37.5,28.25),_hrPin=new(61.5,52.25);
    private PointD? _hrPress;
    private double _hrProgress,_hrReleaseStart,_hrAngle,_hrReleaseAngle;
    private bool _hrSettling,_hrDirect,_hrFacingCaptured;
    private readonly TranslateTransform _hrReleaseTranslation=new();
    private bool _hrReleaseTranslationActive;
    private double? _hrPreviousCenterY;
    private FacingDirection _hrFacing;
    private Matrix _hrForward=Matrix.Identity;
    private Matrix _hrBase=Matrix.Identity;
    private Matrix _hrCaptureForward=Matrix.Identity;
    private double _hrEntryProgress;
    private bool _hrCapturePending;
    private bool _hrBlendCapture;
    private HighResolutionFacePose _hrFace=HighResolutionFacePose.Neutral;
    private HighResolutionFacePose _hrEntryFace=HighResolutionFacePose.Neutral;
    private readonly FaceFollowMotion _hrFaceFollow=new();
    private CheekPullCapture? _hrCheekCapture;
    private bool _hrCheekReleasing;
    private PointD _hrRecoil;
    private HighResolutionFacePose _hrReturnFrom=HighResolutionFacePose.Neutral;
    private double _hrReturnAge=1;
    private readonly Dictionary<(double,Matrix,PointD,PointD,PointD,double,double,HighResolutionFacePose,double),byte[]> _hrFrames=new();
    private readonly Queue<(double,Matrix,PointD,PointD,PointD,double,double,HighResolutionFacePose,double)> _hrFrameOrder=new();
    private readonly Stack<byte[]> _hrSpareFrames=new(12);
    private readonly HighResolutionDisplaySurface _hrDisplay=new();
    private byte[]? _hrDisplayedPixels;
    private PointD _hrNear,_hrFar;
    private double _hrSleepAmount,_hrWakeFrom,_hrWakeAge=1;
    private double _hrFlutterMilliseconds;
    internal AlphaHitTestImage? HighResolutionImage => _hrImage is {Visibility:Visibility.Visible}?_hrImage:null;
    internal HighResolutionFacePose HighResolutionFace => _hrFace;
    internal int HighResolutionRasterizations { get; private set; }
    internal double HighResolutionRenderMilliseconds { get; private set; }
    internal double HighResolutionPreparationMilliseconds { get; private set; }
    internal double HighResolutionSamplingMilliseconds { get; private set; }
    internal double HighResolutionBitmapMilliseconds { get; private set; }
    internal double HighResolutionSoleMilliseconds { get; private set; }
    internal double HighResolutionLayoutMilliseconds { get; private set; }
    internal int HighResolutionLayoutFlushes { get; private set; }
    internal static bool NeedsHighResolutionLayout(FrameworkElement image)=>!image.IsMeasureValid||!image.IsArrangeValid;
    private void EnsureHighResolutionLayout(FrameworkElement image){
        // UpdateLayout also traverses automation peers. A valid image already
        // has correct bounds for hit/sole mapping; don't force another pass.
        // Dirty size/DPI/parent layout still flushes before geometry reads.
        if(!NeedsHighResolutionLayout(image))return;
        var started=System.Diagnostics.Stopwatch.GetTimestamp();
        image.UpdateLayout();HighResolutionLayoutFlushes++;
        HighResolutionLayoutMilliseconds+=System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;
    }
    private void RecordHighResolutionCost(){
        var cost=_hrRaster!.LastCost;
        HighResolutionPreparationMilliseconds+=cost.Preparation;
        HighResolutionSamplingMilliseconds+=cost.Sampling;
        HighResolutionBitmapMilliseconds+=cost.Bitmap;
    }
    internal int HighResolutionSoleQueries { get; private set; }
    private bool _hrDeferDrawing;
    internal void BeginHighResolutionPlatformFrame()=>_hrDeferDrawing=HighResolutionGrabEnabled;
    private readonly Dictionary<(double,Matrix,PointD,PointD,PointD,double,double,HighResolutionFacePose,PointD,double),FootContact> _hrSoles=new();
    private readonly Queue<(double,Matrix,PointD,PointD,PointD,double,double,HighResolutionFacePose,PointD,double)> _hrSoleOrder=new();
    internal string HighResolutionDiagnostic => $"progress={_hrProgress:R}; matrix={_hrForward}; near={_hrNear}; far={_hrFar}";

    private bool RenderHighResolutionGrab(PetSnapshot pet,DirectInteractionSnapshot direct,TimeSpan elapsed)
    {
        var head=direct.Target==DirectInteractionTarget.Body;
        var cheek=direct.CheekPull?.Capture.HighResolution is not null;
        var idle=direct.Target is DirectInteractionTarget.None or DirectInteractionTarget.ClickOnly||direct.PawPull is not null;
        if(!HighResolutionGrabEnabled||(!head&&!idle&&!cheek)){
            if(_hrImage is not null)_hrImage.Visibility=Visibility.Collapsed;
            BodyGroup.Visibility=Visibility.Visible;_hrLag.Reset();_hrFacingCaptured=false;return false;}
        var scale=HighResolutionScale;
        if(!double.IsFinite(scale)||scale<1||scale>1.5)throw new InvalidOperationException("High resolution scale must be 1–1.5.");
        // A disabled body drag retains its actual displayed pose until release.
        if(direct.Target==DirectInteractionTarget.ClickOnly&&HighResolutionImage is not null)return true;
        if(!_sizeChanged&&_huntAwaitingPress&&!_snackPose.Active&&!_sittingRequested&&direct.Target==DirectInteractionTarget.None&&HighResolutionImage is not null)return true;
        _sizeChanged=false;
        _hrRaster??=new();
        if(_hrImage is null){
            _hrImage=new AlphaHitTestImage{Stretch=Stretch.Fill,ContextMenu=BodyGroup.ContextMenu};
            _hrImage.MouseLeftButtonDown+=OnBodyPrimaryPressed;
            ((Canvas)Content).Children.Add(_hrImage);
            RenderOptions.SetBitmapScalingMode(_hrImage,BitmapScalingMode.NearestNeighbor);
        }
        _hrPreviousCenterY=head&&direct.Phase==DirectInteractionPhase.BodyDragSettle&&MeasurePlatformGeometry() is {} previous
            ?previous.Bounds.Y+previous.Bounds.Height/2:null;
        _platformContact.Restore();_extremeLanding.Reset();
        _hrImage.Width=_hrImage.Height=256*scale;Canvas.SetLeft(_hrImage,-56*scale);Canvas.SetTop(_hrImage,-56*scale);
        _hrImage.Visibility=Visibility.Visible;_hrImage.RenderTransform=Transform.Identity;_hrReleaseTranslationActive=false;BodyGroup.Visibility=Visibility.Hidden;
        _hrDirect=head;_directOwnsPresentation=head||cheek;_pendingClickPresentation=false;
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
    private bool HighResolutionFits(double angle,double remaining,double scale)
    {
        var m=GrabTransform(angle,remaining,scale);var viewport=SwingViewport;
        // Conservative source bounds cover all bank silhouettes plus lag support.
        foreach(var p in new[]{new Point(6,5),new Point(88,5),new Point(6,101),new Point(88,101)}){
            var q=m.Transform(p);var v=TranslatePoint(new Point((q.X+24)*scale,(q.Y+24)*scale),viewport);
            if(v.X<1||v.Y<1||v.X>viewport.ActualWidth-1||v.Y>viewport.ActualHeight-1)return false;}
        return true;
    }
    internal double LastRenderedSpread { get; private set; }
    private void DrawHighResolution(double spread)
    {
        if(_hrDeferDrawing)return;
        var inverse=_hrForward;inverse.Translate(_hrRecoil.X/HighResolutionScale,_hrRecoil.Y/HighResolutionScale);inverse.Invert();
        var dpi=VisualTreeHelper.GetDpi(this).DpiScaleX;
        // Repeated contact measurement can differ by ~1e-14 DIP. It must not
        // invalidate an otherwise stationary impact frame every tick.
        static double Q(double value)=>Math.Round(value,10);
        var cacheMatrix=new Matrix(Q(inverse.M11),Q(inverse.M12),Q(inverse.M21),Q(inverse.M22),Q(inverse.OffsetX),Q(inverse.OffsetY));
        var key=(Math.Floor(_hrProgress*224+.5)/224,cacheMatrix,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*dpi,_hrFace,HighResolutionScale);
        var rendered=!_hrFrames.TryGetValue(key,out var frame);
        if(rendered){
            HighResolutionRasterizations++;
            byte[]? reuse=null;
            if(_hrFrames.Count>=12){var expired=_hrFrameOrder.Dequeue();reuse=_hrFrames[expired];_hrFrames.Remove(expired);}
            else if(_hrSpareFrames.TryPop(out var spare))reuse=spare;
            var started=System.Diagnostics.Stopwatch.GetTimestamp();
            frame=_hrRaster!.RenderRetainedPixels(_hrProgress,inverse,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*dpi,_hrFace,HighResolutionScale,reuse,preserveCapacity:true);
            PlatformContactPresentation.InvalidateRetainedPixels(frame);
            RecordHighResolutionCost();
            HighResolutionRenderMilliseconds+=System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            _hrFrames[key]=frame;_hrFrameOrder.Enqueue(key);}
        if(rendered||!ReferenceEquals(frame,_hrDisplayedPixels)){
            var started=System.Diagnostics.Stopwatch.GetTimestamp();
            var bitmap=_hrDisplay.Update(frame!,HighResolutionScale*dpi);
            _hrImage!.SetLivePixels(bitmap,frame!);_hrDisplayedPixels=frame;
            var cost=System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            HighResolutionBitmapMilliseconds+=cost;HighResolutionRenderMilliseconds+=cost;
        }
        EnsureHighResolutionLayout(_hrImage!);
        LastRenderedSpread=spread;
    }
    private bool ApplyHighResolutionPlatform(PlatformPose? pose,double? sole)
    {
        _hrDeferDrawing=false;
        if(HighResolutionImage is not { } image)return false;
        if(_hrReleaseTranslationActive){image.RenderTransform=Transform.Identity;_hrReleaseTranslationActive=false;}
        _hrForward=_hrBase;
        if(_hrDirect&&_hrSettling&&pose is {Phase:PlatformPhase.Falling}){
            // Keep the visible center on the falling host trajectory. The
            // existing pose clock independently straightens around that center;
            // tracking the feet instead adds a temporary downward speed boost.
            DrawHighResolution(0);
            if(_hrPreviousCenterY is {} center&&MeasurePlatformGeometry() is {} recovering){
                _hrReleaseTranslation.Y=center-(recovering.Bounds.Y+recovering.Bounds.Height/2);
                image.RenderTransform=_hrReleaseTranslation;_hrReleaseTranslationActive=true;
                EnsureHighResolutionLayout(image);
            }
            _hrPreviousCenterY=null;return true;
        }
        if(_hrDirect||pose is not {Phase:not PlatformPhase.Suspended} p||sole is not { } target){DrawHighResolution(0);return true;}
        var scale=HighResolutionScale;
        var spread=p.IsExtremeLanding?p.LegSpread:0;
        EnsureHighResolutionLayout(image);
        var current=MeasureHighResolutionSole(spread);if(current is null){DrawHighResolution(spread);return true;}
        var c=current.Value;var x=(c.Left+c.Right)/2/scale-24;var y=c.SoleY/scale-24;
        var poseMatrix=Matrix.Identity;poseMatrix.ScaleAt(1+p.Squash*.5,1-p.Squash,x,y);
        poseMatrix.RotateAt(Math.Clamp(p.Sway*180/Math.PI,-2,2),x,y);
        _hrForward.Append(poseMatrix);
        var after=MeasureHighResolutionSole(spread);
        if(after is not null){
            var correction=(target-after.Value.SoleY)/scale;
            _hrForward.Translate(0,correction);
        }
        DrawHighResolution(spread);
        return true;
    }
    private FootContact? MeasureHighResolutionSole(double spread)
    {
        var inverse=_hrForward;inverse.Translate(_hrRecoil.X/HighResolutionScale,_hrRecoil.Y/HighResolutionScale);inverse.Invert();
        var dpi=VisualTreeHelper.GetDpi(this).DpiScaleX;
        static double Q(double v)=>Math.Round(v,10);
        var matrix=new Matrix(Q(inverse.M11),Q(inverse.M12),Q(inverse.M21),Q(inverse.M22),Q(inverse.OffsetX),Q(inverse.OffsetY));
        var key=(Math.Floor(_hrProgress*224+.5)/224,matrix,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*dpi,_hrFace,_hrRecoil,HighResolutionScale);
        if(_hrSoles.TryGetValue(key,out var cached))return cached;
        var started=System.Diagnostics.Stopwatch.GetTimestamp();HighResolutionSoleQueries++;
        var extent=(int)Math.Ceiling(256*HighResolutionScale*dpi);
        var pixelScale=_hrImage!.ActualWidth/extent;
        var sparse=_hrRaster!.RenderSolePixels(_hrProgress,inverse,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*dpi,_hrFace,2/pixelScale,HighResolutionScale);
        RecordHighResolutionCost();
        var geometry=MeasureImageGeometry(_hrImage,extent,extent,t=>_platformContact.MeasurePixels(sparse,extent,extent,t));
        var duration=System.Diagnostics.Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        HighResolutionRenderMilliseconds+=duration;HighResolutionSoleMilliseconds+=duration;
        if(geometry is not {} measured)return null;
        if(_hrSoles.Count>=24)_hrSoles.Remove(_hrSoleOrder.Dequeue());
        _hrSoles[key]=measured.Contact;_hrSoleOrder.Enqueue(key);return measured.Contact;
    }
    private DirectInteractionPressEventArgs? CreateHighResolutionPress(Point point)
    {
        if(HighResolutionImage is not { } image||!image.TryGetOpaqueSourcePoint(point,out _))return null;
        var scale=HighResolutionScale;
        var display=new Point((point.X-_hrRecoil.X)/scale-80,(point.Y-_hrRecoil.Y)/scale-80);var inverse=_hrForward;inverse.Invert();var source=inverse.Transform(display);
        var headInverse=_hrFace.Head;headInverse.Invert();var faceSource=headInverse.Transform(source);
        if(InteractionHitMap.IsSelectedCheek(new(faceSource.X,faceSource.Y))){
            var matrix=_hrFace.Head;matrix.Append(_hrForward);matrix.Translate(24,24);matrix.Scale(scale,scale);
            var capture=new CheekPullCapture(new HighResolutionCheekCapture(_hrFace,_hrForward,_hrProgress),matrix,_hrFacing);
            var local=image.TranslatePoint(point,this);
            return new(DirectInteractionTarget.RightCheek,new(local.X,local.Y),new(faceSource.X,faceSource.Y),Math.Sign(capture.OutwardUnit.X)){CheekCapture=capture,PressFacing=_hrFacing,IsAttachedCheek=_hrPerchPhase==EdgePerchPhase.Attached};
        }
        if(_hrPerchPhase==EdgePerchPhase.Attached&&source.Y>=70&&source.Y<100&&source.X>=19&&source.X<51){
            var local=image.TranslatePoint(point,this);var right=source.X>=35;
            return new(right?DirectInteractionTarget.PerchRightPaw:DirectInteractionTarget.PerchLeftPaw,new(local.X,local.Y),new(source.X+8,source.Y-6),-1){PressFacing=_hrFacing};
        }
        source=faceSource;
        if(!InteractionHitMap.IsUpperHead(new(source.X,source.Y))){
            var local=image.TranslatePoint(point,this);
            return new(DirectInteractionTarget.ClickOnly,new(local.X,local.Y),new(source.X,source.Y),-1){PressFacing=_hrFacing};
        }
        _hrBlendCapture=_hrProgress>0||Math.Abs(_hrForward.M12)>1e-6||Math.Abs(_hrForward.M21)>1e-6||Math.Abs(Math.Abs(_hrForward.M11)-1)>1e-6||Math.Abs(_hrForward.M22-1)>1e-6;
        _hrEntryProgress=_hrProgress;_hrEntryFace=_hrFace;_hrCaptureForward=_hrForward;_hrCapturePending=true;
        if(_hrReleaseTranslationActive)_hrCaptureForward.Translate(0,_hrReleaseTranslation.Y/scale);
        var window=image.TranslatePoint(point,this);_hrGrab=new(source.X,source.Y);_hrPin=new(window.X,window.Y);_hrFacingCaptured=true;
        return new(DirectInteractionTarget.Body,_hrPin,_hrGrab,-1){PressFacing=_hrFacing,StartsHanging=_hrPerchPhase==EdgePerchPhase.Attached};
    }
    internal Point HighResolutionSourceToPresenter(Point point)
    {
        var p=_hrForward.Transform(point);return new((p.X+24)*HighResolutionScale,
            (p.Y+24)*HighResolutionScale+(_hrReleaseTranslationActive?_hrReleaseTranslation.Y:0));
    }
    internal Point HighResolutionFaceToPresenter(Point point)=>HighResolutionSourceToPresenter(_hrFace.Head.Transform(point));
}

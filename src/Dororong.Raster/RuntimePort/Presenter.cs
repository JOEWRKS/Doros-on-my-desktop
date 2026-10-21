using System.Windows;
using System.Windows.Media;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
namespace Dororong.App.Controls;

// Headless r104 presentation coordinator. Sampling owns no native window.
// Presenter coordinates are unchanged (144*size); raster begins at -56*size.
internal partial class DororongPresenter {
 internal bool HighResolutionGrabEnabled=>true;
 internal double HighResolutionScale {get;private set;}=1;
 private double _density=1;
 internal double Density {get=>_density;set { _=Dororong.Raster.R104Raster.Extent(HighResolutionScale,value);if(_density==value)return;_density=value;_perchLeaving=false;_perchExtent=0;if(_hasFrame){_hrDeferDrawing=false;DrawHighResolution(_spread);} }}
 private readonly HighResolutionGrabRaster _hrRaster=new();
 private readonly HeadSurroundingPresentation _hrLag=new();
 private PointD _hrGrab=new(37.5,28.25),_hrPin=new(61.5,52.25);
 private PointD? _hrPress;
 private double _hrProgress,_hrReleaseStart,_hrAngle,_hrReleaseAngle,_hrEntryProgress;
 private bool _hrSettling,_hrDirect,_hrFacingCaptured,_hrCapturePending,_hrBlendCapture;
 private double? _hrPreviousCenterY;
 private FacingDirection _hrFacing,_activeFacing;
 private Matrix _hrForward=Matrix.Identity,_hrBase=Matrix.Identity,_hrCaptureForward=Matrix.Identity;
 private HighResolutionFacePose _hrFace=HighResolutionFacePose.Neutral,_hrEntryFace=HighResolutionFacePose.Neutral;
 private readonly FaceFollowMotion _hrFaceFollow=new();
 private CheekPullCapture? _hrCheekCapture;
 private bool _hrCheekReleasing;
 private PointD _hrRecoil,_hrNear,_hrFar;
 private HighResolutionFacePose _hrReturnFrom=HighResolutionFacePose.Neutral;
 private double _hrReturnAge=1,_hrSleepAmount,_hrWakeFrom,_hrWakeAge=1,_hrFlutterMilliseconds;
 private bool _hasFrame,_sizeChanged,_directOwnsPresentation,_hrDeferDrawing,_sleepEntryComplete,_locomotionBlocked,_locomotionStarted,_sittingRequested;
 private PointD? _locomotionPosition;
 private PointD _lastRenderedWindowPosition;
 private PetState? _lastRenderedState;
 private readonly LocomotionPresentation _locomotion=new();
 private double _ordinaryBlinkMilliseconds;
 private readonly PounceSession _pounce=new();
 private double _pounceLaunchAge=1.5;
 internal PouncePose Pounce=>_pounce.Current;
 private readonly HuntingSession _huntSession=new();
 private readonly HuntGaze _huntGaze=new();
 private PointD? _huntPointer;
 private double _huntDelta,_huntBlendProgress,_huntRecoveryAge=2.6;
 private bool _huntNear,_huntTracking,_huntAwaitingPress;
 private int _huntFrame,_huntVisualFrame=156;
 private bool? _huntingLastFlip;
 private SnackPose _snackPose;
 private Matrix _snackHead=Matrix.Identity;
 private double _snackRecoveryAge=2.6;
 private EdgePerchPhase _hrPerchPhase;
 private readonly PerchExpressionMotion _hrPerchMotion=new();
 private PerchPawSnapshot? _hrPerchPaw;
 private double _releaseOffset,_spread,_perchReleaseAge=120;
 private byte[] _pixels=[],_perchPixels=[],_perchBlend=[];
 private int _perchExtent;
 private bool _perchLeaving;
 internal byte[] Pixels=>_perchLeaving&&_perchReleaseAge<120?_perchBlend:_pixels;
 internal int Extent=>Dororong.Raster.R104Raster.Extent(HighResolutionScale,Density);
 internal long FrameRevision {get;private set;}
 internal double ReleaseOffset=>_releaseOffset;
 internal HighResolutionFacePose Face=>_hrFace;
 internal bool CapturingPose=>_directOwnsPresentation;
 internal bool Sitting=>_sittingRequested;
 internal void SetArtSize(int percent) { if(percent is <100 or >150)throw new ArgumentOutOfRangeException(nameof(percent));if(HighResolutionScale==percent/100d)return;HighResolutionScale=percent/100d;_sizeChanged=true;_perchLeaving=false;_perchExtent=0; }
 internal void RebaseSizePresentation(PointD offset) {if(_locomotionPosition is {} p)_locomotionPosition=p+offset;}
 internal bool HoldLocomotionWalk(PetSnapshot p)=>p.State==PetState.Walk&&!p.IsDirectInteractionPending&&!_locomotionBlocked&&_locomotion.HoldWalking;
 internal void SetLocomotionBlocked(bool blocked)=>_locomotionBlocked=blocked;
 internal void SetSittingRequested(bool requested)=>_sittingRequested=requested;
 internal void BeginHighResolutionPlatformFrame()=>_hrDeferDrawing=true;
 internal void RenderDesktop(PetSnapshot pet,DirectInteractionSnapshot direct)=>RenderDesktop(pet,direct,TimeSpan.Zero);
 internal void RenderDesktop(PetSnapshot pet,DirectInteractionSnapshot direct,TimeSpan elapsed) {
  if(pet.State is PetState.Idle or PetState.Walk&&!pet.IsDirectInteractionPending&&direct.Target==DirectInteractionTarget.None)
   pet=pet with {Facing=pet.Facing==FacingDirection.Right?FacingDirection.Left:FacingDirection.Right};
  RenderHighResolutionGrab(pet,direct,elapsed);
 }
 internal bool UpdateHuntingWithPounce(PetSnapshot p,DirectInteractionSnapshot d,PointerSample pointer,TimeSpan dt,bool down)=>UpdateHuntingCore(p,d,pointer,dt,down,true);
 private void ResetHunting() { _huntSession.Reset();_pounce.Reset();_huntGaze.Reset();_huntPointer=null;_huntNear=_huntTracking=false;_huntBlendProgress=0;_huntRecoveryAge=2.6;_huntVisualFrame=156;_huntingLastFlip=null; }
 internal void SetSnack(SnackPose pose,PointerSample pointer) {if(pose.Active!=_snackPose.Active){_snackRecoveryAge=SnackRenderer.RecoveryAge(HuntPose.At(_huntVisualFrame/60d).Amount);ResetHunting();}_snackPose=pose;}
 internal PointD GetSnackMouth(){var p=HighResolutionFaceToPresenter(new(29,60));return new(p.X,p.Y);}
 private bool HighResolutionFits(double angle,double remaining,double scale) {
  var m=GrabTransform(angle,remaining,scale);
  foreach(var p in new[]{new Point(6,5),new Point(88,5),new Point(6,101),new Point(88,101)}) {
   var q=m.Transform(p);var x=(q.X+24)*scale+96;var y=(q.Y+24)*scale+96;
   if(x<1||y<1||x>144*scale+191||y>144*scale+191)return false;
  }return true;
 }
 private Matrix Inverse(){var m=_hrForward;m.Translate(_hrRecoil.X/HighResolutionScale,_hrRecoil.Y/HighResolutionScale);m.Invert();return m;}
 private (double,Matrix,PointD,PointD,PointD,double,double,HighResolutionFacePose,double)? _lastKey;
 private void DrawHighResolution(double spread) {
  if(_hrDeferDrawing)return;
  var inverse=Inverse();static double Q(double v)=>Math.Round(v,10);
  var q=new Matrix(Q(inverse.M11),Q(inverse.M12),Q(inverse.M21),Q(inverse.M22),Q(inverse.OffsetX),Q(inverse.OffsetY));
  var key=(Math.Floor(_hrProgress*224+.5)/224,q,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*Density,_hrFace,HighResolutionScale);
  if(_lastKey==key&&_hasFrame)return;
  _pixels=_hrRaster.RenderOwnedPixels(_hrProgress,inverse,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*Density,_hrFace,HighResolutionScale,_pixels,incremental:true);
  _lastKey=key;_hasFrame=true;_spread=spread;FrameRevision++;
 }
 private (FootContact Contact,RectD Bounds)? Measure(byte[] bytes,double translate=0) {
  var n=Extent;var pixelScale=256*HighResolutionScale/n;var minX=n;var minY=n;var maxX=-1;var maxY=-1;
  for(var y=0;y<n;y++)for(var x=0;x<n;x++)if(bytes[(y*n+x)*4+3]>=128){minX=Math.Min(minX,x);maxX=Math.Max(maxX,x);minY=Math.Min(minY,y);maxY=Math.Max(maxY,y);}
  if(maxY<0)return null;
  var left=n;var right=-1;var band=(maxY+1)-2/pixelScale;
  for(var y=Math.Max(0,(int)Math.Floor(band));y<=maxY;y++)for(var x=0;x<n;x++)if(y+1>band+1e-9&&bytes[(y*n+x)*4+3]>=128){left=Math.Min(left,x);right=Math.Max(right,x);}
  var ox=-56*HighResolutionScale-_hrRecoil.X;var oy=-56*HighResolutionScale-_hrRecoil.Y+translate;
  return (new(left*pixelScale+ox,(right+1)*pixelScale+ox,(maxY+1)*pixelScale+oy,minY*pixelScale+oy),new(minX*pixelScale+ox,minY*pixelScale+oy,(maxX-minX+1)*pixelScale,(maxY-minY+1)*pixelScale));
 }
 private (long Revision,int Extent,double Scale,PointD Recoil,double Release)? _geometryKey;
 private (FootContact Contact,RectD Bounds)? _geometry;
 internal long GeometryScans {get;private set;}
 internal (FootContact Contact,RectD Bounds)? MeasurePlatformGeometry(){
  if(!_hasFrame)return null;
  // Geometry is read repeatedly by platform, size-anchor and release handling.
  // Keep only one result; pose offsets can change without a new raster revision.
  var key=(FrameRevision,Extent,HighResolutionScale,_hrRecoil,_releaseOffset);
  if(_geometryKey==key)return _geometry;
  _geometry=Measure(_pixels,_releaseOffset);_geometryKey=key;GeometryScans++;return _geometry;
 }
 private FootContact? MeasureHighResolutionSole(double spread) {
  var inverse=Inverse();static double Q(double v)=>Math.Round(v,10);
  var q=new Matrix(Q(inverse.M11),Q(inverse.M12),Q(inverse.M21),Q(inverse.M22),Q(inverse.OffsetX),Q(inverse.OffsetY));
  var key=(Math.Floor(_hrProgress*224+.5)/224,q,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*Density,_hrFace,_hrRecoil,HighResolutionScale);
  if(_soles.TryGetValue(key,out var found))return found;
  var pixels=_hrRaster.RenderSolePixels(_hrProgress,inverse,_hrGrab,_hrNear,_hrFar,spread,HighResolutionScale*Density,_hrFace,2*Extent/(256*HighResolutionScale),HighResolutionScale);
  var measured=Measure(pixels)?.Contact;
  if(measured is {} foot){if(_soles.Count>=24)_soles.Remove(_soleOrder.Dequeue());_soles[key]=foot;_soleOrder.Enqueue(key);}
  return measured;
 }
 private readonly Dictionary<(double,Matrix,PointD,PointD,PointD,double,double,HighResolutionFacePose,PointD,double),FootContact> _soles=new();
 private readonly Queue<(double,Matrix,PointD,PointD,PointD,double,double,HighResolutionFacePose,PointD,double)> _soleOrder=new();
 internal void ApplyPlatformPose(PlatformPose? pose,double? sole) {
  _hrDeferDrawing=false;_releaseOffset=0;_hrForward=_hrBase;
  if(_hrDirect&&_hrSettling&&pose is {Phase:PlatformPhase.Falling}) {
   DrawHighResolution(0);if(_hrPreviousCenterY is {} center&&MeasurePlatformGeometry() is {} recovering)_releaseOffset=center-(recovering.Bounds.Y+recovering.Bounds.Height/2);
   _hrPreviousCenterY=null;return;
  }
  if(_hrDirect||pose is not {Phase:not PlatformPhase.Suspended} p||sole is not {} target){DrawHighResolution(0);return;}
  var scale=HighResolutionScale;var spread=p.IsExtremeLanding?p.LegSpread:0;
  if(MeasureHighResolutionSole(spread) is not {} c){DrawHighResolution(spread);return;}
  var x=(c.Left+c.Right)/2/scale-24;var y=c.SoleY/scale-24;
  var matrix=Matrix.Identity;matrix.ScaleAt(1+p.Squash*.5,1-p.Squash,x,y);matrix.RotateAt(Math.Clamp(p.Sway*180/Math.PI,-2,2),x,y);_hrForward.Append(matrix);
  if(MeasureHighResolutionSole(spread) is {} after)_hrForward.Translate(0,(target-after.SoleY)/scale);
  DrawHighResolution(spread);
 }
 private void SetHighResolutionPerchFace(){
  HighResolutionMotion(HighResolutionLocomotion.Perch,_hrPerchMotion.EyesClosed);
  if(_hrPerchPaw is {} paw)_hrFace=_hrFace with {Paw=paw.Right?2:1,PawX=paw.Offset.X,PawY=paw.Offset.Y,PawAngle=paw.Angle};
  _hrProgress=0;_hrForward=GrabTransform(0,0,HighResolutionScale);_hrForward.ScaleAt(1,_hrPerchMotion.ScaleY,48,70);_hrBase=_hrForward;
 }
 internal PerchContact? MeasureEdgePerchContact(FacingDirection facing)=>new((facing==FacingDirection.Left?70:44)*HighResolutionScale,(facing==FacingDirection.Left?100:74)*HighResolutionScale,94*HighResolutionScale,43*HighResolutionScale);
 internal void ApplyEdgePerch(EdgePerchPhase phase,FacingDirection facing,TimeSpan delta,bool immediate){
  if(phase==EdgePerchPhase.None){
   if(immediate){_perchLeaving=false;_perchExtent=0;}
   else if(_hrPerchPhase!=EdgePerchPhase.None){_perchLeaving=true;_perchReleaseAge=0;}
   _hrPerchPhase=phase;_hrPerchMotion.Reset();
   if(_perchLeaving&&_perchExtent==Extent){
    _perchReleaseAge=Math.Min(120,_perchReleaseAge+Math.Max(0,delta.TotalMilliseconds));
    if(_perchReleaseAge>=120){_perchLeaving=false;FrameRevision++;return;}
    var count=Extent*Extent*4;if(_perchBlend.Length<count)_perchBlend=new byte[count];
    Buffer.BlockCopy(_pixels,0,_perchBlend,0,count);var row=Math.Clamp((int)Math.Ceiling((104+70*_perchReleaseAge/120)/256*Extent),0,Extent);
    Buffer.BlockCopy(_perchPixels,row*Extent*4,_perchBlend,row*Extent*4,(Extent-row)*Extent*4);FrameRevision++;
   }else _perchLeaving=false;
   return;
  }
  _perchLeaving=false;if(_hrPerchPhase==EdgePerchPhase.None)_hrPerchMotion.Reset();_hrPerchPhase=phase;_hrFacing=facing;
  if(_hrCheekCapture is not null){_hrPerchMotion.OpenEyes();return;}
  _hrPerchMotion.Advance(delta);SetHighResolutionPerchFace();DrawHighResolution(0);
  var length=Extent*Extent*4;if(_perchPixels.Length<length)_perchPixels=new byte[length];Buffer.BlockCopy(_pixels,0,_perchPixels,0,length);_perchExtent=Extent;
 }
 internal bool Hit(PointD point) {
  var ratio=Extent/(256*HighResolutionScale);var x=(point.X+56*HighResolutionScale)*ratio;var y=(point.Y+56*HighResolutionScale-_releaseOffset)*ratio;
  return _hasFrame&&double.IsFinite(x)&&double.IsFinite(y)&&x>=0&&y>=0&&x<Extent&&y<Extent&&Pixels[((int)y*Extent+(int)x)*4+3]>=16;
 }
 internal DirectInteractionPressEventArgs? Press(PointD point){
  if(!Hit(point))return null;var scale=HighResolutionScale;
  var display=new Point((point.X-_hrRecoil.X)/scale-24,(point.Y-_hrRecoil.Y-_releaseOffset)/scale-24);
  var inverse=_hrForward;inverse.Invert();var source=inverse.Transform(display);var headInverse=_hrFace.Head;headInverse.Invert();var faceSource=headInverse.Transform(source);
  if(InteractionHitMap.IsSelectedCheek(new(faceSource.X,faceSource.Y))){
   var matrix=_hrFace.Head;matrix.Append(_hrForward);matrix.Translate(24,24);matrix.Scale(scale,scale);
   var capture=new CheekPullCapture(new(_hrFace,_hrForward,_hrProgress),matrix,_hrFacing);
   return new(DirectInteractionTarget.RightCheek,point,new(faceSource.X,faceSource.Y),Math.Sign(capture.OutwardUnit.X)){CheekCapture=capture,PressFacing=_hrFacing,IsAttachedCheek=_hrPerchPhase==EdgePerchPhase.Attached};
  }
  if(_hrPerchPhase==EdgePerchPhase.Attached&&source.Y>=70&&source.Y<100&&source.X>=19&&source.X<51)
   return new(source.X>=35?DirectInteractionTarget.PerchRightPaw:DirectInteractionTarget.PerchLeftPaw,point,new(source.X+8,source.Y-6),-1){PressFacing=_hrFacing};
  source=faceSource;
  if(!InteractionHitMap.IsUpperHead(new(source.X,source.Y)))return new(DirectInteractionTarget.ClickOnly,point,new(source.X,source.Y),-1){PressFacing=_hrFacing};
  _hrBlendCapture=_hrProgress>0||Math.Abs(_hrForward.M12)>1e-6||Math.Abs(_hrForward.M21)>1e-6||Math.Abs(Math.Abs(_hrForward.M11)-1)>1e-6||Math.Abs(_hrForward.M22-1)>1e-6;
  _hrEntryProgress=_hrProgress;_hrEntryFace=_hrFace;_hrCaptureForward=_hrForward;_hrCapturePending=true;
  _hrCaptureForward.Translate(0,_releaseOffset/scale);_hrGrab=new(source.X,source.Y);_hrPin=point;_hrFacingCaptured=true;
  return new(DirectInteractionTarget.Body,point,_hrGrab,-1){PressFacing=_hrFacing,StartsHanging=_hrPerchPhase==EdgePerchPhase.Attached};
 }
 internal Point HighResolutionSourceToPresenter(Point point){var p=_hrForward.Transform(point);return new((p.X+24)*HighResolutionScale,(p.Y+24)*HighResolutionScale+_releaseOffset);}
 internal Point HighResolutionFaceToPresenter(Point point)=>HighResolutionSourceToPresenter(_hrFace.Head.Transform(point));
 private static double GetBreathingAmount(double phase){if(phase<.38){var u=phase/.38;return u*u*(3-2*u);}if(phase<=.43)return 1;var v=(phase-.43)/.57;return 1-v*v*(3-2*v);}
}

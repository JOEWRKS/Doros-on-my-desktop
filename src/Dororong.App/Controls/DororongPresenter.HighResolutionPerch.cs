using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Platforms;

namespace Dororong.App.Controls;

public partial class DororongPresenter
{
    private EdgePerchPhase _hrPerchPhase;
    private readonly PerchExpressionMotion _hrPerchMotion=new();
    private PerchPawSnapshot? _hrPerchPaw;
    private BitmapSource? _hrPerchLastFrame,_hrPerchReleaseFrame;
    private readonly HighResolutionPerchBlend _hrPerchBlend=new();
    private double _hrPerchReleaseAge;
    private void SetHighResolutionPerchFace(){
        HighResolutionMotion(HighResolutionLocomotion.Perch,_hrPerchMotion.EyesClosed);
        if(_hrPerchPaw is {} paw)_hrFace=_hrFace with {Paw=paw.Right?2:1,PawX=paw.Offset.X,PawY=paw.Offset.Y,PawAngle=paw.Angle};
        _hrProgress=0;_hrForward=GrabTransform(0,0,HighResolutionScale);
        _hrForward.ScaleAt(1,_hrPerchMotion.ScaleY,48,70);
        _hrBase=_hrForward;
    }
    private void ApplyHighResolutionPerch(EdgePerchPhase phase,FacingDirection facing,TimeSpan delta,bool immediateRestore){
        _edgePerch.Restore();
        if(HighResolutionImage is null)return;
        if(phase==EdgePerchPhase.None){
            if(immediateRestore){_hrPerchLastFrame=null;_hrPerchReleaseFrame=null;}
            else if(_hrPerchPhase!=EdgePerchPhase.None){_hrPerchReleaseFrame=_hrPerchLastFrame;_hrPerchReleaseAge=0;}
            _hrPerchPhase=phase;_hrPerchMotion.Reset();
            if(_hrPerchReleaseFrame is {} previous&&_hrImage?.Source is BitmapSource current){
                _hrPerchReleaseAge=Math.Min(120,_hrPerchReleaseAge+Math.Max(0,delta.TotalMilliseconds));
                if(_hrPerchReleaseAge>=120){_hrPerchReleaseFrame=null;_hrPerchLastFrame=null;return;}
                // Same 120ms complementary horizontal clip as the legacy
                // presenter. Copy existing native rows; never resize/fade art.
                if(previous.PixelWidth==current.PixelWidth&&previous.PixelHeight==current.PixelHeight){
                    var merged=_hrPerchBlend.Update(previous,current,_hrPerchReleaseAge);
                    _hrImage.SetLivePixels(merged,_hrPerchBlend.Pixels);
                }
            }
            return;
        }
        _hrPerchReleaseFrame=null;
        if(_hrPerchPhase==EdgePerchPhase.None)_hrPerchMotion.Reset();
        _hrPerchPhase=phase;_hrFacing=facing;
        if(_hrCheekCapture is not null){_hrPerchMotion.OpenEyes();return;}
        _hrPerchMotion.Advance(delta);SetHighResolutionPerchFace();DrawHighResolution(0);
        _hrPerchLastFrame=_hrImage?.Source as BitmapSource;
    }
}

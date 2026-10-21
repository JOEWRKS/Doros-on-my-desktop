using System.Windows.Media;
using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
namespace Dororong.App.Interaction;
// Only the accepted high-resolution capture route is reachable in the Mac host.
internal sealed class CheekPullCapture {
 internal Matrix SourceToWindow {get;}
 internal FacingDirection Facing {get;}
 internal PointD OutwardUnit {get;}
 internal HighResolutionCheekCapture HighResolution {get;}
 internal CheekPullCapture(HighResolutionCheekCapture capture,Matrix matrix,FacingDirection facing) {
  if(!matrix.HasInverse)throw new ArgumentException("Invertible capture required.");
  var axis=matrix.Transform(new System.Windows.Vector(1,0));var length=axis.Length;
  if(!double.IsFinite(length)||length<=0)throw new ArgumentException("Finite capture required.");
  SourceToWindow=matrix;Facing=facing;HighResolution=capture;OutwardUnit=new(-axis.X/length,-axis.Y/length);
 }
 internal double Measure(PointD delta)=>delta.X*OutwardUnit.X+delta.Y*OutwardUnit.Y;
 internal CheekPullCapture TurnToward(double dx) {
  if(!double.IsFinite(dx)||Math.Abs(dx)<4||Math.Sign(dx)==Math.Sign(OutwardUnit.X))return this;
  // Preserve the high-resolution capture when reflecting the seated pose.
  // A legacy 96px fallback would discard the source-resolution art.
  var world=new Matrix(-1,0,0,1,96,0);world.Append(HighResolution.World);
  var matrix=new Matrix(-1,0,0,1,96,0);matrix.Append(SourceToWindow);
  return new(HighResolution with {World=world},matrix,Facing==FacingDirection.Left?FacingDirection.Right:FacingDirection.Left);
 }
}
internal sealed record HighResolutionCheekCapture(HighResolutionFacePose Face,Matrix World,double Progress);
internal sealed record CheekPullSnapshot(CheekPullCapture Capture,double PullDips) {
 internal bool SpringRelease {get;init;} internal double ReleasedPullDips {get;init;}
 internal double RecoilOffset {get;init;} internal double RecoilLift {get;init;}
}

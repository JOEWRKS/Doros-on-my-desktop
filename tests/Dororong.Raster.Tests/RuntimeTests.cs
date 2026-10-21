using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
namespace Dororong.Raster.Tests;
public sealed class RuntimeTests {
 private static PetSnapshot Pet()=>new PetBrain(BehaviorTuning.Default,new R(),new(100,100)).Current;
 [Theory][InlineData(100)][InlineData(125)][InlineData(150)]
 public void NeutralGeometryHitAndCapturedHeadStayAligned(int percent){
  var p=new DororongPresenter();p.SetArtSize(percent);var pet=Pet();p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
  Assert.NotNull(p.MeasurePlatformGeometry());
  var head=p.HighResolutionFaceToPresenter(new(37.5,28.25));
  var press=p.Press(new(head.X,head.Y));Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.Body,press.Target);
  var direct=new DirectInteractionSnapshot(DirectInteractionTarget.Body,DirectInteractionPhase.BodyDragHold,new(500,300),new(500,200),1,0,true){PressFacing=press.PressFacing};
  p.RenderDesktop(pet,direct,TimeSpan.FromMilliseconds(16));
  var shown=p.HighResolutionFaceToPresenter(new(37.5,28.25));
  Assert.InRange(Math.Abs(shown.X-head.X),0,.0001);Assert.InRange(Math.Abs(shown.Y-head.Y),0,.0001);
 }
 [Fact] public void SleepingPhaseWrapKeepsClosedEyes(){
  var p=new DororongPresenter();var pet=Pet() with {State=PetState.Sleep,Phase=.5};
  p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));Assert.True(p.Face.Closed);
  p.RenderDesktop(pet with {Phase=.001},DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));Assert.True(p.Face.Closed);
 }
 [Fact] public void SeatedTrackingDoesNotReplaceSittingTorso(){
  var p=new DororongPresenter();p.SetSittingRequested(true);var pet=Pet();
  p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.FromSeconds(1.05));
  var frame=p.Face.MotionFrame;Assert.Equal(321,frame);
  for(var i=0;i<8;i++){p.UpdateHuntingWithPounce(pet,DirectInteractionSnapshot.None,new(true,new(330,200)),TimeSpan.FromMilliseconds(16),false);p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));}
  Assert.Equal(frame,p.Face.MotionFrame);Assert.True(Math.Abs(p.Face.EyeX)>0);Assert.Equal(-1,p.Face.BodyFrame);
 }
 [Fact] public void CheekCapturesAndRestoresCrouchWithoutUprightSubstitution(){
  var p=new DororongPresenter();var pet=Pet();var face=HighResolutionFacePose.Neutral with {BodyFrame=60};
  var cap=new CheekPullCapture(new(face,System.Windows.Media.Matrix.Identity,0),System.Windows.Media.Matrix.Identity,FacingDirection.Right);
  var direct=new DirectInteractionSnapshot(DirectInteractionTarget.RightCheek,DirectInteractionPhase.CheekPull,default,new(-20,0),1,0,true){CheekPull=new(cap,20)};
  p.RenderDesktop(pet,direct,TimeSpan.FromMilliseconds(16));Assert.Equal(60,p.Face.BodyFrame);Assert.Equal(20,p.Face.Pull);
  p.RenderDesktop(pet,direct with {Phase=DirectInteractionPhase.CheekRelease,CheekPull=new(cap,5){SpringRelease=true}},TimeSpan.FromMilliseconds(16));Assert.Equal(60,p.Face.BodyFrame);
 }
 private sealed class R:IRandomSource{public double NextUnit()=>.5;}
 [Fact] public void EatingBankHasBoundedExactTimelineAndNoExtrapolation(){
  var bytes=new byte[225*225*4];for(var i=0;i<103;i++)Assert.True(FixedEating.Copy(i/60d,bytes));
  Assert.False(FixedEating.Copy(.18,bytes));Assert.False(FixedEating.Copy(2,bytes));Assert.False(FixedEating.Copy(-1,bytes));
 }
 [Fact] public void SeatedCheekReversalRetainsSourceResolutionCapture(){
  var c=new CheekPullCapture(new(HighResolutionFacePose.Neutral,System.Windows.Media.Matrix.Identity,0),System.Windows.Media.Matrix.Identity,FacingDirection.Right);
  var turned=c.TurnToward(20);Assert.NotSame(c,turned);Assert.NotNull(turned.HighResolution);Assert.Equal(FacingDirection.Left,turned.Facing);Assert.True(turned.OutwardUnit.X>0);
  Assert.Same(turned,turned.TurnToward(20));
 }
}

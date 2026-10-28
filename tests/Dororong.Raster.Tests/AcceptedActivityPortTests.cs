using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using System.Windows.Media;
namespace Dororong.Raster.Tests;
public sealed class AcceptedActivityPortTests {
 private static PetSnapshot Pet()=>new(PetState.Idle,default,FacingDirection.Right,0,false,null);
 [Theory][InlineData(100)][InlineData(125)][InlineData(150)]
 public void RuntimeSeatedCursorTapThenBodyClickStaysSeatedAndCancelsOnStand(int size){
  var p=new DororongPresenter();p.SetArtSize(size);p.SetSittingRequested(true);var pet=Pet();
  p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.FromSeconds(1.1));
  // RenderDesktop mirrors authored left-facing art for a right-facing pet.
  var cursor=new Dororong.Core.Geometry.PointD((24+86)*size/100d,(24+65)*size/100d);
  for(var i=0;i<20;i++){
   p.UpdateHuntingWithPounce(pet,DirectInteractionSnapshot.None,new(true,cursor),TimeSpan.FromSeconds(.02),false);
   p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.FromSeconds(.02));
  }
  Assert.True(p.Face.SeatedPawAmount>.1);Assert.Equal(321,p.Face.MotionFrame);
  p.RenderDesktop(pet with{State=PetState.ClickReaction,Phase=.2},DirectInteractionSnapshot.None,TimeSpan.FromSeconds(.02));
  p.RenderDesktop(pet with{State=PetState.ClickReaction,Phase=.3},DirectInteractionSnapshot.None,TimeSpan.FromSeconds(.1));
  Assert.Equal(321,p.Face.MotionFrame);Assert.NotEqual(0,p.Face.SeatedShake);Assert.Equal(0,p.Face.SeatedPawAmount);
  p.SetSittingRequested(false);p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.FromSeconds(.02));
  Assert.Equal(0,p.Face.SeatedShake);
 }
 [Theory][InlineData(IdleActivityKind.Groom,1.1)][InlineData(IdleActivityKind.Stretch,.8)][InlineData(IdleActivityKind.Beg,.8)]
 public void RuntimeIdleActivityReachesRendererAndEndsWithoutResidue(IdleActivityKind kind,double age){
  var p=new DororongPresenter();var pet=Pet();
  p.RenderDesktop(pet with{IdleActivity=new(kind,age)},DirectInteractionSnapshot.None,TimeSpan.Zero);
  Assert.True(kind switch{IdleActivityKind.Groom=>p.Face.GroomingPaw&&p.Face.ChuckleCover>0,IdleActivityKind.Stretch=>p.Face.Stretch>0,_=>p.Face.StompLift>0});
  p.RenderDesktop(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
  Assert.False(p.Face.GroomingPaw);Assert.Equal(0,p.Face.Stretch);Assert.Equal(0,p.Face.StompLift);Assert.Equal(0,p.Face.ChuckleCover);
 }
 [Fact] public void QuestionUsesAcceptedPlacementAndHidesDuringSitSnackAndDirectInteraction(){
  var p=new DororongPresenter();var pet=Pet() with{DoroQuestionPhase=.5};
  p.RenderDesktop(pet,DirectInteractionSnapshot.None);Assert.True(p.Question.Visible);Assert.Equal(1,p.Question.Opacity);Assert.NotEqual(0,p.Face.Head.M12);
  p.SetSittingRequested(true);p.RenderDesktop(pet,DirectInteractionSnapshot.None);Assert.False(p.Question.Visible);
  p.SetSittingRequested(false);p.SetSnack(new(SnackPhase.Chasing,FacingDirection.Right,0,0,0,0),default);
  p.RenderDesktop(pet,DirectInteractionSnapshot.None);Assert.False(p.Question.Visible);
  p.SetSnack(default,default);p.RenderDesktop(pet with{IsDirectInteractionPending=true},DirectInteractionSnapshot.None);Assert.False(p.Question.Visible);
 }
 [Fact] public void StaticDeformedBodyWithChangingBreathingFallsBackToWholeFrame(){
  var full=new HighResolutionGrabRaster();var partial=new HighResolutionGrabRaster();byte[]? a=null,b=null;
  foreach(var f in new[]{HighResolutionFacePose.Neutral with{Stretch=.7},HighResolutionFacePose.Neutral with{SeatedShake=.5},HighResolutionFacePose.Neutral with{ChuckleCover=.6}})
  foreach(var breath in new[]{0d,.4,.8,0}){
   var face=f with{Breathing=breath};a=full.RenderRetainedPixels(0,Matrix.Identity,default,default,default,0,1.5,face,1.5,a,true);
   b=partial.RenderOwnedPixels(0,Matrix.Identity,default,default,default,0,1.5,face,1.5,b,true);
   Assert.Null(partial.LastDirtyRegion);Assert.True(a.AsSpan(0,384*384*4).SequenceEqual(b.AsSpan(0,384*384*4)));
  }
 }
}

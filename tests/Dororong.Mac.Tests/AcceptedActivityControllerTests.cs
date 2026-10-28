using System.Reflection;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.Core.Platforms;
using Dororong.Mac.Rendering;
using Dororong.Portable.Desktop;
namespace Dororong.Mac.Tests;
public sealed class AcceptedActivityControllerTests {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 [Fact] public void ProductionControllerOptsIntoQuestionAndIdleActivities(){
  using var c=new R104DesktopController(new(400,300),new R());
  var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],[]),SceneHealth.Fresh);
  c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
  var loop=typeof(R104DesktopController).GetField("loop",F)!.GetValue(c)!;
  var brain=loop.GetType().GetField("_brain",F)!.GetValue(loop)!;
  var tuning=(BehaviorTuning)brain.GetType().GetFields(F).Single(f=>f.FieldType==typeof(BehaviorTuning)).GetValue(brain)!;
  Assert.Equal(TimeSpan.FromMinutes(1),tuning.DoroQuestionDelay);Assert.True(tuning.IdleActivitiesEnabled);
  Assert.Equal(TimeSpan.FromMinutes(3),tuning.SleepDelay);
 }
 [AvaloniaFact] public void BubbleDrawsAtBothScalesWithoutBitmapAllocationOrPetHitAreaChange(){
  using var s=new R104Surface();s.Present(new(),1,1);s.Measure(new(384,384));s.Arrange(new(0,0,384,384));
  var original=s.ActivePixels.ToArray();
  foreach(var scale in new[]{1d,1.25,1.5})foreach(var tail in new[]{false,true}){
   s.Present(new(),scale,1);s.SetQuestion((true,tail,1));
   using var image=new RenderTargetBitmap(new(384,384),new(96,96));image.Render(s);
   if(Environment.GetEnvironmentVariable("DORO_PORT_PREVIEW") is {} output){
    Directory.CreateDirectory(output);image.Save(Path.Combine(output,$"question-{scale}-{tail}.png"));
   }
   Assert.Equal(1,s.BitmapAllocations);
   Assert.False(s.HitOpaque(new(56*scale+(tail?2:78)*scale+30*scale,56*scale+20*scale)));
   s.SetQuestion(default);
  }
  s.Present(new(),1,1);Assert.Equal(original,s.ActivePixels.ToArray());
 }
 private sealed class R:IRandomSource{public double NextUnit()=>.5;}
}

using System.Reflection;
using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Mac.Rendering;
using Dororong.Portable.Desktop;
namespace Dororong.Mac.Tests;

public sealed class SeatedContinuityTests {
 [Theory][InlineData(100)][InlineData(125)][InlineData(150)]
 public void UnavailableQueriesPreserveSeatedReach(int size){
  using var c=new R104DesktopController(new(400,300),new R(),size);c.SetAutomaticSleep(false);
  var errors=new List<Exception>();c.Faulted+=errors.Add;
  var desktop=new DesktopScene(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],[]);
  var scene=new SceneSnapshot(desktop,SceneHealth.Fresh);
  void Tick(PointerSample pointer)=>c.Advance(TimeSpan.FromMilliseconds(20),pointer,false,scene);
  for(var i=0;i<100;i++)Tick(PointerSample.Unavailable);
  var floor=c.Position.Y+c.Geometry!.Value.Contact.SoleY;
  c.ToggleSit();for(var i=0;i<70;i++)Tick(PointerSample.Unavailable);
  var p=(DororongPresenter)typeof(R104DesktopController).GetField("presenter",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(c)!;
  var origin=c.Position;var paw=0;
  for(var i=0;i<100;i++){
   scene=new(desktop,i is >=16 and <22 or >=40 and <80?SceneHealth.Unavailable:SceneHealth.Fresh);
   Tick(new(true,origin+new PointD(30*c.Scale,85*c.Scale)));
   Assert.Equal(321,p.Face.MotionFrame);
   Assert.Equal(-1,p.Face.BodyFrame);
   if(p.Face.SeatedPawAmount>0)paw++;
   Assert.InRange(Math.Abs(c.Position.Y+c.Geometry!.Value.Contact.SoleY-floor),0,.01);
  }
  Assert.True(paw>40);Assert.Empty(errors);
 }
 private sealed class R:IRandomSource{public double NextUnit()=>.5;}
}

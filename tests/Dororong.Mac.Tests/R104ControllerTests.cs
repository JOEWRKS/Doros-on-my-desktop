using Avalonia.Headless.XUnit;
using Dororong.Mac.Rendering;
using Dororong.Mac.Product;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;
namespace Dororong.Mac.Tests;
public sealed class R104ControllerTests {
 [Theory][InlineData(100)][InlineData(125)][InlineData(150)]
 public void RuntimeSizeCyclesKeepWorldFootAndDoNotFault(int initial) {
  using var c=new R104DesktopController(new(400,300),new R(),initial);c.SetAutomaticSleep(false);
  var failures=new List<Exception>();c.Faulted+=failures.Add;var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],[]),SceneHealth.Fresh);
  for(var i=0;i<90;i++)c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
  Assert.Empty(failures);Assert.True(c.CanResize);
  foreach(var size in new[]{100,125,150,125,100,150}){
   var before=c.Geometry!.Value.Contact;var sole=c.Position.Y+before.SoleY;var center=c.Position.X+(before.Left+before.Right)/2;
   Assert.True(c.RequestSize(size));c.Advance(TimeSpan.Zero,PointerSample.Unavailable,false,scene);
   Assert.Equal(size,c.SizePercent);Assert.InRange(Math.Abs(c.Position.Y+c.Geometry!.Value.Contact.SoleY-sole),0,.01);
   var after=c.Geometry!.Value.Contact;Assert.InRange(Math.Abs(c.Position.X+(after.Left+after.Right)/2-center),0,.01);
  }
  Assert.Empty(failures);
 }
 [AvaloniaFact] public void MenuSliderCallsContinuousSizeWithoutDismiss(){
  var sizes=new List<int>();using var menu=new PetMenuPanel(false,false,true,()=>{},()=>{},_=>true,()=>{},()=>{},size:100,resize:p=>{sizes.Add(p);return true;});
  var dismissed=false;menu.Dismiss+=()=>dismissed=true;
  foreach(var n in new[]{101,102,125,150,149,100})menu.SizeSlider.Value=n;
  Assert.Equal(new[]{101,102,125,150,149,100},sizes);Assert.False(dismissed);
 }
 [AvaloniaFact] public void RejectedResizeRestoresSlider(){
  using var menu=new PetMenuPanel(false,false,true,()=>{},()=>{},_=>true,()=>{},()=>{},size:125,resize:_=>false);
  menu.SizeSlider.Value=150;Assert.Equal(125,menu.SizeSlider.Value);
 }
 [Fact] public void HeadReleaseFallsBackToFloorAndDensityChangeKeepsInputValid(){
  using var c=new R104DesktopController(new(400,300),new R(),150);c.SetAutomaticSleep(false);
  var errors=new List<Exception>();c.Faulted+=errors.Add;
  var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],[]),SceneHealth.Fresh);
  for(var i=0;i<90;i++)c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
  c.Density=2;c.Advance(TimeSpan.Zero,PointerSample.Unavailable,false,scene);
  Assert.Equal(768,c.Extent);
  var p=(Dororong.App.Controls.DororongPresenter)typeof(R104DesktopController).GetField("presenter",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.GetValue(c)!;
  var local=p.HighResolutionFaceToPresenter(new(37.5,28.25));var cursor=c.Position+new PointD(local.X,local.Y);
  Assert.True(c.TryBeginInteraction(cursor));
  c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor),true,scene);
  for(var i=0;i<22;i++)c.Advance(TimeSpan.FromMilliseconds(16),new(true,cursor+new PointD(Math.Sin(i*.4)*55,-i*12)),true,scene);
  Assert.True(c.Capturing);
  for(var i=0;i<150;i++)c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);
  Assert.False(c.Capturing);Assert.Empty(errors);
  Assert.InRange(Math.Abs(c.Position.Y+c.Geometry!.Value.Contact.SoleY-1080),0,1);
 }
 private sealed class R:IRandomSource{public double NextUnit()=>.5;}
}

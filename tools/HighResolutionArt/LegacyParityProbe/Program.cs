using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

internal static class Program
{
 [STAThread] static void Main(string[] args)
 {
  var folder=Path.GetFullPath(args.Single());Directory.CreateDirectory(folder);
  var rows=new List<object>();var flags=BindingFlags.Instance|BindingFlags.NonPublic;
  var motion=new PlatformMotion();typeof(PlatformMotion).GetField("extremeLanding",flags)!.SetValue(motion,true);
  var set=typeof(PlatformMotion).GetMethod("Set",flags)!;
  foreach(var ms in new[]{0,20,35,65,100,1100,1180,1280,1350,1600,1680,1800})
  {
   var p=new DororongPresenter();p.Render(new(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null),DirectInteractionSnapshot.None);
   var canvas=new Canvas{Width=256,Height=256};canvas.Children.Add(p);Canvas.SetLeft(p,56);Canvas.SetTop(p,56);Layout(canvas);
   var before=p.MeasurePlatformGeometry()!.Value;
   if(ms==0)Save((BitmapSource)((Image)p.FindName("DororongImage")).Source,Path.Combine(folder,"legacy-source.png"));
   typeof(PlatformMotion).GetField("phaseSeconds",flags)!.SetValue(motion,ms/1000d);var pose=(PlatformPose)set.Invoke(motion,new object[]{PlatformPhase.Landing,new PointD(0,0)})!;
   p.ApplyPlatformPose(pose,before.Contact.SoleY+pose.Position.Y);Layout(canvas);
   var after=p.MeasurePlatformGeometry()!.Value;
   foreach(var scale in new[]{1d,1.25,1.5}){var bitmap=new RenderTargetBitmap((int)(256*scale),(int)(256*scale),96*scale,96*scale,PixelFormats.Pbgra32);bitmap.Render(canvas);Save(bitmap,Path.Combine(folder,$"legacy-{ms}-{scale:F2}.png"));}
   rows.Add(new{ms,pose.Squash,pose.LegSpread,lift=-pose.Position.Y,before,after});canvas.Children.Clear();
  }
  var swings=new List<object>();
  foreach(var direction in new[]{-1,1})
  {
   var s=new HeadSwingSession();var pointer=new PointD(400,300);s.Advance(TimeSpan.Zero,pointer,true,DirectInteractionPhase.BodyDragHold,0);
   var frames=new List<object>();double angle=0;
   for(var i=0;i<=64;i++)
   {
    // Accelerate, stop with inertia, reverse. Same absolute screen input both ways.
    var dx=i==0?0:i<=16?direction*24:i<=40?0:direction*-12;
    pointer+=new PointD(dx,0);var dt=i==0?0:16;
    angle=s.Advance(TimeSpan.FromMilliseconds(dt),pointer,true,DirectInteractionPhase.BodyDragHold,0);
    frames.Add(new{i,ms=i*16,dt,x=pointer.X,y=pointer.Y,angle});
   }
   var release=new List<object>();for(var ms=0;ms<=440;ms+=10){var progress=Math.Min(1,ms/180d);var a=s.Advance(TimeSpan.FromMilliseconds(10),pointer,true,DirectInteractionPhase.BodyDragSettle,progress);release.Add(new{ms,progress,angle=a});}
   swings.Add(new{direction,frames,release});
  }
  File.WriteAllText(Path.Combine(folder,"probe.json"),JsonSerializer.Serialize(new{method="Actual offscreen WPF DororongPresenter + actual HeadSwingSession. Not desktop capture or physical input.",landing=rows,swings},new JsonSerializerOptions{WriteIndented=true,IncludeFields=true}));
  Console.WriteLine($"Actual presenter: {rows.Count} landing states x3 scales; HeadSwingSession: 2 pointer traces and releases.");
 }
 static void Layout(FrameworkElement c){c.Measure(new(256,256));c.Arrange(new Rect(0,0,256,256));c.UpdateLayout();}
 static void Save(BitmapSource bitmap,string path){var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(bitmap));using var f=File.Create(path);e.Save(f);}
}

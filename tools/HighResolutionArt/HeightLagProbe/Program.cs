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
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static object Field(object o,string n)=>o.GetType().GetField(n,Flags)!.GetValue(o)!;
 static readonly PetSnapshot Pet=new(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
 [STAThread] static void Main(string[] args)
 {
  var dir=Path.GetFullPath(args.Single());Directory.CreateDirectory(dir);
  var falls=new List<object>();
  foreach(var height in new[]{36d,240,799,800})
  {
   var motion=new PlatformMotion();var contact=new FootContact(20,70,87,4);
   var floor=new PlatformSurface(new(1,1,1),1,PlatformKind.Floor,-1000,1000,height+87,default);
   var position=new PointD(0,0);motion.Reset(position);double? hit=null;var rows=new List<object>();
   for(var ms=0;ms<=3000;ms+=20)
   {
    var pose=motion.Advance(new(TimeSpan.FromMilliseconds(ms==0?0:20),position,position,contact,new[]{floor},false,true){Monitors=new[]{new DesktopMonitor(1,new(0,0,1000,1000))}});
    position=pose.Position;if(pose.Phase==PlatformPhase.Landing&&hit is null)hit=ms;
    var age=hit is null?-1:ms-hit.Value;
    var selected=(ms%80==0&&hit is null)||(age>=0&&(age<=160||age%80==0))||pose.Phase==PlatformPhase.Supported;
    rows.Add(new{ms,y=position.Y,phase=pose.Phase.ToString(),squash=pose.Squash,spread=pose.LegSpread,extreme=pose.IsExtremeLanding,age,selected});
    if(selected)
    {
     var p=new DororongPresenter();p.Render(Pet,DirectInteractionSnapshot.None);var c=CanvasFor(p);var before=p.MeasurePlatformGeometry()!.Value;
     // Camera follows the host for readable equal-size local comparisons. World Y stays in JSON.
     p.ApplyPlatformPose(pose,before.Contact.SoleY);Layout(c);
     foreach(var scale in new[]{1d,1.25,1.5})Save(c,scale,Path.Combine(dir,$"fall-{height:0}-{ms}-{scale:F2}.png"));
    }
    if(hit is not null&&pose.Phase==PlatformPhase.Supported)break;
   }
   falls.Add(new{height,screenHeight=1000,hit,rows});
  }
  var lag=new List<object>();var presenter=new DororongPresenter();presenter.Render(Pet,DirectInteractionSnapshot.None);var canvas=CanvasFor(presenter);
  var image=(Image)presenter.FindName("DororongImage");var grabbed=image.TranslatePoint(new Point(37.5,28.25),presenter);var press=Pet.Position+new PointD(grabbed.X,grabbed.Y);var pointer=press;
  var swing=new HeadSwingSession();var lastAngle=0d;var releasePhysics=new PlatformMotion();releasePhysics.Reset(default);var releasePosition=new PointD(0,0);
  var releaseFloor=new PlatformSurface(new(1,1,1),1,PlatformKind.Floor,-1000,1000,123,default);
  for(var ms=0;ms<=1280;ms+=20)
  {
   var settling=ms>=600;var dx=ms==0?0:ms<=200?12:ms<=360?0:ms<600?-8:0;pointer+=new PointD(dx,ms>0&&ms<=200?2:0);
   var phase=settling?DirectInteractionPhase.BodyDragSettle:DirectInteractionPhase.BodyDragHold;
   var release=settling?Math.Min(1,(ms-600)/180d):0;
   var angle=swing.Advance(TimeSpan.FromMilliseconds(ms==0?0:20),pointer,true,phase,release);
   var direct=new DirectInteractionSnapshot(DirectInteractionTarget.Body,phase,press,pointer,1,0,!settling){ReleaseProgress=release,HeadSwingDegrees=angle};
   if(ms>=780)direct=DirectInteractionSnapshot.None;
   presenter.Render(Pet,direct,TimeSpan.FromMilliseconds(ms==0?0:20));Layout(canvas);
   PlatformPose? releasePose=null;
   if(settling){releasePose=releasePhysics.Advance(new(TimeSpan.FromMilliseconds(ms==600?0:20),releasePosition,releasePosition,new(20,70,87,4),new[]{releaseFloor},false,true){Monitors=new[]{new DesktopMonitor(1,new(0,0,1000,1000))}});releasePosition=releasePose.Value.Position;var contactNow=presenter.MeasurePlatformGeometry()!.Value;presenter.ApplyPlatformPose(releasePose,contactNow.Contact.SoleY);Layout(canvas);}
   var surrounding=Field(presenter,"_headSurrounding");var motion=(SurroundingPullMotion)Field(surrounding,"_motion");
   var src=ms>=780?(BitmapSource)image.Source:settling?((Func<double,BitmapSource>)Field(presenter,"_headRecovery"))(release):(BitmapSource)Field(presenter,"_lastHeadSource");
   var center=SurroundingPullRenderer.PinkCenter(PremultipliedFrame.From(src).Pixels);var grab=ms>=780?new PointD(37.5,28.25):HeadPullAnchoring.SourcePoint(src,(CapturedHeadAnchor)Field(presenter,"_headAnchor"));
   var displayed=ms>=780?0:(double)Field(presenter,"_lastHeadSwingAngle");if(!settling)lastAngle=displayed;
   var radians=displayed*Math.PI/180;PointD Local(PointD v)=>new(v.X*Math.Cos(radians)+v.Y*Math.Sin(radians),-v.X*Math.Sin(radians)+v.Y*Math.Cos(radians));
   var near=Local(motion.Current);var far=Local(motion.Far);
   var fieldChecks=new List<object>();foreach(var q in new[]{new PointD(37.5,28.25),new(30,50),new(55,72),new(50,87)})fieldChecks.Add(new{p=q,d=SurroundingPullRenderer.HeadDisplacement(q,center,grab,near,far)});
   lag.Add(new{ms,settling,release,pointer,near=motion.Current,far=motion.Far,localNear=near,localFar=far,center,grab,displayed,lastAngle,platform=releasePose,fieldChecks,selected=ms%40==0});
   if(ms%40==0)foreach(var scale in new[]{1d,1.25,1.5})Save(canvas,scale,Path.Combine(dir,$"lag-{ms}-{scale:F2}.png"));
  }
  var shortDrops=new List<object>();foreach(var distance in new[]{0d,12,36})foreach(var ms in new[]{0,100,180,220,260,340,440}){var r=HeadLandingMotion.Sample(default,distance,1,ms,new(-1000,-1000,2000,2000),new(144,144));shortDrops.Add(new{distance,ms,y=r.WindowPosition.Y,squash=r.Compression});}
  File.WriteAllText(Path.Combine(dir,"probe.json"),JsonSerializer.Serialize(new{method="Actual PlatformMotion.Advance / Presenter / HeadSurroundingPresentation. Offscreen, not desktop input. Fall images use a following camera; world position is in rows.",falls,lag,shortDrops},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("Saved actual height traces, lag trace and WPF samples.");
 }
 static Canvas CanvasFor(DororongPresenter p){var c=new Canvas{Width=256,Height=256};c.Children.Add(p);Canvas.SetLeft(p,56);Canvas.SetTop(p,56);Layout(c);return c;}
 static void Layout(FrameworkElement c){c.Measure(new(256,256));c.Arrange(new Rect(0,0,256,256));c.UpdateLayout();}
 static void Save(Visual v,double scale,string file){var b=new RenderTargetBitmap((int)(256*scale),(int)(256*scale),96*scale,96*scale,PixelFormats.Pbgra32);b.Render(v);var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(b));using var f=File.Create(file);e.Save(f);}
}

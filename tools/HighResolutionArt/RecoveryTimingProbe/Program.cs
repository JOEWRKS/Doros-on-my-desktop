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

internal static class Program
{
 [STAThread] static void Main(string[] args)
 {
  var folder=Path.GetFullPath(args.Single());Directory.CreateDirectory(folder);
  _=new DororongPresenter();
  var canonical=new BitmapImage(new Uri("pack://application:,,,/Dororong.App;component/Assets/dororong-canonical.png"));
  var bank=new SuppliedBodyDragFrames(PremultipliedFrame.From(canonical));
  var positions=typeof(SuppliedBodyDragFrames).GetField("_positions",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(bank)!;
  var lookup=positions.GetType().GetMethod("TryGetValue")!;
  var rows=new List<object>();
  foreach(var start in new[]{1d,.4})
  {
   var source=bank.Sample(start);var recovery=bank.CreateRecovery(source,canonical);
   var times=Enumerable.Range(0,19).Select(i=>i*10d).Concat(Enumerable.Range(0,9).Select(i=>i*22.5)).Distinct().Order().ToArray();
   foreach(var ms in times)
   {
    var p=ms/180d;var frame=recovery(p);object?[] parameters={frame,null};
    var found=(bool)lookup.Invoke(positions,parameters)!;
    var actualPosition=found?(double?)parameters[1]!.GetType().GetProperty("Value")!.GetValue(parameters[1]):null;
    var expectedRemaining=start*7-p*(start*7+1);
    if(expectedRemaining>=0 && (actualPosition is null||Math.Abs(actualPosition.Value-expectedRemaining/7)>1e-9))throw new Exception("Actual bank position mismatch");
    rows.Add(new{start,ms,release=p,actualPosition,bridge=actualPosition is null});
    // Actual recovery source rendered by WPF at a common96-DIP image origin.
    var image=new Image{Source=frame,Width=96,Height=96,Stretch=Stretch.Fill};RenderOptions.SetBitmapScalingMode(image,BitmapScalingMode.HighQuality);
    var c=new Canvas{Width=256,Height=256};c.Children.Add(image);Canvas.SetLeft(image,80);Canvas.SetTop(image,80);Layout(c);
    foreach(var scale in new[]{1d,1.25,1.5})Save(c,scale,Path.Combine(folder,$"body-{start:F1}-{ms:0.###}-{scale:F2}.png"));
   }
  }
  // Actual full presenter, neutral capture: this includes its real registration,
  // canonical bridge, render ownership and short-drop impact presentation.
  var presenter=new DororongPresenter();var pet=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
  presenter.Render(pet,DirectInteractionSnapshot.None);var canvas=new Canvas{Width=256,Height=256};canvas.Children.Add(presenter);Canvas.SetLeft(presenter,56);Canvas.SetTop(presenter,56);Layout(canvas);
  var grabbed=((Image)presenter.FindName("DororongImage")).TranslatePoint(new Point(37.5,28.25),presenter);var press=pet.Position+new PointD(grabbed.X,grabbed.Y);
  var held=new DirectInteractionSnapshot(DirectInteractionTarget.Body,DirectInteractionPhase.BodyDragHold,press,press,1,0,true);
  presenter.Render(pet,held,TimeSpan.Zero);Layout(canvas);
  for(var ms=0;ms<=440;ms+=20)
  {
   var fall=HeadLandingMotion.Sample(new(0,0),36,1,ms,new(-1000,-1000,2000,2000),new(144,144));
   var release=held with {Phase=DirectInteractionPhase.BodyDragSettle,RequiresCapture=false,ReleaseProgress=Math.Min(1,ms/180d),HeadLanding=fall};
   presenter.Render(pet,release,TimeSpan.Zero);Canvas.SetTop(presenter,56+fall.WindowPosition.Y);Layout(canvas);
   foreach(var scale in new[]{1d,1.25,1.5})Save(canvas,scale,Path.Combine(folder,$"presenter-{ms}-{scale:F2}.png"));
  }
  File.WriteAllText(Path.Combine(folder,"timing.json"),JsonSerializer.Serialize(new{method="Actual SuppliedBodyDragFrames.CreateRecovery positions and WPF outputs; neutral DororongPresenter short release. Offscreen, not desktop input.",rows},new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine($"Actual recovery: {rows.Count} timing rows; {rows.Count*3} body WPF images;69 presenter WPF images.");
 }
 static void Layout(FrameworkElement c){c.Measure(new(256,256));c.Arrange(new Rect(0,0,256,256));c.UpdateLayout();}
 static void Save(Visual visual,double scale,string path){var b=new RenderTargetBitmap((int)(256*scale),(int)(256*scale),96*scale,96*scale,PixelFormats.Pbgra32);b.Render(visual);var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(b));using var f=File.Create(path);e.Save(f);}
}

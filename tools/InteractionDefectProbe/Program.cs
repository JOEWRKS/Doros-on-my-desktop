using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Text.Json;
using Dororong.App.Controls;
internal static class Program {
 [STAThread] static void Main(string[] args){
  new Application();
  if(args[0]=="--compare"){Compare(args[1],args[2],args.Contains("--closed-ink"));return;}
  var output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);
  var raster=new HighResolutionGrabRaster();var bank=new HighResolutionLocomotion();
  var head=(float[])typeof(HighResolutionGrabRaster).GetField("head",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(raster)!;
  var cleanMethod=typeof(HighResolutionGrabRaster).GetMethod("ClosedHead",BindingFlags.Instance|BindingFlags.NonPublic);
  var closed=cleanMethod is null?bank.Get(0):(float[])cleanMethod.Invoke(raster,null)!;
  Save("head-open",Texture(head));Save("head-closed",Texture(closed));
  var detail=new DrawingVisual();RenderOptions.SetBitmapScalingMode(detail,BitmapScalingMode.NearestNeighbor);
  using(var g=detail.RenderOpen()){
   g.DrawRectangle(Brushes.White,null,new Rect(0,0,1000,600));
   g.DrawImage(new CroppedBitmap(Texture(head),new Int32Rect(25,90,90,60)),new Rect(0,0,500,333));
   g.DrawImage(new CroppedBitmap(Texture(closed),new Int32Rect(25,90,90,60)),new Rect(500,0,500,333));
  }
  var hd=new RenderTargetBitmap(1000,600,96,96,PixelFormats.Pbgra32);hd.Render(detail);Save("head-detail",hd);
  var cases=new List<(string,HighResolutionFacePose)>{
   ("idle",HighResolutionFacePose.Neutral),
   ("closed",HighResolutionFacePose.Neutral with{Closed=true}),
   ("eat",HighResolutionFacePose.Neutral with{Closed=true,MouthAge=.2}),
   ("perch",HighResolutionFacePose.Neutral with{MotionFrame=HighResolutionLocomotion.Perch})
  };
  foreach(var paw in new[]{1,2})foreach(var y in new[]{0d,8,18})foreach(var angle in new[]{-35d,0,35})
   cases.Add(($"paw-{paw}-y{y}-a{angle}",HighResolutionFacePose.Neutral with{MotionFrame=HighResolutionLocomotion.Perch,Paw=paw,PawY=y,PawAngle=angle}));
  foreach(var (name,face) in cases)Save(name,raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,1.5,face,bodyScale:1.5));
  var sheet=new DrawingVisual();using(var g=sheet.RenderOpen()){
   for(var i=0;i<cases.Count;i++){
    var b=new BitmapImage(new Uri(Path.Combine(output,cases[i].Item1+".png")));
    var crop=new CroppedBitmap(b,new Int32Rect(110,125,170,165));
    var x=i%6*260;var y=i/6*285;
    g.DrawRectangle(i%2==0?Brushes.White:new SolidColorBrush(Color.FromRgb(35,38,42)),null,new Rect(x,y,260,285));
    g.DrawImage(crop,new Rect(x,y+25,255,247.5));
    g.DrawText(new FormattedText(cases[i].Item1,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),15,i%2==0?Brushes.Black:Brushes.White,1),new Point(x+4,y+3));
   }
  }var shot=new RenderTargetBitmap(1560,1140,96,96,PixelFormats.Pbgra32);shot.Render(sheet);Save("sheet",shot);
  if(args.Contains("--full")){
   var hashes=new Dictionary<string,string>();
   foreach(var scale in new[]{1d,1.1,1.25,1.35,1.5}){
    var frames=new List<(string,BitmapSource)>();
    void Add(string name,HighResolutionFacePose pose){
     var bitmap=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,pose,bodyScale:scale);
     var bytes=new byte[bitmap.PixelWidth*bitmap.PixelHeight*4];bitmap.CopyPixels(bytes,bitmap.PixelWidth*4,0);
     var key=$"s{scale:F2}-{name}";Save(key,bitmap);hashes[key]=Convert.ToHexString(SHA256.HashData(bytes));
     frames.Add((name,bitmap));
    }
    Add("idle",HighResolutionFacePose.Neutral);
    foreach(var t in new[]{0d,.2,.48,.8,1.2,1.55})Add($"eat-{t:F2}",HighResolutionFacePose.Neutral with{Closed=true,MouthAge=t});
    foreach(var paw in new[]{1,2}){
     Add($"max-{paw}",HighResolutionFacePose.Neutral with{MotionFrame=HighResolutionLocomotion.Perch,Paw=paw,PawY=18});
     // Exact controller tap envelope, sampled over the complete 600 ms.
     for(var i=0;i<=36;i++){
      var u=i/36d;var angle=22*Math.Sin(u*6*Math.PI)*Math.Sin(u*Math.PI);
      Add($"tap-{paw}-{i:D2}",HighResolutionFacePose.Neutral with{MotionFrame=HighResolutionLocomotion.Perch,Paw=paw,PawAngle=angle});
     }
    }
    var review=new DrawingVisual();using(var g=review.RenderOpen()){
     for(var i=0;i<frames.Count;i++){
      var x=i%10*155;var y=i/10*185;
      g.DrawRectangle(i%2==0?Brushes.White:Brushes.DimGray,null,new Rect(x,y,155,185));
      var bmp=frames[i].Item2;
      var crop=new CroppedBitmap(bmp,new Int32Rect((int)(75*scale),(int)(82*scale),(int)(105*scale),(int)(110*scale)));
      g.DrawImage(crop,new Rect(x,y+20,152,159));
      g.DrawText(new FormattedText(frames[i].Item1,System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),11,i%2==0?Brushes.Black:Brushes.White,1),new Point(x+3,y+2));
     }
    }
    var page=new RenderTargetBitmap(1550,185*((frames.Count+9)/10),96,96,PixelFormats.Pbgra32);page.Render(review);Save($"review-s{scale:F2}",page);
   }
   File.WriteAllText(Path.Combine(output,"hashes.json"),JsonSerializer.Serialize(hashes,new JsonSerializerOptions{WriteIndented=true}));
  }
  void Save(string name,BitmapSource source){using var stream=File.Create(Path.Combine(output,name+".png"));var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(source));e.Save(stream);}
 }
 static BitmapSource Texture(float[] pixels){var b=Array.ConvertAll(pixels,p=>(byte)Math.Clamp(Math.Round(p),0,255));return BitmapSource.Create(225,225,96,96,PixelFormats.Pbgra32,null,b,900);}
 static void Compare(string before,string after,bool closedInk){
  var hashes=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(after,"hashes.json")))!;
  var rows=new List<object>();var unexpected=0;var identical=0;
  foreach(var key in hashes.Keys){
   var a=new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(before,key+".png"))));
   var b=new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(after,key+".png"))));
   var ba=new byte[a.PixelWidth*a.PixelHeight*4];var bb=new byte[ba.Length];
   a.CopyPixels(ba,a.PixelWidth*4,0);b.CopyPixels(bb,b.PixelWidth*4,0);
   var scale=double.Parse(key.Substring(1,4),System.Globalization.CultureInfo.InvariantCulture);
   var count=0;var outside=0;
   for(var i=0;i<ba.Length;i+=4){
    if(ba.AsSpan(i,4).SequenceEqual(bb.AsSpan(i,4)))continue;
    count++;var x=(i/4%a.PixelWidth+.5)/scale-80;var y=(i/4/a.PixelWidth+.5)/scale-80;
    var allowed=key.Contains("-eat-")?(closedInk?x>=11&&x<=51&&y>=30&&y<=60:x>=17&&x<=22&&y>=44&&y<=49):
     !closedInk&&(key.Contains("-tap-")||key.Contains("-max-"))&&y>=68;
    if(!allowed)outside++;
   }
   if(count==0)identical++;unexpected+=outside;rows.Add(new{key,changed=count,outside});
  }
  var report=new{frames=rows.Count,identical,unexpected,rows};
  File.WriteAllText(Path.Combine(after,"comparison.json"),JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine($"Frames {rows.Count}; identical {identical}; outside allowed defect regions {unexpected}");
  var selected=new[]{("s1.50-eat-0.20",new Int32Rect(135,170,60,60)),
   ("s1.50-max-2",new Int32Rect(150,210,80,90)),("s1.50-tap-2-15",new Int32Rect(165,210,65,75))};
  var visual=new DrawingVisual();using(var g=visual.RenderOpen()){
   for(var row=0;row<3;row++)for(var side=0;side<2;side++){
    var x=side*300;var y=row*300;
    g.DrawRectangle(row==1?Brushes.DimGray:Brushes.White,null,new Rect(x,y,300,300));
    var bitmap=new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(side==0?before:after,selected[row].Item1+".png"))));
    g.DrawImage(new CroppedBitmap(bitmap,selected[row].Item2),new Rect(x+15,y+35,270,255));
    g.DrawText(new FormattedText($"{(side==0?"r98":"r99")} — {new[]{"closed fringe","maximum paw","clicked paw joint"}[row]}",System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),16,row==1?Brushes.White:Brushes.Black,1),new Point(x+12,y+8));
   }
  }
  var comparison=new RenderTargetBitmap(600,900,96,96,PixelFormats.Pbgra32);comparison.Render(visual);
  using var file=File.Create(Path.Combine(after,"before-after.png"));var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(comparison));encoder.Save(file);
  if(closedInk){
   var scales=new[]{1d,1.1,1.25,1.35,1.5};var scaleView=new DrawingVisual();
   using(var g=scaleView.RenderOpen())for(var k=0;k<scales.Length;k++)for(var side=0;side<2;side++){
    var scale=scales[k];var x=k*250;var y=side*280;
    g.DrawRectangle(k%2==0?Brushes.White:Brushes.DimGray,null,new Rect(x,y,250,280));
    var bitmap=new BitmapImage(new Uri(Path.GetFullPath(Path.Combine(side==0?before:after,$"s{scale:F2}-eat-0.80.png"))));
    var crop=new CroppedBitmap(bitmap,new Int32Rect((int)(85*scale),(int)(110*scale),(int)(70*scale),(int)(50*scale)));
    g.DrawImage(crop,new Rect(x+10,y+45,crop.PixelWidth*2,crop.PixelHeight*2));
    g.DrawText(new FormattedText($"{(side==0?"r99":"r100")} / {scale:F2}x — detail 2x",System.Globalization.CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),14,k%2==0?Brushes.Black:Brushes.White,1),new Point(x+8,y+10));
   }
   var page=new RenderTargetBitmap(1250,560,96,96,PixelFormats.Pbgra32);page.Render(scaleView);
   using var stream=File.Create(Path.Combine(after,"closed-ink-scales.png"));var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(page));e.Save(stream);
  }
 }
}

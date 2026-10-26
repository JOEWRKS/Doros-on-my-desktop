using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using System.Security.Cryptography;
using System.Text.Json;

internal static class Program {
 [STAThread] static void Main(string[] args){
  var output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);
  if(args.Length>2&&args[2]=="audit"){Audit(output,Path.GetFullPath(args[1]));return;}
  if(args.Length>1&&args[1]=="junction-trace"){
   var posed=new HighResolutionLocomotion().BorrowBody(HighResolutionLocomotion.Sit(1),1.5);
   var raster=new HighResolutionGrabRaster();
   var body=(float[])typeof(HighResolutionGrabRaster).GetMethod("Body",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)!.Invoke(raster,[0d])!;
   body=HighResolutionBodyInk.AtScale(body,375,1.5);
   var warp=new HighResolutionGrabRaster.ChuckleWarp(HighResolutionFacePose.Neutral with{ChuckleCover=1,GroomingPaw=true,GroomSweep=1},1.5);
   var rows=new List<object>();Span<double> pixel=stackalloc double[4];Span<double> scratch=stackalloc double[4];
   for(var row=174;row<=184;row++)for(var col=66;col<=76;col++){
    var x=(col+.5)*96/225;var y=(row+.5)*96/225;var at=(row*225+col)*4;
    for(var c=0;c<4;c++)pixel[c]=posed[at+c];var before=pixel.ToArray();
    warp.RemovePaw(body,x,y,pixel,scratch);var removed=pixel.ToArray();
    warp.Overlay(body,x,y,pixel,scratch);
    rows.Add(new{x,y,before,afterRemoval=removed,afterOverlay=pixel.ToArray()});
   }
   File.WriteAllText(Path.Combine(output,"junction-trace.json"),JsonSerializer.Serialize(rows));return;
  }
  if(args.Length>1&&args[1]=="trace"){
   var body=new HighResolutionLocomotion().BorrowBody(HighResolutionLocomotion.Sit(1),1.5);
   var warp=new HighResolutionGrabRaster.ChuckleWarp(HighResolutionFacePose.Neutral with{ChuckleCover=1},1.5);
   var rows=new List<object>();Span<double> pixel=stackalloc double[4];Span<double> scratch=stackalloc double[4];
   for(var row=183;row<197;row++)for(var col=69;col<75;col++){
    var x=(col+.5)*96/225;var y=(row+.5)*96/225;var at=(row*225+col)*4;
    if(body[at+3]<100||body[at]/body[at+3]>.6)continue;
    for(var c=0;c<4;c++)pixel[c]=body[at+c];var before=pixel.ToArray();
    warp.RemovePaw(Array.Empty<float>(),x,y,pixel,scratch);var removed=pixel.ToArray();
    warp.Overlay(Array.Empty<float>(),x,y,pixel,scratch);
    rows.Add(new{x,y,before,afterRemoval=removed,afterOverlay=pixel.ToArray()});
   }
   var json=System.Text.Json.JsonSerializer.Serialize(rows,new System.Text.Json.JsonSerializerOptions{WriteIndented=true});File.WriteAllText(Path.Combine(output,"source-line-trace.json"),json);Console.WriteLine(json);return;
  }
  var results=new List<object>();
  var selected=Environment.GetEnvironmentVariable("GROOM_PROBE_FRAMES")?.Split(',').Select(int.Parse).ToHashSet();
  foreach(var size in new[]{100,125,150})foreach(var facing in new[]{FacingDirection.Right,FacingDirection.Left}){
   var p=new DororongPresenter();p.SetArtSize(size);var scale=size/100d;
   var pet=new PetSnapshot(PetState.Idle,default,facing,0,false,null);
   var frames=new List<(int frame,BitmapSource image)>();
   var unique=new HashSet<string>();
   for(var frame=0;frame<=228;frame++){
    if(selected is not null&&!selected.Contains(frame))continue;
    p.Render(pet with{IdleActivity=new(IdleActivityKind.Groom,frame/60d)},DirectInteractionSnapshot.None,TimeSpan.Zero);
    p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
    var b=new RenderTargetBitmap((int)p.Width,(int)p.Height,96,96,PixelFormats.Pbgra32);b.Render(p);
    var name=$"{size}-{facing}-{frame:000}.png";
    var raw=Path.Combine(output,"raw");Directory.CreateDirectory(raw);Save(b,Path.Combine(raw,name));
    // Compare identical PNG encode/decode paths; premultiplied raw bytes versus
    // decoded PNG otherwise differ by rounding at unchanged translucent edges.
    var bytes=Pixels(new BitmapImage(new Uri(Path.Combine(raw,name))));var outside=0;var changed=0;
    if(args.Length>1){
     var old=new BitmapImage(new Uri(Path.Combine(Path.GetFullPath(args[1]),"raw",name)));var original=Pixels(old);
     for(var y=0;y<b.PixelHeight;y++)for(var x=0;x<b.PixelWidth;x++){
      var at=(y*b.PixelWidth+x)*4;var diff=false;for(var c=0;c<4;c++)diff|=bytes[at+c]!=original[at+c];
      if(!diff)continue;changed++;
      var ax=(x+.5)/scale; if(facing==FacingDirection.Left)ax=144-ax;
      var ay=(y+.5)/scale;
      if(ax<51||ax>59||ay<97||ay>114)outside++;
     }
     if(outside>0)throw new InvalidOperationException($"Changes outside cleanup ROI: {name}, {outside}");
    }
    var cropX=facing==FacingDirection.Right?38:70;
    var crop=new CroppedBitmap(b,new Int32Rect((int)(cropX*scale),(int)(86*scale),(int)(36*scale),(int)(33*scale)));
    var hash=Convert.ToHexString(SHA256.HashData(Pixels(crop)));var isUnique=unique.Add(hash);
    // Keep every frame in chronological sheets, including repeated poses.
    frames.Add((frame,b));
    results.Add(new{size,facing=facing.ToString(),frame,hash,isUnique,changed,outside,sole=p.MeasurePlatformContact()!.Value.SoleY});
    if(size==150&&frame is 36 or 48 or 70 or 186){Save(b,Path.Combine(output,$"full-{facing}-{frame}.png"));}
   }
   for(var page=0;page*20<frames.Count;page++){
    const int cell=185;var v=new DrawingVisual();using(var dc=v.RenderOpen()){
     dc.DrawRectangle(Brushes.WhiteSmoke,null,new Rect(0,0,cell*5,cell*4));
     for(var i=0;i<20&&page*20+i<frames.Count;i++){
      var (f,b)=frames[page*20+i];var x=i%5*cell;var y=i/5*cell;
      var cropX=facing==FacingDirection.Right?38:70;
      var crop=new CroppedBitmap(b,new Int32Rect((int)(cropX*scale),(int)(86*scale),(int)(36*scale),(int)(33*scale)));
      RenderOptions.SetBitmapScalingMode(v,BitmapScalingMode.NearestNeighbor);
      dc.DrawImage(crop,new Rect(x+5,y+24,172,157));
      dc.DrawText(new FormattedText($"{size}% {facing} f{f} {f/60d:F3}s",CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),11,Brushes.Black,1),new Point(x+3,y+3));
     }
    }
    var sheet=new RenderTargetBitmap(cell*5,cell*4,96,96,PixelFormats.Pbgra32);sheet.Render(v);Save(sheet,Path.Combine(output,$"crop-{size}-{facing}-{page:00}.png"));
   }
  }
  File.WriteAllText(Path.Combine(output,"frames.json"),JsonSerializer.Serialize(results,new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine(output);
 }
 static void Audit(string output,string baseline){
  var rows=new List<object>();
  foreach(var size in new[]{100,125,150})foreach(var facing in new[]{"Right","Left"})for(var frame=0;frame<=228;frame++){
   var name=$"{size}-{facing}-{frame:000}.png";
   var current=new BitmapImage(new Uri(Path.Combine(output,"raw",name)));
   var before=new BitmapImage(new Uri(Path.Combine(baseline,"raw",name)));
   var a=Pixels(current);var b=Pixels(before);var scale=size/100d;
   var creaseChanges=0;var changed=0;double minY=double.PositiveInfinity,maxY=0,minX=double.PositiveInfinity,maxX=0;
   for(var y=0;y<current.PixelHeight;y++)for(var x=0;x<current.PixelWidth;x++){
    var at=(y*current.PixelWidth+x)*4;var diff=false;for(var c=0;c<4;c++)diff|=a[at+c]!=b[at+c];
    if(!diff)continue;changed++;
    var ax=(x+.5)/scale;if(facing=="Left")ax=144-ax;var ay=(y+.5)/scale;
    minX=Math.Min(minX,ax);maxX=Math.Max(maxX,ax);minY=Math.Min(minY,ay);maxY=Math.Max(maxY,ay);
    if(ax>=51&&ax<=60&&ay>=97&&ay<=105)creaseChanges++;
   }
   if(creaseChanges>0)throw new InvalidOperationException($"Required crease changed: {name}, {creaseChanges}");
   rows.Add(new{size,facing,frame,changed,creaseChanges,minX=changed>0?minX:0,maxX,minY=changed>0?minY:0,maxY});
  }
  File.WriteAllText(Path.Combine(output,"crease-audit.json"),JsonSerializer.Serialize(rows,new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine($"Required upper armpit unchanged in all {rows.Count} frames.");
 }
 static byte[] Pixels(BitmapSource b){var converted=new FormatConvertedBitmap(b,PixelFormats.Pbgra32,null,0);var bytes=new byte[b.PixelWidth*b.PixelHeight*4];converted.CopyPixels(bytes,b.PixelWidth*4,0);return bytes;}
 static void Save(BitmapSource b,string path){var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(b));using var f=File.Create(path);enc.Save(f);}
}

using System.IO;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.Json;
using Dororong.App.Controls;

internal static class Program
{
 [STAThread] static void Main(string[] args){
  var output=Path.GetFullPath(args[0]);Directory.CreateDirectory(output);Directory.CreateDirectory(Path.Combine(output,"raw"));
  var raster=new HighResolutionGrabRaster();var records=new List<object>();
  var motion=HighResolutionLocomotion.Sit(1);var head=Matrix.Identity;var offset=raster.MotionOffset(motion);head.Translate(offset.X,offset.Y);
  var baseline=HighResolutionFacePose.Neutral with{Head=head,MotionFrame=motion};
  foreach(var size in new[]{100,125,150})foreach(var left in new[]{false,true})foreach(var kind in new[]{SeatedReaction.Tap,SeatedReaction.Shake}){
   var scale=size/100d;var inverse=Matrix.Identity;if(left)inverse.ScaleAt(-1,1,48,48);
   var count=kind==SeatedReaction.Tap?63:57;var images=new List<BitmapSource>();var dir=left?"Left":"Right";
   byte[]? first=null;var tracking=args.Length>1&&args[1]=="tracking";var aim=new SeatedInteraction();
   for(var frame=0;frame<=count;frame++){
    var target=new Point(0,73+35*Math.Sin(frame/63d*Math.PI*2-Math.PI/2));
    if(tracking)aim.Advance(1/60d,true,false,target);
    var pose=SeatedInteraction.Pose(baseline,kind,frame/60d,tracking?aim.AimAngle:-.35);
    var full=raster.Render(0,inverse,default,default,default,0,scale,pose,bodyScale:scale);
    var bitmap=new CroppedBitmap(full,new Int32Rect((int)(56*scale),(int)(56*scale),(int)(144*scale),(int)(144*scale)));bitmap.Freeze();
    var pixels=new byte[bitmap.PixelWidth*bitmap.PixelHeight*4];bitmap.CopyPixels(pixels,bitmap.PixelWidth*4,0);
    first??=pixels; if(frame==count&&!pixels.SequenceEqual(first))throw new Exception("Endpoint changed");
    for(var y=0;y<bitmap.PixelHeight;y++)for(var x=0;x<bitmap.PixelWidth;x++)
     if((x<2||y<2||x>=bitmap.PixelWidth-2||y>=bitmap.PixelHeight-2)&&pixels[(y*bitmap.PixelWidth+x)*4+3]!=0)throw new Exception("Clipped");
    Save(bitmap,Path.Combine(output,"raw",$"{kind}-{size}-{dir}-{frame:000}.png"));images.Add(bitmap);
   }
   const int cell=155;var visual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.NearestNeighbor);
   using(var dc=visual.RenderOpen()){
    dc.DrawRectangle(Brushes.WhiteSmoke,null,new Rect(0,0,cell*8,cell*8));
    for(var i=0;i<images.Count;i++){
     var x=i%8*cell;var y=i/8*cell;
     dc.DrawImage(images[i],new Rect(x,y+15,144,144));
     dc.DrawText(new FormattedText($"{kind} {size}% {dir} f{i}",CultureInfo.InvariantCulture,FlowDirection.LeftToRight,new Typeface("Arial"),10,Brushes.Black,1),new Point(x+2,y));
    }
   }
   var sheet=new RenderTargetBitmap(cell*8,cell*8,96,96,PixelFormats.Pbgra32);sheet.Render(visual);Save(sheet,Path.Combine(output,$"{kind}-{size}-{dir}.png"));
   records.Add(new{size,left,kind=kind.ToString(),frames=images.Count,endpointsEqual=true,clipped=false});Console.WriteLine($"{kind} {size} {dir}: {images.Count}");
  }
  File.WriteAllText(Path.Combine(output,"summary.json"),JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));
 }
 static void Save(BitmapSource b,string path){var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(b));using var f=File.Create(path);png.Save(f);}
}

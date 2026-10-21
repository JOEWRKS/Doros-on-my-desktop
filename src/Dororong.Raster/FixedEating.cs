using System.IO.Compression;
namespace Dororong.Raster;
internal static class FixedEating {
 private sealed record Frame(int X,int Y,int Width,int Height,byte[] Bytes);
 private static readonly Lazy<Frame[]> Frames=new(Read);
 private static Frame[] Read(){
  using var stream=typeof(FixedEating).Assembly.GetManifestResourceStream("r104-eating")!;
  using var gzip=new GZipStream(stream,CompressionMode.Decompress);using var reader=new BinaryReader(gzip);
  if(reader.ReadInt32()!=104||reader.ReadInt32()!=103)throw new InvalidDataException();
  var result=new Frame[103];
  for(var i=0;i<result.Length;i++){var x=reader.ReadInt32();var y=reader.ReadInt32();var w=reader.ReadInt32();var h=reader.ReadInt32();if(x<0||y<0||w<0||h<0||x+w>225||y+h>225)throw new InvalidDataException();var data=reader.ReadBytes(w*h*4);if(data.Length!=w*h*4)throw new EndOfStreamException();result[i]=new(x,y,w,h,data);}return result;
 }
 internal static bool Copy(double age,byte[] destination){
  if(!double.IsFinite(age)||age<0)return false;var f=Math.Round(age*60);
  if(f<0||f>102||Math.Abs(age-f/60)>1e-9)return false;
  if(destination.Length!=225*225*4)throw new ArgumentException();
  var frame=Frames.Value[(int)f];Array.Clear(destination);
  for(var y=0;y<frame.Height;y++)Buffer.BlockCopy(frame.Bytes,y*frame.Width*4,destination,((frame.Y+y)*225+frame.X)*4,frame.Width*4);
  return true;
 }
}

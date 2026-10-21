using System.IO.Compression;
namespace Dororong.Raster;
internal static class FixedPaws {
 internal static float[] Load(bool far) {
  using var stream=typeof(FixedPaws).Assembly.GetManifestResourceStream("r104-fixed-paws")!;
  using var gzip=new GZipStream(stream,CompressionMode.Decompress);using var reader=new BinaryReader(gzip);
  if(reader.ReadInt32()!=104||reader.ReadInt32()!=2)throw new InvalidDataException("Invalid paw bank.");
  for(var index=0;index<2;index++){
   var length=reader.ReadInt32();if(length!=375*375*4)throw new InvalidDataException();
   var bytes=reader.ReadBytes(length);if(bytes.Length!=length)throw new EndOfStreamException();
   if(index==(far?1:0)){var result=new float[length];for(var i=0;i<length;i++)result[i]=bytes[i];return result;}
  }throw new InvalidDataException();
 }
}

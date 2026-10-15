using System.IO.Compression;
using System.Runtime.CompilerServices;
using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

internal sealed record PremultipliedFrame(RasterImage Source,byte[] Pixels)
{
    internal int Stride=>Source.PixelWidth*4;
    internal static PremultipliedFrame From(RasterImage source)=>new(source,source.CopyPixels());
}
internal static class HeadPullAnchoring
{
    private sealed record Landmark(PointD Point);
    private static readonly ConditionalWeakTable<RasterImage,Landmark> points=new();
    internal static PointD RegistrationPoint(RasterImage image)=>points.GetValue(image,s=>
    {
        var p=SurroundingPullRenderer.PinkCenter(s.CopyPixels());return new(new(Math.Round(p.X),Math.Round(p.Y)));
    }).Point;
    internal static void RegisterRecoveryPoint(RasterImage image,PointD point)=>points.Add(image,new(point));
}
internal static class HeadPullFrames
{
    private static readonly Lazy<RasterImage[]> frames=new(Load);
    internal static RasterImage Canonical {get;}=RasterImage.Load("dororong-canonical.png");
    internal static RasterImage Sample(double progress)
    {
        if(!double.IsFinite(progress))throw new ArgumentOutOfRangeException(nameof(progress));
        var p=Math.Clamp(progress,0,1)*112;var index=(int)Math.Round(p);
        if(Math.Abs(p-index)<1e-9)return frames.Value[index];
        index=(int)Math.Floor(p);var t=p-index;var a=frames.Value[index];var b=frames.Value[index+1];
        var data=a.CopyPixels();var other=b.Pixels.Span;
        for(var i=0;i<data.Length;i++)data[i]=(byte)Math.Round(data[i]*(1-t)+other[i]*t);
        var result=new RasterImage(96,96,data);var from=HeadPullAnchoring.RegistrationPoint(a);var to=HeadPullAnchoring.RegistrationPoint(b);
        HeadPullAnchoring.RegisterRecoveryPoint(result,new(from.X+(to.X-from.X)*t,from.Y+(to.Y-from.Y)*t));return result;
    }
    internal static Func<double,RasterImage> Recovery(RasterImage captured,double strength)
    {
        var span=Math.Clamp(strength,0,1)*7;
        var bridge=new HeadRecoveryFrame([Sample(0),Canonical],1);
        return progress=>
        {
            if(progress<=0)return captured;
            var remaining=span-Math.Clamp(progress,0,1)*(span+1);
            return remaining>=0?Sample(remaining/7):bridge.Sample(-remaining);
        };
    }
    private static RasterImage[] Load()
    {
        using var input=RasterImage.Resource("head-pull-native.pbgra.gz");using var gzip=new GZipStream(input,CompressionMode.Decompress);using var reader=new BinaryReader(gzip);
        if(new string(reader.ReadChars(4))!="HEAD"||reader.ReadInt32()!=1||reader.ReadInt32()!=96||reader.ReadInt32()!=96||reader.ReadInt32()!=113||
           Convert.ToHexString(reader.ReadBytes(32))!="EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C")throw new InvalidDataException("Invalid accepted head bank.");
        var result=new RasterImage[113];
        for(var i=0;i<result.Length;i++)
        {
            var x=reader.ReadDouble();var y=reader.ReadDouble();if(!double.IsFinite(x)||!double.IsFinite(y))throw new InvalidDataException("Invalid head landmark.");
            var data=reader.ReadBytes(36864);if(data.Length!=36864)throw new InvalidDataException("Truncated head bank.");
            result[i]=new(96,96,data);HeadPullAnchoring.RegisterRecoveryPoint(result[i],new(x,y));
        }
        if(gzip.ReadByte()!=-1)throw new InvalidDataException("Trailing head bank payload.");return result;
    }
}

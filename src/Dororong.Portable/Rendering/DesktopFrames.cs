using System.IO.Compression;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.Portable.Rendering;

public static class DesktopFrames
{
    public const int Padding=96, ViewportSize=144+2*Padding;
    public static RasterImage Pad(RasterImage source)
    {
        var data=new byte[ViewportSize*ViewportSize*4];
        for(var y=0;y<144;y++)source.Pixels.Span.Slice(y*576,576).CopyTo(data.AsSpan(((y+Padding)*ViewportSize+Padding)*4,576));
        return new(ViewportSize,ViewportSize,data);
    }
    internal static RasterImage CropViewport(RasterImage source)
    {
        var data=new byte[144*144*4];
        for(var y=0;y<144;y++)source.Pixels.Span.Slice(((y+Padding)*ViewportSize+Padding)*4,576).CopyTo(data.AsSpan(y*576,576));
        return new(144,144,data);
    }
    private static readonly RasterImage[] perch=LoadPerch();
    internal static RasterImage PerchSource(bool closed)=>perch[closed?1:0];
    private static RasterImage[] LoadPerch()
    {
        using var stream=RasterImage.Resource("perch-native.pbgra.gz");using var zip=new GZipStream(stream,CompressionMode.Decompress);
        using var reader=new BinaryReader(zip);
        if(new string(reader.ReadChars(4))!="PRCH"||reader.ReadInt32()!=1||reader.ReadInt32()!=100||reader.ReadInt32()!=100||reader.ReadInt32()!=2)
            throw new InvalidDataException("Invalid accepted perch bank.");
        var hash=new byte[32];zip.ReadExactly(hash);
        if(Convert.ToHexString(hash)!="EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C")throw new InvalidDataException("Wrong perch baseline.");
        var frames=new RasterImage[2];var data=new byte[40000];
        for(var i=0;i<2;i++){zip.ReadExactly(data);frames[i]=new(100,100,data);}
        if(zip.ReadByte()!=-1)throw new InvalidDataException("Unexpected perch bytes.");return frames;
    }
    public static RasterImage Place(RasterImage source,bool flip,int left=24,int top=24)
    {
        var output=new byte[144*144*4];var bytes=source.Pixels.Span;
        for(var y=0;y<source.PixelHeight;y++)for(var x=0;x<source.PixelWidth;x++)
        {
            var xx=left+(flip?source.PixelWidth-1-x:x);var yy=top+y;
            if(xx>=0&&xx<144&&yy>=0&&yy<144)bytes.Slice((y*source.PixelWidth+x)*4,4).CopyTo(output.AsSpan((yy*144+xx)*4,4));
        }
        return new(144,144,output);
    }
    public static RasterImage Perch(bool flip,bool closed=false,double scale=1,PerchPawSnapshot? paw=null)
    {
        var source=perch[closed?1:0];if(paw is not null)source=PerchPawRenderer.Render(source,paw);
        var placed=Place(source,flip,flip?20:16,30);
        return scale==1?placed:Warp(placed,new(1,0,0,scale,0,94*(1-scale)));
    }
    public static PerchContact Grip(bool flip,double? heldSole=null)
    {
        var frame=perch[0];var top=100;
        for(var i=3;i<frame.Pixels.Length;i+=4)if(frame.Pixels.Span[i]>0)top=Math.Min(top,i/400);
        return new(flip?62:44,flip?92:74,94,30+top){HeldSoleY=heldSole};
    }
    internal static RasterImage Flutter(RasterImage source,double phase)=>new(96,96,ForelegFlutterFrame.Render(source,false,phase,null));
    public static (FootContact Contact,RectD Bounds) Measure(RasterImage frame)
    {
        // Axis-aligned native raster: same128-alpha occupied quads and positive
        // area lower2-DIP band as Windows, without allocating four points/pixel.
        var pixels=frame.Pixels.Span;var top=frame.PixelHeight;var bottom=0;var left=frame.PixelWidth;var right=0;
        for(var y=0;y<frame.PixelHeight;y++)for(var x=0;x<frame.PixelWidth;x++)if(pixels[(y*frame.PixelWidth+x)*4+3]>=128)
        {top=Math.Min(top,y);bottom=Math.Max(bottom,y+1);left=Math.Min(left,x);right=Math.Max(right,x+1);}
        if(bottom==0)throw new InvalidDataException("Empty character contact.");
        var footLeft=frame.PixelWidth;var footRight=0;
        for(var y=Math.Max(0,bottom-2);y<bottom;y++)for(var x=0;x<frame.PixelWidth;x++)if(pixels[(y*frame.PixelWidth+x)*4+3]>=128)
        {footLeft=Math.Min(footLeft,x);footRight=Math.Max(footRight,x+1);}
        return(new(footLeft,footRight,bottom,top),new(left,top,right-left,bottom-top));
    }
    private static PointD[] Corners(RasterImage frame)
    {
        var points=new List<PointD>();var pixels=frame.Pixels.Span;
        for(var y=0;y<frame.PixelHeight;y++)for(var x=0;x<frame.PixelWidth;x++)if(pixels[(y*frame.PixelWidth+x)*4+3]>=128)
        {points.Add(new(x,y));points.Add(new(x+1,y));points.Add(new(x,y+1));points.Add(new(x+1,y+1));}
        return points.ToArray();
    }
    public static RasterImage Land(RasterImage frame,PlatformPose pose)
    {
        if(pose.Squash==0&&pose.Sway==0)return frame;
        return Warp(frame,LandingTransform(frame,pose));
    }
    internal static Affine LandingTransform(RasterImage frame,PlatformPose pose)
    {
        if(pose.Squash==0&&pose.Sway==0)return Affine.Identity;
        var contact=Measure(frame).Contact;var sx=1+Math.Clamp(pose.Squash,pose.IsExtremeLanding?-.16:-.10,pose.IsExtremeLanding?.82:.30)*.5;
        var sy=1-Math.Clamp(pose.Squash,pose.IsExtremeLanding?-.16:-.10,pose.IsExtremeLanding?.82:.30);
        var angle=Math.Clamp(pose.Sway,-Math.PI/90,Math.PI/90);var c=Math.Cos(angle);var s=Math.Sin(angle);
        var x=(contact.Left+contact.Right)/2;var y=contact.SoleY;
        var t=new Affine(sx*c,sx*s,-sy*s,sy*c,x-x*sx*c+y*sy*s,y-x*sx*s-y*sy*c);
        var bottom=Corners(frame).Max(p=>t.Transform(p).Y);t=t with{OffsetY=t.OffsetY+contact.SoleY-bottom};
        return t;
    }
    internal static RasterImage Warp(RasterImage source,Affine t,int size=144)
        =>LinearSampling.Render(source,t,size,size);
}

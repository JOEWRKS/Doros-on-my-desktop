using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

// A small torso excursion in source coordinates. No face/hair scaling and no
// movement of the soles. Cached native frames also remain valid press captures.
internal static class BodyBreathingFrame
{
    private static readonly Dictionary<(bool Closed,int Level),BitmapSource> Cache=new();
    private static readonly HashSet<BitmapSource> Frames=new();
    internal static bool Contains(object? source)=>source is BitmapSource image&&Frames.Contains(image);
    internal static BitmapSource Standing(bool closed,double amount)
    {
        var level=(int)Math.Round(Math.Clamp(amount,0,1)*32);
        if(level==0)return UprightRumpSource.Standing(closed);
        var key=(closed,level);
        if(Cache.TryGetValue(key,out var cached))return cached;
        var source=PremultipliedFrame.From(UprightRumpSource.Standing(closed)).Pixels;
        var pixels=Warp(source,level/32d);
        // Keep every head-owned texel, including antialiased hair/ribbon, exact.
        var head=HuntFrames.Instance.Head(closed?1:0).Span;
        for(var i=0;i<pixels.Length;i+=4)if(head[i+3]>0)Array.Copy(source,i,pixels,i,4);
        var image=BitmapSource.Create(96,96,96,96,PixelFormats.Pbgra32,null,pixels,384);image.Freeze();
        Cache.Add(key,image);Frames.Add(image);return image;
    }
    internal static byte[] Warp(byte[] source,double amount)
    {
        var pixels=(byte[])source.Clone();
        if(amount<=0)return pixels;
        for(var y=49;y<74;y++)for(var x=50;x<84;x++)
        {
            // Zero at both boundaries; the lower legs and foot registration
            // cannot move. Maximum outward excursion is less than half a pixel.
            var weight=Smooth((x-50)/13d)*Math.Pow(Math.Sin(Math.PI*(y-49)/25d),2)*amount;
            var sx=x-.4*weight;var sy=y+.22*weight;
            var ix=(int)Math.Floor(sx);var iy=(int)Math.Floor(sy);var fx=sx-ix;var fy=sy-iy;
            for(var c=0;c<4;c++)pixels[(y*96+x)*4+c]=(byte)Math.Round(
                source[(iy*96+ix)*4+c]*(1-fx)*(1-fy)+source[(iy*96+ix+1)*4+c]*fx*(1-fy)+
                source[((iy+1)*96+ix)*4+c]*(1-fx)*fy+source[((iy+1)*96+ix+1)*4+c]*fx*fy);
        }
        return pixels;
    }
    private static double Smooth(double t){t=Math.Clamp(t,0,1);return t*t*(3-2*t);}
}

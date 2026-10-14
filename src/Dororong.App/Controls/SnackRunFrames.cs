using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Geometry;

namespace Dororong.App.Controls;

// Shared source parts for a native diagonal walk and asymmetric snack gallop.
// Gallop forefeet
// land in sequence, then hindfeet gather/push; both pairs have a suspension.
// Move the approved source texels once. The head is copied at integer offset,
// never scaled together with the limbs or filtered through an enlarged bitmap.
internal static class SnackRunFrames
{
    internal const double Stride = 66;
    private static readonly PointD[] Roots = [new(22,71),new(39,77),new(64,74),new(55,70)];
    private static readonly PointD[] Tips = [new(23,79),new(43,85),new(66,82),new(54.5,78.5)];
    private static readonly double[] Offsets = [0,.09,.53,.60];
    private static readonly byte[] Source = PremultipliedFrame.From(UprightRumpSource.Standing(false)).Pixels;
    private static readonly byte[] Head = HeadPixels(false);
    private static readonly byte[] ClosedHead = HeadPixels(true);
    private static readonly byte[] Body = BodyPixels();

    internal static PointD Foot(int leg, double phase)
    {
        var q = (phase + Offsets[leg]) % 1;
        // Short stance and longer gathered swing distinguish this from walking.
        const double stance = .30;
        if (q < stance) return new(-4.2 + 8.4*q/stance, 0);
        var u=(q-stance)/(1-stance);
        return new(4.2-8.4*Smooth(u), -4.0*Math.Pow(Math.Sin(Math.PI*u),2));
    }

    internal static int Bob(double phase, double amount) =>
        (int)Math.Round(-2.4*amount*Math.Pow(Math.Sin(2*Math.PI*(phase-.08)),2));

    internal static BitmapSource Render(double distance, double amount, bool walking=false, bool closed=false)
    {
        amount=Math.Clamp(amount,0,1);
        if (amount<=0) return UprightRumpSource.Standing(closed);
        var phase=((distance/(walking?10:Stride))%1+1)%1;
        var bob=walking?(int)Math.Round(-.55*amount*(1-Math.Cos(phase*4*Math.PI))):Bob(phase,amount);
        var feet=Enumerable.Range(0,4).Select(i=>walking?WalkFoot(i,phase):Foot(i,phase)).ToArray();
        var output=new byte[96*96*4];
        // The occluded fourth leg uses the existing hind-paw pixels, behind
        // the continuous torso. No extra toes or long human-like joint chain.
        for(var y=65;y<88;y++)for(var x=44;x<66;x++)
        {
            var sx=x+11-(feet[3].X*amount);
            var sy=y+3.5-feet[3].Y*amount;
            if(sx<59 || sx>73 || sy<74 || sy>86)continue;
            Sample(Source,sx,sy,output,(y*96+x)*4);
            if(walking)for(var c=0;c<4;c++)output[(y*96+x)*4+c]=(byte)Math.Round(output[(y*96+x)*4+c]*amount);
        }
        var sample=new byte[4];
        for(var y=45;y<94;y++)for(var x=8;x<88;x++)
        {
            // Invert a bounded continuous body field, instead of stretching a
            // rectangular crop (which would pull hair/old paw ink into the gaps).
            var sx=(double)x;var sy=(double)y-bob;
            for(var n=0;n<8;n++)
            {
                var mapped=Map(sx,sy);
                sx+=x-mapped.X;sy+=y-mapped.Y;
            }
            Array.Clear(sample);Sample(Body,sx,sy,sample,0);
            Over(output,(y*96+x)*4,sample,0);
        }
        // Rigid authored head, exact colors/alpha and constant proportions.
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)
        {
            var yy=y+bob;if(yy<0||yy>=96)continue;
            Over(output,(yy*96+x)*4,closed?ClosedHead:Head,(y*96+x)*4);
        }
        var bitmap=BitmapSource.Create(96,96,96,96,PixelFormats.Pbgra32,null,output,384);
        bitmap.Freeze();return bitmap;

        PointD Map(double x,double y)
        {
            double dx=0,dy=0,total=0;
            for(var i=0;i<3;i++)
            {
                var root=Roots[i];var tip=Tips[i];
                var u=Math.Clamp((y-root.Y)/(tip.Y-root.Y),0,1);
                var center=root.X+(tip.X-root.X)*u;
                var w=1-Smooth((Math.Abs(x-center)-(walking?4:3))/5);
                var reach=Smooth(u);
                dx+=w*reach*feet[i].X*amount;
                dy+=w*reach*(feet[i].Y*amount-bob);
                total+=w;
            }
            return new(x+dx/Math.Max(1,total),y+bob+dy/Math.Max(1,total));
        }
    }

    private static PointD WalkFoot(int leg,double phase)
    {
        var q=(phase+(leg%2==0?0:.5))%1;var u=(q-.6)/.4;
        return q<.6?new(-3+10*q,0):new(3-6*Smooth(u),-2.8*Math.Pow(Math.Sin(Math.PI*u),2));
    }

    private static byte[] HeadPixels(bool closed)
    {
        var rgba=HuntFrames.Instance.Head(closed?1:0).Span;var pixels=new byte[rgba.Length];
        for(var i=0;i<rgba.Length;i+=4)
        {
            var a=rgba[i+3];pixels[i+3]=a;
            for(var c=0;c<3;c++)pixels[i+c]=(byte)((rgba[i+2-c]*a+127)/255);
        }
        return pixels;
    }
    private static byte[] BodyPixels()
    {
        var pixels=(byte[])Source.Clone();
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)
        {
            var i=(y*96+x)*4;
            if(Head[i+3]==0)continue;
            // Hidden neck backing: bounded under the opaque head, not a second
            // copy of the hair/ribbon. Its exterior remains the source contour.
            Array.Clear(pixels,i,4);
            if(y>=55&&y<=73&&x>=21&&x<=65&&Head[i+3]>=250)
                for(var c=0;c<4;c++)pixels[i+c]=255;
        }
        return pixels;
    }
    internal static byte[] CopyBody() => (byte[])Body.Clone();
    private static void Sample(byte[] pixels,double x,double y,byte[] output,int at)
    {
        var ix=(int)Math.Floor(x);var iy=(int)Math.Floor(y);
        if(ix<0||iy<0||ix>=95||iy>=95)return;
        var fx=x-ix;var fy=y-iy;var p=(iy*96+ix)*4;
        for(var c=0;c<4;c++)output[at+c]=(byte)Math.Round(pixels[p+c]*(1-fx)*(1-fy)+
            pixels[p+4+c]*fx*(1-fy)+pixels[p+384+c]*(1-fx)*fy+pixels[p+388+c]*fx*fy);
    }
    private static void Over(byte[] target,int at,byte[] source,int from)
    {
        var alpha=source[from+3]/255d;
        for(var c=0;c<4;c++)target[at+c]=(byte)Math.Min(255,Math.Round(source[from+c]+target[at+c]*(1-alpha)));
    }
    private static double Smooth(double u){u=Math.Clamp(u,0,1);return u*u*(3-2*u);}
}

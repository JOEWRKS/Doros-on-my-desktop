using Dororong.Core.Geometry;

// Post-primary surrounding motion. Original artwork/primary render are immutable inputs.
// Fields are authored visual assumptions, not recovered anatomy. All units are native pixels.
namespace Dororong.Portable.Rendering;

internal static class SurroundingPullRenderer
{
    internal static byte[] Head(byte[] source, PointD grab, PointD follow, PointD? distant=null)
    {
        Validate(source,grab,follow,distant??follow);
        var baseline=Pad(source);if(follow==default && (distant??default)==default)return baseline;
        var center=PinkCenter(source);var near=Limit(follow,14);var far=Limit(distant??follow,14);
        return Warp(baseline,p=>HeadDisplacement(p,center,grab,near,far));
    }
    internal static PointD HeadDisplacement(PointD p,PointD center,PointD grab,PointD near,PointD far)
    {
            // Keep the grab and soles stationary in the existing registered pose.
            var pin=Smooth((Length(p-grab)-3)/9);var sole=1-Smooth((p.Y-79)/5);
            if(pin==0||sole==0)return default;
            var face=Bump(p.X,center.X-31,center.X-23,center.X+6,center.X+13)*
                Bump(p.Y,center.Y+3,center.Y+8,center.Y+24,center.Y+31);
            // A rigid face plateau: eyes/mouth translate together, without global face scaling.
            var facePin=Smooth((Length(new PointD(center.X-8,center.Y+16)-grab)-3)/9);
            var hair=Math.Exp(-Math.Pow((p.X-grab.X)/25,2)-Math.Pow((p.Y-grab.Y)/24,2));
            var accessoryLock=1-Smooth((p.X-(center.X+12))/7)*(1-Smooth((p.Y-(center.Y+15))/14));
            var headGate=1-Smooth((p.Y-(center.Y+25))/10);
            var h=(hair*pin*(1-face)+.6*face*facePin*pin)*headGate*accessoryLock;
            var body=Smooth((p.Y-(center.Y+24))/12)*Bump(p.X,12,30,72,85)*sole;
            // Negative sign is material lag behind the grabbed point, not a second pointer move.
            return Scale(near,-.12*h*sole)+Scale(far,-.055*body*pin);
    }
    internal static PointD PinkCenter(byte[] source)
    {
        double sx=0,sy=0,count=0;
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)
        {var i=(y*96+x)*4;if(source[i+3]<128||source[i+2]<=source[i]+12||source[i]<=source[i+1]+12)continue;sx+=x;sy+=y;count++;}
        if(count==0)throw new ArgumentException("No pink reference landmark found.");
        return new(sx/count,sy/count);
    }
    static byte[] Warp(byte[] source,Func<PointD,PointD> field)
    {
        var output=(byte[])source.Clone();
        // Both fields move material by at most 2.45 pixels; the inverse solve cannot
        // sample nonzero alpha beyond source ink bounds + 4 (including bilinear taps).
        var left=160;var top=160;var right=-1;var bottom=-1;
        for(var y=0;y<160;y++)for(var x=0;x<160;x++)if(source[(y*160+x)*4+3]>0)
        {left=Math.Min(left,x);top=Math.Min(top,y);right=Math.Max(right,x);bottom=Math.Max(bottom,y);}
        for(var y=Math.Max(0,top-4);y<=Math.Min(159,bottom+4);y++)for(var x=Math.Max(0,left-4);x<=Math.Min(159,right+4);x++)
        {
            var q=new PointD(x-32,y-32);var uv=q;
            if(field(q)==default){Array.Copy(source,(y*160+x)*4,output,(y*160+x)*4,4);continue;}
            for(var i=0;i<8;i++){var next=q-field(uv);if(Length(next-uv)<1e-8){uv=next;break;}uv=next;}
            var u=uv.X+32;var v=uv.Y+32;var ix=(int)Math.Floor(u);var iy=(int)Math.Floor(v);
            var fx=u-ix;var fy=v-iy;var o=(y*160+x)*4;
            for(var c=0;c<4;c++)
            {
                double Sample(int xx,int yy)=>xx>=0&&xx<160&&yy>=0&&yy<160?source[(yy*160+xx)*4+c]:0;
                var value=(Sample(ix,iy)*(1-fx)+Sample(ix+1,iy)*fx)*(1-fy)+(Sample(ix,iy+1)*(1-fx)+Sample(ix+1,iy+1)*fx)*fy;
                output[o+c]=(byte)Math.Clamp(Math.Round(Math.Round(value,10)),0,255);
            }
        }
        return output;
    }
    static void Validate(byte[] source,PointD grab,PointD a,PointD b)
    {
        if(source.Length!=96*96*4)throw new ArgumentException("96x96 premultiplied input required.");
        if(new[]{grab.X,grab.Y,a.X,a.Y,b.X,b.Y}.Any(v=>!double.IsFinite(v)))throw new ArgumentException("Finite points required.");
    }
    static PointD Limit(PointD p,double limit){var length=Length(p);return length>limit?Scale(p,limit/length):p;}
    static double Length(PointD p)=>Math.Sqrt(p.X*p.X+p.Y*p.Y);
    static PointD Scale(PointD p,double q)=>new(p.X*q,p.Y*q);
    static double Smooth(double t){t=Math.Clamp(t,0,1);return t*t*(3-2*t);}
    static double Bump(double p,double a,double b,double c,double d)=>Smooth((p-a)/(b-a))*(1-Smooth((p-c)/(d-c)));
    internal static byte[] Pad(byte[] source)
    {
        var output = new byte[160 * 160 * 4];
        for (var y = 0; y < 96; y++) Array.Copy(source, y * 384, output, ((y + 32) * 160 + 32) * 4, 384);
        return output;
    }
}

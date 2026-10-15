
namespace Dororong.Portable.Rendering;

// Same local whole-eye displacement field as the approved browser preview.
internal sealed class HuntEyes
{
    private readonly byte[] _rgba = new byte[384 * 384 * 4];
    private readonly byte[] _pbgra = new byte[384 * 384 * 4];
    private static double Smooth(double t) { t = Math.Clamp(t, 0, 1); return t * t * (3 - 2 * t); }
    private static double Bump(double v, double a, double b, double c, double d) => Smooth((v-a)/(b-a))*Smooth((d-v)/(d-c));
    internal RasterImage Paint(double dx, double dy, bool closed,double cheekPull=0,double eyePull=0,double hairPull=0)
    {
        if (!double.IsFinite(dx) || !double.IsFinite(dy)) throw new ArgumentOutOfRangeException(nameof(dx));
        dx = Math.Clamp(dx, -.85, .85); dy = Math.Clamp(dy, -.65, .65);
        var index = closed ? 1 : 0;
        var source = HuntFrames.Instance.Head(index);
        // Preserve authored texels until the pose transform samples the head.
        // The baked enlarged image was already filtered; filtering it again
        // during composition and native-size reduction softened the face.
        for(var y=0;y<384;y++)for(var x=0;x<384;x++)
            source.Span.Slice(((y/4)*96+x/4)*4,4).CopyTo(_rgba.AsSpan((y*384+x)*4,4));
        double Sample(double x, double y, int c)
        {
            // Move on the same unfiltered working grid as the base head.
            // Subtracting a differently filtered base leaves old eye ink behind.
            x=x*4+1.5;y=y*4+1.5;
            var ix=(int)Math.Floor(x); var iy=(int)Math.Floor(y); var fx=x-ix; var fy=y-iy; double v=0;
            for(var yy=0;yy<2;yy++)for(var xx=0;xx<2;xx++)v+=source.Span[(((iy+yy)/4)*96+(ix+xx)/4)*4+c]*(xx==1?fx:1-fx)*(yy==1?fy:1-fy);
            return v;
        }
        foreach(var e in new[]{new[]{15,43,30,60,15,17,27,30,43,46,57,60},new[]{32,46,46,63,32,34,43,46,46,49,60,63}})
        for(var y=e[1]*4;y<e[3]*4;y++)for(var x=e[0]*4;x<e[2]*4;x++)
        {
            var sx=(x+.5)/4-.5; var sy=(y+.5)/4-.5;
            if(sx+.5>=25 && sx+.5<34 && sy+.5>=57 && sy+.5<62)continue;
            var m=Bump(sx+.5,e[4],e[5],e[6],e[7])*Bump(sy+.5,e[8],e[9],e[10],e[11]);
            if(m==0)continue;
            for(var c=0;c<3;c++){var i=(y*384+x)*4+c;_rgba[i]=(byte)Math.Clamp(Math.Round(_rgba[i]+Sample(sx-dx*m,sy-dy*m,c)-Sample(sx,sy,c)),0,255);}
        }
        for(var i=0;i<_rgba.Length;i+=4)
        {
            var a=_rgba[i+3];_pbgra[i+3]=a;
            for(var c=0;c<3;c++)_pbgra[i+c]=(byte)((_rgba[i+2-c]*a+127)/255);
        }
        if(cheekPull!=0) ApplyCheek(source,cheekPull,eyePull,hairPull);
        return new RasterImage(384,384,_pbgra);
    }

    private void ApplyCheek(ReadOnlyMemory<byte> head,double pull,double eye,double hair)
    {
        var original=head.ToArray();
        for(var i=0;i<original.Length;i+=4)(original[i],original[i+2])=(original[i+2],original[i]);
        var changed=OutlineCheekRenderer.Render(original,pull,eye,hair);
        var delta=new int[original.Length];
        for(var i=0;i<delta.Length;i+=4)
        {
            for(var c=0;c<3;c++)delta[i+c]=(changed[i+c]*changed[i+3]+127)/255-(original[i+c]*original[i+3]+127)/255;
            delta[i+3]=changed[i+3]-original[i+3];
        }
        for(var y=0;y<384;y++)for(var x=0;x<384;x++)
        {
            var from=((y/4)*96+x/4)*4;var at=(y*384+x)*4;
            for(var c=3;c>=0;c--)_pbgra[at+c]=(byte)Math.Clamp(_pbgra[at+c]+delta[from+c],0,c==3?255:_pbgra[at+3]);
        }
    }

}


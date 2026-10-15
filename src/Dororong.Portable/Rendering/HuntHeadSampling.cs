
namespace Dororong.Portable.Rendering;

// Recover original texels from the replicated eye-working grid, then reconstruct
// the posed head once. Bounded cubic interpolation preserves thin ink without
// point-sampling shimmer or sharpening/blur passes on the completed character.
internal sealed class HuntHeadSampling
{
    private readonly byte[] working = new byte[384*384*4];
    private readonly byte[] source = new byte[96*96*4];

    internal RasterImage Render(RasterImage head,Affine transform)
    {
        if(head.PixelWidth!=384 || head.PixelHeight!=384 || !transform.HasInverse)
            throw new ArgumentException("Expected a 384px head and invertible pose.");
        head.CopyPixels(working,1536,0);
        // This is exactly identity for replicated artwork. Only the subpixel eye
        // displacement field contains different samples within a source texel.
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)for(var c=0;c<4;c++)
        {
            var sum=0;
            for(var dy=0;dy<4;dy++)for(var dx=0;dx<4;dx++)sum+=working[((y*4+dy)*384+x*4+dx)*4+c];
            source[(y*96+x)*4+c]=(byte)((sum+8)/16);
        }
        var inverse=transform.Inverse();var output=new byte[96*96*4];
        Span<double> wx=stackalloc double[4];Span<double> wy=stackalloc double[4];
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)
        {
            var p=inverse.Transform(new Dororong.Core.Geometry.PointD(x+.5,y+.5));p=new(p.X-.5,p.Y-.5);
            var ix=(int)Math.Floor(p.X);var iy=(int)Math.Floor(p.Y);
            if(ix < -2 || iy < -2 || ix > 96 || iy > 96)continue;
            Weights(p.X-ix,wx);Weights(p.Y-iy,wy);
            var at=(y*96+x)*4;
            for(var c=3;c>=0;c--)
            {
                double value=0;var low=255;var high=0;
                for(var yy=0;yy<4;yy++)for(var xx=0;xx<4;xx++)
                {
                    var sx=ix+xx-1;var sy=iy+yy-1;
                    var sample=sx>=0&&sx<96&&sy>=0&&sy<96?source[(sy*96+sx)*4+c]:0;
                    value+=sample*wx[xx]*wy[yy];
                    if(xx is 1 or 2 && yy is 1 or 2){low=Math.Min(low,sample);high=Math.Max(high,sample);}
                }
                // No ringing colors/alpha beyond the local contributing texels.
                value=Math.Clamp(value,low,high);
                output[at+c]=(byte)Math.Clamp(Math.Round(value),0,c==3?255:output[at+3]);
            }
        }
        var result=new RasterImage(96,96,output);
        return result;
    }
    private static void Weights(double t,Span<double> w)
    {
        var t2=t*t;var t3=t2*t;
        w[0]=-.5*t+t2-.5*t3;w[1]=1-2.5*t2+1.5*t3;
        w[2]=.5*t+2*t2-1.5*t3;w[3]=-.5*t2+.5*t3;
    }
}


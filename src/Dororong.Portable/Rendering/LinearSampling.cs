namespace Dororong.Portable.Rendering;

// WPF's working-surface sampler uses eight fractional bits, rounded output,
// and pixel centres. Do not use the host GPU's independently rounded filter.
internal static class LinearSampling
{
    internal static RasterImage Render(RasterImage source, Affine transform, int width, int height)
    {
        var inverse=transform.Inverse();var input=source.Pixels.Span;var output=new byte[width*height*4];
        for(var y=0;y<height;y++)for(var x=0;x<width;x++)
        {
            var p=inverse.Transform(new(x+.5,y+.5));var sx=p.X-.5;var sy=p.Y-.5;
            var ix=(int)Math.Floor(sx);var iy=(int)Math.Floor(sy);
            var fx=(int)Math.Floor((sx-ix)*256);var fy=(int)Math.Floor((sy-iy)*256);
            for(var c=0;c<4;c++)
            {
                var sum=0;
                for(var dy=0;dy<2;dy++)for(var dx=0;dx<2;dx++)
                {
                    var xx=ix+dx;var yy=iy+dy;
                    if(xx>=0&&xx<source.PixelWidth&&yy>=0&&yy<source.PixelHeight)
                        sum+=input[(yy*source.PixelWidth+xx)*4+c]*(dx==0?256-fx:fx)*(dy==0?256-fy:fy);
                }
                output[(y*width+x)*4+c]=(byte)Math.Clamp((sum+32768)>>16,0,255);
            }
        }
        return new(width,height,output);
    }
}

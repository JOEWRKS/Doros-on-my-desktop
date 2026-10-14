using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

// Chew the existing little omega mouth. No replacement contour, stroke color,
// blush or painted-white cover: only a bounded resampling of its own texels.
internal static class SnackMouthFrame
{
    internal static BitmapSource Chew(BitmapSource source,double age)
    {
        if(age<=.48 || age>=1.55)return source;
        var u=(age-.48)/1.07;
        var beat=Math.Pow(Math.Sin(u*3*Math.PI),2);
        if(beat<.00001)return source;
        var pixels=PremultipliedFrame.From(source).Pixels;
        var output=(byte[])pixels.Clone();
        for(var y=56;y<=64;y++)for(var x=24;x<=34;x++)
        {
            var wx=Smooth((x-24)/2d)*Smooth((34-x)/2d);
            var wy=Smooth((y-56)/2d)*Smooth((64-y)/2d);
            var influence=wx*wy;
            // Include the upper/outer lip instead of pinning it to the old ROI
            // edge. One native-pixel lift makes each of the three closed-mouth
            // beats readable at 96px, without moving the eyes or chin outline.
            var sx=29+(x-29)/(1+.18*beat*influence);
            var sy=60+(y-60)/(1-.08*beat*influence)+.95*beat*influence;
            var ix=(int)Math.Floor(sx);var iy=(int)Math.Floor(sy);
            var fx=sx-ix;var fy=sy-iy;var from=(iy*96+ix)*4;var to=(y*96+x)*4;
            for(var c=0;c<3;c++)output[to+c]=(byte)Math.Round(pixels[from+c]*(1-fx)*(1-fy)+
                pixels[from+4+c]*fx*(1-fy)+pixels[from+384+c]*(1-fx)*fy+pixels[from+388+c]*fx*fy);
        }
        var frame=BitmapSource.Create(96,96,96,96,PixelFormats.Pbgra32,null,output,384);
        frame.Freeze();return frame;
    }
    private static double Smooth(double u){u=Math.Clamp(u,0,1);return u*u*(3-2*u);}
}

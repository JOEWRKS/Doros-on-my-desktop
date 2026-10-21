// r104 effect implementation; authored 60Hz eating overlays use exact WPF pixels.
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

// Vector effect layer at source resolution. The approved head is never painted
// over on disk; chewing is an inverse lookup in HighResolutionFacePose.
internal static class HighResolutionSnackEffects
{
    private static BitmapSource Jewel=>SnackImage.Source;
    internal static float[] Render(double eatingAge,double droolAge){
        var pixels=new float[225*225*4];
        RenderInto(eatingAge,droolAge,new DrawingVisual(),new RenderTargetBitmap(225,225,96,96,PixelFormats.Pbgra32),new byte[pixels.Length],pixels);
        return pixels;
    }
    // Owned by one raster; rows finish before the next preparation. A single
    // source-resolution surface replaces the per-frame 810KB float and203KB
    // byte arrays. The same effect is also reused by sole/display queries.
    internal sealed class Buffer
    {
        private readonly DrawingVisual visual=new();
        private readonly RenderTargetBitmap bitmap=new(225,225,96,96,PixelFormats.Pbgra32);
        private readonly byte[] bytes=new byte[225*225*4];
        private readonly float[] pixels=new float[225*225*4];
        private bool valid;
        private double eating,drool;
        internal float[] Get(double eatingAge,double droolAge){
            if(valid&&eatingAge==eating&&droolAge==drool)return pixels;
            HighResolutionBodyInk.Invalidate(pixels);
            RenderInto(eatingAge,droolAge,visual,bitmap,bytes,pixels);
            eating=eatingAge;drool=droolAge;valid=true;return pixels;
        }
    }
    private static void RenderInto(double eatingAge,double droolAge,DrawingVisual visual,RenderTargetBitmap bitmap,byte[] bytes,float[] pixels){
        if(Dororong.Raster.FixedEating.Copy(eatingAge,bytes)){
            for(var i=0;i<bytes.Length;i++)pixels[i]=bytes[i];return;
        }
        using(var dc=visual.RenderOpen()){
            dc.PushTransform(new ScaleTransform(225d/96,225d/96));
            if(eatingAge>=0){
                var open=HighResolutionGrabRaster.Smooth(eatingAge/.14)*(1-HighResolutionGrabRaster.Smooth((eatingAge-.30)/.18));
                if(open>.01){
                    var pen=new Pen(new SolidColorBrush(Color.FromRgb(80,52,61)),.85){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
                    var g=new StreamGeometry();using(var c=g.Open()){
                        c.BeginFigure(new Point(27,60),true,true);
                        c.QuadraticBezierTo(new Point(29,60.6),new Point(31.2,60),false,false);
                        c.BezierTo(new Point(31,60+open*4.2),new Point(27.2,60+open*4.2),new Point(27,60),true,false);
                    }
                    dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(109,57,72)),pen,g);
                    dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(235,145,170)),null,new Point(29.2,60+open*2),1.1,open*.75);
                }
                if(eatingAge<.34){
                    var swallow=HighResolutionGrabRaster.Smooth((eatingAge-.12)/.22);var size=10*(1-swallow)+1;
                    dc.PushOpacity(1-swallow);dc.DrawImage(Jewel,new Rect(21+8*swallow-size/2,58-size/2,size,size));dc.Pop();
                }
            }else if(droolAge>=0){
                var y=67+4*(.5+.5*Math.Sin(droolAge*4));
                var pen=new Pen(new SolidColorBrush(Color.FromRgb(108,185,222)),1.3){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
                var g=new StreamGeometry();using(var c=g.Open()){
                    c.BeginFigure(new Point(30.8,61.5),false,false);c.BezierTo(new Point(32,65),new Point(30,67),new Point(31,y),true,false);
                }
                dc.DrawGeometry(null,pen,g);
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(174,226,242)),new Pen(Brushes.SteelBlue,.45),new Point(31,y+1),1.3,2);
                dc.DrawLine(new Pen(Brushes.White,.45),new Point(30.8,y),new Point(30.8,y+1.5));
            }
            dc.Pop();
        }
        bitmap.Clear();bitmap.Render(visual);bitmap.CopyPixels(bytes,900,0);
        for(var i=0;i<bytes.Length;i++)pixels[i]=bytes[i];
    }
}

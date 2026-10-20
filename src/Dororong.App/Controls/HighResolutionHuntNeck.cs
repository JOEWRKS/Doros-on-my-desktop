using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

// Same attachment anchors as HuntRenderer, behind both the immutable head and
// the source-resolution forebody. No stroke is painted over either source part.
internal static class HighResolutionHuntNeck
{
    internal sealed class Buffer
    {
        private readonly byte[] bytes=new byte[225*225*4];
        private readonly float[] pixels=new float[225*225*4];
        private readonly DrawingVisual visual=new();
        private readonly RenderTargetBitmap bitmap=new(225,225,96,96,PixelFormats.Pbgra32);
        private (int Body,Matrix Head)? last;
        internal float[] Get(HighResolutionFacePose face){
            var key=(face.BodyFrame,face.Head);
            if(last==key)return pixels;
            HighResolutionBodyInk.Invalidate(pixels);
            RenderInto(face,bytes,pixels,visual,bitmap);last=key;return pixels;
        }
    }
    // Allocation-isolated reference path retained for byte-for-byte tests.
    internal static float[] Render(HighResolutionFacePose face){
        var pixels=new float[225*225*4];RenderInto(face,new byte[pixels.Length],pixels);return pixels;
    }
    private static void RenderInto(HighResolutionFacePose face,byte[] bytes,float[] pixels,
        DrawingVisual? retainedVisual=null,RenderTargetBitmap? retainedBitmap=null)
    {
        var pose=HuntPose.At(face.BodyFrame/60d);
        var front=face.Head.Transform(new Point(16,68));
        var mapped=pose.Map(17,68);var amount=pose.Amount;
        var root=new Point(mapped.X+(23-mapped.X)*amount,mapped.Y+(68-mapped.Y)*amount);
        var old=pose.Map(17.2,73);var blend=HighResolutionGrabRaster.Smooth(amount/.2);
        var chest=new Point(old.X+(root.X-old.X)*blend,old.Y+(root.Y-old.Y)*blend);
        var belly=pose.Map(46,75);
        StreamGeometry Shape(bool fill)
        {
            var g=new StreamGeometry();using(var c=g.Open()){
                c.BeginFigure(front,fill,fill);
                c.BezierTo(new(front.X-1,front.Y+(chest.Y-front.Y)*.35),new(chest.X-.7,chest.Y-1.8),chest,true,false);
                if(fill){c.LineTo(new(belly.X,belly.Y),true,false);c.LineTo(face.Head.Transform(new Point(47,65)),true,false);}
            }g.Freeze();return g;
        }
        var v=retainedVisual??new DrawingVisual();using(var dc=v.RenderOpen()){
            dc.PushTransform(new ScaleTransform(225d/96,225d/96));
            dc.DrawGeometry(Brushes.White,null,Shape(true));
            var pen=new Pen(new SolidColorBrush(Color.FromRgb(68,52,56)),1.295415959252971){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
            dc.DrawGeometry(null,pen,Shape(false));dc.Pop();
        }
        // The native WIC surface carries memory pressure even though its
        // managed wrapper is small. Recreating it for every neck/gaze frame
        // induced full collections; retain one surface with the pixel buffers.
        var bitmap=retainedBitmap??new RenderTargetBitmap(225,225,96,96,PixelFormats.Pbgra32);
        bitmap.Clear();bitmap.Render(v);
        bitmap.CopyPixels(bytes,900,0);
        for(var i=0;i<bytes.Length;i++)pixels[i]=bytes[i];
    }
}

using System.Windows;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

// One owned blend surface for the existing 120ms complementary row clip.
// Do not change the source capture, clip curve, timing, sampling or artwork.
internal sealed class HighResolutionPerchBlend
{
    private byte[] pixels=[],previousPixels=[];
    private WriteableBitmap? bitmap;
    internal byte[] Pixels=>pixels;
    internal BitmapSource Update(BitmapSource previous,BitmapSource current,double age)
    {
        var width=current.PixelWidth;var height=current.PixelHeight;var stride=width*4;
        if(previous.PixelWidth!=width||previous.PixelHeight!=height)
            throw new ArgumentException("Perch blend sources must have matching dimensions.");
        if(bitmap is null||bitmap.PixelWidth!=width||bitmap.PixelHeight!=height||
            bitmap.Format!=current.Format||bitmap.DpiX!=current.DpiX||bitmap.DpiY!=current.DpiY){
            bitmap=new WriteableBitmap(width,height,current.DpiX,current.DpiY,current.Format,null);
            pixels=new byte[stride*height];previousPixels=new byte[pixels.Length];
        }
        // Read both sources before writing: current may itself be the last
        // blend, or both inputs may alias the live display surface.
        current.CopyPixels(pixels,stride,0);previous.CopyPixels(previousPixels,stride,0);
        var boundary=(104+70*age/120)/256*height;
        var row=Math.Clamp((int)Math.Ceiling(boundary),0,height);
        Buffer.BlockCopy(previousPixels,row*stride,pixels,row*stride,(height-row)*stride);
        bitmap.WritePixels(new Int32Rect(0,0,width,height),pixels,stride,0);
        return bitmap;
    }
}

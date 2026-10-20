using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

// One WPF/WIC surface per output size instead of a new frozen native bitmap
// on every animation tick. Retained frame bytes remain separate from scratch.
internal sealed class HighResolutionDisplaySurface
{
    private WriteableBitmap? bitmap;
    private double outputScale;
    internal BitmapSource Update(byte[] pixels,double scale)
    {
        var extent=(int)Math.Ceiling(256*scale);
        if(pixels.Length<extent*extent*4)throw new ArgumentException("Output size mismatch.",nameof(pixels));
        // WIC rounds its exposed DPI. Compare the requested scale, not that
        // rounded value (otherwise 1.1/1.35 recreate the surface every tick).
        if(bitmap is null||bitmap.PixelWidth!=extent||outputScale!=scale){
            bitmap=new WriteableBitmap(extent,extent,96*scale,96*scale,PixelFormats.Pbgra32,null);outputScale=scale;
        }
        bitmap.WritePixels(new Int32Rect(0,0,extent,extent),pixels,extent*4,0);
        return bitmap;
    }
}

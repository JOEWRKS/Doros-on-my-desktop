using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

// Keep one detached source for cursor and effects. Retaining BitmapImage's
// decoder/cache chain kept multiple full-resolution native copies after use.
// No DecodePixelWidth/downsampling: preserve the accepted source sampling.
internal static class SnackImage
{
    private static readonly Lazy<BitmapSource> source=new(Load);
    internal static BitmapSource Source=>source.Value;

    private static BitmapSource Load()
    {
        using var stream=Application.GetResourceStream(new Uri(
            "pack://application:,,,/Dororong.App;component/Assets/snack-jewel.png"))!.Stream;
        var decoder=BitmapDecoder.Create(stream,BitmapCreateOptions.PreservePixelFormat,BitmapCacheOption.OnLoad);
        var frame=decoder.Frames[0];
        var converted=new FormatConvertedBitmap(frame,PixelFormats.Pbgra32,null,0);
        var stride=checked(frame.PixelWidth*4);
        var pixels=new byte[checked(stride*frame.PixelHeight)];
        converted.CopyPixels(pixels,stride,0);
        var result=BitmapSource.Create(frame.PixelWidth,frame.PixelHeight,frame.DpiX,frame.DpiY,
            PixelFormats.Pbgra32,null,pixels,stride);
        result.Freeze();
        return result;
    }
}

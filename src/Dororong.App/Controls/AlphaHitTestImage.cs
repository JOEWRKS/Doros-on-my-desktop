using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Geometry;

namespace Dororong.App.Controls;

public sealed class AlphaHitTestImage : Image
{
    private ImageSource? _cachedSource;
    private byte[]? _pixels;
    private int _pixelWidth;
    private int _pixelHeight;
    private int _stride;

    // The live high-resolution surface is mutable. Install the corresponding
    // alpha bytes on every update; object identity alone is not a frame id.
    internal void SetLivePixels(BitmapSource source,byte[] pixels)
    {
        if(pixels.Length<source.PixelWidth*source.PixelHeight*4)throw new ArgumentException("Pixel size mismatch.",nameof(pixels));
        Source=source;_cachedSource=source;_pixels=pixels;
        _pixelWidth=source.PixelWidth;_pixelHeight=source.PixelHeight;_stride=_pixelWidth*4;
    }

    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        return TryGetOpaqueSourcePoint(hitTestParameters.HitPoint, out _)
            ? new PointHitTestResult(this, hitTestParameters.HitPoint)
            : null;
    }

    internal bool TryGetOpaqueSourcePoint(Point controlPoint, out PointD sourcePoint)
    {
        sourcePoint = default;
        if (!TryMapToSourcePixel(controlPoint, out var pixelX, out var pixelY) ||
            !TryCachePixels())
        {
            return false;
        }

        var alpha = _pixels![(pixelY * _stride) + (pixelX * 4) + 3];
        if (alpha == 0)
        {
            return false;
        }

        sourcePoint = new PointD(pixelX, pixelY);
        return true;
    }

    private bool TryMapToSourcePixel(Point point, out int pixelX, out int pixelY)
    {
        pixelX = 0;
        pixelY = 0;

        if (Source is not BitmapSource bitmap ||
            ActualWidth <= 0 ||
            ActualHeight <= 0 ||
            bitmap.PixelWidth <= 0 ||
            bitmap.PixelHeight <= 0)
        {
            return false;
        }

        var scale = Math.Min(
            ActualWidth / bitmap.PixelWidth,
            ActualHeight / bitmap.PixelHeight);
        var renderedWidth = bitmap.PixelWidth * scale;
        var renderedHeight = bitmap.PixelHeight * scale;
        var offsetX = (ActualWidth - renderedWidth) / 2;
        var offsetY = (ActualHeight - renderedHeight) / 2;

        if (point.X < offsetX ||
            point.Y < offsetY ||
            point.X >= offsetX + renderedWidth ||
            point.Y >= offsetY + renderedHeight)
        {
            return false;
        }

        pixelX = Math.Min(
            bitmap.PixelWidth - 1,
            (int)((point.X - offsetX) / scale));
        pixelY = Math.Min(
            bitmap.PixelHeight - 1,
            (int)((point.Y - offsetY) / scale));
        return true;
    }

    private bool TryCachePixels()
    {
        if (ReferenceEquals(_cachedSource, Source) && _pixels is not null)
        {
            return true;
        }

        if (Source is not BitmapSource bitmap)
        {
            return false;
        }

        // Hit testing consumes alpha only; premultiplication doesn't change it.
        var converted = bitmap.Format==PixelFormats.Pbgra32||bitmap.Format==PixelFormats.Bgra32 ? bitmap : new FormatConvertedBitmap(
            bitmap,
            PixelFormats.Bgra32,
            null,
            0);
        _pixelWidth = converted.PixelWidth;
        _pixelHeight = converted.PixelHeight;
        _stride = _pixelWidth * 4;
        var length=_stride*_pixelHeight;
        if(_pixels?.Length!=length)_pixels = new byte[length];
        converted.CopyPixels(_pixels, _stride, 0);
        _cachedSource = Source;
        return true;
    }
}

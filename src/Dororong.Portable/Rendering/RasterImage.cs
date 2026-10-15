using System.Runtime.InteropServices;
using SkiaSharp;

namespace Dororong.Portable.Rendering;

// Native-size, premultiplied BGRA. Every returned frame owns its bytes.
public sealed class RasterImage
{
    public int PixelWidth { get; }
    public int PixelHeight { get; }
    private readonly byte[] pixels;
    public ReadOnlyMemory<byte> Pixels => pixels;
    public RasterImage(int width, int height, ReadOnlySpan<byte> data)
    {
        if (width <= 0 || height <= 0 || data.Length != checked(width * height * 4))
            throw new ArgumentException("Invalid raster geometry.");
        PixelWidth = width; PixelHeight = height; pixels = data.ToArray();
    }
    public byte[] CopyPixels() => (byte[])pixels.Clone();
    internal void CopyPixels(byte[] target, int stride, int offset)
    {
        for (var y = 0; y < PixelHeight; y++)
            Array.Copy(pixels, y * PixelWidth * 4, target, offset + y * stride, PixelWidth * 4);
    }
    internal SKBitmap ToBitmap()
    {
        var bitmap = new SKBitmap(PixelWidth, PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        for (var y = 0; y < PixelHeight; y++)
            Marshal.Copy(pixels, y * PixelWidth * 4, bitmap.GetPixels() + y * bitmap.RowBytes, PixelWidth * 4);
        return bitmap;
    }
    internal static RasterImage FromBitmap(SKBitmap bitmap)
    {
        var data = new byte[bitmap.Width * bitmap.Height * 4];
        for (var y = 0; y < bitmap.Height; y++)
            Marshal.Copy(bitmap.GetPixels() + y * bitmap.RowBytes, data, y * bitmap.Width * 4, bitmap.Width * 4);
        return new(bitmap.Width, bitmap.Height, data);
    }
    internal static Stream Resource(string name) => typeof(RasterImage).Assembly.GetManifestResourceStream(name)
        ?? throw new InvalidDataException($"Missing approved asset: {name}");
    internal static RasterImage Load(string name, bool clean = false)
    {
        using var stream = Resource(name);
        using var codec = SKCodec.Create(stream) ?? throw new InvalidDataException("Invalid image.");
        var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        using var decoded = new SKBitmap(info);
        if (codec.GetPixels(info, decoded.GetPixels()) != SKCodecResult.Success) throw new InvalidDataException("Image decode failed.");
        var data = FromBitmap(decoded).CopyPixels();
        if (clean) data = CanonicalPixels.Clean(data);
        for (var i = 0; i < data.Length; i += 4)
            for (var c = 0; c < 3; c++) data[i + c] = (byte)((data[i + c] * data[i + 3] + 127) / 255);
        return new(info.Width, info.Height, data);
    }
    internal static void Over(byte[] target, ReadOnlySpan<byte> source)
    {
        for (var i = 0; i < target.Length; i += 4)
        {
            var coverage = source[i + 3] / 255d;
            for (var c = 0; c < 4; c++)
                target[i + c] = (byte)Math.Min(255, Math.Round(source[i + c] + target[i + c] * (1 - coverage)));
        }
    }
}

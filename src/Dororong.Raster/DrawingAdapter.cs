using System.Runtime.InteropServices;
using SkiaSharp;

// Only the three small vector overlays (neck, mouth/snack, hanging flutter)
// use this backend. Their coverage must be compared with WPF separately;
// compiling the shared sampler does not prove vector or native Mac parity.
namespace System.Windows.Media
{
    internal readonly record struct Color(byte R, byte G, byte B, byte A = 255)
    {
        internal static Color FromRgb(byte r, byte g, byte b) => new(r, g, b);
        internal SKColor Native => new(R, G, B, A);
    }
    internal sealed record SolidColorBrush(Color Color);
    internal static class Brushes
    {
        internal static readonly SolidColorBrush White = new(new(255, 255, 255));
        internal static readonly SolidColorBrush SteelBlue = new(new(70, 130, 180));
    }
    internal enum PenLineCap { Flat, Round }
    internal enum PenLineJoin { Miter, Round }
    internal sealed record Pen(SolidColorBrush Brush, double Thickness)
    {
        internal PenLineCap StartLineCap { get; init; }
        internal PenLineCap EndLineCap { get; init; }
        internal PenLineJoin LineJoin { get; init; }
    }
    internal abstract record Transform
    {
        internal abstract SKMatrix Native { get; }
    }
    internal sealed record ScaleTransform(double X, double Y) : Transform
    {
        internal override SKMatrix Native => SKMatrix.CreateScale((float)X, (float)Y);
    }
    internal sealed record MatrixTransform(double A, double B, double C, double D, double X, double Y) : Transform
    {
        internal override SKMatrix Native => new() { ScaleX = (float)A, SkewX = (float)C, TransX = (float)X,
            SkewY = (float)B, ScaleY = (float)D, TransY = (float)Y, Persp2 = 1 };
    }
    internal class Geometry
    {
        internal string? Svg;
        internal readonly List<Segment> Segments = [];
        internal readonly record struct Segment(int Kind, Point A, Point B, Point C, bool Stroke);
        internal static Geometry Parse(string path) => new() { Svg = path };
        internal SKPath Path(bool stroke)
        {
            if (Svg is not null) return SKPath.ParseSvgPathData(Svg) ?? throw new InvalidDataException("Invalid path.");
            var path = new SKPath { FillType = SKPathFillType.EvenOdd };
            Point start = default;
            var gap = false;
            foreach (var s in Segments)
            {
                if (s.Kind == 0) { start = s.A; gap = false; path.MoveTo((float)s.A.X, (float)s.A.Y); }
                else if (s.Kind == 4)
                {
                    // A non-stroked segment starts a new stroke subpath.
                    // Closing that subpath would add a line across the gap.
                    if (stroke && gap) path.LineTo((float)start.X, (float)start.Y);
                    else path.Close();
                }
                else if (stroke && !s.Stroke)
                {
                    gap = true;
                    var p = s.Kind == 1 ? s.A : s.Kind == 2 ? s.B : s.C;
                    path.MoveTo((float)p.X, (float)p.Y);
                }
                else if (s.Kind == 1) path.LineTo((float)s.A.X, (float)s.A.Y);
                else if (s.Kind == 2) path.QuadTo((float)s.A.X, (float)s.A.Y, (float)s.B.X, (float)s.B.Y);
                else path.CubicTo((float)s.A.X, (float)s.A.Y, (float)s.B.X, (float)s.B.Y, (float)s.C.X, (float)s.C.Y);
            }
            return path;
        }
    }
    internal sealed class StreamGeometry : Geometry
    {
        internal GeometryContext Open() { Segments.Clear(); return new(this); }
        internal void Freeze() { }
    }
    internal sealed class GeometryContext(StreamGeometry target) : IDisposable
    {
        private bool close;
        internal void BeginFigure(Point p, bool filled, bool closed) { close = closed; target.Segments.Add(new(0, p, default, default, false)); }
        internal void LineTo(Point p, bool stroke, bool smooth) => target.Segments.Add(new(1, p, default, default, stroke));
        internal void QuadraticBezierTo(Point a, Point b, bool stroke, bool smooth) => target.Segments.Add(new(2, a, b, default, stroke));
        internal void BezierTo(Point a, Point b, Point c, bool stroke, bool smooth) => target.Segments.Add(new(3, a, b, c, stroke));
        public void Dispose() { if (close) target.Segments.Add(new(4, default, default, default, true)); }
    }
    internal sealed class DrawingVisual
    {
        internal readonly List<Action<SKCanvas>> Commands = [];
        internal DrawingContext RenderOpen() { Commands.Clear(); return new(this); }
    }
    internal sealed class DrawingContext(DrawingVisual visual) : IDisposable
    {
        private int depth;
        private static SKPaint Paint(SolidColorBrush brush, Pen? pen = null) => new()
        {
            Color = brush.Color.Native, IsAntialias = true, Style = pen is null ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
            StrokeWidth = (float)(pen?.Thickness ?? 0), StrokeCap = pen?.StartLineCap == PenLineCap.Round ? SKStrokeCap.Round : SKStrokeCap.Butt,
            StrokeJoin = pen?.LineJoin == PenLineJoin.Round ? SKStrokeJoin.Round : SKStrokeJoin.Miter
        };
        internal void PushTransform(Transform transform)
        {
            depth++; visual.Commands.Add(c => { c.Save(); var m = transform.Native; c.Concat(ref m); });
        }
        internal void PushOpacity(double amount)
        {
            depth++; visual.Commands.Add(c => { using var p = new SKPaint { Color = SKColors.White.WithAlpha((byte)Math.Round(Math.Clamp(amount, 0, 1) * 255)) }; c.SaveLayer(p); });
        }
        internal void Pop()
        {
            if (depth == 0) throw new InvalidOperationException("Unbalanced drawing stack.");
            depth--; visual.Commands.Add(c => c.Restore());
        }
        internal void DrawGeometry(SolidColorBrush? fill, Pen? pen, Geometry geometry) => visual.Commands.Add(c =>
        {
            if (fill is not null) { using var path = geometry.Path(false); using var p = Paint(fill); c.DrawPath(path, p); }
            if (pen is not null) { using var path = geometry.Path(true); using var p = Paint(pen.Brush, pen); c.DrawPath(path, p); }
        });
        internal void DrawEllipse(SolidColorBrush? fill, Pen? pen, Point center, double rx, double ry) => visual.Commands.Add(c =>
        {
            if (fill is not null) { using var p = Paint(fill); c.DrawOval((float)center.X, (float)center.Y, (float)rx, (float)ry, p); }
            if (pen is not null) { using var p = Paint(pen.Brush, pen); c.DrawOval((float)center.X, (float)center.Y, (float)rx, (float)ry, p); }
        });
        internal void DrawLine(Pen pen, Point a, Point b) => visual.Commands.Add(c =>
        {
            using var p = Paint(pen.Brush, pen); c.DrawLine((float)a.X, (float)a.Y, (float)b.X, (float)b.Y, p);
        });
        internal void DrawImage(Imaging.BitmapSource source, Rect rect) => visual.Commands.Add(c =>
        {
            var pin = GCHandle.Alloc(source.Bytes, GCHandleType.Pinned);
            try
            {
                using var bitmap = new SKBitmap();
                if (!bitmap.InstallPixels(new(source.PixelWidth, source.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul),
                    pin.AddrOfPinnedObject(), source.PixelWidth * 4)) throw new InvalidOperationException("Cannot bind image pixels.");
                using var p = new SKPaint { IsAntialias = true, FilterQuality = SKFilterQuality.Low };
                c.DrawBitmap(bitmap, new SKRect((float)rect.X, (float)rect.Y, (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height)), p);
            }
            finally { pin.Free(); }
        });
        public void Dispose()
        {
            if (depth != 0) throw new InvalidOperationException("Unbalanced drawing stack.");
        }
    }
    internal static class PixelFormats { internal static readonly object Pbgra32 = new(); }
}

namespace System.Windows.Media.Imaging
{
    internal class BitmapSource
    {
        internal byte[] Bytes { get; private protected set; } = [];
        internal int PixelWidth { get; private protected set; }
        internal int PixelHeight { get; private protected set; }
        internal static BitmapSource Create(int w, int h, double dx, double dy, object format, object? palette, byte[] data, int stride)
        {
            var result = new BitmapSource { PixelWidth = w, PixelHeight = h, Bytes = new byte[checked(w * h * 4)] };
            for (var y = 0; y < h; y++) data.AsSpan(y * stride, w * 4).CopyTo(result.Bytes.AsSpan(y * w * 4));
            return result;
        }
        internal static BitmapSource Decode(Stream stream)
        {
            using var decoded = SKBitmap.Decode(stream) ?? throw new InvalidDataException("Image decode failed.");
            using var bgra = new SKBitmap(new SKImageInfo(decoded.Width, decoded.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
            using (var canvas = new SKCanvas(bgra)) { canvas.Clear(SKColors.Transparent); canvas.DrawBitmap(decoded, 0, 0); }
            var result = new BitmapSource { PixelWidth = bgra.Width, PixelHeight = bgra.Height, Bytes = new byte[bgra.ByteCount] };
            Marshal.Copy(bgra.GetPixels(), result.Bytes, 0, result.Bytes.Length);
            return result;
        }
        internal void CopyPixels(byte[] target, int stride, int offset)
        {
            for (var y = 0; y < PixelHeight; y++) Bytes.AsSpan(y * PixelWidth * 4, PixelWidth * 4).CopyTo(target.AsSpan(offset + y * stride));
        }
        internal void Freeze() { }
    }
    internal sealed class RenderTargetBitmap : BitmapSource
    {
        internal RenderTargetBitmap(int width, int height, double dx, double dy, object format)
        {
            PixelWidth = width; PixelHeight = height; Bytes = new byte[checked(width * height * 4)];
        }
        internal void Clear() => Array.Clear(Bytes);
        internal void Render(DrawingVisual visual)
        {
            // Retained managed pixels; surface wraps them for this draw only.
            // No per-frame native pixel allocation or persistent pinned handle.
            var pin = GCHandle.Alloc(Bytes, GCHandleType.Pinned);
            try
            {
                using var surface = SKSurface.Create(new(PixelWidth, PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul),
                    pin.AddrOfPinnedObject(), PixelWidth * 4) ?? throw new InvalidOperationException("Cannot create raster surface.");
                foreach (var command in visual.Commands) command(surface.Canvas);
                surface.Canvas.Flush();
            }
            finally { pin.Free(); }
        }
    }
}

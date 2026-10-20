// Narrow, assembly-internal geometry vocabulary for the unchanged r104 sampler.
// This is not a replacement WPF UI framework. No platform API is emulated.
namespace System.Windows
{
    internal readonly record struct Point(double X, double Y);
    internal readonly record struct Rect(double X, double Y, double Width, double Height);
}

namespace System.Windows.Media
{
    internal record struct Matrix(double M11, double M12, double M21, double M22, double OffsetX, double OffsetY)
    {
        internal static Matrix Identity => new(1, 0, 0, 1, 0, 0);
        internal readonly Point Transform(Point p) =>
            new(p.X * M11 + p.Y * M21 + OffsetX, p.X * M12 + p.Y * M22 + OffsetY);
        internal void Invert()
        {
            var d = M11 * M22 - M12 * M21;
            if (!double.IsFinite(d) || Math.Abs(d) < 1e-15)
                throw new InvalidOperationException("Singular raster transform.");
            var old = this;
            this = new(old.M22 / d, -old.M12 / d, -old.M21 / d, old.M11 / d,
                (old.M21 * old.OffsetY - old.OffsetX * old.M22) / d,
                (old.OffsetX * old.M12 - old.M11 * old.OffsetY) / d);
        }
    }
}

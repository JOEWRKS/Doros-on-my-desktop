using Dororong.Core.Geometry;
using System.Windows.Media.Imaging;

namespace Dororong.App.Controls;

internal static class SnackRunFrames
{
    // Same source-space stride as the accepted SnackRunFrames; no timing change.
    internal const double Stride = 66;
}

internal static class SnackImage
{
    private static readonly Lazy<BitmapSource> source = new(() =>
    {
        using var stream = typeof(SnackImage).Assembly.GetManifestResourceStream("snack-jewel.png")
            ?? throw new InvalidDataException("Snack resource missing.");
        return BitmapSource.Decode(stream);
    });
    internal static BitmapSource Source => source.Value;
}

// r104's head-lag field only. The legacy body-pull raster isn't a dependency
// of the high-resolution renderer. Parity tests cover this adapter separately.
internal static class SurroundingPullRenderer
{
    private static double Smooth(double t) { t = Math.Clamp(t, 0, 1); return t * t * (3 - 2 * t); }
    private static double Length(PointD p) => Math.Sqrt(p.X * p.X + p.Y * p.Y);
    private static PointD Scale(PointD p, double s) => new(p.X * s, p.Y * s);
    private static double Bump(double v, double a, double b, double c, double d) =>
        Smooth((v - a) / (b - a)) * Smooth((d - v) / (d - c));
    internal static double HeadFacePin(PointD center, PointD grab) =>
        Smooth((Length(new PointD(center.X - 8, center.Y + 16) - grab) - 3) / 9);
    internal static PointD HeadDisplacement(PointD p, PointD center, PointD grab, PointD near, PointD far, double facePin)
    {
        var sole = 1 - Smooth((p.Y - 79) / 5); if (sole == 0) return default;
        var pin = Smooth((Length(p - grab) - 3) / 9); if (pin == 0) return default;
        var accessoryLock = 1 - Smooth((p.X - (center.X + 12)) / 7) * (1 - Smooth((p.Y - (center.Y + 15)) / 14));
        var headGate = 1 - Smooth((p.Y - (center.Y + 25)) / 10);
        var h = 0d;
        if (near != default && headGate != 0 && accessoryLock != 0)
        {
            var face = Bump(p.X, center.X - 31, center.X - 23, center.X + 6, center.X + 13) *
                Bump(p.Y, center.Y + 3, center.Y + 8, center.Y + 24, center.Y + 31);
            var hair = Math.Exp(-Math.Pow((p.X - grab.X) / 25, 2) - Math.Pow((p.Y - grab.Y) / 24, 2));
            h = (hair * pin * (1 - face) + .6 * face * facePin * pin) * headGate * accessoryLock;
        }
        var body = Smooth((p.Y - (center.Y + 24)) / 12) * Bump(p.X, 12, 30, 72, 85) * sole;
        return Scale(near, -.12 * h * sole) + Scale(far, -.055 * body * pin);
    }
}

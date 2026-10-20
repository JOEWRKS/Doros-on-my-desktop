using Dororong.App.Controls;
using Dororong.Core.Geometry;
using System.Windows.Media;

namespace Dororong.Raster;

public readonly record struct RasterTransform(double M11, double M12, double M21, double M22, double X, double Y)
{
    public static RasterTransform Identity => new(1, 0, 0, 1, 0, 0);
    internal Matrix Native => new(M11, M12, M21, M22, X, Y);
}
public sealed record RasterPose
{
    public RasterTransform Transform { get; init; } = RasterTransform.Identity;
    public RasterTransform Head { get; init; } = RasterTransform.Identity;
    public double Progress { get; init; }
    public PointD Grab { get; init; } = new(37.5, 28.25);
    public PointD Near { get; init; }
    public PointD Far { get; init; }
    public double Spread { get; init; }
    public double EyeX { get; init; }
    public double EyeY { get; init; }
    public double Pull { get; init; }
    public double EyeFollow { get; init; }
    public double HairFollow { get; init; }
    public int BodyFrame { get; init; } = -1;
    public int MotionFrame { get; init; } = -1;
    public bool Closed { get; init; }
    public int EffectFrame { get; init; } = -1;
    public double MouthAge { get; init; } = -1;
    public double DroolAge { get; init; } = -1;
    public double Breathing { get; init; }
    public int Paw { get; init; }
    public double PawX { get; init; }
    public double PawY { get; init; }
    public double PawAngle { get; init; }
    public double FlutterPhase { get; init; } = -1;
}

// CPU renderer only. Native host/orchestration is a separate port gate.
// A session is single-caller; returned storage belongs to the caller and can
// be supplied on the next render. Only Extent*Extent*4 bytes are active.
public sealed class R104Raster
{
    private readonly HighResolutionGrabRaster renderer = new();
    public static int Extent(double scale, double displayScale = 1)
    {
        if (!double.IsFinite(scale) || scale < 1 || scale > 1.5) throw new ArgumentOutOfRangeException(nameof(scale));
        if (!double.IsFinite(displayScale) || displayScale <= 0 || displayScale > 4)
            throw new ArgumentOutOfRangeException(nameof(displayScale));
        return (int)Math.Ceiling(256 * scale * displayScale);
    }
    public byte[] Render(RasterPose pose, double scale, byte[]? reuse = null, double displayScale = 1)
    {
        _ = Extent(scale, displayScale);
        ArgumentNullException.ThrowIfNull(pose);
        if (pose.BodyFrame is < -1 or > 156 || pose.MotionFrame is < -1 or > 836 ||
            pose.EffectFrame is < -1 or > 836) throw new ArgumentOutOfRangeException(nameof(pose));
        ReadOnlySpan<double> values = [pose.Transform.M11,pose.Transform.M12,pose.Transform.M21,pose.Transform.M22,pose.Transform.X,pose.Transform.Y,
            pose.Head.M11,pose.Head.M12,pose.Head.M21,pose.Head.M22,pose.Head.X,pose.Head.Y,
            pose.Progress,pose.Grab.X,pose.Grab.Y,pose.Near.X,pose.Near.Y,pose.Far.X,pose.Far.Y,pose.Spread,
            pose.EyeX,pose.EyeY,pose.Pull,pose.EyeFollow,pose.HairFollow,pose.MouthAge,pose.DroolAge,pose.Breathing,
            pose.PawX,pose.PawY,pose.PawAngle,pose.FlutterPhase];
        foreach (var value in values)
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(pose), "Finite pose required.");
        if (pose.Paw is < 0 or > 2 || pose.Progress is < 0 or > 1 || pose.Spread is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(pose));
        var inverse = pose.Transform.Native;
        inverse.Invert();
        var face = new HighResolutionFacePose(pose.Head.Native, pose.EyeX, pose.EyeY, pose.Pull, pose.EyeFollow, pose.HairFollow,
            pose.BodyFrame, pose.MotionFrame, pose.Closed, pose.EffectFrame, pose.MouthAge, pose.DroolAge, pose.Breathing,
            pose.Paw, pose.PawX, pose.PawY, pose.PawAngle, pose.FlutterPhase);
        return renderer.RenderRetainedPixels(pose.Progress, inverse, pose.Grab, pose.Near, pose.Far, pose.Spread,
            scale * displayScale, face, scale, reuse, preserveCapacity: true);
    }
}

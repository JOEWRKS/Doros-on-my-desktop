using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

public readonly record struct Affine(double M11, double M12, double M21, double M22, double OffsetX, double OffsetY)
{
    public static Affine Identity => new(1, 0, 0, 1, 0, 0);
    public bool HasInverse => Math.Abs(M11 * M22 - M12 * M21) > 1e-12;
    public PointD Transform(PointD p) => new(p.X * M11 + p.Y * M21 + OffsetX, p.X * M12 + p.Y * M22 + OffsetY);
    public Affine Then(Affine next)
    {
        var o=next.Transform(new(OffsetX,OffsetY));
        return new(M11*next.M11+M12*next.M21,M11*next.M12+M12*next.M22,
            M21*next.M11+M22*next.M21,M21*next.M12+M22*next.M22,o.X,o.Y);
    }
    public Affine Inverse()
    {
        var d = M11 * M22 - M12 * M21;
        if (!HasInverse) throw new ArgumentException("Singular pose.");
        return new(M22 / d, -M12 / d, -M21 / d, M11 / d,
            (M21 * OffsetY - M22 * OffsetX) / d, (M12 * OffsetX - M11 * OffsetY) / d);
    }
}

// Portions adapted from dotnet/wpf (MIT), copyright .NET Foundation and
// Contributors. See THIRD-PARTY-WPF.txt. Reference revision:
// 65d15adece1df9a73e3d1abc11f8bbf99e9fc7a9, WpfGfx/core/geometry/{bezier,
// bezierflattener,strokefigure,utils}.cpp and core/sw/{aacoverage.h,
// swlib/aarasterizer.cpp}. This is a narrow overlay rasterizer, not WPF UI.
using SkiaSharp;

namespace Dororong.Raster;

// Source-resolution vector coverage. Geometry stays continuous; only the final
// coverage grid uses the same 28.4 / 8x8 convention as WPF's software renderer.
internal static class MilCoverage
{
    // Per-overlay scratch, never an age/pose-keyed cache. Capacity follows the
    // largest single path, not the number of frames or slider positions.
    internal sealed class Scratch
    {
        internal readonly List<List<P>> Contours = [];
        internal readonly List<List<P>> Pool = [];
        internal readonly List<(P Point, P Tangent)> Samples = [];
        internal readonly List<(SKPathVerb Verb, SKPoint A, SKPoint B, SKPoint C, SKPoint D)> Reverse = [];
        internal readonly List<(double X, int W)> Hits = [];
        internal readonly SKPoint[] Points = new SKPoint[4];
        internal int[] Row = [];
    }
    internal readonly record struct P(double X, double Y)
    {
        public static P operator +(P a, P b) => new(a.X + b.X, a.Y + b.Y);
        public static P operator -(P a, P b) => new(a.X - b.X, a.Y - b.Y);
        public static P operator *(P a, double b) => new(a.X * b, a.Y * b);
        internal double Norm => Math.Max(Math.Abs(X), Math.Abs(Y));
        internal double Length => Math.Sqrt(X * X + Y * Y);
        internal SKPoint Native => new((float)X, (float)Y);
        internal P Right => new(-Y, X);
        internal double Dot(P b) => X * b.X + Y * b.Y;
        internal double Cross(P b) => X * b.Y - Y * b.X;
    }
    private static P Point(SKPoint p) => new(p.X, p.Y);
    internal static SKPath Ellipse(double x, double y, double rx, double ry)
    {
        const double k = .5522847498307936;
        var path = new SKPath(); path.MoveTo((float)(x + rx), (float)y);
        path.CubicTo((float)(x + rx), (float)(y + ry * k), (float)(x + rx * k), (float)(y + ry), (float)x, (float)(y + ry));
        path.CubicTo((float)(x - rx * k), (float)(y + ry), (float)(x - rx), (float)(y + ry * k), (float)(x - rx), (float)y);
        path.CubicTo((float)(x - rx), (float)(y - ry * k), (float)(x - rx * k), (float)(y - ry), (float)x, (float)(y - ry));
        path.CubicTo((float)(x + rx * k), (float)(y - ry), (float)(x + rx), (float)(y - ry * k), (float)(x + rx), (float)y);
        path.Close(); return path;
    }
    // The source effects contain smooth cubic runs (or one straight line).
    // Flatten the centreline with tangents before widening, rather than asking
    // Skia to approximate an offset curve using a different error metric.
    internal static SKPath Widen(SKPath path, double thickness, bool round, Scratch? scratch = null)
    {
        scratch ??= new(); var samples = scratch.Samples; samples.Clear(); bool closed = false;
        using var iterator = path.CreateRawIterator(); var p = scratch.Points; SKPathVerb verb;
        while ((verb = iterator.Next(p)) != SKPathVerb.Done)
        {
            if (verb == SKPathVerb.Move) { if (samples.Count > 0) throw new NotSupportedException("One smooth contour expected."); }
            else if (verb == SKPathVerb.Line) { var a = Point(p[0]); var b = Point(p[1]); if (samples.Count == 0) samples.Add((a, b - a)); samples.Add((b, b - a)); }
            else if (verb == SKPathVerb.Cubic) { var a = Point(p[0]); var b = Point(p[1]); var c = Point(p[2]); var d = Point(p[3]); if (samples.Count == 0) samples.Add((a, Tangent(a, b, c, d))); FlattenTangents(samples, a, b, c, d); }
            else if (verb == SKPathVerb.Close) closed = true;
            else throw new NotSupportedException("Smooth polynomial stroke expected.");
        }
        var result = new SKPath { FillType = SKPathFillType.Winding };
        if (samples.Count < 2) return result;
        double radius = thickness * .5;
        double threshold = (radius < .25 ? -2 : 2 * Math.Pow(1 - .25 / radius, 2) - 1) * radius * radius;
        var previousPoint = samples[0].Point; var previousTangent = samples[0].Tangent;
        var radial = previousTangent * (radius / previousTangent.Length);
        var current = new[] { previousPoint - radial.Right, previousPoint + radial.Right };
        using var left = new SKPath(); using var right = new SKPath(); var rails = new[] { left, right };
        left.MoveTo(current[1].Native); right.MoveTo(current[1].Native);
        if (!closed && round) Cap(left, previousPoint, current[1], current[0], radial * -1); else left.LineTo(current[0].Native);
        void RoundTo(P next, P center, P incoming, P outgoing)
        {
            int side = incoming.Cross(outgoing) > 0 ? 0 : 1;
            var end = center + next.Right * (side == 0 ? -1 : 1); var rail = rails[side]; var start = current[side]; double dot = radial.Dot(next);
            double Distance(double d) { double a = .5 * (radius * radius + d); double den = radius * radius - a; return den <= 0 ? 0 : (4d / 3) * (radius - Math.Sqrt(a)) / Math.Sqrt(den); }
            if (dot > threshold) rail.LineTo(end.Native);
            else if (dot >= 0) { var r = Distance(dot); rail.CubicTo((start + radial * r).Native, (end - next * r).Native, end.Native); }
            else
            {
                double real = next.X * radial.X - next.Y * radial.Y, imag = next.X * radial.Y + next.Y * radial.X;
                var mid = new P(Math.Sqrt(Math.Abs(.5 * (radius * radius + real))), (imag > 0 ? 1 : -1) * Math.Sqrt(Math.Abs(.5 * (radius * radius - real))));
                if (mid.Dot(next.Right * (side == 0 ? -1 : 1)) < 0) mid *= -1;
                var r = Distance(Math.Abs(next.Dot(mid))); var middle = center + mid.Right * (side == 0 ? -1 : 1);
                rail.CubicTo((start + radial * r).Native, (middle - mid * r).Native, middle.Native);
                rail.CubicTo((middle + mid * r).Native, (end - next * r).Native, end.Native);
            }
            current[side] = end; radial = next; previousPoint = center;
        }
        void Extend(P point, P direction)
        {
            for (int side = 0; side < 2; side++)
            {
                var next = point + radial.Right * (side == 0 ? -1 : 1); var rail = rails[side]; var last = Point(rail.LastPoint);
                if ((next - last).Dot(direction) < 0)
                {
                    result.MoveTo(last.Native);
                    result.LineTo((side == 0 ? previousPoint : next).Native); result.LineTo((side == 0 ? next : previousPoint).Native); result.Close();
                    rail.LineTo(point.Native);
                }
                rail.LineTo(next.Native); current[side] = next;
            }
        }
        for (int i = 1; i < samples.Count; i++)
        {
            var (point, tangent) = samples[i]; var segment = point - previousPoint;
            var next = tangent.Length >= 1e-6 ? tangent * (radius / tangent.Length) : radial;
            if (radial.Dot(next) < threshold)
            {
                if (segment.Length >= 1e-6) RoundTo(segment * (radius / segment.Length), previousPoint, previousTangent, segment);
                Extend(point, segment); RoundTo(next, point, segment, tangent);
                if (i == samples.Count - 1) Extend(point, tangent);
            }
            else { radial = next; Extend(point, segment); }
            previousPoint = point; previousTangent = tangent;
        }
        if (!closed && round) Cap(left, previousPoint, current[0], current[1], radial); else left.LineTo(current[1].Native);
        ReverseInto(left, right, scratch); left.Close(); result.AddPath(left); return result;
    }
    private static void ReverseInto(SKPath target, SKPath source, Scratch scratch)
    {
        var segments = scratch.Reverse; segments.Clear();
        using var iterator = source.CreateRawIterator(); var p = scratch.Points; SKPathVerb verb;
        while ((verb = iterator.Next(p)) != SKPathVerb.Done) if (verb is SKPathVerb.Line or SKPathVerb.Cubic) segments.Add((verb, p[0], p[1], p[2], p[3]));
        for (int i = segments.Count - 1; i >= 0; i--) { var s = segments[i]; if (s.Verb == SKPathVerb.Line) target.LineTo(s.A); else target.CubicTo(s.C, s.B, s.A); }
    }
    private static P Tangent(P a, P b, P c, P d) => (b - a).Length > 1e-8 ? b - a : (c - a).Length > 1e-8 ? c - a : d - a;
    private static void Cap(SKPath path, P center, P start, P end, P along)
    {
        const double arc = .5522847498307933984; var mid = center + along; var across = (end - center) * arc; along *= arc;
        path.CubicTo((start + along).Native, (mid - across).Native, mid.Native);
        path.CubicTo((mid + across).Native, (end + along).Native, end.Native);
    }
    private static void FlattenTangents(List<(P Point, P Tangent)> result, P a, P b, P c, P d)
    {
        var e0 = a; var e1 = d - a; var e2 = (b - c * 2 + d) * 6; var e3 = (a - b * 2 + c) * 6; int steps = 1; double step = 1;
        void Half() { e2 = (e2 + e3) * .125; e1 = (e1 - e2) * .5; e3 *= .25; steps *= 2; step *= .5; }
        while (Math.Max(e2.Norm, e3.Norm) > 1.5 && step > 1e-3) Half();
        while (steps > 1)
        {
            e0 += e1; var t = e2; e1 += t; e2 += t - e3; e3 = t; steps--;
            result.Add((e0, e1 * 6 - e2 - e3 * 2));
            if (e2.Norm > 1.5 && step > 1e-3) Half();
            else while ((steps & 1) == 0 && e3.Norm <= .375 && (e2 * 2 - e3).Norm <= .375) { e1 = e1 * 2 + e2; e2 = (e2 * 2 - e3) * 4; e3 *= 4; steps /= 2; step *= 2; }
        }
        result.Add((d, Tangent(d, c, b, a) * -1));
    }
    private struct Basis
    {
        internal long E0, E1, E2, E3;
        internal Basis(int a, int b, int c, int d) { E0 = (long)a << 10; E1 = (long)(d - a) << 10; E2 = (long)(6 * (b - 2 * c + d)) << 10; E3 = (long)(6 * (a - 2 * b + c)) << 10; }
        internal long Error => Math.Max(Math.Abs(E2), Math.Abs(E3));
        internal long Parent => Math.Max(Math.Abs(E3), Math.Abs(2 * E2 - E3));
        internal int Value => (int)((E0 + 4096) >> 13);
        internal void Lazy(int shift) { E2 = (E2 + E3) >> 1; E1 = (E1 - (E2 >> shift)) >> 1; }
        internal void Steady(int shift) { E0 <<= 3; E1 <<= 3; if (shift < 3) { E2 <<= 3 - shift; E3 <<= 3 - shift; } else { E2 >>= shift - 3; E3 >>= shift - 3; } }
        internal void Half() { E2 = (E2 + E3) >> 3; E1 = (E1 - E2) >> 1; E3 >>= 2; }
        internal void Double() { E1 += E1 + E2; E3 <<= 2; E2 = (E2 << 3) - E3; }
        internal void Step() { E0 += E1; var t = E2; E1 += t; E2 += t - E3; E3 = t; }
    }
    private static P Fixed(SKPoint p) => new(Math.Floor((double)p.X * 16 + .5), Math.Floor((double)p.Y * 16 + .5));
    private static void Cubic(List<P> points, P a, P b, P c, P d)
    {
        var x = new Basis((int)a.X, (int)b.X, (int)c.X, (int)d.X);
        var y = new Basis((int)a.Y, (int)b.Y, (int)c.Y, (int)d.Y);
        int steps = 1, shift = 0;
        while (Math.Max(x.Error, y.Error) > (24576L << shift)) { shift += 2; x.Lazy(shift); y.Lazy(shift); steps *= 2; }
        x.Steady(shift); y.Steady(shift);
        while (steps > 0)
        {
            x.Step(); y.Step(); steps--; points.Add(new(x.Value / 16d, y.Value / 16d));
            if (steps == 0) break;
            if (Math.Max(x.Error, y.Error) > 196608) { x.Half(); y.Half(); steps *= 2; }
            while ((steps & 1) == 0 && Math.Max(x.Parent, y.Parent) <= 49152) { x.Double(); y.Double(); steps /= 2; }
        }
    }
    internal static List<List<P>> Flatten(SKPath path, Scratch scratch)
    {
        var contours = scratch.Contours; contours.Clear(); List<P>? current = null;
        using var iterator = path.CreateRawIterator(); var p = scratch.Points; SKPathVerb verb;
        while ((verb = iterator.Next(p)) != SKPathVerb.Done)
        {
            switch (verb)
            {
                case SKPathVerb.Move:
                    if (scratch.Pool.Count == contours.Count) scratch.Pool.Add([]);
                    current = scratch.Pool[contours.Count]; current.Clear(); contours.Add(current); var f = Fixed(p[0]); current.Add(f * (1 / 16d)); break;
                case SKPathVerb.Line: current!.Add(Fixed(p[1]) * (1 / 16d)); break;
                case SKPathVerb.Cubic: Cubic(current!, Fixed(p[0]), Fixed(p[1]), Fixed(p[2]), Fixed(p[3])); break;
                case SKPathVerb.Quad:
                    Cubic(current!, Fixed(p[0]), Fixed(new((p[0].X + 2 * p[1].X) / 3, (p[0].Y + 2 * p[1].Y) / 3)),
                        Fixed(new((p[2].X + 2 * p[1].X) / 3, (p[2].Y + 2 * p[1].Y) / 3)), Fixed(p[2])); break;
                case SKPathVerb.Close: break;
                default: throw new NotSupportedException("Coverage path must contain polynomial curves, not conics.");
            }
        }
        return contours;
    }
    internal static void Fill(byte[] pixels, int width, int height, SKPath path, SKColor color, Scratch? scratch = null)
    {
        if (color.Alpha != 255) throw new NotSupportedException("Reference overlays use opaque solid brushes; opacity layers stay on the image backend.");
        scratch ??= new(); var contours = Flatten(path, scratch);
        if (contours.Count == 0) return;
        double min = double.PositiveInfinity, max = double.NegativeInfinity;
        foreach (var contour in contours) foreach (var p in contour) { min = Math.Min(min, p.Y); max = Math.Max(max, p.Y); }
        if (scratch.Row.Length < width) scratch.Row = new int[width];
        var row = scratch.Row; var hits = scratch.Hits;
        for (int py = Math.Max(0, (int)Math.Floor(min)); py < Math.Min(height, (int)Math.Ceiling(max)); py++)
        {
            Array.Clear(row);
            for (int sub = 0; sub < 8; sub++)
            {
                var sy = py + sub / 8d; hits.Clear();
                foreach (var contour in contours)
                    for (int i = 0, j = contour.Count - 1; i < contour.Count; j = i++)
                    {
                        var a = contour[j]; var b = contour[i];
                        if ((a.Y <= sy && b.Y > sy) || (b.Y <= sy && a.Y > sy)) hits.Add((a.X + (sy - a.Y) * (b.X - a.X) / (b.Y - a.Y), b.Y > a.Y ? 1 : -1));
                    }
                hits.Sort((a, b) => a.X.CompareTo(b.X)); int winding = 0; double start = 0;
                foreach (var hit in hits)
                {
                    var previous = winding; winding = path.FillType == SKPathFillType.EvenOdd ? 1 - winding : winding + hit.W;
                    if (previous == 0 && winding != 0) start = hit.X;
                    else if (previous != 0 && winding == 0)
                    {
                        int left = Math.Max(0, (int)Math.Ceiling(start * 8)), right = Math.Min(width * 8, (int)Math.Ceiling(hit.X * 8));
                        if (left < right)
                        {
                            int first = left >> 3, last = (right - 1) >> 3;
                            if (first == last) row[first] += right - left;
                            else { row[first] += 8 - (left & 7); for (int x = first + 1; x < last; x++) row[x] += 8; row[last] += right - (last << 3); }
                        }
                    }
                }
            }
            for (int px = 0; px < width; px++)
            {
                var coverage = row[px]; if (coverage == 0) continue; int index = (py * width + px) * 4;
                int alpha = (color.Alpha * coverage + 32) >> 6;
                pixels[index] = Blend(color.Blue, pixels[index], coverage, alpha);
                pixels[index + 1] = Blend(color.Green, pixels[index + 1], coverage, alpha);
                pixels[index + 2] = Blend(color.Red, pixels[index + 2], coverage, alpha);
                pixels[index + 3] = (byte)Math.Min(255, alpha + (pixels[index + 3] * (255 - alpha) + 127) / 255);
            }
        }
    }
    private static byte Blend(int source, int destination, int coverage, int alpha) => (byte)Math.Min(255, ((source * coverage + 32) >> 6) + (destination * (255 - alpha) + 127) / 255);
}

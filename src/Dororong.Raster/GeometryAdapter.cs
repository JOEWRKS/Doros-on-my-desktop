// Narrow, assembly-internal geometry vocabulary for the unchanged r104 sampler.
// This is not a replacement WPF UI framework. No platform API is emulated.
namespace System.Windows
{
    internal readonly record struct Point(double X, double Y);
    internal readonly record struct Vector(double X,double Y) { internal double Length=>Math.Sqrt(X*X+Y*Y); }
    internal readonly record struct Rect(double X, double Y, double Width, double Height);
}

namespace System.Windows.Media
{
    internal record struct Matrix(double M11, double M12, double M21, double M22, double OffsetX, double OffsetY)
    {
        internal static Matrix Identity => new(1, 0, 0, 1, 0, 0);
        internal readonly bool HasInverse=>double.IsFinite(M11*M22-M12*M21)&&Math.Abs(M11*M22-M12*M21)>1e-15;
        internal readonly Vector Transform(Vector p)=>new(p.X*M11+p.Y*M21,p.X*M12+p.Y*M22);
        internal void Append(Matrix b) { var a=this;this=new(a.M11*b.M11+a.M12*b.M21,a.M11*b.M12+a.M12*b.M22,a.M21*b.M11+a.M22*b.M21,a.M21*b.M12+a.M22*b.M22,a.OffsetX*b.M11+a.OffsetY*b.M21+b.OffsetX,a.OffsetX*b.M12+a.OffsetY*b.M22+b.OffsetY); }
        internal void Translate(double x,double y)=>Append(new(1,0,0,1,x,y));
        internal void Scale(double x,double y)=>Append(new(x,0,0,y,0,0));
        internal void ScaleAt(double x,double y,double cx,double cy)=>Append(new(x,0,0,y,cx*(1-x),cy*(1-y)));
        internal void RotateAt(double degrees,double x,double y) { var r=degrees*Math.PI/180;var c=Math.Cos(r);var s=Math.Sin(r);Append(new(c,s,-s,c,x-x*c+y*s,y-x*s-y*c)); }
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

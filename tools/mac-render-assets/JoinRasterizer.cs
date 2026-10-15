using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.Core.Geometry;

// Export-only extraction of the accepted renderer's vector pass. Every motion
// frame is checked against the pinned binary before its chunk is committed.
internal sealed class JoinRasterizer
{
    private readonly DrawingVisual drawing=new();
    private readonly RenderTargetBitmap bitmap=new(128,128,96,96,PixelFormats.Pbgra32);
    private readonly Pen outline=new(new SolidColorBrush(Color.FromRgb(80,52,61)),1.15)
        {StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};
    private static Point P(PointD p)=>new(p.X,p.Y);
    internal byte[] Render(int frame,double roll)
    {
        var pose=HuntPose.At(frame/60d);var amount=pose.Amount;
        var angle=roll<0?roll*(1-amount)+Math.Max(roll,-.08)*amount:roll;
        Point Head(double x,double y,double? rotation=null)
        {var a=rotation??angle;var dx=x-43;var dy=y-66;return P(pose.Head(43+dx*Math.Cos(a)-dy*Math.Sin(a),66+dx*Math.Sin(a)+dy*Math.Cos(a)));}
        var front=Head(16,68);var mapped=pose.Map(17,68);
        var root=new Point(mapped.X+(23-mapped.X)*amount,mapped.Y+(68-mapped.Y)*amount);
        var u=Math.Min(1,amount/.2);var blend=u*u*(3-2*u);var old=pose.Map(17.2,73);
        var chest=new Point(old.X+(root.X-old.X)*blend,old.Y+(root.Y-old.Y)*blend);
        var frontMaskY=front.Y<root.Y?Math.Max(65,root.Y+1):65;
        using(var dc=drawing.RenderOpen())
        {
            dc.PushTransform(new TranslateTransform(16,16));
            var neck=new StreamGeometry();using(var c=neck.Open())
            {c.BeginFigure(Head(43,52),true,true);c.LineTo(Head(62,52),true,false);c.LineTo(P(pose.Map(62,70)),true,false);c.LineTo(P(pose.Map(43,70)),true,false);}
            neck.Freeze();dc.DrawGeometry(Brushes.White,null,neck);
            void Shape(Point start,(Point,Point,Point)[] curves,params Point[] closing)
            {
                StreamGeometry Make(bool fill)
                {var g=new StreamGeometry();using(var c=g.Open()){c.BeginFigure(start,fill,fill);foreach(var(a,b,end)in curves)c.BezierTo(a,b,end,true,false);if(fill)foreach(var p in closing)c.LineTo(p,true,false);}g.Freeze();return g;}
                dc.DrawGeometry(Brushes.White,null,Make(true));dc.DrawGeometry(null,outline,Make(false));
            }
            Shape(front,[(new(front.X-1,front.Y+(chest.Y-front.Y)*.35),new(chest.X-.7,chest.Y-1.8),chest)],P(pose.Map(46,75)),Head(47,65));
            var a=Head(69,47.5,Math.Min(angle,0));var apex=P(pose.Map(75+2*amount,59+2.5*amount));var b=P(pose.Map(69.8,74));
            var shoulder=a.Y+6;apex.Y=(apex.Y+shoulder+Math.Sqrt(Math.Pow(apex.Y-shoulder,2)+1))/2;
            Shape(a,[(new(a.X+(apex.X-a.X)*(.35+.2*amount),a.Y+(apex.Y-a.Y)*(.18-.08*amount)),new(apex.X,apex.Y-(apex.Y-a.Y)*.55),apex),
                (new(apex.X,apex.Y+(b.Y-apex.Y)*.55),new(b.X,b.Y-4),b)],P(pose.Map(59,74)),Head(59,48));
            dc.Pop();
        }
        bitmap.Clear();bitmap.Render(drawing);var pixels=new byte[65536];bitmap.CopyPixels(pixels,512,0);
        for(var y=81;y<128;y++)for(var x=0;x<68;x++)
        {if(y+.5<frontMaskY+16)continue;var coverage=x<62?1:Math.Clamp(1-(x+.5-62)/6,0,1);for(var c=0;c<4;c++){var i=(y*128+x)*4+c;pixels[i]=(byte)Math.Round(pixels[i]*(1-blend*coverage));}}
        return pixels;
    }
}

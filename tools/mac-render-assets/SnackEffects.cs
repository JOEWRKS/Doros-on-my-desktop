using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

// Export-only native vector effects from DororongPresenter.Snack.cs. Character
// pixels and animation timing are not baked or changed by this atlas.
internal sealed class SnackEffects
{
    private readonly DrawingVisual visual=new();
    private readonly RenderTargetBitmap bitmap=new(96,96,96,96,PixelFormats.Pbgra32);
    private readonly BitmapImage jewel=new(new Uri("pack://application:,,,/Dororong.App;component/Assets/snack-jewel.png"));
    private static SolidColorBrush Brush(byte r,byte g,byte b)=>new(Color.FromRgb(r,g,b));
    private static double Smooth(double u){u=Math.Clamp(u,0,1);return u*u*(3-2*u);}
    private byte[] Render(bool eating,double value,Matrix transform)
    {
        using(var dc=visual.RenderOpen())
        {
            dc.PushTransform(new MatrixTransform(transform));
            if(eating)
            {
                var open=Smooth(value/.14)*(1-Smooth((value-.30)/.18));
                if(open>.01)
                {
                    var pen=new Pen(Brush(80,52,61),.85){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
                    var g=new StreamGeometry();using(var c=g.Open())
                    {c.BeginFigure(new(27,60),true,true);c.QuadraticBezierTo(new(29,60.6),new(31.2,60),false,false);c.BezierTo(new(31,60+open*4.2),new(27.2,60+open*4.2),new(27,60),true,false);}
                    dc.DrawGeometry(Brush(109,57,72),pen,g);dc.DrawEllipse(Brush(235,145,170),null,new(29.2,60+open*2),1.1,open*.75);
                }
                if(value<.34)
                {
                    var swallow=Smooth((value-.12)/.22);var size=10*(1-swallow)+1;
                    dc.PushOpacity(1-swallow);dc.DrawImage(jewel,new(21+8*swallow-size/2,58-size/2,size,size));dc.Pop();
                }
            }
            else
            {
                var drool=new Pen(Brush(108,185,222),1.3){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
                var g=new StreamGeometry();using(var c=g.Open())
                {c.BeginFigure(new(30.8,61.5),false,false);c.BezierTo(new(32,65),new(30,67),new(31,value),true,false);}
                dc.DrawGeometry(null,drool,g);dc.DrawEllipse(Brush(174,226,242),new Pen(Brushes.SteelBlue,.45),new(31,value+1),1.3,2);
                dc.DrawLine(new Pen(Brushes.White,.45),new(30.8,value),new(30.8,value+1.5));
            }
            dc.Pop();
        }
        bitmap.Clear();bitmap.Render(visual);var pixels=new byte[36864];bitmap.CopyPixels(pixels,384,0);return pixels;
    }
    internal static void Export(string output)
    {
        var hash=SHA256.HashData(File.ReadAllBytes(typeof(DororongPresenter).Assembly.Location));
        if(Convert.ToHexString(hash)!="EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C")throw new InvalidDataException("Unapproved source.");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);using var file=new FileStream(output,FileMode.CreateNew);using var writer=new BinaryWriter(file);
        writer.Write("EFCT"u8);writer.Write(2);writer.Write(590);writer.Write(hash);var renderer=new SnackEffects();
        for(var group=0;group<590;group++)
        {
            var eating=group==589;var count=eating?48001:401;
            Matrix t;
            if(eating)t=Matrix.Identity;
            else if(group<157)t=HuntRenderer.HeadTransform(group,default);
            else {var i=group-157;t=new(1,0,0,1,0,SnackRunFrames.Bob(i%48/48d,i/48/8d));}
            using var buffer=new MemoryStream();
            using(var zip=new GZipStream(buffer,CompressionLevel.SmallestSize,true))using(var chunk=new BinaryWriter(zip))
            {
                chunk.Write(count);
                for(var i=0;i<count;i++)
                {
                    var pixels=renderer.Render(eating,eating?i/100000d:67+i/100d,t);var l=96;var r=0;var top=96;var b=0;
                    for(var y=0;y<96;y++)for(var x=0;x<96;x++)if(pixels[(y*96+x)*4+3]!=0){l=Math.Min(l,x);r=Math.Max(r,x+1);top=Math.Min(top,y);b=Math.Max(b,y+1);}
                    if(r==0){l=top=r=b=0;}
                    chunk.Write((byte)l);chunk.Write((byte)top);chunk.Write((byte)(r-l));chunk.Write((byte)(b-top));
                    for(var y=top;y<b;y++)chunk.Write(pixels,(y*96+l)*4,(r-l)*4);
                }
            }
            var bytes=buffer.ToArray();writer.Write(bytes.Length);writer.Write(bytes);
            if(group%100==0)Console.WriteLine($"Effect group {group}/589: {file.Length:N0} bytes");
        }
    }
}

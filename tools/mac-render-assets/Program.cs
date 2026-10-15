using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

internal static class Program
{
    [STAThread] public static void Main(string[] args)
    {
        if (args.Length is <1 or >2 || args.Length==2&&args[1] is not ("--perch" or "--head" or "--joins" or "--effects")) throw new ArgumentException("Pass a NEW output bank path and optional --perch/--head/--joins/--effects; never overwrite silently.");
        var output = Path.GetFullPath(args[0]);
        _ = new System.Windows.Application(); // Register WPF pack resource scheme.
        if(args.Length==2){if(args[1]=="--effects")SnackEffects.Export(output);else if(args[1]=="--joins")ExportJoins(output);else if(args[1]=="--head")ExportHead(output);else ExportPerch(output);return;}
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        using var file = new MemoryStream();
        using var gzip = new GZipStream(file, CompressionLevel.SmallestSize);
        using var writer = new BinaryWriter(gzip);
        writer.Write("JUMP"u8); writer.Write(1); writer.Write(96); writer.Write(96); writer.Write(157);
        var hash = SHA256.HashData(File.ReadAllBytes(typeof(DororongPresenter).Assembly.Location));
        writer.Write(hash);
        var renderer = new HuntRenderer();
        for (var i = 0; i <= 156; i++)
        {
            var frame = new FormatConvertedBitmap(renderer.Render(i, default, crisp: true), PixelFormats.Pbgra32, null, 0);
            var pixels = new byte[96 * 96 * 4]; frame.CopyPixels(pixels, 384, 0); writer.Write(pixels);
        }
        Console.WriteLine($"157 current native jump frames; baseline DLL SHA256 {Convert.ToHexString(hash)}");
        writer.Flush(); gzip.Dispose();
        using var destination = new FileStream(output, FileMode.CreateNew);
        destination.Write(file.ToArray());
    }
    private static void ExportJoins(string output)
    {
        var hash=SHA256.HashData(File.ReadAllBytes(typeof(DororongPresenter).Assembly.Location));
        if(Convert.ToHexString(hash)!="EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C")throw new InvalidDataException("Unapproved source.");
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        using var destination=new FileStream(output,FileMode.CreateNew);using var writer=new BinaryWriter(destination);
        writer.Write("JOIN"u8);writer.Write(2);writer.Write(157);writer.Write(2801);writer.Write(hash);
        var renderer=new HuntRenderer();
        var rasterizer=new JoinRasterizer();
        var field=typeof(HuntRenderer).GetField("_joinPixels",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
        for(var f=0;f<157;f++)
        {
            using var buffer=new MemoryStream();
            using(var zip=new GZipStream(buffer,CompressionLevel.SmallestSize,true))using(var chunk=new BinaryWriter(zip))
            for(var a=-1400;a<=1400;a++)
            {
                // EyeX forces rendering even at default roll. Capture only hidden
                // neck joins, never a quantized head, eye or whole character pose.
                var pixels=rasterizer.Render(f,a/4000d);
                if(a is -1400 or 0 or 1400)
                {
                    renderer.Render(f,new(.1,0,a/4000d));
                    if(!pixels.AsSpan().SequenceEqual((byte[])field.GetValue(renderer)!))throw new InvalidDataException($"Join source mismatch {f}/{a}");
                }
                var left=128;var right=0;var top=128;var bottom=0;
                for(var y=0;y<128;y++)for(var x=0;x<128;x++)if(pixels[(y*128+x)*4+3]!=0)
                {left=Math.Min(left,x);right=Math.Max(right,x+1);top=Math.Min(top,y);bottom=Math.Max(bottom,y+1);}
                chunk.Write((byte)left);chunk.Write((byte)top);chunk.Write((byte)(right-left));chunk.Write((byte)(bottom-top));
                for(var y=top;y<bottom;y++)chunk.Write(pixels,(y*128+left)*4,(right-left)*4);
            }
            var bytes=buffer.ToArray();writer.Write(bytes.Length);writer.Write(bytes);
            if(f%10==0)Console.WriteLine($"Join frame {f}/156, {destination.Length:N0} bytes");
        }
    }
    private static void ExportHead(string output)
    {
        using var buffer=new MemoryStream();
        using(var gzip=new GZipStream(buffer,CompressionLevel.SmallestSize,true))
        using(var writer=new BinaryWriter(gzip))
        {
            writer.Write("HEAD"u8);writer.Write(1);writer.Write(96);writer.Write(96);writer.Write(113);
            writer.Write(SHA256.HashData(File.ReadAllBytes(typeof(DororongPresenter).Assembly.Location)));
            var canonical=new BitmapImage(new Uri("pack://application:,,,/Dororong.App;component/Assets/dororong-canonical.png"));
            var frames=new SuppliedBodyDragFrames(PremultipliedFrame.From(canonical));
            for(var i=0;i<=112;i++)
            {
                var source=frames.Sample(i/112d);var registration=HeadPullAnchoring.RegistrationPoint(source);
                writer.Write(registration.X);writer.Write(registration.Y);
                var frame=new FormatConvertedBitmap(source,PixelFormats.Pbgra32,null,0);
                var pixels=new byte[36864];frame.CopyPixels(pixels,384,0);writer.Write(pixels);
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        using var destination=new FileStream(output,FileMode.CreateNew);destination.Write(buffer.ToArray());
        Console.WriteLine("Exported113 approved head-pull frames and continuous registration.");
    }
    private static void ExportPerch(string output)
    {
        using var buffer=new MemoryStream();
        using(var gzip=new GZipStream(buffer,CompressionLevel.SmallestSize,true))
        using(var writer=new BinaryWriter(gzip))
        {
            writer.Write("PRCH"u8);writer.Write(1);writer.Write(100);writer.Write(100);writer.Write(2);
            writer.Write(SHA256.HashData(File.ReadAllBytes(typeof(DororongPresenter).Assembly.Location)));
            foreach(var source in new[]{PerchExpressionFrames.Open,PerchExpressionFrames.Closed})
            {
                var frame=new FormatConvertedBitmap(source,PixelFormats.Pbgra32,null,0);
                var pixels=new byte[40000];frame.CopyPixels(pixels,400,0);writer.Write(pixels);
            }
        }
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        using var destination=new FileStream(output,FileMode.CreateNew);destination.Write(buffer.ToArray());
        Console.WriteLine("Exported accepted repaired perch open/closed pixels.");
    }
}

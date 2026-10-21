using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Raster;
using SkiaSharp;

internal static class Program
{
    [STAThread] static int Main(string[] args)
    {
        if(args.Length==3&&args[0]=="--benchmark")return Benchmark(args[1],args[2]);
        if(args.Length>0)return Sweep(args);
        int primitiveFailures=0;
        foreach(var (name,geometry) in new (string,Geometry)[]{
            ("rect",new RectangleGeometry(new Rect(12.3,14.6,6.2,8.7))),
            ("triangle",Geometry.Parse("M12.3,14.6 L19.7,17.6 11.2,29.3Z")),
            ("drool",Geometry.Parse("M30.8,61.5 C32,65 30,67 31,69.66997630031181")),
            ("ellipse",new EllipseGeometry(new Point(31,70.66997630031181),1.3,2))})
        foreach(var stroke in new[]{false,true})
        {
            if(stroke&&name is "rect" or "triangle")continue; // target strokes are smooth, not polygon joins
            var color=Color.FromRgb(108,185,222);var brush=new SolidColorBrush(color);
            var pen=new Pen(brush,1.3){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round};
            var visual=new DrawingVisual();using(var dc=visual.RenderOpen()){
                dc.PushTransform(new ScaleTransform(225d/96,225d/96));dc.DrawGeometry(stroke?null:brush,stroke?pen:null,geometry);dc.Pop();}
            var bitmap=new RenderTargetBitmap(225,225,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);var expected=new byte[225*225*4];bitmap.CopyPixels(expected,900,0);
            // Independently declared primitives exercise both portable coverage
            // and widening against a real WPF RenderTargetBitmap.
            using var original=Convert(PathGeometry.CreateFromGeometry(geometry));original.Transform(SKMatrix.CreateScale(225f/96,225f/96));
            using var path=stroke?MilCoverage.Widen(original,1.3*225/96,true):new SKPath(original);
            var actual=new byte[expected.Length];MilCoverage.Fill(actual,225,225,path,new SKColor(108,185,222));
            int count=0,max=0;long sum=0;for(int i=0;i<actual.Length;i++){var d=Math.Abs(actual[i]-expected[i]);if(d>0)count++;max=Math.Max(max,d);sum+=d;}
            Console.WriteLine($"{name} stroke={stroke}: channels={count} max={max} sum={sum}");
            if(count>0)primitiveFailures++;
        }
        return primitiveFailures==0?0:2;
    }
    const BindingFlags Hidden=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    static int Benchmark(string path,string output)
    {
        if(File.Exists(output))throw new IOException("Use fresh output.");
        var a=Assembly.LoadFile(Path.GetFullPath(path));var neck=Buffer(a,"HighResolutionHuntNeck");var drool=Buffer(a,"HighResolutionSnackEffects");
        var poses=Enumerable.Range(0,157).Select(i=>Face(a,i,-20+40*(i/156d))).ToArray();
        var times=new List<double>();float[]? neckStorage=null,droolStorage=null;bool sameStorage=true;
        var initial=GC.GetTotalAllocatedBytes(true);var watch=System.Diagnostics.Stopwatch.StartNew();
        for(int i=0;i<2200;i++)
        {
            var before=watch.ElapsedTicks;
            var n=(float[])neck.Item2.Invoke(neck.Item1,[poses[i%157]])!;
            var d=(float[])drool.Item2.Invoke(drool.Item1,[-1d,i/60d])!;
            var elapsed=(watch.ElapsedTicks-before)*1000d/System.Diagnostics.Stopwatch.Frequency;
            if(neckStorage is not null)sameStorage&=ReferenceEquals(neckStorage,n)&&ReferenceEquals(droolStorage,d);
            neckStorage=n;droolStorage=d;
            if(i>=200)times.Add(elapsed);
            if(i==199)initial=GC.GetTotalAllocatedBytes(true);
        }
        var allocated=GC.GetTotalAllocatedBytes(true)-initial;times.Sort();
        var result=new{path,sha256=System.Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))),pairs=times.Count,sameStorage,
            medianMs=times[times.Count/2],p95Ms=times[(int)(times.Count*.95)],p99Ms=times[(int)(times.Count*.99)],maxMs=times[^1],allocatedBytes=allocated,bytesPerPair=allocated/(double)times.Count,
            scope="Windows-hosted source effect render only; not native Mac timing or process leak proof; no forced GC"};
        File.WriteAllText(output,JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine(JsonSerializer.Serialize(result));return sameStorage?0:2;
    }
    static int Sweep(string[] args)
    {
        if(args.Length!=2)throw new ArgumentException("Accepted DLL and fresh output JSON required.");
        const string accepted="374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322";
        if(System.Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[0])))!=accepted)throw new InvalidDataException("Not r104.");
        if(File.Exists(args[1]))throw new IOException("Use fresh output.");
        _=new Application { ShutdownMode=ShutdownMode.OnExplicitShutdown };
        var reference=Assembly.LoadFrom(Path.GetFullPath(args[0]));var portable=typeof(R104Raster).Assembly;
        var results=new List<object>();var failures=0;
        var expectedNeck=Buffer(reference,"HighResolutionHuntNeck");var actualNeck=Buffer(portable,"HighResolutionHuntNeck");
        var expectedDrool=Buffer(reference,"HighResolutionSnackEffects");var actualDrool=Buffer(portable,"HighResolutionSnackEffects");
        void Compare(string name,object[] expectedArgs,object[] actualArgs,(object Instance,MethodInfo Get) e,(object Instance,MethodInfo Get) a)
        {
            var expected=(float[])e.Get.Invoke(e.Instance,expectedArgs)!;var actual=(float[])a.Get.Invoke(a.Instance,actualArgs)!;
            int changed=0,max=0;long sum=0;var first=new List<object>();
            for(int i=0;i<expected.Length;i++){int d=(int)Math.Abs(expected[i]-actual[i]);if(d>0){changed++;if(first.Count<4)first.Add(new{x=(i/4)%225,y=i/900,c=i%4,e=expected[i],a=actual[i]});}max=Math.Max(max,d);sum+=d;}
            if(changed>0){failures++;if(failures<=12)Console.WriteLine($"{name}: channels={changed} max={max} sum={sum}");}
            results.Add(new{name,changed,max,sum,first,expectedSha256=System.Convert.ToHexString(SHA256.HashData(System.Runtime.InteropServices.MemoryMarshal.AsBytes(expected.AsSpan())))});
        }
        foreach(var frame in Enumerable.Range(0,157))
        foreach(var angle in new[]{-20d,-10,-4.583662361046586,0,7.3,10,20})
        {
            Compare($"neck-{frame}-{angle}",[Face(reference,frame,angle)],[Face(portable,frame,angle)],expectedNeck,actualNeck);
        }
        var random=new Random(104);
        for(int i=0;i<1000;i++)
        {
            int frame=random.Next(157);double angle=random.NextDouble()*40-20;
            Compare($"neck-random-{frame}-{angle:R}",[Face(reference,frame,angle)],[Face(portable,frame,angle)],expectedNeck,actualNeck);
        }
        // Includes real 60Hz ages, arbitrary phase, extrema and late-running effects.
        foreach(var age in Enumerable.Range(0,600).Select(i=>i/60d).Concat(Enumerable.Range(0,401).Select(i=>i*Math.PI/800)).Concat(new[]{10000.123,1e6+.7}).Concat(Enumerable.Range(0,1000).Select(_=>random.NextDouble()*100000)))
            Compare($"drool-{age:R}",[-1d,age],[-1d,age],expectedDrool,actualDrool);
        File.WriteAllText(args[1],JsonSerializer.Serialize(new{accepted,frames=results.Count,failures,nativeMacExecution="NOT_TESTED",results},new JsonSerializerOptions{WriteIndented=true}));
        Console.WriteLine($"Frames={results.Count}, failures={failures}");return failures==0?0:2;
    }
    static (object,MethodInfo) Buffer(Assembly a,string name){var type=a.GetType("Dororong.App.Controls."+name+"+Buffer",true)!;return(Activator.CreateInstance(type,Hidden,null,[],null)!,type.GetMethod("Get",Hidden)!);}
    static object Face(Assembly a,int frame,double degrees)
    {
        var gazeType=a.GetType("Dororong.App.Controls.HuntGazePose",true)!;
        var gaze=Activator.CreateInstance(gazeType,Hidden,null,[0d,0d,degrees*Math.PI/180],null)!;
        var head=a.GetType("Dororong.App.Controls.HuntRenderer",true)!.GetMethod("HeadTransform",Hidden)!.Invoke(null,[frame,gaze])!;
        var face=a.GetType("Dororong.App.Controls.HighResolutionFacePose",true)!;
        return Activator.CreateInstance(face,Hidden,null,[head,0d,0d,0d,0d,0d,frame,-1,false,-1,-1d,-1d,0d,0,0d,0d,0d,-1d],null)!;
    }
    static SKPath Convert(PathGeometry geometry)
    {
        var result=new SKPath{FillType=geometry.FillRule==FillRule.EvenOdd?SKPathFillType.EvenOdd:SKPathFillType.Winding};
        foreach(var f in geometry.Figures){result.MoveTo((float)f.StartPoint.X,(float)f.StartPoint.Y);
            foreach(var s in f.Segments)switch(s){
                case LineSegment l:result.LineTo((float)l.Point.X,(float)l.Point.Y);break;
                case PolyLineSegment l:foreach(var p in l.Points)result.LineTo((float)p.X,(float)p.Y);break;
                case BezierSegment b:result.CubicTo((float)b.Point1.X,(float)b.Point1.Y,(float)b.Point2.X,(float)b.Point2.Y,(float)b.Point3.X,(float)b.Point3.Y);break;
                case PolyBezierSegment b:for(int i=0;i<b.Points.Count;i+=3)result.CubicTo((float)b.Points[i].X,(float)b.Points[i].Y,(float)b.Points[i+1].X,(float)b.Points[i+1].Y,(float)b.Points[i+2].X,(float)b.Points[i+2].Y);break;
                default:throw new NotSupportedException(s.GetType().Name);
            }
            if(f.IsClosed)result.Close();
        }
        return result;
    }
}

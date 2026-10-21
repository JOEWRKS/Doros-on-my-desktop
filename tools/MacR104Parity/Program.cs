using System.Reflection;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Geometry;
using Dororong.Raster;

internal static class Program
{
    private const BindingFlags Hidden = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private const string AcceptedHash = "374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322";
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length is <2 or >3 || (args.Length==3 && args[2]!="vectors")) throw new ArgumentException("Usage: MacR104Parity ACCEPTED_APP_DLL NEW_OUTPUT_DIRECTORY [vectors]");
        var vectors=args.Length==3;
        var path = Path.GetFullPath(args[0]); var output = Path.GetFullPath(args[1]);
        var hash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        if (hash != AcceptedHash) throw new InvalidOperationException("Not the accepted r104 assembly.");
        if (Directory.Exists(output)) throw new IOException("Use a fresh output directory.");
        Directory.CreateDirectory(output);
        _ = new System.Windows.Application { ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown };
        var assembly = Assembly.LoadFrom(path);
        var type = assembly.GetType("Dororong.App.Controls.HighResolutionGrabRaster", true)!;
        var faceType = assembly.GetType("Dororong.App.Controls.HighResolutionFacePose", true)!;
        var source = Activator.CreateInstance(type, Hidden, null, [], null)!;
        var render = type.GetMethod("RenderRetainedPixels", Hidden)!;
        var portable = new R104Raster();
        var tests = new (string Name, RasterPose Pose, bool Vector)[]
        {
            ("neutral", new(), false),
            ("closed", new() { Closed = true }, false),
            ("gaze-down", new() { EyeX = .85, EyeY = .65 }, false),
            ("cheek", new() { Pull = 20, EyeX = -.6, EyeY = .4 }, false),
            ("walk", new() { MotionFrame = 240 }, false),
            ("sit", new() { MotionFrame = 321 }, false),
            ("sleep", new() { MotionFrame = 770, EffectFrame = 835, Closed = true }, false),
            ("release", new() { Progress = .6, Transform = Rotation(21), Near = new(3,-2), Far = new(-2,1) }, false),
            ("perch-paw", new() { MotionFrame = 836, Paw = 1, PawX = 12, PawY = 20 }, false),
            ("flutter", new() { Progress = 1, FlutterPhase = .2 }, true),
            ("neck", new() { BodyFrame = 60 }, true),
            ("eat", new() { Closed = true, MouthAge = .18 }, true),
            ("eat-runtime-60hz", new() { Closed = true, MouthAge = 11/60d }, true),
            ("drool", new() { DroolAge = .7 }, true)
        };
        if(vectors)
        {
            var extended=new List<(string Name,RasterPose Pose,bool Vector)>();
            var flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static;
            var gazeType=assembly.GetType("Dororong.App.Controls.HuntGazePose",true)!;
            var headTransform=assembly.GetType("Dororong.App.Controls.HuntRenderer",true)!.GetMethod("HeadTransform",flags)!;
            foreach(var frame in new[]{16,25,40,60,125,140,155})
            foreach(var angle in new[]{-20d,-4.583662361046586,0,20})
            {
                var gaze=Activator.CreateInstance(gazeType,Hidden,null,[0d,0d,angle*Math.PI/180],null)!;
                var head=(Matrix)headTransform.Invoke(null,[frame,gaze])!;
                extended.Add(($"neck-{frame}-{angle}",new(){BodyFrame=frame,Head=new(head.M11,head.M12,head.M21,head.M22,head.OffsetX,head.OffsetY)},true));
            }
            foreach(var age in new[]{0d,.7,Math.PI/8,3*Math.PI/8,1.234567,10.233,10000.123})
                extended.Add(($"drool-{age:R}",new(){DroolAge=age},true));
            tests=extended.ToArray();
        }
        var results = new List<object>(); var failures = 0; var vectorDifferences = 0;
        foreach(var density in vectors?new[]{1d,2}:new[]{1d})
        foreach (var scale in new[] { 1d, 1.125, 1.25, 1.375, 1.5 })
        foreach (var (name, pose, vector) in tests)
        {
            var forward = new Matrix(pose.Transform.M11,pose.Transform.M12,pose.Transform.M21,pose.Transform.M22,pose.Transform.X,pose.Transform.Y);
            forward.Invert();
            var h = pose.Head;
            var face = Activator.CreateInstance(faceType, Hidden, null,
                [new Matrix(h.M11,h.M12,h.M21,h.M22,h.X,h.Y),pose.EyeX,pose.EyeY,pose.Pull,pose.EyeFollow,pose.HairFollow,
                    pose.BodyFrame,pose.MotionFrame,pose.Closed,pose.EffectFrame,pose.MouthAge,pose.DroolAge,pose.Breathing,
                    pose.Paw,pose.PawX,pose.PawY,pose.PawAngle,pose.FlutterPhase], null)!;
            var expected = (byte[])render.Invoke(source,
                [pose.Progress,forward,pose.Grab,pose.Near,pose.Far,pose.Spread,scale*density,face,scale,null,true])!;
            var actual = portable.Render(pose, scale,displayScale:density);
            var count = R104Raster.Extent(scale,density); count *= count * 4;
            var changed = 0; var max = 0; long absolute = 0;
            for (var i=0;i<count;i++)
            {
                var error = Math.Abs(expected[i]-actual[i]);
                if (error!=0) changed++;
                max = Math.Max(max,error); absolute += error;
            }
            if (changed > 0) { if(vector)vectorDifferences++; else failures++; }
            if (vector && scale == 1.5)
            {
                SavePixels(Path.Combine(output,name+$"-{density}x-wpf.png"),expected,R104Raster.Extent(scale,density));
                SavePixels(Path.Combine(output,name+$"-{density}x-portable.png"),actual,R104Raster.Extent(scale,density));
            }
            results.Add(new { name, scale, density, vector, changedChannels=changed, maxError=max,
                meanAbsoluteError=absolute/(double)count, expectedSha256=Convert.ToHexString(SHA256.HashData(expected.AsSpan(0,count))),
                actualSha256=Convert.ToHexString(SHA256.HashData(actual.AsSpan(0,count))) });
            if(!vectors||changed>0)Console.WriteLine($"{name} {scale} density={density}: changed={changed} max={max} vector={vector}");
        }
        File.WriteAllText(Path.Combine(output,"summary.json"), JsonSerializer.Serialize(new
        {
            acceptedDll=path, hash, sourcePixelFailures=failures, vectorDifferingFrames=vectorDifferences,
            nativeMacExecution="NOT_TESTED", hostIntegration="CONNECTED_NOT_NATIVE_VERIFIED", results
        },new JsonSerializerOptions { WriteIndented=true }));
        Console.WriteLine($"{results.Count} frames; source failures={failures}; vector differences={vectorDifferences}");
        // A vector mismatch is an open parity gate, not a green overall run.
        return failures > 0 ? 1 : vectorDifferences > 0 ? 2 : 0;
    }
    private static RasterTransform Rotation(double degrees)
    {
        var m = Matrix.Identity; m.RotateAt(degrees,48,48);
        return new(m.M11,m.M12,m.M21,m.M22,m.OffsetX,m.OffsetY);
    }
    private static void SavePixels(string path,byte[] bytes,int extent)
    {
        var image=BitmapSource.Create(extent,extent,96,96,PixelFormats.Pbgra32,null,bytes,extent*4);
        var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream=File.Create(path);encoder.Save(stream);
    }
}

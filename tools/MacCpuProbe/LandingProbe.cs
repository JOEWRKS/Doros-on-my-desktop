using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Portable.Desktop;

// Deterministic release/contact/spread workload, not a native Mac benchmark.
internal static class LandingProbe
{
    internal static void Run(string[] args)
    {
        var directory=Path.GetFullPath(args[0]);var output=Path.GetFullPath(args[1]);
        if(File.Exists(output))throw new IOException("Fresh output required.");
        var context=new PortContext(directory);var mac=context.LoadFromAssemblyPath(Path.Combine(directory,"Dororong.Mac.dll"));
        const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
        var type=mac.GetType("Dororong.Mac.Rendering.R104DesktopController",true)!;
        var advance=type.GetMethod("Advance",flags)!;
        var results=new List<object>();
        using var process=Process.GetCurrentProcess();
        var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1470,956))],[]),SceneHealth.Fresh);
        foreach(var size in new[]{100,125,150})foreach(var density in new[]{1d,2d}){
            var c=Activator.CreateInstance(type,flags,null,[new PointD(400,300),new RandomSource(.99),size],null)!;
            object? Get(string name)=>type.GetProperty(name,flags)!.GetValue(c);
            void Tick(PointerSample p=default,bool down=false)=>advance.Invoke(c,[TimeSpan.FromMilliseconds(16),p,down,scene]);
            type.GetMethod("SetAutomaticSleep",flags)!.Invoke(c,[false]);type.GetProperty("Density",flags)!.SetValue(c,density);
            for(var i=0;i<100;i++)Tick();
            var presenter=type.GetField("presenter",flags)!.GetValue(c)!;
            var facePoint=presenter.GetType().GetMethod("HighResolutionFaceToPresenter",flags)!;
            var source=Activator.CreateInstance(facePoint.GetParameters()[0].ParameterType,[37.5,28.25])!;
            var rows=new List<object>();
            for(var cycle=0;cycle<3;cycle++){
                var local=facePoint.Invoke(presenter,[source])!;
                var cursor=(PointD)Get("Position")!+new PointD((double)local.GetType().GetProperty("X")!.GetValue(local)!, (double)local.GetType().GetProperty("Y")!.GetValue(local)!);
                if(!(bool)type.GetMethod("TryBeginInteraction",flags)!.Invoke(c,[cursor])!)throw new Exception("Head capture failed.");
                Tick(new(true,cursor),true);
                for(var i=0;i<45;i++)Tick(new(true,cursor+new PointD(Math.Sin(i*.4)*55,-i*30)),true);
                if(!(bool)Get("Capturing")!)throw new Exception("Capture not active.");
                for(var frame=0;frame<160;frame++){
                    var allocated=GC.GetTotalAllocatedBytes(false);var cpu=process.TotalProcessorTime;var start=Stopwatch.GetTimestamp();Tick();
                    var ms=Stopwatch.GetElapsedTime(start).TotalMilliseconds;
                    var cpuMs=(process.TotalProcessorTime-cpu).TotalMilliseconds;
                    var pixels=(byte[])Get("Pixels")!;var extent=(int)Get("Extent")!;
                    var matrix=presenter.GetType().GetField("_hrForward",flags)!.GetValue(presenter)!;
                    var raster=presenter.GetType().GetField("_hrRaster",flags)!.GetValue(presenter)!;
                    var cost=raster.GetType().GetProperty("LastCost",flags)!.GetValue(raster)!;
                    rows.Add(new{cycle,frame,ms,cpuMs,allocated=GC.GetTotalAllocatedBytes(false)-allocated,
                        m12=matrix.GetType().GetProperty("M12")!.GetValue(matrix),m21=matrix.GetType().GetProperty("M21")!.GetValue(matrix),
                        preparation=cost.GetType().GetField("Item1")!.GetValue(cost),sampling=cost.GetType().GetField("Item2")!.GetValue(cost),
                        position=Get("Position"),platform=Get("Platform"),geometry=Get("Geometry")?.ToString(),
                        hash=Convert.ToHexString(SHA256.HashData(pixels.AsSpan(0,extent*extent*4)))});
                }
            }
            if(!rows.Any(r=>JsonSerializer.Serialize(r).Contains("\"IsExtremeLanding\":true")))throw new Exception("Missing extreme landing; workload is incomplete.");
            ((IDisposable)c).Dispose();results.Add(new{size,density,rows});
            Console.WriteLine($"Landing workload size={size} density={density}: {rows.Count} frames");
        }
        Directory.CreateDirectory(Path.GetDirectoryName(output)!);
        File.WriteAllText(output,JsonSerializer.Serialize(new{nativeMac="NOT_TESTED",stepMs=16,results}));
    }
}

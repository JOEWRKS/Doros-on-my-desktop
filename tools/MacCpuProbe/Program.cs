using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text.Json;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;

// Same deterministic host workload for an immutable test02 or a new build.
// This does not simulate native Quartz cost or claim Apple Silicon performance.
if(args.Length>=3&&args[2]=="soak") { HostSoak.Run(args);return; }
if(args.Length==3&&args[2]=="landing") { LandingProbe.Run(args);return; }
if(args.Length is <2 or >3 || args.Length==3&&args[2]!="memory")throw new ArgumentException("Mac assembly directory, fresh JSON output and optional memory mode required.");
var directory=Path.GetFullPath(args[0]);var output=Path.GetFullPath(args[1]);
if(File.Exists(output))throw new IOException("Fresh output required.");
var context=new PortContext(directory);
var mac=context.LoadFromAssemblyPath(Path.Combine(directory,"Dororong.Mac.dll"));
var raster=context.LoadFromAssemblyPath(Path.Combine(directory,"Dororong.Raster.dll"));
const BindingFlags flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
var type=mac.GetType("Dororong.Mac.Rendering.R104DesktopController",true)!;
var advance=type.GetMethod("Advance",flags)!;var geometry=type.GetProperty("Geometry",flags)!;
var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],[]),SceneHealth.Fresh);
var results=new List<object>();
if(args.Length==3){
 var controller=Activator.CreateInstance(type,flags,null,[new PointD(400,300),new RandomSource(.99),150],null)!;
 type.GetMethod("SetAutomaticSleep",flags)!.Invoke(controller,[false]);type.GetProperty("Density",flags)!.SetValue(controller,2d);
 void Tick()=>advance.Invoke(controller,[TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene]);
 for(var i=0;i<600;i++)Tick();
 var presenter=type.GetField("presenter",flags)!.GetValue(controller)!;
 var sampler=presenter.GetType().GetField("_hrRaster",flags)!.GetValue(presenter)!;
 var pixels=type.GetProperty("Pixels",flags)!;var first=(byte[])pixels.GetValue(controller)!;
 var scratch=sampler.GetType().GetField("outputBuffer",flags)!;
 using var process=Process.GetCurrentProcess();
 var started=Stopwatch.StartNew();var initialAllocated=GC.GetTotalAllocatedBytes(false);var rows=new List<object>();
 for(var batch=0;batch<12;batch++){
  var allocated=GC.GetTotalAllocatedBytes(false);var cpu=process.TotalProcessorTime;var timer=Stopwatch.StartNew();
  for(var i=0;i<300;i++)Tick();
  if(!ReferenceEquals(first,pixels.GetValue(controller)))throw new Exception("Owned frame replaced after maximum-size warmup.");
  process.Refresh();
  rows.Add(new{batch,elapsedMs=started.Elapsed.TotalMilliseconds,batchMs=timer.Elapsed.TotalMilliseconds,
   cpuMs=(process.TotalProcessorTime-cpu).TotalMilliseconds,privateBytes=process.PrivateMemorySize64,
   allocatedBytes=GC.GetTotalAllocatedBytes(false)-allocated,totalAllocatedBytes=GC.GetTotalAllocatedBytes(false)-initialAllocated,
   frameCapacity=first.Length,scratchCapacity=((byte[])scratch.GetValue(sampler)!).Length,
   generation0=GC.CollectionCount(0),generation1=GC.CollectionCount(1),generation2=GC.CollectionCount(2)});
 }
 ((IDisposable)controller).Dispose();
 Directory.CreateDirectory(Path.GetDirectoryName(output)!);
 File.WriteAllText(output,JsonSerializer.Serialize(new{nativeMac="NOT_TESTED",mode="host simulated 3600 ticks after600 warmup; no forced GC",rows},new JsonSerializerOptions{WriteIndented=true}));
 Console.WriteLine($"Memory probe:3600 ticks, stable frame identity/capacity={first.Length}, elapsed={started.Elapsed.TotalSeconds:F2}s");
 return;
}
foreach(var scenario in new[]{"autonomous","idle","seated"})foreach(var size in new[]{100,150})foreach(var density in new[]{1d,2d}){
 var controller=Activator.CreateInstance(type,flags,null,[new PointD(400,300),new RandomSource(scenario=="autonomous"?.5:.99),size],null)!;
 type.GetMethod("SetAutomaticSleep",flags)!.Invoke(controller,[false]);
 type.GetProperty("Density",flags)!.SetValue(controller,density);
 var rows=new List<object>();
 for(var i=0;i<90;i++)advance.Invoke(controller,[TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene]);
 if(scenario=="seated"){
  type.GetMethod("ToggleSit",flags)!.Invoke(controller,[]);
  for(var i=0;i<90;i++)advance.Invoke(controller,[TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene]);
 }
 var times=new List<double>();var frames=0L;var previous=Convert.ToInt64(type.GetProperty("FrameRevision",flags)!.GetValue(controller));
 using var process=Process.GetCurrentProcess();var cpu=process.TotalProcessorTime;var allocated=GC.GetTotalAllocatedBytes(false);
 var wall=Stopwatch.StartNew();
 for(var i=0;i<240;i++){
  var start=Stopwatch.GetTimestamp();
  advance.Invoke(controller,[TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene]);
  times.Add(Stopwatch.GetElapsedTime(start).TotalMilliseconds);
  var revision=Convert.ToInt64(type.GetProperty("FrameRevision",flags)!.GetValue(controller));frames+=revision-previous;previous=revision;
  var pixels=(byte[])type.GetProperty("Pixels",flags)!.GetValue(controller)!;
  var extent=(int)type.GetProperty("Extent",flags)!.GetValue(controller)!;
  rows.Add(new{frame=i,position=type.GetProperty("Position",flags)!.GetValue(controller),geometry=geometry.GetValue(controller)?.ToString(),
   hash=Convert.ToHexString(SHA256.HashData(pixels.AsSpan(0,extent*extent*4)))});
 }
 wall.Stop();var cpuMs=(process.TotalProcessorTime-cpu).TotalMilliseconds;var bytes=GC.GetTotalAllocatedBytes(false)-allocated;
 var first=geometry.GetValue(controller);var scan=Stopwatch.StartNew();
 for(var i=0;i<1000;i++)if(!Equals(first,geometry.GetValue(controller)))throw new Exception("Geometry mutated without a frame.");
 scan.Stop();times.Sort();
 results.Add(new{scenario,size,density,frames,wallMs=wall.Elapsed.TotalMilliseconds,cpuMs,allocatedBytes=bytes,
  tickMedian=times[times.Count/2],tickP95=times[(int)(times.Count*.95)],tickMax=times[^1],repeatedGeometry1000Ms=scan.Elapsed.TotalMilliseconds,rows});
 ((IDisposable)controller).Dispose();
 Console.WriteLine($"{scenario} size={size} density={density} frames={frames} median={times[times.Count/2]:F3}ms geometry1000={scan.Elapsed.TotalMilliseconds:F2}ms");
}
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
File.WriteAllText(output,JsonSerializer.Serialize(new{host=System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
 nativeMac="NOT_TESTED",assembly=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory,"Dororong.Raster.dll")))),results},new JsonSerializerOptions{WriteIndented=true}));
sealed class RandomSource(double value):IRandomSource{public double NextUnit()=>value;}
sealed class PortContext(string directory):AssemblyLoadContext {
 protected override Assembly? Load(AssemblyName name)=>name.Name is "Dororong.Mac" or "Dororong.Raster"
  ?LoadFromAssemblyPath(Path.Combine(directory,name.Name+".dll")):null;
}

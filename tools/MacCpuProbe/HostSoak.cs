using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;

// Test driver only. No native desktop/input APIs, forced GC, heap dump or trim.
// One controller for the entire run. JSONL streams to disk, not an in-memory history.
internal static class HostSoak {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 static object? Get(object o,string n)=>o.GetType().GetProperty(n,F)?.GetValue(o)??o.GetType().GetField(n,F)?.GetValue(o);
 static object? Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,F)!.Invoke(o,a);
 static string Hash(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p)));
 static void Save(string p,object value)=>File.WriteAllText(p,JsonSerializer.Serialize(value,new JsonSerializerOptions{WriteIndented=true}));
 public static void Run(string[] args) {
  if(args.Length!=5)throw new ArgumentException("ASSEMBLY_DIR FRESH_DIR soak MINUTES paced|smoke");
  var assemblyDir=Path.GetFullPath(args[0]);var dir=Path.GetFullPath(args[1]);
  var minutes=double.Parse(args[3],System.Globalization.CultureInfo.InvariantCulture);
  if(minutes<=0||minutes>240||args[4] is not ("paced" or "smoke"))throw new ArgumentException("Invalid bounded run.");
  if(Directory.Exists(dir))throw new IOException("Fresh directory required.");
  Directory.CreateDirectory(dir);var paced=args[4]=="paced";
  using var process=Process.GetCurrentProcess();
  Save(Path.Combine(dir,"run.json"),new{pid=Environment.ProcessId,startUtc=process.StartTime.ToUniversalTime(),startTicks=process.StartTime.ToUniversalTime().Ticks,
   minutes,paced,mode=paced?"Windows host real-wall-clock, 16ms simulation steps; no catch-up":"unpaced harness smoke only",
   nativeMac="NOT_TESTED",nativeWindowsPet="NOT_STARTED_OR_CHANGED",assemblyDir,
   macHash=Hash(Path.Combine(assemblyDir,"Dororong.Mac.dll")),rasterHash=Hash(Path.Combine(assemblyDir,"Dororong.Raster.dll")),
   warmupSeconds=120,cycleSimulatedSeconds=120,recoverySeconds=Math.Min(600,minutes*60/6),sampleSeconds=10,
   limits="Controller/raster only, no native menus/window/upload/GPU. Natural GC only. Sleep or gaps invalidate continuous-run inference."});
  using var samples=new StreamWriter(Path.Combine(dir,"samples.jsonl")){AutoFlush=true};
  using var events=new StreamWriter(Path.Combine(dir,"events.jsonl")){AutoFlush=true};
  void Event(object v)=>events.WriteLine(JsonSerializer.Serialize(v));
  var watch=Stopwatch.StartNew();object? c=null;string reason="completed";string? error=null;
  var ticks=0L;var sizes=0L;var sizeApplied=0L;var sizeRejected=0L;var head=0L;var cheek=0L;var released=0L;var eating=0L;var snackStarts=0L;var sit=0L;
  var faults=0;var gapMax=0d;var tickMax=0d;var samplesCount=0;var cycles=0L;var lastSnack="None";var wasCaptured=false;
  var phase="warmup";var cyclePhase="";var simSeconds=0d;var nextSample=0d;var lastLoop=0d;var requested=-1;var density=2d;
  try {
   var context=new PortContext(assemblyDir);var mac=context.LoadFromAssemblyPath(Path.Combine(assemblyDir,"Dororong.Mac.dll"));
   var type=mac.GetType("Dororong.Mac.Rendering.R104DesktopController",true)!;
   c=Activator.CreateInstance(type,F,null,[new PointD(400,300),new RandomSource(.5),150],null)!;
   type.GetEvent("Faulted",F)!.GetAddMethod(true)!.Invoke(c,[new Action<Exception>(e=>{faults++;Event(new{utc=DateTimeOffset.UtcNow,eventType="controllerFault",error=e.ToString()});})]);
   Call(c,"SetAutomaticSleep",false);type.GetProperty("Density",F)!.SetValue(c,density);
   var presenter=Get(c,"presenter")!;var sampler=Get(presenter,"_hrRaster")!;
   var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],[]),SceneHealth.Fresh);
   var faceMap=presenter.GetType().GetMethod("HighResolutionFaceToPresenter",F)!;
   var pointType=faceMap.GetParameters()[0].ParameterType;
   PointD Position()=>(PointD)Get(c,"Position")!;
   PointD Face(double x,double y) {var p=faceMap.Invoke(presenter,[Activator.CreateInstance(pointType,[x,y])])!;return Position()+new PointD((double)Get(p,"X")!,(double)Get(p,"Y")!);}
   PointD Mouth()=>Position()+(PointD)Call(presenter,"GetSnackMouth")!;
   var dragOrigin=default(PointD);var dragging=false;var bitePointer=default(PointD);
   var until=minutes*60;var recovery=Math.Min(600,until/6);var initialAllocated=GC.GetTotalAllocatedBytes(false);
   // Smoke executes a full two-minute warmup and two cycles regardless of wall cost.
   while(paced?watch.Elapsed.TotalSeconds<until:ticks<22500) {
    var begin=watch.Elapsed.TotalSeconds;var gap=begin-lastLoop;lastLoop=begin;gapMax=Math.Max(gapMax,gap);
    if(ticks>0&&gap>2)Event(new{utc=DateTimeOffset.UtcNow,eventType="loopGap",gapSeconds=gap,ticks});
    var recover=paced&&begin>=until-recovery;
    phase=simSeconds<120?"warmup":recover?"recovery":"repeated";
    var inCycle=(int)(ticks%7500);var cycle=ticks/7500;
    if(inCycle==0){cycles=cycle;Event(new{utc=DateTimeOffset.UtcNow,eventType="cycle",cycle,phase});}
    var desired=recover?"recovery":inCycle<1250?"slider":inCycle<1875?"sit":inCycle<2188?"stand":inCycle<2500?"head":inCycle<3125?"release":inCycle<3375?"cheek":inCycle<4063?"cheekRelease":inCycle<5000?"snack":"autonomous";
    if(desired!=cyclePhase) {
     if(dragging){dragging=false;released++;}
     if(cyclePhase=="sit"&&(bool)Get(c,"Sitting")!)Call(c,"ToggleSit");
     if(cyclePhase=="snack")Call(c,"CancelSnack");
     cyclePhase=desired;Event(new{utc=DateTimeOffset.UtcNow,eventType="phase",cycle,phase=desired,ticks});
     if(desired=="slider") {density=cycle%2==0?2:1;type.GetProperty("Density",F)!.SetValue(c,density);}
     if(desired=="sit"&&!(bool)Get(c,"Sitting")!){Call(c,"ToggleSit");sit++;}
     if(desired is "head" or "cheek") {
      dragOrigin=desired=="head"?Face(37.5,28.25):Face(20,55);
      dragging=(bool)Call(c,"TryBeginInteraction",dragOrigin)!;
      Event(new{utc=DateTimeOffset.UtcNow,eventType="press",target=desired,accepted=dragging,cycle});
      if(!dragging)throw new InvalidOperationException($"Harness {desired} hit was not accepted, cycle {cycle}.");
      if(desired=="head")head++;else cheek++;
     }
     if(desired=="snack"){Call(c,"BeginSnack");snackStarts++;bitePointer=Mouth()+new PointD(170,0);}
    }
    var pointer=PointerSample.Unavailable;var down=false;
    if(desired=="slider"&&inCycle%4==0) {
     var v=(inCycle/4)%100;var size=100+(v<=50?v:100-v);sizes++;
     if((bool)Call(c,"RequestSize",size)!)requested=size;else sizeRejected++;
    }
    if(dragging) {
     var f=desired=="head"?inCycle-2188:inCycle-3125;
     pointer=new(true,dragOrigin+(desired=="head"?new PointD(Math.Sin(f*.08)*100,-Math.Min(450,f*3)):new PointD(-Math.Min(45,f*.8),Math.Sin(f*.07)*5)));down=true;
    }
    if(desired=="snack") {
     if(inCycle==4250)bitePointer=Mouth();
     pointer=new(true,bitePointer);
    }
    var t=Stopwatch.GetTimestamp();Call(c,"Advance",TimeSpan.FromMilliseconds(16),pointer,down,scene);tickMax=Math.Max(tickMax,Stopwatch.GetElapsedTime(t).TotalMilliseconds);
    ticks++;simSeconds+=.016;
    if(requested>=0&&(int)Get(c,"SizePercent")! ==requested){sizeApplied++;requested=-1;}
    var captured=(bool)Get(c,"Capturing")!;
    if(captured!=wasCaptured){Event(new{utc=DateTimeOffset.UtcNow,eventType="capture",captured,cycle});wasCaptured=captured;}
    var snack=Get(c,"Snack")!.ToString()!;
    if(snack!=lastSnack){if(snack=="Eating")eating++;Event(new{utc=DateTimeOffset.UtcNow,eventType="snack",from=lastSnack,to=snack,cycle});lastSnack=snack;}
    if(faults>0)throw new InvalidOperationException("Controller fault; test stopped.");
    if(watch.Elapsed.TotalSeconds>=nextSample||!paced&&ticks%1875==0) {
     process.Refresh();var gc=GC.GetGCMemoryInfo();
     var frames=new Dictionary<string,int>();foreach(var n in new[]{"_pixels","_perchPixels","_perchBlend"})frames[n]=((byte[])Get(presenter,n)!).Length;
     samples.WriteLine(JsonSerializer.Serialize(new{utc=DateTimeOffset.UtcNow,elapsedSeconds=watch.Elapsed.TotalSeconds,simSeconds,ticks,phase,cycle,cyclePhase,density,
      privateBytes=process.PrivateMemorySize64,workingSet=process.WorkingSet64,handles=process.HandleCount,threads=process.Threads.Count,cpuSeconds=process.TotalProcessorTime.TotalSeconds,
      managedEstimate=GC.GetTotalMemory(false),heapAtLastGc=gc.HeapSizeBytes,fragmentedAtLastGc=gc.FragmentedBytes,committedAtLastGc=gc.TotalCommittedBytes,gcIndex=gc.Index,
      lohAtLastGc=gc.GenerationInfo.Length>3?gc.GenerationInfo[3].SizeAfterBytes:0,
      allocatedBytes=GC.GetTotalAllocatedBytes(false)-initialAllocated,gen0=GC.CollectionCount(0),gen1=GC.CollectionCount(1),gen2=GC.CollectionCount(2),
      frames,scratchBytes=((byte[])Get(sampler,"outputBuffer")!).Length,ownedAllocations=Get(sampler,"OwnedFrameAllocations"),bodyCache=((IDictionary)Get(sampler,"cache")!).Count,
      spreadCache=((IDictionary)Get(sampler,"spreadCache")!).Count,soleCache=((IDictionary)Get(presenter,"_soles")!).Count,
      sizes,sizeApplied,sizeRejected,head,cheek,released,eating,snackStarts,sit,frameRevision=Get(c,"FrameRevision"),faults,gapMax,tickMax}));
     samplesCount++;nextSample=watch.Elapsed.TotalSeconds+10;
     if(process.PrivateMemorySize64>1536L*1024*1024)throw new InvalidOperationException("Host diagnostic exceeded 1.5GiB safety ceiling; stopped without GC/trim.");
    }
    if(paced){var remaining=16-(watch.Elapsed.TotalSeconds-begin)*1000;if(remaining>=1)Thread.Sleep((int)Math.Ceiling(remaining));}
   }
   if(head==0||cheek==0||eating==0||sizeApplied<100)throw new InvalidOperationException("Required workload coverage missing.");
  } catch(Exception e) {reason="failed";error=e.ToString();Environment.ExitCode=1;}
  finally {
   Save(Path.Combine(dir,"completion.json"),new{reason,error,endUtc=DateTimeOffset.UtcNow,elapsedSeconds=watch.Elapsed.TotalSeconds,ticks,simSeconds,cycles,samplesCount,
    sizes,sizeApplied,sizeRejected,head,cheek,released,eating,snackStarts,sit,faults,gapMax,tickMax,nativeMac="NOT_TESTED",mode=paced?"wall-clock host":"smoke only"});
   (c as IDisposable)?.Dispose();
  }
 }
}

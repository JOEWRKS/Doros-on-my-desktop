#if !BASELINE
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Windows.Media;
using Dororong.App.Controls;
// Finite renderer ownership diagnostic, not native desktop/mouse/Mac coverage.
internal static class MemoryRun {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 static object? Get(object o,string n)=>o.GetType().GetField(n,F)?.GetValue(o)??o.GetType().GetProperty(n,F)?.GetValue(o);
 internal static void Run(string directory){
  var dir=Path.GetFullPath(directory);if(Directory.Exists(dir))throw new IOException("Fresh directory required");Directory.CreateDirectory(dir);
  using var process=Process.GetCurrentProcess();var r=new HighResolutionGrabRaster();byte[]? pixels=null;
  var head=Matrix.Identity;var offset=r.MotionOffset(321);head.Translate(offset.X,offset.Y);
  var seated=HighResolutionFacePose.Neutral with{Head=head,MotionFrame=321};
  File.WriteAllText(Path.Combine(dir,"run.json"),JsonSerializer.Serialize(new{pid=Environment.ProcessId,startUtc=process.StartTime.ToUniversalTime(),startTicks=process.StartTime.ToUniversalTime().Ticks,durationSeconds=300,warmupSeconds=60,recoverySeconds=60,nativeMac="NOT_TESTED",input="code-fed raster poses; no live pet or forced GC/trim",rasterHash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(typeof(HighResolutionGrabRaster).Assembly.Location)))}));
  using var log=new StreamWriter(Path.Combine(dir,"samples.jsonl")){AutoFlush=true};var watch=Stopwatch.StartNew();long frames=0;double next=0,last=0,maxGap=0;var counts=new int[6];
  while(watch.Elapsed.TotalSeconds<300){
   var now=watch.Elapsed.TotalSeconds;maxGap=Math.Max(maxGap,now-last);last=now;
   var recovery=now>=240;var stage=(int)(frames/90%6);var age=frames%90/90d;
   var scale=recovery?1:1+.5*(1-Math.Cos(frames*.025))*.5;var density=recovery?1:frames/540%2+1;
   var face=recovery?HighResolutionFacePose.Neutral with{Breathing=(frames%120)/120d}:stage switch{
    0=>SeatedInteraction.Pose(seated,SeatedReaction.Tap,age*1.05,-1.15+1.65*age),
    1=>SeatedInteraction.Pose(seated,SeatedReaction.Shake,age*.95),
    2=>seated with{ChuckleCover=Math.Sin(age*Math.PI),GroomSweep=Math.Pow(Math.Sin(age*Math.PI*3),2),GroomingPaw=true},
    3=>HighResolutionFacePose.Neutral with{Stretch=Math.Sin(age*Math.PI),StretchShake=Math.Sin(age*Math.PI*12)*.3},
    4=>HighResolutionGrabRaster.SnackReaction(age*4,5),
    _=>HighResolutionGrabRaster.SnackChuckle(age*2.5)};
   pixels=r.RenderOwnedPixels(0,Matrix.Identity,default,default,default,0,scale*density,face,scale,pixels,true);counts[stage]++;frames++;
   if(now>=next){next=now+5;process.Refresh();var gc=GC.GetGCMemoryInfo();
    log.WriteLine(JsonSerializer.Serialize(new{utc=DateTimeOffset.UtcNow,seconds=now,frames,stage,recovery,privateBytes=process.PrivateMemorySize64,managedEstimate=GC.GetTotalMemory(false),heapAtLastGc=gc.HeapSizeBytes,committedAtLastGc=gc.TotalCommittedBytes,handles=process.HandleCount,threads=process.Threads.Count,ownedAllocations=r.OwnedFrameAllocations,frameCapacity=pixels.Length,scratchCapacity=((byte[])Get(r,"outputBuffer")!).Length,bodyCache=((IDictionary)Get(r,"cache")!).Count,spreadCache=((IDictionary)Get(r,"spreadCache")!).Count,seatedDistance=Get(r,"seatedBodyDistance") is not null}));
   }
   if(process.PrivateMemorySize64>1536L*1024*1024)throw new Exception("Private ceiling exceeded");
   var spare=16-(watch.Elapsed.TotalSeconds-now)*1000;if(spare>0)Thread.Sleep((int)spare);
  }
  File.WriteAllText(Path.Combine(dir,"completion.json"),JsonSerializer.Serialize(new{reason="completed",elapsedSeconds=watch.Elapsed.TotalSeconds,frames,counts,maxLoopGapSeconds=maxGap}));
  Console.WriteLine($"Completed {watch.Elapsed.TotalSeconds:F3}s, {frames} frames");
 }
}
#endif

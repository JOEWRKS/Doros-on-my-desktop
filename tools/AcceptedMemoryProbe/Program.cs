using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.App.Product;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

internal static class Program {
 const string Expected="B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0";
 static readonly string[] Names=["sit-tap","sit-shake","groom","stretch","beg","angry","chuckle","eating","sleep","resize","idle"];
 [DllImport("user32.dll")] static extern uint GetGuiResources(IntPtr h,uint flag);
 [StructLayout(LayoutKind.Sequential)] struct Mbi {public ulong Address,Base;public uint AllocationProtect,Alignment;public ulong Bytes;public uint State,Protect,Type,Alignment2;}
 [DllImport("kernel32.dll")] static extern UIntPtr VirtualQuery(IntPtr address,out Mbi info,UIntPtr size);
 static object[] Regions(){var rows=new List<object>();ulong a=0;while(a<0x7FFFFFFFFFFF){if(VirtualQuery((IntPtr)a,out var r,(UIntPtr)48)==UIntPtr.Zero)break;if(r.State==0x1000&&(r.Protect&0x400)!=0)rows.Add(new{allocationBase=r.Base,address=r.Address,bytes=r.Bytes});var next=r.Address+r.Bytes;if(next<=a)throw new Exception("Region overflow");a=next;}return rows.ToArray();}
 static object? Field(object? o,string name)=>o?.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(o);
 static int Count(object? o)=>o is ICollection c?c.Count:(int?)o?.GetType().GetProperty("Count")?.GetValue(o)??0;
 static long Bytes(object? o)=>o is byte[] b?b.LongLength:o is float[] f?f.LongLength*4:0;
 static object Capacity(DororongPresenter p){
  var raster=Field(p,"_hrRaster");var frames=(IDictionary?)Field(p,"_hrFrames");var spare=(IEnumerable?)Field(p,"_hrSpareFrames");long frameBytes=0,spareBytes=0;
  if(frames!=null)foreach(var v in frames.Values)frameBytes+=Bytes(v);
  if(spare!=null)foreach(var v in spare)spareBytes+=Bytes(v);
  var slots=Count(frames)+Count(spare);var soles=Count(Field(p,"_hrSoles"));
  if(slots>12||soles>24)throw new Exception($"Unbounded presenter cache {slots}/{soles}");
  return new{frameSlots=slots,frameBytes,spareBytes,displayBytes=Bytes(Field(p,"_hrDisplayedPixels")),scratchBytes=Bytes(Field(raster,"outputBuffer")),soleCount=soles,rasterCache=Count(Field(raster,"cache")),spreadCache=Count(Field(raster,"spreadCache")),locomotionCache=Count(Field(Field(raster,"locomotion"),"cache")),seatedDistanceBytes=Bytes(Field(Field(raster,"seatedBodyDistance"),"values"))};
 }
 [STAThread] static int Main(string[] args){
  var output=Path.GetFullPath(args[0]);var duration=double.Parse(args[1]);var recovery=double.Parse(args[2]);
  Directory.CreateDirectory(output);if(File.Exists(Path.Combine(output,"run.json")))throw new Exception("Do not overwrite evidence");
  using var proc=Process.GetCurrentProcess();var hash=Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(DororongPresenter).Assembly.Location)));
  if(hash!=Expected)throw new Exception("Wrong product");
  var app=new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};
  using var samples=new StreamWriter(new FileStream(Path.Combine(output,"samples.jsonl"),FileMode.CreateNew,FileAccess.Write,FileShare.Read)){AutoFlush=true};
  using var events=new StreamWriter(new FileStream(Path.Combine(output,"events.jsonl"),FileMode.CreateNew,FileAccess.Write,FileShare.Read)){AutoFlush=true};
  var p=new DororongPresenter();p.SetArtSize(150);
  var root=new Grid{Background=Brushes.Transparent};root.Children.Add(p);
  root.Children.Add(new TextBlock{Text="메모리 진단 · 자동 재생",VerticalAlignment=VerticalAlignment.Bottom,Background=Brushes.White,Foreground=Brushes.Black,IsHitTestVisible=false});
  var w=new Window{Content=root,WindowStyle=WindowStyle.None,AllowsTransparency=true,Background=Brushes.Transparent,ShowActivated=false,ShowInTaskbar=false,Title="Accepted build memory probe"};
  var viewport=new PetWindowViewport(w,p);var position=new PointD(300,300);
  var menu=new PetContextMenu{Placement=PlacementMode.Relative,PlacementTarget=root,HorizontalOffset=0,VerticalOffset=0};
  var size=150;long resizeApplied=0,menus=0;
  var sizeItem=new PetSizeMenuItem(()=>size,()=>true,n=>{size=n;p.SetArtSize(n);resizeApplied++;return true;},()=>{});
  menu.Items.Add(sizeItem);
  var snackItem=new MenuItem{Header="간식"};snackItem.Template=(ControlTemplate)menu.Resources["SnackTemplate"];menu.Items.Add(snackItem);
  var clock=Stopwatch.StartNew();var ticks=0L;var observed=new Dictionary<string,long>();var phaseFrames=new Dictionary<string,long>();
  double lastTick=0,maxLoopGap=0,lastSample=-5;int previousPhase=-2;bool done=false;string reason="duration_completed";string? error=null;
  var timer=new DispatcherTimer(DispatcherPriority.Background){Interval=TimeSpan.FromMilliseconds(33)};
  void Event(object row)=>events.WriteLine(JsonSerializer.Serialize(row));
  void Sample(string phase){proc.Refresh();var gc=GC.GetGCMemoryInfo();samples.WriteLine(JsonSerializer.Serialize(new{utc=DateTime.UtcNow,seconds=clock.Elapsed.TotalSeconds,pid=proc.Id,startTicks=proc.StartTime.ToUniversalTime().Ticks,phase,ticks,simulatedSeconds=ticks/30d,maxLoopGap,privateBytes=proc.PrivateMemorySize64,workingSet=proc.WorkingSet64,handles=proc.HandleCount,threads=proc.Threads.Count,gdi=GetGuiResources(proc.Handle,0),user=GetGuiResources(proc.Handle,1),managedEstimate=GC.GetTotalMemory(false),heapBytes=gc.HeapSizeBytes,committedBytes=gc.TotalCommittedBytes,fragmentedBytes=gc.FragmentedBytes,gc0=GC.CollectionCount(0),gc1=GC.CollectionCount(1),gc2=GC.CollectionCount(2),allocated=GC.GetTotalAllocatedBytes(false),capacity=Capacity(p),regions=Regions(),resizeApplied,menus,observed}));}
  void Observe(string key,bool active){if(active)observed[key]=observed.GetValueOrDefault(key)+1;}
  void End(string why,string? failure=null){if(done)return;done=true;reason=why;error=failure;timer.Stop();menu.IsOpen=false;Sample("end");File.WriteAllText(Path.Combine(output,"completion.json"),JsonSerializer.Serialize(new{reason,error,pid=proc.Id,startTicks=proc.StartTime.ToUniversalTime().Ticks,endedUtc=DateTime.UtcNow,elapsedSeconds=clock.Elapsed.TotalSeconds,ticks,simulatedSeconds=ticks/30d,maxLoopGap,phaseFrames,observed,resizeApplied,menus}));w.Close();app.Shutdown();}
  w.Closed+=(_,_)=>{if(!done)End("window_closed");};
  w.Show();viewport.SetPosition(position);w.UpdateLayout();
  File.WriteAllText(Path.Combine(output,"run.json"),JsonSerializer.Serialize(new{pid=proc.Id,startUtc=proc.StartTime.ToUniversalTime(),startTicks=proc.StartTime.ToUniversalTime().Ticks,startedUtc=DateTime.UtcNow,hash,durationSeconds=duration,recoverySeconds=recovery,stepSeconds=1/30d,mode="isolated transparent HWND presenter replay, not real mouse or full PetLoop",forcedGC=false,trim=false,livePetUnchanged=true,dpi=VisualTreeHelper.GetDpi(w).DpiScaleX}));
  timer.Tick+=(_,_)=>{try{
   var elapsed=clock.Elapsed.TotalSeconds;var gap=elapsed-lastTick;lastTick=elapsed;maxLoopGap=Math.Max(maxLoopGap,gap);
   if(gap>2)Event(new{kind="loop_gap",utc=DateTime.UtcNow,seconds=elapsed,gap});
   if(elapsed>=duration){End("duration_completed");return;}
   var rest=elapsed>=duration-recovery;var phase=rest?-1:(int)(ticks/180%Names.Length);var age=(ticks%180)/30d;var name=rest?"recovery":Names[phase];
   if(phase!=previousPhase){menu.IsOpen=false;Event(new{kind="phase",phase=name,seconds=elapsed,ticks,utc=DateTime.UtcNow});if(phase==9){sizeItem.Refresh();menu.IsOpen=true;menus++;}previousPhase=phase;}
   var faceRight=(ticks/(180*Names.Length)%2)==0;var facing=faceRight?FacingDirection.Right:FacingDirection.Left;
   var pet=new PetSnapshot(PetState.Idle,position,facing,0,false,null);
   p.SetSittingRequested(phase is 0 or 1);p.SetSnack(default,PointerSample.Unavailable);
   if(phase==1&&age>=1&&age<2.3)pet=pet with{State=PetState.ClickReaction,Phase=age-1};
   if(phase is 2 or 3 or 4){var kind=phase==2?IdleActivityKind.Groom:phase==3?IdleActivityKind.Stretch:IdleActivityKind.Beg;var span=phase==2?3.8:phase==3?2.8:2.4;pet=pet with{IdleActivity=new(kind,age%span)};}
   if(phase is 5 or 6 or 7){var kind=phase==5?SnackPhase.Angry:phase==6?SnackPhase.Chuckling:SnackPhase.Eating;p.SetSnack(new(kind,facing,0,0,age%2.8,0){WithdrawalStreak=5},PointerSample.Unavailable);}
   if(phase==8)pet=pet with{State=PetState.Sleep,Phase=age};
   if(phase==9){var step=(int)(ticks%100);sizeItem.Slider.Value=step<=50?100+step:200-step;sizeItem.Flush();viewport.SetPosition(position);}
   if(phase==-1){pet=pet with{Facing=FacingDirection.Right};}
   var pointer=phase==0?new PointerSample(true,position+new PointD((faceRight?30:114)*size/100d,(75+15*Math.Sin(age*2))*size/100d)):PointerSample.Unavailable;
   p.UpdateHuntingWithPounce(pet,DirectInteractionSnapshot.None,pointer,TimeSpan.FromSeconds(1/30d),false);
   p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.FromSeconds(1/30d));
   _=p.MeasurePlatformContact();
   var f=p.HighResolutionFace;Observe("tap",f.SeatedPawAmount>0);Observe("shake",Math.Abs(f.SeatedShake)>.01);Observe("groom",f.GroomingPaw&&f.ChuckleCover>.1);Observe("stretch",f.Stretch>.1);Observe("beg",phase==4&&f.StompLift>.1);Observe("angry",f.Angry);Observe("chuckle",phase==6&&f.ChuckleCover>.1);Observe("eating",phase==7&&f.MouthAge>=0);Observe("sleep",phase==8&&f.MotionFrame>=706);
   phaseFrames[name]=phaseFrames.GetValueOrDefault(name)+1;ticks++;
   if(elapsed-lastSample>=5){Sample(name);lastSample=elapsed;}
  }catch(Exception ex){End("fault",ex.ToString());}};
  timer.Start();app.Run();GC.KeepAlive(p);return reason=="duration_completed"?0:1;
 }
}

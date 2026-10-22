using Dororong.App;
using Dororong.App.Controls;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;
namespace Dororong.Mac.Rendering;

// Native coordinates are Quartz desktop points, not backing pixels.
// Retina density belongs only to the renderer/surface, never movement speed.
internal sealed class R104DesktopController : IDisposable {
 private readonly DororongPresenter presenter=new();
 private readonly PetLoop loop;
 private readonly PetPlatformRuntime platforms;
 private readonly SceneFeed feed=new();
 private readonly DesktopSceneSource sceneSource;
 private DesktopMonitor[] topology=[];
 private EventHandler? tick;
 private TimeSpan elapsed;
 private PointerSample pointer;
 private bool primary,captured,started;
 private int? pendingSize;
 internal PointD Position {get;private set;}
 internal double Scale=>presenter.HighResolutionScale;
 internal int SizePercent=>(int)Math.Round(Scale*100);
 internal double Density {get=>presenter.Density;set=>presenter.Density=value;}
 internal byte[] Pixels=>presenter.Pixels;
 internal int Extent=>presenter.Extent;
 internal long FrameRevision=>presenter.FrameRevision;
 internal double ReleaseOffset=>presenter.ReleaseOffset;
 internal bool Capturing=>captured;
 internal bool MenuOpen {get;set;}
 internal bool Sitting=>presenter.Sitting;
 internal bool AutomaticSleep {get;private set;}=true;
 internal SnackPhase Snack {get;private set;}
 internal EdgePerchPhase Perch=>platforms.PerchPhase;
 internal object Platform=>platforms.DiagnosticState;
 internal event Action<Exception>? Faulted;
 internal bool CanResize=>loop.CanResize;
 internal (FootContact Contact,RectD Bounds)? Geometry=>presenter.MeasurePlatformGeometry();
 internal R104DesktopController(PointD initial,IRandomSource random,int size=100) {
  Position=initial;presenter.SetArtSize(size);
  presenter.RenderDesktop(new PetBrain(BehaviorTuning.Default,random,initial).Current,Dororong.App.Interaction.DirectInteractionSnapshot.None);
  sceneSource=new DesktopSceneSource(feed);
  platforms=new(sceneSource,_=>new(default,default,1,1),presenter.MeasurePlatformGeometry,presenter.ApplyPlatformPose,
   ()=>feed.Scene?.Monitors??[],presenter.MeasureEdgePerchContact,presenter.ApplyEdgePerch);
  var host=new PetLoopHost(()=>feed.Scene?.Monitors.FirstOrDefault()?.Bounds??new(0,0,1920,1080),
   ()=>new(144*Scale,144*Scale),()=>new(4,4),()=>MenuOpen?PointerSample.Unavailable:pointer,()=>primary,
   ()=>Position,p=>Position=p,presenter.RenderDesktop,()=>{captured=true;return true;},()=>captured=false,presenter.RenderDesktop){
   Platforms=platforms,MeasureSizeAnchor=()=>presenter.MeasurePlatformGeometry() is {} g?new((g.Contact.Left+g.Contact.Right)/2,g.Contact.SoleY):null,
   RebaseSizePresentation=presenter.RebaseSizePresentation,HoldLocomotionWalk=presenter.HoldLocomotionWalk,
   SetLocomotionBlocked=presenter.SetLocomotionBlocked,SetSittingRequested=presenter.SetSittingRequested,
   BeginPlatformFrame=presenter.BeginHighResolutionPlatformFrame,UpdateHunting=presenter.UpdateHuntingWithPounce,
   GetPouncePose=()=>presenter.Pounce,PresentSnack=(p,m)=>{Snack=p.Phase;presenter.SetSnack(p,m);},
   GetSnackMouth=presenter.GetSnackMouth,GetSnackChaseScale=()=>Scale
  };
  loop=new(new(()=>elapsed,()=>{},()=>{}),new(h=>tick+=h,h=>tick-=h,()=>{},()=>{}),host,p=>new(BehaviorTuning.Default,random,p));
  loop.Faulted+=(_,e)=>Faulted?.Invoke(e);
 }
 internal void Advance(TimeSpan delta,PointerSample sample,bool down,SceneSnapshot scene) {
  if(!topology.SequenceEqual(scene.Scene.Monitors)){
   topology=scene.Scene.Monitors.ToArray();sceneSource.InvalidateSynchronousCache();
  }
  feed.Scene=scene.Health==SceneHealth.Unavailable?null:scene.Scene;pointer=sample;primary=down;
  if(!started){loop.Start();started=true;}
  if(pendingSize is {} size&&loop.TryResize(()=>presenter.SetArtSize(size)))pendingSize=null;
  elapsed+=delta;tick?.Invoke(this,EventArgs.Empty);
 }
 internal bool RequestSize(int percent) {if(percent is <100 or >150||!loop.CanResize)return false;pendingSize=percent;return true;}
 internal bool TryBeginInteraction(PointD screen) {
  if(MenuOpen)return false;var press=presenter.Press(screen-Position);if(press is null)return false;
  loop.NotifyDirectInteractionPressed(press);return true;
 }
 internal bool Hit(PointD screen)=>presenter.Hit(screen-Position);
 internal void ToggleSit()=>loop.NotifySitRequested();
 internal void SetAutomaticSleep(bool enabled){AutomaticSleep=enabled;loop.SetAutomaticSleepEnabled(enabled);}
 internal void BeginSnack(){if(!loop.IsSnackActive)loop.ToggleSnack();}
 internal void CancelSnack(){if(loop.IsSnackActive)loop.ToggleSnack();}
 internal void Drop(){loop.NotifyDirectInteractionCanceled();platforms.Clear(Position);}
 public void Dispose()=>loop.Dispose();
 private sealed class SceneFeed:IDesktopSceneNative {internal DesktopScene? Scene;public DesktopScene? Capture(TimeSpan now)=>Scene;}
}

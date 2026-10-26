using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Dororong.App.Controls;
using Dororong.App.Interop;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Platforms;
using Dororong.Core.Geometry;

internal static class Program {
 [StructLayout(LayoutKind.Sequential)] public struct Rect {public int Left,Top,Right,Bottom;}
 [DllImport("user32.dll")] static extern bool GetWindowRect(nint h,out Rect r);
 [DllImport("user32.dll")] static extern uint GetDpiForWindow(nint h);
 [DllImport("user32.dll")] static extern nint SetThreadDpiAwarenessContext(nint c);
 [STAThread] static void Main(string[] args){
  var p=Process.GetProcessById(int.Parse(args[0]));
  var candidates=new DesktopMetadataReader(0).Read()!.Windows.Where(w=>w.ProcessId==p.Id&&w.Visible).ToArray();
  var hwnd=(nint)candidates.Single().Handle;
  var reader=new DesktopMetadataReader(hwnd);var source=new DesktopSceneNative(reader);
  var presenter=new DororongPresenter();presenter.SetArtSize(150);
  presenter.Render(new(PetState.Idle,default,FacingDirection.Right,0,false,null),DirectInteractionSnapshot.None,TimeSpan.Zero);
  presenter.Measure(new(presenter.Width,presenter.Height));presenter.Arrange(new(0,0,presenter.Width,presenter.Height));presenter.UpdateLayout();
  var contact=presenter.MeasurePlatformContact();
  var clock=Stopwatch.StartNew();Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[1]))!);
  using var writer=new StreamWriter(args[1]);
  for(var i=0;i<30;i++){
   var old=SetThreadDpiAwarenessContext(-4);GetWindowRect(hwnd,out var rect);SetThreadDpiAwarenessContext(old);
   var scene=source.Capture(clock.Elapsed)!;var surfaces=PlatformGeometry.Build(scene,100);
   var bars=scene.Windows.Where(w=>w.Taskbar).ToArray();
   var blockers=bars.Select(bar=>new{bar=bar.Key.Handle,items=scene.Windows.Where(w=>w.ZOrder<bar.ZOrder&&w.Visible&&!w.Minimized&&!w.Cloaked&&!w.Excluded&&w.CanOcclude&&!w.IsTransientMenu&&w.Bounds.Y<=bar.Bounds.Y&&w.Bounds.Bottom>bar.Bounds.Y&&w.Bounds.X<bar.Bounds.Right&&w.Bounds.Right>bar.Bounds.X).ToArray()});
   writer.WriteLine(JsonSerializer.Serialize(new{utc=DateTime.UtcNow,pid=p.Id,start=p.StartTime.ToUniversalTime(),hwnd=(long)hwnd,rect=new{rect.Left,rect.Top,rect.Right,rect.Bottom},dpi=GetDpiForWindow(hwnd),neutralContact=contact,scene.Monitors,bars,blockers,surfaces}));writer.Flush();Thread.Sleep(100);
  }
  var finalScene=source.Capture(clock.Elapsed)!;GetWindowRect(hwnd,out var finalRect);
  var foot=contact!.Value;
  var at=new PointD(finalRect.Left+96,finalRect.Top+96);
  var barHere=finalScene.Windows.Single(w=>w.Taskbar&&w.Bounds.X<=at.X+foot.Left&&w.Bounds.Right>=at.X+foot.Right);
  var blocking=finalScene.Windows.Where(w=>w.ZOrder<barHere.ZOrder&&w.Visible&&!w.Minimized&&!w.Cloaked&&!w.Excluded&&w.CanOcclude&&!w.IsTransientMenu&&w.Bounds.Y<=barHere.Bounds.Y&&w.Bounds.Bottom>barHere.Bounds.Y&&w.Bounds.X<at.X+foot.Right&&w.Bounds.Right>at.X+foot.Left).ToArray();
  var clearScene=finalScene with{Windows=finalScene.Windows.Where(w=>!blocking.Contains(w)).ToArray()};
  var clearSurfaces=PlatformGeometry.Build(clearScene,foot.SoleY-foot.VisibleTop);
  var actualSurfaces=PlatformGeometry.Build(finalScene,foot.SoleY-foot.VisibleTop);
  var start=new PointD(at.X,barHere.Bounds.Y-foot.SoleY);
  object Replay(IReadOnlyList<PlatformSurface> next){
   var motion=new PlatformMotion();var pose=motion.Advance(new(TimeSpan.Zero,start,start,foot,clearSurfaces,false,true));
   var first=pose;
   for(var j=0;j<240;j++){pose=motion.Advance(new(TimeSpan.FromMilliseconds(16),pose.Position,pose.Position,foot,next,false,true));if(j==0)first=pose;}
   return new{firstPhase=first.Phase.ToString(),finalPhase=pose.Phase.ToString(),soleY=pose.Position.Y+foot.SoleY,drop=pose.Position.Y-start.Y,pose.Support};
  }
  var proof=new{scope="Read-only native geometry plus separate packaged physics replay; no live pet input or memory attachment. Neutral-art contact inference, not a live internal-state read.",pid=p.Id,hwnd=(long)hwnd,at,foot,neutralWorldSole=at.Y+foot.SoleY,taskbarTop=barHere.Bounds.Y,blocking,observedScene=Replay(actualSurfaces),removeBlockerOnly=Replay(clearSurfaces)};
  File.WriteAllText(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[1]))!,"replay.json"),JsonSerializer.Serialize(proof,new JsonSerializerOptions{WriteIndented=true}));
  Console.WriteLine(JsonSerializer.Serialize(proof));
  Console.WriteLine(args[1]);
 }
}

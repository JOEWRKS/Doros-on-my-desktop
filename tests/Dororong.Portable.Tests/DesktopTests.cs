using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;
using Dororong.Portable.Rendering;

namespace Dororong.Portable.Tests;

public sealed class DesktopTests
{
    [Fact]
    public void BackgroundSceneQueryIsSingleFlightAndCannotKeepExpiredSupportAlive()
    {
        var cache=new SceneCache(background:true);var calls=0;
        using var started=new ManualResetEventSlim();using var release=new ManualResetEventSlim();using var finished=new ManualResetEventSlim();
        DesktopScene? Capture(TimeSpan now)
        {
            if(Interlocked.Increment(ref calls)>1)
            {started.Set();release.Wait(TimeSpan.FromSeconds(5));finished.Set();}
            return new(1,now,monitors,[]);
        }
        cache.Read(TimeSpan.Zero,Capture,monitors);
        try
        {
            cache.Read(TimeSpan.FromMilliseconds(80),Capture,monitors);
            Assert.True(started.Wait(TimeSpan.FromSeconds(3)));
            for(var ms=160;ms<=800;ms+=80)cache.Read(TimeSpan.FromMilliseconds(ms),Capture,monitors);
            Assert.Equal(2,Volatile.Read(ref calls));
            Assert.Equal(SceneHealth.FloorFallback,cache.Read(TimeSpan.FromMilliseconds(800),Capture,monitors).Health);
        }
        finally{release.Set();Assert.True(finished.Wait(TimeSpan.FromSeconds(3)));}
    }
    private static readonly DesktopMonitor[] monitors=[new(1,new(0,0,1000,800))];
    private static MacWindowRow Window(long id=1,RectD? bounds=null)=>new(id,50,bounds??new(100,300,500,300),0,1,true,false);
    private static MacWindowRow Dock()=>new(2,60,new(150,700,500,80),20,1,true,true);
    private static readonly TimeSpan step=TimeSpan.FromSeconds(.016);
    private static SceneSnapshot Read(DesktopScene scene)=>new(scene,SceneHealth.Fresh);
    private sealed class FixedRandom:IRandomSource{public double NextUnit()=>.99;}
    private static DesktopPetController Pet(PointD p)=>new(p,new FixedRandom());
    [Fact]
    public void MetadataExcludesOwnWindowsAndDockNeverProvidesSupport()
    {
        var mapper=new MacSceneMapper();var scene=mapper.Build(TimeSpan.Zero,monitors,
            [new(9,99,new(0,0,1000,800),0,1,true,false),new(3,77,new(100,600,700,200),101,1,true,false),Dock()],99,20,101);
        Assert.DoesNotContain(scene.Windows,w=>w.Key.ProcessId==99);
        Assert.DoesNotContain(PlatformGeometry.Build(scene,70),s=>s.Kind==PlatformKind.Taskbar);
        Assert.Equal(800,Assert.Single(PlatformGeometry.Build(scene,70).Where(s=>s.Kind==PlatformKind.Floor)).Top);
        Assert.DoesNotContain(PlatformGeometry.Build(scene,70),s=>s.Key.Handle==3);
    }
    [Theory]
    [InlineData(0,798,1000,2)] // auto-hidden activation strip is not a shelf
    [InlineData(0,0,1000,800)] // Mission Control/desktop owned by Dock
    [InlineData(0,100,60,600)] // vertical Dock: no horizontal taskbar support
    public void NonShelfDockDoesNotInventHorizontalLedge(double x,double y,double w,double h)
    {
        var scene=new MacSceneMapper().Build(TimeSpan.Zero,monitors,[Dock() with{Bounds=new(x,y,w,h)}],99,20,101);
        Assert.DoesNotContain(PlatformGeometry.Build(scene,0),s=>s.Kind==PlatformKind.Taskbar);
    }
    [Fact]
    public void WindowOcclusionAndReusedIdsAreNotStaleOwners()
    {
        var mapper=new MacSceneMapper();var scene=mapper.Build(TimeSpan.Zero,monitors,[Window(2,new(250,200,100,200)),Window()],99,20,101);
        var pieces=PlatformGeometry.Build(scene,60).Where(s=>s.Key.Handle==1).ToArray();
        Assert.Equal(2,pieces.Length);Assert.Equal(250,pieces[0].Right);Assert.Equal(350,pieces[1].Left);
        var key=scene.Windows.Single(w=>w.Key.Handle==1).Key;
        mapper.Build(step,monitors,[],99,20,101);
        var next=mapper.Build(step*2,monitors,[Window()],99,20,101);
        Assert.NotEqual(key,next.Windows[0].Key);
    }
    [Fact]
    public void SceneFailureFreezesBrieflyThenDropsOnlyToKnownFloors()
    {
        var cache=new SceneCache();var mapper=new MacSceneMapper();var calls=0;
        DesktopScene? Capture(TimeSpan now){calls++;return calls==1?mapper.Build(now,monitors,[Window()],99,20,101):null;}
        Assert.Equal(SceneHealth.Fresh,cache.Read(TimeSpan.Zero,Capture,monitors).Health);
        Assert.Equal(SceneHealth.Cached,cache.Read(TimeSpan.FromMilliseconds(79),Capture,monitors).Health);Assert.Equal(1,calls);
        var failed=cache.Read(TimeSpan.FromMilliseconds(80),Capture,monitors);Assert.False(failed.Reliable);Assert.False(failed.CanAcquireWindow);
        var expired=cache.Read(TimeSpan.FromMilliseconds(500),Capture,[]);Assert.Equal(SceneHealth.FloorFallback,expired.Health);
        Assert.Empty(expired.Scene.Windows);Assert.Single(expired.Scene.Monitors);Assert.False(expired.CanAcquireWindow);
    }
    [Fact]
    public void DelayedCaptureCannotBecomeFreshAtResume()
    {
        var cache=new SceneCache();DesktopScene Capture(TimeSpan _)=>new(1,TimeSpan.Zero,monitors,[ ]);
        cache.Read(TimeSpan.Zero,Capture,monitors);
        Assert.Equal(SceneHealth.FloorFallback,cache.Read(TimeSpan.FromSeconds(2),Capture,monitors).Health);
    }
    [Fact]
    public void QuartzToCocoaPreservesNegativeMonitorCoordinatesWithoutRetinaMultiplication()
    {
        Assert.Equal(new PointD(-900,900),MacSceneMapper.ToCocoaTopLeft(new(-900,180),1080));
        Assert.Equal(new PointD(1400,1280),MacSceneMapper.ToCocoaTopLeft(new(1400,-200),1080));
    }
    [Fact]
    public void PerchReadinessReleaseMoveAndCloseUseSharedWindowsEngine()
    {
        var mapper=new MacSceneMapper();var scene=mapper.Build(TimeSpan.Zero,monitors,[Window()],99,20,101);
        var pet=Pet(new(150,216));var pointer=new PointerSample(true,pet.Position+new PointD(72,50));pet.BeginDrag(pointer.Position);
        pet.Advance(step,pointer,true,Read(scene));Assert.True(pet.Ready);
        pet.Advance(step,pointer,false,Read(scene));Assert.Equal(EdgePerchPhase.Entering,pet.Perch);
        for(var i=0;i<12;i++)pet.Advance(step,pointer,false,Read(scene));
        Assert.Equal(EdgePerchPhase.Attached,pet.Perch);Assert.Equal(206,pet.Position.Y,8);
        scene=mapper.Build(step,monitors,[Window(bounds:new(125,330,500,300))],99,20,101);
        pet.Advance(step,pointer,false,Read(scene));Assert.Equal(new PointD(175,236),pet.Position);
        scene=mapper.Build(step*2,monitors,[],99,20,101);
        pet.Advance(step,pointer,false,Read(scene));Assert.Equal(EdgePerchPhase.None,pet.Perch);Assert.Equal(PlatformPhase.Falling,pet.Platform.Phase);
    }
    [Theory]
    [InlineData(0)] // ordinary foreground app still occludes ordinary ledges
    [InlineData(25)] // elevated overlay/status panel
    [InlineData(101)] // popup menu
    public void ScreenFloorSurvivesOverlayRestoreAndDockChanges(int layer)
    {
        var mapper=new MacSceneMapper();
        var sole=DesktopFrames.Measure(DesktopFrames.Place(MotionRenderer.Standing(),false)).Contact.SoleY;
        var pet=Pet(new(250,800-sole));var initialY=pet.Position.Y;
        for(var i=0;i<120;i++)
        {
            MacWindowRow[] rows=(i/20) switch
            {
                0=>[Dock()],
                1=>[new(3,77,new(0,0,1000,800),layer,1,true,false),Dock()],
                2=>[new(3,77,new(100,600,700,200),layer,1,true,false),Dock()],
                3=>[Dock() with{Bounds=new(150,798,500,2)}],
                4=>[Dock() with{Bounds=new(0,100,60,600)}],
                _=>[]
            };
            var scene=mapper.Build(step*i,monitors,rows,99,20,101);
            pet.Advance(step,PointerSample.Unavailable,false,Read(scene));
            Assert.Equal(initialY,pet.Position.Y,7);
            Assert.Equal(EdgePerchPhase.None,pet.Perch);
            Assert.DoesNotContain(PlatformGeometry.Build(scene,70),s=>s.Kind==PlatformKind.Taskbar);
        }
    }
    [Fact]
    public void DockCannotOfferHangingCueButStillOccludesOrdinaryWindow()
    {
        var scene=new MacSceneMapper().Build(TimeSpan.Zero,monitors,
            [Dock(),Window(bounds:new(100,720,600,60))],99,20,101);
        Assert.False(scene.Windows[0].CanSupport);Assert.True(scene.Windows[0].CanOcclude);
        var pieces=PlatformGeometry.Build(scene,0).Where(s=>s.Key.Handle==1).ToArray();
        Assert.Equal(2,pieces.Length);Assert.Equal(150,pieces[0].Right);Assert.Equal(650,pieces[1].Left);
        var pet=Pet(new(250,616));var pointer=new PointerSample(true,pet.Position+new PointD(72,40));pet.BeginDrag(pointer.Position);
        var dockOnly=new MacSceneMapper().Build(TimeSpan.Zero,monitors,[Dock()],99,20,101);
        pet.Advance(step,pointer,true,Read(dockOnly));Assert.False(pet.Ready);
        pet.Advance(step,pointer,false,Read(dockOnly));Assert.Equal(EdgePerchPhase.None,pet.Perch);
    }
    [Fact]
    public void SceneFailureCannotAcquireDespiteLastFrameReadiness()
    {
        var scene=new MacSceneMapper().Build(TimeSpan.Zero,monitors,[Window()],99,20,101);var pet=Pet(new(150,216));
        var pointer=new PointerSample(true,pet.Position+new PointD(72,50));pet.BeginDrag(pointer.Position);
        pet.Advance(step,pointer,true,Read(scene));Assert.True(pet.Ready);
        pet.Advance(step,pointer,false,new(scene,SceneHealth.Unavailable));Assert.Equal(EdgePerchPhase.None,pet.Perch);
    }
    [Fact]
    public void SnackEntryNeverUsesPointerAsPositionAndResumeCancels()
    {
        var scene=new DesktopScene(1,TimeSpan.Zero,monitors,[]);var pet=Pet(new(200,691));
        pet.Advance(step,new(true,new(900,0)),false,Read(scene));var before=pet.Position;
        pet.BeginSnack();Assert.Equal(before,pet.Position);
        pet.Advance(step,new(true,new(900,0)),false,Read(scene));Assert.InRange(pet.Position.X-before.X,0,185*.016+.001);
        pet.Advance(TimeSpan.FromSeconds(2),new(true,new(900,0)),false,Read(scene));Assert.Equal(SnackPhase.None,pet.Snack);
    }
    [Fact]
    public void FallingTrajectoryMatchesUnmodifiedWindowsPhysics()
    {
        var scene=new DesktopScene(1,TimeSpan.Zero,monitors,[]);var pet=Pet(new(200,100));
        var reference=new PlatformMotion();reference.Reset(pet.Position);
        var contact=DesktopFrames.Measure(DesktopFrames.Place(MotionRenderer.Standing(),false)).Contact;
        var surfaces=PlatformGeometry.Build(scene,contact.SoleY-contact.VisibleTop);
        for(var i=0;i<100;i++)
        {
            var expected=reference.Advance(new(step,reference.Current.Position,reference.Current.Position,contact,surfaces,false,true){Monitors=monitors});
            pet.Advance(step,new(true,new(1000,0)),false,Read(scene));
            Assert.Equal(expected.Position.X,pet.Position.X,7);Assert.Equal(expected.Position.Y,pet.Position.Y,7);
        }
    }
    [Fact]
    public void MonitorRemovalClampsAndOwnInputIsAlphaBounded()
    {
        var pet=Pet(new(-900,500));var scene=new DesktopScene(1,TimeSpan.Zero,monitors,[]);
        pet.Advance(step,new(true,new(900,0)),false,Read(scene));Assert.True(pet.Position.X>=-100);
        Assert.False(pet.Hit(pet.Position));Assert.False(pet.Hit(new(double.NaN,0)));
        Assert.Contains(Enumerable.Range(0,144),y=>Enumerable.Range(0,144).Any(x=>pet.Hit(pet.Position+new PointD(x,y))));
    }
}

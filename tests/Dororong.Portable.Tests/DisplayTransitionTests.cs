using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Portable.Desktop;

namespace Dororong.Portable.Tests;

public sealed class DisplayTransitionTests
{
    public static IEnumerable<object[]> Layouts()
    {
        foreach(var width in new[]{1280,1920,2560})
        foreach(var placement in new[]{"left","right","above","below","primary","clamshell"})
            yield return [width,placement];
    }
    private static DesktopMonitor[] Layout(int width,string placement)
    {
        var internalScreen=new DesktopMonitor(1,new(0,0,1470,956));
        var external=new DesktopMonitor(2,placement switch {
            "left"=>new(-width,0,width,1080),"right"=>new(1470,0,width,1080),
            "above"=>new(0,-1080,width,1080),"below"=>new(0,956,width,1080),
            _=>new RectD(0,0,width,1080)});
        return placement switch {"clamshell"=>[external],"primary"=>[external,internalScreen with{Bounds=new(width,0,1470,956)}],_=>[internalScreen,external]};
    }
    [Theory][MemberData(nameof(Layouts))]
    public void ChangedLayoutCannotPublishOldWindowCoordinatesDuringReadFailure(int width,string placement)
    {
        DesktopMonitor[] original=[new(1,new(0,0,1470,956))];
        var next=Layout(width,placement);var cache=new SceneCache();var mapper=new MacSceneMapper();
        var before=mapper.Build(TimeSpan.Zero,original,[new(10,20,new(300,300,600,400),0,1,true,false)],99,20,24);
        cache.Read(TimeSpan.Zero,_=>before,original);
        var result=cache.Read(TimeSpan.FromMilliseconds(16),_=>null,next);
        Assert.Equal(next,result.Scene.Monitors);
        Assert.Empty(result.Scene.Windows);Assert.False(result.CanAcquireWindow);
    }
    [Fact]
    public async Task DelayedOldLayoutCompletionMustNotResurrectRemovedDisplay()
    {
        DesktopMonitor[] original=[new(1,new(0,0,1470,956))];var next=Layout(1920,"clamshell");
        using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
        var calls=0;var cache=new SceneCache(background:true);
        DesktopScene? Capture(TimeSpan at){if(Interlocked.Increment(ref calls)==2){entered.Set();release.Wait(TimeSpan.FromSeconds(5));}return new(1,at,original,[]);}
        cache.Read(TimeSpan.Zero,Capture,original);cache.Read(TimeSpan.FromMilliseconds(80),Capture,original);
        try{
            Assert.True(entered.Wait(TimeSpan.FromSeconds(3)));
            var changed=cache.Read(TimeSpan.FromMilliseconds(96),Capture,next);
            Assert.Equal(next,changed.Scene.Monitors);
            release.Set();
            // A third query proves that the old pending result was consumed,
            // rather than merely asserting fallback while it is still running.
            for(var i=0;i<300&&Volatile.Read(ref calls)<3;i++){
                await Task.Delay(5);
                var completed=cache.Read(TimeSpan.FromMilliseconds(112+i),Capture,next);
                Assert.Equal(next,completed.Scene.Monitors);Assert.False(completed.CanAcquireWindow);
            }
            Assert.True(Volatile.Read(ref calls)>=3);
        }finally{release.Set();}
    }
    [Theory][MemberData(nameof(Layouts))]
    public void RecoversOnlyFromMatchingFreshSceneAndCanReopenInternalPanel(int width,string placement)
    {
        DesktopMonitor[] original=[new(1,new(0,0,1470,956))];var next=Layout(width,placement);
        var cache=new SceneCache();var mapper=new MacSceneMapper();
        DesktopScene Scene(TimeSpan at,DesktopMonitor[] screens)=>mapper.Build(at,screens,
            [new(10,20,new(screens[0].Bounds.X+50,screens[0].Bounds.Y+300,600,400),0,1,true,false)],99,20,24);
        cache.Read(TimeSpan.Zero,t=>Scene(t,original),original);
        Assert.False(cache.Read(TimeSpan.FromMilliseconds(16),t=>Scene(t,original),next).CanAcquireWindow);
        var fresh=cache.Read(TimeSpan.FromMilliseconds(96),t=>Scene(t,next),next);
        Assert.True(fresh.CanAcquireWindow);Assert.Equal(next,fresh.Scene.Monitors);Assert.Single(fresh.Scene.Windows);
        var reopened=cache.Read(TimeSpan.FromMilliseconds(112),t=>Scene(t,original),original);
        Assert.True(reopened.CanAcquireWindow);Assert.Equal(original,reopened.Scene.Monitors);
    }
    [Fact]
    public void CoordinateConversionPreservesUpperAndNegativeDesktopPoints()
    {
        foreach(var height in new[]{720,956,1080,1440})
        foreach(var point in new[]{new PointD(-2560,-1440),new(0,0),new(1470,-96),new(1920,1080)}){
            var cocoa=MacSceneMapper.ToCocoaTopLeft(point,height);
            Assert.Equal(point,new PointD(cocoa.X,height-cocoa.Y));
            Assert.True(MacSceneMapper.Valid(new(point.X,point.Y,408,408)));
        }
    }
    [Theory][InlineData(1280,720)][InlineData(2560,1440)][InlineData(1470,900)]
    public void ResolutionChangeWithSameDisplayIdAlsoInvalidatesCoordinates(int width,int height)
    {
        DesktopMonitor[] original=[new(1,new(0,0,1470,956))];
        DesktopMonitor[] next=[new(1,new(0,0,width,height))];
        var cache=new SceneCache();cache.Read(TimeSpan.Zero,t=>new(1,t,original,[]),original);
        var result=cache.Read(TimeSpan.FromMilliseconds(16),_=>null,next);
        Assert.Equal(next,result.Scene.Monitors);Assert.False(result.CanAcquireWindow);
    }
    [Fact] public void EquivalentMonitorValuesDoNotInvalidateOrIncreaseQueryRate()
    {
        var cache=new SceneCache();var calls=0;
        DesktopMonitor[] Monitors()=>[new(1,new(0,0,1470,956))];
        DesktopScene Capture(TimeSpan at){calls++;return new(calls,at,Monitors(),[]);}
        cache.Read(TimeSpan.Zero,Capture,Monitors());
        for(var ms=1;ms<80;ms++)Assert.Equal(SceneHealth.Cached,cache.Read(TimeSpan.FromMilliseconds(ms),Capture,Monitors()).Health);
        Assert.Equal(1,calls);
    }
}

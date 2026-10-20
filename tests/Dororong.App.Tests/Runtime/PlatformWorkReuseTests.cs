using System.Reflection;
using Dororong.App.Runtime;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;

namespace Dororong.App.Tests.Runtime;

public sealed class PlatformWorkReuseTests(Xunit.Abstractions.ITestOutputHelper output)
{
    private sealed class Native:IDesktopSceneNative
    {
        internal DesktopScene Scene=new(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],
            Enumerable.Range(1,200).Select(i=>new DesktopWindow(new(i,1,1),new(i,100+i,400,300),i,false,false,false,false,true,true,false,false)).ToArray());
        public DesktopScene Capture(TimeSpan now)=>Scene;
    }
    private static T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(o)!;
    private static IReadOnlyList<PlatformSurface> Surfaces(PetPlatformRuntime runtime,double height)=>
        (IReadOnlyList<PlatformSurface>)typeof(PetPlatformRuntime).GetMethod("BuildHeightSurfaces",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(runtime,[height])!;
    [Fact]
    public void Fresh_but_identical_snapshots_keep_geometry_without_reallocating_all_windows(){
        var native=new Native();var map=new DesktopCoordinateMap(new(20,10),new(-20,-10),1.25,1.25);
        var runtime=new PetPlatformRuntime(new(native),_=>map,()=>(new FootContact(20,70,100,20),new RectD(20,20,50,80)),(_,_)=>{});
        runtime.BeginFrame(TimeSpan.Zero,default,PointerSample.Unavailable,false);
        var first=Field<DesktopScene>(runtime,"_scene");var surfaces=Surfaces(runtime,80);
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=1;i<=100;i++){
            native.Scene=native.Scene with{Revision=i+1,CapturedAt=TimeSpan.FromMilliseconds(i*80)};
            runtime.BeginFrame(TimeSpan.FromMilliseconds(i*80),default,PointerSample.Unavailable,false);
            Surfaces(runtime,80);
        }
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        output.WriteLine($"100 fresh identical200-window snapshots: {allocated:N0} bytes");
        Assert.True(allocated<400_000,$"Allocated {allocated:N0} bytes for unchanged geometry.");
        var last=Field<DesktopScene>(runtime,"_scene");Assert.Equal(101,last.Revision);Assert.Equal(TimeSpan.FromSeconds(8),last.CapturedAt);
        Assert.Same(first.Windows,last.Windows);Assert.Same(surfaces,Surfaces(runtime,80));
    }
    [Fact]
    public void Cached_scene_and_ineligible_perch_do_not_rebuild_window_collections_each_tick()
    {
        var native=new Native();var contact=new FootContact(20,70,100,20);
        var runtime=new PetPlatformRuntime(new(native),_=>new(new(),new(),1,1),()=>(contact,new RectD(20,20,50,80)),(_,_)=>{},
            measurePerch:_=>new(20,70,30,20));
        for(var i=0;i<10;i++){runtime.BeginFrame(TimeSpan.Zero,new(100,980),PointerSample.Unavailable,false);runtime.TryBeginPerch(new(100,980),FacingDirection.Right,false,true);}
        var scene=Field<DesktopScene>(runtime,"_scene");var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<1000;i++){
            runtime.BeginFrame(TimeSpan.Zero,new(100,980),PointerSample.Unavailable,false);
            runtime.TryBeginPerch(new(100,980),FacingDirection.Right,false,true);
        }
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        output.WriteLine($"1000 unchanged scene/ineligible perch ticks: {allocated:N0} bytes.");
        Assert.True(allocated<1_000_000,$"Allocated {allocated:N0} bytes for unchanged metadata.");
        Assert.Same(scene,Field<DesktopScene>(runtime,"_scene"));
    }
    [Fact]
    public void Scene_and_map_changes_invalidate_conversion_including_same_revision()
    {
        var native=new Native();var map=new DesktopCoordinateMap(new(),new(),1,1);
        var runtime=new PetPlatformRuntime(new(native),_=>map,()=>(new FootContact(20,70,100,20),new RectD(20,20,50,80)),(_,_)=>{});
        runtime.BeginFrame(TimeSpan.Zero,default,PointerSample.Unavailable,false);
        var first=Field<DesktopScene>(runtime,"_scene");
        native.Scene=native.Scene with{Windows=[native.Scene.Windows[0] with{Bounds=new(30,40,50,60),Visible=true}]};
        runtime.BeginFrame(TimeSpan.FromMilliseconds(80),default,PointerSample.Unavailable,false);
        var moved=Field<DesktopScene>(runtime,"_scene");Assert.NotSame(first,moved);Assert.Equal(30,moved.Windows[0].Bounds.X);
        map=new(new(),new(),1.5,1.5);
        runtime.BeginFrame(TimeSpan.FromMilliseconds(81),default,PointerSample.Unavailable,false);
        var scaled=Field<DesktopScene>(runtime,"_scene");Assert.NotSame(moved,scaled);Assert.Equal(20,scaled.Windows[0].Bounds.X);
    }
    [Fact]
    public void Cached_surfaces_keep_original_geometry_for_height_window_and_monitor_changes()
    {
        var native=new Native();
        native.Scene=native.Scene with{Windows=native.Scene.Windows.Take(5).Select(w=>w with{Visible=true}).ToArray()};
        var runtime=new PetPlatformRuntime(new(native),_=>new(new(),new(),1,1),()=>(new FootContact(20,70,100,20),new RectD(20,20,50,80)),(_,_)=>{});
        for(var step=0;step<4;step++){
            if(step==1)native.Scene=native.Scene with{Windows=native.Scene.Windows.Select(w=>w with{Bounds=w.Bounds with{Y=400}}).ToArray()};
            if(step==2)native.Scene=native.Scene with{Monitors=[new(2,new(-300,-100,600,500))]};
            if(step==3)native.Scene=native.Scene with{Windows=[]};
            runtime.BeginFrame(TimeSpan.FromMilliseconds(step*80),default,PointerSample.Unavailable,false);
            var logical=Field<DesktopScene>(runtime,"_scene");
            foreach(var height in new[]{0d,90,400,1081,double.NaN,-1}){
                var actual=Surfaces(runtime,height);
                Assert.Equal(PlatformGeometry.Build(logical,height),actual);
                if(double.IsFinite(height))Assert.Same(actual,Surfaces(runtime,height));
            }
            var method=typeof(PetPlatformRuntime).GetMethod("BuildPerchSurfaces",BindingFlags.Instance|BindingFlags.NonPublic)!;
            var perches=(PerchSurface[])method.Invoke(runtime,null)!;
            Assert.Equal(PlatformGeometry.Build(logical,0),perches.Select(p=>p.Surface));
            Assert.Same(perches,method.Invoke(runtime,null));
        }
    }
}

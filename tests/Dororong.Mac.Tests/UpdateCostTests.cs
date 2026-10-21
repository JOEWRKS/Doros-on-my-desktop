using System.Reflection;
using System.Runtime.InteropServices;
using Dororong.App.Controls;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
using Dororong.Mac.Native;
using Dororong.Mac.Rendering;
using Dororong.Portable.Desktop;

namespace Dororong.Mac.Tests;

public sealed class UpdateCostTests
{
    [Fact] public void IdenticalNativeStateIsAppliedOnceAndChangesAreImmediate()
    {
        var s=new WindowUpdateState();
        Assert.True(s.PositionChanged(new(10,20)));Assert.True(s.ClickThroughChanged(false));
        for(var i=0;i<1000;i++){Assert.False(s.PositionChanged(new(10,20)));Assert.False(s.ClickThroughChanged(false));}
        Assert.True(s.PositionChanged(new(10.001,20)));Assert.True(s.PositionChanged(new(10.001,21)));
        Assert.True(s.ClickThroughChanged(true));Assert.True(s.ClickThroughChanged(false));
        s.InvalidatePosition();Assert.True(s.PositionChanged(new(10.001,21)));
    }

    [Theory][InlineData(8,32)][InlineData(8,48)][InlineData(768,3072)]
    public void ContiguousAndStridedCopiesKeepEveryPixelAndPadding(int extent,int stride)
    {
        var source=Enumerable.Range(0,extent*extent*4).Select(i=>(byte)(i*31)).ToArray();
        var memory=Marshal.AllocHGlobal(stride*extent);
        try{
            Marshal.Copy(Enumerable.Repeat((byte)173,stride*extent).ToArray(),0,memory,stride*extent);
            R104Surface.CopyPixels(source,extent,memory,stride);
            var result=new byte[stride*extent];Marshal.Copy(memory,result,0,result.Length);
            for(var y=0;y<extent;y++){
                Assert.Equal(source.AsSpan(y*extent*4,extent*4).ToArray(),result.AsSpan(y*stride,extent*4).ToArray());
                Assert.All(result.AsSpan(y*stride+extent*4,stride-extent*4).ToArray(),b=>Assert.Equal(173,b));
            }
        }finally{Marshal.FreeHGlobal(memory);}
    }

    [Fact] public void GeometryCacheMatchesFreshScanAcrossFramesDensitySizeAndOffsets()
    {
        using var c=new R104DesktopController(new(400,300),new R());c.SetAutomaticSleep(false);
        const BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var p=(DororongPresenter)typeof(R104DesktopController).GetField("presenter",flags)!.GetValue(c)!;
        var scan=typeof(DororongPresenter).GetMethod("Measure",flags)!;
        var scene=new SceneSnapshot(new(1,TimeSpan.Zero,[new(1,new(0,0,1920,1080))],[]),SceneHealth.Fresh);
        void Check(){
            var expected=scan.Invoke(p,[p.Pixels,p.ReleaseOffset]);
            Assert.Equal(expected,(object?)c.Geometry);
            var count=p.GeometryScans;
            for(var i=0;i<20;i++)Assert.Equal(expected,(object?)c.Geometry);
            Assert.Equal(count,p.GeometryScans);
        }
        for(var i=0;i<95;i++){c.Advance(TimeSpan.FromMilliseconds(16),PointerSample.Unavailable,false,scene);Check();}
        foreach(var density in new[]{2d,1d})foreach(var size in new[]{125,150,100}){
            c.Density=density;Check();Assert.True(c.RequestSize(size));
            c.Advance(TimeSpan.Zero,PointerSample.Unavailable,false,scene);Check();
        }
        foreach(var field in new[]{"_hrRecoil","_releaseOffset"}){
            var member=typeof(DororongPresenter).GetField(field,flags)!;
            var original=member.GetValue(p);member.SetValue(p,field=="_hrRecoil"?(object)new PointD(.25,-.75):2.5);
            Check();member.SetValue(p,original);Check();
        }
    }
    private sealed class R:IRandomSource{public double NextUnit()=>.5;}

    [Theory][InlineData(1,1)][InlineData(1.25,2)][InlineData(1.5,2)]
    public void OwnedFrameEqualsCopyPathAndSoleQueriesCannotOverwriteIt(double size,double density)
    {
        var renderer=new HighResolutionGrabRaster();
        var identity=System.Windows.Media.Matrix.Identity;
        var face=HighResolutionFacePose.Neutral;
        byte[]? owned=null;
        foreach(var progress in new[]{0d,.45,1,.2,0}){
            var expected=renderer.RenderRetainedPixels(progress,identity,new(37.5,28.25),default,default,0,size*density,face,size,null);
            var previous=owned;
            owned=renderer.RenderOwnedPixels(progress,identity,new(37.5,28.25),default,default,0,size*density,face,size,owned);
            Assert.Equal(expected,owned);if(previous is not null)Assert.Same(previous,owned);
            renderer.RenderSolePixels(.8,identity,new(37.5,28.25),default,default,0,size*density,face,2*density,size);
            Assert.Equal(expected,owned);
        }
    }
}

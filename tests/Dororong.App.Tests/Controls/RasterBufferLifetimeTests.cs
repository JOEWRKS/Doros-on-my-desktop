using System.Runtime.CompilerServices;
using System.Windows.Media;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class RasterBufferLifetimeTests
{
    [Fact]
    public void Neck_and_ink_reuse_match_independent_pixels_and_clear_old_edges()=>CheekProductTests.Sta(()=>{
        var buffer=new HighResolutionHuntNeck.Buffer();
        foreach(var scale in new[]{1d,1.1,1.25,1.35,1.5})foreach(var frame in new[]{60,90,125,70,156,60}){
            var face=HighResolutionFacePose.Neutral with{BodyFrame=frame,Head=HuntRenderer.HeadTransform(frame,new(.2,.3,.06))};
            var actual=buffer.Get(face);
            var expected=HighResolutionHuntNeck.Render(face);
            Assert.Equal(expected,actual);
            var ink=HighResolutionBodyInk.AtScale(actual,225,scale);
            Assert.Equal(HighResolutionBodyInk.AtScale(expected,225,scale),ink);
            Assert.Equal(RasterAlphaBounds.Get(expected,225),RasterAlphaBounds.Get(actual,225));
            Assert.Same(actual,buffer.Get(face with{Pull=10,EyeX=.8}));
        }
    });

    [Fact]
    public void Weak_caches_do_not_root_discarded_source_arrays(){
        var weak=MakeSource();
        for(var i=0;i<3&&weak.IsAlive;i++){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
        Assert.False(weak.IsAlive);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MakeSource(){
        var pixels=new float[16*16*4];pixels[3]=255;
        HighResolutionBodyInk.AtScale(pixels,16,1.5);RasterAlphaBounds.Get(pixels,16);
        return new WeakReference(pixels);
    }

    [Fact]
    public void Completed_render_workers_do_not_root_discarded_raster()=>CheekProductTests.Sta(()=>{
        var weak=MakeRaster();
        for(var i=0;i<5&&weak.IsAlive;i++){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();Thread.Sleep(10);}
        Assert.False(weak.IsAlive);
    });

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference MakeRaster(){
        var raster=new HighResolutionGrabRaster();
        raster.Render(1,Matrix.Identity,new(37.5,28.25),default,default,0,1.5);
        return new WeakReference(raster);
    }
}

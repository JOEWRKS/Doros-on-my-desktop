using System.Windows.Media;
using Dororong.App.Controls;
using Dororong.Core.Geometry;

namespace Dororong.Raster.Tests;

public sealed class DirtyRegionTests
{
    public static IEnumerable<object[]> Sizes=>from size in new[]{1d,1.125,1.25,1.375,1.5}
        from density in new[]{1d,2d} select new object[]{size,density};

    [Theory][MemberData(nameof(Sizes))]
    public void PartialFramesMatchOriginalFullSamplerThroughBoundsAndFallbacks(double size,double density)
    {
        var partial=new HighResolutionGrabRaster();var full=new HighResolutionGrabRaster();
        byte[]? pixels=null;byte[]? reference=null;var partialCount=0;var fallbackCount=0;
        var transforms=new[]{Matrix.Identity,new Matrix(-1,0,0,1,96,0),new Matrix(.98,.13,-.16,1.02,2,-3)};
        foreach(var transform in transforms){
            var head=Matrix.Identity;head.RotateAt(8,37,28);
            var face=HighResolutionFacePose.Neutral with {Head=head};
            void Frame(HighResolutionFacePose f,double spread=0,PointD near=default,double progress=0){
                reference=full.RenderRetainedPixels(progress,transform,new(37.5,28.25),near,default,spread,size*density,f,size,reference,true);
                pixels=partial.RenderOwnedPixels(progress,transform,new(37.5,28.25),near,default,spread,size*density,f,size,pixels,true);
                var length=R104Raster.Extent(size,density);length*=length*4;
                Assert.True(reference.AsSpan(0,length).SequenceEqual(pixels.AsSpan(0,length)),
                    $"Mismatch size={size},density={density},face={f},spread={spread},region={partial.LastDirtyRegion}");
                if(partial.LastDirtyRegion is not null)partialCount++;else fallbackCount++;
            }
            Frame(face);
            foreach(var amount in new[]{.001,.12,.5,1,.9,.01,0})Frame(face with {Breathing=amount});
            foreach(var eye in new[]{new PointD(-.85,.65),new(.85,-.65),new(.01,0),new(-.85,-.65),default})
                Frame(face with {EyeX=eye.X,EyeY=eye.Y});
            Frame(face with {Breathing=.7,EyeX=.85,EyeY=.65});Frame(face);
            Frame(face with {Closed=true});Assert.Null(partial.LastDirtyRegion);
            Frame(face);Assert.Null(partial.LastDirtyRegion);
            Frame(face with {Pull=20,EyeFollow=8,HairFollow=10});Assert.Null(partial.LastDirtyRegion);
            Frame(face with {Pull=-1.5});Frame(face);Frame(face with {Breathing=.8});
            Frame(face,spread:1);Assert.Null(partial.LastDirtyRegion);Frame(face);
            Frame(face,near:new(3,-2),progress:.6);Assert.Null(partial.LastDirtyRegion);Frame(face);
            Frame(face with {MotionFrame=240});Frame(face with {MotionFrame=241});Assert.Null(partial.LastDirtyRegion);
            Frame(face with {MotionFrame=241,EyeX=.85});Assert.NotNull(partial.LastDirtyRegion);
            Frame(face with {MouthAge=.2});Frame(face with {MouthAge=.6});Assert.Null(partial.LastDirtyRegion);
            Frame(face with {DroolAge=.5});Assert.Null(partial.LastDirtyRegion);Frame(face);
        }
        Assert.True(partialCount>=30);Assert.True(fallbackCount>=30);
        Assert.Equal(1,partial.OwnedFrameAllocations);
    }

    [Fact] public void SliderDensityAndDifferentBufferInvalidateRegionAndReuseHighWater()
    {
        var renderer=new HighResolutionGrabRaster();var full=new HighResolutionGrabRaster();
        byte[]? owned=null;byte[]? expected=null;
        foreach(var size in new[]{1.5,1,1.125,1.375,1.25,1.5})foreach(var density in new[]{2d,1d,2d}){
            var face=HighResolutionFacePose.Neutral with {Breathing=.4};
            owned=renderer.RenderOwnedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,size*density,face,size,owned,true);
            Assert.Null(renderer.LastDirtyRegion);
            expected=full.RenderRetainedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,size*density,face,size,expected,true);
            var length=R104Raster.Extent(size,density);length*=length*4;
            Assert.True(expected.AsSpan(0,length).SequenceEqual(owned.AsSpan(0,length)));
        }
        Assert.Equal(1,renderer.OwnedFrameAllocations);
        owned=renderer.RenderOwnedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,3,
            HighResolutionFacePose.Neutral with {Breathing=.5},1.5,new byte[768*768*4],true);
        Assert.Null(renderer.LastDirtyRegion);
    }

    [Fact] public void RepeatedPartialFramesUseOneOwnedBufferAndLeaveScratchIndependent()
    {
        var r=new HighResolutionGrabRaster();byte[]? bytes=null;byte[]? first=null;long sampled=0;
        for(var i=0;i<400;i++){
            var face=HighResolutionFacePose.Neutral with {Breathing=(i%100)/100d,EyeX=(i%3-1)*.85};
            bytes=r.RenderOwnedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,3,face,1.5,bytes,true);
            if(i==0)first=bytes;else{Assert.Same(first,bytes);Assert.NotNull(r.LastDirtyRegion);sampled+=r.LastRenderedPixelCount;}
            // Sole sampling uses its own scratch and cannot seed/overwrite the retained image.
            if(i%40==0){
                var copy=(byte[])bytes.Clone();
                r.RenderSolePixels(.6,Matrix.Identity,new(37.5,28.25),default,default,0,3,face,6,1.5);
                Assert.Equal(copy,bytes);
            }
        }
        Assert.Equal(1,r.OwnedFrameAllocations);
        Assert.True(sampled<399L*768*768/4);
        // Identity reuse skips all sampling; no new image cache is created.
        var last=HighResolutionFacePose.Neutral with {Breathing=.99,EyeX=-.85};
        r.RenderOwnedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,3,last,1.5,bytes,true);
        Assert.Equal(0,r.LastRenderedPixelCount);
    }

    [Fact] public void ClippedRegionsAtEveryCanvasEdgeAndOffCanvasMatchFullFrame()
    {
        var r=new HighResolutionGrabRaster();var full=new HighResolutionGrabRaster();
        byte[]? owned=null;byte[]? expected=null;
        foreach(var offset in new[]{new PointD(-140,0),new(145,0),new(0,-140),new(0,145),new(400,400)}){
            var inverse=Matrix.Identity;inverse.Translate(-offset.X,-offset.Y);
            foreach(var value in new[]{0d,1,.2,0}){
                var face=HighResolutionFacePose.Neutral with {Breathing=value,EyeX=value*.85,EyeY=-value*.65};
                owned=r.RenderOwnedPixels(0,inverse,new(37.5,28.25),default,default,0,1,face,1,owned,true);
                expected=full.RenderRetainedPixels(0,inverse,new(37.5,28.25),default,default,0,1,face,1,expected,true);
                Assert.Equal(expected,owned);
            }
        }
    }
}

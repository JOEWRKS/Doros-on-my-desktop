using System.Reflection;
using System.Windows.Media;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class PreparedMotionAndSoleTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Prepared_motion_matches_original_stroke_and_keeps_one_bounded_cache(double scale){
        var bank=new HighResolutionLocomotion();var reference=new HighResolutionLocomotion();
        foreach(var frame in new[]{1,72,225,256,257,280,321,322,658,705,706,770,836}){
            var actual=bank.GetBody(frame,scale);
            Assert.True(actual.SequenceEqual(HighResolutionBodyInk.AtScale(reference.Get(frame),225,scale)),$"frame {frame}");
        }
        var retained=bank.GetBody(225,scale);var snapshot=(float[])retained.Clone();
        for(var i=1;i<200;i++)bank.GetBody(i,scale);
        Assert.True(snapshot.SequenceEqual(retained));
        var cache=(System.Collections.IDictionary)typeof(HighResolutionLocomotion).GetField("cache",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(bank)!;
        Assert.Equal(64,cache.Count);
        var frames=Enumerable.Range(225,32).Select(f=>bank.GetBody(f,scale)).ToArray();
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var lap=0;lap<20;lap++)for(var i=0;i<32;i++)Assert.Same(frames[i],bank.GetBody(225+i,scale));
        Assert.InRange(GC.GetAllocatedBytesForCurrentThread()-before,0,65536);
    }

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Borrowed_sole_pixels_match_bitmap_geometry(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();var measurement=new PlatformContactPresentation();
        foreach(var spread in new[]{0d,.2,.6,1})foreach(var angle in new[]{-20d,0,20}){
            var inverse=Matrix.Identity;inverse.RotateAt(angle,43,32);inverse.ScaleAt(.9,1.1,48,87);
            var face=HighResolutionFacePose.Neutral;
            var bitmap=raster.Render(.4,inverse,new(37.5,28.25),default,default,spread,scale,face,true,2*scale);
            var copy=new byte[bitmap.PixelWidth*bitmap.PixelHeight*4];bitmap.CopyPixels(copy,bitmap.PixelWidth*4,0);
            var transform=new TransformGroup();transform.Children.Add(new ScaleTransform(1/scale,1/scale));transform.Children.Add(new TranslateTransform(-56*scale,-56*scale));
            var expected=measurement.Measure(bitmap,transform);var expectedBounds=measurement.VisibleBounds;
            var pixels=raster.RenderSolePixels(.4,inverse,new(37.5,28.25),default,default,spread,scale,face,2*scale,scale);
            Assert.True(copy.SequenceEqual(pixels));
            Assert.Equal(expected,measurement.MeasurePixels(pixels,bitmap.PixelWidth,bitmap.PixelHeight,transform));
            Assert.Equal(expectedBounds,measurement.VisibleBounds);Assert.Equal(0,raster.LastCost.Bitmap);
        }
    });
}

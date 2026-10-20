using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class ContactBufferReuseTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void Repeated_live_and_sparse_contact_reuses_warmed_geometry_storage()=>CheekProductTests.Sta(()=>{
        var pixels=new byte[256*256*4];
        for(var y=40;y<200;y++)for(var x=40;x<200;x++)pixels[(y*256+x)*4+3]=255;
        var helper=new PlatformContactPresentation();var transform=Transform.Identity;
        for(var i=0;i<30;i++){
            PlatformContactPresentation.InvalidateRetainedPixels(pixels);
            helper.MeasureRetainedPixels(pixels,256,256,transform);
            helper.MeasurePixels(pixels,256,256,transform);
        }
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<1000;i++){
            PlatformContactPresentation.InvalidateRetainedPixels(pixels);
            helper.MeasureRetainedPixels(pixels,256,256,transform);
            helper.MeasurePixels(pixels,256,256,transform);
        }
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        output.WriteLine($"Warmed 1000 contact pairs allocated {allocated:N0} bytes.");
        Assert.True(allocated<1_000_000,$"Warmed 1000 contact pairs allocated {allocated:N0} bytes.");
    });

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Shrinking_reused_contours_preserve_frozen_geometry_and_ignore_stale_tail(double scale)=>CheekProductTests.Sta(()=>{
        var pixels=new byte[32*32*4];var helper=new PlatformContactPresentation();
        for(var phase=0;phase<4;phase++){
            Array.Clear(pixels);
            var size=phase==0?28:phase==1?2:phase==2?0:15;
            for(var y=0;y<size;y++)for(var x=0;x<size;x++)if((x+y)%5!=0)pixels[(y*32+x)*4+3]=255;
            var matrix=Matrix.Identity;matrix.Scale(scale,scale);matrix.Rotate(phase*23-20);
            var transform=new MatrixTransform(matrix);
            var source=BitmapSource.Create(32,32,96,96,PixelFormats.Pbgra32,null,pixels,128);source.Freeze();
            var expected=new PlatformContactPresentation();var contact=expected.Measure(source,transform);
            PlatformContactPresentation.InvalidateRetainedPixels(pixels);
            Assert.Equal(contact,helper.MeasureRetainedPixels(pixels,32,32,transform));
            Assert.Equal(expected.VisibleBounds,helper.VisibleBounds);
            Assert.Equal(contact,helper.MeasurePixels(pixels,32,32,transform));
            Assert.Equal(expected.VisibleBounds,helper.VisibleBounds);
        }
    });
}

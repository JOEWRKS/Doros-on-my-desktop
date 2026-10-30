using System.Reflection;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class RibbonJunctionTests
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [Fact]
    public void FoldReachesTheRightContourWithoutTheOldDanglingTip()=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        var head=(float[])typeof(HighResolutionGrabRaster).GetField("head",Private)!.GetValue(raster)!;
        for(var x=153;x<=160;x++){
            var y=106.8+(x+.5-151.8)*3.8/9.45;
            var row=(int)Math.Floor(y);
            var ink=Enumerable.Range(row-1,3).Min(r=>head[(r*225+x)*4]);
            Assert.True(ink<160,$"Fold disconnected at column {x}");
        }
        Assert.True(head[(112*225+156)*4]>235,"Old downward tip remains");
    });

    [Fact]
    public void OriginalHeadIsExactOutsideReportedExteriorSpur()=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        var original=(float[])typeof(HighResolutionGrabRaster).GetMethod("Read",Private)!
            .Invoke(raster,new object?[]{0,225,null})!;
        var actual=(float[])typeof(HighResolutionGrabRaster).GetField("head",Private)!.GetValue(raster)!;
        Assert.True(original[(112*225+164)*4+3]>128); // reproduced protruding body ink
        Assert.Equal(0,actual[(112*225+164)*4+3]);
        var changes=0;
        for(var y=0;y<225;y++)for(var x=0;x<225;x++)for(var c=0;c<4;c++){
            var at=(y*225+x)*4+c;
            if(original[at]==actual[at])continue;
            changes++;
            var spur=x>=162&&x<=166&&y>=109&&y<=116;
            var fold=x>=153&&x<=161&&y>=107&&y<=114;
            Assert.True(spur||fold);
            if(spur)Assert.True(actual[at]<=original[at]);
            if(fold&&c==3)Assert.Equal(original[at],actual[at]);
        }
        Assert.True(changes>0);
        // The ribbon above and its adjoining contour below retain source coverage.
        foreach(var p in new[]{(162,106),(161,117)})
            for(var c=0;c<4;c++)Assert.Equal(original[(p.Item2*225+p.Item1)*4+c],actual[(p.Item2*225+p.Item1)*4+c]);
    });

    [Theory][InlineData("closed")][InlineData("angry")][InlineData("furious")]
    public void HeadVariantsDoNotRestoreTheSpur(string variant)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        var pixels=(float[])(variant=="closed"
            ? typeof(HighResolutionGrabRaster).GetMethod("ClosedHead",Private)!.Invoke(raster,null)!
            : typeof(HighResolutionGrabRaster).GetMethod("AngryHead",Private)!.Invoke(raster,new object[]{variant=="furious"})!);
        Assert.Equal(0,pixels[(112*225+164)*4+3]);
        Assert.True(pixels[(112*225+160)*4+3]>128);
        Assert.True(pixels[(106*225+162)*4+3]>128);
        foreach(var i in Enumerable.Range(0,pixels.Length/4))for(var c=0;c<3;c++)
            Assert.InRange(pixels[i*4+c],0,pixels[i*4+3]+.001f);
    });
}

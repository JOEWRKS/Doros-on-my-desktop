using System.Security.Cryptography;
using System.Reflection;
using Dororong.Raster;
using System.Windows.Media;

namespace Dororong.Raster.Tests;

public sealed class RasterTests
{
    [Theory]
    [InlineData("neutral","1F38A0EB13F23A7A41C9F76A7171AE12DAA56ED99EFF5EB00B8B5134E427E5D9")]
    [InlineData("closed","010EA6438B35F56CAFF68D7279724132158F813235634B37BCAC2A2ED9781BDB")]
    [InlineData("cheek","FE68111F4F5426F13757989E51C6150503FEBE671C8F83D827267FA6EF50C023")]
    public void SourcePixelsMatchAcceptedR104OutsideRibbonRepair(string name,string expected)
    {
        var pose = name switch
        {
            "closed" => new RasterPose { Closed=true },
            "cheek" => new RasterPose { Pull=20, EyeX=-.6, EyeY=.4 },
            _ => new RasterPose()
        };
        var pixels = new R104Raster().Render(pose,1);
        // Retain the historical accepted hash. Restore only the repaired source
        // texels in an isolated renderer to prove all other output stays exact.
        var baseline = new R104Raster();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var sampler = typeof(R104Raster).GetField("renderer",flags)!.GetValue(baseline)!;
        var type = sampler.GetType();
        var original = (float[])type.GetMethod("Read",flags)!.Invoke(sampler,new object?[]{0,225,null})!;
        var head = (float[])type.GetField("head",flags)!.GetValue(sampler)!;
        var closed = (float[])type.GetMethod("ClosedHead",flags)!.Invoke(sampler,null)!;
        for(var y=109;y<=116;y++)for(var x=162;x<=166;x++)for(var c=0;c<4;c++){
            var at=(y*225+x)*4+c;head[at]=original[at];closed[at]=original[at];
        }
        for(var y=107;y<=114;y++)for(var x=153;x<=161;x++)for(var c=0;c<4;c++){
            var at=(y*225+x)*4+c;head[at]=original[at];closed[at]=original[at];
        }
        var oldPixels = baseline.Render(pose,1);
        Assert.Equal(expected,Convert.ToHexString(SHA256.HashData(oldPixels.AsSpan(0,256*256*4))));
        var changed=0;
        for(var i=0;i<256*256*4;i++)if(pixels[i]!=oldPixels[i]){
            changed++;var pixel=i/4;
            Assert.InRange(pixel%256,140,158);Assert.InRange(pixel/256,118,138);
        }
        Assert.True(changed>0);
    }

    [Fact]
    public void RepeatedSizeChangesReuseCallerHighWaterStorage()
    {
        var raster=new R104Raster();var pose=new RasterPose();
        var retained=raster.Render(pose,1.5);
        foreach(var size in new[]{1d,1.25,1.125,1.5,1d})
        {
            var result=raster.Render(pose,size,retained);
            Assert.Same(retained,result);
            var fresh=raster.Render(pose,size);
            var count=R104Raster.Extent(size);count*=count*4;
            Assert.True(result.AsSpan(0,count).SequenceEqual(fresh.AsSpan(0,count)));
        }
    }

    [Fact]
    public void RetinaDensityIsSeparateFromProductSize()
    {
        Assert.Equal(768,R104Raster.Extent(1.5,2));
        var result=new R104Raster().Render(new(),1,reuse:null,displayScale:2);
        Assert.Equal(512*512*4,result.Length);
        Assert.Contains(result,b=>b!=0);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(.99)]
    [InlineData(1.51)]
    public void InvalidProductScaleFailsBeforeRaster(double size) =>
        Assert.Throws<ArgumentOutOfRangeException>(()=>R104Raster.Extent(size));

    [Fact]
    public void NonFiniteAndOutOfRangePosesFail()
    {
        var raster=new R104Raster();
        Assert.Throws<ArgumentOutOfRangeException>(()=>raster.Render(new(){EyeX=double.NaN},1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>raster.Render(new(){MotionFrame=837},1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>raster.Render(new(){Progress=2},1));
        Assert.Throws<InvalidOperationException>(()=>raster.Render(new(){Transform=default},1));
    }

    [Fact]
    public void MatrixInverseRoundTripsRotationAndTranslation()
    {
        var matrix=new Matrix(.9,.4,-.4,.9,12,-5);var inverse=matrix;inverse.Invert();
        var point=new System.Windows.Point(23,58);
        var restored=inverse.Transform(matrix.Transform(point));
        Assert.Equal(point.X,restored.X,10);Assert.Equal(point.Y,restored.Y,10);
    }

    [Fact]
    public void UnstrokedMouthTopDoesNotCloseAsAnExtraLine()
    {
        var geometry=new StreamGeometry();
        using(var context=geometry.Open())
        {
            context.BeginFigure(new(27,60),true,true);
            context.QuadraticBezierTo(new(29,60.6),new(31.2,60),false,false);
            context.BezierTo(new(31,64),new(27.2,64),new(27,60),true,false);
        }
        using var path=geometry.Path(true);
        // Last point remains at the original start, not the new stroke start.
        Assert.Equal(new SkiaSharp.SKPoint(27,60),path.LastPoint);
    }
}

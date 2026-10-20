using System.Windows;
using System.Windows.Media;
using Dororong.App.Controls;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionSoleTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Sole_query_stops_at_exact_contact_band_without_extra_occupied_rows(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        foreach(var spread in new[]{.15,.5,.85,1d})foreach(var angle in new[]{-20d,0,20}){
            var inverse=Matrix.Identity;inverse.ScaleAt(.83,1.45,48,87);inverse.RotateAt(angle,48,87);
            var pixels=raster.RenderSolePixels(0,inverse,new(37.5,28.25),default,default,
                spread,scale,HighResolutionFacePose.Neutral,2*scale,scale);
            var extent=(int)Math.Ceiling(256*scale);var sole=-1;
            for(var y=extent-1;y>=0&&sole<0;y--)for(var x=0;x<extent;x++)
                if(pixels[(y*extent+x)*4+3]>=128){sole=y+1;break;}
            Assert.True(sole>0);
            // Rows merely touching the upper band edge contribute no area.
            var first=(int)Math.Floor(sole-2*scale+1e-9);
            Assert.True(pixels.AsSpan(0,first*extent*4).IndexOfAnyExcept((byte)0)<0,
                $"scale={scale}, spread={spread}, angle={angle}: sampled above contact band");
        }
    });

    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Sparse_sole_queries_match_full_quality_output(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        foreach(var spread in new[]{0d,.15,.5,.85,1})foreach(var angle in new[]{-2d,0,2}){
            var inverse=Matrix.Identity;inverse.ScaleAt(.83,1.45,48,87);inverse.RotateAt(angle,48,87);
            var face=HighResolutionFacePose.Neutral with {Breathing=.7};
            var full=raster.Render(0,inverse,new(37.5,28.25),default,default,spread,scale,face);
            var pixelScale=256*scale/full.PixelWidth;
            var sparse=raster.Render(0,inverse,new(37.5,28.25),default,default,spread,scale,face,true,2/pixelScale);
            var transform=new ScaleTransform(pixelScale,pixelScale);
            var a=new PlatformContactPresentation().Measure(full,transform);
            var b=new PlatformContactPresentation().Measure(sparse,transform);
            Assert.Equal(a.Left,b.Left);Assert.Equal(a.Right,b.Right);Assert.Equal(a.SoleY,b.SoleY);
        }
    });
}

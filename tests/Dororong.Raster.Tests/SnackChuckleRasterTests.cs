using Dororong.App.Controls;
using System.Windows.Media;

namespace Dororong.Raster.Tests;

public class SnackChuckleRasterTests
{
    [Theory][MemberData(nameof(DirtyRegionTests.Sizes),MemberType=typeof(DirtyRegionTests))]
    public void RepeatedChuckleMatchesFullAndRetainsOneOwnedBuffer(double size,double density){
        var r=new HighResolutionGrabRaster();var full=new HighResolutionGrabRaster();
        byte[]? pixels=null;byte[]? reference=null;byte[]? first=null;
        var length=R104Raster.Extent(size,density);length*=length*4;
        for(var cycle=0;cycle<3;cycle++)foreach(var age in new[]{0d,.05,.13,.25,.37,.48,.68,1.05,1.38,1.6,1.75,1.8}){
            var face=HighResolutionGrabRaster.SnackChuckle(age);
            pixels=r.RenderOwnedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,size*density,face,size,pixels,true);
            reference=full.RenderRetainedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,size*density,face,size,reference,true);
            first??=pixels;Assert.Same(first,pixels);
            Assert.True(pixels.AsSpan(0,length).SequenceEqual(reference.AsSpan(0,length)));
        }
        Assert.Equal(1,r.OwnedFrameAllocations);
    }
}

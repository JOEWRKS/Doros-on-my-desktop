using Dororong.App.Controls;
using System.Windows.Media;

namespace Dororong.Raster.Tests;

public sealed class ParallelSoleTests
{
    [Theory][MemberData(nameof(DirtyRegionTests.Sizes),MemberType=typeof(DirtyRegionTests))]
    public void ColumnWorkersMatchSerialFullAndSolePixels(double size,double density)
    {
        var raster=new HighResolutionGrabRaster();byte[]? full=null;byte[]? first=null;
        foreach(var spread in new[]{0d,.001,.22,.51,.94,1,.8,.1,0})foreach(var angle in new[]{0d,.02,-.02}){
            var matrix=new Matrix(.9,angle,-angle,1/(1-.574*spread),0,-20);
            var face=HighResolutionFacePose.Neutral with{Breathing=.8,EyeX=.8};
            var length=R104Raster.Extent(size,density);length*=length*4;
            try{
                RasterRowScheduler.CallerOnly=true;
                full=raster.RenderRetainedPixels(0,matrix,new(37.5,28.25),default,default,spread,size*density,face,size,full,true);
                var expected=raster.RenderSolePixels(0,matrix,new(37.5,28.25),default,default,spread,size*density,face,2*density,size).AsSpan(0,length).ToArray();
                RasterRowScheduler.CallerOnly=false;
                var actual=raster.RenderSolePixels(0,matrix,new(37.5,28.25),default,default,spread,size*density,face,2*density,size);
                Assert.True(actual.AsSpan(0,length).SequenceEqual(expected));
                var same=raster.RenderRetainedPixels(0,matrix,new(37.5,28.25),default,default,spread,size*density,face,size,first,true);
                Assert.True(same.AsSpan(0,length).SequenceEqual(full.AsSpan(0,length)));first=same;
            }finally{RasterRowScheduler.CallerOnly=false;}
        }
    }
}

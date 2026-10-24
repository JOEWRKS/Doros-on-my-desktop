using Dororong.App.Controls;
using Dororong.Core.Behavior;
using System.Windows.Media;

namespace Dororong.Raster.Tests;

public class SnackAngerRasterTests
{
    [Theory][MemberData(nameof(DirtyRegionTests.Sizes),MemberType=typeof(DirtyRegionTests))]
    public void ReactionReturnsExactlyAndIncrementalMatchesFull(double size,double density)
    {
        var r=new HighResolutionGrabRaster();var full=new HighResolutionGrabRaster();byte[]? pixels=null;byte[]? reference=null;
        var length=R104Raster.Extent(size,density);length*=length*4;
        byte[]? neutral=null;
        object? expressionBuffer=null;
        var expressionField=typeof(HighResolutionGrabRaster).GetField("angryHead",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
        foreach(var level in new[]{0,1,2,3,4,5,0,3,1,0})
        foreach(var age in new[]{0d,.11,.27,.55,.89,1.01,1.17,1.45,SnackAngerMotion.Duration(level)}){
            var angry=level>0;
            var face=angry?HighResolutionGrabRaster.SnackReaction(age,level):HighResolutionFacePose.Neutral;
            pixels=r.RenderOwnedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,size*density,face,size,pixels,true);
            reference=full.RenderRetainedPixels(0,Matrix.Identity,new(37.5,28.25),default,default,0,size*density,face,size,reference,true);
            if(angry){expressionBuffer??=expressionField.GetValue(r);Assert.Same(expressionBuffer,expressionField.GetValue(r));}
            Assert.True(pixels.AsSpan(0,length).SequenceEqual(reference.AsSpan(0,length)));
            if(!angry){neutral??=pixels.AsSpan(0,length).ToArray();Assert.True(neutral.AsSpan().SequenceEqual(pixels.AsSpan(0,length)));}
        }
        Assert.Equal(1,r.OwnedFrameAllocations);
    }
}

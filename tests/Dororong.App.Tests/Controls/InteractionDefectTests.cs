using System.Reflection;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public class InteractionDefectTests
{
    private static float[] Paw(HighResolutionGrabRaster raster,float[] source,HighResolutionFacePose face)=>
        (float[])typeof(HighResolutionGrabRaster).GetMethod("PerchPaw",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(raster,[source,face])!;

    [Theory]
    [InlineData(1,-22)]
    [InlineData(1,0)]
    [InlineData(1,22)]
    [InlineData(2,-22)]
    [InlineData(2,0)]
    [InlineData(2,22)]
    public void Maximum_extension_has_a_complete_tip_beyond_old_canvas(int paw,double angle){
        var source=new HighResolutionLocomotion().Get(HighResolutionLocomotion.Perch);
        var output=Paw(new(),source,HighResolutionFacePose.Neutral with{Paw=paw,PawY=18,PawAngle=angle});
        Assert.Equal(375*375*4,output.Length);
        var occupied=new List<int>();
        for(var y=0;y<375;y++)for(var x=0;x<375;x++)
            if(output[(y*375+x)*4+3]>=128)occupied.Add(y);
        Assert.True(occupied.Max()>300,"Tip must extend below the old Y96 crop");
        Assert.True(occupied.Max()<370,"Tip must have transparent gutter");
        // The stationary shoulder and all unrelated source pixels above it
        // are copied exactly, not repainted to cover the defect.
        for(var y=0;y<164;y++)for(var x=0;x<225;x++)for(var c=0;c<4;c++)
            Assert.Equal(source[(y*225+x)*4+c],output[((y+75)*375+x+75)*4+c]);
    }

    [Fact]
    public void Click_wiggle_carries_the_outer_armpit_ink_instead_of_cutting_at_x51(){
        var raster=new HighResolutionGrabRaster();
        var source=new HighResolutionLocomotion().Get(HighResolutionLocomotion.Perch);
        // At the positive tap peak the armpit at source X52,Y71 lies beyond
        // the old X51 selection, but belongs to the selected right paw.
        var output=Paw(raster,source,HighResolutionFacePose.Neutral with{Paw=2,PawAngle=22});
        var nativeX=(int)Math.Round((52+Math.Tan(22*Math.PI/180)+32)*225/96-.5);
        var nativeY=(int)Math.Round((71+32)*225/96-.5);
        var at=(nativeY*375+nativeX)*4;
        Assert.True(output[at+3]>32,"Sloping armpit must not become a transparent slice");
        Assert.True(output[at]<output[at+3]-10,"Authored contour ink must survive");
    }

    [Fact]
    public void Closed_fringe_cleanup_is_bounded_and_reused(){
        var raster=new HighResolutionGrabRaster();
        var source=new HighResolutionLocomotion().Get(0);
        var method=typeof(HighResolutionGrabRaster).GetMethod("ClosedHead",BindingFlags.Instance|BindingFlags.NonPublic)!;
        var clean=(float[])method.Invoke(raster,null)!;
        Assert.Same(clean,method.Invoke(raster,null));
        var changes=0;
        for(var i=0;i<source.Length;i+=4){
            if(source.AsSpan(i,4).SequenceEqual(clean.AsSpan(i,4)))continue;
            changes++;
            var x=i/4%225;var y=i/4/225;
            Assert.True(x>=29&&x<=115&&y>=76&&y<=136);
            Assert.Equal(source[i+3],clean[i+3]);
        }
        Assert.InRange(changes,6,2000);
        Assert.True(clean[(108*225+45)*4+2]>source[(108*225+45)*4+2]+50);
    }

    [Fact]
    public void Closed_lids_have_matching_native_ink_width_and_alpha(){
        var source=new HighResolutionLocomotion().Get(0);
        var clean=HighResolutionClosedInk.Prepare(source);
        double Width(int x,int top,int bottom){
            double width=0;
            for(var y=top;y<=bottom;y++)width+=Math.Clamp((255-clean[(y*225+x)*4+2])/210,0,1);
            return width;
        }
        var left=Enumerable.Range(50,9).Average(x=>Width(x,122,130));
        var right=Enumerable.Range(88,10).Average(x=>Width(x,127,136));
        Assert.InRange(left,1.4,2.6);Assert.InRange(right,1.4,2.6);
        Assert.InRange(Math.Abs(left-right),0,.4);
        for(var i=3;i<source.Length;i+=4)Assert.Equal(source[i],clean[i]);
        Assert.Equal(clean,HighResolutionClosedInk.Prepare(source));
    }
}

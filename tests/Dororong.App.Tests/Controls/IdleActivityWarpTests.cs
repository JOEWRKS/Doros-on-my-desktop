using System.Windows;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class IdleActivityWarpTests
{
    [Fact]
    public void Stretch_reaches_with_leading_paw_without_dragging_planted_foreleg(){
        var warp=new HighResolutionGrabRaster.StretchWarp(HighResolutionFacePose.Neutral with{Stretch=1});
        var arm=new HighResolutionGrabRaster.StompWarp(HighResolutionFacePose.Neutral with{Furious=true,StompReach=16},1,2);
        var leading=arm.ArmPoint(new(23,79));var planted=warp.Map(new(43,85));
        Assert.True(leading.X<15,"Separate leading paw must reach forward");
        Assert.InRange(Math.Abs(planted.X-43),0,.2);
        Assert.True(planted.X-leading.X>25,"Foreleg anchors must remain separate");
    }
    [Theory][InlineData(-1)][InlineData(1)]
    public void Stretch_body_inverse_remains_resolved_during_shake(double shake){
        var warp=new HighResolutionGrabRaster.StretchWarp(HighResolutionFacePose.Neutral with{Stretch=1,StretchShake=shake});
        for(var y=60d;y<=90;y+=.5)for(var x=-5d;x<=65;x+=.5){
            var source=warp.Source(x,y);var mapped=warp.Map(source);
            Assert.InRange(Math.Abs(mapped.X-x)+Math.Abs(mapped.Y-y),0,1e-6);
        }
    }
    [Theory][InlineData(.1)][InlineData(.3)][InlineData(.5)][InlineData(.75)][InlineData(1)]
    public void Stretch_inverse_has_no_fold_or_unresolved_source(double amount){
        var warp=new HighResolutionGrabRaster.StretchWarp(HighResolutionFacePose.Neutral with{Stretch=amount});
        for(var y=45d;y<=92;y+=.5)for(var x=-10d;x<=90;x+=.5){
            var source=warp.Source(x,y);var mapped=warp.Map(source);
            Assert.InRange(Math.Abs(mapped.X-x)+Math.Abs(mapped.Y-y),0,1e-6);
            var a=warp.Map(new(x,y));var b=warp.Map(new(x+.001,y));var c=warp.Map(new(x,y+.001));
            var determinant=((b.X-a.X)*(c.Y-a.Y)-(b.Y-a.Y)*(c.X-a.X))/1e-6;
            Assert.True(determinant>.4,$"Jacobian {determinant} at {x},{y},amount{amount}");
        }
    }
}

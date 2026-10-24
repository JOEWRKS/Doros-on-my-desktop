using System.Windows;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class IdleActivityWarpTests
{
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

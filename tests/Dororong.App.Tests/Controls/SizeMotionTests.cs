using System.Reflection;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public class SizeMotionTests
{
    [Fact]
    public void Raster_scratch_is_reused_across_size_sweeps_without_stale_pixels() => CheekProductTests.Sta(() =>
    {
        var raster=new HighResolutionGrabRaster();
        var field=typeof(HighResolutionGrabRaster).GetField("outputBuffer",BindingFlags.Instance|BindingFlags.NonPublic)!;
        byte[]? first=null;
        foreach(var size in new[]{150,100,125,110,140,150,100})
        {
            var scale=size/100d;var face=HighResolutionFacePose.Neutral;
            var actual=raster.RenderRetainedPixels(0,System.Windows.Media.Matrix.Identity,new(37.5,28.25),default,default,0,scale,face,scale,null);
            first??=(byte[])field.GetValue(raster)!;Assert.Same(first,field.GetValue(raster));
            var fresh=new HighResolutionGrabRaster().RenderRetainedPixels(0,System.Windows.Media.Matrix.Identity,new(37.5,28.25),default,default,0,scale,face,scale,null);
            Assert.Equal(fresh,actual);
            var extent=(int)Math.Ceiling(256*scale);Assert.Equal(extent*extent*4,actual.Length);
        }
    });
    [Fact]
    public void Foot_rebase_does_not_add_walking_distance() => CheekProductTests.Sta(() =>
    {
        var p = new DororongPresenter(); p.SetArtSize(150);
        var pet = new PetSnapshot(PetState.Walk, new(100, 100), FacingDirection.Right, 0, false, null);
        for(var i=0;i<20;i++) p.Render(pet, DirectInteractionSnapshot.None, TimeSpan.FromMilliseconds(16));
        var gait=(LocomotionPresentation)typeof(DororongPresenter).GetField("_locomotion",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(p)!;
        var distance=gait.Distance; var walk=gait.Walk;
        foreach(var size in new[]{100,125,150,130,110})
        {
            p.SetArtSize(size);p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
            var offset=new PointD(8.3,-12.5); p.RebaseSizePresentation(offset);
            pet=pet with { Position=pet.Position+offset };
            p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
            Assert.Equal(distance,gait.Distance);Assert.Equal(walk,gait.Walk);
        }
    });
}

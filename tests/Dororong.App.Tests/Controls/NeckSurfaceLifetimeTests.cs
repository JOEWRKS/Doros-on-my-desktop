using System.Reflection;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class NeckSurfaceLifetimeTests
{
    [Fact]
    public void Neck_buffer_retains_one_native_surface_and_matches_fresh_reference()=>CheekProductTests.Sta(()=>{
        var owned=new HighResolutionHuntNeck.Buffer();
        var field=typeof(HighResolutionHuntNeck.Buffer).GetField("bitmap",BindingFlags.Instance|BindingFlags.NonPublic);
        Assert.NotNull(field);
        var identity=Assert.IsType<RenderTargetBitmap>(field!.GetValue(owned));
        float[]? pixels=null;
        foreach(var frame in Enumerable.Range(0,157).Concat(Enumerable.Range(0,157).Reverse())){
            var face=HighResolutionFacePose.Neutral with{
                BodyFrame=frame,Head=HuntRenderer.HeadTransform(frame,new(.2,-.3,.12*Math.Sin(frame*.1)))};
            var actual=owned.Get(face);
            if(pixels is not null)Assert.Same(pixels,actual);
            pixels=actual;
            Assert.Same(identity,field.GetValue(owned));
            Assert.Equal(HighResolutionHuntNeck.Render(face),actual);
            Assert.Same(actual,owned.Get(face));
        }
    });
}

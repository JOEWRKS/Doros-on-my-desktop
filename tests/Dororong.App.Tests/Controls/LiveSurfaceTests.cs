using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class LiveSurfaceTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Reused_surface_matches_frozen_reference_and_reuses_native_identity(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();var surface=new HighResolutionDisplaySurface();
        BitmapSource? identity=null;byte[]? retained=null;
        for(var i=0;i<20;i++){
            var matrix=Matrix.Identity;matrix.RotateAt((i-10)*3,43,32);
            var face=HighResolutionFacePose.Neutral with{Pull=i%3==0?15:0};
            var expected=raster.Render(i/19d,matrix,new(37.5,28.25),default,default,0,scale,face);
            var before=retained;
            retained=raster.RenderRetainedPixels(i/19d,matrix,new(37.5,28.25),default,default,0,scale,face,scale,retained);
            if(before is not null)Assert.Same(before,retained);
            var actual=surface.Update(retained,scale);
            if(identity is not null)Assert.Same(identity,actual);identity=actual;
            var pixels=new byte[retained.Length];actual.CopyPixels(pixels,actual.PixelWidth*4,0);
            var reference=new byte[pixels.Length];expected.CopyPixels(reference,expected.PixelWidth*4,0);
            Assert.Equal(reference,pixels);Assert.Equal(retained,pixels);
        }
    });
    [Fact]
    public void Mutable_surface_refreshes_hit_test_and_contact_without_stale_alpha()=>CheekProductTests.Sta(()=>{
        var surface=new HighResolutionDisplaySurface();var pixels=new byte[256*256*4];
        var image=new AlphaHitTestImage{Width=256,Height=256,Stretch=Stretch.Fill};
        image.Measure(new(256,256));image.Arrange(new(0,0,256,256));
        var contact=new PlatformContactPresentation();
        pixels[(100*256+80)*4+3]=255;
        image.SetLivePixels(surface.Update(pixels,1),pixels);
        image.Measure(new(256,256));image.Arrange(new(0,0,256,256));image.UpdateLayout();
        Assert.True(image.TryGetOpaqueSourcePoint(new(80,100),out _));
        Assert.Equal(101,contact.MeasureRetainedPixels(pixels,256,256,Transform.Identity).SoleY);
        Array.Clear(pixels);pixels[(120*256+90)*4+3]=255;
        PlatformContactPresentation.InvalidateRetainedPixels(pixels);
        image.SetLivePixels(surface.Update(pixels,1),pixels);
        Assert.False(image.TryGetOpaqueSourcePoint(new(80,100),out _));
        Assert.True(image.TryGetOpaqueSourcePoint(new(90,120),out _));
        Assert.Equal(121,contact.MeasureRetainedPixels(pixels,256,256,Transform.Identity).SoleY);
    });
}

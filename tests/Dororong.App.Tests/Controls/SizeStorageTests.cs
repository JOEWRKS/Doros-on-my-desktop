using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;

namespace Dororong.App.Tests.Controls;

public class SizeStorageTests
{
    [Fact]
    public void Retained_capacity_preserves_pixels_and_ignores_unused_tail() => CheekProductTests.Sta(() =>
    {
        var raster=new HighResolutionGrabRaster();var surface=new HighResolutionDisplaySurface();
        var retained=new byte[384*384*4];Array.Fill(retained,(byte)255);
        var image=new AlphaHitTestImage {Stretch=Stretch.Fill};
        foreach(var size in new[]{150,100,110,125,140,150,100})
        {
            var scale=size/100d;var face=HighResolutionFacePose.Neutral with{EyeY=-.3,Pull=7};
            var actual=raster.RenderRetainedPixels(.5,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face,scale,retained,true);
            Assert.Same(retained,actual);
            var expected=raster.RenderRetainedPixels(.5,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face,scale,null);
            var bitmap=surface.Update(actual,scale);
            var pixels=new byte[bitmap.PixelWidth*bitmap.PixelHeight*4];bitmap.CopyPixels(pixels,bitmap.PixelWidth*4,0);
            Assert.Equal(expected,pixels);
            image.Width=image.Height=256*scale;
            image.SetLivePixels(bitmap,actual);image.Measure(new(image.Width,image.Height));
            image.Arrange(new(0,0,image.Width,image.Height));image.UpdateLayout();
            Assert.False(image.TryGetOpaqueSourcePoint(new(image.Width+1,image.Height+1),out _));
            var contact=new PlatformContactPresentation();
            PlatformContactPresentation.InvalidateRetainedPixels(actual);
            Assert.Equal(contact.MeasurePixels(expected,bitmap.PixelWidth,bitmap.PixelHeight,Transform.Identity),
                contact.MeasureRetainedPixels(actual,bitmap.PixelWidth,bitmap.PixelHeight,Transform.Identity));
        }
        Assert.Throws<ArgumentException>(()=>surface.Update(new byte[1],1));
        Assert.Throws<ArgumentException>(()=>image.SetLivePixels(surface.Update(retained,1),new byte[1]));
    });

    [Fact]
    public void Size_change_reuses_frames_and_active_plus_spares_stay_bounded() => CheekProductTests.Sta(() =>
    {
        var p=new DororongPresenter();p.SetArtSize(150);
        var pet=new PetSnapshot(PetState.Idle,new(0,0),FacingDirection.Right,0,false,null);
        object Field(string name)=>typeof(DororongPresenter).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(p)!;
        void Draw(DirectInteractionSnapshot direct)
        {
            p.Render(pet,direct,TimeSpan.Zero);
            p.Measure(new(p.Width,p.Height));p.Arrange(new(0,0,p.Width,p.Height));p.UpdateLayout();
        }
        Draw(DirectInteractionSnapshot.None);
        var first=Field("_hrDisplayedPixels");
        foreach(var size in Enumerable.Range(100,51).Concat(Enumerable.Range(100,51).Reverse()))
        {
            p.SetArtSize(size);Draw(DirectInteractionSnapshot.None);
            Assert.Same(first,Field("_hrDisplayedPixels"));
        }
        for(var i=0;i<120;i++)
        {
            if(i%7==0)p.SetArtSize(100+i%51);
            Draw(new(DirectInteractionTarget.Body,DirectInteractionPhase.BodyDragHold,new(60,50),new(60,50),1,0,true){HeadSwingDegrees=i%60});
            var active=(IDictionary)Field("_hrFrames");
            var spare=(ICollection)Field("_hrSpareFrames");
            Assert.InRange(active.Count+spare.Count,1,12);
            var buffers=active.Values.Cast<byte[]>().Concat(((IEnumerable)spare).Cast<byte[]>()).ToArray();
            Assert.Equal(buffers.Length,buffers.Distinct(ReferenceEqualityComparer.Instance).Count());
        }
    });
}

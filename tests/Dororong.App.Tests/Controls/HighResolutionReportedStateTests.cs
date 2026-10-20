using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionReportedStateTests
{
    [Fact]
    public void Cheek_mouth_vacated_ink_is_not_protected_as_bangs(){
        var pose=HighResolutionFacePose.Neutral with{Pull=20};
        foreach(var p in new[]{new Point(29.8,58.5),new Point(30.2,58.5),new Point(30.6,58.7)}){
            var source=pose.Source(p,out var donor);
            Assert.True(donor);Assert.Equal(35,source.X);Assert.Equal(p.Y,source.Y);
        }
        // Original mouth ink is moved left one/down two, not redrawn.
        foreach(var source in new[]{new Point(26,59),new Point(29.5,59),new Point(33,60)}){
            Assert.Equal(source,pose.Source(new(source.X-1,source.Y+2)));
        }
    }
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Upward_tracking_has_one_translated_lower_rim_not_a_stationary_duplicate(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        foreach(var dx in new[]{-.85,0,.85}){
            var b=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,HighResolutionFacePose.Neutral with{EyeX=dx,EyeY=-.65});
            var pixels=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(pixels,b.PixelWidth*4,0);
            var shift=Matrix.Identity;shift.Translate(dx,-.65);
            var reference=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,HighResolutionFacePose.Neutral with{Head=shift});
            var baseline=new byte[pixels.Length];reference.CopyPixels(baseline,reference.PixelWidth*4,0);
            // Both the MOVED ink and the VACATED ink must match the original
            // translated eye. r66 compared to stationary colour and forbade
            // legitimate moving ink, contradicting the user's whole-eye motion.
            foreach(var p in new[]{new Point(22.3,55),new Point(39.3,59)}){
                var x=(int)((p.X+80)*scale);var y=(int)((p.Y+80)*scale);
                var at=(y*b.PixelWidth+x)*4+1;
                Assert.True(Math.Abs(pixels[at]-baseline[at])<=10,$"Lower rim {scale} {dx} {p}: G={pixels[at]}, translated original={baseline[at]}");
            }
            // Skin remains at the face location, NOT translated with the eye.
            // Its original pink gradient is legitimately different from a
            // translated whole head. Require clean skin here, not moved skin.
            foreach(var p in new[]{new Point(22.3,56.5),new Point(39.3,60.5)}){
                var at=((int)((p.Y+80)*scale)*b.PixelWidth+(int)((p.X+80)*scale))*4+1;
                Assert.True(pixels[at]>=200,$"Residual rim in vacated skin at {scale} {dx} {p}: G={pixels[at]}");
            }
        }
    });
    [Fact]
    public void Cheek_pull_keeps_near_eye_and_diagonal_bang_boundary_attached(){
        foreach(var pull in new[]{.01,1,4,10,20})foreach(var gaze in new[]{-.85,0,.85}){
            var pose=HighResolutionFacePose.Neutral with{Pull=pull,EyeX=gaze,EyeY=.65};
            foreach(var p in new[]{new Point(22,52),new Point(24,54),new Point(25,50),new Point(27,51)}){
                var source=pose.Source(p,out var donor);Assert.Equal(p,source);Assert.False(donor);
            }
        }
    }
    [Fact]
    public void Capture_facing_returns_to_idle_pointer_owner_without_waiting_for_walk()=>CheekProductTests.Sta(()=>{
        var p=new DororongPresenter{HighResolutionGrabEnabled=true,HighResolutionScale=1.5,Width=216,Height=216};
        ((Canvas)p.Content).Width=216;((Canvas)p.Content).Height=216;
        var pet=new PetSnapshot(PetState.Idle,new(100,100),FacingDirection.Right,0,false,null);
        p.Render(pet,DirectInteractionSnapshot.None,TimeSpan.Zero);
        p.Measure(new(216,216));p.Arrange(new(0,0,216,216));p.UpdateLayout();
        double Side()=>p.HighResolutionSourceToPresenter(new(80,48)).X-p.HighResolutionSourceToPresenter(new(20,48)).X;
        var initial=Math.Sign(Side());var image=p.HighResolutionImage!;
        var at=p.HighResolutionSourceToPresenter(new(37.5,28.25));
        var press=p.CreateDirectPress(image,p.TranslatePoint(at,image));Assert.NotNull(press);Assert.Equal(DirectInteractionTarget.Body,press!.Target);
        p.SetLocomotionBlocked(true); // Airborne/landing retains captured facing.
        p.Render(pet with{Facing=FacingDirection.Left},DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
        Assert.Equal(initial,Math.Sign(Side()));
        p.SetLocomotionBlocked(false);
        p.Render(pet with{Facing=FacingDirection.Left},DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
        Assert.Equal(-initial,Math.Sign(Side()));
        p.Render(pet with{State=PetState.Walk,Facing=FacingDirection.Left},DirectInteractionSnapshot.None,TimeSpan.FromMilliseconds(16));
        Assert.Equal(-initial,Math.Sign(Side()));
    });
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Gaze_does_not_paint_stretched_skin_or_mouth_cleanup(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(HighResolutionFacePose face){var b=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face);var data=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(data,b.PixelWidth*4,0);return data;}
        foreach(var pull in new[]{4d,10,20}){
            var face=HighResolutionFacePose.Neutral with{Pull=pull,EyeFollow=10,HairFollow=10};var baseline=Draw(face);var width=(int)Math.Ceiling(256*scale);
            foreach(var dx in new[]{-.85,.85}){
                var changed=Draw(face with{EyeX=dx,EyeY=.65});var compared=0;
                for(var y=0;y<width;y++)for(var x=0;x<width;x++){
                    var qx=(x+.5)/scale-80;var qy=(y+.5)/scale-80;
                    // Independent skin regions where the reported extra eye
                    // sliver and horizontal mouth-donor line appeared.
                    if((qx>=3&&qx<=13&&qy>=54&&qy<=61)||(qx>=30&&qx<=32&&qy>=57.5&&qy<=58.5)){
                        var at=(y*width+x)*4;Assert.Equal(baseline.AsSpan(at,4).ToArray(),changed.AsSpan(at,4).ToArray());compared++;
                    }
                }
                Assert.True(compared>=60);
            }
        }
    });
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void Gaze_is_continuous_at_zero_including_occluded_top(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(double dy,double pull){var b=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,HighResolutionFacePose.Neutral with{EyeY=dy,Pull=pull,EyeFollow=pull>0?10:0,HairFollow=pull>0?10:0});var data=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(data,b.PixelWidth*4,0);return data;}
        foreach(var pull in new[]{0d,20})foreach(var dy in new[]{-.00001,.00001}){
            var neutral=Draw(0,pull);var frame=Draw(dy,pull);var difference=neutral.Zip(frame,(a,b)=>Math.Abs(a-b)).ToArray();var largest=difference.Max();var at=Array.IndexOf(difference,largest)/4;var width=(int)Math.Ceiling(256*scale);
            Assert.True(largest<=1,$"Zero discontinuity scale={scale}, pull={pull}, dy={dy}, delta={largest}, at=({(at%width+.5)/scale-80},{(at/width+.5)/scale-80})");
        }
    });
    [Fact]
    public void Diagnose_cheek_gaze_difference_bounds()=>CheekProductTests.Sta(()=>{
        var dir=Environment.GetEnvironmentVariable("DORORONG_R62_OUTPUT");if(dir is null)return;
        Directory.CreateDirectory(dir);var raster=new HighResolutionGrabRaster();
        foreach(var pull in new[]{-2d,10,20})foreach(var gaze in new[]{0d,-.85,.85}){
            var pose=HighResolutionFacePose.Neutral with{Pull=pull,EyeFollow=pull>0?10:0,HairFollow=pull>0?10:0,EyeX=gaze,EyeY=gaze==0?0:.65};
            var b=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,1.5,pose);
            using var stream=File.Create(Path.Combine(dir,$"cheek-{pull}-{gaze}.png"));var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(b));encoder.Save(stream);
        }
    });
    [Fact]
    public void Downward_gaze_does_not_replace_upper_eye_with_white_skin()=>CheekProductTests.Sta(()=>{
        const double scale=1.5;
        var b=new HighResolutionGrabRaster().Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,HighResolutionFacePose.Neutral with{EyeY=.65});
        var pixels=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(pixels,b.PixelWidth*4,0);
        // Independently identified points in the reported slit, one per eye.
        // r61 produced RGB(222,213,221) and (182,182,185) here. These points
        // belong to the purple/dark eye, not white facial cleanup or highlight.
        foreach(var p in new[]{new Point(25,50.5),new Point(40,52.5)}){
            var x=(int)((p.X+80)*scale);var y=(int)((p.Y+80)*scale);var at=(y*b.PixelWidth+x)*4;
            // A moving lavender highlight can legitimately be brighter than
            // G160. White skin has no purple chroma (r61: 222/213/221 and
            // 182/182/185). Check material separation, not fixed brightness.
            Assert.True(pixels[at+1]<215&&(pixels[at]-pixels[at+2]>=10||pixels[at+2]-pixels[at+1]>=10),
                $"White gap at {p}: RGB={pixels[at+2]},{pixels[at+1]},{pixels[at]}");
            Assert.Equal(255,pixels[at+3]);
        }
    });
}

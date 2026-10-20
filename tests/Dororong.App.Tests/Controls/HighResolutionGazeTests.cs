using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class HighResolutionGazeTests
{
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Tracking_preserves_stationary_bangs_not_the_moving_eye_rim(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(double x,double y){var b=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,HighResolutionFacePose.Neutral with{EyeX=x,EyeY=y});var p=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(p,b.PixelWidth*4,0);return p;}
        var neutral=Draw(0,0);var width=(int)Math.Ceiling(256*scale);
        // Independent source landmarks, not generated from the compositor mask.
        // r65/r66 incorrectly froze the eye outline too. The user explicitly
        // requires the entire visible eye to move; only HAIR stays fixed.
        var rim=new[]{new Point(21,46),new Point(15,52),new Point(30.8,52.5),new Point(26,48),new Point(39,49)};
        for(var d=0;d<8;d++){
            var p=Draw(.85*Math.Cos(d*Math.PI/4),.65*Math.Sin(d*Math.PI/4));
            foreach(var q in rim){var at=((int)((q.Y+80)*scale)*width+(int)((q.X+80)*scale))*4;Assert.Equal(neutral.AsSpan(at,4).ToArray(),p.AsSpan(at,4).ToArray());}
        }
    });
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Whole_eye_lower_outline_follows_translated_original_not_a_stationary_rim(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(HighResolutionFacePose face){var b=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face);var a=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(a,b.PixelWidth*4,0);return a;}
        var neutral=Draw(HighResolutionFacePose.Neutral);var width=(int)Math.Ceiling(256*scale);
        foreach(var (dx,dy) in new[]{(.85,0d),(-.85,0d),(0d,.65),(0d,-.65)}){
            var actual=Draw(HighResolutionFacePose.Neutral with{EyeX=dx,EyeY=dy});
            var matrix=Matrix.Identity;matrix.Translate(dx,dy);
            var rigid=Draw(HighResolutionFacePose.Neutral with{Head=matrix});
            foreach(var eye in new[]{new Rect(20,54,5,3.8),new Rect(37,58.4,4,3.4)}){
                double error=0,stationaryError=0;var count=0;
                for(var y=0;y<width;y++)for(var x=0;x<width;x++){
                    var q=new Point((x+.5)/scale-80,(y+.5)/scale-80);if(!eye.Contains(q))continue;
                    var at=(y*width+x)*4;
                    for(var c=0;c<3;c++){error+=Math.Abs(actual[at+c]-rigid[at+c]);stationaryError+=Math.Abs(neutral[at+c]-rigid[at+c]);count++;}
                }
                Assert.True(stationaryError/count>5,"Reference must include observable outline movement");
                Assert.True(error/count<8&&error<stationaryError*.3,$"Outline {scale} {eye} ({dx},{dy}): mean error {error/count}, stationary {stationaryError/count}");
            }
        }
    });
    [Theory]
    [InlineData(1)] [InlineData(1.25)] [InlineData(1.5)]
    public void Gaze_eye_texture_matches_a_rigidly_translated_original_head(double scale)=>CheekProductTests.Sta(()=>{
        var raster=new HighResolutionGrabRaster();
        byte[] Draw(HighResolutionFacePose face){var b=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,face);var a=new byte[b.PixelWidth*b.PixelHeight*4];b.CopyPixels(a,b.PixelWidth*4,0);return a;}
        var neutral=Draw(HighResolutionFacePose.Neutral);var width=(int)Math.Ceiling(256*scale);
        foreach(var (dx,dy) in new[]{(.85,0d),(-.85,0d),(0d,.65),(0d,-.65)}){
            var actual=Draw(HighResolutionFacePose.Neutral with{EyeX=dx,EyeY=dy});
            var matrix=Matrix.Identity;matrix.Translate(dx,dy);
            var rigid=Draw(HighResolutionFacePose.Neutral with{Head=matrix});var checkedPixels=0;var movedPixels=0;
            for(var y=0;y<width;y++)for(var x=0;x<width;x++){
                var qx=(x+.5)/scale-80;var qy=(y+.5)/scale-80;var at=(y*width+x)*4;
                // Independent interior feature windows: these must reproduce
                // an actual translated ORIGINAL, not merely change some pixels.
                // Keep the entire shifted sampling footprint inside colour.
                // The old left x25/y54 window sampled the ink rim on upward
                // gaze: requiring its translation reproduced the reported line.
                if((qx>=22&&qx<=24&&qy>=51.5&&qy<=53)||(qx>=38&&qx<=40&&qy>=55&&qy<=57)){
                    for(var c=0;c<4;c++)Assert.True(Math.Abs(actual[at+c]-rigid[at+c])<=1,$"Interior {scale} ({qx},{qy}) gaze({dx},{dy}) channel{c}: {actual[at+c]} != {rigid[at+c]}");
                    checkedPixels++;if(!actual.AsSpan(at,4).SequenceEqual(neutral.AsSpan(at,4)))movedPixels++;
                }
                // Independent hair/mouth/cheek/body landmarks away from the
                // moving eye footprint, not generated from the implementation mask.
                // Include the entire destination pixel footprint. At 1.25x,
                // a pixel centered on x30 also samples x29.8 inside the eye.
                if(qx>=45||qy>=63||(qx>=30+.5/scale&&qx<=32)||qy<=46)
                    Assert.True(actual.AsSpan(at,4).SequenceEqual(neutral.AsSpan(at,4)),$"protected {scale} ({qx},{qy}) gaze({dx},{dy})");
            }
            Assert.True(checkedPixels>=8);Assert.True(movedPixels>=4);
        }
    });
    [Fact]
    public void Gaze_keeps_hair_cheek_and_all_non_eye_coordinates_fixed(){
        foreach(var dx in new[]{-.85,0,.85})foreach(var dy in new[]{-.65,0,.65}){
            var pose=HighResolutionFacePose.Neutral with{EyeX=dx,EyeY=dy};
            for(var y=35d;y<70;y+=.125)for(var x=10d;x<52;x+=.125){
                var q=new Point(x,y);var mapped=pose.Source(q);
                Assert.Equal(q,mapped); // Gaze must not warp any source geometry.
                var moved=HighResolutionGrabRaster.GazeSource(q,dx,dy);
                Assert.Equal(dx,q.X-moved.X,12);Assert.Equal(dy,q.Y-moved.Y,12);
                // Independent landmarks: side hair, central bangs and blush.
                if(x>=43||y>=61||(x>=29&&x<=35)||y<=47)Assert.Equal(q,mapped);
            }
        }
    }
    [Theory]
    [InlineData(1)] [InlineData(1.1)] [InlineData(1.25)] [InlineData(1.35)] [InlineData(1.5)]
    public void Gaze_frames_and_protected_reactions(double scale)=>CheekProductTests.Sta(()=>{
        var output=Environment.GetEnvironmentVariable("DORORONG_R60_OUTPUT");
        if(output is not null)Directory.CreateDirectory(output);
        var raster=new HighResolutionGrabRaster();
        byte[] Save(string name,HighResolutionFacePose pose){
            var image=raster.Render(0,Matrix.Identity,new(37.5,28.25),default,default,0,scale,pose);
            var pixels=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(pixels,image.PixelWidth*4,0);
            if(output is not null){using var stream=File.Create(Path.Combine(output,$"gaze-{scale:F2}-{name}.png"));var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(image));enc.Save(stream);}
            return pixels;
        }
        var neutral=Save("neutral",HighResolutionFacePose.Neutral);
        foreach(var pull in new[]{-2d,0,10,20})Save($"protected-cheek-{pull}",HighResolutionFacePose.Neutral with{Pull=pull,EyeFollow=pull>0?10:0,HairFollow=pull>0?10:0});
        for(var direction=0;direction<8;direction++)for(var step=0;step<=8;step++){
            // Zero -> directional maximum -> zero; the transform stays fixed
            // so a changed hair pixel cannot hide behind whole-head motion.
            var amount=1-Math.Abs(step-4)/4d;var angle=direction*Math.PI/4;
            var pose=HighResolutionFacePose.Neutral with{EyeX=.85*Math.Cos(angle)*amount,EyeY=.65*Math.Sin(angle)*amount};
            var pixels=Save($"d{direction}-s{step}",pose);
            if(step is 0 or 8)Assert.Equal(neutral,pixels);
            else Assert.NotEqual(neutral,pixels);
        }
    });
}

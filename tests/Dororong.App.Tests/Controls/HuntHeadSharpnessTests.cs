using Dororong.App.Controls;
using System.Windows.Media.Imaging;

namespace Dororong.App.Tests.Controls;

public sealed class HuntHeadSharpnessTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Neutral_head_keeps_authored_pixels_before_pose_sampling(bool closed) => CheekProductTests.Sta(() =>
    {
        var source=HuntFrames.Instance.Head(closed?1:0).ToArray();
        var painted=new HuntEyes().Paint(0,0,closed);
        var pixels=Pixels(painted);
        // An untransformed head must not acquire softened colors before its
        // actual pose transform. Pixel centers of every source cell stay exact.
        int changed=0;
        for(int y=0;y<96;y++)for(int x=0;x<96;x++)
        {
            int a=(y*96+x)*4,b=((y*4+2)*384+x*4+2)*4;
            for(int c=0;c<3;c++)if(pixels[b+c]!=(source[a+2-c]*source[a+3]+127)/255)changed++;
            if(pixels[b+3]!=source[a+3])changed++;
        }
        Assert.True(changed==0,$"{changed} neutral head channels were prefiltered before the pose transform");
    });

    [Theory]
    [InlineData(0)]
    [InlineData(156)]
    public void Upright_tracking_preserves_opaque_face_ink_at_zero_gaze(int frame) => CheekProductTests.Sta(() =>
    {
        var source=HuntFrames.Instance.Head(0).ToArray();
        var actual=Pixels(new HuntRenderer().Render(frame,default));
        double error=0;int count=0;
        for(int y=28;y<58;y++)for(int x=20;x<54;x++)
        {
            int at=(y*96+x)*4;
            if(source[at+3]!=255)continue;
            for(int c=0;c<3;c++){error+=Math.Abs(actual[at+c]-source[at+2-c]);count++;}
        }
        Assert.True(error/count<2,$"Neutral face mean channel error {error/count:F3}; tracking must not soften resting ink");
    });
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Half_pixel_eye_motion_moves_ink_without_leaving_the_old_sample(bool closed) => CheekProductTests.Sta(() =>
    {
        var source=HuntFrames.Instance.Head(closed?1:0).ToArray();
        var actual=Pixels(new HuntEyes().Paint(.5,.5,closed));
        // Inside the full-strength left-eye field, half a native pixel is
        // exactly two working-grid cells. No old-color residual belongs here.
        int changed=0;
        for(int y=47*4;y<55*4;y++)for(int x=18*4;x<24*4;x++)
        {
            int from=(((y-2)/4)*96+(x-2)/4)*4,at=(y*384+x)*4;
            Assert.Equal(255,source[from+3]);
            for(int c=0;c<3;c++)if(actual[at+c]!=source[from+2-c])changed++;
        }
        Assert.True(changed==0,$"{changed} displaced eye channels retain mismatched filtering");
    });

    [Theory]
    [InlineData(false,5,0)]
    [InlineData(false,20,0)]
    [InlineData(true,20,0)]
    [InlineData(false,-2,0)]
    [InlineData(false,20,20)]
    public void Cheek_deformation_replaces_old_ink_and_coverage_without_residue(bool closed,double pull,double follow) => CheekProductTests.Sta(() =>
    {
        var original=HuntFrames.Instance.Head(closed?1:0).ToArray();
        for(int i=0;i<original.Length;i+=4)(original[i],original[i+2])=(original[i+2],original[i]);
        var deformed=OutlineCheekRenderer.Render(original,pull,follow,follow);
        var actual=Pixels(new HuntEyes().Paint(0,0,closed,pull,follow,follow));
        int changed=0;
        for(int y=0;y<96;y++)for(int x=0;x<96;x++)
        {
            int from=(y*96+x)*4,at=((y*4+2)*384+x*4+2)*4;
            for(int c=0;c<3;c++)if(actual[at+c]!=(deformed[from+c]*deformed[from+3]+127)/255)changed++;
            if(actual[at+3]!=deformed[from+3])changed++;
        }
        Assert.True(changed==0,$"{changed} cheek channels differ from the approved deformed head");
    });

    static byte[] Pixels(BitmapSource image){var p=new byte[image.PixelWidth*image.PixelHeight*4];image.CopyPixels(p,image.PixelWidth*4,0);return p;}
}

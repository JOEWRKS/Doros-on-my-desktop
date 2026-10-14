using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class HuntHeadSamplingTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void Unrotated_head_is_exact_original_not_a_prefiltered_copy(bool closed)=>CheekProductTests.Sta(()=>
    {
        var rgba=HuntFrames.Instance.Head(closed?1:0).ToArray();
        var rendered=new HuntHeadSampling().Render(new HuntEyes().Paint(0,0,closed),Matrix.Identity);
        var actual=Pixels(rendered);
        for(var at=0;at<rgba.Length;at+=4)
        {
            Assert.Equal(rgba[at+3],actual[at+3]);
            for(var c=0;c<3;c++)Assert.Equal((rgba[at+2-c]*rgba[at+3]+127)/255,actual[at+c]);
        }
        Assert.True(rendered.IsFrozen);
    });

    [Fact]
    public void Slow_rotation_reduces_point_sampling_shimmer_without_losing_thin_ink()=>CheekProductTests.Sta(()=>
    {
        var sampler=new HuntHeadSampling();var eyes=new HuntEyes();
        var current=new List<byte[]>();var legacy=new List<byte[]>();
        for(var n=0;n<=80;n++)
        {
            var gaze=new HuntGazePose(.4,.3,-.1+n*.0025);
            var head=eyes.Paint(gaze.EyeX,gaze.EyeY,false);
            var pose=HuntRenderer.HeadTransform(156,gaze);
            current.Add(Pixels(sampler.Render(head,pose)));
            legacy.Add(Pixels(WpfPose(head,pose)));
        }
        var before=Curvature(legacy);var after=Curvature(current);
        Assert.True(after<before*.65,$"Temporal curvature before={before:F3}, after={after:F3}");
        // Alias contrast is not a visual sharpness score. Bound the retained
        // ink energy separately so a blurred sequence cannot pass only by being
        // temporally flat. Also compare to ordinary native bilinear below.
        var oldDetail=legacy.Average(Detail);var detail=current.Average(Detail);
        Assert.True(detail>oldDetail*.70,$"Ink energy before={oldDetail:F1}, after={detail:F1}");
    });

    [Theory]
    [InlineData(-.3)] [InlineData(-.1)] [InlineData(.1)] [InlineData(.3)]
    public void Single_cubic_pose_keeps_more_line_contrast_than_native_bilinear(double roll)=>CheekProductTests.Sta(()=>
    {
        var head=new HuntEyes().Paint(0,0,false);var sampler=new HuntHeadSampling();
        var native=sampler.Render(head,Matrix.Identity);
        var transform=HuntRenderer.HeadTransform(156,new(0,0,roll));
        var current=sampler.Render(head,transform);var repeated=sampler.Render(head,transform);
        Assert.Equal(Pixels(current),Pixels(repeated));
        var detail=Detail(Pixels(current));var linear=Detail(Pixels(WpfPose(native,transform)));
        Assert.True(detail>=linear*1.05,$"Cubic={detail:F1}; native bilinear={linear:F1}");
        var pixels=Pixels(current);
        for(var at=0;at<pixels.Length;at+=4)for(var c=0;c<3;c++)Assert.True(pixels[at+c]<=pixels[at+3]);
    });

    [Fact]
    public void Rotation_does_not_repaint_body_feet_or_change_rest_source()=>CheekProductTests.Sta(()=>
    {
        var renderer=new HuntRenderer();var source=UprightRumpSource.Standing(false);var original=Pixels(source);
        foreach(var roll in new[]{-.3,-.15,0,.15,.3})
        {
            var gaze=new HuntGazePose(.4,.3,roll);
            var pixels=Pixels(renderer.Render(156,gaze));
            var head=Pixels(new HuntHeadSampling().Render(new HuntEyes().Paint(.4,.3,false),HuntRenderer.HeadTransform(156,gaze)));
            var checkedPixels=0;
            for(var at=76*384;at<pixels.Length;at+=4)
            {
                // A tilted hair tip may legitimately overlap this row. The
                // body outside the transformed head footprint must stay exact.
                if(head[at+3]!=0)continue;
                Assert.Equal(original.AsSpan(at,4).ToArray(),pixels.AsSpan(at,4).ToArray());checkedPixels++;
            }
            Assert.True(checkedPixels>1500);
        }
        Assert.Same(source,renderer.Render(156,default));
    });

    private static BitmapSource WpfPose(BitmapSource head,Matrix pose)
    {
        var visual=new DrawingVisual();RenderOptions.SetBitmapScalingMode(visual,BitmapScalingMode.Linear);
        using(var dc=visual.RenderOpen()){dc.PushTransform(new MatrixTransform(pose));dc.DrawImage(head,new Rect(0,0,96,96));dc.Pop();}
        var bitmap=new RenderTargetBitmap(96,96,96,96,PixelFormats.Pbgra32);bitmap.Render(visual);bitmap.Freeze();return bitmap;
    }
    private static double Curvature(IReadOnlyList<byte[]> frames)
    {
        double total=0;long count=0;
        for(var n=2;n<frames.Count;n++)for(var y=16;y<62;y++)for(var x=10;x<80;x++)for(var c=0;c<3;c++)
        {var at=(y*96+x)*4+c;total+=Math.Abs(frames[n][at]-2*frames[n-1][at]+frames[n-2][at]);count++;}
        return total/count;
    }
    private static double Detail(byte[] pixels)
    {
        double total=0;long count=0;
        for(var y=16;y<62;y++)for(var x=10;x<80;x++)for(var c=0;c<3;c++)
        {var at=(y*96+x)*4+c;total+=Math.Pow(pixels[at+4]-pixels[at],2)+Math.Pow(pixels[at+384]-pixels[at],2);count++;}
        return total/count;
    }
    private static byte[] Pixels(BitmapSource source)=>PremultipliedFrame.From(source).Pixels;
}

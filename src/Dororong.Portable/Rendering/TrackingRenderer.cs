using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

// Port of the amount==0 branch in the accepted HuntRenderer. Only the hidden
// neck joins use Windows-derived coverage; visible body/head texels are sampled once.
public sealed class TrackingRenderer
{
    private readonly HuntEyes eyes = new();
    private readonly HuntHeadSampling sampling = new();
    public static Affine HeadTransform(HuntGazePose gaze)
    {
        var cos = Math.Cos(gaze.Roll); var sin = Math.Sin(gaze.Roll);
        return new(cos, sin, -sin, cos, 43 - 43 * cos + 66 * sin, 66 - 43 * sin - 66 * cos);
    }
    public RasterImage Render(HuntGazePose gaze, bool closed = false, double breathing = 0, RasterImage? walking = null,double cheekPull=0,double eyePull=0,double hairPull=0)
    {
        if (!double.IsFinite(gaze.EyeX) || !double.IsFinite(gaze.EyeY) || !double.IsFinite(gaze.Roll) || !double.IsFinite(breathing))
            throw new ArgumentOutOfRangeException(nameof(gaze));
        if (gaze == default && walking is null && cheekPull==0) return BodyBreathingFrame.Standing(closed, breathing);
        var transform = HeadTransform(gaze);
        var body = SnackRunFrames.CopyBody(); var mask = HuntFrames.Instance.Head(closed ? 1 : 0).Span;
        var inkPixels = HuntJoinBank.Instance.Render(0,gaze.Roll);
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)
        {var i=(y*96+x)*4;if(mask[i+3]>0)inkPixels.AsSpan(((y+16)*128+x+16)*4,4).CopyTo(body.AsSpan(i,4));}
        if (walking is not null)
        {
            if (walking.PixelWidth != 96 || walking.PixelHeight != 96) throw new ArgumentException("Expected native gait.");
            walking.Pixels.Span.Slice(73*384).CopyTo(body.AsSpan(73*384));
        }
        else body=BodyBreathingFrame.Warp(body,Math.Round(Math.Clamp(breathing,0,1)*32)/32d);
        var head=sampling.Render(eyes.Paint(gaze.EyeX,gaze.EyeY,closed,cheekPull,eyePull,hairPull),transform);
        RasterImage.Over(body,head.Pixels.Span);
        return new(96,96,body);
    }
}

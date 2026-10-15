using Dororong.Core.Geometry;
using SkiaSharp;

namespace Dororong.Portable.Rendering;

// Articulated branch of Windows HuntRenderer. Windows-derived join coverage,
// continuous head sampling, and identical native reduction/outline ownership.
internal sealed class HuntingRenderer
{
    private readonly HuntEyes eyes=new();
    private readonly TrackingRenderer upright=new();
    internal static Affine HeadTransform(int frame,HuntGazePose gaze)
    {
        var pose=HuntPose.At(frame/60d);var angle=gaze.Roll<0?gaze.Roll*(1-pose.Amount)+Math.Max(gaze.Roll,-.08)*pose.Amount:gaze.Roll;
        PointD H(double x,double y)=>pose.Head(43+(x-43)*Math.Cos(angle)-(y-66)*Math.Sin(angle),66+(x-43)*Math.Sin(angle)+(y-66)*Math.Cos(angle));
        var o=H(0,0);var x=H(1,0);var y=H(0,1);return new(x.X-o.X,x.Y-o.Y,y.X-o.X,y.Y-o.Y,o.X,o.Y);
    }
    internal RasterImage Render(int frame,HuntGazePose gaze,bool closed=false,double breathing=0,RasterImage? walking=null,double cheekPull=0,double eyePull=0,double hairPull=0)
    {
        var pose=HuntPose.At(frame/60d);var amount=pose.Amount;
        if(amount==0)return upright.Render(gaze,closed,breathing,walking,cheekPull,eyePull,hairPull);
        var jp=HuntJoinBank.Instance.Render(frame,gaze.Roll);
        using var work=new SKBitmap(256,256,SKColorType.Bgra8888,SKAlphaType.Premul);
        using(var canvas=new SKCanvas(work))
        using(var joinImage=new RasterImage(128,128,jp).ToBitmap())
        using(var body=HuntFrames.Instance.Body(frame).ToBitmap())
        using(var paint=new SKPaint{FilterQuality=SKFilterQuality.Low})
        {canvas.Clear(SKColors.Transparent);canvas.DrawBitmap(joinImage,new SKRect(0,0,256,256),paint);canvas.DrawBitmap(body,0,0,paint);}
        var pixels=RasterImage.FromBitmap(work).CopyPixels();UprightRumpSource.Apply(pixels,amount,jp);
        var t=HeadTransform(frame,gaze);
        var posedHead=LinearSampling.Render(eyes.Paint(gaze.EyeX,gaze.EyeY,closed,cheekPull,eyePull,hairPull),
            new(t.M11*.5,t.M12*.5,t.M21*.5,t.M22*.5,(t.OffsetX+16)*2,(t.OffsetY+16)*2),256,256);
        RasterImage.Over(pixels,posedHead.Pixels.Span);HuntOutline.Finish(pixels,amount);
        var native=new byte[36864];
        // RenderTargetBitmap's observed 2:1 reduction averages the working
        // texel footprint even with its nearest hint. Selecting either corner
        // shifts ink by half a native pixel. Match that reduction, no extra blur.
        for(var y=0;y<96;y++)for(var x=0;x<96;x++)for(var ch=0;ch<4;ch++)
        {var i=((y*2+32)*256+x*2+32)*4+ch;native[(y*96+x)*4+ch]=(byte)((pixels[i]+pixels[i+4]+pixels[i+1024]+pixels[i+1028]+2)/4);}
        var result=new RasterImage(96,96,native);
        if(amount<.08)
        {
            // The same transformed head on both sides of the ownership bridge.
            var head=new HuntHeadSampling().Render(eyes.Paint(gaze.EyeX,gaze.EyeY,closed,cheekPull,eyePull,hairPull),HeadTransform(frame,gaze));
            var basePixels=SnackRunFrames.CopyBody();
            var mask=HuntFrames.Instance.Head(closed?1:0).Span;
            for(var i=0;i<basePixels.Length;i+=4)if(mask[i+3]>0)Array.Copy(jp,(((i/4/96)+16)*128+i/4%96+16)*4,basePixels,i,4);
            if(walking is not null)walking.Pixels.Span.Slice(73*384).CopyTo(basePixels.AsSpan(73*384));
            else basePixels=BodyBreathingFrame.Warp(basePixels,Math.Round(Math.Clamp(breathing,0,1)*32)/32d);
            RasterImage.Over(basePixels,head.Pixels.Span);var entry=amount/.08;
            result=Mix(new(96,96,basePixels),result,entry*entry*(3-2*entry));
        }
        return result;
    }
    internal static RasterImage Mix(RasterImage a,RasterImage b,double t)
    {if(t<=0)return a;if(t>=1)return b;var data=a.CopyPixels();var other=b.Pixels.Span;for(var i=0;i<data.Length;i++)data[i]=(byte)Math.Round(data[i]+(other[i]-data[i])*t);return new(a.PixelWidth,a.PixelHeight,data);}
}

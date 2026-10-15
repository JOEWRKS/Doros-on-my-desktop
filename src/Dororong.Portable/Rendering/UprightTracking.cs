using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

// Upright subset of presenter hunting orchestration. Pounce arbitration belongs
// to the later desktop controller, not this renderer/preview controller.
public sealed class UprightTracking
{
    private readonly HuntGaze gaze = new();
    private readonly TrackingRenderer renderer = new();
    private double blend;
    private bool? previousFlip;
    public bool IsTracking { get; private set; }
    public HuntGazePose Pose { get; private set; }
    public double Blend => 1-Math.Pow(1-blend,3);
    public static bool InRange(PointD? pointer)=>pointer is { } p&&double.IsFinite(p.X)&&double.IsFinite(p.Y)&&
        Math.Pow((p.X-48)/310,2)+Math.Pow((p.Y-57)/200,2)<1;
    public void Advance(double seconds,PointD? relativePointer,bool flip,bool blocked=false)
    {
        if(!double.IsFinite(seconds)||seconds<0)throw new ArgumentOutOfRangeException(nameof(seconds));
        seconds=Math.Min(seconds,.1);
        if(blocked){Reset();return;}
        if(previousFlip is { } previous && previous!=flip)gaze.Reset();
        previousFlip=flip;
        IsTracking=InRange(relativePointer);
        PointD? target=relativePointer is { } pointer?new((pointer.X-(flip?64:32))*2,(pointer.Y-53)*2):null;
        var current=gaze.Advance(seconds,target,flip,IsTracking);
        if(IsTracking)blend=Math.Min(1,blend+seconds/.12);
        else if(gaze.IsAtRest)blend=Math.Max(0,blend-seconds/.16);
        var mix=Blend;
        Pose=new(current.EyeX*mix,current.EyeY*mix,current.Roll*mix);
    }
    public RasterImage Render(RasterImage ordinary,bool closed=false,double breathing=0,RasterImage? walking=null)
    {
        if(Blend<=0)return ordinary;
        var result=renderer.Render(Pose,closed,breathing,walking);
        if(Blend>=1)return result;
        var a=ordinary.CopyPixels();var b=result.Pixels.Span;
        for(var i=0;i<a.Length;i++)a[i]=(byte)Math.Round(a[i]+(b[i]-a[i])*Blend);
        return new(96,96,a);
    }
    public void Reset(){gaze.Reset();blend=0;Pose=default;IsTracking=false;previousFlip=null;}
}

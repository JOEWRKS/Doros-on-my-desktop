using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

// Head-only extraction of DirectInteractionController's distance-driven path.
public sealed class HeadDragPresentation
{
    private readonly HeadSwingSession swing=new();private readonly SurroundingPullMotion surrounding=new();
    private DirectInteractionPhase phase;
    private PointD press,windowAnchor,relative,previousPointer;private bool flip,holding;
    private double strength,releaseMs,displayAngle,releaseAngle;
    private RasterImage? captured,last;private Func<double,RasterImage>? recovery;
    public bool Active=>phase!=DirectInteractionPhase.None;
    public bool Holding=>phase==DirectInteractionPhase.BodyDragHold;
    public bool Releasing=>phase==DirectInteractionPhase.BodyDragSettle;
    public double Strength=>strength;
    public double ReleaseProgress=>Math.Clamp(releaseMs/180,0,1);
    public double Angle=>displayAngle;
    public RasterImage? ViewportFrame {get;private set;}
    internal void SetPendingFrame(RasterImage frame){if(phase==DirectInteractionPhase.BodyPending)ViewportFrame=DesktopFrames.Pad(frame);}
    internal void ApplyLanding(Dororong.Core.Platforms.PlatformPose pose)
    {
        if(ViewportFrame is not {} frame)return;
        ViewportFrame=DesktopFrames.Warp(frame,DesktopFrames.LandingTransform(frame,pose),DesktopFrames.ViewportSize);
    }
    internal DirectInteractionPhase Phase=>phase;
    public void Begin(PointD pointer,PointD position,RasterImage visible,FacingDirection facing,bool hanging=false,
        RasterImage? anchorSource=null,Affine? sourceToWindow=null)
    {
        Reset();press=previousPointer=pointer;windowAnchor=pointer-position;flip=facing==FacingDirection.Left;
        captured=visible;var p=SurroundingPullRenderer.PinkCenter(ExtractNative(visible,flip));
        relative=new((flip?120-windowAnchor.X:windowAnchor.X-24)-Math.Round(p.X),windowAnchor.Y-24-Math.Round(p.Y));
        if(anchorSource is not null&&sourceToWindow is {} map)
        {
            // Use the actual displayed image transform, including jump/landing
            // and the distinct100px perch placement, just like WPF Capture.
            long sumX=0,sumY=0,count=0;var data=anchorSource.Pixels.Span;
            for(var y=0;y<anchorSource.PixelHeight;y++)for(var x=0;x<anchorSource.PixelWidth;x++)
            {var i=(y*anchorSource.PixelWidth+x)*4;if(data[i+3]>=128&&data[i+2]>data[i]+12&&data[i]>data[i+1]+12){sumX+=x;sumY+=y;count++;}}
            if(count>0)relative=map.Inverse().Transform(windowAnchor)-new PointD(Math.Round((double)sumX/count),Math.Round((double)sumY/count));
        }
        phase=DirectInteractionPhase.BodyPending;strength=hanging?1:0;
    }
    public void Advance(TimeSpan delta,PointD pointer,bool available,bool down)
    {
        if(!Active)return;
        if(!available||!HeadPullDistance.IsFinite(pointer)){Release();pointer=previousPointer;}
        if(Releasing)
        {releaseMs+=Math.Max(0,delta.TotalMilliseconds);displayAngle=releaseAngle*(1-Smooth(ReleaseProgress));surrounding.Step(default,delta.TotalMilliseconds);if(releaseMs>=180)Reset();return;}
        if(!down){Release();return;}
        if(!HeadPullDistance.TryMeasure(press,pointer,new(4,4),out var pull))return;
        if(!pull.OutsideDeadzone&&phase==DirectInteractionPhase.BodyPending)return;
        var carry=Holding||strength==1||pull.Distance>=80;strength=carry?1:pull.Strength;
        phase=carry?DirectInteractionPhase.BodyDragHold:DirectInteractionPhase.BodyDragEntry;
        displayAngle=swing.Advance(delta,pointer,true,phase,0);
        var offset=pointer-press;var length=Length(offset);var target=length>0?Scale(offset,14*strength/length):default;
        if(carry&&holding)
        {target=delta.TotalMilliseconds is >0 and <=250?Scale(pointer-previousPointer,75/delta.TotalMilliseconds):default;var speed=Length(target);target=speed<.6?default:speed>14?Scale(target,14/speed):target;}
        surrounding.Step(target,Math.Clamp(delta.TotalMilliseconds,0,250));holding=carry;previousPointer=pointer;
    }
    private void Release()
    {
        if(!Active||Releasing)return;
        if(phase==DirectInteractionPhase.BodyPending){Reset();return;}
        recovery=HeadPullFrames.Recovery(last??HeadPullFrames.Sample(strength),strength);phase=DirectInteractionPhase.BodyDragSettle;releaseMs=0;releaseAngle=displayAngle;surrounding.Release();
    }
    public RasterImage Render(double? readinessPhase=null)
    {
        if(phase==DirectInteractionPhase.BodyPending){ViewportFrame=DesktopFrames.Pad(captured!);return captured!;}
        var source=Releasing?recovery!(ReleaseProgress):HeadPullFrames.Sample(strength);if(!Releasing)last=source;
        var sourceAnchor=HeadPullAnchoring.RegistrationPoint(source)+relative;var remaining=Releasing?1-Smooth(ReleaseProgress):1;
        Affine Transform(double degrees)
        {
            var radians=degrees*Math.PI/180;var c=Math.Cos(radians);var s=Math.Sin(radians);var sx=flip?-1:1;
            var t=new Affine(sx*c,sx*s,-s,c,72-48*sx*c+48*s,72-48*sx*s-48*c);
            var point=t.Transform(sourceAnchor);return t with{OffsetX=t.OffsetX+(windowAnchor.X-point.X)*remaining,OffsetY=t.OffsetY+(windowAnchor.Y-point.Y)*remaining};
        }
        var ink=Bounds(source);bool Fits(double angle)
        {var t=Transform(angle);return new[]{new PointD(ink.X,ink.Y),new(ink.Right,ink.Y),new(ink.X,ink.Bottom),new(ink.Right,ink.Bottom)}.All(p=>{var q=t.Transform(p);return q.X>=-95&&q.Y>=-95&&q.X<=239&&q.Y<=239;});}
        var desired=displayAngle;
        if(!Fits(desired)||!Fits(-desired)){var low=0d;var high=1d;for(var i=0;i<12;i++){var mid=(low+high)/2;if(Fits(desired*mid)&&Fits(-desired*mid))low=mid;else high=mid;}displayAngle=desired*low;}
        var a=displayAngle*Math.PI/180;PointD Local(PointD p)=>new((p.X*Math.Cos(a)+p.Y*Math.Sin(a))*(flip?-1:1),-p.X*Math.Sin(a)+p.Y*Math.Cos(a));
        var padded=SurroundingPullRenderer.Head(source.CopyPixels(),sourceAnchor,Local(surrounding.Current),Local(surrounding.Far));var pixels=new byte[36864];
        for(var y=0;y<96;y++)Array.Copy(padded,((y+32)*160+32)*4,pixels,y*384,384);
        var rendered=new RasterImage(96,96,pixels);
        if(readinessPhase is {} flutter)rendered=new(96,96,ForelegFlutterFrame.Render(rendered,true,flutter,sourceAnchor));
        var transform=Transform(displayAngle);
        ViewportFrame=DesktopFrames.Warp(rendered,transform with {OffsetX=transform.OffsetX+96,OffsetY=transform.OffsetY+96},DesktopFrames.ViewportSize);
        return DesktopFrames.CropViewport(ViewportFrame);
    }
    private static RectD Bounds(RasterImage source)
    {var l=96;var r=0;var top=96;var bottom=0;var data=source.Pixels.Span;for(var y=0;y<96;y++)for(var x=0;x<96;x++)if(data[(y*96+x)*4+3]>0){l=Math.Min(l,x);r=Math.Max(r,x+1);top=Math.Min(top,y);bottom=Math.Max(bottom,y+1);}return new(Math.Max(0,l-4),Math.Max(0,top-4),Math.Min(96,r+4)-Math.Max(0,l-4),Math.Min(96,Math.Max(bottom,Math.Min(84,bottom+4)))-Math.Max(0,top-4));}
    private static byte[] ExtractNative(RasterImage frame,bool flip)
    {var data=new byte[36864];for(var y=0;y<96;y++)for(var x=0;x<96;x++)frame.Pixels.Span.Slice(((y+24)*144+(flip?119-x:x+24))*4,4).CopyTo(data.AsSpan((y*96+x)*4,4));return data;}
    public void Reset(){phase=DirectInteractionPhase.None;strength=releaseMs=displayAngle=releaseAngle=0;holding=false;captured=last=null;recovery=null;ViewportFrame=null;swing.Reset();surrounding.Reset();}
    private static double Smooth(double p)=>p*p*(3-2*p);
    private static double Length(PointD p)=>Math.Sqrt(p.X*p.X+p.Y*p.Y);
    private static PointD Scale(PointD p,double s)=>new(p.X*s,p.Y*s);
}

using Dororong.Core.Behavior;
using Dororong.Core.Geometry;

namespace Dororong.Portable.Rendering;

public sealed class HuntingPresentation
{
    private readonly HuntingSession session=new();private readonly PounceSession pounce=new();
    private readonly HuntGaze gaze=new();private readonly HuntingRenderer renderer=new();
    private bool near,tracking;private bool? previousFlip;
    private double blend,launchAge=1.5,recovery=2.6,delta;private PointD? pointer;
    public PouncePose Pounce=>pounce.Current;
    public bool Watching=>tracking||session.IsActive||Pounce.IsEngaged;
    public bool Preparing=>session.IsActive;
    public int Frame {get;private set;}=156;
    public Affine HeadTransform {get;private set;}=Affine.Identity;
    internal bool Rendered {get;private set;}
    private double renderedMix,renderedBreathing;private bool renderedClosed;
    private HuntGazePose renderedGaze;private RasterImage? renderedWalking;
    internal Func<double,double,double,RasterImage>? CaptureCheek()
    {
        if(!Rendered||renderedMix<1)return null;
        var frame=Frame;var g=renderedGaze;var closed=renderedClosed;var breathing=renderedBreathing;var walking=renderedWalking;
        var frozen=new HuntingRenderer();return (pull,eye,hair)=>frozen.Render(frame,g,closed,breathing,walking,pull,eye,hair);
    }
    public void Advance(double seconds,PointD? local,FacingDirection facing,bool blocked)
    {
        delta=Math.Clamp(seconds,0,.1);pointer=local;
        var radius=near?1.2:1;
        near=!blocked&&local is {} p&&Math.Pow((p.X-72)/(116.25*radius),2)+Math.Pow((p.Y-81)/(75*radius),2)<1;
        tracking=!blocked&&local is {} w&&Math.Pow((w.X-72)/310,2)+Math.Pow((w.Y-81)/200,2)<1;
        var old=Pounce;pounce.Advance(delta,near,local?.X-72??double.NaN,facing,blocked);
        if(!old.IsJump&&Pounce.IsJump)launchAge=Math.Clamp(session.FrameAgeSeconds,.7,1.65);
        if(!near&&session.IsActive&&!blocked&&!Pounce.IsEngaged)recovery=SnackRenderer.RecoveryAge(HuntPose.At(Frame/60d).Amount);
        if(blocked||near||Pounce.IsEngaged)recovery=2.6;else recovery=Math.Min(2.6,recovery+delta*.55/.24);
        if(Pounce.IsEngaged||!near)session.Reset();else session.Advance(delta,near,blocked);
        if(blocked){gaze.Reset();blend=0;}
        else if(!tracking&&!(Pounce.Phase==PouncePhase.Track&&pointer is not null)){gaze.Advance(delta,null,false,false);delta=0;}
        if(!blocked)
        {if(Watching)blend=Math.Min(1,blend+seconds/.12);else if(gaze.IsAtRest)blend=Math.Max(0,blend-seconds/.16);}
    }
    public RasterImage Render(RasterImage ordinary,FacingDirection facing,bool closed,double breathing,RasterImage? walking=null)
    {
        Rendered=false;
        if(!Watching&&recovery>=2.6&&gaze.IsAtRest&&blend<=0)return ordinary;
        var flip=facing==FacingDirection.Left;if(previousFlip is {} old&&old!=flip)gaze.Reset();previousFlip=flip;
        PointD? target=pointer is {} p?new((p.X-(flip?88:56))*2,(p.Y-77)*2):null;
        var g=gaze.Advance(delta,target,flip,Pounce.Phase==PouncePhase.Track?target is not null:tracking);delta=0;
        var mix=1-Math.Pow(1-blend,3);g=new(g.EyeX*mix,g.EyeY*mix,g.Roll*mix);
        var age=session.FrameAgeSeconds;
        if(Pounce.Phase==PouncePhase.Flight){var u=Math.Clamp(Pounce.Age/.34,0,1);age=launchAge+(2.05-launchAge)*u*u*(3-2*u);}
        else if(Pounce.Phase==PouncePhase.Landing)age=2.05+.55*Math.Clamp((Pounce.Age-.34)/.28,0,1);
        else if(Pounce.Phase==PouncePhase.Track)age=2.6;
        Frame=(int)Math.Clamp(Math.Round((!session.IsActive&&!Pounce.IsEngaged&&recovery<2.6?recovery:age)*60),0,156);
        HeadTransform=HuntingRenderer.HeadTransform(Frame,g);
        Rendered=true;renderedMix=mix;renderedGaze=g;renderedClosed=closed;renderedBreathing=breathing;renderedWalking=walking;
        return HuntingRenderer.Mix(ordinary,renderer.Render(Frame,g,closed,breathing,walking),mix);
    }
    internal RasterImage TransformJump(RasterImage placed,bool flip)
    {
        if(!Pounce.IsJump)return placed;
        return DesktopFrames.Warp(placed,JumpTransform(flip));
    }
    internal Affine JumpTransform(bool flip)
    {
        if(!Pounce.IsJump)return Affine.Identity;
        var a=Pounce.AngleRadians*(flip?-1:1);var c=Math.Cos(a);var s=Math.Sin(a);var x=24+(flip?65:31);const double y=110;
        var sx=Pounce.ScaleX;var sy=Pounce.ScaleY;
        return new(sx*c,sx*s,-sy*s,sy*c,x-x*sx*c+y*sy*s,y-x*sx*s-y*sy*c);
    }
    public void Reset(){session.Reset();pounce.Reset();gaze.Reset();near=tracking=Rendered=false;previousFlip=null;blend=delta=0;recovery=2.6;Frame=156;pointer=null;HeadTransform=Affine.Identity;}
}

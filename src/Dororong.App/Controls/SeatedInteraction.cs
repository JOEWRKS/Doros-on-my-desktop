using System.Windows;
using System.Windows.Media;

namespace Dororong.App.Controls;

internal enum SeatedReaction { None, Tap, Shake }

// Local presentation only: never moves the desktop window or owns a drag.
internal sealed class SeatedInteraction
{
    internal const double TapDuration=1.05, ShakeDuration=.95;
    internal const double TapCooldown=1.5, ShakeCooldown=3;
    internal SeatedReaction Kind { get; private set; }
    internal double Age { get; private set; }
    internal double AimAngle { get; private set; }=-.35;
    private double tapWait,shakeWait,nearTime;
    private bool wasClick;

    internal void Advance(double seconds,bool eligible,bool click,Point? pointer)
    {
        var dt=double.IsFinite(seconds)?Math.Clamp(seconds,0,.1):0;
        if(pointer is { } aim&&double.IsFinite(aim.X)&&double.IsFinite(aim.Y)){
            var target=PointerAngle(aim);
            AimAngle+=(target-AimAngle)*(1-Math.Exp(-18*dt));
        }
        tapWait=Math.Max(0,tapWait-dt);shakeWait=Math.Max(0,shakeWait-dt);
        var clickEdge=click&&!wasClick;wasClick=click;
        if(!eligible){Kind=SeatedReaction.None;Age=nearTime=0;return;}
        if(Kind!=SeatedReaction.None){
            Age+=dt;
            if(Age>=(Kind==SeatedReaction.Tap?TapDuration:ShakeDuration)){Kind=SeatedReaction.None;Age=0;}
        }
        if(clickEdge&&shakeWait<=0){Kind=SeatedReaction.Shake;Age=0;shakeWait=ShakeCooldown;nearTime=0;return;}
        if(click){nearTime=0;return;}
        // Authored front paw/face side only. A cursor behind the rump cannot
        // provoke a paw reaching away from it. Coordinates are size independent.
        var near=pointer is { } p&&double.IsFinite(p.X)&&double.IsFinite(p.Y)&&
            Math.Pow((p.X-13)/52,2)+Math.Pow((p.Y-65)/43,2)<1;
        nearTime=near?nearTime+dt:0;
        if(Kind==SeatedReaction.None&&tapWait<=0&&nearTime>=.14){
            Kind=SeatedReaction.Tap;Age=0;tapWait=TapCooldown;nearTime=0;
        }
    }

    internal static double PointerAngle(Point pointer)=>Math.Clamp(Math.Atan2(pointer.Y-73,Math.Max(8,25-pointer.X)),-1.15,.5);
    internal static HighResolutionFacePose Pose(HighResolutionFacePose seated,SeatedReaction kind,double age,double aimAngle=-.35)
    {
        static double S(double t)=>HighResolutionGrabRaster.Smooth(t);
        if(kind==SeatedReaction.Tap){
            var envelope=S(age/.18)*(1-S((age-.78)/.27));
            // One raise, two quick little reaches, one lower.
            var beat=Math.Pow(Math.Sin(Math.PI*Math.Clamp((age-.18)/.6,0,1)*2),2);
            return envelope==0?seated:seated with {SeatedPawAmount=envelope,SeatedPawAngle=aimAngle,SeatedPawPulse=beat};
        }
        if(kind==SeatedReaction.Shake){
            var envelope=S(age/.12)*(1-S((age-.63)/.32));
            var wave=Math.Sin(age*Math.PI*16)*envelope;
            var head=seated.Head;head.RotateAt(-1.4*wave,36,62);head.Translate(.88*wave,0);
            return seated with {Head=head,SeatedShake=.88*wave,Closed=envelope>.2};
        }
        return seated;
    }
}

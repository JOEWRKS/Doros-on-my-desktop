// Mac-only extraction of accepted r104 (3813312). Windows source is unchanged.
using System.Windows;
using System.Windows.Media;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
namespace Dororong.App.Controls;
internal static class HuntRenderer {
    internal static Matrix HeadTransform(int frame,HuntGazePose gaze)
    {
        var pose=HuntPose.At(frame/60d);
        var angle=gaze.Roll<0?gaze.Roll*(1-pose.Amount)+Math.Max(gaze.Roll,-.08)*pose.Amount:gaze.Roll;
        PointD Head(double x,double y) => pose.Head(43+(x-43)*Math.Cos(angle)-(y-66)*Math.Sin(angle),
            66+(x-43)*Math.Sin(angle)+(y-66)*Math.Cos(angle));
        var o=Head(0,0);var x=Head(1,0);var y=Head(0,1);
        return new(x.X-o.X,x.Y-o.Y,y.X-o.X,y.Y-o.Y,o.X,o.Y);
    }}
internal static class SnackRenderer {
    internal static double RecoveryAge(double amount)
    {
        // Invert the existing authored crouch recovery, retaining pose depth.
        double lo=2.05,hi=2.6;
        for(var i=0;i<20;i++){var mid=(lo+hi)/2;if(HuntPose.At(mid).Amount>amount)lo=mid;else hi=mid;}
        return (lo+hi)/2;
    }
    internal static int JumpFrame(double age)
    {
        double seconds;
        if(age<SnackSession.JumpPrepareSeconds)
            seconds=.25+.45*Smooth(age/SnackSession.JumpPrepareSeconds);
        else if(age<SnackSession.JumpPrepareSeconds+SnackSession.JumpFlightSeconds)
        {
            var u=(age-SnackSession.JumpPrepareSeconds)/SnackSession.JumpFlightSeconds;
            // Push out of the hunting crouch early, extend during flight.
            seconds=2.05+.55*Smooth(u/.65);
        }
        else
        {
            // The hunting crouch also reaches the forepaws forward. Replaying
            // it as landing compression creates an unwanted second paw jab.
            // Flight already recovers to the planted pose before touchdown.
            seconds=2.6;
        }
        return (int)Math.Clamp(Math.Round(seconds*60),0,156);
    }
    internal static double HappyRoll(double age)
    {
        // Long continuous sways rather than quick tips and held accents.
        // Zero speed at each boundary; the closed-eye neutral endpoints exactly
        // match the last chew and the existing quiet settling phase.
        ReadOnlySpan<double> times=[0,.06,.46,1.10,1.64,2.10,SnackSession.HappySeconds];
        ReadOnlySpan<double> degrees=[0,0,-8,8,-4,0,0];
        if(!double.IsFinite(age)||age<=0||age>=SnackSession.HappySeconds)return 0;
        for(var i=1;i<times.Length;i++)
            if(age<=times[i])
            {
                var u=Smooth((age-times[i-1])/(times[i]-times[i-1]));
                return (degrees[i-1]+(degrees[i]-degrees[i-1])*u)*Math.PI/180;
            }
        return 0;
    }
    private static double Smooth(double u){u=Math.Clamp(u,0,1);return u*u*(3-2*u);}}

using Dororong.Core.Behavior;

namespace Dororong.Portable.Rendering;

// Timing, frame selection, source-pixel chewing and head sway match the accepted
// Windows renderer. Derived vector coverage is isolated from unaffected artwork.
public sealed class SnackRenderer
{
    private readonly TrackingRenderer tracking = new();
    private readonly Dictionary<(int,int),RasterImage> runs = new();
    public Affine HeadTransform { get; private set; } = Affine.Identity;
    public static double RecoveryAge(double amount)
    {
        if(!double.IsFinite(amount)||amount<0||amount>1)throw new ArgumentOutOfRangeException(nameof(amount));
        // Invert the existing authored crouch recovery, retaining pose depth.
        double lo=2.05,hi=2.6;
        for(var i=0;i<20;i++){var mid=(lo+hi)/2;if(HuntPose.At(mid).Amount>amount)lo=mid;else hi=mid;}
        return (lo+hi)/2;
    }
    public static int JumpFrame(double age)
    {
        if(!double.IsFinite(age)||age<0)throw new ArgumentOutOfRangeException(nameof(age));
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

    public RasterImage Render(SnackPose pose, double entryRecoveryAge=2.6)
    {
        if(!double.IsFinite(pose.Age)||pose.Age<0||!double.IsFinite(pose.Distance)||pose.Distance<0||
            !double.IsFinite(pose.RunAmount)||pose.RunAmount<0||pose.RunAmount>1||
            !double.IsFinite(pose.JumpAge)||pose.JumpAge < -1||!double.IsFinite(entryRecoveryAge)||entryRecoveryAge<0||entryRecoveryAge>2.6)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if(!pose.Active){HeadTransform=Affine.Identity;return MotionRenderer.Standing();}
        if(pose.Phase==SnackPhase.Happy)
        {
            var gaze=new HuntGazePose(0,0,HappyRoll(pose.Age));
            HeadTransform=TrackingRenderer.HeadTransform(gaze);
            return tracking.Render(gaze,closed:true);
        }
        var eating=pose.Phase==SnackPhase.Eating;
        HeadTransform=Affine.Identity;
        var frame=pose.JumpAge>=0?JumpFrame(pose.JumpAge):156;
        if(pose.Phase==SnackPhase.Chasing&&pose.Age<.24&&entryRecoveryAge<2.59)
            frame=(int)Math.Round(Math.Min(2.6,entryRecoveryAge+pose.Age*.55/.24)*60);
        RasterImage source;var effectTransform=frame;
        if(frame<156)
        {
            source=JumpBank.Instance.Frame(frame);
            var hp=HuntPose.At(frame/60d);var o=hp.Head(0,0);var x=hp.Head(1,0);var y=hp.Head(0,1);
            HeadTransform=new(x.X-o.X,x.Y-o.Y,y.X-o.X,y.Y-o.Y,o.X,o.Y);
        }
        else if(pose.Phase==SnackPhase.Chasing&&pose.RunAmount>0&&pose.JumpAge<0)
        {
            var phase=(int)Math.Round((pose.Distance%SnackRunFrames.Stride)/SnackRunFrames.Stride*48)%48;
            var level=(int)Math.Round(pose.RunAmount*8);var key=(phase,level);
            effectTransform=157+level*48+phase;
            if(!runs.TryGetValue(key,out source!))
                runs[key]=source=SnackRunFrames.Render(phase*SnackRunFrames.Stride/48,level/8d);
            HeadTransform=new(1,0,0,1,0,SnackRunFrames.Bob(phase/48d,level/8d));
        }
        else source=UprightRumpSource.Standing(eating||pose.Phase==SnackPhase.Settling&&pose.Age<.16);
        if(pose.Phase==SnackPhase.Settling)return source;
        if(eating)source=SnackMouthFrame.Chew(source,pose.Age);
        var pixels=source.CopyPixels();RasterImage.Over(pixels,SnackEffectBank.Instance.Render(eating,pose.Age,effectTransform));
        return new(96,96,pixels);
    }
    public static double HappyRoll(double age)
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
    private static double Smooth(double u){u=Math.Clamp(u,0,1);return u*u*(3-2*u);}
}


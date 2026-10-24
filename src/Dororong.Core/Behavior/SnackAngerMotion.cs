namespace Dororong.Core.Behavior;

// Temporary reactions, without world translation or physics changes.
public static class SnackAngerMotion
{
    public static int StampCount(int streak)=>streak<2?0:streak==2?4:Math.Clamp(streak,3,5);
    // One preparation, uninterrupted same-cadence beats, one final recovery.
    // Do not loop the whole two-beat animation or scale its time.
    public static double Duration(int streak)=>streak<2?SnackSession.AngrySeconds:
        .9+.32*(StampCount(streak)-2);
    public static double Roll(double age, int withdrawalStreak = 1)
    {
        if (withdrawalStreak >= 2) return 0;
        if (!double.IsFinite(age) || age <= 0 || age >= SnackSession.AngrySeconds) return 0;
        var envelope = Smooth(age / .12) * Smooth((SnackSession.AngrySeconds - age) / .22);
        return Math.Sin(age * Math.PI * 10) * envelope * (3 * Math.PI / 180);
    }
    public static double PawLift(double age, int withdrawalStreak)
    {
        if (withdrawalStreak < 2 || !double.IsFinite(age)) return 0;
        var result=0d;
        for(var hit=0;hit<StampCount(withdrawalStreak);hit++)result+=Beat(.04+.32*hit,.19+.32*hit,.28+.32*hit);
        return result;
        double Beat(double start,double peak,double contact)
        {
            if(age<=start||age>=contact)return 0;
            return (withdrawalStreak>=3?4.8:2.4)*(age<=peak?Smooth((age-start)/(peak-start)):
                1-Smooth((age-peak)/(contact-peak)));
        }
    }
    public static double PawReach(double age,int withdrawalStreak)
        => !Active(age,withdrawalStreak)?0:withdrawalStreak>=3?16*Envelope(age,withdrawalStreak):3.2*Envelope(age,withdrawalStreak)+.8*Impact(age,withdrawalStreak);
    public static double Crouch(double age,int withdrawalStreak)
    {
        if(!Active(age,withdrawalStreak))return 0;
        // Establish the forward paw first, then bow into each impact.
        // Only about one third of the pre-pounce crouch, with no jump/sway.
        return .12*Envelope(age,withdrawalStreak)+.22*Impact(age,withdrawalStreak);
    }
    private static double Impact(double age,int streak)
    {
        var count=StampCount(streak);var result=0d;
        for(var hit=0;hit<count;hit++)result+=Press(.19+.32*hit,.28+.32*hit,(hit==count-1?.38:.36)+.32*hit);
        return result;
        double Press(double start,double hit,double end)=>age<=start||age>=end?0:
            age<=hit?Smooth((age-start)/(hit-start)):1-Smooth((age-hit)/(end-hit));
    }
    private static bool Active(double age,int streak)=>streak>=2&&double.IsFinite(age)&&age>0&&age<Duration(streak);
    private static double Envelope(double age,int streak)=>Smooth((age-.02)/.16)*(1-Smooth((age-(.66+.32*(StampCount(streak)-2)))/.24));
    private static double Smooth(double x) { x = Math.Clamp(x, 0, 1); return x*x*(3-2*x); }
}

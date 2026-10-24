using Dororong.Core.Geometry;

namespace Dororong.Core.Behavior;

public enum SnackPhase { None, Chasing, Eating, Settling, Happy, Angry, Snatching, Chuckling }

public readonly record struct SnackPose(SnackPhase Phase, FacingDirection Facing,
    double DeltaX, double Lift, double Age, double Distance)
{
    public bool Active => Phase != SnackPhase.None;
    public bool CarriesJewel => Phase is SnackPhase.Chasing or SnackPhase.Angry or SnackPhase.Snatching;
    public double JumpAge { get; init; } = -1;
    public double RunAmount { get; init; }
    public int WithdrawalStreak { get; init; }
    // Absolute desired mouth position during the sixth-withdrawal pounce.
    // The host applies its real monitor bounds and confirms actual contact.
    public PointD? SnatchMouth { get; init; }
}

// No keyboard, hunger, persistence or rewards. Withdrawal streak is session-local.
public sealed class SnackSession
{
    public const double OfferSeconds = .5;
    public const double AngrySeconds = .9;
    // Reject a one-frame fly-by without requiring the old extra .16s dwell.
    public const double OfferRecognitionSeconds = .08;
    public const double EatingSeconds = 1.7;
    public const double HappySeconds = 2.16;
    public const double SettleSeconds = .32;
    public const double JumpPrepareSeconds = .18;
    public const double JumpFlightSeconds = .48;
    public const double JumpLandingSeconds = .20;
    public const double JumpSeconds = JumpPrepareSeconds + JumpFlightSeconds + JumpLandingSeconds;
    public const double SnatchPrepareSeconds = .04;
    public const double SnatchFlightSeconds = .20;
    public const double SnatchTimeoutSeconds = .65;
    public const double ChuckleSeconds = 1.8;
    private SnackPhase phase;
    private FacingDirection facing;
    private PointD? lastPointer;
    private double age, biteTime, jumpAge = -1, cooldown, distance, jumpHeight, runAmount;
    private bool offerRecognized;
    private bool snatchCelebration;
    private PointD offerPointer;
    private PointD? dwellPointer;
    private int withdrawalStreak;
    private double jumpDistance, jumpTravel;
    private PointD snatchStart;
    private double snatchScale = 1;
    public bool Active => phase != SnackPhase.None;
    public SnackPose Current { get; private set; }
    public bool MealCompleted { get; private set; }

    public void Begin(FacingDirection direction)
    {
        Cancel(); phase = SnackPhase.Chasing; facing = direction;
        Current = new(phase, facing, 0, 0, 0, 0);
    }

    public void Cancel()
    {
        MealCompleted = false;
        phase = SnackPhase.None; lastPointer = null;
        age = biteTime = cooldown = distance = runAmount = jumpDistance = jumpTravel = 0; jumpAge = -1;
        offerRecognized = false;
        snatchCelebration = false;
        dwellPointer = null;
        withdrawalStreak = 0;
        snatchStart=default;snatchScale=1;
        Current = default;
    }

    public SnackPose Advance(TimeSpan elapsed, PointerSample pointer, PointD mouth, bool supported, double chaseScale = 1)
    {
        MealCompleted = false;
        if (!Active) return Current;
        // A lock/resume or lost input must not leave a runaway chase/cursor.
        if (!pointer.IsAvailable || !Finite(pointer.Position) || !Finite(mouth) ||
            !double.IsFinite(elapsed.TotalSeconds) || elapsed.TotalSeconds > 1)
        { Cancel(); return Current; }
        var dt = Math.Clamp(elapsed.TotalSeconds, 0, .05);
        // Logical pet size, not monitor DPI. Keep the legacy/default host at1x.
        chaseScale = double.IsFinite(chaseScale) ? Math.Clamp(chaseScale, 1, 1.5) : 1;
        age += dt; cooldown = Math.Max(0, cooldown - dt);
        if(phase==SnackPhase.Snatching){
            if(age>=SnatchTimeoutSeconds){
                // A clamped/unreachable target is not a successful bite.
                phase=SnackPhase.Chasing;age=biteTime=0;offerRecognized=false;
                lastPointer=dwellPointer=null;withdrawalStreak=5;
                return Current=new(phase,facing,0,0,0,distance){WithdrawalStreak=withdrawalStreak};
            }
            return Current=SnatchPose(pointer.Position,mouth);
        }
        if (jumpAge >= 0)
        {
            jumpAge += dt;
            if (jumpAge >= JumpSeconds) jumpAge = -1;
        }
        var dx = pointer.Position.X - mouth.X;
        var dy = pointer.Position.Y - mouth.Y;
        double move = 0;
        if (phase == SnackPhase.Chasing)
        {
            var previousFacing = facing;
            var movement = lastPointer is { } previous
                ? Math.Sqrt(Math.Pow(pointer.Position.X-previous.X,2)+Math.Pow(pointer.Position.Y-previous.Y,2))
                : double.PositiveInfinity;
            var steady = lastPointer is null || movement <= Math.Max(.35, dt * 18);
            lastPointer = pointer.Position;
            // An airborne pet follows the launch trajectory, not a live cursor
            // servo. Facing and horizontal destination stay fixed until landing.
            if (jumpAge < 0)
            {
                if (Math.Abs(dx) > 30) facing = dx > 0 ? FacingDirection.Right : FacingDirection.Left;
                if (Math.Abs(dx) > 8) move = Math.Sign(dx) * Math.Min(Math.Abs(dx)-8, 185*chaseScale*dt);
            }
            // Holding at the mouth gives, moving past the face does not.
            var atMouth = jumpAge < 0 && Math.Abs(dx) <= 19 && Math.Abs(dy) <= 17;
            // Judge withdrawal against the offered cursor, not one frame's
            // velocity: a slow deliberate pull still counts; pet/size motion
            // beneath a stationary cursor does not falsely punish the user.
            var withdrew = jumpAge < 0 && offerRecognized && !atMouth &&
                (Math.Abs(pointer.Position.X-offerPointer.X)>1 || Math.Abs(pointer.Position.Y-offerPointer.Y)>1);
            var drifted = dwellPointer is { } anchor &&
                (Math.Abs(pointer.Position.X-anchor.X)>1 || Math.Abs(pointer.Position.Y-anchor.Y)>1);
            if (!atMouth || !steady || drifted) { biteTime = 0; dwellPointer = null; }
            if (atMouth && steady)
            { dwellPointer ??= pointer.Position; biteTime += dt; }
            if (atMouth && !offerRecognized && biteTime + 1e-9 >= OfferRecognitionSeconds)
            { offerRecognized = true; offerPointer = pointer.Position; }
            if (!atMouth) offerRecognized = false;
            if (withdrew)
            {
                withdrawalStreak=Math.Min(6,withdrawalStreak+1);age=0;move=runAmount=biteTime=0;
                facing=previousFacing;
                if(withdrawalStreak==6){
                    phase=SnackPhase.Snatching;snatchStart=mouth;snatchScale=chaseScale;jumpAge=-1;
                    if(Math.Abs(dx)>8)facing=dx>0?FacingDirection.Right:FacingDirection.Left;
                    return Current=SnatchPose(pointer.Position,mouth);
                }
                phase=SnackPhase.Angry;
            }
            else if (biteTime + 1e-9 >= OfferSeconds)
            { withdrawalStreak = 0; phase = SnackPhase.Eating; age = 0; move = 0; offerRecognized = false; }
            else if (supported && jumpAge < 0 && cooldown <= 0 && dy < -25 && Math.Abs(dx) < 90)
            {
                jumpAge = 0; jumpHeight = Math.Clamp(-dy, 28, 76); cooldown = 1.5;
                jumpDistance = Math.Sign(dx)*Math.Min(54,Math.Max(0,Math.Abs(dx)-8));
                jumpTravel = 0;
            }
        }
        else if (phase == SnackPhase.Angry && age + 1e-9 >= SnackAngerMotion.Duration(withdrawalStreak))
        {
            phase = SnackPhase.Chasing; age = biteTime = 0; offerRecognized = false;
            lastPointer = dwellPointer = null;
        }
        else if (phase == SnackPhase.Eating && age >= EatingSeconds)
        {
            // Actual bite alone is not a landing. Wait for stable support;
            // ordinary meals retain their existing happy reaction.
            if (!snatchCelebration || supported)
            { phase = snatchCelebration ? SnackPhase.Chuckling : SnackPhase.Happy; age = 0; }
        }
        else if (phase == SnackPhase.Chuckling)
        {
            // Losing the supporting window must never make it laugh in air.
            if (!supported) { phase = SnackPhase.Eating; age = EatingSeconds; }
            else if (age >= ChuckleSeconds) { Cancel(); MealCompleted = true; return Current; }
        }
        else if (phase == SnackPhase.Happy && age >= HappySeconds)
        { phase = SnackPhase.Settling; age = 0; }
        else if (phase == SnackPhase.Settling && age >= SettleSeconds) { Cancel(); MealCompleted = true; return Current; }
        var flight = (jumpAge - JumpPrepareSeconds) / JumpFlightSeconds;
        if(jumpAge>=0)
        {
            // Integrated endpoint, independent of frame rate. Smooth launch and
            // touchdown velocities, with no accumulated cursor corrections.
            var travel=jumpDistance*Smooth(flight);
            move=travel-jumpTravel;jumpTravel=travel;
        }
        var lift = jumpAge < 0 ? 0 : JumpLift(flight,jumpHeight);
        var targetRun = Math.Abs(move) > .01 && jumpAge < 0 ? 1d : 0d;
        runAmount += Math.Clamp(targetRun-runAmount,-dt/.12,dt/.12);
        // Ground gait is measured in authored1x units: larger legs cover more
        // screen distance per stride, without speeding up the animation cycle.
        // Preserve the existing jump trajectory and its phase contribution.
        distance += Math.Abs(move) / (jumpAge < 0 ? chaseScale : 1);
        return Current = new(phase, facing, move, lift, age, distance) { JumpAge = jumpAge, RunAmount = runAmount, WithdrawalStreak = withdrawalStreak };
    }

    private SnackPose SnatchPose(PointD pointer,PointD mouth)
    {
        var u=Math.Clamp((age-SnatchPrepareSeconds)/SnatchFlightSeconds,0,1);
        var blend=Smooth(u);
        var target=new PointD(snatchStart.X+(pointer.X-snatchStart.X)*blend,
            snatchStart.Y+(pointer.Y-snatchStart.Y)*blend-20*snatchScale*Math.Sin(Math.PI*u));
        var authoredAge=age<SnatchPrepareSeconds?JumpPrepareSeconds*age/SnatchPrepareSeconds:
            JumpPrepareSeconds+JumpFlightSeconds*u;
        return new(phase,facing,target.X-mouth.X,0,age,distance){JumpAge=authoredAge,
            WithdrawalStreak=withdrawalStreak,SnatchMouth=target};
    }

    public bool ConfirmSnatch(PointD actualMouth,PointerSample pointer)
    {
        if(phase!=SnackPhase.Snatching||age<SnatchPrepareSeconds||!pointer.IsAvailable||
            !Finite(actualMouth)||!Finite(pointer.Position))return false;
        var dx=pointer.Position.X-actualMouth.X;var dy=pointer.Position.Y-actualMouth.Y;
        if(dx*dx+dy*dy>Math.Pow(8*snatchScale,2))return false;
        phase=SnackPhase.Eating;age=biteTime=0;jumpAge=-1;withdrawalStreak=0;offerRecognized=false;
        snatchCelebration=true;
        lastPointer=dwellPointer=null;
        Current=new(phase,facing,0,0,0,distance);
        return true;
    }

    private static bool Finite(PointD point) => double.IsFinite(point.X) && double.IsFinite(point.Y);
    public static double JumpLift(double flight,double height)
    {
        if(flight<=0||flight>=1)return 0;
        // Keep the central ballistic arc, ease only its first/last58ms. The
        // derivative meets the planted pose at zero instead of a vertical snap.
        return -height*4*flight*(1-flight)*Smooth(flight/.12)*Smooth((1-flight)/.12);
    }
    private static double Smooth(double u){u=Math.Clamp(u,0,1);return u*u*(3-2*u);}
}

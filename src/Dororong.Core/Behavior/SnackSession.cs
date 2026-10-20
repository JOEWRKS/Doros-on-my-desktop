using Dororong.Core.Geometry;

namespace Dororong.Core.Behavior;

public enum SnackPhase { None, Chasing, Eating, Settling, Happy }

public readonly record struct SnackPose(SnackPhase Phase, FacingDirection Facing,
    double DeltaX, double Lift, double Age, double Distance)
{
    public bool Active => Phase != SnackPhase.None;
    public double JumpAge { get; init; } = -1;
    public double RunAmount { get; init; }
}

// No keyboard, hunger, counters, persistence or rewards. One voluntary play session.
public sealed class SnackSession
{
    public const double EatingSeconds = 1.7;
    public const double HappySeconds = 2.16;
    public const double SettleSeconds = .32;
    public const double JumpPrepareSeconds = .18;
    public const double JumpFlightSeconds = .48;
    public const double JumpLandingSeconds = .20;
    public const double JumpSeconds = JumpPrepareSeconds + JumpFlightSeconds + JumpLandingSeconds;
    private SnackPhase phase;
    private FacingDirection facing;
    private PointD? lastPointer;
    private double age, stillTime, biteTime, jumpAge = -1, cooldown, distance, jumpHeight, runAmount;
    private double jumpDistance, jumpTravel;
    public bool Active => phase != SnackPhase.None;
    public SnackPose Current { get; private set; }

    public void Begin(FacingDirection direction)
    {
        Cancel(); phase = SnackPhase.Chasing; facing = direction;
        Current = new(phase, facing, 0, 0, 0, 0);
    }

    public void Cancel()
    {
        phase = SnackPhase.None; lastPointer = null;
        age = stillTime = biteTime = cooldown = distance = runAmount = jumpDistance = jumpTravel = 0; jumpAge = -1;
        Current = default;
    }

    public SnackPose Advance(TimeSpan elapsed, PointerSample pointer, PointD mouth, bool supported, double chaseScale = 1)
    {
        if (!Active) return Current;
        // A lock/resume or lost input must not leave a runaway chase/cursor.
        if (!pointer.IsAvailable || !Finite(pointer.Position) || !Finite(mouth) ||
            !double.IsFinite(elapsed.TotalSeconds) || elapsed.TotalSeconds > 1)
        { Cancel(); return Current; }
        var dt = Math.Clamp(elapsed.TotalSeconds, 0, .05);
        // Logical pet size, not monitor DPI. Keep the legacy/default host at1x.
        chaseScale = double.IsFinite(chaseScale) ? Math.Clamp(chaseScale, 1, 1.5) : 1;
        age += dt; cooldown = Math.Max(0, cooldown - dt);
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
            var movement = lastPointer is { } previous
                ? Math.Sqrt(Math.Pow(pointer.Position.X-previous.X,2)+Math.Pow(pointer.Position.Y-previous.Y,2))
                : double.PositiveInfinity;
            stillTime = movement <= Math.Max(.35, dt * 18) ? stillTime + dt : 0;
            lastPointer = pointer.Position;
            // An airborne pet follows the launch trajectory, not a live cursor
            // servo. Facing and horizontal destination stay fixed until landing.
            if (jumpAge < 0)
            {
                if (Math.Abs(dx) > 30) facing = dx > 0 ? FacingDirection.Right : FacingDirection.Left;
                if (Math.Abs(dx) > 8) move = Math.Sign(dx) * Math.Min(Math.Abs(dx)-8, 185*chaseScale*dt);
            }
            // Holding at the mouth gives, moving past the face does not.
            biteTime = jumpAge < 0 && Math.Abs(dx) <= 19 && Math.Abs(dy) <= 17 && stillTime >= .16
                ? biteTime + dt : 0;
            if (biteTime >= .24)
            { phase = SnackPhase.Eating; age = 0; move = 0; }
            else if (supported && jumpAge < 0 && cooldown <= 0 && dy < -25 && Math.Abs(dx) < 90)
            {
                jumpAge = 0; jumpHeight = Math.Clamp(-dy, 28, 76); cooldown = 1.5;
                jumpDistance = Math.Sign(dx)*Math.Min(54,Math.Max(0,Math.Abs(dx)-8));
                jumpTravel = 0;
            }
        }
        else if (phase == SnackPhase.Eating && age >= EatingSeconds)
        { phase = SnackPhase.Happy; age = 0; }
        else if (phase == SnackPhase.Happy && age >= HappySeconds)
        { phase = SnackPhase.Settling; age = 0; }
        else if (phase == SnackPhase.Settling && age >= SettleSeconds) { Cancel(); return Current; }
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
        return Current = new(phase, facing, move, lift, age, distance) { JumpAge = jumpAge, RunAmount = runAmount };
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

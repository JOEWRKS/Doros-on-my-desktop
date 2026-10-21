// Mac-only extraction of accepted r104 (3813312). Windows source is unchanged.
using System.Windows;
using System.Windows.Media;
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
using Dororong.Core.Platforms;
namespace Dororong.App.Controls;
internal sealed class HeadSurroundingPresentation {
private readonly SurroundingPullMotion _motion=new(); private bool _settling,_holding;private PointD? _previousPointer;
    internal void Advance(DirectInteractionSnapshot snapshot, double elapsedMilliseconds)
    {
        var dt = elapsedMilliseconds;
        var settling = snapshot.Phase == DirectInteractionPhase.BodyDragSettle;
        if (settling && !_settling) { _motion.Release(); dt = 0; }
        _settling = settling;
        var offset = snapshot.PointerPosition - snapshot.PressOrigin;
        var length = BodyPullSession.Length(offset);
        var target = double.IsFinite(length) && length > 0
            ? BodyPullSession.Scale(offset, 14 * Math.Clamp(snapshot.Strength, 0, 1) / length) : default;
        var holding = snapshot.Phase == DirectInteractionPhase.BodyDragHold;
        if (holding && _holding)
        {
            // Once carried, the whole pet follows the mouse. Secondary lag follows
            // its recent velocity, not its distance from an obsolete press origin.
            target = dt > 0 && dt <= 250 && _previousPointer is { } previous
                ? BodyPullSession.Scale(snapshot.PointerPosition - previous, 75 / dt) : default;
            var speed = BodyPullSession.Length(target);
            if (!double.IsFinite(speed) || speed < .6) target = default;
            else if (speed > 14) target = BodyPullSession.Scale(target, 14 / speed);
        }
        _holding = holding;
        _previousPointer = snapshot.PointerPosition;
        // Simulation stays in screen axes; convert only at raster presentation.
        _motion.Step(target, double.IsFinite(dt) ? Math.Clamp(dt, 0, 250) : 0);
    }
    internal (PointD Near,PointD Far) LocalOffsets(FacingDirection facing,double angle)
    {
        var radians=angle*Math.PI/180;var c=Math.Cos(radians);var s=Math.Sin(radians);
        PointD Local(PointD p)=>new((p.X*c+p.Y*s)*(facing==FacingDirection.Left?-1:1),-p.X*s+p.Y*c);
        return (Local(_motion.Current),Local(_motion.Far));
    }
    internal void Reset() { _motion.Reset(); _settling = false; _holding = false; _previousPointer = null; }}

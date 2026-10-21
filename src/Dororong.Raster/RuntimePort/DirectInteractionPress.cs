// Mac-only extraction of accepted r104 (3813312). Windows source is unchanged.
using Dororong.App.Interaction;
using Dororong.Core.Behavior;
using Dororong.Core.Geometry;
namespace Dororong.App.Controls;
internal sealed class DirectInteractionPressEventArgs : EventArgs
{
    internal DirectInteractionPressEventArgs(
        DirectInteractionTarget target,
        PointD windowLocalPosition,
        PointD framePosition,
        double outwardSign)
        : this(target, windowLocalPosition, framePosition, outwardSign, null)
    {
    }

    internal DirectInteractionPressEventArgs(
        DirectInteractionTarget target,
        PointD windowLocalPosition,
        PointD framePosition,
        double outwardSign,
        BodyPullCapture? bodyCapture)
    {
        Target = target;
        WindowLocalPosition = windowLocalPosition;
        FramePosition = framePosition;
        OutwardSign = outwardSign;
        BodyCapture = bodyCapture;
    }

    internal DirectInteractionTarget Target { get; }
    internal PointD WindowLocalPosition { get; }
    internal PointD FramePosition { get; }
    internal double OutwardSign { get; }
    internal BodyPullCapture? BodyCapture { get; }
    internal CheekPullCapture? CheekCapture { get; init; }
    internal FacingDirection? PressFacing { get; init; }
    internal bool IsAttachedCheek { get; init; }
    internal bool StartsHanging { get; init; }
}

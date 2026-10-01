namespace Dororong.App.Controls;

internal sealed class LocomotionPresentation
{
    private const double SitDurationMilliseconds = 1050;
    private const double RiseDurationMilliseconds = 650;
    private const double GaitBlendMilliseconds = 180;
    private double _sitProgress;
    private double _walkProgress;

    // Authored kinematics already contain the acceleration and contact phases.
    // A second whole-body easing here would distort the foot support timing.
    internal double Sit => _sitProgress;
    internal bool UsesRisePose { get; private set; }
    internal double Walk => Smooth(_walkProgress);
    internal double Distance { get; private set; }
    internal bool HoldWalking => _sitProgress > 0;

    internal void Advance(double milliseconds, bool walking, double distance, bool blocked, bool sitRequested = false)
    {
        if (!double.IsFinite(milliseconds) || milliseconds < 0 || !double.IsFinite(distance))
            throw new ArgumentOutOfRangeException(nameof(milliseconds));
        if (blocked)
        {
            _sitProgress = _walkProgress = Distance = 0;
            UsesRisePose = false;
            return;
        }
        if (sitRequested)
        {
            // Reverse on the current path if interrupted; changing banks at an
            // intermediate pose would teleport the feet/head. Full endpoints
            // are shared, so only there may we select another motion path.
            if (_sitProgress == 0) UsesRisePose = false;
            _sitProgress = Math.Min(1, _sitProgress + milliseconds / (UsesRisePose ? RiseDurationMilliseconds : SitDurationMilliseconds));
            _walkProgress = Math.Max(0, _walkProgress - milliseconds / GaitBlendMilliseconds);
            return;
        }
        var rising = _sitProgress > 0;
        if (_sitProgress == 1 && milliseconds > 0) UsesRisePose = true;
        _sitProgress = Math.Max(0, _sitProgress - milliseconds / (UsesRisePose ? RiseDurationMilliseconds : SitDurationMilliseconds));
        if (walking && !rising)
        {
            _walkProgress = Math.Min(1, _walkProgress + milliseconds / GaitBlendMilliseconds);
            Distance = (Distance + Math.Abs(distance)) % 10;
        }
        else
        {
            _walkProgress = Math.Max(0, _walkProgress - milliseconds / GaitBlendMilliseconds);
        }
    }

    private static double Smooth(double x) => x * x * (3 - 2 * x);
}

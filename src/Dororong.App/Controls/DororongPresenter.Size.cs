using System.Windows.Controls;

namespace Dororong.App.Controls;

public partial class DororongPresenter
{
    internal void RebaseSizePresentation(Dororong.Core.Geometry.PointD offset)
    {
        // Foot anchoring is not walking. Do not add this translation to gait distance.
        if (_locomotionPosition is { } previous) _locomotionPosition = previous + offset;
    }
    private bool _sizeChanged;
    internal void SetArtSize(int percent)
    {
        if (percent is < 100 or > 150) throw new ArgumentOutOfRangeException(nameof(percent));
        var scale = percent / 100d;
        if (HighResolutionGrabEnabled && HighResolutionScale == scale) return;
        HighResolutionGrabEnabled = true;
        HighResolutionScale = scale;
        Width = Height = 144 * scale;
        ((Canvas)Content).Width = ((Canvas)Content).Height = 144 * scale;
        // A different size invalidates frame contents, not their storage.
        // Active + spare slots remain bounded by the existing 12-frame limit.
        foreach (var frame in _hrFrames.Values) _hrSpareFrames.Push(frame);
        _hrFrames.Clear(); _hrFrameOrder.Clear();
        _hrSoles.Clear(); _hrSoleOrder.Clear();
        _sizeChanged = true;
    }
}

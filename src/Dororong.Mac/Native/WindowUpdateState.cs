using Dororong.Core.Geometry;

namespace Dororong.Mac.Native;

// One instance per owned overlay, no unbounded native-window dictionary.
internal sealed class WindowUpdateState
{
    private PointD? position;
    private bool? clickThrough;
    internal bool PositionChanged(PointD value)
    {
        if(position==value)return false;
        position=value;return true;
    }
    internal bool ClickThroughChanged(bool value)
    {
        if(clickThrough==value)return false;
        clickThrough=value;return true;
    }
    internal void InvalidatePosition()=>position=null;
}

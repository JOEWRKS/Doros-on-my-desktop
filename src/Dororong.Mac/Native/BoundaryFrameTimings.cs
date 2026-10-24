namespace Dororong.Mac.Native;

// Diagnostic-only maxima, not a retained per-frame history or GPU measurement.
internal sealed class BoundaryFrameTimings
{
    private int count;
    private double interval,reads,controller,upload,move,total,between;
    private double? previousTotal;
    internal void Record(double intervalMs,double readsMs,double controllerMs,double uploadMs,double moveMs,double totalMs)
    {
        count++;interval=Math.Max(interval,intervalMs);reads=Math.Max(reads,readsMs);
        controller=Math.Max(controller,controllerMs);upload=Math.Max(upload,uploadMs);
        move=Math.Max(move,moveMs);total=Math.Max(total,totalMs);
        if(previousTotal is {} previous)between=Math.Max(between,Math.Max(0,intervalMs-previous));
        previousTotal=totalMs;
    }
    internal sealed record Snapshot(int Frames,double MaxIntervalMs,double MaxNativeReadsMs,
        double MaxControllerMs,double MaxUploadMs,double MaxMoveMs,double MaxHandlerMs,double MaxBetweenHandlersMs);
    internal Snapshot Take()
    {
        var result=new Snapshot(count,interval,reads,controller,upload,move,total,between);
        count=0;interval=reads=controller=upload=move=total=between=0;return result;
    }
}

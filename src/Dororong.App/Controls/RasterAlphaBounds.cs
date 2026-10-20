using System.Runtime.CompilerServices;

namespace Dororong.App.Controls;

// Raw occupied source bounds do not depend on pose/scale. Weak keys do not
// retain evicted source banks. Mutable scratch must explicitly invalidate.
internal static class RasterAlphaBounds
{
    internal sealed record Box(double Left,double Top,double Right,double Bottom);
    private sealed class Entry(Box bounds){internal Box Bounds=bounds;internal bool Valid=true;}
    private static readonly ConditionalWeakTable<float[],Entry> cache=new();
    internal static void Invalidate(float[] pixels){if(cache.TryGetValue(pixels,out var entry))entry.Valid=false;}
    internal static Box Get(float[] pixels,int size){
        if(cache.TryGetValue(pixels,out var entry)&&entry.Valid)return entry.Bounds;
        double l=size,r=0,t=size,b=0;
        for(var y=0;y<size;y++)for(var x=0;x<size;x++)if(pixels[(y*size+x)*4+3]>0){
            l=Math.Min(l,x);r=Math.Max(r,x);t=Math.Min(t,y);b=Math.Max(b,y);
        }
        var box=new Box(l,t,r,b);
        if(entry is null)cache.Add(pixels,new(box));else{entry.Bounds=box;entry.Valid=true;}
        return box;
    }
}

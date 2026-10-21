using System.Windows.Media;
using Dororong.Core.Geometry;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    // Mac owns raw pixels instead of a WPF BitmapSource. Render directly into
    // that retained frame, then restore the separate scratch used by sole queries.
    // A sole query must never overwrite the frame currently owned by the surface.
    // Incremental mode is opt-in for the private presenter: the caller must not
    // mutate retained pixels between calls. Public R104Raster keeps full rendering.
    internal byte[] RenderOwnedPixels(double progress,Matrix inverse,PointD grab,PointD near,PointD far,
        double spread,double scale,HighResolutionFacePose face,double bodyScale,byte[]? reuse,bool incremental=false)
    {
        var extent=checked((int)Math.Ceiling(256*scale));var length=checked(extent*extent*4);
        byte[] owned;
        if(reuse is not null&&reuse.Length>=length)owned=reuse;
        else{owned=new byte[length];OwnedFrameAllocations++;}
        var key=new OwnedKey(progress,inverse,grab,near,far,spread,scale,face,bodyScale);
        var region=incremental&&_ownedKey is {} previous&&_ownedReference.TryGetTarget(out var old)&&ReferenceEquals(old,owned)
            ?ChangedRegion(previous,key,extent):null;
        _ownedKey=null;LastDirtyRegion=region;
        if(region is {IsEmpty:true}){
            LastRenderedPixelCount=0;LastCost=(0,0,0);_ownedKey=key;return owned;
        }
        var scratch=outputBuffer;
        try{
            outputBuffer=owned;_renderRegion=region;
            var result=RenderPixels(progress,inverse,grab,near,far,spread,scale,face,false,2,bodyScale);
            if(incremental){_ownedReference.SetTarget(result);_ownedKey=key;}
            return result;
        }finally{outputBuffer=scratch;_renderRegion=null;}
    }
}

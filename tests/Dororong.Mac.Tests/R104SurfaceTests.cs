using Avalonia;
using Avalonia.Headless.XUnit;
using Dororong.Mac.Rendering;
using Dororong.Raster;

namespace Dororong.Mac.Tests;

public sealed class R104SurfaceTests
{
    [AvaloniaFact]
    public void SizeSweepsKeepOneBitmapAndFixedHostExtent()
    {
        using var surface=new R104Surface();
        foreach(var size in new[]{1d,1.125,1.25,1.375,1.5,1.25,1})
        {
            surface.Present(new(),size,1);
            Assert.Equal(R104Raster.Extent(size),surface.ActiveExtent);
            Assert.Equal(384,surface.Width);Assert.Equal(384,surface.Height);
        }
        Assert.Equal(1,surface.BitmapAllocations);
    }

    [AvaloniaFact]
    public void DpiChangeReplacesOneSurfaceAndAlphaUsesActiveExtentOnly()
    {
        using var surface=new R104Surface();
        surface.Present(new(),1.5,1);
        surface.Present(new(),1,2);
        Assert.Equal(2,surface.BitmapAllocations);
        Assert.Equal(512,surface.ActiveExtent);
        Assert.False(surface.HitOpaque(new(300,300)));
        var bytes=surface.ActivePixels.Span;var extent=surface.ActiveExtent;
        var pixel=-1;
        for(var i=0;i<extent*extent;i++)if(bytes[i*4+3]>=16){pixel=i;break;}
        Assert.True(pixel>=0);
        var point=new Point((pixel%extent+.5)/2,(pixel/extent+.5)/2);
        Assert.True(surface.HitOpaque(point));
        surface.Origin=new(5,8);
        Assert.True(surface.HitOpaque(point+new Vector(5,8)));
    }

    [AvaloniaFact]
    public void DisposalDropsPixelOwnershipAndRefusesRendering()
    {
        var surface=new R104Surface();surface.Present(new(),1,1);surface.Dispose();surface.Dispose();
        Assert.Equal(0,surface.ActiveExtent);Assert.True(surface.ActivePixels.IsEmpty);
        Assert.False(surface.HitOpaque(new(120,120)));
        Assert.Throws<ObjectDisposedException>(()=>surface.Present(new(),1,1));
    }
}

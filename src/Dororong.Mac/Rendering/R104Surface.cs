using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Dororong.Raster;

namespace Dororong.Mac.Rendering;

// Fixed-capacity display primitive for the r104 port. Not yet wired to the
// legacy DesktopSession: the accepted input/pose coordinator must move first.
internal sealed class R104Surface : Control, IDisposable
{
    private R104Raster? raster;
    private WriteableBitmap? bitmap;
    private byte[] pixels = [];
    private RasterPose? lastPose;
    private double size, density;
    private Point origin;
    private bool disposed;
    internal int BitmapAllocations { get; private set; }
    internal int ActiveExtent { get; private set; }
    internal ReadOnlyMemory<byte> ActivePixels => pixels.AsMemory(0,ActiveExtent*ActiveExtent*4);
    internal Point Origin
    {
        get=>origin;
        set { Dispatcher.UIThread.VerifyAccess(); if(origin==value)return;origin=value;InvalidateVisual(); }
    }

    internal R104Surface()
    {
        // A size change changes the draw rectangle, never this control's size.
        Width=Height=384;
        RenderOptions.SetBitmapInterpolationMode(this,BitmapInterpolationMode.None);
    }

    internal void Present(RasterPose pose,double productScale,double displayScale)
    {
        Dispatcher.UIThread.VerifyAccess();
        ObjectDisposedException.ThrowIf(disposed,this);
        var extent=R104Raster.Extent(productScale,displayScale);
        if(lastPose==pose&&size==productScale&&density==displayScale)return;
        raster??=new();
        pixels=raster.Render(pose,productScale,pixels,displayScale);
        if(bitmap is null||density!=displayScale)
        {
            var maximum=R104Raster.Extent(1.5,displayScale);
            var replacement=new WriteableBitmap(new(maximum,maximum),new(96,96),PixelFormat.Bgra8888,AlphaFormat.Premul);
            var previous=bitmap;bitmap=replacement;previous?.Dispose();BitmapAllocations++;
        }
        using(var framebuffer=bitmap.Lock())
            for(var y=0;y<extent;y++)
                Marshal.Copy(pixels,y*extent*4,framebuffer.Address+y*framebuffer.RowBytes,extent*4);
        size=productScale;density=displayScale;ActiveExtent=extent;lastPose=pose;
        InvalidateVisual();
    }

    internal bool HitOpaque(Point point)
    {
        if(disposed||bitmap is null)return false;
        var x=(point.X-Origin.X)*density;var y=(point.Y-Origin.Y)*density;
        if(!double.IsFinite(x)||!double.IsFinite(y)||x<0||y<0||x>=ActiveExtent||y>=ActiveExtent)return false;
        return pixels[((int)y*ActiveExtent+(int)x)*4+3]>=16;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if(bitmap is not null&&!disposed)
            context.DrawImage(bitmap,new Rect(0,0,ActiveExtent,ActiveExtent),
                new Rect(Origin,new Size(ActiveExtent/density,ActiveExtent/density)));
    }

    public void Dispose()
    {
        Dispatcher.UIThread.VerifyAccess();
        if(disposed)return;
        disposed=true;bitmap?.Dispose();bitmap=null;raster=null;pixels=[];lastPose=null;ActiveExtent=0;
        InvalidateVisual();
    }
}

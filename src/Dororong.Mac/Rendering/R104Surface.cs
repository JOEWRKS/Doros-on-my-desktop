using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Dororong.Raster;

namespace Dororong.Mac.Rendering;

// Fixed-capacity display for DesktopSession's r104 coordinator. Native Mac
// composition and Retina execution remain separate verification gates.
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
        Upload(pixels,productScale,displayScale);
        lastPose=pose;
    }

    internal void Upload(byte[] source,double productScale,double displayScale)
    {
        Dispatcher.UIThread.VerifyAccess();ObjectDisposedException.ThrowIf(disposed,this);
        var extent=R104Raster.Extent(productScale,displayScale);
        if(source.Length<extent*extent*4)throw new ArgumentException("Active pixels required.");
        pixels=source;
        if(bitmap is null||density!=displayScale)
        {
            var maximum=R104Raster.Extent(1.5,displayScale);
            var replacement=new WriteableBitmap(new(maximum,maximum),new(96,96),PixelFormat.Bgra8888,AlphaFormat.Premul);
            var previous=bitmap;bitmap=replacement;previous?.Dispose();BitmapAllocations++;
        }
        using(var framebuffer=bitmap.Lock())
            CopyPixels(pixels,extent,framebuffer.Address,framebuffer.RowBytes);
        size=productScale;density=displayScale;ActiveExtent=extent;lastPose=null;
        InvalidateVisual();
    }

    internal static void CopyPixels(byte[] source,int extent,nint destination,int stride)
    {
        var rowBytes=checked(extent*4);
        if(stride==rowBytes)Marshal.Copy(source,0,destination,checked(rowBytes*extent));
        else for(var y=0;y<extent;y++)
            Marshal.Copy(source,y*rowBytes,destination+y*stride,rowBytes);
    }

    internal bool HitOpaque(Point point)
    {
        if(disposed||bitmap is null)return false;
        var ratio=ActiveExtent/(256*size);
        var x=(point.X-Origin.X)*ratio;var y=(point.Y-Origin.Y)*ratio;
        if(!double.IsFinite(x)||!double.IsFinite(y)||x<0||y<0||x>=ActiveExtent||y>=ActiveExtent)return false;
        return pixels[((int)y*ActiveExtent+(int)x)*4+3]>=16;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if(bitmap is not null&&!disposed)
            context.DrawImage(bitmap,new Rect(0,0,ActiveExtent,ActiveExtent),
                new Rect(Origin,new Size(256*size,256*size)));
    }

    public void Dispose()
    {
        Dispatcher.UIThread.VerifyAccess();
        if(disposed)return;
        disposed=true;bitmap?.Dispose();bitmap=null;raster=null;pixels=[];lastPose=null;ActiveExtent=0;
        InvalidateVisual();
    }
}

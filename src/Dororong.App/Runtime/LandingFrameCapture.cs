using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Security.Cryptography;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dororong.Core.Platforms;

namespace Dororong.App.Runtime;

internal readonly record struct LandingFrameStamp(double Ms, double WorkMs,
    PlatformPose? Pose, double RenderedSpread, double? WindowLeft, double? WindowTop,
    double? ImageOffsetX=null,double? ImageOffsetY=null);

internal enum FrameCaptureTrigger { StrongLanding, FirstFall }

// Explicit diagnostic only: copies the pet's CPU-side bitmap, never the desktop.
// One armed strong landing, bounded storage, no compression or disk IO per tick.
internal sealed class LandingFrameCapture
{
    internal const int BeforeCount = 16;
    internal const long ByteBudget = 64L * 1024 * 1024;
    private readonly int width, height, stride, capacity;
    private readonly double dpiX, dpiY;
    private byte[][] buffers;
    private readonly Sample[] samples;
    private int beforeWrite, beforeCount, afterCount;
    private bool armed, sawFall, triggered, sealedCapture;
    private double impactMs;
    private string? error;
    private string stopReason="capture-ended";
    private Task<string?>? export;
    private readonly FrameCaptureTrigger trigger;
    private readonly record struct Sample(LandingFrameStamp Stamp, long CopyStartTicks, long CopyEndTicks);
    internal string OutputDirectory { get; }
    internal long BufferBytes => (long)buffers.Length * stride * height;
    internal bool Complete => sealedCapture;
    internal bool Recording => armed && !sealedCapture && export is null;
    internal int SavedFrameCount => triggered ? beforeCount + afterCount : 0;

    internal LandingFrameCapture(string root, BitmapSource initial, FrameCaptureTrigger trigger=FrameCaptureTrigger.StrongLanding)
    {
        this.trigger=trigger;
        width=initial.PixelWidth; height=initial.PixelHeight; stride=checked(width*4);
        dpiX=initial.DpiX; dpiY=initial.DpiY;
        var bytes=checked(stride*height);
        capacity=(int)Math.Min(96,ByteBudget/bytes);
        if(initial.Format!=PixelFormats.Pbgra32 || capacity<=BeforeCount || bytes<=0)
            throw new ArgumentException("Unsupported bitmap or capture exceeds bounded storage.",nameof(initial));
        var prefix=trigger==FrameCaptureTrigger.FirstFall?"release":"landing";
        OutputDirectory=Path.Combine(Path.GetFullPath(root),$"{prefix}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}");
        buffers=new byte[capacity][];
        for(var i=0;i<capacity;i++) buffers[i]=new byte[bytes];
        samples=new Sample[capacity];
    }

    internal void Arm()
    {
        if(triggered||sealedCapture||export is not null)return;
        armed=true; sawFall=false; beforeWrite=beforeCount=0;
    }

    internal void Observe(BitmapSource? source, LandingFrameStamp stamp)
    {
        if(!armed||sealedCapture||export is not null)return;
        if(source is null || source.Format!=PixelFormats.Pbgra32 || source.PixelWidth!=width || source.PixelHeight!=height
            || source.DpiX!=dpiX || source.DpiY!=dpiY){
            error="Source unavailable or size/DPI changed; capture stopped without resampling.";
            stopReason="source-changed";sealedCapture=true;return;
        }
        sawFall|=stamp.Pose?.Phase==PlatformPhase.Falling;
        var atTrigger=trigger==FrameCaptureTrigger.FirstFall
            ?stamp.Pose?.Phase==PlatformPhase.Falling
            :sawFall&&stamp.Pose is {Phase:PlatformPhase.Landing,IsExtremeLanding:true};
        if(!triggered && atTrigger){
            triggered=true;impactMs=stamp.Ms;
        }
        var slot=triggered?BeforeCount+afterCount:beforeWrite;
        var start=Stopwatch.GetTimestamp();
        try{source.CopyPixels(buffers[slot],stride,0);}
        catch(Exception ex)when(ex is ArgumentException or InvalidOperationException or System.Runtime.InteropServices.COMException){
            error=ex.Message;stopReason="copy-failed";sealedCapture=true;return;
        }
        samples[slot]=new(stamp,start,Stopwatch.GetTimestamp());
        if(triggered){
            afterCount++;
            sealedCapture=BeforeCount+afterCount>=capacity || stamp.Ms-impactMs>=800;
            if(sealedCapture)stopReason=BeforeCount+afterCount>=capacity?"capacity":
                trigger==FrameCaptureTrigger.FirstFall?"post-release-window":"post-impact-window";
        }else{
            beforeCount=Math.Min(BeforeCount,beforeCount+1);
            beforeWrite=(beforeWrite+1)%BeforeCount;
        }
    }

    internal Task<string?> ExportAsync()
    {
        if(export is not null)return export;
        sealedCapture=true;
        // Transfer ownership: the recorder must never touch these bytes again.
        var owned=buffers;buffers=[];
        var order=new List<int>();
        if(triggered){
            for(var i=0;i<beforeCount;i++)order.Add((beforeWrite-beforeCount+i+BeforeCount)%BeforeCount);
            for(var i=0;i<afterCount;i++)order.Add(BeforeCount+i);
        }
        var rows=order.Select((slot,index)=>new{
            file=$"{index:D3}.png",slot,relativeToImpactMs=samples[slot].Stamp.Ms-impactMs,
            stamp=samples[slot].Stamp,
            copyStartTicks=samples[slot].CopyStartTicks,copyEndTicks=samples[slot].CopyEndTicks,
            copyMs=(samples[slot].CopyEndTicks-samples[slot].CopyStartTicks)*1000d/Stopwatch.Frequency
        }).ToArray();
        export=Task.Run<string?>(()=>{
            try{
                Directory.CreateDirectory(OutputDirectory);
                var exported=new List<object>();
                foreach(var row in rows){
                    var bitmap=BitmapSource.Create(width,height,dpiX,dpiY,PixelFormats.Pbgra32,null,owned[row.slot],stride);
                    bitmap.Freeze();var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using var image=File.Create(Path.Combine(OutputDirectory,row.file));encoder.Save(image);
                    exported.Add(new{row.file,row.relativeToImpactMs,relativeToTriggerMs=row.relativeToImpactMs,row.stamp,row.copyStartTicks,row.copyEndTicks,row.copyMs,
                        sourcePbgra32Sha256=Convert.ToHexString(SHA256.HashData(owned[row.slot]))});
                }
                using var manifest=File.Create(Path.Combine(OutputDirectory,"frames.json"));
                JsonSerializer.Serialize(manifest,new{
                    evidence="CPU pet bitmap at end of update; NOT screen capture, DWM presentation or GPU scan-out.",
                    trigger=trigger.ToString(),triggered,error,stopReason,width,height,dpiX,dpiY,capacity,bufferBytes=(long)capacity*stride*height,
                    stopwatchFrequency=Stopwatch.Frequency,impactMs,frames=exported
                },new JsonSerializerOptions{WriteIndented=true});
                return OutputDirectory;
            }catch(Exception ex)when(ex is IOException or UnauthorizedAccessException or InvalidOperationException or System.Runtime.InteropServices.COMException){
                Debug.WriteLine($"Landing frame export failed: {ex}");return null;
            }finally{Array.Clear(owned);}
        });
        return export;
    }
}

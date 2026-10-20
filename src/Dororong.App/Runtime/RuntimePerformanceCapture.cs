using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Dororong.App.Controls;

namespace Dororong.App.Runtime;

// Explicit local diagnostics only. No recording without --performance-capture.
// WPF rendering callbacks are not a measurement of monitor/GPU presentation FPS.
internal sealed class RuntimePerformanceCapture : IDisposable
{
    private readonly string path;
    private readonly DororongPresenter presenter;
    private readonly bool inputHost;
    private readonly Func<object>? snapshot;
    private readonly PetLoop? loop;
    private List<object> ticks=new();
    private List<object> memory=new();
    private readonly DiagnosticMemorySampler memorySampler;
    private List<object> states=new();
    private double lastState;
    private readonly Stopwatch clock=Stopwatch.StartNew();
    private readonly DispatcherTimer deadline=new(){Interval=TimeSpan.FromSeconds(60)};
    private List<object> frames=new(),inputs=new();
    private readonly double cpuStart=Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
    private double previous,previousRender;
    private int previousCount;
    private int previousSoles;
    private double previousPreparation,previousSampling,previousBitmap,previousSoleMs;
    private double previousLayout;
    private double previousWindowMove;
    private int previousLayoutFlushes;
    private double previousWorkerWait=RasterRowScheduler.WaitMilliseconds;
    private int previousGen0=GC.CollectionCount(0),previousGen1=GC.CollectionCount(1),previousGen2=GC.CollectionCount(2);
    private long previousAllocated=GC.GetTotalAllocatedBytes(false);
    private bool disposed;
    private readonly LandingFrameCapture? landingFrames;
    private readonly string? landingFrameError;
    internal Task<string?> FrameExport { get; private set; } = Task.FromResult<string?>(null);
    internal Task CaptureExport { get; private set; } = Task.CompletedTask;
    internal RuntimePerformanceCapture(string path,DororongPresenter presenter,bool inputHost,Func<object>? snapshot=null,PetLoop? loop=null,string? landingFrameDirectory=null,
        FrameCaptureTrigger frameTrigger=FrameCaptureTrigger.StrongLanding){
        this.path=Path.GetFullPath(path);this.presenter=presenter;this.inputHost=inputHost;this.snapshot=snapshot;
        memorySampler=new(clock);
        if(landingFrameDirectory is not null){
            try{
                if(presenter.HighResolutionImage?.Source is BitmapSource initial)landingFrames=new(landingFrameDirectory,initial,frameTrigger);
                else landingFrameError="No high-resolution pet bitmap available at capture start.";
            }catch(Exception ex)when(ex is ArgumentException or OverflowException){landingFrameError=ex.Message;}
        }
        this.loop=loop;if(loop is not null)loop.TickMeasured+=OnTick;
        const string durationPrefix="--performance-capture-seconds=";
        var duration=Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith(durationPrefix,StringComparison.Ordinal));
        if(duration is not null&&int.TryParse(duration[durationPrefix.Length..],out var seconds)&&seconds is >=30 and <=300)
            deadline.Interval=TimeSpan.FromSeconds(seconds);
        CompositionTarget.Rendering+=OnRendering;deadline.Tick+=OnDeadline;deadline.Start();
    }
    internal void Input(string target,double? workMs=null){if(!disposed){inputs.Add(new{ms=clock.Elapsed.TotalMilliseconds,target,workMs});if(target=="Body")landingFrames?.Arm();}}
    private void OnTick(double intervalMs,double workMs){
        if(disposed)return;
        var pose=presenter.LastRequestedPlatformPose;
        // End-of-update CPU-side presentation values, not GPU scan-out proof.
        // Unlike the 100ms state samples these preserve the first impact tick.
        ticks.Add(new{ms=clock.Elapsed.TotalMilliseconds,intervalMs,workMs,
            platformPhase=pose is {} p?(int)p.Phase:-1,
            extreme=pose?.IsExtremeLanding??false,
            requestedSpread=pose?.LegSpread??0,renderedSpread=presenter.LastRenderedSpread});
        if(landingFrames is {Recording:true}){
            var window=Window.GetWindow(presenter);
            var offset=window is not null&&presenter.HighResolutionImage is {} image
                ?image.TranslatePoint(new Point(),window):(Point?)null;
            landingFrames.Observe(presenter.HighResolutionImage?.Source as BitmapSource,
                new(clock.Elapsed.TotalMilliseconds,workMs,pose,presenter.LastRenderedSpread,window?.Left,window?.Top,offset?.X,offset?.Y));
        }
    }
    private void OnRendering(object? sender,EventArgs e){
        var diagnosticStarted=Stopwatch.GetTimestamp();
        var now=clock.Elapsed.TotalMilliseconds;
        var gen0=GC.CollectionCount(0);var gen1=GC.CollectionCount(1);var gen2=GC.CollectionCount(2);
        var allocated=GC.GetTotalAllocatedBytes(false);
        DrainMemory();
        var snapshotStarted=Stopwatch.GetTimestamp();
        if(snapshot is not null&&now-lastState>=100){states.Add(new{ms=now,state=snapshot()});lastState=now;}
        var snapshotMs=Stopwatch.GetElapsedTime(snapshotStarted).TotalMilliseconds;
        var workerWait=RasterRowScheduler.WaitMilliseconds;
        var windowMove=loop?.WindowMoveMilliseconds??0;
        frames.Add(new{ms=now,intervalMs=previous==0?0:now-previous,
            rasterMs=presenter.HighResolutionRenderMilliseconds-previousRender,
            rasters=presenter.HighResolutionRasterizations-previousCount,
            soleQueries=presenter.HighResolutionSoleQueries-previousSoles,
            preparationMs=presenter.HighResolutionPreparationMilliseconds-previousPreparation,
            samplingMs=presenter.HighResolutionSamplingMilliseconds-previousSampling,
            bitmapMs=presenter.HighResolutionBitmapMilliseconds-previousBitmap,
            soleMs=presenter.HighResolutionSoleMilliseconds-previousSoleMs,
            layoutMs=presenter.HighResolutionLayoutMilliseconds-previousLayout,
            layoutFlushes=presenter.HighResolutionLayoutFlushes-previousLayoutFlushes,
            windowMoveMs=windowMove-previousWindowMove,
            workerWaitMs=workerWait-previousWorkerWait,
            gen0=gen0-previousGen0,gen1=gen1-previousGen1,gen2=gen2-previousGen2,
            allocatedBytes=allocated-previousAllocated,threadPoolPending=ThreadPool.PendingWorkItemCount,
            snapshotMs,diagnosticMs=Stopwatch.GetElapsedTime(diagnosticStarted).TotalMilliseconds});
        previous=now;previousRender=presenter.HighResolutionRenderMilliseconds;previousCount=presenter.HighResolutionRasterizations;
        previousSoles=presenter.HighResolutionSoleQueries;previousPreparation=presenter.HighResolutionPreparationMilliseconds;
        previousSampling=presenter.HighResolutionSamplingMilliseconds;previousBitmap=presenter.HighResolutionBitmapMilliseconds;
        previousSoleMs=presenter.HighResolutionSoleMilliseconds;
        previousLayout=presenter.HighResolutionLayoutMilliseconds;previousLayoutFlushes=presenter.HighResolutionLayoutFlushes;
        previousWindowMove=windowMove;
        previousWorkerWait=workerWait;
        previousGen0=gen0;previousGen1=gen1;previousGen2=gen2;previousAllocated=allocated;
    }
    private void OnDeadline(object? sender,EventArgs e)=>Dispose();
    private void DrainMemory(){while(memorySampler.TryTake(out var sample))memory.Add(sample!);}
    public void Dispose(){
        if(disposed)return;disposed=true;deadline.Stop();deadline.Tick-=OnDeadline;CompositionTarget.Rendering-=OnRendering;
        if(loop is not null)loop.TickMeasured-=OnTick;
        DrainMemory();memorySampler.Dispose();
        var elapsedMs=clock.Elapsed.TotalMilliseconds;
        var savedFrames=frames;var savedTicks=ticks;var savedInputs=inputs;var savedStates=states;var savedMemory=memory;
        frames=new();ticks=new();inputs=new();states=new();memory=new();
        var landingInfo=landingFrames is null?null:new{landingFrames.OutputDirectory,landingFrames.BufferBytes,landingFrames.SavedFrameCount,
            note="Diagnostic pixel-copy overhead and bounded preallocated buffers included; not hardware presentation."};
        if(landingFrames is not null)FrameExport=landingFrames.ExportAsync();
        // Hand off owned sample lists. Serialization and process queries must
        // not block the live UI when the recording deadline fires.
        CaptureExport=Task.Run(()=>{
        using var process=Process.GetCurrentProcess();
        var result=new{inputHost,elapsedMs,
            cpuMs=process.TotalProcessorTime.TotalMilliseconds-cpuStart,
            memorySchema="r90: samples contain ms/queryMs/value/error; process queries are off the UI thread",
            cpuNote="End CPU queried by export worker after capture detach; may include export dispatch delay.",
            note="frames: WPF callback cadence, not hardware presentation FPS. ticks: actual simulation/update start interval and work cost. memory: once per second, not forced GC. Only explicitly marked native inputs are evidence of input delivery.",
            landingFrameError,landingFrameCapture=landingInfo,
            frames=savedFrames,ticks=savedTicks,inputs=savedInputs,states=savedStates,memory=savedMemory};
        // Write once after capture, so per-frame timing has no disk IO.
        try{
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            // Serialize into a bounded stream buffer. Building the complete
            // UTF-16 JSON string duplicated the multi-megabyte capture at stop,
            // making diagnostic shutdown itself look like a new memory spike.
            using var stream=new FileStream(path,FileMode.Create,FileAccess.Write,FileShare.Read);
            JsonSerializer.Serialize(stream,result,new JsonSerializerOptions{WriteIndented=true});
        }catch(Exception ex)when(ex is IOException or UnauthorizedAccessException){Debug.WriteLine(ex);}
        finally{
            // MainWindow retains the disposed capture until exit. Release the
            // thousands of boxed samples now, even if saving failed.
            foreach(var samples in new[]{savedFrames,savedTicks,savedInputs,savedStates,savedMemory}){samples.Clear();samples.TrimExcess();}
        }
        });
    }
}

using System.Collections.Concurrent;
using System.Diagnostics;

namespace Dororong.App.Runtime;

// No WPF state or UI-thread waits. A slow OS process query cannot stall a frame.
// At most one query runs at once; only two undrained samples can be retained.
internal sealed class DiagnosticMemorySampler : IDisposable
{
    private readonly ConcurrentQueue<object> pending=new();
    private readonly CancellationTokenSource stop=new();
    internal Task Completion { get; }
    internal DiagnosticMemorySampler(Stopwatch clock,Func<object>? read=null){
        var token=stop.Token;
        Completion=Task.Run(async()=>{
            try{
                while(!token.IsCancellationRequested){
                    var started=clock.Elapsed.TotalMilliseconds;
                    object? value=null;string? error=null;
                    try{value=(read??ReadProcess)();}
                    catch(Exception ex)when(ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException){error=ex.GetType().Name;}
                    var ended=clock.Elapsed.TotalMilliseconds;
                    if(token.IsCancellationRequested)break;
                    pending.Enqueue(new Sample(started,ended-started,value,error));
                    while(pending.Count>2)pending.TryDequeue(out _);
                    await Task.Delay(1000,token).ConfigureAwait(false);
                }
            }catch(OperationCanceledException)when(token.IsCancellationRequested){}
            finally{while(pending.TryDequeue(out _)){} stop.Dispose();}
        });
    }
    internal sealed record Sample(double ms,double queryMs,object? value,string? error);
    internal bool TryTake(out object? sample)=>pending.TryDequeue(out sample);
    private static object ReadProcess(){
        using var process=Process.GetCurrentProcess();process.Refresh();var gc=GC.GetGCMemoryInfo();
        return new{managedBytes=GC.GetTotalMemory(false),heapBytes=gc.HeapSizeBytes,fragmentedBytes=gc.FragmentedBytes,
            privateBytes=process.PrivateMemorySize64,workingSetBytes=process.WorkingSet64,handles=process.HandleCount};
    }
    public void Dispose(){try{stop.Cancel();}catch(ObjectDisposedException){}}
}

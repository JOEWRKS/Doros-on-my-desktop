using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace Dororong.App.Controls;

// Rendering is synchronous, but must not wait for general ThreadPool worker
// injection. Fixed background workers sleep when idle; the calling UI thread
// also consumes rows. No WPF objects are accessed from a worker.
internal static class RasterRowScheduler
{
    private static readonly int WorkerCount=Math.Min(5,Math.Max(0,Environment.ProcessorCount/2-1));
    private static long waitTicks;
    [ThreadStatic] internal static bool CallerOnly;
    internal static double WaitMilliseconds=>Interlocked.Read(ref waitTicks)*1000d/Stopwatch.Frequency;
    internal sealed class WorkItem(Action action)
    {
        private Action? pending=action;
        // Clear the queued reference before executing. The consumer may retain
        // its last item while sleeping, but that item no longer owns a raster.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        internal void Run()=>Claim()?.Invoke();
        internal Action? Claim()=>Interlocked.Exchange(ref pending,null);
    }
    private static readonly BlockingCollection<WorkItem> Jobs=new(Math.Max(1,WorkerCount*4));
    static RasterRowScheduler(){
        // GetConsumingEnumerable's iterator retains its last yielded Action
        // while waiting for the next item, rooting that render's whole bank.
        for(var i=0;i<WorkerCount;i++)new Thread(()=>{foreach(var job in Jobs.GetConsumingEnumerable())job.Run();})
            {IsBackground=true,Name=$"Dororong raster {i}"}.Start();
    }
    internal static void For(int start,int end,Action<int> row){
        if(end<=start)return;
        // Sparse sole queries arrive in eight-row bands. Waking five workers
        // and waiting for all of them costs more than this bounded local work,
        // and exposes every tiny band to worker scheduling stalls. Pixel order
        // inside a row is unchanged; large display rasters remain parallel.
        if(CallerOnly||WorkerCount==0||end-start<=8){for(var y=start;y<end;y++)row(y);return;}
        ParallelFor(start,end,row,WorkerCount,static item=>{Jobs.TryAdd(item);});
    }
    // Keep the captured Work closure out of the serial entry method: C# can
    // allocate lifted locals before an early return even if no job is queued.
    internal static void ParallelFor(int start,int end,Action<int> row,int workers,Action<WorkItem> enqueue){
        if(end<=start)return;
        var batch=new RowBatch(start,end,row);
        var items=new WorkItem[workers];
        for(var i=0;i<workers;i++){
            var item=items[i]=new(batch.Work);
            enqueue(item);
        }
        batch.Work();
        // Claim jobs that have not started: there may be no rows left, so
        // waiting for those workers to wake only adds scheduling latency.
        // Active pixel chunks still finish before return.
        // A delayed queue entry retains only an empty WorkItem; a bounded
        // queue also prevents stale entries accumulating behind a stalled OS.
        foreach(var item in items)item.Run();
        var waiting=Stopwatch.GetTimestamp();
        batch.Wait();
        Interlocked.Add(ref waitTicks,Stopwatch.GetTimestamp()-waiting);
    }

    // Completion belongs to pixel-writing chunks, not worker acknowledgements.
    // A worker descheduled after claiming an empty job must not hold the UI.
    // Running chunks still finish before return; late jobs see an exhausted
    // range and no longer retain the row closure / raster through this batch.
    private sealed class RowBatch(int start,int end,Action<int> row)
    {
        private int next=start;
        private int remaining=(end-start+3)/4;
        private Action<int>? action=row;
        private ExceptionDispatchInfo? error;
        private readonly object gate=new();
        internal void Work(){
            for(;;){
                var first=Interlocked.Add(ref next,4)-4;if(first>=end)return;
                try{
                    if(Volatile.Read(ref error) is null)
                        for(var y=first;y<Math.Min(first+4,end);y++)action!(y);
                }catch(Exception ex){Interlocked.CompareExchange(ref error,ExceptionDispatchInfo.Capture(ex),null);}
                finally{
                    lock(gate){
                        if(--remaining==0){action=null;Monitor.PulseAll(gate);}
                    }
                }
            }
        }
        internal void Wait(){
            lock(gate){while(remaining!=0)Monitor.Wait(gate);}
            error?.Throw();
        }
    }
}

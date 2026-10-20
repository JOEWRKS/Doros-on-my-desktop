using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class RenderWaitIsolationTests
{
    [Fact]
    public void Late_empty_jobs_do_not_hold_completed_pixel_buffers(){
        var (weak,jobs)=CreateCompletedBatch();
        for(var i=0;i<3&&weak.IsAlive;i++){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
        Assert.False(weak.IsAlive);
        foreach(var job in jobs)job();
        GC.KeepAlive(jobs);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static (WeakReference,List<Action>) CreateCompletedBatch(){
        var pixels=new byte[1_000_000];var jobs=new List<Action>();
        RasterRowScheduler.ParallelFor(0,128,i=>pixels[i]=255,2,item=>jobs.Add(item.Claim()!));
        return(new WeakReference(pixels),jobs);
    }
    [Fact]
    public void Caller_finishes_all_pixels_without_waiting_for_claimed_but_descheduled_workers()
    {
        var claimed=new List<Action>();
        var hits=new int[128];
        using var queued=new ManualResetEventSlim();
        var render=Task.Run(()=>RasterRowScheduler.ParallelFor(0,hits.Length,i=>Interlocked.Increment(ref hits[i]),2,item=>{
            claimed.Add(item.Claim()!);if(claimed.Count==2)queued.Set();
        }));
        Assert.True(queued.Wait(TimeSpan.FromSeconds(5)));
        bool finished;
        try{finished=render.Wait(TimeSpan.FromMilliseconds(250));}
        finally{foreach(var action in claimed)action();render.GetAwaiter().GetResult();}
        Assert.True(finished,"All rows were done, but the UI still waited for empty worker acknowledgements.");
        Assert.All(hits,n=>Assert.Equal(1,n));
    }

    [Fact]
    public void Active_pixel_writes_must_finish_before_return()
    {
        using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
        var hits=new int[128];
        var render=Task.Run(()=>RasterRowScheduler.ParallelFor(0,hits.Length,i=>{
            if(i==0){entered.Set();release.Wait();}Interlocked.Increment(ref hits[i]);
        },1,item=>{new Thread(item.Run){IsBackground=true}.Start();Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));}));
        Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
        try{Assert.False(render.Wait(100));}finally{release.Set();}
        Assert.True(render.Wait(TimeSpan.FromSeconds(5)));Assert.All(hits,n=>Assert.Equal(1,n));
    }
}

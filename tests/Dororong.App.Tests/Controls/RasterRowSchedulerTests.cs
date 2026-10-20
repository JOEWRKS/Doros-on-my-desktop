using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class RasterRowSchedulerTests
{
    [Fact]
    public void Sparse_eight_row_bands_run_on_caller_without_queue_allocation(){
        var caller=Environment.CurrentManagedThreadId;var count=0;var wrongThread=false;
        Action<int> row=_=>{wrongThread|=caller!=Environment.CurrentManagedThreadId;count++;};
        RasterRowScheduler.For(0,8,row);
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<1000;i++)RasterRowScheduler.For(0,8,row);
        var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        Assert.True(allocated<4096,$"Serial bands allocated {allocated} bytes.");
        Assert.False(wrongThread);
        Assert.Equal(8008,count);
        Assert.Throws<InvalidOperationException>(()=>RasterRowScheduler.For(0,8,_=>throw new InvalidOperationException()));
    }
    [Fact]
    public void Every_row_runs_once_and_empty_ranges_do_nothing(){
        var hits=new int[513];RasterRowScheduler.For(3,510,i=>Interlocked.Increment(ref hits[i]));
        for(var i=0;i<hits.Length;i++)Assert.Equal(i>=3&&i<510?1:0,hits[i]);
        RasterRowScheduler.For(5,5,_=>throw new Exception());
    }
    [Fact]
    public void Exceptions_reach_caller_and_workers_remain_usable(){
        Assert.Throws<InvalidOperationException>(()=>RasterRowScheduler.For(0,127,i=>{if(i==23)throw new InvalidOperationException("sample");}));
        var count=0;RasterRowScheduler.For(0,127,_=>Interlocked.Increment(ref count));Assert.Equal(127,count);
    }
    [Fact]
    public void Concurrent_callers_do_not_share_row_ownership(){
        Parallel.For(0,12,_=>{var hits=new int[97];RasterRowScheduler.For(0,hits.Length,i=>Interlocked.Increment(ref hits[i]));Assert.All(hits,n=>Assert.Equal(1,n));});
    }
}

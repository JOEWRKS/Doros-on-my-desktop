using System.Reflection;
using Dororong.App.Controls;
using Dororong.App.Interop;

namespace Dororong.App.Tests.Controls;

public sealed class RenderReuseRegressionTests
{
    [Fact]
    public void Completed_rows_do_not_wait_for_workers_that_have_not_started(){
        var delayed=new List<RasterRowScheduler.WorkItem>();var hits=new int[129];
        // No worker ever starts until AFTER the synchronous call returns.
        RasterRowScheduler.ParallelFor(0,hits.Length,i=>hits[i]++,5,delayed.Add);
        Assert.All(hits,n=>Assert.Equal(1,n));
        foreach(var job in delayed)job.Run();
        Assert.All(hits,n=>Assert.Equal(1,n));
    }
    [Fact]
    public async Task Started_worker_finishes_before_return_even_if_caller_steals_other_jobs(){
        using var started=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
        var hits=new int[100];var workers=new List<Thread>();var queued=0;
        var rendering=Task.Run(()=>RasterRowScheduler.ParallelFor(0,hits.Length,i=>{
            if(Thread.CurrentThread.Name=="blocked raster test"&&i==0){started.Set();release.Wait();}
            Interlocked.Increment(ref hits[i]);
        },5,item=>{
            if(Interlocked.Increment(ref queued)!=1)return;
            var thread=new Thread(item.Run){IsBackground=true,Name="blocked raster test"};workers.Add(thread);thread.Start();
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
        }));
        try{
            Assert.True(started.Wait(TimeSpan.FromSeconds(5)));
            Assert.NotSame(rendering,await Task.WhenAny(rendering,Task.Delay(50)));
        }finally{release.Set();}
        await rendering.WaitAsync(TimeSpan.FromSeconds(5));
        foreach(var worker in workers)Assert.True(worker.Join(5000));
        Assert.All(hits,n=>Assert.Equal(1,n));
    }
    [Fact]
    public void Failed_caller_drains_unstarted_jobs_without_late_access(){
        var delayed=new List<RasterRowScheduler.WorkItem>();var calls=0;
        Assert.Throws<InvalidOperationException>(()=>RasterRowScheduler.ParallelFor(0,100,i=>{
            calls++;if(i==0)throw new InvalidOperationException();
        },5,delayed.Add));
        var after=calls;foreach(var job in delayed)job.Run();Assert.Equal(after,calls);
    }
    [Fact]
    public void Scratch_ink_keeps_one_weak_entry_across_invalidation_and_scale_changes(){
        var source=new float[16*16*4];Array.Fill(source,255f);
        var first=HighResolutionBodyInk.AtScale(source,16,1.5);
        var table=typeof(HighResolutionBodyInk).GetField("cache",BindingFlags.Static|BindingFlags.NonPublic)!.GetValue(null)!;
        var lookup=table.GetType().GetMethod("TryGetValue")!;
        object Entry(){object?[] args=[source,null];Assert.True((bool)lookup.Invoke(table,args)!);return args[1]!;}
        var entry=Entry();
        for(var i=0;i<50;i++){
            source[0]=i;HighResolutionBodyInk.Invalidate(source);
            var scale=i%2==0?1.25:1.5;
            var result=HighResolutionBodyInk.AtScale(source,16,scale);
            Assert.Same(entry,Entry());Assert.Same(first,result);
            var fresh=(float[])source.Clone();
            Assert.Equal(HighResolutionBodyInk.AtScale(fresh,16,scale),result);
        }
    }
    [Fact]
    public void Window_class_scratch_is_cleared_and_thread_local(){
        var first=DesktopMetadataReader.GetClassNameBuffer();first.Append("old class");
        Assert.Same(first,DesktopMetadataReader.GetClassNameBuffer());Assert.Equal(0,first.Length);
        System.Text.StringBuilder? other=null;
        var thread=new Thread(()=>other=DesktopMetadataReader.GetClassNameBuffer());thread.Start();Assert.True(thread.Join(5000));
        Assert.NotSame(first,other);
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var i=0;i<10000;i++)DesktopMetadataReader.GetClassNameBuffer().Append("x");
        Assert.InRange(GC.GetAllocatedBytesForCurrentThread()-before,0,1024);
    }
}

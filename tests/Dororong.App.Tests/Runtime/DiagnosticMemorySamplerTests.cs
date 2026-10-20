using System.Diagnostics;
using Dororong.App.Runtime;

namespace Dororong.App.Tests.Runtime;

public sealed class DiagnosticMemorySamplerTests
{
    [Fact]
    public async Task Slow_query_does_not_block_construction_poll_or_disposal_and_cannot_publish_after_stop(){
        using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
        var caller=Environment.CurrentManagedThreadId;var reader=caller;
        var sampler=new DiagnosticMemorySampler(Stopwatch.StartNew(),()=>{
            reader=Environment.CurrentManagedThreadId;entered.Set();release.Wait();return new byte[100_000];
        });
        Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
        try{
            Assert.NotEqual(caller,reader);Assert.False(sampler.TryTake(out _));
            sampler.Dispose();Assert.False(sampler.Completion.IsCompleted);
        }finally{release.Set();}
        await sampler.Completion.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.False(sampler.TryTake(out _));sampler.Dispose();
    }

    [Fact]
    public async Task Query_failures_are_explicit_samples_not_zero_memory(){
        using var sampler=new DiagnosticMemorySampler(Stopwatch.StartNew(),()=>throw new InvalidOperationException());
        object? sample=null;
        for(var i=0;i<100&&!sampler.TryTake(out sample);i++)await Task.Delay(10);
        var value=Assert.IsType<DiagnosticMemorySampler.Sample>(sample);
        Assert.Equal(nameof(InvalidOperationException),value.error);Assert.Null(value.value);
        Assert.True(value.queryMs>=0);
        sampler.Dispose();await sampler.Completion.WaitAsync(TimeSpan.FromSeconds(5));
    }
}

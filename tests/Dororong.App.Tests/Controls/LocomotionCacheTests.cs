using System.Reflection;
using Dororong.App.Controls;

namespace Dororong.App.Tests.Controls;

public sealed class LocomotionCacheTests
{
    [Theory]
    [InlineData(225,32)]
    [InlineData(658,48)]
    public void Complete_repeated_gait_reuses_immutable_sources(int first,int count){
        var bank=new HighResolutionLocomotion();var frames=Enumerable.Range(first,count).Select(bank.Get).ToArray();
        var before=GC.GetAllocatedBytesForCurrentThread();
        for(var lap=0;lap<3;lap++)for(var i=0;i<count;i++)Assert.Same(frames[i],bank.Get(first+i));
        Assert.InRange(GC.GetAllocatedBytesForCurrentThread()-before,0,65536);
        for(var i=1;i<=200;i++)bank.Get(i);
        var cache=(System.Collections.IDictionary)typeof(HighResolutionLocomotion).GetField("cache",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(bank)!;
        Assert.InRange(cache.Count,1,64);
        // Eviction never overwrites a layer already borrowed by the renderer.
        var fresh=new HighResolutionLocomotion();for(var i=0;i<count;i++)Assert.True(frames[i].SequenceEqual(fresh.Get(first+i)));
    }
}

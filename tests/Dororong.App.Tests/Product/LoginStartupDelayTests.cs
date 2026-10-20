using System.Diagnostics;
using Dororong.App.Product;
namespace Dororong.App.Tests.Product;

public sealed class LoginStartupDelayTests
{
    private static string Name()=>@"Local\JOEWRKS.Dororong.DelayTest."+Guid.NewGuid().ToString("N");
    [Fact] public async Task Production_login_delay_is_twenty_seconds_and_manual_has_no_wait()
    {
        Assert.Equal(TimeSpan.FromSeconds(20),LoginStartupDelay.Delay);
        var gate=new LoginStartupDelay(Name(),LoginStartupDelay.Delay);
        var manual=gate.WaitAsync([]);Assert.True(manual.IsCompletedSuccessfully);Assert.True(await manual);
        var timer=Stopwatch.StartNew();Assert.True(await gate.WaitAsync([LoginStartupDelay.Argument]));
        Assert.True(timer.Elapsed>=TimeSpan.FromSeconds(19.9));
    }
    [Fact] public async Task Manual_launch_cancels_all_pending_login_launches_without_waiting()
    {
        var name=Name();var gate=new LoginStartupDelay(name,TimeSpan.FromSeconds(20));
        var first=gate.WaitAsync([LoginStartupDelay.Argument]);var second=gate.WaitAsync([LoginStartupDelay.Argument]);
        Assert.False(first.IsCompleted);Assert.False(second.IsCompleted);
        var manual=gate.WaitAsync([]);Assert.True(manual.IsCompletedSuccessfully);Assert.True(await manual);
        Assert.False(await first.WaitAsync(TimeSpan.FromSeconds(3)));Assert.False(await second.WaitAsync(TimeSpan.FromSeconds(3)));
        using var clean=new EventWaitHandle(false,EventResetMode.ManualReset,name);
        Assert.False(clean.WaitOne(0)); // no stale signal after pending processes leave
    }
    [Fact] public async Task Exit_cancellation_ends_wait_without_starting_product()
    {
        using var cts=new CancellationTokenSource();var gate=new LoginStartupDelay(Name(),TimeSpan.FromSeconds(20));
        var task=gate.WaitAsync([LoginStartupDelay.Argument],cts.Token);cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>task.WaitAsync(TimeSpan.FromSeconds(3)));
    }
    [Theory][InlineData("--unknown")][InlineData("--LOGIN-STARTUP")]
    public async Task Unrecognized_argument_does_not_accidentally_delay_manual_launch(string arg)
    {
        var task=new LoginStartupDelay(Name(),LoginStartupDelay.Delay).WaitAsync([arg]);
        Assert.True(task.IsCompletedSuccessfully);Assert.True(await task);
    }
}

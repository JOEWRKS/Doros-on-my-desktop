namespace Dororong.App.Product;

// The wait owns no product mutex, window, tray or motion bank. Manual startup
// cancels pending login launches, then follows the normal immediate startup path.
internal sealed class LoginStartupDelay
{
    internal const string Argument="--login-startup";
    internal static readonly TimeSpan Delay=TimeSpan.FromSeconds(20);
    private readonly string eventName;
    private readonly TimeSpan delay;
    internal LoginStartupDelay():this(ProductIdentity.InstanceName+".CancelLoginDelay",Delay) { }
    internal LoginStartupDelay(string eventName,TimeSpan delay){this.eventName=eventName;this.delay=delay;}
    internal async Task<bool> WaitAsync(string[] args,CancellationToken cancellation=default)
    {
        if(args.Length!=1 || args[0]!=Argument)
        {
            try { using var pending=EventWaitHandle.OpenExisting(eventName);pending.Set(); }
            catch(WaitHandleCannotBeOpenedException) { /* No pending login launch. */ }
            return true;
        }
        using var manual=new EventWaitHandle(false,EventResetMode.ManualReset,eventName);
        var result=await Task.Run(()=>WaitHandle.WaitAny([manual,cancellation.WaitHandle],delay));
        cancellation.ThrowIfCancellationRequested();
        return result==WaitHandle.WaitTimeout;
    }
}

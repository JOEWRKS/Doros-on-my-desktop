using System.Security.Cryptography;
using System.Text;
namespace Dororong.Portable.Desktop;

// Same nonblocking, abandoned-owner recovery contract as the Windows shell.
// Lifetime is lexical around the UI loop, on the acquiring thread.
public sealed class SingleInstanceLease:IDisposable
{
    private readonly Mutex mutex;private readonly int thread=Environment.CurrentManagedThreadId;private bool disposed;
    private SingleInstanceLease(Mutex mutex)=>this.mutex=mutex;
    public static string UserName(string profile)=>"JOEWRKS.Dororong.Mac."+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(profile))))[..24];
    public static SingleInstanceLease? TryAcquire(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);var mutex=new Mutex(false,name);
        try
        {
            bool acquired;try{acquired=mutex.WaitOne(0);}catch(AbandonedMutexException){acquired=true;}
            if(acquired)return new(mutex);mutex.Dispose();return null;
        }
        catch{mutex.Dispose();throw;}
    }
    public void Dispose()
    {
        if(disposed)return;
        if(Environment.CurrentManagedThreadId!=thread)throw new InvalidOperationException("Release the lease on its owner thread.");
        disposed=true;try{mutex.ReleaseMutex();}finally{mutex.Dispose();}
    }
}

using System.Text.Json;
using System.Threading.Channels;

namespace Dororong.Mac.Native;

// Explicit 90-second diagnostic, not a performance benchmark or always-on log.
// Bounded queue; file serialization/writes cannot block the animation thread.
internal sealed class BoundaryDiagnostic : IDisposable
{
    private readonly Channel<object> queue=Channel.CreateBounded<object>(8);
    private TimeSpan? start,last;
    private int samples,dropped;
    private bool closed;
    internal string Path { get; }
    internal Task Completion { get; }
    internal string? Failure { get; private set; }
    internal bool IsRecording=>!closed&&Failure is null;
    internal BoundaryDiagnostic(string path)
    {
        Path=path;
        Completion=Task.Run(async()=>{
            try {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                await using var stream=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);
                await using var writer=new StreamWriter(stream);
                await writer.WriteLineAsync(JsonSerializer.Serialize(new{kind="start",schema=2,build="r104-test10",pid=Environment.ProcessId,utc=DateTimeOffset.UtcNow,
                    durationSeconds=90,intervalSeconds=.5,scope="placement, geometry and handler timing maxima; not GPU or CPU/memory acceptance"}));
                await writer.FlushAsync();
                await foreach(var item in queue.Reader.ReadAllAsync()){
                    await writer.WriteLineAsync(JsonSerializer.Serialize(item));await writer.FlushAsync();
                }
                await writer.WriteLineAsync(JsonSerializer.Serialize(new{kind="end",reason=endReason,samples,dropped}));
            }catch(Exception e){
                Failure=e.GetType().Name;
                // LaunchServices otherwise hides this failure from the command window.
                // Do not include exception messages (which can contain private data).
                Console.Error.WriteLine($"boundary-error: log-write {Failure}");
            }
        });
    }
    private string endReason="disposed-before-deadline";
    internal void Sample(TimeSpan now,Func<object> read)
    {
        if(closed||Failure is not null)return;
        start??=now;
        if(now-start.Value>=TimeSpan.FromSeconds(90)){endReason="duration-complete";Dispose();return;}
        if(last is {} at&&now-at<TimeSpan.FromMilliseconds(500))return;
        last=now;
        object value;
        try{value=new{kind="sample",elapsed=(now-start.Value).TotalSeconds,data=read()};}
        catch(Exception e){value=new{kind="read-error",elapsed=(now-start.Value).TotalSeconds,error=e.GetType().Name};}
        if(queue.Writer.TryWrite(value))samples++;else dropped++;
    }
    public void Dispose(){if(closed)return;closed=true;queue.Writer.TryComplete();}
}

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
    internal BoundaryDiagnostic(string path)
    {
        Path=path;
        Completion=Task.Run(async()=>{
            try {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                await using var stream=new FileStream(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);
                await using var writer=new StreamWriter(stream);
                await writer.WriteLineAsync(JsonSerializer.Serialize(new{kind="start",schema=1,utc=DateTimeOffset.UtcNow,
                    durationSeconds=90,intervalSeconds=.5,scope="native placement and window geometry only; not CPU/memory acceptance"}));
                await foreach(var item in queue.Reader.ReadAllAsync()){
                    await writer.WriteLineAsync(JsonSerializer.Serialize(item));await writer.FlushAsync();
                }
                await writer.WriteLineAsync(JsonSerializer.Serialize(new{kind="end",reason=endReason,samples,dropped}));
            }catch(Exception e){Failure=e.GetType().Name;}
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

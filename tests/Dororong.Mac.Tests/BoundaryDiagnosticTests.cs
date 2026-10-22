using System.Text.Json;
using Dororong.Mac.Native;

namespace Dororong.Mac.Tests;

public sealed class BoundaryDiagnosticTests
{
    [Fact] public async Task StopsAtNinetySecondsAndDoesNotReadEveryFrame()
    {
        var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"doro-boundary-"+Guid.NewGuid()+".jsonl");
        try {
            using var log=new BoundaryDiagnostic(path);var reads=0;
            for(var ms=0;ms<=92000;ms+=100){log.Sample(TimeSpan.FromMilliseconds(ms),()=>new{number=++reads});if(ms%1000==0)await Task.Delay(1);}
            await log.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(180,reads);Assert.Null(log.Failure);
            var lines=File.ReadAllLines(path);Assert.InRange(lines.Length,3,182);
            using var end=JsonDocument.Parse(lines[^1]);
            Assert.Equal("duration-complete",end.RootElement.GetProperty("reason").GetString());
            Assert.Equal(180,end.RootElement.GetProperty("samples").GetInt32()+end.RootElement.GetProperty("dropped").GetInt32());
        }finally{if(File.Exists(path))File.Delete(path);}
    }
    [Fact] public async Task ReadFailureIsDiagnosticAndEarlyCloseIsNotComplete()
    {
        var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"doro-boundary-"+Guid.NewGuid()+".jsonl");
        try {
            using var log=new BoundaryDiagnostic(path);
            log.Sample(TimeSpan.Zero,()=>throw new InvalidOperationException("private text"));log.Dispose();
            await log.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            var text=File.ReadAllText(path);Assert.Contains("read-error",text);Assert.Contains("disposed-before-deadline",text);
            Assert.DoesNotContain("private text",text);
        }finally{if(File.Exists(path))File.Delete(path);}
    }
}

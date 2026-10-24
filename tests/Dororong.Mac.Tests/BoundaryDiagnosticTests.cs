using System.Text.Json;
using Dororong.Mac.Native;

namespace Dororong.Mac.Tests;

public sealed class BoundaryDiagnosticTests
{
    [Fact] public async Task WriteFailureStopsRecordingWithoutOverwritingExistingFile()
    {
        var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),"doro-boundary-"+Guid.NewGuid()+".jsonl");
        await File.WriteAllTextAsync(path,"existing evidence");
        try{
            using var log=new BoundaryDiagnostic(path);
            await log.Completion.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.NotNull(log.Failure);Assert.False(log.IsRecording);
            Assert.Equal("existing evidence",await File.ReadAllTextAsync(path));
            log.Sample(TimeSpan.Zero,()=>throw new Exception("Must not sample failed writer"));
        }finally{File.Delete(path);}
    }
    [Theory]
    [InlineData("--boundary-session=f70a60ef-649a-444f-bbd3-665cf082b8fe","f70a60ef-649a-444f-bbd3-665cf082b8fe")]
    [InlineData("--boundary-session=../../other",null)]
    [InlineData("--boundary-session=",null)]
    [InlineData("--other=value",null)]
    public void SessionArgumentCannotEscapeLogDirectory(string argument,string? expected)
        =>Assert.Equal(expected,Program.ParseBoundarySession([argument]));
    [Fact] public void TimingSeparatesHandlerWorkFromBetweenHandlerWaitAndResets()
    {
        var t=new BoundaryFrameTimings();
        t.Record(16,1,2,3,4,12);t.Record(112,2,50,4,1,60);
        var s=t.Take();Assert.Equal(2,s.Frames);Assert.Equal(112,s.MaxIntervalMs);
        Assert.Equal(50,s.MaxControllerMs);Assert.Equal(4,s.MaxMoveMs);
        Assert.Equal(60,s.MaxHandlerMs);Assert.Equal(100,s.MaxBetweenHandlersMs);
        Assert.Equal(0,t.Take().Frames);
        t.Record(65,1,1,1,1,5);Assert.Equal(5,t.Take().MaxBetweenHandlersMs);
    }
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

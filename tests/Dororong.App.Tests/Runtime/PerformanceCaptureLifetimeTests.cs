using System.Reflection;
using Dororong.App.Controls;
using Dororong.App.Runtime;

namespace Dororong.App.Tests.Runtime;

public sealed class PerformanceCaptureLifetimeTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public void Large_capture_streams_without_allocating_a_complete_JSON_string()=>Controls.CheekProductTests.Sta(()=>{
        var path=System.IO.Path.Combine(System.IO.Path.GetTempPath(),$"dororong-capture-{Guid.NewGuid():N}.json");
        try{
            using var capture=new RuntimePerformanceCapture(path,new DororongPresenter(),false);
            var frames=(List<object>)typeof(RuntimePerformanceCapture).GetField("frames",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(capture)!;
            var payload=new string('x',128*1024);
            for(var i=0;i<100;i++)frames.Add(new{index=i,payload});
            var baselineStart=GC.GetAllocatedBytesForCurrentThread();
            var baseline=System.Text.Json.JsonSerializer.Serialize(new{frames},new System.Text.Json.JsonSerializerOptions{WriteIndented=true});
            var baselineAllocated=GC.GetAllocatedBytesForCurrentThread()-baselineStart;
            Assert.True(baseline.Length>payload.Length*99);
            var before=GC.GetTotalAllocatedBytes(true);
            capture.Dispose();
            capture.CaptureExport.GetAwaiter().GetResult();
            var allocated=GC.GetTotalAllocatedBytes(true)-before;
            output.WriteLine($"String serialization: {baselineAllocated:N0} bytes; streamed capture: {allocated:N0} bytes.");
            // A complete UTF-16 payload alone needs 2*payload.Length*100 bytes.
            // Compare to the old string path as well; don't assume cold JSON
            // metadata/encoding has a fixed arbitrary 4MB allocation budget.
            Assert.True(allocated<payload.Length*100 && allocated<baselineAllocated/2,
                $"Stream {allocated:N0} bytes, string reference {baselineAllocated:N0} bytes.");
            Assert.True(new System.IO.FileInfo(path).Length>12*1024*1024);
            using var stream=System.IO.File.OpenRead(path);
            using var json=System.Text.Json.JsonDocument.Parse(stream);
            Assert.Equal(100,json.RootElement.GetProperty("frames").GetArrayLength());
            Assert.Equal(99,json.RootElement.GetProperty("frames")[99].GetProperty("index").GetInt32());
            Assert.Empty(frames);Assert.Equal(0,frames.Capacity);
        }finally{if(System.IO.File.Exists(path))System.IO.File.Delete(path);}
    });

    [Fact]
    public void Finished_capture_releases_samples_even_when_output_is_a_directory()=>Controls.CheekProductTests.Sta(()=>{
        using var capture=new RuntimePerformanceCapture(System.IO.Path.GetTempPath(),new DororongPresenter(),false);
        foreach(var name in new[]{"frames","ticks","states","inputs","memory"}){
            var list=(List<object>)typeof(RuntimePerformanceCapture).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(capture)!;
            list.Add(new byte[100_000]);
        }
        capture.Dispose();capture.Dispose();
        capture.CaptureExport.GetAwaiter().GetResult();
        foreach(var name in new[]{"frames","ticks","states","inputs","memory"}){
            var list=(List<object>)typeof(RuntimePerformanceCapture).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(capture)!;
            Assert.Empty(list);Assert.Equal(0,list.Capacity);
        }
    });
}

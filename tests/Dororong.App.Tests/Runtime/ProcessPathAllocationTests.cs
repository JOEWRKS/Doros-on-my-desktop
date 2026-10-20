using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Dororong.App.Interop;

namespace Dororong.App.Tests.Runtime;

public sealed class ProcessPathAllocationTests
{
    [Fact]
    public void Native_process_path_repeats_preserve_identity(){
        var method=typeof(DesktopMetadataReader).GetMethod("ReadProcessImagePath",BindingFlags.NonPublic|BindingFlags.Static)!;
        var read=method.CreateDelegate<Func<uint,string>>();var pid=(uint)Environment.ProcessId;
        var expected=read(pid);var before=GC.GetAllocatedBytesForCurrentThread();var timer=Stopwatch.StartNew();
        for(var i=0;i<200;i++)Assert.Equal(expected,read(pid));
        timer.Stop();var allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        Assert.Equal(Environment.ProcessPath,expected,ignoreCase:true);
        if(Environment.GetEnvironmentVariable("DORORONG_PATH_COST") is {} path){Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path,JsonSerializer.Serialize(new{calls=200,allocated,ms=timer.Elapsed.TotalMilliseconds}));}
    }
}

using System.Diagnostics;
using Dororong.Portable.Desktop;
namespace Dororong.Mac.Tests;
public class InstanceTests
{
    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task DuplicateRefusedAndLeaseRecoversAfterExit(bool crash)
    {
        var root=new DirectoryInfo(AppContext.BaseDirectory);while(root is not null&&!Directory.Exists(Path.Combine(root.FullName,"src","Dororong.Mac")))root=root.Parent;
        Assert.NotNull(root);var configuration=new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
        var worker=Path.Combine(root.FullName,"tests","Dororong.InstanceProbe","bin",configuration,"net8.0","Dororong.InstanceProbe.dll");Assert.True(File.Exists(worker));
        var name="Doro-test-"+Guid.NewGuid().ToString("N");var children=new List<Process>();
        Process Start(){var info=new ProcessStartInfo("dotnet"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true};info.ArgumentList.Add(worker);info.ArgumentList.Add(name);var p=Process.Start(info)!;children.Add(p);return p;}
        async Task<string?> Read(Process p)=>await p.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
        try
        {
            var owner=Start();Assert.Equal("ACQUIRED",await Read(owner));
            var duplicate=Start();Assert.Equal("DUPLICATE",await Read(duplicate));await duplicate.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(0,duplicate.ExitCode);
            if(crash)owner.Kill();else await owner.StandardInput.WriteLineAsync("");await owner.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
            var replacement=Start();Assert.Equal("ACQUIRED",await Read(replacement));await replacement.StandardInput.WriteLineAsync("");await replacement.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));Assert.Equal(0,replacement.ExitCode);
        }
        finally{foreach(var p in children){if(!p.HasExited){p.Kill();p.WaitForExit(5000);}p.Dispose();}}
    }
    [Fact]
    public void InstanceIdentityIsStableAcrossInstallLocationsButUserScoped()
    {
        var one=SingleInstanceLease.UserName(Path.Combine(Path.GetTempPath(),"one"));Assert.Equal(one,SingleInstanceLease.UserName(Path.Combine(Path.GetTempPath(),"one")));
        Assert.NotEqual(one,SingleInstanceLease.UserName(Path.Combine(Path.GetTempPath(),"two")));
    }
}

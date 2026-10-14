using Dororong.App.Product;

namespace Dororong.App.Tests.Product;

public class StartupRegistrationTests
{
    [Fact] public void Opt_in_quotes_path_and_can_remove_its_exact_entry()
    {
        string? value=null;var writes=0;
        var registration=new StartupRegistration(@"C:\A B\Dororong.App.exe",()=>value,v=>{value=v;writes++;});
        Assert.False(registration.Enabled);Assert.Equal(0,writes);
        registration.SetEnabled(true);Assert.Equal("\"C:\\A B\\Dororong.App.exe\"",value);Assert.True(registration.Enabled);
        registration.SetEnabled(false);Assert.Null(value);
    }
    [Fact] public void Foreign_entry_is_not_overwritten_or_removed()
    {
        string? value="unrelated.exe";
        var r=new StartupRegistration(@"C:\Pet\Dororong.App.exe",()=>value,v=>value=v);
        Assert.Throws<InvalidOperationException>(()=>r.SetEnabled(true));
        Assert.Throws<InvalidOperationException>(()=>r.SetEnabled(false));Assert.Equal("unrelated.exe",value);
    }
    [Theory] [InlineData("pet.exe")] [InlineData("C:\\a\"b.exe")] [InlineData("C:\\a.txt")]
    public void Invalid_executable_is_rejected(string path) =>
        Assert.Throws<ArgumentException>(()=>new StartupRegistration(path,()=>null,_=>{}));
    [Fact] public void Failed_write_is_reported()
    {
        var r=new StartupRegistration(@"C:\Pet\Dororong.App.exe",()=>null,_=>{});
        Assert.Throws<System.IO.IOException>(()=>r.SetEnabled(true));
    }
}

using Avalonia.Headless.XUnit;
using Dororong.Mac;

namespace Dororong.Mac.Tests;

public sealed class DesktopSessionTests
{
    [Fact]
    public void BundleLaunchDoesNotSelectDiagnosticMode()
    {
        Assert.True(Program.LaunchesPet([],true));
        Assert.False(Program.LaunchesPet([],false));
        Assert.False(Program.LaunchesPet(["--preview"],true));
        Assert.False(Program.LaunchesPet(["--motion-preview"],true));
        Assert.False(Program.LaunchesPet(["--desktop-preview"],true));
    }
    [AvaloniaFact]
    public void OrdinarySessionConstructsNoInspectorAndDisposeIsIdempotent()
    {
        var exits=0;var session=new DesktopSession(false,()=>exits++);
        Assert.False(session.HasInspectionWindow);Assert.False(session.HasVisibleOverlay);
        session.Dispose();session.Dispose();Assert.Equal(0,exits);
    }
    [AvaloniaFact]
    public void InspectorExistsOnlyOnExplicitDiagnosticLaunch()
    {
        using var session=new DesktopSession(true,()=>{});
        Assert.True(session.HasInspectionWindow);Assert.False(session.HasVisibleOverlay);
    }
}

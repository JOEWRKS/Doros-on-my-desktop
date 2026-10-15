using Dororong.Mac.Native;
using System.Runtime.InteropServices;
using Xunit;

namespace Dororong.Mac.Tests;

public sealed class NativeBoundaryTests
{
    [Fact]
    public void QuartzEntryRejectsNonMacBeforeNativeLoading()
    {
        if(!OperatingSystem.IsMacOS())Assert.Throws<PlatformNotSupportedException>(()=>new QuartzDesktop());
        Assert.Equal(16,Marshal.SizeOf<QuartzDesktop.CGPoint>());
    }
}

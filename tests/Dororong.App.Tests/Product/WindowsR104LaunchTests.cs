using Dororong.App.Tests.Controls;
using Xunit;

namespace Dororong.App.Tests.Product;

public sealed class WindowsR104LaunchTests
{
    [Fact]
    public void Ordinary_launch_selects_accepted_renderer_and_size_menu()
        => CheekProductTests.Sta(() =>
        {
            var window = new MainWindow();
            try
            {
                Assert.True(window.UsesHighResolutionArt);
                Assert.InRange(window.SizePercent, 100, 150);
                Assert.NotNull(window.CreateSizeMenuItem());
            }
            finally { window.Close(); }
        });
}

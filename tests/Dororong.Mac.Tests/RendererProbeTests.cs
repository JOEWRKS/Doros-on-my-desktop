using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Dororong.Mac;

[assembly: AvaloniaTestApplication(typeof(Dororong.Mac.Tests.TestAppBuilder))]

namespace Dororong.Mac.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<ProbeApp>()
        .UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public sealed class RendererProbeTests
{
    [AvaloniaFact]
    public void MotionLabPreservesPositionAndCompletesSnack()
    {
        var window=new MotionPreviewWindow();
        try
        {
            window.Show();
            // This test owns the clock through Advance(). CaptureRenderedFrame
            // pumps the dispatcher; a live timer otherwise injects wall time
            // (including rendering/test overhead) and cancels the snack as stale.
            var clock=typeof(MotionPreviewWindow).GetField("timer",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!;
            Assert.IsType<Avalonia.Threading.DispatcherTimer>(clock.GetValue(window)).Stop();
            var initial=window.PetX;
            window.SetMode(PreviewMode.Snack);window.Advance(.016);
            Assert.Equal(initial,window.PetX); // Entering from the toolbar waits for stage input.
            window.SetPointer(new(initial+29,265));
            for(var i=0;i<35;i++)window.Advance(.016);
            Assert.Equal(Dororong.Core.Behavior.SnackPhase.Eating,window.SnackPhase);
            Assert.Equal(initial,window.PetX);
            using(var capture=window.CaptureRenderedFrame())
            {Assert.NotNull(capture);capture.Save(Path.Combine(AppContext.BaseDirectory,"motion-lab-eating.png"));}
            for(var i=0;i<120;i++)window.Advance(.016);
            Assert.Equal(Dororong.Core.Behavior.SnackPhase.Happy,window.SnackPhase);
            for(var i=0;i<180;i++)window.Advance(.016);
            Assert.Equal(Dororong.Core.Behavior.SnackPhase.None,window.SnackPhase);
            window.SetMode(PreviewMode.Walk);window.Advance(.016);Assert.True(window.PetX>initial);
            window.SetMode(PreviewMode.Track);window.SetPointer(new(600,100));window.Advance(.016);
            window.SetMode(PreviewMode.Snack);window.Advance(2); // Resume/lost-input cancellation.
            Assert.Equal(Dororong.Core.Behavior.SnackPhase.None,window.SnackPhase);
        }
        finally{window.Close();}
    }
    [AvaloniaFact]
    public void CanonicalAndSittingRenderAndClose()
    {
        var window = new ProbeWindow();
        try
        {
            window.Show();
            Assert.True(window.Topmost);
            Assert.False(window.ShowInTaskbar);
            Assert.False(window.ShowActivated);
            var image = Assert.IsType<Image>(window.Content);
            var standing = image.Source;
            using (var capture = window.CaptureRenderedFrame())
            {
                Assert.NotNull(capture);
                Assert.Equal(new PixelSize(144, 144), capture.PixelSize);
                capture.Save(Path.Combine(AppContext.BaseDirectory, "probe-standing.png"));
            }
            window.Present(1);
            Assert.NotSame(standing, image.Source);
            using (var capture = window.CaptureRenderedFrame())
            {
                Assert.NotNull(capture);
                capture.Save(Path.Combine(AppContext.BaseDirectory, "probe-sitting.png"));
            }
            window.Present(0);
            Assert.Same(standing, image.Source);
        }
        finally { window.Close(); }
        Assert.False(window.IsVisible);
    }
}

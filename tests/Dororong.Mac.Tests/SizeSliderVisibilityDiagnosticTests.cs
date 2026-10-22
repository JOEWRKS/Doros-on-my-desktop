using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dororong.Mac.Product;

namespace Dororong.Mac.Tests;

public class SizeSliderVisibilityDiagnosticTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Capture_theme_and_slider_geometry(bool dark, bool popup)
    {
        using var panel = new PetMenuPanel(false, false, true, () => { }, () => { }, _ => true,
            () => { }, () => { }, size: 100, resize: _ => true);
        var target = new Button { Content = "target", Width = 100, Height = 100 };
        using var host = new PetMenuHost();
        var window = new Window { Content = popup ? target : panel, SizeToContent = SizeToContent.WidthAndHeight,
            RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light };
        try
        {
            window.Show(); window.UpdateLayout();
            if (popup)
            {
                host.Open(target, () => panel); Avalonia.Threading.Dispatcher.UIThread.RunJobs();
                panel.Measure(new Size(288, double.PositiveInfinity)); panel.Arrange(new Rect(panel.DesiredSize)); panel.UpdateLayout();
            }
            using var frame = window.CaptureRenderedFrame(); Assert.NotNull(frame);
            frame.Save(Path.Combine(AppContext.BaseDirectory, $"size-slider-{dark}-{popup}-fixed.png"));
            var slider = panel.SizeSlider;
            if (panel.Bounds.Height > 0)
            {
                using var panelFrame = new Avalonia.Media.Imaging.RenderTargetBitmap(
                    new PixelSize((int)Math.Ceiling(panel.Bounds.Width * 2), (int)Math.Ceiling(panel.Bounds.Height * 2)), new Vector(192, 192));
                panelFrame.Render(panel);
                panelFrame.Save(Path.Combine(AppContext.BaseDirectory, $"size-panel-{dark}-{popup}-fixed.png"));
            }
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, $"size-slider-{dark}-{popup}-fixed.txt"),
                $"panel={panel.Bounds}; theme={slider.ActualThemeVariant}; bounds={slider.Bounds}; template={slider.Template}; " +
                string.Join("; ", slider.GetVisualDescendants().OfType<Control>().Select(c => $"{c.GetType().Name}/{c.Name}:{c.Bounds}")));
            Assert.True(slider.Bounds.Height > 0);
            Assert.NotNull(slider.Template);
            Assert.Equal(window.ActualThemeVariant, slider.ActualThemeVariant);
        }
        finally { window.Close(); }
    }
}

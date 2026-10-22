using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dororong.Mac.Product;

namespace Dororong.Mac.Tests;

public class SizeMenuHostRegressionTests
{
    [AvaloniaTheory]
    [InlineData(false, false)] [InlineData(false, true)]
    [InlineData(true, false)] [InlineData(true, true)]
    public void Both_routes_render_drag_close_and_reopen(bool tray, bool dark)
    {
        var app = Application.Current!; var priorTheme = app.RequestedThemeVariant;
        app.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        var target = new Button { Width = 100, Height = 100 };
        var owner = new Window { Width = 700, Height = 600, Content = target };
        using var popupHost = new PetMenuHost(); using var trayHost = new StatusMenuHost();
        PetMenuPanel? panel = null; var created = 0; var current = 100; var requested = new List<int>();
        bool IsOpen() => tray ? trayHost.IsOpen : popupHost.IsOpen;
        PetMenuPanel Create()
        {
            created++;
            return panel = new PetMenuPanel(tray, false, true, () => { }, () => { }, _ => true,
                () => { }, () => { }, size: current, resize: n => { requested.Add(n); current = n; return true; });
        }
        void Open() { if (tray) trayHost.Open(Create, _ => { }); else popupHost.Open(target, Create); Dispatcher.UIThread.RunJobs(); }
        try
        {
            owner.Show();
            for (var cycle = 0; cycle < 3; cycle++)
            {
                Open(); Open(); Assert.Equal(cycle + 1, created); Assert.True(IsOpen());
                var shown = panel!; var root = TopLevel.GetTopLevel(shown)!;
                using var frame = root.CaptureRenderedFrame(); Assert.NotNull(frame);
                var slider = shown.SizeSlider;
                Assert.Equal(cycle == 0 ? 100 : 125, slider.Value);
                Assert.True(slider.Bounds.Width > 190); Assert.True(slider.Bounds.Height > 0);
                Assert.NotNull(slider.Template); Assert.Equal(app.ActualThemeVariant, slider.ActualThemeVariant);
                var thumb = Assert.Single(slider.GetVisualDescendants().OfType<Thumb>());
                var track = Assert.Single(slider.GetVisualDescendants().OfType<Track>());
                Assert.True(thumb.Bounds.Width > 0); Assert.True(track.Bounds.Width > 190);
                frame.Save(Path.Combine(AppContext.BaseDirectory, $"size-host-{tray}-{dark}-{cycle}.png"));

                // Actual headless pointer routing through the rendered thumb;
                // not OS mouse input or native macOS acceptance.
                slider.Value = 100; root.UpdateLayout();
                var start = thumb.TranslatePoint(new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), root)!.Value;
                root.MouseDown(start, MouseButton.Left, RawInputModifiers.None);
                root.MouseMove(start + new Vector(track.Bounds.Width / 2, 0), RawInputModifiers.LeftMouseButton);
                root.MouseUp(start + new Vector(track.Bounds.Width / 2, 0), MouseButton.Left, RawInputModifiers.None);
                Assert.InRange(current, 120, 135); Assert.True(IsOpen());
                slider.Focus();
                root.KeyPressQwerty(PhysicalKey.End, RawInputModifiers.None); root.KeyReleaseQwerty(PhysicalKey.End, RawInputModifiers.None);
                Assert.Equal(150, current); Assert.True(IsOpen());
                root.KeyPressQwerty(PhysicalKey.Home, RawInputModifiers.None); root.KeyReleaseQwerty(PhysicalKey.Home, RawInputModifiers.None);
                Assert.Equal(100, current); Assert.True(IsOpen());
                slider.Value = 125;
                if (cycle == 0)
                {
                    // Escape destroys the tray Window; no key-up may be sent to
                    // that disposed headless platform window after dismissal.
                    root.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
                }
                else if (cycle == 1) shown.GetVisualDescendants().OfType<Button>().Single(b => Avalonia.Automation.AutomationProperties.GetName(b) == "메뉴 닫기").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                else shown.SnackButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.False(IsOpen());
                // Preserve the existing 150ms status-item deactivation debounce;
                // a deliberate reopen occurs after that native click guard.
                if (tray) Thread.Sleep(180);
                if (!tray)
                {
                    var popup = (Popup)typeof(PetMenuHost).GetField("popup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(popupHost)!;
                    Assert.Null(popup.Child); Assert.Null(popup.PlacementTarget); Assert.Null(((ILogical)popup).LogicalParent);
                }
            }
            Assert.True(requested.Count >= 12);
        }
        finally { popupHost.Dispose(); trayHost.Dispose(); owner.Close(); app.RequestedThemeVariant = priorTheme; }
    }

    [AvaloniaFact]
    public void Popup_reanchors_without_retaining_the_old_target()
    {
        using var host = new PetMenuHost();
        var first = new Button(); var second = new Button();
        var a = new Window { Content = first, Width = 400, Height = 400, RequestedThemeVariant = ThemeVariant.Light };
        var b = new Window { Content = second, Width = 400, Height = 400, RequestedThemeVariant = ThemeVariant.Dark };
        PetMenuPanel? panel = null;
        PetMenuPanel Create() => panel = new(false, false, true, () => { }, () => { }, _ => true, () => { }, () => { }, resize: _ => true);
        try
        {
            a.Show(); b.Show();
            foreach (var target in new[] { first, second })
            {
                host.Open(target, Create); Dispatcher.UIThread.RunJobs();
                using var frame = TopLevel.GetTopLevel(panel!)!.CaptureRenderedFrame();
                Assert.Equal(target.ActualThemeVariant, panel!.SizeSlider.ActualThemeVariant);
                Assert.NotNull(panel.SizeSlider.Template); host.Close(); Assert.False(host.IsOpen);
            }
            Assert.Throws<InvalidOperationException>(() => host.Open(first, () => throw new InvalidOperationException("fixture")));
            host.Open(second, Create); Assert.True(host.IsOpen); host.Dispose(); host.Dispose(); Assert.False(host.IsOpen);
        }
        finally { host.Dispose(); a.Close(); b.Close(); }
    }
}

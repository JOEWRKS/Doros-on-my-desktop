using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Dororong.App.Product;
using Dororong.App.Tests.Controls;

namespace Dororong.App.Tests.Product;

public class PetSizeTests
{
    [Fact]
    public void Continuous_changes_do_not_postpone_frames_and_do_not_retain_closed_menu() => CheekProductTests.Sta(() =>
    {
        EventHandler? frame = null; var attached = 0; var detached = 0;
        var timer = new Dororong.App.Runtime.CompositionFrameTimer(h => { frame += h; attached++; },
            h => { frame -= h; detached++; });
        var actual = 100; var calls = 0; var saves = 0;
        var item = new PetSizeMenuItem(() => actual, () => true, n => { actual = n; calls++; return true; }, () => saves++, timer);
        void Render(int ms) => frame?.Invoke(null, (System.Windows.Media.RenderingEventArgs)Activator.CreateInstance(
            typeof(System.Windows.Media.RenderingEventArgs), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null, new object[] { TimeSpan.FromMilliseconds(ms) }, null)!);
        for (var i = 0; i < 10; i++)
        {
            for (var j = 1; j <= 5; j++) item.Slider.Value = 100 + i * 5 + j;
            Assert.Equal(i + 1, attached); // Five inputs, only one scheduled frame.
            Assert.Equal(i, calls);
            Render(i * 16);
            Assert.Equal(105 + i * 5, actual);
            Assert.Null(frame);
        }
        Assert.Equal(10, calls); Assert.Equal(attached, detached); Assert.Equal(0, saves);
        item.Slider.Value = 121;
        Render(144); // Duplicate composition timestamp after stop/start stays pending.
        Assert.Equal(150, actual); Assert.NotNull(frame);
        item.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));
        Assert.Equal(121, actual); Assert.Null(frame); Assert.Equal(1, saves);
        Render(2000); Assert.Equal(11, calls);
        item.Finish(); Assert.Equal(1, saves);
    });

    [Fact]
    public void Preference_is_optional_bounded_and_writes_only_changes()
    {
        object? stored = null; var writes = 0;
        var p = new PetSizePreferences(() => stored, n => { stored = n; writes++; });
        Assert.Null(p.Percent);
        p.Save(125); p.Save(125); Assert.Equal(1, writes); Assert.Equal(125, p.Percent);
        Assert.Throws<ArgumentOutOfRangeException>(() => p.Save(151));
        stored = "150"; Assert.Throws<System.IO.IOException>(() => p.Save(125));
        var failing = new PetSizePreferences(() => null, _ => { });
        Assert.Throws<System.IO.IOException>(() => failing.Save(150));
    }

    [Fact]
    public void Slider_updates_once_then_saves_on_dismissal_and_rejects_busy_changes() => CheekProductTests.Sta(() =>
    {
        var percent = 150; var allowed = true; var applications = 0; var saves = 0;
        var item = new PetSizeMenuItem(() => percent, () => allowed, n => { percent = n; applications++; return true; }, () => saves++);
        Assert.Equal(150, item.Slider.Value);
        Assert.Equal("캐릭터 크기 (%)", AutomationProperties.GetName(item.Slider));
        item.Slider.Value = 125; item.Flush();
        Assert.Equal(125, percent); Assert.Equal(1, applications); Assert.Equal(0, saves);
        item.Finish(); item.Finish(); Assert.Equal(1, saves);
        allowed = false; item.Slider.Value = 130; item.Flush();
        Assert.Equal(125, item.Slider.Value); Assert.False(item.Slider.IsEnabled);
        item.Finish(); Assert.Equal(1, saves);
    });

    [Fact]
    public void Slider_preserves_approved_menu_width_and_fits_content() => CheekProductTests.Sta(() =>
    {
        var menu = new PetContextMenu();
        var item = new PetSizeMenuItem(() => 125, () => true, _ => true, () => { });
        menu.Items.Add(new MenuItem { Tag = "snack", Header = "간식 주기" });
        menu.Items.Add(item);
        menu.Measure(new Size(500, 800)); menu.Arrange(new Rect(menu.DesiredSize)); menu.UpdateLayout();
        if (!SystemParameters.HighContrast) Assert.Equal(312, menu.ActualWidth);
        Assert.Equal(76, item.ActualHeight); Assert.True(item.Slider.ActualWidth > 190);
        Assert.True(item.StaysOpenOnClick);
    });
}

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Dororong.App.Runtime;

namespace Dororong.App.Product;

// One control shared by both menus. Only committed, applied sizes are saved.
internal sealed class PetSizeMenuItem : MenuItem
{
    private readonly Slider slider;
    private readonly TextBlock value;
    private readonly Func<int> current;
    private readonly Func<bool> available;
    private readonly Func<int, bool> apply;
    private readonly Action save;
    private readonly CompositionFrameTimer timer;
    private bool syncing, changed;
    internal Slider Slider => slider;

    internal PetSizeMenuItem(Func<int> current, Func<bool> available, Func<int, bool> apply, Action save,
        CompositionFrameTimer? frameTimer = null)
    {
        this.current = current; this.available = available; this.apply = apply; this.save = save;
        Tag = "size"; StaysOpenOnClick = true; Height = 76;
        var panel = new Grid { Margin = new Thickness(11, 6, 11, 6) };
        panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });
        panel.RowDefinitions.Add(new RowDefinition());
        var title = new TextBlock { Text = "크기", FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
        value = new TextBlock { FontSize = 12, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
        slider = new Slider { Minimum = 100, Maximum = 150, TickFrequency = 1, IsSnapToTickEnabled = true,
            SmallChange = 1, LargeChange = 10, VerticalAlignment = VerticalAlignment.Center, IsMoveToPointEnabled = true };
        AutomationProperties.SetName(slider, "캐릭터 크기 (%)");
        Grid.SetRow(slider, 1);
        panel.Children.Add(title); panel.Children.Add(value); panel.Children.Add(slider);
        Header = panel;
        // Use the existing panel spacing and typography, with a real native
        // Slider (range automation, keyboard, high contrast), not a fake track.
        var content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetBinding(ContentPresenter.ContentProperty, new System.Windows.Data.Binding("Header") { RelativeSource = new(System.Windows.Data.RelativeSourceMode.TemplatedParent) });
        Template = new ControlTemplate(typeof(MenuItem)) { VisualTree = content };
        // One pending composition callback, reading the latest value. Restarting
        // a debounce on every mouse move starves updates throughout a drag.
        timer = frameTimer ?? new CompositionFrameTimer();
        timer.Tick += (_, _) => Flush();
        slider.ValueChanged += (_, _) =>
        {
            value.Text = $"{slider.Value:0}%";
            if (syncing) return;
            timer.Start(preserveTimestamp: true);
        };
        slider.PreviewMouseLeftButtonUp += (_, _) => Flush();
        Unloaded += (_, _) => Finish(); // Never retain a closed menu via the static rendering event.
        GotKeyboardFocus += (_, e) => { if (ReferenceEquals(e.OriginalSource, this)) slider.Focus(); };
        Refresh();
    }

    internal void Refresh()
    {
        syncing = true;
        slider.Value = current(); value.Text = $"{current()}%";
        slider.IsEnabled = available();
        ToolTip = slider.IsEnabled ? "100~150% · 방향키 1% · Page Up/Down 10%" : "착지하거나 조작을 마친 뒤 조절할 수 있어요.";
        syncing = false;
    }
    internal void Flush()
    {
        timer.Stop();
        if ((int)slider.Value == current()) return;
        if (available() && apply((int)slider.Value)) changed = true;
        else Refresh();
    }
    internal void Finish()
    {
        Flush();
        if (changed) { changed = false; save(); }
    }
}

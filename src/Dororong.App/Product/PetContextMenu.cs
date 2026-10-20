using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dororong.App.Product;

/// <summary>One native menu surface for character and tray; the pet is never transformed.</summary>
public sealed class PetContextMenu : ContextMenu
{
    public static readonly DependencyProperty ShowVersionProperty = DependencyProperty.Register(
        nameof(ShowVersion), typeof(bool), typeof(PetContextMenu), new PropertyMetadata(false));
    public bool ShowVersion { get => (bool)GetValue(ShowVersionProperty); set => SetValue(ShowVersionProperty, value); }
    public string ProductVersion => "v" + ProductIdentity.Version;
    // The menu must use the same full-resolution source as the snack cursor
    // and eating effects, not retain another URI decoder/cache chain.
    public static System.Windows.Media.ImageSource SnackIcon => Controls.SnackImage.Source;

    public PetContextMenu()
    {
        // Retain the system's fully accessible menu in Windows high-contrast mode.
        if (!SystemParameters.HighContrast)
        {
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/Dororong.App;component/Product/PetMenuResources.xaml", UriKind.Relative)
            });
            Style = (Style)Resources["PetMenuStyle"];
        }
        Placement = PlacementMode.MousePoint;
        AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(OnItemClicked), true);
    }

    public override void OnApplyTemplate()
    {
        if (GetTemplateChild("PART_Close") is Button old) old.Click -= CloseMenu;
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Close") is Button close) close.Click += CloseMenu;
    }

    private void CloseMenu(object sender, RoutedEventArgs e) => IsOpen = false;

    private void OnItemClicked(object sender, RoutedEventArgs e)
    {
        // Keep dismissal reliable even if Windows declines the tray owner's
        // foreground-focus transfer into WPF's internal menu mode.
        if (e.OriginalSource is MenuItem { StaysOpenOnClick: false }) IsOpen = false;
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            IsOpen = false;
            e.Handled = true;
            return;
        }
        base.OnPreviewKeyDown(e);
    }
}

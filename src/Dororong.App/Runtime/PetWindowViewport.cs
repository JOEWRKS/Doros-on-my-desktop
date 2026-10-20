using System.Windows;
using Dororong.App.Controls;
using Dororong.Core.Geometry;

namespace Dororong.App.Runtime;

// Transparent render gutter only. The brain, input and captures continue to use
// the original 144-DIP presenter position/size, not the larger native window.
internal sealed class PetWindowViewport
{
    private const double Padding = 96;
    private readonly Window _window;
    private readonly DororongPresenter _presenter;
    private readonly FrameworkElement? _logicalViewport;

    internal PetWindowViewport(Window window, DororongPresenter presenter)
    {
        _window = window;
        _presenter = presenter;
        _logicalViewport = presenter.Parent as FrameworkElement;
        presenter.HorizontalAlignment = HorizontalAlignment.Center;
        presenter.VerticalAlignment = VerticalAlignment.Center;
        UpdateSize();
    }

    internal SizeD GetPetSize() => new(_presenter.Width, _presenter.Height);
    internal PointD GetPosition() => new(_window.Left + Padding, _window.Top + Padding);
    internal void SetPosition(PointD position)
    {
        // Size changes retain the logical origin and the same transparent gutter.
        UpdateSize();
        _window.Left = position.X - Padding;
        _window.Top = position.Y - Padding;
    }

    private void UpdateSize()
    {
        var width = _presenter.Width + 2 * Padding;
        var height = _presenter.Height + 2 * Padding;
        if (_presenter.HighResolutionGrabEnabled && _logicalViewport is not null)
        {
            // Resizing a layered HWND rebuilds its native backing surface.
            // Keep one maximum-size backing window across the 100–150% range.
            // The original parent remains the exact logical swing viewport;
            // extra transparent space must not change swing caps or anchoring.
            if (PresentationSource.FromVisual(_window)?.CompositionTarget is { } target)
            {
                // The former native resize rounded to device pixels before
                // WPF centered the presenter. Preserve that sub-DIP origin too.
                var dpi = target.TransformToDevice;
                width = Math.Floor(width * dpi.M11 + .5) / dpi.M11;
                height = Math.Floor(height * dpi.M22 + .5) / dpi.M22;
            }
            _logicalViewport.HorizontalAlignment = HorizontalAlignment.Left;
            _logicalViewport.VerticalAlignment = VerticalAlignment.Top;
            _logicalViewport.Width = width;
            _logicalViewport.Height = height;
            _window.Width = _window.Height = 144 * 1.5 + 2 * Padding;
        }
        else
        {
            _window.Width = width;
            _window.Height = height;
        }
    }
}

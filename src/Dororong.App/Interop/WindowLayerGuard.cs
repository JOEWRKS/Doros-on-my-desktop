using System.ComponentModel;

namespace Dororong.App.Interop;

// WS_EX_TOPMOST can remain set while the actual sibling order has fallen behind
// ordinary windows. Inspect that order too; do not fight other topmost apps.
internal sealed class WindowLayerGuard
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMilliseconds(250);
    private TimeSpan? _lastCheck;

    internal void Maintain(nint pet, TimeSpan elapsed)
    {
        if (_lastCheck is { } last && elapsed >= last && elapsed - last < CheckInterval) return;
        _lastCheck = elapsed;
        try
        {
            if (pet == 0 || DesktopMetadataReader.Api.GetWindowThreadProcessId(pet, out var pid) == 0 ||
                pid != Environment.ProcessId || !DesktopMetadataReader.Api.IsWindowVisible(pet) ||
                DesktopMetadataReader.Api.IsIconic(pet) || IsCloaked(pet) ||
                !DesktopMetadataReader.Api.GetWindowRect(pet, out var bounds)) return;

            var topmost = (NativeMethods.GetWindowLongPtr(pet, NativeMethods.GwlExStyle).ToInt64() & NativeMethods.WsExTopmost) != 0;
            if (!topmost)
            {
                Raise(pet);
                return;
            }

            var visited = new HashSet<nint>();
            var candidate = DesktopMetadataReader.Api.GetWindow(pet, 3);
            for (var count = 0; candidate != 0 && count < 256 && visited.Add(candidate); count++)
            {
                if (DesktopMetadataReader.Api.IsWindowVisible(candidate) &&
                    !DesktopMetadataReader.Api.IsIconic(candidate) && !IsCloaked(candidate) &&
                    DesktopMetadataReader.Api.GetWindowThreadProcessId(candidate, out var owner) != 0 &&
                    owner != pid &&
                    (NativeMethods.GetWindowLongPtr(candidate, NativeMethods.GwlExStyle).ToInt64() & NativeMethods.WsExTopmost) == 0 &&
                    DesktopMetadataReader.Api.GetWindowRect(candidate, out var other) &&
                    bounds.Left < other.Right && bounds.Right > other.Left &&
                    bounds.Top < other.Bottom && bounds.Bottom > other.Top)
                {
                    Raise(pet);
                    return;
                }
                candidate = DesktopMetadataReader.Api.GetWindow(candidate, 3);
            }
        }
        catch (Win32Exception)
        {
            // A window may disappear during the bounded read. Recheck next time;
            // a transient desktop change must not terminate the animation loop.
        }
    }

    private static bool IsCloaked(nint window) =>
        DesktopMetadataReader.Api.DwmCloaked(window, 14, out var cloaked, sizeof(int)) == 0 && cloaked != 0;

    private static void Raise(nint pet) => NativeMethods.SetWindowPosChecked(pet, new(-1), 0, 0, 0, 0,
        NativeMethods.SwpNoMove | NativeMethods.SwpNoSize |
        NativeMethods.SwpNoActivate | NativeMethods.SwpNoOwnerZOrder);
}

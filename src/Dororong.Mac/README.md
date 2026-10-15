# macOS parity host — native acceptance pending

This is not an accepted macOS release or a replacement for Dororong.App.
A bundled no-argument launch runs the pet and menu-bar icon without a developer
window. Both product menus retain exit; initialization failures show an actionable
error/exit dialog. The lifetime is explicit, so dismissing a menu does not end it.
Developer-only `--preview`, `--motion-preview` and `--desktop-preview` retain their
explicit probes. They are not the normal app entry point.

Full required features and actual-Mac gates:
[parity checklist](../../docs/specs/2026-10-15-macos-parity.md).

## Windows development

```powershell
dotnet test tests/Dororong.Portable.Tests -c Release
dotnet test tests/Dororong.Mac.Tests -c Release
dotnet test tests/Dororong.RenderParity.Tests -c Release
dotnet run --project src/Dororong.Mac -c Release -- --preview
dotnet run --project src/Dororong.Mac -c Release -- --motion-preview
```

The second test uses headless Avalonia/Skia; it is not macOS native-window proof.
`--motion-preview` opens a bounded test stage with walk/tracking/snack controls.
Move the pointer inside the stage, lift the snack to trigger the existing jump,
or hold it at the mouth to eat/chew/sway. Leaving the stage cancels. This is local
window input, not global Mac cursor/Dock integration or the final product menu.
No preview flag on a raw executable prints the limitation and exits2. A valid
`.app` launched without arguments starts the pet-only desktop session. The
two renderer-only probes do not register
startup, alter the existing pet, capture global input or contact a service.

## Native desktop integration preview (macOS only)

Run the developer output with `./Dororong.Mac --desktop-preview`. On Windows this
flag exits2 before loading native APIs. The visible control window provides snack,
drop, sit/rise, automatic-sleep and exit controls; the transparent pet also has a
B2 context menu. Drag the upper head onto a window/Dock edge and release
when the paws flutter. Head extension follows the Windows4-point deadzone/80-point
distance, carrying retains the grab offset, velocity drives the damped swing, and
release retraces the authored pose in180ms while platform physics owns the fall.
The host now includes the same96-point transparent gutter around its logical144px
pet. Head/cheek hit tests follow the displayed pose; sleeping head descriptors and
perched paw inverse hit tests are connected. Pull the near cheek without carrying
the body; release triggers the original timed backward hop and lands at the new
position. Attached cheeks stay anchored. Tap one perched paw to wave it, or drag
that paw to stretch it. Body-click/wake and extreme-landing presentation are now
connected. These interactions are Windows-tested, not Mac-verified.

- Quartz reads global pointer position/current primary-button state and on-screen
  window/display geometry. It does not read keyboard input, window titles, screen
  images or application documents. No event tap, upload or permission prompt is
  installed. Actual OS permission/denial behavior still needs Mac verification.
- World coordinates are global display points, top-left of the main display.
  Cocoa window placement flips Y against the main display height; do not multiply
  movement by Retina scale. Negative monitor origins are retained.
- The shared Windows Core engines own walking, snack trajectories, falling,
  landing and edge attachment. A visible horizontal Dock shelf is taskbar-like.
  Side Dock is not a horizontal support, matching the Windows vertical-taskbar
  policy. Tiny auto-hide strips and Dock-owned full-screen surfaces are excluded.
- Window queries use one background request at a time, an80ms cadence and500ms
  stale limit. A failed query briefly freezes attachment acquisition, then falls
  back to known monitor floors. A successful empty list means owners really left.
  Popup menus cannot erase Dock support. No `visibleFrame`-derived phantom shelf.
- Only this process's overlay windows are moved. Alpha hit testing lets clicks
  through transparent space. The32px jewel follows the cursor without intercepting
  clicks or hiding the system cursor. The native window level/Spaces configuration
  is implemented, not yet verified on an actual Mac.

Normal hunting now uses the Windows preparation/pounce/landing/cooldown state
rules, including immediate preparation cancellation. Idle breathing/blinking,
approved sit/rise frames,90-second automatic sleep and its wake switch are connected.
Automatic sleep defaults ON and persists in per-user settings. Snack and direct
capture suppress hunting. The host includes96-point transparent gutters on each
side of its144px logical frame. Cheek/paw/body-click and captured-pose hit maps
are connected. Native head-icon menus, optional SMAppService login registration
and per-user single-instance protection are implemented. Actual Mac behavior,
permissions, composed-image differences and full feature acceptance remain open.
Long frame gaps cancel input actions, but actual lock/sleep notifications/recovery
remain unverified. See [current checks and raster limits](../../TASKS.md#macos-hunting-rest-head-drag--2026-10-15).

Native API references checked2026-10-15:
[window metadata](https://developer.apple.com/documentation/coregraphics/cgwindowlistcopywindowinfo(_:_:)),
[display coordinates](https://developer.apple.com/documentation/coregraphics/cgdisplaybounds(_:)),
[pointer location](https://developer.apple.com/documentation/coregraphics/cgevent/location),
[Cocoa placement](https://developer.apple.com/documentation/appkit/nswindow/setframetopleftpoint(_:)),
[Avalonia native handles](https://docs.avaloniaui.net/docs/app-development/native-interop).
Actual behavior/ABI, auto-hidden Dock animation, permissions, mixed Retina,
Spaces/fullscreen and other applications' menus must be tested on both target CPUs.

The Windows-reference test needs the accepted candidate14 DLL at its default
`BaselineDir`, or an explicitly supplied `-p:BaselineDir=ABSOLUTE_PATH`.
It checks that DLL's pinned hash, not a newly modified renderer as its own oracle.
Missing baseline is a failed prerequisite, not a silently skipped comparison.

## Cross-build (not a distributable app bundle)

Run sequentially. Use fresh output folders and preserve previous evidence.

```powershell
dotnet publish src/Dororong.Mac -c Release -r osx-arm64 --self-contained true -p:RuntimeFrameworkVersion=8.0.32 -p:TargetLatestRuntimePatch=false -o artifacts/macos/NEW/osx-arm64
dotnet publish src/Dororong.Mac -c Release -r osx-x64 --self-contained true -p:RuntimeFrameworkVersion=8.0.32 -p:TargetLatestRuntimePatch=false -o artifacts/macos/NEW/osx-x64
```

These are raw developer outputs, not signed/notarized `.app` installers. They
must not be sent as loose folders to ordinary users. The
[tester ZIP pipeline](../../tools/mac-build/README.md) assembles the arm64 `.app`,
head icon, runtime, notices and executable permissions with ad-hoc integrity
checks. This does not supply Developer ID/notarization or native acceptance.
Normal downloaded Gatekeeper launch, actual integration and full parity remain open.
Do not advise disabling OS protection to make this probe run.

Avalonia11.3.22 is pinned. Core and current assets are linked, not copied or
redrawn; the Windows project/installer is unchanged. Current walk/gallop, eye
sampling, chewing, gaze smoothing and happy timing are ported from the accepted
renderer, not the legacy walk bank. See [renderer provenance](../Dororong.Portable/Rendering/README.md).
Windows-derived join/effect coverage and CPU composition replace the previous
backend edge differences. Continuous subpixel coverage is still bounded rather
than universally byte-identical. Current checks and Mac limits are recorded in
[TASKS](../../TASKS.md#macos-rendering-and-pet-only-launch--2026-10-15).
Character rights notices still apply.

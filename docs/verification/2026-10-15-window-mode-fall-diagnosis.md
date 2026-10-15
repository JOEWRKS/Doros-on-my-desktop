# Windows window-mode transition fall — diagnostic checkpoint

User reports a fall when RimWorld changes from a maximized/windowed-fullscreen
state to a window. User clarified the pet was **standing on the taskbar**, not
hanging or standing on the game's window. Screenshot shows part of the pet behind
the taskbar, but does not by itself distinguish world movement from z-order.

## Scope and evidence

Read-only native window metadata and synthetic sequences; no user-window mode
changes, app replacement, physics/renderer modification, commit/push or release.
Diagnostic harness and raw local outputs are under
`artifacts/window-mode-diagnosis-20261015/` (ignored, not for public upload).
The first harness compile lacked System.IO; adding the import allowed execution.
An initial process lookup used only Dororong.App; corrected it to include the
installed Dororong.exe before evaluating the live snapshot.

The harness directly references accepted candidate14 DLLs. Installed App DLL and
tested DLL both have SHA256
`EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C`.
It runs DesktopSceneNative→PlatformGeometry→PlatformMotion, not a replacement
physics algorithm and not the full live PetLoop/OS transition.

## Six standing-state simulations

Synthetic monitor bottom1080, taskbar top1040, initial world sole1040;120 ticks
at16ms per case. Unchanged physical taskbar under the pet unless stated.

| Condition | Maximum sole Y | Final sole Y | Result |
| --- | ---: | ---: | --- |
| Normal maximized bounds ending at1040 → restored window above bar |1040|1040|No fall |
| One snapshot of higher-z full-screen window crossing taskbar |1040.2304|1040|Fall then lift recovery |
| Persistent higher-z full-screen occlusion |1080|1080|Fall to monitor floor |
| Taskbar hidden for one snapshot |1048.2944|1040|Fall,80ms visibility qualification, recovery |
| Taskbar HWND replaced |1045.76|1040|New identity qualification, fall then recovery |
| Floor-only fallback for40 ticks |1080|1040|Fall to floor, then recovery |

These establish shared code behavior under supplied metadata. They do not prove
RimWorld emitted one of these exact sequences or that other applications have
actually reproduced the bug. Several cases already recover when the bar returns;
do not explain a persistent buried state using a one-snapshot fall alone.

## Native snapshot and code boundary

The later read-only snapshot found RimWorld's visible UnityWndClass bounds
0,0..1920,1040 and visible horizontal taskbars at1040..1080. No ordinary
foreground window in that sample removed the taskbar at its top. A snapshot
after the event cannot establish transient metadata, focus or z-order history.

PlatformGeometry removes taskbar intervals crossed by ordinary higher-z windows;
transient menus have a narrower exception. PlatformMotion immediately releases
missing support. DesktopSceneNative requires80ms of visible taskbar geometry;
DesktopSceneSource falls back after500ms without fresh metadata. The layer guard
specifically above overlapping taskbars only runs for held-ready or perched poses,
not normal standing. These are review points, not confirmed incident causes.

TaskbarRecovery.FindPenetration exists with unit-level tests but has no product
call site. This does not prove it would fix this incident; its tests manually
perform correction and must not be mistaken for an integrated runtime guarantee.

## Next evidence / proposed verification

Capture a bounded read-only time sequence while the user repeats the exact
transition: game bounds/identity/z-order, taskbar bounds/visibility/identity,
scene health/support interval and pet native position. Do not log window titles,
keyboard input or screenshots of unrelated apps. Compare maximal→restore and
borderless→windowed as separate cases; then use the captured sequence as a
regression fixture and test another ordinary window transition.

Possible fix depends on the captured cause: bounded transient support retention,
valid visible-bar penetration recovery, or standing z-order correction. Do not
blanket-ignore real full-screen occlusion, freeze all positions, or apply the
perch-only ordering policy to standing without checking intended behavior.
No fix is applied or claimed at this checkpoint; actual transition remains open.

## Follow-up: actual transition captured and occluder identified

The user authorized read-only capture and performed the transitions. Two40-second
traces each collected429 samples, all Fresh, with no reported capture failure:
`bin/Release/net8.0-windows/transition-20261015-140500.json` and
`transition-20261015-140925.json` under the ignored harness directory above.
The second includes crossing-window process/class/style metadata, not titles,
input content or screenshots. This supersedes the earlier uncaptured-event
uncertainty, not the unimplemented-fix or other-app verification boundaries.

Taskbar identity, visibility and bounds stayed stable at1040..1080. The pet's
native336x336 window repeatedly moved from top830 to870: a real40px descent,
not merely z-order concealment. The first sample's unqualified CanSupport state
is the new diagnostic reader's80ms warm-up and is not used as incident evidence.

The second trace identifies the sole qualifying taskbar occluder during four
restored-window episodes:

- Process `Discord`, class `Chrome_WidgetWin_1`, HWND14421498, PID499020.
- Style `0x14030000`; extended style `0x800A8`: layered, transparent/click-through,
  topmost, tool window. Observed bounds `(1,43,1920,1017)`, bottom1060.
- Z-order20, ahead of taskbars28/56. RimWorld itself was behind them at69.
- Calculated primary support shrinks from `[0,1920]` to `[0,1]`; secondary
  support starts at1921 instead of1920, exactly matching the overlay bounds.

One representative sequence, elapsed milliseconds in the second trace:

| ms | Evidence |
| ---: | --- |
|32197|Game restored to `(0,12,1922,1049)`; support still intact; pet top830.|
|32290|Discord overlay crosses bar; primary support becomes `[0,1]`; pet top830.|
|32385 /32478 /32574|Pet descends to838 /862 /870 while overlay persists.|
|36409|Game maximized, overlay no longer crosses bar; full support returns.|
|36595|Pet returns to top830.|

These are independent reads of the accepted DLL's scene/geometry plus the live
pet's native position, not logging injected into the pet loop. Together with the
code path and repeated spatial/temporal match they identify the concrete
misclassification: this click-through Discord overlay is treated as a physical
occluder. Existing NVIDIA-specific exclusion does not cover it. The observed
Discord style is not a popup menu, so the prior Figma menu exception also does
not apply. No claim that WS_EX_TRANSPARENT alone proves every window is harmless.

### Proposed fix and remaining verification

After implementation approval, narrowly classify the observed Discord overlay
using process identity, class and layered/click-through/topmost/tool-window
traits, excluding it from support and occlusion. Keep ordinary Discord windows,
interactive overlay panels, unrelated Chromium apps and real foreground-window
occlusion unchanged. Do not disable Discord or change its settings.

Use the captured geometry/style as regression evidence, include negative
classification cases, and verify restore/maximize repeatedly with the actual
installed correction. Then check another game/window transition where feasible.
Other applications can encounter this shared path with the same overlay, but
native reproduction outside RimWorld has not been performed. Production source,
installed app, artwork, Mac branch behavior and public releases remain unchanged.

## Authorized correction and recheck

User subsequently approved fixing only the overlay while preserving ordinary
Discord-window behavior. This supersedes the earlier no-production-change and
approval-pending checkpoints for the bounded correction below.

`DesktopMetadataReader` now recognizes `Discord.exe` (case insensitive) plus
`Chrome_WidgetWin_1` only with layered, click-through, topmost and tool-window
flags, without child/popup/caption/resizable-frame/app-window traits. The matched
HUD provides neither support nor occlusion. Unknown/denied executable identity,
ordinary Discord windows, interactive or opaque panels and unrelated Chromium
processes retain normal behavior. NVIDIA and transient-menu policies are unchanged.
Physics, motion, artwork, Core and Mac sources are not changed in this work.

Verification:

- Targeted31/31 and win-x64 targeted31/31 passed, zero skips. Sixteen literal
  classifier cases, five native-reader/geometry cases and one40-second repeated
  captured-interval simulation cover Discord; the nine existing NVIDIA cases
  also pass. Two-monitor taskbar edges and ordinary window support/occlusion
  are asserted. The recorded four restore intervals no longer release support.
- Initial targeted run had6 fixture failures: Windows added caption styling to
  the synthetic overlapped HWND, and a window top at43 was below a body-height63
  eligibility threshold. The test helper now restores/asserts the captured style;
  the ordinary support assertion uses body-height20. No production rule was
  weakened to accommodate the fixture. Initial TRX is retained.
- Full App1216/1216 and Core259/259 passed, zero skips. Reports in project
  TestResults: `discord-overlay-full.trx`, `discord-overlay-core.trx`,
  `discord-overlay-targeted-final.trx`, `discord-overlay-win-x64.trx`.
- Product package validator passed exact tested win-x64 App/Core hash parity,
  archive round-trip, metadata/icon/apphost checks and private-desktop native
  start, duplicate exit0 and owner WM_CLOSE exit0.

Local test candidate: `artifacts/product-shell/candidate-20261015-discord-overlay-15/`.

| Artifact | SHA256 |
| --- | --- |
| Portable ZIP | `FCB411EB01FBB4F0CE7F919893B09AB3ECA995AA3F046FCE0AB26E8873978A33` |
| App DLL | `3B398A68BDA8FB98D25552D1EAA43B7E64D9CC912C1AD2DA6B665C349E88BD2C` |
| Core DLL | `956ED054455D49C9C0F90A89A3353F3D4AFC3BB63AF8E0437A8A84AC7F0C1A1F` |

After the user confirmed tray exit, launched the candidate's `runtime/Dororong.exe`
as PID515284. Existing installed files and startup registration were not replaced.
No Setup was built, pinned installer hash changed, commit/push or upload performed.
Public Windows download and Mac test03 ZIP remain unchanged.

The corrected DLL read-only trace is
`artifacts/window-mode-diagnosis-20261015/corrected/bin/Release/net8.0-windows/transition-20261015-144223.json`.
All430 samples were Fresh; the observed Discord HUD style0x14030000/ex0x800A8
had CanSupport=false and CanOcclude=false. Both taskbar surfaces remained full
after diagnostic warm-up and actual pet top stayed833. However, the game stayed
maximized throughout and the HUD bottom reached1040, not1060. This confirms live
classification, **not** the previously failing transition. Native restored-window
acceptance and other-application transitions remain open. The user was asked to
repeat the transition on the running candidate; installation/release is deferred.

## User acceptance and installed update

The user confirmed the pet no longer falls during the transition and requested
updating the installed version. This closes the current-PC RimWorld acceptance
above; it does not establish other-game or other-PC coverage. The user then
confirmed exiting the test candidate through the tray menu.

- Pinned `Build-Installer.ps1` to the accepted candidate15 archive hash above.
  Updated the installer refusal test's stale candidate13 inputs to candidate15.
  Missing input, foreign compiler, mutated archive and existing-output refusal
  tests passed. Existing compiler trust and payload checks were retained.
- Built `artifacts/installer/candidate-20261015-discord-overlay-15/` using the
  pinned Inno7.1.0 compiler. Package/native smoke, staging/compile hash parity,
  product metadata and all10 head-icon sizes passed. Installer is still unsigned.
- Setup SHA256:
  `D35C42775C74DEA04AB7F6B2C5597B0E0FA5DBE453E6BFC0B37DFEE6F33BE484`.
  Local handoff `artifacts/delivery/0.1.3-20261015-discord-overlay/` contains only
  Setup, README and SHA256SUMS. Version remains0.1.3; this is a local correction,
  not a new public release.
- Backup and update evidence:
  `artifacts/installer/host-discord-overlay-update-20261015/`. Preserved the old
  installation, data, shortcut, uninstall registration and candidate14 recovery
  Setup. Baseline includes startup and preference values. No old files deleted.
- Current-user Setup exited0, no reboot. All470 installed payload files and the
  ownership manifest match candidate15. Data, desktop-shortcut preference,
  startup command and preferences match the pre-install snapshot exactly.
  Startup remains enabled for the installed executable; automatic sleep staysOFF.
- Relaunched the **installed** `C:/Users/tjdwo/AppData/Local/Programs/JOEWRKS/Dororong/Dororong.exe`
  as PID521332. Installed App hash matches candidate15; process stayed alive and
  its fresh Started log has no failure. Launch/update reports are in the backup
  directory. The temporary candidate is not the running instance.

Local update is complete. No commit/push, GitHub release replacement, Mac ZIP
change or second login/reboot test was performed. Public Windows candidate14
remains available unchanged until a separate release instruction.

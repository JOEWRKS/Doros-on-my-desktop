# Mac overlay / Dock review

Scope: user requested review of the Windows window-mode fall's equivalent Mac
path. Reviewed Mac test03 source at `20213a36a2d74cb00345264731e6fa8a006cdd82`
on `feature/macos-test03`; no Mac product changes or ZIP replacement.

Sources on that branch: `src/Dororong.Mac/Native/QuartzDesktop.cs`,
`src/Dororong.Portable/Desktop/MacScene.cs`, shared `PlatformGeometry` and
`PlatformMotion`. Mac code is deliberately not merged into Windows main.

## Finding: conditional risk remains

Quartz reads window bounds, owner PID, window layer and whole-window alpha;
bundle identity is used only to identify the Dock. It does not classify other
applications' click-through game overlays. MacSceneMapper marks non-Dock windows
at layer>=0 as occluders, excluding alpha<=0.01/off-screen/own windows. Layers
at/above the menu threshold are transient menus and do not remove Dock support.

Therefore an overlay above the Dock, below the menu threshold, with alpha>0.01
and a rectangle crossing the Dock top can remove its support. PlatformMotion
then drops to the monitor floor. The Windows HWND/class/style fix does not run on
Mac. Whole-window alpha is not proof of opaque pixels throughout its rectangle.
This is a demonstrated code-path risk, **not** proof that Discord on Mac emits
that particular window or that a Mac user has experienced the failure.

## Checks run on Windows

- Existing portable DesktopTests15/15 pass, zero skipped; report
  `tests/Dororong.Portable.Tests/TestResults/mac-overlay-review.trx` in the Mac checkout.
- An ignored diagnostic harness compiles the unchanged MacScene.cs and shared
  Core directly. Evidence:
  `artifacts/window-mode-diagnosis-20261015/mac-review/bin/Release/net8.0/results.json`.
  Each synthetic sequence runs180 ticks at16ms. Dock top700, monitor floor800.

| Supplied metadata | Result |
| --- | --- |
| Normal window restore, behind Dock | Sole stays700; no support loss |
| Front overlay layer25, alpha1, crossing Dock | Support absent180 ticks; sole700→800 |
| Layer0 rectangle deliberately ordered before Dock | Same loss; adversarial row-order case, not a claim about normal Quartz ordering |
| Menu layer101 crossing Dock | Sole stays700 |
| Alpha0 overlay | Sole stays700 |
| Actually auto-hidden Dock (2-point strip) | Drops to800, expected existing policy |

## Remaining boundary / next

Actual Mac first-launch, Dock/full-screen/Spaces transitions and overlay metadata
remain unverified. Obtain the consenting tester's game mode, Dock auto-hide state,
overlay application/bundle/layer/alpha/bounds before selecting a narrow exclusion.
Do not ignore all Discord windows, all nonzero-layer windows, or every real window
crossing Dock. A fix/new Mac ZIP requires a separate implementation instruction.
Windows current-PC acceptance does not close this Mac risk.

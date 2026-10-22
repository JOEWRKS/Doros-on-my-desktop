# Mac r104 test05 — popup theme ownership

## Authority / scope

User authorized fixing Mac popup theme connection, rebuilding Apple Silicon test
app, checking size input and close/reopen in character and status-item menus.
Preserve character art/quality/motion/speed and Windows. No publication, app restart,
installation/settings writes, native input automation or memory/CPU soak this turn.

## Product change

Only `src/Dororong.Mac/Product/PetMenuHost.cs` changes application behavior:

- Establish Popup logical parent with the placement target before opening, so
  Fluent resources/control templates flow to the actual character popup.
- Release parent, placement target, child, Dismiss handler and panel on close;
  cleanup is idempotent on Dispose and attempted-open exceptions.
- Posted focus callback checks the same still-open panel before focusing it.

PetMenuPanel, size callback/coalescing/controller, status-item host, renderer,
simulation, artwork and Windows source are unchanged. No global theme override,
image resampling, timing/speed or slider redesign. The existing tray150ms native
deactivation debounce is retained, not removed to make tests pass.

## Verification

Root: `artifacts/macos/r104-test05-20261022`.

- Original missing-parent diagnosis: see `2026-10-22-mac-slider-visibility.md`.
  Diagnostic fixture no longer supplies its own parent fix; all4 direct/popup
  Light/Dark variants now pass through product code.
- New `SizeMenuHostRegressionTests`: both actual menu hosts x Light/Dark, each
 3 open/adjust/close cycles. Confirms nonzero track/thumb, inherited theme,
  headless pointer dragging from100 to approximately125, Home/End keyboard100/150,
  no dismissal during resize,125 retained into freshly constructed reopened menu.
  Checks duplicate open reuse, Escape, X and snack-command dismissal; popup clears
  logical parent/target/child after close. Different-target/theme reopen, factory
  exception recovery and repeated Dispose also checked.
- Existing popup outside-click dismissal and status host positioning/lifecycle
  checks remain included. Input is Avalonia headless routing, NOT actual macOS
  pointer, Cocoa status-item clicks, Retina or window-server proof.
- Final `mac-final2.trx`: Mac50 tests pass,0fail/skip. Exact original test04
  Core/Portable/Raster DLLs then restored into candidate and host test output.
  `host-with-frozen-renderer.trx`: same50 cases pass (not100 unique cases).
- Actual ARM64 Mac DLL cannot load in x64 Windows test host: `exact-candidate.trx`
  is an aborted run, NOT a native pass. Offline PE comparison confirms host vs
  ARM64 new Mac DLL396 method bodies (including local signatures/EH metadata) and
 12,930,200-byte managed resource blob identical. Architecture remains different.
- Earlier harness errors: missing close-button test property corrected to existing
  accessibility-name lookup; key-up after Escape attempted to use destroyed tray
  window; immediate reopen violated existing150ms debounce. Tests corrected to
  use valid lifecycle/180ms deliberate reopen; product timing was not changed.
- Rendered screenshots for12 host-route/theme/reopen states saved as
  `tests/Dororong.Mac.Tests/bin/Release/net8.0/size-host-*.png`.
  Character Light first-open and tray Dark third-open visually inspected with
  visible track/thumb. Other states checked by geometry/input assertions, not
  claimed as individual manual visual reviews.
- Python package suite11 passed. Packaging verifies236 files,18 native code seals,
  resource/page hashes, ZIP inventory/CRC and Unix execute bits. This is local
  ad-hoc integrity, NOT Apple codesign validation, Developer ID or notarization.

## Preserved binaries / candidate

Source diff against test04 has no Windows/Core/Portable/Raster changes. To avoid
unnecessary rebuilt revision metadata in accepted simulation/render assemblies,
their exact original test04 DLLs are reused before packaging and tested with the
host-compatible new Mac DLL. Final bundle hashes match originals:

- Core `B74C8AA1BDE1F963020934A695000061F417F05FCAFF79F7F6089928B791AC8E`
- Portable `DEED3CD4CC4B6DB7F99B881101CB484E84CAAF4F8FB995E2CCF30DBDFB438887`
- Raster `86C7CBB20A1567CFBFDBB3B68E02AF2D1841FBDA818463C45B83A8F14B31D57F`
- New ARM64 Mac `63C68704FEA2A89984C35107F36C5B1AB299B93DB24F6D91F8DEE2A58D66AF5C`

Local ZIP:
`artifacts/macos/r104-test05-20261022/candidate/Doro-Mac-arm64-r104-test05.zip`

-99,175,856 bytes; SHA256
 `F4BCE5888EEA8CE5F2FDB6280B304C939052B7B4755CFBD0D616BB57688AC08F`.
- Bundle104.5, self-contained.NET8.0.32, Apple Silicon/macOS13+ test target.
- Existing test04 ZIP/public release unchanged. New README identifies popup fix.
- No commit/push/public upload; local artifact only.

## Native gate

Actual M2 launch, Gatekeeper, character right-click popup, Cocoa menu-bar activation,
slider pointer/Retina position, focus/light-dismiss and re-opening are NOT_TESTED.
Essential next check: exit old Mac test app, extract test05, open app; in each menu
verify track/thumb,100↔150 drag without closing, X/Escape/outside-close then reopen.
Prior CPU/GPU/long-memory native limitations are unchanged, not passed by this fix.

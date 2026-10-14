# B2 native menus — local integration, 2026-10-14

Scope: user-accepted B2 applied to existing character/tray commands. This is a
local update, not public distribution or final user acceptance of the running UI.
Visual authority: [DESIGN](../../DESIGN.md#b2-implementation-approval--2026-10-14).

## Verification

- Release win-x64, self-contained .NET8.0.31. App1166/1166 and Core255/255 passed.
  Final App report: `tests/Dororong.App.Tests/TestResults/native-menu-20261014.trx`.
- Native popup/typography checks13/13 passed. Existing left/right routes, standalone
  right release, pending/open disposal, repeated release, reopen, state combinations,
  callback counts, Escape/Enter and activation loss exercised on isolated desktops.
  Keyboard and activation use injected events/messages, not physical input proof.
- Initial keyboard checks exposed Escape and ownerless-menu dismissal gaps.
  Explicit Escape/command closing fixed them; the final full suite includes those
  regressions. The older failed/interrupted run is not the final test result.
- Independent source review caught a right-click test masked by later left clicks;
  now it asserts right-only opening first. B2 vs8 actual WPF RenderTargetBitmap
  captures showed no clipping or command/state composition loss. Captures live in
  `artifacts/ui-menu-20261014/native-proof/`; they are96DPI renders, not desktop photos.
- Embedded Regular/SemiBold/Bold resolve to this assembly's Pretendard, including
  Korean glyphs, rather than an installed-font fallback. OFL hash checked in package.
- Package round-trip, exact tested App/Core parity and isolated executable smoke
  passed. Duplicate exited0; the owned smoke process closed0. Installer refusal
  checks reject missing inputs, foreign compiler, mutated archive and existing output.

## Immutable local candidate

Candidate roots: `artifacts/product-shell/candidate-20261014-native-menu-b2-10/`
and `artifacts/installer/candidate-20261014-native-menu-b2-10/`.

| File | SHA256 |
| --- | --- |
| Portable ZIP | `6E2A383798DA90D1801E93D1B86264016A936ADFEA1B74F57A43DAC89A0A857C` |
| Setup EXE | `5B5D0760AF03C451208C01E4B6066590E50754846045C4B330087BD7023BCE8A` |
| Tested/published App DLL | `FADB8BC7C7A40592A5EA702982925675C135F1BACF31B9D6E446E98A6C0E5171` |

Candidate09 was an intermediate package before the quick-start/font notice update;
it is retained, not installed. The pinned builder now uses candidate10.

## Installed result

- Confirmed no pet process before replacement. Backed up installation, data,
  registration, shortcut and prior candidate08 recovery installer.
- Evidence: `artifacts/installer/host-native-menu-update-20261014/` contains the
  update script, baseline, backups, installer log and `update-result.json`.
- The old backup helper assumed a startup value existed. Initial preflight stopped
  before mutation when it was absent; adjusted the read to represent absence as off.
- Same-version current-user install exited0.470 payload files plus ownership manifest
  match; user data and desktop-shortcut preference preserved. Startup was off before
  and remains off (no Run value added). No restart required.
- Launched installed `C:/Users/tjdwo/AppData/Local/Programs/JOEWRKS/Dororong/Dororong.exe`,
  PID245608. Process path and Started log confirmed; no new diagnostic failure observed.

## Remaining boundaries

- Computer Use returned no targetable pet window after launch; installed process
  and startup log were checked independently. Real pointer clicks, arrow keys,
  Alt+Tab/outside-click and screen-edge placement require local user verification.
- No claim of physical/mixed-DPI or Windows11/other-PC validation. Existing R3 hold
  remains. High-contrast fallback exists but OS high-contrast switching was not tested.
- B2 composition approval is not installed UI acceptance. No commit/push/release,
  rights approval or full JOEDESIGN certification is asserted by these checks.

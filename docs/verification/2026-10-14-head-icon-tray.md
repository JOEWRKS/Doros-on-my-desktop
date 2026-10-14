# Head icons and tray popup stability — 2026-10-14

Scope: authorized local icon replacement and the subsequently reported real tray
right-click flicker. Existing B2 menu composition and character motions preserved.

## Source and correction

- Unmodified isolated head from `artifacts/reference-parts-20261001/parts/head.png`
  preserved as `src/Dororong.App/Assets/dororong-head-source.png`.
  SHA256: `73E2C45B0BE6C61141B9442905BFC9DF99A0509643457B896B3EF8D619150854`.
- Alpha-bound aspect-fit packaging, transparent padding, no repaint. ICO sizes:
  16/20/24/32/40/48/64/96/128/256. Menu resource96px. Apphost, WPF window,
  tray, menu header and setup use the same head; shortcut/uninstall icons resolve
  through the app executable. Installer preserves all ICO frames from frozen DLL.
- Product metadata remains `도로롱이 내 컴에?!`; process filename is `Dororong.exe`.
  Task Manager accessibility was blocked by its higher Windows integrity level.
  Metadata was inspected; actual Task Manager rendering is not verified.
- Former NotifyIcon.ContextMenuStrip opening/cancellation entered the WinForms
  foreground/popup route before queuing WPF. The new route detaches that menu,
  dispatches both MouseUp buttons once, and foregrounds the actual pet owner before
  opening. No post-open foreground call targets the popup. This removes conflicting
  paths identified in code, but the precise physical flicker cause was not reproduced.

## Verification

- Targeted41 checks passed, including source hash,10 PNG ICO frames, transparent
  corners, GDI small sizes, WPF256 frame, title and menu resource loading.
- App full suite1168/1168 passed (`head-tray-full.trx`). After a test-only natural
  dismissal guard and a stale production comment correction, final targeted41/41
  passed (`head-tray-final-targeted.trx`); no production behavior changed between
  these runs. Reports are in `tests/Dororong.App.Tests/TestResults/`.
- Native isolated-desktop checks use a no-activate owner like the pet. Right release
  alone keeps the same popup open through350ms; no legacy Opening event fires.
  Reopen, duplicate releases, callback counts, startup/snack states, pending/open
  disposal, Escape/Enter and outside activation are also covered.
- Rendered head/menu proofs: `artifacts/ui-menu-head-icon-20261014/`, with8 actual
  WPF states under `native-proof/`. Title/close button fit, action hierarchy and
  proportions preserved. These96DPI renders are not physical desktop captures.
- Independent icon/source and tray-path reviews found no scoped production blocker.
  Outside-test natural-dismissal guard added following review.
- Earlier native-menu proof files were regenerated during the first head-icon run;
  subsequent outputs use the dedicated directory above. Accepted B2 browser source
  and `captures-b2/` remain unchanged; old native-proof files are regenerable evidence.

## Delivery status

Candidate roots: `artifacts/product-shell/candidate-20261014-head-icon-tray-12/`
and `artifacts/installer/candidate-20261014-head-icon-tray-12/`.

| File | SHA256 |
| --- | --- |
| Portable ZIP | `346F5321DB83426E22DB0EEBD80CD4592DEB451060FA1E111EBE13676BABBE5D` |
| Setup EXE | `B3190677CC03C5F639A1A2CB01310395FD3C3B42844C94451F9B3D87C7526BBF` |
| Tested/published/installed App DLL | `45C0AA637C5B87EDF5D2797799D6DB40E35C2C519BF06F402703D6575C1F8ED0` |
| App/setup ICO | `2FC229701BE64CA624FFAA5C8CF295BBD39D8EEDB1850383918392DFAEBDDD75` |

- Package exact App/Core parity, archive round-trip and native isolated-desktop
  start/duplicate/close passed. Installer build and refusal tests passed; setup
  icon retains all10 frames and matches tested source bytes. Setup remains unsigned.
- Candidate11 contains icons but predates the tray fix; retained, not installed.
  Candidate10 remains the recovery installer. Builder now pins candidate12.
- User confirmed closure; no pet process was present before update. Installation,
  user data, shortcut and registry backups are under
  `artifacts/installer/host-head-icon-update-20261014/`, alongside the exact update
  script, baseline, update.log, recovery installer and update-result.json.
- Setup exited0. All470 payload files plus ownership manifest match. User data
  and shortcut preference preserved. Startup remains OFF (Run value absent).
- Launched installed `C:/Users/tjdwo/AppData/Local/Programs/JOEWRKS/Dororong/Dororong.exe`,
  PID349828. Process path and Started log confirmed; no new failure in the inspected
  diagnostic tail. Actual taskbar input verification remains with the user.

## Remaining

Real taskbar pointer input/focus stability and exact installed visual acceptance need
user verification. Mixed-DPI, shell icon caching, Windows11/other-PC, high-contrast
switching and full JOEDESIGN certification are not established. Existing R3/R4 holds
and deferred commit/push remain; no public release or permission claim.

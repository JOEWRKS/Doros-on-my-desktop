# Snack play / login startup — local evidence

Scope/acceptance: [feature spec](../specs/2026-10-13-snack-play.md).
Execution owner: [TASKS](../../TASKS.md#snack-play-and-login-startup--2026-10-13).

## Automated and observed results

- Release Core248/248: `artifacts/snack-20261013/results/core-final.trx`.
- Release win-x64 self-contained App1111/1111, before the isolated render-base
  caching change and final tray-menu test: `results/app-final.trx` in that folder.
- Latest code affected-scope App176/176 (snack rendering/session integration,
  startup, tray, platform runtime): `results/app-postcache.trx`. The full1111
  suite was not rerun after this final isolated cache change.
- Targeted12/12 preceded those runs; the initial jump test failed at12DIP.
  It exposed the legacy supported-offset cap. New snack calls permit76DIP,
  preserving the12DIP default for existing interactions. Real platform support
  integration now proves the snack hop reaches height and returns to its floor.
- Native MainWindow/loop smoke: start snack → cancel → close, no diagnostic
  errors. Done through `tools/SnackProof --native-smoke`; not a user acceptance.
- Actual renderer strips inspected on light/dark backgrounds:
  `artifacts/snack-20261013/frames-final.png`. Eating alpha-outline invariance,
  mouth changes and mirrored mouth registration are asserted by tests.
- On-host rendering sample (background regression load,140 measured frames):
  uncached median11.96ms/p9519.55ms; bounded base-cache median1.19ms/p9516.51ms.
  Includes cold cache misses; not a universal FPS guarantee or native input-latency
  measurement. No hidden unbounded frame cache.

## Candidate

`artifacts/product-shell/candidate-20261013-snack-01/runtime/Dororong.exe`

- Portable ZIP SHA256:
  `224A83A86AE577A6E61C7AFEC2710A9E53771B82B0F929361FDA1B10F2E3420A`.
- Existing publisher/package validator passed: pinned Core/Desktop/host8.0.31,
  exact App/Core RID hash parity against tested assemblies, metadata/icon/apphost,
  archive round-trip and isolated-desktop native start/duplicate/WM_CLOSE smoke.
- This is an unpublished local0.1.3 feature candidate. The old installed app and
  earlier candidates remain unchanged. Installer source includes exact-command
  startup cleanup, but a new installer was not built or applied in this task.
- Test startup persistence uses injected storage. Real HKCU Run was not changed
  by automated tests. Reboot/login launch, OS Startup Apps overrides and actual
  uninstall of an opted-in install remain **NOT VERIFIED**.
- No public upload, repository visibility change, release, commit or push.

## Remaining boundaries

- User acceptance of chase speed, jump, mouth/drool and jewel appearance.
- Cursor companion deliberately leaves the ordinary OS pointer unchanged; it
  does not implement a system-wide custom cursor replacement.
- Existing monitor/platform bounds remain; cross-monitor chase traversal is not
  newly implemented or verified. Other-machine/mixed-DPI checks stay in R3 hold.
- Rights/public-distribution gate R4 unchanged; reference-based new jewel is not
  included under the code-only MIT grant.
- The V2 bootstrap candidate failed formal authority/coverage validation; it is
  explicitly non-canonical and unapproved. No full JOEFLOW closure/adoption claim.

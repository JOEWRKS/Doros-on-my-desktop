# Windows login delay and on-demand motion banks — 2026-10-16

## Approved scope

User approved 20 seconds for automatic startup only and loading necessary motion
data, preserving manual startup, pixels and motion timing. Applies to the Windows
login path investigated in the prior review. Mac test04 remains unchanged.
No reboot/logout, remote publication or commit/push is authorized by this change.

## Implementation

- HKCU Run command gains the exact `--login-startup` argument. App waits 20 seconds
  before ProductShell/MainWindow/tray/assets. This is after process launch, not an
  exact 20 seconds from login (Windows may schedule the Run entry later).
  The .NET/WPF process starts before this wait; this defers the pet's initialization,
  not every byte of process/runtime loading and not Windows boot itself.
- The wait holds no product mutex. Manual startup signals a per-user named event,
  cancels pending automatic launches and takes the existing immediate startup path.
  Normal duplicate protection remains in ProductShell; shutdown cancels the wait.
- Registration recognizes the exact legacy command. App/installer migrate only
  this executable's owned legacy opt-in; OFF/foreign paths/unknown args stay intact.
  Uninstaller accepts only the exact legacy or delayed owned command.
- Hunting 161 blocks (157 bodies + 4 head buffers) and locomotion 772 frames now use
  an indexed lossless gzip container. Only requested blocks are decoded; frozen
  outputs are reused, preserving captured object identity. No frame skipping,
  lower resolution, palette, interpolation, curve or duration edits.
- `tools/Build-IndexedMotionBanks.py` deterministically converts the original
  approved compressed banks. Originals are retained for Mac/reference validation;
  Windows embeds only the indexed replacements. This is byte packing, not redraw.

## Verification / handoff

- Full App regression: **1,233 passed**, no skips/failures. Evidence:
  `tests/Dororong.App.Tests/TestResults/startup-lazy-full.trx`.
- A subsequent registration readback correction rejects a failed write that leaves
  the old command intact. Final-source targeted suite: **36 passed**, including
  the new readback test, production 20-second wait, manual cancellation, migration,
  lazy counters and exact comparison of all **933** decoded blocks. Evidence:
  `tests/Dororong.App.Tests/TestResults/startup-lazy-final-targeted.trx`.
  The full suite predates this small correction; it was not rerun unnecessarily.
- Compiled installer ownership-policy harness passed with the pinned compiler at
  `artifacts/installer/toolchain-reference-update-20261002/ISCC.exe`.
  Evidence: `artifacts/installer/.task-1002-test-b774a362a30442d08bf65e3be8ef7aae/`.
  This tests legacy/delayed ownership and rejection boundaries, not an actual
  installed upgrade/removal. The first attempt used a missing default compiler
  path; the explicit pinned path above resolved that tooling issue.
- Final cold decode sample: 929 bitmap frames, median 0.1831 ms, p95 0.7328 ms,
  max 1.4601 ms on this run. Source report:
  `tests/Dororong.App.Tests/bin/Release/net8.0-windows/win-x64/indexed-motion-timings.json`.
  This does not guarantee frame-time ceilings on other PCs.
- Local portable candidate prepared:
  `artifacts/product-shell/candidate-20261016-startup-lazy-16/`.
  ZIP SHA256: `9648BC4E4E5DA086A2FFB84E774BCD4C08DFA12B98F1DF4BA62924942AFBE9F9`.
  All **470** ZIP entries match runtime file lengths/hashes. Published App/Core
  DLLs match the final tested RID DLLs exactly:
  - App: `BC611E978B605CDB0F1FCB5CAD458E1B11F6AC556B46902528996FD432108086`
  - Core: `956ED054455D49C9C0F90A89A3353F3D4AFC3BB63AF8E0437A8A84AC7F0C1A1F`
- **Earlier handoff: native package/startup checks, Setup build and installed update pending.**
  The installed candidate15 was still running (PID 18724) at handoff. Requested
  tray exit; did not terminate it or bypass the package validator's native gate.
  At that checkpoint installed files, Run registration, settings and public files
  were unchanged. Installer archive pin remained candidate15 until the native gate.
  No claim of installed acceptance, reboot, commit/push or publication.

## Authorized local replacement — 2026-10-16

User confirmed tray exit and authorized execution checks and installed replacement.
This supersedes the pending package/install status above, not the next-login or
other-environment limits. The old process was absent before tests and installation.

- Added `tests/Dororong.LoginStartupPackage.Tests.ps1`, reusing the package test's
  private-desktop harness with a fixed login argument and visible-window counting.
  The initial harness attempt failed on an empty desktop: `EnumDesktopWindows`
  returned false with error 0. The harness now treats only that error-free empty
  list as empty and still throws on actual API errors. It cleaned up only its own
  process; no product change or validation bypass was needed.
- Candidate native startup passed: first automatic window at 28,396.72 ms from
  process launch, manual override at 1,063.63 ms. Pending login canceled, duplicate
  exited 0 without stopping the owner, owners closed normally with exit 0.
  Evidence: `artifacts/product-shell/candidate-20261016-startup-lazy-16/login-native.json`.
  The delay is 20 seconds before pet initialization; loading/initialization adds
  time. These measurements are not a cold-login or boot benchmark.
- Full package validator passed: exact tested DLLs, pinned Core/Desktop/host 8.0.31,
  metadata/icon/apphost binding, ZIP round-trip and isolated native smoke.
  `artifacts/installer/candidate-20261016-startup-lazy-16/package-validation.log`.
  Builder input-refusal checks passed for missing input, foreign compiler,
  mutated archive and existing output. The new archive is now explicitly pinned.
- Setup built with the pinned, verified compiler; signature remains **NotSigned**
  under the existing user decision, not a newly signed release.
  `artifacts/installer/candidate-20261016-startup-lazy-16/Doros-on-my-desktop-Setup-0.1.3-win-x64.exe`
  is 65,405,574 bytes, SHA256
  `FAA189875DACF616E69BAEBD55D813101D58A45EF442E9A0316553AE503452C3`.
  Build evidence, sources, inventory and hashes are in that directory.
- Before replacement, backed up every installed file and existing user-data file
  with hash checks, shortcuts, registration/preferences snapshot and the exact
  candidate15 recovery Setup. Backup/evidence root:
  `artifacts/installer/host-startup-lazy-update-20261016/`.
  No old candidate, backup or user-data history was deleted.
- Silent current-user update exited 0. All 470 installed payload files and the
  ownership manifest match candidate16. Existing user data and preferences are
  unchanged; automatic sleep remains OFF and the desktop shortcut remains absent.
  The Start-menu shortcut targets the same installed executable.
- Automatic startup remains ON. Only its owned command changed from the quoted
  installed executable to that same command plus ` --login-startup`. This was
  verified before launching the installed app; no registry opt-in toggle occurred.
  `update-result.json` and `baseline.json` under the backup root are authoritative.
- Installed-path native test passed: automatic first window at **21,408.19 ms**,
  manual override at **1,152.73 ms**, pending cancellation/duplicate/normal close
  all passed. `installed-login-native.json` in the backup root. This checks the
  installed process, not Windows actually invoking the Run key on a new login.
- Relaunched the installed product normally for the user, PID **15572**; stayed
  alive and wrote `Started` without a failure in the observed launch log.
  `launch-result.json` in the backup root. User visual acceptance is not inferred.
- No reboot/logout, actual uninstall, commit/push, public replacement or Mac ZIP
  modification. Next: ordinary use and next normal login/boot feedback.

## Measured loading difference and limits

The same CPU-side probe rendered 120 idle samples (not elapsed animation time).
Three installed-baseline runs allocated 44,981,832 bytes and took 126.77–133.25 ms;
three optimized runs allocated 2,531,144–2,531,152 bytes and took 16.19–17.85 ms.
Working set at that point was about 125 MB versus 83 MB. This is a probe result,
not a measured improvement in Windows boot time or a complete app RAM benchmark.

Raw evidence: `artifacts/startup-scale-20261016/probe-{1,2,3}.json` and
`optimized-{1,2,3}.json`, with the probe source in that directory. Baseline App
SHA256 is `3B398A68BDA8FB98D25552D1EAA43B7E64D9CC912C1AD2DA6B665C349E88BD2C`;
optimized probe checkpoint SHA256 is
`6F00F6E22862E74B55DD60C5BEC4AEC3F78E1FA77B1E1F1CE5A223AC6F7AF1D9`.
That checkpoint predates only the registration readback fix and unused-import
cleanup; its motion loader is identical to the final candidate.

Large hunting/locomotion banks no longer decode unused frames. Already-lazy
interaction banks remain as before; a small shared PNG set still loads at presenter
construction. This is not a claim that all startup work or all textures are lazy.

## Open items retained

- Next normal login: verify 20-second behavior and measured boot/login impact;
  current microbenchmarks are not a cold-boot causal measurement.
- Mac test04 actual launch, Gatekeeper, Dock-front z-order, ordinary windows,
  fullscreen/Spaces, Retina, input/menu/login/lifecycle still need a real Mac.
- Enlargement is not implemented: 1.25×/1.5× proof needs a separate decision;
  smooth larger artwork needs a higher-resolution motion set.
- R3 other Windows environments and R4 asset/character rights remain open.
- No Mac remote test04 publication or Windows public replacement this task.

# v0.1.3 installer hardening — current-host lifecycle

Scope: user-authorized local candidate preparation and update, normal exit,
uninstall/reinstall and existing-data checks. No commit, push, release, signing,
account/security-setting change or rights clearance. This is the maintainer's
existing Windows host, not clean-PC or cross-PC acceptance.

## Candidate and recovery

- New installer: `artifacts/installer/candidate-20261002-security-lifecycle-01/`.
  Setup SHA256:
  `7FEA86E01163679EFF5AF42EC31C2D8C73D215DDF82A92DC159C19468F458565`.
  Version remains0.1.3; the installer uses the new incoming-file collision guard.
- App payload is unchanged from `candidate-20261002-window-layer-01`:469 files,
  bundled .NET8.0.31. Portable ZIP SHA256:
  `F31BE3457B0DA75E7743479E2724117A57017A2AFD65D864DA009AB562A6AF7A`.
- Reused the separately pinned October2 local builder; the tracked builder's
  historical archive pin was not weakened or changed. Compiler7.1.0 hash,
  trusted Pyrsys signature/provenance, source/generated input hashes and payload
  parity checked before/after compilation. Setup remains unsigned.
- Fresh package validation passed: exact App/Core tested-RID hashes, runtime/host
  pack hashes, metadata/icon/notices, ZIP round-trip, isolated native startup,
  duplicate exit0 and owner exit0 via WM_CLOSE. This is not a host tray-exit test.
- Before host mutation: no product process; non-elevated token; exact registration,
  current469-file payload/ownership, no unexpected installation extras or reparse
  entries. Verified backup of all472 installation files, one user log file,
  Start shortcut and uninstall registration. Desktop shortcut was absent.
- Recovery installer copied and verified:
  `A2022DA21B4076080FDB9FCEE3CA174BCED82C4E54FF115CF3E5870A845370EB`.
  Backups and old candidates remain under ignored local artifacts. Recovery is
  available but has not been exercised; no power-loss rollback guarantee.

## Actual host results

Evidence: `artifacts/installer/host-security-lifecycle-20261002/`.
The task-specific `Verify-Host.ps1` pins the exact candidate/recovery installers,
checks recovery copies before each phase and refuses a running product.

| Phase | Result / evidence |
| --- | --- |
| Same-version0.1.3 update | PASS, Setup exit0; all469 payload hashes and current ownership match; registration and Start shortcut target correct; desktop opt-out and user-data bytes preserved; no silent auto-launch. `update-process.json`, `update-verified.json`, `update.log`. |
| Installed start / duplicate | PASS, installed PID57080 remains alive, duplicate exits0; original log byte prefix intact. `updated-launch-result.json`. |
| Normal tray exit | PASS, user confirmed tray exit; matching Started/Stopped events for PID57080, no remaining pet process and original log prefix intact. `normal-exit-verified.json`; post-exit log SHA256 `2A6D5ACBFE9171B85D99EC951C8D955F7F6A061C5EFB69CF5E3F77932DC01B4C`. |
| Uninstall / data preservation | PASS, verified new uninstaller exit0; installation root, registration and shortcuts actually absent after bounded cleanup wait; post-exit user log exactly unchanged. `uninstall-process.json`, `uninstall-verified.json`, `uninstall.log`. |
| Reinstall | PASS, Setup exit0; all469 payload files and ownership match, registration/Start shortcut restored, desktop opt-out and post-exit log preserved; no silent auto-launch. `reinstall-process.json`, `reinstall-verified.json`, `reinstall.log`. |
| Reinstalled start / duplicate | PASS, installed PID1240 alive, duplicate exit0, exactly one pet; actual executable and loaded coreclr paths inside installation and runtime hash matches manifest. Original data prefix intact. `launch-result.json`. |

The previous program installation was removed by its verified uninstaller and
then restored by the new Setup, not by deleting directories manually. All
recovery copies remain. Log changes from explicitly starting/stopping the app
are expected append-only diagnostics; removal and reinstall did not modify them.
Reinstalled app was left running for the user. Its visible behavior was not
newly accepted in this test. The prior update provided the normal tray-exit
check; the final reinstalled instance was not force-closed.

Re-ran `node --test` after the lifecycle:41/41 pass, zero failures/skips. Product
App/Core sources were not changed or rebuilt in this task; prior full-suite
Core241/App1099 evidence remains the source-test reference. Build evidence's
`lifecycleValidation: NOT RUN` is its immutable pre-install build snapshot;
this later report and host-phase JSON files supply actual installation evidence.

## Boundaries

No unexpected user extras were present in this installation; preservation is
verified for the existing user log. Incoming collision refusal was tested with
the compiled shared policy harness and a failing accessibility-only mutation in
the prior fix task, not by corrupting this host's ownership metadata.
The existing check/copy race against unrelated simultaneous same-user writers
remains. Neither this test nor the source audit certifies current CVE absence,
all Windows environments or character distribution permission. Other-PC/DPI
coverage and rights clearance remain open.

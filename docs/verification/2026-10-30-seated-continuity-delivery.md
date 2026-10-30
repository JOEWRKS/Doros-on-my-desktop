# Seated continuity — Windows installation / Mac test13 delivery

User authorized Mac correction, commit/push, installed Windows replacement and
direct downloads. The bounded fix and Windows21-test result are in
[the reproduction record](2026-10-30-seated-pointer-continuity.md).

## Mac correction and checks

The Mac runtime extraction still passed world-motion suspension directly to pose
blocking at four handoffs. Applied the same retained-supported-sitting helper.
No raster, authored shape, timing, slider, native screen bounds or landing edits.
Test13/bundle104.13 identifies this package; test12 ribbon corrections included.

- Windows-host Mac controller before correction:3/3expected failures, standing
  source(-1) instead of seated(321), at100/125/150%.
- After:295/295Mac source/controller tests pass, including short and prolonged
  query outages during seated paw reaction and invariant world sole.
- Packaging/launcher mock tests14/14pass. These are not native Mac executions.
- First test compilation incorrectly used the WPF property name; corrected to
  the Mac presenter's existing `Face` before the reproduction run. Initial compile
  failure retained in `artifacts/seated-mac-before.log`, not counted as a test pass.
- `artifacts/macos/seated-test13-20261030/candidate/verification.json`:237files,
  18native code hashes, resource seals, ZIP CRC/inventory/executable modes verified.
  Publish/package4managed DLLs match (`managed-identity.json`).
- ZIP97,927,295bytes; SHA256
  `88173636c8f6c9852a375e304e10e3ed6f815a1e7c9f24bf381c9575ac993745`.
- Apple Silicon arm64, macOS13+ target, bundled.NET8.0.32; ad-hoc integrity only,
  no Developer ID/notarization. M2/native launch/Gatekeeper/Retina/CPU/GPU memory
  remain NOT_TESTED. No unchanged long soak or full authored-frame parity rerun.

## Windows installer and actual host update

- Latest portable ZIP is unchanged from the tested prior turn:
  `f09b9f367807a4cdc9fc4e1644cf03dcddf5ed84c89aed07bb32bd66b423fce2`.
- Builder pin/fixture updated.4refusal checks pass. Required native startup,
  duplicate exit0 and normal close0 passed on isolated desktop (PID71312).
- `artifacts/installer/candidate-seated-continuity-20261030/`:470payload files
  verified before/after compile. Inno7.1.0trusted compiler unchanged.
- Setup77,653,396bytes; SHA256
  `5646e760425d619c256da5c70243ca2dec8e4a09bcd56c8f4787a91e9ac80b78`.
  Setup0.1.3unsigned, same-version replacement supported.
- Old installed PID19480 was identity checked and closed normally via WM_CLOSE.
  No forced process termination. Full prior installation/data, shortcuts and
  registration/preferences backed up under
  `artifacts/installer/host-seated-continuity-20261030/`.
- Setup exit0; all470installed payload files and ownership manifest exact.
  Data, preferences, desktop shortcut preference, Start menu target verified.
  Startup command retained exactly with `--login-startup`.
- New installed PID32788, UTC2026-10-30T13:07:29.5840747Z, same installation path,
  responsive with1visible window. App DLLSHA256
  `9b79d6326fddbdf98bd389b12dd1a3b6ab00876392da20c7ae5769a7974a28be`.
  `update-result.json` and `owner-reopened.json` are the installation evidence.
- This supersedes the prior record's unchanged-installation boundary. No actual
  user mouse acceptance of this revision, other-PC installation or reboot tested.

## Publication

Local artifacts ready. Source commit/push and direct release verification will
be recorded here after completion. Previous release assets will be retained.
The original user's transient native trigger remains unrecorded; the regression
demonstrates and fixes the reproduced cause without claiming incident telemetry.

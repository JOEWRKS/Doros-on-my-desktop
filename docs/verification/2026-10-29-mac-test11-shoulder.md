# Accepted shoulder correction / Mac test11 — 2026-10-29

User accepted the running Windows v5 and requested commit/push plus the Mac update.
The accepted source was pushed as `0951ad4fc0bcc67ac6f1a8f9b488fd684289553f`
to `feature/macos-r104-arm64`; remote branch identity verified.

## Scope

The shared Stomp/Stretch partials preserve the accepted arm size, rigid transform,
timing and original head. Stretch separates the leading paw from the supporting
leg; the upper shoulder follows the original contour upward behind the head.
Both Windows and Mac body samplers invoke the same reaching-paw composition.
Windows acceptance and visual evidence are in
[the shoulder record](2026-10-29-shoulder-behind-head.md).

Mac test11 updates the package/diagnostic identity to104.11 and documents the
accepted correction. Existing menu theme, slider, native frame/topology and landing
implementations are unchanged. No new asset cache or retained frame collection.
The running Windows PID38720, installation, settings and startup remain untouched.
Unrelated AGENTS.md/.joeness edits excluded from commits.

## Exact distributed-code verification

Evidence: `artifacts/macos/shoulder-test11-20261029/`.

- `tests/mac-test11.trx`:292passed,0failed,0skipped.
- `tests/raster-test11.trx`:109passed,0failed,0skipped.
- `tests/packaging.log`:14packaging/launcher tests pass. The diagnostic launcher
  is optional and simulated with stubbed Mac tools; testers need only open the app.
- The osx-arm64 Mac DLL differed from the ordinary test-build DLL; no test identity
  equivalence was assumed. Raster/Core/Portable matched. Copied all four published
  DLLs into a fresh isolated test directory for an attempted package-code regression.
  This attempt failed assembly loading and aborted:245failed,1passed of246executed,
  not a passing run. `assembly-architecture.json` confirms the published Mac DLL's
  PE machine0xAA64 (ARM64), versus0x014C for the host test build; Windows x64 cannot
  load that executable assembly. The architecture-neutral packaged Raster/Core
  successfully ran the separate4,848-frame comparison. No package binary was
  rewritten to bypass architecture. `tests/mac-distributed-test11.trx` is retained.
- `distributed-code.json`: Mac/Raster/Core/Portable DLLs are byte-identical between
  that attempted test input, osx-arm64 publish and signed package contents. This
  identity check is not an execution pass for the ARM64 Mac assembly. The292pass
  count above applies only to the Windows-host build of the same Mac source.
- Exact published Raster/Core copied into a Windows-host probe with hashes checked;
  all4,848 frame IDs and active-pixel hashes match the accepted Windows v5 reference.
  `distributed-probe-identity.json` / `distributed-parity-result.json` record this.
  Raster SHA256: `C5C61D5AC144956872093E0A09708138F3CCCF00D5AD0A8C4F7ED87D7A22AE1A`.
- This repeats pixel arithmetic on the distributed code, not native Mac screenshots.
  Previous2706-frame visual evidence is reused for identical pixels; no new
  all-frame direct visual-inspection claim. No unchanged long memory soak repeated.
- Existing unused `_locomotionStarted` field warning remains; build/test errors0.

## Test application

`candidate/Doro-Mac-arm64-r104-test11.zip`,97,926,327bytes.
SHA256: `0d0ca46dcb4cd8e3475f027b6b95ad9f0e1db056fff4254ab47d30358110a24a`.
Apple Silicon arm64, macOS13+ target, bundled.NET8.0.32. Version104.11.
`candidate/verification.json`:237files,18native CodeDirectory checks, managed
resource seals and ZIP content/CRC/executable modes checked. Ad-hoc integrity
signature only; no Developer ID/notarization or native Apple codesign claim.
The package is local; this turn did not publish a GitHub release or replace test10.

Quit the old Mac pet, Finder-extract the ZIP, move Doro Mac Test.app to Applications,
then open it. Settings remain in the existing user location. No command/log request.
The changed native check is stretch/paw separation and stamp shoulder behind hair
at100/125/150%, including raising/striking/lowering. Native M2 execution, Retina,
CPU/native upload memory, external/clamshell and landing gates remain NOT_TESTED.

Accepted source delivery and local package are complete; real Mac acceptance
remains open. Mac test11 packaging/evidence is committed separately from0951ad4.

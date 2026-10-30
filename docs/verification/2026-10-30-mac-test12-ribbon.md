# Accepted ribbon correction / Mac test12 — 2026-10-30

User accepted the connected ribbon fold and authorized updating the Mac version
and committing/pushing. Shared/Windows source was committed and pushed as
`92fa865` on `feature/macos-r104-arm64`. Appearance and focused tests are in
[the ribbon correction record](2026-10-30-ribbon-junction.md).

## Mac scope

Test12 (bundle104.12) includes the same bounded head preparation: remove the
exterior spur, connect the inner fold to the right contour, erase the old downward
tip. Shared animation geometry, timing, slider, native screen/topology and landing
code remain intact. Package/optional diagnostic identity and tester README updated.
No running Windows app, installation, settings or startup configuration changed.

## Exact package and checks

Evidence: `artifacts/macos/ribbon-test12-20261030/`.

- `candidate/Doro-Mac-arm64-r104-test12.zip`:97,926,910bytes.
  SHA256 `25d46ab540f118de3aa32b44d73826c51738005375aefe870442f8f189a7ed43`.
- Apple Silicon arm64; macOS13+ target; bundled.NET8.0.32.
  `candidate/verification.json`:237files,18native CodeDirectory checks, managed
  resource seals, ZIP CRC/inventory/executable modes verified. Ad-hoc integrity
  only; no Developer ID/notarization or Apple-native codesign/Gatekeeper verdict.
- `tests/mac-test12-final.trx`:292passed,0failed,0skipped on Windows host.
  This tests the Mac source built for the host, not the ARM64 Mac executable.
- `artifacts/mac-test12-packaging-tests-final.log`:14tests passed. Optional
  diagnostic launcher uses stubbed Mac tools; this is not a native launcher run.
- `distributed-verification.json`:4,848unique frame IDs and active-pixel hashes
  match the accepted Windows candidate exactly. Windows reference DLL SHA256
  `313B5798C0B6A26B9921AD4B038ECA1EF1D6AD11DE0B83F3801EBCBF4C68A6E9`.
- Exact published Raster/Core DLLs were copied into an isolated Windows-host
  probe, and their hashes match the final package. Mac/Portable DLLs also match
  publish versus package. The ARM64 Mac assembly itself was not executed on Windows.
  Raster SHA256 `7774ab6d1ae2763db4cd080b807d75cf8a7b7cba6a6a1ef4233e8654ab11d568`.
- Pixel comparison covers100/125/150%,both facings, seated/groom/stretch/beg/
  question sequences and sampled anger/chuckle with Retina-density checkpoints.
  Arithmetic parity is not direct visual inspection of all4,848frames. The latest
  user-reference visual inspection remains the72pose samples in the ribbon record.

## Initial failed attempts and recovery

Concurrent publish/test builds collided on the Portable project's intermediate
cache (MSB3491). The test build then remained without progress and was explicitly
stopped; its MSB4166 termination is not a pass. Only this task's diagnostic/build
process tree was stopped, never the desktop pet. The publish was rerun sequentially
with one build node and succeeded; host tests were rerun separately with one node
and shared compilation disabled, producing the292-test result above.

The launcher simulation also exceeded its60second subprocess guard in a no-start
case. Its stubs emit their full result synchronously; simulating30/120poll iterations
spawns many unnecessary Windows shell processes. The test fixture now uses2/3polls
while preserving both timeout paths. Production waits remain30/120seconds unchanged.
The initial run was stopped and retained; all14tests pass in the final run.
Initial/final logs remain under `artifacts/mac-test12-*.log`.

## Delivery boundaries

Local test12 ZIP complete; source/package metadata committed and pushed separately
from the accepted shared-code commit. No GitHub release upload or replacement of
prior public archives was requested/performed. Native M2 launch, Retina display,
CPU/native upload memory, external/clamshell behavior and Gatekeeper remain NOT_TESTED.
No unchanged memory soak was repeated. User need only quit the old Mac pet, extract
the ZIP, move Doro Mac Test.app into Applications and open it; no log request.

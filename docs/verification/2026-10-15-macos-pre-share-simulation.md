# Mac test02 — pre-share Windows simulation

Scope: user requested additional checks before sharing. Diagnostic code and
records only; no production fix, repackaging, installation, commit/push or upload.

## Target and method

- ZIP: `artifacts/macos/tester-20261015-arm64-02/Doro-Mac-arm64-test02.zip`.
- SHA256: `db9490d67043bb28cdc6093685a93530e79378272e099684a4abe5e7c8cc7db3`.
- Rechecked234 ZIP entries against staged bytes/paths/modes, CRC, and18 native
  code hashes/resource seals. This is integrity verification, not Apple trust.
- New `tools/mac-build/pre-share-check` references distributed DLLs directly;
  Windows native Skia provides the rendering backend. No source rebuild replaces
  the product assemblies. Loaded Portable SHA256
  `DBB61FB0C30E18F2E140711224AC68DEAD9E69B36EC48EAE9D69F8ACBAF4885F`;
  Core `0759E47C2FAE5A268FF5915FE9DD5D15FF89C30BEDBBBE8E6124189535B0B992`.
- First run: `pre-share-simulation.json`; confirmatory run:
  `pre-share-simulation-confirm.json`, both beside the ZIP. Reports are retained
  separately; the second adds coupled controller feeding/walking checks.

## Results

- No assertion/exception failures in tested managed paths. Complete meal phases
  at30/60/120fps; coupled desktop chase→eat→happy→settle→none and autonomous walk
  resumed. Visibility and premultiplied pixel checks passed.
- Seed2151015:2,400 mixed controller frames, moving/removed windows, Dock presence,
  monitor removal/negative coordinates, menu flag, grab/drop, snack cancel,
  sitting/sleep setting, missing pointer and long elapsed gaps. Finite positions,
  bounded desktop area, cancellation, visible valid frames passed. This random
  mix did not prove every cheek/paw/rest state; no claim of complete coverage.
- Confirmatory controller timing: median0.172ms, p95 10.288ms, max126.309ms on this
  Windows host. Not an M1 benchmark or a guarantee of frame rate.
- Read-only review covered normal session start/disposal/error paths, native
  pointer/window bridge, callback lifetime and login wrapper. Windows headless
  success cannot validate Objective-C calls, OS APIs, focus or permissions.

## Confirmed performance issue — unresolved

Historical finding in test02. Subsequently addressed for test03 by the separately
authorized [bounded effect-cache fix](2026-10-15-macos-effect-cache-fix.md).
Measurements below remain the original diagnosis; test02 itself is unchanged.

First eating frame synchronously took476.224ms then592.612ms in two fresh
processes; after eight running poses evicted its cache, the next meal took
457.557ms then523.620ms. Each eating-group load allocated approximately15.9MB
on the calling thread. A warm eating frame took0.121–0.226ms.

Cause: `SnackEffectBank.Render` decompresses and materializes all48,001 eating
patches in one call. Its four-group FIFO evicts that group while running/chasing.
`DesktopSession.Tick` calls the renderer on the UI thread, so this is a synchronous
render stall, not merely slow build/startup. Exact delay on Mac is unmeasured.
The harness reports successful functional execution **and timing warnings**;
PASS/exit0 does not mean this issue is resolved. Initial chase cost varied48–228ms
and should also be measured after any optimization.

Recommended next fix (not applied): bounded independently compressed blocks,
following the join bank approach, decode only needed adjacent samples and retain
a bounded cache. Preserve original patch bytes and continuous interpolation;
verify all samples losslessly, then repeat first/reloaded meal timings before
creating a new ZIP. Do not trade the accepted appearance for lower sample quality.

## Handoff boundary

Recommend fixing the reproducible stall before sharing test02 for general use.
No crash was found in this bounded simulation; this is not a clean bill of health.
Actual Mac launch/Gatekeeper, native menu and input, Retina/Spaces/Dock, permissions,
login, update/removal and visual acceptance remain unverified. Existing ad-hoc-only
signature/no-notarization warning remains. Windows product/assets/installer and
the exact test02 ZIP were not changed.

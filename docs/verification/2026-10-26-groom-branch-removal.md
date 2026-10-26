# Grooming branch removal — 2026-10-26

The user rejected the downward twig still visible in the junction candidate.
The previous visual no-defect judgment is superseded for this defect; its test
counts do not establish that the requested appearance was achieved.

## Cause and bounded correction

The seated source foreleg contained both the required diagonal connection and
a downward branch. The previous cleanup began at authored y82.5 and retained
the upper branch. Its protected-pixel test incorrectly treated that branch as
required crease ink. Removal now starts at y78 along a narrow diagonal mask,
keeping the main contour and upper junction. Source cleanup remains active
through raising and lowering; the first attempt faded it with paw cover and
visibly brought the branch back during lowering, so that attempt was rejected.

Changed product code: HighResolutionGrabRaster.Chuckle.cs. Paw geometry, body
geometry, motion timing, artwork, Core, Mac, settings and installation unchanged.
No new frame buffers or caches. No heap/trace/forced GC or live input performed.

## Verification

- Final related tests: 37 passed, 0 failed, 0 skipped; evidence
  `artifacts/groom-branch-20261026/tests/branch-v2.trx`.
- An earlier run had three test-fixture failures from an empty body sample;
  the fixture now supplies the actual source body. That run is not final proof.
- All 1,374 chronological WPF frames (0..228, 60fps; 100/125/150%, both facings)
  directly inspected as 72 sheets assembled into 12 montages. No frames omitted.
  Examined raise, wipe and lower for the downward branch and main connection;
  no remaining branch observed in this bounded sequence.
- Pixel checks: zero changes outside the grooming ROI, identical soles and
  endpoints; 396 existing motion samples exactly match the accepted baseline.
- Evidence: `artifacts/groom-branch-20261026/summary.json`, `visual-review.json`,
  `before-after-f70.png`, and `frame-review.html`. Images are renderer outputs,
  not live desktop captures. The old evidence remains unchanged.

## Package and limits

- Package: `artifacts/product-shell/candidate-groom-branch-20261026/`.
- App SHA256: `0C56E05D4950B89D1036B7F89AEE3ADCEF2F937B8F667E87EAEB1472EBD0862A`.
- Core SHA256: `46358E2D9EC0452DA6BEE40C64BFF0A20E7EE7819D1103A1D091BBE9D18ED29A`.
- ZIP SHA256: `99E4107EC5EB0E97A26826F4CD993A930B1B44EE8B262EEB55DEA0D32ED0B72A`.
- Publish script verified packaged App/Core match tested binaries.
- At final process query no Dororong/Dororong.App process was present. No process
  was terminated or launched in this correction turn. Live replacement/acceptance
  and nativeMac verification remain pending. No commit or push performed.

Next: use this candidate for replacement, retaining the live acceptance boundary.

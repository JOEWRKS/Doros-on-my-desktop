# Shoulder contour continued behind hair — 2026-10-29

The user's red line clarified that v4 incorrectly lowered the shoulder by trimming
it. The intended correction continues the existing shoulder curve upward behind
the head. v4 is rejected, and its previous inspection does not imply acceptance.

## Correction

In `HighResolutionGrabRaster.Stomp.cs`, remove the proximal edge clipping and
extend the moving body layer upward from the old y68 cutoff to y60–64, fading
with deployment. This samples the original body contour and lets the original
head layer occlude it. No stroke is painted over the hair or face. The distal
forepaw, arm matrix, ink width and motion timing are unchanged. Stretch shares
this same compositor. Only this source file changed in this follow-up.

## Evidence

Root: `artifacts/stretch-stomp-repair-20261029/shoulder-behind-head/`.

- Exact App SHA256: `FD3384C4378ADB33AF0FAE65471548C930C32A4B7EF9D4A683B1B87823275BD9`,
  recorded after product/probe copy verification in `full/identity.json`.
- 2,706 target frames at60Hz,100/125/150%,both facings. Exact-pixel duplicate
  junctions were grouped after mirroring left-facing crops for comparison;
  all804 unique junction images directly inspected on6 enlarged sheets, plus
  24 whole-character key poses and the same-time before/after closeup. Every
  source frame maps to its reviewed image in `review-index.json`. This is rendered
  image review, not live desktop observation or individually opened2706screenshots.
- The restored shoulder edge rises behind the hair; no exposed cut-cap shelf,
  gap or detached stroke observed in reviewed junctions. `closeup.png` shows
  v4/current at1.00s/150%; `../frame-review-v5.html` compares the full sequence.
- All282 accepted stamp geometry/pose records exact.2,210 images differ fromv4
  only in the upper foreleg region; changed bounds recorded in review-index.json.
- `../tests/shoulder-behind-head.trx`:48passed,0failed,0skipped.
- `parity-result.json`:4,848Windows/Mac managed frame hashes match;3,636 unrelated
  motion frames match accepted baseline. Windows build0warnings/errors; Mac
  managed build retains the existing unused-field warning,0errors.

## Status / limits

At comparison delivery, user acceptance and package/live/nativeMac validation were
pending. The subsequent authorized local package and launch are recorded below.
NativeMac and live motion acceptance remain unverified.

## Local application and launch

User requested “이대로 적용해서 실행해봐”. Applied the v5 source to a fresh
`artifacts/product-shell/candidate-shoulder-behind-head-20261029/runtime/`
portable candidate using `tools/Publish-Product.ps1` and pinned runtime8.0.31.

- RID-specific related tests:48passed/0failed/0skipped in
  `../tests/shoulder-behind-head-win-x64.trx` relative to the evidence root.
- Exact published assemblies loaded by Render-Verified;24 representative PNGs
  byte-identical to reviewed v5 (`package-check/identity.json`,
  `package-frame-comparison.json`). Prior complete sequence evidence remains above.
- ZIP SHA256: `3D777A60FA34BB9F26C478DCBAEE4379E0798C92FE059D5BBC8AF6BDFBE54144`.
- No existing pet process detected before launch. PID38720 started at
  `2026-10-29T09:16:30.2501288Z`; diagnostic Started at09:16:35UTC.
  Process responds; EnumWindows confirms visible owned window12716868.
  `launch.json` and `window-check.json` record the actual candidate path and checks.
- Preferences and startup registration compare unchanged before/after
  (`settings-comparison.json`). No installed-file replacement, commit, push or
  release publication. No automated pet input or live animation acceptance claimed.

The candidate was run for direct observation. User subsequently replied
“좋아 이대로 커밋 푸시하고 맥버전도 수정 드가”, accepting this Windows candidate
and authorizing source delivery and the Mac update. This supersedes the pending
Windows user acceptance above; native Mac execution remains unverified.

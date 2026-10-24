# Accepted paw shape: smooth angular bends — 2026-10-24

Subsequently rejected with screenshot8425c644: the outer arm still had a flat shelf
and corner. Earlier visual review missed this despite technical checks.
[Elliptical follow-up](2026-10-24-snack-elliptical-paw.md) is current; neither
candidate below was accepted as the final roundness.

## Follow-up: round the bends further

- User requested “좀 더 둥글게 만들어” after the tangent-only candidate. That
  candidate is not accepted as the final curvature; the underlying upright arm
  shape remains the reference.
- Increase the radius of the tip and lower outside turn without blur. Two local
  anchors move less than2 logical units; remaining anchors, attachment, inner
  crease endpoint, motion and line width are unchanged. Distance tile capacity
  and per-frame rendering path are unchanged.
- Initial `rounder-curves.trx` and `rounder-final.trx` each passed15/16 but failed
  the new2-unit sampled contour-displacement bound (first tip, then lower bend).
  Reduced those local offsets; did not relax the test. Evidence is retained in
  `artifacts/snack-rounder-curves-20261024/`.
- Final `rounder-release.trx`16/16 and `rounder-raster-release.trx`10/10 passed.
  Added broad incoming bend radius and matching vertical tangents, kept the
  sampled2-unit displacement bound. Existing five-size frame/eye/foot/opacity
  checks remain. Archive comparison:2,155 other reaction frames and109 motion
  samples exactly unchanged against the preceding launched candidate.
- Visually inspected all109 frames at150% in `frames-release/chuckle-1.500-00..13`
  plus the before/after closeup: rounded tip and outside turn, no new open joint
  or double contour observed. Other sizes were automatically tested, not a claim
  of manual review of every size or actual desktop acceptance.
- Separate package `candidate-snack-rounder-curves-20261024` verified against the
  tested DLLs. ZIP SHA256
  `8D17E1D41D1F23BB1798989068D34009CB8759DED532DE9B0371AE01185C79C5`.
  App SHA256 `40FD9D281AC294F8B3B6384C897EE8FB75B1DA44C89D651690029129B53CADFF`;
  Core SHA256 unchanged (`6D006F504BA57CE044F6DE2C0C29FC4D3A2A952BFF281C4B9258FCDB35B23816`).
- User reported tray exit; process absence confirmed. Announced and launched
  PID27332 at03:46:05KST, exact-path alive/responding after3s. Actual rounding
  acceptance pending. No forced termination, asset/settings/installer change.

## Authority and cause

- User accepts the current arm shape: “팔의 모양 자체는 ㄱㅊ은데 굴곡이 각져있음”.
  This supersedes the pending general-shape acceptance in the upright/joint report,
  but is not acceptance of this smoothing candidate.
- Baseline is `candidate-snack-connected-trace-20261024`, not the previously
  rejected sideways or small round-paw candidates.
- Five visible cubic joins included tangent-direction mismatches up to18.435°
  (lower outside bend18.225°). The contour was also flattened to12 line segments
  per cubic. These are geometry issues, not reasons to blur or resize the arm.

## Narrow correction

- Keep every accepted cubic anchor and handle length. Align incoming/outgoing
  handle directions at the five visible joins; keep the unstroked interior return.
- Flatten each cubic into32 segments before constructing the same fixed145×109
  distance tile. No blur, extra frame/size cache or timing change.
- The persistent22 control points add352B of point payload, not a growing cache.
  Authored/standing distance tiles remain63,220B/84,100B.
- Changed renderer source: `HighResolutionGrabRaster.Chuckle.cs`; it is also
  linked into the portable raster project. No Core, other motion, source art,
  settings or installer edits in this follow-up.

## Evidence and limits

- `artifacts/snack-smooth-curves-20261024/smooth-final.trx`:16/16. Added exact
  anchor preservation, equal handle lengths, aligned forward tangents and sampled
  contour displacement less than0.3 logical unit (65 samples per cubic).
  Existing five-size tests cover109 frames per size, protected eyes/feet, opaque
  connection, exact endpoints and raising/lowering agreement.
- First portable build exposed unsupported WPF Point subtraction in its shim.
  Replaced it with equivalent scalar arithmetic, then rebuilt both projects.
  `smooth-raster.trx`:10/10, five sizes×two densities, repeated retained/full
  pixel agreement and fixed output ownership. This is not a long-run memory test.
- First App run `smooth-curves.trx`16/16; all71 proof PNGs are byte-identical to
  the final scalar-arithmetic build in `frames-final`.
- Visually inspected all109 frames at150% (`00..13` sheets), plus rising frames
  8..15 at100% and125%, and the before/after closeup. The outer bend and tip stay
  attached without a new double outline or open shoulder in these offline frames.
  Not all545 scale/frame combinations were manually inspected.
- Final archive comparison against the baseline: exact2,155 prior reaction
  frames at five sizes, and exact109 chuckle Head/Cover/Bob samples.
- Actual desktop appearance acceptance remains pending. No native-Mac, broad
  performance, long-memory or release-completion claim.

## Package / startup

- Fresh package: `artifacts/product-shell/candidate-snack-smooth-curves-20261024/`.
- Package verified identical App/Core DLLs to the tested win-x64 output.
- ZIP SHA256 `D109744F88A6E9F831CC42DC2F573B0E89A023F8E69A82B4012BDFCEB4FAEBCF`.
- App SHA256 `2927F94701A63617067006C9D15F634D02528B9C3DC41662C18047CBC4CA083C`.
- Core SHA256 `6D006F504BA57CE044F6DE2C0C29FC4D3A2A952BFF281C4B9258FCDB35B23816`
  is unchanged from the preceding candidate.
- No existing pet at launch. Announced and started the candidate, PID33696;
  alive at its exact runtime path after3s. No forced process termination or
  automated desktop input. Startup is not user acceptance of appearance.

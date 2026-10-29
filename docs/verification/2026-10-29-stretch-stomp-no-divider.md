# Stretch / stamp: remove rejected divider and repair outer shoulder — 2026-10-29

## Failure and correction

The second candidate added a chin-to-armpit curve inside the white body. The user
rejected that visible divider. The earlier visual-success claim was wrong: a
connected line and passing regression tests did not make this anatomy acceptable.
That candidate and its evidence remain preserved as `final-reworked` and v2.

The original outer join used the deep armpit notch at source (28,76.4). Rotating
that notch with the raised foreleg brought the join toward the neck, narrowing
the visible shoulder. The current join begins on the rounded proximal foreleg
at (27,79.4), with its local boundary tangent. The added interior stroke and the
previous shoulder bump are removed. The outer filled silhouette provides the
connection. Stretch retains the separately extended leading paw and planted leg;
the rejected interior divider is also removed there.

## Exact candidate and evidence

All artifact paths below are under `artifacts/stretch-stomp-repair-20261029/`.

- App SHA256: `DA23F2A0001539A0EAF03574906B6980D23007B101C8EF87961640D480A12424`.
- `no-divider/identity.json`: product/probe DLL copy and hash identity checked.
- `tests/outer-shoulder.trx`: 48 related tests passed, including three size cases
  preventing a dark divider in the shoulder interior. Existing xUnit warnings
  remain; no failing or skipped tests in this selection.
- `no-divider-comparison-metrics.json`: all 282 accepted stamp root/tip/pose
  records exact. Arm transform, lift/reach/crouch timing unchanged. 2,238 of
  2,706 target images differ; changed pixels fit the inspected forebody region.
- `no-divider-visual-review.json`: 18 chronological contact sheets cover every
  60Hz frame at 100/125/150%, both facings (2,706 frames). Direct inspection was
  of these contact sheets plus 24 full-character key poses, not a claim of
  individually opening 2,706 full-resolution screenshots. No added divider,
  detached seam, hole or duplicate outline observed in the reviewed regions.
- `no-divider-parity-result.json`: Windows and Mac managed raster hashes match
  for 4,848 frames; 3,636 unaffected-motion hashes match the accepted baseline.
  This is Windows-host rendering, not a native Mac test. Mac build retains its
  existing unused-field warning.
- `no-divider-before-after.png` and `frame-review-v3.html`: rejected v2 versus
  current candidate, at identical timestamps. Original accepted baseline remains
  separately available in `before-verified`.

## Limits and status

The unwanted stroke removal and outer-join change are directly visible. Shape
acceptance remains pending; passing tests do not replace it. Live desktop input,
native macOS behavior and packaging/install verification were not performed in
this correction. Installed pet, settings and published packages were unchanged.
No commit, push or release was made. Preserve the candidate for visual review;
do not promote it as accepted automatically.

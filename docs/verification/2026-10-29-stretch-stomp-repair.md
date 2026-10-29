# Stretch foreleg / stamp shoulder repair — 2026-10-29

## User rejection — same day

User rejected the shown result as visually unchanged. The prior candidate is
**REJECTED**, not a resolved visual defect. Numeric parity, geometry preservation
and observed absence of sampling artifacts do not establish the requested limb
separation/shoulder anatomy. The prior report overstated that distinction; its
visual-success conclusion is withdrawn. The subsequent candidate and its separate
evidence are recorded in [the rework report](2026-10-29-stretch-stomp-rework.md).

User authorized correcting both shapes, preserving accepted stamp limb size,
rotation, cadence and other motions, with per-frame visual review at each size.
This is a local source/render candidate for comparison, not user acceptance,
installed replacement or a new public Windows/Mac release.

## Change

- Stretch: isolate the horizontal reach to the leading foreleg (x28–38 falloff)
  instead of translating the adjacent planted foreleg/lower chest with it.
  Preserve the existing head/rump/shake and2.8second timing. The narrower transition
  needs backtracked Newton steps to avoid oscillating source coordinates.
- Stamp: add a smooth shoulder-volume correction to JointDistance between the
  moving root and planted foreleg. Both endpoint values and first derivatives
  are zero for the added term. Existing arm matrix, pivot, length, rotation,
  time functions, expression, stroke sampling and other reactions remain unchanged.
- Both edits are in source partials shared by Windows and Mac Raster. No artwork
  bank, presenter timing, Core behavior, installed app or settings was changed.

## Verification

Root: `artifacts/stretch-stomp-repair-20261029/`.

- Before: immutable accepted App SHA256
  `B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0`.
- After tested/rendered App SHA256
  `D24D0AD64F672C51D0EEC2BF47FE0A0A3CC1438B7AE04BC664B329ED67E543A6`.
- `before-verified/` and `after/`:2,706frames each,60Hz,100/125/150%,both facings.
  Per combination: stretch169, third reaction75, fourth94, fifth113.
  Initial twelve whole-character key poses per motion were reviewed, then all
  2,706 candidate frames' changed forebody regions were visually inspected in
  the18 chronological `visual-all/` sheets. No detached speck, duplicate inner
  contour, arm hole or cut edge was observed there. This is rendered inspection,
  not a claim of real desktop input or a universal aesthetic pass.
- `comparison-metrics.json`:2,226frames changed; stamp changes limited to the
  shoulder x24–40.4/y68–87 in source-facing coordinates. Stretch changes limited
  to the intended lower forebody x0–62.8/y68–88. Mirrored bounds recorded too.
  These changes are wholly contained in the visually inspected regions.
- `geometry.json`: all282 third/fourth/fifth reaction samples preserve exact arm
  root/tip coordinates and lift/reach/crouch values before vs after. Existing
  independent rigid-arm geometry/size/trajectory and repeated-hit tests pass.
- `final-related.trx`:42 related renderer/warp/runtime tests pass. Added planted
  foreleg preservation and shake/inverse regression cases; final-warp8/8 passes,
  with5 cases overlapping the42 total: **45 unique tests**, no skips/failures.
- `parity-result.json`:4,848 Windows/Mac managed-host frame hashes match, including
  density2 checkpoints.3,636 other-motion frames match the prior accepted Windows
  reference exactly (tap, shake, groom, beg, question, first/second anger, chuckle).
  Native Mac execution/compositor and live-user operation remain NOT_TESTED.

## Rejected/invalid intermediate evidence

- Attempt1 isolated the leg but retained undamped inverse steps. Visible small
  inner protrusions and two failed inverse tests (.75/1 amount, residual around10
  authored units) rejected it. Backtracking removes that numerical defect; the
  final inverse tests cover95,475 original grid points plus shake cases.
- Initial `before/` full-frame run accidentally used the newer reference copied
  by an incremental build. Hash verification caught it before it was used as
  delivery evidence. That run is invalid for before/after comparisons. Explicitly
  copied and verified the immutable App/Core in the baseline probe and reran all
  2,706frames into **before-verified/**. The viewer uses only this verified folder.

## Review deliverables / limits

- `frame-review.html`: synchronized before/after, motion/size/facing selection,
  individual frame stepping and60Hz-time playback; every source PNG retained.
- `Stretch-before-after.png`, `Angry5-before-after.png`: same1.0second150% frame,
  nearest-neighbor enlargement, left before/right after. No invented/repainted art.
- Candidate shape acceptance remains with the user. No installer update, pet
  restart, Mac app repackaging, commit/push or public asset replacement occurred.
  No new full-system performance or long-duration memory result is claimed.

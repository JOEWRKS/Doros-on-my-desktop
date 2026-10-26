# Seated head occlusion correction — 2026-10-26

## User-reported failure and cause
The user rejected our own AFTER preview: residual overlap remained. The prior
shoulder-depth review missed it. Its synthetic test only protected radius <=9
from the shoulder; the 9–13 DIP fade band and distal foreground still modified
real hair/face pixels. Those 16 passes did not establish correct head occlusion.

## Correction
Remove SeatedPaw.OverHead and its render call altogether. The authored arm is a
body layer, composited behind the original head by the existing premultiplied
alpha compositor. No radial fade or alternate foreground paw path remains.
The source hair tips are preserved; their dark contour is present in the neutral
original too. Bowed arm distance geometry, pointer tracking, timing, shake,
Core, Mac, original art, installer and settings are unchanged by this correction.

## Evidence
- 18 affected tests pass: `artifacts/seated-head-occlusion-20261026/tests/final.trx`.
- Replacement regression uses actual rendered head pixels, protecting fully
  opaque source cubic stencils in the affected region. 3 scales ×2 directions
  ×5 angles ×9 transition/hold ages =270 rendered comparisons. No mismatch.
  It replaces the inadequate synthetic radius-only test.
- 768 tap frames generated: fixed/moving pointer ×100/125/150% ×2 directions
  ×64 frames. All 12 enlarged shoulder/head contact sheets inspected in order;
  hair remains in front throughout raise/reach/lower. This is generated diagnostic
  input, not a recording of real mouse input or native Mac verification.
- Frame endpoints equal; no output edge clipping. 348 shake frames byte-identical
  to previous weak-shake frames;396 other existing-motion samples identical.
- `head-comparison.png`: neutral original / rejected fade / corrected compositor,
  same150% right-facing region, frame20 for the two reaching versions.
- `frame-review.html` and `visual-review.json` retain individual frames/evidence.

## Package and runtime
App SHA256 B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0
matches tested assembly. Core SHA256
46358E2D9EC0452DA6BEE40C64BFF0A20E7EE7819D1103A1D091BBE9D18ED29A unchanged.
ZIP SHA256 0D607A095B27DA750DFE8729344516E76D9E872873747F2B55F6038D743ACCDE.
Package: `artifacts/product-shell/candidate-seated-head-occlusion-20261026`.
No previous pet process remained. Launched PID83300 at UTC13:06:44.5587123;
input idle, responsive, not exited. `launch.json` records exact executable/hash.
No forced process termination or automated user input.

## Limits
User subsequently accepted this exact running candidate on2026-10-26 and
requested commit/push (“ㅇㅇ 이대로 커밋 푸시”). No native Mac,
long-duration memory, new full-suite, or every possible cursor trajectory claim.
The rejected shoulder-depth package is superseded and was not launched here.

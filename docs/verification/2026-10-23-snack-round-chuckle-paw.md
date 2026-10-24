# Round mouth-cover forepaw correction — 2026-10-23

Subsequently rejected: user supplied a broad folded-limb outline, not this circular
pad/narrow wrist. [Current correction](2026-10-24-snack-folded-chuckle-paw.md).
Results below remain historical technical evidence, not user shape acceptance.

## Authority / failure

- User rejects the previous chuckle forepaw screenshot as a pointed/diagonal limb,
  not the reference's rounded mouth-covering paw. Prior technical tests and visual
  inspection did not establish shape acceptance. Prior report is retained and linked.
- Authorized correction: rounded pad at mouth, natural forearm connection, verify
  raising and lowering frame by frame. Preserve all accepted other motion/art/speed.
- Cause: rotating the standing leg also exposed its diagonal cut/attachment as the
  visible hand silhouette. Opacity/feet tests failed to test the desired hand shape.

## Correction

- Product-code change is confined to `HighResolutionGrabRaster.Chuckle.cs`:
  authored round elliptical pad plus connected bent forearm silhouettes, composited
  at destination resolution with the existing body ink color and scale-aware width.
  The forearm joins from below with an open chest attachment, not a closed cap drawn
  across the torso. Original source standing leg blends into this pose during raising;
  the same geometry reverses on lowering. No source art/banks or texture assets edited.
- Refined the initial candidate's weak interior forearm line during entry/exit;
  keep the exposed contour but defer the interior line while the arm is still within
  the standing torso. Widened/raised the round pad to cover the omega mouth fully.
- Core state/landing/eating, sixth jump and1–5 reactions unchanged. Same chuckle
  head/cover/bob values,1.8s duration, .26s raise/.32s lower and four pulses.
- No new frame cache, bitmap bank or retained image buffers; only geometry scalars.

## Evidence

- Root: `artifacts/snack-round-chuckle-20261023/`.
- Five-size109-frame sequences (545 frames): planted feet byte-identical, shoulder
  opaque, exact neutral return. New mouth coverage/eye-region comparisons atfive sizes.
  Raising/lowering corresponding poses checked at17 points per size (<=1byte rounding).
- Saved final150% sequence14 sheets/all109 frames manually opened; other four sizes'
  hold sheets opened. This is saved renderer output, not actual desktop acceptance.
- Final render test results and packaging are appended below. WPF runtime four cases
  (two sizes × window/floor) passed in `round-paw-final.trx`; five frame-sequence checks
  also passed there. That report additionally retains five failed initial eye checks:
  the original assertion covered the entire top image, including the deliberately
  bobbing back. Restricted the assertion to the actual eye region; five eye/mouth
  checks pass in `round-eye-mouth-region.trx`. No product change was made to pass
  that test correction.
- Retained vs full raster10/10 atfive sizes × densities1/2, repeated sequences,
  byte-identical output, same owned buffer. Not a long-run native/GPU memory test.
- Isolated archived comparison:2,155 accepted reaction frames atfive sizes match
  exactly, both against the accepted continuous-stamp and previous chuckle binaries.
  Previous chuckle's109 head/cover/bob samples also exactly match; only paw geometry
  changes. Existing comparison helper's optional timing check added, old mode preserved.
- No live input automation, heap/trace pause, forcedGC/trim, process killing,
  source art/settings/installer changes or public release. Native Mac not tested/built.

## Delivery / boundary

- User confirmed readiness “ㅇㅇ” after no existing pet was found. Separate tested
  candidate and launch details follow. Actual corrected appearance remains subject
  to user inspection; no acceptance inferred from automated tests or startup.
- Final combined render15/15 passed (`round-paw-verified.trx`); added lower-iris
  preservation check also5/5 passed (`round-lower-iris.trx`). All use the same product
  binary; additional test code did not change the package hash.
- Packaged `artifacts/product-shell/candidate-snack-round-chuckle-20261023/`;
  publish and independent prelaunch App/Core hashes match tested binaries.
  App `877AE653A0AD1F584F20CBBF3BE656A9D3A19EEADE33447EEE55E38514F6FF4E`;
  Core unchanged `6D006F504BA57CE044F6DE2C0C29FC4D3A2A952BFF281C4B9258FCDB35B23816`;
  ZIP `F085199693BA6B1A017003410F3BC706022CE8A40EAFFE0A6EBB1E8C4823C0BD`.
- No existing pet, announced launch. PID72040 started2026-10-23 23:50:36KST,
  same-path alive/responding after3s. Actual corrected shape acceptance remains open.

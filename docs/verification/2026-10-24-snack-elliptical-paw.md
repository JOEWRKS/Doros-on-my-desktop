# Round outer paw, not a rounded rectangle — 2026-10-24

## Rejection and corrected criterion

User rejected PID27332 with screenshot8425c644: the lower arm still reads as a
flat shelf and corner. Earlier tangent and radius checks passed but did not prove
the requested round silhouette. The previous visual review missed this; those
passes are not appearance acceptance. Source-image size and pose remain accepted,
not the rejected angular outer curve.

The prior arbitrary2-unit same-parameter displacement limit kept the corner
anchors too close to the rejected shape. Replace that implementation constraint
with a whole-contour roundness and overall-size check; retain the eye, opacity,
feet, transition and motion invariants.

## Change

- Outer contour is an elliptical sweep centered(21,68.7), radii9/11.1, from
  bottom(21,79.8) around the outside to top(21,57.6). Tip is a second ellipse arc
  with radii4.5833/4.4. This replaces the shelf/corner instead of just smoothing
  the corner's tangent. No blur or wider ink.
- Attachment endpoints(35,76)/(35,70), inner crease endpoint(24.125,70.4167),
  motion and rendering scale remain. The outer envelope stays within the prior
  arm's overall size; the path inside that envelope intentionally changes.
- Only chuckle geometry and its regression/proof test changed. Same fixed
  distance tile, same per-frame path; no frame/size cache added. Existing other
  app, Core, source artwork, installer and settings not modified by this task.

## Verification

Evidence root: `artifacts/snack-arc-paw-20261024/`.

- `arc-release.trx`16/16: sampled elliptical radius within0.03%, envelope,
  attachment and tangent checks; existing five-size109-frame invariants.
- `arc-raster.trx`10/10: five sizes×two densities, retained/full pixels equal,
  fixed output ownership. Not a long-memory or native-Mac verification.
- Manual review: all109150% frames in00..13 sheets, enlarged before/after,
  right-facing before/after frames8/32/96 matching the screenshot's facing.
  The flat lower shelf is replaced by a visibly curved sweep in these renders.
  Other sizes automatically tested; no claim all545 were manually reviewed.
- Initial/final proof PNGs identical; final test-only addition is the right-facing
  comparison. Source pose and reflected presentation are offline render evidence,
  not actual desktop interaction acceptance.
- Archive comparison versus the rejected rounder candidate: exact2,155 other
  reaction frames and exact109 Head/Cover/Bob samples. Core binary unchanged.

## Handoff

Separate candidate `candidate-snack-arc-paw-20261024`, published DLLs verified
against the tested win-x64 output. ZIP SHA256
`CB57D9E8A9D3E8419AFFC402ECC02045EEFBEAB18330F6CDC1CA1ED82314D546`;
App `C3F4141EF71BD8A4A464E49C55BE2B469B5D93665755F0A585DC10BF0D4A8B43`;
Core `6D006F504BA57CE044F6DE2C0C29FC4D3A2A952BFF281C4B9258FCDB35B23816`.
Existing pet absent; announced and launched PID5000 at03:54:15KST, exact runtime
path, alive/responding after3s. Startup is not visual acceptance.
User subsequently explicitly accepted this candidate: “ㅇㅇ 통과”. This completes
the current Windows rounded-forepaw/mouth-covering chuckle correction. Preserve it
as the accepted baseline. No additional testing or code changes in the acceptance
turn. Prior rejected candidates are not relabeled as accepted; native-Mac testing
and public release are not implied. No automatic input or forced termination.

# Ribbon-side pixel spur — 2026-10-30

## Confirmed cause and correction

User screenshot identifies a dark spur just outside the right ribbon. Exporting
the exact accepted Windows package's head and body separately reproduces it in
the 225px head layer before compositing. It is not produced by the latest shoulder
rotation. The head separation retained adjacent dark ink: the earlier masking
script removes neutral white exterior but preserves dark ink. This establishes
the retained-layer defect, not the date it first became visible.

`HighResolutionGrabRaster.Ribbon.cs` clips only the exterior spur within source
x162–166/y109–116, with coverage following the adjacent contour. All premultiplied
channels receive the same coverage. The normal ribbon above and contour below
are retained. Preparation runs once on the normal/closed arrays; angry variants
inherit the normal array. Windows and the Mac shared sampler call the same helper.
No per-frame buffer, cache, timing, arm geometry, original bank, settings or
installed package is changed.

## Evidence

Root: `artifacts/ribbon-junction-20261030/`.

- `before/`: accepted package head/body/composite exports and layer inspection.
- `comparison/before-after.png`: actual old/new render, identical crop and scale.
- `review-square/`: 600 pose conditions = 4 head variants ×25 angles(-12…12deg)
  ×3 sizes(100/125/150%) ×2 facings. Each is shown on light/dark backgrounds in
  12 contact sheets, all directly inspected. The reported spur is absent and no
  new opening or clipped ribbon was observed in this set. These are authored
  pose samples, not every possible runtime animation frame or live-screen input.
  Earlier `review/` sheets used an unequal display aspect ratio and are superseded.
- Windows related tests:78/78, including4 new source-region/variant checks.
- Raster suite first run:106 pass/3 historical whole-image hash failures. Those
  three tests were revised without changing their historical expected hashes:
  an isolated renderer restores only the repaired source texels, reproduces the
  old hash exactly, and verifies corrected output differs only in the small
  projected ribbon region. Targeted rerun:3/3 pass. No product change between
  the106-test passing run and these corrected assertions. Original failures kept.
- `tests/` contains all three TRX results. `identity.json` ties the inspected
  sheets/comparison to candidate App SHA256
  `842A9A4391F42C2E3F80345A0F0134F95EE89963F3822549B3ABD1B38C1F74E2`.
- New Windows regression also proves source pixels outside the bounded repair
  region are exact and checks neighboring retained contour pixels and Pbgra bounds.

## Boundaries

Candidate source/build and rendered inspection complete. Existing running pet,
installed app and published Windows/Mac archives were not replaced. No new
native Mac run, full distributed-package parity, live interaction, or long memory
soak claim. User acceptance of this correction remains open. No commit/push or
release was requested for this follow-up.

## Follow-up: left inner fold correction

User then circled the crooked line immediately left of the removed exterior spur.
The source contains both a stepped diagonal fold with an abrupt tip and a dark
inward remnant on the adjacent contour. The initial exterior-only fix did not
address either. `SmoothRibbonInnerStroke` reconstructs only the short diagonal
within x153–158/y108–114 as a gently curved, round-ended stroke; a separate
x158–161/y109–114 strip smooths the inward contour. Original alpha is unchanged
in these opaque interior regions. Exterior correction remains in effect.

- Latest comparison: `inner-stroke-v2/before-after.png` under the artifact root.
  BEFORE is the prior exterior-only candidate, not the uncorrected release.
- Final `inner-review/`:600pose conditions,12light/dark contact sheets directly
  inspected at100/125/150%, both facings,4head variants,25head angles. The local
  kink is removed without an observed new gap in these samples. This is not a
  claim of exhaustive runtime frames or native Mac screen review.
- Final focused rerun: Windows4/4 and Raster3/3, in `inner-final-*.trx`.
  Tests retain the original historical hashes by restoring only the two repaired
  source regions in an isolated reference renderer; all output changes remain
  confined to the projected ribbon region. Source outside the regions is exact.
- `inner-identity.json`: App build and inspected DLL both
  `B708009A7C3589EE147127F497026A0D6DC97FC56A67C9298E5DF76847719DEA`.
- No motion/geometry/timing or running/installed/public package change. Prior
  broader test results belong to the exterior-only candidate; this follow-up
  reran the affected focused checks, not those broader suites.

## Correction to the user's connecting line

The preceding smoothing was rejected. It rounded a free tip instead of connecting
the two lines. The red annotation specifies a shallow descending fold from left
to right, meeting the existing contour. `ConnectRibbonFold` now erases the old
downward tip and draws that continuous connection. This replaces the previous
local geometry; the initial exterior-spur repair remains. The new bounded source
region is x153–161/y107–114 in addition to the prior exterior repair.

- `connected-fold/before-after.png`: BEFORE is rejected inner-stroke-v2; AFTER is
  the actual new renderer. The connected fold is directly visible, not an overlay.
- `connected-review/`:72unique samples =4head variants×3angles(-12/0/+12deg)
  ×3sizes×2facings, each on light/dark background. All12sheets directly inspected;
  no disconnection or leftover downward tip observed. Not exhaustive frame review.
- `tests/connected-windows.trx`:5/5; `connected-raster.trx`:3/3. Added explicit
  column-wise continuous ink and removed-tip assertions so the previous wrong
  topology cannot pass merely by staying in the bounded region.
- `connected-identity.json` ties source build, comparison DLL, review DLL and
  shared Raster build. No app restart, package replacement, nativeMac verification,
  commit/push or release performed. User visual acceptance remains open.

## Acceptance and delivery authorization

User accepted the connected-fold appearance with the follow-up instruction to
update through the Mac version and commit/push. This supersedes the visual
acceptance gate above. Native Mac execution remains separate. Source and Mac
package delivery are recorded in the test12 delivery document once verified.

# r99 — closed fringe and hanging paw contours

## Scope and cause

User requested three specific visual fixes after the bounded r98 memory
investigation: upper-eye residue over the bangs while eating, a cropped
maximally extended hanging paw, and a cut armpit during the clicked-paw wiggle.
No broad memory soak was restarted. The previous bounded observation is
complete; that is not a universal leak certificate.

Reproduced using the immutable published r98 App DLL in
`artifacts/memory-root-audit-20261020-r98/final-release`.

- Closed head: the bank retained a small open upper-lid spur over the left
  fringe at native225 coordinates (44..46,108)/(44..45,109).
  A lazily retained closed-head copy replaces only these five texels with
  adjoining existing fringe texels. Bank/assets, alpha, closed eyelids,
  mouth, open eyes and hair tips are untouched. All closed expressions use
  the same cleaned head, avoiding a switch back to the spur after eating.
- Maximum paw: the 225-square intermediate ended at logicalY96 while
  stretched paw geometry extends below it. Its scratch is now375-square
  with a32-DIP gutter at the same texel density; bounds, ink compensation
  and final sampling use that origin/size. No lower-resolution upscale.
- Armpit: the old X19..35/X35..51 source rectangles cut through the authored
  outer contour, which extends toX53. Selection now includes the outer
  stroke (X17..35/X35..54). The deformation starts at its actual stationary
  boundaryY70 instead of introducing Y69-based displacement at row70.
  The joint therefore has zero displacement at its first affected row.
  This corrects the local paw-root mapping; controller gain/limits, tap
 600ms envelope, release220ms and all other motion clocks are unchanged.

The padded source and stroke result are reused, not allocated each frame.
The larger persistent scratch pair adds2,880,000bytes versus the old pair,
and the lazily prepared closed head adds810,000bytes once per raster.
This is explicitly bounded additional storage, not a claimed memory saving.
r98 neck-surface/perch-blend reuse remains unchanged.

## Verification

Evidence root: `artifacts/visual-defects-20261020-r99`.

- Release publish: success, no build warnings/errors.
- Targeted18 tests: `tests/r99-targeted.trx`, zero failed/skipped.
  Includes complete tip beyond the former canvas, unchanged stationary
  shoulder, armpit ink, closed cleanup bounds and retained-buffer identity.
  Forty paw/stroke preparations still allocate less than65,536bytes total.
- Related83 tests: `tests/r99-regression.trx`, zero failed/skipped.
  Covers presenter state/host, gaze/cheek, snack, perch expression and r98
  neck-surface/perch-blend lifetime contracts. Test compilation emitted
  pre-existing xUnit1031 warnings in unrelated tests; not suppressed.
- Published r98/r99 renderer probe:415frames each at1/1.1/1.25/1.35/1.5.
  Includes eating stages, both maximum paws and both entire600ms clicked
  paw envelopes sampled at60Hz. Saved PNGs in `before`/`after`.
- `after/comparison.json`:75frames fully identical; zero changed pixels
  outside the specified closed-fringe patch neighborhood or paw rowsY>=68.
  This is a constrained visual regression comparison, not a claim that
  every changed pixel is automatically correct.
- Directly inspected all five `after/review-s*.png` contact sheets,
  `before/sheet.png`, `after/sheet.png`, enlarged head details and
  `after/before-after.png`. Observed rounded complete tips, connected
  armpit outline, removed closed-fringe spur. Sheets alternate light/dark
  backgrounds. Original resolution PNGs are retained alongside the sheets.
- Direct Release-DLL rendering/state replay is not physical mouse input or
  a screen recording of the normal desktop app.

Changed product file: `src/Dororong.App/Controls/HighResolutionGrabRaster.cs`.
Tests: `InteractionDefectTests.cs`, `PerchPawReuseTests.cs`.
Probe: `tools/InteractionDefectProbe/Probe.csproj`, `Program.cs`.
Source art files, Core and installed copy/settings unchanged.

## Candidate and normal launch — 21:21 KST

Candidate folder: `artifacts/visual-defects-20261020-r99/candidate`.
AppSHA256: `816633B7203A68AFFEC3F66044F647D9F81801CBE860DDE17F83F1CA5366D22D`.
The App DLL used by the101 tests has exactly the same hash.
CoreSHA256: `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`,
identical to r98.

User acknowledged the exit request. Verified old app absent and normal log
`Stopped` for PID46620 atUTC12:17:25.9913333. Launched candidate normally with
`--high-resolution-grab=1.5`; no measurement, opaque-window, frame-capture
or automatic-input options. PID36292/startUTC12:21:20.0067552.
Normal `Started` logUTC12:21:20.9947128; process responding.

Start was announced and user asked to inspect only snack eating, maximum
hanging-paw pull and clicked-paw wiggle. Actual desktop appearance/user
acceptance is pending, not passed. No forcedGC, trim, pause-capable heap
inspection, app kill, settings/installation change, automation or shutdown.

Next: collect feedback on these three fixes only. No repeat memory testing
without a new observed failure or separate requested scope.

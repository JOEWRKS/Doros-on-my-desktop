# r100 — closed fringe and eyelid line consistency

## Request and scope

User supplied two cropped closed-expression screenshots and requested
consistent fringe and eye lines. This is the closed expression shown in the
screenshots, not a redesign of the open eyes, character or original assets.
r99 paw-tip/armpit corrections remain. No memory-soak, automatic input,
pause-capable diagnostics, installed-copy/settings change or shutdown.

The closed head had irregular/thick fringe joins and softer grey eyelid
strokes. r100 prepares that expression once at225px native resolution:
extract the existing ink centerline, locally average its subpixel positions,
and apply a common2-native-pixel stroke/antialias coverage with the same
dark ink to the selected fringe/lid area. Existing adjoining colour texels
provide the exposed background. No replacement silhouette or eye path is
hand-drawn; outline thickness necessarily changes the selected edge pixels.
This is a bounded local rendering correction, not pixel identity inside
the edited region.

The original bank remains immutable. Whole-head alpha is unchanged;
open expression, mouth, side hair tips, body and motion code are untouched.
Preparation uses temporary arrays once; the retained closed head continues
to be reused. No new per-frame preprocessing or cache of animation frames.
The earlier five-texel r99 spur cleanup is retained before this preparation.

## Evidence

Root: `artifacts/closed-ink-20261020-r100`.

- First2.4px trial in `trial`: inspected but superseded by the finer2px
  candidate. The trial was not launched as the normal pet.
- Final candidate Release publish succeeded without warnings/errors.
- `tests/r100-targeted.trx`:17tests passed, zero skipped/failed.
  Includes local cleanup bounds/alpha/determinism/reuse, eyelid thickness,
  paw buffer reuse and actual presenter snack/sleep/state rendering.
  Existing unrelated xUnit1031 compile warnings remain.
- Exact r99 published renderer vs r100:415frames at
  1/1.1/1.25/1.35/1.5scale. `after/comparison.json`:385identical;
  only30closed-expression eating frames differ; zero differences outside
  the selected fringe/lid neighborhood. The unchanged frames include both
  paws'600ms tap sequences and maximum extension.
- Directly inspected `after/head-detail.png`,
  `after/closed-ink-scales.png` and the trial sheet. The final five-size
  comparison uses light/dark backgrounds and2x display magnification;
  original native-resolution saved frames remain available.
- Eyelid equivalent ink-width measurement on saved native PNG:
  left columns50..58/rows122..130 and right88..97/rows127..136,
  sum clamp((255-red)/210,0,1), averaged across columns:
  r99 approximately2.62/3.05px; r100 approximately2.13/2.27px.
  These measurements support reduced width mismatch in those straight
  portions, not a guarantee of mathematically identical projected width
  at every curve, junction, size or screen pixel.

New product file `HighResolutionClosedInk.cs`; one call added to
`HighResolutionGrabRaster.ClosedHead`. Updated `InteractionDefectTests.cs`
and the existing comparison probe. Artwork files and Core unchanged.

AppSHA256 `BD00BBFD46134F54453F786DE796AFA44A8F84533E786CA401C7B28937EDD23C`;
published and tested DLL hashes match.
CoreSHA256 `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`
matches r99.

## Normal execution — 21:33 KST

User acknowledged exit request; old pet process absence checked.
Started `candidate/Dororong.App.exe --high-resolution-grab=1.5` normally,
PID59664/startUTC12:33:25.7301641. No capture/performance/opaque-host flags.
Normal `Started` logUTC12:33:26.5926916 and process responding confirmed.
Start announcement asks only to inspect the closed eyes and fringe during
snack eating. Actual screen acceptance remains pending; published-DLL
frame inspection is not direct desktop input verification.

Next: user feedback on these selected lines, without restarting broad
memory or unrelated motion checks.

## Subsequent acceptance

User: “ㅇㅇ 통과. 이제 문제 없으면 사이즈 슬라이더”.
The r100 selected line correction is accepted; the pending screen-acceptance
statement above is historical. Next scope is the r101 size slider, not another
round of unrelated memory or motion tests.

# Upright red trace / forepaw connection correction — 2026-10-24

## User authority and inspection failure

- User rejected the sideways C paw and provided the 516x401 red drawing:
  `codex-clipboard-7e21be44-e2f4-4f26-afb6-ce79411dd869.png`.
  Its silhouette is an upright forepaw, narrow rounded tip at the mouth, long
  outside edge and descending inside crease. The previous broad lateral shape
  was an incorrect interpretation, not an accepted shape.
- Traced the supplied pixel coordinates using mouth-side calibration
  `(200,212) -> (26,60)`, 4.8 reference pixels per logical unit. Preserved head,
  pulse/cover timing, previous reactions, source images, settings and installer.
- The upright candidate was launched at00:36:50KST, PID63700, after confirming
  no existing pet. Same-path alive/responding after3s. App SHA256
  `B02A5BDDEDFDEF90DBED7BE4EA05A80C277D93184A71BCCACC4E25E60B04C62F`.
- User then explicitly rejected the unnatural lower arm/torso junction and
  challenged the claimed frame-by-frame inspection. Saved frames had been viewed,
  but inspection accepted a visible lower notch and transitional double outlines.
  That was a deficient visual decision, not a lack of user instruction. Passing
  opacity/foot tests did not justify presenting the shape as finished.

## Confirmed causes / corrections

1. Lower contour consisted of two cubics whose tangents changed abruptly at the
   trace's `(200,295)` junction. Replaced that local lower return with a smooth,
   tangent-continuous sweep into the torso; retained upright tip/outside/inner line.
2. Transition crossfaded two already-inked images, exposing both outlines. Replaced
   RGB crossfade with interpolation between signed silhouette-distance fields,
   then drew one outline. Backing stays opaque from the start of the transition.
3. A 125% reverse-transition comparison differed by three channel levels at
   step2/byte237304. `pixel[3] < 255` selected different composite branches for
   floating-point roundoff near full opacity. Added a1e-8 alpha tolerance; the
   unchanged <=1-channel reverse test now passes. Pose curves were not changed.
4. Early backing attempts left a triangle/pinhole inside the upright arm. Extended
   the behind-head backing and added an explicit opaque interior region assertion.

## Evidence, including failed attempts

Root: `artifacts/snack-upright-trace-20261024/`.

- `upright-trace.trx`15/15 did not test the interior triangle; visual inspection
  found it and added a test. `upright-final.trx` and `upright-verified.trx` each
  failed5 interior tests; retained. `upright-filled.trx`15/15 passed but its candidate
  was rejected for the joint/transition issues described above.
- `upright-morph.trx` failed5 shoulder checks. `upright-connected.trx`,
  `upright-connected-final.trx`, and `upright-retrace-detail.trx` retain the125%
  roundoff failure; none is called a final pass.
- Final `upright-release.trx`:15/15, five sizes,109 sequence frames per size,
  opaque shoulder/interior, eye clearance, upright tip/body shape, absence of
  lateral extension, unchanged planted feet, exact endpoints and reverse agreement.
- Final `upright-release-raster.trx`:10/10, five sizes times two densities,
  repeated retained/full pixels equal and a single owned output frame buffer.
- All109 frames at150% in `frames-release/chuckle-1.500-00..13.png` inspected,
  not just first/last or the held pose. Five sizes were automatically checked;
  do not imply all545 frames were manually inspected.
- `frames-release/joint-before-after.png` shows f3(0.050s), f32(0.533s),
  f105(1.750s), before above and after below. The old notch and double outline
  are visibly reduced in these offline renders. Comparison shown to user;
  actual desktop reaction acceptance remains pending.
- Archive comparison against the launched upright candidate: exact RGBA match
  for2,155 previous-reaction frames across five sizes/streaks1–5; exact109
  chuckle Head/Cover/Bob samples. Core binary unchanged.
- Distance fields have fixed capacity:63,220B authored outline +84,100B resting
  silhouette, not growing age/size caches. Bounded renderer test is not a
  long-run process-memory/GPU test or a measured native-Mac performance result.

## Current package / startup

- Subsequent user feedback accepts this arm shape but rejects angular bends.
  [Smoothing follow-up](2026-10-24-snack-smooth-paw-curves.md) supersedes the
  pending general-shape acceptance below; it does not approve the new candidate.

- `artifacts/product-shell/candidate-snack-connected-trace-20261024/`.
- ZIP SHA256 `6E153276BC9587097279E89588A38982F5D0D542AC0E9E48E4863B6D2E86F17F`.
- App SHA256 `BD5231ED9079CED6A3C341E186D9F0C2B98D9B298D96FBDA7AC3ACE7ECBEC3BB`.
- Core SHA256 `6D006F504BA57CE044F6DE2C0C29FC4D3A2A952BFF281C4B9258FCDB35B23816`.
- Packaging verified test/runtime DLL identity. Existing pet was absent;
  announced and launched at00:52:25KST, PID12124, same-path alive/responding after3s.
- No forced termination, desktop automatic input, installer/settings/asset change,
  public release or native-Mac verification. Next is actual user inspection of
  this corrected connection and transition, not inferred acceptance from startup.

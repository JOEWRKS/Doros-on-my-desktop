# Snack motion correction — 2026-10-13

Scope/visual authority: [the user's six corrections](../specs/2026-10-13-snack-play.md#user-motion-correction--2026-10-13).
This supersedes the first snack candidate's visual result, not its dated test
history or unrelated startup/release boundaries.

## Slow happy sway — 2026-10-14

User requested a slower, longer “흔-들 흔-들” rhythm after candidate07.
Changed only Happy duration/angle timing:960ms→2160ms, removed held extrema,
retained8/8/4-degree amplitudes and eased zero-speed reversals. No sampler,
artwork, body/foot, chewing or cancellation behavior changes.

- Evidence: `artifacts/snack-sway-20261014/`. Affected App43 and full Core255
  passed. App includes actual-loop support/interruption, exclusive hunting hold,
  original source/chewing and accepted head-sampler regressions. Tests cover the
  whole longer phase, neutral endpoints, continuous departure from extremes,
  and angular speed below38 degrees/sec (old main crossing peaked120deg/sec).
- Full App1156 was passed for candidate07; not rerun for this timing-only change.
  Do not present that previous run as a fresh full regression.
- Renderer export `frames/`: complete eating→sway→settle at60fps and contact
  sheet spanning the whole gesture. These are renderer proofs, not desktop captures.
- Inspected the extreme tilt on white/dark at native/3x scale; head/neck/foot
  appearance remains the accepted rendering. `sway-preview.mp4` preserves the
  full slower timing without text overlays.
- Candidate `artifacts/product-shell/candidate-20261014-snack-sway-08/` passed
  publisher/tested RID hash parity, runtime8.0.31, ZIP roundtrip and isolated
  native startup/duplicate/normal close (PID346564, exit0). Portable archive
  SHA256 `17EE3E449884B8BD974EF506F37DBA8758CFD13D70DA2A9F24F2928BA4C7F35D`.
- After the user's exit confirmation and a no-process check, launched its
  `runtime/Dororong.exe`: PID213888, Responding=true, Started diagnostic
  `2026-10-14T09:22:35Z` (18:22 KST). Visual acceptance was pending at launch;
  the subsequent user acceptance is recorded in [TASKS](../../TASKS.md#happy-after-eating--2026-10-14).
  No installed replacement, commit/push, startup setting or release change.

## Happy after eating — 2026-10-14

The later [slow sway adjustment](#slow-happy-sway--2026-10-14) supersedes only
this candidate's timing; the following is preserved execution history.

Authority: [approved gesture](../specs/2026-10-13-snack-play.md#happy-after-eating--approved-2026-10-14).

- Added one `Happy` phase between Eating and Settling. It retains snack ownership
  for960ms, with zero chase delta/lift, fixed facing and immediate existing exits.
- `SnackRenderer.HappyRoll` uses eased8/-8/4-degree accents and brief holds,
  then zero rotation. Rendering calls the same accepted upright closed-eye head
  sampler/hidden neck backing; no new assets, body deformation or mouth redraw.
- Evidence root: `artifacts/snack-happy-20261014/`. Focused App41, full Core255
  and full App1156 pass (`results/app-focused.trx`, `results/core-full.trx`,
  `results/app-full.trx`). The full App run includes actual-loop planted support
  and direct-press interruption cases (not selected by the narrower name filter).
- `tools/SnackProof --happy` exports eating→happy→settling at60fps and all61
  happy-phase samples on white/dark backgrounds. Inspected `frames/contact-sheet.png`
  and the enlarged extreme tilt: no visible open neck seam or displaced feet.
  `happy-preview.mp4` is a renderer preview (both directions, native/3x), not a
  desktop recording. New tests check exact neutral endpoints, fixed lower feet,
  accepted closed-eye rendering, zero endpoint velocity, phase order, cancellation
  and real PetLoop support/direct interruption.
- Fresh candidate: `artifacts/product-shell/candidate-20261014-snack-happy-07/`.
  Runtime `runtime/Dororong.exe`; portable archive
  `Doros-on-my-desktop-Portable-0.1.3-win-x64.zip`, SHA256
  `342402FF94A5779E2E50FC645D730971283EA5C3F91D3586F260E5E66E3795E7`.
  Publisher and package validation passed: tested App/Core RID hash parity,
  Core/Desktop/host8.0.31, metadata/icon/apphost binding, ZIP roundtrip and
  isolated native startup/duplicate/normal close (owned PID343708, exit0).
- After the user's tray-exit confirmation, verified no existing pet process and
  launched candidate07: PID222968, Responding=true, diagnostic Started at
  `2026-10-14T09:16:32Z` (18:16 KST). This confirms local startup, not user
  acceptance of the gesture. Installed replacement, actual login/uninstall,
  environment/rights gates remain open. No startup setting mutation,
  commit/push or publication. Canonical96 asset hash remains
  `699348D1973709F228D843341AC5312AA7F449D57B9BFC76576256231E259A78`.

## Upright tracking interpolation — 2026-10-14

Later user visual acceptance is recorded in [TASKS](../../TASKS.md#upright-tracking-interpolation--2026-10-14);
the following pending statements describe the original verification handoff.

User accepted the two snack corrections, then reported newly shimmering face ink
in ordinary tracking tilt and approved head-only resampling without renewed blur.

Cause and scope:

- The shared-source change used a4x texel-replicated eye-working head directly in
  a rotated96px target. WPF Linear point-like sampling of that reduction changed
  the selected narrow ink coverage abruptly with small angles. Ordinary upright
  tracking uses this branch; it is **not** the NearestNeighbor final crouch pass.
- Replacing Linear with HighQuality in that same draw produced byte-identical
  sweep metrics/output. An exact coverage-integration trial reduced temporal
  artifacts but reduced line contrast and took median16.96ms; rejected, code removed.
- `HuntHeadSampling` recovers native texels from the replicated grid (exact for
  undeformed artwork; area resolve only for the existing subpixel eye field),
  then applies one inverse-mapped bounded Catmull-Rom reconstruction. Local color
  and alpha bounds prevent ringing; premultiplied channels remain <=alpha. The
  result is composited1:1 with the untouched body. No final-image blur/sharpen,
  temporal history blending, angle snapping, source-image editing or new rig.
- Only the existing upright branch and its already-existing near-neutral recovery
  endpoint use this sampler. Other crouch composition/outline and snack rendering
  keep their previous paths. Captured upright cheek/readiness uses the same sampler.

Evidence root: `artifacts/tracking-sampling-20261014/`.

- `tools/SnackProof --gaze-sweep`:241 frames, roll-.3..+.3 in.0025rad steps,
  fixed gaze(.4,.3). Native plus4x diagnostic PNGs and PBGRA in `before/`,
  `high-quality-trial/`, `coverage/`, `cubic/`; `metrics.json` in each.
  `rotation-before-after.mp4` shows old left/new right, without text overlays.
  These are renderer exports, not desktop captures. Inspected tilted/neutral
  native and enlarged frames; user runtime visual acceptance remains necessary.
- Mean absolute temporal second difference in fixed head ROI: old1.14991,
  unchanged HighQuality1.14991, coverage.45215, selected cubic.41298.
  Mean spatial gradient energy:4186.85 /4186.85 /2584.14 /3100.64 respectively.
  These quantify a fixed sweep, not perceived blur percentages. Aliasing itself
  inflates the old gradient energy. The new reconstruction also exceeds ordinary
  native bilinear line contrast in four tested angles, rather than passing only
  by smoothing away all detail. Render median6.59ms/p95 11.03ms on this host;
  not an all-state/mixed-DPI performance guarantee.
- Focused51/51 passed (`results/cubic-targeted-final.trx`). New checks cover exact
  original open/closed head texels at rest, reduced continuous-angle curvature,
  bounded retained ink energy, higher contrast than native bilinear, repeatable
  frozen output and unchanged body outside the rotated head footprint. Existing
  source/cheek/flutter/chewing checks passed. An initial new body assertion wrongly
  included a legitimately rotated hair tip below row76; corrected to exclude the
  actual head footprint, not to permit body changes.

- Full App1150/1150 passed (`results/app-full.trx`). Publisher/package validation
  passed: exact tested App/Core DLL parity, runtime8.0.31, archive roundtrip,
  isolated native startup/duplicate/WM_CLOSE (PID256648 exited0). Core behavior
  is unchanged in this correction; its previous252-pass record is not a new run.

Local candidate06: `artifacts/product-shell/candidate-20261014-tracking-sampling-06/runtime/Dororong.exe`.
ZIP SHA256: `272F86524831BB056783A12A231EBDE361E6FABC1C1B7D21F735A21318E73F82`.
After confirming the old process had exited and no Dororong.exe/Dororong.App.exe
remained, launched PID267976. It remained responsive; Started log
2026-10-14T08:15:51.7941749Z, no fault for that process at handoff.
The user's delayed tray-exit reply arrived after this initial launch; PID267976
had stopped normally at08:16:16Z. Rechecked no existing pet, relaunched the same
candidate as PID92548, confirmed responsive and Started08:17:14.8864610Z.

Existing installed app, startup registration, assets, commit/push and public
release are untouched. Candidate06 replaces05 for local tracking review only.
The user's runtime judgment of tilt smoothness/sharpness remains open; technical
metrics and automated checks do not substitute for that acceptance.

## Snack entry and visible chewing — 2026-10-14

Subsequent user review: `문제 없다` accepts these two snack corrections in the
running candidate05. The original cursor-start report was not identically
reproduced; do not retroactively claim its complete cause. The new tracking
shimmer report is handled separately above.

Resumed on the user's explicit `작업 시작`. Two candidate04 observations are
recorded in TASKS; this run does not assume their full cause from the report.

- Confirmed entry defect: `ToggleSnack` unconditionally cleared platform motion
  and the presenter sole correction. An actually seated presenter on a window
  then lost exact contact when returning upright and fell through that window.
  Keep the existing support/correction when changing activity; only an attached
  perch is explicitly released. Direct-carry cancellation retains its own cleanup.
  The corrected real-presenter regression failed on the old code for sitting
  (`entry-before2.trx`) and passed after the change (`entry-after.trx`). An earlier
  fixture started below the window and fell to the floor before the tested action;
  that fixture failure is not used as evidence of the production defect.
- Chewing already advanced in time, but the old x25..33/y58..62 warp pinned the
  upper/outer lip to its boundary and moved the rest by less than one native pixel.
  Peak difference was only6 pixels changing red by>=40; half-beats had0. The actual
  loop/presenter regression found0 readable beats rather than3 (`before.trx`).
  Expanded the bounded source-mouth warp to x24..34/y56..64, with approximately
  one-pixel lift and a small lobe-width change. The existing omega artwork, eyes,
  chin, alpha and all outside pixels are preserved; no replacement mouth asset.
  Peaks now change18 pixels by>=40, half-beats12, and all three rests return exactly
  to source. This is a diagnostic contrast threshold, not user visual acceptance.
- Focused21/21 passed (`focused.trx`): real-loop eating has three beats, exact
  local-source rendering/return, idle/sitting starts away from pointer, seated
  window/taskbar support, moving window support, perch release, existing chase,
  jump, interruption and unavailable-input behavior. The shared runtime harness
  now includes snack presentation and opt-in production-style timed rendering,
  sitting and locomotion blocking instead of testing only position numbers.
- Diagnostic tool: `tools/SnackProof --mouth-metrics` and `--mouth`. Inspected
  `artifacts/snack-start-chew-20261014/mouth.png`, a renderer crop, not a desktop
  capture. Whole-character assets and the approved sitting bank are unchanged.
- Full App1142/1142 and Core252/252 passed (`app-full.trx`, `core-full.trx`).
  Real MainWindow start/cancel/close smoke passed without diagnostic errors.
  Publisher/package validation passed: exact tested App/Core RID parity,
  self-contained runtime8.0.31, archive roundtrip and isolated desktop startup,
  duplicate instance and normal WM_CLOSE (test PID267376 exited0).

Local candidate05: `artifacts/product-shell/candidate-20261014-snack-entry-chew-05/runtime/Dororong.exe`.
ZIP SHA256: `1656F3A26535318BB7DD1C59EE37656BCB0FE174A0560153055D2414881F2ACA`.
Confirmed no existing Dororong.exe/Dororong.App.exe, then launched PID256544.
It remained responsive; Started log2026-10-14T06:26:09.6448792Z, no fault for that
process at handoff. Existing installed app, startup settings, source images and
public/private GitHub state were not changed. No commit/push/release.

Remaining boundary: ordinary/sitting start tests do not reproduce a character
initially appearing **at the cursor**. Asked which menu and preceding pose; no
answer yet. The confirmed support loss must not be claimed as the full cause of
that report. Local candidate05 is running for user review; visual/interaction
acceptance remains open. It supersedes candidate04 for this local review only.
Evidence root: `artifacts/snack-start-chew-20261014/` (TRX files in `results/`).

## Shared-source correction — 2026-10-14

User approved the proposal in the diagnostic below. Production changes now:

- `LocomotionFrames` returns the cleaned canonical standing object at zero
  movement; actual post-walk idle no longer selects the different bank endpoint.
  Native walking uses the same extracted source body/head as snack running, with
  the existing diagonal foot timing and small integer head bob. No head resize.
- Upright `HuntRenderer` composes canonical body ink with only hidden neck joins
  and the posed original head at native size. Crouch blends its complete upright
  body ownership near the endpoint, not only a rump patch. The first tiny crouch
  amount eases the join handoff with the same posed head on both sides.
- Ordinary hunting and snack jumps now share the same sampling: posed head on
  the working grid, then native texel selection without a second linear filter.
  Existing crouch outline/flutter finishing remains intact. It is not a global
  nearest-neighbor switch on the WPF image or a redraw of the character.
- `BodyBreathingFrame` moves only a bounded torso field (maximum0.4px outward);
  source head texels and feet remain exact. Whole-image breathing scale is1.
  Seated/sleep artwork remains untouched and is no longer enlarged for breathing;
  new localized breathing is limited to upright idle/tracking, not a new sleep rig.
- All source assets and the accepted sitting bank are unchanged. Canonical225
  SHA256 remains `F96EC30CBD18429E6BA1138BFA4EB44F331974C9820D36EE97A02FE518E46504`;
  locomotion bank remains `A2DB46AC7F3B781327A80AAEDDE0755CD9C7768D11E66FE76407358F4E8BBC57`.

Evidence under `artifacts/snack-20261014/`:

- `render-consistency-final.png`: initial idle, both neutral tracking callers,
  locomotion neutral, snack neutral and actual post-walk idle all have0 changed
  body pixels and0 silhouette differences in the diagnostic ROI. Active walking
  head-detail measure is22343.9804, equal to source at this phase, versus6437.6134
  before. The test also compares every head pixel in32 walking phases after bob
  registration; the measure is not a perceptual blur percentage.
- `source-motion-v3.png`: actual WPF source frames on white/dark backgrounds;
  rows are gaze extremes, near-neutral crouch entry/exit, walking, torso breathing,
  and sitting. Inspected these as renderer proofs, not desktop captures.
- Initial full run:1111 passed/15 failed. Fourteen failures assumed fixed idle
  bitmap bytes/object identity despite breathing now being in the raster. Updated
  those checks to exact eye pixels, canonical source family, unchanged landing
  geometry and bounded connected torso excursion. The remaining real first-crouch
  neck discontinuity was fixed, not relaxed. A trial native outline rewrite broke
  six outline/flutter checks and was removed; the existing finishing pass remains.
- Affected-scope196/196 passed (`results/app-source-targeted-v3.trx`);
  latest Core252/252 and full App1134/1134 passed (`results/core-source-final.trx`
  and `results/app-source-final.trx`). These include original sit-bank hashes,
  direct capture/flutter/landing and new neutral/32-phase head/torso invariants.
  Cached snack render benchmark on this host: median0.35ms/p95 6.84ms, not a
  mixed-DPI or all-motion frame-budget guarantee. User visual acceptance is open.

Local handoff: `artifacts/product-shell/candidate-20261014-shared-source-04/runtime/Dororong.exe`.
Publisher and package validation passed: tested App/Core DLL parity, runtime
8.0.31, archive roundtrip, isolated native startup/duplicate/normal close.
ZIP SHA256: `C6E3572467BD5B6CC5668CABC817F02A3D83ED9BF2E807195F6B0DF27B9B4D84`.
Confirmed no existing Dororong process, then launched this candidate, PID324016;
process remained responsive and its Started log is2026-10-13T15:21:17Z
(2026-10-14 local). No new Fault log at the handoff check.

No installed-app replacement, commit, push, public release, startup registry
mutation or other-PC validation in this correction. This candidate supersedes
candidate03 for local user review, not installed/public release acceptance.

## Previous diagnosis: cross-state body and sampling mismatch

The user subsequently reported inconsistent sharpness across states and a
different ordinary/tracking body. This diagnostic does not change production
code or imply acceptance of candidate03.

Reproduce: `dotnet run --project tools/SnackProof/Proof.csproj -- --consistency artifacts/snack-20261013/render-consistency.png`.
Native source comparisons remove breathing, head angle, window placement and
DPI differences. Columns: initial standing, neutral tracking linear, neutral
tracking nearest, locomotion neutral, neutral snack, active walk, actual idle
after a scripted walk. First row native size; second row diagnostic enlargement.

- Actual presenter switches from `UprightRumpSource.Standing` to the locomotion
  bank's neutral frame after `_locomotionStarted` becomes true. The two idle
  sources do not match. In the fixed lower-body ROI x13..82/y73..89,332 pixels
  differ and29 pixels change the alpha>=128 silhouette classification. The
  actual walk->idle presenter capture reproduces the same result.
- Neutral tracking reconstructs joins/body and then overlays only a bounded
  original rump patch. The same ROI has427 differing RGBA pixels but only one
  threshold-silhouette difference: much of this neutral mismatch is edge/ink
  rendering, not a large body size change. Changing only linear to nearest does
  not fix it. The original body was not reused wholesale.
- Neutral snack matches the original lower body exactly in this ROI. Neutral
  heads also have equal measured detail. In contrast, the sampled active walk
  has lower head detail before any presenter breathing transform. Its export
  path filters a transformed high-resolution intermediate back to96px. Active
  gait body differences themselves are expected movement, not identity errors.
- Previous whole-image breathing resampling and state-specific sampling modes
  add further visible inconsistency. Existing transition raster blending can
  also mix unlike body contours; its contribution was not quantified here.

Correction proposal: one authoritative standing body and source-part set for all
neutral endpoints; preserve existing approved seated endpoint. Derive movement
from these shared parts, transform only moving parts, sample once for native
output, and remove whole-character breathing scaling. Enforce neutral body
pixel/contour identity before tuning sharpness across moving states. Do not make
everything equally blurry or apply global nearest-neighbor as a substitute.
No complete sweep of every carry/perch/interaction frame or mixed-DPI display is
claimed. This turn changed the diagnostic tool and records only, not the app.

## Follow-up review: landing, flight and original mouth

The motion-02 candidate was **not accepted**: user reported a landing forepaw
jab, airborne cursor following, and replacement lips instead of chewing the
existing mouth. The following results supersede those portions of motion-02.

- Cause: landing mapped a32% compression back into `HuntPose` recovery, which
  also extends forepaws; airborne `SnackSession` recalculated movement/facing
  from the live pointer every tick. Landing now stays at planted frame156.
  Flight snapshots its bounded horizontal endpoint/facing, integrates a smooth
  path independently of subsequent pointer movement, and eases the first/last
  12% of the vertical arc. The middle remains parabolic; peak stays at most76DIP.
- Closed chewing now warps only the original mouth texels in a9×5 native region.
  No replacement lip geometry, white cover or added blush remains in this phase.
  Rest endpoints are source-identical. The temporary open mouth retains the
  source upper lip and adds the reference-like lower opening/tongue underneath.
- Core252/252: `artifacts/snack-20261013/results/core-arc-mouth.trx`.
  Latest affected App163/163: `app-arc-mouth.trx` in the same folder. Tests include
  pointer reversal during flight, captured landing endpoint, zero endpoint
  velocity, unchanged landing paw pixels and exact original-mouth return.
  The1117 full App run below belongs to motion-02, not these later edits.
- Inspected actual WPF output `arc-mouth.png` and enlarged source comparison
  `mouth-source-compare.png`. These are renderer proofs, not desktop captures
  or user acceptance.
- New local candidate `artifacts/product-shell/candidate-20261013-snack-arc-mouth-03/runtime/Dororong.exe`:
  publisher and package validation passed, including tested App/Core DLL parity,
  pinned runtime, archive roundtrip and isolated native start/duplicate/close.
  ZIP SHA256 `D84B423CA16B407E7CBD4BFB83ECE22606B27716E02ECCC19EB642D6D5D5F2E9`.
  User confirmed tray exit; new candidate launched. No installed replacement,
  public upload, commit or push. Visual acceptance remains open.

### Breathing blur diagnosis — production behavior NOT changed

`ApplyBreathing` scales the whole96px image to1.024×1.012 at maximum inhale.
The image uses WPF `HighQuality` filtering around a fractional pivot. A controlled
actual-presenter A/B kept source, facing, phase and other transforms fixed and
changed **only** `ImageBreathingScaleTransform` to1×1. The resulting
`artifacts/snack-20261013/breathing-ab.png` was inspected: upper row breathing,
lower row scale disabled; columns phases0/.19/.4/.7. Source pixels and renderer
are otherwise identical. Reproduce with `tools/SnackProof --breathing`.

At phase0 the captures were byte-identical. At maximum inhale9166 channel bytes
differed. A fixed face-region Laplacian-energy diagnostic went from22343.98
(scale1) to6400.31 (scaled); this is a controlled high-frequency-detail metric,
**not** a perceptual percentage of blur. Both image inspection and the isolated
transform test confirm resampling softens face/hair/ribbon detail.

This is **not the sole snack-blur explanation**: `ApplySnack` explicitly sets
both breathing scales to1; prior snack composition added a separate resize path.
Fractional window/DPI placement and other rotating poses were not exhaustively
audited in this comparison.

Proposed fix, awaiting approval: stop scaling the entire sprite. Keep head/face/
ribbon pixels fixed and animate only a small, source-aligned torso breathing
pose with planted feet. A temporary constant1×1 scale is the simplest sharpness
fix but removes breathing. Globally forcing nearest-neighbor is not recommended:
it can trade softness for stepping/shimmer without fixing whole-body rescaling.
No breathing implementation or acceptance is claimed in this follow-up.

## Cause and correction

- The previous snack compositor filtered a native96 hunting composite into a
 192px target and then displayed it at96DIP. It also used the walking bank for
 running and changed only world height for jumping. The new stationary/eating
 compositor copies native pixels and composites only local effects. Exact-pixel
 tests compare every pixel outside the mouth/drool region with the approved
 standing frame; alpha-only tests were insufficient to detect the earlier blur.
- `SnackRunFrames` defines four asymmetric contacts, short support, gathered
 swing and suspension, instead of calling `LocomotionFrames.Walk`. It samples the
 original body once and copies the rigid head at an integer bob offset. The far
 hindpaw uses existing hindpaw pixels behind the torso. No new character asset.
- `SnackSession` now owns anticipation/flight/landing clocks. `SnackRenderer`
 samples the authored hunting crouch/recovery for jumping, with unfiltered
 working-grid composition only on this new path. Existing hunting rendering
 keeps its previous sampling mode. Height remains the platform system's job.
- Mouth is open/accept/close then three closed-lobe chewing beats. `Settling`
 retains exclusive PetLoop control for320ms after eating. Presenter also blocks
 hunting if polled independently during snack control.
- Hunting behavior still cancels immediately on pointer departure, but its
 display follows the authored recovery. Cheek capture uses the displayed frame,
 not the already-cancelled behavior frame. Legacy reflection fixtures needed
 the additional displayed-frame seed; assertions were not weakened.

## Observed evidence

- Core Release250/250: `artifacts/snack-20261013/results/core-motion.trx`.
- Full latest App Release1117/1117: `artifacts/snack-20261013/results/app-motion-full.trx`.
- Affected App Release321/321: `artifacts/snack-20261013/results/app-motion-targeted-final.trx`.
  Includes jump/landing, eating ownership, native pixel identity, asymmetric
  gait/suspension, hunting/tracking/pounce/cheek and platform regressions.
- Initial affected run:313 pass/4 fail. The four fixture failures seeded only
  behavior frame, leaving the new displayed frame at rest. Both reflection
  fixtures now seed the same actual rendered pose. Original failures retained in
  `app-motion-targeted.trx`; the passing rerun above is the newer result.
- Actual MainWindow snack start/cancel/close smoke passed without diagnostic
  errors. This is not a user acceptance or desktop motion capture.
- Actual WPF renderer strips inspected on light/dark backgrounds:
  `artifacts/snack-20261013/motion-final.png`; actual hunting departure sequence
  `hunt-recovery.png` was also inspected. Continuous180-frame60fps renderer
  export: `motion-v3.mp4` (run/jump/eat panels; no desktop/cursor-capture claim).
- Renderer benchmark140 measured frames after10 warm-up frames: median0.29ms,
  p956.76ms on this host. Includes run-cache misses; not an FPS/latency guarantee.
  Run cache has a finite48phase×9amplitude domain; jump cache has157 frames max.

## Local candidate

`artifacts/product-shell/candidate-20261013-snack-motion-02/runtime/Dororong.exe`

- Portable SHA256 `E632D5B7FC325B60726C44EF0EDCF87DBAC193BDDE4DD6EB90F9EBC59E1AE6BF`.
- Publisher and package checks passed: exact tested App/Core DLL parity,
  Core/Desktop/host8.0.31, renamed apphost, ZIP roundtrip, isolated-desktop start,
  duplicate exit0 and owned-process WM_CLOSE exit0.
- User confirmed the old candidate's tray exit. The old install/candidate files
  were retained; the new isolated candidate was launched for acceptance.

## Remaining boundaries

- Local playback/user acceptance of the new gait, jumping and mouth remains open.
- No claim that every historical direct-manipulation transition was redesigned;
  captures/safety cancellations intentionally retain immediate behavior ownership.
- Installed-app upgrade, real login startup and uninstall execution stay pending
  under the original feature verification. R3/R4 holds, no-public-release status,
  and incomplete formal V2 bootstrap are unchanged.

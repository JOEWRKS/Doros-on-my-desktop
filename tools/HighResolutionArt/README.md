# Original-fixed high-resolution artwork

Work in progress. Windows has an explicit opt-in grab migration test path;
default Windows and Mac runtime selection is unchanged.

## Actual Windows loop connection — r51 PARTIAL (2026-10-18)

`build-runtime-grab.cjs` makes a separate immutable-input body/head bank.
Build the app and use `--high-resolution-grab=1` (also1.25/1.5) for the local
test option only. [Evidence and known limitations](../../artifacts/runtime-grab-20261018-r51/visual-review.md).
Do not publish/enable by default: new-pose latency and other interaction maps
are unfinished. `r51-review.cjs`, `r51-verify.cjs`, and `serve-r51.cjs` provide
saved actual-loop output review; no original art is edited.

## Height / trailing motion — r50 SAVED-FRAME REVIEW (2026-10-18)

Run `HeightLagProbe`, `r50-review.cjs`, `r50-verify.cjs`, then `serve-r50.cjs`.
`height-lag-r50.cjs` follows actual platform falling rather than the no-platform
short-drop fallback. [Direct review and runtime limits](../../artifacts/height-lag-20261018-r50/visual-review.md).
Original art is unchanged; application integration remains pending.

## Paw sampling / recovery clock — r49 SAVED-FRAME REVIEW (2026-10-18)

`landing-recovery-r49.cjs` corrects lower-paw reconstruction and the actual
seven-interval+bridge recovery clock without editing source art. Run
`RecoveryTimingProbe`, then `r49-review.cjs` and `r49-verify.cjs`;
`serve-r49.cjs` serves the review page. [Direct frame observations, real WPF
evidence, softer-edge/identity-bridge limits and unverified runtime](../../artifacts/landing-recovery-20261018-r49/visual-review.md).
This is a preview correction, not application integration or full visual parity.

## Legacy comparison / anchored swing — r48 PARTIAL REVIEW (2026-10-17)

`LegacyParityProbe` captures real WPF output and spring values; `legacy-parity.cjs`
connects measured sole compression, pointer inertia, capture anchor and angled
release without editing original art. Run `legacy-parity-review.cjs`,
`legacy-parity-comparison.cjs`, `legacy-parity-verify.cjs`, then
`serve-legacy-parity.cjs`. [Direct observations, unresolved sampling/timing and
remaining runtime boundaries](../../artifacts/legacy-parity-20261017-r48/visual-review.md).
This supersedes only the covered r47 gaps, not full migration completion.

## Legacy landing restoration — r47 PARTIAL REVIEW (2026-10-17)

`restored-landing.cjs` restores the existing neutral release compression and
extreme landing curves. Run `RuntimeMotionProbe`, then
`restored-landing-review.cjs` / `restored-landing-verify.cjs` and
`serve-restored-landing.cjs`. [Exact commands, audit and review boundaries](../../artifacts/restored-landing-20261017-r47/visual-review.md).
The r45/r46 squash omission was a functional regression in the preview, not an
approved motion change. Strong paw appearance, inertia/anchors/angled release,
height triggers, secondary lag and product integration remain unfinished.

## Neutral release → landing — r45 REVIEW CHAIN (2026-10-17)

`grab-release.cjs` reuses r43 geometry in reverse with the unchanged r33 head and
r44 idle endpoints. A translation-only rebound preserves artwork; legacy squash
and arbitrary-angle/hard landing are not covered. Run `grab-release-review.cjs`,
then `grab-release-verify.cjs`, and `serve-grab-release.cjs` for the saved page.
Evidence: `artifacts/grab-release-20261017-r45/visual-review.md`. Delivered
`preview-*.png` uses the unchanged saved chain; release is6.67x slower for review.
This supersedes r44's release-open state only for this neutral preview path.

## Matched idle → entry → held tilt — r44 REVIEW CHAIN (2026-10-17)

`grab-idle-tilt.cjs` independently renders current idle body with the approved r33
head, then connects unchanged r43 entry geometry and r37 held tilt. This versioned
preview selects the matched idle; historical common-motion families stay intact.
Run `grab-idle-tilt-review.cjs` to completion, then `grab-idle-tilt-verify.cjs`.
`serve-grab-idle-tilt.cjs` serves the saved review page. Evidence and precise
observation limits: `artifacts/grab-idle-tilt-20261017-r44/visual-review.md`.
Other expressions/families, release/landing and app integration remain open.

## Source-traced entry — r43 REVIEW PREVIEW (2026-10-17)

`grab-eight-traced.cjs` corrects02–07 against authored source contours.
`grab-boundary-connected.cjs` uses corresponding anatomical contour sections,
fixed r33 head, and local signed endpoint raster residuals. r42 total-length
correspondence flattened the emerging paw and was rejected.
Run `grab-boundary-review.cjs`, `grab-boundary-verify.cjs`, then
`grab-boundary-present.cjs`. `serve-grab-boundary.cjs` serves that review folder.
Evidence: `artifacts/grab-boundary-connected-20261017-r43/visual-review.md`.
Endpoint **bodies** match with the same approved head. Full old idle still differs
because its older head mask is selected there; no runtime integration claim.

## Eight-source body entry — r40 PARTIAL PREVIEW (2026-10-17)

`grab-eight-source.cjs` fits body ink from the eight original body-drag keys and
interpolates contour coordinates; it does not blend transparent sprite images.
Run `grab-eight-sequence.cjs`, then `grab-eight-sequence.test.cjs` for three-size
saved frames and independent APNG checks. `serve-grab-eight.cjs` serves only the
review folder on a random loopback port. Original artwork/product code untouched.
Evidence: `artifacts/grab-eight-source-20261017-r40/visual-review.md`.
The new first/last contours still differ from current idle/accepted held;
do not select this family in runtime before boundary matching/user review.

## Fixed-endpoint transport — r39 FAILED REVIEW (2026-10-17)

`grab-entry-transport.cjs` preserves exact idle/held endpoint pixels, but its
two-source landmark transport ghosts intermediate paws. Do not select it.
`grab-entry-layer-audit.cjs` shows exact static raster partitions, **not** closed
animation-ready skin layers. `grab-entry-transport.test.cjs` checks saved output
identity and limited protection; passing tests do not override visual failure.
Evidence: `artifacts/grab-entry-transport-20261017-r39/visual-review.md`.

## Entry body reconstruction — r38 FAILED REVIEW (2026-10-17)

`grab-entry-study.cjs`, `grab-entry.cjs`, `grab-entry-endpoint-check.cjs` are
isolated diagnostic tools, NOT the current motion entry point. Saved evidence:
`artifacts/grab-entry-20261017-r38/visual-review.md`. The source-key contour
candidate fails endpoint preservation and retains an angular hind-foot notch.
Do not connect or publish it. No full continuous-frame review was performed.

## Held-pose tilt transition — r37 (2026-10-17)

`node tools/HighResolutionArt/grab-tilt-transition.cjs` exports isolated eased
head tilting using immutable r33 artwork/r35 composition. This does NOT replace
the rejected idle-to-grab body path. Output: `artifacts/grab-tilt-transition-20261017-r37/`.
`node tools/HighResolutionArt/grab-tilt-transition.test.cjs` verifies saved files,
lower-body invariance and independent decoding of every APNG presentation frame.
See output `visual-review.md` for actual observations and unverified boundaries.

## Intermediate-angle audit — r36 (2026-10-17)

`node tools/HighResolutionArt/grab-tilt-audit.cjs` inspects the unchanged r35
renderer at 0.5-degree steps across seven sizes, with finer zero-boundary probes.
`node tools/HighResolutionArt/grab-tilt-playback.cjs` builds the presentation
APNGs from saved frames and independently decodes/verifies every frame.
Output: `artifacts/grab-tilt-audit-20261017-r36/`; use `playback-*.png` for playback.
Direct ordered-sheet observations are in `visual-review.md`. This is not a
claim of real-time video observation or runtime integration. Artwork unchanged.

## Left attachment occlusion — r35 (2026-10-17)

`node tools/HighResolutionArt/grab-left-occlusion.cjs` uses unchanged r33 sources
and adds a local body-strip occlusion behind the tilted head in an isolated study.
Output: `artifacts/grab-left-occlusion-20261017-r35/`. The 63 stills and 14 sheets
have separate technical manifest and direct `visual-review.md` observations.
`--verify-replay` additionally checks hashes against a previously generated matrix.
No transition/runtime selection. User visual acceptance and temporal behavior are
not established by the static comparison. Source images are never overwritten.

## Scale/rotation inspection — r34 (2026-10-17)

`node tools/HighResolutionArt/grab-rotation-review.cjs` reads r33 sources and
exports 54 whole-pose and 15 diagnostic head-only stills, no artwork edits.
`node tools/HighResolutionArt/grab-rotation-detail.cjs` exports the local detail.
Output: `artifacts/grab-rotation-review-20261017-r34/`; direct observations are
in `visual-review.md`, not implied by generation. Head-only +16-degree left
attachment needs work. No transitions/runtime integration or full visual pass.

## Local left spike removal — r33 (2026-10-17)

`node tools/HighResolutionArt/grab-left-spike.cjs` generates and checks the
isolated still in `artifacts/grab-contour-still-20261017-r33/`. Uses preserved
r32 source; removes only the four user-authorized dark stalk pixels. Saved
source/output locality checks are not automatic visual acceptance. Before/after
white and dark inspection is in the verification document, r33. No transitions.

## Exterior-white junction mask — r32 (2026-10-17)

Still-only candidate; not connected to transitions or runtime. Generate with
`node tools/HighResolutionArt/grab-junction-mask.cjs`, inspect with
`node tools/HighResolutionArt/grab-junction-inspect.cjs r32`, verify saved files
with `node tools/HighResolutionArt/grab-junction-mask-check.cjs`.
Outputs: `artifacts/grab-contour-still-20261017-r32/`. Source r31 is preserved.
`masked-head-225.png` removes exterior white alpha; `locked-head-225.png` remains
the original. Same pen, local right body attachment correction. Visual review
and scope limits: verification document, r32. User acceptance is still pending.

## Original grab contour still — r31 (2026-10-17)

Isolated candidate, **not accepted and not transition-connected**. Generate with
`node tools/HighResolutionArt/grab-contour-still.cjs`; inspect saved output with
`node tools/HighResolutionArt/grab-contour-still-check.cjs`. Outputs:
`artifacts/grab-contour-still-20261017-r31/`. The source08 body/forepaw contour
is reconstructed separately; accepted r27 head RGBA remains unchanged.
Same-head-size comparison and residual measurement limits are documented in
`docs/verification/2026-10-17-continuous-scale-review.md`, r31. The r30 body is
preserved outside the inner hind-foot edit. Junction defects remain unresolved;
`node tools/HighResolutionArt/grab-junction-inspect.cjs r31` reproduces diagnostic
layer/region views. Do not select this
candidate for runtime or use its numerical checks as visual acceptance.

## Direct-motion preview — r29 (2026-10-17)

**REJECTED: the visual review claim is withdrawn. Do not treat this candidate as
approved artwork.** See `docs/verification/2026-10-17-continuous-scale-review.md`,
`r29 rejection and reporting incident audit`. Diagnostic comparison only:
`node tools/HighResolutionArt/r29-rejection-audit.cjs`. No artwork fix yet.

`direct-motion.cjs` adds grab/hold/swing/release and strong landing using the
accepted head. Run `node tools/HighResolutionArt/direct-motion-review.cjs` and
`node --test tools/HighResolutionArt/direct-motion.test.cjs tools/HighResolutionArt/current-motion.test.cjs`.
Outputs: `artifacts/direct-motion-20261017-r29/`. Final review:254 frames and30
size samples; details and conservative swing-cap limits are in
`docs/verification/2026-10-17-continuous-scale-review.md`, r29.
Perch, cheek/paw/local body pull, intermediate eyes, arbitrary grab anchors/hair
lag and runtime integration remain. The r28 pending list below is historical.

## Current common-motion preview — r28 (2026-10-17)

The user accepted r27. `current-motion.cjs` selects that head for the common
families and connects accepted r23 sleep/wake, body click and snack overlays.
Run `node tools/HighResolutionArt/current-motion.cjs --full`, then
`node tools/HighResolutionArt/current-motion-handoff.cjs` for the no-text video
frames and scale sheets. Test with `node --test tools/HighResolutionArt/current-motion.test.cjs`.
Outputs: `artifacts/accepted-motion-connection-20261017-r28/`.
Review and limits: `docs/verification/2026-10-17-continuous-scale-review.md`, r28.
Grab/perch/cheek/paw/local body-pull/hard-landing and intermediate eye expressions
are still unmigrated; this is not a complete interaction or runtime replacement.
Historical sections below retain their then-current acceptance boundaries.

## Current endpoint candidate — r27 ink/junction correction (2026-10-17)

Same `pointedHairTip:true` opt-in now uses original-range near-black ink and a
matched junction width, retaining the pointed endpoint. The user's gray-line
rejection supersedes r26's visual pass. `pointed-tip-proof.cjs` writes fresh
artifacts to `artifacts/pointed-hair-tip-20261017-r27/`; r26 outputs are retained.
Proof includes a three-way crop, protected head/body assertions, dark junction
connectivity, native core widths and288 size/rotation outputs. Runtime unchanged.

## Pointed endpoint candidate — r26 (2026-10-17)

User explicitly chose a pointed tip rather than the original blunt endpoint.
Use `connected-families.create({pointedHairTip:true,size:144})`; this includes
r25 boundary restoration, without contrast recovery. Only source225 rectangle
x97–110,y161–169 is editable. Run `node tools/HighResolutionArt/pointed-tip-proof.cjs`
for protected-region assertions, before/after and288 size/rotation frames.
Outputs: `artifacts/pointed-hair-tip-20261017-r26/`. User acceptance is pending.
Historical opt-outs and runtime assets remain unchanged; r25 snapshot hashes
describe the earlier source revision, not the newly extended shared modules.

## Current head candidate — original cut restoration r25 (2026-10-17)

User rejected r24's still-missing inner hair-tip stroke. r25 repairs the clipping
boundary using original pixels; it does not use contrast recovery or redraw ink.
Use `connected-families.create({restoreHairTip:true,outlineRecovery:false,size:144})`.
Run `node tools/HighResolutionArt/hair-tip-proof.cjs --source-only` for source-first
comparison, then the same command without the flag for288 rotation/size frames.
Tests: `node --test tools/HighResolutionArt/hair-tip.test.cjs`.
Outputs: `artifacts/hair-tip-boundary-20261017-r25/`.
User visual acceptance pending; app/install/slider unchanged. Evidence and
scope: `docs/verification/2026-10-17-continuous-scale-review.md`, r25 section.

## Head-outline candidate — r24 (2026-10-17)

`node tools/HighResolutionArt/head-outline-proof.cjs` exports288 size/rotation
frames and16 review sheets. `node --test tools/HighResolutionArt/head-outline.test.cjs`
checks preservation and bounded chin width. `head-outline-handoff.cjs` exports
the short before/after sheet and source/frame hashes after review.
Use `connected-families.create({outlineRecovery:true,size:144})` for the candidate;
the default stays historical/off. This recovers only existing lower-head ink
contrast and never changes source art or alpha. User review is pending.
Evidence: `docs/verification/2026-10-17-continuous-scale-review.md`, r24 section.

## Current review: original sleep contour still — r23 (2026-10-17)

`node tools/HighResolutionArt/sleep-reference-still.cjs` exports the original and
an isolated static body/feet contour study with the approved head locked.
Output: `artifacts/high-resolution-sleep-still-20261017-r23/`.
r23 corrects only the forepad divider occluded by white body padding in the head
layer. `forepaws-comparison.png` compares that region directly with the original.
This is not connected to transitions or runtime. User accepted this still on2026-10-17.
r21's visual shape assessment was withdrawn after direct original comparison.

`node tools/HighResolutionArt/continuous-scale-review.cjs` produces an isolated
1.0–1.5 size/DPI study. It does not enable a slider or alter production rendering.
See `docs/verification/2026-10-17-continuous-scale-review.md` for limits and remaining work.

## Previous sleep correction — r21 (2026-10-17)

The r20 audit found overlong forepads, a low rear lobe and fading-in ink.
r21 uses rounded resting pads, a settled rear envelope and opaque occluded ink.
`node tools/HighResolutionArt/motion-proof.cjs --full` now exports only the affected
sleep/wake sequences to `artifacts/high-resolution-sleep-20261017-r21/`.
Without `--full` it writes the original/r20/current comparison and final still.
`node --test tools/HighResolutionArt/sleep-pose.test.cjs` checks preservation,
geometry and the130 saved frames. The original96 sleep is the posture authority;
the accepted225 face texture stays unchanged. The belly settles between distinct
resting pads and the rear fold retains its inner contour. No runtime replacement.
Historical r19/r20 artifacts are retained, not overwritten or relabeled as corrected.
Their original source-hash checks describe that old revision, not current code.

## Previous shared transitions — r19 (2026-10-17)

The user accepted r18 and requested connection to the remaining motions.
At r19, `node tools/HighResolutionArt/motion-proof.cjs --full` exported r19; without
`--full` it writes only the sample sheet. `--r18` retains the earlier run exporter.
Output: `artifacts/high-resolution-connected-20261017-r19/`.
`node --test tools/HighResolutionArt/connected-families.test.cjs` checks the result.
Idle/tracking/walk/run/sit/rise/hunt/jump/eat/happy/sleep/wake share the fixed-rump
and rigid folded-paw attachment. Walk/run blend their transforms, not two images.
The accepted face layers remain unchanged; chewing alters only the mouth region.
All48 accepted full-run frames still match r18. The837 transition samples cover
nine sequences at60fps, with35 numbered detail sheets and a text-free MP4.
This is a **partial preview migration**, not an app update: squint/blink,
grab/perch/cheek/paw/body-click/hard-landing and snack jewel/drool effects still
need migration, followed by Windows/Mac origins, hit masks and runtime integration.
See the verification report's r19 section for review and test boundaries.

## Previous running socket correction — r18 (2026-10-17)

The user rejected r17's remaining hind socket bump. At r18, `motion-proof.cjs`
defaulted to `rooted-run-review.cjs`. `--r17` retains the earlier exporter.
Output: `artifacts/high-resolution-motion-20261017-r18/`;168 run PNGs,14 numbered
detail sheets, old/new comparison and a text-free four-second MP4.
`node --test tools/HighResolutionArt/rooted-run.test.cjs` checks this candidate.
The original upper rump curve ends at an exact subdivision, then one tangent
connector joins the unchanged rigid folded paw. Far socket skin is torso-clipped.
Face textures and rendered face pixels, r17 sitting and existing walking remain
unchanged. No new artwork or face redraw; no runtime selection or installation.
See the verification report's r18 section for the failed clip-only trial and
the direct visual review. Historical manifests retain their original source
hashes; they are evidence for their saved versions, not current-source checks.

## Previous motion correction — r17 (2026-10-17)

User rejected r16's seated knee bump and shortening-looking, slow run.
At r17, default `motion-proof.cjs` ran `polished-motion-review.cjs`, outputting only
the356 affected run/sit/rise frames to `artifacts/high-resolution-motion-20261017-r17/`.
Run `node --test tools/HighResolutionArt/motion-polish.test.cjs` to verify.
`motion-polish.cjs` owns the seated distal-envelope correction and forward reach:
exposed leg shape rotates without scaling; hidden socket skin blends to the body.
Its `.56`second run cadence is used by the60fps preview (old video:1.6seconds).
This is not a deployed change to app movement speed. Face/master, walking and
tracking remain preserved. Runtime integration remains pending.

## Previous accepted-source motion path — r16

The user accepted the r15 cheek-cleaned still and authorized motion connection.
`node tools/HighResolutionArt/motion-proof.cjs --r16` runs `accepted-motion.cjs`.
Check with `node --test tools/HighResolutionArt/accepted-motion.test.cjs`.
The saved r15 source feeds semantic head/body assembly directly; no old96 eye
transfer is used. Closed eyes ignore gaze displacement. Open eyes retain their
original225 face on the same body. Body geometry, timing and sampling stay intact.
604 still/motion PNGs,26 complete review sheets,14 closed-face detail sheets,
two hind-leg sheets and a text-free MP4 are in
`artifacts/high-resolution-motion-20261016-r16/`.
Squint and the remaining motion families are pending, not silently taken from
rejected candidates. Windows/Mac runtime integration and installer updates remain
pending. The sections below retain historical methods and rejected evidence;
`motion-proof.cjs --legacy-r5` explicitly reproduces the old proof, and its exported
`setup()` is retained for historical tests. New work uses `accepted-motion.setup()`.

`input/canonical-225.png` is the retained source-based master accepted as the
direction on 2026-10-16. It is not an AI restoration. Its provenance, generator,
proxy exceptions and hash are in `docs/verification/2026-10-16-scale-preview.md`.
The historical original and current production assets remain untouched.

`canonical-fixes.cjs` contains two explicit source-coordinate geometry patches:
rounded hind-leg junction/ankle and foreground ribbon stroke continuity.
It preserves every pixel outside the named regions. This is a local character
correction, not a generic automatic restoration tool. The rounder local silhouette
is intentionally not alpha-identical to the original.

Run from repository root:

```
node --test tools/HighResolutionArt/canonical-fixes.test.cjs
node tools/HighResolutionArt/preview.cjs
```

The existing workspace `@napi-rs/canvas` is used; `LOCOMOTION_CANVAS` can select an
existing installation. No package installation or network request is performed.
Preview output goes to `artifacts/scale-fixed-corrections-20261016` and may be
regenerated there. Runtime files, installers and approved fixtures are not changed.

The requested complete migration includes original-part reconstruction where no
large source exists. Track its scope and status in ROADMAP/TASKS, not here.

## Shared motion assembly

`posture.cjs` uses the source-aligned sitting body parts at rest as the shared
upright body, with the high-resolution-only folded hind contour in `body.cjs`.
This removes the old canonical-body / sitting-body ownership switch. The225px
hair/face/ribbon texture stays unchanged; original-fixed
body and shared assembly are shown separately for visual comparison. This is
not a claim that the body silhouette is pixel-identical to the original still.

`source-expressions.cjs` now transfers the actual authored96px closed-eye pixels
through `expressions.cjs`, using bounded cubic sampling without new lid curves.
The r4 redrawn closed lids were rejected for changing the expression. This retains
the original small-source detail limit, not newly reconstructed225px eye detail.
The r4 squint composition is unchanged and remains unaccepted. Open face, mouth,
body geometry and timing are not changed by the closed-eye correction.
`upright.cjs` preserves Windows foot phase/trajectory, head pivot,
eye displacement and body-only breathing. The raised tracking limit is14degrees
instead of the rejected20degree candidate; lowering remains20degrees and happy
sway retains its accepted2.16seconds and8degree peaks. The complete leg paths
now move from hip to toe; the old short ankle raster warp/cropped far paw is gone.
`sampling.cjs` samples premultiplied source pixels once into the output footprint;
there is no whole-character scale/blur animation. Art remains antialiased, not a
new limited-palette pixel style.

```
node tools/HighResolutionArt/motion-proof.cjs
node --test tools/HighResolutionArt/motion.test.cjs
node --test tools/HighResolutionArt/defects.test.cjs
node tools/HighResolutionArt/defect-review.cjs
node tools/HighResolutionArt/review-video.cjs
```

The proof exports144px character content in a192px transparent canvas, with
24px padding and logical origin unchanged. It covers open/closed tracking,
walk32, gallop48, breathing33, sit/rise65 and happy66 samples. Gait amplitude is
continuous in the renderer; exported review cycles show full amplitude only.
Manifest lists actual files and hashes. Runtime192px consumption, masks and
remaining motion families are not implemented by these preview exporters.

The r5 output is also rejected: broad96px eye transfer overwrote bangs and left
source225 upper-eye ink. It is retained for diagnosis, not visual acceptance.
`closed-eye-layer.cjs` and `closed-eye-mask-review.cjs` provide an isolated r6
face-aperture proof in `artifacts/high-resolution-closed-eye-20261016-r6`.
This candidate is not yet selected by `motion-proof.cjs` or a runtime. Source
hair ownership and remaining source96 expression detail are checked separately.
Rejected r2/r3/r4/r5 outputs remain.
The user also rejected r6 and requested a redraw. r7 is the generated donor,
not a whole-character replacement. The subsequent user-authorized r8 local
composite uses `bangs-composite.cjs`: source225 alignment/mask, protected eyelids
and mouth, original alpha, and unchanged pixels outside the patch. Export with
`node tools/HighResolutionArt/bangs-composite-review.cjs`; check with
`node --test tools/HighResolutionArt/bangs-composite.test.cjs`.
Output: `artifacts/high-resolution-bangs-composite-20261016-r8/`.
r8 was subsequently rejected for clipped bangs and blurry lids. The current local
candidate is r9: `node tools/HighResolutionArt/bangs-face-finish-review.cjs`, checked
with `node --test tools/HighResolutionArt/bangs-face-finish.test.cjs`.
Output: `artifacts/high-resolution-bangs-face-20261016-r9/`. It changes the seam and
local lid contrast, not the expression geometry. Original96px eye-detail limits
remain; this is not a user-accepted or runtime-selected asset.
r9 was also rejected for damaged eye-adjacent hair and angular lids. The current
candidate is r10: `node tools/HighResolutionArt/eye-fringe-repair.cjs`, with focused
checks in `eye-fringe-repair.test.cjs`. Output and exact imagegen donor prompt are
in `artifacts/high-resolution-eye-fringe-20261016-r10/`. It remains unaccepted and
not selected by any runtime or motion exporter; r9 is historical, not current.
r10 was rejected against the actual original, including cheeks. Current isolated
face proof: `node tools/HighResolutionArt/original-closed-repair.cjs` and
`node --test tools/HighResolutionArt/original-closed-repair.test.cjs`.
r11 starts from original225 plus original96 closed-eye measurements, NOT r6–r10.
`source-lid-contours.cjs` traces and rounds the measured raster ink boundary;
it does not use generated hair/face or a hand-authored replacement eye curve.
Output: `artifacts/high-resolution-original-face-20261016-r11/`. This original
master proof does not select or regenerate the existing posture/motion pipeline.
r11 was rejected for copying eye surroundings and leaving white rectangular
erasure. r12 (`eye-only-composite.cjs`, `eye-only-composite.test.cjs`) separates
original225 clean-face reconstruction from the transparent lid-only layer.
See `artifacts/high-resolution-eye-only-20261016-r12/layers.png` for actual saved
layers. Only original-source skin colors repair the open-eye holes; no96px skin
is pasted. This candidate remains unaccepted and runtime selection is unchanged.
r12 was rejected after direct original comparison. Current r13 preview:
`node tools/HighResolutionArt/locked-eyes.cjs` and
`node --test tools/HighResolutionArt/locked-eyes.test.cjs`.
`source-anatomy-lock.cjs` owns original fringe/rim and cheek protection; final
pixels are compared directly with canonical-225, not merely an edited clean plate.
Output: `artifacts/high-resolution-locked-eyes-20261016-r13/`. Source masks are
explicit anatomical interpretations, not proof of user acceptance. Runtime remains
unchanged and the full motion migration is still incomplete.
r14 assembles the r13 face with the previous corrected body/legs, preserving its
ribbon and semantic head/body ownership. Export with
`node tools/HighResolutionArt/face-body-assembly-review.cjs`; verify with
`node --test tools/HighResolutionArt/face-body-assembly.test.cjs`.
Output: `artifacts/high-resolution-face-body-20261016-r14/`. The previous body
source is checked against the saved r6 assembly; only its face is replaced, not
its earlier body corrections. This is a still preview, not runtime integration.
The current cheek-only follow-up is r15: run `node tools/HighResolutionArt/cheek-residue-review.cjs`
and `node --test tools/HighResolutionArt/cheek-residue.test.cjs`. Its explicit37-point
mask removes inherited iris/lower-eye remnants; all other r14 pixels stay exact.
Output: `artifacts/high-resolution-cheek-20261016-r15/`. Source/head layers are
reassembled, not patched only in a flattened presentation image. No runtime selection.
The resulting still/transparent patch are not yet selected by the motion exporter
or app. Existing expression/motion exporters remain historical unaccepted proofs.
`node tools/HighResolutionArt/closed-eye-review.cjs` exports the eye-only source /
rejected r4 / correction comparison separately, plus ten angle/gait review frames.
The far hind paw is a separate rear layer: foreground skin alone masks its ink.
The torso/near-leg exterior is drawn once; seated distal paw ink remains in front
while proximal socket ink is haunch-occluded. The far paw is exposed1.5 logical
units lower than r3 at full gait without changing its shape or phase trajectory.
Gaze weights remain continuous. Earlier raster-expression and failed candidate
modules are retained as historical evidence, not selected by setup.
`repair-review.cjs --pages` produces a complete frame index and26 numbered sheets,
plus80 walk/run hind-paw detail views. Tests do not substitute for visual review.
The source225
head boundary includes the complete ribbon. `parts.cjs` keeps semantic layers
attached to in-memory assemblies: never re-cut a flattened assembled PNG and
assume it has preserved head/body ownership. Persisted runtime layer metadata
and texture integration remain future work, not an implemented loading contract.

`review-video.cjs` prepares120 review PNGs. Optional MP4 encoding uses the already
available project ffmpeg, not a downloaded tool. The MP4 is only a motion preview;
lossless PNGs and their checksums are the artwork evidence.

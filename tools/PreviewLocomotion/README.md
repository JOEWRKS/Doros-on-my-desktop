# Walk / sit motion study

Preview renderer and deterministic frame export for the desktop product.

## Current complete-part sitting implementation

`reference-parts.cjs` draws independent closed source-aligned chest, haunch and
limb artwork, with original head pixels. Hidden attachment skin is authored in
each part. The chest and rump move rigidly; fixed-length two-segment legs place
the forefeet, reach forward with the hindfoot and then lower the rump. Small
seated-view paw contour corrections are local to those paws. Visible lines stay
with their parts; no assembled raster silhouette is retraced.

```powershell
node --test tools/PreviewLocomotion/verify-reference-parts.cjs
node tools/PreviewLocomotion/export-reference-parts.cjs
node tools/PreviewLocomotion/export-reference-parts.cjs artifacts/reference-parts-20261001 --frames
node tools/PreviewLocomotion/audit-reference-outlines.cjs after
```

Output: `artifacts/reference-parts-20261001/`. `four-poses.png` is standing,
support placement, hindfoot reach, seated. `reference-comparison.png` is original
standing / new standing / seated reference / new seated. Separate source overlays,
colour-coded depth, joint overlay and complete PNG/SVG parts are included.
`sit-preview.mp4` is the locally encoded caption-free4-second60fps preview.
The user accepted this motion/line weight for a local app update on2026-10-02.
`export-product.cjs` now bakes the same native96 sit frames (open/closed eyes)
and a reverse path for rising; the existing512 walk frames remain unchanged.
Sit playback is1050ms, with the existing650ms rise. Local packaging/install
status is recorded in TASKS; preview exports alone do not prove installation.
Tests sweep121 poses, with opaque joints, constant bone lengths, planted support
feet and rigid torso shapes. The seated reference is not a renderer dependency.
An additional perimeter-ink test checks all240 playback frames. The audit exports
all64 distinct moving poses in playback order, on white/dark backgrounds. Retained
before/after sheets show the corrected foreleg/belly outline interruptions.
Exposed borders are stroked from each original part path after layered fills;
other parts clip hidden edges. No composite contour is inferred or reshaped.
Interior/exterior ink coverage is combined by maximum and composited once,
so overlapping paths cannot darken the same antialiased edge twice. Body ink
width/color are measured from the source; an optical-width regression compares
the rendered exposed rump against it. `audit-reference-outlines.cjs single-ink`
exports the current white/dark frame review without overwriting prior evidence.
Old cutout and rendered-outline tools below are retained as historical studies.

## Historical artwork repair — original image cutouts

The normal static export now cuts the existing standing and seated PNGs into
five cropped parts each. Reassembly preserves every source RGBA pixel, including
the original rump, straight forelegs and folded hindfoot. The seated source is
used directly under the user's latest instruction; only integer alignment is
applied. No shape reconstruction or rescaling is applied to native artwork.

```powershell
node tools/PreviewLocomotion/export-original-contour.cjs
node --test tools/PreviewLocomotion/verify-source-cut-artwork.cjs
```

Output: `artifacts/source-cut-artwork-20261001/`, with transparent `standing.png`,
`seated.png`, cropped `layers/`, placement `parts.json`, white/dark review sheets
and verification. These are static exposed cutouts, **not a finished animation
rig**. Hidden joint surfaces and motion remain unfinished. The application and
installed bank are not replaced. Visual acceptance is not inferred from old chat.

### Joint coverage experiment

`source-cut-artwork.cjs` also exposes `renderTraced(angles)`: source-derived body
coverage plus authored hidden socket skin and locally bound strips along actual
cut boundaries. The exterior outline is traced continuously; the seated thigh
crease is separate and clipped to skin. The head remains the original texture.
This diagnostic supports head ±8°, forelegs ±12°, hindfoot ±18°, with 81 sampled
combinations per pose. It is not a full sitting transition or a completed leg rig.
The traced body's opaque backing changes some original partial alpha; its rest
silhouette differs by at most two native pixels in the measured comparison.
The static `render()`/export above still reconstructs original RGBA exactly.

```powershell
node --test tools/PreviewLocomotion/verify-source-cut-joints.cjs
node tools/PreviewLocomotion/export-source-cut-joints.cjs
```

Evidence: `artifacts/source-cut-joints-20261001/`: angle sheet, bare-cutout versus
completed-skin comparison, hidden surfaces and scope. Big limb bends, moving
shoulders/hips, all intermediate sit frames and temporal quality remain untested.

## Original-contour static endpoint study — rejected, historical

The new isolated `original-contour-*` modules use complete original-proportioned
closed part artwork, including hidden shoulders/thighs, then bind two anatomical
leg segments. The head keeps original source pixels; source and seated reference
PNGs are not modified. The reference PNG appears only in comparison panels.
Opaque skin overlap, hidden `rootInk` and visible distal `ink` are separate: no alpha closing,
whole-body shrink, image morph or post-render hole filling.

```powershell
node --test tools/PreviewLocomotion/verify-original-contour-source.cjs tools/PreviewLocomotion/verify-original-contour-art.cjs tools/PreviewLocomotion/verify-original-contour-render.cjs tools/PreviewLocomotion/verify-original-contour-export.cjs
node tools/PreviewLocomotion/export-original-contour.cjs --legacy-contour
```

Output: `artifacts/original-contour-static-20261001/`: transparent native96px
standing/seated PNGs, white/dark native and3x comparisons, complete-part sheet,
separate joint overlay and measured/hash evidence JSON. Individual character
images have no labels. Tests prove local structural properties, **not artistic
acceptance**. Distal hindfoot ink stays visible while root ink is occluded.
Seated floor contour still differs from the reference;
review the static endpoints before any motion connection. No animation, product
bank replacement, installation, commit or release is included. The failed earlier
prototype below remains historical and its failure is not waived.

Before extending to arbitrary animated poses, enforce measured bone lengths:
current articulation consumes tip direction, not arbitrary tip distance.

## Current correction study — incomplete, not an accepted seated pose

The user rejected the previous seated output as squashed. The source haunch had
been assigned to folding leg paint; a global alpha closing/reoutline pass then
concealed partition problems. That pass is removed. Original body tissue stays
on separately rigid chest/haunch plates, with the chest upright over planted
forelegs. Hidden source-derived socket paint is bounded by original opaque cover.
Head art and bone lengths are unchanged; no seated raster or endpoint tween.

The current output is **not ready for acceptance or integration**. Volume is
improved, but exposed forelegs still look short and the belly remains unlike the
reference. Root overlap/coverage is incomplete: focused checks are27 passed,
1 failed. The strict coverage check finds low-alpha notch pixels in28 of82
sampled sit/rise poses. Do not waive this test or use global silhouette filling
to make it pass. Complete original-proportioned part artwork/socket ownership
needs review before another animation export.

```powershell
node --test tools/PreviewLocomotion/verify-canonical-bind.cjs tools/PreviewLocomotion/verify-canonical-sit.cjs
node tools/PreviewLocomotion/export-canonical-sit.cjs --sheet-only
```

Current diagnostic output: `artifacts/canonical-volume-20261001/` (contact sheet,
standing / current seated / reference-only comparison, endpoints and joint image).
No revised movie was generated. Previous rejected output remains historical at
`artifacts/canonical-sit-20261001/`. The native application, product bank and
installed app are unchanged by this local study. Execution facts live in TASKS.

## Accepted original-first artwork and internal joints

The geometric skin below was rejected: it changed the original body/leg shape.
`canonical-bind.cjs` now separates the **entire canonical standing drawing**
into head, torso, far-front, near-front and near-hind paint. Reassembly has zero
decoded RGBA differences from the source, including antialiased outline pixels.
No substituted ellipse/capsule body, thicker foot, redrawn outline or cloned
occluded hindleg. Root/bend/tip joints are placed inside that original skin;
there is no separate ankle axis. Foreleg chains are straight at rest.

```powershell
node --test tools/PreviewLocomotion/verify-canonical-bind.cjs
node tools/PreviewLocomotion/export-canonical-bind.cjs
```

Outputs under `artifacts/canonical-bind-20261001/`: white/dark original / bound
artwork / joint-overlay comparison, transparent rest drawing, separate paint
layers, ownership map, `rig.json`, and source-hashed `verification.json`.
Joint rotation/bending actually deforms isolated original limb paint in the
diagnostic API; the subsequent sitting renderer is documented above.
The accepted forward-reach/plop timing remains the motion reference, **not**
its geometric masses as replacement artwork. No product bank or installation
changes; this section preserves the accepted static drawing/bind study.

## Historical rejected trial: geometric skin on the accepted connected rig

**Blocking motion accepted; this geometric character skin rejected.** The rig from
the forward-plop study is unchanged. `connected-sit-skin.cjs` takes its actual
pose, not a second animation timeline. Only the canonical standing PNG supplies
raster pixels: the isolated head/face/hair/ribbon moves as one rigid layer.
White body and limb shapes follow the approved masses/joints. Their union gets
one inset outline; near-hind/front interior edges establish foreground ownership.
Small root masks hide ink only, never remove opaque connecting skin. No seated
image, endpoint dissolve, animated body scaling or source-asset modification.

```powershell
node --test tools/PreviewLocomotion/verify-connected-skin.cjs
node tools/PreviewLocomotion/export-connected-skin.cjs artifacts/connected-skin-20261001 --sheet-only
node tools/PreviewLocomotion/export-connected-skin.cjs artifacts/connected-skin-20261001
```

Outputs: white/dark contact sheet, canonical / new standing / new seated
comparison, transparent4x endpoint PNGs, and720 frames for a12s/60fps movie.
The movie shows native96px and3x views on both backgrounds, normal then half
speed, with no captions or diagnostic bones. `skin-provenance.json` records
source hashes; `accepted-source/` preserves the accepted rig. Body silhouette
is a reconstructed approximation, not a pixel-identical canonical rest frame.
Eyes stay as supplied by the canonical image; gaze/blink and application state
integration are not implemented by this local skin trial. Installed app and
product banks remain untouched pending visual acceptance.

## Accepted blocking: forward-reaching hindlegs and a seated plop

**Blocking motion accepted; no character skin in this renderer.** The user requested a forward
hindleg reach followed by a plop, with limb thickness calibrated to the source.
`connected-sit-rig.cjs`
has fixed internal chest/hip attachments and four two-segment legs: forelegs
are collinear supports; hindlegs use folding IK. No terminal foot segment or independent foot rotation. Rounded tips
finish a single continuous limb outline. Spine/neck/limb lengths and rigid
head/chest/pelvis volumes stay fixed. Front tips remain planted; hindfeet lift
and reach13.5units toward the face, finishing ahead of the haunch. A held
preparation gives way to a quicker late descent and one small pelvic rebound.
Rise gathers the feet before lifting the haunch instead of reversing the fall. Sit/rise
each take900ms; the head follows the connected neck. The broad head, lower belly
and fuller haunch follow hand-read canonical mass landmarks. Far limbs stay
behind the torso; near limbs stay in front, with only small root sockets hidden.
Neither alpha nor length scaling is used. See TASKS
for calibration values and the explicit limits of this abstract study.
The renderer draws these exact mass centers/joints, not a decorative overlay on
a separate outline tween. No image assets, texture scaling or opacity fades.

```powershell
node --test tools/PreviewLocomotion/verify-connected-sit.cjs
node --test tools/PreviewLocomotion/verify-blocking-depth.cjs
node tools/PreviewLocomotion/export-connected-sit.cjs
```

Optional arguments: `[output-directory] [--sheet-only]`. Default artifacts:
`artifacts/forward-plop-20261001/`. Clean color contact sheet and720 frames at
60fps,12seconds: one normal-speed cycle then one half-speed cycle. Pink head,
teal chest, orange pelvis, blue forelegs, purple hindlegs. Darker limbs are farther
away. Parallel floor guides account for projected near/far depth; decorative
outlines are inset within the physical mass radii. No captions or character art.
`joint-sheet.png` is a separate x-ray diagnostic; the movie never uses it.

### Contracts for subsequent motion work

- Grounded forelegs drive chest height from foot anchors and fixed support
  length. Animate the support lean, not an unrelated chest-height curve. A
  future step/lift needs an explicit new contact mode, not bone stretching.
  Near/far fore widths are13/9units and hind width11, calibrated to the canonical
  near forefoot's13px section, far forefoot's8px and hindfoot's10..11px at the
  same scale. Rounded near-fore fill is11px at y82: approximate, not identical art.
  Fore support is7.5units rather than10, compensating for the larger near sole
  radius without raising the standing body. The far shoulder is fixed2units
  lower to match its smaller sole while preserving the same support/lean.
- The spine preserves24-unit length; pelvic height controls the sitting fold.
  The settling rebound belongs to the pelvis while forefeet remain planted.
- Renderer depth is far hind/front → torso/neck → near hind/front → head.
  Only a small curved root socket is masked, following the limb's root/tangent
  (at most15% of its first segment, capped at1.2units), never its parent's whole
  ellipse. Near limbs remain visible even inside their own torso mass. Joint overlays are
  opt-in (`bones:true`) and are not evidence of visible-layer correctness.
- Ground contact, straightness, connectivity and raster ordering have separate
  regressions. Passing them does not approve the visual style or unseen motions.

This is a kinematic motion proof, not a physical simulation or finished skin.
The accepted seated still remains a later appearance target. Product/browser
banks, source PNGs and installed application remain unchanged. The approved next
gate was user acceptance of this simplified body action before skinning; that
acceptance now permits the isolated skin trial above, not product integration. The
previous accepted block/video and its `accepted-source/` snapshot remain under
`artifacts/connected-sit-blocking-20260930/`.

`node tools/PreviewLocomotion/export-proportion-comparison.cjs artifacts/forward-plop-20261001` produces a
same-scale source / previous / revised comparison (standing and sitting).
It requires the local artifact's preserved `before-source/` snapshot; it does
not modify or feed reference pixels into the rig. `comparison-provenance.json`
records source hashes, measured sole alignment, registration and approximate hand-read landmarks. The
previous long-leg cartoon variant remains in `artifacts/connected-cartoon-sit-20260930/`.

## Previous visual review: joint-skinned sitting motion

**Static shape accepted; motion rejected.** User approved the revised
seated endpoint and then authorized movement toward that shape. `skeleton-sit.js`
exposes `renderSoft(time, options)` and `renderSeatedEndpoint(options)`, in addition
to its unchanged rigid-study `render()`. Inverse-bound skin controls follow actual
bone transforms; bounded armpit/paw and haunch tissue corrections respond to bone
angles. The canonical head remains rigid. Body paint is reconstructed white skin,
not preserved canonical body texels. Fixed bone lengths remain unchanged; no
seated-image pixels, endpoint contour interpolation, alpha dissolve or pose swap
are used. The tucked paw is geometrically occluded, with transverse tissue
compression but no bone shortening or opacity fade.

```powershell
node tools/PreviewLocomotion/export-seated-endpoint.cjs
node tools/PreviewLocomotion/export-skeleton-study.cjs artifacts/joint-skin-motion-20260930 --soft-skin
node --test tools/PreviewLocomotion/verify-skeleton-sit.cjs
```

Optional argument: output directory. Default artifacts:
`artifacts/seated-skin-endpoint-20260930/` — transparent endpoint, joint overlay,
light/dark comparison (reference left, candidate right), and input hashes.
Reference registration is(+3,-12), in the comparison/test only. This exporter
does not generate a movie or change source PNGs, browser, product bank or app.
The foreleg gap, haunch coverage and lower-body reference overlap have regression
checks. The approved static snapshot in `assets/seated-skin-approved-test.png` is
loaded by tests only, not by any renderer. New motion checks protect it, the
canonical standing body (including a lower-body-local comparison), silhouette
continuity, volume and path joins. Reconstructed standing skin is similar, not
pixel-identical, to the canonical image. These checks are not a substitute for
user acceptance of the actual playback. No product integration/install yet.

## Previous visual review: rigid canonical-only skeletal sit

**Not visually accepted.** The reference-alignment follow-up still differs in
foreleg length/gap and haunch contour. See the open reference-likeness item in
`TASKS.md`; passing rig/overlap checks is not a visual acceptance claim.

The previous endpoint-mixed version below was rejected as visually melting.
`skeleton-sit-rig.js` solves four fixed-length three-segment legs; each skin part
in `skeleton-sit.js` rotates/translates with its bone, without scale. Front feet
stay planted, hind feet lift/tuck, the torso rotates and the head settles later.
Only `dororong-canonical.png` is read. No seated or sleeping texture is sampled;
the final seated pose is the rig output, not an image replacement.

This is a local **visual-acceptance study**, not the installed/product renderer.
The existing browser/product bank below is unchanged pending visual approval.
Body cutout unions are re-outlined after subpixel seam closing; bounded neck and
joint paint fills newly exposed joins. Hair/face paint is retained. Consequently
the rest body is visually calibrated to canonical, not byte-identical to it.
There is no target contour blend or rest/final image shortcut. It is 2D kinematics,
not simulated animal biomechanics.

```powershell
node --test tools/PreviewLocomotion/verify-skeleton-sit.cjs
node tools/PreviewLocomotion/export-skeleton-study.cjs
```

Optional exporter arguments: `[output-directory] [--sheet-only]`. Artifacts default
to `artifacts/skeleton-sit-20260930/`: contact sheet, input hashes and900 movie frames
(15 seconds at60fps). Clean left / actual joints right; native top / enlarged bottom;
normal speed then one-third speed. Preview timing is950ms sit /750ms rise. The
exporter never writes the product bank, original assets or installed application.

## Previous product bank: articulated sit and separate rise (not visually accepted)

The final `assets/seated-final-10-2.png` stays unchanged. `seating-motion.js`
models front-foot placement, shoulder support, lifted hind-foot relocation,
two-link hind-leg IK, curved pelvic descent and delayed head settling. The
renderer transports the authored texture through those landmarks. Silhouette
blending handles changing occlusion/creases, not the motion trajectory itself.
This is a choreographed 2D kinematic animation, not a rigid-body physics simulator.

Sitting lasts 900ms; rising is a separate 650ms support/push/replant trajectory.
An interrupted movement reverses on its current path without swapping poses.
The seated and standing endpoints are shared. The Windows presenter consumes
prebaked poses for low runtime cost; there is no live mesh work per desktop tick.
`LOCO v2` stores 386 poses per eye state: 65 sit, 256 unchanged walk and 65 rise.
The browser reads `assets/seated-articulated-bank.png` and the product reads
`src/Dororong.App/Assets/locomotion.pbgra.gz`.

```powershell
node --test tools/PreviewLocomotion/verify-seating-motion.cjs tools/PreviewLocomotion/verify-seating-export.cjs
node tools/PreviewLocomotion/export-seating-motion.cjs
node tools/PreviewLocomotion/export-product.cjs
node --test tools/PreviewLocomotion/verify-articulated-bank.cjs
node tools/PreviewLocomotion/verify-seated-png-bank.cjs
node tools/PreviewLocomotion/verify-product-matte.cjs
```

The motion exporter accepts `[output-directory] [previous-atlas.png]`. Without
a previous atlas it still exports the current bank and contact sheet; only the
before/after movie frames are omitted. The previous `seated-final-10-2-bank.png`
is retained unchanged for comparisons. Set `SEATING_BEFORE_BANK` to a pre-change
LOCO v1 capture to check all 512 walking frames and four endpoints byte-for-byte.

Movement reference: [Hannah Branigan's tucked-sit explanation](https://hannahbranigan.dog/articles/the-tucked-sit/).
Timing and overlap are adapted for this short-legged, large-headed character;
they are not measurements claimed from a real animal.

## Historical: final authored 10-2 morph transition

The previous browser used the user's transparent `assets/seated-final-10-2.png`, copied
without byte changes. Its authored alpha and white body paint are preserved;
integer registration (+3,-12) aligns its head without rescaling or redesigning
the pose. Legacy opaque/JPEG import alone uses white-background matte recovery.
65 baked poses connect canonical standing to this endpoint, and standing up
uses the same bank in reverse. Head color stays in registered coordinates;
body contour depth interpolates between endpoints. A narrow premultiplied
coverage handoff matches exact Canvas-scaled endpoint pixels without an alpha pop.
Rise refinement: `sit-correspondence.js` maps matching toe, cleft, fold and rump
landmarks before sampling both contour and interior color. This replaces the
fixed-coordinate interior dissolve that made knee/leg lines smear during rise.
Interior/contour sampling blends over continuously sampled depth; it must not
switch on rounded texel cells, which caused a one-frame dark knee-ink pop.
Standing render preparation also discards faint rump specks with no adjacent
visible boundary (alpha>=40), retaining connected antialias pixels. Canonical
files on disk and the supplied seated PNG remain unchanged.
Walking remains on the existing renderer. The browser does no live morph baking.

```powershell
node tools/PreviewLocomotion/verify-authored-sit.cjs
node --test tools/PreviewLocomotion/verify-seated-png.cjs
node tools/PreviewLocomotion/verify-rise-features.cjs
node tools/PreviewLocomotion/verify-rise-color-continuity.cjs
node tools/PreviewLocomotion/verify-rump-fringe.cjs
node tools/PreviewLocomotion/export-authored-sit.cjs
node tools/PreviewLocomotion/verify-authored-bank.cjs
node tools/PreviewLocomotion/verify-ink.cjs
node tools/PreviewLocomotion/verify-delivery.cjs
```

Frames and reverse-order metadata: `artifacts/repro/locomotion/authored-10-2/`.
Bank: `assets/seated-final-10-2-bank.png`. Duration is650ms;65 uniformly sampled
pose amounts are selected with smoothstep playback timing. `export-product.cjs`
exports the native96 product bank with open/closed eyes. The old JPEG is kept
only as a legacy matte-regression fixture, not an active art source.

## Historical procedural sitting fallback

The following describes the superseded procedural sitting renderer and its
legacy tests, not the authored10-2 bank used in the current browser preview.

Run from the repository:

```powershell
node tools/PreviewLocomotion/verify.cjs
node tools/PreviewLocomotion/verify-raster.cjs
node tools/PreviewLocomotion/verify-ink.cjs
node tools/PreviewLocomotion/verify-sit-continuity.cjs
node tools/PreviewLocomotion/verify-sit-topology.cjs
node tools/PreviewLocomotion/verify-sleep-shape.cjs
node tools/PreviewLocomotion/verify-belly-join.cjs
node tools/PreviewLocomotion/verify-seated-floor.cjs
node tools/PreviewLocomotion/verify-delivery.cjs # while preview server is running
node tools/PreviewLocomotion/server.cjs
```

Open http://127.0.0.1:2785/ . The left/upper reference is the canonical image
translated without pose deformation; it is not a recording of the old product.
The new pose uses three limb influences on one continuous body mesh and a
separate rigid head/ribbon, sampled in premultiplied color at 2x. No cut-out
limb seams are composited. Source textures are embedded in the served HTML.

Controls: connected sequence, treadmill walk, sit, stand, pause/scrub, speed,
mirror, black background and authored foot-contact markers. The sequence turns
at its resting endpoints; mirroring reverses both art and travel direction.
Canonical closed eyes provide periodic blinking. Stopped standing uses the
original bitmap directly. Protected head attachment remains pinned when sitting.

Regression checks exercise fixed world-space stance, planted forefeet, tucked
rear, attachment width, and actual timeline/cycle continuity. Raster checks use
the real renderer with bundled `@napi-rs/canvas` (override its module path with
`LOCOMOTION_CANVAS` if needed): 20 full-amplitude walking phases, first-motion
alignment, preserved head/forefeet at four sit amounts, opaque rounded haunch,
no protruding paw, filled torso and visible redrawn stroke. Walking ink is
reinforced inside its existing alpha boundary, never brightening existing ink;
three sampled joints retain at least95% of resting integrated ink over a gait.

The seated rear is extracted directly from `dororong-sleep.png`: its actual
outer alpha boundary and diagonal knee ink, translated down3px to the forefoot
floor. The user accepted the convex rear volume. A shallow tangent-aware belly
bridge joins the back of the planted front paw to that unchanged rear, replacing
the deep standing-root notch; obsolete internal root ink is cleared. This
replaces the rejected invented seated curves, signed-distance blend and
flattened/sheared-foot trial. A source-shape regression detects a substituted
haunch instead of the sleeping contour. The outer
stroke clips a2.8-source-pixel round stroke inside the body (1.4px visible).
The thin sleeping knee ink is strengthened and alpha-clipped to the rear fill.
Latest seated refinement compresses that crease35% and tucks it1.5px inward.
The lower front legs continuously align to the rear's local83.75px floor:
their authored sole heights81.75/87.25 were previously unequal. The preview
settles7outputpx during sit to keep that new shared floor on the stage ground.
This intentionally supersedes exact sitting-forepaw pixel preservation; head
preservation and walking rendering remain unchanged. A raster floor test covers
all three visible soles and the lower, more compact hindleg crease.
No generated artwork is used. Head/ribbon/front feet keep their original assets.
For the first12% of sit amount, a premultiplied coverage/ink handoff between the
moving mesh and moving contour avoids an abrupt pixel-to-vector style change.
There is no standing-to-seated endpoint-image dissolve. A101-frame regression
checks bounded contour movement and pixel change, including entry; topology
checks reject self-crossing paw joins. The former green-marker transport probe
was removed because the approved rear redraw intentionally replaces that texture.
Source asset files are unmodified.

Limitations: this is a visual trial pending approval of the redrawn seated rear.
Its fixed stroke is calibrated by visual comparison with standing art, not an
assertion of identical hand-drawn ink at every subpixel. Only
steady gait contact is mathematically locked; start/stop blends gather feet.
Raster cache quantization is for the browser preview, not a production timing
contract. App physics/tray/pause integration still needs separate approval.

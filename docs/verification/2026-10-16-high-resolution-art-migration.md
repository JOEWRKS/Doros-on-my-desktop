# Original-part art migration — 2026-10-16

## Authority and scope

User preferred the original-fixed1.5x still, requested replacement of all other
character artwork, rounded hind-leg corners, and repair of the ribbon stroke
where it meets the body. User explicitly approved reconstructing poses available
only at96–100px from the225px original head/ribbon and contour parts, keeping the
existing pose and motion timing. This replaces the former one-still-only scope.
It does not authorize a new character design, changed animation timing, default
desktop size, installation, public release, or commit/push.

## Current implementation and evidence

- Retained master: `tools/HighResolutionArt/input/canonical-225.png`, copied
  byte-identically from the prior original-fixed proof, SHA256
  `1D84AA57E2824ADD4450427D56B0D45EB729B5EEDBE9F0FCC7A6FAB9F766483C`.
  Provenance: [previous proof](2026-10-16-scale-preview.md#original-fixed-15x-still-proof).
- `canonical-fixes.cjs` implements two bounded native-source geometry changes:
  hind leg `[133,169,35,31]`, ribbon junction `[157,109,10,16]` in225px coordinates.
  Hind-leg inner corner and ankle use tangent curves; foreground white ribbon
  backing and a continuous stroke separate the ribbon edge from the rear body.
  Final master differs at374pixels, zero changes outside these rectangles.
  The requested local silhouette correction intentionally changes local alpha;
  whole-source silhouette identity is no longer claimed.
- Corrected master SHA256:
  `E6CE3C1ACEF4877E7E81E50445FF681A52442ADED2A4BFCC59EEB2F696080F88`.
- Outputs in `artifacts/scale-fixed-corrections-20261016/`: `canonical-fixed-225.png`,
  `before-144.png`, `after-144.png`, `details-before-after.png`, `verification.json`.
  Opened final native144 and nearest4x detail comparison. Face/eyes/mouth remain
  untouched; rear corner is rounder and ribbon edge continues through the join.
  User visual acceptance of these new local corrections is still pending.
- `parts.cjs` partitions the corrected225 master using the existing head polygon
  in its96-unit logical coordinate system. Head/body have no double ownership;
  adding their RGBA recovers the source exactly. Moving-part backing/coverage
  beyond the tested sitting construction remains to be integrated per motion.
- `reference-parts.cjs` adds opt-in output resolution and a high-resolution head
  input; default remains96px. Existing pose equations, joint lengths, timing and
  z-order are unchanged. `sequence-proof.cjs` exports65 open-eye sit proof frames
  at144px with the225px head/ribbon, not an enlarged96px head.
- First high-resolution proof exposed a double-scale ink-layer bug. Resetting the
  coverage canvas transform before pixel upload corrected it. The final contact
  sheet was reopened; detached oversized strokes no longer appear. This failure
  is retained here rather than counted as a passing first attempt.

Tests: `node --test tools/HighResolutionArt/canonical-fixes.test.cjs
tools/HighResolutionArt/parts.test.cjs` passed9 cases. Existing
`node tools/PreviewLocomotion/verify-reference-parts.cjs` passed4. Checks cover
source identity, region isolation, re-decoded PNG pixels, transparent canvas
corners, exact part ownership, opaque sockets and ink on exposed body edges
across all65 high-resolution sit frames. These are not all-motion visual acceptance
or a proof that every default production pixel remains identical.

## Continued eye / posture / upright migration

User authorized continuing eye closure, pose connections, tracking/walking and
remaining motions while retaining original identity and existing behavior.

- `expressions.cjs`: the current user-authored96px closed/squint eye patches are
  transferred onto the225px original. A sparse difference-mask first attempt
  left iris flecks; continuous local eye-patch support removes those leftovers.
  Hair, mouth and body outside the eye support are unchanged. Eye patches retain
  their96px source-detail limit; this is not newly restored225px eye detail.
- `posture.cjs`: the accepted sitting rig's source-aligned rest body is now the
  shared body for the migrated idle, tracking, walk/run and sit/rise candidates.
  This is an explicit original-part reassembly, not pixel identity to the old
  original-fixed body. Source-fixed still is retained alongside the assembly in
  the contact sheet. Existing bone lengths, support/reach/drop order, final pose
  and1050/650ms sit/rise timings remain. No bitmap crossfade is used.
- Standing / zero gaze / zero gait / sit entry / rise finish have exact matching
  output pixels. First nonzero pose samples are checked separately to catch a
  hidden second-body switch. Reverse sequence matches the same sit trajectory.
- `upright.cjs`:225px shared parts, existing96-unit gait coordinates,32 walking
  phases,48 gallop phases, original foot trajectories / integer bob, eye offsets
  and(43,66) neck pivot. Body-only breathing retains planted feet and fixed face.
  All upright states use the same rump; tracking cannot select a wider body.
- `happy.cjs`: original2.16second,0/-8/+8/-4/0degree slow sway, closed eyes and
  exact neutral endpoints. Eating mouth/drool and hunt/jump are not yet migrated.
- A real copy-path regression was caught: canvas `high` smoothing modified even
  a1:1 head copy. Exact-size integer head copies now bypass that filter. The
  default96 renderer is checked against all130 existing product sit frames.
- A second caught boundary error assigned a pixel of the repaired ribbon to the
  body. The narrow native225 ribbon strip now belongs to the head; split ownership
  is still exclusive and reconstructs the original RGBA. Protected opaque head
  pixels, including the repaired ribbon, are checked against the original master.
- Direct bounded cubic source sampling with2x2 output-pixel coverage replaces
  the first point-sampled candidate's jagged edges. Premultiplied interpolation
  avoids transparent-edge colour fringes. No new palette restriction is imposed.

Output: `artifacts/high-resolution-motion-20261016/` —609 PNGs including source
masters/contact sheet, manifest, and a separate4second review MP4. Motion PNGs
contain144px character content at1.5x in192px canvases, origin(24,24), to preserve
the silhouette at±20degree head rotations. Logical world coordinates remain96.
The review video illustrates motion; compressed video is not a PNG pixel oracle.
Opened the light-background contact sheet and dark-background review samples.
Expression/pose readability and no obvious enlarged detached ink were observed;
this does not substitute user visual acceptance or native Windows/Mac display.

Verification: the final motion suite passed12cases; previous master/parts9 and
reference-parts4 also passed (25total). All609 saved PNG hashes/dimensions are
re-decoded; motion borders stay transparent,130 legacy product sit frames remain
byte-identical, gait equations and neutral/reverse boundaries pass.

An additional broad flood-fill diagnostic found partial coverage239 at(112,134)
in `walk-open-23.png`, in the near/far-paw overlap below the rump. The4x dark
inspection is `artifacts/scale-fixed-corrections-20261016/gait-seam-inspection.png`.
The diagnostic originally treated every enclosed background pocket between legs
as an internal body socket; that is not a valid universal occupancy contract for
overlapping legs. The test now checks explicit interior torso socket positions,
which remain opaque in all walk/run phases. The paw-overlap boundary itself is
**still a visual-review item**, not an automatically filled or verified-fixed
seam. It must be reviewed with the rest of the gait before product selection.

Next: resolve remaining paw-overlap visual acceptance, migrate hunt/jump and
sleep/grab/perch/cheek/eating, then integrate shared resolution/origin with
Windows/Mac rendering and interaction masks. No all-motion completion is claimed.
No production asset, installer, Mac ZIP, running app, commit/push or release change.

## Five-defect correction — r2

The user rejected the raised-head angle, lowered-head back notch, walking hind
thickness, gallop paw bulge and duplicated ribbon in the previous review. That
candidate is **visually rejected for those items**; its25 passing tests did not
validate those shapes. The old PNGs/video remain unchanged for comparison.

Causes and corrections:

- The old96-coordinate head polygon cut across the complete225px ribbon. More
  importantly, upright re-partitioned an already assembled body/head bitmap:
  shoulder skin became head-owned while other head details could remain on the
  body. `parts.cjs` now traces the complete source225 head/ribbon boundary and
  retains the original semantic head beside the closed body rig. All original
  opaque head texels are preserved; no head/face redraw or generation occurred.
- The neck/backing was limited to the old head mask, with no complete rear
  shoulder underneath it. Upright now samples the independently closed body
  parts, not a notched raster cut. The upper body remains covered during lowering.
- The old short ankle deformation weighted several legs across the same bitmap.
  It distorted the lower contour into a bulge. The fourth paw was a rectangular
  crop of the near hind paw, not a complete limb. Both mechanisms are removed.
  `body.cjs` opts only the high-resolution candidate into the source225 rounded,
  folded-triangle hind contour. Whole closed leg paths and their ink move with
  the same hip-to-toe mapping. The far hind limb is a complete closed part behind
  the torso, hidden at neutral and progressively exposed with gait amplitude.
- The over-raised tracking extreme changes20→14degrees, based on the user's
  angle complaint. Lowering remains−20degrees; pivot(43,66), gaze-eye limits,
  gait foot phase/target formulas, body bob, sit timing and accepted2.16second
  happy sway with±8degree peaks remain. Leg skin deformation changes deliberately;
  this is not byte-identical motion art or unchanged raised tracking range.

Current outputs: `artifacts/high-resolution-motion-20261016-r2/`.

- `five-defects-before-after.png`: old row / repaired row / repaired detail.
- `hind-source-comparison.png`: corrected original225 projection, new rest,
  walk23/32 and gallop16/48 with matching hind-leg crops.
- `walk-hind-all.png` / `run-hind-all.png`: all32/48 open-eye hind-leg crops,
  opened at integer enlargement to check the fold and phase-dependent bulge.
-609 manifest-listed PNGs, `motion-contact-sheet.png`, and `motion-review.mp4`
  (576×432,30fps,4seconds). Comparison PNGs are additional review evidence.
  The previous output directory is preserved; only r2 is the current candidate.

Validation:30 checks pass (master/parts9, existing reference4, motion12, new
defect5). New checks cover exact semantic head retention including transparency,
complete ribbon foreground ownership, no stationary coloured head remnants in
the headless body, exposed rear-shoulder alpha at lowered angles,14degree raised
limit with distinct8degree sway, and hind silhouette sections within2source
pixels of the corrected225 reference at y182–196. Existing tests re-decode all
609 manifest PNGs, compare source hashes, verify neutral handoffs and reverse
sit, and preserve all130 default96 production sitting frames byte-for-byte.
`git diff --check` passes (repository LF/CRLF warnings only).

Visual review: opened both source225 images, the old/new five-case comparison,
the source/hind comparison, the complete light-background contact sheet and all
80 gait hind crops. The back notch and duplicate ribbon are no longer visible
in those views; the hind contour follows the source fold without the prior
ankle protrusion. Raised-head exposure is reduced. This is an observed candidate
repair, not the user's aesthetic acceptance or native-app validation.

Skill influence: original-shape comparison and defect evidence take precedence
over passing dimension/pixel tests. The existing antialiased mascot contract is
retained; no binary-alpha/palette conversion or new character design is imposed.
Source, previously rejected output, current proof and runtime are kept distinct.

Remaining: user visual acceptance of r2; remaining art families and Windows/Mac
runtime layer/origin/hit-mask integration. In-memory semantic layer registration
is a preview implementation, not a persisted runtime PNG loading contract.
No installed app, original asset, installer, Mac ZIP, commit/push or release was
changed. Next boot confirmation and the existing environment/rights items remain.

## Remaining hind-line and eye defects — r3 partial repair

User rejected r2 after identifying overlapping hind-leg ink and pixelated-looking
eyes. The previous statement that no ankle protrusion was seen did not establish
correct overlap/ink ownership; r2 is not visually accepted.30 passing checks did
not cover the remaining interior line or expression source-detail quality.

Diagnosis and implementation:

- The whole hind contour had been classified as visible foreground ink, including
  the socket under the haunch. Moving it to `rootInk` alone left another seam in
  overlap views. The accepted partial fix unions the authored vector skin paths
  before drawing ONE upright silhouette outline. It does not trace a raster or
  blur/erase the rendered character. Closed body geometry and original hind fold
  stay; the default96 product renderer is unchanged. Seated intentional interior
  articulation stays separate from the upright-only suppression of internal ink.
- `upright.cjs` used an abrupt zero-displacement mouth rectangle inside the eye
  displacement field. Its discontinuity can shear a lower-eye boundary during
  gaze movement. The replacement uses a continuous weight that is exactly zero
  inside the mouth and blends outside it. This removes a code-level source of
  stepping, not the finite resolution of original eye art.
- Open eyes use the original225 texture. Closed/squint expressions still came
  from96px raster patches enlarged onto225px. They have different edge/detail
  quality and cannot truthfully be described as restored225px expressions.

Two source-scale expression trials were rejected locally. The first painted
source-scale lids and compressed original irises but misclassified some dark
iris/outline pixels as pink hair/skin, leaving a second line. Tightening that
condition in the second trial still left remnants at the patch boundary. Both
are visible in `r3/eyes-attempt.png` and `eyes-attempt2.png`. Following the skill's
two-attempt stop rule, further automatic reconstruction was stopped. The trial
code is isolated in `expression-detail-candidate.cjs`, and setup retains the
previous known-limited expressions instead of silently selecting the failed art.

Alternative needed next: separately author source225 closed/squint eyelids with
explicit original iris/skin/hair ownership (not an enlarged96 difference hull),
then compare only those eye patches to the authored expression at native size
before reinserting into motion. No permission to redesign the face is inferred.

Evidence: `artifacts/high-resolution-motion-20261016-r3/union-line-comparison.png`
shows r2 above / vector-union repair below at gallop0/16/24/40. The redundant
internal line disappears in these views. `r2/eye-leg-diagnostic.png` separates
open/closed/squint and tracking; nearest enlarged crops are diagnostic views,
not a claim that the character should be displayed with nearest sampling.
r2 exported motion frames/video are preserved. Its auxiliary five-case comparison
was regenerated during the first diagnostic; r2's manifest-listed PNGs, rather
than that auxiliary image, remain the immutable comparison source.

New regression checks cover continuity across the former mouth-rectangle edges
and no buried ink in genuinely interior uninked hind/haunch skin over48 gait
phases, including the far hind leg. An initial test incorrectly classified pixels
made opaque by the outline as opaque skin; the corrected oracle uses a separate
uninked geometry mask. This does not count legitimate exterior narrow gaps as
interior skin and is not a blanket claim that every junction is aesthetically
accepted. Prior source/alpha/legacy tests remain separate from visual acceptance.

Final checks:32 pass (master/parts9, reference4, motion12, defect7), including
all609 newly saved PNG hashes/dimensions and unchanged130 legacy product sit
frames. `git diff --check` passes with LF/CRLF warnings only. These results do
not close the expression-quality failure. No replacement full-success video
was delivered while that defect remains.

The r3 manifest explicitly labels partial status and unresolved closed/squint
detail. Runtime integration, installation, package/release and commit/push were
not performed. Eyes are NOT fixed as a complete family. Next is the source-scale
eye-authoring decision above, then user review and the remaining migration.

## Full replacement checklist

### r4 layered occlusion and source-scale expressions

The user rejected the r3 comparison, then explicitly requested preserving leg
forms, hiding only occluded lines, matching original eyes and inspecting every
frame. The r3 “partial repair” assessment above is retracted: the union removed
the far-paw boundary and left a small disconnected mark at gallop16. That output
and the earlier eye attempts remain as failure evidence, not accepted artwork.

Changes in r4:

- `reference-parts.cjs`: keep the rear hind paw out of the foreground union.
  Clip its own outline only with foreground skin; retain the body's overlapping
  edge in front. The far paw's shape is unchanged. `upright.cjs` exposes it1.5
  logical units lower at full gait so its skin does not read as a thin line.
  Phase curves, stride offsets, head pivot and timing are unchanged.
- Frame review found another r3 classification error during sitting: the entire
  hind contour had become haunch-occluded root ink. `body.cjs` now keeps only
  proximal edges in root ink; the distal folded paw stays foreground ink.
- `source-expressions.cjs`: explicit225px eye masks, skin colours sampled from
  the original face and source-scale lid curves referenced to authored96px
  expressions. Squint exposes the actual225px iris under the lid, not an enlarged
  or vertically compressed96px eye. The open face is unchanged. Mouth and
  adjacent skin are copied exactly from the original after the local eye pass.
  This is local authored reconstruction, not recovered missing source detail.
- The first local squint preview let a purple crescent show above the lid. The
  opening mask was aligned to the lid before export. A mouth preservation test
  found a one-level skin colour change at the eye-mask fringe; the explicit
  mouth protection fixed it. Neither intermediate is the delivered candidate.

Direct visual review (separate from automated tests):

- Folder: `artifacts/high-resolution-motion-20261016-r4`.
- All605192px motion/still outputs were viewed on `review-page-01.png` through
  `review-page-26.png`, without skipping intermediate phases. The192px canvas
  contains144px character content and24px padding. Light/dark sheet backgrounds
  alternate. Sit/rise pages were reopened after restoring distal paw ink.
- Coverage: standing3; open/closed walk32each, run48each, tracking25each,
  breathing33each, sit65each, rise65each; closed happy sway66.
- `review-walk-legs.png` and `review-run-legs.png` show all80 open-eye gait paw
  crops at2x. `eye-authority.png`, `eye-grid.png`, final225px expression masters
  and `repair-comparison.png` were inspected against original/authored references.
- Observed: far paws remain separate from foreground body; the gallop16 dot is
  replaced by the correct overlap boundary; no hidden duplicate socket line was
  seen. Sitting retains the folded-paw boundary. No purple residue was seen in
  closed eyes, and squint iris stays below the lid. Open face/rose/ribbon remain
  source-based. Exact texture identity is not claimed for newly authored closed
  lids and the local reconstructed skin beneath them.
- `review-index.json` maps every sheet cell to the actual filename/SHA256.
  `motion-review.mp4` is a4second summary, not all605 frames at playback speed.

File/behaviour validation:35 checks pass (master7, parts2, motion12, defects7,
source expressions3, legacy reference4). They cover source protection, actual
hidden-paw occlusion and exposed-paw presence, every saved PNG hash/dimension,
padding, exact sit/rise reversal, unchanged130 legacy product frames and timing.
An early run started before export completed and failed3 missing-manifest checks;
the final completed export was rechecked successfully. These are not aesthetic
acceptance scores. The earlier blanket no-interior-ink test was replaced because
legitimate foreground-over-rear boundaries must exist; hiding ALL ink is not the
correct oracle.

Skill influence: original-feeling and part ownership checks caught the sit
regression before handoff; default binary-alpha/palette conversion was not applied
to this approved antialiased mascot. Status: visually reviewed candidate,
user acceptance pending. Runtime display at Windows/Mac DPI remains unverified.
No app asset selection, install, package, commit, push or release was performed.
Next: user comparison, remaining families below, then platform integration.

| Family | Current source/consumer | Status / next |
| --- | --- | --- |
| Standing, breathing, blink |225px original;96px authored variants are expression references | r4 source225 lids/iris, shared body/head and33 breathing samples directly reviewed. User acceptance and runtime selection pending. |
| Head tracking / hunt | HuntFrames/HuntRenderer,96px head and256px body bank;384px enlarged head is not new source detail | r4 tracking25 angles each open/closed,−20..+14degrees directly reviewed. Hunt crouch/joins still pending. |
| Walking / snack running | SnackRunFrames native96 raster deformation | Shared high-resolution parts using unchanged gait formulas;32walk/48gallop phases for both eyes ready. Runtime caching/amplitude integration pending. |
| Sit / rise | source-aligned reference-parts curves,96px bank | Both eye states, shared upright endpoint,65samples and exact reverse path ready. Runtime bank integration/user acceptance pending. |
| Jump / landing | hunt body and native landing composition | Rebuild layers with unchanged motion/timing; contact and padding tests pending. |
| Sleep / wake |96px sleep, crouch and tuck source images | Original-part reconstruction approved, not yet implemented. |
| Head/body grab and settle |96px authored keys and113 layered frames | Original-part reconstruction approved, not yet implemented. |
| Edge hanging / paw interaction |100px perch plus runtime deformation | Reconstruct from same head/ribbon and preserve paw timing/contacts; pending. |
| Cheek stretch / recoil | native facial masks and deformation | Transfer masks/pivots to high-resolution coordinates, preserve source face; pending. |
| Eat / happy head sway | mouth and eye overlays, shared head motion | Accepted2.16second closed-eye happy sway migrated with66samples. Eating mouth/drool pending. |
| Icons / platform exports |96px head icon, Windows resources, Mac exported banks | Derive from final shared master and regenerate after motion set verification; pending. |

Snack jewel and UI graphics are not character artwork and do not need replacement
for this request. Keep logical96-unit world coordinates independent of texture
resolution; test alpha hit regions, offsets, padding, contact points and Windows/
Mac output together before selecting the new set. Do not install a mixture of
new standing art and old-motion art. Current app, installer, Mac ZIP and remotes
remain unchanged. Boot check and existing environment/rights items remain open.

## r5 authored closed-eye restoration

The user rejected r4's redrawn closed lids as eyebrow-like and requested only
smoothing of the existing expression. The earlier r4 expression-fidelity
assessment is withdrawn; its technical tests and review history remain above.
The cause was manually authored225px Bezier lids replacing the actual shorter,
shallower96px authored eye strokes, not merely a sampling defect.

`source-expressions.cjs` now routes closed eyes through `expressions.cjs`: actual
pixels from `Assets/dororong-closed-eyes.png`, localized to changed-eye regions,
with bounded cubic interpolation. Both replacement closed-lid curves are removed.
No new eye geometry, generative reconstruction, whole-face blur or sharpening is
used. Open eyes, r4 squint, body/leg shapes and motion timing are unchanged.
The96px source remains a detail limit: interpolation smooths edges but does not
recover missing high-resolution expression detail. Squint remains unaccepted,
not implicitly approved by this closed-eye-only correction.

Evidence: `artifacts/high-resolution-closed-eye-20261016-r5/` contains
`closed-eye-comparison.png` (original / rejected r4 / correction, matched logical
crop), `closed-eye-motion-review.png` (five angles in standing/walking), and
`frames-01.png` through `frames-14.png`. All335 exported closed-eye still/motion
frames were opened and directly inspected on these numbered sheets: standing1,
walk32, gallop48, tracking25, breathing33, sit65, rise65 and happy66. The short,
shallow source eyelids remain visible through these poses, without the rejected
long replacement curve. This is visual review of saved images, not runtime/DPI
validation or user acceptance. Enlarged crops still show the small source's
soft raster edge; no claim of new225px eye detail is made.

`review-index.json` maps sheets to filenames/hashes. Re-exported full proof is
`artifacts/high-resolution-motion-20261016-r5/` (609 PNG entries). Its272 open/
squint files are byte-identical to r4. The source-expression regression now tests
actual authored-pixel transfer instead of incorrectly requiring closed eyes to
ignore that input. All36 targeted/existing tests passed:

```
node tools/HighResolutionArt/motion-proof.cjs
node tools/HighResolutionArt/closed-eye-review.cjs
node tools/HighResolutionArt/closed-eye-review.cjs --frames
node --test tools/HighResolutionArt/*.test.cjs tools/PreviewLocomotion/verify-reference-parts.cjs
```

Skill influence: original/candidate side-by-side shape checks take precedence
over sharper redrawing; the existing antialiased mascot is preserved instead of
applying a new limited-palette pixel style. Current status: corrected preview,
user visual acceptance pending. Next: expression review, then remaining motion
families and platform integration. Existing app/installers/Mac ZIP and original
assets are untouched; no commit, push or release.

## r6 face aperture and bang ownership

The user identified a residual brow-position stroke and collapsed bangs in r5.
The earlier expression-preservation assessment is withdrawn. Its broad convex
hull of changed96px texels copied low-resolution hair across the225px foreground
boundary, while failing to fully clear source225 upper-eye ink. The r5 technical
tests did not distinguish eye/hair ownership and therefore missed these defects.

This repair is isolated to a single-face proof before another full export.
`closed-eye-layer.cjs` uses two manually inspected225px face apertures rather than
a hull of96px differences. The actual authored96px closed-eye texture is sampled
inside them, while original hair remains outside. These polygons are replacement
masks, not new eyelid curves. Eleven explicitly listed source pixels where the
open-eye lash protruded onto pink hair are restored from original hair texture
five pixels to their left. This limited texture repair is not a claim that every
hair pixel is unchanged.

The first aperture comparison still showed left upper-eye ink; the aperture was
corrected against the source pixel grid, and the lash spur on hair was then
isolated. Current saved `candidate-closed-225.png`, `candidate-left-grid.png`,
`mask-comparison.png` and `closed-face-comparison.png` were directly opened.
The final comparison shows authored96 closed eyes / rejected r5 / the new
assembled face. The dangling dark stroke under the center lock and the left
upper-eye spur are removed in this still; the source225 bang lines remain crisp
instead of being replaced by the coarse96px boundary. Source96 eyelid edges
still have their inherent softness; new eye detail was not invented.

Evidence directory: `artifacts/high-resolution-closed-eye-20261016-r6/`.
Four focused checks pass (`node --test tools/HighResolutionArt/closed-eye-layer.test.cjs`):
outside-aperture/repair and alpha invariance; protected pink bangs and selected
central-strand pixels; residual-ink locations; original eyelid sample interiors.
These checks support the named boundaries, not aesthetic acceptance or motion
validation. The final still is a candidate pending user review. No new335-frame
visual-review claim is made; r5 remains rejected. The r6 module is intentionally
not yet selected by the motion exporter or application.

Skill influence: review the enlarged face and original part ownership before
repeating bulk generation; do not treat source-pixel sampling as proof that the
correct semantic part was preserved. Next: face confirmation, integrate the
candidate, then check affected motion frames. App assets, installers, Mac ZIP,
commit/push and releases are unchanged.

## r7 front-bangs redraw preview

The user rejected the remaining ll-shaped/angular residue in r6 and explicitly
requested that the front bangs be redrawn. Imagegen skill was used for an actual
raster redraw rather than another mask repair; its identity-preserving edit
instructions constrained the request, but they do not guarantee pixel invariance.

Built-in imagegen output was directly viewed and copied into
`artifacts/high-resolution-bangs-redraw-20261016-r7/bangs-redraw-preview.png`.
The adjacent README contains the exact prompt, input roles, SHA256 and limits.
Output is1254x1254 with actual transparency. The detached angular brow residue
is not visible under the continuous redrawn fringe. However other linework,
shading and framing were re-expressed despite the local-edit prompt. Thus this
is a generated shape preview, not an approved/production-ready replacement.
No whole-image downscale or merge was performed to disguise those changes.

User acceptance, isolated bangs extraction/alignment, target-size rendering and
affected motion review remain open. App assets, existing candidates, installers,
Mac ZIP, commit/push and releases are unchanged. Next: review the bangs shape,
then isolate and align only that part without replacing the rest of the character.

## r8 isolated bangs and face-boundary composite

User authorized cutting ONLY the bangs and faulty face boundary from r7 into the
existing character, leaving other parts unchanged. No new image generation was
performed. `bangs-composite.cjs` samples the selected r7 donor into the225px r6
assembled base through an explicit local polygon with protected eye/mouth areas.
The original r6 base and r7 donor remain untouched; their hashes and paths are
saved in `verification.json` beside the output.

Initial uniform alignment showed duplicate left-fringe ink. The donor's normalized
left foot39 was aligned to existing35, keeping right foot109 fixed (x scale74/70,
x offset109-109*74/70; y unchanged). The mask then includes both the old and new
fringe boundaries. A second enlarged view exposed a right-edge double line; that
edge was included in the edit region, with an inward2px seam blend limited to
pink hair above y114. The forehead cleanup is fully covered, not blurred. No
whole-character transformation occurs. Generated semi-alpha does not dim the base:
the composite retains the base alpha and blends only patch RGB.

Saved outputs in `artifacts/high-resolution-bangs-composite-20261016-r8/`:

- `closed-composite-225.png`: assembled character with the local repair.
- `bangs-face-patch-225.png`: isolated transparent patch at the same coordinates.
- `patch-mask-225.png`: explicit edit boundary and protected holes.
- `before-after.png`: unchanged base / final composite at144px display size and
  a3x source-pixel head crop. Directly opened after saving.
- `comparison.png`: base / registered donor / composite. Donor is not the output.
- `verification.json`: source hashes, changed bounds and invariance results.

Final changed count4000, bounds x29..115/y64..126. Outside-mask changes0;
alpha changes0. Five focused tests passed: mask/alpha invariance, independent
protected eye/mouth/body/ornament/outer-hair rectangles, transparent patch extent,
saved-PNG reproducibility and dimension rejection. Both existing eyelid textures
and mouth remain exact. This does not claim that the existing eyelids gained new
detail or that every previous art defect is solved.

Visual review of the saved144px view and enlarged head found the previous narrow
angular residue replaced by the selected donor's continuous fringe. The initial
double-edge findings above were corrected before delivery. No full-motion or
Windows/Mac display verification was performed this turn. The result is a local
composite ready for visual review, not runtime-selected. Next: user review, then
connect the patch to the expression pipeline and inspect affected motion frames.
Existing app assets, installers, Mac ZIP, commits and releases remain unchanged.

## r9 seam and eyelid contrast candidate

The user rejected r8: bangs appeared cut and the eyes alone looked blurry. Its
previous visual assessment is superseded, not its outside-mask invariance result.
Pixel inspection found the r8 top mask at y64 crossing dark donor ink at x61–62
and83–85. The initial explanation blaming the rectangular eye protection for
cutting donor hair was too strong: a direct query did not find the hypothesized
dark donor pixels there. This explanation was corrected to the observed upper
seam crossing. Side attachment quality remains a visual judgment, not an invariant.

`bangs-face-finish.cjs` moves the top seam into pink space below the existing
crown, preserves the crown through y65, and protects local eyelid polygons. The
existing low-resolution eyelid texture receives a local neutral-ink contrast
adjustment without spatial resampling or newly drawn curves. This improves edge
contrast but cannot recover missing original detail or guarantee perceived shape
identity. No global sharpen, body edit or new image generation was performed.
An initial canvas destination-out trial erased the mask; explicit pixel-alpha
subtraction replaced it before the saved candidate was checked.

Output: `artifacts/high-resolution-bangs-face-20261016-r9/`. The saved
`comparison.png` shows the full head at source-pixel enlargement; `before-after.png`
shows rejected r8 / r9 at144px on light and dark backgrounds. Both were directly
opened. The washed-gray lid fringe is reduced; source96px eye detail remains
limited. No full-motion visual pass or user acceptance is claimed.

`verification.json`:4171 changed pixels,0 outside hair/eye masks,0 alpha changes,
293 eye-region pixels tonally adjusted, runtimeSelected=false. Eight tests passed
across `bangs-composite.test.cjs` and `bangs-face-finish.test.cjs`: previous default
composite unchanged, local edit bounds, crown/mouth/body/ribbon/outer-hair byte
identity and saved-PNG reproducibility. These checks do not establish aesthetics.
Next: visual review of this candidate before expression/motion integration.
App assets, installers, Mac ZIP, commit/push and releases remain untouched.

## r10 connected eye-adjacent fringe and rounded lids

The user rejected r9's eye-adjacent bangs and angular lids. Its visual assessment
is withdrawn; the earlier invariance results remain historical technical facts.
The imagegen skill was used to repair continuous hair junctions and rounded short
closed-eye strokes together, without reusing r9's contrast-threshold sharpening.
The generated whole character was NOT substituted into the product.

`tools/HighResolutionArt/eye-fringe-repair.cjs` registers this new donor and copies
only a continuous fringe/upper-face mask into the existing r9 character. Initial
two-patch trials showed lid displacement and doubled hair ink; registration and
a connected patch replaced those trials before review. Only the upper pink seam
is blended. No spatial blur or threshold contrast pass is applied to the eyes.

Artifacts, exact prompt, input roles and limitations:
`artifacts/high-resolution-eye-fringe-20261016-r10/README.md`.
`comparison.png` (r9/r10,144px and3x full-head detail) and
`candidate-light-dark.png` were directly inspected. The corner joins now have
continuous outlines in the saved view and the eyelids are shallow curves rather
than stepped dark blocks. This is a candidate, not user acceptance or a complete
motion-frame validation. The local generated line shape is not pixel-identical
to the original96px expression, and source-style fidelity still needs review.

Saved candidate:4448 changed pixels,0 outside-mask changes,0 alpha changes.
Two focused tests pass: edit confinement/alpha and independent crown, outer-hair,
ribbon, mouth, body rectangles. Next: user visual review before motion integration.
Runtime, existing assets, install packages, Mac ZIP, commits and releases unchanged.

## r11 original-based face reset

The user rejected r10 against the original: central fringe, eye-adjacent hair,
closed-eye shape and cheek appearance drifted. The previous invariance tests used
an already altered base; they proved locality, not original identity. Original
side-by-side inspection confirmed this, superseding r10's visual assessment.
The user explicitly authorized restarting from the original. No new generation
was used. Rejected outputs remain for evidence, not as inputs.

`original-closed-repair.cjs` starts from the accepted source-derived225 master,
SHA256 `1d84aa57e2824add4450427d56b0d45eb729b5eedbe9f0fcc7a6fab9f766483c`, and authored
96px closed expression, SHA256
`0d20eed5873a7e4277d6ed539474d875c9b8df79998b1efd74f46aa87662f488`.
It replaces only eye apertures and the open-eye lash spur, preserving surrounding
original hair and pink cheek texture. An initial broad spur cleanup crossed the
original fringe edge; it was narrowed to explicit per-row spur coordinates.

`source-lid-contours.cjs` extracts the original raster's50% neutral-ink coverage
boundary with marching squares. One corner-rounding pass removes source-pixel
corners; it does not draw an invented centreline or sharpen the blurred raster.
The right eye extraction excludes adjacent hair at x44 and x42+ below y57.
There are two contours: left x17.03–25.63/y51.16–54.15, right
x34.20–43.88/y55.02–57.15 in original96 coordinates. They retain the source's
short shallow expression, but this contour interpretation is not pixel identity
or recovery of nonexistent high-resolution eye detail.

The first protected-region test caught three changed surrounding-mouth pixels.
The mouth rectangle was explicitly excluded, then all four tests passed:
edit-mask/alpha confinement; original cheek exteriors, blush cores, mouth, crown,
side locks, ribbon and body rectangles; two source-bound eyelid contours; saved
PNG reproduction from the original inputs. Final:1288 changed pixels,0 outside
the eye/spur mask,0 alpha changes. Full source hashes and result are in
`artifacts/high-resolution-original-face-20261016-r11/verification.json`.

Directly inspected `comparison.png`: original225 hair/cheeks, original96 closed
expression and r11 at144px plus enlarged source-pixel head views. Also inspected
`candidate-light-dark.png`. Original central fringe and cheek silhouette now come
from the source rather than the generated composite. This is an isolated original
master proof, not the earlier reassembled body/posture candidate. No posture or
motion exporter selects it. User visual acceptance and affected-motion review
remain open; tests do not replace either. App, installer, Mac ZIP, commit/push and
release files are unchanged. Next: original-based face review before integration.

## r12 transparent eyelids only

User identified r11's copied surroundings and white rectangular patches. Its
four passing tests proved narrow scope, not absence of patches INSIDE that scope.
The r11 code copied96px RGB across the eye apertures and then whitened rectangular
lid regions. Both paths are removed in the new `eye-only-composite.cjs` candidate.

The source225 open-eye holes are now repaired independently using inverse-distance
interpolation of fifteen explicitly listed original skin-color samples. Existing
pink cheek/hair pixels, outer face, mouth and alpha remain protected. This is a
reconstructed clean plate, not a claim that hidden skin under the open eyes was
recovered exactly. The same small original lash-spur repair remains explicit.

The authored96px expression supplies ONLY the two previously measured eyelid
contours. They are rendered on a transparent225px canvas and composited over the
clean plate. No source96 skin, white backdrop, hair or blush is imported. No
post-composite rectangular whitening occurs. Saved layers:
`artifacts/high-resolution-eye-only-20261016-r12/clean-face-225.png`,
`eyes-only-225.png`, `candidate-225.png`, and `layers.png` (checkerboard evidence).
Directly inspected layers and original225/original96/candidate `comparison.png`;
the previous white rectangular surroundings are no longer visible in these views.

Four focused tests pass: composition changes0 pixels outside eyelid alpha;
transparent lid layer confined to two tight ink bounds; clean face independent
of expression input, with outside-eye source repaint not affecting the composite;
original protected anatomy/alpha and saved-PNG reproduction. The visible-ink
color test excludes alpha<=16 from RGB-only bounds because unpremultiplication
rounds nearly transparent RGB; extent/alpha checks still apply to those pixels.
Final:1231 changed source225 pixels,0 outside edit mask,0 alpha changes,
0 non-eye compositing changes. These are engineering checks, not user acceptance.
Next: source/layer visual review before motion integration. No runtime, installer,
Mac ZIP, commit/push or release changes. Earlier candidates remain rejected.

## r13 source-anatomy lock and direct original audit

User required original bangs and cheeks be locked before erasing open eyes and
adding transparent closed lids. A preceding direct r12 audit confirmed its clean
plate changed surrounding pixels; the old0 non-eye compositing claim only compared
an already edited clean plate to the final. It was not original-preservation proof.

`source-anatomy-lock.cjs` now defines source225 fringe boundaries and the inner
side-hair/face rim independently of the eraser. Cheek pigment outside actual new
lid ink is additionally protected. `locked-eyes.cjs` starts from original RGBA and
allows edits only inside explicit open-eye support, excluding these protected
regions. The known open-eye lash spur is an explicit eye-only exception. No new
generated asset is used; the existing transparent source-derived lid layer is kept.
Uncovered eye skin is still reconstructed from original225 color samples, not
claimed as recovered original hidden detail or copied96px skin.

The first conservative fringe lock also retained some upper open-eye ink. Direct
viewing identified it as brow-like residue. Source-pink extents and original ink
junctions were inspected to separate the hair boundary from the removable lash.
This anatomical interpretation is explicit and reviewable; pixel invariance does
not by itself prove the semantic boundaries perfect or constitute user acceptance.

Final saved original-to-candidate comparison:1020 changed pixels,0 changes in the
defined protection mask,0 outside the allowed eye edit mask,0 alpha changes.
Original master hash remains
`1d84aa57e2824add4450427d56b0d45eb729b5eedbe9f0fcc7a6fab9f766483c`.
Four tests pass: direct original/final invariance, independent old-failure
coordinates and anatomical rectangles, original cheek pigments outside eyelid
ink, and PNG reproducibility. Explicit points include central fringe(70,123),
side-hair edge(105,124), and cheek(96,142); these no longer change as in r12.

Saved evidence: `artifacts/high-resolution-locked-eyes-20261016-r13/` contains
`candidate-225.png`, `protected-mask.png`, `eye-edit-mask.png`, `eyes-only.png`,
`difference.png`, `comparison.png`, `candidate-light-dark.png`, and
`verification.json`. Directly viewed original/final/magenta-difference at source
pixel enlargement, plus144px light/dark output. No full-motion review this turn.
Next: user original-based face review before motion integration. Runtime, installed
assets, packages, Mac ZIP, commit/push and release state remain unchanged.

## r14 current face on previous corrected body/legs

User requested the latest face combined with the earlier corrected body and legs,
then a direct check that bangs/cheeks did not change in assembly. The r13 proof
had intentionally used the original body, not the corrected body rig; that proof
was not a body rollback or a new body approval.

`face-body-assembly-review.cjs` uses the saved r6 raw source and corrected assembly
as body references. It copies the r13 face into the original semantic head area
left of x120, preserving the previous ribbon/source corrections, then runs the
existing posture assembly without geometry/pose changes. The old input regenerated
through today's assembly equals the saved old assembly byte-for-byte, confirming
the body being reused rather than assuming its provenance from a filename.

Saved old body assembly SHA256:
`66d68868bdd1d85c1cb6e534e156b257e5cc1db59ee8d307b095abcbb1743631`.
r13 face SHA256:
`8c4fc6c9d2a8d18cfc6d0a90f74e1f0953ff8c6e414b897c80c66c66061ccd42`.
Full source identities and result are saved in
`artifacts/high-resolution-face-body-20261016-r14/verification.json`.

Results:1373 changed pixels versus the old assembly, all inside the face region;
0 outside-face changes;0 opaque-face differences versus r13;0 regenerated-old-body
differences. Four focused tests pass, including an additional final-vs-original225
comparison for protected opaque fringe/cheek pixels (not only final-vs-r13).
Transparent head edges naturally composite over the body; they are not claimed as
full-frame pixel identity with the standalone original-body face proof.

Directly inspected saved `inspection.png`: r13/combined enlarged face, and old/new
body and neck seam. Also inspected `combined-light-dark.png` at144px. No new gap
was visible at the static chin/body seam, and the corrected legs match the prior
saved assembly. User acceptance and moving/rotated seam behavior are not inferred
from this still check. Saved `candidate-225.png`, `assembly-source-225.png` and
`head-layer-225.png` retain the actual output/inputs, with `comparison.png` optional.

Next: assembled still review, then affected-motion integration and frame inspection.
No full animation export, runtime asset replacement, installer, Mac ZIP, commit/push
or release change was performed.

## r15 cheek eye-residue correction

User reported black cheek stains in r14. Direct original225/r14 coordinate crops
showed residual lower iris/eyelid pixels, including purple samples at (48,128)
and (92,138), and dark samples at (43,126), (48,131), (89,138). They match the
open-eye source exactly and have no overlap with the new transparent closed lids.
The earlier pink-color protection confused red-biased iris/edge colors with blush;
some retained pixels also crossed the manually interpreted eye/cheek boundary.
Thus source identity alone had preserved a defect. r14's assembly invariance is
still valid, but its face is not a clean-cheek acceptance baseline.

`cheek-residue-review.cjs` lists37 individually reviewed residual pixels. Local
four-neighbor skin interpolation fills only those points, using the immutable
surrounding colors; it does not blur/resample the face or import closed96 skin.
The cleaned source is passed through the existing semantic head/body assembly.
The exact point list, input/output hashes and counts are in
`artifacts/high-resolution-cheek-20261016-r15/verification.json`.
Previous r13/r14 files and historical tests are unchanged; these37 explicit points
supersede their cheek protection only for the reported residual eye defect.

Results:37 changed pixels,0 outside-mask changes,0 alpha changes. Five focused
tests pass: source provenance/no new-lid overlap, whole-frame preservation outside
the points, protected anatomical regions, consistent source/head/master repair,
and saved PNG reproducibility. This is an existing antialiased illustration repair,
not palette quantization, a pixel-art redesign or a new image-generation pass.

Directly opened saved `comparison.png` (before/after face at5x) and `preview.png`
(144px light/dark). The isolated purple/dark marks below both closed eyes are no
longer visible; existing pink blush texture remains. Bangs, closed-eye shapes,
mouth, cheek exterior, ribbon and prior corrected legs are unchanged. User visual
acceptance and animated/rotated rendering remain unverified. Technical identity
does not establish acceptance of all earlier artwork.

Next: r15 still review, then affected-motion integration and frame inspection.
No runtime asset, installer, Mac ZIP, commit/push or release changes.

## r16 accepted face connected to shared motion

User accepted r15 and requested motion connection while retaining bangs, cheeks,
corrected body/legs, with direct frame-by-frame review. `accepted-motion.cjs`
loads the r15 semantic source, rebuilds the shared posture assembly and asserts
exact equality with the saved accepted225px master. Open expression restores only
the original225 face within the existing face ownership; both expressions use
the same corrected body/ribbon source. No generated donor,96px skin patch or
legacy closed-eye transfer enters this path. The default `motion-proof.cjs` CLI
now calls this path; `--legacy-r5` and its historical setup API remain explicit
regression tools, not the current visual proof.

`upright.cjs` adds an opt-out for eye displacement. Closed-face tracking/happy
motion rotates the accepted head without locally warping bangs/cheeks. Initial
testing found that ignored nonzero gaze inputs still bypassed the neutral fast
path and changed composite sampling. They are now normalized to zero before
neutral-path selection. Default open-eye behavior, original gait formulas,
body geometry, sampling and2.16second happy timing were not changed.

Output: `artifacts/high-resolution-motion-20261016-r16/`.
`manifest.json` pins actual input/code hashes and604 output PNGs with parameters:
two standing frames; per expression32 walk,48 run,25 tracking,33 breathing,
65 sitting,65 rising; plus66 closed happy frames. Output is192px with144px
content and24px origin padding. Source masters are225px. Rise reverses the same
authored sitting path; this does not introduce new motion timing.

Direct visual review completed for every numbered sheet:

- `review-01.png` through `review-26.png`: all604 complete frames, light/dark
  backgrounds, in file order without sampling omissions.
- `faces-01.png` through `faces-14.png`: all335 closed-face frames at2x output
  detail, in addition to full-frame views. The fixed detail crop is not a full
  head view at every angle; full sheets supply the complementary silhouette.
- `legs-walk.png`, `legs-run.png`: all80 open gait hind-leg crops at2x.

Observed: no recurrence of the isolated dark/purple cheek stains, no additional
ribbon, no new exposed overlapping hidden-leg stroke, no visible new head/body
gap or silhouette clipping in these outputs. Bangs and cheek exterior keep the
accepted source form through rotation; original blush texture is retained.
Rotation naturally resamples output pixels, so the claim is preserved source
artwork/shape, not byte-identical screen pixels at different angles. The shared
body retains the earlier folded hind contour and seated distal boundary. This
does not establish acceptance of unexported motion families or all future angles.

Checks:10 current accepted-motion/cheek tests,7 shared-renderer defect checks,
and3 targeted legacy checks (Windows gait formulas, all130 existing96px sit
frames, body-only breathing) pass. Current checks include source/head identity,
body equality between expressions, closed-gaze invariance, rest/happy endpoints,
first-motion continuity, all604 PNG hashes/decode/padding/socket probes, full
review index coverage and exact sit/rise reversal. This is not a full application
test-suite run. Historical r5 manifest tests were not refreshed to fake current
source identity; their saved artifacts remain historical evidence.

`motion-review.mp4` is a6second text-free six-panel convenience preview encoded
from180 composite frames with existing local ffmpeg; a full decode check passed.
The lossless PNGs, not the compressed video, are the artwork evidence.
Top row: closed tracking / open walk / open gallop. Bottom row: closed sit-rise /
closed breathing / closed happy sway.

Next: user motion review, remaining squint/hunt/jump/sleep/grab/perch/cheek/eating
migration, then Windows/Mac texture/origin/hit-mask integration. Squint was not
silently inherited from rejected candidates. No runtime bank, installer, running
app, Mac ZIP, commit/push or release was changed. User acceptance of r15 is not
automatic acceptance of r16 or completion of the full artwork migration.

## r17 seated bump and reaching gallop — 2026-10-17

User identified a protrusion below the seated hindquarter and asked for forward
reaching legs instead of shortening, with a faster run. This explicitly supersedes
timing/path preservation only for the requested gallop and seated contour repair.
r16's earlier no-new-defect observation missed the seated protrusion and did not
establish user acceptance. The input screenshots remain user evidence.

Diagnosis: the two-bone near-hind blend puts its lowest knee-envelope points near
(57.55,88.18) logical coordinates at full sit, below the adjoining seated curve.
`sit-diagnosis.png` separates that purple leg from the orange haunch. Legacy run
uses tip-weighted vertical lift, reducing exposed leg height; additionally the
r16 video played48 samples at30fps, i.e. a1.6second cycle. That video cadence was
not a measurement of installed-app speed (the app uses distance-driven phase).

`motion-polish.cjs` now smooths the seated distal vector envelope as the haunch
drops; no pixel eraser, face change or whole-body scaling is used. Fill/ink follow
the same corrected points. `reference-parts.cjs` treats an extra part with an
existing name as a replacement at its original depth, not a duplicate layer.
The unchanged default96 renderer still matches all130 saved product sit frames.

Run: exposed leg contours rotate around hinges inside the body overlap, reaching
50degrees forward and18degrees back. Length and width below the hinge are rigid;
only hidden attachment skin blends into the stationary torso. A first local
sample rotating around the old high roots still hid too much leg, so the hinge
was moved into the visible attachment overlap before the final sequence export.
Body bob is continuous1.6 logical units rather than rounded2.4units. Foot phases
remain staggered. `.56`second cadence is explicit in the new module and used by
the60fps review video, approximately2.86times the previous preview cadence.
It is not yet a deployed change to world movement speed or app phase policy.

Opt-in `smoothSit`/`reachRun` preserve the historical default paths and the accepted
r15 rest master. `accepted-motion.setup({polished:true})` selects these corrections.
Default motion-proof CLI now exports r17; `--r16` retains the earlier entry.
Output: `artifacts/high-resolution-motion-20261017-r17/` with source/frame hashes
in `manifest.json`, source comparison in `sample.png`, and6second text-free
`motion-review.mp4` (left sit/rise, right run).

Direct review: `review-01.png`–`review-15.png` include every356 affected frame
(48run+65sit+65rise for each expression). `detail-sit-1.png`–`detail-sit-3.png`
cover all65 seated hind contours; `detail-run-1.png`–`detail-run-2.png` cover all48
run leg sets at2x. The pointed seated bump is removed with a continuous bottom
curve; paws visibly reach forward and return without the old tip-pull shrink.
No additional cheek stain, displaced bangs, duplicated ribbon or exposed socket
gap was observed in these outputs. Small source texture and normal angle-dependent
occlusion remain; this does not mean identical screen silhouettes at every angle.

Six focused checks pass: exposed length/width invariance across phases/amplitudes,
forward reach/loop/cadence, seated envelope bound/rest preservation, accepted face
and master identity, all64 walk frames plus six tracking extrema/midpoints exactly
matching r16, and all356 saved frames/source hashes/decode/padding/review coverage/
sit-rise reversal. The separate default96 regression passes all130 original sit
frames. No full application suite was claimed. The MP4 was fully decode-checked.

Next: r17 visual acceptance, remaining expression/motion migration, then runtime
integration. No Windows/Mac runtime assets, installation, release or commit/push
were changed. Existing unrelated startup/Mac edits were preserved.

## r18 fixed rump and rigid folded hind paw — 2026-10-17

User rejected the r17 running rump/paw bump (clipboard9d213340) and explicitly
authorized fixing the attachment while retaining leg length/folded shape and
leaving the face untouched. The earlier r17 bump-free assessment is withdrawn.
Its length test sampled only the rigid distal section; it did not establish a
clean combined socket silhouette. The root weight and the exposed rounded lower
haunch could produce an extra lobe when combined with the swinging paw.

A first r18 trial rotated the whole hind part rigidly and clipped proximal skin
against the torso. Direct enlarged review still showed a step where the old
haunch bottom met the paw, so that trial was not delivered as a fix. The final
candidate retains the upper haunch cubic exactly to t=.5 (74.25,65.4375 in logical
coordinates), then uses one tangent-matched connector to the rigid folded paw.
The obsolete exposed lower-haunch lobe is no longer unioned alongside that paw.
This changes only the proximal connection; original distal cubic controls from
source(163.7,175.4) around the toe to source(138.1,173.1) are rigidly transformed,
not shortened, scaled or repainted. Far-hind proximal skin is torso-clipped.
The common renderer supports an optional vector skin shape with consistent fill,
outline and occlusion. Legacy paths still pass their saved-output regression.

Opt-in: `accepted-motion.setup({polished:true,fixedRunRoots:true})`.
Default motion-proof CLI exports r18; `--r17` and `--r16` preserve historical
entry points. Saved r17 images/manifests were not overwritten. Source hashes in
historical manifests describe their original source state, not the new code.

Output: `artifacts/high-resolution-motion-20261017-r18/`:
-168 PNGs:48 full-run phases plus12 phases at each .25/.5/.75 amplitude, for
 both open/closed expressions. This is a sampled preview, not every continuous
 runtime phase or an app startup/stop transition test.
-`review-01.png`–`review-14.png`: all168 frames directly opened and inspected in
 sequence, with2x lower-body crops and whole-character thumbnails on light/dark
 backgrounds. The reported outer socket bump/step was not observed in the final
 saved set; ordinary paw overlap and the retained inward fold remain visible.
-`comparison.png`: r17 top, r18 bottom, same phases0/12/24/36 at output size.
-`motion-review.mp4`: four seconds,60fps,.56second cycle, open/closed side by side,
 no captions; full decode check passed. This is preview cadence, not app speed.

Four new tests passed: original distal control points and rigid distances over
96 phases/five amplitudes; fixed rump subdivision/connector tangent; source head
identity and all168 rendered face/fringe/cheek rectangles equal to previous
rendering, all64 walking frames unchanged and all130 r17 sitting frames unchanged;
and saved PNG hashes/source hashes/decode/dimensions/padding/review coverage.
The initial whole-upper-row face assertion also included the right rump and
failed on changed body pixels (x134–137 in the first frame). The corrected test
checks the actual face/fringe/cheek rectangle x<125,y<125 plus full source-head
identity; it does not label rump changes as face preservation failures.
Six relevant existing tests passed, including saved r16 walking/tracking and all
130 legacy96 sitting frames. No full application test suite is claimed.

No face assets, sitting timing, front-leg motion, Windows/Mac runtime, installed
app, installer, Mac ZIP, commit/push or release were changed in this correction.
Remaining: user visual acceptance, runtime transition/integration checks and the
already planned other expression/motion families. A passing geometry/pixel test
does not replace the user's visual acceptance.

## r19 shared motion and transition preview — 2026-10-17

The user accepted r18 and requested the remaining motions use the same face and
folded leg shape, including direct inspection of transitions for recurring bumps.
This resolves r18's acceptance wait. It does not accept new r19 poses in advance.

`connected-families.cjs` uses the r15 original-source face layers and r18 shared
fixed-rump/tangent connector. `upright.cjs` and `posture.cjs` opt into this body at
rest as well as in motion, avoiding the previous switch back to an older rest
body. `motion-polish.cjs` exposes reusable rigid-paw and clipped far-paw helpers.
The distal folded contour rotates/translates; it is not scaled or shortened.
Walking/running interpolate transforms and continuous body bob rather than
cross-fading two silhouettes. Existing run cadence remains .56seconds in preview.

Hunt uses the existing hunt timeline and front-body movement, with a rigid folded
rear paw and unchanged source head. Jump uses the existing .18second preparation,
.48second flight and .20second landing; recovery reaches neutral before landing
instead of inserting another front-paw reach. The preview includes illustrative
world lift, not the app's target/floor/collision controller. Eating ports the
existing mouth-open and local chew motion; its1.7second duration, .32second settle
and2.16second happy sway are retained. The rest of the face is unchanged.
Sleep/wake reconstruct curled legs from source parts, retaining the .36second
body tuck and .18second wake duration. An initial trial exposed rotated leg roots
below the body. Contact translations were corrected before the saved review;
no leg was shortened to hide the error. The earlier low-resolution sleep pixels
are a pose reference, not the final source or a pixel-identity guarantee.

During final inspection, an endpoint test found that the far hind paw could
return to the walk-only branch at exactly100% run blend. All blend weights now
use the same rigid transform and proximal clip, including the endpoint. Across
48phases the full blend equals the run renderer exactly; the near-endpoint test
guards against a discontinuous paw replacement. The48 accepted r18 full-run
images remain pixel-identical after this fix.

Output: `artifacts/high-resolution-connected-20261017-r19/`.
`motion-proof.cjs --full` is the current exporter; no `--full` writes only the
sample. Historical `--r18`, `--r17`, `--r16`, `--legacy-r5` remain explicit.
The837 PNGs cover nine60fps sequences: idle/run55, walk/run73, tracking/walk49,
sit/rise103, hunt/recovery157, jump/landing53, eat/happy252, sleep/wake46 and
breathing49. `review-index.json` maps each frame exactly once to35 numbered
sheets. Each slot includes a1.75x lower-body/face crop and a whole-character
thumbnail, alternating light/dark backgrounds. All saved samples were directly
opened for visual inspection; no repeated external socket lobe was observed.
Ordinary overlapping paws and the original inward fold are retained, not erased.
The small far front paw in hunt is partially hidden by the hair, not detached:
all157 hunt images have one substantial8-connected alpha component at128/255
(components smaller than4pixels excluded). This connectivity check supplements,
not replaces, direct visual inspection. No claim covers every continuous input,
arbitrary interruption, flipped view or actual runtime state transition.

Six focused tests passed, covering neutral pixel-identical joins, accepted48-frame run
identity, unchanged closed-face source/local-mouth-only changes, rigid distal
control distances, gait-blend endpoints and saved hashes/decode/padding/review
coverage. The separate legacy96 sit regression covers130 original frames.
No full app suite or deployed rendering test is claimed. The five-second,
eight-panel `motion-review.mp4` is a text-free preview, not an app capture.
Full MP4 decode check passed. After the endpoint correction, walk/run and
tracking/walk sequences were re-exported and sheets03–08 were directly inspected
again. The other seven sequences' ordered PNG-hash digests were unchanged from
the already inspected set; no stale earlier sheet is used as evidence for a
changed frame. The complete coverage is837/837 sampled frames, not a claim of
arbitrary runtime-state coverage.

Remaining, explicitly **not connected here**: squint/blink transitions;
head/body grabbing and release/swing; edge perch; cheek/paw pull/flutter;
body-click and strong landing; snack jewel/drool overlays; app hit masks, origin,
resolution selection, Windows/Mac rendering/state integration. Sleep pose fidelity
and all new r19 visuals remain subject to user review. This is a partial migration,
not a replacement of the installed product. No runtime assets, installed app,
installer, Mac ZIP, release, commit or push were changed. Unrelated startup/Mac
working-tree changes remain intact.

## r20 original-reference sleep pads and fold — 2026-10-17

User rejected the r19 sleep pose (clipboard bbc40ca2), then explicitly authorized
repair against the original sleep, with visible forepads/folded rear leg and no
face or running changes. The screenshot came from sleep/wake, not the separate
sit/rise renderer. r19's frame review did not establish correct original posture;
that assessment is withdrawn for sleep. Original authority is the existing
`src/Dororong.App/Assets/dororong-sleep.png`, viewed alongside the saved candidate.
It shows two rounded resting pads below the chin, a low belly and a retained
inner rear fold. The original96 face is not transplanted: the approved225 face
remains the protected asset, including its existing head translation.

Causes: r19 rotated the long leg parts up into the standing torso, retained a
standing underside instead of a settled belly, and rendered with `pose(0)` whose
union-outline branch deliberately skipped internal limb ink. Thus rigid-control
distance preservation and an unbroken silhouette did not prove readable paws.

`sleep-pose.cjs` now owns only sleep/wake body geometry. Forelimbs rotate/translate
without length scaling into the two resting-pad positions; the rear distal source
curve remains rigid. The belly's vector controls settle to a low original-guided
profile instead of leaving a standing-height wedge. This is authored body-shape
deformation plus rigid limbs, not a crossfade between raster images. The source
rear inner curve is subdivided to show its exposed folded portion, and the near
forepad includes its laid-down outer edge. The common renderer gains opt-in
interior-part ink/occlusion/opacity controls; their defaults leave other modes
unchanged. The head is composed on top from the same unmodified source.

Failed local trials: simply lowering the feet exposed long forearm roots and
allowed the near forearm to hide the rear crease. Changing angle alone produced
standing-looking toes and an empty diagonal belly. The settled torso and explicit
pad/fold ink resolve those distinct causes. First transition sheets then exposed
ghost-like inner lines too early and a forearm stroke tip while it rotated past
the fringe. Pad/fold ink is now revealed only during settling, and the forearm
edge grows from its existing endpoint after clearing the hair. No output-frame
erasing, rectangular skin patch, face redraw or running change was used.

Saved output: `artifacts/high-resolution-sleep-20261017-r20/`.
`comparison.png` shows original / rejected r19 / current at enlarged and actual
144px display sizes. `sleep-final.png` is the transparent final still.
`sleep-000..064` and `wake-000..064` are130 uniform normalized pose samples,
reviewed directly on `review-01..09.png` in both light/dark backgrounds. The
final44/64 mid-pose was additionally enlarged to inspect the suspected isolated
stroke; the saved output has no such detached dot. The observed result retains
separate front pads and a rear fold; no additional exposed socket bump or broken
body contour was observed in these samples. These are sampled pose checks, not
every continuous phase, interruption or actual app interaction.

Five tests passed (four focused plus one relevant legacy regression):
- The225 closed-head asset is identical. At all46 saved r19 sleep/wake phases,
  every fully covered rendered head pixel matches r19 exactly, including bangs,
  cheeks and face. The initial test sampled only the pixel center, incorrectly
  treating an antialiased ribbon-edge pixel as fully opaque and reporting alpha
  247→248. The corrected mask checks all four actual coverage samples. Translucent
  boundary pixels can composite against changed body coverage; they are not
  claimed to be fully invariant. No face texels or head transform changed.
- All96 accepted r18 run frames (48 per expression) remain pixel-identical.
- Distal rear-control distances and forelimb rigid lengths remain invariant over
  65poses; neutral sleep/wake joins match closed idle exactly. Saved-image checks
  also require ink at the forepad division and rear-fold locations. Ink presence
  is not an independent proof of visual quality.
- All130 PNGs match the current selected source hashes, decode at192×192, retain
  transparent margins and occur exactly once in the review index. Wake is the
  exact reverse of the same authored body path.
- The default96 sit renderer still reproduces all130 original product frames.

The text-free five-second MP4 uses the existing .36second tuck and .18second wake
with holds for viewing, at60fps; full decode check passed. Eye opening/squint is
still outside this body-only correction. `motion-proof.cjs --full` now writes
the r20 sleep proof. Older r19 artifacts/manifests remain unchanged historical
evidence; their old source-hash test is not a current-source pass claim.

No installed app, production assets, installer, Mac ZIP, release, commit or push
changed. Other pending migration/interaction work remains in the r19 boundary
list. Next: user visual review of this corrected posture, then remaining direct
interaction migration and platform integration; overall migration is incomplete.

## r21 rounded sleep pads and continuous ink — 2026-10-17

The subsequent read-only audit of all130 r20 pose samples found: sleep036–047
forepad ink fading in, sleep050–060 rear ink fading in, and sleep056–064 longer
forepads/low rear lobe compared with the original96 sleep. Wake showed the reverse.
This supersedes r20's visual pass for those properties. The user authorized
fixing exactly these problems while preserving the approved face and running.

Original observations fixed before correction: two short rounded pads meet below
the chin; the near pad turns up into the belly instead of forming one long bar;
the rear fold meets the underside and does not hang below it. The original96
sleep is the posture reference, not a replacement face. The same approved225
head and the existing head translation remain unchanged.

Changes in `sleep-pose.cjs`: move the chest behind the pads; shorten/round the
near forearm's hidden upper contour and author the far pad's resting projection;
keep fill/ink on shared paths. The rear bottom control envelope settles onto a
curved resting underside instead of retaining a dangling standing toe. This is
an intentional sleep-only contour articulation in addition to joint rotation,
not rigid preservation of every resting control distance and not raster morphing.
The rear crease uses a fixed source-curve subdivision at every pose; its terminal
stroke reaches the underside and is clipped to the union skin. All interior ink
is fully opaque and revealed by part/head occlusion, with no opacity ramp or
late-growing forearm stroke. `reference-parts.cjs` adds opt-in skin clipping;
defaults for other renderers are unchanged. Neutral endpoints use the exact
existing closed-idle body path, avoiding antialias-only endpoint differences.

Local trials exposed: lifting the entire rigid rear paw removed the lobe but
left a floating crease; the first rounded far pad left a vertical slot between
paws; a flat rear control cap made a squared corner. These were corrected by
the continuous skin-clipped crease, overlapping rounded pad junction, and curved
rear resting envelope. The early neutral pixel regression failed (different
curve rasterization/ink), then passed after restoring the exact neutral path.
The old rigid-resting-control assertion was replaced explicitly because it
contradicted the authorized resting-shape correction; unchanged running is still
checked against independent accepted r18 PNGs, not against the new generator.

Final output: `artifacts/high-resolution-sleep-20261017-r21/`.
All130 final saved PNGs were directly viewed in `review-01..09.png` after the last
geometry change. These are65 uniform normalized poses in each direction, not130
consecutive video frames. The saved intermediate sleep044 and final still were
also enlarged for local inspection; the final comparison was checked at enlarged
and144px display sizes. No late opacity appearance, separated rear crease,
vertical forepad gap or additional rear lobe was observed in the final samples.
The posture is reference-guided rather than pixel-identical to the old96 image;
the protected225 face/placement is intentionally retained.

| Final review | Observed result |
| --- | --- |
| sleep000–015 / wake049–064 | Standing join and initial tuck; face held; no new socket protrusion observed. |
| sleep016–031 / wake033–048 | Rounded pads begin folding; no new disconnected exterior stroke observed. |
| sleep032–047 / wake017–032 | Forepad boundary travels with the foot; rear crease remains connected to the underside rather than fading in. |
| sleep048–063 / wake001–016 | Pads settle together without the slot; rear underside rounds into the belly without a low lobe. |
| sleep064 / wake000 | Two rounded short pads and visible rear fold, compared directly with original and r20. |

Six checks passed: five in `sleep-pose.test.cjs`, plus the `default96` regression
in `motion.test.cjs`. They cover exact approved head source and fully opaque
rendered head pixels at46 reference phases; all96 open/closed r18 run frames
pixel-identical; neutral endpoint identity and resting silhouette contact/ink;
constant opaque skin-clipped stroke settings across64 nonneutral poses; all130
saved hashes/dimensions/margins/review slots and exact reversed wake; and all130
legacy96 sitting PNGs unchanged. Translucent head-edge compositing over changed
body coverage is not claimed pixel-identical. Numerical checks do not substitute
for the visual review or user acceptance.

The text-free `motion-review.mp4` is five seconds/60fps, using the same reviewed
pose samples with the existing .36s tuck/.18s wake timing. Full video decode
passed. No actual app playback/interruption or platform integration was tested.
No production artwork, installed app, installer, Mac ZIP, release, commit or push
changed. Next: user review of the r21 pose, then the already scoped remaining
direct-interaction migration/platform work; overall replacement remains incomplete.

## r22 source-contour still before transitions — 2026-10-17

Direct comparison after r21 revealed remaining source-shape deviations: the pads
were still thick and downward-hanging, their junction with the belly made a step,
and the rear fold read like a short slash on a flat belly. The previous shape-pass
assessment is withdrawn; absence of a lobe or opacity ramp did not establish
original posture fidelity. User authorized a static source-matched body/feet
first, without face/run edits or transition connection.

`sleep-reference-still.cjs` is a separate, unconnected code-native vector study.
The original96 sleep was inspected enlarged with a5px coordinate grid. Observed
landmarks: far pad around x11–28/y74–82; near pad x28–50/y74–82; rear fold begins
near64/72 and curves to60/79; the rear exterior rounds into the underside near
y80 rather than below the front pads. These source observations, not the rejected
rig's controls, define the body outline and two interior curves. Hidden body skin
supports the unchanged225 head at the same +8 logical y translation. The first
static draft exposed body ink alongside the left hair; its hidden upper-left
body attachment was moved behind the hair without modifying head texels.

Final saved output is `artifacts/high-resolution-sleep-still-20261017-r22/`:
`comparison.png` (original left / static candidate right, enlarged and144px),
`candidate.png` (transparent192px), `body-225.png`, `actual-size-dark.png`, and
`manifest.json` with source/output hashes and silhouette measurements.
The final comparison and dark-background actual-size image were opened directly.
Observed: flattened pads resting beneath the chin, continuous pad-to-belly curve,
rounded rear outline and curved rear fold meeting the underside. This is a
static reference-guided candidate, not user acceptance or a transition pass.

Protection evidence: all r21 manifest renderer/source hashes remain unchanged.
The separate still has4864 fully covered head pixels identical to the previously
delivered r21 PNG;0 differences. The approved face texture, placement and running
code are unchanged. Translucent edge pixels may composite against different body
coverage and are not claimed identical. The old low-resolution original face
does differ from the locked approved face; this work does not replace it.
Independent alpha128 bottom-silhouette samples across original x12–76 give mean
absolute error0.28 logical pixels, maximum0.83. This lower-boundary check is not a
measurement of all contours or an automatic fidelity verdict.

The two targeted preservation tests also passed: closed head source/opaque output
at46 saved phases, and all96 open/closed accepted r18 run PNGs pixel-identical.
The new still script passes Node syntax checking; these are not animation tests
for the unconnected r22 posture.

No transition frames were regenerated or connected. No installed app, runtime
asset, installer, Mac ZIP, commit/push or release changed. Next is user comparison
of this still; transition work remains gated on that posture decision. Remaining
direct interactions and platform integration remain incomplete.

## r23 forepad divider and head skirt — 2026-10-17

User found the r22 forepads ambiguous. Enlarging original versus actual saved r22
showed the key mismatch: original's front-pad divider connects to the underside
of the chin, while r22's divider starts below it with a visible white gap. The
original chin descends toward that junction; the approved225 face is retained
rather than reshaped to the original96 face. r22's bottom-silhouette agreement
did not detect this interior separation defect.

Read-only sampling at logical x28: locked head chin ink around y73.5–74, followed
by opaque near-white BODY padding at y74.5–75.5, then transparency by y76.5.
That padding was covering the top of the body-ink path. No facial redraw or new
foot shape was needed. The isolated still now renders the full forepad divider
as a foreground body stroke, confined below the chin, instead of placing its
top behind the head's body skirt. Its body-layer duplicate is removed to avoid
double antialiasing. The head asset/transform, overall outline, rear fold and
running renderer are unchanged; transition composition remains untouched.

Output: `artifacts/high-resolution-sleep-still-20261017-r23/`, including full
`comparison.png`, `forepaws-comparison.png`, `actual-size-dark.png`, transparent
`candidate.png` and manifest. The enlarged paw comparison, whole-character
comparison and actual-size dark view were directly opened. The divider now
joins the chin underside and separates the two flattened pads. User visual
acceptance is pending; this is not a claim that every original face/pose detail
is identical.

Saved candidate versus r22 differs at35 pixels, all within the deliberately
bounded forepad region x62–72/y135–146 on the192px output.4842 protected opaque
head pixels outside that region are identical.8 previously head-mask-opaque
pixels in the white body skirt change because of foreground body ink; their old
RGB channels are all>=190, and crossing darker chin/face ink fails the check.
Do not repeat r22's claim that ALL opaque head-mask pixels remain identical:
the source head is unchanged, but the padding overlap is the actual fix.
All r21 renderer/source hashes still match, including face/run/transition input.
The unchanged outer silhouette retains the r22 bottom measurements. No new run
test is claimed; the already passing run result has unchanged source dependencies.

Only the isolated still generator and linked records were updated. Historical
r22 outputs remain. No transition regeneration, runtime/app, installer, Mac ZIP,
commit/push or release change. Next is user review of the forepad comparison;
transition connection remains gated on that posture decision.

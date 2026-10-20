# r53 hover / tracking / cheek — rejected candidate

## Scope and latest outcome

User authorized hover/tracking scale retention and cheek pull/release connection,
with original artwork/quality preserved and actual interaction verification.
**Not complete. Candidate rejected by the user after launch.** The screenshots
`codex-clipboard-627625b9-8179-461e-bdce-e46602d3cd51.png` and
`codex-clipboard-cfc60091-0565-405b-942e-ef97cd6ecc5f.png` show an objectionable
extended face/cheek and a protruding forepaw. The user also reports old art
appearing in other states. Technical success does not override these failures.

## Implementation attempted

- `DororongPresenter.HighResolutionGrab.cs` routes Curious/Startled, active
  tracking/hunt and high-resolution cheek capture to the source-resolution raster.
- `DororongPresenter.Hunting.cs` normalizes pointer coordinates by display scale.
- `HighResolutionFacePose.cs` applies gaze/cheek inverse sampling to the existing
  head. Cheek capture freezes the current head/body transform; existing controller
  pull/release and FaceFollowMotion provide the timeline. `CheekPullCapture.cs`
  carries this metadata and forbids accidental use of its legacy raster method.
- `HighResolutionGrabRaster.cs` samples the unchanged source head and a new
  body-only hunt bank. `build-runtime-hunt-body.cjs` derives 157 poses from the
  existing high-resolution body rig. `Dororong.App.csproj` embeds that bank.
- `DororongPresenter.xaml.cs` excludes local cheek recoil from support measurement.

This is still opt-in (`--high-resolution-grab=1.5`), not the default renderer.
Original PNGs and the approved grab bank were not changed. This does not imply
unchanged output: the new cheek deformation and hunt body composition are wrong.

## Failures, cause and verification gaps

1. **Cheek visual parity failed.** An early v1 field sheared the lower iris; v2
   restricted it to the cheek root, removing that local symptom. However v2
   carries only the edge stretch and far-eye/hair lag. It omits the existing
   `FacialCheekRenderer` near-eye/mouth rigid follow and the full
   `OutlineCheekRenderer` composition. Extending an isolated cheek contour is
   not equivalent to the accepted product motion. The user rejected the result.
   The previously accepted cheek progression and current product capture were
   opened after rejection for comparison. Do not treat v2 as an approved replacement.
2. **Forepaw composition omitted an existing product correction.** The new bank
   reused `hunt-motion.js.map`, but the legacy product also uses
   `gaze-compose.js.drawForelegs` to replace both foreleg contours and their belly
   connection. Omitting that pass exposes a small protruding pad during crouch/sway.
   `r53-defect-study.cjs` exports separate body/composite views at 0/.7/1.25/1.45s.
   Direct review reproduces the defect in the body itself, not in a modified face.
3. **Legacy fallback remains.** `RenderHighResolutionGrab` returns false for
   unsupported states; the caller immediately runs legacy `RenderPose`. Ordinary
   walking after tracking ends, sitting, sleep/wake, snacks, perch and other
   interactions are not all migrated. This permits both art and size changes.
   An enlarged legacy bitmap or frozen high-resolution idle would hide the
   symptom while violating quality/behavior requirements; neither was adopted.
4. **My visual review was insufficient.** I opened all 25 v2 sheets, but failed
   to reject the stretched-cheek appearance and protruding paw visible in those
   samples. The commentary saying no new issue was found is withdrawn for these
   defects. This was not fixed by increasing test counts. The fixture sampled
   tracking entry and a frozen cheek pose; it did not establish the entire held
   sway loop or all fallback transitions. It is not full feature preservation.

## Checks actually run

Root: `artifacts/runtime-face-20261018-r53/`.

- `tests/face-v1.trx`: 5 passed; visually rejected v1, not final evidence.
- `tests/face-v2.trx`: 5 passed. 360 stored slots / 300 distinct PNGs across
  1/1.1/1.25/1.35/1.5 and both facings. Tests check output size, alpha bounds,
  captured input mapping, press continuity, pull and release continuity.
- `tests/loop-v2.trx`: 18 passed / 5 failed. The new fixture incorrectly expected
  the feet to remain on the floor during the existing cheek-release hop.
- `tests/loop-diagnostic.trx`: 5 failures retained as diagnostic evidence.
  The corrected assertion compares world lift to `CheekSpringMotion.Lift`;
  the hop was not removed to satisfy the test.
- `tests/regression-v3.trx`: 252 passed / 0 failed / 0 skipped. Includes real
  PetLoop/Presenter/controller code with **synthetic** input and scene metadata.
- `tests/full-final.trx`: 1,262 passed / 0 failed / 0 skipped; 6m10s.
  This is the full App test suite, not a full visual or native interaction pass.
- Release publish succeeded. Original source/grab-bank hashes were checked by
  `r53-review.cjs` against the r51 source manifest.
- An additional v3 export intended to save every return sample failed to build
  because the concurrently running full test host locked the test DLL
  (MSB3027/MSB3021). No v3 visual success or exported return sequence is claimed.
  The added return-frame Save calls remain in the test for the next corrected run.

## Direct viewing ledger

Opened `sheet-v2-{scale}-{start}.png` for all scales
`1.00,1.10,1.25,1.35,1.50` and starts `0,12,24,36,48`.
Each sheet contains 12 distinct samples on white and dark backgrounds. The
manifest maps each sheet to its PNGs. Initial multi-image output was truncated;
all sheets were subsequently reopened one or two at a time. This is **300 distinct
saved samples viewed**, not 360 independent observations or every live frame.
The result is visually failed per the findings above, not passed.

After rejection, directly opened:
- `artifacts/checkpoints/cheek-face-20260907-approved/preview/progression-nearest3x.png`
- `artifacts/community-20261015/images/native/08-cheek-pull.png`
- `defect-study/hunt-layers.png` (body/composite decomposition)
- `defect-study/hunt-forebody-layers.png` (diagnostic correction)
- `tools/PreviewLocomotion/fixtures/approved-hunt-2799/hunt.png`

## Diagnostic correction, not adopted

`tools/HighResolutionArt/hunt-forebody.cjs` evaluates the existing product's
foreleg replacement curves at source-part resolution, retaining the measured
outline color/width. It is used only by `r53-defect-study.cjs --forebody`.
Direct inspection shows a broader forward-reaching forepaw instead of the small
stub, but the isolated body's upper attachment is still open. The original
renderer also has dynamic neck joins; these must be carried across and checked
under gaze before adoption. **No corrected bank or executable was published.**

## Actual execution and next boundary

The r52 test process was replaced by r53 at
`artifacts/runtime-face-20261018-r53/app-final/Dororong.App.exe`, PID118188,
with `--high-resolution-grab=1.5`. The diagnostic Started event at
2026-10-17T18:08:14Z verifies startup only. The supported computer-use inventory
did not return the pet before or after replacement. No guessed handle or
unrelated window was used. Agent-driven native mouse verification remains
unperformed; user feedback is actual negative evidence, not acceptance.
After the rejection/diagnosis, the exact r53 test process was stopped. Its files
remain available as rejected evidence; the installed app was not stopped or replaced.

Next: reproduce the complete existing cheek feature/contour behavior from the
high-resolution source; carry over the foreleg replacement AND dynamic neck
attachment; enumerate and migrate remaining fallback state boundaries without
disabling behavior; compare same-time legacy/new renders and held-sway/return
sequences at all five scales, then validate actual desktop interactions.
Latency, other DPI/environment coverage, all-motion migration and prior roadmap
gates remain open. Installer, Mac, settings, commit/push and public release were
not changed. No technical or product completion is claimed.

Skills: joewrks-pixel-art required immutable-source evidence and direct viewing,
but that procedure did not prevent the inadequate visual judgment documented
above. Computer-use target restrictions prevented native input verification.

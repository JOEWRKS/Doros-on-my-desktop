# ROADMAP — 도로롱이 내 컴에?!

## Current stretch / stomp visual feedback · 2026-10-29

- Delivered overview exposes flattened wake-stretch forelimb and missing-looking
  stamp shoulder. Exact accepted Windows keyframe reproduction confirms both shapes;
  existing Mac frame hashes match. [Diagnosis](docs/verification/2026-10-29-stretch-stomp-shape-audit.md).
- First comparison rejected as visually unchanged; second rejected because its
  inner shoulder divider unnaturally split the body. Both visual-success claims
  withdrawn; prior evidence retained in TASKS and verification documents.
- Current candidate removes the divider and joins the rounded proximal foreleg
  to the body, preserving the accepted stamp arm transform/timing. Stretch keeps
  a separate reaching paw and planted leg.48 related tests pass;282 stamp geometry
  records exact;4848 host parity and3636 unaffected-motion hashes exact.
- Contact-sheet inspection covers2706 chronological frames at3sizes/both facings,
  plus24 full-character poses. [Current correction and limits](docs/verification/2026-10-29-stretch-stomp-no-divider.md).
- User pinpointed the hair/upper-shoulder step, then rejected v4's lowered edge.
  Current v5 removes clipping and extends the original shoulder contour upward
  behind the head, matching the direction of the user's red reference line.
  All804 unique junction images covering2706frames inspected;48tests pass;
  282geometry records,4848host parity and3636unrelated frames remain exact.
  [Behind-head correction](docs/verification/2026-10-29-shoulder-behind-head.md).
- User authorized applying v5; packaged candidate launched locally as PID38720.
  48 RID tests pass;24 packaged keyframes match v5; visible window/startup confirmed.
  User accepted the running candidate and authorized commit/push plus Mac update.
  Accepted source pushed as0951ad4. Apple Silicon test11 locally packaged;
  292Mac/109Raster/14packaging tests pass and4,848distributed frames match v5.
  [Exact package and limits](docs/verification/2026-10-29-mac-test11-shoulder.md).
  Next/open gate: nativeMac test11 appearance/execution. Settings/installed releases
  unchanged; test11 has not been publicly released.

## Accepted Windows / Mac test10 delivery · 2026-10-29

- Separate Windows `655c896` and Mac `91591d5` commits pushed on the existing feature
  branch. Both exact packages published as separate prereleases; remote tags verified.
- Setup/portable/Mac ZIP and checksums anonymously downloaded and hash-matched.
  [Direct downloads and evidence](docs/verification/2026-10-29-windows-mac-public-delivery.md).
- Delivery complete; installed pet/settings retained. Native Mac acceptance below
  remains open; publication does not convert host checks into real Mac verification.
- Requested motion image handoff also complete: existing overview/GIFs/frame sheets
  bundled in `artifacts/Doro-motion-images-20261029.zip`; details in TASKS.

## Accepted Windows installation · 2026-10-28

- User-authorized local replacement complete: accepted seated/head-occlusion package
  installed with470matching payload files; settings/data/shortcuts preserved.
- Existing startupON migrated from old portable r104 to installed executable.
  Installed PID64404 launched, responsive with one visible window. [Evidence](docs/verification/2026-10-28-windows-accepted-installed-update.md).
- No new-login/reboot/uninstall or clean-PC claim. Subsequent publication/source push
  completed in the October29 delivery above.
  No remaining local replacement work; native Mac test10 gate remains below.

## Accepted build memory gate and Mac test10 · 2026-10-28

- Accepted Windows bounded900second memory observation completed without sustained
  accumulation; no unchanged Windows soak repeated. [Evidence](docs/verification/2026-10-26-accepted-memory-mac-preparation.md).
- Mac implementation/local test package complete: seated/idle/meal/DORO paths connected,
  previous popup/screen/topology/landing fixes preserved.4848exact accepted-frame matches;
  292Mac+109Raster+14packaging checks pass.300second post-port ownership check bounded.
- Apple Silicon test10 version104.10 now publicly downloadable above; exact artifact and limits in
  [verification](docs/verification/2026-10-28-mac-test10-accepted-motion-port.md).
  Original local stage made no Windows product changes; source/public delivery is
  completed in the October29 record above.
- Next/open gate: real M2 execution, Retina/input/native upload-memory/CPU, upper-screen
  drag/external-clamshell and landing smoothness. Native acceptance is NOT_TESTED;
  do not request repeated logs or call the host checks a native Mac pass.

## Accepted seated interactions · 2026-10-26

- User requested cursor paw taps, seated click shake/cooldown, and flattening
  the front-chest bump. [Scope](docs/specs/2026-10-26-seated-interactions.md).
- First and lower-paw candidates rejected by user: source stretching does not
  match an authored limb reaching toward the pointer. History retained in TASKS.
- Curved and shoulder-depth candidates rejected: the latter's AFTER preview
  still changed hair in the radial fade band; its review missed the defect.
- Current `candidate-seated-head-occlusion-20261026` removes foreground head
  painting entirely, preserving original head over the curved arm.
  18 tests pass including270 real-render comparisons;768 enlarged tap frames
  inspected,348 shake frames/396 existing samples unchanged.
  [Evidence](docs/verification/2026-10-26-seated-head-occlusion.md).
- No previous pet remained; replacement launched PID83300, responsive.
  User accepted this version. Source commit `deb5cae` pushed to
  `feature/macos-r104-arm64`; exact remote hash verified. Bounded delivery complete.
  No installer/settings/Mac update or long-duration memory claim for this correction.

## Current below-taskbar diagnosis · 2026-10-24

- Current cause established from live bounds and isolated packaged-physics A/B:
  Task Manager overlaps secondary taskbar top by5px, removing its support interval;
  physics falls40px to monitor floor. [Evidence](docs/verification/2026-10-24-below-taskbar-current-state.md).
- Diagnosis complete for current position; original triggering event unrecorded.
  Product/running app unchanged. A fix would require a bounded taskbar-overlap
  policy preserving real hide/fullscreen and ordinary-window behavior; not applied
  under this diagnosis-only request. No repeated general motion/memory tests.


## Current ambient-motion source delivery · 2026-10-24

- User chose to retain the current version and requested commit/push. Existing
 0.40s grooming entry remains; the0.70s preview is declined, not applied.
- Deliver bounded Windows implementation/tests/evidence on the existing feature
  branch. Current responsive app and stored settings remain untouched. See TASKS.
- No public release/installer update, main merge or nativeMac delivery requested.
  This decision supersedes pending visual-preference review below, without adding
  unmeasured manual, full-suite or long-duration-memory test claims.
- Completed: source commit `91c94d6` pushed and exact remote hash confirmed.
  No further work remains for this bounded delivery; running version retained.

## Grooming sit preview · 2026-10-24

- Requested comparison only: current0.40s entry versus0.70s smoother-start/end
  candidate using identical seated artwork. GIF/APNG prepared in
  `artifacts/groom-sit-comparison-20261024/`; bounded evidence in TASKS.
- Product and running pet unchanged. User declined the candidate and requested
  delivery of the current version. Preview closed without application.

## Three selected ambient motions · 2026-10-24

- Approved concepts3/5/6: waking stretch, occasional post-jewel begging, idle
  grooming. [Bounded conditions](docs/specs/2026-10-24-idle-activities.md).
- Implemented and candidate launched after normal user exit. [Evidence](docs/verification/2026-10-24-idle-activities.md):
  final Core339/339, Windows related72/72,3,258 generated frame checks and visual
  sequence review; existing396samples identical, platform/buffer checks complete.
- Preserve accepted art/motions/settings; no additional hunger or persistent state.
- User subsequently selected the current motion version for source delivery;
  the smoother sit preview was declined. Source delivery is recorded above.
- Grooming armpit correction2026-10-25: user authorized removal of retained old
  foreleg ink only. Groom-only narrow cleanup implemented; accepted paw geometry,
  body geometry and timings retained. [Evidence](docs/verification/2026-10-25-groom-armpit-line.md#bounded-correction--2026-10-25):
  36 related tests,1,374 frame comparisons at100/125/150% both facings,629 distinct
  crops visually reviewed,396 existing-motion samples unchanged. Separate Windows
  candidate launched at user request2026-10-25 21:18KST,PID80624 after previous
  process exit confirmed. User REJECTED it: required armpit line disappeared.
  Prior tests/review did not protect that crease. [Restoration](docs/verification/2026-10-25-groom-crease-restoration.md)
  now preserves the original upper crease and trims only its lower tail.39 tests,
  all1,374 chronological frames directly inspected across72 sheets; protected
  crease/exterior/soles unchanged and396 existing-motion samples identical.
  New `candidate-groom-crease-20261025` launched at user replacement request,
  PID61732,2026-10-25 21:48:56KST after no existing pet process confirmed.
  Frame presentation/reinspection found a faint/disconnected-looking junction
  at maximum wipe (150% Right f70); earlier no-defect visual judgment corrected.
  [Subsequent correction](docs/verification/2026-10-25-groom-junction.md) confirms
  standing-paw removal attenuated seated junction ink. Preserve that source
  junction while keeping the lower-tail cleanup.37 tests pass; all1,374 frames
  reviewed again; exterior/soles/endpoints and396 existing-motion samples unchanged.
  That junction candidate was rejected for its retained downward branch.
  [Branch removal2026-10-26](docs/verification/2026-10-26-groom-branch-removal.md)
  removes it without weakening the main connection, including raise/lower.
  Final37 tests pass; all1,374 chronological frames directly reviewed; outside
  ROI/soles/endpoints and396 existing samples unchanged. Latest package is
  `candidate-groom-branch-20261026`; no pet process present at final query.
  Included in the newer seated-interaction candidate above; next replacement
  uses that combined candidate. Live visual acceptance remains pending.
- NativeMac is not opted in/tested. Earlier Downloads r104 diagnostic target
  differs from the feature package; current candidate launch is recorded above.

## Accepted DORO? source delivery · 2026-10-24

- Explicit request: commit/push and update the accepted version. Preserve accepted
  front tilt/lower bubble, minute repetition, sleep policy and stored preferences.
- Existing running PID38372 already matches the final tested App/Core binaries;
  local accepted version applied, retain it without restart. Reuse existing tests.
- Deliver on existing feature branch, no main merge or installer/public-release
  replacement. NativeMac question UI and live timing measurement are not new claims.
- Completed: source commit `cfa75e6` pushed and exact remote hash confirmed.
  Local running accepted version retained unchanged; no additional work required
  for this bounded delivery. Await a separately requested change/release action.

## Lower question bubble · 2026-10-24

- User-directed position-only adjustment:8 scaledDIP downward; preserve accepted
  front tilt, other artwork/motions and60-second repeat/180-second sleep logic.
- Renderer10/10 and six size/direction detail images checked. Core unchanged.
  [Evidence](docs/verification/2026-10-24-doro-minute-repeat.md).
- User explicitly accepted the lower position: “ㅇㅇ 이 위치면 됨. 통과”. Position
  adjustment complete; preserve it and await a separately requested change. No public
  release, unrelated rework or nativeMac verification is implied.
- Lower candidate launched after user exit, PID38372 at19:43:44KST;
  exact-path response confirmed and settings retained.

## Accepted tilt / one-minute repeat · 2026-10-24

- Frontward tilt accepted. Latest requested schedule:60-second first/repeat;
  sleepOFF keeps repeating, sleepON takes precedence at180seconds. Preserve art,
  motion speed, bubble and saved preferences.
- [Evidence](docs/verification/2026-10-24-doro-minute-repeat.md): Core331 and Windows19
  pass, including repeated scheduling/deadline/option/reset boundaries. No renderer
  changes or repeat of unrelated completed verification.
- Implementation/automated checks complete; normal-use cadence acceptance pending.
  NativeMac question presentation remains outside this Windows delivery.
- Minute-repeat candidate launched PID49632 at19:39:18KST, exact-path responding;
  stored150%/sleepOFF preserved. No installer/public release/commit/push change.
- Next: review new timing in normal use. Earlier90-second single-use policy below
  is historical, superseded by this explicit user change.

## 90-second DORO? / 180-second sleep · 2026-10-24

- Frontward candidate subsequently accepted; new minute-repeat schedule above
  supersedes the earlier cadence. Preserve previous implementation/test history.

- Follow-up direction correction: frontward, away from the body, per user request.
  Only roll sign reversed;20 focused tests and both-facing visual samples checked.
  Forward candidate packaged; earlier direction superseded, user acceptance pending.
- After user tray exit, forward candidate launched PID46244 at19:31:01KST;
  exact-path response confirmed, existing settings retained.

- User-approved scope: at90seconds without pet interaction, use the accepted walk,
  then head tilt with `DORO?`; change default sleep to180seconds. Preserve other art,
  motions, speed, platform behavior and stored preferences.
- [Implementation/evidence](docs/verification/2026-10-24-doro-question-and-sleep.md):
  one question per inactivity period, interaction resets, sleepOFF independent.
  Core327 and Windows341 tests pass; whole150% question sequences reviewed in both
  directions, with100/125% detail comparisons. Separate candidate packaged.
- Windows implementation/automated verification complete; new visual acceptance
  remains pending. Mac question UI is not ported or natively verified by this work.
- Candidate launched PID38564 at15:55:26KST after confirming no existing pet;
  exact-path alive/responding. Stored150%/automatic sleepOFF left unchanged.
- Next: normal-use review of the new reaction. No new public release/commit/push,
  repeat soak, or further motion feature is authorized by this entry.

## Current-version source delivery · 2026-10-24

- Explicit request: commit/push the accumulated current source and launch the tested
  Windows candidate. Use the existing feature branch; no main merge or release
  asset replacement is implied. Preserve accepted appearance/motion/preferences.
- Candidate App/Core hashes match the previously tested380/380 output. Source
  commit150190b pushed and exact remote identity confirmed. Windows candidate
  launched PID27656 at15:19:16KST, exact-path alive/responding; preferences retained.
- Requested source delivery/launch complete. This entry records delivery, not new
  visual acceptance; no additional feature work or repeated test requested.
- Native-Mac acceptance and exact historic capture-trigger attribution remain
  outside this source-delivery verification; no new user test is requested.

## Unexpected Windows platform drop · 2026-10-24

- Scope: independently diagnose/correct reported spontaneous/capture-associated
  drop while preserving accepted arc-paw appearance, motions, timing and settings.
- [Evidence](docs/verification/2026-10-24-unexpected-platform-drop.md): corrected
  failure-expiry-as-owner-loss and confirmed zero-alpha occluder handling. Retain
  existing support during unknown metadata, preserve confirmed real loss behavior.
- 380regressions passed; separate candidate applied through normal app close/start,
  PID1084,04:50:21KST,150%/automatic sleepOFF unchanged. Core binary unchanged.
- Limited completion: reproduced causes fixed/applied; exact historical trigger
  and actual clipboard-capture interaction are not retrospectively verified.
- Next: no requested user reproduction, repeated broad checks or automatic monitor.
  Preserve this correction and accepted visuals; public release/Mac work remain
  outside this task's completion claim.

## Round outer silhouette after rejected corner smoothing · 2026-10-24

- PID27332 explicitly rejected. Tangent smoothing was insufficient; actual target
  is a visibly rounded outer sweep, preserving overall arm size and motion.
- [Evidence](docs/verification/2026-10-24-snack-elliptical-paw.md): elliptical outer
  contour, unchanged attachment/crease/motion, five-size checks,109-frame visual
  review and right-facing comparison. Separate candidate launched PID5000,
  startup confirmed at03:54:15KST.
- User explicitly accepted the arc-paw candidate: “ㅇㅇ 통과”. This correction is
  complete; preserve its rounded forepaw and chuckle motion as the baseline.
- Next: await a separately approved change; no repeated checks of this accepted
  correction. Native-Mac verification/public release are not implied by acceptance.

## Accepted arm / smooth curvature · 2026-10-24

- User accepts arm shape; only angular curvature remains in scope. Preserve
  anchors, size, other reactions and timing. Five visible joins now share tangents.
- [Verification](docs/verification/2026-10-24-snack-smooth-paw-curves.md): five-size
  tests, full150% sequence review and archived motion/pixel preservation checked.
- User requested further rounding after PID33696: broader tip/lower turn, with
  bounded local changes and unchanged motion. Follow-up tests/frame review passed;
  separate rounder candidate launched PID27332 at03:46:05KST.
- Superseded by explicit rejection and elliptical follow-up above; not accepted.

## Upright forepaw / continuous connection · 2026-10-24

- Current authority is the latest upright red trace, followed by explicit rejection
  of its unnatural lower arm/torso joint. Prior sideways shape is rejected.
- [Evidence](docs/verification/2026-10-24-snack-upright-trace-and-joint.md): corrected
  curve junction, two-image transition and backing/alpha issues; preserved earlier
  reactions and timing. Final frame/raster checks and before/after review recorded.
- Corrected candidate launched PID12124 at00:52:25KST, startup confirmed. User later
  accepted arm shape but requested smoother bends; follow-up above supersedes the
  pending general-shape acceptance. No actual Mac, broad
  performance/memory or public-release completion claim.

## User-drawn folded forepaw · 2026-10-24

- Subsequently rejected; upright trace/connection follow-up above is current.

- Latest red-outline drawing replaces the rejected ball-shaped hand/narrow wrist.
  Scope: horizontal tip and broad curved arm into chest, preserve other art/motion.
- [Evidence](docs/verification/2026-10-24-snack-folded-chuckle-paw.md): corrected shape,
  transition/feet/eye checks, five-size render and bounded raster verification;
  Windows candidate packaged. Immediate launch authorized by current user request.
- Launched PID39200 at00:09:55KST; startup confirmed. Next: actual user acceptance
  of the new silhouette. NativeMac
  and long-run memory acceptance are not inferred from these targeted checks.

## Round chuckle forepaw · 2026-10-23

- Subsequent red drawing rejects this candidate; folded-paw section above is current.

- User rejected the pointed chuckle hand; approved rounded mouth-cover pad and natural
  arm connection correction only. Existing motion/art/quality/speed remain protected.
- [Evidence](docs/verification/2026-10-23-snack-round-chuckle-paw.md): geometry corrected,
  full raise/hold/lower sequence reviewed, preservation and focused tests recorded.
- Separate tested candidate launched after user readiness(PID72040), startup confirmed.
  Next: actual user acceptance. No nativeMac or broad memory campaign; earlier
  chuckle shape is rejected, not accepted by passing tests.

## Sixth-snatch landing chuckle · 2026-10-23

- Forepaw appearance subsequently rejected; round-paw correction above is current.

- User accepted continuous second stamping, then requests mouth-cover shudders after
  sixth catch/eat and landing. Preserve all accepted earlier motion/art/quality/speed.
- [Evidence and remaining boundary](docs/verification/2026-10-23-snack-post-snatch-chuckle.md):
  inspected video24–27s, implemented support-gated reaction and fixed discovered arm
  connection/alpha seams. Automated render/core/preservation evidence recorded.
- Final WPF runtime13/13/new render5/5 and retained/full raster10/10 complete;
  separate tested candidate launched after authorized exit(PID71060), startup confirmed.
  Next: actual user appearance acceptance of new chuckle. No nativeMac/long-memory
  pass or public-release scope inferred.

## Second uninterrupted motion · 2026-10-23

- Latest authority rejects whole-clip looping. Extend active stamping continuously at
  original cadence; one preparation and one final recovery, with other reactions preserved.
- [Evidence](docs/verification/2026-10-23-snack-continuous-second.md): implementation and
  targeted regression and final no-reset checks complete; candidate launched(PID69584),
  startup confirmed. Subsequently user explicitly accepted (“통과야”). New landing
  chuckle above is separate and does not reopen the accepted second-motion timing.

## Restore second cadence / refine stamping ink · 2026-10-23

- Whole-cycle repetition was subsequently rejected. Continuous-motion entry above
  supersedes that timing target; ink refinement remains unchanged.

- User accepts other escalation motions, rejects second slowdown. Preserve original
  two-beat cadence and repeat for longer; refine ink without changing accepted large arm.
- [Correction and evidence](docs/verification/2026-10-23-snack-original-tempo-ink.md):
  waveform/pixel/runtime regression and five-size saved-frame review complete. Actual
  second reaction and perceived outline sharpness remain next. Candidate packaged and
  launched after authorized close(PID5680), startup confirmed but appearance not inferred.
  No native Mac or long-memory claim; no new broad testing scope.

## Withdrawal escalation · 2026-10-23

- User accepted the large arm/clean outline below and requests second longer,
  3/4/5 strikes, sixth fast jump/catch. Preserve accepted geometry/face/art.
- [Evidence](docs/verification/2026-10-23-snack-escalation.md): implementation, host tests,
  repeated-hit pixel checks and separate Windows packaging complete. Native Mac and
  actual desktop timing/long-memory remain unverified; no expanded pass claimed.
- Candidate launched after authorized close (PID44512); startup confirmed. Next:
  obtain actual user acceptance of the new reactions, not inferred from startup.
  No extra broad memory campaign or public release is authorized by this change.

## Third stamp large size + clean outline · 2026-10-23

- Latest authority: preserve earlier large forepaw, retain clean ink. The small-arm
  goal below was an assistant misinterpretation, now superseded.
- [Evidence](docs/verification/2026-10-23-snack-large-clean-outline.md): large trajectory
  and silhouette restored;330 archived-frame alpha comparisons unchanged, first two
  reactions unchanged;45 focused host checks and saved-frame inspection complete.
- Separate candidate packaged/hash matched and launched(PID67896); startup confirmed.
  User subsequently accepted this appearance; new escalation above is next. Native Mac/live desktop timing/long memory
  are not inferred.

## Third stamp no elongation · 2026-10-23

- Superseded by the user's clarification above: arm SIZE was accepted, stretched
  ink was not. This historical target must not be treated as current authorization.

- Latest user rejects stretched forepaw outline. Replace the old2.5x preparation
  scale with original-size shoulder rotation; retain first two reactions/face/timing.
- [Evidence](docs/verification/2026-10-23-snack-rigid-outline.md):40focused checks and
  actual-loop saved-frame inspection complete. Candidate packaged/launched(PID68324),
  startup confirmed. Next: user's appearance acceptance. Prior seam-only acceptance
  is not assumed.

## Third stamp shoulder follow-up · 2026-10-23

- Latest user screenshot rejects the body contour again. Prior saved-frame judgment
  was insufficient; motion/face acceptance is retained, not extended to this seam.
- [Focused correction and runtime-route evidence](docs/verification/2026-10-23-snack-joint-followup.md):
  local join replacement; full-display comparison plus explicit kink regression.
  Final39 functional checks/package complete; candidate PID51768 launched and startup
  confirmed. Next is user's visual acceptance, not another broad memory test.
  Native Mac/actual desktop timing/long memory are not inferred from saved host frames.

## Third stamp body-contour repair · 2026-10-23

- Motion feel now accepted; body contour is not. Keep pivot/angle/timing and repair shoulder
  compositing only. [Evidence](docs/verification/2026-10-23-snack-shoulder-contour.md).
- Full55-frame visual inspection and5-size275-frame checks replace sparse keyframes for
  this seam. Final checks/package/authorized launch complete(PID68476); user appearance
  acceptance remains next.
  Native Mac and long-memory validation are not inferred from host proofs.

## Third reaction fixed-axis rotation · 2026-10-23

- Latest drawing supersedes the extended/sheared third foreleg. Target: rigid foreleg
  swinging twice around a fixed shoulder; accepted second motion/face retained.
- [Implementation and host proofs](docs/verification/2026-10-23-snack-pivot-stamp.md) complete;
  separate candidate packaged/launched(PID64812); next is actual user acceptance.
  Native Mac, real screen timing and long-memory acceptance are not inferred.

## Second small stamp / third extended front slap · 2026-10-23

- Latest user decision: accepted small stamp becomes second reaction; third reaches the
  leading forepaw out in front and hits twice, per supplied drawing. Accepted faces retained.
- [Implementation and evidence](docs/verification/2026-10-23-snack-front-slap.md): host checks,
  5-size/enlarged proof, packaging and authorized launch done(PID50704). Next: user acceptance.
  Native Mac and long-memory results are not inferred from host checks.

## Third withdrawal forward stamp · 2026-10-23

- User authorized launch; PID61928 started19:59:34 KST and startup confirmed. Launch step
  below is complete; next remains actual user acceptance of forward/down two-hit motion.

- User clarified/rejected the vertical-only candidate. Current target is one forepaw
  extended forward, then two forward/down hits synchronized with a small pre-pounce-like bow.
  Accepted face and first/second reactions remain unchanged; no jump/world translation.
- [Implementation and host/saved-frame evidence](docs/verification/2026-10-23-snack-forward-stomp.md)
  complete. Separate Windows candidate prepared; next is authorized launch and actual user
  motion acceptance. Native Mac and long-memory behavior remain unverified, not claimed passed.

## Third withdrawal forepaw stamp · 2026-10-23

- Launch completed on user go-ahead, PID5316 at19:44:09 KST. This supersedes the launch
  pending step below; next is user's actual third-withdrawal stamp acceptance.

- Clean-wrinkles expression accepted by user; prior pending appearance notes now historical.
- User requested third reaction's head shake replaced by a planted one-forepaw stamp.
  Two lifts/downstrokes implemented, stationary head/body position, exact planted endpoints;
  first/second reaction, .5s offer and .9s total reaction unchanged.
- [Host/code-input and saved-frame evidence](docs/verification/2026-10-23-snack-stomp.md) complete.
  Next: close existing pet and launch separate Windows candidate for actual motion acceptance.
  Native Mac, actual mouse and long memory remain unverified; no repeated logs/soak required.

## Far brow / bridge fold correction · 2026-10-23

- Clean-wrinkles candidate launched at user's request (PID57304). Startup confirmed;
  next remains user's three-withdrawal appearance check, not further automatic testing.

- Current user clarification: restore muzzle wrinkle feeling without disconnected noisy dots.
  Two separated continuous curves implemented; annotated bridge/omega/timing unchanged.
  Clean-wrinkles saved renders and targeted checks complete; actual appearance acceptance next.
  This supersedes the empty muzzle/no-dots candidate, not the earlier bridge annotation.

- Latest user change: remove ellipsis-like mouth hatches. Four strokes removed and contaminated
  mouth-fill donor corrected; bridge guides/omega/motion preserved. No-dots local candidate ready
  after host/render checks; actual appearance acceptance still next.

- Current authority is user's red-circle/blue-guide annotation. Previous short-line fix targeted
  the wrong stroke. Red connector removed, old bridge folds replaced at blue guides; annotated
  candidate is now latest. Saved-render/host checks done; user appearance acceptance remains next.

- Latest follow-up: screen-left added fold half-length; screen-right added fold↘45°.
  Two-curve-only correction and saved render checked; local fold-angle candidate awaits acceptance.

- Latest user feedback rejects duplicated far brow and asymmetric long bridge line.
  [Correction/evidence](docs/verification/2026-10-23-snack-brow-bridge-correction.md): authored
  far brow retained without overlay; shortened near connector; two short bridge folds added.
- Host checks and saved5-size enlargement complete; actual appearance acceptance remains next.
  Timing/physics/assets and installed app unchanged. No native Mac acceptance claimed.

## Enlarged scrunched reference refinement · 2026-10-23

- Latest user close-up supersedes the third reaction's thin zigzag/hatches. Near lower-lid
  pinch, nose/cheek folds and clenched omega implemented; first two stages/timing unchanged.
- [Saved rendering and code-input evidence](docs/verification/2026-10-23-snack-scrunch-refinement.md)
  completed on host, including exact dirty/full comparisons and unchanged far-eye/hair seam.
- Next: new local Windows candidate actual interaction/visual acceptance. Native Mac remains
  unverified; no logs/soak, installation/settings/public release changes required here.

## Reference anger and third-withdrawal escalation · 2026-10-23

- User's two images replace the prior squeezed-eye expression only. Open eyes/brows/one
  vein at1–2 withdrawals; two veins/scrunched mouth from3. Existing .5s/.9s timing preserved.
- [Evidence and scope](docs/verification/2026-10-23-snack-reference-expressions.md): code-input
  controller checks,400 dirty/full frames, five-size saved inspection completed.
- New Windows candidate prepared separately. Next: user closes prior app, then short actual
  withdrawal/appearance acceptance. Actual Mac and long native memory remain unverified;
  no repeated logs/soak, new public release or other idle features are part of this change.

## Jewel withdrawal reaction candidate · 2026-10-23

- User authorized .5s pre-eating dwell, interrupted offer→brief anger→chase recovery.
  [Local implementation and evidence](docs/verification/2026-10-23-snack-withdrawal-reaction.md).
- Core/shared Windows+Mac reaction is connected; host regression and five-size render
  checks passed. After user closed old pet, Windows candidate launched at17:48 KST
  (PID27292); startup confirmed, actual interaction acceptance pending.
- Next: short actual candidate interaction/visual acceptance; native Mac remains
  unverified. No new logging/soak requirement, public release or idle/weight scope expansion.


## Mac test09 public tester handoff · 2026-10-22

- User authorized direct download delivery. [test09 prerelease](docs/releases/macos-r104-test09.md)
  now public; anonymous download size/SHA256 match the already-checked local candidate.
- Local-only status below is superseded for binary delivery only. Native M2 landing,
  external/clamshell and long native memory remain unverified; no new logs/repeated
  actions requested. No source push, Windows edits, or further monitoring started.

## Mac test09 landing wait candidate · 2026-10-22

- User declined further logs; previous log-acquisition next step is superseded.
- [Confirmed host bottleneck and bounded fix](docs/verification/2026-10-22-mac-test09-landing-cost.md):
  parallelize only expensive columns of the currently required sole row, preserving
  exact sampling/physics. Host changed-spread mean time improved19.5–35.0%;2,880
  pixel/position/geometry/state comparisons unchanged. Failed optimizations removed.
- Local arm64 test09 handoff; native M2 hitch elimination, external/clamshell and
  long native memory remain unverified. Total CPU improvement is not claimed.
- No additional logs/repeated user actions requested. Do not mark native acceptance
  complete from host results or resume old monitoring/shutdown automations.

## Mac test08 partial native feedback · 2026-10-22

- User reports upward movement works; landing/spread stutter persists. Record
  partial user feedback, not universal/native trace pass. External still untested.
- Resent log attachments are exact copies of test07-era schema1 inputs, not new
  stage timings. Next: identify current session output from existing terminal/path
  before investigating landing cost; no repeated geometry test or speculative speed change.

## Mac test08 native-placement repair candidate · 2026-10-22

- Confirmed placement mismatch addressed with owned-overlay constraint override;
  test08 built locally. [Implementation/evidence/limits](docs/verification/2026-10-22-mac-test08-native-placement.md).
- Launch instructions and runtime now distinguish app lookup, app launch, fresh
  diagnostic start and verified end; no repeated blind retry instruction.
- Host284/package11/launcher10scenarios and2880 golden rows pass, NOT native Mac.
  Next is targeted native top/release placement acceptance; separate rendering
  delays need stage evidence. Windows/accepted art/physics unchanged.
- JOENESS failure-report analysis delegated to the user-specified existing thread;
  no JOENESS edits here. No new public release, arbitrary app restart or soak.

## Mac native placement cause identified / report delivered · 2026-10-22

- [Two-log analysis and guidance failure report](docs/verification/2026-10-22-mac-boundary-analysis-and-guidance-incident.md)
  supersedes the missing-log gate below. Native actual window top clamps atY33
  in37samples while model/requested coordinates continue upward; seven falling
  samples also remain clamped. Fix the Mac placement layer, not physics speed/art.
- Additional cadence/fallback delay remains unresolved; no frame-time cause or
  universal performance pass inferred. External/clamshell remains NOT_TESTED.
- Next: placement repair with matching render/input coordinates, robust diagnostic
  launcher status/path handling, and targeted native validation. No repeat geometry
  recording requested; this turn delivered analysis, not an implemented repair.

## Mac test07 native acceptance still blocked · 2026-10-22

- Upper-screen restriction persists per user; falling stutter newly reported.
  External/clamshell NOT_TESTED. [Follow-up](docs/verification/2026-10-22-mac-test07-native-followup.md).
- Next: bounded native requested/actual placement evidence on internal screen,
  plus identify stutter phase; existing half-second geometry log cannot certify
  frame timing. Do not repeat host matrices or alter accepted motion speculatively.

## Mac test07 tester delivery · 2026-10-22

- User-authorized test07 prerelease is public; exact candidate ZIP was independently
  downloaded without authentication and its hash matched. [Delivery/steps](docs/releases/macos-r104-test07.md).
- Supersedes local-only status below, not native acceptance. Next: affected Mac's
  upper-screen head drag and window-edge support after clamshell transition only.
  No repeat generic soak requested; Windows release unchanged.

## Mac screen-transition hardening / test07 · 2026-10-22

- User authorized combinatorial host testing before another native diagnostic.
  Reproduced two stale-topology caches and fixed their transition invalidation;
  delayed old queries cannot restore removed displays. [Evidence](docs/verification/2026-10-22-mac-topology-test07.md).
- Host checks: Mac180, final transition42, package11;2880 unchanged stable-layout
  frame/coordinate rows. Local test07 ARM64 app ready; Windows/art/physics untouched.
- Not all possible OS states/native success: upper-screen cutoff not reproduced,
  and the tester's clamshell failure is not yet proved resolved. Next is actual
  affected-Mac top drag and clamshell window-edge recheck, not a generic soak.
  Optional90second native log remains available if needed; no public upload.

## Mac display-boundary investigation / native evidence gate · 2026-10-22

- Upper-screen/clamshell investigation authorized; controller's synthetic internal
  top drag and three external-only window-perch layouts pass. Native cause is NOT
  established. No speculative movement/perch change or Windows change.
- [Bounded test06 diagnostic and limits](docs/verification/2026-10-22-mac-display-boundary.md).
  Local ARM64 ZIP with opt-in90second readback logging; Mac57/package11 host tests.
- Follow-up authorized combination testing/test07 is recorded above. Native symptom
  acceptance remains open. No soak, automation, interruption or public release.

## M2 native feedback triage · 2026-10-22

- Tester reports stable animation/100–150% size/Dock floor, saved sleep setting,
  singleton and ~200MB at~5hours. [Evidence limits](docs/verification/2026-10-22-m2-tester-feedback.md).
- Exact build not identified; do not clear test05/native-all-paths or universal
  memory/CPU gates. Next priority proposed: upper-screen drag restriction and
  clamshell external-monitor window support, not repeated generic memory soaks.
- Follow-up investigation authorized; current evidence/status is recorded above.

## Mac popup slider visibility defect · 2026-10-22

- Character popup missing logical/theme parent reproduced in host diagnostic;
  direct-window tests had missed this route. [Evidence](docs/verification/2026-10-22-mac-slider-visibility.md).
- Authorized repair completed locally: popup theme parent and cleanup,50 Mac host
  tests/11 package tests; both routes' pointer/keyboard/close/reopen checks covered.
  [test05 candidate and limits](docs/verification/2026-10-22-mac-popup-test05.md).
- Original test04 renderer/simulation binaries reused; Windows/art/motion unchanged.
  Next: actual M2 verification. Local ZIP ready; no public upload or native pass.

## Windows size-label report · 2026-10-22

- Diagnosis reproduces the small `크기` glyph defect independently of menu
  clipping; derived size row lacks other items' weight style and uses Normal12/
  Display formatting. [Evidence](docs/verification/2026-10-22-windows-size-label.md).
- Follow-up authorized and implemented in the Windows release worktree: title-local
  formatting,19 targeted tests,20 DPI/state render comparisons and local ZIP ready.
  Mac untouched; next is actual user popup confirmation, not a broad motion retest.

## Mac test04 direct tester delivery · 2026-10-22

- Latest Mac direct download requested. Publish exact verified test04 ZIP with
  matching source and [M2 essential checks](docs/releases/macos-r104-test04.md).
  Rebuild/Windows changes excluded; preserve old releases.
- Public prerelease419740104/tagmacos-r104-test04 published; original ZIP/server digest/
  fresh public download match. Matching source119f2f5 pushed; Windows release unchanged.
  Next gate remains actual Mac CPU and memory/graphics acceptance, not a repeat host soak.

## Mac test04 host long-run memory investigation · 2026-10-21

- User separately authorized two-hour Windows-host Mac-code repeat testing before
  native acceptance. Driver qualification and two-hour wall-clock run completed.
  Product/art/motion/quality/speed and Windows remain unchanged.
- [Run identity, scope and evaluation](docs/verification/2026-10-21-mac-r104-host-soak.md).
  718samples/9,007 applied resizes,head/cheek29each,28 complete meals; no sustained
  accumulation in the tested host workload (private medians~126.8MiB, buffers fixed).
  No confirmed defect warrants speculative code changes or another identical soak.
  Universal leak-free/native graphics/latency acceptance is not inferred.
- Next: native Mac window/GPU/CPU acceptance. Bounded follow-up removed; no restart.
  This adds an approved host-memory gate, not a restart of old Windows pet soaks.

## Mac dirty-region test04 candidate · 2026-10-21

- Changed-region rendering authorized. Breathing/gaze use bounded recomputation;
  all other changed poses/scale/effects fall back to original full rendering.
  Windows, artwork, motion, sampling quality, speed and frame cadence unchanged.
- Host8,640 pixel/coordinate/revision samples match; retained image storage bounded.
  Host150%/density2 idle tick7.92→3.65ms, not an M2 performance claim. Total process
  memory equality and native graphics/long-run behavior remain unverified.
- [Implementation and evidence](docs/verification/2026-10-21-mac-r104-regions-test04.md).
  111 host/package tests passed; local arm64 test04 ZIP integrity complete.
  Next: same-M2 CPU, transition-edge and memory check; no native pass inferred.
  This supersedes test03 as the next local candidate; old packages/releases remain.

## Mac CPU test03 candidate · 2026-10-21

- Confirmed duplicate geometry scans, intermediate pixel copies and identical native
  state updates removed without Windows/art/motion/speed/FPS/resolution changes.
  [Evidence and boundaries](docs/verification/2026-10-21-mac-r104-cpu-test03.md).
- 98 host/package tests pass; repeated samples preserve pixels/coordinates/timing.
  Host idle cost falls, but aggregate CPU does not improve in every scenario.
  test03 local arm64 ZIP and integrity checks complete; M2 CPU99.8% resolution/native
  acceptance remains open. Prior ZIP/releases preserved; no public upload.
- Next: the exact test03 candidate on the same M2/display/size for idle/walk/sit.
  Profile a remaining high-CPU native run before attributing any further cause.
  No repeated Windows memory tests, new public-release approval or power automation.

## Mac test02 requested delivery · 2026-10-21

- Publish the existing exact test02 ZIP with matching source and concise tester
  instructions. Completed: matching source `0a6b3c8` pushed/tagged
  `macos-r104-test02`, public ZIP re-downloaded and SHA256 verified.
- [Instructions](docs/releases/macos-r104-test02.md). Next is the user's actual
  Apple Silicon run; no native acceptance or new Windows changes are implied.

## Windows r104 requested delivery completed · 2026-10-21

- Separate Windows release branch/tag pushed (`release/windows-r104`, `windows-r104`,
  source `9904185`); public portable download hash verified. See TASKS Windows
  delivery entry. Pending Mac work remains isolated and unpushed.
- Accepted art/motion/speed preserved; ordinary-launch mode connected. 1,535 tests
  and active-desktop lifecycle passed; private-desktop timeout and other-PC/signing
  limits remain explicitly unverified. No repeat soak or installation changes.

제품·사용법은 [README](README.md), 실행·검증 기록은 [TASKS](TASKS.md)가 원본이다.
이 문서는 승인된 방향, 현재 단계와 단계별 완료 조건을 관리한다.

## 현재 위치 · 2026-10-19

- **r104 Apple Silicon test02 · 목/침 경계 수정(10월21일):** WPF와 다른 coverage/
  곡선 외곽선 생성 원인을 수정, 원본4,102표본 및5크기·2밀도350프레임 동일 확인.
  [검증·패키지·남은 조건](docs/verification/2026-10-21-r104-vector-coverage.md).
  Windows/원화/수락 동작·속도/슬라이더 변경 없음. test02 ZIP 생성·무결성 확인.
  아래 test01의 동적 목/침 경계 차이 미완은 이 결과로 대체. 실제 맥 실행·성능·메모리와
  사용자 수락은 아직 미완, 비정규 먹기 진단 fallback 차이도 전체 통과로 숨기지 않음.
  다음은 정확한 test02의 실제 맥 검수. 종료된 Windows 메모리 검사/전원 자동화 재개 없음.

- **r104 Apple Silicon test01 패키지 생성·전체 수락 미완(10월21일 후속):** Mac 기본
  고해상도 실행·상호작용·연속 슬라이더 연결, 발 고정/밀도 전환/잡기·놓기 회귀 검사.
  자동 검사80개·자세237표본 동일, 픽셀70표본 중55개 동일. 고정 팔/실제 먹기 효과
  차이는 수정했으나 동적 목/침 경계 차이와 실제 맥 실행·성능·메모리는 미확인.
  [확정 후보·검증 경계](docs/verification/2026-10-21-r104-source-and-mac-port.md#connected-r104-apple-silicon-test-candidate--2026-10-21).
  `candidate-final/Doro-Mac-arm64-r104-test01.zip`만 전달. 다음은 그 후보의 맥 검수와
  경계 차이 해결·수락. Windows 코드/원화/설치본 유지, 기존 메모리 반복 검사 재개 없음.
  아래 foundation 연결·패키징 미완은 본 항목으로 대체하며 native gate는 유지한다.

- **r104 최종 소스 푸시 완료·Apple Silicon 전체 이식 미완(10월21일):** 사용자
  푸시·맥 버전 요청과 M1 이상/r104 전체 이식 선택 확인.
  [범위와 필수 이식·검증 순서](docs/verification/2026-10-21-r104-source-and-mac-port.md).
  Windows 수락 소스 `3813312` 원격 SHA 확인. 공통 고해상도 렌더/고정 표시
  버퍼 기반 및 arm64 개발 빌드까지 확인했으나, 벡터20표본 차이·최신 상호작용/
  연속 슬라이더의 Mac 연결·패키징·실제 맥 검증은 남음. 기본45표본만 바이트동일,
  기존 맥 test04나 개발 빌드를 r104 완성본으로 대체 표시하지 않는다.
  Windows 설치본/동작/화질/속도는 유지, 종료된 메모리 검사는 반복하지 않음.

- **r104 그래픽생성경로 한정추적완료(10월20일23:43):** 관리자승인·전후고지ETW손실0,동일렌더진단첫1000변경+34MiB를텍스처/투명창표시버퍼로주소·스택확정;동일1000반복추가0/앞발추가0. [근거](docs/verification/2026-10-20-r104-graphics-root.md#result--2026-10-20-2343-kst). 정상r104기존영역도추적중증가0. 그래픽보유재사용관찰·수정할지속누적결함미확인이라제품무변경,수락화질/동작유지. 과거5.875MiB정확생성스택·내부free상태는소급확정불가/전체누수통과아님. 진단종료·추가재조작없음,다음확정조건은정상앱새증가순간의스택. 아래추적준비상태대체.

- **r104 잔여5.875MiB 생성경로추적 준비:** 사용자명시후속요청으로동일렌더DLL별도진단단계재생/주소·호출스택대조준비. [근거·제한](docs/verification/2026-10-20-r104-graphics-root.md). 기존앱·수락동작미변경,과거스택소급확정불가. 다음은관리자추적승인후유한재현·증가/보유구분,확정결함만수정. 현재원인확정·제품검증통과아님.

- **r104 그래픽보유 한정관찰종료(10월20일23:30):** 동일PID601초117표본정상,기존한영역+5.875MiB후마지막174.6초안정/후속표본동일. [근거](docs/verification/2026-10-20-r104-graphics-retention.md#final-bounded-result--2026-10-20-2330-kst). 고지후heap은bitmap1/frame12/보행64·추가동작버퍼확인,그래픽추가분정확보유원인·수시간미완. 지속누적확정아니므로제품추측수정없음/수락그림·속도·화질유지. 한정예약중지/추가재조작불필요. 다음은새반복증가재현시현재할당스택추적;전체누수해결통과아님. 아래진행중상태대체.

- **r104 그래픽보유 후속 한정관찰중(10월20일23:13):** 사용자반복증가확인요청으로동일PID/수락실행유지,10분외부주소별메모리관찰시작. [범위](docs/verification/2026-10-20-r104-graphics-retention.md). 시작write-combine134.81MiB/제품변경없음. 실제반복은사용자요청·응답대기이며내부입력계측없음. 다음은주소별반복상승/쉬는구간회수판정,증가확인시원인추적·필요수정. 멈출검사는직전고지,미확인통과금지. 아래자동재검사없음은이번명시요청범위만대체.

  23:14완료보고수신/초기3분기존한영역+1.75MiB,다른4영역동일. 아직누수여부미판정;남은기록후한정heartbeat결과확인·중지예정. 추가사용자조작불필요.

- **r104 조절지연/프레임폐기수정 한정검증완료(10월20일22:56):** 실제494반영/5왕복,중앙6.625ms·사용자문제없음. 6분동일PID정상기록/사후고지덤프로회수대기프레임57.26→.56MiB확인(횟수다른실행비교),동일2000변경할당검사별도. [최종범위](docs/verification/2026-10-20-r104-size-frame-storage.md#final-bounded-actual-result--2026-10-20-2256-kst). 전체private약339MiB/그래픽계열write-combine약135MiB보유는남음;2표본안정만확인/전체누수해결·장시간통과아님. r104정상유지·한정계측종료,자동재검사없음. 남은미확인은현재그래픽영역할당스택/장시간보유이며수락형태를바꾸는추측수정은하지않음. 아래r103/r104기록중상태대체.

- **r104 프레임저장재사용/실제기록중(10월20일22:46):** r103실제949조절에서지연중앙6.824ms로개선확인,별도덤프로남은57.26MiB프레임회수대기원인확정. 기존12슬롯재사용으로보강/44검사·50프레임동일,합성warm100조절할당42.4→1.1MiB. [근거](docs/verification/2026-10-20-r104-size-frame-storage.md). r104 PID32820/6분한정실제기록중,사용자“문제 없음”. 다음은입력·메모리·후반보유판정,아직전체누수해결아님. 그림/화질/속도/설치본유지.

- **r103 조절지연원인수정/실제반복기록중(10월20일22:30):** 반복native투명창size재구성이주지연으로확인돼고정backing/기존논리viewport와픽셀반올림유지. 관련25검사/50프레임·좌표동일,메모리live캐시·free공간·그래픽영역분류. [근거/제한](docs/verification/2026-10-20-r103-size-window-lifetime.md). 정상r103 PID45036 내부300/외부360초로실제슬라이더반복측정중. 수락형태·화질·속도·발위치유지,전체누수/무지연통과아님. 다음은실제입력로그·지연·후반보유및사용자피드백판정.

- **r102 연속조절수락·한정실제기록종료(10월20일22:10):** 사용자좋음보고,100~150% 실제53번크기반영/왕복1회·241.87초메모리관찰. 후반약291MiB평탄,적용시간30~47ms잔존/조절전대비일회보유분소유자·장시간실제반복미확인. [최종범위](docs/verification/2026-10-20-r102-continuous-size.md#final-bounded-actual-result--22082210-kst). 수락형태·속도·화질유지,PID62576그대로실행·계측종료. 이번연속조절수정인계,무지연/전체누수해결선언및무기한재검사없음. 아래기록중상태대체.

- **r102 연속크기갱신 실제기록중(10월20일22:04):** r101기본조절괜찮음보고후연속추종개선요청. 입력대기해제·크기보정의보행거리분리·scratch재사용. 관련41검사/같은크기픽셀유지, 합성반복할당감소확인. [근거](docs/verification/2026-10-20-r102-continuous-size.md). 정상r102 PID62576 내부180/외부240초실제조절·회수기록중/아직성능메모리통과아님. 다음은성공크기적용횟수·시간과사용자화면확인/후반메모리대조. 원화·화질·동작·속도·설치본유지.

- **r100 선수정수락 / r101 크기슬라이더 실제수락대기(10월20일21:49):** 사용자수락후 기존B2양메뉴100~150% 슬라이더/저장/발위치보존 연결. 관련27검사·게시DLL양메뉴/5크기검수완료, 정상투명r101 PID49528 실행. [증거·미확인](docs/verification/2026-10-20-r101-size-slider.md). 동작·형태·속도·원화·설치본유지. 다음은 실제마우스조절·클릭위치·재시작저장복원 확인. 고대비/모니터DPI/모서리·포커스실제검증미완; 과거메모리검사재개하지않음. 아래r100수락대기만대체.

- **r100 닫힌 표정 앞머리·눈 선 정리/실제수락대기(10월20일21:33):** 요청한 선만 native동일굵기/색으로 정리. 관련17검사·415프레임 비교(385동일·범위밖0)·5배율시트 직접검수. [근거](docs/verification/2026-10-20-r100-closed-fringe-lid-ink.md). 원화·열린눈·입·몸·머리끝·동작/속도·r99앞발수정 유지. 사용자종료확인후 정상투명r100 PID59664 실행, 다음은간식중닫힌눈/앞머리 실제화면 피드백. 메모리/타동작 반복검사를 재개하지 않음.

- **r99 간식·매달린 앞발3건 수정/실제수락대기(10월20일21:21):** 앞머리 잔선 국소합성, 최대당김 중간캔버스 여백, 클릭흔들림 겨드랑이 경계 교정. 관련101검사/5배율415프레임 비교·시트직접검수 완료. [근거와 범위](docs/verification/2026-10-20-r99-snack-perch-contours.md). 사용자기존앱종료후 정상투명1.5배r99 PID36292 실행/Started확인. 원화·타수락동작·속도·설치본·설정 유지. 다음은 이번3건 실제화면 피드백이며 아직수락통과아님. 이전메모리 한정검사는완료된그대로 두고 수시간반복검사를이번범위로재개하지않음. 아래 역사적 메모리 미검증은 전체해결주장제한이지 이번 다음작업을 끝없이확장하는조건이아님.

- **r98 조작후5분한정관찰완료·지속증가미재현(10월20일17:54):** 동일PID/301.13초146표본/최대간격2.135초. private약270.6MiB·범위0.172MiB/GDI80고정,후속260.31MiB로감소. [근거](docs/verification/2026-10-20-r98-memory-root-audit.md#final-bounded-follow-up--1754-kst). 추측추가수정·덤프·재조작없음/한정heartbeat PAUSED/r98유지. 장시간·늦은혼합입력계측·외형수락은미완이며전체누수해결아님. 다음확인범위는수시간후보검증과실제동작수락;무기한감시는시작하지않음. 아래관찰중상태대체.

- **r98 최초10분기록완료·조작후보유관찰중(10월20일17:49):** 사용자완료보고/정확횟수·시각·외형수락미확인. 601.79초291표본연속확인,후반private약230→273MiB/GDI33→80증가;입력기록은앞5분Body2건만있어혼합검증통과아님. [근거](docs/verification/2026-10-20-r98-memory-root-audit.md#user-completion-and-initial-actual-result--17461749-kst). 같은PID46620유지·재조작없이300초후속패시브관찰/한정예약갱신. 다음은자연회수·계속증가분리,미확인누수해결선언금지.

- **r98 정상 실행·실제 조작 기록 중(10월20일17:35):** 사용자r97종료확인후검증동일hash/PID46620/정상투명1.5배실행. 내부300초·외부600초첫표본확인/시작고지,혼합조작요청. [근거](docs/verification/2026-10-20-r98-memory-root-audit.md#normal-r98-launch--2026-10-20-1735-kst-measurement-in-progress). 아직실제조작·메모리통과아님. 다음은입력/완료보고와측정종료대조,수시간미완유지. 아래r97유지·r98교체대기만대체.

- **r98 메모리 원인 분리·2개 반복생성 수정, 정상앱 검증 대기(10월20일):** 목의매프레임네이티브bitmap/매달리기복귀2대형배열생성을재사용으로교정. [근거](docs/verification/2026-10-20-r98-memory-root-audit.md). 최종27검사·477프레임hash동일,게시DLL실행목1570프레임자연Gen2 74→0/1.5배복귀warm할당16.64→0.10MB,실제WPF혼합32회warmGDI42~212→34~38. 실제r97cache상한확인/그래픽65.996MiB는24분후표본동일/nativeheapfree보유분리. 전체영구누수해결·수시간후보통과선언아님. 수락동작/형태/속도/원화/Core유지,r97 PID38088유지. 다음은기존앱종료확인후r98정상교체·실제혼합입력및장시간후보검증;진단자동재생을사용자입력검증으로부풀리지않음. 아래r97원인미완중확정된2경로는본항목으로대체.

- **r97 큰 영역 생성 경로 확인/추가 보유 미완(10월20일17:00):** 실제 시작·혼합조작 ETW 두건 저장/손실0. write-combine증가23.34MiB가WPF/D3D텍스처·렌더타깃·효과버퍼경로임을주소/스택대조로확인. 이후패시브구간에추가22.19MiB/새영역보유가있어단순초기캐시·누수해결판정불가. [근거](docs/verification/2026-10-20-r97-graphics-allocation-stacks.md). 제품변경없음/수락동작유지/PID38088실행유지. 다음은warm반복추가분스택·실제보유분리,장시간미완.

- **r97 할당 수집 경로 복구, 메모리 원인 미완(10월20일16:47):** WPR 저장0x80010106 실패를 분리. 직접ETW5초시험의 파일·할당스택 수집 확인/손실0이나 시작구간·반복조작·큰네이티브영역 소유 확인은 아직 아님. [근거](docs/verification/2026-10-20-r97-allocation-trace-attempts.md). 제품 변경 없이PID31300유지/추적종료. 다음은 사용자 종료준비와 명확히 동기화한 동일r97 시작 추적.

- **r97 관리자 추적 진행 중(10월20일):** 사용자 승인 후 같은 실행본의 시작·반복 조작 할당 추적. 첫 시도는 지연된 종료 질문으로 사용자 종료/조작 미실행 및 ETL 저장 미확인이라 실패로 분리. 별도 재시도 PID31300 시작, 결과 분석 대기. 제품 동작·형태·속도 변경 없음. [실행 원장](TASKS.md#r97-remaining-native-classification--offline-evidence-privileged-trace-pending--2026-10-20).

- **r97 잔여네이티브분류진전/호출경로미완:**과거기본힙commit증가13.34MiB중free증가11.38MiB확인;큰두영역은write-combine메모리,소유자미확정. [근거](docs/verification/2026-10-20-r97-native-heap-classification.md). 추측제품패치없음/동작유지. 다음검사는관리자승인후동일r97시작부터할당추적이며아직미실행;장시간/전체메모리해결미완.

- **r97 한정실제검증완료,전체메모리미완:**601초/간식입력5건,후반5분약268MiB평탄(중앙값범위0.42MiB). 고지후heap으로실제메뉴·커서동일원본해상도source공유/디코더체인미보유확인. [근거](docs/verification/2026-10-20-r96-snack-detached-source.md). 전체native초기증가/절감량·장시간·직접시각미확인,추가추측패치없음/r97실행유지/이번heartbeat종료. 아래측정중상태만대체.

  r97 heartbeat PAUSED 확인. 앱과PC는종료하지않음.

- r97사용자반복완료보고수신,외형·횟수독립검증아님.600초기록진행중이며완료후단일후속확인heartbeat r97예약(약16:14:46),분석후해제. 아직메모리통과판정없음.

- **r97 실제 측정중(10월20일16:01:38KST):**기존종료확인후정상투명1.5배PID30432/600초외부메모리·300초입력기록시작. 메뉴/간식각5회반복·외형피드백요청,실제결과미완. [근거](docs/verification/2026-10-20-r96-snack-detached-source.md). 아래교체대기상태대체;전체native절감·장시간검증미완유지.

- r97게시DLL직접재검사59통과/효과120동일로 준비검사완료. 아래게시재검사대기만해제,실제메뉴사용/메모리재검증은사용자종료준비후진행하며미완유지.

- **r96 실제 반복검증 완료·r97 메뉴공유 보강 대기:**사용자화질/동작변화없음,간식5/몸6/볼5입력기록·600초관찰후마지막5분약274MiB평탄. 고지후heap에서간식decoder참조제거/기록목록해제확인,전체private절감·장시간누수해결은미확인. 메뉴별도URI로딩재현후같은source공유로보강,59관련검사·320픽셀비교/효과120동일;게시DLL재검사후실제교체·메뉴반복검증필요. [상세](docs/verification/2026-10-20-r96-snack-detached-source.md). 수락동작·원화·화질·속도/설치본/설정유지,r96실행중. 아래r96대기상태만대체,나머지native할당원인미완.

- **r96 후보·실제검증 대기:**간식원본픽셀을축소하지않고 공유이미지에서디코더/캐시체인참조를분리. 첫12검사/원본전체픽셀·8배율256표시·r95효과120프레임동일. [근거](docs/verification/2026-10-20-r96-snack-detached-source.md). 실제메모리절감·나머지native보유원인은미확인,추가회귀후실제반복사용측정예정. 기존형태/속도/설치본유지,턱걸림범위제외.

- **10월20일 메모리 후속 원인 부분 확정:**저장된 r95 dump에서 간식1254×1254 픽셀 세 벌 약18MiB를 원본과 전체 바이트 일치로 확인,작은 간식커서가 원본해상도 디코더/캐시를 보유하는 경로 확인. [근거](docs/verification/2026-10-20-r95-snack-native-retention.md). 전체약60MiB 증가의 나머지는 미확정,제품 수정·실제 재검증은 아직 없음. 다음은 승인 후 표시품질 보존하는 로딩/캐시 보강·반복사용 비교. 작은 놓기 턱은 원본에도 존재한다는 사용자 판단으로 범위 제외하며 이전 보류/수정필요 상태를 대체. 자동화/종료 재실행 없음.

- **종료 요청 수락 · 10월20일03:02:14 KST:**r95-3 자동화 중지 후 사용자 승인된 강제 완전 종료60초 명령 exit0. 실제 전원 차단·이후 재기동 방지는 확인 불가. 아래 종료 예정/관찰 중 상태는 이 실행 결과와 최종 검증 기록으로 대체.

- **10월20일 r95 3시간 검증 종료:**1072표본,정상 완료. 초기 조작 후 약60MiB 증가했으나 후반2시간은 약264MiB로 평탄;관리객체 증가2.06MiB/이미지캐시 상한 유지,나머지 네이티브 보유 영역 분리 확인. 지속 누수 미재현으로 추가 제품 변경 없음. 초기 네이티브 증가 전량의 호출경로·하루종일/다른 환경은 미확인으로 남기며 모든 누수 해결로 표기하지 않음. [최종 근거](docs/verification/2026-10-19-r95-three-hour-soak.md). 이번 한정 검증·분석은 완료,기존 동작 유지. 사용자 승인에 따라 결과 저장 후 자동화 중지·완전 종료 진행;다음 기술 조사 조건은 새로운 지속 증가 재현.

- **10월20일 종료 조건 추가 승인:** 메모리 검사와 필요한 보강·재검증 완료 후 결과를 저장하고 PC 완전 종료. 사용자 강제 종료 승인,기존 heartbeat에 연결. 현재 종료 예약은 없음;미완료 시 자동 종료 조건 미충족. 빠른 시작을 우회하며 BIOS/WOL 재기동 방지는 미검증. 상세는 [측정 기록](docs/verification/2026-10-19-r95-three-hour-soak.md).

- **r95 추가3시간 실제 메모리 측정 중.** [근거](docs/verification/2026-10-19-r95-three-hour-soak.md). 23:56:42 시작,10월20일02:56:42 종료 예정,30분 간격 채팅 후속 확인 등록. 00:01 사용자 첫 반복 조작 완료 확인(자동 입력 계측 아님);204→최대271.39→261.39MiB 증가 관찰,원인 미확인. 동작·형태·속도 유지. 다음은 조작 후 회복/보유 추세 대조,지속 증가 시 고지 후 원인 진단·수정. 결과 미확인.

  00:27 확인:조작 후 약262MiB/GDI92 유지,현재 급증 지속은 없으나 초기 대비 약58MiB 보유 원인 미확인. 정상 기록을 계속하며 종료 시 사전고지 후 heap으로 보유 주체 대조 예정. 완료·전원 종료 조건 아직 미충족.

- **r95 추가30분 실제 측정 완료·지속 증가 미재현.** [근거](docs/verification/2026-10-19-r95-long-run-retention.md). 같은PID40712,Private203.72~205.13MiB/시작·끝 약204MiB,GDI48유지. 고지 후 heap에서 늘어난3.13MiB는 상한4개 착지 퍼짐 캐시와 보정본;보행64개 유지. 추가 제품 수정 없이 재사용 검사 추가/관련25개 통과,동작·형태·속도 유지. 아래5분의 장시간 미확인은 이30분 구간에 한해 대체. 초기 상승 전체 원인·수시간/모든 조작 누수는 미확인. 다음은 실제 지속 증가가 재현될 때 보유 주체를 특정해 수정;작은 놓기 걸림은 사용자 지시대로 보류.

- **r95 메모리 정리·5분 실제 측정 완료,장시간 미확인.** [근거](docs/verification/2026-10-19-r95-metadata-snapshot-capacity.md). 사용자 놓기 작은걸림은해결아닌보류. 반복창객체/임시배열할당정리,관련140검사통과,동작·원화·속도·Core 미변경. 정상투명r95 PID40712에서기록버퍼없는300초관찰:private145.71→167→205.95MiB;사전고지heap live107.88MB,렌더러각1개/캐시64상한/기록버퍼없음. 중간상승전체원인·장시간누수·잔여렌더지연은미확인. 다음은장시간보유추세대조,전체성능통과선언않음.

- **r93 놓기 동작 사용자 수락·성능/메모리 미완.** [근거](docs/verification/2026-10-19-r93-center-fall.md). 중심 낙하와 자세복귀 분리 후 감속 여부 질문에 사용자 “굳.” 확인,이번 놓기 동작을 유지한다. 중력/180ms복귀·원화·착지퍼짐 유지,관련93검사/620픽셀·335착지시각·75놓기원시이미지동일.120초 기록은 입력0/놓기프레임0이므로 사용자 수락을 에이전트 실제 프레임 검수로 확대하지 않는다. 다음은 수락동작 고정하에 잔여 외부렌더지연·장시간메모리 확인. 이번 후속은 기록만 갱신,실행본/설정 미변경. 아래 r92실행상태는 사용자종료로 대체.

- **r92 놓기 직후 자세/낙하 상쇄 수정 후보·실제 기록 중.** [근거](docs/verification/2026-10-19-r92-release-onset.md). 사용자 각도/수직 차이를 먼저 조사,물리는 즉시 진행하나 자세 축소가 보이는 하강을 상쇄함을 확인. 기존180ms/형태/중력·착지 유지,재사용 표시 위치 보정 최종673검사/620픽셀·335착지시각동일. 머리 역행/2회 raster 중간 후보는 미실행 철회.22:07정상투명1.5배180초 기록 시작,실제 프레임/사용자 수락 미완;다음은 기울인 놓기와 수직 놓기 대조. r91 사용자 종료. 기존 외부 렌더 지연·장시간 메모리 문제는 이 동작 수정으로 해결 판정하지 않는다.

- **r91 동일 창 정보 재사용 수정·실제 검증, 낙하 버벅임 잔존.** [근거](docs/verification/2026-10-19-r91-scene-reuse.md).651검사/620픽셀·335착지프레임시각 동일,원화·Core·수락동작/속도유지. 실제1.5배300초 머리4입력/강한착지3회 공중퍼짐보존·5PNG직접검수. 사용자 낙하끊김보고,긴tick11/callback23·창setter73.10ms·workerwait53.29ms로전체지연미해결. 낙하34~42ms갱신과다른시점73ms창지연구분.600초외부메모리후반263.2→238.5MiB,사전고지2heap큰배열96/85.68MB동일·cache64재사용;장시간전체누수통과아님. 다음은창/합성동기대기·활성worker지연을수락동작변경없이수정·실제부하대조. r91정상실행유지/기록종료,아래r90실행상태만대체.

- **r90 측정 지연·빈 작업자 대기 수정 및 실제 검증, 비렌더 지연·장시간 메모리 미완.** [근거](docs/verification/2026-10-19-r90-observer-worker-wait.md). 프로세스조회/저장 UI분리,완료 판단을 작업자 응답에서 실제 픽셀완료로 변경. 기준경로 실패 보존·정정 후647개 검사/420일반·80팔·120효과픽셀/335합성착지시각 동일. 실제1.5배300초 입력6/강한착지1회 CPU접촉전후5장 직접검수,사용자 동작 “같았어”. UI진단max2.75ms/조회104.95ms는UI밖/worker대기23.38ms. 긴tick5/callback27 잔존해 전체지연통과 아님. heap live float86.49MB/dead0/간식버퍼1/완료RowBatch잔존없음,장시간누수 미검증. 덤프전 일시정지 사전고지·완료후응답확인. 다음은 비렌더갱신 공백 원인추적과 장시간 보유대조. r90 실행유지/기록종료,수락동작·형태·속도·설치본 변경없음. 아래r89 실행상태만 대체.

- **r89 실제 동작 보존 확인, 갱신 지연·장시간 보유는 미완.** [근거](docs/verification/2026-10-19-r89-effects-layout.md). 효과40프레임할당40.66MB→117.6KB,644검사/420일반·80팔·120효과픽셀/335착지합성시각동일. 실제1.5배300초 입력30건/일반착지5회,사용자 동작·속도 “같았음”. 강한착지0회로 해당 내부프레임 검수 미완. 긴tick10/max156.72ms,callback17/max156.68ms 잔존;진단callback88.99ms·worker대기68.38ms·layout flush31142회로 원인 분리 필요. managed peak190.85MiB/끝190.32MiB,후속heap live float97/98.33MB·dead0·간식버퍼1개이나 장시간 누수 통과 아님. 덤프 시 일시정지 발생은 측정 후 진단 영향이며 사용자에게 설명,향후 사전 고지. 다음은 측정 자체 지연과 실제layout 무효화/worker대기 분리·장시간 보유 및 강한착지 검증. r89 실행 유지/기록 종료,수락동작·원화·설치본 변경없음. 아래 r88 실행상태만 대체.

- **r88 반복 할당 수정·실제 동작 보존 확인, 잔여 지연/장시간 메모리 미완.** [근거](docs/verification/2026-10-19-r88-residual-allocations.md). 실제 추적으로 수면효과/몸캐시 충돌과 창identity 반복생성 확인,효과scratch·사전·윤곽weak-entry 재사용. 반복검사 할당65.68MB→66.6KB 및7.27→1.94MB,627검사/420일반·80팔 픽셀/335착지합성시각 동일. Core·원화·수락동작/속도 유지. 정상투명1.5배300초 입력11/강한착지5회,사용자 새문제없음 확인. 실제긴tick1/callback6잔존,managed peak190.36MiB/끝183.73MiB;heap dead float0/cache64재사용이나장시간누수통과아님. 다음은 motion을바꾸지않고 raster외부 긴갱신구간/장시간보유 진단. r88실행유지,전체성능/출시완료선언안함. 아래r87 실행상태는본항목으로대체하며착지수락은유지.

- **r87 공중 퍼짐 시작 — 실제 동작 사용자 수락, 성능·메모리 미완.** [실행 근거](docs/verification/2026-10-19-r87-precontact-expansion.md). 사용자 승인에 따라 접촉 직전 공중에서 기존 곡선으로 퍼지게 수정,원화/형태/속도/접촉 후 복귀 유지. Core285/앱598개,420일반·80팔 픽셀 동일. 정상투명1.5배300초 머리4/볼1입력,4강한착지 모두 공중 퍼짐 후 접촉;저장 이미지5개 직접 열람. 사용자 “일단 동작은 통과. 자연스럽다”로 이번 착지 수락. r86 보조 타이머 실패/제거와 팔 반복 배열 절감은 상세 근거 보존. 50ms초과callback2회와 managed peak209.49MiB/끝171.38MiB로 잔여지연·장시간누수 판정 미완. r87 실행 유지,다음은 수락 동작을 바꾸지 않고 잔여 할당/보유·지연 조사. 아래r85 착지 미해결은 본 수락으로 대체하며 전체 출시/환경 검증으로 확대하지 않는다.

- **r85 발 계산 절감 검증, 실제 착지 턱 걸림 미해결.** [근거](docs/verification/2026-10-19-r85-sole-band.md). 원화/속도/Core 유지,73검사·420픽셀·335합성프레임 동일. 실제300초 입력26/강한착지16회/첫66PNG고유7개 직접검수. 첫출력 간격31.48ms로r84의32.29ms와거의같고사용자걸림지속보고. 메모리peak646.78MiB후일부회수/heap확인,누수완료아님. 다음은초기단계생략/속도변경없이접촉→첫퍼짐출력경계수정과후반상호작용할당분리. 아래r84의실행상태·다음은본기록으로대체. 전체완료선언하지않음.

- **r84 앱 내부 착지 기록 구현·실제64장 확보/대조 완료.** [근거](docs/verification/2026-10-19-r84-internal-landing-frames.md). opt-in CPU bitmap recorder최대96프레임/64MiB,원화/Core/형태·속도r82동일.33검사/5배율합성저장후정상투명1.5배120초실제Body2건. 첫강한착지64장중고유픽셀6개직접열람/전체시각·해시검사,접촉→첫퍼짐CPU출력32.29ms/첫work17.24ms(sole10.13ms)확인. 다음은기존곡선/시작프레임유지하에sole/sampling비용절감·동일clip재대조. 내부진단완료이지시작지연해결/DWM검수/장시간누수완료가아니다. 현재r84실행유지/기록종료,별도사용자녹화요구대체. 아래옛실행상태는이기록으로대체한다.

- **r82 메모리/지연 실제 관측 감소, 강한 착지 시작지연 미완 / r83 철회.** [실행원장](TASKS.md#r81r82-native-memory-and-r83-impact-onset--candidate-rejected-and-reverted--2026-10-19). r81보행·대롱·볼문제없음확인,r82실제300초입력15건/긴tick1회/managedpeak157.15MiB이나장시간누수완료아님. r83은1295앱회귀/279Core/420픽셀검사뒤실제180초에서첫퍼짐47~81%생략으로사용자“속도가빨라짐”거절. 시계변경철회/Core r82동일/04:26:00기존r82정상투명1.5배복원. **바닥접촉뒤시작지연미해결**,다음은기존속도유지하에표시시작지연분리/착지녹화대조와잔여렉·메모리조사. 도구에투명창없어직접화면미검수. 아래옛실행/후보상태는본기록으로대체하며전체완료아님.

- **r80 중복 작업 수정·실제 측정, 보행 문제없음 확인 / 잔여 지연 미완.** [근거](docs/verification/2026-10-19-r80-idle-walk.md).불가능한 매달림 계산/scene 반복 복사 및8줄 worker 대기를 줄이고 최종550개 회귀/420픽셀 동일 확인. 원화/형태/속도 유지. 정상 투명1.5배300초 무조작 Idle/Walk/Sleep 측정,사용자 보행 문제없음 확인(다른 자세 안 봄). 메모리 증가 후 회수/단일표시surface/cache64 확인,긴tick32회·최대196.622ms로 전체 렉/장시간 누수 해결 아님. r80 실행 유지. 다음은 잔여 raster·worker/WPF 대기 분리 및 보행/native 할당과 장시간 보유 추적. 직접 화면/다른 자세·크기/환경은 미완 유지.

- **r79 착지 체감 개선 확인, 일반 지연/메모리 미완.** [근거](docs/verification/2026-10-19-r79-native-memory.md).정상 투명1.5배300초 실제 볼6회·머리10회 복귀/강한 착지2회. 사용자 착지 퍼짐 “빨라짐”,새 이상 없음 확인. 전체50ms초과tick22회/최대154.360ms와 managed최대259.9MiB/종료 부근171.8MiB로 전체 렉/누수 해결 아님. 원화/형태/속도 유지,r79 실행 유지. 다음은 일반Idle/Walk의 할당·보유 객체와 샘플링/대기 비용 분리 조사. 아래 준비 대기는 해제했으며 직접 화면/장시간/다른 크기·환경 검수는 미완 유지.

- **r78 실제 기본 조작 확인, 강한 착지/메모리 미완 → r79 실제 준비 대기.** [근거](TASKS.md#r78-native-measurement-and-r79-contact-storage--remaining-latency-and-memory--2026-10-19).300초 실제 볼7건·머리8건·강한 착지4회. 사용자는 기본 문제없음/높은 착지 퍼짐 조금 느림을 보고했다.26회 긴 tick과 후반 메모리 증가·일부 회수 관측. 실제 heap의 표시 surface1개 재사용은 확인했지만 전체 누수 해결 아님. r79는 접촉 계산의 배열/CWT 교체 및 진단 저장 문자열 복사를 줄였고416개 회귀/420표본 픽셀 동일/게시 DLL 동일을 확인한 후보로,원화/형태/속도 유지. 다음은 사용자 종료/준비 확인 후 실제 메모리 재측정과 강한 착지 지연 조사. r78 실행 유지,자동 교체 없음. 아래 r78 대기 상태는 이 실제 측정으로 대체하며 전체 성능·장시간·직접 시각·다른 환경 검수는 미완 유지.

- **r77 놓기 수정/체감 개선 수락, 메모리·지연 미완 → r78 대기.** [근거](TASKS.md#r77-accepted-release-fix-and-r78-memory-investigation--native-recheck-pending--2026-10-19). 실제180초 볼5건·잡기/착지7회에서 사용자 움찔 소멸/체감 개선 확인.35회 긴 tick과 메모리 증가가 남아 heap dump까지 조사했다. 보행 캐시 상한만으로 누수 없음 판정하지 않는다. r78은 표시 비트맵 반복 생성/폐기를 재사용 surface로 대체하고405개 회귀/420픽셀 동일을 확인한 후보이며 실제 비교는 사용자 “ㄱㄷ”로 대기 중이다. 원화/수락 형태/속도 유지. 다음은 준비 확인 후 native 메모리·지연 재측정,보유 경로 재대조다. 아래 r76 놓기 중첩 미완은 r77 수락으로 진전했지만 전체 성능/누수/직접 화면/다른 환경 검수는 미완 유지.

- **r76 실제 측정 — 볼 잡기 자세/체감 개선 확인,놓기 중첩·메모리 미완.** [근거](TASKS.md#r76-native-motion-and-memory--improved-but-release-conflict-remains--2026-10-19). 정상 투명1.5배120초에서 볼3건·머리9건 및 복귀 기록. 사용자는 잡기 자세 유지/끊김 감소를 확인했지만 놓기 후 일어남과 씰룩 중첩을 반려했다.50ms 초과tick10회/최대199.727ms·CPU 증가·후반 보유 메모리 증가가 남아 전체 성능/누수 통과 아님. 아래 실행 대기는 해제됐고 다음은 놓기 이중 진행과 raster 지연/자율 보행 메모리 증가 분리다. 강한 착지/다른 크기/장시간/직접 시각 검수는 미완 유지.

- **r76 볼 자세/메모리 수정 — 실제 재검증 대기, 전체 미완.** [근거](TASKS.md#r76-cheek-ordering-and-memory-retention--native-recheck-pending--2026-10-19). 엎드림 볼 캡처 전 초기화와 렌더 작업자의 폐기 객체 잔류 참조를 수정하고,목/몸/착지 배열 재사용·윤곽 캐시·측정 종료 목록 해제를 적용했다. 최종381개 회귀 통과·420픽셀 동일·동일 DLL 후보 게시,원화/형태/속도 유지. 반복 할당 감소는 확인했지만 시간 개선은 혼재하여 렉 해결 아님. 사용자 “ㄱㄷ”로 실행/측정 대기. 다음은 준비 후 실제 볼 진입과 흔들기/놓기/착지·보유 메모리 추이 검증이다. 아래 미수정 진단은 후보 코드 수정으로 진전했으며 전체 누수/성능/실제 시각 검수는 미완 유지.

- **추가 자세 결함 진단 — 엎드림 중 볼 누름 복귀, 미수정.** [근거](docs/verification/2026-10-19-r75-native-measurement.md). 사용자 질문으로 버튼 폴링/볼 MouseDown 캡처 순서를 대조했다. 폴링이 먼저면 엎드림 몸 초기화,이벤트 먼저면 유지되는 경로를 주입 검사2개로 재현. 실제 사용자 클릭의 OS 순서는 미확인. 제품은 미수정이며 수정 시 캡처 전에 자세가 초기화되지 않도록 분리해야 한다. r75 체감 개선과 별개 결함으로 유지한다.

- **r75 실제 측정 — 체감 개선 수락, 지연/CPU 미완.** [근거](TASKS.md#r75-native-measurement--perceived-improvement-with-remaining-cost--2026-10-19). 사용자 준비 후 정상 투명1.5배120초 실행,머리10건·착지 복귀10회(강한 착지2회) 기록과 사용자 “줄었음” 확인. 실제 tick 간격50ms 초과13회/최대93.783ms와 CPU/할당 증가가 남아 완전 해결 아님. 아래 준비 대기는 해제됐고 실행본 유지. 다음은 수락 형태/속도 유지하에 잔여 잡기 지연과 일반 구간의 고빈도 렌더/할당 조사다. 직접 화면/다른 크기/지속 검수는 미완 유지.

- **r73/r74 체감 개선 실패 · r75 갱신 주기 후보, 사용자 준비 대기.** [근거](TASKS.md#r75-cadence-candidate--r73-r74-stutter-rejected--2026-10-19). r74 실제120초에도50ms 초과16회,사용자는 흔들기/낙하 렉이 그대로라고 확인했다. 계산/할당 감소만으로 해결 판정하지 않는다. 처리 뒤16ms 재대기를 화면 합성 시점당1회 갱신으로 바꾼 r75는 회귀261개·추가3개/360픽셀 표본 검사를 통과했지만 실제 실행 미검증. 수락 형태/속도/원화 유지. 사용자 “ㄱㄷ”로 교체/실행/측정 중지 상태이며 준비 후 실제 입력·tick 간격·체감 대조가 다음이다. 전체 모션/성능 및 다른 크기/환경 검수는 미완 유지. 아래 r72 잔여 비용 조사 상태의 후속이며 과거 기록은 유지한다.

- **r72 실제 조작 측정 완료 — 지연 잔존, 전체 미완.** [근거](TASKS.md#r72-native-measurement--latency-remains--2026-10-19). 사용자 준비 확인 후 정상 투명1.5배60초 측정,머리 잡기4건과 네 차례 낙하/착지/평상시 상태 복귀 확인. 시작5초 제외50ms 초과7회·최대81.926ms로 성능 통과 아님. 강한 착지 복귀와 잡기 유지의 샘플링 비용이 남았다. 실제 영상/다른 크기/지속 검수는 미완. 사용자 직접 종료 확인,재실행하지 않음. 아래 후보 실제 측정 대기는 이 기록으로 대체하며 다음은 잔여 샘플링 비용 분리·개선 및 실제 재검증이다.

- **r72 렌더 비용 수정 — 실제 재검증 대기, 전체 미완.** [근거](TASKS.md#r72-render-cost-optimization--native-recheck-pending--2026-10-18). 기울어진 착지의 불필요한64회 균일 영역 샘플링과 잡기/준비 계산·임시 할당을 줄였다. 수락 원화/형태/속도 유지,5크기360표본 픽셀 동일과 회귀 검사·90쌍 저장 비교 확인. 기울어진 착지 계산은 줄었지만 실제 버벅임 해결은 미확인이다. 사용자의 준비 대기 지시로 r70을 유지하며 후보 실행/측정은 시작하지 않았다. 다음은 준비 확인 후 실제 조작·착지 복귀 지연 재측정과 화면 대조다. r71의 제품 수정 미실행은 이 후보 작업으로 대체하지만 실제 지연/전체 검수 미완은 유지한다.

- **r71 실제 모션/지연 점검 — 지연 발견, 전체 미완.** [근거](TASKS.md#r71-native-motion-and-latency-audit--latency-open--2026-10-18). 같은 수락 r70을 유지해54개 자동 검사와 정상 투명1.5배 실제 실행 기록을 확인했다. 사용자 볼2건·머리 잡기10건·매달림2회 상태 전환이 기록됐고,시작5초 제외50ms 초과 콜백8회/최대68.664ms 지연이 남았다. 화면 녹화 미확보로 시각 검수·전체 크기/모션·지속 성능은 미완. 다음은 놓기/착지 복귀·잡기 지연의 세부 비용 분리와 녹화 대조이며 이번 점검에서는 제품 코드를 수정하지 않았다.

- **r70 형태·속도 사용자 수락.** 사용자가 문제없음과 유지 지시를 확인했다. 아래 r70 수락 대기는 이 후속으로 대체한다. 같은 r70 실행 파일의 형태·속도를 고정한다. 자동 검사와 실제 OS 입력/화면·성능 근거는 분리하며 전체 완료로 확대하지 않는다.

- **r70 간식 추격 배율 보정 — 후보 실행, 실제 수락 미완.** [근거](TASKS.md#r70-proportional-snack-speed--candidate-verification--2026-10-18).185×배율 DIP/s,1배와 같은 발 주기를 적용했다. 원화·r69앉기/수면·수락 합성 유지.5크기 이동/정지 검사와 회귀67개 및75개 저장 표본 검수,동일 게시 DLL18개 재검사 후 정상 투명1.5배 r70으로 교체했다. 조작 도구에 펫 창이 없어 실제 입력/화면은 미검증이며 다음은 후보의 간식 추격·정지 체감 확인이다.50ms 초과 tick 제한과 기존 지속 성능/환경 검증은 별도 미완 유지.

- **r69 앉기 추적·수면 눈 뜸 수정 — 후보 실행, 실제 수락 미완.** [근거](TASKS.md#r69-seated-tracking-and-sleep-latch--candidate-running--2026-10-18). 수락 원화/형태를 유지하고 앉기 추적과 수면 반복 경계를 수정했다. 관련60개 자동 검사·5크기 저장 표본 검수 후 정상 투명1.5배 실행. 조작 도구에 펫이 노출되지 않아 실제 입력/화면은 미검증. 간식 속도는 전 배율185 DIP/s로 동일하고 미변경;실제 지연 측정은 별도다. 다음은 후보의 실제 앉기/수면 확인이며 기존 일반 모션·지속 성능·환경 검증은 미완 유지.

- **r68 후속 — 사용자 문제없음 보고, 정상 투명 실행 복귀.** [범위](TASKS.md#r68-native-motion-audit--capture-blocked--2026-10-18). 사용자 확인을 에이전트 실제 검수나 모든 크기/장시간 통과로 확대하지 않는다. 수락 형태는 유지하며 다음은 일반 모션·수면/깨우기·간식·몸 클릭과 상태별 잡기 경계, 이후 크기별/지속 실행 점검이다. 설치·배포 변경 없음.

- **r68 실제 모션 묶음 검수 — 화면 캡처 차단, 미완.** [실행 근거](TASKS.md#r68-native-motion-audit--capture-blocked--2026-10-18). 동일 r67 불투명 진단 창을 찾았지만 최초/복구 캡처 모두 타임아웃됐다. 사용자 중지와 현재 모니터 재개 범위를 준수했고 실제 포인터 입력은 하지 않았다. 수락 형태·코드·설정·설치본 유지. 다음은 캡처 복구 또는 실제 조작 녹화 확보 후 호버 종료·잡기/놓기/착지·매달리기 검수이며 정상 투명 복귀도 남아 있다. 실제 동작 통과나 전체 완료가 아니다.

- **r67 눈 전체 추적 — 사용자 수락.** 사용자가 수정본에 "됐다"고 확인했다. [수락 범위와 근거](TASKS.md#r67-whole-eye-tracking--candidate-running-native-acceptance-open--2026-10-18). 이번 눈 추적 형태는 유지한다. 다음 체크는 호버 종료 복귀/지연, 잡기·기울어진 놓기·강한 착지, 창/작업표시줄 준비·매달림·해제다. 전체 크기/입력 조합, 전체 모션·성능 및 에이전트 실제 OS 조작 검증까지 수락으로 확대하지 않는다. 아래 r67 후보 수락 대기는 이 후속으로 대체되며 당시 검사 기록은 유지한다.

- **r67 눈 전체 추적 — 후보 실행, 실제 수락 미완.** [최신 근거](TASKS.md#r67-whole-eye-tracking--candidate-running-native-acceptance-open--2026-10-18). r66의 고정 눈 테두리는 사용자 요구와 달라 반려됐다. 원본 눈 윤곽까지 방향 이동시키고 볼 피부 복구와의 경계를 보완했다. 관련66개·회귀46개 검사 및 크기별 저장 시트 대조를 수행했다. 동일 게시 DLL 핵심31개 재검사 후 정상 투명1.5배 Release로 교체했으나 실제 OS 마우스 검증은 미완이다. 다음은 수정본 추적→볼 당김→놓기의 실제 확인이다. 원화·입 수정·몸/볼 형태·설치본 유지 및 전체 모션/성능 미완은 그대로다. 아래 r66/r65 고정 윤곽 기준은 이 후속으로 대체된다.

- **r66 입 잔선·눈 아래 중복 선 — 후보 실행, 실제 수락 미완.** [최신 원인·검증·한계](TASKS.md#r66-mouth-and-lower-eye-line--candidate-running-native-acceptance-open--2026-10-18). r65의 두 결함을 사용자 피드백으로 반려했다. 입 보호 분기 충돌과 테두리를 끌어오는 눈 색상 샘플링을 수정하고 관련91개 검사·저장 프레임 대조 후 정상 투명1.5배 Release로 교체했다. 원화·몸·다리·볼 형태·설치본은 유지한다. 실제 마우스 검증은 조작 창 미노출로 미완이다. 다음은 수정본의 추적→볼 당김→놓기 실제 확인이며 전체 모션·성능·설치/출시 완료가 아니다.

- **r65 눈 테두리 고정 — 후보 실행, 실제 수락 미완.** [최신 검증·실패·한계](TASKS.md#r65-fixed-eye-rim--candidate-running-native-acceptance-open--2026-10-18). r63 볼 흰 틈과 r64 눈 테두리 누락은 사용자 피드백으로 반려됐다. 눈 테두리는 원본 위치에 두고 내부만 이동시키며 볼 합성과 방향 복귀 수정은 유지한다. 관련85개 검사와 크기별 저장 표본 열람 후 정상 투명 Release1.5배로 교체했다. 실제 마우스 조작은 도구에 창이 노출되지 않아 미검증이다. 다음은 이 후보의 실제 추적→볼 당김→놓기 확인이다. 원화·몸·뒷다리는 미변경이고 뒷다리는 조사만 했으며 전체 모션·성능·설치/출시 미완은 유지한다. 아래 r62 미수정 상태의 후속은 이 항목이다.

- **r62 방향 고정·볼 잔선 — 결함 재현, 미수정.** [원인과 미검증 경계](TASKS.md#r62-facing-and-cheek-defects--diagnosed-not-fixed--2026-10-18). 머리 press 후 Idle 방향 잠금과 새 눈 합성이 볼 피부 복구 좌표에 적용되는 문제를 분리해 재현했다. 다음은 정상 추적 방향 소유권 및 눈/피부 복구 분리 보완, 외곽선 추가 대조다. 수락 원화·몸·볼 형태 유지와 기존 전체 모션·지연 미완은 그대로다.

- **r61 눈 평행 이동 수정 — 정상 실행 교체, 실제 확인 대기.** [반려 원인과 후속 근거](TASKS.md#r61-rigid-gaze--candidate-replacement-pending--2026-10-18). r60은 눈을 변형해 사용자 반려됐다. 원본 눈의 동일 이동과 고정 머리 가림으로 바꾸고 저장 표본·참조 렌더를 대조했다. 사용자 종료 확인 후 새 Release를 정상 투명1.5배로 실행했다. 프로세스 실행 확인은 실제 조작 통과가 아니다. 다음은 이 실행본의 실제 시선/복귀 확인이다. 원화·볼·몸 보존과 전체 모션·지연 미완은 유지한다.

- **r60 눈 추적 영역 분리 — 저장 검수, 실제 확인 대기.** [변경과 검증 경계](TASKS.md#r60-gaze-isolation--native-confirmation-pending--2026-10-18). 수락 원화·볼·몸을 유지하고 눈 내부 시선만 움직이게 제한했다. 크기별 저장 표본은 직접 열람했으나 투명 앱 창이 조작 도구에 노출되지 않아 실제 조작은 확인 대기다. 정상 투명 Release1.5배 실행으로 이전 진단 창을 교체했다. 다음은 실제 추적/복귀 결과 확인이며 전체 모션·버벅임·설치/배포 미완은 유지한다.

- **r59 추적 시 눈 옆 머리 이동 — 원인 조사, 미수정.** [근거와 검증 경계](TASKS.md#r59-gaze-hair-contamination--diagnosed-not-fixed--2026-10-18). 추적 눈의 넓은 변형 영역에 머리 경계가 포함되는 코드 경로를 확인했다. 다음은 눈 선택을 국소화한 뒤 크기별 진입/복귀·실제 조작 대조다. 수락한 r57 형태·원화는 변경하지 않았으며 전체 모션과 버벅임 미완은 유지한다.

- **r57 팔/볼 경계 및 몸 선 수정 — 사용자 수락.** [원인·검수·수락 범위](TASKS.md#r57-contour-feedback--candidate-native-verification-pending--2026-10-18). r57 Release 실행 후 사용자가 수정 사항에 문제가 없다고 확인했다. 팔 경계·볼 당김/반동·확대 시 몸 선 보정을 수락 범위로 기록한다. 원화·얼굴/머리 선은 유지한다. 다음은 기존 전체 모션 정합·지연 및 붙기 세부 조건 등 미완 항목 점검이다. 이번 수락을 전체 완료로 확대하지 않으며 설치·배포는 진행하지 않았다.

- **r56 반복 렌더 축소·실제 피드백 수정 후보 — 전체 미완.** [실행·검증·미확정 원인](TASKS.md#r56-performance-and-native-feedback--incomplete--2026-10-18). 첫 Release에서 사용자가 호버 종료 끊김·준비 팔 누락·작업표시줄 접근 실패를 보고했다. 후속 호버 복귀/팔 표시/등 선 후보와 관련 검사·저장 표본 확인까지 진행했다. 실제 작업표시줄 실패 원인과 후속 사용자 조작 수락, 큰 크기의 느린 tick은 남는다. 다음은 후속 Release 실제 조작·위치/발판 로그 대조와 남은 상태 정합이다. 원화·완료 기준은 유지하고 설치본은 교체하지 않는다. 아래 r55의 전체 미완을 해소한 것이 아니다.

- **r55 상태 연결 후보·버벅임 확인 — 전체 미완, 설치 교체 안 함.** [실행·성능·미검증 경계](TASKS.md#r55-runtime-continuation-and-performance-diagnosis--incomplete--2026-10-18). 수면/간식/매달리기/몸 클릭 연결을 추가하던 중 사용자 버벅임 지적으로 성능 진단을 우선했다. 계산량을 줄였지만 실제 루프의 느린 착지 구간과 일부 상태 연결·윤곽 대조·실제 마우스 검증은 남는다. 다음은 착지 반복 렌더 축소와 지연 재검사 후 누락된 상태 경계 및 Release 실제 조작 검증이다. 원화·동작/화질 완료 기준은 유지하며 아래 r54 미완을 전부 해소한 것이 아니다.

- **r54 앞발·일반 모션 보완 — 일부 수정, 전체 미완/교체 안 함.** [변경·검수·남은 결함](TASKS.md#r54-partial-render-repair--not-accepted--2026-10-18). 사냥 앞발/배/목 연결과 걷기·앉기·일어서기의 고해상도 경로를 보완했다. 볼은 여전히 시각 불합격이며 수면·간식·매달리기 등 구형 경로, 몸 클릭 누락, 일반 모션에서 잡기/놓기 전환, 실제 입력·성능 검증은 남는다. 다음은 이 결함과 상태 경계를 바로잡은 뒤 실제 앱 확인이다. 원화·기존 동작/화질 완료 기준은 유지한다. 실행본/설치/출시를 교체하지 않았다. 아래 r53 미완 중 앞발 및 일반 이동 경로만 저장 표본·코드 범위에서 진전했다.

- **r53 호버·추적·볼 연결 — 사용자 결함 지적, 후보 불합격.** [실행·검수 실패·누락 원인·다음](TASKS.md#r53-hover-tracking-and-cheek-connection--rejected--2026-10-18). 호버/추적과 볼 입력을 연결했으나 볼 형태·사냥 앞발·구형 이미지 전환이 남는다. 기존 볼의 눈/입 반응과 사냥 앞발 보정/목 연결을 모두 이식하지 않은 것이 확인됐다. 자동 검사와 저장 표본 열람을 시각 통과로 보지 않는다. 다음은 이 누락 및 남은 상태별 전환 교정 후 같은 시점 원본 대조·실제 조작 검증이다. 원화 보존·기존 동작/화질 완료 기준은 유지하며 실제 마우스 검증, 전체 교체·설치·출시는 미완료다. 아래 r52의 비머리 입력 미완은 일부 진전했지만 해소되지 않았다.

- **r52 잡기·놓기 묶음 보완 — 최종 관련 검사 통과, 전체 통합 미완.** [실행·실패·직접 검수·실제 실행 경계](TASKS.md#r52-runtime-grab-batch-validation--2026-10-18). 재잡기 무시·자세 초기화·잡기점 소수 좌표 손실을 수정하고 5개 크기를 검사했다. 원화와 기존 완료 기준은 유지한다. 테스트 실행본은 켰지만 조작 도구에서 창을 찾지 못해 실제 마우스 검증은 미완이다. 지연 급증·다른 상호작용/모션 연결도 남는다. 다음은 이 경계 보완과 실제 데스크톱 검증이며, 설치·전체 교체·출시 완료가 아니다. 아래 r51의 재잡기/잡기점·검사 범위는 이 후속 기록으로 진전했다.

- **r51 실제 루프 연결 — 부분 구현, 출시 불가.** [실행·직접 검수·성능 문제·남은 경계](TASKS.md#r51-actual-loop-grab-release-and-dynamic-platforms--2026-10-18). Windows 검사 옵션에서 잡기·놓기·이동/제거 발판을 연결했다. 기존 완료 기준과 원화는 유지한다. 저장 표본 검수와 합성 입력 기반 루프 검사는 진전했지만, 새 자세 생성 지연·다른 상호작용 경계·실제 창/마우스 검증은 미완이다. 다음은 지연 보완과 넓은 창 여백/잡기점 검사 후 실제 데스크톱 확인. 기본 실행·설치·Mac·전체 이식 완료가 아니다.

- **r50 낙하 높이·후행 움직임 — 미리보기 연결 및 저장 표본 검수.** [실행·직접 대조·한계](TASKS.md#r50-height-and-trailing-motion--2026-10-18). r49의 높이·후행 미완은 고정 바닥 표본에서 진전했다. 현재 Windows 경로는 짧은 낙하 보조가 아니라 발판 물리임을 정정했다. 원화와 기존 완료 기준은 유지한다. 다음은 사용자 시각 확인과 실제 런타임의 동적 발판·잡기점·입력 연결 검증이다. 앱/설치 적용, 연속 재생과 전체 이식·다른 상호작용 미완은 남는다.

- **r49 발 선·몸 복귀 시간 보완 — 저장 프레임 검수, 사용자 확인 대기.** [실행·직접 대조·한계](TASKS.md#r49-paw-sampling-and-recovery-clock--2026-10-18). r48의1배 발 선 끊김을 줄이고 기존 복귀 시간 배분을 맞췄다. 원화 보존과 전체 동작/화질 기준은 유지한다. 아래 r48의 두 미완은 이번 저장 표본 범위에서 진전했으며 완전한 픽셀 일치나 실시간 통과가 아니다. 다음은 결과 확인 후 기존 실제 높이·후행 움직임·런타임 연결 순서다. 앱/설치·전체 이식·다른 상호작용 미완은 남는다.

- **r48 실제 착지 렌더 대조·잡기 연결 — 미리보기 부분 완료.** [실행·직접 검수·남은 차이](TASKS.md#r48-legacy-landing-and-anchored-swing--2026-10-17). 발바닥 기준 압축과 잡기 관성·잡기점·기울어진 놓기를 연결했다. r47의 실제 렌더 대조 미완은 저장 WPF 표본 범위에서 해소했다. 다만1배 강한 착지의 가는 발 선 출력과 기존 몸 복귀 타이밍 차이는 남아 있어 전체 통과가 아니다. 다음은 이 두 차이 보완 후 기존 실제 높이·후행 움직임·런타임 연결 순서다. 원화 보존·전체 동작/화질 완료 기준과 앱/설치·다른 상호작용 미완은 유지한다.

- **r47 기존 기능 감사·착지 부분 복원 — 전체 이식 미완료.** [누락/변경 대조·실행·직접 관찰·다음](TASKS.md#r47-legacy-motion-audit-and-landing-restoration--2026-10-17). r45/r46의 압축 누락과 임의 타이밍은 기존 기능 보존 실패다. 중립 일반/부분 놓기의 압축·복귀와 기존 시간 수치를 복원했다. 강한 착지는 수치 복원까지만 확인했고 발 퍼짐 외형은 추가 대조가 필요하다. 다음은 기존 실제 렌더와 대조 후 남은 관성·잡기점·기울어진 놓기·실제 높이·후행 움직임 연결이다. 전체 동작 보존 기준과 다른 상호작용·양 플랫폼 통합·앱/설치 미완은 유지한다. 아래 r45는 당시 기록이며 기능 보존 판단은 이 후속으로 정정한다.

- **r45 놓기·짧은 착지 — 중립 경로 미리보기 준비.** [실행·직접 검수·미완](TASKS.md#r45-neutral-release-and-landing--2026-10-17). 수락한 r44 형태를 유지하고 역방향 자세 복귀와 위치 반동을 연결했다. r44 놓기/착지 미완은 이 중립 경로만 진전했다. 다음은 시각 확인 후 기울어진 채 놓기·강한 착지 등 남은 연결이다. 전체 동작·화질 기준, 다른 표정/상호작용, 실제 조작·양 플랫폼 통합과 앱/설치 미완은 유지한다.

- **r44 평상시 마스크 정합·잡기 기울임 연결 — 검수용 미리보기 준비.** [실행·직접 검수·제한](TASKS.md#r44-matched-idle-and-held-tilt-connection--2026-10-17). 승인된 얼굴/머리 끝/몸·발을 유지하고 평상시→8장 기반 잡기→기울임을 연결했다. r43 머리 마스크 미완은 이 뜬눈 중립 경로에서 해소했다. 다음은 시각 확인 후 다른 공통 모션·표정의 마스크 정합과 남은 놓기/착지 등 기존 연결 순서다. 전체 교체·실제 조작·양 플랫폼 통합과 앱/설치 적용은 미완료이며 기준은 바꾸지 않는다.

- **r43 원본 중간 자세 교정·진입 연결 — 미리보기, 전체 완료 아님.** [실행·대조·남은 경계](TASKS.md#r43-source-contour-correction-and-boundary-preview--2026-10-17). 02~07 몸/발을 다시 맞추고 8장 대조 및 크기별 저장 전환 표본을 직접 확인했다. 같은 승인 머리로 합성한 양끝 몸은 일치한다. 구형 평상시의 머리 파츠 차이는 남아 있으므로 다음은 시각 확인과 공통 평상시 머리 마스크 정합, 이후 수락 잡기 기울임 연결이다. 아래 r40의 “양끝만 남음” 해석은 이번 교정으로 대체한다. 앱·설치본 미적용, 전체 교체·이전 미완과 완료 기준은 유지한다.

- **r40 원본8장 기반 동적 윤곽 미리보기 — 부분 구현.** [실행·171개 표본 직접 관찰·남은 경계](TASKS.md#r40-eight-source-body-entry-preview--2026-10-17). 사용자 지시로 기존01~08 전부를 다시 대조해 앞발/뒤 몸 윤곽을 구성하고 좌표로 연결했다. 이전 반투명 혼합은 쓰지 않는다. 원화/앱은 미수정이다. 다음은 이8개 자세를 유지하면서 기존 평상시·수락 잡기 자세와 시작/끝 윤곽을 맞추는 작업이다. 현재 양끝은 동일하지 않아 앱 연결·전체 완료가 아니며, 사용자 시각 확인과 이전 미완은 남는다.

- **r39 시작·끝 보존 확인 / 중간 전환 불합격.** [실행·직접 관찰·다음 경계](TASKS.md#r39-fixed-endpoint-transport-rejected--2026-10-17). 기존 시작과 수락 끝은 저장 픽셀까지 같지만, 중간에 반투명 뒷발·겹선이 생긴다. 몸 전체 변형/혼합은 채택하지 않는다. 앞/뒤 분리 진단도 정지 재합치기만 확인했으며 움직일 수 있는 닫힌 파츠는 아니다. 다음은 원래 몸 파츠 기반의 가려진 면·앞발 선 소유권 구성 후 중간 가림 검증이다. 전환·앱 미연결이며 기존 미완은 유지한다.

- **r38 평상시→잡기 후보 — 자체 검수 탈락·재작성 방식 중단.** [실행/실패/대안 경계](TASKS.md#r38-idle-to-grab-entry-candidate-rejected-in-self-review--2026-10-17). 원본 중간 몸·발을 따라 구성했지만 기존 시작/수락 끝과 출력이 다르고 뒷발 사이 각짐이 남았다. 연결 완료가 아니며 원화·앱 미수정이다. 다음은 기존 시작·끝 몸 레이어를 고정하는 대안으로 진입 경계를 먼저 맞추는 작업이다. 대안은 아직 미구현/미검증이며 전체 전환과 이전 미완은 유지한다.

- **r37 잡힌 자세 내 기울임 연결 — 부분 미리보기.** [실행·검수·미완 경계](TASKS.md#r37-held-pose-tilt-transition-preview--2026-10-17). 후속 지시로 중립→좌우 기울임→중립만 연결했다. 수락한 얼굴/몸/머리 끝 원화는 유지한다. 아래 r36의 지시 대기는 이 범위만 해소했다. 평상시→잡기는 반려된 몸 변형이 남아 미연결이며, 다음은 원본01~08에 맞춘 중간 몸·발 윤곽 교체다. 전체 전환·놓기/착지·앱 검증과 기존 미완은 남는다.

- **r36 기울임 미리보기 — 사용자 수락.** [사용자 확인과 승인 경계](TASKS.md#r36-tilt-preview-accepted--2026-10-17). 제시한 연결부/머리 끝 기울임 결과의 확인 대기를 해소했다. 얼굴/몸/머리 끝 원화와 합성 코드는 유지한다. 다음은 후속 연결 지시 대기이며, 기존 전환 미연결 지시는 그대로다. 전체 모션·실제 앱 검증 및 기존 미완은 남고, 에이전트의 실시간 관찰 여부나 기록된 미세 색 차이는 바꾸지 않는다.

- **r35 왼쪽 연결부 가림 — 정지 표본 검수, 사용자 확인 대기.** [원화 보존·63개 직접 관찰·기술 검사·미검증 경계](docs/verification/2026-10-17-continuous-scale-review.md#r35-left-attachment-occlusion). 얼굴/몸 원화는 유지하고 머리 뒤 국소 가림을 추가했다. r34 파임은 확인한 크기/각도 표본에서 보완했다. 다음은 결과 확인/필요한 국소 보완이며 전환은 계속 미연결. 연속 재생·다른 표정·앱과 기존 미완 항목은 남는다.

- **r34 크기별 회전 검수 — 전체 회전 표본 이상 미발견 / 머리 단독 연결 보완 필요.** [54개 전체 회전·15개 머리 단독 진단·직접 관찰](docs/verification/2026-10-17-continuous-scale-review.md#r34-scale-and-rotation-inspection). 머리 단독+16도에서 왼쪽 몸 윗선이 드러나 각진다. 원본 아트는 미수정이다. 다음은 얼굴/몸 원화를 유지하는 접합 가림/회전 중심 방안 검토이며 전환 연결은 계속 보류한다. 연속 중간값·실제 재생·앱은 미검증.

- **r33 왼쪽 머리 아래 검은 돌출 제거 — 정지 후보 확인 대기.** [실제 수정4픽셀·흰/어두운 배경 대조·보호 근거](docs/verification/2026-10-17-continuous-scale-review.md#r33-left-dangling-ink-removal). 사용자 후속 승인으로 해당 돌출만 제거했다. 나머지 머리/얼굴/몸 출력은 유지하며 r32 보고에서 놓친 잔여 결함을 정정했다. 다음은 정지 후보 확인/보완이다. 전환·앱 연결은 하지 않으며 다른 각도/배율과 기존 미완 항목은 남는다.

- **r32 바깥 흰 여백·정지 접합부 보완 — 사용자 확인 대기.** [흰/어두운 배경 직접 대조·보호 검사·한계](docs/verification/2026-10-17-continuous-scale-review.md#r32-exterior-white-mask-and-still-junction-correction). 얼굴/머리의 비흰색 원본과 기존 선 설정을 유지하고 외부 흰 여백 및 오른쪽 몸 연결점을 정리했다. r31 접합 미해결은 이 정지 범위만 진전했다. 다음은 정지 후보 확인/보완이며 전환은 사용자 지시대로 아직 연결하지 않는다. 앱·다른 각도/배율·전체 모션 미검증.

- **r31 뒷발 사이 부분 수정·머리 접합부 미해결.** [저장 비교·보존 검사·남은 원인](docs/verification/2026-10-17-continuous-scale-review.md#r31-hind-foot-correction-and-unresolved-junctions). 머리/얼굴 출력은 유지하고 뒷발 곡선만 보완했다. 머리 파츠의 흰 여백이 몸 선을 덮는 문제가 남아 시각 통과가 아니다. 다음은 실제 머리색/선을 보호하는 외부 여백 마스크 검토와 정지 비교 보완이다. 전환·앱 연결은 계속 보류한다.

- **r30 원본 잡기 몸·발 정지 후보 — 비교 대기, 통과 아님.** [원본 좌우 비교·머리 보존·남은 단면 차이](docs/verification/2026-10-17-continuous-scale-review.md#r30-original-grab-contour-still-candidate). 승인된 얼굴/머리 끝을 고정하고 원본08의 몸·발 윤곽을 별도로 구성했다. 다음은 정지 비교의 확인/보완이다. 전환은 확인 전까지 연결하지 않으며 앱·설치본 미적용이다. r29 반려/보고 실패와 나머지 전체 교체 항목은 유지한다.

- **r29 사용자 반려·허위/과장 검수 보고 정정 — 시각 불합격, 아트 미수정.** [사고 보고·원본 8키 대조·프레임 재점검](docs/verification/2026-10-17-continuous-scale-review.md#r29-rejection-and-reporting-incident-audit). 아래 r29의 검수 완료에 따른 진전 판단은 철회한다. 다음은 승인된 얼굴/머리 끝을 고정하고 원본 잡기 몸·발 정지 윤곽부터 교정하는 작업이다. 기존 전체 동작/화질 기준은 유지하며, 이 후보의 추가 확장·앱 적용은 진행하지 않는다. 강한 착지 런타임 대조와 이전 전체 보고 감사는 미완료다.

- **r29 잡기·흔들림·놓기·강한 착지 미리보기 연결/검수.** [프레임 검수·근거·회전 제한](docs/verification/2026-10-17-continuous-scale-review.md#r29-direct-grab-and-landing-preview). 전체 동작·화질 일치라는 완료 기준은 유지하며 부분 진행이다. 다음은 가장자리 매달리기·볼/앞발·몸 당기기·중간 표정과 잡기 기준점/머리카락 지연 연결, 이후 Windows/Mac 통합이다. 앱·설치본·슬라이더 미적용이며 사용자 시각 수락과 실제 조작 검증은 남아 있다. 아래 r28 미완 목록은 이 범위만 진전했다.

- **r27 사용자 수락 · r28 공통 모션 연결/검수, 전체 교체 진행 중.** 승인된 머리 끝·선과 얼굴/몸 파츠를 유지해 공통 모션을 연결하고, r23 눕기 전환·몸 클릭·간식 효과를 추가했다. [928프레임 직접 검수·검사·미완 경계](docs/verification/2026-10-17-continuous-scale-review.md#r28-accepted-head-connected-to-common-motions). 아래 r27 수락 대기는 해소했다. 다음은 기존 순서의 잡기/매달리기·볼/앞발·강한 착지·표정 연결, 이후 Windows/Mac 통합이다. 모든 동작의 외형/화질 일치라는 완료 기준은 유지하며 아직 전체 교체 완료가 아니다. 앱·설치본·슬라이더 미적용이다.

- **r27 머리 끝 연결부 — 선 색·굵기 보정, 시각 확인 대기.** r26의 회색·가늘어진 연결부 지적으로 해당 시각 통과를 철회했다. 뾰족한 끝은 유지하고 원화의 짙은 선과 연결했다. [대조·검사 범위](docs/verification/2026-10-17-continuous-scale-review.md#r27-endpoint-ink-and-junction). 얼굴·몸은 보존하며 다음은 사용자 확인 후 기존 남은 모션/전환 연결이다. 앱·설치본·슬라이더 미적용이다.

- **r26 머리 끝 — 뾰족한 형태로 국소 수정.** 사용자가 다른 머리 끝처럼 뾰족하게 마무리하도록 승인해 아래 r25의 형태 결정 대기를 해소했다. 얼굴·몸은 고정하고 끝점만 수정했다. [비교·검수 범위](docs/verification/2026-10-17-continuous-scale-review.md#r26-pointed-endpoint). 다음은 사용자 시각 확인 후 기존 눕기 전환·남은 모션 연결이다. 앱·설치본·슬라이더 미적용, 전체 교체 미완료다.

- **r25 끝점 후속 — 원화 자체의 각진 끝 형태 확인, 추가 형태 수정 판단 대기.** 사용자가 끝부분 잘림을 지적해 원화와 재조립 끝점을 직접 비교했다. 진한 선·분홍색은 동일하며 원화에도 같은 평평한 끝이 있다. [비교·밝은 배경 차이·승인 경계](docs/verification/2026-10-17-continuous-scale-review.md#r25-endpoint-follow-up-original-shape-versus-clipping). 다음은 원본 복원을 유지할지 끝점만 둥글게 다듬을지 확인이다. 추가 아트 수정·앱 적용은 하지 않았다.

- **옆머리 절단 경계 수정 — r25 원화 대조·회전 검수 완료, 사용자 확인 대기.** 원래 선과 주변 픽셀을 그대로 포함하도록 절단 범위만 보완했다. 얼굴·몸·원래 선의 색과 두께는 보존하며 r24 농도 보정은 이번 후보에 사용하지 않는다. [원화 대조·288프레임 직접 검수·5개 검사](docs/verification/2026-10-17-continuous-scale-review.md#r25-original-hair-tip-boundary-restoration). 다음은 복구본 시각 확인 후 기존 눕기 전환·상호작용 교체 순서다. 앱·설치본·슬라이더는 미적용이다.

- **r24 옆머리 끝 결함 확인 — 시각 통과 철회.** 사용자 확대 이미지와 원화/조립본/실제 머리 파츠 대조로 절단 경계가 원래 선을 누락하는 것을 확인했다. [직접 비교·정정](docs/verification/2026-10-17-continuous-scale-review.md#r24-rejected-source-hair-tip-clipping). 농도 보정으로 해결되지 않는 결함이며 다음은 원래 옆머리 선을 포함하도록 절단 경계를 교정하는 작업이다. 얼굴·몸 보존, 이후 크기/회전 재검수 조건은 유지한다.

- **머리 외곽선 보완 — r24 미리보기·크기별 회전 검수 완료, 사용자 확인 대기.** 얼굴·몸 원본은 보존하고 머리 아래 기존 선의 출력 농도만 제한적으로 보정했다. [수정 경계·288프레임 검수·5개 검사](docs/verification/2026-10-17-continuous-scale-review.md#r24-source-conditioned-head-outline-recovery). 아래 미수정 진단은 이 후속으로 대체한다. 다음은 r24 시각 확인 후 기존 눕기 전환·상호작용 교체 순서다. 실제 앱·설치본·슬라이더는 미적용이며 모든 배율·각도의 무결점을 뜻하지 않는다.

- **연속 확대 검수 보완 — 머리 외곽선 가독성 미통과.** 사용자가 비교본의 머리 선 소실을 지적했다. 원화에는 선이 있지만 작은 크기의 회전 출력에서 턱·옆머리의 얇은 선이 약해지는 것을 확대 대조했다. [진단·미수정 경계](docs/verification/2026-10-17-continuous-scale-review.md#follow-up-head-outline-rejection). 아래 조사는 가능성 확인이지 화질 통과가 아니다. 다음은 얼굴 형태를 보존한 얇은 외곽선 출력 보완·재검수이며 슬라이더는 미착수다. 앞발 수락과 기존 미완 항목은 유지한다.

- **r23 눕기 정지 자세 사용자 수락 · 연속 크기 조절 조사 완료.** 아래 r23 확인 대기는 사용자의 “이건 해결”로 해소했다. 전환 연결·나머지 상호작용 교체·앱 통합은 아직 미완료다. [1.0~1.5배 중간 크기 비교와 DPI 한계](docs/verification/2026-10-17-continuous-scale-review.md). 연속 슬라이더는 가능하지만 이번에는 진단만 했고 기능·설치본은 변경하지 않았다. 다음은 수락된 눕기 자세의 전환 연결과 기존 전체 교체 순서다. 슬라이더 구현은 후속 지시 전까지 미착수다.

- **눕기 앞발 구분 후속 — r23 정지 비교, 사용자 확인 대기.** r22에서 앞발 경계가 턱 아래에서 끊기는 문제를 확대 비교로 확인했다. 머리 파츠에 포함된 흰 몸 여백 위로 앞발 경계만 표시하도록 정지 합성 순서를 수정했다. [원인·국소 변경·보존 경계](docs/verification/2026-10-16-high-resolution-art-migration.md#r23-forepad-divider-and-head-skirt--2026-10-17). 다음은 앞발 확대 비교 확인이며, 전환 연결은 계속 보류한다. 얼굴 원본·뒷몸·달리기는 변경하지 않았다.

- **눕기 원본 윤곽 우선 — r22 정지 비교본, 전환 연결 보류.** 원본과 재대조한 결과 r21도 앞발·배·뒷다리 형태가 달라 시각 수락 대상에서 제외했다. 사용자 요청에 따라 얼굴·달리기를 보존하고 원본 몸/발 윤곽을 별도 정지 자세로 구성했다. [비교·보존 근거](docs/verification/2026-10-16-high-resolution-art-migration.md#r22-source-contour-still-before-transitions--2026-10-17). 다음은 원본과 나란히 자세 확인이며, 확인 전 전환·앱에는 연결하지 않는다. 아래 r21은 당시 기록이다.

- **눕기 시각 검수 후속 — r21 수정·검사, 사용자 확인 대기.** r20의 긴 앞발·뒷다리 돌출·늦게 진해지는 선을 재검수에서 확인하고, 사용자 승인으로 눕기/일어나기만 수정했다. 원본의 짧고 둥근 앞발과 접힌 뒷다리 기준으로 교정했으며 얼굴·달리기는 보존했다. [프레임별 검수·검사·한계](docs/verification/2026-10-16-high-resolution-art-migration.md#r21-rounded-sleep-pads-and-continuous-ink--2026-10-17). 다음은 수정 자세의 사용자 확인, 이후 기존 직접 조작 연결이다. 앱 적용·전체 아트 교체는 미완료다. 아래 r20은 당시 기록이며 이 후속이 최신 상태다.

- **최신 시각 결함 후속 — r19 눕기 거부, r20 원본 자세 기준 재수정.** 앞발과 뒷다리 접힌 선이 사라지는 결함을 사용자가 지적했다. 얼굴·달리기는 유지하며 눕기/깨우기만 다시 구성했다. [원인·수정·검수 경계](docs/verification/2026-10-16-high-resolution-art-migration.md#r20-original-reference-sleep-pads-and-fold--2026-10-17). 다음은 이 자세의 사용자 확인과 기존에 남은 직접 조작 연결이다. 앞선 r19 검수는 눕기 형태의 적합성을 입증하지 못했으며, 전체 교체·실제 앱 적용은 여전히 미완료다.

- **확대 아트 최신 상태 — r18 사용자 수락, r19 공통 모션·전환 연결 진행.** 아래 r18 수락 대기는 사용자의 “이걸로 나머지 모션도 연결” 요청으로 해소했다. 평상시·추적·걷기/달리기·앉기/일어나기·사냥/점프·먹기/식후 반응·눕기/깨우기를 같은 연결 구조로 구성했다. [실행·검수 범위](docs/verification/2026-10-16-high-resolution-art-migration.md#r19-shared-motion-and-transition-preview--2026-10-17). 다음은 잡기·매달리기·볼/앞발·몸 클릭·강한 착지 및 표정/간식 효과 연결, 이후 Windows/Mac 합성·판정 통합이다. 이번 결과는 미리보기이며 전체 교체·앱 적용·사용자 시각 수락은 아직 미완료다. 기존 순서·완료 기준은 유지한다.

- **확대 화질 — 원화 파츠 기반 전체 아트 교체 승인·진행 중.** 1.5배 원화 고정과 작은 원본 자세의 파츠 재조립을 승인했다. r15 정지 합성본 수락 후 r16을 모션에 연결했으나, 사용자가 앉은 자세의 혹과 달릴 때 짧아지는 다리·느린 속도를 지적했다. 이 범위는 최신 승인에 따라 변경하며, r17에서 앉기 윤곽·앞으로 뻗는 달리기·빠른 박자를 수정해 영향 프레임을 검수했다. [범위·실행·전체 교체표](docs/verification/2026-10-16-high-resolution-art-migration.md). 순서는 공통 원본 교정 → 모든 자세/표정 재조립 → Windows/Mac 합성·판정 검증 → 사용자 확인이다. 완료 기준은 모든 동작의 외형·화질 일치와 승인 변경 밖의 기존 모션·타이밍·접촉·조작 보존. 다음은 r17 모션 확인, 나머지 표정·모션 교체와 양 플랫폼 통합이다. 직접 검수·자동 검사 통과는 사용자 수락이 아니다. 전체 교체·앱 적용은 미완료이며 설치본과 다음 부팅 확인 대기는 그대로다.

- **위 아트 항목의 후속 상태 — r17 달리기 혹 지적으로 재수정.** r17 확인 대기는 r18로 대체한다. 사용자 승인에 따라 얼굴·접힌 발 모양은 보존하고 엉덩이와 다리의 연결부만 수정했다. r18의168프레임 직접 검수와 관련10개 검사는 완료, 사용자 시각 수락과 실제 앱 연결은 미완료다. 다음은 r18 확인 후 기존 전체 교체 순서로 진행한다. [원인·실패 시도·보존 근거](docs/verification/2026-10-16-high-resolution-art-migration.md#r18-fixed-rump-and-rigid-folded-hind-paw--2026-10-17). 이전 r17의 혹 없음 판단은 철회하며 완료 기준·제품 범위는 바꾸지 않는다.

- **Windows 자동 실행 최적화 — 로컬 설치 교체·실행 검사 완료, 다음 로그인 대기.** 자동 실행만 20초 대기, 수동 실행 즉시 시작·대기 취소, 무손실 모션 지연 로딩. [현재 실행·남은 항목](TASKS.md#windows-login-delay-and-on-demand-motion--2026-10-16). 완료 기준은 기존 화질·수동 실행 보존과 패키지·설치·다음 로그인 확인이다. 소스·픽셀·패키지·설치 경로 검사를 통과하고 설정을 보존한 candidate16을 실행했다. 다음은 평소 사용 수락과 다음 정상 로그인/부팅 영향 확인. 재부팅·공개 교체·커밋/푸시는 하지 않았으며 확대는 미구현이다.

- **Mac 바닥 정책 변경 — 사용자 승인.** 화면 맨 아래에서 걷고 z-order만 Dock 앞에 표시한다. 일반 창 판정은 유지한다. test04 ZIP과 Windows 측 검사는 완료, 실제 Mac 표시는 미검증. [수정·파일·확대/시작 부하 조사](TASKS.md#mac-screen-floor-and-scalestartup-review--2026-10-16). 확대는 제안·미구현이며, 후속 승인된 Windows 자동 실행 최적화는 위 항목에서 관리한다.

- **Windows 창 모드 전환 낙하 — 사용자 수락·설치·main 푸시·공개 갱신 완료.** 일반 Discord 창은 유지하고 확인된 클릭 통과형 오버레이만 제외했다. [실행·원격 인계 근거](TASKS.md#windows-window-mode-fall-investigation--2026-10-15). candidate15의 공개 파일과 체크섬 일치까지 확인했다. 다른 앱·PC 전환은 미검증이며 R3 보류 조건은 유지한다.
- **Mac 관련 검수 — 조건부 오버레이/Dock 발판 소실 위험 확인.** 당시 검수는 위 TASK에 보존한다. 이후 10월16일 사용자 결정으로 Dock 발판 자체를 쓰지 않는 방향으로 대체했다. F4 완료가 아니며 원격 test03은 보존한다.

- **새 승인 작업: F4 macOS 기능 동등성 이식 — 구현 시작.** Windows에서 개발하고 Mac 사용자가 실제 검증하는 방식. 현재25개 기능 항목을 모두 목표로 유지하며, [구현·수락 체크리스트](docs/specs/2026-10-15-macos-parity.md)를 따른다. 초기 렌더러/호스트 검사 기반과 교차 빌드는 실제 Mac 확인을 대신하지 않는다.
- **현재 상태: v0.1.3 Preview 공개·다운로드 확인 완료 — 2026-10-15.** 사용자 승인으로 기존 공개 보류를 이번 Preview에 한해 변경했다. R3·R4는 완료나 허가로 처리하지 않으며 1단계 전체 검증은 여전히 미완료다. [게시·다운로드 근거](TASKS.md#public-preview-release--2026-10-15).
- **현재 독립 개발: F1 현재 PC의 실제 로그인 자동 실행까지 사용자 수락 완료 / F2 비공개 main 반영 완료.** [현재 실행 근거](TASKS.md#login-startup-acceptance-and-private-main-checkpoint--2026-10-15)를 따른다. 기존 출시 게이트는 유지하며 공개 릴리스 완료는 아니다.
- **F3 자동 눕기 옵션: 구현·검사·로컬 설치·사용자 수락·비공개 main 반영 완료.** [기능 근거](TASKS.md#automatic-sleep-option--2026-10-14), [커밋·원격 일치 근거](TASKS.md#login-startup-acceptance-and-private-main-checkpoint--2026-10-15).
- **공개 후에도 남는 작업: R3 다른 환경 검증·R4 권리 확인.** 두 항목은 보류/미확인으로 유지하고 이번 Preview 안내에 고지한다. [이번 승인·실행 범위](TASKS.md#public-preview-release--2026-10-15).
- 10월 2일 승인된 앉기 동작과 일반 창 뒤로 밀림 복구의 로컬 업데이트·실행 검증을 반영했다. 다른 PC 검증이나 메일함·GitHub 상태 확인은 수행하지 않았다.
- 이후 사용자가 Clipchamp 앞 표시를 확인하고 비공개 main 커밋·푸시를 승인했다. 저장소 비공개 상태를 재확인했으며, 이 소스 체크포인트는 R5 공개 배포 완료가 아니다.
- 충돌 보호 로직을 넣은 새 로컬 설치본으로 현재 PC의 업데이트·정상 종료·제거·재설치·데이터 보존을 확인했다. 수정·검증 결과는 후속 비공개 main 체크포인트 대상이며 공개 배포가 아니다.
- 10월 2일에는 Windows 11 검증 환경이 준비되지 않은 상태에서 사용자가 커밋·푸시를 선택했다. 당시 결정은 R3 완료나 검증 면제를 뜻하지 않는다. [당시 체크포인트 기록](TASKS.md#security-and-lifecycle--private-main-checkpoint--2026-10-02).
- 항목 상태가 바뀌면 근거와 위 현재 단계·다음 우선순위를 함께 갱신한다. 작업 하나의 완료만으로 단계를 완료 처리하지 않는다.

## 1단계 — 출시 준비 · 미완료

목표: 수락된 기존 기능의 v0.1.3 후보를 배포 가능한 상태로 검증한다. 새 기능 개발은 포함하지 않는다.
기존 기능 기준과 수락 기록: [현재 제품 상태](TASKS.md#current-product-state).

| 필수 항목 | 상태 | 완료 조건 / 근거 또는 미검증 경계 |
| --- | --- | --- |
| R1 로컬 후보 준비 | 완료 — 10월 2일 로컬 후보 갱신 | 승인된 앉기 동작·선 두께·일반 창 복구의 앱은 유지하고 설치 파일 충돌 보호 로직을 반영한 새 후보 생성. [현재 후보·패키지 근거](docs/verification/2026-10-02-security-lifecycle-host.md). 배포 전 최종 소스와 후보의 일치 확인은 R5에 포함. |
| R2 실제 설치 수명주기 | 완료 — 현재 PC의 기본 경로 | 기존0.1.2→0.1.3 업그레이드에 이어 새 설치본의 동일 버전 업데이트·정상 트레이 종료·제거·재설치·로그/바로가기 보존·실행/중복 실행 검사 통과. [현재 증거와 한계](docs/verification/2026-10-02-security-lifecycle-host.md). 깨끗한 PC, 다른 환경 및 모든 실패/복구 경로 검증을 뜻하지 않는다. |
| R3 다른 환경 | 보류 — 2026-10-05 사용자 결정 | [환경 검사표](docs/ENVIRONMENT-TESTING.md)의 깨끗한 Windows PC, 배율·다중 모니터, 작업표시줄, 잠금·절전 복귀 등 필수 사례를 실행하고 결과 기록. 계획 작성은 검증 완료가 아니다. 별도 Windows 11 테스트 PC를 당장 확보하기 어려워 보류하며, PC 확보 후 재개한다. 완료 조건은 유지한다. [결정·적용 기록](TASKS.md#r3-environment-hold--authorized-2026-10-05). |
| R4 공개 권한 | 보류 | [권리 검토](docs/RIGHTS-REVIEW.md)의 출처·가공·이미지 동봉·소스 재배포 범위에 필요한 근거 확보. 문의 발송은 허가가 아니다. 회신·허가는 저장소 기록상 미확인이다. |

R1–R4가 모두 확인되어야 이 단계가 끝난다. 검증 대기·보류는 열린 항목이다.
검사 범위를 줄이거나 대체하려면 사용자 결정과 이유를 남기며, 항목을 지우거나 완료로 바꾸지 않는다.

## 2단계 — Preview 공개 배포 · 게시/다운로드 완료

목표: 검증된 설치본을 초보 사용자도 찾고 설치할 수 있게 제공한다.
의존성: 원래 1단계 완료 후 공개하는 순서였다. 2026-10-15 사용자가 한계 고지를 유지한 Preview 공개를 명시적으로 승인해 이번 실행 순서를 변경했다. 1단계 전체 완료나 권리 확보를 뜻하지 않는다.

- [x] **R5 최종 배포 묶음** — 완료. candidate14 설치본·갱신 안내문·체크섬을 v0.1.3 Preview로 게시하고 저장소를 공개했다. 이전 릴리스·보관 브랜치는 보존했다. [실행 근거](TASKS.md#public-preview-release--2026-10-15).
- [x] **R6 다운로드 확인** — 완료. 로그인 없이 README 링크·릴리스 페이지에 접근하고 3개 파일을 실제 다운로드해 원본/공개 체크섬과 일치를 확인했다. [동일 실행 근거](TASKS.md#public-preview-release--2026-10-15).

공개 후 요청한 이미지 제공·설치본 중심 보안 검사는 완료했다. [범위·근거·한계](TASKS.md#community-images-and-installer-security-review--2026-10-15). 전체 앱의 안전 보증이나 R3·R4 완료가 아니다.

숨김 파일 패키징 검사 보완·검증은 완료했다. 회귀8건에 이어 사용자가 기존 앱을 종료한 뒤 전체 패키지 검사와 격리 환경의 시작·중복 실행·정상 종료가 모두 통과했다. [실행 근거와 이전 실패 기록](TASKS.md#hidden-file-package-parity-hardening--2026-10-15). 이 보완의 남은 검사는 없으며 R3·R4는 그대로다. 후속 사용자 승인에 따른 [main 체크포인트 범위](TASKS.md#public-main-hardening-checkpoint--2026-10-15)는 검사 코드·기록이며, 검증된 공개 설치본은 그대로 유지한다.

다음: R3 환경 확보 후 검증, R4 권리 근거 확보. 공개 완료는 이 두 항목의 완료를 대신하지 않는다.

실행 결과는 TASKS에 기록하고 여기에는 상태와 근거 링크만 갱신한다.
비공개 전환·문의 발송 이력은 [기존 실행 기록](TASKS.md#rights-inquiry-and-private-repository--authorized-2026-09-21)을 보존한다. 이번 재공개 승인은 위 별도 결정이며 문의 재발송은 포함하지 않는다.

## 이후 후보 / 보류 결정

- **F4 macOS 이식 — 2026-10-15 사용자 승인, 진행 중.**
  목표: 현재 v0.1.3의 기능·외형을 유지한 macOS판. Windows판과 공개 설치본은 보존한다.
  순서: 공통 픽셀/호스트 검사 기반 → 모션·상호작용 이식 → 창/Dock·메뉴·자동 실행 → 패키징·실제 Mac 검증.
  완료 기준: [MAC-01–25](docs/specs/2026-10-15-macos-parity.md)의 기능·실패/복구·시각 기준을 Mac에서 확인하고 사용자 수락. 단순 빌드 통과는 완료가 아니다.
  현재: 화면 바닥·Dock 앞 표시 정책의 arm64 test04 로컬 ZIP 준비. 일반 창 판정·모션·픽셀은 유지하고 자동 검사95개와 배포 DLL 시뮬레이션 통과. [정책·파일·검증 근거](TASKS.md#mac-screen-floor-and-scalestartup-review--2026-10-16). 실제 Mac 동작은 미검증이다.
  이전 게시 이력: 먹기 효과 로딩을 개선한 test03은 별도 `feature/macos-test03` 브랜치와 `macos-test03` 사전 릴리스로 게시했고 익명 다운로드 해시 일치를 확인했다. [test03 근거](TASKS.md#macos-test03-branch-and-prerelease--2026-10-15). 해당 원격 파일과 Windows main/릴리스, 이전 test02는 보존했다. test04는 아직 원격 게시하지 않았다.
  미완: 실제 Mac 다운로드/압축 해제·첫 실행·메뉴/포커스·자동 실행·중복 실행, 창/Dock·권한·Retina/Spaces, 미세 합성 오차의 실제 화면 수락, 지원 OS·업데이트/제거·개발자 서명/공증. 테스트 묶음은 macOS13+ 전제이며 임시 무결성 서명만 있다. Mac 실행/보안 경고 통과나 제품 수락은 미확인이다.
  다음: [확인된 arm64 테스터](TASKS.md#macos-tester-architecture-clarification--2026-10-15)의 macOS 버전과 test04 첫 실행/먹기/종료/중복 실행, 화면 바닥·Dock 앞 표시·창 전환·좌표·입력 및 메뉴·로그인을 검증한다. Intel 빌드 경로는 유지한다. 새 원격 게시·결제·자동화 실행은 별도 승인 대상.

- **F3 자동 눕기 옵션 — 2026-10-14 사용자 수락 완료.**
  목표: 앉기·걷기·간식은 유지하며 자동 수면만 선택적으로 끈다.
  순서: 공통 설정·Sleep 진입 차단 → 양쪽 메뉴 → 저장/상태/회귀 검사 → 로컬 설치·사용자 확인.
  완료 기준: 기본 ON, OFF 재실행 유지, 양쪽 메뉴 상태 일치, 누운 상태 해제, 나머지 동작 보존 및 사용자 확인.
  현재: [구현·검사 근거](TASKS.md#automatic-sleep-option--2026-10-14). 수락 버전의 이미지·다른 모션은 유지한다.
  미완/다음: 기능과 비공개 main 체크포인트는 종료. 새 작업은 별도 지시를 따른다.
- **F1 간식 놀이·선택적 자동 실행 — 2026-10-15 현재 PC 사용자 수락 완료.**
  목표: 수치·숙제 없이 메뉴에서 꺼낸 간식을 쫓고 먹는 자발적 놀이.
  순서: 간식 모션/추격과 메뉴 연결 → 자동 실행 설정 → 회귀·로컬 실행 확인 → 사용자 수락.
  완료 기준: [범위·수락 기준](docs/specs/2026-10-13-snack-play.md#acceptance)의 기능·취소·기존 조작 보존 검사와 사용자 시각·조작 수락.
  현재: 등록·제거·재설치 검증에 이어 실제 컴퓨터 시작 후 자동 실행을 사용자가 확인했다. [수락 근거와 관찰 경계](TASKS.md#login-startup-acceptance-and-private-main-checkpoint--2026-10-15).
  미완: 현재 PC의 승인된 F1 범위는 없음. 다른 환경과 공개 권한은 R3·R4에 남긴다.
  다음: 이번 기능 작업과 검증 기록의 비공개 main 반영은 종료. 공개는 기존 출시 게이트를 따른다.
- **F2 니케풍 UI — 2026-10-14 사용자 수락·파일 정리·비공개 main 반영 완료.**
  목표: 기존 기능을 유지하면서 메뉴를 니케의 전체 UI 언어에 맞게 재디자인한다.
  순서: 웹 원본 비교 → 메뉴 대표 시안 → 실제 UI 적용·검수·사용자 확인 → 커밋·푸시.
  완료 기준: [수락된 B2 방향과 보존 범위](DESIGN.md)를 바탕으로 시각 기준을 확인하고,
  간식/앉기/자동 실행/로그/종료의 기존 접근·상태·취소를 보존한 실제 UI 수락.
  현재: 자체 트레이와 피그마 메뉴 수정 모두 사용자 확인으로 통과했다. 설치 묶음·문서를 정리하고 비공개 main 커밋·푸시 및 원격 일치를 확인했다. [실행 근거](TASKS.md#accepted-ui-and-snack-checkpoint--2026-10-14).
  다음: 이번 UI 작업은 종료한다. R3·R4 보류는 유지하며, 새 작업이나 공개 배포는 별도 지시를 따른다.
- 코드 서명: 사용자가 건너뛰기로 결정한 항목. 이번 릴리스 필수 조건으로 다시 추가하지 않으며, 서명 없음 안내는 유지한다.
- 캐릭터 교체·코드만 배포 등의 대안: 권리 검토 문서의 검토 후보일 뿐, 승인된 구현이나 배포 범위가 아니다.

## 역사와 역할

- [기존 M1 표현·애니메이션 로드맵](docs/roadmaps/2026-08-28-dororong-m1-expression-animation-roadmap.md)은 당시 범위·순서의 기록으로 보존한다. 현재 전체 출시 단계나 완료 상태를 대신하지 않는다.
- [보관 안내](docs/ARCHIVE.md)의 이전 기록을 유지한다. 과거 미완료 항목을 근거 없이 완료 처리하거나 현재 작업으로 자동 재개하지 않는다.
- 승인된 디자인의 세부 근거는 해당 [기능 설계](docs/specs/)와 후속 사용자 결정에서 확인한다. 이 문서는 제품·디자인 사양을 대체하지 않는다.

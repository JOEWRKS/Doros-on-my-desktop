# r69 seated tracking, sleep latch and snack-speed investigation

Date: 2026-10-18. Status: candidate running; native interaction acceptance OPEN.

## Request and scope

Allow mouse tracking while seated, investigate eyes opening during sleep, and
answer whether snack chasing has the same speed at 1x and larger pet sizes.
Preserve accepted r67 artwork, cheek/eye compositor, body/feet, hair tips and line
weight. No installation/settings changes. Snack speed is investigated, not changed.

## Causes and changes

- `DororongPresenter.Hunting.cs`: `_sittingRequested` was a global gaze block.
  High-resolution seated gaze now runs independently of body preparation.
  Sitting/standing-up still blocks proximity dwell and pouncing; stale hunt-body
  recovery is reset so it cannot replace the authored seated body. Legacy path
  remains unchanged.
- `DororongPresenter.HighResolutionGrab.cs`: an active gaze previously excluded
  the ordinary locomotion body. Seated locomotion now retains its body frame and
  head offset while using accepted gaze transforms. Blink behavior remains active.
  Added a read-only internal face-pose diagnostic for regression tests.
- `DororongPresenter.HighResolutionStates.cs`: Sleep phase repeats every four
  seconds in PetBrain, but `Closed` was recalculated from `phase >= .0225`.
  This reopened eyes at phase wrap despite a fully settled sleeping body.
  Completed sleep entry now latches closed eyes until Sleep is exited. Initial
  entry timing and the explicit waking blend are unchanged. Existing attention
  waking in PetBrain is not removed; an actual state change to Idle can open eyes.
- No asset, raster sampling, palette, eye/cheek deformation or outline edits.

## Snack-speed answer

`SnackSession.Advance` uses `185 * dt` DIP, independent of pet scale.
`PetLoop.TickSnack` adds `pose.DeltaX` directly to window X; no later scale factor.
Injected product-loop tests at 1, 1.25 and 1.5 measured 185 DIP over fifty 20ms
ticks. At 1.5x the body-relative travel rate is 2/3 of 1x. Physical screen pixels
also depend on monitor DPI; 185 DIP must not be called 185 physical pixels.

Existing elapsed handling caps each SnackSession update at 50ms. A core test with
ten 100ms updates moves 92.5 DIP over one second of supplied wall time. This proves
the conditional slowdown, NOT that the currently running user's app incurred it.
No fresh native timing capture was made here and neither speed nor cap was changed.

## Verification evidence

Artifacts: `artifacts/seated-sleep-20261018-r69/`.

- `tests/r69-before.trx`: five sleep cases fail specifically at phase 0 after
  settling. Five seated cases stopped at a test setup error: 1000ms was shorter
  than the existing 1050ms sit transition (frame318 vs321). Do NOT claim those
  failures reproduced missing tracking. The source block establishes that cause.
- `tests/r69-after.trx`: sleep5 pass; seated5 still fail the same setup assumption.
  Test setup corrected to1200ms; no product sit-duration change.
- `tests/r69-final.trx`:16 pass. Ten presenter cases across1/1.1/1.25/1.35/1.5:
  seated horizontal/vertical gaze, departure, seated body retained, no pounce during
  dwell; sleep through three phase wraps and explicit waking. Six injected
  product-loop cases at1/1.25/1.5: seated facing+gaze and snack distance.
- `tests/r69-regression.trx`:28 pass: HighResolutionStateTests,
  HighResolutionGazeTests, HighResolutionLocomotionTests and high-resolution
  hover-departure recovery. These are not native OS input tests.
- `tests/r69-snack.trx`:16 core SnackSession tests pass, including documented
  normal/capped elapsed distances. Total distinct passing cases above:60.
- `tests/r69-published-runtime.trx`:same16 targeted cases repeated with win-x64
  build. Tested and published App DLL hashes identical:
  `C34C8FAF9D93024BB11E0ECB12F5A0C5456E8AF74EE9D2FCABE287DA071F53CB`.
- `final-frames/`:160 saved samples. `tools/r69-review-sheets.ps1` creates ten
  labeled review sheets retaining the display-scale pixels (only empty canvas
  margin is cropped). All ten sheets directly opened via view_image. Observed:
  seated head/eyes respond in four directions and recover while paws/seated body
  stay in place; blink is visible in seated samples; sleep phase .995→0→.005→.02
  stays closed; waking samples open eyes. No new silhouette defect observed in
  these samples. This is not exhaustive frame-by-frame or live-display certification.
  Pixel-art skill used for saved-image review, not new artwork conversion;
  no palette/native conversion or image-generation delivery was performed.

## Source locks checked unchanged

| Source | SHA256 |
| --- | --- |
| r33 masked-head-225.png | 12d403f30a7e1f638fa6ea2516a8ad4344c3f86b85f9e503c7d1911b3dd84edf |
| r33 body-padded-375.png | 53474e7cbe3e08a63fe43551ae7d067cdd085942f4731557fe4b80a0f6ba648a |
| high-resolution-grab.bank | f3281baff95167d40f827b0928724192f52048c3ee590379fbd473e2f46a9a04 |
| high-resolution-hunt-body.bank | d3514dd116c5916df0201e16ced2c8ccb5892dbaccc7774c4bc502405b0fd802 |
| high-resolution-locomotion.bank | 5a4d7ffd79542b3a06c704604bf4ecb0b1a6f3a128ee4552423255da7b18676a |

## Actual execution boundary

User confirmed exit; process query found no Dororong running. Launched separate
normal transparent1.5x Release candidate with only `--high-resolution-grab=1.5`:
`artifacts/seated-sleep-20261018-r69/release/Dororong.App.exe`, PID20352.
Expected path and Responding=True verified. Computer-use list_windows did not
expose Dororong. No native pointer action or live screenshot verification was
performed. Do not convert launch/liveness, saved renders or injected inputs into
actual interaction PASS. Installer and settings unchanged; candidate left running.

Next: user's actual seated tracking and sustained sleeping/wake check on this
candidate; investigate actual snack timing only with new runtime evidence if
slowdown remains. Remaining roadmap motion/performance/environment checks remain
open and are not closed by these60 automated cases.

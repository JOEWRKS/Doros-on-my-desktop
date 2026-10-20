# r70 scale-proportional snack chase

Date: 2026-10-18. User authorized snack speed scaling while preserving artwork
and the latest seated/sleep fixes. Runtime verification is separate from tests.

## Change

`SnackSession.Advance` accepts an optional logical `chaseScale` (default1, finite
clamp1–1.5). Ground speed is now185×scale DIP/s. The speed limit is applied inside
the existing `min(remaining gap, speed×dt)` calculation, NOT multiplied onto the
already limited output: the final approach still stops8 DIP from the cursor.
Direction deadband, bite range, feeding timers and elapsed50ms cap are unchanged.

Ground `SnackPose.Distance` accumulates movement/scale in authored1x gait units.
This preserves leg-cycle timing and run ramp while larger legs travel farther.
Jump travel, lift, durations and jump contribution to gait phase remain unchanged.
PetLoopHost supplies the current presenter scale through GetSnackChaseScale;
legacy hosts default to1. It is logical pet size, not monitor DPI. Existing window
clamping/platform physics still receive the final screen-space movement normally.

| Pet scale | Expected and injected-loop one-second travel (DIP) |
| --- | --- |
| 1 | 185 |
| 1.1 | 203.5 |
| 1.25 | 231.25 |
| 1.35 | 249.75 |
| 1.5 | 277.5 |

## Evidence

Artifact root: `artifacts/snack-scale-20261018-r70/`.

- `tests/r70-before.trx`: baseline1x passes; four larger-scale cases fail at the
  first tick (all previously moved3.7 DIP per20ms, instead of scaled values).
- `tests/r70-targeted.trx`:18 pass: five sizes through the injected product loop
  including each tick's travel/gait distance and stopping to eat, plus retained
  r69 seated-facing and seated/sleep tests. These are NOT OS mouse gestures.
- `tests/r70-core.trx`:31 pass, including left/right travel, normalized gait
  distance/run ramp, live scale changes, near-target overshoot prevention, unchanged
  airborne trajectory and the existing snack suite.
- `tests/r70-regression.trx`:18 pass: source-resolution state rendering, snack menu,
  supported sitting/standing snack entry, perch release, carry interruption,
  lost-input cancellation, feeding/happy and landing cases.
- Total above:67 distinct passing cases. Win-x64 publication check recorded below.
- `frames/`:75 saved samples (15 per scale). `tools/r70-review-sheets.ps1` generates
  five display-pixel review sheets; all five were directly opened with view_image.
  Observed run entry and different leg phases at matching sample times, then stop
  and eating entry; no new silhouette/face/outline defect observed in these samples.
  Review is sampled, not exhaustive every-frame or live-frame pacing certification.
  The pixel-art skill was used for saved-image inspection, not new image conversion.
  No artwork or rendering algorithm was edited.

## Preserved files

SHA256 before/after match for accepted code:

| File under Controls | SHA256 |
| --- | --- |
| DororongPresenter.HighResolutionGrab.cs | 637424C942152F4225482ADC8B05A2BCD142AABB3DE4FD23AB2CA70D8D2A4FA9 |
| DororongPresenter.HighResolutionStates.cs | EDEF58253EF0BF13ACFB2D91E7C1BBAB260065B6DCC2F53D0557AE8AD402A4EB |
| DororongPresenter.Hunting.cs | EAA1B7D4BAF5B9A23CFD87B7F73FF04C1833EEF39CA98F1D63CE15E97B7AC502 |
| HighResolutionGrabRaster.Gaze.cs | DE03873701B29950C3F56942403BA14691C975E11A21F967F8A540135E3CD730 |
| HighResolutionFacePose.cs | 472030DB4CB09C46BA865FD2F6F19C4D8D0FDEC9BAF71442DEB47A8F1FA058FF |

r33 masked-head/body and grab/hunt-body/locomotion bank hashes also rechecked and
match the five locks in the r69 report. No palette, sampling, face/cheek shape,
body/feet, hair tips, outline-weight, installer or persisted settings changes.

## Release and native boundary

Published separate win-x64 Release under `release/`:

- App DLL: `D5643B2C93098E93693608EDF5A593ED4DE85DB026D3986E87965597A725E3C0`
- Core DLL: `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`

`tests/r70-published-runtime.trx`:18 targeted cases pass again with win-x64.
Tested App/Core DLL hashes match both published hashes above.

User confirmed old app exit; no Dororong process remained. Launched normal
transparent1.5x r70, PID38840, with only `--high-resolution-grab=1.5`. Expected
path/arguments and Responding=True verified. Computer-use list_windows did not
expose Dororong, including a second query after startup/liveness check. No native
mouse gesture or live screenshot inspection was performed. Actual interaction
and subjective speed acceptance remain UNVERIFIED. Candidate left running;
installer/settings unchanged. Liveness is not display or interaction proof.
The existing elapsed cap can still slow movement when ticks exceed50ms. This task
does not claim to resolve every latency source or certify actual subjective speed.
Next: actual running candidate snack chase/stop experience, preserving the r69
seated/sleep follow-up and other open roadmap motion/performance checks.

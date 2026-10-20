# r72 render cost reduction — native recheck pending

The user authorized fixing release/landing recovery and held-grab latency while
preserving the accepted r70 shapes, artwork and snack speed. This is a renderer
optimization candidate, **not a verified native stutter fix or a release completion**.

## Evidence and causes

The r71 native capture contained 12 actual user presses and eight callbacks over
50ms after the first five seconds (max 68.664ms). See [r71](2026-10-18-r71-runtime-motion-audit.md).
Those logs locate delays during held grabs and after release, but cannot assign
every delay to a particular platform phase. Two full rasters between callbacks
are not proof of redundant work: two loop ticks can legitimately have occurred.

Before changing sampling, `RenderCostRegressionTests` exported 360 deterministic
frames: five scales (1, 1.1, 1.25, 1.35, 1.5), six pose families and 12 samples each.
The baseline had cost instrumentation only; rendering arithmetic was still r70.
All exports/timings live under `artifacts/render-cost-20261018-r72/`.

Confirmed computational inefficiency: the lower-body constant-colour shortcut
required an identity head matrix. A tilted head disabled it even where head
features could not affect the feet, forcing 8x8 sampling of uniform pixels.
This greatly increased the controlled tilted-landing cost. Grab deformation also
evaluated its exponential hair field when an exact-zero gate discarded it.
Release frame preparation still has material cost (about 6ms in the final 1.5x
sample average); it is not declared solved by the landing optimization.

## Changes and preservation boundary

- Check the inverse-transformed head footprint instead of requiring identity.
  Only use constant coverage below world Y76 and head-local Y71, with no posed
  body/neck/effects/snack/flutter. Nonzero material lag additionally requires
  world Y84, where the existing field is exactly zero. Varying edge pixels retain
  full 8x8 coverage; there is no reduction of resolution, quality or frames.
- Skip hair-field arithmetic when the existing head/accessory gate is exactly
  zero or the near displacement is zero. Nonzero fields retain their arithmetic.
- Hoist the eight body-ink sampling offsets out of the source-pixel loop, with
  the same floating-point calculations. Outline width and colour are unchanged.
- Reuse a fully cleared raster scratch array. Every returned frozen bitmap owns
  its independent copy; retained-image tests check this after a second render.
  This removes a large managed allocation per draw/sole query. It is **not**
  evidence that GC was the sole cause of r71 native stalls.
- Add preparation/sampling/bitmap and sole cost counters to explicit performance
  capture, plus platform phase/squash/spread state, to distinguish remaining costs.
  `rasterMs` still includes sole work; `rasters` counts full renders only. New
  stage totals and `soleMs` overlap and must not be summed together.

No motion timing, gaze/cheek geometry, bank assets, installation or settings edits.
The Core DLL remains byte-identical to accepted r70, including proportional snack
chase speed. Five locked input hashes match r70:

| Input | SHA256 |
| --- | --- |
| r33 masked-head-225.png | 12D403F30A7E1F638FA6EA2516A8AD4344C3F86B85F9E503C7D1911B3DD84EDF |
| r33 body-padded-375.png | 53474E7CBE3E08A63FE43551AE7D067CDD085942F4731557FE4B80A0F6BA648A |
| high-resolution-grab.bank | F3281BAFF95167D40F827B0928724192F52048C3EE590379FBD473E2F46A9A04 |
| high-resolution-hunt-body.bank | D3514DD116C5916DF0201E16CED2C8CCB5892DBACCC7774C4BC502405B0FD802 |
| high-resolution-locomotion.bank | 5A4D7FFD79542B3A06C704604BF4ECB0B1A6F3A128EE4552423255DA7B18676A |

## Controlled timing (not native FPS)

Same deterministic inputs, PNG encoding excluded, 12 samples per family at1.5x.
These include cold entries and are not statistically isolated whole-app latency.
`before/costs.json`, `after/costs.json`, `final/costs.json` preserve all samples.

| Family | Before mean ms | First candidate mean ms | Final repeat mean ms |
| --- | ---: | ---: | ---: |
| grab | 9.14 | 7.93 | 8.33 |
| release | 13.50 | 12.08 | 11.99 |
| landing | 17.46 | 14.20 | 15.61 |
| tilted landing | 35.87 | 16.76 | 18.09 |
| tracking | 2.88 | 2.42 | 2.93 |
| cheek | 4.83 | 5.32 | 5.67 |

The tilted-landing reduction is substantial in this workload. Cheek timing did
not improve; do not describe all motions as faster. The final tilted average
still exceeds a16.67ms budget before other app work, so a smooth60Hz claim would
be unsupported. Actual gesture trajectories and machine load may differ.

## Verification

- `tests/final-pixels.trx`:360 raw Pbgra32 SHA256 values match the independently
  captured baseline, complete occupied sole bands match full-render pixels,
  and earlier frozen bitmaps survive scratch reuse unchanged. One parameter-loop
  test, not360 separately reported tests. Five sizes, no native pointer input.
- `tests/regression.trx`:64 pass. Existing high-resolution/injected-loop coverage
  includes grab/release/regrab, landing, moving support, perch, gaze/cheek,
  sitting/sleeping and snack speed. The optional old performance-export test has
  no output environment set in this suite; its pass is not a timing measurement.
- `tests/snack-speed.trx`:31 core snack session/scale cases pass.
- `tests/shared-renderer.trx`:46 shared surrounding-pull, head anchoring and
  platform-contact cases pass, including byte equality to the approved legacy
  deformation renderer. `git diff --check` on the changed product paths is clean.
- Agent directly viewed all five `review/<scale>.png` sheets:90 before/after pairs
  (samples00/05/11 of each family). No new head/face/body/feet/outline differences
  visible in these pairs. This is a saved-frame sample review, **not all360
  visually inspected and not native continuous playback**.
- Initial `tests/candidate.trx` failed because the first comparator compared PNG
  decoded unpremultiplied bytes against raw Pbgra32. PNG edge conversion is not
  lossless in that representation. The corrected test compares the baseline's
  raw-byte hashes; `candidate-raw.trx` and `final-pixels.trx` pass. The failed test
  remains saved and is not hidden or counted as a product visual regression.

## Candidate and actual execution boundary

Release prepared at `artifacts/render-cost-20261018-r72/release/Dororong.App.exe`.
Published and tested win-x64 DLL hashes match:

- App: `5EB22A03DE946255FFDC60927702D3955D03C866EAA1F14D2A2B2478D92B4FB0`
- Core: `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`

The user answered the replacement/readiness question with **"잠만 준비되면 말할게"**.
Honor that: no app replacement, native input or new60-second capture has started.
At23:57 KST the existing r70 PID52936 was still running/responding. No current-turn
native window screenshot was obtained. Prior transparency/capture limitations
remain historical evidence, not a new tool check in this turn.

Next: once the user says ready, verify tray exit and start this candidate normally
at1.5x with `--performance-capture=<absolute path>/native-interactions.json`; give
the start signal immediately. No heavy tests during capture. Ask for held-grab,
tilted release and complete landing recovery, then evaluate delivered inputs,
phase costs, >33.333/50ms callback tails and actual footage if available. Do not
infer visual success from counters. Other scales, sustained operation and all
motion combinations remain unverified in native execution.

# r92 release onset: recovery cancelling visible descent

## User correction and bounded scope

After r91 the user reported a single hitch immediately on release, then clarified: upright release looks like straightening first and falling afterward; release during angled dangling catches. User stopped r91 and explicitly asked that this observation be examined before launching a diagnostic. No r92 launch occurred before that investigation. Remaining r91 HWND/worker delay and hours-long memory questions are not silently closed by this motion work.

## Reproduction

`artifacts/render-cost-20261019-r92/tests/onset-before.trx` records actual high-resolution presenter + PetLoop + platform solver at1.5x,16ms steps,0/+45/-45degrees. This is a controlled product-render test, not native input/DWM evidence. Physics already starts immediately, with host drop23.04DIP at160ms. In the upright case the visible sole moves only about2DIP because recovering body shortening cancels descent. In the angled cases the sole reverses direction during recovery. Thus the issue is not accurately described as a physics state waiting until release animation ends.

Added an explicit world-trajectory assertion before changing presentation: `onset-repro.trx` has3 expected failures (about2DIP discrepancy by32ms). Preserved diagnostic output documents the mismatch. Existing tests asserting only falling phase/window displacement did not catch visible geometry cancellation.

## Candidate and rejected intermediate

First intermediate enabled the normal sole compensation for all released heads. `onset-after.trx`20passed, but examining head coordinates exposed an upward head pull for+45degrees. This was NOT accepted or launched; sole-only success was insufficient.

The nonnegative-compensation intermediate passed `onset-no-upward.trx`3cases, but broad `final.trx` failed5of667 (662passed): all five actual-loop scales detected2rasterizations in one tick. Baking correction into the pixel transform invalidated the image when the runtime retired/reapplied the offset. That intermediate was NOT launched. A parallel rebuild also hit MSB3027 because the broad testhost owned the output DLL; no product crash is inferred. Subsequent builds are serialized.

Current candidate retains the original release shape/angle curve and180ms recovery duration. While a released head is Falling, it adds only nonnegative vertical compensation when the recovering silhouette would lift the sole against gravity. It never pulls the head upward just to cancel a taller rotated silhouette. This offset now uses a single reusable WPF TranslateTransform rather than another rasterization. Existing presentation rebase retires this translation each tick for full releases as well as partial releases, preserving the solver's velocity. Hit mapping/source coordinates include the displayed offset, and a new head press folds it into the captured matrix. Rendering clears the translation-active flag with the actual transform to avoid stale source coordinates. Held head behavior and ordinary grounded/landing rendering paths remain unchanged. Core gravity/landing timing, authored banks and installation/settings are unmodified.

`translation-recheck.trx`46passed, including the previously failing five-scale real-loop raster bound. Added regrab coverage initially had6assertion failures because it incorrectly required pointer capture immediately at BodyPending; coordinate continuity assertions passed. The controller intentionally starts capture only after drag displacement. The test now asserts BodyPending, then moves20DIP and checks capture. This is a test-contract correction, not a production behavior change. `regrab-recheck.trx` preserves the initial15pass/6fail. Final combined recheck is pending.

## Diagnostics

The existing bounded frame recorder supports an explicit `--frame-capture-trigger=release` option, using the same single64MiB maximum allocation,16pre-trigger frames and800ms post-trigger bound. It triggers at the first Falling CPU output after Body input, not at a hardware button-up timestamp. Manifest `trigger=FirstFall` and `relativeToTriggerMs` distinguish this from landing; legacy relativeToImpactMs remains for schema compatibility but is NOT contact time in release mode. Each stamp now includes image offset in the host window, so raw bitmap movement is not mistaken for final placement. Default strong-landing recording is unchanged. Relevant initial diagnostic/continuity tests28passed before the presentation change.

## Final candidate checks and native start

`tests/final-recheck.trx`:673passed/0failed, including15release trajectories,6regrabs, five-scale actual-loop single-raster bounds and prior landing continuity. Recovery progress assertions retain180ms. Baseline r91 comparison:420general+80paw+120effect pixel hashes identical;335synthetic landing pixel hashes and relative times identical. These do not assert that the intentionally corrected world trajectory is identical.

Directly opened4final PNGs: `release-1.50-45-01`, `release-1.50-45-06`, `release-1.50-0-10`, `release-1.00--45-06` in `release-frames-final`. No new silhouette tearing seen in those samples. These are raw CPU images without window/display translation and do not prove screen smoothness;75saved files are not75directly inspected images.

Tested/published App SHA256: `ACB250038D8D04AD581B390D9D5F6FDA9A49553DE4779F3A9516ED832DAF9302`. Core remains `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`; grab/hunt/locomotion banks retain prior hashes. Earlier published intermediate was overwritten only in this candidate's isolated output directory before launch. Installation/settings unchanged.

After notifying the user, launched normal transparent1.5x at22:07:00 KST,PID148376,180-second performance/memory capture and FirstFall frame capture. Process responding confirmed; native input, saved frame results and user acceptance still pending. Requested tilted release first, then upright release. Recording ends around22:10 but app stays running. No live dump/profiler or intentional process pause in this iteration. This does not close r91 external/compositor delays or hours-long retention.

### Native result superseding the pending status above

180seconds completed,4Body presses recorded,80release PNGs/error null. Opened016/024/031 directly; raw images show recovery but do not encode the full world trajectory by themselves. User reported **“놓고나서 한번 감속이 됨”**, so r92 motion is NOT accepted. After5seconds tick max45.0325ms/0over50, rendering callback max62.1608ms/7over50. Managed peak174.17MiB/end169.96MiB; this is not hours-long stability or proof of a leak fix. No pausing diagnostics ran.

Controlled trajectory output explains a limitation in the foot-based criterion: at+45/1.5 the head moves13DIP in one16ms step near160ms but5.68DIP near192ms. Foot compensation adds temporary apparent speed while straightening and removes it at recovery end. Monotonic feet are therefore insufficient for the user's uniform falling request. The user requested a simple solution; subsequent r93 uses visible-center gravity rather than foot tracking. See [r93](2026-10-19-r93-center-fall.md). r92 runtime was left running until the user agreed to close it for r93; do not interpret this report as acceptance.

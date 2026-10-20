# r93: independent center descent and pose recovery

## Scope and correction

User rejected r92's single deceleration after release and explicitly asked to solve it simply. Replace the temporary foot-based downward correction with one visible-center anchor. Render the unchanged recovery frame once, then translate that frame so its bounding-box center retains its previous local position; the existing platform motion supplies gravitational descent. The original180ms recovery, art matrices/frames, gravity, landing pre-expansion and snack speed remain unchanged. Existing rebase retires the display translation without changing velocity; regrab incorporates the offset.

This changes the acceptance invariant deliberately: neither feet nor head must independently follow pure gravity while a silhouette rotates/shortens. The **whole visible center** must follow gravity through recovery and its end without a temporary speed boost. Tests compare each16ms center displacement against900*(t²-previousT²), keep recovery progress assertions and the one-raster-per-tick bound, and cover regrab during recovery. This is not a relaxed version of the rejected foot-only test.

Initial `center-recheck.trx`:22passed/20failed. WPF geometry queries were observing the previous RenderTransform arrangement immediately after assignment. Added the existing conditional `EnsureHighResolutionLayout` after the release transform assignment, not an unconditional global layout flush. `center-layout.trx`:21passed, all five scales/0,+45,-45 and six regrabs; inspected+45/1.5 output shows center steps exactly match gravity through192/208ms recovery handoff. Native behavior is still unverified. Prior r92 user-reported deceleration remains unresolved until new actual evidence.

Final related regression `tests/final.trx`:93passed/0failed. Baseline r91:420general+80paw+120effect hashes and335landing hashes/timestamps identical.75raw release PNGs also exactly match r92, isolating the changed display trajectory from shape/speed/frame changes. Tested and published App SHA256 `1CB2F7C81A136114CC33D629A30FC65DDB174DB1382A1402FB599BE87A2FD963` match. No installation/settings changes, live dump or intentional process pause. r92's180second memory observation is not a memory leak pass.

User confirmed r92 exit,process absence checked. After start notification,r93 launched normal transparent1.5x22:15:34 KST,PID144664,120second capture with release frames. Process responding confirmed; user asked to compare tilted/upright release. Actual result and user acceptance pending. App remains running after capture.

## Actual run boundary

120second export completed22:17:35. **Inputs0,release frames0,triggered false,error null.** Therefore no actual release/landing verification occurred in this captured interval; do not infer the reason for missing input or claim motion passed. User feedback remains pending. After5seconds tick max49.0458ms/0over50,callback max54.1746ms/4over50. Managed peak153.03MiB/end141.96MiB,private end235.57MiB. This is an idle/no-direct-input observation only, not a loaded performance comparison or long-term memory result. App remains running,installation unchanged. The user was told recording ended with0input and asked to report current release behavior; no automatic relaunch or dump.

`Measure-Release.ps1` reads saved bitmap alpha bounds plus recorded image/window offsets for world-center estimates; it does not capture DWM. End-of-update timestamps may bunch when frame cost changes, so their finite-difference speeds must not be confused with solver velocity or screen FPS. r93 has no release rows to analyze.

## Subsequent user acceptance — 2026-10-19

In response to the question about r93's one-time deceleration on tilted/upright release, the user replied **“굳.”** Record the release behavior as user accepted and preserve it as the baseline. This supersedes the earlier pending-user-acceptance status, not the evidence limits: the timed capture still contains0inputs/0release frames, no new agent screen inspection occurred, and loaded performance/hours-long memory remain unverified. This follow-up changes records only, not product code, settings, installation or the running process.

# r71 accepted-r70 motion and latency audit

Date: 2026-10-18. The user accepted the current form and snack speed and requested
remaining motion/latency inspection in actual execution. Preserve r70. No product
code, art, speed, installer or settings changes in this audit.

## Fixed target and automated coverage

Release: `artifacts/snack-scale-20261018-r70/release/Dororong.App.exe`.
App DLL SHA256: `D5643B2C93098E93693608EDF5A593ED4DE85DB026D3986E87965597A725E3C0`.
Core DLL SHA256: `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`.
The win-x64 test directory contains these same two DLL hashes; no rebuild needed.

`artifacts/runtime-audit-20261018-r71/tests/r71-motion.trx`:54 pass. Selected existing
checks cover source-resolution walk/sit/rise/blink and sleep/snack/click/perch-paw
states; gaze entry/departure and cheek interaction; grab, partial release/cancel,
regrab, moving/removed support, extreme landing repeated-raster prevention and
bottom-edge perch eligibility/attachment. Various existing cases use1–1.5x (some
three sizes, others five), not every scenario at every size. These are presenter
or injected-loop tests, NOT actual OS interaction or live visual acceptance.

Standalone1.5x sampler timing: `sampler/timings.json`,15 measured frames per kind
after3 warmups, PNG encoding outside the timed region. Mean/max milliseconds:

| Kind | Mean | Max |
| --- | ---: | ---: |
| idle | 2.584 | 3.924 |
| tracking | 2.944 | 3.753 |
| cheek | 7.412 | 25.880 |
| grab | 9.257 | 14.318 |
| flutter | 4.225 | 7.736 |
| landing | 18.046 | 22.300 |
| run | 5.666 | 6.819 |
| eat | 2.676 | 4.267 |

Landing exceeds a16.67ms/60Hz budget in this sampler. Cheek has an isolated25.88ms
sample. This is a cost/risk finding, not actual displayed FPS or proof that a
specific user's gesture dropped frames. Sampler snapshots were generated but not
visually opened this turn; no new saved-frame visual PASS claimed. Automated
tests completed before native capture, so this audit did not run them concurrently
with the captured interactive process.

## Native access and first execution capture

Computer-use list_windows returned no Dororong target while normal transparent
r70(PID38840) ran. No native input action/screenshot performed by the agent. Prior
opaque-host attempts in r68 had failed capture twice; this audit did not repeat
those disruptive launches or use alternative OS automation.

After user confirmation and a no-process check, started the SAME r70 normal
transparent1.5x exe(PID130592) with an explicit local performance-capture option.
`native-performance.json` covers60001.7582ms. The user subsequently confirmed
they had NOT performed the requested gestures. Input events0;552 state samples
contain only Idle/Walk with direct=None and perch=None. This is a baseline
idle/walk record, NOT input failure or interaction validation. Hover is not
separately identified in this log format.

After excluding first5000ms,4407 WPF callback intervals: mean12.482ms,
p50=10.556ms,p95=28.645ms,p99=32.663ms,max42.437ms.32 intervals exceed33.333ms;
0 exceed50ms or100ms. Raster cost per callback mean2.009ms,p95=5.931ms,
max14.864ms; multiple-raster callbacks0. CPU time16281.25ms across60s (~27.13%
of one core, not total multicore usage).

Startup first5s:402 positive callback intervals, max46.402ms; max raster19.191ms;
0 intervals over50ms. These callbacks are NOT GPU/monitor presentation FPS, and
the timing does not establish that every transition is visually smooth.

Read-only summarizer: `tools/r71-summarize-runtime.ps1 -Path <capture.json>`.
State associations are nearest prior100ms samples, never exact gesture timings.

## Second capture — coordinated interaction run

User said ready after being asked to exit and prepare for the60s gesture window.
Confirmed no Dororong process remained. Started unchanged r70 at1.5x, PID52936,
with `--performance-capture=.../native-interactions.json` at2026-10-18 23:05:40 KST.
The user was prompted to perform hover departure, cheek pull/release, head
grab/shake/drop and perch/release. `native-interactions.json` then recorded
60027.3955ms,544 state samples and12 actual application press events: RightCheek2,
Body10. The high-resolution hit map labels upper-head grab as Body; ordinary
body clicks are ClickOnly. Agent did not inject these events.

Recorded CheekPress/Pull/Release and BodyPending/DragEntry/Hold/Settle paths, plus
perch Entering→Attached near35.79→35.92s and55.55→55.76s; later head grabs detach.
Position samples show falling after the release near23.05s, reaching the lower
support near23.91s and settling around24.27s. That is runtime transition evidence,
not visual certification of limbs/outline or exact landing animation phase. The
current diagnostic state does not expose PlatformPhase/Squash/LegSpread/gaze.

Steady capture(after5s):3910 callback intervals, mean14.075ms,p50=12.562ms,
p95=31.117ms,p99=43.870ms,max68.664ms.96 intervals>33.333ms,8>50ms,0>100ms.
Raster/sparse-contact processing per callback mean2.686ms,p95=9.689ms,
p99=14.321ms,max43.574ms.7 callbacks accumulate2 full rasterizations. CPU time
32937.5ms (~54.87% of one core), not a whole-machine utilization metric.
Startup first5s has one additional>50ms interval(max51.767ms); max raster23.042ms.

**Latency finding stays OPEN.** The largest gap at24.652s is68.664ms with29.169ms
recorded raster/contact cost and2 full rasterizations.24.870s/24.956s have56.279/
60.351ms intervals and38.770/43.574ms cost, after the earlier drop reached support.
Another66.541ms gap appears at32.715s. Four other>50ms gaps occur with the nearest
state sample in BodyDragHold. These correlations narrow the expensive paths;
they do not uniquely identify a line-level root cause or prove a redundant frame.

Code review: HighResolutionRenderMilliseconds includes both DrawHighResolution
and sparse MeasureHighResolutionSole costs. Full raster count increments only in
DrawHighResolution; two between CompositionTarget callbacks can be separate loop
ticks, not necessarily a duplicate pose. ApplyHighResolutionPlatform measures
sole before/after platform transforms and then draws. Further profiling needs
separate tick IDs, platform phase and cost buckets to distinguish unnecessary
re-rendering from valid changing-pose/contact work. No optimization or artwork
change was made merely on the callback count.

## Result and remaining boundaries

- User's native gestures reached the app and the listed state transitions were
  captured at1.5x. Original idle/walk run is retained separately.
- No agent-performed live screenshot/video review: the transparent app is not
  exposed by computer-use, and no user recording was supplied this turn. Do not
  mark visual correctness, hover smoothness, all motion combinations, every size
  or sustained operation PASS. Sleep/sit/paw-local input were not specifically
  observed in this native interaction capture.
- Actual callback latency spikes exist; no overall performance PASS. No hardware
  presentation-FPS measurement, no claim that the sampler alone proves dropped
  displayed frames, and no claim of a completed fix.
- Same accepted r70 remains normally transparent at1.5x(PID52936), Responding=True.
  Performance capture has automatically ended. Installer/settings/art/speed and
  product source are unchanged. Only a read-only report script and audit records
  were added/updated.
- Next: diagnose per-tick platform/contact/draw timing around the release→landing
  recovery and dragging spikes; obtain actual video for the visual transitions.
  Product optimization requires a follow-up change request; preserve the accepted
  shape/speed and verify any change on the same native scenarios.

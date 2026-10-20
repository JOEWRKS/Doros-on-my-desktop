# r72 native grab/release/landing measurement — latency remains

## Run identity and limits

The user confirmed readiness and that the previous instance had been closed.
No Dororong.App process remained. Verified the candidate DLL hashes against
[r72 candidate evidence](2026-10-18-r72-render-cost.md), then launched normal
transparent1.5x Release at2026-10-19 00:10:54 KST (PID131240). Immediately told
the user to perform held shaking, tilted release and full landing recovery.

Executable: `artifacts/render-cost-20261018-r72/release/Dororong.App.exe`.
Arguments: `--high-resolution-grab=1.5` and absolute
`--performance-capture=.../native-interactions-20261019.json`.
Raw evidence: `artifacts/render-cost-20261018-r72/native-interactions-20261019.json`.
App SHA256: `5EB22A03DE946255FFDC60927702D3955D03C866EAA1F14D2A2B2478D92B4FB0`.
Core SHA256: `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`.

No source, artwork, speed, settings or installer changes in this measurement.
No build/heavy test ran during capture. The user, not agent OS input injection,
performed the gestures. No screenshot/video was acquired; live appearance and
continuous visual smoothness are not verified. No new native-UI capture attempt
was made during this measurement.

## Delivered input and recovery state

Capture lasted60015.2381ms, `inputHost:false`. Five native presses reached the
handler: four `Body` (upper-head grab in this hit map) at15590.518,24532.232,
37561.529,42655.211ms, plus one `ClickOnly` at41623.707ms.

Each head grab produced Entry/Hold and release/settle states, followed by Falling,
Landing and Supported. Approximate first observed timestamps (100ms state sampling):

| Sequence | Released/falling ms | Landing ms | Supported ms | Extreme |
| --- | ---: | ---: | ---: | --- |
|1|20596.710|21246.042|21784.593|false|
|2|33256.904|34235.359|36010.212|true|
|3|39220.094|40285.435|42077.099|true|
|4|48626.542|49710.582|50149.370|false|

These confirm state delivery/recovery, not exact visual timing, tilt angle,
per-frame appearance, or correctness of every height-dependent choice. The third
sequence includes the ClickOnly press during recovery, so it is not an identical
repeat of the second. No perch sequence occurred.

## Timing result: not passed

Exclude first5000ms for the steady-window summary (retain startup separately):

-3785 WPF callback intervals:mean14.539ms.
-p50 **13.2243ms**, p95 **31.9094ms**, p99 **45.9801ms**, max **81.9259ms**.
-136 callbacks >33.333ms, **7 >50ms**,0 >100ms.
-Combined full/sparse render cost per callback:mean2.9702ms, p959.7955ms,
  p9914.3291ms, max53.0690ms. Three callbacks accumulated two full renders.
-CPU34734.375ms /60015.2381ms =57.88% of one logical-core equivalent,
  not57.88% of the entire machine.
-Startup also had a61.448ms callback at62.7199ms, render cost6.5536ms.

The old r71 had eight >50ms callbacks and max68.664ms, but its gestures/input
count differed. Seven versus eight is **not proof of improvement**; this run's
worst callback is longer. No overall stutter-fixed or60FPS claim is supported.
WPF callback cadence is not GPU/monitor presentation FPS.

## Cost localization

All values below are callback-aggregated milliseconds. State is the latest100ms
sample, not an exact per-render phase marker. `soleMs` is included in render cost
and stage totals; do not sum it again.

| Capture ms | Interval | Render | Preparation | Sampling | Bitmap | Sole | Nearest state |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
|30641.622|57.743|16.856|0.86|15.53|0.46|0|Hold/Suspended|
|41415.197|58.083|34.230|3.43|27.97|0.85|13.46|Landing|
|41499.591|51.099|48.104|3.40|41.74|0.59|21.90|Landing|
|41622.807|68.336|41.909|2.92|35.75|0.94|22.19|Landing|
|43045.033|81.926|32.537|0.82|31.38|0.33|0|Hold/Suspended|
|43186.586|79.721|53.069|0.77|51.98|0.32|0|Hold/Suspended|
|44258.587|50.193|13.615|0.45|12.88|0.29|0|Hold/Suspended|

The three landing outliers occur in sequence3 near spread1→.736→.112 and
squash.574→.181→-.153, i.e. compression/recovery. Each has two sparse sole
queries and one full draw. Held-grab outliers have no sole queries; they cannot
be explained as landing/contact work. Sampling dominates the measured render
cost, but the difference between callback interval and render cost is not
localized by this capture. Sampling elapsed time includes thread scheduling;
do not call all of it pure arithmetic CPU or attribute everything to GC.

## End state and next boundary

Capture reached Supported/Idle at60014.1244ms and saved the full60-second file.
After the agent announced measurement completion, the process disappeared.
User explicitly confirmed **"내가 끔 측정 끝났대서"**. This is user-requested
shutdown, not a crash. No automatic relaunch; no pet is left running by this turn.

Next work within the outstanding performance task: isolate sampling cost for
held deformation separately from extreme landing's two sole queries/full draw;
preserve accepted pixels and motion timing, then repeat native validation.
Other sizes, continuous visual review, sustained operation and all motion
combinations remain open. Current result is successful measurement, failed
latency acceptance, and unverified live visual quality.

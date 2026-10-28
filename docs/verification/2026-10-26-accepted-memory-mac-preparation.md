# Accepted Windows memory and Mac update preparation — 2026-10-26

## Fixed baseline and scope
User accepted `candidate-seated-head-occlusion-20261026`, then asked for memory
leak testing and preparation for a Mac update if stable. Source `deb5cae`, delivery
record `384f828`; Windows App SHA256
B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0.
No product-code change during this observation. Preserve accepted artwork,
motion shape/timing, current Windows process, installation and settings.

## Measurement design
Evidence root `artifacts/accepted-memory-20261026/`.
- Qualification: separate diagnostic PID28504,150.062wall seconds, normal end.
  Actual rendered face counters confirm tap/shake/groom/stretch/beg/angry/chuckle/
  eating/sleep;180 size applications and1 menu opening. Not a memory stability pass.
- Frozen diagnostic uses the exact packaged App/Core DLLs. Real transparent WPF
  HWND, product presenter/display surfaces/menu control. Code-fed snapshots and
  cursor targets; no OS input injection, no real user-action claims, not full PetLoop.
- `actual/`: PID60004/startTicks639286179034465525; UTC13:25:03.4465525Z.
 900 real seconds, first180warmup, repeated workload until720, final180seconds
  stable idle without scripted interaction.33ms dispatch/1/30simulated step with
  no catch-up; simulated and elapsed times logged separately.
- `live/`: existing accepted pet PID83300/startTicks639286168045587123,
  observer84796 reads private/GDI/USER/handles and VirtualQueryEx write-combine
  regions every5seconds for900seconds. This is not total GPU VRAM or input logging.
- Every sample checks identity; UTC/monotonic intervals and maximum loop gap
  distinguish continuous observation from sleep/interruption. Missing or failed
  paths cannot pass. Final normal end and all required counts must be present.
- Compare180–360,360–540,540–720seconds and720–900recovery; exclude initial
  cache filling. Track managed estimate, natural-GC heap/committed/fragmentation,
  allocated bytes (activity, not retained bytes), frame/spare/scratch capacities,
  cache counts, native regions, handles and GUI resources.
- No forced GC, working-set trim, heap dump, trace, live app restart, app shutdown
  or PC shutdown. Diagnostic process closes only its own window on completion.
- Persistent same-workload growth would require current ownership investigation
  before a product correction. A total-private peak alone is not proof of a leak.

## Mac preparation findings (not a new Mac release)
Current Mac source builds on Windows (two unused-field warnings). It links shared
Windows face/partial-render files but does NOT wire the new seated paw/contour,
shake, idle activity or DORO-question presentation into its own runtime/sampler.
The unused `seatedBodyDistance` warning is consistent with that missing call path.
A successful build would silently omit requested features if packaged now.

Required implementation map:
1. `Dororong.Raster/HighResolutionGrabRaster.cs`: connect stretch/seated warps,
   contour, bounded distance field and body-only paw compositor. Preserve Mac
   dirty-region/parallel-sole logic; do not overwrite this file with Windows copy.
2. `RuntimePort/Presenter*`: port accepted idle/seated application, captured-pose
   recovery, sitting click/facing path. Reuse shared motion functions where possible.
3. `RuntimePort/PetLoop.cs`: suppress seated click hop and connect activity events.
   `R104DesktopController` must opt in to1minute DORO/idle activities like Windows;
   default Core alone does not enable them. Add native bubble presentation preserving
   the accepted position. Existing snack code is connected but requires latest
   full-phase pixel comparison, not a claim based solely on shared file inclusion.
4. Compare exact current Windows frame baselines at100/125/150%, both facings,
   through raise/hold/lower and transitions. Density1/2, full vs incremental buffers,
   no head overwrite, no grooming branch, correct soles, no stale-frame remnants.
5. Retain previous Mac repairs listed below; rerun relevant host checks after changes.
6. Build a new uniquely named Apple Silicon tester only after feature/pixel/memory
   gates. Keep test09 ZIP intact; use exact bundle name/path in instructions, include
   portable runtime, seal finalized contents, verify executable modes, ZIP CRC,
   extracted inventory and all signed hashes. Native Finder/codesign/Gatekeeper
   remains an actual-Mac gate. Do not require tester logs or a .command launcher.

Previous failures to preserve against:
- Popup logical theme parent, slider visibility, right-click/status menu close/reopen
  and content disposal (`PetMenuHost`).
- Visible-pet screen coordinates and owned-overlay NSWindow constraint; multi-monitor
  origin/scaling/topology/clamshell and window support. Do not replace native overlay
  constraint policy or revert to full transparent-frame confinement.
- Landing parallel sole-row sampling, same exact pixels/contact geometry. Host
  timing improvement is not a guarantee of M2 smoothness.
- Dirty-region bounds, bounded cache/scratch/display ownership, native surface reuse.
- Moved .app launcher path and explicit launch/start/end errors; package primary
  instructions remain direct .app launch. No fabricated fresh logs from older files.
`mac-preserve-manifest.json` pins these files before any future port.

Preparation checks already run:239 Mac host regression tests pass (menu lifecycle,
slider/overlay/boundary/surface/instance);43 raster tests pass (dirty/full rendering,
parallel soles, snack anger/chuckle). These test current pre-port paths, not new
feature parity or native M2 behavior. Results under `mac-tests/`.

## Final result — 2026-10-26 22:40KST

Both records ended normally with unchanged PID/start identity. Component probe:
900.052real seconds,180samples, maximum sample gap5.06668s, maximum UTC-vs-monotonic
difference0.0000024s, maximum loop gap1.09867s. Live observation:900.999seconds,
178samples,max gap5.28025s,time difference0.01265s. No sleep-sized/unexplained long
recording gap detected; this is not an OS sleep-event certification or frame-time pass.
The workload advanced605.967simulated seconds, not900simulated seconds. Initial
loop/scheduling delays, including concurrent host builds/tests, are retained.

7 complete mixed cycles plus a partial8th cycle before the final recovery switch.
Recorded1260size applications and7menu openings. Rendered active-frame counters:
tap992,shake208,groom1057,stretch630,beg889,angry1260,chuckle784,eating1260,sleep1260.
These are rendered frame counts from generated snapshots, not user click counts,
withdrawal counts or complete feeding-session counts. Recovery began at720.023s
and ran to900.052s; its3794ticks did not introduce scripted interactions.

Window medians (MiB except handles/GDI):

| Metric |180–360s|360–540s|540–720s|720–900s recovery|
|---|---:|---:|---:|---:|
| Probe private |245.230|244.465|248.166|239.586|
| Probe managed estimate |86.392|86.584|87.098|87.607|
| Heap at last natural GC |84.678|84.034|84.053|84.112|
| Managed committed |92.137|93.781|93.781|93.781|
| Probe write-combine |84.484|81.547|87.391|82.484|
| Probe handles |642.5|659.5|657|628|
| Probe GDI |26|24|26|26|
| Live pet private |247.715|246.520|246.520|246.637|
| Live write-combine |41.684|41.684|41.684|41.684|
| Live handles |678|675|679|676|
| Live GDI |80|80|80|80|

Probe private had a transient273.738MiB peak, then fell; the initial95.891MiB cold
sample is not the retained-memory baseline. Same-cycle idle medians rose to266.795
then returned to251.215MiB by cycle6; no continuing same-phase staircase observed.
Managed estimates include allocations since GC. Last-GC heap and committed values
are not current live-object counts. Final GC collection counts59/44/39 were natural;
no forced GC or trim. Allocation counter grew through active rendering as expected.

After warmup: active+spare frame slots12 throughout, combined frame storage7077888B;
display capacity589824B (aliases cache),scratch589824B,locomotion cache64,
seated distance159300B all bounded. Do not sum aliases as independent allocations.
Renderer body cache1. Sole/spread caches were0: those paths were NOT exercised by
this component replay and are not passed by an empty counter. Full desktop physics,
head drag/landing/perch/withdrawal triggers are not covered by snapshot injection.
Probe write-combine allocation bases stayed3 and live bases stayed2. Subregions
were committed/released inside those bases; retained pages alone do not identify
their exact owner or represent total GPU VRAM. No new-owner dump was warranted by
this bounded trend. Live USER count remained48–49 after warmup; probe12–15.

## Decision and remaining scope
No sustained retained-memory accumulation reproduced in this900second workload
or parallel live observation. No confirmed product defect from this run, so no
product patch, forced collection, quality/timing change or speculative cache trim.
The finite observation is complete; no repeated soak/monitor was created. Probe
and observer exited normally; accepted live PID83300 remains responsive.

Mac update preparation is complete:239Mac host +43raster +14packaging checks pass,
source-preservation hashes remain unchanged, and missing new-motion connections
are itemized above. It is ready for the Mac implementation stage, not a new Mac
binary ready for testers. No new Mac app distributed and no product source changed.

Limits:15minutes is not multi-hour/all-path leak certification. Native M2 CPU,
macOS window/upload/GPU memory, Retina, external-clamshell and native landing
smoothness remain NOT_TESTED. User logs/repeated operations were not requested.
`summary.json`, `verdict.json`, raw samples/events/completion and TRX/package logs
retain the exact evidence. No further Windows repetition is required by this result;
retest only if later code changes or a concrete new symptom justify it.

Follow-up2026-10-28: the pending Mac implementation/package stage above is completed
locally in [test10 evidence](2026-10-28-mac-test10-accepted-motion-port.md). Native
Mac gates remain open; the original preparation record is historical.

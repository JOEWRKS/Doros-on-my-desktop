# r94: native metadata allocation reuse; accepted r93 motion frozen

## Actual diagnosis

User requested remaining latency/memory work without changing accepted motion,shape,speed or complicating the implementation; warn before pausing diagnostics. Existing r93 PID144664 was still running. Explicit warnings preceded45second EventPipe sampling (`r93-profile.nettrace`,16MiB buffer,sampled-thread-time+gc-verbose) and then heap capture (`r93-before.dmp`). Neither interval is a normal cadence acceptance run. Process Responding=true after heap completion. No forced GC,install/settings changes or external uploads.

Trace analysis skips first5seconds. Leading estimated allocation ticks:DesktopWindow16,206,696B,NativeWindow15,347,992B,class-name strings13,214,848B,two HashSet growth paths7,675,200B and7,142,568B. These are sampled estimates,not exact object totals. UI sampled stacks include pixel sampling and row-worker wait; sample counts include waits and are not CPU percentages. r93's prior4callback gaps over50ms had no GC increment,so reducing allocation does NOT prove those gaps fixed.

Heap live float arrays96/85,678,704B,dead float arrays0,live byte arrays851/23,406,852B. Float storage matches prior r91 snapshot magnitude. This supports bounded image storage rather than an observed growing image-array leak; cross-build snapshots and one r93 heap do not establish hours-long/native-resource stability. Snapshot files stay local.

## Small change

Only native window metadata allocation is changed. Native APIs still query owner,class,bounds,visibility,cloaking,styles and optional executable identity every pass. The reader reuses its two private bounded enumeration HashSets. It reuses class strings and immutable NativeWindow records only when newly read values match. Two private dictionaries retain only the latest successful enumeration; scratch is cleared after swap so disappeared HWND records are not retained. Failed enumeration preserves the prior successful snapshot policy.

DesktopSceneNative reuses the prior immutable DesktopWindow at the same z-order slot only when key (including generation),bounds and every published flag match. Taskbar reveal delay is evaluated before comparison. Successful absence/PID reuse/order/flags still produce new records. Published list ownership remains immutable; no renderer,art,gravity,recovery timing,cache capacity or animation speed changes.

`tests/before.trx`:2expected failures proving duplicate record creation before modification. `metadata.trx`:79passed including native move/absence tests and taskbar/overlay/PID/failure regressions. `allocation.trx`:200unchanged windows*100captures allocate182,400B (budget500,000B); this is a controlled mapped-record test,not a claimed runtime percentage. Final related regressions and native result pending. r93 exit requested;user confirmed.

## Remaining boundary

Pixel-worker/OS compositor delays are not yet fixed or attributed to the same cause. Repeated metadata allocation is a confirmed fix candidate,not proof of solving all render latency or every memory issue. Actual same-host runtime comparison is required. Accepted r93 release remains the baseline and is not reopened.

## Candidate gate and actual launch

`tests/final.trx`:386 passed,0 failed. Against r93:420 generic raster hashes,335 synthetic landing pixel hashes and impact-relative timestamps,and75 release PNG hashes match. These are deterministic CPU frame comparisons,not actual screen validation. The final test DLL and published App DLL match SHA256 `5A40EC4F26CCA86DD6CE2F03B3DE5592BF8DC78B4968A23DF7171A3A661E280B`. Core remains `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`; three authored banks remain unchanged.

After user confirmed exit and process absence was checked,r94 started at2026-10-19 22:39:33 KST,PID139172,isolated `release/Dororong.App.exe`,1.5x transparent,300second performance capture and bounded strong-landing CPU-frame recorder. Responding=true. Start announced to user;requested early shake/tilted release/landing,cheek and treat input. No builds,traces,dumps or forced GC during this normal interval. A separate process-counter observer samples every10seconds for360seconds without a heap walk. Actual input,latency,memory and user visual acceptance remain pending until results are read.

User initially answered that behavior was unchanged,then explicitly reported another hitch on release at approximately22:41KST. The latter supersedes any broad motion-pass inference. Requested one tilted and one upright release while recording. The current CPU image recorder is strong-landing-triggered,not release-triggered;100ms state snapshots alone cannot establish subframe continuity. Use per-update/per-callback timings to isolate observed gaps and do not claim the user's hitch disproved by deterministic pixel equality.

User then asked to leave the small release hitch alone and focus only on memory. This is a deliberate deferral,not a fix or pass. No motion or renderer change was made in response.

## r94 actual memory evidence

`native.json`:300012.4571ms,12 Body presses and1 RightCheek press;69 strong-landing CPU frames. These counts establish input delivery,not automatic visual acceptance of every motion. The bounded recorder reserved56,623,104B. Total allocated bytes689.55MiB across the run;after the first5seconds,1.964MiB/s vs r93 prior3.595MiB/s. Workloads and durations differ (r93 input0 vs r94 input13),so this is an observation,not a controlled percentage improvement. After60seconds private bytes varied260,411,392–315,445,248 rather than growing monotonically. Managed peak185,635,144B includes temporary diagnostics and retained cache;private bytes are not equivalent to live managed objects.

`process.json`:non-pausing10second samples for351.38seconds. Last sample private267,612,160B,handles633. After the normal recording/observer ended,a fresh warning preceded `r94-after.dmp`. Offline live totals:r93 111,867,284B vs r94 111,301,471B. r94 live float arrays95/84,868,680B (r93 96/85,678,704B),dead float arrays0;live byte arrays1150/23,454,179B (r93 851/23,406,852B),dead byte arrays478/42,064B. The recorder's large buffer is no longer live. Example roots lead through the active presenter to HighResolutionGrabRaster and its bounded HighResolutionLocomotion frame cache;no old raster bank is established as leaked. These are comparable-scale snapshots,not proof of hours-long/native-resource stability. Heap collection itself touches pages;post-dump working set is not a normal-cadence comparison.

After a separate warning,`r94-profile.nettrace` used the same45second16MiB/sample+GC profile as r93. Existing analyzer excludes first5seconds and reports top20 estimated allocation stacks. NativeWindow/DesktopWindow records,class strings and HashSet growth no longer appear among those top20;absence from that list is not a zero-allocation claim. Leading residual metadata allocation is NativeWindow[] List capacity growth6,076,200B estimated;captured taskbar membership predicate2,025,400B. This motivates a tiny r95 follow-up:reserve the already bounded enumerated count and use a direct membership loop. No new caches,physics changes or forced GC. r95 test/native application pending at this entry. r94 remains running/responding,installation/settings unchanged.

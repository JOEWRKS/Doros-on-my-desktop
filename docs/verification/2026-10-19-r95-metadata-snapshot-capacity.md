# r95: memory-only follow-up, behavior unchanged

## Scope and evidence

User explicitly deferred the small release hitch and requested memory cleanup only. Keep the hitch unresolved/deferred,not passed. r93 accepted shape/speed and r94's unchanged renderer remain locked. No installation or settings changes.

[r94 evidence](2026-10-19-r94-native-metadata-reuse.md) documents actual45second allocation traces,300second interaction recording and live heap snapshots. Live managed objects did not grow between those r93/r94 snapshots (111,867,284 vs111,301,471B);large recorder buffers no longer live. This does not certify hours-long/native-resource leak freedom. The r94 sampled residual NativeWindow[] List growth was6,076,200B and taskbar membership predicates2,025,400B in the top20 allocation stacks. Estimates are sampled and cannot be equated with retained memory.

## Minimal implementation

Only `DesktopSceneNative.cs` changes relative to the published r94:

- Reserve `min(enumerated HWND count,4096)` slots in the new snapshot list. Preserve list ownership;do not pool or mutate published arrays. Legitimate extra taskbars may still grow the list normally.
- Replace the captured `Any` taskbar-membership predicate with an equivalent direct loop. Preserve ordered native queries,taskbar discovery and invalidation.

No renderer,animation,physics,cache size,quality or speed change. No forced collection or working-set trim.

## Verification

New real-native-window test checks reserved capacity and independent immutable historical snapshots after order reversal. First run failed because the test registered one Win32 class name five times;fixed fixture uses unique names and disposes partial setup in `finally`. Preserve failed `tests/metadata.trx` (139pass/1fail). Final `tests/metadata-final.trx`:140pass,0fail,including taskbar omission/order/move,PID/failure,overlay and background scene reuse. Existing xUnit1031 warnings remain in unrelated tests.

Published/tested App DLL both `F45F0633233D9187DC6CDA7CFB260D26C7156D7CC73911ED616958B373977EF0`. Core unchanged `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`. r94's420 raster/335 landing/75 release deterministic comparisons remain previous evidence,not newly rerun r95 checks.

## Actual launch, memory-only

User confirmed r94 exit;process absence checked. Started isolated r95 normal transparent1.5x at2026-10-19 22:52:39 KST,PID40712;Responding=true. Only `--high-resolution-grab=1.5`,NO performance capture,NO frame recorder,NO interaction-test host. User told not to operate for this memory-only run. External process-counter sampler every10seconds/300seconds,`artifacts/render-cost-20261019-r95/process.json`. No trace/dump/forced GC during this normal interval. Actual memory result pending. Any subsequent potentially pausing diagnostics require a new warning and are outside the normal interval.

## Actual result and limits

`process.json` contains30 observations over290.93seconds within the300second observation process. Private memory145.71MiB atfirst observation,166–167MiB from40–171seconds,then202.38MiB at181seconds and205.36–205.95MiB over191–291seconds. This is a real upward step,not a flat five-minute graph. Handles598–641,last618;not a monotonic handle-growth pattern. No internal diagnostics buffer exists in this run. Differences vs r94's diagnostic/interactive run are NOT an attributable percentage improvement.

Fresh warning before22:57:52+ heap collection,normal observation already complete. `after-normal.dmp`:live25,193 objects/107,880,950B;float arrays88/82,401,712B,byte arrays141/22,913,074B. No dead actual float arrays,dead byte arrays221/677,624B. Exactly one HighResolutionLocomotion and one HighResolutionGrabRaster;no live RuntimePerformanceCapture or LandingFrameCapture. Locomotion dictionary `_count=64`, `_freeCount=0`,matching its existing64-entry bound,not a new memory limit. This corroborates bounded large cache retention;the single end snapshot cannot allocate every byte of the earlier38MiB step to one cause. Private bytes include committed managed slack/native/WPF memory and are not live object totals. Do not call the entire step a leak or claim every native retention source fixed.

Confirmed:redundant metadata record/HashSet/string creation reduced in r94,remaining List growth/predicate allocation removed in r95 code,140 related tests passed,r95 actually ran for the normal5minute memory observation and remains responding. Approved movement/shape/speed/Core/banks untouched. Not confirmed:hours-long native/managed growth,zero remaining render latency,or resolution of the explicitly deferred small release hitch. No forced GC,working-set trim,cache-size reduction or installation/settings mutation. Final running candidate is r95 PID40712;do not silently substitute r94 results as r95 interaction acceptance.

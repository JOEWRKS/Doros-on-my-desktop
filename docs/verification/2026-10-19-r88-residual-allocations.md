# r88 — remaining allocation and rendering cadence investigation

## Scope

User asked to preserve r87 accepted precontact landing, shape, art and speed while fixing remaining rendering delays and memory growth, with native verification. No Core/animation/asset/settings/installer change. This is not authorization to reinterpret the accepted motion.

## Evidence before modification

- Running r87 PID97856 was still alive after the original300-second recording. Before heap collection private256,856,064B, working set325,070,848B: lower than its previous approximately290.61MiB private endpoint, not proof of an unbounded leak or of leak absence.
- `artifacts/render-cost-20261019-r88/r87-before.dmp`:95 live float arrays84,868,680B;29 dead float arrays23,490,696B. Bounds-cache41 dead containers/entry arrays also present. Distinguish retained objects, awaiting-GC allocations and process-private memory. Snapshot taken outside acceptance measurement; diagnostics can pause the process.
-45-second EventPipe `r87-profile.nettrace` with sampled-thread-time/gc-verbose,16MiB buffer. Existing r80 analyzer excludes first5 seconds; sample estimates, not exact byte census. Top stacks include identity dictionary growth19.52MiB, per-window closure6.79MiB, and BorrowBody float arrays7.72MiB. Other native/window strings and geometry allocations remain.
- r87's two callback intervals over50ms had raster work only1.05/1.53ms, worker wait~.055ms. First coincides with gen0+gen1, second has no GC count change. Do not label both as slow raster calculation or claim GC as sole cause. WPF callback cadence is not DWM presentation FPS.

## Reproduced causes and fixes

1. Sleep effect `Get` and render-only `BorrowBody` shared a64-entry FIFO. Effect entries are immutable, so evicting them cannot provide a reusable body array; repeated sleep/walk creates fresh810KB slots. Add a separate single render-only effect scratch, reused after synchronous row joins; exact decompressed pixels unchanged. General `Get` remains immutable. Do not raise cache capacity or change frame selection.
2. DesktopSceneNative rebuilt/growed identity dictionaries and list capacity on every80ms acquisition. Double-buffer only the private identity map, clear/reuse scratch and swap after successful capture. Published snapshots remain separate immutable lists. Preserve successful absence/PID reuse and taskbar visibility clock. Move clipping out of the per-window LINQ closure and keep first-largest monitor overlap tie behavior. DesktopSceneSource allows only one capture in flight.
3. RasterAlphaBounds removed/reinserted the same mutable source key, leaving weak-table tombstones and replacement containers. Retain a weak entry and invalidate its value, with a new immutable Box only when rescanned. Do not mutate previously returned Box or retain weak source keys strongly.

## Verification so far

- `tests/allocation-before.trx`: both new budgets failed on old product: sleep/walk65,678,528B;100 snapshots of200windows7,268,000B.
- `tests/targeted.trx`:127 pass. Same allocation tests after fixes:66,584B and1,941,600B respectively. Includes body buffer immutability, scene identity/absence/taskbar/occlusion and weak-source lifetime checks. This isolated reduction is not an identical-workload native performance percentage.
- Additional tests compare all65 sleep effect/body source pairs at five sizes to immutable reference, preserve unrelated retained effects and bounds, and check bounds-entry reuse/recomputation.
- `tests/final.trx`:627 pass,0 fail/skip,3m22s. Runtime/platform/perch, buffer lifetime/reuse, cache, new allocation/source-equivalence and render checks. The127 targeted checks overlap and are not added to627.420 render hashes and80 paw hashes match references; five-size synthetic clips each67frames (335total) match r87 raw pixel hashes and relative timestamps exactly. Native measurement pending.
- Core SHA256 remains r87 `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`.
- Published and tested App DLL SHA256 both `5A680C32FCE2D81520B1033BDFCF3AA4F36E2EE2546FA4D08307A3A450B6D019`. All three source art banks unchanged.

## Native / remaining boundary

User confirmed tray exit for replacement; no Dororong.App process found afterward. Normal transparent1.5x r88 PID32932 launched2026-10-19 18:41:04 KST,300seconds with `native.json` and `native-frames/`. Actual start announced; first minute cheek/grab/drop requested, then passive observation until about18:46. No heavy builds/tests/traces/dumps during this acceptance measurement. Actual timing/memory and user interaction preservation must be checked before acceptance. All-lag-fixed or long-term leak-free is not established by the allocation tests. No external screen recording requested; internal CPU capture available for the landing boundary.

During recording, user answered **“문제 없었음”** to whether cheek release/shaking/landing kept accepted r87 motion and speed without new stutter or shape issues. Record as user confirmation for those interactions, not agent direct screen inspection or a memory/performance pass. Later phase requested no further pointer interaction for passive memory sampling.

## Native result and post-capture heap

- `native.json` SHA256 `3080B63EC1F20C5122C628A2A6E98E1D3B0B32D49D1F6F10E3E9B132A636E3FE`,300011.7632ms,CPU348296.875ms. Eleven inputs: Body8,RightCheek3. Last input85.623s/last Dragged state sample85.942s, so do not label the whole interval after60s unmanipulated. Five strong physical landings all preceded by airborne spread and first physical Landing spread1.
- First clip `native-frames/landing-20261019-094108-8c0cd4d2fe7947e8b8138f74679a4ab5/`: no error,post-impact-window. Directly opened013/014/015/016/017 PNGs. CPU output at-68.23ms upright,-36.82ms spread.67266,-21.68ms .99053,contact0ms1,+25.86ms recoil.94071. Shape/timing sampling differs with native pointer path and tick schedule; synthetic exact timing/pixels remain identical. CPU frames are not monitor/DWM scan-out.
- Excluding first5s: tick35319,mean8.353ms/p9930.0518/max63.6119,over50ms1. Update work max33.1005ms,over50ms0. Callback35839,max73.0163ms,over50ms6. Dragged max42.741ms,Idle46.9845,Sleep44.9553,Walk63.6119. Do not claim latency improvement: r87 had0 long ticks/2long callbacks and different input/state workload.
- The single long tick at110793.416ms was Walk/Grounded,update work11.8448ms. Corresponding callback73.0163ms had raster1.236ms,workerWait.0222ms,gen0/1/2delta0. All six long callback samples had GC deltas0. Another landing-recovery callback at29.362s had15.83ms raster/7.87ms worker wait. Sampling/sole/wait overlap; do not add them. Remaining callback scheduling/presentation delay not isolated to a proven code cause by this capture; do not call it fixed or certainly external.
- Total allocation1546.985MiB,gen2six. Managed peak190.3599MiB,near299s183.731MiB. Private30s267.52MiB,150s266.31MiB,299s273.43MiB. r87 peak209.49MiB/endpoint171.38MiB and total1781.24MiB: lower peak/total here, higher managed endpoint. No controlled improvement percentage or monotonic leak conclusion. Both runs include the54MiB internal frame buffer and growing diagnostic sample lists during recording.
- After recording, collected `r88-after.dmp` (not part of acceptance timing).102 live float arrays99,178,848B; **zero dead float arrays** at this snapshot; byte arrays23,449,102B,so diagnostic54MiB frame storage no longer rooted. Locomotion cache64 entries/all64 reusable,one effectScratch835. One bounds weak-table container, no former41dead-container buildup. One FlutterLayers plus its arms also retained after interactions. Live floats are higher than pre-r88 long-running snapshot84.87MB; different warmed interactions/caches, not proof all retained-memory issues disappeared.
- User confirmed motion preserved; r88 remains running normally, installation/settings untouched. Specific repeated allocation fixes verified. **Residual callback/tick latency and long-term memory stability remain open**, along with other native sizes/monitors and full interaction combinations. Next diagnostic target is the long interval outside measured raster work (dispatcher/composition scheduling versus other UI work), not another change to accepted motion speed or curve.

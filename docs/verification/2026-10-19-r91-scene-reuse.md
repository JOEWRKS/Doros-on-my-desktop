# r91 immutable scene reuse / external render delay investigation

## Scope and evidence

Accepted r87 pre-contact spreading and r90 motion/shape/speed remain fixed. No artwork, Core motion clock, installation or settings edits. r90 native PID142928 was observed alive before diagnostic work. User was explicitly warned before the 45-second EventPipe trace and before the heap collection; both are outside r90's acceptance recording.

Evidence lives in `artifacts/render-cost-20261019-r91/`:

- `r90-profile.nettrace`, `r90-profile.analysis.json`: trace starting21:12:41 KST,45seconds. Analysis skips first5seconds. Allocation tick estimates include16.52MB of DesktopWindow mapping and5.44MB of PlatformGeometry int arrays. UI sampled stacks include HwndTarget.UpdateWindowSettings through Window.Left/PetLoopHost.SetWindowPosition. Sample counts include blocked threads and are not CPU percentages or proof of the exact prior119ms gap.
- `r90-long.dmp`: collected21:13:44 KST, about21m23s after launch. Live float arrays97/86,488,728B exactly match the earlier post-recording r90 heap; no dead float arrays listed. Live byte arrays1181/23,778,276B versus1183/23,794,724B earlier. One HighResolutionLocomotion and one HighResolutionGrabRaster instance. This rules out accumulation of these particular arrays over this interval, not indefinite/native/WPF leaks. Bounded locomotion cache retention is not itself a leak.

## Changes and rejected hypothesis

Fresh desktop acquisitions preserve revision/freshness but share our own immutable ordered window/monitor collections when their complete record values are unchanged. Producer-owned arrays are copied, never retained directly. Only latest snapshot is retained. Coordinate mapping reuses collections only under the same map; perch/height geometry reuses them only when both collections and required height agree. Changes to bounds, topology, flags, identity, order, DPI mapping and expiry still invalidate relevant work.

`PetLoopHost` now measures accumulated Window.Left/Top setter duration; native frame diagnostics expose `windowMoveMs` separately. This is attribution, not a claim of eliminating HWND/DWM latency.

Tested subpixel HWND movement hypothesis before any viewport change:19 same-physical-pixel logical targets produced zero WM_WINDOWPOSCHANGED messages, while logical coordinates stayed exact. WPF already suppresses these native moves, so no viewport/native positioning change was made. `move-before.trx` preserves the passing negative result.

Repeated fresh-but-identical200-window scene test allocated2,594,680B before the fix and failed the budget (`scene-before.trx`); after reuse it allocates62,680B over100 acquisitions and retains current revision/time. Targeted36 tests pass (`targeted.trx`), including changed metadata, mutable producer arrays, map/readback and source expiry. This is controlled allocation evidence, not an actual-runtime improvement percentage.

## Candidate validation / native recording started

User authorized replacement after tray exit; process absence checked. `tests/final.trx`:651 passed,0 failed.420 general/80 paw/120 effect raw pixel samples equal r90. Five-scale synthetic landing manifests contain335 frames with identical sourcePbgra32Sha256 and relativeToImpactMs to r90. These are automatic parity results, not new direct visual inspection of all samples.

Tested and published App DLL SHA256 both `E4C1A516E44251B8A1035F89F9A5478BBBA3F2083FBA5042B347FBF5E586A106`. Core SHA256 `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D` and grab/hunt/locomotion bank hashes match r90. Separate `release/` publish; installation/settings untouched.

Normal transparent1.5x r91 launched21:28:17 KST,PID144592; Responding=true at21:28:19. Start announced immediately.300second native capture plus bounded landing recorder; requested high drop, shake, cheek and snack. An external10-second process-memory sample runs for600seconds without target suspension or forced GC. No builds, traces or heap collection during the native acceptance interval. Native results still pending at this checkpoint.

No overall latency or memory pass. Remaining native metadata allocations, dispatcher/HWND delay attribution and longer retention remain open. Actual CPU bitmap frames, user observations and DWM scan-out are distinct evidence categories.

## Native result: latency remains, not accepted as fixed

`native.json` SHA256 `A3C035AB9FDEC04F61E8AA2022C43A014C2C408E8B88CEA2B06A6A54CB90F164`,elapsed300008.4971ms,CPU366171.875ms summed across cores. Four Body inputs; ordinary landing at72.731s,strong landings at138.738/143.397/148.027s. No cheek/snack inputs recorded, so those native paths are unverified in this run. User reported falling stutter while doing other PC work, not unchanged-motion/performance acceptance. Aggregate CPU51% at21:32:21 is a single later snapshot, not proof that competing work caused the hitch; GPU/DWM timing is not captured.

Excluding first5s:tickmax79.1372ms/11over50,workmax78.6731ms/8over50,callbackmax91.5139ms/23over50,rastermax59.4967ms/1over50,workerwaitmax53.2862ms/1over50,layoutmax1.4541ms,diagnosticmax.5092ms. Window settermax73.0968ms/mean.4416ms. At114.100s callback91.51ms contained window setters73.10ms,raster5.18ms,workerwait.041ms andGCdeltas0. At110.939s workerwait53.29ms explains most of59.50ms raster elapsed. These distinct stalls are not fixed by snapshot reuse and do not prove one universal OS/GPU cause. Prior trace independently places sampled UI stacks in HwndTarget.UpdateWindowSettings through the positioning callback. A direct SetWindowPos replacement is not presumed safe or sufficient: WPF still processes positioning messages. No unsafe sync-suppression or unvalidated viewport replacement was applied.

Allocated1058.96MiB,Gen2four;managedpeak178.086MiB/end174.896MiB,private60s250.184/end260.258MiB. Different input/state mix from r90 prevents normalized improvement claims. Record lists and bounded landing-buffer storage are part of capture memory.

Strong landing clip `native-frames/landing-20261019-122818-83258eb4c6b94f9bb4cf9c1b0cdd536f/frames.json` triggered,errornull. Directly opened014/015/016/017/018 PNGs:contact-54.77ms upright airborne, -18.95ms airborne spread.8198,contactspread1,+18.94msrecoil.96,+40.02ms.95. All three strong tick entries have pre-contact spreading and first contactspread1. This verifies preserved CPU output at the inspected boundary, not absence of falling stutter or DWM frame drops.

After explicit pause warning, collected `r91-after.dmp` outside capture,finished21:35:40. App Responding=true afterward. Live float96/85,678,704B,dead float arrays0 (dead weak-table containers are not float arrays),live byte1118/23,436,248B,one HighResolutionLocomotion,no live RowBatch listed.

## Extended retention / final boundary

External observation `process-memory.json`:60 samples from21:28:54 to21:38:46 KST,about37seconds after app startup through10m29s. Post-export private memory about263.2MiB,then250.3MiB,then238.5MiB at last sample. Handles at last sample615. First warned heap collection is within this external observation and can affect working set/CPU; it is outside native cadence recording. These samples are process counters, not live heap bytes or a guarantee about eventual OS page release.

Second heap collected only after a new explicit pause warning,completed21:39:27 (~11m10s since app launch). `r91-long.dmp` live float96/85,678,704B exactly unchanged from first heap,dead float arrays0;live byte1114/23,411,544B decreased slightly,one HighResolutionLocomotion,no live RowBatch. Responding=true after collection. Snapshot references show the locomotion owner retaining its cache,closedHead,bodyScratch,effectScratch and compressed bytes,not an ever-growing number of renderer owners. No product forced-GC/cache eviction workaround or art reduction applied.

All three high-drop windows had callback gaps:135–139smax42.21ms,140–144smax41.53ms,145–149smax33.65ms. Their window-setter maxima14.76/15.51/20.33ms are separate from the91.51ms callback at114.10s; do not attribute the latter to the user's specific fall. CPU frames establish preserved spread, not smoothness. User's falling-stutter report remains a failure of the requested latency outcome.

Cache fields in the final heap:dictionary `_count=64,_freeCount=0`;reusable-key set `_count=64,_freeCount=0`. These are the bounded64 reusable frame slots,not unbounded retained motion history.

r91 remains running normally; recorders and external observer completed. Confirmed fix is unchanged-scene allocation/rebuild reduction,not all renderer external latency. Accepted art/Core/shape/speed is unmodified with automatic parity and inspected landing boundary. Native cheek/snack, GPU scan-out, all-scale native cadence and hours-long/native-resource leakage remain unverified. Next is synchronized window/compositor positioning and active pixel-worker stalls under competing load,without bypassing required presentation synchronization or changing motion. Prior r90 performance/retention limits remain historical; this report replaces only current runtime status.

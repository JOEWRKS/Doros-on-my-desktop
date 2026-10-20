# r89 — effect buffers and valid-layout fast path

## Scope and evidence

User requested fixing remaining update delays and retained-memory causes while preserving accepted r87/r88 motion, shape and speed. No art, Core, curve, frame selection, snack speed, settings or installation change.

`artifacts/render-cost-20261019-r89/r88-long.dmp` from running r88 PID32932:
- Process private memory497,156,096B before capture, versus about277MB shortly after the previous acceptance recording.
- Live float arrays still102 /99,178,848B, exactly the same as `r88-after.dmp`; dead float arrays now217 /175,775,208B. The observed increase is mostly waiting-for-GC large arrays, not evidence of growing live float roots. This does not exclude other leaks.
- User confirmed **“방금 했어”** when asked about snack chasing/eating after the earlier recording. Existing snack/drool effect creates a fresh203KB byte array and810KB float array and RenderTargetBitmap per render, including repeated sole/display queries. Isolated reproduction below proves this allocation path; not every dead object in the dump has an allocation history.
-40-second `r88-profile.nettrace` with sampled-thread-time/gc-verbose,16MiB buffer, after heap collection and outside acceptance timing. Allocation samples excluding first5s:17.79MiB `WindowAutomationPeer.GetNameCore`, called from `AutomationPeer.UpdateSubtree`→`ContextLayoutManager.fireAutomationEvents/UpdateLayout`→`DororongPresenter.DrawHighResolution`→`PetLoop.OnTick`. This proves forced layout re-enters automation traversal, not that it alone caused the earlier63.6ms interval. Native metadata/geometry allocations also remain. No snack float stack among top25 during this later40-second sample; snack may already have finished.

## Changes

- HighResolutionSnackEffects.Buffer owns one DrawingVisual, source-resolution RenderTargetBitmap, byte array and float array. RenderOpen replaces the commands; Clear removes old bitmap contents; exact accepted geometry, colors, ages and sample math retained. Cache only the exact same eating/drool pair for redundant geometry/display preparation. Invalidate weak bounds on each changed image. General Render still returns independent arrays for reference callers. Raster rows are synchronous before reuse.
- DrawHighResolution/ApplyHighResolutionPlatform call UpdateLayout only if measure or arrange is invalid. Valid layout already supplies correct actual size and transform. Dirty size/DPI/parent layout still flushes before geometry/hit queries; normal WPF automation processing is not disabled. No global accessibility setting changes.
- Record layout flush count/time separately; prior raster cost excluded the following UpdateLayout. Also record the performance recorder's own callback cost (`diagnosticMs`) to avoid attributing diagnostic overhead to the product. No physics cadence/timer modification.

## Verification

- `tests/effects-before.trx`: allocation test failed40frames40,658,320B; other two tests passed. Before-product120full render PNG/raw hashes saved at five scales, two effect kinds,12ages each.
- `tests/targeted.trx`:30pass. Warm40frame allocation117,624B after.120effects hash-identical to before; fresh vs owned effects compare empty/changed/eat/drool states to detect uncleared pixels and stale bounds. Existing sleep/snack/attention/attached paw and hit/sole checks included.
- `tests/final.trx`:644pass/0fail/0skip,4m10s. Includes runtime/platform/perch, all new effect tests, five-size valid/dirty layout and hit capture checks, lifetime/remaining-allocation, high-resolution state/host/sole/live surface checks. Earlier30 overlap, not added to644.
-420general hashes,80paw hashes and120effect hashes match their pre-change references.335synthetic landing frames at1/1.1/1.25/1.35/1.5 match r88 raw hashes AND relative timestamps exactly.
- Core hash unchanged `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`.
- Published/tested App DLL both `251B96D7A556D142372DF3836A033323BF2DA205D4A80FEC3951E1E5AF55827B`.

## Native status / limits

At19:02KST r88 PID32932 was still running; this was the candidate-only boundary. After the user's exit confirmation, process absence and candidate hashes/644-test result were rechecked. r89 launched normally transparent at1.5x, PID13864,20:11:06KST,300-second performance capture plus bounded internal landing recorder. No installation/settings change or forced termination.

### Actual run

- `artifacts/render-cost-20261019-r89/native.json`, SHA256 `9E0A8D77792912D3C67ECE526C2851DE581FF8B635E38B9F6EC535A6B01332D8`, elapsed300017.7639ms. User said **“같았음”** when asked whether snack/cheek/grab/landing motion and speed remained the same. This is user observation, not direct DWM inspection.
-30 inputs: Body9,RightCheek7,ClickOnly8,PerchLeftPaw3,PerchRightPaw2,snack-toggle1. Snack toggle is logged; successful chase/eating appearance is covered only by the user's response, not a separate eating event field. Last input163417ms, so the requested90-second interaction/passive split was not actually followed exactly.
- Five ordinary landing entries at38.529/58.497/67.332/79.021/101.047s. **No extreme landing**. Internal clip `native-frames/landing-20261019-111108-7d99c34a618d487b97a6a847f1de2d05/frames.json` says triggered=false,error=null,capture-ended. No native landing PNGs to inspect; strong-landing frame acceptance remains unverified in this run. Earlier r87/r88 acceptance and identical synthetic frames are not substituted for this missing sample.
- Excluding first5s: tick interval max156.7224ms,10 over50ms; work max156.3926ms,5 over50ms; WPF callback max156.6761ms,17 over50ms. CPU419875ms summed across cores. Different interaction workload from r88 prevents a normalized improvement/regression percentage; these data **do not pass residual latency**.
- New instrumentation: layout total3149.1994ms,max7.9324ms,31142 forced flushes after5s. Most live renders still invalidate layout, so the valid-layout fast path does not remove most native flushes. Do not describe the conditional guard as a complete layout fix.
- Recorder callback itself took88.9879ms at117727.9637ms and44.6351ms at134808.401ms. This confirms substantial diagnostic overhead inside those callbacks; it does not identify whether process queries, snapshot gathering or scheduling caused the entire duration. Other slow samples include workerWait49.0582ms/raster55.6883ms and workerWait68.3751ms/raster73.1088ms. Not all delays are recorder-only or GC; investigate independently before another fix.
- Total allocation1750.718MiB,Gen2 collections11. Managed peak190.853MiB,end-near190.320MiB. Private memory313.86MiB at30s,318.29MiB at150s,319.37MiB at299s. Managed memory also retains the opt-in recorder's33926 frame/33577 tick/2817 state samples during recording; the growth is not automatically product-object leakage.

### Post-recording heap and diagnostic pause

`r89-after.dmp` was collected **after** the saved300-second capture. This operation can suspend the app; the user noticed a stop and asked whether it was intentional. Explained that heap collection caused a diagnostic pause, apologized for not warning immediately before it, and checked PID13864 responding=true afterward. This pause is outside `native.json`; do not count it as an observed in-capture regression or hide diagnostic impact. Warn before any future live dump.

- Live float arrays97 /98,331,928B; dead float array listing empty. Exactly one live HighResolutionSnackEffects.Buffer. Live byte arrays875 /23,796,223B; the54MiB landing recorder storage is no longer retained as live byte arrays after export. These findings support bounded effect-buffer reuse, not an all-process leak-free claim.
- Earlier r88 long heap contained217 dead float arrays/175,775,208B after snack use. Different GC timing and workload mean the r89 post-run absence cannot establish a long-term leak improvement percentage. No forced collection was added to product code.

Native motion/speed unchanged by user confirmation; remaining scheduling/worker waits, diagnostic cost, live layout invalidation, long-duration retention and strong-landing frame verification remain open. App remains running, capture ended. Next: remove observer-induced stalls without changing accepted rendering, isolate live layout invalidation/worker wait, then retest with declared diagnostics and a strong-landing sample. Internal CPU bitmap evidence is not DWM presentation; no external recording request.

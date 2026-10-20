# r96 shared detached snack image — actual repetition measured; r97 menu follow-up pending

## Authorized scope

User requested reducing snack decoding/cache retention and investigating remaining memory growth, preserving original art, image quality, accepted motion/shape/speed. Announce pausing diagnostics immediately before collection. Small release hitch is excluded by the user, not a target for this change. No install/settings/power changes.

Baseline: [r95 diagnosis](2026-10-20-r95-snack-native-retention.md), full snack image pixels in three native payloads (~18MiB), total original private rise59.75MiB. Other native ownership was not established.

## Change

`SnackImage.cs` owns one lazy, frozen, detached premultiplied BitmapSource, shared by SnackCursorWindow, HighResolutionSnackEffects and legacy SnackRenderer. The loader closes its resource stream and does not retain the BitmapDecoder/BitmapFrame/FormatConvertedBitmap chain after copying the full-resolution source pixels. Cursor initialization waits until first visible use.

No downsampling/DecodePixelWidth was applied: preserving exact accepted sampling takes priority over speculative savings. Source PNG bytes/dimensions unchanged. This removes the long-lived decoder/cache-chain references; actual collection time and private/native savings need runtime evidence. No forced GC or working-set trim in the app.

App candidate SHA256 `457F4D755204FA2EE4DF209F4199D1F5D9AC21125A0F75402BD794A78895AFF7`; Core unchanged `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`.

## Verification so far

- Baseline test-directory App DLL hash matched published r95 `F45F0633233D9187DC6CDA7CFB260D26C7156D7CC73911ED616958B373977EF0` before baseline export.
- Baseline effect export executed before rebuilding. Candidate120 effect frames,5 scales, match baseline SHA256 exactly.
- `SnackImageRetentionTests`: shared identity/frozen/detached type, all1254×1254 premultiplied pixels equal original conversion. Cursor/chewing rendering comparisons at1/1.1/1.25/1.35/1.5/2/2.5/3 scales,4 output sizes and4 fractional offsets per scale:256 comparisons equal.
- First targeted run12passed/0failed (`artifacts/snack-memory-20261020-r96/tests/snack-first.trx`). Baseline export is a separate setup run, not an additional distinct regression case.
- Wider related regression in progress. Existing xUnit1031 warnings are unrelated test blocking-operation warnings, not runtime faults.

Related regression finished223passed/0failed (`regression.trx`,2m36s); the initial12 are included in this broader set and must not be added again as distinct cases. No agent live visual/interaction verification yet.

## Actual run started

User answered readiness/exit request; installed process absence checked. Started isolated normal transparent1.5x r96 at2026-10-20 15:14:58.810KST,PID32936/start ticks639280736988104765,external observerPID33700. Existing300second performance/input capture is enabled; pixel-frame recorder is not. External observer runs600seconds with2second counters. `run.json`/`samples.jsonl` confirm the process and initial samples. User was told start time and asked to perform about5snack chase/eat repetitions first. Their completion/visual feedback is pending. Recording completion alone does not authorize exiting the pet.

Around15:16:40KST user replied "끝." to the snack repetition request. This records user-reported completion of that requested sequence, not an independently measured five complete meals or visual acceptance. Then requested head grab/shake/release/landing and cheek pull/release about5times each. Exact input/phase coverage will be read from the completed recorder. No image-quality feedback was included in the first reply.

Second requested sequence completed per user "끝"; then, asked explicitly whether snack quality and character shape/speed changed, user replied **"문제 없었음"**. This is user acceptance of this run's appearance/behavior, not agent direct screen verification or memory acceptance.

`runtime.json` completed300021.525ms,16native input events:5snack-toggle,6Body,5RightCheek. First input23894.791ms,last226888.543ms. The state sampler records generic Idle/Walk/Dragged, not individual meal-completion counts; five full meals remain user-reported. No pixel recorder. Max tick interval117.0896ms,three intervals>50ms: do not claim zero render latency. Around minute5–8 private bytes stay approximately287MB; no total-private-memory reduction claim. Await final external observation and announced post-observation heap to distinguish retained image ownership from recorder garbage/GC slack.

## Native verification plan and limits

Use isolated Release, normal transparent1.5x, existing300second opt-in input/performance recorder without pixel-frame capture. External2second counter observations continue600seconds, with PID start ticks checked. Recorder allocations affect first5minute private-byte measurements; separate post-recording counters and heap owner evidence. Ask for repeated snack cycles first, then normal cheek/head/landing actions, record user completion boundaries separately from instrumented inputs. Stop/trace/dump only with fresh warning outside normal samples where possible. No automatic native inputs.

Do not equate synthetic pixel equality with live acceptance, or code reference removal with measured memory savings. Investigate remaining large native regions only from observed evidence. Need first-use/repeated-use/end ownership comparisons before marking memory fixed; hours-long retention and all devices remain outside this short run.

## Completed observation and offline ownership (supersedes pending status above)

`completion.json`:600.568829seconds,298samples,normal duration completion,ended15:25:01.104KST. Sample span598.507182seconds,max gap2.054707seconds; same PID/start ticks. Minute0–9 private-byte medians:
232929280,247529472,268232704,266983424,285851648,287162368,287350784,287309824,287244288,287244288.
The final five minute medians vary by188416bytes (~0.18MiB); last private287244288bytes (273.94MiB). This is short-run plateau evidence, not total reduction versus r95 or proof of no leak. Startup allocations and the first five minutes' recorder differ from the historical r95 workload.

After normal observation, explicitly warned user of possible brief stop, then collected `after-repeat.dmp`. Collection finished and app remained responding. No forcedGC/trim/restart. Offline findings:

- Live managed126169046bytes; live float102arrays/99178848bytes; committedGC144314368bytes. Do not equate GC committed slack or private bytes with live retained application objects.
- Static `SnackImage.source` Lazy points to CachedBitmap `020ba1392298`,1254×1254, `_source=null`, safeMIL`020ba1392420`→native`020b9eb45a18`. No retained source decoder chain. Eight live BitmapImages are96×96, not the snack. Live PngBitmapDecoder count3 versus4 in old r95. Evidence:`detailed-source-owners.txt`,`snack-flutter-ownership.txt`,`float-and-snack-handle.txt`.
- Raw snack pixel matches dropped from three oldr95 payloads to two r96 payloads. **This alone cannot quantify live native savings:** dump memory can include freed native heap contents. Remaining rawBGRA payload ownership is not established; don't report two retained copies or a proven6/12MiB saving from byte matching.
- One FlutterLayers instance retains plate+twoarms and their three scale-compensated arrays:6×2250024=13500144bytes (~12.87MiB). Old r95 snapshot had no FlutterLayers. The current group is bounded by one instance; therefore total float growth across these differently exercised runs is not evidence of snack leakage. No speculative clearing of accepted animation caches was implemented.
- Grab/spread dictionaries each4entries. Disposed RuntimePerformanceCapture has five zero-length lists pointing to the same empty object array; no large captured sample lists remain rooted there.
- GPU counters after inputs report Dedicated37367808/Shared21340160/TotalCommitted63459328bytes. These may overlap other accounting and are not additive to processprivate or definitive allocation ownership.
- r96 remains running,responding; user reported appearance/motion unchanged. No agent live-screen inspection. No WPR/IFEO/native tracing or power/installation/settings change.

Remaining: native allocation stacks for the original gross increase are unknown; short plateau doesn't close hours-long/all-actions memory verification. Source chain removal is verified, exact total savings are not.

## r97: omitted menu consumer, isolated follow-up

Repository search found the snack menu template still loading its own URI BitmapImage. A new actual WPF template test failed before fix (`menu-before.trx`,expected shared CachedBitmap,actual URIimage). This demonstrates a separate load path, **not** that the current r96 dump's remainingBGRA payload belongs to the menu; no snack BitmapImage was live in that snapshot.

Changed only menu source to public `PetContextMenu.SnackIcon` returning the same internal shared source, referenced by x:Static. Template dimensions36×36,HighQuality sampling,style and behavior unchanged. Test instantiates five templates and checks shared identity; extended exact pixel checks to36px,now8scales×5sizes×4offsets=320 comparisons.

After fix59related tests passed (`menu-after.trx`),120effect hashes equal historicalr95 again (`snack-menu-memory-20261020-r97/effects`). This reruns some earlier tests; counts are not additive.
Prepared isolated r97 Release,AppSHA256`C90E62313A08BDB16A0A17DF32EB3D7278D2B4CE947ABD74FE1C63CFBBBA2E00`. Publish produced a different assembly hash than initial test build, so published AppDLL was placed in the generated test output and is being re-tested directly before launch. CoreSHA256 and original PNGSHA256 remain unchanged. r97 is **not running** yet; real menu repeated-open/use memory and appearance are unverified. r96 acceptance must not be transferred to this unlaunched candidate.

Final candidate check:the exact published DLL passed the same59tests (`published-r97.trx`),and all120effect hashes matched r95 again (`published-effects-r97.trx`,`published-effects/pixels.json`). Test-output AppDLL and release AppDLL hashes both match `C90E6231…2E00`. This supersedes the re-test-in-progress status; no additive test-count claim. Current r96 PID32936 still responding,not stopped. Next:ask user to close r96,announce r97 launch only after verifying replacement,then measure actual menu open/close/snack repetitions. No automatic input or unannounced pausing diagnostics.

## r97 actual launch — 2026-10-20 16:01:38 KST

User replied "ㅇㅇ" to closing r96/readiness request. No Dororong processes remained before launch; did not terminate any process automatically. Verified published hash, then launched normal transparent1.5x r97 PID30432,startUTC2026-10-20T07:01:38.1943424Z/startTicks639280764981943424. External600second observerPID33040,internal300second input/performance capture,no pixel recorder. Evidence:`artifacts/snack-menu-memory-20261020-r97/run.json`,`samples.jsonl`,reproducible Start-Measurement.ps1/Observe.ps1. App/helper responding and samples writing confirmed. Immediate first file read raced helper initialization (file not yet created); next read succeeded with2/4second samples,not a recorder failure.

User was told measurement started and requested approximately5menu open/close repetitions then5snack chase/eat repetitions,with completion and any appearance changes reported separately. Actual input/coverage,completion counters,and memory findings are pending. Menu-open count is not instrumented by the existing generic input recorder; don't treat snack-toggle count as proof of five menu openings. r96 prior acceptance doesn't establish this run's visual result. Observe about16:11:41KST; normal observation contains no stop/heap/forcedGC/trim. A new warning is required before any subsequent pausing diagnostic. App remains running after recording ends.

At about16:03:45KST the user replied "ㄲ",interpreted and acknowledged as completion of the requested repetitions. Exact repetitions and unchanged appearance were not explicitly confirmed; no memory/visual pass inferred. At16:04:59,same process identity/responding,98samples through16:04:57,private246.62MiB at that sample; runtime export and observation completion not yet due. Scheduled one bounded result follow-up via heartbeat`r97`,approximately16:14:46KST,after planned observation completion. Prompt requires gap/PID checks,real input analysis,announced-only heap if needed,ledger update and disabling itself; no auto input/restart/exit/power action. Initial COUNT1 schedule was rejected with no future runs; COUNT2 creation succeeded (one future follow-up),no duplicate active r97 monitor. Prior r95 shutdown automation remains paused.

## r97 completed observation and announced post-run dump — 2026-10-20

Heartbeat delivered16:14:28KST. `completion.json` reports normal duration completion at16:11:41.943KST,601.180056seconds/296samples/error null. Same PID30432/startTicks verified again; sample span599.084502seconds,max gap2.087020seconds,no sleep-size gaps. `summary-final.json` is reproducible with `Summarize.ps1`.

Minute0–9 private medians(bytes):217577472,251250688,253980672,258732032,275623936,281522176,281583616,281174016,281141248,281456640. Last sample281489408bytes=268.45MiB; final5minute medians range442368bytes=0.42MiB. GDI medians96 throughout final5minutes,USER49–51,handles698–705. Startup first sample133.56MiB grows to268.45MiB; **not** a claim of no memory growth from launch. Post-input/capture plateau is not a quantified saving against r96(different actions/caches) or a long-term/all-path no-leak result.

Internal300030.631ms record has12inputs:snack-toggle5,Body4,ClickOnly2,RightCheek1,first27693.036ms/last240730.378ms. No frame recorder. Five full meals/menu-open counts are not instrumented and remain user-reported completion. One tick interval exceeds50ms,max108.2661ms; zero-lag pass not granted. No agent live visual verification or explicit new r97 appearance acceptance.

After normal observation, warned immediately before `dotnet-dump collect --type Heap`; saved `after-repeat.dmp`,collection complete and app responded normally afterward. No forcedGC/trim,auto input,restart/exit,power/settings/install changes. Subsequent commands operate offline only.

Offline ownership:

- Live managed122901526bytes; float94arrays/95901856bytes; byte865arrays/23779155bytes; GCcommitted144388096bytes. These are not interchangeable with totalprocessprivate; retainednativeallocation stacks for original gross increase remain unknown.
- One shared1254×1254 CachedBitmap`02bc08c52b70`,source=null,safeMIL`02bc08c53330`. `SnackImage.source` Lazy`02bc08d66600` value is this sameobject.
- Actual SnackCursorWindow`02bc08debb40`→Image`02bc08debdd0`→RenderData and effectivevalues reference the sharedobject.
- Actual menu`02bc08d0fcf8`→MenuItem/templateNameScope list`02bc08f6dd28`,index9 Image`02bc08f6f360`→effectivevalues`02bc08fa13a8` has value`02bc08c52b70`. Thus menu and cursor really share it after use,not merely a code/test expectation. `pathto` alone took a dispatcher detour,so this direct template/Image/value chain is the decisive evidence.
- Remaining eight live BitmapImages are96px wide withdecoder=null; eleven live CachedBitmaps comprise one1254px,eight96px,two100px. No separately rooted snack BitmapImage found. This does not count GPU textures or freed native pixel remnants as live copies.
- One renderer,oneFlutterLayers,oneSnackCursorWindow,onePetContextMenu. Grab cache4entries,spread0inthissnapshot. Disposed performance recorder has fiveemptylists pointing to onezero-lengtharray; no capturedsamplebufferretention there.
- Source evidence:`heap-summary.txt`,`source-objects.txt`,`shared-root-details.txt`,`snack-reference-paths.txt`,`menu-images.txt`,`menu-source-value.txt`,`menu-and-recorder-owners.txt`,`remaining-bitmap-images.txt`.

Conclusion:the r96/r97 decoder-chain removal and menu/cursor image sharing are confirmed in actual process memory. No continued rise was reproduced in the last5minutes of this bounded run. No further speculative product patch made. Exact gross native saving,initial native allocation owners,hours-long r97 retention and direct visual/other-scale acceptance remain open. Current r97 stays running/responding. End this one-shot follow-up and pause heartbeat`r97`;do not revive the old shutdown automation.

Automation update returned `r97`,status`PAUSED`;one-shot follow-up closed. Existing prompt/schedule/thread preserved. No PC shutdown or pet exit requested/executed.

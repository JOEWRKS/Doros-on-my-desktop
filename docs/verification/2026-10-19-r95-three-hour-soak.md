# r95 three-hour actual-process memory observation — completed, no sustained growth reproduced

## Completion shutdown authorization — 2026-10-20

User requests automatic power-off after inspection and necessary reinforcement are complete, and explicitly permits force-closing apps: "파일 저장해 둘 테니 강제 종료해". Same-thread heartbeat r95-3 was updated, not duplicated, to save evidence/results, announce shutdown, disable itself and invoke `shutdown.exe /s /f /t 60 /d p:0:0` ONLY after investigation and necessary fixes/revalidation are actually complete. There is no OS shutdown timer armed now. Do not shut down merely because3hours elapsed or label unresolved work complete to satisfy the condition. If follow-up count expires while necessary work remains, extend the same heartbeat as needed; if blocked, report unfinished status rather than automatically treating it as done. The user can cancel a future60second shutdown with `shutdown /a`.

Read-only power inspection: HiberbootEnabled=1; network adapter/keyboard/mouse/USB controller have wake capability enabled; lastwake reports no identifiable source. Recent Power-Troubleshooter events describe low-power resume with unknown wake source, not proof of the specific previous failed shutdown. Wake-timer query requires administrator rights and was unavailable. No permanent power/registry/device changes were made. Full shutdown without /hybrid avoids hybrid fast startup; never use /r,/g,/sg,/h. Firmware/Wake-on-LAN re-power remains unverified. Command success cannot certify physical power-off or absence of later wake. User was warned about both unsaved-work loss and this verification limit.

User requested hours of actual runtime including repeated interactions, no accepted motion/shape/speed changes, fixes only for demonstrated growth, and advance notice for pausing diagnostics.

## Frozen target and observation

- Same native transparent r95, PID40712, started2026-10-19T13:52:39.2895781Z; start ticks639280147592895781 prevent PID reuse being mistaken for continuity.
- Executable: `artifacts/render-cost-20261019-r95/release/Dororong.App.exe`; App SHA256 F45F0633233D9187DC6CDA7CFB260D26C7156D7CC73911ED616958B373977EF0.
- No product code, settings, assets, timing or cache-capacity changes. Previous30minute evidence: [retention investigation](2026-10-19-r95-long-run-retention.md).
- External hidden PowerShell observer: `artifacts/memory-soak-20261019-r95/Observe-Soak.ps1`,10800seconds,10second samples, bounded lifetime. It only reads process/native resource counters, with no screenshots, input injection, process suspend, forced GC, working-set trim or diagnostic attachment.
- `run.json` stores actual start/planned end; `samples.jsonl` flushes each record; `completion.json` distinguishes duration completion, target exit, PID reuse and observer error. Process uptime does not substitute for recorded span. Sleep/long sample gaps must be reported, not credited as continuous active runtime.

## Interaction evidence and decision rule

This execution has no app input/state logging. Agent has not performed native GUI interactions. Ask the user to repeat grab/shake/release/high landing, cheek pull/release, and treat chase/eat, and report completion time. Record these as user-reported workload, not instrumented counts or agent visual verification. Without confirmation, repeated-interaction coverage remains unverified even if memory is stable.

Compare initial/final10minute medians and intermediate30minute windows, resource counts and sample gaps. A persistent rise across successive windows or repeated post-interaction baselines is a reason to investigate, not itself proof of a leak. A bounded warm-up step is not automatically a defect. If needed, announce pausing heap/trace collection immediately before execution, keep it outside normal observation, and inspect owners before changing code. Preserve accepted motion/shape/speed; no speculative cache trimming.

Follow up in this chat approximately every30minutes using a heartbeat; remain quiet for unchanged/non-actionable state. Notify on growth needing investigation, interruption/failure, completion or required user action. Stop the follow-up after final analysis; no indefinite monitoring. Results pending.

## Started and follow-up registered

Observer PID150116 started successfully; actual observation starts2026-10-19 23:56:42 KST, planned end2026-10-20 02:56:42 KST. Incremental samples were inspected and both helper/target were running. Same-thread heartbeat `r95-3` is ACTIVE,30minute interval with at most7 scheduled checks; disable after completed analysis or interrupted observation. Initial routing/argument errors did not create tasks; only the successful destination=thread call created this automation.

User was asked to keep PC awake and pet running, and to report completion after roughly five repetitions each of grab/shake/release/high landing, cheek pull/release, and treat chase/eat. Confirmation is pending. No claims of repeated-interaction completion yet. A later reported batch can be correlated separately; do not infer action counts from memory changes.

## First user-reported interaction batch — 2026-10-20 00:01 KST

User replied "끝" to the requested repeated-interaction sequence; recorded upon checking at00:01:39KST. This supersedes the pending confirmation for this batch only. The exact input timestamps/counts are not instrumented;00:01 is an approximate report boundary, not an exact last-input timestamp. No omitted action was reported. Agent did not visually verify the sequence.

Same target start ticks639280147592895781 and both observer/target processes remain running. At23:56:42 private204.05MiB/GDI48/USER24; a step appears around23:59:45 private268.90MiB/USER49. Observed peak through00:01:47 is271.39MiB at00:00:36;latest261.39MiB/GDI92/USER49. The increase is material and NOT dismissed as a proven cache warm-up. Neither attribution to a specific action nor a leak is established. Observe post-batch persistence/resource recovery, then investigate owners if sustained. No pausing dump,forced GC,restart or production edit on this reply; normal3hour capture remains intact. Next heartbeat must compare against this elevated post-interaction baseline as well as the original204MiB baseline.

## First heartbeat — 2026-10-20 00:27 KST

184 samples through00:27:37,maximum adjacent gap11.013seconds; both helper and original target still responding with matching start time. No completion.json yet. Latest private274,718,720B (261.99MiB),GDI92,USER48,handles601. Post-batch5minute medians:261.51,261.52,261.52,261.99,261.99MiB. This is approximately57.94MiB above the first sample but only about0.5MiB above the early post-batch baseline. GDI stays92 in every full post-batch window;USER48–52,not an ongoing rising resource count. First10minute median274,210,816B includes the interaction step, so it is not a clean pre-interaction baseline;last10minute median274,718,720B. Keep the original204.05MiB baseline explicit.

Read-only source audit:one SnackCursorWindow is constructed per loop,Hide/Show reuse that instance,Close registered for loop disposal. StyledTrayMenuBridge clears current on menu close and removes Opened handler. HighResolutionHuntBodies is capped at4 reused arrays;HighResolutionSnackEffects.Buffer retains one drawing/bitmap/byte/float set. These are candidates for first-use retention,NOT proof that they account for the observed58MiB. No demonstrated unbounded owner and no speculative change. Preserve the uninterrupted observation;compare end heap with pre-batch after-longrun.dmp after advance warning at completion, unless a new urgent growth pattern warrants earlier diagnosis. No pausing diagnostic or shutdown initiated at this checkpoint.

## Completed three-hour observation — 2026-10-20

`completion.json`:duration_completed,error=null,1072samples,10803.981seconds,ended02:56:45.994KST. First sample23:56:42.280,last02:56:35.976;sample span10793.695seconds,maximum adjacent gap11.013seconds. No long sleep/interruption gap was observed. Same PID40712/start time remained responsive after the recorder exited. User-reported repeated actions at00:01 are the workload evidence; no additional action counts or visual acceptance are claimed.

| Interval (minutes since start) | Samples | Median private bytes |
| --- | ---: | ---: |
| 0–30, includes interaction step | 178 | 274,259,968 |
| 30–60 | 178 | 274,911,232 |
| 60–90 | 179 | 276,611,072 |
| 90–120 | 179 | 276,615,168 |
| 120–150 | 179 | 276,611,072 |
| 150–180 | 179 | 276,615,168 |

First private213,962,752B (204.05MiB),last276,615,168B (263.80MiB),increase62,652,416B (59.75MiB),not hidden. Peak284,577,792B (271.39MiB) during reported interaction period. First10minute median274,210,816B includes this step;last10minute median277,266,432B,affected by small oscillation. The last four30minute medians differ by at most4096B,showing a plateau for approximately two hours,NOT ongoing accumulation at the initial step's rate. Later GDI80–82 and USER48–51;last80/51,handles594/threads17. Resource counts do not grow with uptime during the plateau. This is a bounded runtime result,not lifetime/no-leak certification.

## Post-observation heap and native-memory comparison

After an explicit pause warning, `after-three-hours.dmp` was captured successfully outside the recorded interval;app responsive afterward. Offline analysis never forced GC or trimmed the working set. Compare with `artifacts/memory-longrun-20261019-r95/after-longrun.dmp` taken before this interaction batch:

| Measure | Before batch | After3hours |
| --- | ---: | ---: |
| SOS live managed bytes | 111,035,842 | 113,194,207 |
| float arrays | 96 /85,678,704B | 97 /86,488,728B |
| byte arrays | 139 /22,781,938B | 212 /23,730,684B |
| GC committed | 121,847,808B | 127,045,632B |

Live managed delta2,158,365B (2.06MiB). The only additional float array is810,024B,pointed to by the one HighResolutionSnackEffects.Buffer (`pixels=01589d6ac2d8`);its byte buffer and RenderTargetBitmap are also single instances. One raster,one locomotion,one snack cursor,one PetContextMenu,one RenderTargetBitmap;no RuntimePerformanceCapture/LandingFrameCapture instance. Locomotion64 active slots despite dictionary version19980;spread4 and grab-body4 active slots. No dead float arrays reported. No evidence of a growing collection of abandoned renderer/frame instances. Product DLL SHA unchanged from frozen candidate.

Saved details: `end-heap-summary.txt`, `end-owners.txt`, `end-cache-counts.txt`, `end-runtime.txt`, `baseline-runtime.txt`, `module-delta.json`. Twenty additional native/managed module mappings include IME/text input,accessibility/UIAutomation and CoreUI components. Their mapped image sizes are NOT private-memory charges and cannot simply be added to explain the process delta.

Offline `Read-DumpMemoryInfo.ps1` reads Windows minidump MemoryInfoListStream;it never attaches to the app. `memory-region-summary.json` shows committed MEM_PRIVATE regions184,213,504→247,033,856B (+62,820,352B);mapped/image regions also differ. The largest private allocation-base deltas are27,394,048B at199327B0000,9,175,040B at199367B0000,and three6,291,456B regions. These addresses locate the retention but do not identify allocation call stacks. Together with the small managed-live delta and flat final two hours,this separates substantial non-managed/runtime retention from an accumulating image-array leak. Exact native allocator/module attribution remains unproven;do NOT label the whole59.75MiB as a known harmless cache or say it was fixed. Existing toolset lacks native heap allocation-history evidence for the initial step;no retrospective trace can manufacture it.

## Final disposition

The requested bounded3hour test and post-run comparison are complete. Continued memory growth after the reported repeated actions was not reproduced in the latter approximately two hours. No confirmed new defect warrants a product patch;no product code,appearance,physics,speed,settings or installation changed and no untested candidate replaced r95. No new product tests are represented as run during this soak;earlier25retention cases are earlier evidence only. Added external observation/offline analysis helpers and evidence documentation.

Limits remain explicit:the full initial native-memory step is not attributed to an allocation stack;all-day runs,new interaction batches throughout the entire duration,other machines and every motion are not certified. This is not a claim that every memory concern is resolved. There is no demonstrated ongoing leak or pending patch/revalidation in this measured run;do not introduce speculative changes to force a lower private-byte reading. Any future renewed sustained growth needs native allocation-history capture from before reproduction. Deferred small release hitch remains deferred.

Per the user's completed-test power-off instruction,the same heartbeat will be disabled and full non-hybrid shutdown requested only after this report and ledgers are saved. Shutdown command acceptance and physical off/wake prevention remain separate facts;the latter cannot be observed from the powered-off agent. No permanent wake settings have been changed.

Execution result:heartbeat r95-3 updated to PAUSED successfully. After a separate immediate warning,`shutdown.exe /s /f /t 60 /d p:0:0` returned exit0 at2026-10-20 03:02:14KST. `shutdown-request.json` records this accepted shutdown request. Expected countdown end is approximately03:03:14KST unless canceled/blocked. Actual power-off and later re-wake are NOT verified. This supersedes earlier "not scheduled/running" descriptions only for final shutdown state.

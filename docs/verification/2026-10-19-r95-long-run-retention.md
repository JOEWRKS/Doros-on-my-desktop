# r95 long-run retained-memory investigation

## User scope and frozen candidate

User requested the remaining memory-growth cause,fixes supported by evidence,and actual long-duration validation without altering accepted motion,shape or speed. Warn before any pausing diagnostics;unverified is not pass. The small release hitch remains explicitly deferred,not resolved.

At2026-10-19 23:04:39 KST the SAME r95 PID40712,started22:52:39,was still responding,private215,126,016B,handles610. No restart/settings/install change. App SHA256 `F45F0633233D9187DC6CDA7CFB260D26C7156D7CC73911ED616958B373977EF0`,Core and banks unchanged. [Earlier evidence](2026-10-19-r95-metadata-snapshot-capacity.md) has the first5minute observation and the first end heap. Do not equate elapsed uptime with continuously recorded data.

## Long-run protocol

An additional1800second external observer uses10second process counters,`artifacts/memory-longrun-20261019-r95/process.json`. The pet has no internal performance log or frame recorder. No forced GC,working-set trim,trace attachment,heap dump or shutdown during this interval. Same app stays active;user does not need to perform repeated test operations. Input/state is not instrumented,so this is not proof of all interaction paths or specific idle-only workload. Any final heap collection will be separately warned and outside the timed observation. This local single-machine interval is not an all-day,other-PC,sleep/resume or multi-monitor acceptance.

Separate test process may run bounded-cache regression tests during the observation;it does not attach to,force collection in,or alter the pet. Its CPU load means this interval is not a clean rendering-latency benchmark. Test iteration count never counts as actual elapsed runtime.

At23:16:03KST a second read-only sampler starts recording private/working-set bytes,handles,thread count and Win32 GDI/USER object counts every10seconds for1150seconds,`native-resources.jsonl`. This queries counters only;no screenshot,input automation,window manipulation or process suspension. First sample:17threads,48GDI,26USER objects. Native object counts cover only their recorded subinterval and do not represent every D3D/COM allocation.

## Prior heap/source correlation

The first r95 end heap has107,880,950B live managed objects;GC committed121,716,736B,allocated heap116,286,752B. Private process memory includes more than either value. Actual float arrays:79*810,024B,8*2,250,024B,1*409,624B,total82,401,712B. Locomotion holds64 bodies at maximum64 entries;32 bodies payload25,920,000B vs64 payload51,840,000B. This explains a bounded25.92MB source of warm-up growth,NOT an exact attribution of the whole earlier38MiB private-memory step without pre-step heap data.

Existing borrowed-frame ownership/reuse,weak-cache lifetime and repeated sleep/walk tests are relevant evidence. A new mixed walk/sit/run/sleep test asserts that1050 additional frames use the same64 borrowed body arrays. It does not modify product code or cache limits. Completed results follow below; interim observations are superseded by the full interval.

## Regression results

`tests/retention.trx`:25passed,0failed. `tests/mixed-final.trx`:the new mixed-motion case passes again after using reference-HashSet membership rather than structural array comparison in the assertion.1050 warm mixed frames reuse exactly64 body arrays;temporary allocations599,280B in first run/599,360B in final rerun. Existing40-miss regression:owned path18,880B vs immutable path32,419,840B;existing warm sleep/walk cycle66,584B. Weak-table and completed-worker discard tests pass. These execute in a separate test process and cannot substitute for the native wall-clock observation. Product App DLL rebuilt during tests still exactly matches published/running r95 SHA;no production-code changes this turn.

## Workload attribution limit

The previous r93 no-input120second recording entered Sleep at90.17seconds without a matching large private-memory step at that point. Therefore sleep entry alone is not a demonstrated universal cause of the r95 first-run step. Asked about r95's first5minutes;user answered that they do not remember whether they interacted. Leave that workload unknown. Cache payload growth is demonstrated in controlled tests and bounded retention in the prior heap;neither supplies missing input/state history or exact native allocation attribution for the old step.

## Completed actual observation

The 1800-second observer completed normally. It saved 180 samples from 23:05:36.048 to 23:35:31.718 KST (1795.67 seconds between first and last samples; the final sampling sleep is outside that span). Same PID40712, started22:52:39, remained responsive: about43minutes total uptime at the end, NOT43minutes of uninterrupted measurement.

| Private process memory | Bytes |
| --- | ---: |
| First | 213,798,912 |
| Last | 213,794,816 |
| Minimum | 213,618,688 |
| Maximum | 215,089,152 |
| First5minute median | 213,798,912 |
| Last5minute median | 213,704,704 |

Thus the recorded range is about203.72–205.13MiB and first-to-last is minus4096B, not sustained private-memory growth. Handles514–632, first611/last534. The supplemental sampler completed115samples from23:16:03.914 to23:35:07.600;GDI48 throughout,USER24–26,threads16–19. These counters do not cover all native allocations and are not an all-day leak guarantee. Both observers ended before heap collection. No forced GC or working-set trimming was performed in the pet.

## End heap and retained-array attribution

After explicit advance warning, a Heap dump was collected into `artifacts/memory-longrun-20261019-r95/after-longrun.dmp`; the app resumed responding and remained running. Analysis files: `heap-summary.txt`, `array-owners.txt`, `spread-cache.txt`, `spread-ink.txt`. This paused diagnostic is outside the normal observation and its working-set effects are excluded.

| Managed heap | First5minute dump | After additional30minutes |
| --- | ---: | ---: |
| Live object bytes reported by SOS | 107,880,950 | 111,035,842 |
| Actual float arrays | 88 /82,401,712B | 96 /85,678,704B |
| Byte arrays | 141 /22,913,074B | 139 /22,781,938B |
| GC committed bytes | 121,716,736 | 121,847,808 |

The exact float-array increase is8*409,624B=3,276,992B, about3.13MiB. The79 arrays of810,024B and8 arrays of2,250,024B are unchanged;160-square arrays increase1→9. In the new dump, the raster spread dictionary has `_count=4`, `_freeCount=0`:source arrays01589b38d540/01589c7d7620/01589c89f690/01589c967700. Four `HighResolutionBodyInk.Entry` objects point at derived arrays01589c7735e8/01589c83b658/01589c9036c8/01589c9cb738. This is the existing four-slot spread cache plus one reusable outline-compensation array per slot, not unbounded addition on every landing. Source `HighResolutionGrabRaster` reuses the expired slot at count4;`HighResolutionBodyInk` reuses its associated derived buffer. `gcroot` alone did not show the derived weak-table paths;the Entry fields and live-object listing were inspected instead, so zero direct roots was NOT interpreted as proof these arrays are dead.

Locomotion dictionary remains64 active entries (`_count64`, `_freeCount0`, version1058);only one locomotion and one raster instance. No live RuntimePerformanceCapture or LandingFrameCapture instance. Dead-float query reports no actual float arrays. The modest live-byte increase is chiefly the bounded spread warm-up, while GC committed grows only131,072B and process private memory is stable. Do not call committed/free space a leak or pretend the earlier unexplained private-memory step is fully attributed.

## Disposition and limits

No new continuously growing owner was reproduced in this interval. Therefore no additional production patch, cache shrink, timer/physics change, image change, settings change or r96 build was made. The existing r95 allocation fixes stay active. Added the mixed-motion retention regression and diagnostic evidence only. The new test is one of25 passing cases; its rerun is not a26th distinct case.

This completes the specified30minute measured investigation, not all-hours/all-workloads leak acceptance. Remaining: exact allocation attribution of the original first5minute step (workload unknown), multi-hour/repeated-interaction growth, and previously deferred release hitch. Further product changes require a reproduced growing owner; do not remove bounded warm caches merely to reduce a displayed number. Current r95 stays running.

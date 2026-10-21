# Mac test04 Windows-host two-hour memory soak — 2026-10-21

## Authorization and scope

User explicitly requested long repeated resizing/action-transition memory testing
that can run on Windows, investigate/fix confirmed accumulation, preserve accepted
motion/shape/quality/speed and the Windows version, and separate native Mac gates.
This authorizes a bounded background headless diagnostic, not desktop input into
the existing pet. No application source, artwork, package, settings, installation,
power configuration, or native desktop state changed in this task.

Status: **COMPLETED — no sustained accumulation observed in this host workload**.
Not a universal/native Mac leak-free verdict. No forced GC, working-set trim, heap dump
or trace. Any potentially pausing diagnostic must be announced immediately before
it runs. No prior Windows soak/shutdown automation is resumed.

## Driver and coverage qualification

`tools/MacCpuProbe/HostSoak.cs` adds a separate driver mode; Program.cs dispatches
it and README documents the limits. `AnalyzeSoak.ps1` is read-only and does not
automatically pass or diagnose a leak from private bytes.

One Mac R104DesktopController is retained for the entire test. No controller/frame
history list. JSONL samples and sparse transition events stream to disk.
The 7,500-tick cycle (120 simulated seconds,16ms per step) exercises:

- Size waves100–150%, requesting every fourth tick; density alternates1/2 each cycle.
- Sit/stand, head grab/shake/release, cheek pull/release.
- Snack chase→Eating→Happy→Settling→None, then autonomous idle/walk time.
- Natural collection only. Private/working set/managed estimate, last-GC heap,
  fragmentation/commit/LOH, allocations, natural GC counts, handles/threads,
  display/scratch capacities, owned-frame allocations and bounded cache counts.

Inputs are calls into the product controller, **not native UI/mouse acceptance**.
Head/cheek hit acceptance and capture changes, snack phases and applied/rejected
size requests are logged. Rejected requests at startup are counted, not hidden.
Native menu, NSWindow, bitmap upload, GPU or perch/paw paths are outside this driver.
Controller faults, failed scripted hits, missing basic coverage or private memory
over1.5GiB fail and stop only the diagnostic. A catch-up burst is never used.

Qualification run: `artifacts/macos/r104-host-soak-20261021/smoke01/` completed
22,500 steps /360 simulated seconds in84.905 real seconds. Three head/cheek/eating/
sit cycles each,939 size requests/928 applied/11 rejected,6 releases,0 faults.
This is only driver coverage qualification, **not a long-run pass**. The later
build adds LOH telemetry and the1.5GiB guard; no product DLL changed.

## Immutable actual run

Artifacts: `artifacts/macos/r104-host-soak-20261021/`:
`actual/run.json`, `samples.jsonl`, `events.jsonl`, eventual `completion.json`;
stdout/stderr in the parent. Full executable dependencies copied to `frozen-host/`.

- Diagnostic PID88972, startUTC2026-10-21T13:23:46.3339946Z,
  startTicks639281858263339946. BelowNormal priority to limit desktop interference.
-120 actual wall-clock minutes; first120 simulated seconds are warmup. Final600
  actual seconds stop scripted interactions and continue autonomous rendering.
  Rendering/pacing can make simulated time less than wall time; both are recorded.
  Planned end:2026-10-22 approximately00:23:47KST, not a pass deadline.
- Ten-second samples; compare UTC/monotonic intervals, loop gaps, PID/start identity,
  and completion reason. Sleep or gaps cannot count as continuous execution.
- Raster SHA256 `86C7CBB20A1567CFBFDBB3B68E02AF2D1841FBDA818463C45B83A8F14B31D57F`
  exactly matches the test04 candidate Raster. Host Mac assembly hash
  `69678B9ABD064B44923CB0E325617306370F89C68634869142D86CA22ED4B1DE`;
  Windows-host target, not the osx-arm64 app binary execution.
- Bounded follow-up heartbeat `mac-test04-2`, every20minutes/up to7 checks, pauses
  on completion/failure after saving/reporting results. Quiet if unchanged. No
  infinite monitor, auto restart, public upload or PC shutdown.

## Evaluation method

Compare first/last ten-minute windows,20-minute windows and same-phase cycle tails;
separate first-use caches and alternating density from cumulative growth. Use
natural GC history to distinguish committed/free space from retained objects;
GetGCMemoryInfo reflects the last GC, not current live memory. Streamed logs also
allocate small objects. Check display/scratch high-water storage and4/4/24 cache
bounds separately from overall process private memory. Final recovery is a
scripted-input-free interval, not a guarantee that natural GC runs or frees pages.

Persistent same-workload growth warrants owner investigation and confirmed-cause
Mac-only correction with relevant pixel/regression/retest evidence. Do not alter
frozen running binaries. Final results below determine whether this was necessary.

Native macOS window/compositor/upload/GPU memory, real M2 CPU, Retina screen and
native user input remain **NOT_TESTED**, regardless of this host result.
Windows App/Core diff against3813312 remains empty. Accepted test04 ZIP is unchanged;
no rebuilt test app is necessary unless an actual product defect is fixed.

## Final bounded result — 2026-10-22 00:27KST

`actual/completion.json` reports normal completion at UTC2026-10-21T15:23:47.6350465Z,
7,200.0118 real seconds. The same process/start identity was verified at successive
checks through100minutes, and it exited normally after completion. No restart.
718 valid samples, maximum interval11.22393 seconds (nominal10), maximum difference
between UTC and monotonic interval0.0000277 seconds, maximum loop gap1.37502 seconds.
No long/sleep-sized sample gap or >2second loop-gap event occurred. This supports
the recorded continuous two-hour host observation, not OS power-event certification.

233,760 ticks represent3,740.16 simulated seconds, not two hours of simulated motion:
Windows scheduling and no-catch-up pacing make these times different. BelowNormal
priority and other desktop work can affect latency. Maximum measured Advance was
1,375.01ms, so this memory result must NOT be called a smooth-frame/render-latency
pass or native M2 performance improvement.

### Workload actually recorded

-9,077 resize requests:9,007 applied,70 rejected by normal controller eligibility.
-29 head grabs and29 cheek presses, each accepted;58 releases. Capture events116
  (58true/58false), no controller faults or failed-hit aborts.
-29 sit cycles;29 snack starts,28 complete Eating→Happy→Settling→None paths.
  The last Chasing session began at15:13:46UTC and was deliberately canceled when
  the final recovery phase began at15:13:47.624UTC. Do not claim29 completed meals.
-28 complete mixed cycles plus a partial final cycle; later cycle markers are
  bookkeeping during recovery, not additional actions.
- Recovery lasts600.011 seconds; its60 samples span6606.423–7198.795seconds.
  No scripted inputs after entry. No native menu, perch/paw, OS input, visual or
  compositor/GPU checks were included.

### Memory comparison

All sizes below are MiB; each comparison window has60 samples. Initial window is
after the first full simulated cycle (242.158–834.029real seconds), not startup.
Last repeated window6004.143–6596.392seconds, recovery6606.423–7198.795seconds.

| Metric, median | Initial stable10min | Last repeated10min | Final recovery10min |
|---|---:|---:|---:|
| Process private |126.7969|126.7832|126.8086|
| Managed estimate, no forced GC |108.3271|104.0156|104.1524|
| Heap at last natural GC |104.5231|104.4938|104.4780|
| Committed managed heap at last GC |113.1641|113.2227|113.2227|
| Handles |272|270.5|270|
| Threads |9|9|9|

Six consecutive20-minute private medians:126.7832,126.7930,126.8516,126.9297,
126.8750,126.8047MiB. The last window returned below middle-window levels rather
than continuing to rise. Late repeated peak130.9375MiB fell back; peak alone is
not a leak. Fragmentation median changed0.352→4.384MiB while committed heap only
changed0.0586MiB and total heap stayed level; this does not establish a retained
object increase. Last-GC values are not an exact current live-object census.

All718 samples retained display2,359,296B and scratch2,359,296B, owned allocation
counter2 throughout. Body cache reaches4, sole cache24 and both stay bounded;
spread cache and perch buffers remain0 (those paths NOT_EXERCISED, not passed).
Natural gen0/1/2 counts end126/16/7. About1.080GiB cumulative allocations were
recorded by the last sample; cumulative allocation is traffic, not retained usage.
No forced collections, trimming or pausing inspection was used to produce stability.

Machine-readable full statistics: `actual/summary-final.json`; raw samples/events
and original completion are preserved. Frozen Raster still matches test04 hash;
accepted test04 ZIP remains `007775F4237DB6B75C9D914E1637B03AF14EB7AAD3F76C215CFDEB103DFBB51A`.
Windows App/Core remain unchanged against3813312. Only driver/analysis tooling and
verification/TASKS/ROADMAP records changed for this request.

### Decision and remaining gate

**No sustained buffer/cache/process-memory accumulation reproduced in this bounded
two-hour host test.** Therefore no speculative product fix, heap dump, repeated
soak, new package, commit or upload is warranted from this evidence. The requested
host observation is complete; this does not prove all possible memory paths safe.
Native macOS window/bitmap upload/compositor/GPU retention, actual M2 CPU/Retina
and native input remain NOT_TESTED. The next required acceptance is on the Mac,
not another identical Windows-host run. Follow-up `mac-test04-2` is removed after
this report; no shutdown action and no old automation reactivation.

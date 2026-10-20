# r98 — memory ownership and repeated allocation audit

## Scope and verdict

User requested a comprehensive cause review instead of another unclassified
measurement/repeat-input cycle. Preserve accepted artwork, quality, motion,
shape, speed, settings and installed build. The small release hitch remains
explicitly out of scope.

**Two product allocation defects are identified and corrected:** neck frames
recreated a native WIC render target despite reusing managed arrays; the
120ms perch-release clip recreated two large arrays and a bitmap every frame.
These are allocation/collection churn, not proof of permanently rooted leaks.
Large graphics pools and heap free space are classified separately below.
Do not call the entire memory issue or multi-hour candidate verification passed.

Running real pet remains published r97, PID38088, start UTC
2026-10-20T07:50:18.8097533Z. No restart, synthetic desktop input, forced GC,
working-set trim, driver change, permanent power setting change or shutdown
was performed. A live heap snapshot was announced immediately beforehand;
subsequent dump analysis was offline. Diagnostic WPF replay windows were
announced, used separate processes and closed themselves.

## Evidence and ownership

Evidence roots:

- `artifacts/memory-root-audit-20261020-r97`
- `artifacts/memory-root-audit-20261020-r98`
- Prior address-matched ETW attribution:
  [r97 graphics allocation stacks](2026-10-20-r97-graphics-allocation-stacks.md).

At the live r97 snapshot (~17:08 KST), reachable managed objects total
123,627,491 bytes; GC committed is 131,809,280 bytes. The earlier different
r97 session had 122,901,526 reachable bytes. This is descriptive, not a
controlled savings comparison.

`cache-owners.txt`, `cache-counts.txt`, `managed.txt` show:

| Owner | Observed retention | Interpretation |
| --- | --- | --- |
| HighResolutionGrabRaster | One instance; body cache 4, spread cache 0 | Body limit 4; spread limit 4 in source |
| HighResolutionLocomotion | One instance; body cache 64 | Bounded full motion-cycle cache, not unbounded accumulation |
| HighResolutionHuntBodies | One instance; body cache 4 | Reused sources, limit 4 |
| Neck/snack/FlutterLayers | One owner each | Flutter key224; no repeated retained owner chain found |
| Display | One WriteableBitmap | Existing display-surface reuse preserved |
| RenderTargetBitmap | One total and one reachable at this snapshot | Do not claim hundreds of dead targets in this dump |
| Menu/cursor/snack source | Prior r97 verified shared1254px detached source | Previous snack fix preserved; no additional source decode in this patch |

Additional source review: presenter frames12 and sole entries24 are bounded;
derived ink/alpha caches use weak keys; legacy breathing is quantized to32
levels×2 eyes, legacy walk to8×32×2, indexed motion has a fixed array.
Runtime capture detaches handlers/disposes its sampler on completion.
No speculative cache flushing or reduced-quality substitution was introduced.

`native.txt`: default native heap committed49,832KiB includes29,279KiB
already-free space (58% external fragmentation). Committed/private memory
therefore does not equal live leaked objects. One other heap could not be
decoded by the debugger; that is not a confirmed corruption finding.

The three committed write-combine regions remained:
33,091,584 +33,554,432 +2,555,904 =69,201,920 bytes (65.996MiB).
They match the prior07:59:29/08:00:46UTC readings, the heap snapshot, and
`passive-final-regions.json` at08:23:45UTC with the same PID/startTicks.
This is sampled stability over24minutes, not continuous sampling or proof
against all future growth. Prior ETW established WPF/D3D texture uploads,
render targets and effects as allocation paths; NVIDIA stack presence alone
does not establish a driver defect.

## Corrected lifetime defects

### Neck render target

`HighResolutionHuntNeck.Buffer` had reusable byte/float arrays but
`RenderInto` created a new DrawingVisual and225×225 RenderTargetBitmap on
every changed neck/gaze frame. Native WIC memory pressure was not removed by
the earlier managed-array reuse.

Buffer now owns one visual and one render target, clearing before rendering.
Geometry, colors, sampling, body ink, head/cheek artwork and timing are
unchanged. The allocating reference path is kept only for equivalence tests.

Actual published r97 versus final r98, same deterministic157-frame sequence
repeated10times:

- Added natural Gen2 collections: **74 → 0**.
- Warm managed allocations per157frames:567,712 →457,184 bytes.
- All157 pixel hashes equal; forward/reverse314-frame reference test also
  retains the same target and float-array identities.
- This short isolated replay's final private bytes are not a savings measure:
  r98 deliberately does not force collection of its small temporary geometry.

### Perch release clip

`ApplyHighResolutionPerch` previously copied current/previous frames into two
fresh arrays and created/froze a new BitmapSource for each clip frame.
`HighResolutionPerchBlend` now owns two buffers and one WriteableBitmap,
reallocating only when dimensions/format/DPI change. Both sources are read
before output writes to preserve aliased-input behavior. AlphaHitTestImage
receives the current live pixels, avoiding stale hit-test data.

Source capture,120ms duration, complementary row boundary, render order and
shape are unchanged. Published presenter replay: both directions,5 scales,
20frames per direction,10cycles per scale.200 first-cycle pixel hashes equal.
Warm median allocated bytes per pair of perch/return sequences:

| Scale | r97 | r98 |
| --- | ---: | ---: |
| 1 | 7,471,584 | 132,192 |
| 1.1 | 9,036,168 | 129,432 |
| 1.25 | 11,597,640 | 129,440 |
| 1.35 | 13,536,992 | 129,440 |
| 1.5 | 16,643,992 | 104,232 |

The probe copies pixels into a reused readback array. The earlier
`perch-before` run allocated its readback each frame and is retained for
history, but excluded from this allocation comparison.

## Published-code verification

Final Release:
`artifacts/memory-root-audit-20261020-r98/final-release`.

- App SHA256:
  `54E2E567949C7424376C27C1DF774C58235B684DB0E518A937F082B22E848075`.
- Core SHA256:
  `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`,
  identical to accepted r97 Core.
- Final regression27passed/0failed/0skipped: neck lifetime, perch blend,
  high-resolution state transitions, perch expression, mutable surface/hit
  testing. Test output App DLL hash exactly matches final publish.
- Earlier neck-only candidate20passed; not added to27 as disjoint coverage.
- Build had6 existing xUnit1031 warnings in unrelated capture/wait tests.
- No artwork/bank, Core physics/timing, settings or installer edits this turn.

`tools/MemoryLifecycleProbe` references the exact published assemblies, uses
native WPF rendering and an actual transparent WPF window/menu, and records
PID/start/hash, resources and completion. It is deterministic state replay,
**not an end-to-end mouse-input or visual acceptance test**.

GPU replay includes idle/walk, tilted grab, spread, neck, cheek, run/drool,
eat, flutter, perch paw plus actual menu open/close.32cycles×120frames.
Final candidate completed134.33seconds, baseline133.47seconds. Both completed
all3,840frames/32menu cycles, with120/120 first-cycle pixel hashes identical.
Together with the neck and perch sequences,477 baseline/candidate frame
hashes are identical. Machine-readable comparison: `comparison.json`.

Warm cycles8..32:

| Metric | r97 | r98 |
| --- | ---: | ---: |
| GDI objects min..max | 42..212 | 34..38 |
| USER objects min..max | 18..19 | 17..18 |
| Handles min..max | 641..663 | 642..683 |
| Added Gen2 collections during warm range | 1 | 0 |
| Private bytes min..max | 198,795,264..221,220,864 | 197,902,336..209,309,696 |
| Write-combine first→last | 31,911,936→32,698,368 | 31,649,792→32,436,224 |

Both runs add786,432 bytes of graphics capacity during that warm range.
Neither is exactly flat throughout. r98 cycles25..32 remain32,436,224 bytes.
The removed accumulating native-bitmap churn is demonstrated by the GDI and
isolated neck result; this does not justify claiming all retained graphics
capacity eliminated. Handle maxima overlap; no handle-reduction claim.
Baseline GDI falls again after natural collection: accumulation awaiting
collection, not a proven permanent GDI leak.

## Limits and next action

- Candidate normal pet launch is confirmed by the follow-up below. Human
  mixed-input acceptance and multi-hour observation remain unverified.
- No exact whole-process savings claim: startup state, normal collection and
  GPU capacity differ across processes.
- WPF/driver resource capacity can persist after a menu/effect closes.
  Plateau samples and bounded replay do not prove every future path leak-free.
- No further speculative memory/driver fix is justified by retained capacity
  alone. r98 has now replaced the closed r97 in normal execution; preserve
  the installed copy/settings. Mixed-input instrumentation and multi-hour
  acceptance remain incomplete.

Changed product files: `HighResolutionHuntNeck.cs`,
`DororongPresenter.HighResolutionPerch.cs`, new`HighResolutionPerchBlend.cs`.
Added lifetime tests and published-assembly diagnostic probe/comparison
script; updated this evidence record, TASKS and ROADMAP.

## Normal r98 launch — 2026-10-20 17:35 KST, measurement in progress

User reported shutting down r97; process absence was checked before launch.
The exact verified final App/Core hashes above were rechecked. Started normal
transparent1.5x r98 at UTC08:35:33.5636347, PID46620,
startTicks639280821335636347. Passive observer PID29936. App and observer
responding and first samples saved. Evidence: `actual/run.json`,
`actual/samples.jsonl`; internal runtime capture300seconds, external passive
memory600seconds. No frame dump, forced GC, trim or automatic input.

Start was announced after process/sample confirmation. User was asked to
mix menu/snack, head grab/release/landing, cheek pull/release and perch/return,
report completion and leave the app running. Actual completion and shape
acceptance are pending; start confirmation is not memory verification.
Existing paused heartbeat `r97` was repurposed for this single result review,
with a finite schedule ending UTC08:55; no duplicate monitor or r95 shutdown
automation enabled. Initial COUNT1 update was rejected for no future runs;
the bounded UNTIL update returned ACTIVE.

This supersedes the earlier statement that r97 is still running. Installed
copy/settings/accepted artwork and motion unchanged. On completion inspect
recorded inputs, sample continuity and late resource trends before deciding
whether a separately announced heap investigation is necessary.

## User completion and initial actual result — 17:46–17:49 KST

User reported “끝”. This is completion reporting, not exact counts/timing or
explicit visual acceptance. `actual/summary-final.json` is reproducible with
`tools/MemoryLifecycleProbe/Summarize-Actual.ps1`.

First passive recording completed normally:601.788seconds,291samples,
maximum monotonic gap2.421seconds/wall gap2.421seconds, same PID/startTicks,
app still responding. Private per-minute medians:
225.38,229.93,233.48,238.10,238.99,240.77,240.84,240.82,281.60,286.84MB.
GDI stayed33 through minute7, then80 at the end; USER reached48–50 and
handles694–712 in the last minute. That late increase needs post-use follow-up,
not an automatic leak diagnosis or memory-pass declaration.

The internal recorder ended at300seconds before the late increase. It
contains Body inputs at4.104s and166.240s; no recorded cheek/snack inputs and
all recorded perch states are None. Do not claim the requested mixed actions
were instrumented, nor infer that the user did not perform them later.
Within those300seconds: Gen0/1/2 deltas69/23/5, total allocated662,547,160B;
one tick interval>50ms (max63.019ms), nine rendering callback intervals>50ms
(max55.591ms). These are CPU-side diagnostics, not displayed-frame proof.
Last managed sample125,308,200B/GC heap138,253,344B/fragmented20,774,808B.
Different workload prevents an exact r97 savings claim.

No new user repetition or restart requested. A separate passive300second
post-input observation started UTC08:47:59.525, helper37524, same app:
`after-input/run.json`, `samples.jsonl`, eventual `completion.json`.
Internal recorder is off. This is not multi-hour verification. Passive
VirtualQueryEx in `actual/after-input-regions.json` found60,026,880B
write-combine committed, without pausing/heap capture/forcedGC.
At08:49:06UTC the app and helper respond, private283,762,688B/GDI80.
Await the full bounded interval before classifying its trend.
Existing heartbeat r97 updated to check this result and then pause itself;
finite schedule ends09:00UTC. Product code, installed app and settings
unchanged in this follow-up.

## Final bounded follow-up — 17:54 KST

`after-input/completion.json`: duration_completed, no error,
301.1307285seconds,146samples. Maximum monotonic interval2.1353094seconds;
maximum UTC interval2.1342001seconds. No sleep-sized gap within this recording.
The gap between the first recording and this separate follow-up is not
represented as continuously sampled. PID46620/startTicks639280821335636347
still matches and responds. Observer37524 has exited normally.
Machine-readable results: `after-input/summary-final.json`.

| Metric | First → last / range |
| --- | --- |
| Private | 283,779,072 →283,758,592 bytes |
| Private range | 283,709,440..283,889,664 bytes; span180,224B =0.172MiB |
| Per-minute private medians | 283,742,208;283,762,688;283,865,088;283,787,264;283,758,592 bytes |
| GDI | 80 throughout |
| USER | 48..51; first49 / last48 |
| Handles | 684..708; first699 / last684 |

**No sustained post-input increase was observed in this five-minute window.**
Private stayed near270.6MiB rather than continuing the previous step upward.
A later ordinary process read at08:54:03UTC was272,957,440B (260.31MiB),
lower without forcedGC/trim/restart. This is observed decline, not evidence
identifying which allocator released it.

No new heap/trace, product patch or user repetition was justified by these
results. Do not identify the earlier approximately44MiB step as a permanent
leak, but equally do not certify all retained memory or a specific late
interaction's ownership from this passive sample alone. Original mixed
actions after the300second input cutoff remain uninstrumented; user “끝”
is not explicit visual acceptance. Multi-hour r98 validation remains open.

This single bounded result review is complete. Heartbeat r97 PAUSED was
confirmed by the automation tool. The normal r98 pet remains running;
settings, installed copy, artwork, motion and speed unchanged. No PC shutdown
or old r95 automation reactivation. No indefinite monitoring is left active.

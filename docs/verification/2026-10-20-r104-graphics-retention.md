# r104 — bounded repeated-resize graphics-retention observation

## Final bounded result — 2026-10-20 23:30 KST

Observation completed normally at14:22:58.2664900UTC:601.3456s,117samples,
same PID/startTicks throughout. Maximum monotonic gap5.5996s, wall gap5.5994s
for requested5s sampling: no sleep-sized discontinuity. Each address scan is
non-atomic; observer scan time peaked261.87ms, not measured target UI latency.
Evidence: `actual/summary-final.json`, samples and completion under
`artifacts/graphics-retention-20261020-r104`.

**Additional commitment was observed, but sustained per-repeat accumulation
was not established.** Do not mark whole-memory leak absence or reclamation passed.

| Time from first sample | Write-combine committed bytes |
| --- | ---: |
| first |141,357,056|
| ~21s |141,750,272|
| ~26s |142,929,920|
| ~83s |143,192,064|
| ~422s |147,517,440|
| last (~596s) |147,517,440|

Net+6,160,384B =5.875MiB. The first increase plateaued for about339s, then
another4.125MiB appeared. All changes occurred within existing allocation base
1827193290752 (7,405,568→13,565,952B). Four other allocation bases retained
their exact committed totals. No new allocation base was seen.
The last174.6s of normal record were unchanged; read-only snapshot at14:28:06UTC
also matched147,517,440B. The interval after recording is **not** continuously
sampled and includes the separately announced diagnostic below.

Minute private medians(MiB):331.07,332.14,332.35,332.35,332.35,332.44,
333.48,345.02,344.96,343.45. Final private360,136,704B (~343.45MiB), versus
346,218,496B initially. This includes managed/native categories; cannot equate
the net difference with graphics leakage. GDI settled68 after initial68–72;
USER44–47; finalhandles602 vs693initial. Full ranges in summary-final.json.

### Post-record diagnosis, announced separately

One heap dump collected after immediate user-facing pause warning:
`actual/post-observation.dmp`; completed successfully, same process responding
afterward. No forced collection, trim, restart, quality or product change.
Offline outputs: managed.txt, owners.txt, array-roots.txt, cache-owners.txt,
baseline-cache-owners.txt, bounded-counts.txt.

- One live presenter/WriteableBitmap/RenderTargetBitmap; frame slots12, spare0,
  locomotioncache64. No escalating bitmap-owner chain demonstrated.
- Reachable managed total123,128,284B vs earlier95,408,768B. Float arrays
  95,901,856B vs68,901,568B: twelve additional2,250,024B arrays.
- Current held-body cache4 and one FlutterLayers object are present; earlier
  baseline had flutterLayers=null. Source has4body slots and one current flutter
  owner, not unbounded per-size storage. Current release-angle field88 vs earlier0
  also differs. These snapshots show additional motion state since the earlier
  dump, **not when/how often the user manipulated it** or proof of extra input
  inside this ten-minute interval.
- Flutter initialization includes offscreen WPF paw rasterization, but no current
  allocation stack/timing links it to the4.125MiB step. Do not claim this as the
  established native-growth cause. Nor do managed float buffers explain
  PAGE_WRITECOMBINE bytes.

**Disposition:** finite follow-up ended; no repeat-operation request or speculative
product patch. User's completed-operation report is preserved separately from exact
input counts, which were not recorded. Accepted slider/appearance/quality/speed
unchanged; app remains running. Pause the result heartbeat.

Remaining: precise ownership/live-versus-pool interpretation of the additional
5.875MiB and all-day/other-DPI recurrence are unverified. If renewed repeated growth
is reproduced, current allocation-stack capture is the next targeted diagnostic;
this is not authority to restart indefinitely or remove accepted visual effects.
The following in-progress notes are historical and superseded by this result.

## Scope and status — 2026-10-20 23:13 KST

User authorized checking whether graphics-related memory keeps accumulating after
repeated accepted slider use; fix only an established cause if continued growth
occurs. Preserve accepted motion, appearance, quality, speed, assets and installer.
This supersedes the previous no-further-testing disposition only for this request.
No product code changed. No restart, automated mouse input, GC, trim or suspend.

Normal transparent r104 remains PID32820, startUTC
2026-10-20T13:46:38.3929362Z / ticks639281007983929362.
Executable: `artifacts/size-storage-20261020-r104/release/Dororong.App.exe`.
App SHA256 verified unchanged:
`374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322`.

## Read-only measurement

`tools/MemoryLifecycleProbe/Observe-Graphics.ps1` opens a query-only process
handle, checks PID/start each sample, enumerates committed PAGE_WRITECOMBINE
regions via VirtualQueryEx, and records addresses, allocation bases, sizes,
private/working set/GDI/USER/handles, wall/monotonic time and query duration.
These are process-address-space graphics-like allocations, **not total GPU VRAM**;
current allocation stacks remain uncollected. Prior r97 stack attribution must
not be promoted to proof of every current allocation.

Smoke run: 10.367s /5 samples, no error. Query work21–31ms on observer process,
not a target UI pause. Private346,185,728B; write-combine141,357,056B throughout.
Same graphics total as prior13:55UTC snapshot; intervening continuity unmeasured.

Main record: `artifacts/graphics-retention-20261020-r104/actual`.
Helper65156; requestedUTC14:12:55.9226530Z, firstsample14:12:57.1059979Z.
Bounded600s,5s interval, expected finish about23:23KST.
First private346,218,496B; write-combine141,357,056B; GDI68/USER44.
First region list has six regions in five allocation bases (one split region).
Compare aggregated allocation bases, not raw region count as an object count.

User asked after recording start: five100–150% round trips, menu closed/rest20s,
five more, close menu/report completion, leave app running. User reported
“끝” at about23:14KST; this confirms reported completion, not measured exact count.
The existing internal recorder expired: **no current exact input counts**.
User report and observation windows are separate evidence from instrumented
slider events or direct screen inspection. Do not imply requested counts occurred.

Interim through14:15:56UTC (~180s): write-combine143,192,064B, +1,835,008B
(1.75MiB) vs baseline. Four large allocation bases unchanged; the fifth base
1827193290752 grew7,405,568→9,240,576B. This is additional commitment within an
existing allocation, not five new surfaces or proof of a permanent leak.
Private348,491,776B at this point; early peak354.875MiB. Await remaining idle
interval before judging growth/retention. No extra user repetition requested.

Existing finite memory-result heartbeat `r97` repurposed (not a second monitor):
10-minute interval, two occurrences maximum, completion analysis then pause.
Old r98 prompt replaced; no old app or shutdown operation is authorized.

## Pending decision

After completion: verify identity, reason, monotonic/wall gaps; compare minute
windows and per-allocation growth/release, plus final idle portion. If no renewed
growth is reproduced, do not invent a cache purge or claim all leaks fixed.
If continued accumulation occurs, investigate current ownership; announce
immediately before any pausing heap/trace and exclude it from normal timing.
Do not repeat unrelated visual tests already accepted.
Record bounded result and limitations in this file/TASKS/ROADMAP; disable the
finite follow-up. Do not restart/kill app or PC, or revive old shutdown automation.

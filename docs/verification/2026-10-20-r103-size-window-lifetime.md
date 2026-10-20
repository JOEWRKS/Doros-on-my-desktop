# r103 — continuous size: native window lifetime and retained memory

## Scope and current status

**Superseding actual result:** six-minute run completed normally, same PID/start,
361.9366s/173samples, max gap2.3553s. Successful size applications949,100–150%,
between87.69–178.90s in two main interaction periods. Apply median6.8242ms,
p9510.8827ms, p9914.1062ms, max17.727ms. These are update-handler times, not
mouse-to-screen latency. User: “끝. 위치야 바뀌는데 딱히 문제는 아닌듯.”
This reports no concerning position issue, not proof of invariant desktop foot
position or an explicit separate quality check.

Private memory peaked398.86MiB, post-first period~296MiB, post-second~326MiB,
ending331.64MiB. Last30s331.56–332.23MiB; GDI66–68, USER46–47. No continuing
steep rise in the final idle window, but the second operation block did leave
more committed memory. Total-private reduction / all-memory-fixed is not passed.

After normal observation ended, a separately announced heap snapshot found
96,027,564B reachable objects, slightly below old r102's96,401,972B with a
different workload. The64-slot motion cache remains bounded; capture sample
lists are disposed/empty. **181 byte arrays with exact slider-frame sizes total
67,709,576B; only13 /7,668,024B are reachable. Thus168 obsolete frame-sized arrays,
60,041,552B (~57.26MiB), are awaiting natural collection.** This is allocation
churn, not a permanently rooted leak. r103 `SetArtSize` still clears its frame
cache without retaining slots and exact-length reuse rejects different sizes;
that remaining cause is being corrected in r104. No forced collection.
Sources: actual/managed.txt, all-and-owners.txt, frame-arrays.txt,
big-byte-arrays.txt, summary-final.json.

Actual post-input write-combine committed91,222,016B; later passive snapshot
91,811,840B. The difference is589,824B, not proof of zero graphics growth.
Previous controlled diagnostic reduction is not a claim that this larger
real-user workload has lower total private than r102.

User accepted r102 slider following and requested remaining resize latency and
retained-memory investigation. Preserve artwork, quality, motion, speed and foot
position. Only product change: `Runtime/PetWindowViewport.cs`. No Core, renderer,
slider timing, image source, installed build or saved-preference changes.

Candidate actual run started 2026-10-20 22:30:34 KST after user reported exiting
r102 and process absence was checked. PID45036 / startUTC
2026-10-20T13:30:34.1632978Z / ticks639280998341632978; helper60180.
Internal capture300s / passive external360s. **Actual repeated-input result is
pending**, not a pass. User was told to do five100–150% round trips, close menu,
rest20s, then five more and leave the app running. These are requested actions,
not yet verified actions.

Final published App SHA256:
`9237E47488B4CD8C4F7BB0FA42E5270B6A07DD994417154DE31915663272E2A6`.
Tests and final viewport probe reference that identical DLL. Core stays
`D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`.
Final release is framework-dependent like r102. An intermediate self-contained
publish was used only as a DLL source for a framework-dependent diagnostic
probe; it was never launched as the normal pet or installed.

## Cause and correction

r102 set transparent native Window.Width and Window.Height at every changed
percent, then moved the window for foot anchoring. A real transparent-HWND
component replay separated these operations from drawing. Initial600-step
replay: median total16.77ms; native size stage10.93ms; render3.44ms. The actual
normal r102 run had median30.27ms per successful size application, so the
component replay is not presented as identical real-user latency.

r103 keeps a408×408DIP outer backing window for the100–150% range. The original
parent remains the current logical viewport (pet size plus96DIP each side),
left/top aligned. Presenter centering, swing bounds, logical position, foot
anchoring and pointer mapping continue to use the old dimensions. Native
device-pixel rounding of those dimensions is preserved: at96DPI,110% used a350px
viewport and95.8DIP presenter origin, not350.4px/96DIP. No image resampling,
resolution reduction, transform-based enlargement or pose-time change.

Legacy low-resolution behavior remains on the existing variable viewport.
Transparent unused outer area has no background/hit target. The same existing
WPF DPI mapping is read; different physical-monitor DPI transitions remain
unverified.

## Memory ownership: existing r102 process, not a speculative leak

Evidence root: `artifacts/size-lifetime-20261020-r103`.

Before collecting `r102-retained.dmp`, chat explicitly warned the app could
pause. Collection completed; all further SOS/CDB analysis was offline. No
forcedGC, working-set trim or process termination. This diagnostic interval
does not count as normal performance measurement.

- Reachable managed objects:96,401,972B. One live WriteableBitmap, one
  HighResolutionLocomotion owner with its bounded64-entry cache; not51 retained
  display bitmaps for all slider sizes. Float arrays total68,901,568B and byte
  arrays24,201,637B include art/caches/scratch.
- GC committed space totals162,447,360B. Already-free LOH46,921,992B plus
  SOH5,780,048B. Thus private/committed bytes are not all live retained objects.
- Default native heap32,208KiB committed includes11,121KiB free. One other heap
  could not be decoded by CDB; do not label that an app corruption diagnosis.
- Read-only VirtualQueryEx snapshot: four committed write-combine allocations
  total71,168,000B. Prior r97 address/stack evidence established WPF/D3D
  upload/render-target/effect allocation paths in that earlier process.
  Current total alone does not prove the owner of every byte or a driver leak.

Sources: `managed.txt`, `owners.txt`, `cache-owners.txt`, `native-heap.txt`,
`r102-regions.json`. No r102 pre-input heap was collected, so the entire
~96MiB pre/post difference cannot be assigned to one owner or claimed reclaimed.
The confirmed correction removes repeated native surface-size churn rather
than purging useful motion caches or forcing counters down.

## Controlled diagnostic replay

`NativeResize.cs`:600 deterministic idle size applications on a real transparent
WPF window, with per-stage stopwatch timing and passive counters. No desktop
mouse injection and no forced collection. Same framework-dependent probe for
baseline/fix, sequential runs without overlapping builds/tests.

`native-before-final` / `native-after-final` tested the fixed-backing candidate
before the final sub-DIP rounding correction:

| Measure | r102 | fixed backing diagnostic candidate |
| --- | ---: | ---: |
| total apply median (exclude first20) |19.45ms|7.91ms|
| total apply p95 |31.17ms|13.58ms|
| max |82.41ms|192.70ms|
| last private |196,870,144B|160,325,632B|
| last write-combine committed |98,197,504B|68,931,584B|
| GDI / USER |16 /12|16 /12|

Candidate max192.70ms was183.92ms in final window movement, not native-size
change(0.19ms); it is retained as an outlier, not hidden or called fixed.
Candidate write-combine held67,751,936B from steps250–550 then ended68,931,584B.
This is a bounded graphics allocation reduction under replay, not proof of
zero future graphics-pool expansion or actual-user private-byte savings.
The final rounded candidate requires the actual run above for performance.

## Correctness checks

- `tests/rounding-final.trx`:25 passed,0 failed. Includes live foot anchoring,
  resize rejection during direct/fall/snack, gait rebase, slider/timer lifecycle,
  wide swing and viewport tests. New real-HWND test checks every integer
  slider value100–150 against the former native-size mapping.
- `viewport-before-valid2/viewport.json` vs `viewport-final/viewport.json`:
  50/50 identical serialized records — five sizes, both facings, five
  angles(-88,-45,0,45,88); rendered pixel hashes, logical viewport dimensions,
  presenter origin, transform diagnostics and bounds match.
- First viewport proof (`viewport-before`, `viewport-after`) was invalid after
  the first window because default WPF shutdown stopped later layouts.
  Discarded, not counted. Explicit shutdown and positive layout guards added.
- Subsequent comparison detected the real0.2DIP origin difference at110/140%.
  Native device-pixel rounding correction resolved it; earlier intermediate
  proof is not final acceptance.
- One initial unit-test harness arranged a never-shown Window, producing zero
  layout. Corrected to arrange the fixture root. Its failure is retained in
  `tests/focused.trx`; final native-HWND test is separate.

Artifacts are local. No install, registry/power changes, auto app exit or PC
shutdown. No existing long-run automation re-enabled. Next: validate actual
size events, time/gap identity, repeated-burst/post-input memory, and user
feedback; only then report the bounded outcome.

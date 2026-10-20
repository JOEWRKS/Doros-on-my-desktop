# r104 — preserve frame storage across accepted continuous resizing

## Final bounded actual result — 2026-10-20 22:56 KST

Completed normally:360.2714s,172samples, same PID32820/startTicks, maximum
monotonic gap2.3541s. No app restart, sleep-sized gap or diagnostic pause inside
the observation. Capture ended13:52:41.5266289Z; the announced heap collection
was **after** this, not included in timing.

Actual successful size applications:494 between27.556–57.627s, covering100–150%.
Log shows ten directional sweeps/five round trips; not494 round trips and not
the originally requested two separated five-round-trip blocks. No extra repeat
was requested to inflate this bounded result. User reported “문제 없음.”

| Actual update metric | r104 |
| --- | ---: |
| median apply work |6.6251ms|
| p95 |12.1218ms|
| p99 |16.1631ms|
| max |24.2256ms|

The r103 latency improvement remains in this run (r103 median6.8242ms;
r10230.2696ms). These are handler times, not GPU-presented FPS or mouse-to-screen
latency. Different actual workloads are not a controlled speedup ratio.

Private peaked359.48MiB and ended338.84MiB. Final30s338.57–338.87MiB; median GDI68,
USER45, handles702. Later idle windows were not monotonically increasing
(240–270s median339.97MiB;270–300s337.24MiB;final338.77MiB). No continuous leak
is established by this bounded interval, and total-private savings are **not**
established either.

### Final heap: the corrected frame path

`actual/managed.txt`, `owners.txt`, `slot-counts.txt`:

- All slider-sized frame arrays:13 /7,668,024B; reachable12 /7,078,176B.
  Only **one /589,848B (~0.56MiB)** awaits collection, versus r103's
  168 /60,041,552B. Actual runs had different numbers of applications; use the
  equal2,000-change replay below for causal allocation-reduction quantification.
- Snapshot caught the frame cache mid-render: dictionary12slots minus1free =
  11entries, spare stack0. One live WriteableBitmap. Motion cache64, one owner.
  No51-size cache or expanding retained-owner chain found.
- Reachable managed total95,408,768B. GC committed135,659,520B; already-free
  LOH21,218,320B. Useful caches, free heap space and unreachable objects are
  different categories, not all permanently leaked memory.

### Remaining memory limitation

Read-only `final-regions.json` and `followup-regions.json`: five committed
write-combine allocations total**141,357,056B (~134.81MiB)**. Same addresses/sizes
at13:54:06 and13:55:54UTC (107.65s apart). This is two stable snapshots, not
continuous multi-hour evidence. Prior r97 ETW attributed similar regions to
WPF/D3D upload/render-target/effect paths; exact current allocation stacks were
not collected. Do not claim a driver bug, that all graphics retention is fixed,
or that the whole app now has lower private memory. No speculative graphics
purge, forcedGC, working-set trim or quality change.

**Disposition:** native-size churn and discarded-frame allocation causes fixed;
actual slider behavior accepted and bounded repeated run completed. Overall
graphics-memory retention / long-duration leak absence remain unverified.
r104 left running normally; both finite recorders finished. No new automation,
automatic input, further user repetition, app termination or PC shutdown.

## Scope / current actual state

Continuation of the user's request to fix resize delay and memory retention
without changing accepted slider behavior, artwork, quality, shape or speed.
r103 corrected native backing-window churn and verified949 actual size updates:
median6.8242ms vs previous r10230.2696ms. See
[r103 evidence](2026-10-20-r103-size-window-lifetime.md).

r103's separately announced post-run heap revealed the remaining confirmed
allocation cause:181 exact-size slider-frame byte arrays /67,709,576B existed;
only13 /7,668,024B were reachable. The168 obsolete arrays /60,041,552B were
awaiting natural GC, not permanently retained by a live owner.

Current r104 normal transparent process started after user exit confirmation
and absence check at22:46:38 KST:
PID32820/startUTC2026-10-20T13:46:38.3929362Z/ticks639281007983929362;
external observer47888,300s internal/360s external.
App SHA256 `374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322`.
Published probe and tested DLL match. Core remains
`D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`.

User answered “문제 없음” to the repeated-adjustment/visual-regression question.
This is user acceptance feedback, not independent OS visual inspection or an
exact input count. **Performance/memory result is pending at this entry.**
No synthetic desktop input, forcedGC, trim, installation or preference changes.

## Small, bounded lifetime correction

- `SetArtSize` used to clear the12-entry cache and abandon its pixel arrays.
  New spare stack retains those same slots. Active+spare slots together stay
  at most12; not12 per size and not a51-size bitmap cache.
- Presenter rendering requests high-water reuse. An oversized array is reused
  if it can hold the new frame; allocation is necessary only when a slot first
  grows. Ordinary retained-raster callers keep the default exact-size contract.
- Display surface and alpha-hit image validate sufficient capacity and consume
  only the bitmap's explicit width/height/stride. Spare tail bytes are not
  displayed, hit-tested or scanned for feet. Existing contour invalidation and
  dimension checks remain in place.
- Native bitmap dimensions, image sampling, DPI, compositing, frame keys,
  animation clocks and slider dispatch are unchanged. r103 logical viewport
  and native-pixel rounding are retained.

Tradeoff: after shrinking, at most12 buffers keep the largest capacity they
have used. At96DPI/150%,12×589,824B =7,077,888B, the already-existing full-size
12-frame capacity. This avoids dozens of MiB of dead resize arrays rather than
trying to make private bytes instantly return to startup. Other physical-DPI
transitions are not certified by this run.

## Verification before actual launch

Evidence: `artifacts/size-storage-20261020-r104`.

- `tests/storage.trx`:44 passed,0failed. Includes new high-water frame identity,
  active+spare≤12/no duplicate slots, exact pixels after grow/shrink, stale-tail
  exclusion from alpha/sole measurements, too-short-buffer rejection, existing
  live surfaces, motion/sole, foot mapping and slider lifecycle.
- `viewport/viewport.json` vs final r103:50/50 pixel hashes, logical dimensions,
  presenter origins, transforms and bounds identical.
- Sequential same-host published-DLL presenter stress,2,000 changes each:

| Allocation measure | r103 | r104 |
| --- | ---: | ---: |
| total allocated managed bytes |918,193,632|73,997,712|
| warm100-change median allocation |44,446,344|1,120,984|
| ending private bytes |81,448,960|83,283,968|
| natural Gen2 count |217|259|

The allocation reduction is established. Ending-private savings and fewer
collections are **not** claimed: WIC bitmap lifetime/memory pressure remains
separate. This is deterministic component replay, not actual mouse interaction
or proof of final screen latency.

## Files / limits

Product files: DororongPresenter.Size.cs, DororongPresenter.HighResolutionGrab.cs,
HighResolutionGrabRaster.cs, HighResolutionDisplaySurface.cs, AlphaHitTestImage.cs.
New regression: SizeStorageTests.cs. No original art files modified.
One pending bounded actual run; no old automation or shutdown request reactivated.
Next: same-PID/gap/finish validation, actual successful updates and latency,
post-input memory; if another heap is necessary, announce immediately beforehand
and keep it outside normal measurement.

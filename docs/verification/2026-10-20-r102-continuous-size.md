# r102 — continuous size input and bounded allocation

## Authority / protected scope

User reported r101 size/foot/click operation “괜찮음”, but rejected the wait-until-stop
feel. Requested continuous slider following, preserving artwork/quality, accepted
motion/speed and foot placement, with actual repeated input performance/memory
verification. This is a local interaction repair, not a new menu composition.

## Confirmed causes and changes

1. PetSizeMenuItem reset its 100ms timer on every ValueChanged. A continuous drag
   could indefinitely postpone application. Now it arms one pending composition
   callback and consumes the latest integer size without queuing intermediate work.
   Mouse release/dismissal flush the final choice; Unloaded detaches the static
   render subscription and saves any applied change once.
2. CompositionFrameTimer's optional timestamp-preserving start deduplicates the
   same composition timestamp across slider stop/restart. Existing normal-loop
   Start() behavior remains unchanged.
3. Foot anchoring translates the pet's origin; this is not walking distance.
   RebaseSizePresentation adjusts the previous locomotion origin so the next
   walking render does not add that translation to gait distance.
4. Published r101 resize replay confirmed repeated full-frame scratch allocation.
   HighResolutionGrabRaster now retains one geometrically grown high-water scratch
   (<2x largest requested size). Only the current image region is read/copied;
   retained/display frames remain exact-sized. No resampling, source simplification,
   frame-quality reduction, global cache expansion or forced GC.
5. Explicit performance capture logs successful `size:N` applies and their workMs.
   These measure application cadence/duration, **not** raw input-to-display latency.

## Scoped verification

Artifact root: `artifacts/size-stream-20261020-r102`.

- `tests/stream-final.trx`: 41 passed, 0 failed/skipped. Latest-value scheduling
  during continuous input; same-timestamp suppression; unload/final save; gait
  distance preservation; scratch reuse/pixel parity; footprint and sole geometry.
  Existing xUnit blocking-test analyzer warnings elsewhere remain unchanged.
- Published-DLL probe `proof/evidence.json`: both menu layouts, 12 routed key-event
  checks, five sizes100/110/125/140/150. All five raw idle pixel hashes match r101;
  character96 menu PNG also exactly matches r101 (AC797A3DEC094C3558EBC5F5EEBC88054D9061C05CC3BB64A69563D133C861CE).
  These are component rendering/event tests, not physical desktop input.
- `baseline-stress/stress.json` vs `stress/stress.json`: 2,000 source-quality
  resize/sole renders each. Warm100-change allocation ~82.0–82.1MiB →42.3–42.4MiB.
  Total allocated1,748,428,984→918,183,824B. Candidate private71.5–81.7MiB across
 20 batches, GDI2 stable; no monotonic accumulation in this synthetic replay.
  Candidate timing median3.23ms / p95 5.15ms / p99 6.58ms / max85.98ms. This is not screen
  FPS. Baseline overlapped test compilation, so timing ratios are not an isolated
  speedup claim. Natural Gen2 counts129 vs215 differ; do not claim fewer GCs.
- Independent read-only reviewer checked timestamp lifecycle, scratch/retained
  ownership, explicit-extent readers, gait correction, and measurement boundaries.
  No blocking defect identified in that scope. Formal JOEDESIGN certification and
  unobserved native-focus/high-contrast/multi-monitor cases are not claimed.

## Actual run — recording started 22:04 KST

User replied `ㄱㄱ` to exit/readiness request. Prior app absence checked.
Normal transparent r102 PID62576/startUTC2026-10-20T13:04:00.9670563Z,
Started13:04:02.0798567Z and Responding=true verified.
App DLL C194803A709EB2E7A52DBDC00BB1F5DBE752CD0600A6E3BEB9E4D9A0343EEE31.
Core remains D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D.
Details: `actual/run.json`; internal180s, external240s/helper63124.

User asked to sweep100↔150 for first minute, report end, close menu and keep pet
running for idle recovery. No automated desktop input, heap capture, process pause,
forced GC/working-set trim, installation or power changes.

Actual apply count, visible smoothness and post-input memory are pending until
records/user feedback are checked. Do not turn the synthetic run into an actual
input pass or claim universal/long-duration leak resolution.

## Final bounded actual result — 22:08–22:10 KST

The preceding pending-recording statement is superseded by this result.

- User: “ㅇㅇ 좋네 문제 없는듯”. Accepted continuous following/visible result;
  no newly reported foot/shape/click issue. This is user visual feedback, not
  an independent screen recording or exact repetition-count report.
- `actual/completion.json`: duration_completed, no error, 241.8669s, 114 samples.
  Same PID/start ticks and Responding confirmed. Max monotonic gap2.5058s,
  max wall gap2.5053s; no sleep/long observation gap in this bounded record.
- `actual/runtime.json`: successful size applications53, range100–150,
  first131.539s/last135.649s. Values show one direction reversal. This is one
  observed down/up sweep, **not** 53 full sweeps, a full minute of input, or a
  long-duration real-input repetition test. Other direct-input events0.
- Apply work: median30.27ms/p9544.89ms/p9945.93ms/max46.55ms. Active loop tick
  interval median9.37ms/p9525.39ms/max40.64ms, loop work median2.39ms/p956.88ms.
  Separate apply work is excluded from loop work. These do not establish display
  FPS or input-to-photon latency, and do not support a zero-stutter claim.
- Before this first size interaction, private memory was~195MiB. Input interval
  peak355.74MiB, then~291MiB. Last30s private290.76–291.52MiB/median291.16MiB;
  GDI66, USER45 and medianhandles701. Post-record spot check290.88MiB.
  No sustained rise in the last observed idle segment; one-time retained increase
  from pre-interaction baseline is real and is **not** assigned a proven owner by
  this observation. No heap dump or forced collection was performed.
- `actual/summary-final.json` stores full30-second windows and stats. Initial
  summary console projection printed nulls due PowerShell Hashtable projection;
  saved JSON was valid. Console projection corrected to PSCustomObject and the
  actual saved values independently read back above.

No further changes made to the accepted executable after this run. r102 remains
running normally; diagnostic collection and external helper ended. Installation,
art assets and motion speed remain unchanged. User-requested continuous behavior
and bounded validation are handed off; actual multi-sweep/long-duration retained
memory behavior and eliminating the measured30–47ms apply tail are not claimed.
No indefinite repeat tests or new monitoring automation.

# r90 — observer isolation and pixel-completion waits

## Scope / actual evidence

User requested separating measurement overhead from renderer waits, checking memory, and preserving accepted motion, shape and speed. Warn before any diagnostic that may pause the running app. No art, Core, curves, frame selection, snack speed, settings or installation edits.

r89 recorded an88.9879ms diagnostic callback and worker waits up to68.3751ms. The callback performed OS process-memory queries synchronously on the WPF thread. Its counter did not subdivide process queries and state snapshot cost, so the entire88.99ms cannot be attributed conclusively to a specific syscall. r90 moves OS queries off the UI thread and records queryMs and snapshotMs separately. Live layout invalidation remains an open path; r89's conditional layout guard still flushed31142times in the native run.

## Changes

- DiagnosticMemorySampler: one sequential asynchronous query loop, cancellable delay, at most two pending samples, explicit failure samples; no WPF access and no UI wait on a slow query. Stop does not wait for a pending OS query. Samples are timestamped at query start and contain query duration/value/error, not false zero values.
- RuntimePerformanceCapture drains finished samples without querying the process. At deadline it detaches callbacks and transfers owned sample lists to a worker for JSON streaming; it releases lists after export. MainWindow awaits capture/frame exports only when the user is actually closing the app. CPU end queried by export worker includes possible dispatch delay, explicitly labeled. No forced GC added to product.
- RasterRowScheduler formerly waited for every claimed worker to acknowledge completion, including workers descheduled before doing any rows. Now completion follows finished pixel chunks. Still-running pixel writes must finish before returning; late empty jobs need not wake first. Finished batches clear the row delegate so delayed jobs do not root whole raster banks. Synchronous per-pixel math/order within each row unchanged; no reduced-quality or skipped-frame fallback.

## Verification

- `artifacts/render-cost-20261019-r90/tests/wait-before.trx`: deterministic claimed-but-descheduled-worker reproduction failed, while active-pixel safety test passed (1fail/1pass). This demonstrates one real scheduling defect, not proof that all r89 stalls used it.
- `tests/targeted.trx`:18pass including worker correctness/exceptions/concurrent callers, buffer weak lifetime, five-scale layout checks, diagnostic cancellation/error handling and streamed-export lifetime.
- `tests/final.trx`:646pass/1fail/0skip in3m58s. The failure was an incorrectly supplied paw baseline path (directory-like `paw-before` instead of `paw-before.json`), not a pixel mismatch. Preserved this result. Corrected the invocation and added an explicit await for asynchronous capture export in the landing integration test; `final-recheck.trx`:10pass. By testId the combined final results are647 unique tests passed (the10 overlap, not657). Two existing theory cases share a display name, so grouping by name would undercount by1.
-420general/80paw/120snack-effect raw hashes match pre-change references. All335synthetic landing frames (67at each1/1.1/1.25/1.35/1.5) match r89 raw hashes AND relative timestamps. Pixel-level computation and accepted motion clocks unchanged.
- Published/tested App DLL SHA256 `AF23353862CD1ED38F04F74F3418407F1AFC461905FF15289CAEBEB488EF69D2`; Core unchanged `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`. Test warnings about intentionally blocking worker/STA synchronization were not product build errors.

## Native run

User answered “00” to tray exit; process absence verified. Started normal transparent1.5x r90 PID142928 at20:52:21KST,300seconds, announced start immediately. No tests/builds/dumps during recording. User replied **“같았어”** to whether interactions and accepted motion remained the same. This is not a claim of perceived performance improvement.

`artifacts/render-cost-20261019-r90/native.json` SHA256 `37380FB503E5AE79D2A10B83F1AD38DB3371D6E7F787917B338C484D0D6F1DE3`. Elapsed300007.0485ms, CPU373750ms summed across cores (export-worker end query may include dispatch delay). Inputs6:Body3,RightCheek2,snack-toggle1; last input175767.7527ms. Not the same workload as r89's30inputs, so no normalized improvement percentage.

Excluding first5s:

| Metric | r90 observed |
| --- | --- |
| UI diagnostic callback | max2.748ms,p99.141ms,0over50ms |
| UI snapshot portion | max2.7435ms |
| Background process query |294samples,max104.9536ms,p9982.0887ms,10over50ms,0errors |
| Raster | max29.689ms,0over50ms |
| Worker completion wait | max23.3849ms,p99.7578ms,0over50ms |
| Forced layout | max1.9722ms,28198flushes |
| Tick interval | max119.2828ms,5over50ms |
| Tick work | max55.5886ms,1over50ms |
| WPF callback interval | max121.6085ms,27over50ms |

This demonstrates that slow OS query durations are now measured outside UI callbacks; it does not make scheduling/CPU interference disappear. r89's UI diagnostic max88.99ms and worker wait68.38ms were not observed in these corresponding r90 counters. Residual long callback intervals remain and **do not pass complete latency acceptance**. At29.920s the121.61ms callback interval had raster5.27ms,layout.030ms,worker wait.032ms andGC deltas0. A55.59ms update occurred just after the snack toggle at30.974s; temporal proximity alone is not an identified blocking stack. Other long intervals also have short raster work. No claim that every delay is a worker wait or GC.

### Actual strong landing

Three landing entries:10.918s strong,19.935s and86.015s ordinary. Strong first contact renderedSpread1, preceding Falling sample.995237. Internal clip `native-frames/landing-20261019-115222-7087c4e1b8364578ab2f3aace69927fd/frames.json`:triggered=true,error=null,70frames. Directly opened PNG013/014/015/016/017:

-013:contact-81.35ms,upright airborne,spread0.
-014:contact-48.46ms,airborne spread.674665.
-015:contact-30.31ms,airborne nearly expanded,.995237.
-016:first contact,spread1.
-017:contact+18.63ms,recoil.940025.

These actual CPU frames preserve pre-contact spreading, alongside the user's unchanged-motion response. They are not DWM scan-out proof or a new native run at all five scales. Full recovery beyond the bounded clip is covered by runtime logs/user observation, not direct inspection of every displayed frame.

### Memory / warned post-recording dump

Allocated1674.956MiB,Gen2count5. Managed peak185.321MiB,end-near174.466MiB. Private memory313.94MiB at60s,312.18MiB at150s,320.66MiB at299s; thus do not call private memory entirely flat. Recording retains frame/tick/state lists and a bounded54MiB landing buffer until export. First asynchronous memory sample precedes landing-buffer allocation, so its21.38MiB baseline is not comparable to r89's synchronous initial sample.

After recording/export, explicitly warned the user that heap collection could briefly stop the app, then collected `r90-after.dmp`. Collection completed and PID142928 Responding=true was checked. This diagnostic stop is outside `native.json`; no silent dump and no forced-GC product workaround.

- Live float arrays97 /86,488,728B; dead float listing empty.
- Live byte arrays1183 /23,794,724B: bounded54MiB landing storage no longer retained live after export.
- Exactly one live snack buffer. No live RasterRowScheduler.RowBatch found at the snapshot. DiagnosticMemorySampler itself remains one object owned by the stopped recorder; no live Sample instances were listed (JSON type metadata is not sample payload).
- Different state/cache composition from r89 means lower live-float bytes are not a leak-reduction percentage. This short run and snapshot establish these retention boundaries only; long-duration growth and native/WPF allocations remain unproven.

## Remaining / handoff

r90 remains running normally; capture finished. Accepted shape/speed/motion unchanged by tests and user response. Observer UI blocking and empty-worker acknowledgement waits are corrected and validated within the stated scope. Remaining dispatcher/non-raster gaps, frequent live layout invalidation and long-duration retention are not passed. Next is to attribute those non-raster gaps with an explicitly announced trace and compare post-export retention over a longer bounded interval. No installation/settings change or unannounced pause. Keep CPU-frame inspection, user observation and hardware presentation distinct.

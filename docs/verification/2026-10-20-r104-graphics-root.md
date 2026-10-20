# r104 remaining graphics commitment — creation/retention investigation

## Result — 2026-10-20 23:43 KST

User approved the elevated finite trace. Start and stop/potential stalls were
announced immediately beforehand. Diagnostic run completed; no product patch
was justified by the reproduced behavior.

### Collected evidence

Root:`artifacts/graphics-root-20261020-r104`.
Unique ETW session `DororongAllocation-814669e5e7894d45bde9d8137200c8be`,
helper69884, UTC14:37:24.6164696–14:39:40.398178 (135.78s).
ProbePID69048/startUTC14:37:32.4131884,109.1175s successful replay.
Published r104 DLL hash checked inside probe; product unchanged.
Collector/probe exited, owned session absent, normalPID32820 still responding.

Analyzer emitted UTC event timestamps for phase correlation. `trace/graphics-
summary.json` and `graphics-events.json` join observed committed allocation-base
address ranges to that **same PID's** VirtualAlloc stacks. Symbolized probe and
normal-app analyses both report **EventsLost=0**.

| Probe phase | committed start→end | new commit calls into observed graphics ranges |
| --- | ---: | ---: |
| warm |9,437,184→13,131,776B|9|
| menu |13,131,776→17,657,856B|17|
| first1,000 size changes |17,657,856→53,309,440B|96|
| repeated identical1,000 changes |53,309,440→53,309,440B|0|
| first flutter / repeated flutter / subsequent rest |53,309,440→53,309,440B|0|
| closed window, managed raster/display retained |53,309,440→53,309,440B|0|

These are deterministic component calls through the published renderer, not
instrumented mouse gestures or performance acceptance under trace load.
Warm/menu include diagnostic text/layout, so their byte totals are not an
exact model of the normal app's complete UI. Asynchronous composition can
straddle phase boundaries; address-correlated stack paths are stronger evidence
than phase labels alone.

### Creation path established for the reproduced growth

First-size-phase35,651,584B net increase matches the96 commit requests:

- 61calls /28,966,912B:
  `CHwBitmapColorSource::CreateTexture → CD3DVidMemOnlyTexture::Create →
  CD3DDeviceLevel1::CreateTexture`.
- 35calls /6,684,672B:
  `CD3DDeviceLevel1::PresentWithGDI → CD3DSurface::ReadIntoSysMemBuffer →
  CD3DDeviceLevel1::CreateRenderTargetUntracked`.
- Full stacks continue throughD3D9/NVIDIA user-mode driver andWindows graphics
  memory manager to committed virtual memory. Incidental module presence is
  not proof of a driver bug.
- Menu-phase stacks include blur/drop-shadow intermediate textures; no shadow
  removal or quality reduction was made.

This ties **this probe's graphics commitment** to texture creation and transparent
window presentation, rather than accumulating managed pixel-array owners.
`HighResolutionDisplaySurface.Update` intentionally creates an exact-size
WriteableBitmap when output extent/scale changes. Repeating the same changes
after warm-up produced **zero further committed-region growth**, consistent with
reuse of already committed graphics capacity. This is observed bounded reuse,
not proof that every byte is an unused cache or that all driver/WPF resources
are free. The exact internal suballocation/free-list owner is not exposed by
these VirtualAlloc stacks.

Closing the diagnostic window while retaining managed renderer/display owners
did not return this committed capacity. AtUTC14:39:22.297–.302, **after the final
sample and application shutdown began**, ETW recorded reserve releases/
decommits for the same ranges. This is teardown release, not ordinary window
close reclamation; process exit reclamation alone is never leak-absence proof.

### Normal app and historical5.875MiB boundary

The same system trace also covered the unchanged normal r104PID32820.
`trace/normal-app.symbolized-events.jsonl` contains143 virtual-memory events
overall, **zero** in the original growing allocation's32MiB address range.
`normal-app-early.json` and `normal-app-final.json` both show the original five
allocations total147,517,440B, including13,565,952B in that allocation.
No new input was injected into the normal app, and no further growth was
reproduced there during this trace.

Thus the current-renderer growth mechanism and bounded reuse are demonstrated;
the exact historical5.875MiB call sites remain unrecorded and cannot honestly
be certified retroactively. The evidence does **not** establish a continuing
per-repeat leak or an application defect requiring a patch. No arbitrary
cache purge, software-rendering switch, original art/quality/motion change.

**Disposition:** requested bounded creation-path investigation completed with
this explicit historical-attribution limitation. No new version/installation,
no further user repetitions, no recurring monitor. Preserve accepted r104.
A future fresh step in the normal app would need tracing active at that instant
to attribute its exact bytes; do not claim all-memory leak absence.
Preparation notes below are historical and superseded by this result.

## Scope — 2026-10-20

User explicitly requested attribution of the retained5.875MiB and correction
only if defective, preserving accepted slider, appearance, quality and speed.
Previous observation:
[bounded graphics result](2026-10-20-r104-graphics-retention.md).

Normal r104 PID32820/startTicks639281007983929362 remains running.
No product change, restart, GC, trim, input injection or installation change.

## Evidence limitation

The historical5.875MiB increase occurred without allocation-stack tracing.
Heap snapshots do not retroactively recover those call stacks. Earlier r97
stacks cannot be represented as the current r104 region's creation evidence.
Same-class reproduction can explain a mechanism, but cannot certify the exact
historical bytes without a matching recorded event.

Existing dump excludes a growing managed bitmap chain (one WriteableBitmap),
shows bounded frame/motion caches and newly prepared flutter/body data, but
does not link the late4.125MiB native increase to those objects.

## Targeted diagnostic prepared, not yet executed

`tools/SizeSliderProbe/GraphicsReplay.cs` loads the exact published r104 DLL
(SHA256374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322),
creates its own transparent HWND and replays separately marked warm/menu/
first-resize/repeated-resize/first-flutter/repeated-flutter/rest/closed-window
phases. Records PID, UTC/monotonic phase times, allocation-base regions and
memory counters. No simulated desktop input or changes to normal-app settings.
Uses native resolution, retained raster storage and accepted NearestNeighbor
display policy. Component replay is **not** actual mouse interaction acceptance.
Closed-window phase keeps managed raster/display owners alive deliberately;
it only tests visual/channel disconnection, not total resource disposal.

Before first execution, compiler preparation caught reversed spread/scale
arguments and an incorrect probe-only sampling policy; both corrected before
running. Initial successful build alone was not runtime verification.

Proven bounded direct-ETW collector is available at
`artifacts/native-allocation-20261020-r97/symbol-analyzer/Analyzer.exe`.
Captures Process/Thread/ImageLoad/VirtualAlloc and allocation stacks in unique
session; maximum180s, owns/disposes only its own session. This requires elevation
and includes local process/module metadata. User approval requested, pending.
Announce immediately before diagnostic start/stop; keep timing separate from
normal app performance. No repeated manual interaction requested.

Next: after authorization, collect around probe start and correlate exact probe
addresses/phase times with symbolized allocation stacks. Check event loss,
repeated-work growth and after-close retention before deciding if a product
defect exists. Preserve unexplained native pooling as unverified, not fixed.

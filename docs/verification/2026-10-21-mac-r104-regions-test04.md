# Mac r104 bounded dirty-region renderer / test04 — 2026-10-21

## Scope

User authorized changed-region recomputation, exact comparison to the existing
full renderer, bounded memory, and a rebuilt Apple Silicon tester. Preserve
accepted motion/shape/quality/speed and Windows. Actual Mac execution must not be
called a pass. No active app was paused/restarted and no native trace, forced GC,
working-set trim, automated desktop input, installation change or power action ran.
Existing uncommitted test03 changes were retained, not reset or overwritten.

## Implementation and fallback contract

- Mac `OwnedFrame` opts into incremental rendering only for the internal presenter,
  which owns unmodified retained bytes. Public `R104Raster` still renders fully;
  callers may modify their returned buffer without violating a hidden cache contract.
- Reuse requires the same live buffer identity, size/density/body scale, inverse
  transform, progress, grab/lag/spread and every face parameter except EyeX/EyeY/
  Breathing. Pose, angle, blink, size, cheek/paw/lag, snack/effect changes fall back
  to full rendering. Nonzero spread, material lag, cheek follow, snack effects and
  flutter are conservatively excluded from local updates.
- Breathing changes body samples only in the sampler predicate x[50,84),y[49,74).
  Gaze is clipped to its stationary aperture x(16,45),y[46,63]. Expanded rectangles
  are transformed through unchanged head/body matrices; all four corners and a
  destination-pixel margin cover sampling footprints. No approximate pixel values.
- Clear the dirty rectangle and recalculate its intersection with the original
  full-render bounds. All other bytes remain in the existing display frame. Return
  to zero/boundary transitions erase old content using the same destination region.
  Unsafe/unhandled transitions clear and render the full frame. Sole sampling uses
  the independent original scratch, never the display frame.
- Only fixed-size pose/region metadata and a weak identity are added. No frame/tile
  history, extra image buffer, strong cached-frame owner or expanding dictionary.
  High-water retained and sole buffers keep the previous allocation policy.
- The main sampler is now a Mac-local arithmetic-identical copy with two hooks:
  selective clear and loop clipping/pixel count. `VerifySampler.ps1` reconstructs
  the expected copy from unchanged Windows source and rejects any other difference.
  Shared partial sampling/AA/gaze/compositing routines, banks and Windows remain
  untouched. This explicit copy avoids injecting platform changes into Windows.

No frame-rate cap, motion time, source image, resampling filter, interpolation,
display density, hit threshold, speed or upload cadence changed. Head movement,
walking pose changes and interactions still use full rendering where required;
this is not a general tile renderer for every animation.

## Pixel and behavior verification (Windows x64 host only)

Artifacts: `artifacts/macos/r104-regions-20261021/`.
The immutable baseline-host Raster SHA256 is
`89F2559576C8244E00F8DDF50BEA961A7704484BB40F464A464E2E61B4723CE5`,
matching test03's full-render Raster. Mac controller baseline is its host build,
not an Apple Silicon execution. No attempt to treat host rendering as native GPU.

- 13 dirty-region regression cases: five sizes100/112.5/125/137.5/150% × density1/2,
  identity/reflection/affine transforms, rotated head, breath return-to-zero, gaze
  extremes/zero crossings, combined gaze/breathing, blink, cheek, spread, lag,
  walking frame, mouth/drool fallback, slider/density changes and new buffer identity.
  Canvas edge/off-canvas bounds are also checked. These include998 active-frame
  exact comparisons against the original full sampling path (960 main+18 size+
  20 canvas-edge frames), plus400 repeat/copy-independence checks.
- Three before/after controller pairs,12 conditions ×240 ticks:8,640 compared
  samples. Every active-pixel SHA256, world position, contact geometry and published
  revision count agrees. There are2,880 sequence samples repeated three times,
  not8,640 independent poses or actual screen frames.
- The copied main sampler differs only by the reviewed hooks; sampling arithmetic
  is identical. This is backed by `tools/MacCpuProbe/VerifySampler.ps1`.
- Existing test03 non-60Hz eating diagnostic-fallback differences against Windows
  remain out of scope. No new image difference was found in the tested sequences;
  this does not claim every possible pose or a native screen passed.

Final suites: Raster59 and Mac headless41 passed, zero failures/skips. The existing
unused `_locomotionStarted` warning remains; no unrelated state code was changed.
Package regression11 tests also passed:111 distinct host/package tests total.

## Host cost: test03 full versus test04 incremental

Three sequential pairs; median of tick medians, milliseconds. The clock advances
by simulated16ms ticks. Tick timing covers controller Advance, not native Quartz/
Avalonia composition. Full results including outliers: `comparison.json`.

| Scenario | Size/density | test03 | test04 |
|---|---:|---:|---:|
| Idle breathing |100%/1|0.9373|0.4652|
| Idle breathing |100%/2|3.5911|1.6628|
| Idle breathing |150%/1|1.9721|0.9914|
| Idle breathing |150%/2|7.9171|3.6498|
| Autonomous idle→walk |150%/2|8.9860|4.1670|
| Seated, mostly unchanged |150%/2|0.0079|0.0157|

About54% lower idle tick time in the150%/density2 host case, not a promised54%
M2 CPU reduction. Aggregate process CPU also includes probe hashes/row preparation:
idle150%/2 median7875→2281.25ms per240 ticks, but seated150%/1 is78.125→93.75ms
and seated150%/2 is unchanged296.875ms. No claim that every condition improves.
The autonomous case contains idle and walking, not an isolated walking benchmark.

## Bounded memory evidence — not a native long-run leak verdict

One controller at150%/density2,600 warmup ticks, then12 batches of300 additional
idle ticks. Same retained buffer identity throughout3,600 ticks. Both baseline and
candidate retain2,359,296-byte display and2,359,296-byte sole scratch arrays.
No additional image cache and no large-frame replacement after warmup. Size-cycle
regression starts at maximum capacity and also verifies only one owned allocation.

| Host process metric | test03 | test04 |
|---|---:|---:|
| Private range, MiB |68.375–73.113|71.844–75.219|
| Private first→last, MiB |68.375→73.113|72.684→73.027|
| Allocated during3,600 ticks, MiB |14.393|14.403|
| Measured wall time, seconds |29.76|13.62|

Both remained at generation2 collection count3 in these measured batches; no
forced collection was performed. Candidate peak private memory was higher in this
short run, so **total process memory equality/reduction is not proved**. A constant
weak reference/pose metadata is not zero overhead. The confirmed result is no extra
pixel storage and no repeated frame-buffer accumulation in this workload. Raw
`memory-baseline.json` / `memory-candidate.json` preserve every batch. These are
simulated-time host runs, not hours of real-time use or native graphics-memory data.

## Native gate / handoff

test04 actual M2 CPU, display/GPU memory, long-run retention, Retina/Spaces/input
and visual acceptance are NOT_TESTED. test02 tester feedback cannot pass test04.
Use the exact test04 ZIP at the same size/display; compare30 seconds of idle,
walking and sitting. Check gaze→cheek release/blink/size changes for residual edges.
Do not lower image quality or resume Windows memory soaks to mask native results.

Package is local only; no commit/push/public release. Prior test03 ZIP is preserved.

## Verified local candidate

- `artifacts/macos/r104-test04-20261021/candidate/Doro-Mac-arm64-r104-test04.zip`
- SHA256 `007775F4237DB6B75C9D914E1637B03AF14EB7AAD3F76C215CFDEB103DFBB51A`
- 99,175,502 bytes,236 files,18 native code objects checked, bundle104.4,
  arm64 self-contained .NET8.0.32, macOS13+ test target. Page/special-slot digests,
  managed/resource seals, ZIP inventory/CRC and executable bits checked by the
  packager. These are not Apple's codesign, notarization or Gatekeeper acceptance.
- Candidate Mac DLL `39700D56DB5B019963A44D79D09E895DD7FDF3B472F28158D36F225557286192`;
  Raster DLL `86C7CBB20A1567CFBFDBB3B68E02AF2D1841FBDA818463C45B83A8F14B31D57F`.
- test03 ZIP rechecked unchanged:
  `2C5228B6DE29D2A6FE2250F66EB2DCF65B57CD0BBAD605AD8573D6BA25878A8C`.
- Windows App/Core diff versus accepted3813312 is empty. No image asset edits.
  New/changed implementation: Mac-only DirtyRegion/OwnedFrame/local main sampler,
  its project include and presenter call. Regression tests, probe, packaging and
  ledger docs updated. Prior test03 implementation remains in the dirty worktree.

# Bounded Mac-controller host CPU probe

This console executes the Mac r104 controller without native Quartz/Avalonia
windows. It is **not Apple Silicon or Activity Monitor verification**.

Build `dotnet build tools/MacCpuProbe -c Release`. Before changing product code,
freeze the host-built `Dororong.Mac.dll` and `Dororong.Raster.dll` in an immutable
baseline directory. Do not load osx-arm64 published assemblies on Windows.
Dependencies/Core come from the probe output, so keep them unchanged between pairs.

Run `dotnet tools/MacCpuProbe/bin/Release/net8.0/MacCpuProbe.dll BASELINE_DIR NEW.json`;
use the probe build directory for the candidate. Existing JSON is never replaced.
`Compare.ps1 -Directory DIR` checks three pairs named baseline-full2..4.json /
candidate-full2..4.json and prints JSON. Preserve all runs, not just favorable ones.

Each process uses 100/150% and density1/2, a 16ms simulated timestep, 90 warmup
ticks and 240 measured ticks. Autonomous uses deterministic random=.5 (idle then
walk), idle uses .99 (no walk); seated toggles sit and warms for another90 ticks.
The clock is simulated, execution is not real-time. No mouse/keyboard injection
or currently running pet is involved. Full hashes, positions and contact geometry
are compared. `frames` counts published raster revisions, not OS display frames.

Tick timing surrounds Advance only. Aggregate process CPU/wall/allocation figures
also include hash/JSON-row preparation and geometry reads, not native presentation.
CPU sums worker time and is not a percentage. Geometry1000 includes reflection/JIT
and can contain outliers. Short host runs, tiering, GC and other machine activity
limit inference; do not claim all CPU cases improve or infer M2 performance.

Optional third argument `memory` runs one150%/density2 idle controller for600 warmup
ticks, then12 batches of300 ticks. It asserts fixed retained-frame identity and
records frame/scratch capacities, private bytes, allocated bytes and natural GC
counts. No forced GC/trim. Simulated ticks are not a wall-clock/native soak.

`VerifySampler.ps1` checks the Mac-local main sampler against the Windows source:
only region clearing, loop clipping and a diagnostic pixel counter may differ.
Pixel sampling/AA/compositing arithmetic must remain identical. This check does
not replace before/after frame equality or Mac execution.

## Long host memory soak

`MacCpuProbe.dll ASSEMBLY_DIR FRESH_DIR soak 120 paced` runs one controller for
two real wall-clock hours with 16ms simulation steps, paced without catch-up.
Each 7,500-tick cycle runs 100–150% size waves (every fourth tick), sitting,
head grab/release, cheek pull/release, snack chase/eating, then autonomous behavior.
Density alternates 1/2 per cycle. The first simulated two-minute cycle is warmup;
the last ten real minutes stop scripted interactions. Heavy rendering can make
simulation time shorter than wall time; both are logged. This is not a native UI
slider/mouse/graphics test. No current desktop app is controlled or interrupted.

`soak 2 smoke` executes three unpaced cycles (six simulated minutes) just to prove
the harness reaches the intended paths. It is not a two-minute or long-run test.
Faults, failed hits, missing snack-eating coverage or >1.5GiB private memory stop
the test with a failed completion record. Process PID/start ticks, assembly hashes,
ten-second process/GC/buffer/cache samples and sparse transition events are streamed
to disk. No frame/event history is retained in RAM; no forced GC or working-set trim.

`AnalyzeSoak.ps1 -Directory DIR` reads this evidence and reports first/last ten-minute,
20-minute and same-cycle-phase statistics and wall/monotonic sample gaps. It does
not automatically declare a leak or a pass. `GetGCMemoryInfo` sizes describe the last
natural GC, not current live memory. Process private bytes can include committed
free heap. Investigate persistent same-workload increases, not an isolated peak.
Sleep/long gaps invalidate continuous-run claims. Actual Mac native window/upload,
GPU retention and Apple Silicon CPU still need the Mac. No package/source push occurs.

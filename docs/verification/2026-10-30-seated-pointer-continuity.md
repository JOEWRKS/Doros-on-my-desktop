# Seated pointer reaction: intermittent standing frames — 2026-10-30

## Report and established cause

User reports standing flashes when approaching the seated pet with the mouse.
The running installed app was PID19480; it was not stopped, replaced, or given
automated input during this investigation.

The real Windows `PetLoop` + WPF presenter reproduces the same visible symptom
when desktop metadata acquisition temporarily fails while support remains valid.
`PetPlatformRuntime.SuspendsAutonomousMotion` combines unavailable scene data
with actual falling/landing/perching. `PetLoop` also passed that combined flag
to `SetLocomotionBlocked`. This suppressed the seated motion source (321), chose
neutral standing (-1), and canceled the local paw reaction, even though physics
remained `Supported`. A successful query restored sitting and restarted the paw.

Before: at each of 100/125/150% character sizes, two short query interruptions
produced16standing frames in a90frame sequence. Trace and actual presenter PNGs:
`artifacts/seated-pointer-continuity-20261030/before/`.
Simple pointer crossings with fixed screen coordinates and pixel-rounded window
readback passed4cases even before the change; approach alone was not reproduced
as the trigger. There is no telemetry from the user's original incident proving
which native query failed then. The reproduced defect is confirmed; attributing
every reported flash to that trigger would exceed the evidence.

## Correction

Added `BlocksLocomotionPresentation(sittingRequested)` and used it at the four
Windows loop presentation handoffs. Retained supported sitting no longer loses
its source solely because autonomous movement is suspended. Actual fall,
landing, lifting, missing geometry, and perch ownership keep their priority.
`SuspendsAutonomousMotion` and physics behavior are unchanged.

No art, raster geometry, reach angle, paw size, reaction timing, or cache changes.
Shared runtime has the new helper, but the separate Mac loop is unchanged;
no Mac package/update or nativeMac validation is included in this task.

## Evidence and failures retained

- `before.trx`:4pointer-crossing conditions passed before modification.
- `outage-before.trx`:3expected failures,16standing frames per size.
- `after.trx`:14passes;3failures in the new support-removal assertion. All90
  preceding seated frames per size were correct. Fixture initially placed the
  pet below the test window edge, so it stood on the monitor floor: deleting
  that window could not cause a fall. Separated that check, moved the window
  top to350, and explicitly assert window handle1 before removing support.
- First RID command rejected unsupported `dotnet test --self-contained` flag;
  replaced with `-p:SelfContained=true`. Next test compilation exposed a test-only
  nonexistent `PointD.Length`; replaced with component-distance calculation.
  Neither failed invocation counts as verification.
- Final `rid-final.trx`:21/21passed, self-contained win-x64 .NET8.0.31. Covers
  fixed screen-pointer crossings/readback rounding, seated taps/click shakes/gaze,
  background query handling, short interruptions,1second window-query expiry,
  world-position preservation, and confirmed supporting-window removal/fall.

Visual review: directly inspected all270chronological presenter frames
(90per size,20ms simulation step) on9contact sheets, including raise, extended
tap, lower, and repeated reaction around both interruptions. No standing source
appears in those corrected sequences. This is controlled runtime input plus
actual WPF raster output, not user mouse input or a recording of their desktop.

[Before/after and full frame review](../../artifacts/seated-pointer-continuity-20261030/frame-review.html).
Comparison image: `review/comparison.png`, top before / bottom after,
frames15,18,24,27 at125%.

## Delivery boundary

The current installed app and settings remain unchanged. No commit, push, public
release, installation, forced GC, heap/trace pause, or long memory soak performed.
Local portable candidate built and470archive files match runtime byte-for-byte;
ZIP CRC and exact tested App/Core DLL identity verified in
`artifacts/seated-pointer-continuity-20261030/package-verification.json`.

- Candidate: `artifacts/product-shell/candidate-seated-continuity-20261030/`
- ZIP: `Doros-on-my-desktop-Portable-0.1.3-win-x64.zip`,99,584,790bytes.
- ZIP SHA256: `f09b9f367807a4cdc9fc4e1644cf03dcddf5ed84c89aed07bb32bd66b423fce2`.
- Tested/published-local App DLL: `9b79d6326fddbdf98bd389b12dd1a3b6ab00876392da20c7ae5769a7974a28be`.
- Non-RID visual-export App DLL: `aa308262d3ff76f8400cdfc56d347a495e1bcb952ae078cc35bfc25368a4019d`.

Existing PID19480 remains responsive at the same installed path. New native
product launch, live replacement/user acceptance, and nativeMac remain unverified.
The existing published Setup does not contain this new correction.

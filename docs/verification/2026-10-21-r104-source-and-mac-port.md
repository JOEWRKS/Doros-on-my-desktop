# Accepted r104 source checkpoint and Apple Silicon port

## Scope

User requested pushing the current final source and a MacBook version, selected
Apple Silicon (M1 or later), then explicitly selected a full r104 port rather
than rebuilding the older Mac tester.

The accepted Windows behavior, artwork, image quality, speed and continuous
slider remain the reference. This request does not authorize replacing the
running Windows installation, changing accepted appearance, disabling macOS
security, or publishing a new binary release.

## Source identity

- Existing branch: `feature/macos-test03`.
- Origin: `https://github.com/JOEWRKS/Doros-on-my-desktop.git`.
- Previous committed checkpoint: `9e0b02f`.
- Windows Release `Dororong.App.dll` rebuilt from the current source matches the
  accepted r104 binary SHA256:
  `374666338E0AA082F84B49B8EAD1E4BF0397F0CD08658BC24F336F78F882B322`.
- Accepted run uses the high-resolution launch route (for example
  `--high-resolution-grab=1.5`); a fresh profile without that option or a saved
  size is not evidence of the accepted high-resolution configuration.
- Ignored runtime artifacts, heaps, ETL traces and personal runtime logs are not
  part of the source push.

## Windows source validation

Release Core suite: 285 passed. App suite: 1,534 passed, zero failed/skipped,
15 minutes 11 seconds. TRX evidence is retained locally at
`artifacts/final-source-20261021-r104/tests/{core,app}.trx`.
Mac packager unit suite: 11 passed on Windows.
These are source/build checks, not new native visual or memory acceptance.
Previously completed memory experiments are not restarted.

## Mac baseline mismatch

The existing `Dororong.Mac` / `Dororong.Portable` implementation is the earlier
144px renderer and test04 floor behavior. It does not consume the r104
high-resolution banks and does not implement the accepted continuous slider.
Repackaging that implementation is **not** completion of this request.

Required sequence:

1. Preserve and push the accepted Windows source checkpoint.
2. Reuse r104 source-resolution raster algorithms and immutable banks in the
   portable renderer; isolate WPF-specific raster/display services.
3. Port current pose/input orchestration, including the accepted release-center
   trajectory, precontact expansion, gaze, cheek, sitting/sleep and hanging paws.
4. Integrate the 100–150% continuous slider, fixed-capacity display buffers,
   scaled hit/foot geometry and proportional snack movement in the Mac host.
5. Compare reference pixels/geometry and state sequences against the fixed r104
   checkpoint; build and integrity-check a self-contained `osx-arm64` bundle.
6. Verify that exact bundle on an actual Apple Silicon Mac. Native window/Dock
   ordering, Retina input, permissions, launch and lifecycle are not validated
   by Windows compilation or headless tests.

No Mac release or complete r104 parity is claimed at this checkpoint.

## Windows push completed

Commit `3813312379fd7122f72a031f692237560b637ec3` was pushed to
`origin/feature/macos-test03`. A subsequent `git ls-remote` returned that exact
hash. No force push, release/tag replacement, installation or normal-app restart
was performed. New Mac work is separate from this accepted Windows checkpoint.

## First portable-renderer checkpoint (incomplete Mac port)

Added `Dororong.Raster` (`net8.0`), directly compiling the accepted sampler,
source banks, gaze/cheek/closed-line algorithms, body ink and bounded caches.
Only geometry and the WPF-only vector overlays use internal adapters. Windows
does not reference this assembly. `R104Surface` provides a fixed-capacity
Avalonia bitmap and active-rectangle alpha hit sampling, with product size and
display pixel density kept separate. **It is not yet connected to DesktopSession.**

Validation:

- 12 portable-raster tests passed on Windows: three fixed r104 pixel hashes,
  caller-owned buffer reuse, density/size separation, invalid input, geometry
  and non-stroked mouth-edge closure.
- Existing Portable baseline: 60 passed on Windows.
- Mac headless suite: 27 passed on Windows (24 existing + 3 surface tests).
  After the Origin invalidation setter change, the three surface tests were
  rebuilt and rerun successfully. These are not actual Mac tests.
- `tools/MacR104Parity`, exact accepted Windows DLL, 5 product sizes and 13 poses:
  all 45 non-vector samples byte-identical. All 20 vector-overlay samples still
  differ. Latest evidence: `artifacts/macos/r104-port-20261021/parity-03`.
  Changed channels per sample: flutter 372–756, neck 14–33, eating 140–300,
  drool 92–197. Maximum channel difference reaches 65 in eating. Stored WPF/
  portable eating and flutter PNG pairs were inspected; that is not native
  interaction acceptance. The comparison intentionally has a nonzero result
  for remaining vector differences rather than declaring all frames passed.
- The adapter initially closed a stroke subpath across a deliberately
  non-stroked mouth segment. Corrected closure to the original figure start;
  a regression test covers the extra-line failure. Remaining vector
  differences are not declared resolved.
- `dotnet publish` of the development Mac project for `osx-arm64`, self-contained
  .NET 8.0.32, succeeded with the new render module referenced. Apphost and
  `libcoreclr` are ARM64; Skia/Avalonia native libraries include ARM64 slices.
  Output is `artifacts/macos/r104-port-20261021/compile-only`, **not a deliverable
  r104 app bundle**: the normal application still uses its legacy controller.
  No native execution, ad-hoc signing, `.app` packaging or notarization for this
  new port has been completed.

User reports the Mac is for testing and does not know its OS version. Keep OS
version unknown; retain the existing macOS 13+ test prerequisite rather than
claiming this machine is supported.

Required next work remains vector-overlay equivalence review, the current
r104 state/input/landing coordinator, continuous-slider/persistence integration,
native host geometry, final packaging and actual Apple Silicon execution.
Do not substitute this compile-only output for the requested full port, and do
not ask the user to repeat the closed Windows memory investigation.

## Connected r104 Apple Silicon test candidate — 2026-10-21

This supersedes the foundation-only **implementation** status above, not the
native acceptance gate. The normal Mac DesktopSession now uses the r104
headless coordinator and high-resolution surface, not DesktopPetController's
legacy 144px renderer.

### Implementation and preservation boundary

- Windows App/Core source remains identical to `3813312` (empty targeted Git
  diff). The accepted running/release App DLL still has the SHA256 recorded
  above. No Windows application restart, installer, settings or artwork change.
- Mac-only PetLoop extraction removes the WPF production factory. Current
  direct-input controllers, platform/perch runtime, behavior clocks, hunting,
  pounce, locomotion and expression timing compile from unchanged source.
  High-resolution pose/recovery methods are extracted into the Mac raster
  assembly; native display, contact measurement and input capture are adapters.
- Normal desktop input connects head grab/swing/release, cheek capture/recoil,
  hanging paws, sitting/sleep, gaze, snacks and precontact landing. The seated
  cheek reflection keeps its high-resolution capture instead of falling back
  to a legacy capture. This adapter branch has a specific regression test,
  not a claim of new Windows behavior.
- The 100–150% slider queues the latest value for the next tick, preserves the
  world foot through PetLoop.TryResize, and saves the Mac-only size setting
  after 350ms inactivity (or shutdown). Busy interaction resize is rejected.
  The transparent host stays 408 points; the backing bitmap stays at maximum
  capacity for its display density. Product scale and Retina density stay
  separate. Density refresh happens before measuring the new extent.
- One retained active frame, bounded 24-entry sole cache and retained perch
  blend buffers avoid an unbounded per-size image cache. This is a code/storage
  property, **not measured native graphics-memory or latency acceptance**.

### Effects

Fixed hanging-paw texture coverage is imported without resampling from the
hash-checked accepted DLL. All 103 possible 60Hz eating overlay ages are likewise
exported as occupied rectangles (not full character frames). The runtime already
quantizes those ages to 60Hz, so this does not change its clock. Banks total
8,498 compressed bytes; no growing age cache or new artwork.

- Paw bank SHA256:
  `11E18B62F477CF33C32E91E357002A97C8B72281FB84119A1015C7847240E3B9`.
- Eating bank SHA256:
  `C30C4D02A484FF3C4989C17B656A385291D267F846A955CBC136ECDFAF7602CF`.
- Dynamic image sampling changed from Skia cubic to linear to match the WPF
  sampling route; the non-60Hz diagnostic eating sample maximum channel error
  fell from 65 to 28.
- `parity-06/summary.json`: 70 total samples, 55 byte-identical (45 original
  non-vector, 5 flutter, 5 runtime-60Hz eating). 15 samples still differ:
  neck 5 (14–33 channels, max22), drool 5 (92–197 channels, max21), and the old
  arbitrary 0.18-second eating probe 5 (79–193 channels, max28). That last age
  is not emitted by the 60Hz coordinator; it still exercises the fallback.
  The comparison deliberately returns nonzero. Dynamic neck/drool use unchanged
  paths and timings, but backend boundary coverage remains an open equality
  gate, not a passed visual equivalence result.
- Saved neck/drool WPF and portable PNG pairs were inspected. No native screen
  or complete motion sequence was visually inspected on a Mac.

### Executed checks

All these checks ran on Windows, not an Apple Silicon machine.

- Portable raster/runtime suite: **36 passed**, zero failed/skipped.
- Mac headless suite: **33 passed**, zero failed/skipped. Includes ordinary menu
  lifecycle, continuous value callbacks/rejection, 3-size foot anchoring, density
  transition, whole-loop head grab/swing/release and return to the floor.
- Same-DLL pose sequence comparison: **237 samples**, zero pose-field differences,
  across 100/125/150%, gaze, sitting, sleep phase wrap/wake and snack phases.
  This checks numeric pose values, not native display latency or all interactions.
- Packager suite: **11 passed** after updating its synthetic publish fixture to
  require Dororong.Raster.dll. The initial run with the old fixture failed before
  reaching its intended checks; that failure was corrected rather than ignored.
- TRX: `artifacts/macos/r104-port-20261021/tests/{raster-final,mac-final}.trx`.
  The three size-cycle cases were additionally rerun with both world-foot X and
  Y assertions: `tests/mac-foot-xy.trx`, 3 passed (not added as distinct tests).
  Pose evidence: `artifacts/macos/r104-port-20261021/runtime-parity-final.json`.
- Self-contained `osx-arm64` publish with .NET8.0.32 succeeded.
  Published Raster DLL matches the Windows-hosted test build byte-for-byte:
  `5F2A86713959D32EF2C71D4B4C8FA42F56FC046CE4D0DF7DA755F198017602B5`.
  Published Mac DLL:
  `535D127979E99F3696F3F016E80D7247968F72BF4586677F6593CFA7437F7A12`.
  Managed source checks do not execute the ARM64 native libraries.
  Final source cleanup after packaging removed trailing empty lines only; no
  executable statements changed after the recorded publish.

### Exact deliverable and remaining gates

Deliver **only**:
`artifacts/macos/r104-test01-20261021/candidate-final/Doro-Mac-arm64-r104-test01.zip`

- 99,162,683 bytes; SHA256
  `3e32340918f04486799f022caba20d9e91835d4193814c983aa83f0bc37536ba`.
- 235 files; 18 native code hash checks, resource seals, ZIP CRC/content and Unix
  executable permissions verified by the local packager.
- Contains `Doro Mac Test.app`, Korean/English limitations and license notices.
  Apple Silicon, macOS13+ test prerequisite; the user's actual OS remains unknown.
- Ad-hoc integrity signature only. No Developer ID/notarization. Not Apple's
  native codesign verification, Gatekeeper acceptance or a safety guarantee.
- Earlier `candidate` folder is an intermediate with an older English README;
  do not distribute it. No signed bundle was edited after signing.
- **NOT TESTED:** actual Apple Silicon launch/exit/security prompt, native Retina
  coordinates, window/Dock/Spaces ordering, all interactive sequences, frame
  pacing, and long-running native/graphics memory.
- Full r104 Mac parity is **not closed**. Remaining work is actual-Mac validation
  of this identified candidate and resolution/acceptance of dynamic edge coverage,
  not another repetition of the concluded Windows memory investigation.

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

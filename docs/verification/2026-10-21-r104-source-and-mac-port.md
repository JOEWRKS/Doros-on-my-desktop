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

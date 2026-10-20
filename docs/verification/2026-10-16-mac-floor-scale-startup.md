# Mac floor, scale and login-startup review — 2026-10-16

## Decision and implementation

[Discord's official overlay is Windows-only](https://support.discord.com/hc/en-us/articles/217659737-Game-Overlay-101).
The previous Mac review reproduced a generic synthetic occlusion path, not an
actual Mac Discord failure. Current Quartz metadata does not expose another
window's click-through state. No process-name guess was added.

The user instead explicitly chose **physical screen bottom**, in front of Dock
in **z-order**, not Dock-top height. This supersedes MAC-12 Dock support/hanging.
Ordinary window support, hanging and occlusion remain unchanged. This is an
approved Mac-only policy difference, not identical Windows behavior.

- MacSceneMapper no longer makes Dock a support. Dock still occludes ordinary
  window ledges behind it; the shared screen floor cannot be occluded.
- DesktopSession starts the sole at the physical screen bottom. Existing native
  window level `CGWindowLevelForKey(7)+1` remains in front of Dock. No other app
  mutation, new native API, permission or motion/rendering change.
- No Windows replacement, startup setting change, restart/logout, commit/push or
  remote upload. Scale/startup are diagnosis/proposal only.

## Verification and artifact

Portable60/60, Mac/headless24/24, packaging11/11 pass (95 total). Desktop18 are
included in60, not counted twice. Cases cover normal/elevated/menu occlusion,
Dock hide/side/removal, no Dock hanging cue, retained ordinary-window occlusion
and hanging/move/close. TRX names: `screen-floor-full-test04.trx` and
`screen-floor-host-test04.trx` in the corresponding test projects' TestResults.

Self-contained arm64 publish uses .NET8.0.32. Bundle/ZIP verification checks234
files,18 native code hashes, resource seals, CRC, inventory and Unix execute bits.
All16 Portable embedded resources equal test03 (`resource-parity.json`). Core
source is unchanged from test03 source commit20213a3; rebuilt Core hash matches
the current Windows build. This is not native Mac execution/visual acceptance.

Final artifact directory: `artifacts/macos/tester-20261016-arm64-04/`.

- `Doro-Mac-arm64-test04.zip`:85,449,266 bytes, bundle0.1.3.4.
- SHA256 `ab6ab1ba8ae3f1b55cce56f914120d223ff2380d9c497526af15d1b778cf2794`.
- `pre-share-simulation-cold-first.json`:5 groups PASS, no recorded warnings or
  failures: floor transitions, cold effects, meals30/60/120fps, seeded2400-frame
  desktop changes, coupled feeding→happy→settling→walking. Loaded Core/Portable
  hashes match the bundle. Windows CPU timings are not Mac performance evidence.
- The earlier `pre-share-simulation.json` warmed controller resources before
  effect timing. Preserved as functional evidence only; corrected harness runs
  cold effects first in a fresh process. No product repackaging was needed.
- Apple Silicon/macOS13+ test candidate. Ad-hoc integrity only; no Developer ID
  or notarization. Gatekeeper and native execution remain UNVERIFIED. Existing
  test03 ZIP and remote release are preserved.

Next: real Mac floor placement, Dock-front z-order and input, maximize/restore,
menus/overlays, Dock orientation/auto-hide, ordinary window hanging and Retina.
Spaces/fullscreen visibility also remains native-only. F4 is not complete.

## Enlargement: proposal only

Runtime standing/sleep/locomotion art is96×96; supplied hanging art100×100.
A225×225 canonical source exists, but is not a complete higher-resolution set of
every accepted retouch/pose. The384px hunting head working image is enlarged from
96px (`export-product-hunt.cjs`), not additional original detail. Current head
sampling reconstructs rotation back to96px.

Recommended low-work first step:1.25×/1.5× proof using one final premultiplied-alpha
sampling step from original source, composing pose/output scale where possible.
Do not repeatedly enlarge already rotated/downsampled frames. This can improve
consistency but cannot invent missing detail; outline/motion need visual review.

Exact integer2× nearest-neighbor is crisp but intentionally shows bigger square
pixels. Smooth2× or larger needs a consistent high-resolution part/motion set
based on approved art. Independent AI enlargement per frame risks face/ink drift.
Scale gutters, hit tests, grips/soles, movement bounds and mouth alignment together;
changing only window size is insufficient. Retina/fractional scaling needs tests.
Double size can approach4× pixel memory/work, relevant to startup cost too.

## Windows login-startup: findings, not a causal boot diagnosis

Installed App SHA256:
`3B398A68BDA8FB98D25552D1EAA43B7E64D9CC912C1AD2DA6B665C349E88BD2C`.
One Dororong process was observed, PID18724. HKCU Run has one quoted installed
executable command with no custom delay. It starts at login, not before sign-in.
[Microsoft Run-key guidance](https://learn.microsoft.com/en-us/windows/win32/setupapi/run-and-runonce-registry-keys)
notes Windows may itself defer these entries.

- Read-only30.464s sample:1.765625 CPU seconds /12 logical CPUs =0.48% total CPU
  (5.80% of one core); average265.02MiB working set,219.93MiB private bytes.
  Current session, not a controlled idle/boot benchmark; no restart loop observed.
- Exact installed DLL, three fresh-process CPU-side WPF/presenter/first-render
  probes:0.568/0.436/0.419s. No product window/native loop. Excludes process/.NET
  loader, cold disk, compositor/shell and login contention. Not full-ready times.
- OS boot15:27:32, process start15:29:00.57, Started log15:29:02.17. Time since boot
  is NOT attributable to Doro. Started precedes Loaded/PetLoop, so the1.6s interval
  is not full-ready time either.
- Boot Performance logs were inaccessible under current privileges. No elevation
  or security change attempted. No comparable startup ON/OFF trace exists here;
  the perceived slowdown is neither confirmed nor ruled out.
- Avoidable work: the first breathing head-mask access constructs HuntFrames,
  decoding157 body bitmaps (~39.25MiB raw bodies) as well as head data.
  LocomotionFrames similarly builds772 frames (~27.14MiB raw) on first access.
  These are optimization candidates, not proof of the boot symptom.

Proposed next implementation, requiring approval: auto-start-only15–30s delay
before heavy UI/assets, manual launch immediate; preserve single-instance and
registry/uninstaller ownership. Separately load required head/body frames lazily
without pixel changes. Compare matched ON/OFF login traces on future normal
restarts to measure causality. No startup code/settings changed. Diagnostic
scripts/results: ignored `artifacts/startup-scale-20261016/`.

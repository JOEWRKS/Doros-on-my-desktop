# Mac display-boundary investigation / test06 diagnostic — 2026-10-22

## Status and authority

User authorized investigating/fixing upper-screen drag restriction and clamshell
external-monitor window mounting, preserving accepted appearance/quality/motion/
speed and Windows. The tester's exact build, external display geometry, affected
window type and whether lid closure happened during execution are unknown; user
answered that they do not know. See the M2 feedback document for provenance.

**Not fixed / native cause not yet established.** No speculative native override,
larger movement limit, changed perch threshold, or removed occlusion was applied.
The local test06 ZIP is explicitly a diagnostic candidate, not a repair claim.
Mac AppKit placement and actual Quartz window rows cannot be observed from this
Windows host. The next required evidence is a bounded native boundary recording.

## Investigation and excluded host failures

- Quartz cursor/display/window data use the common desktop point coordinate space.
  Cocoa placement flips Y using the primary display height. Current overlay has
  96-point offset and transparent padding; requested position is cached without
  actual NSWindow frame readback. Whether AppKit constrains that padding is a
  hypothesis, not an established defect on the tester's Mac.
- R104 movement area is derived from the visible silhouette on the nearest
  monitor, not a fixed upper fifth. Actual controller head-grab tests at internal
  1470x956 reach visible top within20 points at size100/density1 and150/density2.
- Actual controller acquires a layer-zero ordinary window on an external-only
  1920x1080 scene at origins(0,0),(-1920,0),(1470,-1080). These are synthetic scene
  and code-driven input tests, NOT lid-close, external hardware or real mouse tests.
- Capture currently rejects a whole snapshot on malformed identity/layer/bounds/
  alpha, then SceneCache degrades. No captured native failure is available to
  justify changing that safety behavior or ignoring an unknown occluder.
- Reviewed Avalonia11.3.22 native AvnWindow/WindowImpl/WindowBaseImpl sources.
  SystemDecorations.None clears the titled style flag; no custom constrainFrameRect
  override was found. Apple documents automatic constraints for titled windows,
  so this alone does not prove the borderless overlay is being clamped.
  Sources: https://github.com/AvaloniaUI/Avalonia/tree/11.3.22/native/Avalonia.Native/src/OSX
  and https://developer.apple.com/documentation/appkit/nswindow/constrainframerect(_:to:)

## Diagnostic-only implementation

- Opt-in `--boundary-diagnostics`; normal launch does not create a boundary log.
- 90seconds maximum at0.5second cadence, queue8; async serialization/file writes.
  Dropped samples and early disposal are explicit. No heap/trace, forced GC,
  automatic interaction, app pause, process shutdown or permission changes.
- Reads actual owned NSWindow frame and Cocoa primary-screen frame; records them
  alongside requested Quartz placement, visible geometry, scale/density, cursor,
  capture state, current/scene monitors, freshness, available window boundaries and
  derived perch surfaces. Native capture failure categories replace silent nulls
  for diagnostic visibility; existing acceptance/rejection behavior is unchanged.
- Window geometry log limited to first512 entries with total count disclosed.
  No window titles, application names, screenshots, keyboard content or upload.
  Files stay in `~/Library/Application Support/JOEWRKS/Dororong/Logs/boundary-*.jsonl`.
- ZIP launcher `화면 경계 진단.command` opens this sibling .app with the flag.
  Existing singleton remains: tester must close old pet first, no forced kill.
  macOS Gatekeeper/launcher/native interop execution remain unverified.
- Existing test05 popup-parent fix retained. No rendering, simulation, input
  thresholds, image assets, Windows App/Core implementation or settings changed.

## Verification evidence

Folder: `artifacts/macos/boundary-20261022`.

- Scoped controller/diagnostic tests7/7; full Mac host57/57.
- Re-ran57/57 using exact original test04 Core/Portable/Raster dependencies.
- Package tests11/11 including diagnostic launcher execute bits; ARM64 publish
  succeeds; archive CRC, inventory, code/resource hashes verified.
- Host/ARM64 Mac assemblies491 method bodies and12,930,200 embedded-resource bytes
  compare identically. This is offline parity, NOT ARM64 execution.
- Original dependencies retained byte-for-byte:
  - Core B74C8AA1BDE1F963020934A695000061F417F05FCAFF79F7F6089928B791AC8E
  - Portable DEED3CD4CC4B6DB7F99B881101CB484E84CAAF4F8FB995E2CCF30DBDFB438887
  - Raster 86C7CBB20A1567CFBFDBB3B68E02AF2D1841FBDA818463C45B83A8F14B31D57F
- ZIP `candidate/Doro-Mac-arm64-r104-test06-diagnostic.zip`:99,188,202bytes,
  SHA2565C6803B7584A9ECE7CC7E36CB44546862903EC31FE17FA456F5CD845E18AC488.
  237files/18native seals, ad-hoc only; no Developer ID/notarization/public upload.

## Required next step (not another generic soak)

Run the launcher on the affected Mac; during90seconds try the upper drag and an
ordinary window's top edge, keep app open until the log has `duration-complete`.
If clamshell requires a separate run, do one in that configuration. Send only the
corresponding boundary JSONL and identify normal/clamshell. Requested vs actual
frame distinguishes native clamp/reference mismatch from model bounds; scene
freshness/capture status/surfaces distinguishes failed acquisition from occlusion.
Fix only the evidenced path, then retest those two interactions on the actual Mac.
Until then both reported issues and native repair validation remain OPEN.

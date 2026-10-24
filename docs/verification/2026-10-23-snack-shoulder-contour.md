# Third stamp shoulder/body contour — 2026-10-23

## User report and authority

User accepts the feel of the rigid-axis stamp but reports damaged body lines during it.
Screenshot `codex-clipboard-38b3112f-8598-408f-aa15-51f960093d50.png` is visual evidence.
Scope is contour repair with frame-by-frame verification, not a new motion. Axis, rotation,
deployment, timing, accepted face and second reaction remain unchanged. User answered go-ahead
to close the current pet and replace it after final checks.

## Cause and correction

- The old compositor erased an arm-shaped area and overlaid an independent diagonal
  collar line. That line crossed old belly ink or failed to cover the cut edge as the arm
  rotated. Sparse keyframe inspection did not adequately validate this seam.
- A complementary soft mask was composited source-over: two50%-coverage portions of
  opaque white gave75% alpha, causing a gray/translucent seam at deployment/recovery.
- Removed the separate repair stroke. Union the filled torso, arm and a curved shoulder
  bridge; reconstruct only the small collar's exterior ink, discarding interior cutout ink.
  The bridge extends across the source-mask edge rather than stopping at its vertical step.
- Partition coverage is now added/clamped rather than source-over. This preserves opaque
  interior when complementary pieces coincide. No original assets or facial paths changed.
- Changes are in shared `HighResolutionGrabRaster.Stomp.cs` plus focused render tests.
  No new cached texture/frame array. This uses local sampling, not a memory-soak result.
  Core motion code is unchanged from the accepted-feel candidate.

## Evidence and limits

Artifacts `artifacts/snack-shoulder-contour-20261023/`:

- `before/frames-00..06.png`:55frames of the previous candidate logic, at1/60s over0..0.9s.
  These preserve the defective diagonal/cut edge. `before.trx` is capture only, not a pass
  for appearance. Gray background makes alpha holes visible.
- Intermediate straight/small bridges removed the crossing but left a vertical step.
  Atx30→30.5 the measured contact edge jumped72.25→76.5DIP. Rejected in saved inspection.
- Final `after/frames-00..06.png`:all55frames opened and inspected, including preparation,
  both lifts/contacts and recovery, full character plus enlarged shoulder region.
  No old crossing stroke or translucent startup stripe observed in these saved frames.
- `after/contact-boundary.csv`:4x sampling of contact contour. Adjacent0.5DIP columns in
  repairedx25..34 have at most1DIP edge change; new regression checks both contacts.
- New5-size test exercises55frames each (275rendered frames) at100/112.5/125/137.5/150%:
  opaque white torso interior, unchanged rear-foot pixels and exact endpoint restoration.
  An initial ROI atY68 wrongly included the legitimate bobbing chin at impact;4cases failed.
  Moved ROI below the chin toY70..72; retained the same opacity/ink thresholds.
- Existing rigid pivot/radius, inverse, recovery, face and controller checks remain active.
- `raster.trx`:10/10,400frames retained/full equality, one output allocation and expression
  buffer reuse. `mac-host.trx`:5/5 Windows-hosted controller code inputs, not native Mac.
- Actual mouse/screen acceptance, native Mac and long-running memory remain unverified.
  No capture of the user's screen, automated input, pause/heap collection, install or settings
  changes. No new timing/CPU pass is inferred from saved-frame tests.

## Delivery

- `windows-verified.trx`:19/19, including all275frames and both contact-step regressions.
  Total targeted final checks34 (Windows19/Raster10/Mac-host5). Core was not rerun because
  its motion code/binary is unchanged; candidate Core hash matches the prior pivot build.
- All seven final55-frame sheets opened after the alpha correction, not only impact frames.
  `git diff --check` exit0. Tested/candidate DLL hashes matched.
- Candidate `artifacts/product-shell/candidate-snack-shoulder-contour-20261023/runtime/Dororong.exe`.
  No existing pet; authorized launch20:39:51KST, PID68476, alive/responding after3s with
  exact candidate path. No automatic input or process termination.
- App SHA256 `BC1F18C518E5BB4A8DFD452EF091423215C0B2E655D7A1531834D362BA54886F`
- Core SHA256 `B17665D8D68C1895BDBF8D8D290CC25A9C395DE8E84E5BE602D6DEC8E890C65F`
- ZIP SHA256 `9B8FA26DAE699AF91C963817320732B35191A1B82EE93FBAB96027BD3FDEF3C8`

User's actual new contour acceptance remains pending. Installation/settings untouched.

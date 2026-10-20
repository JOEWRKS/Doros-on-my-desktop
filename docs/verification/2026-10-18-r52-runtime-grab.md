# r52 runtime grab verification — 2026-10-18

## Boundary and result

User requested the remaining grab/release/dynamic-platform work and verification
as one batch, preserving original artwork. The installed app was closed by the
user. This is still an **opt-in Windows test path**, not a completed high-resolution
product migration. No installer, public release, commit, push or Mac change here.

Artifacts: `artifacts/runtime-grab-20261018-r52/`. Code and output hashes are in
`receipt.json`; source/bank identities and individual review images are in
`manifest.json`. The manifest generator deliberately retains `VISUAL_PENDING`;
the actual observation record is this document, not an automatically inferred pass.

## Problems, causes and changes

1. Regrabbing during `BodyDragSettle` was ignored: the previous direct controller
   retained ownership and discarded the queued press. PetLoop now cancels the old
   release at the displayed position before beginning the new interaction. Five
   scale fixtures first reproduced the position drift, then passed after the fix.
2. A new press reset the displayed tilted/partly recovered pose to neutral. Capture
   retains its progress and affine transform; entry interpolates from that display
   while pinning the grabbed point. Ordinary neutral entry is not blended needlessly.
   The old r51 entry had a small snap when its support was already swaying. This is
   why 39 final entry PNGs differ; they are not an unreported artwork replacement.
3. Legacy-head to new grab anchoring initially used integer alpha-hit coordinates.
   Three added tests failed with 0.5/0.625/0.75 DIP error. Hit classification still
   uses opaque pixels, but the grab anchor now keeps continuous image coordinates.
   The three tests pass on the final revision.
4. New-pose rendering repeatedly sampled large transparent gutters and constant
   interiors. Destination bounds, exact constant-footprint shortcuts, SIMD channel
   arithmetic and byte-exact integer sole translations reduce that work. Fractional
   corrections still sample immutable originals. No lower-quality sampling or
   resized intermediate character image was substituted.

## Automatic checks and failures retained

- `tests/all-app.trx`: 1,247 passed, zero failed/skipped on the earlier r52 revision.
  This precedes the final legacy anchor and capture-gate edits; it is not represented
  as a full-suite run on the final revision.
- `tests/final-targeted-v4.trx`: final affected regression set, **238 passed**,
  zero failed/skipped. Covers platform loop, host, anchor, landing, swing/recovery.
- `tests/final-all-scales-v4.trx`: final **18 passed**, zero failed/skipped.
  This overlaps the regression set and is not added to it as unique coverage.
- Five scales: 1, 1.1, 1.25, 1.35, 1.5. Synthetic scene/input drives the real
  PetLoop and Presenter through moving/removed supports, ordinary/strong falls,
  cancellation, partial release and regrab. Position ownership, actual alpha sole,
  capture/release balance and stationary-impact cache reuse are asserted.
- A transparent real HWND exercises the production viewport factory, both facings,
  two head grab points, angles -88/-45/0/45/88, release and same-origin regrab.
  This is layout/render automation, **not native mouse interaction validation**.
- Initial host tests used a transparent point as the second grab location. The
  fixture was corrected to an opaque head point, not the product hit map.
- `cancel.trx`, `final-targeted.trx`, `final-all-scales.trx` retain the genuine
  earlier failures. They are superseded by the named v4 runs, not deleted.
- First publish with `--no-restore -r win-x64` failed because the RID target was
  absent. Normal restore/publish succeeded. First launch used the wrong executable
  basename; the actual published file is `Dororong.App.exe` and was then launched.

## Direct visual ledger

The primary agent directly opened all **35 sheets / 237 distinct rendered images**
on white and dark backgrounds. These are saved samples, not every continuous-time
frame or a live-desktop recording.

- `host-sheet-{1.00,1.10,1.25,1.35,1.50}-{0,8,16}.png`: 15 sheets, 100 distinct
  images covering 140 slots. Final `host-final-v4` PNG hashes equal the directly
  reviewed `host-v3` images; they were not counted again as another observation.
- `loop-sheet-{1.00,1.25,1.50}-{0,8}.png`: six sheets, all 39 changed entry frames.
- `loop-sheet-{1.10,1.35}-{0,8,16,24,32,40,48}.png`: 14 sheets, 98 distinct
  images from the two newly added intermediate-scale runs.
- The final loop exports 480 slots; 249 of the original three-scale 288 slots are
  byte-identical to previously reviewed r51 outputs. The other 39 are listed above.
  Combined host/loop manifest has 620 slots; **620 directly viewed unique images
  is NOT claimed**.

Observed: saved entry/release shapes keep the approved face, pointed hair ends and
head/body attachment; both mirrored extreme-angle silhouettes fit the canvas.
No new open seam, duplicate ribbon or clipped hair tip was observed in these
samples. Strong landing still intentionally compresses/spreads then recovers.
Its thin paw stroke is light at small size; this is not a claim of identical line
contrast at every fractional scale. The original art and bank hashes are unchanged.

The review page's automatic sample stepping is explicitly not real-time playback:
saved frame gaps vary. Use it to inspect frames, not judge motion speed.

## Performance and actual app boundary

Final v4 fixture tick time (milliseconds, this machine, not isolated benchmark):

| Scale | Mean | Maximum |
| --- | ---: | ---: |
| 1 | 4.20 | 30.38 |
| 1.1 | 7.71 | 46.97 |
| 1.25 | 5.96 | 40.32 |
| 1.35 | 7.02 | 54.61 |
| 1.5 | 10.17 | 59.85 |

These improve the r51 measured means of about 8.0/11.6/19.2 ms at 1/1.25/1.5,
but remaining spikes exceed a 16.7 ms frame budget. **Smooth 60 fps is not passed.**

Final local candidate: `artifacts/runtime-grab-20261018-r52/app-final/Dororong.App.exe`,
launched with `--high-resolution-grab=1.5`, PID 59440. Process path and diagnostic
`Started` at `2026-10-17T17:33:37Z` were confirmed. This confirms startup only.
The supported Windows computer-use window inventory did not return the pet,
including a final retry after launch. No invented HWND or unrelated app was used
to bypass that limit. No direct desktop mouse drag, capture-loss or moving-window
visual pass is claimed. Candidate remains running for the user's inspection;
installed files and settings were not replaced.

## Remaining work and next

- Native desktop input/window-motion inspection is blocked by the available
  computer-use target inventory; needs a supported visible target or human check.
- Latency spikes remain. Further profiling/optimization must preserve output.
- The opt-in high-resolution input layer still handles upper-head capture only.
  Cheek/paw/body-click and other motion paths are not a completed high-resolution
  migration; legacy fallback can change displayed size/art. Do not make it default.
- Nonzero trailing deformation during regrab, all expressions and all cross-state
  boundaries are not proven by the zero-lag host fixture.
- Existing full migration, Windows/Mac integration, slider, installation and
  user acceptance gates remain open. Next is remaining runtime boundaries and
  native validation, not release or a declaration that all features are complete.

Skills used: joewrks-pixel-art (immutable source identity and explicit frame ledger),
computer-use (supported target discovery; its missing target prevents native claims).

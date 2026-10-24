# Third withdrawal: fixed-axis foreleg swing — 2026-10-23

## User decision

The extended/sheared foreleg in the prior candidate is rejected. Latest blue-axis drawing
requires a fixed shoulder pivot with the whole foreleg rotating up/down, not bending
through a displacement field. First reaction, accepted second small stamp and face ink
remain unchanged. Previous request authorized build then launch after closing the pet;
no pet process is currently present.

## Change

- Third foreleg is partitioned directly from immutable body pixels at sampling time.
  Its outstretched shape is prepared along its own longitudinal axis, preserving transverse
  width. Preparation finishes before the first peak; during both strokes the shape and
  length are constant. A single affine rigid rotation changes by40degrees for each lift.
- Source pivot(29,74) deploys to(30,69.3), then stays fixed through both strokes. Tip follows
  a circular arc, not separate x/y displacement. .28/.60s contacts and .9s reaction retained.
- Static body and arm are composited separately. A short antialiased shoulder collar closes
  the vacated seam behind the arm/head. No original image assets or facial paths changed.
- No arm texture cache/array is allocated: source is sampled directly, transforms are value
  types. Existing recovery amounts retract the pose; zero pose returns exact baseline.
- Shared Windows/raster implementation updated. Legacy96px preview does not gain this pose.

## Verification and rejected attempts

Artifacts: `artifacts/snack-pivot-stamp-20261023/`.

- Initial cutout/pivot left a visible shoulder outline gap in saved contact frames; rejected.
- A uniform enlarged paw also enlarged width and looked too bulky; replaced with longitudinal
  preparation. Final stroke uses only rigid rotation, not that preparation changing per beat.
- Initial numerical test compared floating-point Points exactly and failed at~7e-15DIP;
  pivot is now asserted to9decimal places, like the inverse/length tests. Not a visible drift.
- `core.trx`20/20: withdrawal rules, two beats, accepted second curve and third limits.
- `windows-verified.trx`12/12: fixed pivot/constant radius throughout BOTH strokes, inverse
  affine coordinates, second warp inverse, endpoint restore, interrupted recovery, mirrored
  rear soles, accepted faces, Windows presenter code inputs at3sizes. World position unchanged.
- `raster-verified.trx`10/10:400frames across sizes/densities, retained/full pixel equality,
  same expression buffer and one output allocation. Not a memory soak.
- `mac-host.trx`5/5: Mac controller on Windows with code inputs, NOT native Mac execution.
- Saved5-size sequence and enlarged second/third contact opened and inspected; GIF is a saved
  frame animation, not actual desktop recording. Native screen smoothness/impact feel and
  user's acceptance remain unverified. No automatic input or long-memory pass claimed.

## Delivery

Separate candidate `artifacts/product-shell/candidate-snack-pivot-stamp-20261023` packaged.
Tested/candidate DLL hashes matched, diff check exit0. No existing pet; launched20:22:57KST,
PID64812, same-path process alive/responding after3s. No process termination/automatic input.

- App SHA256 `FB0430ABE3B1D7BBD548D089ED52DC023052240D16235DAA0DC8495591EDB33A`
- Core SHA256 `B17665D8D68C1895BDBF8D8D290CC25A9C395DE8E84E5BE602D6DEC8E890C65F`
- ZIP SHA256 `7CA7300FF90586FA355F79346D0ACB0060204A457BCB2299ECE3DDFF446E2E54`

Installation/settings/public release unchanged. Actual motion acceptance pending.

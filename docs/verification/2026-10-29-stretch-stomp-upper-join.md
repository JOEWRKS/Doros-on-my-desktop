# Hair / upper shoulder ledge correction — 2026-10-29

The user clarified the defect with a red circle: the exposed outer shoulder just
below the leading hair, not the lower armpit join or an interior divider. The
previous v3 result did not resolve this specific ledge. Earlier artifacts remain
available and are not relabeled as accepted.

## Change

`HighResolutionGrabRaster.Stomp.cs` now tapers the proximal upper arm coverage
under the hair before rebuilding the union outline. Previously the rotating
straight cut cap protruded out from under the hair and made a short shelf. The
taper is in arm-local coordinates and fades with deployment. It ends before the
distal paw, and the existing transform, timing, head artwork and ink process stay
unchanged. Stretch uses the same compositor and receives the same local fix.

Only this source file was changed in this follow-up; prior uncommitted work is
retained. No live pet, settings, installed files or published packages changed.

## Current evidence

Artifact root: `artifacts/stretch-stomp-repair-20261029/upper-join/`.

- App SHA256 `D3434BCC7EFC6596C2DBB26916F03576D14526C2090E828B1DC557FB8366FB69`;
  exact copied probe identity recorded in `full/identity.json`.
- All 2,706 target frames rendered at60Hz, 100/125/150%, both facings. All18
  chronological enlarged junction contact sheets directly inspected, plus24
  whole-character poses and a same-time closeup. These are rendered sheets,
  not individually opened2,706 desktop screenshots. No hole/detached stroke
  observed at the reviewed junction during preparation, lift, hit and return.
- `change-scope.json`: 2,210 frames changed relative to v3; changes confined to
  proximal upper foreleg region. All282 accepted stamp transform/pose records exact.
- `../tests/upper-join.trx`:48/48 related tests passed, no skipped/failed tests.
- `parity-result.json`:4,848 Windows/Mac managed-render frames exactly match;
  3,636 unrelated-motion frames match accepted baseline. Mac compilation has
  the existing unused `_locomotionStarted` field warning; no new errors.
- `before-after.png`: the user's indicated junction at1.00s/150%, v3 versus
  current. `../frame-review-v4.html` offers all same-time frame comparisons.

## Limits / next

The shelf reduction is directly visible in the enlarged comparison. Visual
acceptance remains with the user. Live input, native Mac runtime, installer and
packaging verification were not done in this correction. No release, commit,
push or process replacement is included. Review v4 before promoting the candidate.

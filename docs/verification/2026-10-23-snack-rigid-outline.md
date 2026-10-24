# Third stamp: remove forepaw/ink elongation — 2026-10-23

## User authority / confirmed cause

Latest `codex-clipboard-9898c619-f8f5-43ed-bd75-eabc439b9190.png` rejects the elongated
arm outline. Previous shoulder candidate is not accepted. The earlier code applied
`I + 1.5*deploy*u*uᵀ`: at full deployment the longitudinal scale was2.5, transverse
scale1. The old test only compared the already-elongated arm between both strikes;
it did not compare against the original forepaw. Prior seam-only fixes missed this.

## Correction and scope

- Remove axial scaling, shear and pivot translation. Rotate original cutout about
  registered canonical forepaw shoulder(22,71). Preparation angle35°, swing40°;
  the timeline, .28/.60 contacts, .9s duration, accepted head bow/face and first two
  reactions remain unchanged. Third spatial trajectory is deliberately shorter,
  not claimed identical to the rejected elongated one.
- Remove the long Hermite connector to the neighbouring foot. Retain the shoulder
  plate and use a small stationary root cap; select only the leading forepaw,
  not neighbouring torso ink. Re-outline the local union at existing size-compensated
  width. Normalize overlapping premultiplied coverage instead of brightening ink.
- No original artwork, Core timing, installer/settings or public release edits.
  No new frame cache/buffer. Shared renderer carries the correction to Mac source,
  but native Mac is NOT_TESTED and no Mac package was produced this turn.

## Attempts and evidence

Artifacts: `artifacts/snack-rigid-outline-20261023/`.

- attempt1: removed stretching but old long connector filled the space between legs.
- attempts2/3: shorter connector still produced transient root alpha marks.
- attempt4: simple original shoulder/arm union removed those marks but left a small
  root notch. attempt5 adds the fixed root cap; saved final root has no long stretched
  connection. Earlier attempt captures are not appearance passes.
- attempt5's old x27..37 curvature check failed(.5 versus.25): x33..37 now belongs
  to the original neighbouring leg, not the deleted long connector. Kept the .25
  threshold on the actual root join x27..31 rather than smoothing original artwork
  to satisfy an obsolete connector assertion. Recorded failure is not concealed.
- New55-frame source-geometry test: unit basis lengths, zero shear, determinant1,
  original shoulder position and distances to five original outline landmarks.
  Covers preparation, both swings and return, not just constant-length swing samples.
- Five sizes ×55frames: distal visible forepaw stays within original shoulder-radius
  bound13.2DIP (includes sampling margin), torso opaque, rear pixels unchanged,
  exact endpoint restoration. Rejected2.5x extension is outside this bound.
- `windows-final.trx`:25/25,51s. Includes224 code-input actual-loop/display frames
  (100/150%, both facings) compared byte-for-byte to full render with actual state.
  Array comparison uses Span.SequenceEqual now; equality criterion is unchanged.
- All56 final150% canonical-facing runtime frames opened as seven correctly
  proportioned sheets. This is saved-frame inspection, not live desktop/mouse acceptance.
- `raster-final.trx`:10/10,400 full/retained frames, one owned frame allocation and
  expression-buffer reuse. `mac-host.trx`:5/5, Windows-hosted controller checks.
- Total focused final tests40; no long-memory/native-Mac/real-frame-timing claim.
- Core SHA256 unchanged: `B17665D8D68C1895BDBF8D8D290CC25A9C395DE8E84E5BE602D6DEC8E890C65F`.

## Delivery

- Candidate `artifacts/product-shell/candidate-snack-rigid-outline-20261023/runtime/Dororong.exe`.
- App SHA256 `299E7D8DB9D1867F3FBD15903A6042F2EF6921CB761F76C7F75D19FBDD72C291`.
- ZIP SHA256 `184521D4B75F8BB99FA2AA7B2DC88E65928E551FD975F22AA9AB516FCA41E533`.
- Tested/candidate App/Core hashes matched. No existing pet process; announced launch,
  PID68324 at21:19:25KST, exact-path alive/responding after3s. No automated inputs.
- Actual user acceptance remains pending; startup is not an animation acceptance result.

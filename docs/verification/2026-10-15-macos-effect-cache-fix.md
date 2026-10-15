# Mac test03 — bounded eating-effect decode

User authorized fixing the pre-share eating stall, preserving motion/quality,
rechecking and preparing a new shareable ZIP. No upload/commit/push or Windows
installation changes. This resolves the specific managed-code stall in the
[test02 investigation](2026-10-15-macos-pre-share-simulation.md), not Mac acceptance.

## Change and appearance preservation

- EFCT v2 whole-group decoding replaced by EFCT v3 independently compressed
 32-sample blocks, eight-block bounded cache and synchronized shared-cache access.
  Interpolation reads both adjacent blocks when needed. Timing, frame choice,
  rounding, source art and `SnackRenderer`/Core animation code are unchanged.
- `pack_effects.py` compared all284,190 original/reblocked patch records byte-for-byte.
  Combined raw-patch SHA256 before/after:
  `0396e8794069952e3f108f9bc66942a9da71909968a4748435fc771a4b1aff52`.
 9,158 blocks; bank7,751,196 bytes, SHA256
  `47b4ade0ed0aca8969282f2e6e9857385433d03b51da56c08566f50dca9414d4`.
  Original v2 bank and conversion report remain under `artifacts/macos/effect-cache-03/`.
- New runtime tests independently decode the records and compare3,858 rendered
  outputs across every group and all eating block boundaries, including fractional
  interpolation, final partial block and the .48s cutoff. Cache-miss allocation
  is bounded below500KB for a two-block request; shared concurrent reads agree.

## Checks and timing

- Portable57 tests: `effect-blocks-03.trx`; Mac headless24:
  `effect-cache-host-03.trx`; packaging11 unit tests pass.
- Windows-reference renderer suite: `effect-block-parity-03.trx` passes with
  unchanged thresholds;1,873 byte-exact frame comparisons and941 fine-sample
  comparisons remain within prior bounds. No new rendering approximation.
- arm64 self-contained publish succeeds. Final ZIP's staged DLLs match its verified
  bytes; the new diagnostic harness executed those DLLs on Windows. Four scenario
  groups pass, including2,400 mixed frames, meals at30/60/120fps and full controller
  meal→happy→settle→normal walking. `pre-share-simulation.json` beside test03 ZIP.
- Prior first/reloaded eating loads476–593ms /458–524ms. Final packaged-code run:
  first eating0.609ms, subsequent meal0.137ms; approximately117KB allocated per
  load instead of15.9MB. Warm eating0.245ms. These are this Windows host's CPU
  measurements, not Mac frame-rate guarantees.
- First publish trial recorded a324.7ms cold-chase warning while other tests/build
  activity overlapped; retained in `effect-cache-03/simulation-publish.json`.
  The final separate-process packaged-code run measured48.9ms cold chase and no
 100ms-threshold warnings. This does not prove startup is always hitch-free.
  Mixed-frame median0.137ms/p95 9.243ms/max80.228ms; full Mac UI/GPU remains untested.

## Handoff artifact

- `artifacts/macos/tester-20261015-arm64-03/Doro-Mac-arm64-test03.zip`
-85,449,044 bytes; SHA256
  `8ccd577446457a95802b119a345e6e74280f3b11d9597972be67e21632f5e2d0`.
- Bundle version0.1.3.3; Apple Silicon/macOS13+ tester; self-contained.NET8.0.32.
 234 files,18 native code hashes/resource seals, ZIP CRC/paths/bytes/executable
  modes pass the bounded integrity checker. Instructions updated to test03.
- Loaded Portable DLL SHA256
  `3F1E6510CE5E0D1612326953016DFF5670791ECB7678FD1DA2D2F69C5A178E3A`;
  Core remains `0759E47C2FAE5A268FF5915FE9DD5D15FF89C30BEDBBBE8E6124189535B0B992`.
- Old test01/test02 retained. test02 SHA remains
  `db9490d67043bb28cdc6093685a93530e79378272e099684a4abe5e7c8cc7db3`.
  Windows App/Core source diff is empty; no installed/public Windows artifact changed.

## Remaining gate

This is ready to hand to the consenting Apple Silicon tester, not a verified Mac
release. Native first launch, AppKit input/menu, Dock/Spaces/Retina, permissions,
login, update/removal and user visual acceptance remain open. Ad-hoc integrity
only: no Developer ID/notarization; Gatekeeper may block it. Keep normal OS
protection enabled. No actual Mac result is marked PASS.

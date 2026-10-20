# r86 — early contact sample and owned paw preparation

## Scope

User requests removal of the pre-expansion hitch while preserving accepted shape and unfolding speed, real-frame verification and memory growth correction. r85 only reduced sole work; it did not resolve the reported hitch. Do not reintroduce r83's phase-time jump or claim CPU captures are screen recordings.

## Changes

- `CompositionFrameTimer`: after a regular tick publishes an extreme Landing pose with zero spread, schedule one Render-priority dispatcher tick requested after4 ms. This tick uses the existing PetLoop stopwatch delta. It does not add physics time, reset the animation clock or advance to a minimum spread. A normal intervening composition frame cancels it; stopping, re-entry, changed ownership and stale/double callbacks are guarded. The supplemental tick never schedules another supplemental tick. Other states retain the existing composition cadence. Actual timer delay is OS-dependent and must be measured; requested4 ms is not a measured claim.
- `PetLoop`: wire the early tick only for the high-resolution zero-spread strong contact.
- `HighResolutionGrabRaster.PerchPaw`: replace per-preparation source clone with one owned810,000-byte float buffer; invalidate/reuse its stroke-compensated derivative. Copy unchanged source bytes before each new pose. Workers finish synchronously before reuse. Source bank, art, curve and settings remain unchanged.

## Automated evidence

Artifacts: `artifacts/render-cost-20261019-r86/`.

- Reproduced allocation failure before modification: `paw-allocation-before.trx` reports64,826,536B for40 paw/stroke preparations.
- After modification: same allocation check passes under65,536B; source immutability, reused derived buffer and restoring prior pose pixel contents are checked.
- Exported80 full-raster paw poses before changing the implementation, at1/1.1/1.25/1.35/1.5 scale. All80 post-change raw Pbgra32 hashes match `paw-before.json`.
- Initial targeted14 pass. Final `tests/final.trx`:74 pass,87 seconds. Includes one-shot/cancel/stale callback timer tests, first-contact age zero, owned-buffer/lifetime, live surface, sole and recorder integration. Do not sum overlapping runs.
- All420 general render samples match r85-before hashes. All335 deterministic Core+Presenter clip frame hashes and relative times (67 per scale) match r85. Those tests use explicit sample times; they do not prove the production dispatch timer is timely.
- Published/tested App DLL SHA256 `F00DA37C3F245F140324CEA66F8D70E5D8B7923E70BBAC072FB70465C1ADDC9D`. Core remains `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888` (r82), no art-bank modifications.

## Native run

User confirmed readiness; old process absence checked. Normal transparent1.5x r86 PID40340 started2026-10-19 17:37:11 KST.300-second `native.json` and `native-frames/`; start announced; immediate high drop then attached-paw pulls requested. No builds/tests/traces/dumps during recording.

Saved17:42:12; JSON SHA256 `775AEDBA155198F05210BCB1F5C418C87DB8841FF2905BF0786C3510F140FBC6`. Inputs8:Body5,PerchLeftPaw1,PerchRightPaw1,RightCheek1. Four extreme contacts; first expansion output gaps29.34–31.84ms, first spread .82–.84. First clip contact214401.1365ms, zero-spread frame016 then .82 at+29.65ms. Requested4ms timer did **not** produce an earlier recorded expansion. Do not infer why from the requested interval alone; this candidate did not meet the onset goal.

User clarified that the pet should expand **in air just before contact**, rather than land in its original pose then expand; explicitly answered “맞아. 닿기 직전 공중에서부터 퍼져야 해”. This authorizes pre-contact expansion, not a faster unfolding curve. r87 removes the ineffective supplemental timer, preserves the paw-memory fix and implements the newly explicit start condition.

After initial5s, tick max49.3837ms/>50ms0; callback max55.736ms/>50ms13. Managed peak217.88MiB/final217.88MiB, allocated1795.97MiB, gen2 four. Lower than r85's646.78MiB peak, but different input counts and a still-rising final segment prevent a long-term leak pass. The isolated same-work allocation regression is stronger evidence for the paw buffer fix than cross-run peak comparison.

## Limits and next

Timer candidate failed to meet the user's onset request and was removed, not retained as a successful optimization. Paw reuse remains verified by allocation/pixel tests and exercised in native input, but overall memory/long-term leak claims remain open. See r87 for pre-contact implementation and actual-frame verification. Native images are CPU bitmap recordings, not DWM or monitor recordings.

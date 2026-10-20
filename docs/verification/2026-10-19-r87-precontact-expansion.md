# r87 — unfold in air before contact

## User decision and scope

During r86 measurement the user identified the actual unwanted sequence: original upright pose reaches the floor, then expansion starts. Asked explicitly whether expansion should start while still airborne; user answered **“맞아. 닿기 직전 공중에서부터 퍼져야 해”**. This changes the start condition, not the accepted35ms unfolding curve, maximum shape, stroke/art, snack speed or grounded hold/recovery. r85/r86 post-contact cost/cadence work was not a completed fix for that requirement.

## Implementation

- Remove r86's ineffective supplemental dispatcher timer and restore original composition scheduling. Keep its verified owned paw/stroke buffers.
- `PlatformMotionInput.AnticipateExtremeLanding` is opt-in for compatibility; live `PetPlatformRuntime` enables it. Core default callers keep their former collision-start contract.
- After each falling step, predict only the next35ms using the same gravity1800, velocity cap1200,8ms subdivisions and first-crossing geometry as real motion. Use current surfaces and horizontal travel. Only a predicted drop meeting the existing80%-of-monitor strong-impact threshold receives the anticipation pose; do not acquire support in air.
- Evaluate the original expansion formula at `35ms - timeToContact`, applying the existing squash maximum. Actual position continues falling. Surface disappearance is re-evaluated; no predicted support is retained as a physical owner.
- When the same predicted surface is actually contacted, continue expansion at its peak instead of publishing another upright frame. The normal contact clock remains zero, so grounded hold, hop and recovery timing stay unchanged. Unexpected contacts without a preceding prediction retain the original fallback; a long blocked update cannot manufacture intermediate frames.
- Initial candidate advanced the entire landing clock35ms and failed two held-duration tests. Fixed before native r87 execution by separating expansion age from the existing grounded clock. Preserve these failure records, not just final passes.

## Verification

Artifacts `artifacts/render-cost-20261019-r87/`.

- `tests/core-final.trx`:285 pass. New1/4/8/16ms sampling checks verify original35ms curve in air, no airborne support, peak at contact and unchanged recoil curve; short drop and removed-surface cases included. Existing fallback/r83 no-skipped-start guards remain.
- First broad app run `regression.trx`:596 pass /2 fail (grounded hold shifted35ms), fixed as described above.
- `tests/final.trx`:598 pass,108 seconds. Runtime, platform, perch and raster regression coverage. Actual PetLoop+Presenter test now explicitly asserts airborne nonzero spread and first grounded spread1 at all five scales, plus supported recovery.
- 420 general raw render hashes unchanged versus r85 baseline;80 paw poses unchanged versus pre-buffer r85 baseline. Existing shapes are sampled earlier, not redrawn or resampled at lower quality.
- Synthetic final clips:67 frames each at1/1.1/1.25/1.35/1.5. Before final hold fix, directly opened seven representative frames spanning all sizes and1.5 pre-contact→contact. Final synthetic assertions verify pre-contact Falling samples; these are not native user-input images.
- App published/tested SHA256 `4CCB66AA00103DB57187597D454110882D3E9761642435AFECC21A6F252ED3DE`; Core now intentionally changed to `D08C0695F289CCA41C4137651549005FF34809E5D09D44605A6FF8379076C14D`. Do not describe Core as unchanged. Art sources untouched. r86 paw allocation proof remains40 preparations64,826,536B before, below65,536B after.

## Native r87

User ready, r86 absence checked. Normal transparent1.5x PID97856 started2026-10-19 17:52:16 KST;300-second `native.json` and `native-frames/`. Start announced; high drop and attached-paw pulls requested. No builds/tests/traces/dumps during recording.

- Completed 300005.1011ms; JSON SHA256 `37E5876A10C075E03629D4FE505755A87C2C30344BCD040699DFE2F464278710`. Five inputs: four Body, one RightCheek. No attached-paw input in this run; do not claim native paw coverage here (r86 had both paws).
- Four strong physical contacts: each first Landing tick rendered spread1, preceded by Falling spread0.94676/0.99856/0.999999/0.99635. This is actual user-input runtime evidence, not just synthetic tests.
- First clip `native-frames/landing-20261019-085217-9f63ededbfa84b6e9cd517d2b1e2c7e9/frames.json`: triggered, no error, post-impact-window. Directly opened PNG013/014/015/016/018: upright at-61.97ms; airborne partial spread at-33.32ms (~0.49) and-16.29ms (~0.95); contact0ms spread1; recoil+34.38ms ~0.94. These timestamps are CPU output times; not exact DWM presentation times or the mathematical35ms curve duration.
- User explicitly reported **“일단 동작은 통과. 자연스럽다”**. Accept this landing motion/feel, preserving current shape and speed. This does not certify every motion, size, environment or memory stability, and is not agent OS-screen inspection.
- Excluding startup5s: tick interval p99 29.0947ms/max46.729ms/over50ms0; update work max24.9783ms. Rendering callback interval max54.254ms/over50ms2. Thus residual cadence delays remain; no all-lag-fixed claim.
- Managed peak209.4878MiB, near299s171.3806MiB; total allocations1781.24MiB, gen2 collections5. Managed rose134.60MiB at30s→204.09MiB at270s then partly reclaimed. Private near299s290.61MiB. Diagnostic recorder itself retains54MiB during capture. Input/state workloads differ from r85/r86, so this is not a controlled percentage improvement or long-term leak proof. r86's isolated paw allocation reduction remains verified; other retention/allocation remains open.
- Native recording ended about17:57:17; r87 process remained running at handoff. No installation/settings/art changes or further motion tuning after acceptance.

## Limits

CPU bitmap output and requested poses are not DWM/monitor presentation evidence. Other native sizes/monitors, unusual moving/removing supports during the35ms anticipation window, long stalled ticks and long-term memory retention need separate acceptance. Do not call the whole motion/performance/memory scope complete from these checks.

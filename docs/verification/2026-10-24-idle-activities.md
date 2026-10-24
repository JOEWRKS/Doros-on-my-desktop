# Wake stretch / post-meal begging / grooming — bounded verification

## Authority and scope

User selected suggestions3/5/6, then confirmed normal exit for replacement with
“00”. [Conditions](../specs/2026-10-24-idle-activities.md). Windows opt-in only;
accepted artwork banks, question timing/position, anger/stomp/snatch/chuckle,
installation and saved preferences are preserved. No commit/push requested.

## Implementation and discovered correction

- Core schedules activities without resetting the inactivity clock. Sleep and
  minute-question deadlines outrank grooming; direct/snack input cancels ambient
  work. Completed meal pulse is separate from cancellation. Wake stretch waits
  for initiating input and retains the existing180ms wake blend.
- Presenter uses original source-resolution pixels. Grooming reuses the accepted
  rounded paw with a separate shoulder-pivot sweep; its default-zero fields leave
  the accepted chuckle unchanged. No new frame-by-age cache or artwork bank.
- Initial stretch fixed-point inverse was inadequate where horizontal paw reach
  and vertical rump lift interact. Visual sheets showed repeated dark contour
  samples under the front body. Numeric reproduction of that old12-step inverse
  found residual20.9437 authored units at amount1,target(44.75,73.75).
- Replaced that new-motion-only inverse with a coupled two-axis Newton solve.
  Five amount grids/95,475 target samples check forward residual<=1e-6 and positive
  local determinant>.4. Final stretch rise/hold/return/shake sheets were visually
  inspected; the observed dark bunching is absent. Existing motions do not enter
  this deformation path.
- Initial raster-only sole assertion was overly strict for raising the lowest
  paw. Local raster sole can change; the production host anchoring is the relevant
  world-space check. At100/125/150% the integration tests keep sole on the support
  within1.1DIP throughout all three motions, without changing platform code.
- An initial integration-test final snack assertion used an unavailable pointer;
  the session correctly canceled it. Corrected the test input, not the product.

## Completed checks

Artifacts root: `artifacts/idle-activities-20261024/`.

- Final Core339/339: `core-final.trx`. Includes new8 scheduling/completion/cancel/
  suspension/priority tests, plus existing behavior regressions.
- Final Windows Release win-x64 related72/72: `final-rid/*.trx`.
  Includes all new renderer/storage/warp/host tests and accepted question,
  stomp and chuckle tests. Exact tested binaries match packaged App/Core.
- Renderer3,258 generated frames: stretch169, beg145, groom229 ×3sizes ×2facings;
  `frames-final/`102contact sheets. Automated viewport alpha bounds, nonempty
  image and exact neutral endpoints/return checked. Visual review covered the
  complete100% source-facing sequences plus selected mirrored125/150% transition
  sheets. This is generated WPF presentation, not live desktop/manual input or
  an assertion that every larger-scale frame was individually visually inspected.
- Production PetLoop integration: real WPF presenter plus synthetic scene/time/
  cursor, all three sizes; grooming→meal/beg→sleep/wake/stretch→snack ownership.
  Physical user mouse actions and native desktop compositor were not simulated
  into a claim of actual user acceptance.
- Storage test:2,088 renders across8cycles ×3sizes ×3motions ×29samples. After
  warm-up the same frame arrays are reused; active+spares<=12, motion bodies<=64
  and the warmed body-array identity set remains unchanged. No forcedGC/trim.
  This proves bounded inspected buffers, not a long-duration total-private/GPU
  memory pass. No new long soak was requested or claimed.
- Accepted package versus candidate: `accepted-motion-hashes.json` and
  `candidate-motion-hashes.json`,396 existing-motion raster hashes and local sole
  positions, zero differences. Covers idle/question, eating/happy/chuckle and
  anger streaks1–5 at100/125/150% both directions plus sleep samples.
  `tools/IdleActivityProbe` ran in separate processes against each assembly set.
- Shared Raster project compiled successfully. Its existing unused-field warning
  remains. This is not a nativeMac build/runtime/visual acceptance claim.

## Partial/obsolete runs, not counted as final passes

- `render-initial/`:6failed local-sole assertions, before corrected host criterion.
- `render-v2/`:6renderer passes,3test-input failures, before final stretch inverse.
- `app-regression/`: broad non-RID initial-build run was deliberately stopped
  after final RID checks completed, before launching the candidate, to avoid
  leaving CPU-intensive obsolete tests beside user review. Only this task's
  validated testhost PID50340/path/parent21864 was stopped, never a pet process.
  It reports690completed passes and aborted overall; **not a complete-suite pass**.

## Candidate and launch

Fresh package: `artifacts/product-shell/candidate-idle-activities-20261024/`.

- App SHA256 `5B2A091B6843D1FC8B22C25028761BA1CBFC7B23D16F675B007080BDDCE6264B`
- Core SHA256 `E27CDF84D7C68C099A3373A27DA7644A97502D3CF3D449F3FB4E25A7AF34180C`
- Portable ZIP SHA256 `5024EE3C93C642DEC22C41C622505167A8A3057F8A4E969249932FBCBDF3CB14`

User tray-exited prior PID38372; no existing Dororong was found before launch.
Announced launch then started exact candidate PID52280 at
`2026-10-24T11:53:10.3267280Z` (20:53:10KST).
Stored SizePercent150 / AutomaticSleepEnabled0 unchanged.

## Remaining acceptance

Subsequent user decision: keep the current version and commit/push; decline the
separate0.70s grooming-sit preview. Existing0.40s entry remains. This closes the
preference decision below for source delivery, but adds no measured physical-input,
nativeMac or long-soak evidence. Running App/Core hashes were rechecked unchanged.

User visual/timing preference for the three new motions is pending. No nativeMac
implementation/verification or full-system soak claimed. No installer/public
release, commit/push, automatic input, app termination or power action performed.
For manual review: leave unoccupied for first grooming ~45seconds; eat a jewel
and wait for celebration to finish for begging (first/alternate meals); enable
automatic sleep if desired, let it sleep at3minutes and wake it for stretching.
Do not silently alter the user's sleepOFF preference to force a preview.

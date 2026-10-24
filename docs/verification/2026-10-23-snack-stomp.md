# Three-withdrawal forepaw stamp — 2026-10-23

## Authority and preserved baseline

User accepted the clean-wrinkles candidate's face with "좋다. 잘했어" and requested that
the third reaction stamp one forepaw on the ground, in place, instead of shaking its head.
This accepts the earlier face, not this newly implemented movement. Existing PID57304
was confirmed on the accepted candidate and was not stopped by the agent.

## Implementation

- `SnackAngerMotion.Roll(age,streak)` retains the old curve for first/second withdrawals,
  returns zero for third/capped later withdrawals. Existing streak/reset policy unchanged.
- `PawLift`: two5DIP lifts, .04→.19→.28s and .36→.51→.60s. Slower lift, faster downstroke,
  then planted hold to existing .9s reaction end. No world translation/physics, audio,
  facial redraw, asset, meal timing or chase-speed changes.
- Both high-resolution presenters pass `StompLift` into the shared face pose. Continuous
  inverse source lookup moves only the near foreleg x31..52/y69..96, preserving shoulder
  attachment, distal paw pixels and all other areas. No copied rectangle, no replacement art.
  The monotone field has derivative>=.464, avoiding overlap/foldover. Endpoint0 bypasses it.
- No new frame/source/cache arrays. Existing single-sampling AA and output scratch reused.
  Mac dirty-region key includes the new record field automatically and takes full redraw
  on changed lift; retained-vs-full pixel comparison confirms no stale paw remnants.
- Captured-pose recovery reduces lift to zero rather than retaining a raised paw until exit.
- Legacy96px preview stops its third head shake but does not receive the new high-resolution
  leg warp; this is not claimed as legacy-render parity. Product high-resolution paths use it.

## Evidence

Under `artifacts/snack-stomp-20261023/`:

- `core.trx`19/19: .5s withdrawal path, streak/reset, two lift intervals and planted endpoints,
  zero third roll, unchanged first/second roll and no first/second paw lift.
- `windows-final.trx`9/9: accepted expression checks; five sizes (100/112.5/125/137.5/150%),
  exact pixels outside one foreleg, exact contact frame restoration, both mirrored facings,
  captured-pose lift recovery, real presenter/loop driven by code inputs at100/125/150%.
  Loop verifies position does not move, third head matrix stays identity, paw rises above4.5,
  first/second paw remains0 and reaction exit restores0. This is not actual mouse testing.
- `mac-host-final.trx`5/5: same code-input controller at100/125/150%, density1/2 combinations;
  stationary position/head, paw/exit/reset checks. Windows host, NOT native macOS execution.
- `raster.trx`10/10: five sizes×two densities,400 reaction frames. Retained/full exact pixels,
  neutral exact return and single output/expression buffer reuse. Not a long memory soak.
- `proof/stomp-five-sizes.png` opened and inspected for attached leg/clean contour and contact
  recovery. Columns:0/.12/.19/.24/.28/.51/.60s; rows:five sizes. Only one leg visibly lifts.
- `proof/stomp.gif`:36 saved frames at25ms average interval, one.9s reaction; rendered preview,
  not screen capture. No claim of actual playback smoothness on the user's desktop.

## Delivery and remaining boundary

Separate local Windows package:
`artifacts/product-shell/candidate-snack-stomp-20261023/runtime/Dororong.exe`.
No install/settings/public upload/commit/push or current-app interruption.
User has been asked to close the existing app before launching the new candidate.
Actual mouse and user motion acceptance/native Mac execution remain NOT_TESTED.
No new Mac ZIP or unsupported claim of long-run memory/CPU improvement.

Packaging complete; candidate App/Core DLL hashes equal tested DLLs:

- App `708570910EAB95C35C95C03A08C1EA498F29D218001C2EF04E48C058E4432F4D`
- Core `CE74377D2B8A345EA44383CF8426255B5F43CC4D7B7EA1BF27621FB7F44F94A9`
- ZIP `1772182A5AB88E4757A3D8D12482DE7A7CF966808E38707A605E99556901A20E`

Final `git diff --check` success. Existing PID57304 still running at handoff;
new candidate has not been launched or accepted yet.

## Authorized launch follow-up

User answered go-ahead to the close-and-launch prompt. No Dororong process remained.
App/Core hashes rechecked against above. Launched this candidate at2026-10-23 19:44:09 KST,
PID5316, exact executable path and Responding=True confirmed after3s. No automatic inputs,
termination, installation or settings edits. Startup is confirmed, actual stamp appearance
and user acceptance remain pending.

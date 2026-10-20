# r65 fixed eye rim — candidate, actual interaction acceptance open

**Superseded/rejected:** user reported mouth residue and a duplicate lower-eye
line after this launch. The inspections below missed those defects. See
[r66 repair and limits](2026-10-18-r66-mouth-lower-eye-line.md); r65 is not accepted.

## User feedback and cause

After the r64 cheek fix was launched, the user showed a missing eye rim during
tracking (`codex-clipboard-f23b54d0-b512-44bf-b775-4bbd3c770e91.png`). r64 still
removed and transported the whole eye contrast, including the ink rim. Its
stationary hair clip could cut the translated outline. Source immutability and
interior texture tests did not prove rim preservation.

## Change

The original eyelid/outer eye outline now stays at its original source location.
Only the purple interior and highlight use a rigid texture displacement. The
source selection is annotated inside the original rim, with source-edge colour
continuation beneath that rim. It does not clear the eye to white skin, warp the
iris geometry by position as r60 did, blur the eye or redraw/thicken the outline.
Matching old/new interior samples make the zero-motion limit continuous.

The r64 cheek ownership repair, skin-donor exclusion, contour coverage and facing
handoff remain. Near-eye placement during a cheek pull is fixed with captured
gaze; mouth/far-eye/hair-follow retain their prior reactions. The now-obsolete
shifted whole-eye ownership mask was removed from cheek selection.

Body/leg code and original banks remain untouched. The user's latest hind-leg
scope is inspection only. The original standing 225px/96px sources show two
front feet and one hind foot; current walking adds the otherwise hidden far hind
part for nonzero gait amount. See the r64 report for exact sources and code.

## Evidence boundaries

Artifacts: `artifacts/gaze-rim-20261018-r65/` (trial1 and final are separate).
Trial1's focused old suite passed 21 tests and its extrema/down comparison was
directly viewed. New final tests additionally compare independent source rim
landmarks at all 8 directions, 5 sizes. Final results recorded after completion.
Native OS input is not inferred from automated tests or saved render sheets.
The transparent pet has not exposed a targetable computer-use window in this
turn. User feedback remains authoritative; no overall motion/performance,
installation or release completion is claimed.

## Final candidate evidence and handoff

- Release related suite: 85 passed, 0 failed, 0 skipped (1m54s),
  `tests/Dororong.App.Tests/TestResults/r65-related.trx`.
- Directly viewed final `eyes-{1.00,1.10,1.25,1.35,1.50}.png`:
  8 directions x 9 steps per size. Original rim stays visible in these saved
  samples while interior colour/highlights move. Also viewed final
  `cheek-comparison.png` (9 combinations), `down-before-after.png`, and all ten
  `interaction-{size}-{Right,Left}.png` sheets (440 saved samples). No large
  eye-adjacent wedge or missing rim was observed in those samples. This is a
  saved-sample inspection, not proof of every possible continuous input state.
- The interaction harness still has the previously documented body switch at
  local cell 37 because it does not update hunting during that section. These
  sheets do NOT certify body transition timing or live animation smoothness.
- `final/preservation.json`: all 5 locked source files unchanged; 25 reference
  comparisons, zero changes outside the defined local cheek/near-eye repair
  regions. Neutral/rebound references remain identical. Source immutability is
  not used as a substitute for runtime composition verification.
- After the user's shutdown response, no Dororong process remained. Launched
  `artifacts/gaze-rim-20261018-r65/release-app/Dororong.App.exe` normally transparent
  at 1.5x with performance capture. Verified single PID 114492, expected path,
  Responding=True. Installer and settings were not changed.
- Computer-use window enumeration again did not expose Dororong. No actual
  OS-pointer tracking/pull/release was performed by the agent. Native visual
  acceptance, full motion/performance verification remain OPEN. Callback logs
  are not displayed-FPS evidence. Next is actual tracking then cheek pull/release
  feedback on this candidate, without changing the accepted source art/legs.

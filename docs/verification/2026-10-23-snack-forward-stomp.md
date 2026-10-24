# Forward-reaching third-withdrawal stamp — 2026-10-23

## Latest user authority

User rejected the vertical-only paw lift: instead extend one forepaw forward first, then
bow slightly (like pre-pounce but smaller) while hitting forward/down twice. Previous face
acceptance remains valid; the vertical-only stomp was not accepted. No new facial ink,
asset, first/second reaction, .5s offer/.9s reaction, chase speed or physics changes requested.

## Implemented sequence

- Near forepaw reaches3.2DIP by.18s, advances another.8DIP on each hit. It stays forward
  between hits, then returns during.66–.9s. Lift reduced5→2.4DIP.
- Hits at.28/.60s synchronize with crouch peaks. Base crouch12%, peak34% of the pre-pounce
  head bob/roll. Same head pivot(43,48), bob6.3×amount and roll−.07×amount, plus a small
  forward translation−2×amount. This is not a jump or sideways head shake.
- Continuous front-body bend, with stationary other sole anchors, follows that head motion;
  only selected near forepaw gets additional forward/lift displacement. Back torso/feet are
  outside the bend. Original artwork is sampled once, no rectangular patch or added frame cache.
- Shared `SnackReaction` creates matching Windows/Mac high-resolution poses. All three
  new pose amounts decay during captured-pose recovery. Existing legacy96px preview does
  not gain this high-resolution warp; no legacy/native Mac parity is claimed.
- Scalar bracketed inversion avoids accumulated coordinate error in the paw transition.
  Body bounds expanded conservatively during this pose. Default/nonreaction path unchanged.

## Evidence / fixes during verification

Directory `artifacts/snack-forward-stomp-20261023/`:

- Initial `windows.trx`8/10: front-bend influence reached a rear sole; narrowed its fade
  endpoint fromx60 tox54. This fixed both rear-anchor and mirrored-pixel checks.
- `windows-final.trx` and `windows-verified.trx`9/10: fixed-point inverse convergence missed
  the1e-5DIP numerical bound (including0.000674DIP). Replaced the paw inverse with bounded
  scalar bisection, increased the small bow inverse bound; did not weaken the tolerance.
  A test-only namespace qualification compile error was corrected before final execution.
- `core-final.trx`19/19: two lifts, forward preparation before both hits, small synchronized
  crouch, planted endpoints, unchanged first/second reactions and withdrawal/reset behavior.
- `windows-final2.trx`10/10: numerical inverse grid / other sole anchors, both facings,
  rear-foot pixels, 5sizes exact endpoint restoration, captured-pose recovery, accepted face
  checks, real presenter/loop with code inputs at100/125/150%. Body world position unchanged;
  loop reaches expected lift/reach/crouch and uses the matching crouched head matrix.
- `raster-final.trx`10/10:5sizes×2densities,400 reaction frames, retained/full exact pixels,
  neutral return and bounded output/expression buffer reuse. Not a memory soak.
- `mac-host-final.trx`5/5: Windows-hosted Mac controller code-input checks, not a native Mac run.
- Opened final `proof/stomp-five-sizes.png`: columns0/.12/.19/.28/.36/.51/.60/.76/.9s,
  rows100/112.5/125/137.5/150%; inspected forward paw, small bow, attachment and final return.
  `proof/stomp.gif` contains37 saved frames, not a desktop recording; actual perceived impact
  and smoothness still require user acceptance.

## Delivery / remaining

Local Windows candidate:
`artifacts/product-shell/candidate-snack-forward-stomp-20261023/runtime/Dororong.exe`.
Prior process5316 was absent at last read-only check. No process was killed or launched;
asked user whether ready for launch. Installation/settings/public release untouched.
Actual mouse, user motion acceptance/native Mac and long memory remain NOT_TESTED.

Packaging complete; tested/candidate DLL hashes matched. `git diff --check` successful.

- App SHA256 `F720B5F40CA936816081A5904D7069E787C52449BDAC060B8A2A9D6AF84D451E`
- Core SHA256 `CD84BEEB00FD5D1471C234A5BD997CB7AECFAB4C583C22CD9BBF4419101E4E24`
- ZIP SHA256 `727E92F6BCB5F3E0D09DE78460631B678E924CCB668228C91F546A18F1FC2675`

## Authorized launch

User replied go-ahead. No existing Dororong process; App/Core hashes reverified as above.
Launched candidate at2026-10-23 19:59:34 KST, PID61928. Exact path and Responding=True
confirmed after3s. No automatic input, process termination, install/settings edits.
Startup confirmed; actual motion appearance and user acceptance remain pending.

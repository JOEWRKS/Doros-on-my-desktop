# r67 — whole-eye tracking, not a fixed eye outline

## User rejection and cause

The user clarified that the iris colour moves vertically and head/body respond,
but the eye itself does not follow direction. r65/r66 deliberately kept the eye
outline stationary. This was the wrong product behavior, not lost mouse input.
The test `Tracking_preserves_original_eyelids_and_outer_rim` encoded that mistake;
passing it did not establish the requested tracking. r66 is rejected for gaze
behavior. Its independent mouth-residue repair remains in place.

## Change boundary

- Use original whole-eye contrast, including lower/side outline, and translate
  it by the existing gaze offsets (.85/.65 DIP maximum). Remove original-position
  contrast before adding the translated feature. Do not redraw outlines, amplify
  input, change source art, or widen strokes. Keep original bangs as foreground.
- Replace the fixed-colour continuation cache with whole-eye selection; remove
  its obsolete allocation. Vacated skin comes from original skin below each eye
  instead of a shared white strip, preserving nearby pink shading.
- Cheek skin cleanup used a smaller, stationary near-eye selection and erased
  the edge of a moving eye. Preserve a stable near-eye sweep footprint before
  the skin mapping. Mouth mapping and far-eye/hair reaction are retained.
- This is runtime compositor code, not a regenerated/redrawn raster asset.
  Pixel-art skill guidance is used for original-versus-output visual inspection;
  its new-art generation receipt is not claimed. Existing accepted antialiasing
  and destination sampling remain unchanged.

## Failed trials and corrected test criteria

- trial1: direct inspection found a faint old lower-left outline. Its mask
  falloff was inside the source ink. trial2 expands the boundary into skin.
- trial2: direct cheek comparison exposed the near-eye truncation during left
  gaze plus pull. The skin-donor branch was erasing the moving eye footprint.
- The old lower-line test compared against stationary iris colour. With the
  whole eye moving, legitimate lower ink now crosses that point. Replace it
  with a translated ORIGINAL reference for moved ink, plus a clean-skin check
  below. The separate whole-outline test requires mean RGB error <8 and less
  than30% of stationary-reference error, at both lower outlines, all five sizes,
  and four cardinal directions. Fixed hair and other regions remain protected.
- A strict whole-head reference for vacated SKIN failed: moving an entire head
  also shifts the pink skin gradient, but eye-only motion must not. Keep <=10
  green-channel error on moved ink; below it require clean skin (G>=200).
  Do not describe this skin criterion as exact pixel identity to a shifted head.
- trial3: near-eye ownership depended on the moving footprint. New zero-crossing
  coverage during pull20 found delta7 at logical(19,55.67), scale1.5, dy1e-5.
  Final candidate uses the fixed union of the full gaze sweep, so ownership does
  not toggle with the current gaze. The <=1 continuity tolerance is retained.

## Evidence boundary

Final results are recorded below only after completion. Deterministic WPF
renders, injected controller input and PetLoop tests are NOT OS mouse execution
or display-FPS evidence. The existing face-sequence exporter does not call
UpdateHunting during every release/return frame, so its body-state switch cannot
certify real-loop body timing. Source art and installer/settings stay unchanged.
Full motion/performance and actual-user acceptance remain open.

## Final saved-render and test results

- Release focused/integration suite:66 passed/0 failed/0 skipped,1m21s
  (`tests/Dororong.App.Tests/TestResults/r67-final.trx`). Remaining applicable
  r66 regression cases:46 passed/0 failed/0 skipped,22s (`r67-regression.trx`).
  The union is112 distinct passing cases; no full-repository-suite claim.
- Also ran31 eye/mouth/cheek regressions with win-x64 target (`r67-published-core.trx`),
  all passed. Tested App DLL and published App DLL SHA256 are identical:
  `DEC439D8DED051EFE8CFC5E3C6B9D3D867BA483B25B5179ED87F55F5FA2FED32`.
  These31 repeat the focused cases, not31 additional distinct tests.
- Directly opened all five `final/eyes-*.png` sheets (360 direction/step renders),
  all ten `final/interaction-*-{Left,Right}.png` sheets (440 saved controller
  sequence renders), all six `final/loop-sheet-*.png` (288 injected PetLoop
  entry/return renders), final before/after and nine-state cheek comparison.
  Observed whole-eye outline movement, original-position cleanup, attached
  near-eye edge during pull, and retained mouth repair in these samples. No
  claim about every possible pointer path or real display-frame delivery.
- `artifacts/whole-eye-20261018-r67/final/r66-preservation.json`: five locked
  source hashes unchanged. Fifteen neutral/rebound/no-pull references identical;
  ten positive-pull references change21–50 pixels each, within the near-eye sweep
  region only. Outside that region:zero changed pixels. This restores original
  near-eye-edge ownership; it is NOT a claim that every facial pixel is unchanged.
  Mouth, cheek silhouette, head tips and body outside that scope are unchanged
  in this comparison. Existing installer/settings untouched.

## Actual execution boundary

User confirmed previous app exit. Checked no Dororong process remained, then
launched normal transparent1.5x Release:
`artifacts/whole-eye-20261018-r67/release/Dororong.App.exe`, PID121776.
Verified expected path and Responding=True. Before and after replacement,
computer-use `list_windows()` returned no Dororong target window. No OS-pointer
action was performed; actual tracking→cheek pull→release remains UNVERIFIED.
The normal candidate is left running for user acceptance. No installer update.
Process liveness or optional timing log is not display FPS or interaction proof.

## Subsequent user acceptance — 2026-10-18

The user replied "ㅇㅇ 됐다. 이제 다음 체크할 거" after replacement. Record
acceptance of this whole-eye tracking correction. Do not infer that every scale,
cheek combination or remaining motion was tested, or convert user feedback into
agent-performed native-input evidence. Preserve the accepted form. Remaining
checks return to hover departure/latency, grab→tilted release→hard landing, and
window/taskbar readiness→attachment→detachment from the existing roadmap.

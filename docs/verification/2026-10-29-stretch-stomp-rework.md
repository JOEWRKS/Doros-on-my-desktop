# Stretch / stamp visible anatomy rework — 2026-10-29

**Subsequently rejected by the user:** the added chin-to-armpit seam divides the
white body unnaturally. This candidate is not visually accepted or a resolved
shape defect. Follow-up removes the divider and repairs the actual outer join;
see the subsequent no-divider evidence. Prior numeric results retain only their
measured scope.

The user rejected the first comparison as visually unchanged. That visual-success
claim was wrong and remains withdrawn. The earlier numeric tests established
continuity and preservation, not the requested shape improvement.

## Actual correction

- Stretch now composites the leading foreleg separately instead of stretching
  both forelegs/lower chest together. The supporting foreleg stays planted;
  the extended forepaw has its own silhouette and shoulder boundary. The existing
  upper-body bow, head, shake and 2.8-second timing remain.
- Stamp retains its accepted enlarged rigid arm transform, pivot and timing.
  The rounded shoulder union gains a curved inner boundary from below the head
  to the moving underside; its endpoint follows that underside instead of floating.
- Original artwork, Core behavior, other expressions/motions, installed pet,
  settings and public release assets were not changed. The shared Mac raster hook
  matches Windows; this is not a new native Mac package or native execution test.

## Rejected follow-up attempts and evidence identity

- The quick probe initially loaded old App DLL `D24D0AD...`, while the newly built
  App was `2C546CAB...`. Therefore `followup-try2` and other pre-identity quick
  probe images are invalid evidence for their purported new source. Do not infer
  new geometry behavior from them. This stale-copy failure was discovered by
  hashing both files, not by assuming a successful build refreshed dependencies.
- `Render-Verified.ps1` now copies exact App/Core files, compares hashes, saves
  `identity.json`, and stops on mismatch or renderer failure. Final evidence below
  was generated through this guard. Earlier user-rejected candidate was separately
  identified; the stale follow-up probe is not an excuse for its insufficient shape.
- `try2-verified`: actually rendered the new arm, but the seam clipping at Y81
  left a detached lower endpoint during stretch. Expanded its bounded ink region.
- `try3` / `reworked`: shape visible, but the five-unit stretch arm drop failed
  all six presenter scale/facing sole guards (39/45 passed). Changed the drop to
  two units; did not loosen the sole tolerance or alter stamp trajectories.

## Final evidence

Root: `artifacts/stretch-stomp-repair-20261029/`.

- Before: `before-verified`, immutable accepted App SHA256
  `B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0`.
- Candidate: `final-reworked/identity.json`, App SHA256
  `B89502C30B95285D075094E5A1B5C7F21B5D3EB3A2B4C9C050740D373AB14F15`.
  Final source cleanup removed trailing blank lines only after this capture.
- `tests/reworked-final.trx`: **45/45** related tests pass, no skips. Includes
  presenter clipping/sole/return guards, rigid arm size/rotation/cadence, contour
  continuity, other feet, interior opacity, stretch inverse and separate anchors.
- `final-reworked`: **2,706** rendered frames, 60 Hz, 100/125/150%, both facings.
  All changed-region frames were directly inspected in the 18 `final-visual-all`
  sheets, including entry, maximum reach, impacts and return. Twelve full-body
  key poses per motion were also inspected. Observed separate stretch limbs and
  connected stamp seam; no detached endpoint, duplicated outline, hole or clipped
  forepaw found in those inspected regions. This is rendered inspection, not
  real mouse input or user acceptance. The added inner shoulder line is intentional.
- `final-comparison-metrics.json`: 2,226 frames differ from accepted baseline;
  changes confined to forebody. All **282** stamp pose/root/tip records match
  exactly. The reviewed ROI contains the entire changed-pixel bounds.
- `final-parity-result.json`: **4,848/4,848** Windows vs managed Mac raster frame
  hashes match; **3,636/3,636** unaffected-motion hashes match accepted Windows.
- `final-visual-review.json` records inspected sheet hashes. `frame-review.html`
  and `frame-review-v2.html` now show the final rework, using `final-reworked/raw`.
  The earlier rejected viewer is retained as `rejected-frame-review.html`.

## Limits / next

Source/render correction prepared for comparison. User shape acceptance, actual
desktop operation of this candidate and native Mac display remain pending.
No live pet restart, installation, commit, push or public release was performed.
Next is acceptance of the shown anatomy before replacing an accepted release.

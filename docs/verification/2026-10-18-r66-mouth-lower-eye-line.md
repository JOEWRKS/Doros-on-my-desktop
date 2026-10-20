# r66 mouth residue and lower-eye duplicate line

**Superseded for gaze behavior:** the user confirmed that colour moves but the
eye itself does not follow direction. Keeping the rim fixed was an incorrect
implementation/test criterion, not an input failure. Mouth repair is retained;
see [r67 whole-eye tracking](2026-10-18-r67-whole-eye-tracking.md).

## Authority and failures

The user rejected r65 with mouth screenshots `1cc14993` / `38456ca8` and eye
screenshot `0d5022e1`, explicitly clarifying that the defect was the extra line
BELOW the iris, not its highlight. The agent's initial highlight diagnosis was
wrong. This report supersedes r65's pending visual acceptance, not its historical
test results. User then authorized fixing both mouth and eyes.

No original raster, head/hair tips, body, leg bank, cheek envelope, installer or
settings change is authorized or made. Pixel-art skill source/shape comparison
rules are used for this deterministic compositor repair; this is not a new
pixel-art generation/conversion delivery. Existing soft-alpha sampling remains.

## Reproduced causes and repair

1. The bang-preservation early return covered x16..31/y43..60. Outside the eye
   aperture it returned ORIGINAL source pixels even where the mouth's old ink
   should have been cleaned. The moved mouth was then shown below that residual
   fragment. Exempt ONLY the old/new mouth footprint from this hair guard. The
   existing mouth displacement (up to one left/two down) and original ink remain.
2. r65's iris donor rows included the dark lower outline. Upward gaze sampled
   that ink into the interior, ABOVE the stationary lower rim. Source pixel
   inspection showed left ink in rows130/131, right ink139/140. Re-annotate iris
   colour at actual 225px pixel centres; build a source-colour continuation from
   interior texels only, before parallel rendering. Old/new colour samples use
   the same sampler so zero motion is continuous. No new outline, no blur, no
   source-art rewrite, and no scaling of iris geometry.

At 1.5x in the failed r65 upward frame, logical (22.3,55) / (39.3,59) had green
114 / 80 from the transported ink. The first corrected frame had 188 / 179.
These values are diagnostic, not a full visual acceptance metric.

## Trials and test corrections

- `trial1`: mouth guard was initially shortened to y54. Saved comparison removed
  the reported mouth fragment and lower-eye line. Focused old suite: 23 passed,
  3 failed. Two old rigid-sampling windows reached the original lower ink when
  shifted upward, incorrectly requiring the very rim transport being removed.
  Windows now stay inside true colour, with unchanged <=1 channel tolerance.
- The old white-slit test's fixed G<160 rejected an original cool highlight at
  RGB169/168/183. Check chroma separation from white skin instead; the actual old
  r61 failures RGB222/213/221 and182/182/185 still fail. The lower-rim regression
  compares same-scale neutral coverage (within15) because a 1x pixel also covers
  stationary original ink; a universal 1.5x absolute G150 was invalid at1x.
- Mouth mapping and lower-rim regressions plus rigid interior/white-slit checks:
  10 passed after these evidence-based oracle corrections.
- Initial `final`: 91 related tests passed (1m50s), but the independent r65
  preservation comparison found 118 changed pixels OUTSIDE the mouth across
  ten pulled reference images. This candidate was NOT launched. Narrowing the
  entire guard to y54 had changed nearby skin ownership. Preserve this failed
  evidence; do not label it a successful preservation check.
- `final2`: retains the original guard and exempts only actual mouth footprints.
  Final results and native execution boundary are appended after completion.

## Verification limitations

Saved deterministic frames and injected controller/PetLoop inputs are not real
OS mouse interaction or display-FPS proof. The existing interaction export's
local cell37 body switch comes from not updating hunting in that harness segment;
it cannot certify body timing. This task changes only eye/mouth composition.
Overall motion/performance and native acceptance stay open.

## Final2 result and handoff

- Release related suite: 91 passed / 0 failed / 0 skipped, 1m45s.
  TRX: `tests/Dororong.App.Tests/TestResults/r66-final2.trx`.
- `final2/r65-preservation.json`: all five original locked head/body/bank SHA256
  values unchanged. Of 25 r65 neutral/rebound/pull reference images, 15 identical;
  ten pulls change only 6–12 pixels each inside the mouth region. Zero pixels
  outside that region changed. Gaze frames have their separately scoped iris
  repair and are not described as identical to r65.
- Direct visual review: original source diagnostic, final2 before/after image,
  all ten final2 interaction sheets (440 saved frames), final2 eye sheets1.35/1.50.
  Initial-final eye sheets1.00/1.10/1.25 were directly viewed, and all360 direction
  frame SHA256 values were checked identical between initial-final and final2.
  Thus the final2 eye samples reviewed cover five scales, eight directions and
  nine steps. Mouth fragment and duplicated lower-eye line were absent in these
  inspected samples. This is not a claim about every possible input/pose.
- User confirmed exit; no existing Dororong process remained. Published and
  launched `artifacts/face-boundaries-20261018-r66/release-final2/Dororong.App.exe`
  with `--high-resolution-grab=1.5`, normal transparent mode and performance
  capture. Verified PID128236, expected path, Responding=True.
- Computer-use enumeration immediately after launch and again after startup
  exposed no Dororong target window. Actual OS-pointer interaction was NOT
  performed. Existing installer/settings stayed untouched. User acceptance and
  the broader motion/performance work remain open; process liveness is not FPS
  or interaction acceptance.

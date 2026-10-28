# Mac test10 — accepted Windows motion port — 2026-10-28

## Scope and result
User authorized continuing the Mac update after the accepted Windows memory gate.
The2026-10-26 accepted Windows behavior is connected to the Mac runtime and
packaged as Apple Silicon test10. This is a locally built test candidate, not a
native-Mac-accepted release. No public upload, commit/push, installer/settings
change, live-pet input/restart or Windows product source modification this turn.

Prior Windows15minute memory evidence remains in
[the preparation record](2026-10-26-accepted-memory-mac-preparation.md); it was not repeated.
Exact accepted reference: `candidate-seated-head-occlusion-20261026/runtime`,
App SHA256 B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0.

## Confirmed missing connections and changes
- Mac sampler compiled the shared new partial files but never called StretchWarp,
  SeatedWarp/SeatedPaw or the seated chest contour map. Added identical sampling
  in the body layer. Original head/hair still cover the authored rounded paw.
- Ported accepted idle/seated presentation and captured-pose recovery. Seated
  clicking stays seated with the accepted gentle shake/cooldown; no click-hop.
- Enabled shared brain1minute DORO? and idle activities; default sleep remains3min.
  Resize/sit notify activity; completed meal notifies the beg scheduler.
- DORO? uses one retained vector drawing, accepted glyph outline/placement/opacity,
  and no extra image cache, native window or hit region. Native composition is open.
- Existing body deformations now conservatively use full-render fallback during
  gaze/breathing changes; the old rectangular dirty bounds did not describe a
  deformed body. Unchanged upright gaze/breathing still use partial updates.
- Source hashes for PetMenuHost, OverlayFramePolicy and QuartzDesktop match the
  preparation manifest. No popup theme/native frame/topology changes. Fixed bitmap
  capacity and bounded parallel8x8 sole sampling remain; modified files reviewed
  by diff and exercised by the existing regression suites.

## Evidence
Root: `artifacts/macos/accepted-20261028/`.
- `pixel-comparison.json`: **4,848 / 4,848 exact active-buffer SHA256 matches**
  against a separately executed probe referencing the immutable accepted DLL.
  Complete60Hz new seated tap/shake, groom/stretch/beg/question sequences at
  100/125/150%, both facings; density2 checkpoints and sparse anger/chuckle frames.
  Final candidate rerun after fallback guard also matched. Not every possible
  pose/input/density; not an all-frame native screenshot claim.
- `motion-review.png`: representative enlarged contours visually inspected;
  `bubble/question-*.png`: retained Avalonia bubble at three sizes/both tails,
  render tested;150% left-tail output visually inspected. These are Windows-host
  renders. Frame hashes cover the full selected sequences; visual inspection
  is explicitly representative, not falsely claimed for every frame.
- **292 Mac +109 Raster +14 packaging tests passed**, no skipped cases. The20
  new/ported activity tests and2 bubble/controller cases are included in those
  totals, not extra. TRX/logs under `tests/`.
- Launcher fixture initially exceeded its15second subprocess deadline in two
  simulated no-start/timeout cases while suites ran. Increased only the fixture
  deadline to60seconds; unchanged failure cases subsequently passed. Production
  launcher only changes its matching test10 identity.
- Distributed-code `pre-share.json` initially failed because normal meals were
  required to visit every enum value, including withdrawal-only Angry/Snatching/
  Chuckling. Updated the fixture to assert normal-meal phases and separately
  execute six withdrawals, actual snatch contact and supported-only chuckling
  at30/60/120fps. `pre-share-final.json` has no failures. No product behavior was
  changed to satisfy this stale assumption. This legacy Portable/Core check is
  supplementary; current high-resolution rendering is covered separately above.
- Initial cold legacy-render cost337ms vs final51ms is host-run variation, not a
  product performance fix or M2 smoothness proof. Native latency stays open.

## Bounded post-port memory
`memory/run.json`: PID30972/startUTC2026-10-28T14:18:19.7601514Z,
startTicks639287938997601514. Frozen Raster SHA256
9B9AD38933B24D4F2EB884B26240B95E754B1A6A47C1FA1BCF7FB7252C1BC712 matches the
packaged Raster exactly. Code-fed renderer poses/size100–150%/density1–2;
60seconds warmup, repeated workload to240seconds,60seconds neutral recovery.
No native input, forced GC, trim, heap dump or trace.

Normal completion300.015seconds,13,424frames,60samples. Max loop gap0.134sec;
max sample gap5.029sec; largest UTC/monotonic interval difference0.041sec. No
sleep-sized gaps. Recorded counts include recovery iterations and are not claims
of actual repeated mouse gestures.

| Window seconds | private median MiB | heap at last natural GC MiB |
|---|---:|---:|
|60–120|162.506|143.474|
|120–180|163.637|143.472|
|180–240|163.746|143.483|
|240–300 recovery|163.758|143.490|

After warmup: owned frame capacity2359296B and allocation counter184 unchanged;
184 is the initial high-water growth history, not184 retained frames. Body cache1,
one seated-distance object; managed committed size plateaued at160014336B.
Private memory plateaued rather than returning to cold baseline. No sustained
retained ownership growth was reproduced in this finite workload; do not call
this universal leak freedom. Scratch/sole/spread paths were not exercised here;
macOS native upload/window/GPU memory is NOT_TESTED. Handles/threads and natural-GC
snapshots are recorded without forcing recovery. No memory patch warranted.

## Deliverable and remaining native gates
`candidate-final/Doro-Mac-arm64-r104-test10.zip`
SHA256: cc3868867d3146aa9fb2e28ee4f331f5a6569b4d83a26506aca6f9a243cc084d
Size: 97925957bytes;237files;18native CodeDirectory checks. arm64,
macOS13+ target, bundled.NET8.0.32, version104.10. Ad-hoc integrity signature,
not Developer ID/notarized and not Apple's native codesign/Gatekeeper validation.
`verification.json` checks native pages/special slots, managed resource seals,
ZIP bytes/CRC/path/executable flags. `distributed-code.json` confirms identical
published/packaged managed DLLs; exact tested Raster hash is included.
Earlier `candidate/` is superseded solely by removing a README trailing blank;
use **candidate-final only**. Source/probe output and raw evidence remain local.

Tester instructions: quit old app, Finder-unzip, move Doro Mac Test.app to
Applications and open the.app. No .command or logs requested. Retain existing
security guidance in the bundled README. Real M2 CPU/memory, Retina input/visuals,
upper-screen drag, external/clamshell, native menus and landing smoothness remain
NOT_TESTED for this candidate. Existing accepted Windows remains unchanged.

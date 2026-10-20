# r63/r64 gaze, cheek ownership and facing — candidate, native acceptance open

**Superseded candidate:** after r64 launch the user reported one eye's rim
disappearing during tracking. Whole-eye displacement was still wrong ownership.
The follow-up r65 fixes the rim/iris split; r64 is not an accepted completion.

## Scope and preserved authority

User requested downward-gaze slit, stale facing lock, gaze/cheek composition and
rough cheek contour together; accepted source head, body and cheek envelope stay.
During r63 native feedback the user identified a large white wedge specifically
during cheek pull/release. This is a failed candidate, not a passed native run.
The subsequent request to expose the far hind paw was narrowed by the user to
**inspect the original and explain first**. No body/leg implementation changed.

## Causes and changes

- r61 translated an already hair-clipped eye layer. Moving it downward exposed
  the removed top as skin. r63 extends only the occluded source edge, clips at
  the stationary destination hair opening and subtracts the same extended layer
  at the old position, so the limit at zero is continuous. Interior eye texture
  still uses one rigid displacement; this is not r60's nonuniform eye warp.
- Skin cleanup donors were treated as physical face coordinates and received
  gaze again. A donor flag now prevents that. The stretched cheek interior also
  cannot receive a second near-eye copy; the eye owns its own feature region.
- Captured head-press facing remained until Walk. Ordinary Idle/Walk ownership
  now releases it; blocked fall/landing retains it. Gaze resets when facing flips.
- Only the exposed pulled-cheek contour receives 4x4 rather than 2x2 coverage.
  No blur, new stroke, changed line width or changed cheek-envelope geometry.
- **r64 feedback repair:** the old near-eye reaction translated the eye down
  2 DIP and toward the cheek up to 1 DIP, then cleaned the vacated footprint to
  skin. That footprint included the diagonal bang boundary. Merely extending
  its top elongated the highlight (trial5, rejected); keeping the bang but still
  translating the eye left a doubled boundary (trial6, rejected). The candidate
  now keeps the near eye anchored with its captured gaze during cheek pull.
  The mouth reaction, far-eye follow, hair-follow gesture and cheek envelope
  remain. This is an explicit change to near-eye reaction motion, not a claim
  that every prior reaction pixel is identical.

## Original hind-paw inspection (read-only)

Directly viewed `tools/HighResolutionArt/input/canonical-225.png` and
`src/Dororong.App/Assets/dororong-canonical.png`. Both standing originals visibly
show two forepaws and one hind paw. No separate far-hind contour is visible.
The source measurement has three visible-leg records. Current `upright.cjs`
adds `extraParts.farHind` only for `amount>0`, with offset `-9+7*amount`; the
connected path follows the same condition. Thus the fourth visible paw during
walking is the implemented gait's addition, not visible in the standing original.
No claim about an unseen original animation is made from these still images.

## Candidate history and evidence

`artifacts/gaze-cheek-20261018-r63/` retains trial1–7, first Release, native
callback log and enlarged user feedback. The first native Release was PID112596.
Related tests 78 passed; later focused regressions 11 passed. Those tests did
not detect the user-reported cheek wedge. They are not a visual success proof.
Trial1 fixed the donor line but retained an eye sliver; trial2 isolated it.
Trial3 refined contour coverage; trial4 corrected the zero-motion limit.
Trial5/6 were rejected as above; trial7 is the r64 near-eye ownership candidate.

Final r64 evidence directory: `artifacts/gaze-cheek-20261018-r64/final/`.
Final test, preservation and inspected-frame results are recorded below after
completion. Saved WPF renderer frames and synthetic PetLoop input are not OS
mouse input or displayed-frame-rate verification.

- Final r64 related suite: 80 passed (`r64-related.trx`). Source hashes 5/5
  unchanged; 25 baseline comparisons have no changed pixel outside the declared
  eye/bang repair and exposed cheek-contour regions. 15 neutral/rebound-zero-gaze
  comparisons remain identical; 10 positive-cheek poses intentionally differ.
- Directly opened 10 interaction sheets (5 scales, both facings, 44 cells each),
  the 9-pose cheek comparison and gaze sheets at 1, 1.1 and 1.25. The large
  user-reported cheek wedge was absent in these inspected samples. This did not
  validate the full eye rim; the later user report rejects that inference.
- The standalone presenter sequence shows a one-frame body return/rest change
  around cell37. That harness does not advance hunting during its later Render
  calls. This sheet therefore cannot certify runtime body transition timing.
  The synthetic PetLoop support test runs separately; no native body pass made.
- User closed r63; process absence checked. r64 normal transparent 1.5x launched
  as PID15680. Computer-use filtered windows still empty. No agent OS input.

## Native boundary

Computer-use skill/API was read and used. `sky.list_windows()` did not expose
the existing r61 nor the first r63 normal transparent pet (empty filtered list).
No coordinates were guessed; no real drag was performed by the agent.
User explicitly closed r61; process absence was verified before r63 launch.
The user then supplied the cheek-wedge screenshot, which overrides any candidate
visual-pass inference. r64 replacement and actual sequence remain pending here.
Settings, installed application, original art banks and release/distribution
state are not replaced. Existing broader motion/performance incompleteness stays.

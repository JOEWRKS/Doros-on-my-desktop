# Enlargement preview — 2026-10-16

## Scope and inputs

User deferred boot acceptance until a later normal boot and requested enlargement
quality proposals/previews first. No production art, renderer, installer, Mac ZIP,
Git remote or settings changes authorized or performed in this preview.

Primary reference: current `src/Dororong.App/Assets/dororong-canonical.png`, 96x96,
SHA256 `699348D1973709F228D843341AC5312AA7F449D57B9BFC76576256231E259A78`.
Secondary detail reference: original 225x225 `dororong-canonical-source.png`,
SHA256 `F96EC30CBD18429E6BA1138BFA4EB44F331974C9820D36EE97A02FE518E46504`.
Both were opened and inspected. The secondary source is not a complete current
high-resolution motion set; see the prior scale review. Runtime cleanup may remove
a few disconnected low-alpha pixels; this comparison uses the source PNG, not a
captured live WPF frame.

## Preview and findings

- Built-in imagegen made one transparent high-resolution restoration exploration.
  Preserved original output; local copy:
  `artifacts/scale-preview-20261016/hires-concept-unapproved.png`.
  SHA256 `FBF401DAB1167B1462435E69BE503EEB7295D793C4B24373349E20ECCD853065`.
- Prompt intent: Image1 primary for current silhouette/body outline/proportions,
  Image2 facial/hair-detail reference only; same pose, half-lidded purple eyes,
  tiny w-mouth, pink hair/rose/ribbon, short white body, no tail, transparent,
  crisp thin charcoal contours, no new details or style/shape reinterpretation.
  This is a generated candidate, not exact tracing, native pixel output or a
  fidelity guarantee. No palette quantization or production grid conversion.
- Direct review: contours are cleaner but the eye shapes/highlights, body outline
  and relative proportions drift from the current art. **Not approved for use.**
  Pixel-art review separated sharpness from identity preservation; sharper alone
  is not a pass. One generated still does not validate any animation or seam.
- Interactive comparison shows the unchanged96px PNG with pixelated enlargement,
  browser interpolated enlargement, and the unchanged generated PNG reduced to
  the same CSS canvas size. Sizes120/144/192px are 1.25x/1.5x/2x relative to96px.
  These are browser sampling illustrations, not exact WPF/Mac filter simulations
  or physical-size guarantees across DPI settings. Each asset retains its canvas
  padding; generated silhouette/placement are not assumed pixel-aligned.
- Conversation preview source:
  `C:/Users/tjdwo/.codex/visualizations/2026/08/25/01a03970-f3b0-7e43-8f9d-62ec267475ae/doro-scale-comparison.html`.
  PNG bytes are embedded unchanged. Base85 transport keeps the fragment below1MB;
  it is not an image transform. Browser inspection confirmed images at1.5x/2x
  and selector updates through1.25x/1.5x/2x. Product integration is unverified.

## Proposed path, awaiting decision

For truly smooth larger output without reinterpretation, trace/refine approved
source contours into higher-resolution **shared parts**, lock face/body landmarks
and line weight, then reuse those same parts across all motions and resample once
to the target output. Validate standing, tracked-head rotation, sitting, hanging,
eating and seams at the selected size; scale bounds/hit-tests/pivots together.
Do not independently AI-upscale each frame. This is more work than a size toggle.

Integer2x nearest-neighbor is the low-work identity-preserving alternative, with
deliberately larger visible pixels. Fractional smoothing can conceal steps but
cannot create source detail. The generated candidate is a direction comparison,
not the final recommended replacement. No enlargement implementation accepted.

Next: user chooses size and whether larger visible pixels are acceptable; if not,
approve a fixed-contour high-resolution part proof before motion-wide work.
Boot acceptance remains deferred; Mac native testing, R3/R4 remain open.

## Original-fixed 1.5x still proof

User authorized one stationary1.5x original-fixed comparison, not app integration.
The preceding size/method decision is superseded only for this proof. Pixel-art
review separated identity preservation from sharpness; no fixed-palette or
binary-alpha conversion was imposed on this anti-aliased character.

A second built-in image edit tried current96 alone with locked canvas positions,
face landmarks, line weight and silhouette, no beautification or new details.
It again changed eyes/outline and was rejected, not used in the final proof:
`C:/Users/tjdwo/.codex/generated_images/01a03970-f3b0-7e43-8f9d-62ec267475ae/exec-15ad515f-f8c6-43d7-a2cc-f68e2b583719.png`.
No further generation retries. This is not evidence that prompting locks pixels.

Final route reuses the repository's existing source-derived geometry, not an AI
redraw or newly invented trace. Ran unchanged `tools/Generate-CanonicalArt.ps1`
with pinned source and body mask, output only under `artifacts/scale-fixed-20261016`.
The source225 body uses the existing inward outline; eye/mouth/hair art remains
from the original. Export uses existing six-component/twelve-pixel resize-proxy
cleanup and one premultiplied high-quality bicubic225-to144 reduction. The current
runtime96 is independently enlarged96-to144 with the same sampling policy as a
comparison. Neither source PNG was altered; pinned hashes still match above.

Files under `artifacts/scale-fixed-20261016/`:
- `Export-Preview.ps1`: preview-only export and source invariant checks.
- `source-fixed-master-225.png`: existing source/outline/proxy path, not new art.
  SHA256 `1D84AA57E2824ADD4450427D56B0D45EB729B5EEDBE9F0FCC7A6FAB9F766483C`.
- `source-fixed-final-144.png`: final transparent144px still.
  SHA256 `07C68347B8764D978C99E8D2C83CF2CE99BC70B50E9810B62A56D21EA7897D9F`.
- `current-upscaled-144.png`: current96 enlarged for equal-size comparison.
  SHA256 `402FFC015594F315590EFF1B1C95522CA9104FB9244CBCBD314D59C72FD2DBB6`.
- `source-evidence/` and preliminary `source-fixed-144.png` retain intermediate
  evidence. Preliminary output was replaced in the comparison because it omitted
  existing resize-proxy cleanup and showed tiny body ink specks.

Checks: generated225 before proxy changed0 protected-mask pixels and0 alpha
pixels versus background-extracted source. Final proxy changes exactly12 RGB
pixels, alpha unchanged, membership hash
`2B9CB6D649884DA2DC826963E3258A23DAFAAE9A1B071335B168834746463A54`:
`(156,130),(125,150),(119,151),(100,156),(145,156),(145,159),
(146,159),(147,159),(144,160),(145,160),(146,160),(147,160)`.
These are existing proxy exceptions, not a claim of whole-source RGB identity.
Face/eyes/mouth are not redrawn. Final144 output naturally resamples colors and
alpha; source-level invariance does not mean144 pixels equal96 pixels.

Opened final PNGs and browser comparison at144px. Original expression and shape
read closer than the rejected generated candidates; contours are crisper than
96-to144 enlargement. Existing authored96 corrections are not automatically
proven equivalent to this historical225 path; final user acceptance and motion
parity remain open. No claims of perfect recovery, new detail or vector tracing.

Comparison fragment:
`C:/Users/tjdwo/.codex/visualizations/2026/08/25/01a03970-f3b0-7e43-8f9d-62ec267475ae/doro-original-fixed-150.html`.
Two PNGs embedded unchanged, displayed at144CSSpx. This verifies browser still
presentation only, not WPF/Mac animation, physical DPI size or seams. Next is user
judgment on this still before any wider work. Product/installer/remotes unchanged.

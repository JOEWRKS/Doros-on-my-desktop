# Third stamp: rejected shoulder contour follow-up — 2026-10-23

## Authority and failure

User's `codex-clipboard-705e32f6-64f0-47a8-93dd-027fca980fa5.png` rejects the
previous shoulder candidate. This supersedes its pending appearance acceptance, not
the accepted rigid pivot motion, second reaction or furious face. The preceding
inspection missed an obvious extra bend in the lower shoulder outline. Numeric
opacity/contact-step tests did not establish a natural contour.

## Reproduction and confirmed scope

- Added real `PetLoop` → presenter → live display bitmap capture using code pointer
  input, not desktop automation. Three withdrawals produce the third reaction.
- Before evidence: `artifacts/snack-joint-20261023/before/`. The actual route is
  progress0, no motion/body frames, no lag; only its facing transform differs from
  the old synthetic proof. No evidence of an unrelated pose or cache being mixed in.
- The old bridge attached to source(26,75), inside the paw rather than its outline.
  Its reconstruction stopped atY81, and filling the bridge over the old outline
  preserved a second protruding bend. Saved frames also show this; the prior visual
  judgment was insufficient, not a failure by the user to provide evidence.

## Correction

- Register to inner foreleg edge(28,76.4), following the transformed edge tangent
  and the stationary body's lower curve. Keep the original arm matrix untouched.
- Replace the vacated inner-root silhouette locally instead of merely filling over
  its old alpha. Extend the reconstruction through the join before returning to
  untouched original ink. Match existing size-compensated outline width.
- First attempt only extended/reshaped the bridge; it still kept the old protrusion.
  Rejected in `attempt1/`. `attempt2/` replaces that obsolete silhouette.
- Core timing/axis/extension, first and second reaction, accepted face and source
  artwork unchanged. No new runtime cache/frame allocation, install/settings edits.
- Shared Point subtraction initially failed the portable build; replaced it with
  explicit coordinate subtraction. This is recorded as a failed build, not a pass.

## Verification

All artifacts below are under `artifacts/snack-joint-20261023/`.

- `final/runtime-{scale}-{side}/`:56display frames each, 16ms code steps,
  100/150% × two facings =224frames. Each live display bitmap is compared pixel-for-
  pixel with the full renderer using the presenter's actual matrix, face, progress,
  lag and spread. This checks pipeline agreement, not independent aesthetic validity.
- All56frames at150% canonical facing visually opened in seven sheets, plus mirrored
  contact and five-size overview. Shoulder lift/contact/recovery inspected against
  the rejected screenshot and before frames. Old double bend no longer observed in
  these saved frames. Actual mouse/window presentation remains unverified.
- Five-size55-frame checks retain opaque torso, unchanged rear feet and exact
  endpoints; pivot/radius, interruption recovery and accepted expression tests retained.
- New contact regression: maximum quarter-DIP sampled second difference across
  x27..37 is .25DIP (rejected candidate .5DIP), at both contacts. This addresses the
  missed kink specifically; it is not an all-animation aesthetic certificate.
- `windows.trx`23/23 before the additional kink regression/portable coordinate syntax
  correction. `windows-final.trx`24/24 after both changes (2m13s).
- `mac-host.trx`5/5 controller checks on Windows, not native Mac. Total final functional
  checks39. `proof-aspect.trx` separately regenerates the55-frame inspection sheets:
  the old48x36→256x128 crop distorted aspect; corrected to256x192. Runtime-route sheets
  already preserve aspect. This inspection error is not concealed by test counts.
- `raster-final.trx`10/10:400 full/retained comparisons, one owned frame allocation,
  expression buffer reuse. Not a long-running memory test.
- Core SHA256 remains `B17665D8D68C1895BDBF8D8D290CC25A9C395DE8E84E5BE602D6DEC8E890C65F`.
- Native Mac, real desktop input/timing and long-memory tests not performed. No
  automated user input, heap pause, process killing or public upload.

## Delivery

- Separate candidate: `artifacts/product-shell/candidate-snack-joint-20261023/runtime/Dororong.exe`.
- App SHA256 `5EDF0E56E4B4D2AEA9A8460BBE9EDE4E294FFE3D4F63D27405EF90B4E8A0C196`.
- ZIP SHA256 `3CF2348B5806CBE57B7BF34F2A835ECE5654D7F5E22C403A73F45FB2C093DFEE`.
- App/Core hashes matched tested DLLs. No existing pet process; announced candidate
  launch, PID51768 at21:04:10KST, same-path alive/responding after3s. No automated inputs.
- User's actual appearance acceptance remains pending. Startup is not an animation pass.

# Voluntary snack play — 2026-10-13

## Product authority

The user approved the proposed menu → jewel snack → unlimited-range chase →
bounded jump → stationary mouth offer → closed-eye chewing flow in this chat,
and requested Windows login startup in the same implementation. This replaces
the earlier Backspace/typing triggers and numeric care proposal, not existing
character interactions or release gates. No hunger, score, daily goal, penalties,
keyboard hook, text/clipboard inspection, currency or shop.

## Behavior and UI

- Character right-click and tray menus: **간식 주기 / 간식 취소** and
  **Windows 로그인 시 실행**. Existing Sit, logs and exit remain.
- During snack play only, pointer distance does not limit pursuit. Existing
  monitor bounds and real platform contact still constrain travel. A high snack
  triggers an occasional bounded hop, not flight or teleportation.
- Hold the snack still near the mouth to offer it. Passing across the mouth
  does not feed. One accepted snack moves into the open mouth, then the mouth
  closes for three closed-eye chewing beats. Eating lasts1.7s, followed by the
  approved2.16s happy head sway and a320ms quiet settle before ordinary
  tracking/hunting can resume. There is no inventory cost
  or session counter. A mid-flight offer waits until the jump has landed.
- Menu cancellation, a direct character press, missing pointer or a long timer
  gap ends play. Sitting interrupts it. Perching/carry and autonomous hunting
  cannot simultaneously own chase position. Real falling/contact remain active.
- Cursor implementation is a transparent, click-through jewel companion centered
  on the pointer. It does **not** replace the Windows cursor scheme, hide another
  application's cursor, take focus or swallow other applications' clicks. The
  normal OS pointer can remain visible over the jewel. This implementation
  distinction must remain visible in handoff, not be called a global cursor swap.
- Startup is off until enabled in either menu. It registers only this user's
  quoted executable in HKCU Run, and can remove only the exact matching command.
  A different existing command is preserved and reported. No service, admin task,
  global setting or automatic registration on ordinary app launch.
- Uninstall cleanup checks the exact installed executable command before deleting
  that one startup value. Existing startup choices survive in-place updates.
  Windows Startup Apps can independently disable an entry; a reboot/login is a
  separate runtime acceptance check, never inferred from registry tests.

## Visual contract

Preserve the approved character silhouette, head/body ratio and existing closed
eyes. New mouth/cheek animation is local; do not squash or dissolve the whole
sprite. Drool extends below the mouth only while chasing, and disappears when
eating or canceled. Both facing directions must keep mouth/food alignment.
The generated jewel is a draft requiring user visual acceptance, not a claim
of rights clearance or approval of new character artwork.

### User motion correction — 2026-10-13

The user rejected the first candidate's blurred snack character and accelerated
walk, and explicitly requested these six corrections: sharp snack rendering,
dedicated quadruped run, hunting-inspired jump, reference-like open/accept/close/
sideways-three chewing mouth, delayed hunting/tracking while eating, and smooth
motion transitions (particularly hunting to ordinary).

- Keep native96 output. Stationary/eating uses the approved standing source with
  local mouth/drool overlays only; do not rescale/re-filter its entire face/body.
- Run is an asymmetric short-legged gallop: forefeet land in sequence, hindfeet
  gather/push, with suspension between support groups. Use source body/head
  artwork and proportions, not the walking bank at a different playback rate.
  [Quadruped gait reference](https://arxiv.org/abs/0809.2415) supports asymmetric
  within-pair footfalls; exact timing/amplitude is a cartoon adaptation, not a
  biomechanical simulation or source-provided numeric recipe.
- Snack jump:180ms grounded hunting-like crouch,480ms bounded flight,200ms
  landing/recovery. Only physics moves world height; renderer changes posture.
- Mouth reference is the user's DORO figure image attached in this correction.
  Use its smiling open mouth as expression reference, not the photo/text as an
  app asset. Preserve existing closed-eye artwork. Draw a small tongue, bring the
  jewel into the opening, then keep the omega/sideways-three mouth closed while
  its two lobes chew. Do not cycle a plain open oval for the whole eating phase.
- Eating and settling exclusively own behavior; pointer approach cannot build
  hunting dwell. Hunting departure cancels its behavior immediately while the
  displayed crouch follows the authored recovery over about240ms. Direct presses
  and safety cancellation retain their existing immediate interruption semantics.

Implementation proof and remaining acceptance: [motion correction evidence](../verification/2026-10-13-snack-motion.md).

### Follow-up correction — landing, trajectory and source mouth

The user's next review rejects the landing forepaw jab and airborne cursor
retargeting. Landing must keep the planted ordinary paws, not replay the hunting
crouch. Capture horizontal destination/facing at jump start; follow one bounded
arc, with eased takeoff/touchdown, then resume pursuit after landing.

The user also clarified that closed chewing means animating the **existing mouth**,
not drawing a new sideways-three mouth. Preserve its original lip pixels, shape
and color, deform them locally for three small chewing beats, and restore the
exact source at the end. The reference-like temporary open lower mouth/tongue
remains for acceptance; it closes back into the authored mouth.

Whole-character breathing blur was requested for diagnosis and a proposed fix,
not an automatic redesign. Production breathing is unchanged in this follow-up.
The measured cause and proposed head-fixed/body-only approach are recorded in
the verification document; approval and implementation remain pending.

### Shared-source correction — approved 2026-10-14

The user approved fixing inconsistent state sharpness/body identity using shared
original body/head parts. Neutral idle, tracking, post-walk idle and snack must
share the same cleaned source. Walking moves source parts rather than displaying
a prefiltered head. Remove whole-image breathing enlargement; ordinary upright
breathing moves only the torso, keeping head and soles fixed. Preserve approved
seated artwork and the existing direct-interaction ownership/outline contracts.
This supersedes the earlier "breathing diagnosis only" boundary above, not the
source-art identity or user visual-acceptance requirement.

### Upright tracking resampling — approved 2026-10-14

After accepting the snack entry/chewing correction, the user reported shimmering
face pixels during ordinary head tilt and approved a head-only interpolation fix,
explicitly requiring that the character not become blurry again. Preserve source
art, neutral pixels, body and existing motions. Check consecutive slow-rotation
frames as well as stationary line contrast; do not blur the whole sprite or smooth
by mixing old frames. This is not approval to redesign the head or body.

### Happy after eating — approved 2026-10-14

The user replaced the suggested hops with a left/right happy head tilt and
approved implementation. After the final original-mouth chew, keep the existing
closed eyes and mouth: briefly hold neutral, tip8 degrees to one side,8 to the
other, then4 back, and return to center. The initial960ms accented version was
superseded by the user's slower, longer “흔-들 흔-들” feedback: move continuously
through each reversal without a held accent. The2.16s head gesture reaches its
extremes at.46s,1.10s and1.64s, then centers at2.10s; the last sway is smaller.
This is followed by the existing320ms
quiet settle, which reopens the eyes. Mirroring follows the pet's facing.

Body and feet stay planted. Reuse the accepted head-only interpolation and hidden
neck backing; no whole-sprite scale, new facial artwork, hearts, text, scores or
additional jump. Suppress autonomous tracking/hunting throughout the reaction,
but retain immediate direct press, menu cancellation, sitting and safety exits.
Actual support motion/falling must remain active. Renderer proof and automated
checks are not user visual acceptance.

## Acceptance

- Both menus start/cancel one session; existing interactions remain usable.
- Far pointer causes running; high pointer causes bounded hop and real landing.
- Stationary offer eats once; moving pointer cannot accidentally trigger chewing.
- Cancel/exit/fault/input loss retire the jewel and return position ownership.
- Eating closes eyes, shows multiple mouth beats and preserves outline alpha.
- One completed snack triggers the closed-eye happy tilt below; planted feet,
  neutral endpoints and immediate direct-interaction interruption are preserved.
- Startup quoting, opt-in, disable, conflict and failure paths are tested without
  modifying this machine's real startup configuration during automated tests.
- Latest source builds; targeted and regression results belong in TASKS.
- User visual/runtime acceptance, login startup and installer cleanup execution
  remain separate from code/test completion.

## Asset provenance

Reference: [NIKKE jewel shown by Midasbuy](https://www.midasbuy.com/midasbuy/th/buy/nikkegl),
image `https://cdn.midasbuy.com/images/icn_inappshop_jewel_01.45b6e412.png`.
Retrieved only as reference under ignored `artifacts/snack-20261013/`.

New asset: `src/Dororong.App/Assets/snack-jewel.png`, generated with built-in
image generation from that reference. Prompt: a single original, simplified
chibi cyan/blue faceted vertical diamond, thin dark outline, one white facet
shine, legible at 24×32, transparent background; no circuitry, reflection,
wrapper, text or extra objects. The reference itself is not bundled.
This remains a NIKKE-inspired derivative asset under the existing rights hold;
MIT code licensing does not grant rights to it.

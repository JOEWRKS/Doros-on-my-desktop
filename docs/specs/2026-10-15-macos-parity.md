# macOS parity implementation checklist

## Authority and scope

User request (2026-10-15): develop on Windows, ask Mac users to test, and
implement the same features as the current Windows product. Baseline: public
v0.1.3 candidate14, source checkpoint d061177. This is not approval to remove
window/Dock hanging, ship a reduced product, buy signing, publish or contact testers.
Keep the existing WPF product, artwork and Windows installer unchanged.

2026-10-21 updated authority: user requests the accepted **r104** Windows
experience on **Apple Silicon (M1 or later)** and explicitly chooses a full port
over a rebuild of the existing tester. The table below remains historical
test04 implementation evidence, not r104 parity. Current work and validation
gates are in [the r104 port record](../verification/2026-10-21-r104-source-and-mac-port.md).
Intel packaging is not a prerequisite for this Apple Silicon delivery.

2026-10-16 authorized exception: user clarified physical screen-bottom movement
in front of Dock in z-order, not Dock-top height. Dock support/hanging is replaced
by screen-floor movement; ordinary-window support/hanging is retained. This later
decision supersedes the original Dock-equivalence requirement only (MAC-12).

This human implementation checklist follows AGENTS/TASKS/ROADMAP. It is not a
JOEFLOW V2 closure/manifest; the old snack candidate is not canonical authority.
Existing visual authority remains DESIGN and accepted product records.

## Approach and acceptance

- Separate Avalonia host; reuse Core behavior/platform math and approved assets.
  Port WPF-dependent pixel rendering without rescaling/distorting the character.
- Windows development/build/tests are valid evidence only for those operations.
  macOS executable output does not prove macOS execution or feature parity.
- Start with renderer/host probes, then interactions, then native OS integration.
  This order does not exclude later features from the required release scope.
- Target builds for Apple Silicon and Intel. Exact supported macOS versions must
  be declared after framework/native API requirements and actual tester coverage
  are known. Do not claim every Mac is supported.
- No keylogging, screenshot collection or automatic log uploads. Read only the
  minimum pointer/window geometry needed. Native permission requirements and
  denial behavior must be verified, not guessed. Denied permissions must not
  crash, loop prompts or silently claim full operation.
- No Gatekeeper disable instructions. Signing/notarization and clean-download
  launch are a separate Mac distribution gate, not waived by Windows signing deferral.

## Checklist

Status per item: TODO / IMPLEMENTED / WINDOWS_CHECKED / MAC_VERIFIED.
An item is complete only after applicable behavior, failure/recovery and visual
acceptance are verified on Mac. Tests alone do not authorize a visual redesign.

| ID | Required parity | Acceptance / important edge cases | Current |
|---|---|---|---|
| MAC-01 | Same source proportions and outlines | Identical neutral silhouette; no rump artifacts, seams, extra neck ink | Partial: neutral cleanup port + Windows probe; Mac TODO |
| MAC-02 | Pixel renderer | Normal/Retina scale, head rotation, no new blur or jagged deformation | Partial: Windows-derived outlines/effects and CPU composition refined; strict reference checks pass; residual subpixel coverage and Mac/Retina TODO |
| MAC-03 | Idle, breathing, blink | Same timing and local motion, no whole-character scaling blur | Partial: torso breath, shared blink and desktop idle clock connected; reference checks pass; Mac TODO |
| MAC-04 | Walking | Same four-leg gait; feet begin when body moves | Partial: current gait/timing and shared desktop movement connected; Windows comparison; Mac TODO |
| MAC-05 | Sit/rise | Accepted articulated frames, front legs and z-order retained | Partial: approved bank/timing connected to desktop diagnostic controls and movement hold; Mac TODO |
| MAC-06 | Sleep and automatic-sleep option | Default ON, wake on OFF, persist across restart, other actions unchanged | Partial: default/wake + atomic JSON persistence and B2 menu connected; corrupt-save/restart Windows checks; Mac TODO |
| MAC-07 | Pointer tracking | Same ranges, eye/head/body response and smooth enter/exit | Partial: upright easing/native input, posed head/cheek inverse input and hunting/snack/direct arbitration connected; Mac TODO |
| MAC-08 | Hunting/pounce | Preparation, immediate cancel on exit, jump/land and transitions | Partial: Windows state/cancellation plus corrected body/head bridge, outline and native reduction; continuous-frame comparison checked; native Mac acceptance TODO |
| MAC-09 | Head drag and dangle | Correct grab anchor, swing, release/landing, no cursor jump | Partial: grab/gutter plus body-click wake, pending sole and extreme spread/frozen source connected; reference math and simulated interruption checked; native/composed Mac TODO |
| MAC-10 | Cheek pull | Same shape/length; no body dragging; timed spring/back-hop and new landing position | Partial: captured standing/seated/hunting/perched pose, existing raster/follow/spring/hop math and refined composition Windows checked; native Mac TODO |
| MAC-11 | Window edge hanging | Same readiness/release boundary; moving/closed/minimized windows recover safely | Partial: shared engine, advertised cue/release and native adapter; synthetic move/close tests pass; actual minimized/window behavior/Mac TODO |
| MAC-12 | Screen floor in front of Dock (approved Mac exception, 2026-10-16) | Walk at physical screen bottom; Dock is not a support/perch; remain visually in front of Dock; ordinary window support preserved | Partial: test04 mapper/startup and synthetic/package checks pass; actual Dock z-order, orientation/auto-hide and fullscreen transitions require Mac validation. Replaces previous Dock-top hanging requirement by user decision. |
| MAC-13 | Hanging paw interaction | One paw tap-wave or stretch; pet stays anchored; no seam exposure | Partial: same local render/inverse hit-test and tap/pull/release timing connected;48 exact reference poses and anchored/owner-loss tests pass; Mac TODO |
| MAC-14 | Snack mode entry/cancel | Existing location preserved; jewel follows pointer without intercepting input | Partial: desktop entry preserves position, global click-through jewel connected; native input/cancel/Mac TODO |
| MAC-15 | Snack chase/run/jump | Same gallop and ballistic jump, not cursor teleport/floating | Partial: current frames/Core session connected to native movement and platform engine; Mac TODO |
| MAC-16 | Eating and happy sway | Open/eat/chew/slow sway; hunting/tracking suppressed until transition ends | Partial: original mouth/sway, snack ownership and normal-hunt suppression connected; actual Mac TODO |
| MAC-17 | Character menu | Same approved style, snack/sit/auto-sleep/startup/exit states and behavior | Partial: B2 popup/commands, live startup states and headless keyboard/outside-dismiss/failure recovery checks; native Mac TODO |
| MAC-18 | Menu-bar icon/menu | Doro head; primary/secondary click, no flicker; logs/settings/exit | Partial: native NSStatusItem left/right action, dedicated B2 window and disposal connected; headless lifetime/resize checks; native focus/flicker/Retina TODO |
| MAC-19 | Optional login startup | Default OFF; opt-in/out survives login; relocated app and OS-denied state handled | Partial: SMAppService register/unregister/status and pending approval/settings connected; macOS13+ valid .app required; fake-service failures/readback checked; actual login/denial/relocation TODO |
| MAC-20 | Window layering/input | Transparent hit test, no focus steal; ordinary windows; Spaces/fullscreen policy tested | Partial: owned NSWindow placement/level/Spaces flags and alpha click-through implemented; actual Mac TODO |
| MAC-21 | Multi-display geometry | Negative origins, mixed scale, screen removal, clamp/recovery, pointer conversion | Partial: point-coordinate conversion and synthetic removal/clamp checked; mixed-Retina/Mac TODO |
| MAC-22 | Lock/sleep/resume | No runaway chase, stale surface or large time-step jump | Partial: long-frame cancel/stale-scene limits checked; OS notifications/actual resume/Mac TODO |
| MAC-23 | Settings/logs/single instance | Local user data, atomic saves, duplicate refusal, no private window text logged | Partial: settings/logs plus per-user mutex; Windows separate-process duplicate/normal/crash restart checks pass; native failure/lifecycle TODO |
| MAC-24 | App packaging/lifecycle | .app, head icon, update/rollback/uninstall and data preservation; arm64/x64 | Partial: test02 arm64 ZIP removes normal-launch inspector, preserves product-menu exit/error recovery; Windows bundle checks; native launch/lifecycle and x64 bundle TODO |
| MAC-25 | Distribution/security | Dependencies, signature/notarization, quarantine launch; never disable OS protection | Partial: pinned runtime8.0.32, ad-hoc code/resource hash checks and no-bypass guidance; Developer ID/notarization, Apple signature validation and downloaded quarantine launch TODO |

## Implementation milestones

1. Portable asset reader and transparent host probe; compile both Mac RIDs.
   Probe is deliberately not a parity product and must say so.
2. Port production pixel algorithms and compare reference frames for all actions.
   Do not substitute legacy walk-bank frames for the current gait.
3. Connect interaction orchestration and Mac pointer/window/Dock adapters.
4. Port approved menu design, persistence, menu-bar and startup lifecycle.
5. Package a tester build, run Mac checklist, resolve differences, then request
   publication approval. No public macOS release before actual Mac acceptance.

Milestone1 now has a buildable probe and automated renderer checks. This does
not complete any full feature row. Detailed evidence belongs in
[TASKS](../../TASKS.md#macos-parity-foundation--2026-10-15).

Milestone2 now has walk/tracking/snack rendering and a bounded interaction lab.
It does not complete all-action rendering or any full Mac feature row. Current
comparison results, raster differences and next integration boundary are in
[the motion execution record](../../TASKS.md#macos-walk-tracking-snack-renderers--2026-10-15).

## Tester handoff (when a testable candidate exists)

Milestone3 now has native pointer/window/Dock adapters and a desktop-integration
preview with shared physics and current walk/tracking/snack frames. It is not
complete interaction arbitration or Mac acceptance. [Current execution evidence
and next work](../../TASKS.md#macos-desktop-integration--2026-10-15).

Hunting/rest/head-drag paths have now been connected and checked against the
Windows baseline. Remaining source/composite differences and actual-Mac gates
are retained in [that execution record](../../TASKS.md#macos-hunting-rest-head-drag--2026-10-15).
Pose input, host gutters and cheek/paw integration now have Windows-side evidence
in [the current record](../../TASKS.md#macos-direct-input-cheek-paw--2026-10-15).
No row has become MAC_VERIFIED; actual native/visual acceptance is still required.
Click/wake/extreme landing, B2 component and persistence evidence now lives in
[the next execution record](../../TASKS.md#macos-click-wake-landing-and-menu--2026-10-15).
Native status-item, login registration and single-instance paths are now connected;
Windows-side tests and developer bundle scaffold are recorded in
[the lifecycle record](../../TASKS.md#macos-native-menu-startup-instance--2026-10-15).
Actual Mac execution and login acceptance remain required, not scope exclusions.

Record Mac model/chip, macOS version, resolution/scaling, monitors and Dock mode.
The current local arm64 test04 ZIP and exact hash are in the
[screen-floor record](../../TASKS.md#mac-screen-floor-and-scalestartup-review--2026-10-16).
The remote test03 and earlier test01/test02 remain historical, unchanged artifacts.
It requires macOS13+ for this test; OS version and native execution remain unknown.
Send exact version/hash and checklist IDs. Tester records PASS/FAIL/NOT_TESTED,
steps, expected/actual result and optionally a cropped recording. Ask them to
review logs before sharing; do not request passwords, unrelated windows or screenshots.
Test installation/first launch with normal OS security, permission refusal then
grant/revocation, all25 rows, restart/login, update and removal. Keep failures open.

Sources checked 2026-10-15:
- https://docs.avaloniaui.net/docs/platform-specific-guides/macos
- https://docs.avaloniaui.net/docs/deployment/macos
- https://developer.apple.com/developer-id/

# Desktop menu visual direction

Status: B2 mockup and installed UI accepted on2026-10-14; own-tray stability and
Figma-menu support confirmed by the user. Runtime limits remain tracked in TASKS.
Scope: NIKKE-inspired desktop menus, authorized2026-10-14. Existing character
authority remains in AGENTS-linked artwork/interaction specs and TASKS acceptance.
This file does not replace those sources or expand product features.

## Bounded automatic-sleep option — 2026-10-14

- User requested an option to disable lying down. Add `자동 눕기` to both menus,
  using the existing54-DIP switch row, ON/OFF text and color roles. Default ON;
  retain snack priority, character sitting, startup, tray logs, exit and version.
- Reuse the current B2 composition and native keyboard/dismissal behavior; one
  extra row, no new panel or character artwork. Moon vector distinguishes sleep
  from the startup monitor icon. Existing settings section gains only this row.
- Independent scope review requires matching menu/runtime state, persistence,
  unchanged non-sleep actions and checks at screen corners with the taller menu.
  Native proof: `artifacts/ui-menu-sleep-option-20261014/native-proof/`.
  Local render inspection is not user acceptance or full JOEDESIGN certification.
  Execution and remaining limits belong in [TASKS](TASKS.md#automatic-sleep-option--2026-10-14).

## Inspected references

User explicitly asked the agent to find web images and use NIKKE's overall UI.
Inspected actual images in browser screenshots, not just search descriptions:

- Primary: [NIKKE settings panel, Dengeki](https://dengekionline.com/articles/171104/)
  ([image](https://dengekionline.com/images/BXjF/i2bE/jO5x/abSb/nfHunb8sNFeUFQ55STIkibQnBJl1mJKBVyFwImLBjzMAgXLzcq8GrLlDirDeXlYLxbdcjJqHaHlWwg67_main.jpg)).
  Blue header, white inner panel, gray text/control tracks, cyan selected states,
  modest corner rounding, compact aligned rows. Owns surface/control language.
- Secondary: [NIKKE lobby, beginner guide](https://nikke.gg/beginners-guide/)
  ([image](https://i0.wp.com/nikke.gg/wp-content/uploads/image-365.png?resize=1024%2C529&ssl=1)).
  Character-led space, rectangular icon+label actions, layered/slightly angled
  action blocks, white/blue contrast. Borrow action hierarchy, not counters,
  ads, notification dots, store systems or full-screen scenery.
- Secondary: [NIKKE confirmation popup, Inven](https://www.inven.co.kr/webzine/news/?news=296226)
  ([image](https://static.inven.co.kr/column/2024/06/03/news/i1692632557.jpg)).
  White body, blue title band with subtle geometric facets; neutral cancel and
  cyan confirmation, faint watermark. Borrow button-role contrast and restrained
  geometry, not currency/cost or copied game assets.

These are dated screenshots, not proof of the latest game's full UI or motion.
Refero live search returned NO_SUBSCRIPTION. Local OMD source corpus was not
located in the inspected locations; no OMD retrieval or completed JOEDESIGN
review is claimed. User directed web-reference research instead. Refero's
bundled color-role guidance supplements readability, not NIKKE visual authority.

## Proposed translation / reference lock

- Compact on-demand menu beside the character/tray, not a permanent dashboard.
- Dominant white/cool-gray panel; blue/cyan title/action roles, dark readable text.
  Cyan highlights primary snack and enabled/selected states, not every item.
- Snack give/cancel remains the first action; keep sitting (character menu),
  Windows-login toggle, logs (tray), version and exit functions discoverable.
  No hunger, meters, tasks, streaks, rewards, currency, store or fake controls.
- Rectangular icon+label rows, fine borders and limited corner rounding; faint
  geometric texture only in chrome. Do not copy NIKKE logos, fonts or UI bitmaps.
- Keep native Windows menu keyboard/focus/dismissal semantics and actual state
  checks. Readability must survive desktop backgrounds, DPI and screen edges.
- Preserve transparent pet rendering, alpha hit testing, sharp head sampling,
  support physics and accepted motions; menu styling must not transform the pet.

Exact colors, type sizes, panel geometry and interaction animation are still
proposals, not measured source tokens. First proof: character context menu plus
tray equivalent in normal/snack-active/startup-on/off states, followed by visual
review before broad integration. The isolated proof below supersedes the earlier
research-only state; the later acceptance and integration below now govern menus.
Commit/push waits until the requested UI is built and checked, per user instruction.

## Draft A2 — 2026-10-14

- Proof: `artifacts/ui-menu-20261014/index.html`; screenshot
  `artifacts/ui-menu-20261014/captures-v2/0-full.png`. Local preview on port2814.
- Proposed, not source-measured tokens: white body, header #216cbd, primary #36d4f3,
  ink #243442; Malgun Gothic/Segoe UI; 288px menu, 62px snack, 44px secondary rows,
  54px two-line startup row. Fine layered border, 2–3px corners and header facets.
- One composition at wide/narrow widths. Character menu keeps sit; tray keeps logs
  and noninteractive version. Snack/cancel and startup on/off remain explicit.
  Character `Exit`→`종료` is proposed wording normalization, not a new action.
- Uses unmodified original96px character and existing jewel. The surrounding desktop
  is a labeled illustrative scene, not an actual desktop capture or new app screen.
- Scope review, rendered observations and corrections: [TASKS proof record](TASKS.md#nikke-menu-mockup-a--2026-10-14).
  Native menu semantics/DPI/edge placement, integration and user visual acceptance
  remain open. No full JOEDESIGN conformance/certification claim.

## Draft B2 — finish refinement, 2026-10-14

- User judged A2 similar but too raw and requested a more NIKKE-like finish.
  A2 is not accepted; its HTML is preserved as `artifacts/ui-menu-20261014/index-a2.html`.
- Reinspected primary settings and secondary confirmation original images. Refine
  the same compact composition: blue shell extends behind inset white panel,
  fine light/dark edge pairs, restrained facets, faint contained watermark, a subtle
  cyan button highlight and lower edge, and white switch thumb on colored track.
- Proposed local typography uses installed Pretendard (regular/semibold/bold), with
  Noto Sans KR/Malgun Gothic fallback. No game font or bitmap was copied. Header19px
  bold, action16px, row14px, startup12px and state11px; widths/actions unchanged.
  Font redistribution and native WPF rendering remain integration work, not approved
  by a browser preview using locally installed fonts.
- Current proof remains `artifacts/ui-menu-20261014/index.html` on port2814; final
  image `artifacts/ui-menu-20261014/captures-b2/0-full.png`. See [execution record](TASKS.md#nikke-menu-finish-b--2026-10-14).

## B2 implementation approval — 2026-10-14

- User: `ㅇㅇ 이 정도면 좋다. 이 시안으로 실제 메뉴에 적용 ㄱㄱ`.
  Approves B2 composition and native implementation, not yet the installed result.
- Shared WPF menu preserves the approved288-DIP panel, blue header, white inset,
  cyan snack/pale cancel, switch states and distinct character/tray commands.
  Character exit wording is normalized to `종료` as shown in B2.
- Unmodified Pretendard Regular/SemiBold/Bold is embedded from the official pinned
  source; SIL OFL notice travels in the package. No OS font installation or game
  UI/font asset reuse. Provenance: `src/Dororong.App/Assets/Fonts/README.md`.
- Current execution, comparison and remaining runtime boundaries: [TASKS](TASKS.md#native-b2-menu-integration--2026-10-14).

## Head brand icons — 2026-10-14

- User requested Doro's head for app, tray, menu and other product icons.
  This is a bounded brand-asset replacement, not a redesign of accepted B2.
- Reuse the existing isolated original head, now preserved unchanged as
  `src/Dororong.App/Assets/dororong-head-source.png`. Transparent aspect-fit exports
  replace the whole-body app icon and menu-header diamond. No face repainting.
- Snack, startup, logs and exit retain their distinct functional symbols; title,
  commands, state colors and B2 geometry remain unchanged. Product identity and
  executable name remain unchanged. Subsequent installed-version acceptance is
  recorded in [TASKS](TASKS.md#accepted-ui-and-snack-checkpoint--2026-10-14).
- Icon and tray-behavior evidence: [TASKS](TASKS.md#head-icons-and-tray-popup-stability--2026-10-14).

# Tasks

Direction, current stage and release completion gates live in [ROADMAP](ROADMAP.md).
This file remains the sole active execution/verification ledger; a finished task
does not close a roadmap stage with open requirements.

## Public preview release — 2026-10-15

- After being told that rights and other-PC verification remain unresolved, user
  explicitly requested updated instructions/download links, the latest installer
  release and public repository visibility. This supersedes the publication hold
  for this Preview only; it does not establish rights permission or close R3/R4.
- Prepare v0.1.3 Preview from unchanged accepted candidate14 (Setup SHA256
  `491EF2C3F5B1691C097CB5456CBF7C45D51E19C96140FDB86004D6A11DC94B97`).
  Update external bilingual instructions and direct versioned download links;
  retain unsigned/compatibility/asset-rights disclosures. Existing releases and
  Git history remain intact. No code, assets, installed app or settings changed.
- Updated docs committed as `90ec4722b4f1c0153205c3419a891939723aa713`;
  v0.1.3 tag points to that exact commit. App/asset sources are unchanged from
  the accepted checkpoint. Limited release hygiene found no case-sensitive known
  token/private-key signatures in reachable patch history; initial case-insensitive
  hits were embedded image data. This was not a comprehensive security audit.
- Published [v0.1.3 Preview](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/tag/v0.1.3)
  (release413338168) and changed repository1349611819 to public via its admin API.
  Exactly three assets: Setup, README and SHA256SUMS. Server digests matched local
  files before publication. v0.1.1/v0.1.2 assets and history were not changed.
- Anonymous requests (no authorization/cookies) confirmed public repository,
  release page HTTP200, README's direct v0.1.3 link and exact tag target. Downloaded
  all three assets; byte hashes and published checksums matched. Evidence:
  `artifacts/release-20261015/anonymous-verification.json`, checked 2026-10-15
  01:50:11 UTC. Setup is66016082 bytes. Release README SHA256:
  `1F53BB56CDB368D72EC8FBBECE8926EFBD2A633FC739BF2114BEB29EB59E5E2E`.
- R5/R6 Preview publication/download checks are complete. R3 and R4 remain
  unresolved; no rights approval, broad compatibility or stable-release claim.
  Next: rights evidence and other-environment validation when available.

## Login startup acceptance and private main checkpoint — 2026-10-15

- User confirmed `컴 켰는데 자동 실행 잘 된다. 이제 커밋 푸시하자`.
  This closes F1's actual-login acceptance on this PC and authorizes a private
  main checkpoint. Boot itself was observed by the user, not by the agent.
- Read-only corroboration: Run entry targets the installed EXE; live PID345128
  started at 10:28:26 +09:00 from that path. Installed App/Core hashes still match
  candidate14 recorded in [package evidence](docs/verification/2026-10-14-sleep-option.md).
  No startup setting change, rebuild, repeated test run or restart was needed.
- F3 installed-menu acceptance remains recorded below. Include its code/tests,
  concise usage docs and lifecycle evidence; exclude installers, backups and
  generated caches.
- Implementation checkpoint `de7ced1d6407354f7b2e04642e93ad111c1ec71e` pushed
  to private origin/main; remote hash matched and the working tree was clean.
  Staged whitespace and credential-signature checks passed. This documentation
  follow-up records that result without changing the tested application.
- Earlier real-login-pending statements are historical and superseded by this
  acceptance only. R3 other environments and R4 rights remain held; no release,
  tag or repository visibility change is authorized by this checkpoint.

## Automatic sleep option — 2026-10-14

- User requested a disable-lying option; implemented as `자동 눕기` in character
  and tray menus. Default ON preserves existing behavior. OFF blocks only the
  autonomous Sleep gate; sit/walk/snack/direct interaction remain independent.
- Per-user DWORD preference is loaded before the loop starts and saved/verified
  before live state changes. Missing preference defaults ON without writing;
  unreadable values/write failure report an error rather than fake success.
  Preference is user data retained across uninstall, separate from the Run entry.
- Re-enable starts a fresh inactivity delay. OFF wakes an existing Sleep in place.
  Independent review caught the missing Idle wake bridge and a frozen-core-phase
  edge; settled sleep now reuses existing two90ms wake steps timed by presentation
  elapsed, armed only by the setting action. Existing other wake behavior is kept.
- Core259 and initial full App1193 passed. After the narrow wake correction,
  final affected23 tests passed, including native both-menu states, corner placement,
  keyboard dismissal and wake completion while core Idle phase is frozen.
  TRX: `sleep-option-core`, `sleep-option-full`, `sleep-option-final-wake`.
  Initial targeted failure was a test fixture with unavailable pointer cancelling
  snack as designed; supplied a valid pointer and the preservation test passed.
- Inspected actual WPF character ON/tray OFF renders: original B2 roles retained,
  labels and switches unclipped. This is not installed-user acceptance or complete
  JOEDESIGN certification. [Visual scope](DESIGN.md#bounded-automatic-sleep-option--2026-10-14).
- User confirmed tray exit. Candidate14 package/native smoke and installer build
  passed; actual update verified470-file parity and preserved data/startup/shortcuts.
  Installed preference service OFF persisted into a separate process; ON and
  original absent value restored. Installed app relaunched; automatic sleep defaults
  ON, Windows startup stays OFF. [Local evidence](docs/verification/2026-10-14-sleep-option.md).
  Next: installed user checks the menu toggle; exact user acceptance is pending.
  No commit/push, public release, logout or restart authorized by this feature.
- Subsequent user acceptance: `ㅇㅇ 잘 된다` confirms the installed option.
  F3 is accepted. User deferred the real startup test until turning the computer
  on tomorrow (2026-10-15), not an immediate logout/reboot. Run entry was still
  absent at this check; advised enabling `Windows 로그인 시 실행` before shutdown
  and checking for the pet after login without launching it manually. No startup
  setting changed and no scheduled task, restart or commit/push performed.

## Startup and uninstall host validation — 2026-10-14

- User authorized finishing startup/uninstall checks, then confirmed tray exit.
  Verified the matching normal Stopped event before backing up the installation,
  user data, shortcut, registration and same-version recovery installer.
- On the current non-elevated Windows10 host, the installed startup service passed
  real registry ON/OFF/ON. The actual uninstaller removed its exact owned startup
  entry, installed root, registration and shortcuts while preserving user data.
- Reinstalled unchanged candidate13:470 payload files and ownership match, start
  shortcut restored, desktop preference and original startup OFF preserved.
  Installed executable launched successfully; original data bytes remain intact.
  [Execution evidence and method boundaries](docs/verification/2026-10-14-startup-lifecycle-host.md).
- F1's new-setting uninstall check is closed. Actual Windows logout/login is still
  untested: direct execution of the saved Run target is not a logon test. No logout,
  reboot, code/asset/package change, commit/push or release was performed.
  Next: coordinate a real login test after the user saves their open work.

## Accepted UI and snack checkpoint — 2026-10-14

- User confirmed `ㅇㅇ 이제 안 내려간다. 정리 완료하고 커밋 푸시하자`.
  This accepts the installed Figma-menu fix and authorizes the requested main
  checkpoint after cleanup; it does not waive other-PC/startup/rights boundaries.
- Candidate13 remains unchanged. Latest evidence: App1185/Core255, targeted81,
  package/installer checks,470-file installed parity and user confirmation.
  [Exact package and cleanup record](docs/verification/2026-10-14-figma-tray-support.md).
- Private repository and synchronized main confirmed before committing. Include
  current feature/UI/code/tests/docs plus preserved approved project records;
  exclude local installers, generated proofs, backups and build/test caches.
  No new public release, visibility change or tag.
- Implementation commit `de257270387c5a2ee58ea2bb8a67451ffb876d42` pushed to
  private origin/main; remote ref matched exactly. Current handoff checksums,
  staged path hygiene and credential-signature scan passed. The unmodified
  upstream OFL notice retains its original trailing space; authored-text whitespace
  checks passed. No application change after the recorded full-suite verification.
- This documentation follow-up closes F2's accepted local UI checkpoint. Remaining:
  F1 real login/uninstall checks, held R3 environment coverage and R4 rights.
  The subsequent host-validation entry above closes the uninstall portion only.

## Figma tray support and clean installer handoff — 2026-10-14

- User confirmed Doro's own tray popup now stays open and approved commit/push.
  They then requested cleanup first, and reported a separate Figma tray-menu fall.
  Cleanup/commit/push waits on this new regression; no commit or push has occurred.
- Live read-only metadata identified a Chromium popup overlapping the taskbar's
  top by12px, previously treated as a supporting/occluding ordinary window.
  Narrow style classification and process-independent transient-menu protection
  now preserve the taskbar while retaining ordinary-window occlusion.
- Reproduced3 old failures, then targeted81/Core255/full App1185 passed.
  Candidate13 packaged, installed and relaunched with470-file parity and preserved
  data/settings. Setup/README/checksums handoff is separate from historical outputs;
  no old files deleted or moved. [Evidence and cleanup scope](docs/verification/2026-10-14-figma-tray-support.md).
- Next: user checks Figma menu, then the previously approved private main commit/push.
  File/installer handoff cleanup is done. Public release remains held.
  Subsequent acceptance and main authorization are recorded above.

## Head icons and tray popup stability — 2026-10-14

- User requested head brand icons, then reported that actual tray right-click
  repeatedly opened/dismissed the menu and confirmed the app was closed.
- Existing isolated head reused unchanged; multi-size ICO and menu head resource
  applied to app/window/tray/header/installer brand surfaces. Functional action
  symbols and product identity remain unchanged. [Visual scope](DESIGN.md#head-brand-icons--2026-10-14).
- Removed the cancelled WinForms-opening detour. Left/right native releases now
  enter one guarded WPF opening route. The actual pet owner is activated before
  opening, not the nonactivating popup after capture. Direct command refresh,
  single callback dispatch and pending/open disposal remain intact.
- Independent scoped review found no production blocker; strengthened the outside
  activation test to accept natural dismissal before injecting a fallback message.
  Real user-session cause/reproduction remains an inference, not isolated-test proof.
- Full App1168 and final targeted41 checks passed. Tested/published/installed app
  hashes match; candidate12 installed with470-file parity and data/settings preserved.
  Startup remains off; installed app relaunched. Setup retains all10 icon sizes.
  [Evidence and remaining boundaries](docs/verification/2026-10-14-head-icon-tray.md).
  Next: user checks real tray right-click stability and head icons.
  No commit/push/public release in this step.
  Subsequent user confirmation: the popup no longer disappears; accepted for
  commit/push. Separate Figma-menu support report and ensuing hold are recorded above.

## Native B2 menu integration — 2026-10-14

- User accepted B2 and explicitly requested actual-menu integration. [Approval](DESIGN.md#b2-implementation-approval--2026-10-14).
- Shared `PetContextMenu`/`PetMenuResources.xaml` for character and tray. Existing
  NotifyIcon left/right routes now open the WPF surface through `StyledTrayMenuBridge`;
  command callbacks and startup-state checks remain on their existing paths.
- Duplicate/queued openings are guarded; disposal cancels queued work and closes
  the popup. Escape and command selection explicitly close the custom menu even
  without an owning Window's focus transfer. No pet artwork, sampling, motion,
  support or alpha-hit-test code changed in this UI integration.
- Initial25 targeted checks passed, including actual WPF popup creation on an
  isolated Windows desktop, four state combinations per menu, callback counts,
  repeated opening and open/pending disposal. Keyboard/activation messages here
  are injected test events, not physical mouse/keyboard evidence.
- Native96DPI render proofs: `artifacts/ui-menu-20261014/native-proof/`.
  Compared all8 states to accepted B2. Independent UI UX Pro Max-informed review
  found preserved composition/commands and no clipping; corrected its finding
  that the right-click test could be masked by subsequent left-click opening.
- Official pinned Pretendard fonts and OFL notice embedded/bundled; package test
  checks the license hash. Final App1166/Core255 passed; package/installer checks
  passed and candidate10 installed with470-file parity, preserved data/settings.
  Installed app launched. [Detailed evidence and limits](docs/verification/2026-10-14-native-menu.md).
- Actual-product user acceptance and physical input check remain: Computer Use
  cannot target this pet window. Mixed DPI/other-PC checks remain open. Next: user
  checks the installed menus before the requested commit/push. No public release.

## NIKKE menu finish B — 2026-10-14

- User requested more polished NIKKE-like finish after judging A2 raw. Reinspected
  actual settings/confirmation reference images; retained commands, state behavior,
  original art, compact menu composition and production boundary.
- Refined layered shell/white inset, borders/shadows, cyan button, typography,
  icon weight and startup switch. [Visual decisions](DESIGN.md#draft-b2--finish-refinement-2026-10-14).
- Preserved A2 HTML and screenshots. First B inspection found watermark extending
  beyond panel; contained it, corrected original PNG's transparent foot margin in
  the illustrative desktop, and increased title to19px bold for hierarchy/contrast.
- Final browser collector reran the seven affected render/state/keyboard targets:
  19 evidence entries, no configured assertion failure or page error, in
  `artifacts/ui-menu-20261014/captures-b2/`. Wide1280×920 and narrow390px observed.
  Source images/screenshots show finish differences, not exact game-token recovery.
- Independent Impeccable-informed finish review inspected final wide/narrow/focus
  captures and reported no P0–P2 blocker within that scope. It did not certify game
  similarity, full accessibility or native behavior; those limits remain explicit.
- Only mockup and records changed. Native app/installed files, commit/push and
  public distribution untouched. Exact visual approval and native DPI/focus/edge
  validation remained open at this draft step. Later B2 acceptance and native
  integration are recorded [above](#native-b2-menu-integration--2026-10-14).

## NIKKE menu mockup A — 2026-10-14

- User requested a visual draft. Produced isolated interactive HTML in
  `artifacts/ui-menu-20261014/index.html`, not production WPF/WinForms changes.
  [Design scope/tokens](DESIGN.md#draft-a2--2026-10-14). No install/commit/push.
- Independent preimplementation scope review checked current command mapping;
  pinned criteria and authority hash are in the artifact's `review-scope.md`.
- Actual wide1280×920 and narrow390px renders, snack/startup combinations on both
  menus, keyboard/Escape/reopen and close/exit simulation captured by the installed
  JOEDESIGN localhost collector. Seven target observations, no browser errors or
  failed configured assertions: `captures-v2/0-observation.json` through `6-observation.json`.
  This is a draft browser check, not native Windows menu validation.
- Measured in browser: menus288px, snack62px, secondary rows44/54px. Narrow has no
  horizontal overflow; pet bottom equals taskbar top. Original assets load unchanged.
- Impeccable C1 visual review identified stale captions after toggling and pet
  occlusion at narrow width; corrected both, plus narrow title/description wrapping
  and header focus contrast. Original captures retained; C1 confirmed fixes against
  `captures-v2/0-full.png`, `1-full.png`, `4-result.png`.
- UI UX Pro Max compact-control/focus guidance and current Web Interface Guidelines
  informed native buttons, named switches, visible keyboard focus and decorative-icon
  hiding. This does not certify exhaustive accessibility or native behavior.
- Fresh independent visual critic inspected final wide/narrow/changed-state/focus
  captures without builder verdicts. No P0–P2 blocker to showing this draft;
  small secondary version/state text remains a P3 observation for native DPI review.
- Remaining: exact user visual acceptance, production integration, Windows DPI/edge/
  focus testing. Full JOEDESIGN machine conformance/host certification not completed;
  unavailable OMD/Refero boundaries remain recorded in DESIGN. Next: user reviews A2,
  then approved UI implementation before the already requested commit/push sequence.

## Installed snack update and NIKKE UI direction — 2026-10-14

- 사용자가 수락된 후보08의 실제 설치본 업데이트를 승인했다.
  검증·백업 후 설치 경로에서 실행했다. [설치 근거](docs/verification/2026-10-14-snack-installed-update.md).
- 기존 자동 실행 켜짐을 유지하며 임시 후보 경로를 설치 경로로 옮겼다.
  실제 재로그인·제거 검증 완료로 보지는 않는다.
- 사용자는 UI를 니케 느낌으로 만든 뒤 커밋·푸시하도록 순서를 지정했다.
  직접 웹 레퍼런스를 찾으라는 후속 지시에 따라 로비·설정·팝업 원본을
  확인하고 [DRAFT 방향](DESIGN.md)을 기록했다. UI 코드는 아직 변경하지 않았다.
- 다음은 메뉴 대표 시안·검수·사용자 확인이다. 이번에는 커밋·푸시하지 않았다.

## Happy after eating — 2026-10-14

- 후속 피드백 `좀 더 천천히. 더 길게 흔-들 흔-들`에 따라 빠른 까딱 리듬을
  느린 연속 흔들림으로 변경했다. 원래 각도·외형·직접 중단은 유지한다.
  [느린 버전 검사·실행 근거](docs/verification/2026-10-13-snack-motion.md#slow-happy-sway--2026-10-14).
  영향 범위 검사·패키지 검증 후 후보08을 로컬 실행했다. 사용자가
  `ㅇㅇ 이 정도면 좋다. 이대로 가자`로 느린 식후 흔들림을 수락했다.
  이번 수락은 해당 동작에 한정한다. 수락 기록만 갱신하며 코드·실행본은 유지한다.
- 사용자 요청과 `ㄱㄱ` 승인에 따라 식후 폴짝 대신 감은 눈으로 좌우 고개를
  까딱하는 반응을 구현했다. [승인 범위](docs/specs/2026-10-13-snack-play.md#happy-after-eating--approved-2026-10-14).
- 기존 머리 보간·목 연결을 재사용하고 몸·발·원본 자산을 유지한다.
  자동 추적·사냥은 반응 종료까지 대기하며 직접 조작은 즉시 중단한다.
- 검사·연속 프레임·후보 실행 상태는 [이번 근거](docs/verification/2026-10-13-snack-motion.md#happy-after-eating--2026-10-14)에 둔다.
  전체 회귀·패키지 검사를 통과했고 종료 확인 후 후보07을 로컬 실행했다.
  사용자 시각 수락, 설치본 교체와 출시 게이트는 별개다. 커밋·푸시는 하지 않았다.

## Upright tracking interpolation — 2026-10-14

- 사용자가 후보05의 간식 두 문제에 `문제 없다`고 확인했다. 이어 평시 추적의
  얼굴 기울임 픽셀 떨림을 보고하고, 다시 흐려지지 않는 머리 보간 수정을 승인했다.
- 몸·원본을 유지한 한 번의 머리 회전 보간을 적용했다. 원인, 폐기한 실험,
  연속 프레임 비교와 검사 근거는 [실행 기록](docs/verification/2026-10-13-snack-motion.md#upright-tracking-interpolation--2026-10-14)에 둔다.
- 전체 회귀·패키지·로컬 후보 검증 후 후보06을 실행했다. 정확한 검사 수와
  경로·해시·실행 근거는 위 기록에 둔다. 이후 재실행한 후보06에 사용자가
  `ㅇㅇ 깔끔.`으로 추적 얼굴의 시각 수락을 확인했다. 설치본 교체,
  실제 로그인·제거 검사와 기존 출시 게이트는 별개로 유지한다.

## Snack entry and visible chewing — resumed 2026-10-14

- 사용자 `작업 시작` 지시로 아래 두 건의 보류를 해제하고 수정을 재개했다.
  설치본 교체·커밋·푸시·공개 배포 승인은 포함하지 않는다.
- 앉은 상태에서 간식을 시작할 때 창 지지를 잃는 경로를 재현·수정했다.
  원본 입의 세 번 씹기도 실제 재생 경로에서 확인하도록 검사했다.
  원인·수정·검사·실행본 근거는 [이번 실행 기록](docs/verification/2026-10-13-snack-motion.md#snack-entry-and-visible-chewing--2026-10-14)에 둔다.
- 당시에는 `커서 위치에서 시작`한다는 관찰 전체의 동일 재현과 사용자 수락이
  미확인이었다. 후속 수락과 새 추적 문제는 [다음 작업](#upright-tracking-interpolation--2026-10-14)에 기록했다.
- 전체 검사·패키지·실행 확인 후 후보05를 로컬 실행했다. 정확한 검사 수,
  경로·해시·실행 PID는 위 실행 기록에 둔다. 설치본은 교체하지 않았다.

## Snack candidate04 user findings — deferred 2026-10-14

이 보류는 [후속 작업 시작](#snack-entry-and-visible-chewing--resumed-2026-10-14)으로 해제했다. 아래는 당시 기록이다.

- 사용자 요청: 이번에는 기록만 한다. 내일 사용자가 작업 시작을 지시한 뒤
  재현·원인 조사·수정을 시작한다. 자동 예약이나 백그라운드 작업은 하지 않는다.
- 미해결 1 — 간식 주기를 처음 누르면 도로롱 위치가 마우스 커서에서 시작했다가
  작업표시줄로 갑자기 바뀐다. 자연스러운 이동이 아니라 순간 위치 변경처럼 보인다.
  사용자 관찰이며 정확한 재현 조건과 원인은 아직 확인하지 않았다.
- 미해결 2 — 먹는 중 옴뇸뇸 씹는 동작이 보이지 않고 입이 움직이지 않는다.
  사용자 관찰이며 렌더링·재생 경로 또는 움직임 크기 중 무엇이 원인인지는 미확인이다.
- 재개 시 확인: 첫 간식 시작의 위치 연속성, 실제 실행 화면에서 보이는 기존 입의
  씹기와 원복. 이전 자동 검사 통과를 이 두 문제의 해결·사용자 수락으로 보지 않는다.
- 이번 변경은 TASKS/ROADMAP 기록뿐이다. 코드·자산·실행본 변경, 테스트 실행,
  패키징·설치·커밋·푸시는 하지 않았다. F1 사용자 수락은 미완으로 유지한다.

## Shared source and body-only breathing — 2026-10-14

- User approved the shared-source correction. Unified neutral body endpoints,
  native walking/head parts, upright tracking and bounded torso breathing;
  preserved original assets and the accepted sitting bank. Scope and evidence:
  [current correction](docs/verification/2026-10-13-snack-motion.md#shared-source-correction--2026-10-14).
- Controlled comparison now has zero neutral body/contour differences, including
  actual post-walk idle. Latest full App1134 and Core252 passed. Portable/hash
  parity/native lifecycle validation passed; launched shared-source candidate04
  after confirming no existing process. Exact package/log evidence is linked above.
- Follow-up user findings and the current resumption boundary are recorded in
  [the deferred review](#snack-candidate04-user-findings--deferred-2026-10-14). Existing
  installer/login/uninstall, environment and rights boundaries remain separate.

## Snack motion correction — 2026-10-13

- Latest user review reports cross-state blur/body mismatch. Controlled native
  comparisons confirmed separate initial-idle and post-walk-idle body sources,
  tracking ink reconstruction differences, and prefiltered active walking heads.
  Actual presenter walk->idle reproduces the neutral mismatch. Current findings
  and a shared-source/single-sampling correction proposal are recorded at the top
  of [motion evidence](docs/verification/2026-10-13-snack-motion.md). Diagnostic
  tool/records only in this turn; no new app build/replacement or visual acceptance.
- Follow-up user review rejected landing paw reach, airborne cursor corrections
  and redrawn chewing lips. Corrected these with planted landing, captured/eased
  flight and source-mouth deformation. Latest Core252/affected App163 passed.
  Diagnosed whole-sprite breathing scaling with a controlled actual-presenter
  A/B; proposed head-fixed/body-only breathing, but did not change that behavior.
  New candidate package/parity/native smoke passed and the candidate was launched
  after confirmed tray exit. Current follow-up evidence and acceptance status are at the
  top of [motion evidence](docs/verification/2026-10-13-snack-motion.md).
- User rejected the first snack candidate and requested sharp rendering, a new
  quadruped run, hunting-inspired jump, reference-based mouth/chewing, exclusive
  eating control and smooth hunting recovery. [Approved correction scope](docs/specs/2026-10-13-snack-play.md#user-motion-correction--2026-10-13).
- Implemented native local compositing, source-based asymmetric run, phased
  jump, open/accept/closed-lobe chewing plus settling, and authored hunting
  recovery without continuing launch dwell. Existing artwork and old installed
  package remain unchanged; no commit/push/publication.
- Core250 and affected App321 passed. Causes, initial fixture failures, visual
  exports, benchmark and remaining checks have one owner:
  [motion evidence](docs/verification/2026-10-13-snack-motion.md).
- Full latest App1117 passed; publisher/package/hash-parity/native smoke passed.
  After the user's confirmed tray exit, launched the new isolated motion candidate.
  Exact candidate/hash and proof files live in the evidence above. Next: user
  visual acceptance; installer/login/uninstall checks remain separate pending work.

## Snack play and login startup — 2026-10-13

- User explicitly approved the menu/jewel/chase/jump/stationary-offer/closed-eye
  eating proposal plus login startup. Bounded behavior and visual acceptance
  live in [the feature spec](docs/specs/2026-10-13-snack-play.md), not the old
  numeric care or Backspace-trigger proposals. F1 is independent of R3/R4 holds.
- Added a pure `SnackSession`, exclusive PetLoop ownership, local mouth/drool
  rendering, click-through jewel companion, both menu entry points, optional
  quoted HKCU startup registration and exact-command uninstall cleanup.
- Initial targeted jump integration caught the existing 12-DIP supported-hop
  clamp. Added a bounded per-call lift limit for snack play (76); the old default
  stays 12. The fixed targeted suite passed 12/12, including actual platform
  landing. Initial debug suites passed Core248/App1105 (before later render
  caching and final menu test); these are not claimed as latest-binary evidence.
- Inspected the actual WPF chewing/drool frame strip on light/dark backgrounds:
  `artifacts/snack-20261013/frames.png`. Eye closure uses existing art. The
  interior mouth changes, while a raster assertion preserves eating-frame alpha
  contours. This is renderer inspection, not desktop capture or user acceptance.
- Measured uncached Release snack rendering: median11.96ms/p95 19.55ms on this
  host during regression testing. Added bounded reusable base-frame caching;
  post-change timing and final tests remain pending at this entry.
- V2 definition bootstrap was attempted but failed surface/coverage validation.
  The unapproved attempt is preserved as `product-definition/dororong-snack-play/state.candidate.json`,
  not canonical state. No Semantic Closure, generated-manifest approval, or
  completed JOEFLOW planning delivery is claimed. Current explicit implementation
  authorization and existing project document ownership remain distinct.
- Final verification/candidate and exact remaining boundaries:
  [local evidence](docs/verification/2026-10-13-snack-play.md). Release Core248,
  App1111 and latest affected-scope176 passed; actual window start/cancel/close
  and published-portable hash/native smoke passed. The after-cache results are
  kept separate from the earlier full suite, not added into a misleading total.
- Prepared and launched the isolated feature candidate for user testing. No
  registry Run entry was changed by automated tests; no installed-app replacement,
  public release, commit or push. Next: user play/visual acceptance, then separately
  validated installed update/login/uninstall paths. Formal V2 bootstrap remains
  incomplete as described above, not claimed to pass by the runtime checks.

## R3 environment hold — authorized 2026-10-05


- Applied the user's explicit hold decision to [ROADMAP](ROADMAP.md): R3 status,
  current stage and eligible next work changed together. The reason and resume
  condition live in its R3 row, not in a second issue ledger.
- Preserved required checks, R1/R2 completion evidence, R4 rights gate and R5/R6
  dependencies. Hold is neither completion nor a waiver; the existing independent
  R4 work remains eligible. Earlier R3-next statements are dated history;
  ROADMAP owns the current status and next work.
- JOENESS review: approval → R3 criterion/hold → this execution receipt → current
  next work is connected; links/headings and diff checked. Only ROADMAP and this
  ledger changed. No application tests, setup/package changes, new chat, email,
  publication, commit or push in this task.

## JOENESS 0.3.2 project connection — approved 2026-10-05

- User approved the proposed connection refresh in this chat. Official fresh
  Check → Apply → Check returned current/clean. Updated only the owned AGENTS
  block and its setup baseline; this entry is the execution receipt.
- Exact owner sections now distinguish product/visual approval, execution proof,
  roadmap status and rights issues. Workers update affected task/roadmap records
  together; approved goals/order/criteria stay separate from factual updates.
- Traced [R2](ROADMAP.md) criteria/status against the existing
  [host lifecycle evidence](docs/verification/2026-10-02-security-lifecycle-host.md).
  No application checks were rerun. No release status changed, so ROADMAP stays
  unchanged; its next work remains R3, with R4 independently open.
- Fresh-chat instruction delivery remains unverified; no new chat was created.
  Existing outside instructions, history and prior edits are preserved. No
  personal-package reinstall, app/assets/deployment changes, commit or push.

## Portfolio media and implementation handoff — 2026-10-03

- Prepared the requested local portfolio handoff at
  `artifacts/portfolio-handoff-20261003/HANDOFF.md`, focusing on Windows app
  behavior/implementation rather than ownership of the original character design.
  No message was sent back to another thread; the requester will retrieve it.
- Reused two visually inspected actual desktop crops and three renderer stills,
  plus the existing September30 29-second renderer video. Six media copies are
  byte-identical to originals. No new capture, redraw, text overlay or app launch.
  Handoff distinguishes staged renderer media from OS capture, omitted newer
  features, local0.1.3/source parity boundaries and preserved older0.1.2.
- `review-code.zip` contains31 selected source/test/license files from
  HEAD1765c8706d334da6e0001a001953021ffa12dcd3, not a standalone build.
  Fresh Git diff confirms included sources unchanged. Initial raw ZIP parity
  failed because core.autocrlf exports uniformCRLF from mixed working endings;
  a minimal comparison confirms identical text after normalizing both sides.
  Corrected artifact checker verifies exact content moduloCRLF/LF, preserving
  separate raw/normalized hashes rather than claiming raw-byte code equality.
- Handoff checker PASS: six media hash matches and31 source-content matches;
  manifest records provenance/classification/dimensions and SHA256. Inspected
  October2 historical TRX counters Core241/App1099; no new app suite or runtime
  acceptance run. Windows11/other-PC coverage and character/public-use rights
  remain unresolved. No public asset upload, repository visibility change,
  release, email, source/installer change, commit or push in this task.

## Rights inquiry follow-up with media — 2026-10-03

- User approved the revised polite follow-up and three existing media files.
  Sent to the official inquiry address as a reply in the September21 thread,
  with the approved real-name signature followed by the activity name.
- Fresh Gmail read-back verifies SENT, recipient, original-thread reply context,
  exact approved new-body prefix, and all three attachment names/types/sizes.
  The connector also quotes the original inquiry after the new body; no duplicate
  send was performed. Delivery, reading and publication permission are not proven.
- Attachments: two actual desktop screenshots from
  `artifacts/community-media-20260930/desktop-captures/opaque-cursor/`
  (`05-perched.png`, `03-head-with-cursor.png`) and the existing29-second
  `complete-demo/dororong-demo-no-text.mp4` under the same media root.
  The approved text identifies the video as an app-renderer demonstration,
  not an OS recording. No executable or account/chat screenshot attached.
- Rights status remains unresolved; see docs/RIGHTS-REVIEW.md. Only that document
  and this ledger were updated for this action. Earlier README edits preserved;
  no source/app/installer changes, commit, push, visibility change or release.

## README clarification and publication scope — 2026-10-02

- User authorized the three README clarifications identified in review: correct
  Korean character-menu Exit text, describe both tray buttons for main/v0.1.3
  while distinguishing v0.1.2, and explain authorized GitHub sign-in for the
  preserved private-release download. Only README and this ledger changed.
- User also expressed interest in publication. Publication scope/decision is not
  confirmed; existing R3/R4 gates remain open. Official SHIFT UP guidelines and
  GitHub visibility consequences were rechecked; no new asset permission was
  established. No repository visibility change, release or code-only export.
- Documentation wording/diff and relative local links checked. No product code,
  binaries, installed app or historical files changed; no new runtime-test claim.
  These documentation edits have not been committed or pushed in this step.

## Security and lifecycle — private main checkpoint — 2026-10-02

- User chose to commit/push the current work after checking Windows11 test
  options. Current host is Windows10 Pro; firmware virtualization is disabled
  and no ready Windows11 environment was established. No setup/settings change.
  R3 remains unverified, not waived; rights/public-release gates remain open.
- Scope: preview URL guard, installer incoming ownership/collision guard,
  regression tests and audit/lifecycle documentation. Generated installers,
  backups, personal logs and test outputs stay ignored. No tag, release or
  repository visibility change. Repository id1349611819 verified private/main;
  fetched origin/main matched starting HEAD5e65307, without divergence.
- Fresh pre-commit suites: Core241/App1099, zero failures/skips, and Node41/41.
  Installer payload, build-refusal and compiled shared-policy checks pass with
  explicitly pinned Inno7.1.0. Current installed Setup hash remains unchanged.
  TRX files: artifacts/verification/security-checkpoint-20261002/. Diff checks
  and credential-pattern scan pass. Remote commit equality/clean working tree
  are checked after the normal push and reported in the completion handoff.
- Fresh rebuilding changes App/Core DLL hashes relative to the frozen candidate.
  Core informational version now records5e65307 instead of67f7504; Core method
  bodies match in the read-only PE probe. App managed resources match, but the
  candidate includes six WPF GeneratedInternalTypeHelper methods absent from
  the rebuild; no App source files changed. Exact binary equivalence is not
  claimed. Existing candidate/install/backup bytes are not replaced, nor is
  package validation rerun against these new reference outputs. R5 requires a
  freshly frozen/tested package and explicit archive pin before distribution.

## Hardened installer — current-host lifecycle — 2026-10-02

- User authorized the recommended next lifecycle verification. Scope: prepare a
  fresh local candidate, verified recovery backups, same-version update, normal
  exit, uninstall/reinstall and existing-data preservation. No commit/push/release.
- New candidate `candidate-20261002-security-lifecycle-01` builds the current
  incoming collision guard around the unchanged accepted469-file0.1.3 app.
  Exact pinned archive/compiler/source/generated parity and fresh package/native
  validation passed. Setup remains unsigned; previous candidates stay intact.
- Verified full472-file installation, one user log, Start shortcut and uninstall
  registration backups plus pinned recovery Setup before changing the host.
  Non-elevated same-version update exit0; payload, data and shortcut preference
  checks pass. Installed start/duplicate pass, current PID57080.
- User tray exit confirmed; matching Started/Stopped events and absent process
  checked. Verified new uninstaller exit0; installation root/registration/
  shortcuts removed, post-exit user log unchanged. Reinstall exit0, all469
  payload/ownership hashes correct; registration/Start shortcut restored and
  desktop opt-out/log preserved. No forced process termination or root deletion.
- Reinstalled launch/duplicate pass, installed PID1240 left running; actual
  executable and loaded bundled runtime paths/hash verified, original log prefix
  preserved. Re-ran Node41/41 with zero failures/skips. App source unchanged;
  no redundant App/Core rebuild. Full prior Core241/App1099 is not a new run.
- R2 current-host basic lifecycle is complete; R3 other environment and R4
  rights remain open. Recovery copies preserved, not exercised. This does not
  prove all failure paths, clean-PC compatibility or visual acceptance. Evidence:
  [current-host report](docs/verification/2026-10-02-security-lifecycle-host.md).

## Preview request and installer collision fixes — 2026-10-02

- User authorized both follow-up fixes, without commit/push/publication or host
  installation. Prior audit record and sealed scan remain unchanged.
- Three loopback previews now catch only URL parse failure and return400;
  normal routes, query/absolute HTTP targets, MIME types,404 and asset-error500
  behavior preserved. Added tests/preview-servers.test.cjs: real Node parser
  and response on in-memory Duplex, no listener or existing preview contacted.
  Before patch9 malformed cases failed with ERR_INVALID_URL/15 controls passed;
  after patch30 preview cases pass, full node --test41/41 passes.
- Shared PreflightIncomingFiles checks safe/access/reparse/type policy and
  refuses occupied incoming leaves absent from previous case-insensitive file
  ownership. AllPreflight rechecks it at initialize, prepare and ssInstall.
  Extended compiled PolicyHarness covers collision/case variants, user-byte
  preservation, owned update/missing repair, fresh and nested absent files,
  directory collisions and reintroduced filenames. Policy PASS; a temporary
  accessibility-only mutation fails 'unowned incoming collision refused'.
- In isolated temporary source copy: full Core241/App1099 pass, zero failures
  or skips; Node41 pass. All169 product/patch files hash-match the tested copy.
  Changed JS syntax and git diff --check pass (Git LF/CRLF notices only).
  Full Inno7.1.0 installer script compiles with historical frozen staging inputs;
  output is test-only compile evidence, not a new distributable installer.
- Local Codex Security target-bound artifacts retain fix_report.md, Node log,
  policy GREEN/mutation RED logs and both TRX files. Fresh independent boundary
  investigation and candidate review found no confirmed in-scope regression.
- Existing preflight/copy race against unrelated concurrent same-user writers
  remains; absent paths are not reserved. No current-CVE or host lifecycle/
  other-PC/rights validation added. Existing Setup/installed app unchanged;
  already-running developer previews need restart to load this source fix.
  R2/R3/R4 remain open. No code delivery or release-gate closure claimed.

## Standard source security audit — 2026-10-02

- User authorized a standard whole-repository audit, not remediation or delivery.
  Audited immutable revision `5e6530749ac82daacc1612c177dddb2964c78a2d`.
  Codex Security scan `624c9846-a023-4f75-b735-a7205fcbce7c` completed and sealed;
  its local workbench retains report.md, findings.json, coverage.json and evidence.
- All442 executable-source/project/markup/PSD1 paths fully reviewed;453 unique
  files reviewed overall from689 tracked files. Remaining236 art, large literal
  data and documentation files explicitly classified/excluded in coverage.
- One validated LOW finding: malformed absolute HTTP request targets can throw
  out of unguarded URL parsing and stop three optional loopback developer previews
  (`tools/PreviewLocomotion/server.cjs:21`, `serve-pounce.cjs:10`,
  `tools/CheekSpringPreview/server.cjs:4`). Exact callbacks and the native Node
  v26.3.0 HTTP parser were checked with mocks/in-memory Duplex, without sockets.
  Installed pet impact, disclosure, code execution or elevation not established.
- Separate nonsecurity data-preservation concern: a newly added payload filename
  may overwrite an unowned same-name user file during registered installer
  upgrade (`installer/Dororong.iss:133-158,190-191`). No lower-authority attack
  path established. Proposed follow-ups, not implemented: guarded URL parsing
  returning400, and refusal of incoming paths absent from previous ownership.
- No application, live preview, installer, build or full test suite executed.
  Current CVE advisories/third-party implementations, host ACLs, installed binary
  parity and the ignored October2 builder remain outside this source audit.
  No safety certification, current test-pass claim or release-gate closure.
- Source stayed clean through scan sealing. This subsequent ledger entry is the
  only repository edit; no code fixes, commit, push or publication performed.
  R2/R3 installation/environment and R4 rights gates remain unchanged.

## Accepted local v0.1.3 — main checkpoint — 2026-10-02

- User confirmed the installed pet remains visible in front of Clipchamp, then
  explicitly requested committing and pushing the current accepted work.
  Scope: private main source checkpoint, not a tag, release or visibility change.
- Include v0.1.3 preparation, head/cheek sharpness, accepted complete-part sit/rise,
  ordinary-window recovery, regression tests, source/export tools and project
  documentation. Historical motion studies remain labelled as such. Generated
  installers, captures, videos, local backups and test outputs stay ignored.
- Remote repository identity/visibility verified: JOEWRKS/Doros-on-my-desktop,
  id1349611819, private, default branch main. Fetch showed no divergence.
- Fresh pre-commit validation: Core241/App1099 passed, zero failures/skips
  (`artifacts/window-layer-20261002/test-results/precommit-full.trx`); accepted
  renderer/bank checks7/7, installer payload/refusal/compiled-policy checks,
  syntax checks for53 changed JS/CJS files and staged diff checks passed.
  No credential-pattern matches or generated EXE/ZIP/video/backup files in
  the100-file source checkpoint. The user-authorized delivery is a normal push
  to private main, with remote commit equality checked in the completion handoff.
  Publication rights and other-environment acceptance remain unresolved gates.

## Ordinary-window layer recovery — 2026-10-02

- User authorized fixing the pet being obscured by Clipchamp. Two live native
  observations found Clipchamp above the pet despite the pet retaining
  WS_EX_TOPMOST. The original triggering event is not established. Existing
  repair only covered overlapping taskbars during perch-ready/attached states.
- Added a general layer guard after rendering, checked at most once per250ms.
  It repairs a lost native topmost flag or an overlapping ordinary window above
  the pet, retaining position, size and focus. Other topmost windows do not
  trigger this repair; the existing taskbar/perching guard remains unchanged.
- Regression tests first failed on the absent general host callback. Native
  topmost-loss recovery, no repeated reorder, bounded recheck, other-topmost
  coexistence, runtime wiring and existing perch cases passed9/9. The live
  stale-flag state is not claimed as a deterministic automated reproduction:
  Windows ignored the attempted test-only direct style-bit simulation.
- Core241 and full App1099 passed, zero failures/skips; TRX retained in
  `artifacts/window-layer-20261002/test-results/window-layer-full.trx`.
  Fresh candidate `candidate-20261002-window-layer-01` passed tested-DLL parity,
  runtime8.0.31/archive verification and isolated native startup/duplicate/close
  smoke. ZIP SHA256 `F31BE3457B0DA75E7743479E2724117A57017A2AFD65D864DA009AB562A6AF7A`;
  Setup SHA256 `A2022DA21B4076080FDB9FCEE3CA174BCED82C4E54FF115CF3E5870A845370EB`.
- User confirmed normal tray exit. Prior installed copy, user data and registry
  backup retained under `artifacts/window-layer-20261002/`. Normal installer
  update exited0, all469 payload files matched, data/shortcut preferences
  preserved. Installed PID30444 started; duplicate exited0. No Git/public release.
- Activated the existing Clipchamp window using Computer Use. All12 half-second
  live native samples found petZ21 before ClipchampZ34, zero overlapping ordinary
  windows above the pet; foreground stayed Clipchamp HWND2885436 throughout.
  Evidence: `artifacts/window-layer-20261002/clipchamp-layer-samples.json`.
  Window capture timed out (`FrameArrived timed out`); subsequent accessibility
  observation returned no usable tree. The user subsequently confirmed the pet
  is visible in front of Clipchamp. Agent-side screenshot and maximize/restore
  verification are NOT claimed. Initial trigger of stale style/order still unknown.

## Accepted reference sitting — local product update — 2026-10-02

- User explicitly requested updating the application with the accepted sitting
  artwork and single-ink line weight. Scope: local app/package update, no Git,
  public release or change to the outstanding rights/environment gates.
- Backed up the prior locomotion bank and manifest in
  `artifacts/reference-product-20261002/`. Exporter now bakes all130 open/closed
  sit frames directly at native96 from `reference-parts.cjs`; rise uses the same
  part trajectory in reverse to share both endpoints. The512 walking frames
  remain byte-identical. Sit duration is1050ms, matching the accepted preview;
  existing rise duration650ms and interruption ownership are retained.
- New reference-byte equality and reverse-path tests failed on the previous
  bank, and the timing test failed on the prior900ms duration. After integration,
  bank checks4/4 (including walking preservation), renderer/bank checks7/7,
  native interaction/timing checks21/21 and Core241/241 passed. Two existing
  manual-sit tests assumed completion after1s; updated their waits to1.1s for
  the now1050ms motion. The first full run caught two more identical assumptions
  in supported-hunt sitting and seated-blink entry (1094pass/2fail); both were
  corrected, focused4/4 passed, then final full App **1096/1096 passed**.
- Published a fresh local candidate `candidate-20261002-reference-sit-01`.
  Exact tested DLL parity and package/runtime/isolated-startup/single-instance
  checks passed. ZIP SHA256:
  `F4BC1B90776BBFB3262BF2F58A6347428ECC46EB09AB540FF4C2C01445BA5B38`.
  Preparing an artifact-only local installer builder with this explicit pin;
  tracked public-builder pins and old candidates remain unchanged. Official
  Inno7.1.0 portable compiler was restored using the existing verified tool;
  its pinned hash and valid signature match.
- Built local Setup0.1.3; compiler/validator exit0,469 staged files matched.
  Setup SHA256 `A4D0DB06BCAB134F63A74E0B10BD09DD1B6A8544CC4F64E91F3CF7CC08ABAD46`.
  Retained compiler/payload/signature guards; code signing remains intentionally
  absent. Backed up472 installed files, user data, shortcut and registration.
  Normal per-user in-place upgrade exited0; all469 installed payload hashes,
  ownership manifest and0.1.3 registration verified. User data remained exact,
  desktop shortcut preference and Start shortcut were preserved. Evidence:
  `artifacts/reference-product-20261002/install-baseline.json`,
  `installed-verified.json`, `install.log`; prior binary copies are in
  `installed-backup/` and data in `data-backup/`. No uninstall was performed.
- Installed executable launched as PID4020; duplicate exited0 and only the
  installed process remained. Launch evidence is `launch-verified.json`.
  This is current-host upgrade/startup evidence, not uninstall/reinstall,
  clean-machine acceptance, public distribution or permission from the IP owner.
- Actual WPF-rendered sitting and native white/black bank comparisons inspected.
  Raw bank SHA256: `fad5142a6dac4b2d0bb2dae576b6c1b7ccc893d85468dc97e5e9d4cc1d286c82`.

## Source-aligned complete parts and anatomical sit — local implementation — 2026-10-01

- Latest user accepted the outline continuity level, then requested source-like
  body line weight. The new optical-width regression failed before the change:
  source rump1.2954px-equivalent vs additive-rendered rump1.7085px-equivalent.
  The normal part ink and exposed-border repair were source-over composited
  twice, making their AA pixels darker. They now contribute maximum coverage
  to a single ink mask, composited once; stroke width/color come from source
  measurement. Depth/socket clipping, all part shapes and motion are unchanged.
- Fresh source comparison: rendered rump1.3539px-equivalent vs source1.2954
  (same exposed-rump optical measurement, not a whole-body pixel identity).
  Focused suite **25/25 passed**, including source-width checks,240-frame
  perimeter checks and121-pose rig/skin checks. Core **241/241 passed** and
  App **1096/1096 passed** with the documented Release/win-x64 test commands.
  Inspected all64 moving poses on white/dark; regenerated/decode-checked the
  caption-free4s60fps preview. Prior thick-ink evidence is preserved under
  `artifacts/reference-ink-20261001/before/`; current audit is
  `artifacts/reference-parts-20261001/outline-audit-single-ink/`.
  Both reference PNG hashes are unchanged. No installed-product/Git/release
  mutation or final visual acceptance is implied by this line-weight correction.
- Follow-up: user reported intermediate outline interruptions. Inspected all64
  distinct playback poses (video frames33–96; other frames hold the endpoints)
  individually on white, then all64 corrected poses on white/dark. Found exposed
  foreleg/belly boundaries missing ink during placement/reach/drop: the selective
  ink paths did not cover newly exposed root edges and layered white faces
  erased neighbouring edge strokes. Added a240-frame perimeter-ink regression;
  it failed before correction at video frame46, native pixel47,77.
- Corrected exterior ink ownership: stroke each unchanged closed part curve
  after layered painting, sequentially clipping against every other skin face.
  The sequential clips exclude the union even at triple overlaps. Original
  artwork paths, joint timing, head pixels and source/reference PNG hashes are
  unchanged; no composite raster tracing, geometry inflation or image morph.
  Cached per-pose paths avoid repeating skinning for the outline clips.
- Fresh focused checks **24/24 passed**, including the240-frame ink regression
  and121-pose attachment/rig checks. Regenerated the preview and comparisons;
  decoded all240 MP4 frames successfully (4s,60fps,576x288, no captions/audio).
  Evidence: `artifacts/reference-parts-20261001/outline-audit-before/`,
  `outline-audit-after/`, `outline-before-after.png`, `sit-preview.mp4`.
  This fixes the observed local preview gaps; native integration, installer,
  release, Git and user visual acceptance remain unchanged/not claimed.
- Current explicit user instruction: implement the proposed complete-part
  artwork and joint solution. No old chat reply is treated as present acceptance.
- Added `reference-parts.cjs`: closed, authored chest/haunch/three visible limb
  paths. Visible curves follow the source; hidden neck, shoulder and thigh skin
  exists in the parts themselves. Original head texture retained. Rump and chest
  are separate rigid shapes; each leg has two fixed-length segments, no ankle.
  The far/near forepaw contours have explicit small seated-view corrections;
  these do not scale bones or the torso. Seated PNG is comparison-only here.
- Implemented support placement → hindfoot lift/reach → late hip drop → small
  settle. Forefoot positions are fixed after support placement. Articulated
  curve points are sampled before skinning; transforming Bezier handles had
  made fill and ink follow different curves at a bend and was corrected.
- Named vector faces use one same-geometry nonzero fill beneath normal layered
  painting to avoid overlapping AA cracks. There is no raster-derived outline,
  largest-component selection, bridging strip or post-render hole repair in
  this renderer. Hidden root ink is clipped against its named covering part;
  exposed outer-knee ink remains visible when the knee protrudes below the rump.
- Tightened connection checks found an actual late-drop underlap gap between
  the foreleg and haunch. Completed the hidden left haunch surface to overlap
  under the foreleg. Exterior AA classification uses eight-neighbour background
  connectivity; body opacity remains >=250, not merely >0.
- Fresh focused suite **23/23 passed**, including two new tests sweeping121
  sit poses for fixed bone lengths, planted forefeet, rigid chest/haunch geometry,
  opaque attachment samples and no enclosed underfilled body pixels. Individual
  limb artwork covers its own root/knee before composition. These are technical
  properties, not user visual acceptance or proof of naturalness.
- Exported `artifacts/reference-parts-20261001/`: four poses, source comparison,
  overlays, colour-coded depth, joint overlay, complete part PNG/SVGs, rig JSON,
  240 rendered frames and a4-second60fps caption-free `sit-preview.mp4`.
  Inspected native-derived white/dark composites and part/depth sheets. Source
  and reference PNGs unchanged. Native app/bank, installer and Git untouched.
- Current handoff is reviewable local artwork/motion implementation. The endpoint
  is source-aligned rather than RGBA-identical; visual acceptance is not claimed.
  Product integration, rise motion and arbitrary other actions are not completed
  by this sit renderer. Prior cutout/whole-outline experiments remain historical.

## Original cutout joint coverage — rejected approach / historical bounded experiment — 2026-10-01

- User asked whether cut pieces remain seamless under rotation and suggested
  tracing the original if they do not. Static pixel equality does not prove this.
- Reproduced gaps in bare rotated parts. Hidden socket polygons alone covered
  pivots but failed full cut-boundary probes at the neck edge and between forelegs.
  Bound small connecting skin strips to both sides of original cut boundaries.
- Visual inspection then found broken source ink despite covered gaps. Added
  a source-derived continuous exterior trace and a separate seated thigh crease;
  retained original head texture. A subsequent test found that the crease could
  protrude outside skin and enclose transparent air; clipped it to the skin path.
- Fresh scoped checks: **21/21 passed** (the previous19 plus two pose sweeps).
  Each pose samples81 combinations: head -8/0/8°, forelegs -12/0/12° independently,
  hindfoot -18/0/18°. Interior socket probes are opaque; cut-boundary probes have
  coverage; no enclosed transparent body pixels in the tested body region.
  Rotated exterior endpoints allow AA rather than requiring opacity at the edge.
- Static cutouts still reconstruct original RGBA exactly. Traced opaque skin
  is **not** pixel-identical: source seated paw ink contains partial-alpha gaps;
  rest silhouette maximum deviation is bounded to2 native pixels. White matte
  of hidden backing alone differs by at most1 channel level. No shape acceptance
  inferred. Inspected white/dark angle and before/after sheets at
  `artifacts/source-cut-joints-20261001/`.
- This is a bounded joint/ink diagnostic, not an all-angle guarantee or finished
  standing-to-sitting transition. Large bends, root translations, temporal motion
  and product integration remain unverified. No installation/Git action.

## Artwork repair — original image cutouts — 2026-10-01

- The user explicitly rejected treating old “ㄱㄱ” messages as current approval.
  The traced static artwork has **not been accepted**. Historical approval
  interpretations below must not authorize further work or animation.
- Current instruction: fix the artwork; cutting the previous original images
  is explicitly allowed. Replaced the normal static exporter with cropped
  original standing/seated layers. The seated image is now actual source art,
  superseding the previous experiment's reference-only restriction.
- Root cause: the traced torso moved chest/pelvis curve points independently;
  its seated rump/floor and leg connections diverged from the source. The
  standing redraw also changed 730 decoded pixels in the regression's RED run.
- Exported five cropped layers per pose, native transparent endpoints, native/3x
  white/dark sheets and placement manifest to
  `artifacts/source-cut-artwork-20261001/`. Normal `export-original-contour.cjs`
  now exports these; `--legacy-contour` retains the rejected diagnostic explicitly.
- Fresh verification: **19/19 focused checks pass**. Both reconstructed poses
  have **0 differing source RGBA pixels**; separately saved cropped PNGs also
  reconstruct exactly, with one owner per visible pixel. White/dark sheets and
  isolated parts inspected. Both source hashes unchanged. Product tests not
  rerun: no native application or bank code changed in this repair.
- This fixes the static silhouette by retaining the original artwork. Cropped
  exposed pieces do not supply hidden joint surfaces; articulated transitions
  remain unfinished and are not represented as repaired by exact reassembly.
  No animation, installation, Git action or user visual acceptance claimed.

## Original-contour artwork redesign — rejected static study — 2026-10-01

Historical process record below; its approval interpretations are superseded
by the user's correction above, not evidence of present approval.

- [x] User approved the proposed original-contour complete artwork + joint
  approach with “ㄱㄱ”. Per the architectural brainstorming path, wrote
  [design](docs/superpowers/specs/2026-10-01-original-contour-sitting-design.md).
  First implementation milestone is standing/seated static visual comparison,
  not animation, installation or product integration. New body artwork is
  source-proportioned, not required to be byte-identical to old body AA pixels;
  that distinction is explicit in the proposed design for user review.
- [x] Self-reviewed scope, artwork/pose/compositor separation, closed hidden
  parts, reference-only seated raster, line ownership, visual acceptance,
  unchanged head texture, and no automatic product/Git/release operations.
  No implementation or generated artwork in this design-writing step. Source
  and application tests were not rerun; document creation is not a fix.
- [x] Follow-up “ㄱㄱ” approved the written design. Created and self-reviewed
  [static implementation plan](docs/superpowers/plans/2026-10-01-original-contour-static.md):
  original measurements/head, complete parts, endpoint poses/compositing,
  static comparison export. Five failure modes have owner-task test assertions;
  Native execution is recommended, not yet selected. Documentation only:
  no renderer/artwork/app changes and no tests rerun in this planning step.
- [x] User's next “ㄱㄱ” approved the plan and recommended Native execution.
  New isolated source/art/pose/render/export modules implement static endpoints.
  Complete hidden roots/thighs are actual closed skin; head pixels unchanged;
  two anatomical segments consume supplied joint positions. Reference-only PNG
  is not a runtime dependency. Explicit depth and hidden root ink clipping keep
  skin opaque; no global closing, nonuniform body scaling or endpoint image morph.
- [x] RED→GREEN checks caught missing modules, fixed-style outline measurement,
  left hair-fringe ownership, missing seated rump rim and hidden thigh ink.
  Final focused suite **18/18 passed**; exported standing outer contour maximum
  distance **1 native pixel**, head differing texels **0**, socket minimum alpha
  **255**. Canonical hash remains unchanged. Visual output is provisional:
  seated hindfoot exposure and floor contour still differ from the reference.
  Evidence: `artifacts/original-contour-static-20261001/` (7 final PNG/JSON files;
  2 earlier task diagnostic PNGs also retained). No animation generated.
- [x] Fresh read-only whole-change review found no Critical issue, one Important:
  torso clipping buried distal hindfoot ink along with root ink. Split authored
  `rootInk` from visible distal `ink`; preserve opaque skin and clip roots only.
  `forward distal hindfoot contour remains visible over seated rump skin`
  failed first, then passed; complete new suite18/18 passed after this one fix
  pass. Actual native/3x white/dark output re-inspected; no reviewer re-dispatch.
- [x] Native Core **241/241** and App **1,096/1,096** passed. App first reported
  NETSDK1047 (missing win-x64 restore target); documented existing-dependency
  restore resolved the test environment without changing project source.
  Historical bind/sit rerun remains **27 passed,1 failed**:
  `articulated white body has no trapped transparent cut holes across sit and rise`.
  That rejected tool is preserved, not repaired or waived by the new experiment.
  Content snapshot of657 pre-existing out-of-scope files is unchanged, including
  canonical PNG/product sources/assets. HEAD remains67f7504; no Git/installation.
- [ ] User static visual acceptance. Exact seated floor curvature/foot exposure
  may still need artistic adjustment; no animation or product integration until
  accepted/authorized. Review Minor deferred: an edited tip on the same bone ray
  can change distance without changing rendered geometry (current endpoints are
  unaffected); enforce measured segment lengths before future arbitrary poses.

## Squashed seated artwork — correction investigated, NOT complete — 2026-10-01

- [x] User rejected the previous seated output as squashed. The prior technical
  checks and review were not artistic acceptance. Do not use the previous movie
  as an accepted result or install this prototype.
- [x] Traced two structural causes: broad source haunch tissue was assigned to
  the folding hindleg, and the global moved-body alpha closing/reoutline pass
  hid poor partition coverage behind a low broad silhouette. Removed that pass;
  broad haunch pixels now stay on the torso. The visible original foreleg root
  contour remains on its limb rather than being moved with the head/haunch.
- [x] Added fixed original-derived hidden artwork under existing opaque cover,
  separately rigid chest/haunch plates, and an upper foreleg socket overlap.
  Chest stays upright above planted forelegs while the haunch rotates. No
  seated raster, endpoint shape tween, broad silhouette closing or erosion.
  A bounded2.5px fore-root underlap remains; it is not a complete socket solution.
- [x] New regressions initially failed for haunch ownership and independently
  renderable body artwork; they now pass. Plate alpha area remains within1.5%
  under rigid motion, and common rigid placement/rest reconstruct canonical
  RGBA exactly. This proves those local properties, NOT a natural seated shape.
- [x] Read-only review confirmed improved volume but flagged incomplete front
  socket coverage, AA cracks between separately filled bridges, remaining
  between-leg notch gaps, insufficient rise coverage, and still-short-looking
  exposed forelegs. Expanded hole verification to82 sit/rise poses and combined
  adjacent bridge paths; retained the strict failing coverage assertion.
- [x] Latest focused run: **27 passed,1 failed** (10bind +18sit). Coverage still
  fails in28/82 sampled poses; several have only1–2 low-alpha4x pixels near
  (26.75,77.25). This is unresolved, not a passed or waived verification.
  Current white/dark contact sheet/endpoints/comparison are diagnostic only:
  `artifacts/canonical-volume-20261001/`. No revised movie/package/install.
  Canonical PNG SHA256 remains
  699348d1973709f228d843341ac5312aa7f449d57b9bfc76576256231e259a78.
- [ ] Visual correction remains incomplete: seated belly/foreleg exposure is
  still unlike the reference. Repeated seam patches are stopped under the
  systematic-debugging architecture-review rule. The next work is complete
  original-proportioned chest/limb artwork and socket/joint ownership review,
  not another global silhouette patch. User acceptance, integration and release
  are still open; no commit/push or unrelated dirty-worktree changes.

## Historical rejected original-bound sitting and rising — 2026-10-01

- [x] User accepted the original-first static drawing/bind with “이대로 구현해봐”
  and authorized connecting sitting on that basis. Implemented isolated
  `canonical-sit.cjs` and its local exporter/tests; no native app/product bank
  replacement or installation. The rejected geometric skin stays historical.
- [x] Runtime uses only immutable canonical standing paint: source torso and
  foreleg plates, near-hind paint split into two rigid bones at its original
  knee, and one rigid original head. Original-bound spine and all two-segment
  bone lengths stay fixed; forefeet remain planted and front chains straight.
  No ankle axis, cloned far-hind paint, seated raster or endpoint contour tween.
- [x] Hind tip reaches13.5px toward the face with3.2px lifted clearance, then
  holds before the8px late hip drop and one.65px settling rebound. Sitting and
  separately timed foot-gathering rise each take900ms. A small hip preload
  keeps the nearly straight original hindleg reachable during preparation.
- [x] Connecting strips follow transformed original cut borders; internal
  knee underlap repairs AA gaps. Original moved paint union drives outside
  body ink; narrow-cut closing radius1.5native/max gap span3px removes a tiny
  gray belly notch. Source forebody corner was separated from head ownership
  without changing the assembled canonical image. Canonical rest RGBA remains
  exact; source contour median ink is100/84/88, not the initial lighter gray.
- [x] Independent review found an actual rest/motion body-ink pop: near-zero
  head/body transforms triggered a full body style switch. Added three failing
  rendered regressions, then drove same-pose cut/ink correction continuously
  by non-head body-cut separation. Head-only motion and common rigid placement
  no longer rebuild the body; entry/exit converge to exact original paint.
  No standing/seated endpoint raster or contour is blended by this repair.
- [x] Added a real crossing-crease regression: a moved hind crease appeared
  inside the opaque near-front paw. It failed at the hand-checked4x pixel
  (159,311), then passed after source near-fore coverage took foreground
  ownership. Array-order invariance, opaque joints, transparent cut holes,
  source pink-head area, supplied-joint effects and input validation also pass.
  Focused bind/sit checks:26 passed,0 failed (9bind +17sit).
- [x] Follow-up read-only review confirmed zero rendered RGBA difference at
  near-rest sit .0001 and rise .919 (native and4x), same-pose repair rather than
  endpoint-art interpolation, body/head independence and opaque foreground
  ownership. No Critical/Important findings remain in its bounded scope;
  closing-radius comment corrected. Combined local motion-study checks:
  **72 passed,0 failed**. No .NET rerun: native app and banks are unchanged here.
- [x] Exported and visually inspected updated contact sheet, canonical standing /
  new seated / reference-only comparison, transparent endpoints and diagnostic
  joint image under `artifacts/canonical-sit-20261001/`. Encoded caption-free
  `canonical-sit.mp4`:12s/60fps/768x440/H.264,873601bytes; normal then half speed.
  All720 encoded frames decode successfully. All720 PNG headers have correct
  dimensions;12 representative whole-frame pixel comparisons match the final
  renderer, and provenance input hashes match current source. Canonical PNG
  SHA256 remains699348d1973709f228d843341ac5312aa7f449d57b9bfc76576256231e259a78.
- [ ] New seated shape/timing still requires user visual acceptance; tests do not prove
  artistic acceptance. Native installation, product integration, commit/push
  and release remain outside this local motion preview.

## Original-first standing artwork and internal joint bind — 2026-10-01

- [x] User rejected the geometric body skin and explicitly corrected the order:
  match the original character drawing first, then implement joints inside it.
  User approved the proposed static original/overlay/joint comparison. This
  supersedes the body appearance of the preceding trial, not the accepted
  forward-hindfoot reach and plop timing.
- [x] Added isolated `canonical-bind.cjs`: partitions immutable canonical RGBA
  into head, torso, far-front, near-front and near-hind paint with exclusive
  ownership. Rest reconstruction comes from separated layers, not a special
  shortcut to the unpartitioned source. Original body/haunch/leg proportions,
  outline colors and alpha are retained; no geometric replacement masses.
- [x] Bound root/bend/tip joints inside original skin, straight front chains,
  no ankle axis, no duplicated hidden far-hind paint. Diagnostic root rotations
  and knee bends consume these joints and original limb texels; they are not
  decorative bone overlays on a separate motion path.
- [x] Seven failing missing-feature checks became seven passing checks: exact
  source reconstruction and 4x nearest-neighbor reconstruction; exclusive paint
  coverage and landmarks; internal three-point joints and straight forelegs;
  actual root/bend effects; immutable rest after diagnostic operations.
  Independent read-only review confirmed source fidelity and scope but found a
  bend-rendering coverage gap. Added an eighth check of actual bend-only limb
  pixels. An in-memory renderer mutation ignoring bend fails that check (0
  changed pixels); real implementation passes all8. No source mutation needed.
  Combined local bind/skin/rig/depth/legacy motion/export checks after that
  regression: **54 passed,0 failed**.
  No .NET rerun: no native application code or product bank changed in this task.
- [x] Exported and visually inspected white/dark `comparison.png`, transparent
  `standing.png`, joint overlay and paint ownership map under
  `artifacts/canonical-bind-20261001/`, plus native isolated layers, `rig.json`
  and hashed `verification.json`. Source comparison: **0 different pixels,
  maximum RGBA channel difference 0** over the full96x96 decoded image.
- [x] User accepted this static drawing/bind and requested motion implementation;
  the subsequent motion study is recorded above. Application integration/bank generation,
  installed-app replacement, commit/push and release were not performed.

## Canonical head and connected white skin — 2026-10-01

- [x] User accepted the forward-plop blocking and authorized implementation,
  then explicitly approved the skin design: rigid canonical head/face/hair/ribbon,
  joint-driven white body/limbs, near-limb foreground ownership, preview before
  installer replacement. `connected-sit-rig.cjs` is byte-identical to its
  accepted snapshot under `artifacts/connected-skin-20261001/accepted-source/`.
- [x] Added `connected-sit-skin.cjs`, consuming supplied pose coordinates with
  no second motion timeline. Head pixels come only from unchanged canonical
  standing art and receive one translate/rotate transform per frame. Body and
  limb union receives one inset outer outline; near-hind then near-front ink
  establishes ownership inside that union. No seated raster, image dissolve,
  animated scaling or modification to source artwork/product banks.
- [x] First skin inspection found small root notches: applying the blocking
  root mask to opaque skin removed40 fully covered pixels at rest. Added a
  failing coverage regression, then retained the connected white union and
  masked only foreground ink. Ten skin checks and46 combined JS checks pass,
  covering source head/ribbon pixels, actual-joint consumption, knee/paw opacity,
  transparent exterior/floor, constant head area, scale validation and root fill.
- [x] Read-only review found a test gap: white knee/paw centers alone could
  not prove foreground ownership against a white body. Added real dark-edge
  overlaps proving near-hind over torso, far-hind behind torso, near-front over
  hind ink independent of array order, and head over crossing limbs. Omitting
  the foreground ink pass in memory now fails two depth tests; unchanged code
  passes. Follow-up review independently passed all10 skin checks and closed the
  important coverage gap with no remaining findings in its bounded scope.
  No approved rig or renderer change was needed for this review fix.
  Fresh unchanged-app tests pass Core241 +App1096 with zero failures (TRX in
  `test-results/`); this is not verification of new-skin application integration.
- [x] Exported white/dark skin contact sheet, canonical / reconstructed standing /
  reconstructed seated comparison, transparent4x endpoint PNGs, and caption-free
  `connected-skin.mp4` (12s/60fps/768x440,544804 bytes), with native and3x views.
  All720 frames decode; provenance source hashes match. Initial misrouted sheet
  output was moved intact into `initial-export-misrouted/`, not deleted.
- [ ] User rejected this skin after seeing its changed shape; retained as a
  historical trial, not the current appearance reference. Body is a geometric reconstruction,
  not pixel-identical standing artwork. Blink/gaze, native application state
  integration, product-bank replacement and installation are not part of this
  local trial; no commit, push or distribution was performed.

## Forward hindleg reach, seated plop and source-sized limbs — 2026-10-01

- [x] User approved a short design: straight planted front supports, hindfeet
  reaching toward the face, held preparation then a quick pelvic drop and one
  small rebound, with limb thickness compared directly against the canonical.
  Changed only the local connected rig, its checks/export configuration/docs.
- [x] Hindfeet now travel13.5units (60 to46.5) with3.2units of lifted clearance,
  arriving before the pelvis lands. Sitting descent is held then accelerated
  late; contact at normalized .72 is followed by one .65-unit rebound peaking
  at .8. Rise gathers the feet before lifting the haunch, not reversed falling.
  Legs keep fixed segment lengths and attachments; no image morph or scaling.
- [x] Read unchanged canonical alpha: near fore x36..48 at y82 (13px), far
  fore x19..26 at y78 (8px), hind x60..70 at y78 /61..70 at y80 (11/10px).
  Proxy diameters are near fore13, far fore9 and hind11. Near rendered blue
  fill is11px at y82; rounded proxy shape is approximate, not identical art.
  Fore support shortened10 to7.5 to retain standing chest/head height. Far
  shoulder and sole center are both2units lower, preserving the support vector
  and ground contact with its narrower radius. Rise lean amplitude reduced
  to .7 so shortened front supports still read nearly vertical.
- [x] TDD reproduced missing forward reach/plop/thickness, then a raster-width
  mismatch and overly broad far forepaw. Final18 focused /36 combined JS checks
  pass. Read-only review found no actionable issue:10,002 sampled poses preserved
  contact/lengths with no front drift;402 rendered poses retained near-hind knees.
  Final far-fore delta separately passed attachment/contact/depth checks. Parent
  inspected final regenerated source/before/after sheet. Playback acceptance,
  final skin likeness and product integration remain outside that review.
- [x] Final video `forward-plop.mp4` is12s/60fps/600x420 (249024 bytes), one
  normal-speed cycle then half-speed. All720 frames decode; both provenance
  manifests match current sources. Fresh unchanged-app regression command
  `dotnet test --no-restore --no-build` passes Core241 +App1096, zero failures;
  TRX files are under the artifact's `test-results/`. This is not WPF integration.
- [x] User accepted this blocking with “좋아 구현 시작”; subsequent approval
  authorizes the isolated skin trial above. Outputs are under
  `artifacts/forward-plop-20261001/`; before snapshots remain in `before-source/`.
  Character PNGs, installed application and product banks are unchanged. No
  commit, push, installer replacement or skin integration was performed.

## Local root cover instead of whole-parent limb occlusion — 2026-10-01

- [x] User clarified the concern was legs being hidden by the body, not the
  proxy's silhouette, and approved near limbs in front with only their root
  connections covered. Removed whole-parent-ellipse clipping from the blocking
  renderer. A small curved socket follows each near limb's root/tangent, hiding
  its root cap and at most15% of its first segment (capped at1.2units).
  Far limbs remain behind the torso; head stays in front. The rig is byte-identical
  to the before snapshot: no proportion, joint or movement changes this turn.
- [x] Test-first regressions reproduced two failures in the old renderer:
  own-parent overlap erased near limbs, and the actual near-hind knee vanished.
  Both now pass, including parent-size independence and50 sit/rise knee samples.
  Final16 focused /34 combined JS checks pass. Read-only review independently
  inspected202 rendered poses without missing near knees/legs or material root
  gaps. Final skin/seams and user artistic acceptance were not judged; those
  exclusions are appropriate for this colored, render-only motion study.
- [x] Fresh unchanged-app regression run (`dotnet test --no-restore --no-build`)
  passes Core241 +App1096, zero failures/skips; TRX evidence is in the artifact's
  `test-results/`. This does not validate new-rig WPF integration.
- [x] Clean contact/joint sheets, reference-before-after comparison and
  `local-root-cover.mp4` are under `artifacts/local-root-cover-20261001/`.
  The video is12s/60fps/600x420 (248247 bytes); all720 frames decode. Both
  provenance manifests match current source hashes. `before-source/` preserves
  the previous implementation. Original artwork, product and installer unchanged.
- [ ] User visual acceptance remains open. This supersedes the previous
  whole-parent root mask, not the straight-support rig. Skinning and installed-app
  integration have not been performed or declared ready.

## Straight foreleg supports and depth audit — 2026-10-01

- [x] User identified that the original forelegs read as straight supports,
  unlike the previous113-degree standing elbow. Explicitly authorized fixing
  the structure and visually checking depth order for a possible motion base.
  Replaced foreleg IK folding with two collinear5-unit segments. The planted
  foot anchors and10-unit support radius now determine chest height from its
  horizontal shift. No independently animated chest Y can force a bent elbow.
  Forefeet remain fixed; the pelvis descends on the fixed24-unit spine. The
  small settling rebound moves the pelvis only, not the straight front supports.
- [x] Camera-depth passes are explicit: far hind/front, torso and neck,
  near hind/front, then head. Near roots are occluded only by their own parent
  ellipse; distal near limbs can cover unrelated torso instead of being buried
  behind everything. Clean rendering is now the default. X-ray bones are opt-in
  and exported separately, not overlaid on the movie used to judge depth.
- [x] Test-first failures reproduced folded forelegs and near-limb burial.
  Added actual raster checks for parent-root occlusion, near/far ordering,
  input-array order independence and head priority, plus opt-in diagnostics.
  Independent read-only review found no actionable visual or code issue in
  the bounded preview:8,002 pose samples preserve collinearity/fixed lengths
  and ground clearance;130 clean frames keep both forelegs visible with fixed
  rendered sole rows. Follow-up review confirms clean-by-default rendering.
- [x] Final14 focused /32 combined JS checks pass. Fresh unchanged-app tests
  pass Core241 +App1096 (`dotnet test --no-restore --no-build`); TRX files are
  under the artifact's `test-results/`. Clean `straight-forelegs.mp4` is
  12s/60fps/600x420 (232448 bytes); all720 frames decode, and source hashes match
  both render and comparison provenance. This is not new-rig WPF integration.
- [ ] User visual acceptance remains open. Artifacts live under
  `artifacts/straight-forelegs-20261001/`: clean video/contact sheet, separate
  joint sheet, source/before/after comparison, and `before-source/` snapshots.
  Comparison alignment now measures the actual rendered sole rows instead of
  reusing an old projection offset. Source artwork is unchanged. This remains
  a local motion study: skin likeness, playback acceptance, future motions and
  application/installer integration are not declared complete.

## Canonical-proportioned connected blocking — 2026-10-01

- [x] User found the simplified legs too long and asked for direct comparison
  with original Dororong proportions. Compared unchanged canonical standing PNG
  and seated reference (+3,-12 registration) at a common5x pixel scale.
  Hand-read main head/face envelope is approximately x7..65/y25..68; side-lock
  tips extend to y74 but must not be treated as the full-width chin. Belly is
  around y75; near/far fore soles around y86/81. These are visual mass landmarks,
  not inferred anatomical joints or a pixel-perfect outline specification.
- [x] Reproportioned the existing two-segment rig: head radii29/21.5 (was23/22),
  chest14/10 at rest height18 (was10/10 at25), pelvis14/15 at23 (was12/12 at25).
  Fixed internal attachments replace mass-center roots; upper legs are
  geometrically occluded by the torso instead of fully exposed. Fore chains
  total12.2 (was24, -49%); hind14 (was24, -42%). Spine remains24; neck19;
  no terminal foot axis, dynamic scaling, opacity dissolve or artwork input.
  Fore soles remain fixed; hind lift2.4 and step6, one small seated rebound.
  Reduced rising support shift to keep the shortened chains reachable.
- [x] Test-first proportion and rendered-landmark checks failed against the old
  rig. All8 focused and26 combined JS checks pass, including dense sit/rise
  connectivity, ground contacts, constant dimensions and independent fore-elbow
  articulation across the cycle. Previous source preserved in
  `artifacts/connected-proportions-20261001/before-source/`.
  Comparison columns are **original / previous / revised**, rows stand/sit:
  `reference-before-after.png`; source hashes/landmarks are recorded alongside.
- [x] Independent read-only code/visual review reports no actionable finding:
  proportions are closer in the same-scale comparison. Its10,002-pose audit
  found zero root error/forefoot drift, nonnegative floor clearance and at least
  0.239-unit IK extension margin. Caption-free `proportioned-sit-blocking.mp4`
  is12s/60fps/600x420; all720 frames decode. Final source hashes match provenance.
  Fresh unchanged-app regression passes Core241 +App1096 with
  `dotnet test --no-restore --no-build`; TRX files are in this artifact's
  `test-results/`. This does not establish WPF integration of the new rig.
- [ ] Superseded by the straight-foreleg correction above. This remains a color-mass motion study; no
  source PNG, product bank, app, installer, commit or push changes. Skin
  connection stays deferred; exact silhouette and playback appeal are not
  established by proportion tests or code review.

## Connected color-mass sitting block — approved 2026-09-30

- [x] **Cartoon simplification, before skinning.** User accepted the connected
  block, then requested fewer leg axes and cuter cartoon movement. Skinning is
  deferred for this review. Removed the terminal paw segment/rotation: every leg
  now has two fixed12-unit segments, three points, and a rounded tip. One rounded
  polyline avoids crossing segment outlines. Hindfeet take a shorter6-unit step
  with3.2-unit lift; sit/rise each take900ms. A single0.55-unit body rebound follows
  seated contact; head/chest/hip volumes remain fixed and forefeet stay planted.
  Previous accepted source is preserved under the old artifact's `accepted-source/`.
- [x] Test-first simplification reproduced three old-rig failures; all6 focused
  contracts and24 combined JS motion/export/bank checks now pass. Independent
  read-only review reports no actionable finding, with a2,001-sample-per-direction
  attachment/length/floor audit. This is not artistic acceptance. New evidence:
  `artifacts/connected-cartoon-sit-20260930/`, contact sheet and caption-free
  `cartoon-sit-blocking.mp4` (12s/60fps/600x420); all720 frames decode and input
  hashes match final sources. Normal cycle0–4s, half speed4–12s.
  Fresh unchanged-binary regression completed2026-10-01 KST: Core241 +App1096
  pass (`dotnet test --no-restore --no-build`); TRX files are in this artifact's
  `test-results/`. This does not test skin or new motion integration into WPF.
- [ ] Superseded for visual review by the user's leg/body proportion correction
  above. No character skin, product bank, app or installer changes.

### Previously accepted connected block

- [x] User rejected the joint-skinned playback despite accepting its seated
  still. Code diagnosis: front chains rotate as rigid sticks, roots do not share
  trunk attachments (up to14.46px discrepancy), and folded foot skin compresses
  65%. User approved changing the architecture and delivering **colored head,
  chest, pelvis and four limbs first**, before any skin or product integration.
- [x] Added independent `connected-sit-rig.cjs`: shared chest/hip roots, fixed
  spine24 /neck15, four two-bone IK chains with fixed paw segments. World-space
  near/far ground planes are projected consistently. Foretoes stay planted;
  hindfeet lift before relocation, pelvis rotates/downward around the chest,
  and the neck/head follow their connected parent. Sit and rise have independent
  timing. Rigid mass dimensions do not shrink; no images or skin inputs exist.
- [x] Added `connected-sit-blocking.cjs` and `export-connected-sit.cjs` for a
  caption-free color proxy: pink head, teal chest, orange pelvis, blue forelegs,
  purple hindlegs; near/far shades and real joint lines expose attachment and
  contact behavior. Artifacts: `artifacts/connected-sit-blocking-20260930/`.
  This is choreographed kinematic blocking, not a dynamics simulation or a
  finished character rendition. Approved seated artwork remains a future skin
  target, not something this proxy is claimed to match pixel-for-pixel.
- [x] Five test-first contracts pass: shared roots/fixed lengths, planted soles
  and lifted steps, support-before-descent with actual front-elbow articulation,
  continuous sit/rise endpoints, and real colored rendering without image input.
  Independent review found no connectivity issue; it caught decorative ink
  extending below contact geometry. A raster test reproduced that failure;
  inset outlines fix it and all capsule-radius floor checks now pass.
  Follow-up review has no remaining actionable finding for this bounded proxy.
  Final23 JS motion/export/bank checks pass. Caption-free720-frame video fully
  decodes: `connected-sit-blocking.mp4`,12s/60fps/600x420,473772 bytes. Input
  hashes match final sources. Normal-speed cycle0–4s, half-speed cycle4–12s.
  Fresh unchanged-app regression also passes:241 Core +1096 App using
  `dotnet test --no-restore --no-build`; TRX files are in the artifact's
  `test-results/`. This does not claim proxy integration into the application.
- [x] User accepted this connected block, then requested the cartoon
  simplification above before skinning. Earlier acceptance does not approve
  the new motion or a finished character rendition.

## Canonical-only skeletal sitting — approved 2026-09-30

- [x] **Soft-skin seated endpoint — visually accepted.** User approved
  limited joint-region skin reshaping and an endpoint-first gate before another
  transition. Added isolated `renderSeatedEndpoint()` (existing `render(time)`
  unchanged): canonical head plus reconstructed white body/outline anchored to
  forepaws, hip and knee. This is not literal standing-body texture preservation
  or a seated-image blend. No seated reference pixels enter the renderer.
  Static light/dark comparison and joint overlay:
  `artifacts/seated-skin-endpoint-20260930/`. Left is reference; right is candidate.
  Independent review finds the previous short/boxy legs, shallow gap and haunch
  notch resolved, with no material head/body seam; ready for **static** user
  acceptance, not animation completion. User then confirmed **이 자세로 진행**.
  Approval applies to the static shape, not the subsequent motion. No
  app/install/product bank was replaced.
  New regression reproduced the gap failure against the previous rigid endpoint;
  registered reference sampling corrected an initially misidentified edge pixel.
  The static-stage 17 JS checks passed. Fresh unchanged-binary .NET regression
  with `--no-restore --no-build` passes Core241 + App1096; this is not new skin
  integration into WPF. TRX output is under the artifact's `test-results/`.

- [ ] **Joint-skinned motion — visually rejected; superseded by connected blocking.** After static approval,
  added `renderSoft()` using inverse-bound surface control points driven by the
  existing fixed-length rig. Forefeet place/support, hindfeet lift/tuck, then the
  pelvis descends; rising uses its independent rig path. Body skin is reconstructed,
  not source-pixel-identical. No endpoint-image/contour dissolve or final pose swap.
  Reviewer caught a changed standing silhouette: corrected belly ownership plus
  bounded bone-angle-driven armpit/paw-pad and haunch tissue corrections. A new
  source-relative lower-body test reproduced failure before calibration. Folded
  paw tissue compresses across its bone and is geometrically occluded, without
  alpha fading; this removes ink poking through the accepted haunch.
  `assets/seated-skin-approved-test.png` is an immutable test-only copy of the
  accepted static artifact, never a rendering input. The endpoint regression
  compares against it independently (maximum premultiplied difference <=2).
  All18 skeletal/motion/export/bank JS tests pass. New checks also cover local
  standing shape, volume, frame continuity, and sit/rise joins. Evidence:
  `artifacts/joint-skin-motion-20260930/`, native/enlarged 15-second clean preview
  plus joint contact sheet. This is a renderer study, not desktop capture.
  Independent follow-up review: no actionable findings for preview handoff;
  canonical standing silhouette overlap98.11% whole /95.61% lower body, no
  detached lower-body components across65 samples per direction. Final clean
  `sit-joint-skin-preview.mp4` is15s/60fps/384x440 (241618 bytes); all900 frames
  decode successfully and render input hashes match final sources. Review and
  numerical tests do not decide whether playback feels natural to the user.
  User subsequently rejected the movement. Further curve/skin tuning is stopped;
  the connected color-mass block above is the approved next direction.

- [ ] **Historical rigid-cutout reference likeness not accepted.** User requires the endpoint to resemble
  the seated original, not merely be a possible sitting pose. Follow-up measured
  the aligned reference(+3,-12), moved forepaw goals to(26,86)/(40,86), shifted
  pelvis left, reassigned the haunch to the upper hind limb and added local ground
  contact/occlusion treatment. Reference-paw and lower-body silhouette tests
  reproduced failures (old silhouette IoU0.712); all6 focused tests now pass.
  However independent visual review still finds shorter/boxier front legs,
  shallower central gap and a rear notch. These are **unresolved visual issues**;
  numerical tests do not establish likeness. Comparison evidence:
  `artifacts/skeleton-sit-reference-20260930/reference-comparison.png` (reference
  left / candidate right). Stop further parameter-only patches; propose limited
  joint-region skin deformation while retaining canonical-only textures and
  skeletal motion, then approve the endpoint before building the next transition.
  This does not replace the app or represent a visually accepted result.
  Follow-up verification:6 focused JS tests and unchanged-app regression suite
  (241 Core +1096 App, no-build) pass; TRX files are in the comparison artifact's
  `test-results/`. Removed unused front-leg IK solve noted by the reviewer.
  These technical checks do not close the unresolved visual requirement.

- [x] User rejected the previous articulated/morphed preview as still melting.
  New approval: standing image only; seated drawing is visual reference, not an
  endpoint texture. Fixed-length leg axes must directly drive skin; front support,
  hind tuck, rotating torso and delayed head settle. First deliver clean and joint
  overlay previews; product/install replacement remains outside this acceptance gate.
- [x] Added `skeleton-sit-rig.js` and `skeleton-sit.js`: four three-segment chains,
  two-link IK to each ankle, fixed paw segment, rigid rotation/translation per
  skin part. Canonical white body cutouts are joined with bounded neck/joint paint;
  their union is re-outlined and subpixel partition seams closed. Opaque face/hair
  texels are preserved; the body is not pixel-identical to canonical at rest.
  No seated/sleep image, endpoint contour tween, part scaling, or final-pose swap.
- [x] Five test-first contracts failed before implementation and now pass: bone
  lengths, skin transforms, support/lift, path joins, and actual rendered output.
  Independent review caught the original-image rest shortcut causing an endpoint
  pop. Reproduced red at max255 premultiplied channel difference, removed the
  shortcut, and passed epsilon raster checks. Reviewer separately verified all
  endpoints at scales1–4 (zero pixel difference). Sheet-only provenance fixed too.
- [x] Added `export-skeleton-study.cjs`: original-resolution and enlarged clean /
  joint-overlay views, normal speed then one-third speed, no captions. Evidence:
  `artifacts/skeleton-sit-20260930/`. This is a renderer study, not desktop capture.
  Existing product assets, source images and running app have not been replaced.
- [x] Calibrated native outline contrast after measuring lower-body ink mass
  dropping from20,739 to14,606 with the1px reconstruction. A failing regression
  caught the loss; the1.4px kernel passes90–130% source-ink bounds without changing
  alpha or joints. Reviewer rechecked: no remaining findings in preview scope.
- [x] Final verification:16 JS motion/bank tests pass; existing .NET suite passes
  241 Core +1096 App tests (no-build, unchanged app binaries; this does not claim
  skeletal integration into WPF). Final900 frames encoded to
  `artifacts/skeleton-sit-20260930/skeleton-sit-preview.mp4` (15s,60fps,768x440);
  full FFmpeg decode passes. Input hashes match final sources; scoped diff check
  passes. TRX evidence is in the same artifact directory's `test-results/`.
- [ ] User visual acceptance remains pending. No commit, push, install or
  publication. Product bank integration is deferred until the motion is accepted.

## Previous articulated sitting — rejected visually 2026-09-30

- [x] User approved replacing the 650ms endpoint morph with front-foot placement,
  forward support, lifted hind-foot tuck, pelvic descent and delayed head settle,
  preserving the final seated artwork. A separate rise path is part of the same
  approved change. Scope is local implementation and comparison media, not install,
  commit, push or publication. Release/right gates remain open.
- [x] Added a 2D motion model with two-link hind-leg IK and contact phases, routed
  authored texture landmarks through it, with front-paw shape timing independent
  of pelvic lowering. Sit 900ms / rise 650ms; interrupted actions retain their
  current path. LOCO v2 app/preview integration has separate rise poses.
- [x] Test-first evidence: five motion-contract tests failed before the model;
  two app integration tests failed against old650ms/reversed sitting, then passed.
  Independent read-only review found a real2x endpoint sampling jump (max62),
  reproduced by a new failing test and corrected with endpoint coverage handoff.
  Eight motion/export JS tests pass, including1001 pose /501 mesh samples per path, actual
  rendered head motion and both exact authored endpoints. Reviewer rechecked fixes.
- [x] Rebaked the reviewed atlas/product bank:772 frames, raw SHA256
  `f8da2b5fa605085305ee741b2e1168a03fe650e1a81fcb296265b455dfa64699`.
  Three bank tests verify provenance, every frame hash and unchanged standing,
  final seated poses and512walking frames; endpoint PNG and premultiplied-alpha
  validators pass. Full .NET suite passed241 Core +1096 App tests before the last
  raster-only rebake; rebuilt against the final bank and reran21 locomotion/sitting
  tests, including byte-exact embedded-frame hashes, all passing. Eight JS
  motion/export tests reran successfully against final sources. Scoped diff check
  passes. TRX evidence: `artifacts/seating-motion-20260930/test-results/`.
- [x] Prepared native/enlarged normal-speed and one-third-speed comparison:
  `artifacts/seating-motion-20260930/sit-comparison-preview.mp4` (15s,60fps,
  768x440; old left/new right; native top/enlarged bottom). FFmpeg full-stream
  decode passes. This is renderer output, not a desktop recording.
- [ ] Not accepted: user reports a melting appearance despite technical tests.
  Superseded by the canonical-only skeletal approach above. Running/installed
  desktop executable was not replaced; no commit, push or publication performed.

## Community media — requested 2026-09-30

- [x] Supply saved transparent PNGs after the user switched from desktop capture
  to cutouts. Copied existing files without image generation, redrawing, resizing
  or background edits: 8 main selections and 18 authored transition frames in
  `artifacts/community-media-20260930/transparent-poses/`, bundled as
  `dororong-transparent-poses.zip`. Main poses include idle, tracking, cheek pull,
  perch, head-held, sleep, closed eyes and canonical standing. All 26 PNGs have
  both visible and fully transparent pixels. These are stored renderer/source
  assets, not desktop screenshots; walking and pounce sequences are not included.
- [x] Correct capture PNG alpha after the user reported the replacement's white box.
  Root cause measured in both head PNGs: exactly 1,024 pixels (32x32 cursor
  rectangle) had alpha0 despite containing valid screen/cursor RGB. In
  `04-head-with-cursor.png`, bounds were (113,13)..(144,44); in `03-head-hold.png`,
  (213,86)..(244,117). Image inspection alone hid this defect; the earlier clean
  claim below was incorrect. An alternate Desktop Duplication capture also
  retained nonopaque cursor pixels, so switching capture APIs alone is not a fix.
  Export the captured RGB as opaque PNG (`format=rgb24`), without flattening on
  a black/white matte or repainting/replacing any character/cursor pixels.
  Verified all 3 final images pixel-by-pixel: RGB changes0, nonopaque pixels0,
  dimensions unchanged. Final files are in `desktop-captures/opaque-cursor/`;
  `dororong-real-screenshots-fixed.zip` contains only these 3 corrected PNGs.
  Supersedes earlier screenshot ZIPs. No app/source edits; this fixes capture
  encoding, not character rendering. Future desktop exports must use RGB24.
- [x] Retake the head-hold screenshot with the real cursor explicitly included
  (`gdigrab -draw_mouse 1`) after the user reported a capture-only black square.
  Native desktop source `retry-cursor-inspection.png` shows the held pet and arrow;
  delivered its unscaled 200x92 privacy crop `desktop-captures/04-head-with-cursor.png`.
  Initial visual inspection showed an arrow and no surrounding chat text, but
  missed the alpha defect subsequently confirmed and fixed above. A live region burst missed the moving
  pet and is not delivered. `dororong-real-screenshots-cursor.zip` contains the
  two earlier context shots plus this new cursor-inclusive head shot only.
  That delivered PNG was not a clean retake; the corrected opaque files above
  supersede it. App code unchanged.
- [x] Capture two real Windows-desktop screenshots, not renderer composites.
  Launched the existing latest local Release executable (not installed/replaced)
  and a new blank Notepad window. `gdigrab` captured the actual desktop;
  delivered two native 640x360 taskbar-region PNGs without scaling, added text
  or compositing. Inspected both to exclude browser tabs, chat content and account
  details. `artifacts/community-media-20260930/desktop-captures/dororong-real-screenshots.zip`
  contains only `01-desktop.png` and `02-taskbar.png`, not full-screen inspection
  captures. No source changes or upload. Dororong/blank Notepad remain open.
- [x] On the user's ready signal, capture the real head-hold/dangling pose.
  Inspected the desktop, then captured the left-side 300x430 screen region
  directly with `gdigrab`. Selected `03-head-hold.png` without scaling, text or
  compositing; visible head grab, hanging body and Windows taskbar, no chat/tabs.
  `desktop-captures/dororong-real-screenshots-3.zip` now bundles the three selected
  screenshots only. The original two-image ZIP is retained. Told the user the
  capture was complete and they could release the mouse. No app/source changes.
- [x] Capture the requested real taskbar-perched photo on the subsequent request.
  `perch-inspection.png` captured the desktop with mouse capture enabled and
  RGB24 output; its native 180x130 privacy crop is
  `desktop-captures/opaque-cursor/05-perched.png`. Visually inspected the front
  paws hanging below the taskbar top edge; every pixel has alpha255. No pose
  compositing, scaling, added text or source edits. The pointer was outside this
  crop. The earlier `02-taskbar.png` was rejected because the pet was down in the
  taskbar area, and is not a valid perched reference; do not reuse it for that purpose.
  A separate window-top photo was not captured. Earlier limitation: Computer Use
  cannot target Dororong's transparent window; Notepad's window capture also
  timed out twice after refreshing selection. Stopped UI automation and asked
  the user to arrange a head-hold/perch pose and reply when ready. Desktop
  capture itself works; do not substitute staged renderer images for this request.
- [x] Add the omitted head-carry demonstration and visible tracking cursor.
  Latest delivery: `artifacts/community-media-20260930/complete-demo/dororong-demo-no-text.mp4`
  (29s, 640x480/60fps, silent, no text). Timeline: idle 0–3s, cheek 3–9s,
  tracking/pounce 9–17s, head lift/swing/stop/release 17–24s, perch paws 24–29s.
  Actual presenter head hit enters the production loop/controller; 225 held
  frames and -19.7..19.7-degree swings observed, with inertia after the pointer
  stops and a release/settle. Tracking uses the same visible cursor coordinates
  as the loop input, traveling across an on-screen arc before approaching to
  trigger a real pounce. The old off-screen tracking samples were replaced.
  Verification: old export fails the expanded scene-count check; all 1,740 new
  source frames pass exact-2x/no-text-margin checks; cursor bounds/travel, real
  Flight/Landing phases, head hold/swing/release and stopped-pointer inertia pass.
  Encoded head/tracking contact sheets inspected; all 1,740 MP4 frames decode.
  Only the ignored exporter/verifier, new media and this ledger changed this turn;
  no new app source edits, full app-suite rerun, installation or publication.
  Retains the prior blur fix and supersedes the 21-second delivery below.
- [x] Approved app-level blur fix and final text-free video.
  `HuntEyes` now preserves authored head texels until the pose transform, instead
  of starting from a prefiltered 4x head and filtering again. Eye displacement
  samples the same working grid; cheek deltas cancel identically premultiplied
  original pixels, avoiding old eye/mouth/outline ghosts. Geometry, asset payloads,
  pose timing, final rotation sampling and outline repair remain unchanged.
  Regression RED: 4 neutral tests failed (face mean channel error 11.638);
  after the first fix, 6 deformation tests exposed the mixed-filter residual.
  Both fixed: focused 10/10 PASS; final neutral face error 0.000. One additional
  full cheek-follow case is included in the final suite. Release full run:
  Core241/App1092 PASS, zero failures/skips; evidence in
  `artifacts/verification/blur-20260930/blur-final_net8.0_20260930184451.trx`
  and `blur-final_net8.0_20260930184834.trx`. An obsolete pre-final Debug suite
  was stopped (1041 passed so far, not a complete run); a concurrent Debug
  rebuild hit that testhost's file lock, then focused/final Release runs passed.
  Read-only code review
  caught the residual and found no Important/Critical findings after correction.
  Final renderer export: `artifacts/community-media-20260930/renderer-final/dororong-demo-no-text.mp4`
  (21s, 640x480, 60fps, silent, no text). All 1,260 source frames pass exact-2x
  preservation checks; MP4 fully decodes; contact sheet and native tracking/cheek
  comparisons inspected. This is a production-renderer demonstration on a staged
  backdrop, not an OS recording. It supersedes both exports below.
  No installed-app replacement, package rebuild, commit, push or public upload.
  The frozen September 21 package does not contain this source change; package
  parity/rebuild and user visual acceptance remain separate follow-up work.
- [x] User rejected all text overlays; deliver replacement video only:
  `artifacts/community-media-20260930/text-free/dororong-demo-no-text.mp4`
  (21 seconds, 640x480/60fps, silent). This supersedes the captioned media below.
  Controlled idle/tracking/cheek comparisons in `blur-check/` reproduce softness
  before MP4 encoding. Main image uses HighQuality sampling, cheek overlay uses
  NearestNeighbor; HuntRenderer also composites/rotates/resamples at 256 then 96.
  Prior direct 3x export magnified that difference. Replacement renders at native
  144px first and uses exact 2x pixel repetition, with no text drawing.
  Export regression check failed on the old output's interpolated pixels, then
  passed on all 1,260 new frames; full MP4 decoding and contact-sheet inspection
  passed. This removes extra export interpolation, not the existing app renderer's
  tracking softness. Product code/installed app remain unchanged; that source-level
  issue is diagnosed but not fixed in this media-only revision.
- [x] Produce four PNGs and a silent 21-second, 1280x720/60fps H.264 MP4 in
  `artifacts/community-media-20260930/`; deliver only those five files in
  `dororong-images-and-video.zip`. Real WPF presenter/controller output on a
  staged neutral background, explicitly labeled as a renderer demonstration,
  not a live Windows screen recording. Cheek and pounce use the production loop;
  hanging uses an attached-pose setup and the production paw controller.
- [x] Inspect representative images and encoded-video contact sheet; decode all
  1,260 video frames without errors and confirm ZIP has exactly five media files.
  No product source edits, installation replacement, public upload or post.
  User writes the post; media acceptance and publication rights remain separate.

## JOENESS 0.3 project connection — approved 2026-09-30

- Scope: connect existing sources through AGENTS, add the approved overall
  ROADMAP, and retain this execution ledger and the historical M1 roadmap.
  Routine factual records are authorized; questions, opinions and no-record
  requests do not trigger record writes. Product code/assets/packages stay unchanged.
- [x] Apply and recheck the managed connection; verify 16 local links/anchors
  and whitespace. The 631-file source/docs baseline (including the historical
  M1 roadmap and pre-existing edits outside this ledger) is unchanged.
  Removed only the retired legacy wrapper and replaced the conflicting sole-plan
  rule; preserved the other project rule. Corrected stale unsent-inquiry status
  against the existing sent-inquiry record. No app test rerun, commit, push,
  package replacement or external publication performed for this documentation task.
- [ ] In a fresh chat opened in this project, confirm that the agent can identify
  the connected sources, current roadmap stage and unmet release gates.
  Local setup validation alone does not prove fresh-chat instruction delivery.

## v0.1.3 release, environment coverage and rights review — requested 2026-09-21

User requested public delivery of the accepted tray fix plus a thorough rights
review and guidance for other environments. Prepare patch v0.1.3 while keeping
v0.1.2/older releases and the archive intact. Review rights findings before public
asset upload; do not imply non-commercial attribution establishes permission.
Keep current runtime8.0.31, stable identity/paths, unsigned disclosure and hashes.
No signing purchase, BIOS/security changes or new accounts.

### Rights inquiry and private repository — authorized 2026-09-21

User approved making the repository private and sending the SHIFT UP inquiry
through Chrome. Preserve all commits/tags/releases; do not delete old assets.
- [x] Verify repository identity/access, change visibility to private and verify.
  GitHub repository id1349611819, JOEWRKS/Doros-on-my-desktop: private=true.
  Pre/post Git refs and both releases' asset IDs/sizes/digests unchanged.
  No Pages, public forks or stars at preflight. No deletion or history rewrite.
- [x] Compose and send inquiry to official ugc@shiftup.co.kr in Chrome; accurately
  use user's revised scope: routine app/publication inquiry, web-original-based
  artwork made with drawing tools and AI help; code license separate from IP.
  Do not include the superseded provenance/history discussion in the email.
  User confirmed final send. Gmail displayed '메시지 전송됨' on 2026-09-21.
  No attachments; no delivery/read/permission approval claimed.
- [x] Record outcomes without committing mailbox contents or account details.
  docs/RIGHTS-REVIEW.md updated with final inquiry. No commit/push performed.

- [x] Check official franchise fan-content policy and exact image provenance.
  Official kr/ja SHIFT UP guidelines reviewed: game-distribution and simple
  reproduction restrictions, third-party rights condition. Desktop-pet scope
  unresolved. User has no source URL/permission record (web-search image).
  docs/RIGHTS-REVIEW.md records primary sources, asset hashes and the sent inquiry.
- [x] Verify v0.1.3 version metadata with expected RED then full GREEN suites.
  Focused RED: 4 old-version failures/28 pass. Core241 PASS. First App run
  exposed one stale numeric Version assertion; corrected that expectation.
  Final App1081 PASS, zero failures/skips (v013-final-verified.trx).
- [x] Freeze package/installer, validate startup and SHA256; retain local evidence.
  Package validator/native smoke, payload suite and All installer suite PASS.
  Local candidate-20260921-v013-01, 469 payload files, runtime8.0.31, unsigned.
  See docs/verification/2026-09-21-v013-local-candidate.md for hashes/limits.
  No host install/update/uninstall or other-PC acceptance performed for v0.1.3.
- [x] Record practical cross-PC/mixed-DPI test matrix and explicit untested cases.
  docs/ENVIRONMENT-TESTING.md: physical-PC route without virtualization, scaled
  displays, lifecycle, sleep/resume and evidence template. No other PC tested.
- [ ] Resolve rights/publication gate before upload; report any missing authority.
  HOLD: see [R4](ROADMAP.md#1단계--출시-준비--미완료) and
  [rights review](docs/RIGHTS-REVIEW.md). Inquiry sent and repository made private
  as recorded above; no permission approval is recorded. Local candidate work
  does not clear the outstanding installation/environment/publication gates.
- [ ] If cleared, commit/push/tag and publish new installer/checksum assets,
  verify anonymous downloads, update main documentation without changing old assets.

This is the sole active task ledger. Historical detail is preserved in the
[pre-cleanup archive](docs/ARCHIVE.md), including the complete previous ledger.

## Current product state

- Repository is now private pending rights clarification. Previously public
  installer v0.1.2 preview remains preserved, bundled .NET 8.0.31, unsigned.
- The current local v0.1.3 includes left-click tray support, head sharpness,
  accepted source-aligned sitting/rising and ordinary-window layer recovery.
  The installed update and visible-over-Clipchamp result were accepted by the
  user. Preserved v0.1.2 release assets do not include these updates.
- Latest completed application verification: Core241/App1099, zero failures/skips.
- The user authorized a private main source checkpoint on2026-10-02; see the
  checkpoint record at the top of this ledger. This is not a public release.
- Local installed-copy acceptance: [tray report](docs/verification/2026-09-21-tray-left-click.md).
- Public release and installer checks: [release report](docs/verification/2026-09-21-v012-release.md).
- Clean-PC/mixed-DPI coverage and character distribution permissions remain
  incomplete. Signing deferred. v0.1.3 release is now requested, subject to the
  rights review above before public upload.

## Repository cleanup — approved 2026-09-21

User requested a clean main while retaining historical files separately.
Scope: remove obsolete artifacts/internal notes from Git tracking, not from
the local disk; preserve current source, assets, tools, tests and documentation.

- [x] Verify clean main matches origin at abe15b56e69c5da205fcd55f496a1ec2e084b373.
- [x] Push archive/pre-cleanup-2026-09-21 at that exact commit and verify remote.
  It preserves all126 tracked artifact files,6 internal notes and old TASKS.md.
- [x] Check exact legacy paths are not referenced by active src/tests/tools,
  README or BUILD guide. Generated artifact output paths remain supported.
- [x] Untrack artifacts/ and .superpowers/; all132 local files remain present.
  Ignore both output areas, link the archive and condense the active ledger.
- [x] Export the staged tree to a fresh isolated folder without historical
  artifacts; run full Core/App tests there, check current-source hash parity.
  Exported tree0a338f455a31f1062cadab208bea775c9516908c to a fresh temporary
  source folder. Core241/App1081 PASS, zero failures/skips; App clean-main.trx.
  src/tests/tools/installer Git trees match the archive exactly. All132 local
  archived files match their original Git blobs; zero tracked artifacts remain.
- [x] Commit/push cleaned main and verify remote parity, zero tracked files in
  the excluded areas, preserved archive commit, and clean working tree.
  Cleanup commit d9e90a3 pushed and verified against remote main. Archive still
  points to abe15b5 with all132 files; excluded areas empty in the main tree.
  Working tree clean after cleanup push. This ledger closes the verified task.

No history rewrite, force push, release changes, local backup deletion or
product behavior changes are part of this cleanup.

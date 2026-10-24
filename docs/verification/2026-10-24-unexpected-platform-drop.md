# Unexpected Windows platform drop — 2026-10-24

## 결과 요약

- 입력 없이 떨어질 수 있는 결함 두 개를 재현·수정했다: 창 조회 만료를
  실제 발판 소실로 처리한 오류, 완전 투명한 창을 발판 가림으로 처리한 오류.
- 관련 회귀 검사380개 통과. 현재 PC의 투명 NVIDIA 창 분류도 직접 재확인했다.
- 기존 도로롱을 정상 종료하고 수정본PID1084로 교체했다. 크기150%, 자동
  눕기OFF는 유지됐다. 수락한 그림·동작 코드는 수정하지 않았다.
- 사용자가 겪은 바로 그 순간의 창 순서/조회 기록은 없으므로, 그 사건의
  단일 원인을 확정했다고 주장하지 않는다. 아래 우선순위는 증거별 추정이며,
  두 코드 결함의 재현·수정 사실과 구분한다. 사용자에게 추가 재현/로그를 요구하지 않는다.

## Request and scope

User reported a sudden downward move during clipboard capture and also while
typing, and requested autonomous diagnosis, correction and a report without
asking them to reproduce it. Reference: clipboard6ef9559e. The screenshot alone
does not identify the supporting HWND, its earlier position or the triggering
event. Preserve the explicitly accepted arc-paw candidate, all artwork, motion,
timing, slider, preferences and installer. No Mac changes in this task.

## Evidence and causes

Evidence root: `artifacts/platform-drop-20261024/`.

### 1. Confirmed: failed/stale acquisition became confirmed support removal

`DesktopSceneSource` expires snapshots500ms after their acquisition began. This
freshness policy is correct: a delayed query must not masquerade as a fresh one.
However, `PetPlatformRuntime` replaced an expired scene with monitors and **zero
windows**, marked that replacement reliable, and passed it to platform physics.
An established window/taskbar owner was consequently removed. Perching likewise
treated Expired as reliable. An unrelated HWND metadata race, repeated failures
or a delayed background query could thus drop the pet without pet input.

Before-fix regression `tests/before.trx`: expected stationary(100,100), actual
(100,500), solely by failing metadata acquisition. The old ordinary-support and
real-presenter perch tests explicitly expected expiry-induced falling; those
contracts were corrected, not hidden. Initial widened run `platform-after.trx`
had292passes/1failure because the old perch test still demanded that wrong fall.

Correction: expired metadata preserves an established HWND owner/perch rather
than claiming it disappeared. It suspends autonomous travel until fresh evidence
returns; it does not publish stale geometry as fresh. Startup/free fall retain
the independent monitor floor. Confirmed monitor removal still bounds/detaches
the pet. Fresh confirmed closing/minimizing/hiding/cloaking/occlusion still causes
ordinary falling. Owner translation on recovery is applied once, not replayed.

### 2. Confirmed: a completely invisible live window was a physical occluder

Read-only native observation found NVIDIA Overlay.exe HWND66088:
CEF-OSC-WIDGET, bounds(0,0,1919,1080), visible=true, extendedStyle134742144,
GetLayeredWindowAttributes succeeded with LWA_ALPHA(2), alpha0. Old reader marked
it CanSupport=true and CanOcclude=true. It was **behind** the taskbar in the
observed samples; these samples do not prove its order at the reported incident.
Its sibling66092 was already correctly exempted by the earlier click-through
NVIDIA rule. The invisible sibling lacked that click-through bit.

Native offscreen fixture reproduces the problem: changing only the invisible
window's z-order removes an ordinary window or taskbar platform. Before-fix
`alpha-before.trx`:4failures/5passes, including both unwanted falls. The fixture
creates its own offscreen HWND, reads real Win32 opacity/class/style metadata,
and translates only bounds/z-order into a synthetic scene; it does not operate
the user's windows or invoke their capture feature.

Correction: only a **successful** global-alpha query with LWA_ALPHA and alpha0
disables support/occlusion. Alpha1..255, color-key-only, unknown or per-pixel
layering retain previous behavior. Opacity is re-read, not cached as permanent
exclusion; the HWND identity survives changes. Once opaque, it can occlude again.
Current actual HWND66088 now reads CanSupport=false/CanOcclude=false in
`native-after/samples.jsonl`; no change was made to the NVIDIA window itself.

[Microsoft's native API contract](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getlayeredwindowattributes)
confirms global alpha0 means invisible only when LWA_ALPHA is active, and a
failed query must not be interpreted as an invisible per-pixel-layered window.

## Incident attribution: ranked, not fabricated

1. Metadata outage/latency converted to support loss: direct matching no-input
   fall reproduced. Compatible with transient capture/window-list churn, but no
   incident-time metadata history exists to prove that exact trigger.
2. Invisible overlay temporarily above its owner: defective classification exists
   on this PC and its fall is reproduced; incident-time z-order is unrecorded.
3. A genuinely visible capture/notification window occluding a support, actual
   taskbar hiding, or autonomous walking beyond a real window edge: possible
   normal surface-loss paths, not established in the supplied screenshot. No
   blanket ignore-all-Electron/fullscreen-window patch was applied.

The read-only30s baseline made317successful queries,0failures,max216.2998ms.
Additional style capture:32queries,0failures,max24.5042ms. Fixed reader capture:
32queries,0failures,max24.2957ms. These are separate short observations, not a
performance benchmark or proof that the reported intermittent trigger never
occurs. Existing app diagnostics show no loop fault for the running candidate;
they do not record historical support transitions. The installed Codex source
contains transparent overlay windows but that alone is not proof its capture
window caused this incident.

## Verification and preservation

- Final `tests/final-platform.trx`:380passed,0failed,0skipped,2m16s. Includes
  platform loop/perch, native opacity/NVIDIA/Discord overlays, taskbar enumeration,
  process identity failure, background freshness, tray menu and reuse regressions.
- New regression coverage:80/499/500/5000/60000ms outages on window/taskbar,
  fresh recovery, real disappearance/minimize/hide/cloak/replacement/occlusion,
  perch retention, independent monitor removal, native opacity boundary values,
  front/back overlay changes and later opaque occlusion.
- Only Windows `PetPlatformRuntime` and `DesktopSceneNative` product files changed
  in this task. No changes to accepted rendering, Core behavior/physics, Mac,
  portable raster, artwork, installer, settings or startup registration.
- Core SHA remains6D006F504BA57CE044F6DE2C0C29FC4D3A2A952BFF281C4B9258FCDB35B23816,
  identical to accepted arc-paw runtime. No new visual acceptance is inferred.
- No injected input, forced GC/trim, heap dump, process suspension, forced kill
  or PC shutdown. Native diagnostic collects metadata, not window titles/content.
- Residual limitation: capture-specific live interaction and the exact historic
  trigger are not verified; neither is claimed fixed by a screenshot-only guess.

## Package / running handoff

Published `artifacts/product-shell/candidate-platform-drop-20261024/` with App/Core
hashes matched against the tested win-x64 output. ZIP SHA256:
`A2AA2FE507587120E9FB4D084539B54A2D3F6A40B45A460E716F73867F85885A`.
App SHA256:`7B4FD3AAF899AD1624A37EA9C3EC56206F6E91046E68BD397E78CAE84916473A`.

After explicit immediate announcement, verified PID5000/startTicks639283784559739915
and exact arc-paw executable path, identified its single main window, sent WM_CLOSE
and waited for normal exit. No forced termination. Launched the separate candidate:
PID1084/startUTC2026-10-23T19:50:21.0283921Z (04:50:21KST). Exact path remained alive
and responding. `replacement.json` records this and unchanged preferences150% /
AutomaticSleepEnabled0. Original candidate/ZIP and installer remain untouched.
App startup is confirmed, not a claim of live clipboard-capture visual acceptance.
No public upload, commit/push or indefinite monitoring was performed.

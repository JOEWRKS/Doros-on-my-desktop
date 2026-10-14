# Figma tray menu support and local file handoff — 2026-10-14

User accepted Doro's own tray popup stability and requested commit/push, then asked
for file/installer cleanup first. Before that work finished, the user reported
falling below the taskbar when Figma's tray menu opened. Commit/push is deferred
until this new fix is checked; no public release or rights change is authorized.

## Observed cause

- User reopened Figma's menu for a read-only native metadata capture. No UI state
  was changed by the probe. Raw evidence: `artifacts/figma-tray-menu-20261014/native-scene.json`.
- Figma PID321096 menu HWND15339574 was `Chrome_WidgetWin_1`, bounds
  x1427..1729, y840..1052; style0x96000000, extended style0x80088.
  Explorer's taskbar was y1040..1080. The menu overlaps its supporting edge by12px.
- The existing reader marked this menu CanSupport=true and not transient.
  Geometry therefore removed the overlapping taskbar interval. The overflow panel
  itself ended at y1040 and did not cross that edge in this capture.
- Previous September13 handling covered only #32768 menus owned by the taskbar's
  process. Other-process regression fixtures reproduced3 failures (standing falls,
  entering/attached grips detach);7 controls passed before the change.

## Bounded correction

- Recognize the observed Chromium popup only with popup + layered/topmost/tool
  styles and without child/caption/resizable-frame/app-window styles. Never exempt
  all Figma/Chromium windows by executable or class alone.
- Classified menus cannot support the pet. Transient menus from any process do
  not cut taskbar surfaces; they still occlude ordinary windows. Real windows and
  genuinely hidden/removed taskbars retain their normal geometry behavior.
- No artwork, animation, topmost ordering, pet position freeze or stale taskbar
  geometry is introduced. Own-tray popup fix and head icons remain unchanged.
- Targeted81/81, Core255/255 and final full App1185/1185 passed (zero skips).
  Reports: `figma-menu-targeted.trx`, `figma-menu-core.trx`, `figma-menu-full.trx`
  in their projects' TestResults folders. Final package/update results are below.
  Tests include literal captured geometry, repeated opens/closes, standing/hanging,
  real taskbar disappearance and11 style-classification positive/negative cases.

## Cleanup / delivery

- Keep source/test/tools/docs in their existing directories; preserve historical
  candidates, original assets, backups and evidence paths. No recursive deletion.
- Prepare a separate ignored `artifacts/delivery/` handoff with only the verified
  Setup EXE, bilingual README and SHA256SUMS. No source ZIP, runtime DLL pile,
  debug symbols, build logs or user data in the handoff.
- `tools/Prepare-LocalDelivery.ps1` validates installer build evidence/hash,
  refuses existing destinations/reparse paths and verifies copied bytes.
- Installed replacement completed as below; user-session Figma menu acceptance remains pending.

## Final local package

Candidate: `candidate-20261014-figma-tray-13` under product-shell and installer.

| File | SHA256 |
| --- | --- |
| ZIP | `B1E11E7A9DE63BF874A7BF2CAA2FE188A067FFE7F769E27E67AE283AD0775DD0` |
| Setup | `9001FAFBA6ED232971A98D071A39E316863A0412FABED8EC9B84953F306A8E17` |
| App DLL | `2F6687788DD83848F3BF46E2874C4C9DA43B7AFCDC4C5CA41F42F8B82438812C` |
| Core DLL | `B2F18514ABBE8B35246B048068A9D341E50D90CAA4CBCA8DBD63E5EB8F52DB9F` |

Exact tested App/Core parity, ZIP round-trip, isolated native start/duplicate/close,
installer build, metadata/icon checks and unsafe-input refusal checks passed.
Unsigned local handoff: `artifacts/delivery/0.1.3-20261014/` contains exactly3 files.
Checksums and refusal to overwrite that existing handoff passed with bytes unchanged.
Git ignores it; artifacts index and BUILD/ARCHIVE/README separate current handoff
from historical candidates and developer outputs. No old files deleted or moved.

## Installed result

User confirmed tray exit and process inventory was empty. Backup, prior candidate12
recovery installer, update script and logs are in
`artifacts/installer/host-figma-tray-update-20261014/`.
Setup exited0; all470 payload files plus ownership manifest matched. User data,
desktop-shortcut preference and startup OFF were preserved. Installed App/Core hashes
match the table. Installed Dororong.exe relaunched as PID380544; its path and Started
log were checked. No new diagnostic failure in the inspected tail.
No commit/push yet; new Figma menu behavior still requires user confirmation.

## Subsequent user acceptance

User confirmed the pet no longer falls when opening Figma's tray menu and requested
cleanup completion plus commit/push. This closes the reported current-PC behavior
check, not all applications, mixed DPI, Windows11 or public-distribution permission.
See the [checkpoint record](../../TASKS.md#accepted-ui-and-snack-checkpoint--2026-10-14).

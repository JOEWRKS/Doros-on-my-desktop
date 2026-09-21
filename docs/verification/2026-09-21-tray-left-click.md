# Tray left-click local change — 2026-09-21

The user approved opening the existing tray menu with either mouse button.
The adapter now handles left-button release, reuses the existing menu, skips
null/disposed/visible menus, activates the popup and detaches on disposal.
Right-click remains the native NotifyIcon route. Menu contents and commands are
unchanged. The packaged Korean/English guide describes both buttons.

## Evidence

- Initial left-click regression: expected one Opening, observed zero (RED).
- Focused adapter/native probes: 15 passed before the two additional guard cases.
- Final Core suite: 241 passed, zero failed/skipped.
- Final App suite: 1081 passed, zero failed/skipped;
  `tests/Dororong.App.Tests/TestResults/tray-left-click-final.trx`.
- Read-only review found no critical/important defect. Its missing/disposed-menu
  coverage comment was addressed before the final full App run.
- `git diff --check` passed.
- Publish succeeded to `artifacts/product-shell/candidate-20260921-tray-left-01`.
  The publisher verified App/Core byte parity against tested win-x64 outputs.
- Local ZIP SHA256:
  `82E86BAFA4565F8D761550FACEB941E4F577C23D8F518F4687C5A07539B6B03F`.

## Acceptance boundary

Native popup probes run in separate processes on private desktops. They exercise
repeat opening, Escape handling, outside-window message filtering, native right
click and callbacks without disturbing the user's desktop. They do not establish
physical Explorer input, actual focus transfer, mixed-DPI positioning or live
installed-copy behavior. Initial per-thread desktop cleanup failed because IME
helpers persisted; process isolation resolved that harness issue.

At handoff, the original installed Dororong process (PID26312) was still running.
Normal exit was requested; it was not forcibly stopped. Strict package smoke,
local installer preparation and replacement remain pending. No installed files
were changed. Do not treat the local portable as the public v0.1.2 artifact:
product metadata remains 0.1.2, but this local candidate has a different hash.
Public release files and the tracked installer archive pin remain untouched.
No commit, push or public release was performed.

After normal exit: validate the new package, prepare a separately identified
local installer retaining all compiler/payload checks, back up the current
installation/data, then install and ask for actual left/right click, outside
click, Esc and menu-command acceptance. Do not replace immutable public assets.

## Installed-copy continuation

The user subsequently confirmed exit. PID26312 stopped normally (diagnostic
Stopped event), and preflight found no running pet. The following supersedes
the pending installation status above:

- Strict package validator and isolated native startup/single-instance smoke
  passed. Bundled runtime 8.0.31 and tested DLL/archive parity were verified.
- Built a separate local installer using the artifact-only `Build-Local.ps1` in
  `artifacts/installer/host-tray-left-20260921`. It pins this local ZIP and keeps
  the original compiler hash/signature, payload validation and build parity
  checks. Build evidence records the actual copied script. Tracked public
  builder and public release files were not changed.
- Local Setup SHA256:
  `485F3F5E7215CDB6A699FA26FD5943887B7296CED963BD170D94DD5DA4369B03`.
- Fresh backup in `artifacts/installer/host-tray-left-20260921` verified 472
  installation files, one data file, registration and Start-menu shortcut.
- Setup exit 0; no restart. All 469 payload hashes, ownership list, 472 total
  installed files, registration and shortcuts verified. Data stayed unchanged
  during installation; desktop shortcut opt-out preserved.
- Installed process PID59856 launched/responding; duplicate exited 0. Original
  user-data bytes preserved, with only log appends permitted. Own diagnostic
  Started event observed; no failure observed for this PID at handoff.

Evidence: `package-validation.log`/`build-evidence.json` under the local installer
candidate and `baseline.json`, `update-verified.json`, `launch-result.json` under
the host backup directory. No uninstall, force kill, commit, push or public
release occurred. Physical tray clicks, actual outside-click/Esc dismissal and
visible layout still require the user's confirmation on the running copy.

The user subsequently replied "잘된다" to the installed-copy verification
request. Local manual acceptance is therefore recorded as confirmed by the
user, not as automated UI verification. Mixed-DPI/clean-PC coverage remains
outside this check. No public release or push was requested or performed.

# Public v0.1.2 current-host upgrade — 2026-09-21

Status: installed upgrade, payload/data/shortcut checks and startup/duplicate
checks passed. Visual character/tray interactions and normal tray exit are
pending user confirmation; this is not full visual or clean-machine acceptance.

## Inputs and backup

- User approved backing up installed 0.1.1 and updating to public 0.1.2.
- Ran the actual anonymous GitHub download retained from release verification:
  `Doros-on-my-desktop-Setup-0.1.2-win-x64.exe`, SHA256
  `D3AEB4CB700B6B64823A6CA455D8913351AE648F7BD7ABE261D84D3357C4DB6C`.
- Non-elevated current-user target: `%LOCALAPPDATA%/Programs/JOEWRKS/Dororong`.
- User data: `%LOCALAPPDATA%/JOEWRKS/Dororong`.
- Evidence: `artifacts/installer/host-v012-upgrade-20260921` (ignored locally).
- Fresh verified backups: 472 installation files, one data file, Start shortcut
  and exported uninstall registration. Original backups remain intact.
- The preceding 0.1.1 candidate installer is also retained. Backup availability
  is not a claim that downgrade or rollback was exercised.

## Results

| Check | Observed result |
| --- | --- |
| Before update | Registered 0.1.1, old payload hashes matched; no pet running |
| Negative new-payload check | Correctly rejected old `Dororong.App.deps.json` |
| Public installer | Exit 0; log confirms success; no Windows restart |
| Installed payload | All 469 files match frozen 0.1.2 manifest; 472 total including ownership/uninstaller metadata |
| Product registration | Correct name, publisher, version 0.1.2 and stable installation/uninstaller paths |
| User data during update | Exact file count and hashes preserved |
| Shortcuts | Start target retained; desktop opt-out preserved |
| Silent update | No automatic product launch |
| Explicit installed startup | PID 26312 running; exact installed executable and bundled coreclr module paths |
| Duplicate launch | Exit 0; only original installed instance remains |
| Data after startup | Original bytes preserved; only log append allowed |
| Diagnostic log | PID 26312 `Started`, version 0.1.2; no failure event observed for that PID |

The outer pre-upgrade test initially expected the first changed file to be
`Dororong.App.dll`. Actual sorted inventory rejected `Dororong.App.deps.json`
first, correctly reflecting the version change. Correcting that test expectation
allowed the planned update; no product or installer change was necessary.

## Handoff and limitations

Computer Use initialized, but `list_windows` exposed no pet window and
`list_apps` exposed the installation with an empty window list. Its running
flag was false while process/module/log checks confirmed the process alive;
therefore discovery is not used as evidence that the app stopped. No guessed
window handle or alternative UI injection was attempted.

User confirmation is still needed for visible character, tray 0.1.2 label,
normal tray exit, tracking, pounce, cheek interaction and window/taskbar hanging.
The installed 0.1.2 process is left running for that confirmation.

No uninstall/reinstall, rollback, clean-machine, mixed-DPI or power-loss test was
performed in this step. No code signing, trust/security-setting change, new
release, commit or push occurred. Product source and frozen release files are
unchanged. This report and TASKS.md are local documentation changes only.

Subsequent history: the user confirmed the character was visible, then requested
tray left-click support. PID26312 later stopped normally before installing that
local candidate. See [tray left-click verification](2026-09-21-tray-left-click.md)
for replacement and user acceptance; the pending items above describe the
original public-upgrade handoff, not the current installed process.

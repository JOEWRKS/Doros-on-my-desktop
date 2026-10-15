# Automatic sleep option — 2026-10-14

Scope and checks: [TASKS](../../TASKS.md#automatic-sleep-option--2026-10-14).
No art changes. Two existing90ms tucked/crouched wake frames are reused when
disabling settled sleep. Entry-in-progress follows the prior Sleep→Idle behavior.

## Local candidate

Product/installer candidate: `candidate-20261014-sleep-option-14`, unpublished0.1.3.
ZIP SHA256: `8B135B76436F7BBBE0ACBF644F39A04A90DAD95A6876E5680CD7DB8E3FCEF8DD`.
Publisher checked tested App/Core parity. Full App1193 predates the narrow Idle
wake correction; final affected23 ran against the final binaries before publishing.
This distinction is intentional, not a claim that the final build reran all tests.

Setup SHA256: `491EF2C3F5B1691C097CB5456CBF7C45D51E19C96140FDB86004D6A11DC94B97`.
App DLL: `EF1603705D1CB6073E9D0DBFF8562EAC94E8D4EC920B29A9A9FEB47319E6274C`.
Core DLL: `B86415D930B7A8ED0DFD4A1270837F0B75D26C4F2954E6EE82888DD2CE8E19E1`.
Package round-trip/native start/duplicate/close and installer builder validation
passed. Existing candidates were not overwritten. Clean3-file handoff:
`artifacts/delivery/0.1.3-20261014-sleep-option/`.

## Actual installation

Evidence/backup: `artifacts/installer/host-sleep-option-update-20261014/`.
User tray exit and empty process inventory confirmed before replacement. Previous
candidate13 installation, data, shortcut, registration and recovery setup backed up.
Update exited0. All470 payload files and ownership matched; existing user data,
start-menu target, desktop preference and Windows startup OFF preserved.

Installed service tested through two independent PowerShell processes: save OFF,
fresh-process read OFF, save ON, then restore original absent preference. This is
real registry persistence, not a physical menu click or full application restart
while OFF. Installed app then launched as PID392620 with one Started event and no
other diagnostic event in inspected records. Final automatic sleep defaults ON;
Windows-login startup remains OFF. User acceptance of the new toggle is pending.
Subsequent installed-option acceptance and deferred next-day startup test are
recorded in [TASKS](../../TASKS.md#automatic-sleep-option--2026-10-14).

## Settings and boundaries

`HKCU\Software\JOEWRKS\Dororong\Preferences\AutomaticSleepEnabled` is a DWORD
0/1; absent means ON. This user preference survives updates/removal, like logs.
Automated preference tests use injected stores, not the user's registry. They
cover a new reader after saving, invalid values and save failures. Actual next-process
read and installed menu acceptance are recorded separately when observed.

Native isolated-desktop tests cover all8 snack/startup/sleep combinations for
each menu, one-shot command callbacks, outside/Esc dismissal, reopening and4
screen corners. They do not establish all DPI/monitor configurations or high-contrast
visual conformance. Inspected default-DPI renders are under
`artifacts/ui-menu-sleep-option-20261014/native-proof/`.

F1 real Windows logon, R3 other environments and R4 rights remain open/held.
No commit/push or public release in this feature step.

The dated F1 logon limitation above is superseded by the user's next-day
[actual-startup acceptance](../../TASKS.md#login-startup-acceptance-and-private-main-checkpoint--2026-10-15).
Other-environment and rights boundaries remain unchanged.

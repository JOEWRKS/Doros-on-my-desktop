# Startup and uninstall host validation — 2026-10-14

## Target and authority

User requested the remaining startup/uninstall verification and confirmed normal
tray exit. Current host: Windows10 build19045, non-elevated current-user install.
Source checkpoint: `1e41ec553c03ab2ed28e2a1c07e793e9d0c11c16`.
Unchanged candidate13 and its checksums: [package record](2026-10-14-figma-tray-support.md#final-local-package).
No code, assets or installer were rebuilt. No logout/reboot or publication.

## Execution

Ignored local evidence: `artifacts/installer/host-startup-lifecycle-20261014/`.
`Verify-Host.ps1` runs explicit Backup, Startup, Uninstall, Reinstall and Launch
phases. It rejects running pets, unexpected identity/paths, reparse points,
unreviewed install extras, changed recovery files and existing attempt evidence.
Installer processes have bounded waits and are not force-terminated on timeout.

| Check | Observed result |
| --- | --- |
| Normal exit / backup | Prior installed PID380544 recorded Stopped.473 installation files,1 data file, registration, shortcut and verified recovery setup backed up. |
| Startup registration | Installed `StartupRegistration` executed its real registry read/write methods via reflection: OFF→ON→OFF→ON passed. Explicit installed EXE path supplied because the probe runs in PowerShell. This is not a physical menu click. |
| Actual uninstall | Exit0 and completion log; product directory, uninstall registration and shortcut removed. Exact owned `JOEWRKS.Dororong` Run entry removed. |
| Data preservation | All user-data files byte-identical across uninstall and reinstall. No user-data deletion. |
| Same-version reinstall | Exit0;470 payload files and ownership manifest match candidate13. Start-menu target restored, desktop shortcut still absent, startup still OFF. Installer did not auto-launch. |
| Installed launch | PID43692 started from the verified installed EXE matching the saved Run target; one live instance and Started event, no additional diagnostic event in the inspected process records. Original data bytes preserved; only the log appended. |

Results: `baseline.json`, `startup-result.json`, `uninstall-result.json`,
`reinstall-result.json`, `launch-result.json`, installer/uninstaller logs.
Recovery copies remain local. The removed installation was restored with the
same verified installer; no manual recursive deletion was used.

## Remaining boundaries

- Actual Windows logon startup is **not tested**. Launching its saved executable
  does not exercise Explorer's logon processing, startup-app policy or timing.
- Final setting restored to original **OFF**, installed app left running.
  Coordinate a real login test after the user saves their work, enable through
  the product menu, then check the new-session process and diagnostic Started
  event before opening it manually. Restore the user's chosen setting afterward.
- A foreign Run entry was not injected into this live profile for an additional
  uninstall cycle. Existing conflict unit tests and exact-match code inspection
  are not a new end-to-end foreign-entry preservation test.
- No clean-PC, Windows11, mixed-DPI, reboot or rights/publication claim. Existing
  held gates remain unchanged. No commit/push performed in this verification.

The dated real-logon limitation is superseded by the user's next-day
[actual-startup acceptance](../../TASKS.md#login-startup-acceptance-and-private-main-checkpoint--2026-10-15).
This does not expand the host or uninstall-case coverage above.

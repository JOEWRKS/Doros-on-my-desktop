# Accepted Windows installed replacement — 2026-10-28

User explicitly requested replacing the Windows installed edition with the
current accepted build. This authorizes the local update and relaunch, not
commit/push/public distribution or a Mac change.

## Exact source and installer
Used the immutable `candidate-seated-head-occlusion-20261026/runtime`; no App/Core
rebuild or artwork/motion edits. Portable SHA256
0D607A095B27DA750DFE8729344516E76D9E872873747F2B55F6038D743ACCDE.
App SHA256 B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0.

Updated the builder's exact approved archive pin and refusal/build fixtures,
retaining compiler hash/provenance/Authenticode, payload and fresh-output guards.
Missing/foreign/mutated/existing-output refusal checks passed.
Native private-desktop smoke initially timed out after WM_CLOSE. The harness
posted WM_CLOSE to every owned top-level HWND, including hidden framework/helper
windows. Changed the harness to close only visible owned windows; same frozen
binary then started, rejected a duplicate with exit0 and closed normally with
exit0. Exact hidden-window failure sequence was not traced; no product shutdown
bug/fix claim. Test-only cleanup affected only its owned processes, never a user pet.

Final package validation passed: pinned self-contained.NET8.0.31, RID App/Core
hashes, apphost/icon/metadata, ZIP round-trip and native startup/duplicate/close.
Installer path: `artifacts/installer/candidate-accepted-seated-20261028/Doros-on-my-desktop-Setup-0.1.3-win-x64.exe`.
Installer SHA256 FA33A02ED22301235D53FDA5B9FE187238A8B733AD06507805A5CD8C351712BF. Version0.1.3, unsigned,
470payload files. Same-version local replacement; no new public release claim.

## Local operation and verification
No existing Dororong process was running before either backup or update.
Evidence/backup: `artifacts/installer/host-accepted-seated-update-20261028/`.
`Update-Host.ps1` pinned install identity/path and rejected reparse points,
then copied/hash-verified every old installation and data file, saved Start-menu
shortcut/registration/preferences and the previous recovery installer. No old
candidate/portable folder/backups were deleted. Recovery was prepared, not exercised.

Current-user silent Setup exited0. All470installed payload hashes and the ownership
manifest match the accepted candidate. All preexisting user-data hashes and registry
preferences remained unchanged during installation. Desktop shortcut absence retained;
Start-menu shortcut targets the installed executable.

Automatic startup was ON but pointed to the old downloaded r104 portable build.
Verified that exact command and target App hash109FBA19…965A81, preserved ON, and
migrated only `HKCU/Software/Microsoft/Windows/CurrentVersion/Run/JOEWRKS.Dororong`
to the installed executable plus ` --login-startup`. Other Run entries untouched.
Preferences remain SizePercent150 and AutomaticSleepEnabled0. Expected diagnostic
log append occurred after launch; prelaunch preservation was checked separately.

Launched `C:/Users/tjdwo/AppData/Local/Programs/JOEWRKS/Dororong/Dororong.exe`,
PID64404/startUTC2026-10-28T14:47:19.3126211Z. After8seconds Responding=true,
one process and one visible nonzero owned window; matching accepted App hash.
Diagnostic log records Started, with no observed launch failure. Left running.
This is startup/window verification, not a fresh visual acceptance of every motion.

## Limits / next
Local installed replacement complete. Actual new Windows login, restart, clean-PC
install and uninstall were not performed. No unrelated product/settings changes,
public upload or Mac modification. Next existing roadmap gate is native Mac test10
acceptance; no repeated unchanged Windows memory/frame test needed for installation.

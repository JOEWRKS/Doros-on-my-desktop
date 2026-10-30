# Ribbon correction installer update — 2026-10-30

User requested updating the installer after receiving the latest portable/Mac
links, then explicitly authorized graceful closure and reopening of the existing
pet while running mandatory validation. This task builds/publishes the installer;
it does not silently install it on the host.

## Built and published

[Windows Setup direct download](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-ribbon-20261030/Doros-on-my-desktop-Setup-0.1.3-win-x64.exe)

- Version0.1.3;77,655,110bytes; SHA256
  `8f117c36b9aa4079bcbeffa91ec491c02370b94d17ed4c5f88d0d649799effbe`.
- Source package: exact accepted ribbon portable ZIP2725f07ecc6b1642…9b7574d,
  `artifacts/product-shell/candidate-ribbon-connected-20261030/runtime`.
-470product files, unchanged relative to the verified portable. Builder's approved
  archive pin and refusal/build fixture updated. Four refusal cases pass.
- Pinned Inno7.1.0 compiler hash/provenance and valid compiler Authenticode checked;
  before/after staging/compilation payload hashes, generated manifest, icon,
  installer metadata and Setup hash checked. Generated Setup remains unsigned.
- Required package validation passes: self-contained.NET8.0.31, RID App/Core
  identity, apphost/icon/metadata, ZIP round-trip and isolated native smoke.
- Build output/evidence: `artifacts/installer/candidate-ribbon-connected-20261030/`.
- Appended Setup and `installer-SHA256SUMS.txt` to the existing
  `windows-ribbon-20261030` prerelease; portable/Mac/old releases preserved.
  Both new assets anonymously downloaded and size/SHA256 matched; upload digests
  also match. Evidence: `artifacts/installer/ribbon-delivery-20261030/release/`.

## Failures distinguished and resolved

Initial startup gate ran while installed PID51968 held the per-user mutex.
An isolated duplicate reproduction exited0 with the existing owner still alive:
this was normal duplicate prevention, not a product startup crash. User then
authorized graceful close/reopen. `CloseMainWindow()` did not find the tool window;
an owned, visible HwndWrapper was enumerated and sent WM_CLOSE after PID/start/path
checks. No forced user-app termination was used.

After that, the old validator's fixed2.5second startup probe could see a running
process before its window existed, so its immediate WM_CLOSE assertion failed.
The original app was reopened in the cleanup path. The validator now waits up to
30seconds for an actual visible owned window, asserting the owner remains alive.
It still requires visible startup, duplicate exit0 and normal WM_CLOSE exit0.
Final run observed window readiness after an additional701ms, then all native
checks passed. This is a test-readiness correction, not an app timing/behavior change.
Failure logs are retained alongside `build-final.log` under the delivery root.

## Restored app and limits

After successful validation/build, the same existing installed executable was
reopened as PID19480/startUTC2026-10-30T08:28:45.3190224Z. Responsive with one
visible owned window. Its EXE/App hashes match the pre-close installation; App
SHA256`B09899C12C1844B9F08F5EC4C67A3B189C33D2ADC01D65D02E8C55517E306DE0`.
No installation/settings/startup update was performed. Normal diagnostic logging
may append on app launch. New Setup's host install/upgrade/uninstall/reboot cycle
was not executed; compiler/payload and packaged-app smoke checks are narrower.

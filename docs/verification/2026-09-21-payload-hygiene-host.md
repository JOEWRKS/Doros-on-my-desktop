# Payload hygiene: current-host lifecycle acceptance — 2026-09-21

The maintainer explicitly approved testing update, uninstall and reinstall of
the local candidate against the installed product. The new unsigned installation
is retained; public GitHub assets, source HEAD and signing status were not changed.
This is a non-elevated current-user host test, **not a clean-machine test**.

## Frozen inputs and backup

- Candidate: `artifacts/installer/candidate-20260921-payload-hygiene-01/Doros-on-my-desktop-Setup-0.1.1-win-x64.exe`.
- Candidate SHA256: `2098CDAFB486EEA3DEB3F402666701273108ED0B5F89F939DBAD2D76DE29BD7D`.
- Previous installer SHA256, checked for recovery availability:
  `5D8C747D8BB8A1B9BCD2B9AA83AD0726426A40112206592AEE070BC9D264313B`.
- Target: `%LOCALAPPDATA%/Programs/JOEWRKS/Dororong`.
- User data: `%LOCALAPPDATA%/JOEWRKS/Dororong`.
- Evidence and recovery copies: `artifacts/installer/host-payload-hygiene-20260921`.

Before mutation, the existing 0.1.1 identity and all472 old payload hashes matched
the previous frozen manifest. The backup contains475 installation files (payload,
ownership manifest and uninstaller metadata), one data file, the Start shortcut
and exported product uninstall registration. Tree and shortcut hashes were
verified; backup hashes were checked again before subsequent phases. Desktop
shortcut ownership was0 and remained disabled. No pet process was running.

No unowned installation extras or reparse paths were accepted. The historical
Sandbox-only harness was not modified or invoked with its gate bypassed. The
task-specific host harness remains beside the local evidence, not in the product.

## Observed acceptance

| Check | Result |
| --- | --- |
| New-payload check against old installation | Expected refusal at old App DLL hash; no mutation |
| Same-version0.1.1 update with new candidate | Setup exit0; all469 new payload hashes and ownership manifest match |
| Obsolete owned files | Both PDBs and loose canonical-source PNG removed; originals retained in backup |
| Registration and shortcuts after update | Correct per-user name/version/path; Start target correct; desktop remains absent |
| Silent update/reinstall | No automatic product launch; no Windows restart requested |
| Uninstall | Exit0 observed before postcondition check; completion log confirms success; installation root, registration and shortcut removed |
| Data during update/uninstall/reinstall | Exact baseline file count and SHA256 preserved |
| Reinstall | Setup exit0; all469 payload files, ownership, registration and Start target restored |
| Installed startup | New installed process17744 remained running |
| Duplicate launch | Second instance exited0; only the first installed instance remained |
| Data after explicit startup | Original bytes retained; append allowed only for logs |

The initial uninstall postcondition checked the directory too soon after the
launcher exited and reported `Installation files remain`. The uninstaller log
then recorded successful temporary-file cleanup and the entire target disappeared
without intervention. The artifact-only verifier now waits up to10seconds for
actual root absence instead of assuming launcher exit means cleanup is finished.
A read-only `VerifyUninstalled` phase then passed before reinstall. No files were
force-deleted, no security checks were skipped, and no product fix was required.

Evidence files include `baseline.json`, `update-process.json`,
`update-verified.json`, `uninstall.log`, `uninstall-verified.json`,
`reinstall-process.json`, `reinstall-verified.json` and `launch-result.json`.
The first uninstall's process exit was observed by the harness but did not receive
a separate process receipt because the later immediate postcondition failed.

## Limits and handoff

- Same-version repair/update was exercised, not a new version-number migration.
- No production code changed in this acceptance step; source suites were not
  rerun. Their earlier results and packaged-byte parity are in
  [package verification](2026-09-21-payload-hygiene.md).
- No assertion of visual behavior, mixed-DPI, clean-machine, power-loss or
  failure rollback coverage. Recovery copies were verified but rollback was not
  needed or exercised.
- Code signing remains intentionally deferred. Asset-rights verification is a
  separate unresolved requirement, not established by these tests.
- No commit, push or public release performed. Backups are retained for recovery;
  the local candidate is installed and running.

업데이트·제거·재설치와 중복 실행 방지 검사를 통과했습니다. 사용자 데이터와
바로가기 설정은 유지됐고 새 검증본을 설치해 실행해 둔 상태입니다. 원본 백업은
보관하며, 이 결과를 다른 PC 전체의 호환성이나 공개 배포 완료로 간주하지 않습니다.

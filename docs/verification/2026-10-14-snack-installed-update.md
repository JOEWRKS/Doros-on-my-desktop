# Accepted snack version — installed update

User authorized installing accepted candidate08 and deferring commit/push until
the NIKKE-inspired UI work. No publication or rights permission is implied.

- Pinned `tools/Build-Installer.ps1` to the exact accepted portable SHA256
  `17EE3E449884B8BD974EF506F37DBA8758CFD13D70DA2A9F24F2928BA4C7F35D`;
  updated build-test fixtures to that candidate. Compiler/signature, payload,
  fresh-output and exact-hash checks remain intact; no arbitrary hash override.
- Build refusal tests pass: missing input, foreign compiler, modified archive,
  existing output. Builder's package validation passes (RID parity, runtime8.0.31,
  archive roundtrip and isolated native lifecycle).
- Setup: `artifacts/installer/candidate-20261014-snack-sway-08/Doros-on-my-desktop-Setup-0.1.3-win-x64.exe`.
  SHA256 `851BDF736CDBE7B8B6527775D74DA5880CFB78447CFE7DC1681A56A2C85147BB`.
  Unsigned, version0.1.3,469 payload files. Build/source/compiler hashes in its
  `build-evidence.json`; same-version local update, not a new public release.
- Host evidence: `artifacts/installer/host-snack-update-20261014/`. Pinned
  `Update-Host.ps1` verified no running pet, exact installed identity/location,
  no reparse entries, then backed up installation/data/registration/shortcut and
  prior recovery Setup with hash verification. Recovery was not exercised.
- Setup exit0. All469 installed payload hashes and ownership match candidate08.
  User-data hashes and desktop opt-out preserved; Start shortcut points to
  `C:/Users/tjdwo/AppData/Local/Programs/JOEWRKS/Dororong/Dororong.exe`.
- Existing enabled HKCU `JOEWRKS.Dororong` Run entry pointed to candidate08.
  Kept enabled and migrated only that exact command to installed executable;
  unrelated commands would be refused. Baseline/result JSON preserves both values.
- Launched installed executable PID344416, Responding=true. This verifies host
  update and startup, not actual login restart, uninstall cleanup, clean-PC or
  new UI acceptance. No app source/artwork changes in this update task.

Existing R3/R4 holds remain. UI source research lives in root DESIGN.md; source
inspection is not a finished visual proof or user acceptance. Commit/push not run.

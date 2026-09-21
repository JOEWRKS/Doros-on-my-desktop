# 0.1.1 rebrand and installer verification — 2026-09-21

## Deliverable

- Display name: **도로롱이 내 컴에?! / Doro's on my desktop?!**.
- Repository: `JOEWRKS/Doros-on-my-desktop` (renamed from `Doropet`).
- Installer: `Doros-on-my-desktop-Setup-0.1.1-win-x64.exe`, 62,778,062 bytes.
- SHA256: `5D8C747D8BB8A1B9BCD2B9AA83AD0726426A40112206592AEE070BC9D264313B`.
- Unsigned preview, per-user Windows x64, self-contained .NET 8.0.31.
- Internal `Dororong.exe`, AppId, install/data directories and instance identity
  remain stable for upgrade compatibility. Windows shortcut omits `?`.
- Release assets are separate from GitHub-generated source-code archives.

## Automated checks

- Core: **241 passed**, no failures/skips.
- App: **1,072 passed**, no failures/skips (`rebrand-verified.trx`).
- Isolated initial-render checks: **9 passed** after test bootstrap correction.
- Payload safety, builder refusal and compiled shared installer policy: PASS.
- Package: 472 files, complete archive round-trip and App/Core tested-byte parity.
- Native private-desktop smoke: bundled CoreCLR loaded, duplicate exit0,
  primary exited0 through normal WM_CLOSE. This is not a clean-machine test.
- Installer: pinned compiler signature/hash, staged-file parity, explicit inventory,
  PE metadata, unsigned state and final SHA256 verified.
- Quick start, project MIT license and bundled runtime notices included.
- Independent read-only review: no actionable code defect identified.

A repeat run exposed existing order-dependent test initialization: a static image
bank could parse a pack URI before WPF initialized. The test assembly now runs
the `Application` static initializer before tests. The isolated failing class and
full suite pass; no production rendering or animation code changed.

Renaming the remote changed SourceLink build metadata. The initial package/test
hash mismatch was refused, then the final published bytes were retested.

## Host lifecycle acceptance

Non-elevated current-user installation; original 470 installation files, user
data, registry and Start shortcut backed up before mutation.

| Check | Result |
| --- | --- |
| New Start shortcut name occupied by a test sentinel | Installer refused; existing app/data/old shortcut/sentinel unchanged |
| Actual old-name0.1.0 → new-name0.1.1 upgrade | PASS; all472 payload hashes, data, registry and shortcut target verified |
| Legacy shortcut removal | PASS; only owned old name removed |
| Directory ownership | Original entries retained, new `licenses` directory added |
| Silent install | No autolaunch or unexpected desktop shortcut |
| Uninstall | Owned payload/registration/shortcuts removed; user data unchanged |
| Reinstall with desktop shortcut selected | PASS; desktop link verified, data unchanged |
| Same-version repair with desktop shortcut unselected | PASS; owned desktop link removed, Start link retained |
| Installed startup and duplicate launch | PASS; one installed process remained, duplicate exited0, original data prefix preserved |

The package smoke appended only its Started/Stopped diagnostic events; original
log bytes were checked before refreshing the lifecycle baseline. Lifecycle
actions preserved that complete refreshed data snapshot. Test collision files
were removed only after exact-content verification. Installation backups remain.

## Provenance and limits

Public release: [v0.1.1](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/tag/v0.1.1),
tagged source commit `7b77d01b35ae51870810cf2c3dd7c94245bd47d8`.
Anonymous API access, full Setup EXE download and checksum download all passed.
The downloaded installer matches the SHA256 above and GitHub's asset digest.
The public README's direct installer link and source-ZIP warning were verified.

- Product candidate: `artifacts/product-shell/candidate-20260921-rebrand-02`.
- Installer candidate: `artifacts/installer/candidate-20260921-rebrand-01`.
- Guarded host evidence: `artifacts/installer/host-rebrand-20260921`.
- Portable archive SHA256: `980BA470180226A450277B25F915B31FC63B2ABC346B5B5A3462AAA656E66E31`.
- App DLL SHA256: `5E2814961BBAC75A0B6590695D8767D082E297868CD08E715CC92C14F0B83B04`.
- Core DLL SHA256: `EA065798A0B6389CAA653B1B907AA1135D783236EC6C260FF46E1D3312FEE203`.

This host originally had no desktop shortcut. Old0.1.0 upgrades with desktop
ownership enabled were reviewed but not host-tested; new0.1.1 desktop opt-in/out
was exercised. Clean-machine, mixed-DPI, broader failures and power-loss recovery
are not certified. Character-art distribution permissions remain unverified;
code licensing does not grant rights to the character assets.

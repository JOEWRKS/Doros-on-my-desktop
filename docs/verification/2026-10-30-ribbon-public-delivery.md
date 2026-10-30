# Accepted ribbon builds / direct downloads — 2026-10-30

User requested direct download links for both accepted builds. Published new
prereleases without replacing existing assets or changing the latest designation.

- [Windows portable ZIP](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-ribbon-20261030/Doros-on-my-desktop-Portable-0.1.3-win-x64.zip)
- [Mac Apple Silicon test12 ZIP](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/macos-r104-test12/Doro-Mac-arm64-r104-test12.zip)

Windows tag `windows-ribbon-20261030` targets accepted source92fa865;
Mac tag `macos-r104-test12` targets package commit4c4f840.

| ZIP | Bytes | SHA256 |
| --- | ---: | --- |
| Windows portable |99584844|`2725f07ecc6b1642bb5812af9b0401482e6e463c0650003e39416b5759b7574d`|
| Mac test12 |97926910|`25d46ab540f118de3aa32b44d73826c51738005375aefe870442f8f189a7ed43`|

Both ZIPs and their checksum files were anonymously downloaded from the direct
URLs and matched local size/SHA256. GitHub upload digests also match. Evidence:
`artifacts/release-ribbon-20261030/plan.json`, `*-published.json`,
`anonymous-verification.json`, `windows-package-check.json`.
Connected GitHub tools lack release-asset upload; the prior verified API script
was reused with fresh pinned plans and existing credential helper, without
writing credentials to evidence files.

## New Windows package

Built a fresh self-contained win-x64 candidate using the existing publisher and
pinned.NET8.0.31. Initial framework-dependent test attempt aborted because this
host lacks installed8.0.31; retained `connected-win-x64.trx` is not a pass.
Self-contained retry passed5/5 focused tests (`connected-win-x64-final.trx`).
Publisher verified exact App/Core identity against these RID-specific test DLLs.
The packaged renderer's16PNGs covering72pose samples match the accepted reference
byte-for-byte. ZIP CRC and all470file contents verified against package directory.
No app was launched/replaced and no settings changed during this delivery.

## Mac and boundaries

The exact already-verified test12 ZIP was uploaded unchanged; prior292host tests,
14packaging tests and4,848frame parity are described in
[test12 verification](2026-10-30-mac-test12-ribbon.md). Publication adds no actual
Mac execution evidence. Native M2/Retina/Gatekeeper/CPU/native-memory checks remain
NOT_TESTED. Windows download is portable, not a newly updated installer.

Quit the old app before extracting/running the Windows ZIP. On Mac, Finder-extract
and move Doro Mac Test.app to Applications before opening; included README retains
the security/first-launch guidance. No command execution or log request required.

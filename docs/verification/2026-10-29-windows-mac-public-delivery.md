# Accepted Windows and Mac test10 public delivery — 2026-10-29

User explicitly requested separate Windows/Mac commits, push and direct downloads.
Publication began October28 KST; anonymous download verification finished October29.
No product rebuild or running-app/settings change was made during delivery.

## Source and release identity

- Branch: `feature/macos-r104-arm64`, pushed and remote identity verified.
- Windows packaging/evidence: `655c896b87ce2c452e556fed4461fee5524ca575`.
  Accepted executable product code remains `deb5cae17174ba3748baec2e89f35abe9f456480`.
- Mac implementation/evidence: `91591d5f6a84df8d169f46bb85ceddc1e60c73b2`.
- Public prereleases: `windows-accepted-20261028` and `macos-r104-test10`.
  Remote tags resolve to the respective full commit hashes above; neither is a draft.
  Existing releases/assets and latest designation were not replaced.

## Anonymous downloads verified

| Artifact | Bytes | SHA256 |
|---|---:|---|
| Windows Setup | 77649395 | `fa33a02ed22301235d53fda5b9fe187238a8b733ad06507805a5cd8c351712bf` |
| Windows portable | 99585828 | `0d607a095b27da750dfe8729344516e76d9e872873747f2b55f6038d743accde` |
| Mac test10 ZIP | 97925957 | `cc3868867d3146aa9fb2e28ee4f331f5a6569b4d83a26506aca6f9a243cc084d` |

- [Windows Setup](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-accepted-20261028/Doros-on-my-desktop-Setup-0.1.3-win-x64.exe)
- [Windows portable ZIP](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-accepted-20261028/Doros-on-my-desktop-Portable-0.1.3-win-x64.zip)
- [Mac Apple Silicon test10 ZIP](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/macos-r104-test10/Doro-Mac-arm64-r104-test10.zip)

All three binaries and both SHA256SUMS files were downloaded without authentication
and matched their local source bytes/hashes. GitHub upload state/size/digest also matched.
Local evidence: `artifacts/release-accepted-20261028/plan.json`, platform-specific
`*-published.json`, and `anonymous-verification.json`. No credentials are retained there.

## Verification boundaries

Reuse the [Windows installation evidence](2026-10-28-windows-accepted-installed-update.md)
and [Mac port evidence](2026-10-28-mac-test10-accepted-motion-port.md); publication did
not rerun or broaden those tests. Actual Mac M2 execution, native CPU/memory, Retina,
external/clamshell and landing smoothness remain NOT_TESTED for test10.
Windows is unsigned; Mac is ad-hoc only, without Developer ID/notarization.
Public distribution authorization does not establish third-party asset rights.
Unrelated preexisting AGENTS.md and .joeness/setup-state.json edits were excluded.

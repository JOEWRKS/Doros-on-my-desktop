# v0.1.3 local candidate verification · 2026-09-21

**Unpublished. Rights clarification and environment acceptance remain open.**
Prepared from main `67f7504da45135cbd9ccfcd48951957a4952fd4e` plus local
version/packaging/documentation changes. No new tag, commit, push or release.
Existing public v0.1.2 and archive were not changed.

## Verified

- Expected version RED: 4 failures, 28 passes before metadata update.
- Core suite: 241 passed, zero failures/skips.
- Initial App suite: 1080 passed, one stale numeric assembly-version expectation.
  Corrected expectation to 0.1.3.0; full rerun: 1081 passed, zero failures/skips.
  Evidence: `tests/Dororong.App.Tests/TestResults/v013-final-verified.trx`.
- Product package: exact tested App/Core RID byte parity, metadata/icon/apphost,
  bundled Core/Desktop/host 8.0.31 and ZIP round-trip passed.
- Native smoke on an isolated desktop: package app loaded its own runtime,
  duplicate exited 0, owned primary exited 0 through WM_CLOSE.
- Installer payload suite: inventory, path/reparse protection and sentinels passed.
- Installer All suite: refusal cases, compiled policy harness, staged payload
  parity, metadata, unsigned state and final hash passed; 469 payload files.

## Frozen local artifacts

Under `artifacts/product-shell/candidate-20260921-v013-01/`:

`Doros-on-my-desktop-Portable-0.1.3-win-x64.zip`

SHA256: `C8C82D2301F28B0D9252AE2F07DE88294886062BF4338BF431A3FBDBFBB60313`

Under `artifacts/installer/candidate-20260921-v013-01/`:

`Doros-on-my-desktop-Setup-0.1.3-win-x64.exe`

SHA256: `FB7868E7CD6E90DC79DDE1A08BB47AAB15881DBB0D4196E945EF6DEB5521D6A2`

Local `build-evidence.json` records source/generated-input hashes, compiler
provenance, validator/compiler exit 0 and payload inventory. Package smoke
output is in `package-validation.log`. Build-Installer.ps1 pins this ZIP.

## Not verified / publication boundary

- This v0.1.3 installer was not installed, upgraded or uninstalled on the host.
- No separate physical PC, clean account, mixed-DPI or sleep/resume acceptance
  was performed for this candidate. See [test matrix](../ENVIRONMENT-TESTING.md).
- Unsigned; no certificate purchased or security settings changed.
- Image source/permission and official-policy coverage remain unresolved.
  See [rights review and unsent inquiry](../RIGHTS-REVIEW.md).
- Technical success is not distribution permission. No public upload or
  rights-holder contact occurred, and existing public files were not removed.

# Plan

This is the sole active task ledger. Historical detail is preserved in the
[pre-cleanup archive](docs/ARCHIVE.md), including the complete previous ledger.

## Current product state

- Public installer: v0.1.2 preview, bundled .NET 8.0.31, unsigned.
- main additionally supports the tray menu on left click; user confirmed the
  installed local candidate works. Public v0.1.2 assets do not include this change.
- Previous full verification: Core241/App1081 passed, zero failures/skips.
- Code, controls and bilingual documentation committed/pushed to main.
- Local installed-copy acceptance: [tray report](docs/verification/2026-09-21-tray-left-click.md).
- Public release and installer checks: [release report](docs/verification/2026-09-21-v012-release.md).
- Clean-PC/mixed-DPI coverage and character distribution permissions remain
  incomplete. Signing deferred. No new release or installer upload authorized.

## Repository cleanup — approved 2026-09-21

User requested a clean main while retaining historical files separately.
Scope: remove obsolete artifacts/internal notes from Git tracking, not from
the local disk; preserve current source, assets, tools, tests and documentation.

- [x] Verify clean main matches origin at abe15b56e69c5da205fcd55f496a1ec2e084b373.
- [x] Push archive/pre-cleanup-2026-09-21 at that exact commit and verify remote.
  It preserves all126 tracked artifact files,6 internal notes and old TASKS.md.
- [x] Check exact legacy paths are not referenced by active src/tests/tools,
  README or BUILD guide. Generated artifact output paths remain supported.
- [x] Untrack artifacts/ and .superpowers/; all132 local files remain present.
  Ignore both output areas, link the archive and condense the active ledger.
- [x] Export the staged tree to a fresh isolated folder without historical
  artifacts; run full Core/App tests there, check current-source hash parity.
  Exported tree0a338f455a31f1062cadab208bea775c9516908c to a fresh temporary
  source folder. Core241/App1081 PASS, zero failures/skips; App clean-main.trx.
  src/tests/tools/installer Git trees match the archive exactly. All132 local
  archived files match their original Git blobs; zero tracked artifacts remain.
- [ ] Commit/push cleaned main and verify remote parity, zero tracked files in
  the excluded areas, preserved archive commit, and clean working tree.
  Ready for normal main push; final transport/parity results reported in chat.

No history rewrite, force push, release changes, local backup deletion or
product behavior changes are part of this cleanup.

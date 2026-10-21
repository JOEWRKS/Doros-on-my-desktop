# Accepted Windows r104 delivery — 2026-10-21

## Scope and source

User requested the existing final Windows version committed/pushed and a direct
download. Work is isolated on `release/windows-r104` in the existing clean Windows
release worktree. Base: `3813312379fd7122f72a031f692237560b637ec3`, already verified
on remote `feature/macos-test03`. Pending Mac test02 changes remain untouched and
are not included in this delivery. Existing public v0.1.3 assets are not replaced.

Only application-source difference from the accepted checkpoint is the ordinary
launch fallback to `--high-resolution-grab=1.5` in MainWindow. Previously the
accepted renderer required that explicit diagnostic-era launch argument. Saved
size still takes precedence, and explicit arguments retain their previous handling.
No art, renderer, Core simulation, timing, interaction or slider implementation
changed. App assembly metadata remains 0.1.3; release tag/filename identify r104.

## Build and evidence

`tools/Publish-Product.ps1` produced a self-contained Windows x64 package with its
existing pinned Core/Desktop/apphost 8.0.31, licenses and portable instructions.
No installer, registry setting, existing pet or PC power state was changed.

- ZIP bytes: 99,561,076.
- ZIP SHA256: `27BB7D390F6042B4972D152F4B9EE1E2EE8949DF9CEA55BA61AB3A1549DB0103`.
- App DLL SHA256: `109FBA197CAC5FFDD795F43C25353CEEB59A4982D87450E66058D1B967965A81`.
  Matches both the full-suite input and targeted launch-test output.
- New ordinary-launch test: 1 passed, 0 failed/skipped (`artifacts/windows-r104/tests/launch.trx`).
- Full App suite: 1,534 passed, 0 failed/skipped (about 11 minutes;
  `artifacts/windows-r104/tests/windows-r104.trx`). Together with the separately
  built new startup case, 1,535 cases passed against the matching App DLL.
- Existing package validator reached its native smoke after metadata, licenses,
  runtime pack, App/Core test DLL parity, apphost binding and ZIP round-trip checks.
  It then timed out waiting for owned PID15068 to exit on an inactive private
  desktop. The harness cleaned up its own process. **Overall validator exit 1,
  not a full package PASS.** Exact cause of that private-desktop timeout was not
  established and is not labelled a fixed application defect.
- Separate active-desktop run using `tools/Test-R104ActiveDesktop.ps1`: PASS.
  Owned PID12180 loaded the packaged runtime, a duplicate exited 0, and PID12180
  exited 0 after WM_CLOSE. Runtime coreclr hash:
  `BBBCAEDB369617695280CAAA60CDA1ABA4FA3C5B557532A89D9F4A9174A5B692`.
  This is launch/lifecycle evidence, not direct visual inspection of all motions.
- No heap capture, forced GC, working-set trim, input simulation or long soak.

Artifacts live under `artifacts/product-shell/candidate-r104-final` and
`artifacts/windows-r104` in the Windows delivery worktree. The final ZIP is a
byte-identical rename to `Doros-on-my-desktop-r104-win-x64.zip`.

## Limits and publication

Other-PC, clean-user, mixed-DPI and native visual regression checks are not newly
claimed. Prior user acceptance of r104 remains the visual/interaction authority.
No new installer lifecycle test is implied by this portable release. Code signing
and character-image distribution permission remain unverified; existing rights
notices remain. Publication is authorized by the current user request, not inferred
from prior Mac work. GitHub draft asset sizes and SHA256 digests match local files.
Source commit `99041859b421f23d41ae67ae5a30e163806b7d5a` was pushed to
`release/windows-r104`; remote branch and `windows-r104` tag both matched it.
GitHub release ID419275770 is public (unsigned preview, not marked latest;
old installer assets remain untouched). A fresh unauthenticated download from
the direct release URL returned 99,561,076 bytes with the same ZIP SHA256 above.
The downloaded copy is retained as `artifacts/windows-r104/public-download-check.zip`.

[Verified direct ZIP](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-r104/Doros-on-my-desktop-r104-win-x64.zip).
Subsequent ledger-only commits do not change the tagged source or asset bytes.

# Apple Silicon tester bundle

This packages a **test candidate**, not a Mac-verified release. No script uploads,
registers login startup, uses a personal certificate or disables OS protection.
The tester only extracts the ZIP, moves the `.app` to Applications and opens it;
the included README describes the first-launch security boundary and recovery.
The r104 port tester opens the pet and menu-bar icon only; diagnostic controls require an
explicit developer flag. Its floor is the screen bottom, visually in front of Dock;
Dock is no longer a support/perch. The r104 high-resolution runtime and continuous
slider are connected. Test04 preserves the earlier coverage and update-cost fixes,
and recomputes only bounded breathing/gaze regions in unchanged poses. Other
transitions use the full renderer; no extra image cache or FPS/quality reduction. Native
execution remains an open gate. See the dated vector verification record.
Test10 connects the accepted2026-10-26 seated/idle/meal/question behavior and
preserves earlier popup theme, native frame policy, topology and parallel sole fixes.
DORO? uses a retained vector outline; changing active body deformations fall back
to full sampling. Its Windows-host evidence is in the2026-10-28 verification record.
Existing packages are not overwritten.
macOS13 is this full-feature tester's minimum, not a verified global support claim.

## Build on Windows

Use fresh output directories. Python standard library only. No artwork resampling:
the ICNS container reuses the existing head ICO's16/32/64/128/256 PNG bytes.

```powershell
dotnet publish src/Dororong.Mac -c Release -r osx-arm64 --self-contained true -p:UseAppHost=true -p:RuntimeFrameworkVersion=8.0.32 -p:TargetLatestRuntimePatch=false -o artifacts/macos/NEW/publish
python -B -m unittest discover -s tools/mac-build -p test_package_tester.py -v
python -B tools/mac-build/package_tester.py --publish artifacts/macos/NEW/publish --output artifacts/macos/NEW/candidate --signer PATH/rcodesign.exe --notices PATH/notices --nuget C:/Users/USER/.nuget/packages
```

Supply rcodesign0.29.0 from the author's
[release](https://github.com/indygreg/apple-platform-rs/releases/tag/apple-codesign%2F0.29.0).
Verify its published SHA256 before executing. The Windows x64 ZIP used here has
SHA256 `54bb500e2da7a8de02fcae0f331d1cac6e6d7173b4281042ff9c528ba3159aaa`.
The tool remains under ignored artifacts, not shipped to testers. Configuration
profiles and RCODESIGN environment overrides are excluded; no timestamp network
request or Developer ID credential is used.

Supply these original notice files in `--notices`:

- `Avalonia-LICENSE.txt`: [exact package source revision](https://raw.githubusercontent.com/AvaloniaUI/Avalonia/627ae9ef921621e27e7aa58df2796fbadb50af88/licence.md).
- `MicroCom-LICENSE.txt`: [upstream license revision](https://raw.githubusercontent.com/kekekeks/MicroCom/4b8a38f773c109bad558ee3713d9f16d80776e42/LICENSE).

.NET/SkiaSharp/HarfBuzz notices are copied from their pinned restored NuGet
packages. Project MIT, asset-rights caveat and Pretendard OFL accompany the bundle.
No settings, keys, PDBs, local logs or unrelated source folders are packaged.

## Integrity and limitations

rcodesign applies ad-hoc integrity signatures, not Developer ID authentication or
notarization. Its0.29 verifier reports an empty-CMS error for ad-hoc output and
explicitly warns it is unreliable; we do not report that command as passing.
Its default nested-code rules also omit managed DLL/JSON inside `Contents/MacOS`.
The packager adds explicit SHA256 resource entries/rules and re-signs the main
executable against the finalized Info.plist/CodeResources, using relative paths
to avoid Windows drive-letter/scoped-argument ambiguity.

A bounded parser checks each native CodeDirectory page and special-slot digest,
all universal slices, nested cdhashes and every managed/resource file. ZIP CRC,
byte parity, path safety, complete inventory and Unix executable modes are checked.
Mutation regressions ensure missing external seals and changed code fail.
These checks are **not** Apple's `codesign --verify`, Gatekeeper acceptance,
notarization, an antivirus verdict or proof that the app runs on macOS.

`verification.json`, ZIP SHA256 and per-file manifest identify the exact candidate.
The app must not be edited after signing. Do not send earlier failed attempt folders.

## Real Mac gate

Verify the exact downloaded ZIP, Finder extraction/executable permissions, native
signature validation and first launch with normal OS protection. Record chip/OS
and PASS/FAIL/NOT_TESTED using the [parity checklist](../../docs/specs/2026-10-15-macos-parity.md).
Check first launch/exit/duplicate, icon, permissions, menu, Dock/windows, login,
update/removal/data preservation. If macOS reports damaged/malicious code, stop
and report the message; do not remove quarantine or disable Gatekeeper/SIP.
Apple's [per-app opening guidance](https://support.apple.com/102445) is included
for a tester who trusts the source and chooses to test this unnotarized build.

`assemble-probe.sh` remains the earlier raw-publish Mac assembly aid. It does not
replace this tester ZIP pipeline, signing checks or native acceptance gate.

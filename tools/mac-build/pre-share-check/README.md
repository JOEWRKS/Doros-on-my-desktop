# Distributed-code pre-share simulation

Runs the **packaged** Core/Portable assemblies on Windows with Windows Skia.
No project reference rebuilds product code. This is not macOS emulation, a native
launch test, a security audit or an approval to ship. Verify the ZIP against the
staging tree with `package_tester.verify_zip` / `verify_bundle` first.

From the repository root (PowerShell):

```powershell
dotnet run --project tools/mac-build/pre-share-check/PreShareCheck.csproj -c Release "-p:MacArtifactDir=D:/JOEWRKS/DororongDesktopPet/artifacts/macos/tester-20261015-arm64-02/package/Doro Mac Test.app/Contents/MacOS" -- "D:/JOEWRKS/DororongDesktopPet/artifacts/macos/tester-20261015-arm64-02/pre-share-simulation-confirm.json"
```

Use a fresh output JSON name when retaining prior evidence. The report records
loaded assembly SHA256 values; compare them with the verified bundle. Nonzero exit
means an exception/assertion failed. **Zero exit does not clear timing warnings**:
review `warnings` and `effect_timings`. The100ms stall threshold is diagnostic,
not an approved product acceptance limit or an Apple Silicon benchmark.

Coverage: cold/reloaded eating-effect cost, complete meals at30/60/120fps,
deterministic mixed inputs/window and monitor changes/input loss/resume gaps,
frame premultiplication/visibility, full desktop meal-to-autonomous-walk recovery.
The seeded sequence is40 nominal seconds with injected2-second gaps, not40 seconds
of real Mac use. It does not establish all interaction targets were exercised.
Actual AppKit, Finder/Gatekeeper, GPU/Retina, permissions, Spaces/Dock, native
status menus and login lifecycle remain Mac-only checks.

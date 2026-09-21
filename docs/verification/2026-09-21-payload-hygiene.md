# Local payload hygiene verification — 2026-09-21

Subsequent user-approved host installation tests are recorded separately in
[host lifecycle acceptance](2026-09-21-payload-hygiene-host.md). The status below
describes the earlier package-build handoff, before that host test.

Status: local unsigned candidate only. No commit, push, release replacement,
host installation or certificate enrollment was performed. Existing v0.1.1
remains the public release. Runtime stays 8.0.31; pet behavior/artwork is unchanged
in source. This is not a clean-machine or install/upgrade/uninstall acceptance.

## Artifacts

- Repository base: `01cc859cd936c529227fda72bfa298f392785249`, main + working changes.
- Portable: `artifacts/product-shell/candidate-20260921-payload-hygiene-02/Doros-on-my-desktop-Portable-0.1.1-win-x64.zip`.
- Installer: `artifacts/installer/candidate-20260921-payload-hygiene-01/Doros-on-my-desktop-Setup-0.1.1-win-x64.exe`.
- Machine-readable evidence and package smoke log: `build-evidence.json` and
  `package-validation.log` beside the installer (local ignored artifacts).
- Installer staged payload: 469 files, plus one generated ownership manifest.
  Signature status: `NotSigned`.

| File | SHA256 |
| --- | --- |
| Portable ZIP | `D3274FFD8914DD203FED59728A83D27C98952193BEB2DDCEFEC807A249D26F41` |
| Setup EXE | `2098CDAFB486EEA3DEB3F402666701273108ED0B5F89F939DBAD2D76DE29BD7D` |
| Dororong.App.dll | `70218A1725B0044F9C7C6B972A0068D4BC636C6BA7C4540E3C7FA0B7CA502F61` |
| Dororong.Core.dll | `A0F8DE6D612A01F9D97A01DD9A7EA4460D3F1F5548321B7D3AA39EF38A7A7775` |

Exactly three paths disappeared compared with the previous 472-file runtime:
`Dororong.App.pdb`, `Dororong.Core.pdb`, and
`Assets/dororong-canonical-source.png`. Freshly rebuilt App/Core DLLs differ from
the older candidate but match the tested RID outputs byte-for-byte. All other
retained files match the older runtime by SHA256. Required embedded resources,
quick start and license notices remain present.

The source PNG remains SHA256
`F96EC30CBD18429E6BA1138BFA4EB44F331974C9820D36EE97A02FE518E46504`.
Normal Release App/Core PDBs remain available (109064 / 29276 bytes).

## Checks observed

- Core: 241 passed; App: 1072 passed, both 0 failed / 0 skipped, with the
  [documented Release commands](../BUILD.md#source-and-tests).
- Initial regression RED: old `candidate-20260921-rebrand-02` rejected with
  three unwanted files, before native execution.
- First implementation RED: fresh `payload-hygiene-01` still contained Core's
  PDB. SDK 9.0.304 adds reference symbols via `ReferenceCopyLocalPaths` separately
  from the current-project symbol flag. A publish-item filter now excludes all
  PDBs when that flag is false, without deleting source/build output.
  The failed candidate is preserved, not a deliverable.
- Final publisher exited 0. Full package validator PASS: 469 files, pinned
  Core/Desktop/host 8.0.31, exact tested App/Core parity, executable metadata/icon,
  archive round-trip and native launch on an isolated desktop. Duplicate process
  exited 0; harness-owned process closed normally.
- Copied negative fixture `candidate-20260921-payload-hygiene-negative-01`
  with Core PDB reintroduced: validator rejected count 1 before launch. The clean
  candidate and its archive remained unchanged.
- Publisher existing-output refusal PASS; sentinel preserved.
- Installer old archive pin correctly rejected the new ZIP before output
  creation. Updated pin is the verified ZIP hash above, not a relaxed check.
- Installer payload suite PASS: inventory, metadata, path/reparse checks and
  preservation. Installer All suite PASS: refusal cases, compiled directory/file/
  shortcut policy harness, build, explicit inventory, version, signature state
  and hashes. Policy harness aborts before installation by design.
- Independent final read-only review: no Critical/Important/Minor implementation
  findings. Reviewer independently checked ZIP inventory, artifact/source hashes,
  build/source preservation and tested DLL parity; executable test results were
  inspected as evidence, not rerun by the reviewer.

## Reproduction

Use fresh output directories; scripts intentionally refuse existing output.
Current installer pin is tied to the ZIP above, not arbitrary rebuilt archives.
Commands were run from `D:/JOEWRKS/DororongDesktopPet`.

```powershell
pwsh -NoProfile -File tools/Publish-Product.ps1 -OutputPath artifacts/product-shell/candidate-20260921-payload-hygiene-02
pwsh -NoProfile -File tests/Dororong.ProductPackage.Tests.ps1 -PackagePath artifacts/product-shell/candidate-20260921-payload-hygiene-02/runtime -ArchivePath artifacts/product-shell/candidate-20260921-payload-hygiene-02/Doros-on-my-desktop-Portable-0.1.1-win-x64.zip
pwsh -NoProfile -File tests/Dororong.ProductPackage.Tests.ps1 -PublisherPath D:/JOEWRKS/DororongDesktopPet/tools/Publish-Product.ps1 -VerifyExistingOutputRefusal
pwsh -NoProfile -File tests/Dororong.InstallerPayload.Tests.ps1 -PreparedToolchainPath D:/JOEWRKS/.worktrees/DororongDesktopPet-m1-expression-animation/artifacts/installer/toolchain-inno-7.1.0-x64-20260912-02
pwsh -NoProfile -File tests/Dororong.InstallerBuild.Tests.ps1 -Phase All -CompilerPath D:/JOEWRKS/.worktrees/DororongDesktopPet-m1-expression-animation/artifacts/installer/toolchain-inno-7.1.0-x64-20260912-02/ISCC.exe -CandidatePath D:/JOEWRKS/DororongDesktopPet/artifacts/installer/candidate-20260921-payload-hygiene-01
```

The compiler was read from the preserved existing toolchain, not modified. Its
SHA256 is `D06EBD38F38E3CEE60A3C50CC45BD449D77E0BC6A5CABC607EA9886808E4DE1A`,
version 7.1.0, valid Pyrsys B.V. Authenticode signature.

## Signing boundary

Subsequent maintainer decision: skip code signing for this release. The readiness
conditions below are retained for future reference, not an active signing task.
No applications, purchases or integration will proceed without a new request.

Local Windows code-signing stores contained no matching certificate; SignTool
is available. Free SignPath eligibility remains unresolved because the project's
MIT license excludes character assets and their distribution rights are not
verified. See [signing readiness](../BUILD.md#code-signing-readiness-2026-09-21).
No paid/free application, new account, private-key export, trust-root change,
signature or public release was made. Provider/identity and rights clarification
are required before the signing step can proceed.

배포용 파일에서만 디버그 심볼·작업용 원본 이미지를 제외했습니다. 새 설치본은
로컬 검증용 미서명 파일이며, 기존 공개 배포본과 설치된 프로그램은 변경하지 않았습니다.

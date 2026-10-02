# Build, test and package

The working tree prepares **unpublished v0.1.3**. The public release remains
v0.1.2; new publication is on hold for [rights clarification](RIGHTS-REVIEW.md).
See the [other-environment checklist](ENVIRONMENT-TESTING.md) for manual acceptance.

Run from the repository root on Windows x64 with a .NET 8-compatible SDK.
Run build/test commands sequentially to avoid WPF assembly locks.
저장소 루트에서 실행하며, WPF 파일 잠금 방지를 위해 빌드·검사는 순서대로 실행합니다.

## Source and tests

```powershell
dotnet restore DororongDesktopPet.sln
dotnet test tests/Dororong.Core.Tests --configuration Release --no-restore
dotnet restore tests/Dororong.App.Tests/Dororong.App.Tests.csproj --runtime win-x64 -p:RuntimeFrameworkVersion=8.0.31 -p:TargetLatestRuntimePatch=false
dotnet test tests/Dororong.App.Tests/Dororong.App.Tests.csproj --configuration Release --runtime win-x64 --no-restore -p:SelfContained=true -p:RuntimeFrameworkVersion=8.0.31 -p:TargetLatestRuntimePatch=false
```

To run from source (not a packaging prerequisite):

```powershell
dotnet run --project src/Dororong.App/Dororong.App.csproj --configuration Release
```

Source builds use the internal apphost `Dororong.App.exe`; packaged builds use
`Dororong.exe`. Exit an existing pet through its tray before launching a source
build. Absence from the taskbar does not mean the pet has stopped.

## Portable package

The explicit self-contained App test above is a publishing prerequisite.
The publisher verifies App/Core DLLs against those tested RID outputs.
Do not rebuild reference DLLs between testing, publishing and validation.
Choose a new, unused candidate directory.

```powershell
pwsh -NoProfile -File tools/Publish-Product.ps1 -OutputPath artifacts/product-shell/candidate-NEW
pwsh -NoProfile -File tests/Dororong.ProductPackage.Tests.ps1 -PackagePath artifacts/product-shell/candidate-NEW/runtime -ArchivePath artifacts/product-shell/candidate-NEW/Doros-on-my-desktop-Portable-0.1.3-win-x64.zip
```

The complete runtime folder is required; distributing only the EXE will not work.
Runtime and native host versions are pinned to 8.0.31 by the packaging tools.
New packages produced by `Publish-Product.ps1` exclude PDB debug symbols and the loose
`Assets/dororong-canonical-source.png`; ordinary build output keeps them.
The package validator rejects their reintroduction before its native smoke test.
Embedded character resources and all license notices remain included.

새 배포본에는 디버그 심볼과 작업용 원본 이미지를 넣지 않습니다.
개발용 빌드·원본 파일은 보존하며, 실행에 필요한 내장 이미지는 유지합니다.

## Installer and verification

The historical public 0.1.2 candidate is `candidate-20260921-v012-01`.
The current installed local 0.1.3 installer is `candidate-20261002-security-lifecycle-01`,
using the unchanged `candidate-20261002-window-layer-01` app payload;
see the [current host verification record](verification/2026-10-02-security-lifecycle-host.md).
The earlier `candidate-20260921-v013-01` and its
[verification report](verification/2026-09-21-v013-local-candidate.md) are historical.
See [release verification](verification/2026-09-21-v012-release.md) for hashes
and the exact acceptance coverage. Local `artifacts/` outputs are intentionally
not committed. End users should download the Setup EXE from
[GitHub Releases](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/tag/v0.1.2),
not GitHub's source-code ZIP.

Installer builds require the verified payload archive and pinned Inno Setup
compiler. Existing toolchain/payload paths are local prerequisites, not files
included by cloning this repository. The builder pins the payload SHA256 and
refuses an existing output directory.

```powershell
pwsh -NoProfile -File tools/Build-Installer.ps1 -PackagePath artifacts/product-shell/candidate-20260921-v013-01/runtime -ArchivePath artifacts/product-shell/candidate-20260921-v013-01/Doros-on-my-desktop-Portable-0.1.3-win-x64.zip -CompilerPath C:/your-verified-toolchain/ISCC.exe -OutputPath artifacts/installer/candidate-NEW
```

The tracked builder still pins the historical September21 candidate shown above;
it does not silently package the newer October2 binaries. The current local installer
used a separately hash-pinned builder retained with its local evidence. Before making
a new distributable, validate the chosen archive and deliberately update the pin.
Replace the compiler placeholder
with your verified Inno Setup 7.1.0 path. An independently rebuilt archive is
not automatically trusted: validate it and deliberately update the hash pin.
The published v0.1.1 release remains available unchanged.
The preceding 0.1.1 payload-hygiene candidate passed current-host update,
uninstall and reinstall checks; see [host lifecycle acceptance](verification/2026-09-21-payload-hygiene-host.md).
That historical evidence is not a 0.1.2 host-upgrade or clean-machine test.

Exit the pet before package/installer validation: it includes a strict
single-instance application smoke test. Historical Sandbox handoffs retain
older candidate pins and do not validate the current build.

## Code-signing readiness (2026-09-21)

The maintainer chose to defer code signing for this release. It is not an active
release gate; the notes below are future prerequisites only. Keep unsigned
disclosure, published checksums and package/installer validation. Windows may
show an unknown-publisher or SmartScreen warning; do not disable security checks.
This decision does not waive the separate artwork-rights or installation checks.

이번 출시에서는 사용자 결정으로 코드 서명을 생략합니다. 미서명 안내와 파일 해시·
배포 검증은 유지하며, 인증서 신청·결제·연동은 별도 요청 전까지 진행하지 않습니다.

Signing is **not configured**. Local Windows certificate stores contain no
code-signing certificate; the Windows SDK SignTool is available. No account,
application, certificate purchase or trust-store change has been made.

[SignPath Foundation](https://signpath.org/terms) offers free signing to eligible
open-source projects, but requires open-source licensing of all components,
verifiable builds, contributor MFA and release approval. Our MIT code license
explicitly excludes character assets, whose distribution permissions remain
unverified. Eligibility therefore needs provider clarification and asset-rights
evidence; do not claim acceptance or change the art license to qualify.
The [application page](https://signpath.org/apply) is the next step only after
that clarification and the maintainer's approval.

Before a future signed release, configure the approved provider, sign only
project-owned binaries before packaging and sign the final installer afterward.
Verify trusted signatures and timestamps, and compute release hashes **after**
signing. Update the unsigned apphost-parity checks to verify both payload
provenance and signature integrity; never merely skip those checks. A valid
signature does not guarantee immediate SmartScreen reputation
([Microsoft guidance](https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/code-signing-options)).

무료 서명은 캐릭터 리소스의 라이선스·배포 권리와 서비스 자격 확인이 선행되어야 합니다.
현재 서명 완료 상태가 아니며, 인증서 신청이나 결제는 하지 않았습니다.

## Installation, support and release limits

- Per-user install: `%LOCALAPPDATA%\Programs\JOEWRKS\Dororong`.
- Local diagnostics: `%LOCALAPPDATA%\JOEWRKS\Dororong\logs`; no automatic upload.
- Start-menu shortcut is installed; desktop shortcut and launch-after-install
  are opt-in. Silent installation does not launch the pet.
- Exit normally before replacing or uninstalling. Retain the previous portable
  folder for rollback. Portable copies are not imported or removed by the installer.
- Display name: `도로롱이 내 컴에?!` / `Doro's on my desktop?!`.
  Shortcut: `도로롱이 내 컴에!` (Windows forbids `?` in filenames).
  Internal executable, AppId and data paths retain the old Dororong identity
  so existing installations are recognized rather than duplicated.
- The installer is unsigned. Clean-machine and broader failure-path
  acceptance, mixed-DPI/live Windows coverage and rights review remain release gates.
  No power-loss rollback guarantee is made.
- For support, provide the candidate hash, Windows version, reproduction steps
  and reviewed logs. Installer logging: `/LOG="C:\your-folder\dororong-setup.log"`.

설치·테스트 성공은 모든 Windows 환경의 동작이나 캐릭터 재배포 권한을 보장하지 않습니다.
미리보기 배포이며 남은 검증과 권리 확인이 필요합니다.

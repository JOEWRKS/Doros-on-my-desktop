# 예전 파일 · Historical files

main은 현재 코드·리소스·테스트·도구·문서를 유지합니다. 예전 실험 이미지,
미리보기 ZIP, 내부 작업 메모는 아래 보관 브랜치에서 확인하세요.
main keeps current source, assets, tests, tools and documentation. Historical
experiments, preview ZIPs and internal notes remain in the archive branch.

**[archive/pre-cleanup-2026-09-21](https://github.com/JOEWRKS/Doros-on-my-desktop/tree/archive/pre-cleanup-2026-09-21)**

Snapshot: `abe15b56e69c5da205fcd55f496a1ec2e084b373`

- [실험·미리보기 126개 · 126 experiment/preview files](https://github.com/JOEWRKS/Doros-on-my-desktop/tree/abe15b56e69c5da205fcd55f496a1ec2e084b373/artifacts)
- [내부 작업 메모 6개 · 6 internal work notes](https://github.com/JOEWRKS/Doros-on-my-desktop/tree/abe15b56e69c5da205fcd55f496a1ec2e084b373/.superpowers)
- [전체 이전 작업 기록 · Full previous task history](https://github.com/JOEWRKS/Doros-on-my-desktop/blob/abe15b56e69c5da205fcd55f496a1ec2e084b373/TASKS.md)

이전 설계·검증 문서의 산출물 경로는 당시 기록입니다. 해당 파일은 위 스냅샷에서
찾아보세요. Git 기록과 기존 릴리스는 변경하지 않았으며, 보관 ZIP은 설치본이 아닙니다.
Artifact paths in older design/verification notes describe historical evidence;
consult the snapshot above. Git history and existing releases are unchanged.
Archived preview ZIPs are not the current installer.

## 로컬 파일 구분 · Local organization

- `artifacts/delivery/`: 검증된 설치 EXE·사용 안내·체크섬만 담은 사용자용 묶음.
  Verified installer handoffs only; not public releases.
- `artifacts/product-shell/candidate-*`, `artifacts/installer/candidate-*`:
  과거/현재 불변 빌드 후보와 검증 근거. Immutable build candidates and evidence.
- `artifacts/installer/host-*`: 설치 전 백업·복구 설치본·설치 검사 기록.
  Installation backups, recovery installers and host verification records.
- 기타 `artifacts/`: 원본 분리 자료·시안·모션/화면 검사 산출물.
  Source derivation, design and rendering proofs; not user installation files.

기록에 연결된 예전 경로는 이동·삭제하지 않습니다. 최신 사용 파일만 delivery에서
구분하며, artifacts 전체와 bin/obj/TestResults는 Git에 넣지 않습니다.
Historical paths remain unchanged so existing evidence stays usable. Only the
delivery folder is a user handoff; generated artifacts and build caches stay out of Git.

# 도로롱이 내 컴에?! · Doro's on my desktop?!

니케의 도로롱(Doro) 팬 캐릭터를 바탕으로 만든 Windows 데스크톱 펫.
A Windows desktop pet based on the NIKKE-derived Dororong (Doro) fan character.

## 다운로드 · Download

현재 저장소는 배포 권한 확인을 위해 비공개이며, v0.1.3은 로컬 검증본입니다.
The repository is private pending distribution permission; v0.1.3 is a locally tested candidate.
아래 링크는 보존된 이전 v0.1.2 파일입니다. / The link below is the preserved v0.1.2 installer.
다운로드하려면 저장소 접근 권한이 있는 GitHub 계정으로 로그인해야 합니다.
Downloads require signing in with a GitHub account that has access to this private repository.

**[Windows 설치 파일 · Windows installer](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/v0.1.2/Doros-on-my-desktop-Setup-0.1.2-win-x64.exe)**

Windows x64 · v0.1.2 Preview · 런타임 포함 / Runtime included · 관리자 권한 불필요 / No admin required

설치 후 시작 메뉴의 **도로롱이 내 컴에!**에서 실행하세요. 업데이트 전에는 트레이에서 종료하세요.
Install, then launch **도로롱이 내 컴에!** from the Start menu. Exit through the tray before updating.

**Code → Download ZIP / Source code는 설치 파일이 아닙니다. / Source-code ZIPs are not installers.**

## 기능·사용법 · Features & controls

아래 조작은 현재 main / v0.1.3 기준입니다. v0.1.2의 트레이 메뉴는 우클릭만 지원합니다.
Controls below describe main / v0.1.3. The v0.1.2 tray menu supports right-click only.

| 조작 · Control | 동작 · Action |
| --- | --- |
| 머리 드래그 · Drag head | 이동 · Move |
| 볼 드래그 · Pull cheek | 늘이기·탄성 복원 · Stretch & spring back |
| 마우스 접근·머무르기 · Move/linger nearby | 추적·짧은 도약 · Track & pounce |
| 창·작업표시줄 가장자리에서 준비 동작 후 놓기 · Release at an edge when ready | 매달리기 · Hang |
| 매달린 앞발 클릭·드래그 · Click/drag a hanging paw | 한쪽 발 휘젓기·당기기 · Wave & stretch |
| 캐릭터 우클릭 · Right-click character | 간식 / 앉아 / 자동 실행 / 종료 · Snack / Sit / Startup / Exit |
| 트레이 좌·우클릭 · Left/right-click tray | 간식 / 자동 실행 / 로그 / 종료 · Snack / Startup / Logs / Exit |

걷기·숨쉬기·눈 깜빡임·수면은 자동으로 재생됩니다. 제거는 Windows 앱 목록에서 가능합니다.
Walking, breathing, blinking and sleeping are automatic. Uninstall through Windows' app list.

**main / v0.1.3:** 트레이 좌클릭, 관절 기반 앉기·일어서기, 머리 선명도 개선, 일반 창 뒤로 밀림 복구. 이전 v0.1.2 설치 파일에는 포함되지 않습니다.
**main / v0.1.3:** left-click tray menu, articulated sit/rise, sharper head rendering and window-layer recovery. Not included in the older v0.1.2 installer.

## 개발 중인 간식 놀이 · Snack-play development build

로컬 설치본에 적용된 개발 변경입니다. 기존 공개 릴리스에는 포함되지 않습니다.
Local installed development build; not included in the existing public release.

- 캐릭터 우클릭 또는 트레이 → **간식 주기**. 주얼을 따라 달리고 위로 들면 점프합니다.
  Right-click the pet or open the tray → **간식 주기** to start a jewel chase.
- 입 앞에서 잠깐 멈추면 눈을 감고 먹습니다. 같은 메뉴의 **간식 취소**로 중단합니다.
  Hold it still near the mouth to feed; **간식 취소** cancels.
- 주얼은 클릭을 가로채지 않는 커서 동반 이미지입니다. 시스템 커서 설정은 바꾸지 않습니다.
  The jewel is a click-through cursor companion, not a system cursor replacement.
- **Windows 로그인 시 실행**을 체크하면 자동 실행됩니다. 기본은 꺼짐이며 같은 메뉴에서 해제합니다.
  Enable **Windows 로그인 시 실행** for login startup; off by default and reversible.
- 배고픔 수치·숙제·접속 벌칙·키보드 내용 수집은 없습니다.
  No hunger meter, chores, absence penalties or keyboard-content collection.

[구현 범위·검증 경계 · Scope and verification boundaries](docs/specs/2026-10-13-snack-play.md)

## 구성·개발 · Structure & development

- `src/Dororong.App`: WPF 화면·Windows 연동 / UI & Windows integration
- `src/Dororong.Core`: 행동·발판 로직 / Behavior & platform logic
- `tests`: 회귀 검사 / Regression tests
- `tools`, `installer`, `docs`: 도구·설치·문서 / Tools, packaging & documentation

[빌드·검사 안내 · Build & test](docs/BUILD.md) · [릴리스·체크섬 · Release & checksums](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/tag/v0.1.2)

로컬 사용자용 설치 묶음은 `artifacts/delivery/`, 빌드 후보·검사·백업은 `artifacts/`의 별도 폴더에 둡니다. Git에는 포함하지 않습니다.
Local installer handoffs live in `artifacts/delivery/`; candidates, proofs and backups stay separate and outside Git.

[예전 실험·작업 기록 · Historical experiments & notes](docs/ARCHIVE.md)

## 주의·라이선스 · Notes & license

코드 서명 없는 미리보기로 Windows 경고가 표시될 수 있습니다. 보안 기능을 끄지 마세요. 모든 PC·혼합 DPI 검증은 완료되지 않았습니다. 자동 업데이트·계정·데이터 전송은 없으며, 개발본의 자동 시작은 선택 사항입니다.
Unsigned preview: Windows may show a warning; do not disable security checks. Clean-PC/mixed-DPI coverage is incomplete. No auto-update, accounts or data upload; the development build offers optional autostart.

직접 작성한 코드는 [MIT](LICENSE). 캐릭터·원본/파생 이미지·명칭·상표는 제외되며 각 권리자에게 귀속됩니다. 외부 의존성은 각 라이선스를 따릅니다.
Original code is [MIT](LICENSE); character art, derived assets, names and trademarks are excluded and belong to their respective owners. Dependencies retain their own licenses.

메뉴 글꼴 Pretendard는 SIL OFL 1.1이며 [출처·고지](src/Dororong.App/Assets/Fonts/README.md)를 동봉합니다.
Menu typography uses Pretendard under SIL OFL 1.1; its license is included with the app.

**승리의 여신: 니케에서 파생된 도로롱 팬 캐릭터 이미지를 바탕으로 한 비공식·비상업 목적의 팬메이드 2차창작**입니다. 공식 승인·제휴는 없으며 원 팬아트 작가와 이미지 배포 허락은 아직 확인되지 않았습니다. 코드 라이선스는 캐릭터 리소스의 이용·재배포 권한을 부여하지 않습니다.
An **unofficial, non-commercial fan-made derivative** using reference images of the **GODDESS OF VICTORY: NIKKE**-derived Doro fan character. No official affiliation or endorsement. Original fan-artist and asset distribution permissions remain unverified; the code license grants no rights to the character assets.

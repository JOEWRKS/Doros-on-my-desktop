# 도로롱이 내 컴에?! · Doro's on my desktop?!

> **Windows r104 수락본 배포 브랜치 / Accepted Windows r104 delivery.**
> 현재 진행 중인 Mac 이식 변경은 포함하지 않습니다.

니케의 도로롱(Doro) 팬 캐릭터를 바탕으로 만든 Windows 데스크톱 펫.
A Windows desktop pet based on the NIKKE-derived Dororong (Doro) fan character.

## 다운로드 · Download

**ZIP 전체를 압축 해제하고 Dororong.exe를 실행하세요. 별도 .NET 설치는 필요 없습니다.**
**Extract the entire ZIP and run Dororong.exe. No separate .NET installation is needed.**

**[Windows r104 바로 다운로드 · Download Windows r104](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-r104/Doros-on-my-desktop-r104-win-x64.zip)**

Windows x64 · r104 portable · 약 100MB / About 100MB · 관리자 권한 불필요 / No admin required

기존 앱을 트레이에서 종료한 다음 새 폴더의 Dororong.exe를 실행하세요. EXE만 따로 옮기면 안 됩니다.
Exit the previous pet, then run Dororong.exe in the extracted folder. Do not move the EXE alone.
기존 설치판은 자동 교체하지 않습니다. / The older installed copy is not replaced automatically.

[버전 안내·체크섬 · Release notes & checksums](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/tag/windows-r104)

**Code → Download ZIP / Source code는 설치 파일이 아닙니다. / Source-code ZIPs are not installers.**

## 기능·사용법 · Features & controls

수락한 r104 조작과 100~150% 연속 크기 슬라이더를 포함합니다. / Includes accepted r104 interactions and continuous sizing.

| 조작 · Control | 동작 · Action |
| --- | --- |
| 머리 드래그 · Drag head | 이동 · Move |
| 볼 드래그 · Pull cheek | 늘이기·탄성 복원 · Stretch & spring back |
| 마우스 접근·머무르기 · Move/linger nearby | 추적·짧은 도약 · Track & pounce |
| 창·작업표시줄 가장자리에서 준비 동작 후 놓기 · Release at an edge when ready | 매달리기 · Hang |
| 매달린 앞발 클릭·드래그 · Click/drag a hanging paw | 한쪽 발 휘젓기·당기기 · Wave & stretch |
| 캐릭터 우클릭 · Right-click character | 간식 / 앉아 / 자동 눕기 / 자동 실행 / 종료 · Snack / Sit / Auto sleep / Startup / Exit |
| 트레이 좌·우클릭 · Left/right-click tray | 간식 / 자동 눕기 / 자동 실행 / 로그 / 종료 · Snack / Auto sleep / Startup / Logs / Exit |

걷기·숨쉬기·눈 깜빡임·수면은 자동으로 재생됩니다. 제거는 Windows 앱 목록에서 가능합니다.
Walking, breathing, blinking and sleeping are automatic. Uninstall through Windows' app list.

**자동 눕기**를 끄면 자동 수면만 멈춥니다. 기본은 켜짐. 앉기·걷기·간식은 유지되며 설정은 재실행 후에도 기억합니다.
Turn off **자동 눕기 (Auto sleep)** to disable automatic sleep only (on by default). Sitting, walking and snacks remain available; the choice persists across restarts.

**v0.1.3:** 새 메뉴·머리 아이콘, 관절 기반 앉기·일어서기, 머리 선명도와 창·작업표시줄 표시 개선.
**v0.1.3:** refreshed menus and head icons, articulated sit/rise, sharper head rendering and improved window/taskbar behavior.

## 간식 놀이·자동 실행 · Snack play & startup

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

[빌드·검사 안내 · Build & test](docs/BUILD.md) · [릴리스·체크섬 · Release & checksums](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/tag/v0.1.3)

로컬 사용자용 설치 묶음은 `artifacts/delivery/`, 빌드 후보·검사·백업은 `artifacts/`의 별도 폴더에 둡니다. Git에는 포함하지 않습니다.
Local installer handoffs live in `artifacts/delivery/`; candidates, proofs and backups stay separate and outside Git.

[예전 실험·작업 기록 · Historical experiments & notes](docs/ARCHIVE.md)

## 주의·라이선스 · Notes & license

코드 서명 없는 미리보기로 Windows 경고가 표시될 수 있습니다. 보안 기능을 끄지 마세요. 다른 PC·Windows 11·혼합 DPI 검증은 완료되지 않았습니다. 자동 업데이트·계정·데이터 전송은 없으며 자동 시작은 선택 사항입니다.
Unsigned preview: Windows may show a warning; do not disable security checks. Other-PC, Windows 11 and mixed-DPI coverage is incomplete. No auto-update, accounts or data upload; autostart is optional.

직접 작성한 코드는 [MIT](LICENSE). 캐릭터·원본/파생 이미지·명칭·상표는 제외되며 각 권리자에게 귀속됩니다. 외부 의존성은 각 라이선스를 따릅니다.
Original code is [MIT](LICENSE); character art, derived assets, names and trademarks are excluded and belong to their respective owners. Dependencies retain their own licenses.

메뉴 글꼴 Pretendard는 SIL OFL 1.1이며 [출처·고지](src/Dororong.App/Assets/Fonts/README.md)를 동봉합니다.
Menu typography uses Pretendard under SIL OFL 1.1; its license is included with the app.

**승리의 여신: 니케에서 파생된 도로롱 팬 캐릭터 이미지를 바탕으로 한 비공식·비상업 목적의 팬메이드 2차창작**입니다. 공식 승인·제휴는 없으며 원 팬아트 작가와 이미지 배포 허락은 아직 확인되지 않았습니다. 코드 라이선스는 캐릭터 리소스의 이용·재배포 권한을 부여하지 않습니다.
An **unofficial, non-commercial fan-made derivative** using reference images of the **GODDESS OF VICTORY: NIKKE**-derived Doro fan character. No official affiliation or endorsement. Original fan-artist and asset distribution permissions remain unverified; the code license grants no rights to the character assets.

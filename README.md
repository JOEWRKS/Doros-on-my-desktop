# 도로롱이 내 컴에?! · Doro's on my desktop?!

니케의 도로롱(Doro) 팬 캐릭터를 바탕으로 만든 Windows 데스크톱 펫.
A Windows desktop pet based on the NIKKE-derived Dororong (Doro) fan character.

## 다운로드 · Download

현재 저장소는 배포 권한 확인을 위해 비공개이며, v0.1.3은 로컬 검증본입니다.
The repository is private pending distribution permission; v0.1.3 is a locally tested candidate.
아래 링크는 보존된 이전 v0.1.2 파일입니다. / The link below is the preserved v0.1.2 installer.

**[Windows 설치 파일 · Windows installer](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/v0.1.2/Doros-on-my-desktop-Setup-0.1.2-win-x64.exe)**

Windows x64 · v0.1.2 Preview · 런타임 포함 / Runtime included · 관리자 권한 불필요 / No admin required

설치 후 시작 메뉴의 **도로롱이 내 컴에!**에서 실행하세요. 업데이트 전에는 트레이에서 종료하세요.
Install, then launch **도로롱이 내 컴에!** from the Start menu. Exit through the tray before updating.

**Code → Download ZIP / Source code는 설치 파일이 아닙니다. / Source-code ZIPs are not installers.**

## 기능·사용법 · Features & controls

| 조작 · Control | 동작 · Action |
| --- | --- |
| 머리 드래그 · Drag head | 이동 · Move |
| 볼 드래그 · Pull cheek | 늘이기·탄성 복원 · Stretch & spring back |
| 마우스 접근·머무르기 · Move/linger nearby | 추적·짧은 도약 · Track & pounce |
| 창·작업표시줄 가장자리에서 준비 동작 후 놓기 · Release at an edge when ready | 매달리기 · Hang |
| 매달린 앞발 클릭·드래그 · Click/drag a hanging paw | 한쪽 발 휘젓기·당기기 · Wave & stretch |
| 캐릭터 우클릭 · Right-click character | 앉아 / Exit · Sit / Exit |
| 트레이 우클릭 · Right-click tray | 로그 폴더 열기 / 종료 · Open logs / Exit |

걷기·숨쉬기·눈 깜빡임·수면은 자동으로 재생됩니다. 제거는 Windows 앱 목록에서 가능합니다.
Walking, breathing, blinking and sleeping are automatic. Uninstall through Windows' app list.

**main / v0.1.3:** 트레이 좌클릭, 관절 기반 앉기·일어서기, 머리 선명도 개선, 일반 창 뒤로 밀림 복구. 이전 v0.1.2 설치 파일에는 포함되지 않습니다.
**main / v0.1.3:** left-click tray menu, articulated sit/rise, sharper head rendering and window-layer recovery. Not included in the older v0.1.2 installer.

## 구성·개발 · Structure & development

- `src/Dororong.App`: WPF 화면·Windows 연동 / UI & Windows integration
- `src/Dororong.Core`: 행동·발판 로직 / Behavior & platform logic
- `tests`: 회귀 검사 / Regression tests
- `tools`, `installer`, `docs`: 도구·설치·문서 / Tools, packaging & documentation

[빌드·검사 안내 · Build & test](docs/BUILD.md) · [릴리스·체크섬 · Release & checksums](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/tag/v0.1.2)

[예전 실험·작업 기록 · Historical experiments & notes](docs/ARCHIVE.md)

## 주의·라이선스 · Notes & license

코드 서명 없는 미리보기로 Windows 경고가 표시될 수 있습니다. 보안 기능을 끄지 마세요. 모든 PC·혼합 DPI 검증은 완료되지 않았습니다. 자동 업데이트·자동 시작·계정·데이터 전송은 없습니다.
Unsigned preview: Windows may show a warning; do not disable security checks. Clean-PC/mixed-DPI coverage is incomplete. No auto-update, autostart, accounts or data upload.

직접 작성한 코드는 [MIT](LICENSE). 캐릭터·원본/파생 이미지·명칭·상표는 제외되며 각 권리자에게 귀속됩니다. 외부 의존성은 각 라이선스를 따릅니다.
Original code is [MIT](LICENSE); character art, derived assets, names and trademarks are excluded and belong to their respective owners. Dependencies retain their own licenses.

**승리의 여신: 니케에서 파생된 도로롱 팬 캐릭터 이미지를 바탕으로 한 비공식·비상업 목적의 팬메이드 2차창작**입니다. 공식 승인·제휴는 없으며 원 팬아트 작가와 이미지 배포 허락은 아직 확인되지 않았습니다. 코드 라이선스는 캐릭터 리소스의 이용·재배포 권한을 부여하지 않습니다.
An **unofficial, non-commercial fan-made derivative** using reference images of the **GODDESS OF VICTORY: NIKKE**-derived Doro fan character. No official affiliation or endorsement. Original fan-artist and asset distribution permissions remain unverified; the code license grants no rights to the character assets.

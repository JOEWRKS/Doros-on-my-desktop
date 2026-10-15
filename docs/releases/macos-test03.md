# Mac test03 — Apple Silicon 테스트판

**[ZIP 바로 다운로드](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/macos-test03/Doro-Mac-arm64-test03.zip)** · [SHA256](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/macos-test03/Doro-Mac-arm64-test03.zip.sha256)

M1 이상 · macOS13 이상 · 약85MB · .NET 별도 설치 불필요.
Windows판과 별도의 사전 테스트판이며 **실제 Mac에서 실행 검증하지 않았습니다.**

## 설치

1. 위 ZIP을 받아 압축을 풉니다. GitHub의 Source code ZIP이 아닙니다.
2. `Doro Mac Test.app`을 응용 프로그램 폴더로 옮겨 실행합니다.
3. 종료는 캐릭터 우클릭 또는 메뉴 막대 머리 아이콘 → 종료.

## 보안 경고

Developer ID 서명·Apple 공증이 없습니다. 임시 무결성 서명만 있으며,
GitHub 다운로드도 Gatekeeper 검사 대상입니다. 첫 실행이 차단될 수 있습니다.
신뢰하고 테스트에 동의하는 경우에만 [Apple 공식 안내](https://support.apple.com/ko-kr/102445)의
앱별 열기 절차를 확인하세요. '손상됨'·'컴퓨터 손상' 경고가 나오면 중단하고
경고 화면을 알려주세요. Gatekeeper/SIP를 끄거나 격리 속성을 삭제하지 마세요.
이번 코드·픽셀·파일 검사 통과는 Apple 보안 검사 통과나 안전성 보증이 아닙니다.

## 확인할 것

- 실행·종료·두 번 실행해도 한 마리만 표시되는지.
- 걷기·추적·간식 따라가기·먹기·식후 흔들림, 특히 먹기 시작 시 멈칫 여부.
- 머리/볼/앞발 조작, 창·Dock 매달리기, 우클릭·메뉴 막대 메뉴.
- 이상하면 칩 모델·macOS 버전·재현 방법·짧은 영상을 알려주세요.

test03은 먹기 효과를 작은 구간으로 읽도록 개선했습니다. 효과 데이터284,190개를
무손실로 보존하고 모션·보간·재생 시간은 유지했습니다. Windows 시뮬레이션에서
첫 먹기 처리 약500ms→0.61ms; Mac 성능 수치가 아닙니다.

SHA256: `8ccd577446457a95802b119a345e6e74280f3b11d9597972be67e21632f5e2d0`

## English

Apple Silicon (M1+), macOS13+. Download the ZIP above, extract, move `Doro Mac Test.app`
to Applications and open. No separate.NET installation. Exit via the pet/menu-bar menu.
Test build, **not native-Mac verified**. No Developer ID or Apple notarization;
ad-hoc integrity only. Gatekeeper may block it. Never disable OS protection or
remove quarantine. Report warnings, Mac/OS details and reproduction steps.
Test03 bounds eating-effect decoding without changing original pixels or timing.

비공식·비상업 팬메이드이며 캐릭터·원 이미지 권리는 별도입니다. 원 팬아트 작가와
이미지 재배포 허락은 미확인입니다. 코드만 MIT이며 자산 권리까지 허가하지 않습니다.
Unofficial non-commercial fan work; code MIT excludes character/assets. Asset permissions unverified.

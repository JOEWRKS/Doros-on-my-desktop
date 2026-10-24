# 오른쪽 눈썹 중복 / 양쪽 미간 주름 수정

## 사용자 지적과 원인

이전 scrunch 후보의 오른쪽 눈썹이 두 개로 보이고, 미간 세로선은 왼쪽만 길고
오른쪽은 누락됐다는 사용자 피드백이다. 원본/후보 확대 렌더를 각각 저장해 대조했다.
원래 오른쪽 윗눈 선 위에 추가 대각선을 덧그린 것이 중복 표현의 원인이었다.
이전 코 주름은 수평 호만 있고 양쪽 짧은 미간 주름의 명시적 한 쌍은 없었다.
왼쪽 아래 눈꺼풀 안쪽 연결부를 길고 굵게 올려 그려 미간의 긴 선처럼 읽혔다.

## 수정 범위

- `HighResolutionGrabRaster.Angry.cs`: 오른쪽 추가 대각선을 제거하고 원본 윗눈 선
  한 개를 그대로 사용. 이 공통 수정은 1·2회/3회 화남 모두에 적용된다.
- 세 번째 표정의 왼쪽 아래 눈꺼풀 안쪽 연결부 끝점을 낮추고 폭1.12→.85DIP로 축소.
- 세 번째 표정에 좌/우 미간 주름을 각각 세로1.3DIP, 폭.75DIP로 추가.
  오른쪽은 머리 경계와 겹치지 않는 안쪽 피부 위치로 조정했다.
- 입·나머지 주름·화남 표시·머리/몸 윤곽·원화·회수 횟수·반응 시간·속도는 유지.
  기존 표정 버퍼 한 개 재사용. 설치본/설정/공개 패키지 변경 없음.

## 확인한 근거

`artifacts/snack-brow-20261023/`:

- `before/neutral-face.png`, `before/furious-face.png`는 수정 전 별도 저장했고 대조했다.
- `proof/furious-face.png`, `proof/anger-five-sizes.png`를 열어 최종 확대/5크기를 확인했다.
- Windows 간식61/61 (`windows-final.trx`). 추가 검사는 원본 오른쪽 눈썹 영역의
  native 픽셀이 그대로인지와 좌우 주름 각 영역에 실제 추가된 검은 픽셀이 있는지를
  확인한다. 단순 표정 플래그 검사가 이 누락을 잡지 못했던 부분을 보완했다.
- Raster10/10 (`raster.trx`): 5크기×2밀도/400프레임 변경영역-전체 일치와 복귀,
  표정/출력 버퍼 재사용. git diff --check 성공(기존 CRLF 안내만).
- 같은 간식 코드입력 회귀를 포함하지만 실제 사용자 마우스/화면 수락은 아니다.

## 후보 및 미확인

새 로컬 Windows 후보:
`artifacts/product-shell/candidate-snack-brow-20261023/runtime/Dororong.exe`.
검사 App DLL SHA256 `D6FAC74AAC56957E86DD5C4796BAFB1712316C0A6D473681713CC82988B232F8`.
패키징 완료. 게시 App/Core DLL은 검사 DLL과 동일 해시를 확인했다.
ZIP SHA256 `D7CF16310287FF5F1DF0AF7FD5438AA08D97D901F5EA97152AABA8C5BD9DD6A8`.
확인 당시 실행 중 Dororong 프로세스 없음. 이 턴에서 앱 종료/실행은 하지 않았다.
실제 화면 수락·실제 Mac 실행은 미확인. 공개 업로드/커밋/푸시/Mac ZIP 생성 없음.
다음은 이 후보 외형 수락이며 새 로그 제출이나 장시간 검사 반복은 요구하지 않는다.

## 후속: 미간선 길이/각도 재지정

사용자는 화면 왼쪽(캐릭터 오른쪽) 선이 여전히 길며 반대쪽은 ↘ 방향이어야 한다고
지적했다. 추가한 미간 곡선 두 개만 수정했다. 화면 왼쪽 세로 범위1.3→.65DIP,
오른쪽은 시작(32.5,53)→끝(33.4,53.9), 제어점도 동일 직선의 ↘45도로 변경했다.
선 폭, 눈꺼풀 연결부/눈썹/입/그 외 그림과 시간 판정은 이번 후속에서 바꾸지 않았다.

`artifacts/snack-fold-angle-20261023/expression.trx` 2/2,
`raster.trx` 10/10. 확대 저장 렌더를 열어 확인했다. 실제 화면 수락은 미확인이다.
`proof/furious-face.png`가 이 수정의 확대본이다. 이전 정지 그림의 픽셀 검사를
사용자 외형 수락으로 간주하지 않는다.
별도 후보 `artifacts/product-shell/candidate-snack-fold-angle-20261023/runtime/Dororong.exe`.
패키징 완료, 검사/게시 App·Core DLL 해시 일치 확인.
ZIP SHA256 `2D2879CFAD669EBBF5A5F36DBC72024191949A3A6214D8907ED3442306A2AB07`.

## 최종 위치 지시: 빨간 원 삭제 / 파란 가이드로 교체

사용자가 `codex-clipboard-fec0f00a-f8c1-49d5-afa8-afaabb93a512.png`에 직접 표시했다.
이전 답변에서는 사용자가 길다고 한 선을 새 미간선으로 잘못 짚었고, 실제 빨간 원은
near lower-lid 안쪽 상승 연결선이었다. 검사 성공과 별개로 대상 해석이 잘못됐다.
이 주석 이미지가 기존 길이/각도 추정 대신 현재 시각적 기준이다.

- 빨간 원의 상승 연결 Curve를 삭제하고, 바깥 아래눈 선도 원 영역 전에 끝내도록 수정.
- 기존 미간 Curve 두 개 삭제·대체. 파란 가이드를 원본 렌더 crop 좌표로 환산하여
  왼쪽(28.5,52.08)→(27.25,54.42), 오른쪽(32.08,52.75)→(33.42,55.42)의 곡선으로 작성.
- 나머지 입/가로 코주름/화남표시/머리·몸/시간/버퍼 정책 유지.
- `artifacts/snack-annotated-20261023/expression-final.trx`2/2, `raster.trx`10/10.
  원에 해당하는 native x60..63/y132..134의 모든 RGB>220 검사로 잔선 제거 확인.
  확대 `proof/furious-face.png` 및 5크기 저장 렌더 직접 확인. 실제 화면 수락은 미확인.
- 후보 `artifacts/product-shell/candidate-snack-annotated-20261023/runtime/Dororong.exe`.
  검사 App DLL SHA256 `8742DE6134FF4202C3BDB976116FDEB9279AEE911C4EF49EC2D7FA20606E2327`.
  설치/설정/공개 배포/앱 실행 변경 없음. 이전 후보들은 사용자 수락본이 아니다.
  패키징 완료 및 검사/게시 DLL 해시 일치 확인. ZIP SHA256
  `6088C9D229B7389B19113E9CBF5E8833E5F9D28967C03E4A6633A19356814757`.

## 후속: 입 위 점처럼 보이는 획 제거

사용자가 입 위 `…` 표현을 지적하고 수정을 요청했다. 해당 위치의 짧은 Ink4획과
불필요해진 Ink helper만 제거했다. 파란 가이드 미간선, 코 위 두 호, omega 입,
기존 화남 표시 및 동작은 유지했다.

새 무잔선 검사에서 처음1건 실패(native66,135). 점 제거 후에도 같은 행의x83을
피부색으로 복사하면서 눈 가장자리 색이 묻어 옅은 가로 띠를 만들던 것을 확인했다.
입 삭제 영역의 donor를 이미 아래눈 삭제에 사용하는 깨끗한 피부(83,147)로 바꿨다.
미간/입 경로 자체는 바꾸지 않았다. 실패 `artifacts/snack-no-dots-20261023/expression.trx`
보존, 최종 `expression-final.trx`2/2 및 `raster-final.trx`10/10 성공.
입 위native x66..75/y134..135 RGB>220, 기존 삭제 원 영역/양쪽 미간/머리 경계 검사 유지.
최종 `proof/furious-face.png`와5크기 저장 렌더를 열어 점/옅은 띠 제거를 확인했다.
실제 마우스 실행/사용자 수락/native Mac 검증은 하지 않았다.
최신 별도 Windows 후보는 `artifacts/product-shell/candidate-snack-no-dots-20261023/runtime/Dororong.exe`.
설치본·설정·공개 배포·앱 실행 변경 없음.
패키징 완료 및 검사/게시 App·Core DLL 해시 일치 확인. App SHA256
`9E70A256917E12DC866A2DE91A1BBCFF3645A866E7B8162423E8A11D6425E24D`,
ZIP SHA256 `A67FE4FF308C3626219DBCF471FAF565FF68081CD997BCB1D46AFCE1E7073D39`.

## 후속: 점 대신 깨끗한 입 위 곡선 주름 복원

사용자는 `codex-clipboard-9aba1f02-c65b-4b20-8fea-9725c7b41478.png`와 함께
지저분한 픽셀을 없애려던 것이지 주름 자체를 전부 없애려던 것이 아니라고 명확히 했다.
no-dots 후보의 빈 입 위 영역 대신 안쪽으로 휘는 연속 곡선2개를 넣었다.
기존4개 점 모양 획을 복구하지 않았다. 폭.55DIP, 왼쪽(27.5,56.85)→(28.5,57.8),
오른쪽(32.65,56.85)→(31.45,57.8). 전체 곡선을 한 번만 AA 합성하는 기존 helper 사용.
중앙 피부 공간과 입선 위 여백을 유지하며 원화/미간/입/동작/속도/캐시 정책은 유지한다.

- `artifacts/snack-clean-wrinkles-20261023/expression.trx`: 처음 연결성 검사가 입 모서리
  픽셀까지 포함하여1건 실패. 영역을 실제 주름에 한정하고 곡선 끝을.25DIP 올렸다.
- `expression-final.trx`: 중앙 omega 꼭짓점까지 빈 공간으로 요구한 검사1건 실패.
  입 모양을 바꾸지 않고, 주름 바로 아래 두 영역의 여백을 확인하도록 검사 좌표 수정.
  이 두 실패는 검증 영역 설계 오류이며 앱 렌더 결함으로 부풀리지 않는다.
- 최종 `expression-verified.trx`2/2: 두 곡선의 연결성/세로 길이, 중앙 피부/입 위 여백,
  삭제된 빨간 원 영역, 양쪽 미간, 원본 눈썹/홍채/알파/중립 복귀를 확인.
- `raster-final.trx`10/10:5크기×2밀도/400프레임 변경영역-전체 렌더 동일 및 버퍼 재사용.
- 최종 `proof/furious-face.png`, `proof/anger-five-sizes.png`를 열어 확대/5크기 저장 렌더 확인.
  이것은 실제 사용자 마우스 조작/화면 수락이나 native Mac 검증은 아니다.
- 별도 Windows 후보 `artifacts/product-shell/candidate-snack-clean-wrinkles-20261023/runtime/Dororong.exe`.
  설치본/설정/앱 실행/공개 배포는 변경하지 않는다. 다음은 사용자 외형 확인이며
  추가 로그나 장시간 메모리 반복검사는 이 시각적 수정 범위에 필요하지 않다.
패키징 완료, 검사/게시 App·Core DLL 해시 동일. App SHA256
`B1FA0C9C159114B9E812175B4D8FD678ADAEFF575AD8C0FABD3E0711536B17BD`.
ZIP SHA256 `02DE3A968BF9944358F41C395227098067406950730CCA05295FCAA8CE09B349`.
`git diff --check` 성공(기존 줄바꿈 안내만).

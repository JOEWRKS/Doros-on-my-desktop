도로롱이 내 컴에?! — Mac 테스트 03 (v0.1.3 기반)

Apple Silicon(M1 이상), macOS 13 이상용 테스트 묶음입니다.
실제 Mac에서 아직 실행 검증하지 않았습니다. 정식 Mac 지원/안전 보증이 아닙니다.
테스트 03은 먹기 시작 시 효과 로딩 지연을 개선했습니다. 모션·픽셀·재생 시간은 유지했습니다.
Intel/Windows용이 아닙니다. .NET·Rosetta·터미널 명령 설치는 필요하지 않습니다.

실행
1. ZIP을 Finder에서 더블클릭해 압축을 풉니다.
2. Doro Mac Test.app을 응용 프로그램 폴더로 옮깁니다.
3. 앱을 더블클릭합니다. 도로롱과 메뉴 막대 머리 아이콘만 나타납니다.
   캐릭터 우클릭 또는 메뉴 막대 머리 아이콘 → 종료로 닫습니다.
   일반 실행에는 개발 검사창이 없습니다. 초기화 실패 시에만 오류·종료 창이 뜹니다.

첫 실행 보안 안내
이 테스트판에는 개발자 신원 서명(Developer ID)과 Apple 공증이 없습니다.
파일 무결성용 임시 서명(ad-hoc)은 개발자 인증이나 악성코드 검사를 뜻하지 않습니다.
따라서 첫 실행이 차단될 수 있고, 아무 경고 없이 실행된다고 보장하지 않습니다.
배포자를 신뢰하고 테스트에 동의할 때만 Apple의 앱별 예외 안내를 확인하세요:
https://support.apple.com/ko-kr/102445
앱 실행을 시도한 뒤 시스템 설정 → 개인정보 보호 및 보안에 '확인 없이 열기'가
표시될 수 있습니다. 항목이 없거나 '손상됨'/'컴퓨터 손상' 경고가 뜨면 중단하고
경고 문구를 알려주세요. Gatekeeper/SIP를 끄거나 격리 속성을 삭제하지 마세요.

먼저 확인할 것
- 칩 모델과 macOS 버전, 화면 배율/모니터 수를 알려주세요.
- 도로롱 걷기·마우스 추적 → 간식 주기 → 먹기·식후 흔들림.
- 머리 잡기/놓기, 볼 당기기, 몸 클릭, 창/Dock에 걸치기와 앞발 조작.
- 캐릭터 우클릭과 메뉴 막대 머리 아이콘 좌·우 클릭, 밖 클릭/Escape 닫기.
- 자동 눕기 OFF 저장 → 종료 → 다시 열어 OFF 유지 확인.
- 앱을 두 번 열어도 한 마리인지, 정상 종료 후 다시 켜지는지.
- 자동 실행은 기본 OFF. 원할 때만 메뉴에서 켭니다. '시스템 승인 필요'는
  등록 완료 표시가 아닙니다. 로그인 항목에서 승인 후 상태를 다시 확인하세요.
  작업 중 강제 로그아웃하지 말고 다음 정상 로그인 때 한 번만 실행되는지 확인하세요.
권한 요청은 자동으로 띄우지 않습니다. 창/Dock 인식이 안 되면 임의로 권한을
추가하거나 시스템 보호를 끄지 말고 현상을 알려주세요.

오류 보고
경고 문구, 수행한 동작, 기대 결과/실제 결과를 알려주세요.
메뉴의 '로그 폴더 열기'로 로그를 찾을 수 있습니다. 공유 전 내용을 확인하세요.
설정/로그: ~/Library/Application Support/JOEWRKS/Dororong
키 입력·화면 이미지·창 제목을 수집하거나 로그를 자동 전송하지 않습니다.
macOS의 실제 권한/다중 화면/잠자기·로그인 동작은 이번 테스트의 확인 대상입니다.

업데이트 / 제거
기존 Mac 테스트판이 있다면 먼저 종료하고 자동 실행을 꺼주세요.
이전 .app은 별도 폴더에 보관하고 새 .app을 응용 프로그램 폴더에 넣으세요.
동시에 여러 버전을 실행하지 마세요. 설정은 유지됩니다.
제거 시 자동 실행 OFF와 OS 로그인 항목을 확인하고 종료한 뒤 .app을 휴지통으로
옮기면 됩니다. 설정/로그는 자동 삭제하지 않습니다. 실패 시 상태를 보고해주세요.

권리 / 라이선스
비공식·비상업 목적의 팬메이드 2차창작입니다. NIKKE에서 파생된 도로롱 팬 캐릭터
참고 이미지에 기반합니다. SHIFT UP 등과의 공식 제휴·승인은 없습니다.
원 팬아트 작가와 이미지 재배포 허락은 미확인입니다.
직접 작성한 코드만 MIT이며 캐릭터·그림·명칭·상표 권리는 제외됩니다.
구성요소 고지는 .app 내부 Contents/Resources/Licenses에 동봉했습니다.
파일 해시는 FILE-HASHES.json과 ZIP 옆 SHA256 파일로 제공됩니다.
해시 일치는 변조/전송 오류 확인일 뿐, 안전성 인증이나 Mac 실행 검증이 아닙니다.

English summary
Apple Silicon, macOS 13+ tester build. Extract, move Doro Mac Test.app into
Applications, then double-click. Includes .NET. Normal launch shows only the pet
and menu-bar head icon; use either pet/menu-bar menu to exit. No diagnostic window
opens during normal startup. Failures show an exit/recovery dialog.
No Developer ID or Apple notarization; ad-hoc integrity
only. Native execution and Gatekeeper acceptance are UNVERIFIED. Follow Apple's
per-app guidance only if you trust the source; never disable OS protections.
Report Mac/OS details and exact failures. Login startup is optional and off by
default. Turn it off before removal; local settings/logs remain. Code MIT excludes
character assets; unofficial non-commercial fan work, asset permissions unverified.

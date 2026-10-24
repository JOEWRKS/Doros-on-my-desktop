도로롱이 내 컴에?! — Mac r104 테스트 09 · 착지 계산 비용 감소 후보

상단 이동 수정은 유지합니다. 착지 때 발 판정에 필요한 한 행의 무거운 픽셀 계산을 기존 작업 스레드에 나누었습니다.
원화·표시 해상도·샘플 수·퍼짐 시간·낙하 속도·Windows는 바꾸지 않았습니다.
사용자에게 추가 로그나 반복 조작을 요구하지 않습니다. 아래 진단 커맨드는 선택 도구이며 실행에 필요 없습니다.
실제 M2/AppKit에서 끊김 해결은 미확인입니다. Windows 호스트의 코드 검증과 구분합니다.
일반 사용: 기존 앱을 메뉴에서 종료한 다음 이 묶음의 Doro Mac Test.app을 실행합니다.
진단 실패 보강: 기록 폴더 쓰기 확인, 시작 오류 파일 보존, 중복 실행/쓰기 실패 표시를 추가했습니다.
정상 기록 실패의 정확한 사용자 환경 원인은 확정되지 않았습니다.

이전 test08 변경 및 선택 진단 안내 (이번 사용자에게 재검사 요구 아님)

test07 로그에서 모델은 위로 가지만 실제 창은 Y=33에서 막히는 것을 확인했습니다.
펫/간식 투명 창에만 상단 프레임 제약을 해제하는 정책을 적용했습니다. 메뉴 창은 바꾸지 않습니다.
호스트 검사는 실제 맥 AppKit 검증이 아닙니다. 상단 제한/낙하 끊김의 실제 해결은 미확인입니다.
이전 화면 전환·팝업 슬라이더 수정은 유지합니다. 외장 클램쉘은 여전히 미검증입니다.
원화·렌더·동작·속도·Windows는 변경하지 않았습니다. 일반 실행에서는 진단 기록이 꺼져 있습니다.
이번 확인: 앱을 정상 실행하고 화면 위쪽 드래그와 놓은 직후 낙하가 막히는지만 확인합니다.
문제가 남을 때만 아래 90초 진단을 사용하세요. 예전 전체 체크리스트/장시간 검사를 반복할 필요 없습니다.
1. 기존 도로롱을 메뉴에서 종료합니다. ZIP을 풀고 앱과 .command를 같은 폴더에 둡니다.
   앱만 응용 프로그램으로 옮긴 경우에도 test09 한 개를 찾아 실행합니다. 없거나 여러 개면 안내 후 중단합니다.
2. 처음 실행 허용이 필요하면 아래 보안 안내대로 앱을 한 번 열고 메뉴에서 종료합니다.
3. '화면 경계 진단.command'가 캐릭터를 직접 켭니다. 앱을 따로 켜지 마세요.
   터미널에 '기록 시작 확인'이 나온 뒤 위로 잡아 올리기·놓기를 합니다. 파일은 시작 때부터 생성됩니다.
4. 앱과 터미널은 켜둡니다. 터미널은 파일에서 완료를 확인한 뒤 종료하고, 펫은 계속 실행됩니다.
   기록이 없거나 조기 종료되면 성공이라고 하지 않고 오류를 표시합니다. 반복 실행하지 말고 메시지를 전달하세요.
5. 완료되면 Finder에서 보낼 로그 파일을 자동 선택합니다. 직접 찾으려면 Finder에서 Command+Shift+G 후:
   ~/Library/Application Support/JOEWRKS/Dororong/Logs
   터미널에 표시된 boundary-세션ID.jsonl 하나만 공유해 주세요. 예전 로그나 개인 파일은 필요 없습니다.

로그에는 실제 펫 창 위치, 요청 위치, 모니터/다른 창의 경계 좌표, 커서 위치, 단계별 최대 처리시간이 포함됩니다.
창 제목·앱 이름·화면 이미지·키 입력은 기록하지 않으며 자동 전송도 하지 않습니다.
기록은 최대 90초/초당 2회, 큐 8개로 제한됩니다. 단계별 처리시간은 화면 표시/GPU 시간이나 CPU·메모리 수락 검사가 아닙니다.
파일 끝의 duration-complete가 없으면 완료 기록으로 해석하지 않습니다.
이 진단 실행에서 heap/GC/앱 정지/자동 조작/권한 변경은 하지 않습니다.

Apple Silicon(M1 이상), macOS 13 이상용 테스트 묶음입니다.
test05는 캐릭터 우클릭 팝업의 테마 연결을 수정한 후보입니다.
우클릭·메뉴 막대 메뉴에서 슬라이더 표시, 드래그 조절, 닫기·다시 열기를 확인해 주세요.
호스트 자동 검사는 실제 macOS 창·입력 검수를 대신하지 않습니다.
실제 Mac에서 아직 실행 검증하지 않았습니다. 정식 Mac 지원/안전 보증이 아닙니다.
이 후보는 같은 프레임의 발/외곽 재검사, 표시용 픽셀 중복 복사와 같은 창 상태의
반복 설정을 줄였습니다. 해상도·프레임 주기·모션 시간은 변경하지 않았습니다.
test04는 호흡·눈 이동 때 변경 영역만 다시 계산합니다. 다른 자세·각도·크기·효과
변경은 전체 렌더로 복귀합니다. 별도 이미지 캐시 없이 기존 표시 버퍼를 재사용합니다.
test02의 M2 CPU 약 99.8% 피드백 이후 만든 후보이며 실제 Mac 개선은 미확인입니다.
같은 크기·화면에서 평상시/앉기/보행을 각각 30초 정도 보고 CPU 범위를 비교해 주세요.
호흡·눈 추적 후 볼 당김/놓기, 눈 감기, 100↔150% 조절 시 잔선/잘림도 확인해 주세요.
CPU 수치와 함께 macOS 버전·크기·상태를 알려주세요. 호스트 검사는 Mac CPU 검증이 아닙니다.
수락된 Windows r104의 고해상도 원화·상호작용·100~150% 연속 크기 조절을 연결했습니다.
이전 144px 테스트판을 확대해 포장한 것이 아닙니다. Windows 설치본은 변경하지 않습니다.
Dock을 발판으로 쓰지 않고 화면 맨 아래에서 걷습니다.
Dock보다 앞쪽(z-order)에 표시하며 일반 창 걸치기는 유지합니다.
미확인: 실제 Apple Silicon 실행·Retina 조작·창 순서·성능·장시간 메모리.
목 연결·침 효과의 경계 계산을 수정하고 r104와 원본 크기 및 배율별 픽셀을 비교했습니다.
실제 실행에 쓰이지 않는 비정규 먹기 시점의 진단용 대체 렌더에는 경계 차이가 남습니다.
전체 픽셀 동일 또는 Mac 전체 동작 통과를 주장하는 빌드가 아닙니다.
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
- 머리 잡기/놓기, 볼 당기기, 몸 클릭, 일반 창에 걸치기와 앞발 조작.
- 화면 바닥에서 걷는지, Dock 아이콘 앞에 표시되는지 확인하세요. Dock에는 걸치지 않습니다.
- 창 최대화/복원·메뉴/오버레이 표시·Dock 숨김/좌우 이동 때 바닥 높이가 바뀌지 않는지 확인하세요.
- 캐릭터 우클릭과 메뉴 막대 머리 아이콘 좌·우 클릭, 밖 클릭/Escape 닫기.
- 자동 눕기 OFF 저장 → 종료 → 다시 열어 OFF 유지 확인.
- 크기 100↔150% 연속 왕복 → 발 위치/클릭 위치 확인 → 종료·재실행 시 크기 유지.
- 앉은 채 추적·볼 당기기, 수면 눈 감김, 기울여 잡은 상태 놓기·착지 직전 퍼짐.
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
This r104 port tester walks at the physical screen bottom, in front of Dock (z-order), not on
Dock's top edge. Ordinary window support/hanging remains. Please test window-mode
transitions, overlays and Dock orientation/auto-hide; native layering is unverified.
The r104 high-resolution renderer, interaction coordinator and continuous size
slider are connected. Test02 corrects neck/drool software coverage and compares
source and scaled frames against the accepted r104 DLL. Non-60Hz eating ages
(diagnostic fallback, not emitted by the runtime) still differ. This is not an
all-possible-frame-equivalent or native-Mac-accepted release.
No Developer ID or Apple notarization; ad-hoc integrity
only. Native execution and Gatekeeper acceptance are UNVERIFIED. Follow Apple's
per-app guidance only if you trust the source; never disable OS protections.
Report Mac/OS details and exact failures. Login startup is optional and off by
default. Turn it off before removal; local settings/logs remain. Code MIT excludes
character assets; unofficial non-commercial fan work, asset permissions unverified.

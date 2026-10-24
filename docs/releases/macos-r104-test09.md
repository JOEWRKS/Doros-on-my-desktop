# Mac r104 test09 — Apple Silicon 테스트 앱

[ZIP 바로 다운로드](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/macos-r104-test09/Doro-Mac-arm64-r104-test09.zip)

Apple Silicon(M1 이상), macOS 13 이상. .NET/Rosetta 별도 설치 불필요.

## 실행

1. 기존 도로롱을 메뉴에서 종료합니다.
2. ZIP을 풀고 `Doro Mac Test.app`을 응용 프로그램에 옮겨 기존 앱을 교체합니다.
3. `Doro Mac Test.app`을 더블클릭합니다. `.command` 파일은 실행할 필요 없습니다.

Developer ID 서명·Apple 공증이 없는 테스트판입니다. 보안 기능을 끄지 않습니다.
추가 로그나 반복 테스트 제출은 요청하지 않습니다.

## 배포 확인과 한계

- 2026-10-22 공개 prerelease `macos-r104-test09`, release420714460/asset665860349.
- 기존 검수 ZIP 그대로 업로드. 99,195,870bytes, version104.9, arm64/.NET8.0.32.
- SHA256 `2f08c3022461705a65e86be38461651afe8db114457abcb2602c4e73d154774e`.
- 로그인 없이 공개 링크로 재다운로드하여 크기·SHA256 일치를 확인했다.
- [수정 및 호스트 검증](../verification/2026-10-22-mac-test09-landing-cost.md).
  실제 M2 착지 끊김 해소, native 표시/장시간 메모리와 외장 클램쉘은 미검증이다.
- 바이너리 전달용 릴리스다. 태그는 이전 공개 소스 기준점
  `f15a0322b01b5ef13e47fcc5527354abbd375e2e`이며 추가 로컬 test08/test09 변경의
  정확한 소스 커밋이 아니다. 이 전달에서 소스 커밋·푸시는 하지 않았다.
- Windows/설정/기존 릴리스/수락한 원화·형태·화질·동작·속도는 변경하지 않았다.

비공식·비상업 팬메이드. 직접 작성한 코드만 MIT이며 캐릭터·이미지·상표는 제외한다.
캐릭터 이미지 재배포 허락은 미확인이고 공식 승인·제휴를 의미하지 않는다.

# Windows r104 — 수락본 포터블

[Windows x64 바로 다운로드](https://github.com/JOEWRKS/Doros-on-my-desktop/releases/download/windows-r104/Doros-on-my-desktop-r104-win-x64.zip)

기존 도로롱을 트레이에서 종료 → ZIP 전체 압축 해제 → **Dororong.exe** 실행.
EXE만 따로 옮기면 안 됩니다. .NET 포함, 관리자 권한 불필요.
설치 프로그램이 아니므로 기존 설치판·바로가기는 자동 교체하지 않습니다.

- 수락된 r104 그림·상호작용·속도·연속 크기 슬라이더를 유지합니다.
- 일반 실행과 로그인 실행도 r104 고해상도 렌더러를 선택합니다.
  초기 크기는 150%이며 저장된 크기가 우선합니다.
- 기존 assembly 버전 0.1.3은 보존합니다. 이 배포 식별자는 **windows-r104**이며
  이전 v0.1.3 설치 파일을 덮어쓰지 않습니다.
- Mac 이식의 후속 수정은 포함하지 않습니다.

## 검증과 한계

자세한 패키지 검사 결과는
[배포 검증 기록](https://github.com/JOEWRKS/Doros-on-my-desktop/blob/release/windows-r104/docs/verification/2026-10-21-windows-r104-delivery.md)에 있습니다.
회귀 1,534개와 새 실행 검사 1개, 실제 데스크톱 실행·종료를 확인했고 공개 ZIP을 다시 받아 해시를 대조했습니다.
다른 PC·혼합 DPI·새 사용자 환경 검증은 미완료입니다.
코드 서명 없는 미리보기이므로 Windows 경고가 나올 수 있습니다. 보안 기능을 끄지 마세요.
캐릭터·이미지 배포 허락은 미확인이고, 공개는 공식 승인·제휴를 의미하지 않습니다.
직접 작성한 코드만 MIT이며 캐릭터·이미지·상표는 제외됩니다.

SHA256 (ZIP): `27BB7D390F6042B4972D152F4B9EE1E2EE8949DF9CEA55BA61AB3A1549DB0103`

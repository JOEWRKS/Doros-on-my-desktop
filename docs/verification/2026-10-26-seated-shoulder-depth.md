# 앉은 앞발 어깨/머리 겹침 수정 · 2026-10-26

사용자가 curved-paw 실행 화면에서 어깨가 머리 앞으로 튀어나온다고 지적했다.
원인은 `SeatedPaw.OverHead`가 앞발뿐 아니라 둥근 어깨 뿌리까지 머리 위에
합성한 것이다. 이전 프레임 검수가 이 겹침 문제를 놓쳤다.

`OverHead`에서 어깨 중심 반경9DIP는 전경 합성을 금지하고,9~13DIP는
앞발 쪽으로 부드럽게 전환한다. 팔 자체 윤곽/곡선, 방향 추적, 뻗는 거리,
시간/쿨타임/몸 털기와 몸통 합성은 유지했다. 머리 뒤의 팔 렌더는 유지하므로
어깨가 잘리거나 투명 구멍이 나도록 삭제하는 방식이 아니다.

검증 근거: `artifacts/seated-shoulder-depth-20261026/`.

- 검사16개 통과. 추가 회귀검사는5개 목표 각도×64프레임에서 어깨 반경 안의
  머리 색을 바꾸지 않는지, 위로 든 앞발 끝은 전경에 유지되는지 확인한다.
- 고정/움직이는 목표×100/125/150%×양방향 앞발768프레임을 순서대로
  직접 확인했다. 시작/끝 픽셀 일치, 잘림 없음. 확대 전후 비교도 확인했다.
- 몸 털기348프레임은 이전 검수본과 바이트 일치. 기존 동작396표본 동일.
  상세 해시는 `visual-review.json`, 전후는 `before-after.png`.
- 코드로 넣은 진단 입력이며 사용자 실제 조작이나 수락으로 간주하지 않는다.
  긴 메모리/CPU 검사와 실제 Mac 검증은 수행하지 않았다.

패키지: `artifacts/product-shell/candidate-seated-shoulder-depth-20261026/`.
검사한 App/Core와 패키지 일치 확인.
App SHA256 `D9BD3FC6EEFBC0713FFB6E085CE41D338FEF3A22788E10FB1387CB8AF8960E6B`.
ZIP SHA256 `AC0D7010B77ED2E8EC5DA2E92D17EE9A76792CC7C9BBA94686A8155C141056CF`.
이전 실행 PID37560 유지, 정상 종료 요청 후 교체 대기. 실제 외형 수락 미확인.
설치본/설정/원화/Core/Mac 변경 없음.

## Superseded after user rejection

User found residue in this AFTER preview. The 9–13 DIP fade still changed hair;
the radius-only test and visual review missed it. See [correction](2026-10-26-seated-head-occlusion.md).
Do not treat this candidate as accepted or its review as proof of no residue.

# r78 실제 측정과 r79 접촉 계산/진단 저장 할당 수정

## 실행과 사용자 확인

2026-10-19 사용자 준비 확인 뒤 r78 정상 투명 1.5배를 02:28:33.526 KST 실행했다. PID138056, `artifacts/render-cost-20261019-r78/release/Dororong.App.exe`. App DLL SHA256 `F6EAFC87E203060D238F418AB62126BEEA02663B255067AED45980DD6B874924`를 실행 전 대조했다. 기존 프로세스 없음과 로그 파일 미존재 확인 후 300초 측정 옵션으로 실행했다. 설치본·설정·원화·형태·속도 미변경.

실제 OS 입력은 사용자가 했다. 에이전트가 화면을 직접 보거나 직접 마우스로 검수한 결과가 아니다. 이번 실행에서 창 목록 재조회/시각 캡처는 하지 않았다. 약90초에 조작 종료를 안내했지만 마지막 실제 입력은94.417초, 마지막 볼 복귀 상태는96.002초다.

- 직접 입력: Body8, RightCheek7, ClickOnly2. 볼7번 모두 Release→None; 머리8번 모두 Falling→Landing→Supported. 강한 착지4번(69.699/75.960/80.974/89.476초 첫 상태 표본).
- 사용자 답: **“일단 문제는 없었는데 높이서 떨궜을 때 쫙 퍼지는게 아주 조금 느렸음”**. 기본 조작 문제 없었다는 범위만 확인. 강한 착지의 느린 퍼짐은 미해결이며 전체 통과가 아니다.
- `artifacts/render-cost-20261019-r78/native-interactions.json`, 24,480,306B, SHA256 `427DD7FC8FB52D168ED7F680BB5055648005C75640229B0172C168E6F9D107E0`. 저장 완료 뒤 추가 조작 불필요 안내. 앱은 종료하지 않았다.

## 실제 성능/메모리

elapsed300007.959ms, CPU429140.625ms(단일 논리 코어 대비 약143%). 시작5초 제외 tick33532개: 평균8.798ms, p95 17.485ms, p99 30.484ms, 최대212.919ms, 50ms 초과26회. tick work 평균3.571ms, p95 10.558ms, 최대198.505ms. WPF callback34041개, 평균8.666ms/p99 33.609ms/최대201.206ms/50ms 초과32회. 하드웨어 표시 FPS 측정이 아니다.

표시 raster30598회, 누적 managed 할당9156.44MiB, gen2 collection delta21회. r77의180초/15300raster/5092.76MiB/355회와 길이·동작·초기 캐시 상태가 다르므로 직접적인 동일 부하 개선율이나 누수 해결 판정으로 사용하지 않는다.

| 약 경과초 | managed MiB | heap MiB | private MiB |
|---|---:|---:|---:|
| 0 |22.1|22.9|107.4|
|30|54.3|71.2|162.7|
|60|58.6|72.6|159.9|
|90|58.8|73.0|161.4|
|120|60.5|74.3|164.4|
|150|101.9|101.2|191.6|
|180|163.8|161.6|252.7|
|210|171.6|162.8|256.8|
|240|242.9|241.5|333.8|
|270|247.3|242.9|334.9|
|299|119.4|153.5|277.7|

managed 최대261,358,584B(276.874초).136.136초부터 자율 Walk/Idle을 반복하고218.617초 Sleep으로 전환한다. 후반 전체가 정지된 idle 부하는 아니다. 메모리가 올라갔다가 회수되는 부분이 확인됐지만 장시간 누수 배제 근거는 아니다. 측정 종료 후 private356,954,112B(02:33:42)에서284,893,184B(02:41:09)로 감소했다. 강제GC하지 않았다.

강한 착지 주변 네 구간의 최대 tick 간격은36.584/47.328/32.968/44.989ms, 합산 raster 비용 최대24.118/27.458/21.726/22.058ms. 같은 구간 gen2 delta0. 작은 지연이 남았다는 근거이고, 사용자가 느낀 퍼짐 지연의 단일 원인 확정은 아니다. 원래 착지 곡선/속도는 바꾸지 않았다. 주요 긴 지연 중 볼 구간에는 sampling/sole 계산 비용이 관측됐으나 표본 인접 비용을 인과 관계로 단정하지 않는다.

## 실제 heap 조사

사용자에게 잠깐 멈출 수 있음을 알린 뒤 heap dump를02:36:11~24 수집했다. 측정이 끝난 이후여서 위300초 성능 수치에 덤프 중지가 포함되지 않는다. `artifacts/render-cost-20261019-r78/native-memory-1.dmp` 307,801,337B. 실제 프로세스 데이터이며 공개 업로드 금지.

- WriteableBitmap1개, CachedBitmap11개, BitmapSourceSafeMILHandle123개. r78의 한 표시 surface 재사용은 실제 heap에서도 확인했다. 모든 native 자원 누수가 없다는 뜻은 아니다.
- GC allocated161,257,968B/committed166,662,144B, free45,408,944B. float95개84,868,680B. 실제 locomotion cache64개, version299. capture.disposed=true, frames.size=0, previousCount30598. 측정 목록은 종료 후 비워졌다.
- byte[]→Point[] 접촉 캐시의 CWT Container4223개와 Entry[]4223개가 있었다. live pixel slot이 갱신될 때 Remove/Add를 반복하고, 매 contact마다 List·ToArray·변환 Point[]를 새로 만들던 코드와 대조했다. 이 경로의 반복 할당은 확인했지만 전체 메모리 증가가 모두 이 때문이라고 주장하지 않는다. ready for finalization0은 조사 시점 값이다.

## r79 수정과 자동 검사

`PlatformContactPresentation`의 재사용 pixel key에는 같은 weak cache entry/List를 유지하고 invalid flag만 갱신한다. raw sole용 List와 변환 Point[]도 용량을 재사용한다. 유효 개수를 별도로 관리해 윤곽 축소/빈 픽셀에서 이전 꼬리를 사용하지 않는다. alpha128 경계, row union, affine 변환, sole band clipping 계산 자체는 유지한다. weak key는 presenter 폐기/배율 변경 시 해당 키가 회수될 수 있도록 유지한다.

진단 기록의 `JsonSerializer.Serialize`→전체 UTF-16 문자열→File.WriteAllText 대신 FileStream에 직렬화한다. 종료 시 큰 로그 전체 복사 경로 제거이며 실행 중 메모리 증가 전체의 수정으로 확대하지 않는다.

- `artifacts/render-cost-20261019-r79/tests/contact-before.trx`: 수정 전6개 중5통과/1실패. warmed contact1000쌍에107,209,528B 할당.
- `contact-fixed.trx`: 접촉/affine/live surface 관련35개 통과. 별도 측정 출력은 후속 검사에서1000쌍128,000B. 합성 fixture의 할당 감소이지 실제 native 전체 메모리 감소 수치가 아니다.
- `memory-pixels.trx`:9개 중8통과/1실패. 5크기7종12장=420개 raw Pbgra32가 r76 변경 전 baseline과 동일하고 live surface도 동일. `pixels/costs.json` 및 PNG420개 저장. 직접 시각 검수 아님.
- 위 실패는 새 JSON 검사에서 cold serialization 할당을 임의의4MiB 이하로 가정한 데서 발생했다(7,971,936B). 제품을 통과시켰다고 쓰지 않는다. 검사 기준을 기존 전체 문자열 직렬화와 직접 비교하고, 전체 payload의 UTF-16 사본 절반보다 작은지 확인하도록 바꿨다. 후속 결과는 아래에 기록한다.
- 원화 grab/hunt-body/locomotion bank 및 Core DLL 해시가 수락 r70/r77 기록과 동일. 원화·모양·속도와 착지 타이밍은 수정하지 않았다.
- `regression.trx`: 최종416개 통과,4분54초. 접촉/affine/alpha/live surface,PetLoop,고해상도 렌더,수명,놓기·착지 경계 포함. warmed contact1000쌍89,856B(앞 검사128,000B; 측정 실행마다 달라 전체 native 절감량으로 환산하지 않는다). JSON 비교는 전체 문자열59,369,384B 대 stream55,528B,출력100개 레코드와 마지막 인덱스/파일크기/종료 목록 해제 확인. 이 비교는 앞 실패의 cold4MiB 고정 예산과 다른 검증이며 cold7.97MB 실패를 삭제하지 않는다.
- 후보 `artifacts/render-cost-20261019-r79/release/Dororong.App.exe`. Release `--no-build` 게시 후 검사/게시 App DLL SHA256 동일 `610636D8390D1C78CA598D7428E2FB9C3EAF751E486B3C532B3D7DD611A8FF21`. Core SHA256 `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`. r79는 아직 실행하지 않았다.
- r78은02:47경에도 PID138056으로 실행 중(private350,670,848B,handles603).02:41경보다 다시 증가했으므로 안정화 선언하지 않는다. 사용자에게 r79 실제 재측정 준비 시 r78 트레이 종료 후 알려 달라고 요청했다. 강제 종료/자동 교체 없음.

## 남은 경계

r79 최종 회귀/후보 식별은 위와 같이 완료했고 실제 측정은 사용자 준비 대기다. 실행 중인 r78은 유지한다. r79 실제 장시간 메모리·강한 착지 퍼짐·입력/화면 재검증 전에는 누수/렉 해결이나 전체 통과를 선언하지 않는다. 다른 크기의 실제 OS 상호작용과 다른 환경도 미검증이다.

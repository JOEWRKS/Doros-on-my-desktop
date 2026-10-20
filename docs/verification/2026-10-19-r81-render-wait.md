# r81 — 렌더 작업자 대기와 반복 캐시/창 이름 할당

## 범위와 시작 근거

사용자 요청:남은 렌더 지연과 메모리 증가 원인을 수정하고 실제 실행으로 검증. 원화·수락 형태·속도·설정·설치본 유지,미확인은 통과 처리하지 않는다. r80의550개 회귀/420픽셀/300초 실제 무조작 결과 및 heap 근거는 [r80 기록](2026-10-19-r80-idle-walk.md)에 있다. 보행만 사용자 문제없음 확인,긴tick32회 및 장시간 누수 미완을 이어받았다.

시작 시r80 PID76060 실행 확인(03:34:56 KST,private225,890,304B/handles607). 이전03:32:50 private217,759,744B와 이 한 표본만으로 누수 판단하지 않는다. 후속 종료 요청에 사용자 “ㅇㅇ”,이후 프로세스 없음 확인. 강제종료/설치본 교체 없음.

## 변경과 원인 경계

1. `RasterRowScheduler`는 caller가 모든 행을 처리해도5개 worker job 모두 시작돼 countdown을 내릴 때까지 기다렸다. r80 sparse8줄 우회 뒤에도 큰 표시 raster에 남은 구조다. 이제 caller가 자신의 행 처리를 끝낸 뒤 원자적으로 아직 시작하지 않은 job을 직접 처리한다. 이미 실행 중인 job은 기존처럼 완료까지 기다리므로 반환 후 픽셀 쓰기/해제된 countdown 접근이 없다. 오래된 queue entry는 pending delegate가 비워져 렌더 객체를 붙잡지 않고,queue도worker수×4로 제한한다. queue가 꽉 차면 제출하지 않은 job을 caller가 처리한다. 행별 계산·순서·품질·프레임 시계는 변경하지 않는다. 이 구조적 불필요 대기와 r80의 모든 지연32회 인과가 확정됐다는 뜻은 아니다.
2. `HighResolutionBodyInk`는 같은 scratch 원본/출력 배열을 쓰면서도 매 invalidation마다 ConditionalWeakTable 키를 Remove/Add했다. 살아 있는 약한 참조 항목의 Scale/Valid만 갱신해 tombstone/컨테이너 교체를 피한다. 픽셀 알고리즘/선 두께 값은 그대로다. 약한 키가 폐기 원본을 살리지 않는 검사를 유지한다.
3. `DesktopMetadataReader.ReadWindow`와 taskbar class 조회의256자 StringBuilder를 thread별 재사용한다. r79 실제 할당 표본에서 이 호출 경로 char배열 약252MiB를 확인했다. 문자열 결과/윈도우 상태/프로세스 ID는 캐시하지 않는다. 매 호출 Clear,thread간 버퍼 분리,기존 native API/오류 판정 유지. 모든 native 마샬링/string 할당이 사라졌다는 주장은 아니다.
4. 실제 capture JSON에 `workerWaitMs`를 추가한다. 실제 진행 중인 worker 완료를 기다리는 구간의 Stopwatch 누적 차이이며,전체 sampling/OS/DWM 대기와 같지 않다. 계산량·원화·모션에는 영향 없고 측정 비용은 작은 timestamp/counter 연산이 추가된다.

## 자동 검사

- `tests/targeted.trx`:29개 통과. 처음에는 새 async worker 검사에 blocking Task.Wait analyzer 경고2개가 있었고,await 기반 검사로 정리했다(제품 코드 오류로 인한 실패는 아님).
- `tests/pixels.trx`:25개 통과.5배율1/1.1/1.25/1.35/1.5×7종×12=420 raw Pbgra32가r80과 동일. retained live/sparse sole/기존 frozen 불변 비교 포함. 직접 시각 검수는 아니다.
- `tests/r80-cache-before.trx`:새 cache identity 검사1개를 기존r80 App DLL로 실행해 Assert.Same 실패 재현. 전용 artifact 복사 폴더만 r80 DLL을 사용,제품/test 원본 폴더 변경 없음. 동일 검사 r81에서는 통과한다.
- 새 worker 검사는 실제 worker를 아예 시작하지 않고 call이 끝난 뒤 queue를 실행해도 각 행1회/late write없음을 확인한다. 이미 시작한 worker를 잠시 막으면 렌더가 먼저 반환하지 않는 검사,caller 예외 후 delayed queue가 buffer를 만지지 않는 검사도 포함한다. OS 스케줄링/실제 화면 입력 검사는 아니다.
- 창 이름 버퍼의 동일 thread 재사용/비움,다른thread 격리,10000회 scratch호출 할당1KiB 미만 검사. native 환경의 모든 GetClassName 오류 조합을 실행했다고 보지는 않는다.
- grab/hunt-body/locomotion 원화 SHA256 및 Core DLL SHA256은 r80 기록과 동일.

## r81 최종 검사와 실제 실행

`regression.trx`:555개 통과/실패0/건너뜀0,4분22초. workspace Release 게시 App DLL과 검사 DLL SHA256 `BB345964ACF90897E7B3AB5154D80A71DFA54146AF4216BF326D526C9AF8128C` 동일,Core hash 유지.
03:43:49 KST 정상 투명1.5배 PID138712 실행,300초 capture 시작을 안내했다. 이 구간 빌드/테스트/덤프/추적 병행 없음. 사용자는 무조작 요청 이후 실제로 조작했고,그 구간을 무조작으로 보고하지 않는다.

`artifacts/render-cost-20261019-r81/native-idle-walk.json` SHA256 `C0FCDF27568B7F03DC49E73CA3F8A45A8E3A8E9145CA9517E4647A0C8F597D5E`,elapsed300000.5954ms/CPU360500ms. 입력4건:Body53.863/216.741/222.537초,RightCheek220.793초. 상태 표본Dragged30/Idle1254/Sleep531/Walk990;directBodyDragEntry1/Hold29/Settle5/BodyPending2/CheekPull7/CheekRelease4/None2757. 약100ms 상태 표본은 정확한 개별 전환 경계가 아니다.

- 시작5초 제외tick32820개,mean8.989/p9521.784/p9931.399/max123.382ms,50ms초과6회(Idle2/Walk4). workmean3.340/p958.821/max103.609ms/50ms초과5회.
- callback33387개,mean8.836/p9524.280/p9936.118/max103.782ms/50ms초과21회. 실제 모니터/GPU FPS 아님.
- 할당2085.65MiB,gen2증가6회,표시raster29679회. r80의2924.60MiB/긴tick32회보다 작은 관측값이지만,r80은입력0/r81은입력4이며 상태 비중·전체 PC 부하를 통제하지 않아 동일 조건 개선율로 계산하지 않는다.
- WorkerWait 합4979.439ms/평균callback당0.149ms/max89.280ms.75.974초raster64.91ms중wait57.38ms,212.583초raster101.59ms중wait89.28ms. 해당callback gen0/1/2=0. 시작하지 않은 worker 대기를 없앤 뒤에도 **실행 중인 worker 완료 대기**가 긴 구간을 실제로 확인했다.
- managed peak252.22MiB,r80의200.42MiB보다 높다.0/30/60/90/120/150/180/210/240/270/299초 managed=21.9/86.7/150.4/205.5/247.1/133.5/190.1/137.2/179.8/249.5/171.0MiB. private=94.0/173.3/230.5/290.3/333.4/252.6/276.0/281.3/304.9/339.3/264.2MiB. 회수는 있지만 메모리 안정화 통과 아님.

사용자 확인 **“걷는 거엔 없어”**,이어서 **“대롱에도 없어 볼따구랑”**:보행·대롱거림·볼의 체감 끊김/이상 없음 범위로 기록한다. 모든 전환/크기/장시간·에이전트 직접 시각 검수로 확대하지 않는다. 직접 화면 열람은 이번 turn 하지 않았다.

## r81 실제 heap과 추가 원인

300초 종료 이후 잠깐 멈출 수 있음을 안내하고 `native-memory-1.dmp` 수집. 실제 프로세스 데이터이며 외부 공개/업로드 금지. 이 정지는 위 성능 수치에 포함되지 않는다. 강제GC 없음.

- heapallocated213,927,752B/committed220,160,000B,free9,505,664B.
- float배열187개171,234,088B 중 SOS `dumpheap -live/-dead` 결과 live93개95,091,832B/dead94개76,142,256B. 회수를 기다리는94개는 모두810,024B 배열이다. 전체 메모리 증가를 영구 누수라고 부르지 않는다.
- 보행 cache 실제64개/version704. `HighResolutionBodyInk` 약한 참조 컨테이너1개(이전r80표본165개),entry13개,표시WriteableBitmap1개. cache 재등록 수정이 실제 heap에 반영된 근거이며 전체 native누수 배제는 아니다.
- 큰2.25MB배열 표본은 FlutterLayers 경로로도 보유됨을 확인했다. 이번 run에는 직접 잡기/볼 입력이 있어r80 무조작heap과 보유 상태가 다르다.
- 알려진 GetBody miss 경로는810KB 배열을Clone하여64개 FIFO 교체 때마다새로 만든다. dead배열94개 전부의 과거 할당 스택을 dump만으로 복원할 수는 없으나,앞선 EventPipe 호출스택과 코드로 반복 생성 경로가 확인되어r82에서렌더 전용 수명 계약으로 재사용한다.

## 후속 r82 후보

사용자가r81 트레이 종료 요청에 “ㅇㅇ” 응답했고 프로세스 없음 확인. r81의 수락 형태/속도를 유지하며,렌더가 끝난 뒤 사용되지 않는 owned motion cache 슬롯만 재사용한다. 일반Get/GetBody로 외부에 제공된프레임은 영구 immutable 계약을 유지하고 재사용 대상에서 제외한다. 캐시 총상한64개/원본bank/시뮬레이션Core는 그대로다. 상세 후보/실제 결과는 [r82 기록](2026-10-19-r82-owned-motion.md)에서 관리한다.

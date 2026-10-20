# r77 — 볼 놓기 단일 복귀와 보행 캐시/접촉 계산

## 범위와 원인

수락 r70 원화/형태/간식 속도와 r76 잡기 개선을 유지한다. 설치본·설정은 변경하지 않는다.

- 볼을 잡을 때 HuntingSession이 초기화돼 놓은 뒤 새 준비 동작이 시작되는 동시에, presenter가 캡처된 몸을 별도로 upright(156)까지 보간했다. 서로 다른 두 몸 타임라인이 충돌했다.
- 엎드림 볼 당김/반동 중 준비 시계를 일시 정지하고, 놓은 뒤 그 시계로 이어간다. 별도 몸-to-upright 보간은 제거하고 기존 얼굴/시선 복귀는 유지한다. 포인터가 멀어지면 기존 departure recovery 하나가 담당한다. 도약 dwell은 볼 조작 중 초기화해 즉시 튀지 않게 한다.
- 보행 캐시는 raw float와 선 보정 float를 각각 보유했다. 수정은 같은 결과의 보정 완료 배열만 최대64개 보유하고 raw/선 보정 scratch 한 쌍을 재사용한다. 공개된 배열은 불변이며 캐시에서 빠져도 덮어쓰지 않는다.
- 포화 캐시의 `Dictionary.Keys.First()`는 삭제 슬롯 재사용 때문에 최근 삽입을 즉시 제거할 수 있었다. 포화 후 보행 반복 검사5개가 실패해 확인했고, 명시적 FIFO로 수정했다. 이는 반복 압축 해제/할당 원인이지만 실제 지연 전체의 원인이라고 단정하지 않는다.
- 발 접촉 계산은 sparse 픽셀을 WIC BitmapSource로 만들었다가 다시 읽었다. 같은 샘플 버퍼를 동기 소비하도록 바꿔 임시 비트맵/왕복 복사를 제거했다. 실제 표시 비트맵은 여전히 frozen copy이며 다음 렌더로 덮어쓰지 않는다.

## 자동 검증과 실패 이력

`artifacts/render-cost-20261019-r77/tests/`:

- `release-before.trx`: 새 놓기 타임라인6개 모두 수정 전 실패(첫 복귀 BodyFrame71→72). 최종 사용자 움찔 영상 재현으로 확대하지 않는다.
- `release-fixed.trx`: 놓기 근거리5크기/원거리 및 기존 누름 순서 등16개 통과.
- `memory-sole-pixels.trx`: 최초11개 중6통과/5실패. 픽셀420개/발 접촉 byte·기하 비교는 통과했으나 포화 캐시 반복 참조 검사5개 실패. 위 FIFO 수정의 근거다.
- `final-pixels.trx`: 최종 코드6개 통과. 5크기7종12장=420개 렌더가 r76 변경 전 baseline과 raw Pbgra32 SHA256 동일. 별도 원본 보정 알고리즘과 5크기13종 motion 배열을 대조한다. 해시 검사는 직접 시각 검수가 아니다.
- `regression.trx`: 최종399개 통과(3분33초). PetLoop, high-resolution rendering, raster lifetime, capture lifetime, composition timer, contact geometry, prepared motion cache를 포함한다. 자동 상태 주입은 실제 OS 마우스 입력이 아니다.

## 후보 식별

- `artifacts/render-cost-20261019-r77/release/Dororong.App.exe`
- 게시/테스트 App DLL 동일: `2741FDA171B9D70F9E97CC57F96E4F8035DB8B5B9E103777A2658F8EDD075DF2`
- Core DLL 수락 r70 동일: `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`
- grab bank: `F3281BAFF95167D40F827B0928724192F52048C3EE590379FBD473E2F46A9A04`
- hunt body bank: `D3514DD116C5916DF0201E16CED2C8CCB5892DBACCC7774C4BC502405B0FD802`
- locomotion bank: `5A4D7FFD79542B3A06C704604BF4ECB0B1A6F3A128EE4552423255DA7B18676A`

## 실제 검증 경계

사용자가 준비 대기를 요청했으므로 실행하지 않고 자동 검사를 마무리했다. 이후 “ㄱㄱ”로 준비 확인해 정상 투명1.5배 r77(PID139160)을 02:01:27 KST 실행했다.180초 기록을 저장했고 앱은 강제 종료하지 않았다.

- 실제 입력: 볼5건,머리7건. 볼5건 모두 놓기→None,머리7건 모두 낙하→Landing→Supported 복귀. 강한 착지3회 포함. 마지막 직접 입력100.308초,마지막 복귀102.408초라 안내했던90초와 달리 실제 자율 구간은 그 이후다.
- 사용자 “ㅇㅇ. ㅇㅇ”: 놓기 움찔 소멸과 r76 대비 체감 끊김 감소를 확인했다. 에이전트의 직접 화면 검수로 바꾸지 않는다. Computer Use 창 목록에 펫이 없었고 실제 OS 조작은 사용자가 했다.
- elapsed180036.938ms,CPU302656.25ms. 시작5초 제외 tick16499개: 평균10.609ms,p95 22.224ms,p99 32.128ms,최대222.625ms,50ms초과35회. 후반120~180초에는50ms초과0회지만 앞 구간에는 볼/잡기 지연이 남았다. r76과 동작/길이가 달라 단순 횟수 비교로 개선 판정하지 않는다.
- 총 할당5092.76MiB,gen2 355회. 약90/120/150/179초 managed104.6/107.3/128.8/123.9MiB,private268.3/273.3/325.8/317.7MiB. 종료 직후 별도 프로세스 확인 private약694.7MiB로 증가했다가02:07:30 약529.9MiB로 내려왔다. 회수되는 부분이 있다는 관찰이며 누수 없음 판정은 아니다.
- raw record `artifacts/render-cost-20261019-r77/native-interactions.json`, SHA256 `9984A5E7E916E4749127A0A548744D1566266AFAE2B0619C22EB1C601D271A49`.

## 반복 증가 후속 조사

사용자가 원인 규명/해결을 재요청해 workspace에만 dotnet-dump10.0.750501을 설치하고 r77 heap dump를 수집했다. 원격 전송/강제GC/프로세스 강제 종료 없음. 대상은 펫 PID139160뿐이다. `artifacts/render-cost-20261019-r77/native-memory-1.dmp`는 실제 프로세스 데이터이므로 공개 업로드 금지.

덤프의 managed allocated156,243,392B/committed160,641,024B,그중 free33,451,800B. float101개98,368,824B. 실제 locomotion cache64개,JSON PooledByteBufferWriter의 rentedBuffer=null,index0. 큰 배열 참조는 presenter→raster→locomotion cache 및 FlutterLayers에서 확인했다. CachedBitmap40개,finalization ready0. 이 한 시점으로 native 누수/누적 증가 전체를 배제할 수 없다.

r78에서는 매 프레임 frozen BitmapSource/WIC 객체를 만드는 표시 경로를 같은 픽셀의 단일 WriteableBitmap으로 대체하는 후보를 검사한다. 기존12개 프레임 캐시는 재사용 byte 배열로 유지하고,변경된 alpha 클릭 판정/접촉 경계를 명시적으로 갱신한다. 이는 반복 할당 경로 수정이며 실제 메모리 안정화는 비교 실행 전 미확인이다. 사용자 “ㄱㄷ”로 r78 실행/측정은 대기한다.

### r78 후보 검증

- 실제 r77 dump의 capture.disposed=true,previousCount=15300. 원래 코드상 표시 raster miss마다 frozen BitmapSource를 만들므로 약3분에15300개 표시 비트맵 생성 경로를 탔다. 한 시점 CachedBitmap40개/캐시12개와 반복 할당량은 다른 지표다. bounded cache만 검사한 과거 검증으로 native allocation pressure를 배제할 수 없었다.
- r78 `live-surface.trx` 초기29개 중26통과/3실패.1.1/1.35의 WIC 반올림 DPI를 요청 DPI와 직접 비교해 surface가 매번 재생성되던 후보 결함2개를 수정했다. 나머지는 Source 없이 먼저 배치한 새 테스트의 크기 문제로,실제 Source 설치 뒤 재배치해 클릭 검사를 수행하도록 수정했다. 실패를 누락하지 않는다.
- 이어진 병렬 build는 기존 testhost DLL 잠금으로 실패했다. 기존 검사 종료 후 순차 재실행했다.
- `surface-pixels.trx`: 최종15개 통과.5크기420개 원래 frozen 픽셀/raw hash와 live display 픽셀 동일,5크기 surface 객체/byte buffer 동일 참조 재사용,mutable alpha·접촉 경계 및 native-host harness 검사 포함. 실제 OS 사용자 입력 검수는 아니다.
- 게시/테스트 App DLL SHA256 동일 `F6EAFC87E203060D238F418AB62126BEEA02663B255067AED45980DD6B874924`.
- 후보 `artifacts/render-cost-20261019-r78/release/Dororong.App.exe`. Core/원화 SHA256은 위 수락값 유지. r78은 아직 실행하지 않았다.
- 사용자 대기 중 r77 프로세스가 더는 조회되지 않음을02:14:10 확인했다. 에이전트가 종료하지 않았다. 준비 확인 전 r78을 켜지 않는다.
- r78 `regression.trx`: 최종405개 통과(3분59초),게시/검사 DLL 동일. 원화3개 bank와 Core 해시 재대조 동일. r77보다 짧은 테스트 시간/렉 개선/장시간 누수 해결로 해석하지 않는다. 실제 r78 메모리·지연·화면은 미검증이고 사용자 준비 대기다.

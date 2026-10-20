# r82 — 렌더 전용 모션 버퍼 재사용

## 근거와 범위

[r81 실제 기록](2026-10-19-r81-render-wait.md)에서 렌더worker 대기57/89ms와 회수대기810KB float배열94개(76,142,256B)를 확인했다. 수락 원화/형태/속도/설정/설치본 유지. 렌더가 완전히 끝난 배열에만 재사용 권한을 부여하고,공유·보관되는 프레임은 변경하지 않는다.

## 수정

`HighResolutionLocomotion.BorrowBody`는 다음BorrowBody 호출 전까지만 유효한 렌더 전용 참조를 반환한다. `HighResolutionGrabRaster.RenderPixels`만 이 계약을 사용한다. 모든 행worker는r81 scheduler에서 동기적으로 완료된 뒤 반환하므로 다음렌더가 시작할 때 이전body참조에 쓰는worker가 없다. head/effect는 일반Get 경로이며 독립 원본을 유지한다.

- dictionary/FIFO 전체상한64개 유지. owned slot의 오래된 항목을 evict할 때 배열을 재사용하고 알파/선 보정의 derived-cache 유효성을 무효화한 뒤 원래 데이터로 채운다. 스트로크 연산/픽셀값은 바꾸지 않는다.
- Get/GetBody가 owned frame을 요청하면 해당 슬롯의 재사용 권한을 제거해 이후 eviction에서도 외부 보관참조를 덮어쓰지 않는다. frame0의 감은머리원본도 별도보존한다. Get/GetBody의 기존 immutable API 계약은 그대로다.
- scale<=1은원본압축데이터를 재사용슬롯으로읽고,scale>1은기존stroke보정결과를Clone대신재사용슬롯에Copy한다. 일반 effect/raw frame 호출에 필요한 할당은 여전히 남을수있다. 메모리 증가 전체가 사라졌다고 간주하지 않는다.

## 렌더 dispatch 대안 실험

테스트용 thread-local CallerOnly 경로로 같은raster의 단일호출자/기존병렬샘플링을 비교했다. 앱 기본값은false라제품은병렬유지. `DORORONG_DISPATCH_PROFILE=1`일때만선택된test가실험하며,scheduler동작을앱설정으로노출하지않는다.
각종류5warmup+60표본,1.5배,동일픽셀비교. `artifacts/render-cost-20261019-r82/tests/dispatch.trx`1개통과.

| kind | parallel mean ms | caller-only mean ms |
| --- | ---: | ---: |
| idle | 1.745 | 7.335 |
| walk | 1.360 | 5.826 |
| grab | 1.808 | 6.928 |
| landing | 10.183 | 35.639 |

전체단일스레드방식은추가계산지연이커적용하지않았다. 이실험은native상호작용성능/전체PC상태통제벤치마크가아니다. r81에서확인한잔여worker대기까지제거됐다는주장없음.

## 검사 진행

`targeted-pixels.trx`27개통과:5크기owned slot64개재사용/기존immutable보존/수치대조/borrowedcache warm40miss4MB미만,기존cache/sole/raster수명검사와420개r80원시픽셀동일. 직접시각검수아님. 이후감은머리·수면effect와currentbody가서로덮어쓰지않는5배율검사및immutable path와할당직접비교를추가했다.

- `regression.trx`:562개 통과/실패0/건너뜀0,4분25초. 임의성능실험은환경옵션없음으로이회귀내에서no-op이며별도dispatch.trx에서실행했다.
- `owned-layers-final.trx`:수면레이어보존추가후11개통과. 동일warmup후40cache miss의managed할당은기존immutablepath32,419,840B/ownedrenderpath18,880B. 큰배열을보관하는일반API는기존계약대로새배열을반환한다. 이단위검사비율을실제앱전체개선율로사용하지않는다. 회귀와추가검사의중복항목을독립573개로합산하지않는다.
- 게시Release App DLL과검사DLL동일: `A25828A4459F39A3F624941100C5E572D00930356AC9BFE0AADC09C9FAFCA3E4`. Core DLL `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`,원화3bank hash r80/r81과동일.

## 실제 측정 시작

사용자의r81종료확인뒤프로세스없음재확인.03:59:42 KST PID142876,정상투명1.5배Release실행. `--performance-capture-seconds=300`,출력 `artifacts/render-cost-20261019-r82/native-motion-memory.json`. 첫2분무조작/이후3분사용자볼·머리잡기·흔들기·놓기·착지안내. 측정중빌드/테스트/덤프/trace미실행.

## 실제 결과와 사용자 거절

로그 SHA256 `661A6E96CCE3625BC0F21BBF4A7ABE12546366CBF2AC8FA36AC5EB9848F7738F`,300034.8882ms/CPU355343.75ms. 실제 입력15건(Body11/RightCheek4),각 놓기/착지복귀11회 및 볼놓기4회,강한착지5회. 마지막수동착지복귀212.120초. 시작5초 제외tick32524개,mean9.0714/p9519.4752/p9931.2257/max51.915ms,50ms초과1회(57.824초Walk,입력전). workmax41.5621ms/50ms초과0. callback33386개/max58.6294ms/50ms초과18회는GPU FPS가아니다. workerWait총2706.95ms/max9.3678ms.

누적할당1724.57MiB/표시raster29403/gen2 12회(r81은6회). managed peak157.146MiB/299초139.47MiB,private299초244.18MiB. managed 0/30/60/90/120/150/180/210/240/270/299초=21.90/80.89/83.95/96.35/125.98/110.95/116.45/118.11/139.64/139.73/139.47MiB. 마지막heap154.106MiB/fragment15.153MiB. r81보다낮은지연/할당/peak관측이나입력과상태비중이달라통제개선율아님. 5분만으로장시간누수해결판정불가.

측정후실제heap `native-memory-1.dmp`: float배열111개97,829,064B,그중live95개84,868,680B/dead16개12,960,384B. r81dead94개76,142,256B보다작지만GC직후시점/상태통제없음. 보행cache64개/version678,재사용가능set32개(count64-free32),표시WriteableBitmap1개/BodyInk CWT container1개. 일반immutableAPI/effect할당과유효캐시는남는다. dump는측정후수집했고강제GC없음. 공개업로드하지않는다.

사용자 **“쫙 퍼지는게 이전의 한박자 느린 걸로 회귀한 거 같은데”**,추가확인 **“바닥에 닿은 뒤 잠깐 멈춤”**. 강한착지미통과. r79와r82 Core DLL은동일하며100ms상태표본에서충돌후~0.1초내spread .84~.98→1을관측했지만첫표시프레임/실제DWM지연은판정못한다. 직접영상/에이전트화면검수없음. 낮은CPU지연로그로사용자결함을기각하지않는다.

후속조사에서충돌tick의잔여시간을버리고phaseSeconds=0으로그리는기존경로를확인했다. r83에서원래강한착지곡선은유지하면서그시간손실을고친다. 이는이전버전에도있어r82체감회귀전체의인과확정은아니다. [r83검증](2026-10-19-r83-impact-onset.md).

# r84 — 앱 내부 착지 프레임 기록

## 범위

사용자환경의Xbox Game Bar는투명펫을녹화대상으로선택하지못하고첨부된구형캡처도구에는동영상버튼이없었다. 외부녹화를반복요청한잘못된안내를중단하고사용자“시작해”에따라앱내부기록을구현한다. r83의시계진행변경은철회상태그대로유지한다. 원화/렌더형태/펼침곡선/간식속도/설치본/설정미변경. 이번변경은진단기능이지시작지연수정이아니다.

## 구현·메모리·표시 증거 경계

- `--performance-capture=<json>`와`--landing-frame-capture=<directory>`를함께명시할때만생성. 앱기본실행에는픽셀복사/버퍼/디스크저장없음.
- 실제Body누름후arm,낙하를거친첫강한착지한번만trigger. 직전16개tick을ring으로유지하고착지부터최대80개또는800ms까지수집. 반복조작으로완료clip을덮어쓰지않는다.
- 처음만고정배열할당,전체64MiB상한/최대96프레임.1.5배384×384는56,623,104B(54MiB). DPI에따라cap감소;예산상지원불가크기는명시적진단오류로남기고앱을종료시키지않는다. 크기/DPI/픽셀형식이바뀌면resample없이중지/오류기록.
- tick말미에현재HighResolutionImage.Source의Pbgra32를복사. pixelcopy시작/끝Stopwatch tick,복사ms,성능기록과같은기준ms,요청PlatformPose/위치/퍼짐,CPU렌더spread,창Left/Top을저장한다. 모든행렌더완료뒤읽어재사용scratch덮어쓰기와분리한다.
- 기록deadline/사용자정상종료때만PNG인코딩/디스크저장을시작. background작업으로내보내고정상종료시에는완료까지기다린다. rawbuffer소유권이관찰자에서exporter로이동하고성공/실패후배열참조를비운다. 출력은고유run폴더이며기존파일을덮어쓰지않는다.
- `frames.json`에trigger/error/stopReason/원래버퍼예산/각PNG와타임스탬프/원시Pbgra32 SHA256기록. PNG의unpremultiplied변환과원시픽셀해시는별개다. 낙하/강한착지미발생시이미지0개/trigger=false로저장한다.
- **이것은CPU측펫bitmap이며화면녹화가아니다.** WPF외부clip/최종DWM합성/모니터scan-out을검증하지못한다. 다른창/바탕화면/개인화면은수집하지않는다. 진단용54MiB와복사시간이측정에포함되므로기존비진단실행의누수·성능과단순비교하지않는다.

## 검사

- 초기`capture.trx`10개통과. 테스트의blocking-await analyzer경고3개는async테스트로수정했다.
- `capture-final.trx`15개통과. opt-in/arm/낙하선행/최대96/순서/이미지복사/크기변경중지/디스크실패/중복export/버퍼해제,5배율실제presenter출력저장포함.
- 합성테스트자료`artifacts/render-cost-20261019-r84/synthetic-frames`는실제마우스조작이아닌결정적Core+Presenter실행이다.1.5배016/017/018 PNG를직접열어0/16/32ms의중립→퍼짐→완전퍼짐이미지가저장되는것을확인했다. 전체native시각검수라고주장하지않는다.
- 게시App DLL과검사DLL SHA256 `445978EC306B9B81E9E66E85FE82BADA54CB93F94668EB67FFE2D7AA8A6B2BAA`. Core hash r82와동일 `2F45BC49FD9E58B8B5716E2F92F3E9641CCEEB975C52C08F2B31CA28BDF90888`,원화3bank hash그대로.
- `wired-final.trx`:33개통과(52초). 실제RuntimePerformanceCapture의Body입력→OnTick→Dispose→PNG/manifest내보내기통합검사,기존live surface/owned body버퍼검사포함. 앞선검사와중복합산하지않는다.

## 실제 기록 시작

사용자교체준비“ㅇㅇ”,기존r82종료확인후16:54:58 KST PID142288 정상투명1.5배r84실행.120초성능기록`native-landing.json`,프레임출력`native-frames/`로명시적옵션활성화. 높은곳잡기→놓기한번요청/실행시작안내. 측정중빌드/테스트/trace/덤프없음.

## 실제 프레임과 직접 대조 결과

16:56:59기록저장/비동기PNG내보내기완료확인. 실제Body2건(6.761초/30.584초),첫강한착지clip만정책대로저장. `native-frames/landing-20261019-075459-d1b6ad9cdfd442a49e2b105514c8a9bb/`에384×384 PNG64개와frames.json. triggered=true/error=null/stopReason=post-impact-window. 충돌전256.47ms부터후814.92ms까지이며이후복귀전체는이번clip범위밖이다. 두번째조작으로첫clip을덮어쓰지않았다.

실제원시픽셀해시고유값6개:000~016동일중립17개,017/018/019/020각1개,021~063동일hold43개. 014/017/018/019/020/021을직접열어6종류픽셀내용을대조했고015/016도열었으며해시/알파7533픽셀로중립과동일함을확인했다. **64개의독립화면을직접봤다는주장이아니다.** 고유CPU이미지내용6개를직접열람하고64개순서/시각/좌표/픽셀동등성을검사한것이다. 해당창의실제DWM프레임은여전히미관측.

| PNG | 접촉 프레임 대비 CPU 완료 ms | 표시 리소스 spread | work ms | 관찰 |
| --- | ---: | ---: | ---: | --- |
| 015 | -16.24 | 0 | 0.71 | 낙하중중립,poseY855.47 |
| 016 | 0 | 0 | 1.01 | Landing진입,poseY874/windowTop778,중립그대로 |
| 017 | 32.29 | 약0.84 | 17.24 | 첫퍼짐 |
| 018 | 66.38 | 약0.98 | 19.36 | 최대근처 |
| 019 | 87.27 | 약0.94 | 20.56 | 작은반동 |
| 020 | 108.15 | 약0.99 | 20.52 | hold직전 |
| 021 | 129.12 | 1 | 20.67 | 완전퍼짐유지시작 |

첫퍼짐업데이트시작간격16.06ms,work17.24ms. 이전work1.01ms를고려하면end-to-end32.29ms와맞는다. 동일callback의raster17.00ms/preparation2.17ms/sampling14.43ms/sole10.13ms/bitmap0.09ms/workerwait5.86ms. sole은raster의부분구간이고workerwait는sampling과겹치므로합산하지않는다. 해당callback GC0/1/2증가없음. **퍼짐진행을앞으로건너뛰는r83수정없이**,첫퍼짐처리중접촉계산·sampling이상당시간을사용함을실제확인했다. 이데이터만으로전체OS표시지연이나체감원인의유일성을확정하지않는다.

프레임복사평균0.1737ms/p950.2626ms/max0.3762ms(저장64표본). 이작은추가비용도실측에포함되고상시무비용이라고주장하지않는다.

성능로그SHA256 `0EB85ECDA57EA4F95212843358F948155A9CAE94DE658D7E5E2E2495530C6697`,manifestSHA256 `0EFE6C8874EB27B9CF1EB094DE3918A3ECFED9648E7F45C2C4CC11AD7D49B94E`.120007.7263ms/CPU111640.625ms.5초제외tick12208개/max47.6577ms/50ms초과0,workmax39.6856/50ms초과0,callbackmax54.924/50ms초과8. 이것은기록기능검증이지렉해결판정아님. 누적할당761.00MiB/gen2 3회,managedpeak182.93MiB/119.9초170.15MiB/private268.46MiB에는명시적54MiB영상버퍼가포함돼기존r82와단순비교불가. export후버퍼소유권해제코드/자동검사확인,실제heap전후덤프는미실행.

## 인계

내부기록기능구현과실제clip획득은완료,사용자에게별도녹화프로그램을요구하지않는다. 현재r84 PID142288 정상투명실행유지/120초진단종료. 수락원화/형태/속도동일,착지시작지연수정은아직안했다. 다음은기존퍼짐곡선/초기프레임을유지하면서첫퍼짐의sole/sampling계산비용을분리·줄이고같은내부clip으로재대조하는것이다. 화면실표시/다른배율native/복귀전체/장시간메모리미완을유지한다.

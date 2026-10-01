# Original-contour Static Artwork Implementation Plan

**Historical, rejected static result.** Do not execute this completed plan again
or treat old chat replies as current approval. The latest user authorized original
image cutouts; current implementation and evidence are recorded in `TASKS.md`.

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 원본 비율의 완성된 부위 그림과 관절로 기본·앉은 끝자세를 만들고, 정지 비교로 사용자 수락을 받는다.

**Architecture:** 원본 계측/윤곽 데이터 → 닫힌 부위 그림 → 자세 입력 → 면과 선을 구분한 합성으로 분리한다. 머리는 원본 텍스처를 유지하고, 몸은 원본을 본뜬 명시적 곡선으로 그린다. 기존 실패 도구 및 제품은 변경하지 않는 격리된 Canvas 정지 실험이다.

**Tech Stack:** CommonJS, Node.js `node:test`, 기존 번들 `@napi-rs/canvas`; 새 패키지/엔진 없음.

**Spec:** `docs/superpowers/specs/2026-10-01-original-contour-sitting-design.md`

## Global Constraints

- 작업 루트: `D:/JOEWRKS/DororongDesktopPet`. 아래 명령은 모두 이 위치에서 실행한다.
- 원본: `src/Dororong.App/Assets/dororong-canonical.png`, 96×96. 파일을 수정하지 않는다.
- 참고: `tools/PreviewLocomotion/assets/seated-final-10-2.png`; 정렬·계측·비교 전용이며 실행 그림에 붙이지 않는다.
- 머리/얼굴/머리카락/리본은 원본 한 장. 크기 변형 없이 위치·회전만 허용한다.
- 다리는 root/bend/tip 3관절, 발바닥 접점은 별도 측정점이며 추가 발목 축이 아니다.
- 가려진 어깨·허벅지도 닫힌 면으로 완성한다. 원본 alpha=255로 새 그림을 자르지 않는다.
- 임의 타원/캡슐 몸, 몸통 전체 축소, 이미지 모핑, 전신 가중 변형, 알파 closing/erosion, 사후 구멍 메우기 금지.
- 기본 순서: 뒤쪽 다리 → 몸통/엉덩이 → 가까운 뒷다리 → 가까운 앞다리 → 머리. 보이지 않는 반대 뒷다리는 그리지 않는다.
- 기본 윤곽은 원본 외곽과1native-pixel 이내를 초기 기술 목표로 한다. 몸의 AA/RGBA 동일성은 요구하지 않는다.
- 흰색/어두운 배경, 실제96px/확대 보기, 부위 그림 및 별도 관절 표시를 모두 제공한다.
- 애니메이션·제품 프레임 뱅크·설치·커밋·푸시·배포 제외. 체크포인트는 로컬 변경 검토이며 커밋 지시가 아니다.
- 기존 `canonical-bind.cjs`/`canonical-sit.cjs`, 그 실패 테스트 및 이전 산출물 보존. 새 검사 통과로 이전 실패를 완료 처리하지 않는다.

## Review Focus

1. 흰 배경에서 숨은 연결부 틈이 어두운 배경에서 드러남 → Task3 `opaque sockets stay opaque in both endpoint poses`.
2. 보이지 않는 윗뒷다리 그림이 다시 빈 면이 됨 → Task2 `hidden roots and upper thigh are complete filled artwork`.
3. 부위 배열 순서에 따라 앞다리가 뒤 선에 덮임 → Task3 `depth is explicit and foreground skin hides rear ink`.
4. 앉은 PNG를 없애면 실행 그림이 바뀜 → Task4 `runtime is independent of seated reference`.
5. 잘못된 자세 좌표/배율로 찌그러진 그림을 조용히 출력함 → Task3 `invalid coordinates and scales are rejected`.

---

## Files and shared contracts

모든 신규 도구/검사는 `tools/PreviewLocomotion/` 안에 둔다.

| 파일 | 책임 |
| --- | --- |
| `original-contour-data.json` | 원본을 본뜬 곡선, 머리 마스크/기준점, 실측 굵기·접점, 두 끝자세의 저작 데이터 |
| `original-contour-source.cjs` | 읽기 전용 원본 계측, 머리 추출, 데이터 검증 |
| `original-contour-art.cjs` | 완전한 부위 면 및 보이는 선; 관절에 연결되는 곡선 기준점 |
| `original-contour-poses.cjs` | 기본/앉은 정지 자세와 자세 검증; 시간 함수 없음 |
| `original-contour-render.cjs` | 명시적 z순서, 불투명 피부 겹침, 별도 선 가림 합성 |
| `export-original-contour.cjs` | 정지 비교/부위/관절/근거 JSON 내보내기 |
| `verify-original-contour-source.cjs`, `verify-original-contour-art.cjs`, `verify-original-contour-render.cjs`, `verify-original-contour-export.cjs` | 위 책임별 회귀 검사 |

공유 타입(JSDoc): `Point=[number,number]`; `LegName='farFront'|'nearFront'|'nearHind'`.
`Pose={head:{pivot:Point,angle:number},chest:Point,pelvis:Point,legs:Array<{name:LegName,joints:[Point,Point,Point],sole:Point}>}`.
`PathCommand={op:'M'|'L'|'C'|'Z',points:Point[]}`; 각각 점 개수 1/1/3/0.
`Part={name:'torso'|LegName,fill:PathCommand[],ink:PathCommand[][],rootInk:PathCommand[][],root:Point,coverage:Point[]}`. 최종 검수 보완: 숨길 뿌리 선(`rootInk`)과 보여야 할 발끝 선(`ink`)을 따로 저작하며, 피부 면은 공통으로 유지한다.
`coverage`는 저작한 연결부 안쪽의 검사점들이다. 원본 alpha나 완성 합성 그림에서 자동 생성하지 않는다.
`Measurements={headMask:PathCommand[],headLandmarks:Point[],bodyBoundary:Point[],ink:{rgb:[number,number,number],width:number},legWidths:Record<LegName,number>,standing:Pose}`. 앉은 자세 데이터는 완성된 부위 그림을 기준으로 Task3에서 저작한다.
각 검사 파일은 Node assert/test와 기존 Canvas를 가져오며 `pixels(canvas)`는 RGBA 배열을 반환하는 로컬 도우미로 정의한다. 테스트 준비는 각 Task의 인터페이스를 사용한다.

### Task 1: 원본 계측과 변하지 않는 머리

**Files:** Create `original-contour-data.json`, `original-contour-source.cjs`; Test `verify-original-contour-source.cjs`.

**Interfaces:**
- Consumes: `Image`/Canvas API, 원본 PNG 및 참고 PNG(계측 단계에서만).
- Produces: `measure(source:Image,canvasApi:object):Measurements`, `extractHead(source:Image,measurements:Measurements,canvasApi:object):Canvas`, `validateMeasurements(measurements:Measurements):void`.

- [x] **Step1: Write failing tests.** `setup()`은 원본을 불러와 `measure`/`extractHead` 결과 `{source,m,head}`를 반환한다. 독립적으로 원본을96px Canvas에 그려 마스크 내부 픽셀을 대조한다.

```js
test('source dimensions and original head are preserved', async()=>{
  const {source,m,head}=await setup();
  assert.deepEqual([source.width,source.height],[96,96]);
  assert.equal(head.width,96); assert.equal(head.height,96);
  assert.ok(m.headLandmarks.length>=3);
  assertHeadPixelsEqualSourceInsideMask(head,source,m.headMask);
  assert.equal(alphaAt(head,16,69),0); // body corner is not head
  assert.ok(alphaAt(head,35,65)>128); // jaw remains
});
test('measurement distinguishes bone tips from sole contacts', async()=>{
  const {m}=await setup();
  for(const leg of m.standing.legs){
    assert.equal(leg.joints.length,3);
    assert.notDeepEqual(leg.sole,leg.joints[2]);
    assert.ok(m.legWidths[leg.name]>0);
  }
  assert.ok(m.ink.width>0); assert.equal(m.ink.rgb.length,3);
});
```

`alphaAt` reads one pixel; `assertHeadPixelsEqualSourceInsideMask` compares original RGBA where the independently rasterized mask is fully opaque, and verifies zero head alpha outside it.

- [x] **Step2: Run** `node --test tools/PreviewLocomotion/verify-original-contour-source.cjs`. Expected FAIL: new source module missing; no earlier test weakening.
- [x] **Step3: Implement** the three signatures above. Trace source contours/width bands in96px coordinates; record visible shafts separately from hidden root estimates and label those estimates in JSON metadata. Fit source outline color/width; retain untouched head pixels with a source-pixel mask. During authoring, compare the seated reference aligned by three head landmarks, not body-only scaling; store ratio observations, not its pixels. Put standing measurement joints inside the traced complete-part sketch and revise those estimates when Task2 finishes the artwork. Seated joints wait until Task3; old8px/13.5px guesses are not defaults. No PNG reference import in runtime functions.
- [x] **Step4: Run** the same test command. Expected PASS. Save original hash and actual measurements in data metadata; inspect source/head mask overlay so the test does not bless a mistaken cut.
- [x] **Step5: Checkpoint** review only Task1 paths via `git diff --` plus untracked file inspection. No commit; do not touch product files.

### Task 2: 원본을 닮은 완전한 부위 그림

**Files:** Create `original-contour-art.cjs`; Modify `original-contour-data.json`; Test `verify-original-contour-art.cjs`.

**Interfaces:**
- Consumes: `Measurements`, `Pose`, `PathCommand` from Task1/shared contracts.
- Produces: `buildParts(m:Measurements,pose:Pose):Part[]`, `renderPart(part:Part,m:Measurements,canvasApi:object):Canvas` (96×96; independent skin/ink artwork).

- [x] **Step1: Write failing tests.** `setup()` loads Task1 measurements; rasterize fill paths independently with Canvas for assertions.

```js
test('hidden roots and upper thigh are complete filled artwork',async()=>{
  const {m}=await setup(); const parts=buildParts(m,m.standing);
  assert.deepEqual(parts.map(p=>p.name).sort(),['farFront','nearFront','nearHind','torso']);
  for(const p of parts){
    assert.equal(p.fill[0].op,'M'); assert.equal(p.fill.at(-1).op,'Z');
    assert.ok(p.coverage.length>=2);
    for(const q of [p.root,...p.coverage]) assert.ok(fillContains(p,q));
    assert.ok(filledPixelCount(renderPart(p,m,canvas))>0);
  }
  assert.ok(upperThighArea(parts.find(p=>p.name==='nearHind'),m.standing)>0);
});
test('standing widths and outward contours follow measurements',async()=>{
  const {m}=await setup(); const parts=buildParts(m,m.standing);
  for(const name of ['farFront','nearFront','nearHind'])
    assert.ok(Math.abs(measuredShaftWidth(parts,name,m.standing)-m.legWidths[name])<=1);
  assert.ok(parts.every(p=>p.ink.every(path=>path.length>0)));
});
```

`fillContains` uses independent Canvas path hit testing; `upperThighArea` counts fill on the root-to-bend segment, excluding distal shin. `measuredShaftWidth` samples perpendicular to the visible source shaft at the width station stored in data metadata. No generic ellipse surrogate or empty upper thigh may satisfy this test.

- [x] **Step2: Run** `node --test tools/PreviewLocomotion/verify-original-contour-art.cjs`. Expected FAIL: new art module missing.
- [x] **Step3: Implement** `buildParts`/`renderPart`. Author closed Bézier boundaries from the original body/limbs, extending hidden roots underneath opaque cover. Bind root/bend/tip-relative landmarks to supplied joints; keep measured thickness and rounded rump, and do not outline internal cuts. Define `coverage` with at least two interior socket points per part. Seated silhouette is not an interpolated endpoint path: the same authored parts consume a different pose.
- [x] **Step4: Run** Task1+2 tests. Expected PASS. Inspect independent parts: complete thigh/root tissue, original thin/tapered legs, no alpha=255 clipping. Revise art/measurement together if inconsistent; do not force a pose with whole-part nonuniform scaling.
- [x] **Step5: Checkpoint** scoped local diff and independent-part images only. No animation, commit or installation.

### Task 3: 두 끝자세 및 선/면 가림 합성

**Files:** Create `original-contour-poses.cjs`, `original-contour-render.cjs`; Modify `original-contour-data.json`; Test `verify-original-contour-render.cjs`.

**Interfaces:**
- Consumes: Task1 `measure`/`extractHead`, Task2 `buildParts`, shared types.
- Produces: `getPose(name:'standing'|'seated',m:Measurements):Pose`; `validatePose(pose:Pose):void`; `createRenderer(source:Image,m:Measurements,canvasApi:object):{render(pose:Pose,scale?:number):Canvas,renderHead(pose:Pose):Canvas,renderParts(parts:Part[],pose:Pose,scale?:number):Canvas}`. Scale defaults1 and supports1/3; all coordinates/angles finite.

- [x] **Step1: Write failing tests.** `setup()` returns `{source,m,r}` via Task1 and `createRenderer`; `copyPose` uses `structuredClone`.

```js
test('opaque sockets stay opaque in both endpoint poses',async()=>{
  const {m,r}=await setup();
  for(const name of ['standing','seated']){
    const p=getPose(name,m), parts=buildParts(m,p), image=r.render(p);
    for(const part of parts) for(const q of part.coverage)
      assert.ok(alphaAt(image,...q)>=254,'socket must not expose background');
    assertSocketInkNotDoubled(image,parts,m); // declared socket ink ownership, not global repair
  }
});
test('depth is explicit and foreground skin hides rear ink',async()=>{
  const {m,r}=await setup(); const p=getPose('seated',m), parts=buildParts(m,p);
  assert.deepEqual(pixels(r.renderParts(parts,p)),pixels(r.renderParts([...parts].reverse(),p)));
  assertForegroundOccludesRearInk(r,parts,p,m);
});
test('pose changes artwork, not head size or body scale',async()=>{
  const {m,r}=await setup(), a=getPose('standing',m), b=copyPose(a);
  b.legs.find(l=>l.name==='nearHind').joints[1][0]-=2;
  assert.notDeepEqual(pixels(r.render(a)),pixels(r.render(b)));
  assert.deepEqual(pixels(r.renderHead(a)),pixels(r.renderHead(b)));
  assertStandingBoundaryDistance(r.render(a),m.bodyBoundary,1);
  assertStraightForelegs(getPose('seated',m));
});
test('invalid coordinates and scales are rejected',async()=>{
  const {m,r}=await setup(), p=getPose('standing',m), bad=copyPose(p);
  bad.pelvis[0]=NaN; assert.throws(()=>r.render(bad),/finite|coordinate/i);
  for(const s of [0,-1,Infinity]) assert.throws(()=>r.render(p,s),/scale/i);
});
```

Independent helpers: `assertSocketInkNotDoubled` checks predeclared socket cross-sections against the source ink-width budget; `assertForegroundOccludesRearInk` places a conspicuous rear test stroke across a fully covered near-foreleg interior and verifies foreground RGB/alpha; no production repair used as oracle. `assertStandingBoundaryDistance` compares exposed raster body boundary (alpha>=128) bidirectionally with source boundary, excluding the head mask, maximum distance1. `assertStraightForelegs` checks root/bend/tip collinearity within0.25px distance and downward direction.

- [x] **Step2: Run** `node --test tools/PreviewLocomotion/verify-original-contour-render.cjs`. Expected FAIL: new pose/render modules missing.
- [x] **Step3: Implement** the interfaces. Now author seated joints inside Task2's completed artwork and store them in data JSON: hips down, forward near-hindfoot, two straight supporting forelegs; preserve original thickness/volume. Render source head rigidly, using body curves driven by anatomy rather than global deform. Fixed z-order ignores caller array ordering. Separate opaque skin overlap from hidden root ink; do not punch alpha holes to hide lines. Render native96px once; enlarged3x review uses nearest-neighbor on that result rather than repeatedly resampling.
- [x] **Step4: Run** Tasks1–3 tests. Expected PASS. Visually inspect white and `#202630` composites at96px/3x: round rump, upright exposed forelegs, forward hindfoot, nonflattened belly. A numerical pass with a bad shape is a failed visual milestone: fix artwork/pose, never lower boundary/coverage assertions to disguise it.
- [x] **Step5: Checkpoint** scoped files and static endpoints. Old27-pass/1-fail prototype remains historically unresolved; no product replacement.

### Task 4: 정지 비교 산출물과 사용자 수락 인계

**Files:** Create `export-original-contour.cjs`; Test `verify-original-contour-export.cjs`; Modify `tools/PreviewLocomotion/README.md` (isolated tool section), `TASKS.md` (this milestone only).

**Interfaces:**
- Consumes: Tasks1–3 APIs; reference PNG only in exporter comparison panel.
- Produces: `exportStatic(outDir:string):Promise<{outDir:string,files:string[]}>`; CLI `node tools/PreviewLocomotion/export-original-contour.cjs [output-directory]`. Default output `artifacts/original-contour-static-20261001/`; unknown flags fail.

- [x] **Step1: Write failing tests.** Export into a freshly created OS temp directory using `mkdtempSync`; do not overwrite earlier artifacts. Test names/assertions:

```js
test('static export contains native and enlarged comparison evidence',async()=>{
  const result=await exportStatic(tempDir);
  for(const f of ['standing.png','seated.png','comparison-native.png','comparison-3x.png',
    'parts.png','joint-overlay.png','verification.json']) assert.ok(result.files.includes(f));
  for(const f of ['standing.png','seated.png']) assert.deepEqual(await pngSize(tempDir,f),[96,96]);
  const v=readJson(tempDir,'verification.json');
  assert.equal(v.animationGenerated,false); assert.equal(v.visualAcceptance,'pending');
  assert.deepEqual(v.backgrounds,['#ffffff','#202630']);
});
test('runtime is independent of seated reference',async()=>{
  const {r,m}=await loadRendererWithOnlyCanonicalSource();
  assert.ok(pixels(r.render(getPose('seated',m))).some(Boolean));
  assertNoReferenceDependencies(['original-contour-source.cjs','original-contour-art.cjs',
    'original-contour-poses.cjs','original-contour-render.cjs']);
});
```

`loadRendererWithOnlyCanonicalSource` returns `{r,m}` via Tasks1–3 without any reference image. `assertNoReferenceDependencies` checks CommonJS dependency loading and source strings for seated PNG path/import; exporter is deliberately excluded. A changed reference may change the comparison panel, not `standing.png`/`seated.png`.

- [x] **Step2: Run** `node --test tools/PreviewLocomotion/verify-original-contour-export.cjs`. Expected FAIL: exporter missing.
- [x] **Step3: Implement** `exportStatic` and guarded `require.main===module` CLI. Comparisons show original/new standing and aligned reference/new seated, both backgrounds and native/3x sizes. Head landmarks align reference by rigid translation/rotation only; do not distort body. `parts.png` shows complete independent artwork, `joint-overlay.png` is separate from clean character images. Evidence JSON records source/reference/tool hashes, measured contour distance, head invariants, pose data, technical results and pending visual acceptance; do not hardcode unrun PASS. Export PNG/JSON only, no text in the clean individual character images.
- [x] **Step4: Run** `node --test tools/PreviewLocomotion/verify-original-contour-source.cjs tools/PreviewLocomotion/verify-original-contour-art.cjs tools/PreviewLocomotion/verify-original-contour-render.cjs tools/PreviewLocomotion/verify-original-contour-export.cjs`, then `node tools/PreviewLocomotion/export-original-contour.cjs`. Expected all new tests PASS,7 evidence files, no video. Inspect actual outputs with `view_image`; record concrete remaining differences, not just test counts. Verify original SHA256 and product files unchanged relative to the pre-execution dirty-worktree snapshot.
- [x] **Step5: Update** README with exact command/static scope, TASKS with actual checks/artifact links and **visual acceptance pending**. Present clean standing/seated and white/dark comparison to user, then stop for visual review. No motion connection until separately accepted/authorized; no commit/push/install.

## Plan self-review and execution boundary

Coverage: Task1 source/head/style; Task2 complete hidden contours/proportions; Task3 real joint influence/depth/opaque sockets/two endpoints; Task4 comparison/evidence/visual gate. All five Review Focus conditions map to tests. Shared type/property names are consistent; test helpers have independent oracles rather than production self-checks. No whole animation algorithm or new product integration is hidden in this plan.

User approved Native execution with follow-up “ㄱㄱ”. The four tasks are implemented locally; fresh whole-change review and one TDD fix pass completed. New suite18/18 passes. Static visual acceptance is still pending; animation, installation and Git operations are not authorized by this milestone. Review Minor about arbitrary tip-distance edits is deferred before future animation. Execution facts and prior failed prototype checks remain in TASKS.

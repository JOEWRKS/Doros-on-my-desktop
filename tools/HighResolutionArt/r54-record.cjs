'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/runtime-face-20261018-r54');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,f))).digest('hex');
const sources=['src/Dororong.App/Assets/high-resolution-grab.bank','artifacts/grab-contour-still-20261017-r33/masked-head-225.png','artifacts/grab-contour-still-20261017-r33/body-padded-375.png'];
const changed=['src/Dororong.App/Controls/HighResolutionFacePose.cs','src/Dororong.App/Controls/HighResolutionGrabRaster.cs','src/Dororong.App/Controls/HighResolutionHuntNeck.cs','src/Dororong.App/Controls/HighResolutionLocomotion.cs','src/Dororong.App/Controls/DororongPresenter.HighResolutionGrab.cs','src/Dororong.App/Assets/high-resolution-hunt-body.bank','src/Dororong.App/Assets/high-resolution-locomotion.bank','tools/HighResolutionArt/hunt-forebody.cjs','tools/HighResolutionArt/build-runtime-hunt-body.cjs','tools/HighResolutionArt/build-runtime-locomotion.cjs'];
const sheets=JSON.parse(fs.readFileSync(path.join(out,'sheet-index.json')));
// The following notes are the agent's direct observations recorded after
// opening all 34 sheets, not a visual verdict inferred by this script.
const report={status:'PARTIAL_NOT_ACCEPTED',originals:Object.fromEntries(sources.map(f=>[f,sha(f)])),candidate:Object.fromEntries(changed.map(f=>[f,sha(f)])),
 reviewedSheets:sheets.map(r=>({...r,sha256:sha('artifacts/runtime-face-20261018-r54/'+r.filename)})),
 directObservations:{hunt:'356 stored slots: 1x/1.5x frames 0..156 and 14 keys at 1.125/1.25/1.375. The rejected isolated forepaw stub was not seen in these neutral-gaze samples. Intermediate/subframe and all-angle coverage is not claimed.',locomotion:'279 stored slots: actual Presenter calls at 1/1.25/1.5, 93 each, covering walk, stop, sit, rise and clock continuation. Same head and source-part body remain in use. Closed expression is visible during sit slots 46..50; the later slots labelled blink do not contain a second blink.',cheek:'v1/v2/v3 max-pull and v3 same-size product comparison were viewed. Near-eye and contour differences remain. No visual pass.'},
 checks:{focused:{passed:24,failed:0,file:'tests/r54-regression.trx'},loopAndProduct:{passed:200,failed:0,file:'tests/r54-loop-regression.trx'},finalExporter:{passed:1,failed:0,file:'tests/final-sequence.trx',meaning:'export only; not a visual assertion'},finalHuntPngIdentity:{compared:356,mismatches:0}},
 pending:['Cheek feature/outline parity','Sleep/wake, snacks/happy, perch and other interaction legacy fallbacks','Walking/seated capture and full grab/release transition parity','Curious/startled/body-breathing behavior parity in the high-resolution path','Full angle and subframe review','Native mouse interaction and real-time performance'],
 boundaries:{originalArtEdited:false,installerChanged:false,testAppLaunched:false,macChanged:false,commitOrPush:false,nativeInteractionVerified:false}};
report.cheekFollowup={selectedDiagnostic:'v5',extraViewed:['v4/cheek-sheet-0.png','v5/cheek-sheet-0.png','v6/cheek-sheet-0.png'],v6:'Outline pass not adopted; removed from runtime, source retained in v6/HighResolutionCheekOutline.cs',visualPass:false};
report.checks.finalRegression={passed:224,failed:0,skipped:0,file:'tests/r54-final-regression.trx'};
report.checks.finalExporter.file='tests/final2-sequence.trx';
report.pending.push('High-resolution body clicks currently return no input target');
fs.writeFileSync(path.join(out,'review-record.json'),JSON.stringify(report,null,2)+'\n');
console.log(JSON.stringify({status:report.status,sheets:sheets.length,storedSlots:sheets.reduce((n,r)=>n+r.frames.length,0)}));

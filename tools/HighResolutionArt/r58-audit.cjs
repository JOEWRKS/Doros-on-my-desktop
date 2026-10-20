'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/motion-audit-20261018-r58');fs.mkdirSync(out,{recursive:true});
function stats(a){a=a.filter(Number.isFinite).sort((a,b)=>a-b);return a.length?{n:a.length,mean:a.reduce((a,b)=>a+b,0)/a.length,p50:a[Math.floor((a.length-1)*.5)],p95:a[Math.floor((a.length-1)*.95)],p99:a[Math.floor((a.length-1)*.99)],max:a.at(-1),over16_67:a.filter(x=>x>16.67).length,over33_33:a.filter(x=>x>33.33).length}:null;}
const file=process.argv[2]||'artifacts/contour-feedback-20261018-r57/native-20261018.json';
const log=JSON.parse(fs.readFileSync(path.resolve(root,file),'utf8')),groups={};
for(const row of log.states||[]){const s=row.state;const k=JSON.stringify({pet:s.pet,core:s.core,state:s.state,direct:s.direct,perch:s.perch});groups[k]=(groups[k]||0)+1;}
const report={source:file,elapsedMs:log.elapsedMs,cpuMs:log.cpuMs,cpuCorePercent:log.cpuMs/log.elapsedMs*100,nativeInputs:log.inputs,interval:stats(log.frames.slice(10).map(f=>f.intervalMs)),raster:stats(log.frames.slice(10).filter(f=>f.rasters>0).map(f=>f.rasterMs)),stateCounts:groups,exampleState:log.states?.[0],note:'Actual WPF callback log, not display FPS. Zero inputs does not verify input gestures.'};
fs.writeFileSync(path.join(out,path.basename(file,'.json')+'-summary.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
if(!fs.existsSync(path.join(out,'shape-lock.json'))){
 const names=['artifacts/grab-contour-still-20261017-r33/masked-head-225.png','artifacts/grab-contour-still-20261017-r33/body-padded-375.png','src/Dororong.App/Assets/high-resolution-grab.bank','src/Dororong.App/Assets/high-resolution-hunt-body.bank','src/Dororong.App/Assets/high-resolution-locomotion.bank','src/Dororong.App/Controls/HighResolutionFacePose.cs','src/Dororong.App/Controls/HighResolutionBodyInk.cs','src/Dororong.App/Controls/HighResolutionGrabRaster.Flutter.cs','src/Dororong.App/Controls/HighResolutionGrabRaster.cs','artifacts/contour-feedback-20261018-r57/release-app/Dororong.App.dll'];
 fs.writeFileSync(path.join(out,'shape-lock.json'),JSON.stringify(names.map(file=>({file,sha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(root,file))).digest('hex')})),null,2));
}

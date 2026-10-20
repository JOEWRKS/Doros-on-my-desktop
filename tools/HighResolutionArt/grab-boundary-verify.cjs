'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const {create,dir}=require('./grab-boundary-connected.cjs');
const {diff}=require('./grab-entry-transport.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const report=JSON.parse(fs.readFileSync(path.join(dir,'review.json'),'utf8'));
 for(const [f,h]of Object.entries(report.sources))assert.equal(sha(path.join(root,f)),h);
 for(const r of report.records)assert.equal(sha(path.join(dir,r.file)),r.sha256);
 const code=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8');
 const decode=new Function('assert',code.slice(code.indexOf('const table='),code.indexOf('async function main()'))+';return decodeParts;')(assert);
 const animations=[];
 for(const a of report.animations){
  assert.equal(sha(path.join(dir,a.file)),a.sha256);const frames=decode(fs.readFileSync(path.join(dir,a.file))),scale=Number(a.file.match(/(\d\.\d\d)/)[1]);assert.equal(frames.length,a.order.length);
  const saved=new Map();
  for(let k=0;k<frames.length;k++){
   const i=a.order[k];if(!saved.has(i))saved.set(i,await api.loadImage(path.join(dir,`frame-${scale.toFixed(2)}-${String(i).padStart(2,'0')}.png`)));
   const im=saved.get(i),c=api.createCanvas(480,300),g=c.getContext('2d');
   for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(j*240,0,240,300);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(`${scale}x / slow inspection / f${i}`,j*240+7,22);g.drawImage(im,j*240+(240-im.width)/2,35);}
   assert.deepEqual(pixels(await api.loadImage(frames[k]),api),pixels(c,api));
  }animations.push({file:a.file,decodedFrames:frames.length,pixelIdentity:true,crc:true});
 }
 const m=await create(),oldHead=m.common.s.rigs.open.parts.head,bd=m.body(0),hp=premultiply(oldHead,api),p=new Float64Array(4),q=new Float64Array(4);
 const oldHeadComposition=surface(api,144,(x,y)=>{sample(bd,375,(x+32)*225/96-.5,(y+32)*225/96-.5,p);sample(hp,225,x*225/96-.5,y*225/96-.5,q);return over(p,q);},32);
 const idle=api.createCanvas(240,240);idle.getContext('2d').drawImage(m.common.render('idle',0),24,24);
 const isolated=diff(idle,oldHeadComposition);assert.equal(isolated.changedPixels,0,'Body and compositor must match when old head is substituted for diagnostic only');
 const old=pixels(oldHead,api),locked=pixels(m.head,api);let headChanged=0;for(let i=0;i<old.length;i+=4)if(old.slice(i,i+4).some((v,c)=>v!==locked[i+c]))headChanged++;
 for(const t of [-.1,1.1,NaN,Infinity])assert.throws(()=>m.body(t));
 const result={status:'TECHNICAL_CHECKS_PASSED_NOT_VISUAL_ACCEPTANCE',animations,savedFrames:report.records.length,sourceHashesUnchanged:true,oldHeadDiagnostic:{changedSourcePixels:headChanged,originalIdleVersusSameBodyWithOldHead:isolated,originalIdleVersusLockedHead:diff(idle,m.render(0)),conclusion:'All current idle/start composite differences are isolated to the pre-existing r33 versus old head layer difference. The old layer was used only for diagnosis, not restored.'},scope:'Preview only; no real-time or app claim'};
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

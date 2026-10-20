'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {dir,pose}=require('./grab-release.cjs'),{presentation,lowest}=require('./grab-release-review.cjs'),{apng}=require('./grab-tilt-audit.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
// Do not draw the release floor under the preceding elevated review sequence.
function beforeRelease(im,scale){const c=api.createCanvas(480,300),g=c.getContext('2d');g.imageSmoothingEnabled=false;
 for(const [i,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(i*240,0,240,300);g.fillStyle=i?'white':'#222';g.font='13px sans-serif';g.fillText(`${scale}x / idle - grab - tilt`,i*240+7,20);g.drawImage(im,i*240+Math.floor((240-im.width)/2),28);}return c;}
async function main(){
 const report=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'))),priorDir=path.join(root,'artifacts/grab-idle-tilt-20261017-r44'),prior=JSON.parse(fs.readFileSync(path.join(priorDir,'manifest.json')));
 for(const [f,h]of Object.entries(report.sources))assert.equal(sha(path.join(root,f)),h);
 for(const r of report.frames)assert.equal(sha(path.join(dir,r.file)),r.sha256);
 const code=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8'),decode=new Function('assert',code.slice(code.indexOf('const table='),code.indexOf('async function main()'))+';return decodeParts;')(assert),verified=[];
 for(const scale of [1,1.25,1.5]){
  const records=report.frames.filter(r=>r.scale===scale),imgs=await Promise.all(records.map(r=>api.loadImage(path.join(dir,r.file)))),floor=lowest(imgs[88]);
  const packed=imgs.map((im,i)=>presentation(im,scale,`${scale}x / release f${i} / ${pose(i*.005).phase}`,floor).toBuffer('image/png'));
  const slow=report.animations.find(a=>a.scale===scale&&a.kind==='slow');assert.equal(sha(path.join(dir,slow.file)),slow.sha256);
  const unpacked=decode(fs.readFileSync(path.join(dir,slow.file)));assert.equal(unpacked.length,89);
  for(let i=0;i<89;i++)assert.deepEqual(pixels(await api.loadImage(unpacked[i]),api),pixels(await api.loadImage(packed[i]),api));
  const chain=[],timed=prior.animations.find(a=>a.scale===scale).timed;
  for(let i=0;i<=180;i++){const r=timed[i];assert.equal(sha(path.join(priorDir,r.file)),r.sha256);chain.push(beforeRelease(await api.loadImage(path.join(priorDir,r.file)),scale).toBuffer('image/png'));}
  chain.push(...packed);for(let i=0;i<18;i++)chain.push(presentation(imgs[88],scale,`${scale}x / settled idle`,floor).toBuffer('image/png'));
  const file=`preview-${scale.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,file),apng(chain));const decoded=decode(fs.readFileSync(path.join(dir,file)));assert.equal(decoded.length,chain.length);
  for(let i=0;i<decoded.length;i++)assert.deepEqual(pixels(await api.loadImage(decoded[i]),api),pixels(await api.loadImage(chain[i]),api));
  verified.push({scale,slowFrames:89,connectedFrames:decoded.length,file,sha256:sha(path.join(dir,file)),decodedPixelIdentity:true});console.log('verified '+file);
 }
 for(const t of [-1,NaN,Infinity,.441])assert.throws(()=>pose(t));for(const d of [-1,NaN,Infinity,37])assert.throws(()=>pose(.1,d));
 for(const d of [0,18,36]){assert.equal(pose(.44,d).y,d);assert.equal(pose(0,d).progress,1);assert.equal(pose(.18,d).progress,0);for(let i=0;i<=88;i++)assert(pose(i*.005,d).y<=d);}
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify({status:'ENCODING_AND_SOURCE_VERIFIED_VISUAL_REVIEW_SEPARATE',manifestSha256:sha(path.join(dir,'manifest.json')),sourceHashesUnchanged:true,savedFrames:report.frames.length,verified,poseBoundsChecked:true,verifierSha256:sha(__filename),presentation:'No floor guide before release; all sprite pixels reused. Release shown 6.67x slower; hard reset only at repeat boundary.'},null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

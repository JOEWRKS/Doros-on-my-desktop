'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {create,dir}=require('./grab-idle-tilt.cjs'),{presentation}=require('./grab-idle-tilt-review.cjs'),{diff}=require('./grab-entry-transport.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const report=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'),'utf8'));
 for(const [f,h]of Object.entries(report.sources))assert.equal(sha(path.join(root,f)),h);
 for(const r of report.records)assert.equal(sha(path.join(dir,r.file)),r.sha256);
 const code=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8'),decode=new Function('assert',code.slice(code.indexOf('const table='),code.indexOf('async function main()'))+';return decodeParts;')(assert);
 const decoded=[],timedSheets=[];
 for(const a of report.animations){
  assert.equal(sha(path.join(dir,a.file)),a.sha256);const frames=decode(fs.readFileSync(path.join(dir,a.file)));assert.equal(frames.length,a.frames);const images=[];
  for(let i=0;i<frames.length;i++){
   const r=a.timed[i];assert.equal(sha(path.join(dir,r.file)),r.sha256);const im=await api.loadImage(path.join(dir,r.file));images.push(im);
   const expected=presentation(im,a.scale,`idle - entry - tilt / ${r.seconds.toFixed(2)}s`);
   assert.deepEqual(pixels(await api.loadImage(frames[i]),api),pixels(expected,api));
  }
  decoded.push({scale:a.scale,frames:frames.length,crcAndPixelIdentity:true});
  // Inspect the actual timed playback at 1.5x, including nonlinear entry samples.
  if(a.scale===1.5)for(let start=0;start<images.length;start+=18){
   const ids=Array.from({length:Math.min(18,images.length-start)},(_,i)=>start+i),c=api.createCanvas(1200,Math.ceil(ids.length/6)*390),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(const [n,i]of ids.entries()){
    const x=n%6*200,y=Math.floor(n/6)*390;g.fillStyle='#dedbdf';g.fillRect(x,y,200,390);g.fillStyle='#222';g.font='13px sans-serif';g.fillText(`1.5x t${i} ${(i/30).toFixed(2)}s`,x+4,y+16);
    for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+j*184,198,182);g.drawImage(images[i],36,36,180,180,x+9,y+23+j*184,180,180);}
   }const file=`timed-sheet-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));timedSheets.push({file,frames:ids});
  }
 }
 const m=await create(),near=[];for(const seconds of [.4,2.8,3.2,4,4.6,5.2,6])for(const scale of [1,1.25,1.5])near.push({seconds,scale,before:diff(m.at(seconds-1e-6,scale),m.at(seconds,scale)),after:diff(m.at(seconds,scale),m.at(seconds+1e-6,scale))});
 for(const t of [NaN,-1,6.401,Infinity])assert.throws(()=>m.at(t));
 // Three-size local crops around mask/entry/tilt and the occlusion sign crossing.
 const probes=[.4,.433333333,2.766666667,2.8,3.2,3.233333333,4.566666667,4.6,4.633333333],cropSheets=[];
 for(const scale of [1,1.25,1.5]){
  const c=api.createCanvas(1200,540),g=c.getContext('2d');g.imageSmoothingEnabled=false;
  for(const [i,t]of probes.entries()){
   const im=m.at(t,scale),x=i%3*400,y=Math.floor(i/3)*180,sx=Math.floor(40*scale),sy=Math.floor(82*scale),w=Math.ceil(62*scale),h=Math.ceil(38*scale);
   g.fillStyle='#dedbdf';g.fillRect(x,y,400,180);g.fillStyle='#222';g.font='14px sans-serif';g.fillText(`${scale}x / ${t.toFixed(3)}s / attachment 2x`,x+5,y+18);
   for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x+j*200,y+24,198,154);g.drawImage(im,sx,sy,w,h,x+j*200+4,y+26,w*2,h*2);}
  }const file=`attachment-${scale.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));cropSheets.push(file);
 }
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify({status:'TECHNICAL_ONLY_DIRECT_REVIEW_SEPARATE',manifestSha256:sha(path.join(dir,'manifest.json')),sourceHashesUnchanged:true,progressFiles:report.records.length,decoded,timedSheets,cropSheets,near,verifierSha256:sha(__filename)},null,2));console.log(JSON.stringify({progressFiles:report.records.length,decoded,timedSheets:timedSheets.length,cropSheets}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

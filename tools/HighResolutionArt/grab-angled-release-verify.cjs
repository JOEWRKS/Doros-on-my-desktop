'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs'),{diff}=require('./grab-entry-transport.cjs');
const {dir,create,pose}=require('./grab-angled-release.cjs'),{presentation,lowest}=require('./grab-release-review.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const report=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json')));for(const [f,h]of Object.entries(report.sources))assert.equal(sha(path.join(root,f)),h);
 const code=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8'),decode=new Function('assert',code.slice(code.indexOf('const table='),code.indexOf('async function main()'))+';return decodeParts;')(assert),decoded=[],unique=new Map();
 for(const a of report.animations){
  const rec=report.frames.filter(r=>r.id===a.id),imgs=[];for(const r of rec){assert.equal(sha(path.join(dir,r.file)),r.sha256);const im=await api.loadImage(path.join(dir,r.file));imgs.push(im);if(!unique.has(r.sha256))unique.set(r.sha256,{...r,aliases:[]});unique.get(r.sha256).aliases.push(r.file);}
  assert.equal(sha(path.join(dir,a.file)),a.sha256);const parts=decode(fs.readFileSync(path.join(dir,a.file)));assert.equal(parts.length,a.frames);const floor=lowest(imgs.at(-1));
  for(let i=0;i<parts.length;i++){const j=Math.max(0,Math.min(imgs.length-1,i-12)),r=rec[j],expected=presentation(imgs[j],r.scale,`${r.hard?'HARD':'NORMAL'} ${r.angle}deg / f${j}`,floor);assert.deepEqual(pixels(await api.loadImage(parts[i]),api),pixels(expected,api));}
  decoded.push({id:a.id,frames:parts.length,pixelIdentity:true});
 }
 const m=await create(),probes=[];
 for(const scale of [1,1.25,1.5])for(const angle of [-8,8])for(const t of [0,.02,.04,.06,.079,.08,.081]){
  const file=`probe-${scale.toFixed(2)}-${angle<0?'m':'p'}8-${Math.round(t*1000)}.png`,im=m.render(t,angle,false,scale);fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));const saved=await api.loadImage(path.join(dir,file));assert.deepEqual(pixels(saved,api),pixels(im,api));
  if(t===0){const prior=await api.loadImage(path.join(root,`artifacts/grab-left-occlusion-20261017-r35/head-${scale.toFixed(2)}-${angle<0?'m':'p'}8.png`));assert.equal(diff(saved,prior).changedPixels,0);}
  probes.push({file,scale,angle,seconds:t,sha256:sha(path.join(dir,file)),aliases:[file]});
 }
 const sheets=[];for(const scale of [1,1.25,1.5]){const records=[...unique.values()].filter(r=>r.scale===scale).concat(probes.filter(r=>r.scale===scale));
  for(let from=0;from<records.length;from+=18){const slice=records.slice(from,from+18),c=api.createCanvas(1200,Math.ceil(slice.length/6)*510),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let j=0;j<slice.length;j++){const r=slice[j],im=await api.loadImage(path.join(dir,r.file)),x=j%6*200,y=Math.floor(j/6)*510,sx=Math.floor(24*scale),w=Math.ceil(112*scale);g.fillStyle='#ddd';g.fillRect(x,y,200,510);g.fillStyle='#222';g.font='12px sans-serif';g.fillText(`${r.angle}deg ${Math.round(r.seconds*1000)}ms ${r.hard?'H':'N'}`,x+3,y+16);
    for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+k*244,198,242);g.drawImage(im,sx,0,w,im.height,x+Math.floor((198-w)/2),y+22+k*244,w,im.height);}}
   const file=`unique-${scale.toFixed(2)}-${from}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,scale,records:slice});
  }
 }
 for(const t of [-1,NaN,Infinity])assert.throws(()=>pose(t,0));for(const angle of [-17,17,NaN])assert.throws(()=>pose(0,angle));
 for(const [f,h]of Object.entries(report.sources))assert.equal(sha(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify({status:'TECHNICAL_ONLY_DIRECT_REVIEW_SEPARATE',sourceHashesUnchanged:true,manifestSha256:sha(path.join(dir,'manifest.json')),decoded,totalSavedFrames:report.frames.length,uniqueSprites:unique.size,probes,sheets,verifierSha256:sha(__filename)},null,2));console.log(JSON.stringify({decoded:decoded.length,frames:report.frames.length,unique:unique.size,probes:probes.length,sheets:sheets.map(s=>s.file)}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

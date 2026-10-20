'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs'),{dir,pose}=require('./restored-landing.cjs'),{presentation,lowest}=require('./grab-release-review.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const report=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json')));
 for(const [f,h]of Object.entries(report.sources))assert.equal(sha(path.join(root,f)),h);
 assert.equal(sha(path.join(dir,'runtime-curves.json')),report.runtimeCurveFileSha256);
 const code=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8'),decode=new Function('assert',code.slice(code.indexOf('const table='),code.indexOf('async function main()'))+';return decodeParts;')(assert),decoded=[],contact=[];
 for(const a of report.animations){
  const rec=report.frames.filter(r=>r.id===a.id),imgs=[];
  for(const r of rec){assert.equal(sha(path.join(dir,r.file)),r.sha256);imgs.push(await api.loadImage(path.join(dir,r.file)));}
  assert.equal(sha(path.join(dir,a.file)),a.sha256);const parts=decode(fs.readFileSync(path.join(dir,a.file)));assert.equal(parts.length,a.frames);
  for(let i=0;i<parts.length;i++){const j=Math.max(0,Math.min(imgs.length-1,i-12)),r=rec[j],label=`${r.id.split('-')[0]} ${r.scale}x / ${Math.round(r.seconds*1000)}ms`,expected=presentation(imgs[j],r.scale,label,(32+87+(r.options.distance??36))*r.scale);assert.deepEqual(pixels(await api.loadImage(parts[i]),api),pixels(expected,api));}
  // Record actual occupied sole rather than treating logical pivot87 as proof of contact.
  const floor=lowest(imgs.at(-1));for(let j=0;j<rec.length;j++)if(rec[j].seconds>=.22&&rec[j].pose.lift===0)contact.push({id:a.id,index:j,delta:lowest(imgs[j])-floor});
  decoded.push({id:a.id,frames:parts.length,pixelIdentity:true});
 }
 for(const t of [-1,NaN,Infinity])assert.throws(()=>pose(t));
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify({status:'TECHNICAL_ONLY_VISUAL_REVIEW_SEPARATE',sourceHashesUnchanged:true,manifestSha256:sha(path.join(dir,'manifest.json')),decoded,contact,contactDeltaRange:[Math.min(...contact.map(r=>r.delta)),Math.max(...contact.map(r=>r.delta))],frames:report.frames.length,unique:report.unique,verifierSha256:sha(__filename)},null,2));
 console.log(JSON.stringify({decoded:decoded.length,frames:report.frames.length,contactDeltaRange:[Math.min(...contact.map(r=>r.delta)),Math.max(...contact.map(r=>r.delta))]}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

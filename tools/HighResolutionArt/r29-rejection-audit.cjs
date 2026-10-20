'use strict';
// Diagnostic only: never rewrites source art or r29 delivery artifacts.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create}=require('./direct-motion.cjs');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 const m=await create(),{api,root}=m.common.s;
 const dir=path.join(root,'artifacts/direct-motion-20261017-r29-audit');fs.mkdirSync(dir,{recursive:true});
 const report={status:'REJECTED_NOT_FIXED',method:'Pink-pixel bounds width normalized to144; pink top/center aligned. Diagnostic registration, not runtime screenshot or exact head landmark measurement.',keys:[],sequences:[]};
 function analyze(im,removeBackground=false){
  const c=api.createCanvas(im.width,im.height),g=c.getContext('2d');g.drawImage(im,0,0);const p=g.getImageData(0,0,c.width,c.height),d=p.data,w=c.width,h=c.height;
  if(removeBackground){const seen=new Uint8Array(w*h),queue=[];const visit=(x,y)=>{if(x<0||y<0||x>=w||y>=h)return;const k=y*w+x,i=k*4;if(seen[k]||Math.min(d[i],d[i+1],d[i+2])<240)return;seen[k]=1;queue.push(k);};for(let x=0;x<w;x++){visit(x,0);visit(x,h-1);}for(let y=0;y<h;y++){visit(0,y);visit(w-1,y);}for(let n=0;n<queue.length;n++){const k=queue[n],x=k%w,y=Math.floor(k/w);visit(x-1,y);visit(x+1,y);visit(x,y-1);visit(x,y+1);}for(const k of queue)d[k*4+3]=0;}
  let l=w,r=0,t=h,b=0;for(let y=0;y<h;y++)for(let x=0;x<w;x++){const i=(y*w+x)*4;if(d[i+3]>128&&d[i]>110&&d[i]-d[i+1]>22&&d[i]-d[i+2]>5){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}}
  const scale=144/(r-l+1);let bl=w,br=0,bb=0;for(let y=b+1;y<h;y++)for(let x=0;x<w;x++)if(d[(y*w+x)*4+3]>128){bl=Math.min(bl,x);br=Math.max(br,x);bb=Math.max(bb,y);}
  // Drawing uses the unmodified source. Flood removal is only a coarse metric.
  return {im,pink:[l,t,r,b],scale,belowPinkWidth:(br-bl+1)*scale,belowPinkDepth:(bb-b)*scale};
 }
 const pairs=[];for(let n=0;n<8;n++){
  const key=String(n+1).padStart(2,'0'),file=path.join(root,'src/Dororong.App/Assets/user-body-drag',key+'.png'),ref=await api.loadImage(file),candidate=m.render('grab',n/7);
  pairs.push([analyze(ref,true),analyze(candidate)]);report.keys.push({key,progress:n/7,sourceSha256:sha(fs.readFileSync(file)),candidateSha256:sha(candidate.toBuffer('image/png')),reference:pairs[n][0],candidate:pairs[n][1]});
  delete report.keys[n].reference.im;delete report.keys[n].candidate.im;
  // Retain images separately after removing unserializable image references.
  pairs[n][0].im=ref;pairs[n][1].im=candidate;
 }
 for(let page=0;page<4;page++){
  const c=api.createCanvas(960,720),g=c.getContext('2d');g.fillStyle='white';g.fillRect(0,0,960,720);
  for(let row=0;row<2;row++){const n=page*2+row;for(let side=0;side<2;side++){
   const q=pairs[n][side],x=side*480,y=row*360;g.fillStyle='#222';g.font='16px sans-serif';g.fillText(side?'Candidate p='+Number(n/7).toFixed(3):'Authored key '+String(n+1).padStart(2,'0'),x+22,y+26);
   g.save();g.beginPath();g.rect(x,y+35,480,310);g.clip();g.imageSmoothingEnabled=false;g.drawImage(q.im,x+240-(q.pink[0]+q.pink[2]+1)/2*q.scale,y+55-q.pink[1]*q.scale,q.im.width*q.scale,q.im.height*q.scale);g.restore();
  }}fs.writeFileSync(path.join(dir,'keys-'+(page+1)+'.png'),c.toBuffer('image/png'));
 }
 for(const p of pairs)for(const q of p)delete q.im;
 const old=path.join(root,'artifacts/direct-motion-20261017-r29'),manifest=JSON.parse(fs.readFileSync(path.join(old,'manifest.json')));
 report.originalManifestSha256=sha(fs.readFileSync(path.join(old,'manifest.json')));
 report.sourceMismatches=Object.entries(manifest.sources).filter(([f,h])=>sha(fs.readFileSync(path.join(__dirname,f)))!==h).map(([f])=>f);
 report.frameMismatches=manifest.frames.filter(f=>sha(fs.readFileSync(path.join(old,f.name+'.png')))!==f.sha256).map(f=>f.name);
 report.angles=Array.from({length:37},(_,n)=>{const requested=n/36*176-88;return {frame:n,requested,actual:m.grab.safeAngle(1,requested*Math.PI/180)*180/Math.PI};});
 for(const sc of manifest.sequences){const frames=manifest.frames.slice(sc.start,sc.start+sc.count),runs=[];let prev=null;for(const f of frames){if(f.sha256!==prev){runs.push({first:f.name,last:f.name,count:1});prev=f.sha256;}else{runs.at(-1).last=f.name;runs.at(-1).count++;}}report.sequences.push({...sc,uniqueHashes:new Set(frames.map(f=>f.sha256)).size,duplicateRuns:runs.filter(r=>r.count>1)});}
 fs.writeFileSync(path.join(dir,'audit.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({dir,keys:report.keys.map(k=>({key:k.key,referenceWidth:k.reference.belowPinkWidth,candidateWidth:k.candidate.belowPinkWidth,referenceDepth:k.reference.belowPinkDepth,candidateDepth:k.candidate.belowPinkDepth})),sequences:report.sequences},null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});

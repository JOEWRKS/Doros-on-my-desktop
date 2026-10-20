'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root,basePath,donorPath}=require('./bangs-composite-review.cjs'),{finish}=require('./bangs-face-finish.cjs'),{pixels}=require('./sampling.cjs');
const out=path.join(root,'artifacts/high-resolution-bangs-face-20261016-r9');
async function main(){
 fs.mkdirSync(out,{recursive:true});
 const base=await api.loadImage(path.join(root,basePath)),donor=await api.loadImage(path.join(root,donorPath)),old=await api.loadImage(path.join(root,'artifacts/high-resolution-bangs-composite-20261016-r8/closed-composite-225.png'));
 const r=finish(base,donor,api),save=(n,c)=>fs.writeFileSync(path.join(out,n+'.png'),c.toBuffer('image/png'));
 save('closed-composite-225',r.image);save('hair-mask-225',r.mask);save('eye-mask-225',r.eyeMask);
 const c=api.createCanvas(864,560),g=c.getContext('2d');g.fillStyle='#edeae3';g.fillRect(0,0,864,560);
 for(const [i,[im,label]] of [[base,'Original eye shape'],[old,'Rejected r8'],[r.image,'Fringe ends / eye edge correction']].entries()){
  g.fillStyle='#302b33';g.font='12px sans-serif';g.fillText(label,i*288+10,24);g.imageSmoothingEnabled=true;g.imageSmoothingQuality='high';g.drawImage(im,i*288+72,40,144,144);
  g.imageSmoothingEnabled=false;g.drawImage(im,25,50,96,120,i*288,185,288,360);
 }save('comparison',c);
 const pair=api.createCanvas(576,400),p=pair.getContext('2d');
 for(const [i,im] of [old,r.image].entries()){
  p.fillStyle='#edeae3';p.fillRect(i*288,0,288,200);p.fillStyle='#222a30';p.fillRect(i*288,200,288,200);
  p.fillStyle='#302b33';p.font='14px sans-serif';p.fillText(i?'r9 candidate':'r8 rejected',i*288+16,24);
  p.imageSmoothingEnabled=true;p.imageSmoothingQuality='high';p.drawImage(im,i*288+72,32,144,144);p.drawImage(im,i*288+72,220,144,144);
 }save('before-after',pair);
 const a=pixels(base,api),b=pixels(r.image,api),m=pixels(r.mask,api),e=pixels(r.eyeMask,api);let changed=0,outside=0,alpha=0;
 for(let i=0;i<a.length;i+=4){if(a[i+3]!==b[i+3])alpha++;if([0,1,2].some(k=>a[i+k]!==b[i+k])){changed++;if(!m[i+3]&&!e[i+3])outside++;}}
 const report={changedPixels:changed,outsideHairAndEyes:outside,alphaChanges:alpha,eyeEdgeChangedPixels:r.sharpened,runtimeSelected:false};
 fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={out};

'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const out=process.argv[2]?path.resolve(process.argv[2]):path.join(root,'artifacts/gaze-cheek-20261018-r64/final');
async function data(file){const im=await api.loadImage(fs.readFileSync(file)),c=api.createCanvas(im.width,im.height),g=c.getContext('2d');g.drawImage(im,0,0);return {width:im.width,p:g.getImageData(0,0,im.width,im.height).data};}
async function main(){
 const locked=JSON.parse(fs.readFileSync(path.join(root,'artifacts/motion-audit-20261018-r58/shape-lock.json'))).slice(0,5);
 const sources=locked.map(r=>({...r,unchanged:crypto.createHash('sha256').update(fs.readFileSync(path.join(root,r.file))).digest('hex')===r.sha256}));
 const comparisons=[];
 for(const scale of [1,1.1,1.25,1.35,1.5])for(const name of ['neutral','protected-cheek--2','protected-cheek-0','protected-cheek-10','protected-cheek-20']){
  const file=`gaze-${scale.toFixed(2)}-${name}.png`,a=await data(path.join(root,'artifacts/gaze-isolation-20261018-r60/before',file)),b=await data(path.join(out,file));
  let changes=0,outsideRepair=0,l=b.width,t=b.width,r=0,bt=0;
  for(let i=0;i<b.p.length;i+=4)if([0,1,2,3].some(k=>a.p[i+k]!==b.p[i+k])){
   const px=i/4%b.width,py=Math.floor(i/4/b.width),x=(px+.5)/scale-80,y=(py+.5)/scale-80;
   changes++;l=Math.min(l,x);t=Math.min(t,y);r=Math.max(r,x);bt=Math.max(bt,y);
   // Allow repaired eye/bang ownership and exposed cheek-edge area only.
   if(!((x>=15&&x<=32&&y>=42&&y<=60)||(x>=0&&x<=20&&y>=52&&y<=65)))outsideRepair++;
  }
  comparisons.push({file,changes,outsideRepair,bounds:changes?[l,t,r,bt]:null});
 }
 const report={sources,comparisons,allSourcesUnchanged:sources.every(x=>x.unchanged),outsideRepair:comparisons.reduce((n,r)=>n+r.outsideRepair,0),nativeInteraction:'UNVERIFIED',userAcceptance:'NOT_ACCEPTED'};
 const sheet=api.createCanvas(640,320),g=sheet.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,640,320);g.imageSmoothingEnabled=false;g.font='17px sans-serif';
 for(const [i,[folder,label]] of [[path.join(root,'artifacts/gaze-cheek-20261018-r63/final'),'Before: cheek pull'],[out,'Candidate: eye boundary fixed']].entries()){
  const im=await api.loadImage(fs.readFileSync(path.join(folder,'cheek-20--0.85.png')));g.fillStyle='#222';g.fillText(label,i*320+10,24);
  g.drawImage(im,144,183,26,30,i*320+10,40,208,240);
  g.drawImage(im,120,155,144,135,i*320+205,162,115,108);
 }
 fs.writeFileSync(path.join(out,'cheek-boundary-before-after.png'),sheet.toBuffer('image/png'));
 fs.writeFileSync(path.join(out,'preservation.json'),JSON.stringify(report,null,2));console.log(report);
}
main().catch(e=>{console.error(e);process.exitCode=1});

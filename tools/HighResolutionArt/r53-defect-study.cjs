'use strict';
// Diagnostic decomposition only. No source art, generated banks or runtime files change.
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
const {premultiply,sample,over,surface}=require('./sampling.cjs');
const hunt=require('../PreviewLocomotion/hunt-motion.js');
const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
async function main(){
 const m=await require('./grab-boundary-connected.cjs').create(),rig=m.common.s.postures.open.rig;
 const hp=premultiply(m.head,api),dir=path.join(root,'artifacts/runtime-face-20261018-r53/defect-study');fs.mkdirSync(dir,{recursive:true});
 const canvas=api.createCanvas(1080,760),g=canvas.getContext('2d');g.fillStyle='#eee';g.fillRect(0,0,1080,760);g.font='16px sans-serif';
 const corrected=process.argv.includes('--forebody');
 const pen=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-contour-still-20261017-r33/measurement.json'))).pen;
 for(const [i,age]of [0,.7,1.25,1.45].entries()){
  const p=hunt.sample(age),shift=[p.sway,-1.6*p.amount+.22*p.sway],rp=hunt.map(64,73,p),tip=hunt.map(65,81,p),angle=Math.atan2(tip[1]-rp[1],tip[0]-rp[0])-Math.atan2(8,1),co=Math.cos(angle),si=Math.sin(angle);
  const map=([x,y])=>[tip[0]+(x-65)*co-(y-81)*si,tip[1]+(x-65)*si+(y-81)*co];
  let body=rig.render(rig.pose(0),{includeHead:false,mapOverride:()=> ([x,y])=>hunt.map(x,y,p),extraParts:{haunch:rootedHaunch(shift[1],shift[0]),nearHind:connectedHind(map,shift)}});
  if(corrected)body=require('./hunt-forebody.cjs').apply(body,p,api,pen);
  const bd=premultiply(body,api);
  const a=new Float64Array(4),b=new Float64Array(4),c=Math.cos(p.headRoll),s=Math.sin(p.headRoll);
  const composite=surface(api,144,(x,y)=>{sample(bd,225,x*225/96-.5,y*225/96-.5,a);const dx=x-43,dy=y-48-p.headBob;sample(hp,225,(43+dx*c+dy*s)*225/96-.5,(48-dx*s+dy*c)*225/96-.5,b);return over(a,b);},16);
  fs.writeFileSync(path.join(dir,`hunt-${corrected?'forebody-':''}${age}.png`),composite.toBuffer('image/png'));
  const x=i*270;g.fillStyle='#222';g.fillText(`hunt ${age}s — original head / body`,x+5,20);g.imageSmoothingEnabled=false;
  g.drawImage(composite,x+20,34);g.drawImage(body,x+20,225);
  // Integer inspection of the front-paw area, not a new product output.
  g.drawImage(composite,20,90,90,55,x,490,270,165);
 }
 const file=corrected?'hunt-forebody-layers.png':'hunt-layers.png';
 fs.writeFileSync(path.join(dir,file),canvas.toBuffer('image/png'));
 console.log(path.join(dir,file));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

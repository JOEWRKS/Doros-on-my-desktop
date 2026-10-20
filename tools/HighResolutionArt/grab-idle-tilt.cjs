'use strict';
// Current grab preview chain. Historical common-motion/r43 renderers stay reproducible.
const path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const {premultiply,sample,over,surface}=require('./sampling.cjs');
const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
const dir=path.join(root,'artifacts/grab-idle-tilt-20261017-r44');
const smooth=t=>{assert(Number.isFinite(t));t=Math.max(0,Math.min(1,t));return t*t*t*(10+t*(-15+6*t));};
async function create(){
 const entry=await require('./grab-boundary-connected.cjs').create(),tilt=await require('./grab-tilt-transition.cjs').create();
 // Independently render the existing common idle BODY, then select the same
 // accepted head as entry and held. Do not blend old/new complete sprites.
 const rig=entry.common.s.postures.open.rig;
 const idleBody=rig.render(rig.pose(0),{includeHead:false,extraParts:{haunch:rootedHaunch(0),nearHind:connectedHind(q=>q)}});
 const bp=premultiply(idleBody,api),hp=premultiply(entry.head,api);
 function idle(scale=1.5){const a=new Float64Array(4),b=new Float64Array(4);return surface(api,96*scale,(x,y)=>{
  sample(bp,225,x*225/96-.5,y*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);
 },32);}
 function render(frame,scale=1.5){
  assert(Number.isFinite(frame)&&frame>=0&&frame<=152);assert(Number.isFinite(scale)&&scale>=1&&scale<=1.5);
  if(frame===0)return idle(scale);
  if(frame<=56)return entry.render(frame/56,scale);
  return tilt.render(scale,frame-56);
 }
 // Review time only. Smooth temporal entry keeps all authored key geometries;
 // it changes traversal speed, never the body/foot/head shape at a given progress.
 function at(seconds,scale=1.5){
  assert(Number.isFinite(seconds)&&seconds>=0&&seconds<=6.4);
  if(seconds<=.4)return idle(scale);
  if(seconds<2.8)return entry.render(smooth((seconds-.4)/2.4),scale);
  if(seconds<=6)return tilt.render(scale,(seconds-2.8)*30);
  return tilt.render(scale,96);
 }
 return {idle,render,at,entry,tilt,idleBody,head:entry.head};
}
module.exports={create,dir,smooth};

'use strict';
const {split,remember}=require('./parts.cjs'),{create:reference}=require('../PreviewLocomotion/reference-parts.cjs');
const {artwork}=require('./body.cjs');
const {premultiply,sample,over,surface}=require('./sampling.cjs');
// A shared source-art assembly, not a bitmap blend between two different
// bodies. Approved seated part geometry, bones, pose curves and timings stay
// unchanged; its rest pose also owns idle, look, walk and gallop body contours.
function create(originalMaster,nativeMeasurement,api,{size=144,padding=16,smoothSit=false,connected=false,outlineRecovery=false,restoreHairTip=false,pointedHairTip=false}={}){
 const head=split(originalMaster,api,{restoreHairTip:restoreHairTip||pointedHairTip}).head;
 if(pointedHairTip)require('./pointed-tip.cjs').apply(head,api);
 const rig=reference(nativeMeasurement,api,{outputScale:225/96,headOverride:head,artwork,unionOutline:true});
 const master=rig.render(rig.pose(0)),headPixels=premultiply(head,api),restPixels=premultiply(master,api);
 function render(t,{bodyOnly=false}={}){
  if(t===0&&!connected){const v=new Float64Array(4);return surface(api,size,(x,y)=>sample(restPixels,225,x*225/96-.5,y*225/96-.5,v),padding);}
  const p=rig.pose(t),extraParts=smoothSit?{nearHind:require('./motion-polish.cjs').seatedHind(p)}:{};
  if(connected){
   const {connectedHind,rootedHaunch}=require('./motion-polish.cjs'),[a,b,c]=p.legs[2].joints;
   const angle=Math.atan2(c[1]-b[1],c[0]-b[0])-Math.PI/2,co=Math.cos(angle),si=Math.sin(angle);
   const map=([x,y])=>[b[0]+(x-65)*co-(y-71)*si,b[1]+(x-65)*si+(y-71)*co];
   extraParts.nearHind=connectedHind(map,p.haunch);extraParts.haunch=rootedHaunch(p.haunch[1],p.haunch[0]);
  }
  const bodyCanvas=rig.render(p,{includeHead:false,extraParts});
  if(bodyOnly)return {body:bodyCanvas,headY:p.head[1],headX:p.head[0]};
  const body=premultiply(bodyCanvas,api),b=new Float64Array(4),h=new Float64Array(4);
  return surface(api,size,(x,y)=>{
   sample(body,225,x*225/96-.5,y*225/96-.5,b);
   sample(headPixels,225,(x-p.head[0])*225/96-.5,(y-p.head[1])*225/96-.5,h);
   return over(b,h);
  },padding,outlineRecovery?require('./head-outline.cjs').create(headPixels,(x,y)=>[x-p.head[0],y-p.head[1]]):null);
 }
 remember(master,{head,rig,artwork});
 return {master,head,rig,render};
}
module.exports={create};

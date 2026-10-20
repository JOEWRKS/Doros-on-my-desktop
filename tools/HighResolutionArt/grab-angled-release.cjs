'use strict';
const path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs'),{sample,premultiply,over,surface}=require('./sampling.cjs');
const {smooth}=require('./grab-idle-tilt.cjs');
const dir=path.join(root,'artifacts/grab-angled-release-20261017-r46');
const duration=hard=>hard?2.1:.52;
function pose(t,angle,hard=false){
 assert(Number.isFinite(t)&&t>=0&&t<=duration(hard));assert(Number.isFinite(angle)&&Math.abs(angle)<=16);
 const tilt=angle*(1-smooth(t/.08)),progress=1-smooth((t-.08)/.18),impact=t-.3;
 let lift=0;
 if(hard){if(impact>1.1&&impact<1.6)lift=32*Math.sin(Math.PI*(impact-1.1)/.5)**2;
  else if(impact>1.6&&impact<1.8)lift=3*Math.sin(Math.PI*(impact-1.6)/.2)**2;
 }else if(impact>.04&&impact<.22)lift=1.8*Math.sin(Math.PI*(impact-.04)/.18)**2;
 return {tilt,progress,y:36*Math.min(1,t/.3)**2-lift,lift,phase:t<.08?'untilt':t<.26?'return':t<.3?'fall':hard?(impact<1.1?'impact-hold':impact<1.6?'big-rebound':t<2.1?'settle':'idle'):(impact<.04?'contact':t<.52?'rebound':'idle')};
}
async function create(){
 const previous=await require('./grab-release.cjs').create(),hp=premultiply(previous.previous.head,api);
 function render(t,angle,hard=false,scale=1.5,{positioned=true}={}){
  assert(Number.isFinite(scale)&&scale>=1&&scale<=1.5);const p=pose(t,angle,hard),body=previous.previous.entry.body(p.progress),a=new Float64Array(4),b=new Float64Array(4),r=-p.tilt*Math.PI/180,c=Math.cos(r),s=Math.sin(r);
  return surface(api,96*scale,(x,y)=>{if(positioned)y-=p.y;
   // Reuse r35's approved local occlusion only while the held body is unchanged.
   const sy=p.tilt>0&&x>=18&&x<29&&y>=57&&y<66.5?66.5:y;
   sample(body,375,(x+32)*225/96-.5,(sy+32)*225/96-.5,a);
   const hx=43+(x-43)*c-(y-66)*s,hy=66+(x-43)*s+(y-66)*c;
   sample(hp,225,hx*225/96-.5,hy*225/96-.5,b);return over(a,b);
  },32);
 }
 return {render,previous};
}
module.exports={dir,duration,pose,create};

'use strict';
const path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs'),{sample,premultiply,over,surface}=require('./sampling.cjs');
const {smooth}=require('./grab-idle-tilt.cjs');
const dir=path.join(root,'artifacts/grab-release-20261017-r45');
const duration=.44,returnDuration=.18,fallDuration=.22;
function pose(seconds,distance=36){
 assert(Number.isFinite(seconds)&&seconds>=0&&seconds<=duration);assert(Number.isFinite(distance)&&distance>=0&&distance<=36);
 const progress=1-smooth(seconds/returnDuration),fall=Math.min(1,seconds/fallDuration),impact=seconds-fallDuration;
 // No squash or stretch: preserve the authored head and body geometry. A small
 // rigid rebound substitutes for the old whole-character scale deformation.
 const rebound=impact>.04&&impact<.22?1.8*(.4+.6*distance/36)*Math.sin(Math.PI*(impact-.04)/.18)**2:0;
 return {progress,y:distance*fall*fall-rebound,rebound,phase:seconds<.18?'return':seconds<.22?'fall':impact<.04?'contact':seconds<duration?'rebound':'idle'};
}
async function create(){
 const previous=await require('./grab-idle-tilt.cjs').create(),hp=premultiply(previous.head,api);
 function render(seconds,scale=1.5,{distance=36,positioned=true}={}){
  assert(Number.isFinite(scale)&&scale>=1&&scale<=1.5);const p=pose(seconds,distance),body=previous.entry.body(p.progress),a=new Float64Array(4),b=new Float64Array(4);
  return surface(api,96*scale,(x,y)=>{if(positioned)y-=p.y;
   sample(body,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);
  },32);
 }
 return {previous,render,pose};
}
module.exports={create,pose,dir,duration,returnDuration,fallDuration};

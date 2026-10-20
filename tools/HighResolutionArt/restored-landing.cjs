'use strict';
const path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs'),{sample,premultiply,over,surface}=require('./sampling.cjs');
const legacy=require('./landing-motion.cjs'),{smooth}=require('./grab-motion.cjs');
const dir=path.join(root,'artifacts/restored-landing-20261017-r47');
function pose(t,{hard=false,strength=1,distance=36}={}){
 assert(Number.isFinite(t)&&t>=0&&t<=(hard?2.02:.44));assert(Number.isFinite(strength)&&strength>=0&&strength<=1);assert(Number.isFinite(distance)&&distance>=0&&distance<=36);
 const impact=t-.22,p=hard&&impact>=0?legacy.extreme(impact):{squash:0,amount:0,lift:0};
 return {progress:strength*(1-smooth(t/.18)),y:distance*Math.min(1,t/.22)**2-p.lift,squash:hard?p.squash:legacy.compression(t*1000,strength,distance),spread:p.amount,lift:p.lift};
}
async function create(){
 const prior=await require('./grab-release.cjs').create(),hp=premultiply(prior.previous.head,api);
 function render(t,options={},scale=1.5){
  assert(scale>=1&&scale<=1.5);const p=pose(t,options),body=prior.previous.entry.body(p.progress),a=new Float64Array(4),b=new Float64Array(4);
  // Restore the existing whole-character squash around soleY87. Input artwork
  // is immutable; the deliberate landing deformation is not a redrawn face.
  const sx=1+p.squash*(options.hard?.5:5/9),sy=1-p.squash;
  return surface(api,96*scale,(x,y)=>{
   y-=p.y;x=48+(x-48)/sx;y=87+(y-87)/sy;
   const bx=legacy.inverseX(x,y,p.spread);
   sample(body,375,(bx+32)*225/96-.5,(y+32)*225/96-.5,a);
   // Paws spread only in the body layer, never by pulling the hair-tip pixels.
   sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);
  },32);
 }
 return {render,prior};
}
module.exports={create,pose,dir};

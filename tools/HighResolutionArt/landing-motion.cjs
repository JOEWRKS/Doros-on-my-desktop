'use strict';
const {sample,over,surface}=require('./sampling.cjs');
const {smooth}=require('./grab-motion.cjs');
function inverseX(x,y,amount){
 const reach=24*amount*smooth((y-66)/17);if(!reach)return x;
 let lo=x-reach,hi=x+reach;for(let i=0;i<20;i++){const u=(lo+hi)/2;if(u+reach*(2*smooth((u-46)/14)-1)<x)lo=u;else hi=u;}return (lo+hi)/2;
}
function compression(age,strength=1,distance=36){
 const t=Math.max(0,Math.min(440,age))-220;
 const c=t<0?0:t<40?.18*smooth(t/40):t<120?.18-.22*smooth((t-40)/80):-.04*(1-smooth((t-120)/100));
 return c*(strength*.4+.6*distance/36);
}
function extreme(age){const t=Math.max(0,Math.min(1.8,age)),s=smooth;
 const impact=t<.035?1-(1-t/.035)**3:t<.065?1-.06*s((t-.035)/.03):t<.1?.94+.06*s((t-.065)/.035):1;
 const squash=t<.1?.574*impact:t<1.1?.574:t<1.28?.574-.734*s((t-1.1)/.18):t<1.6?-.16*(1-s((t-1.28)/.32)):t<1.68?.12*s((t-1.6)/.08):.12*(1-s((t-1.68)/.12));
 return {squash,amount:impact*(1-s((t-1.1)/.28)),lift:t>1.1&&t<1.6?32*Math.sin(Math.PI*(t-1.1)/.5)**2:0};
}
function create(model,grab){
 const body=grab.body(0),{s,heads}=model;
 function render(amount,{size=144,padding=32,eye='open',squash=0,lift=0}={}){
  amount=Math.max(0,Math.min(1,amount));const a=new Float64Array(4),b=new Float64Array(4);
  return surface(s.api,size,(x,y)=>{
   y+=lift;x=48+(x-48)/(1+squash*.5);y=87+(y-87)/(1-squash);
   const bx=inverseX(x,y,amount);sample(body,225,bx*225/96-.5,y*225/96-.5,a);
   // Apply the existing lower-body spread to semantic body only: the accepted
   // pointed hair at logical y~70 must not be pulled sideways with the paws.
   sample(heads[eye],225,x*225/96-.5,y*225/96-.5,b);return over(a,b);
  },padding);
 }
 return {render};
}
module.exports={create,compression,inverseX,extreme};

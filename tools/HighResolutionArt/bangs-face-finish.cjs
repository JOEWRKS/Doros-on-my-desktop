'use strict';
const {composite}=require('./bangs-composite.cjs');
const {pixels}=require('./sampling.cjs');
// Protect local lid strokes without reserving unrelated rectangular face areas.
const eyePolygons=[
 [[35,120],[44,120],[47,122],[61,122],[63,124],[62,129],[46,130],[36,126]],
 [[78,127],[85,127],[90,129],[99,128],[104,126],[106,130],[102,134],[89,136],[79,133]]
];
// Follow the pink space below the existing crown instead of cutting its ink at y64.
const polygon=[[44,84],[52,74],[62,68],[73,66],[85,69],[96,76],[108,89],[115,108],
 [116,124],[103,127],[81,127],[76,127],[65,127],[56,123],[44,120],[29,119],[34,108],[39,96]];
function trace(g,p){g.beginPath();p.forEach(([x,y],i)=>i?g.lineTo(x,y):g.moveTo(x,y));g.closePath();}
function mask(api,eyesOnly=false){
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.fillStyle='#fff';
 if(eyesOnly){for(const p of eyePolygons){trace(g,p);g.fill();}return c;}
 trace(g,polygon);g.fill();const d=g.getImageData(0,0,225,225),e=pixels(mask(api,true),api);
 for(let i=3;i<d.data.length;i+=4)d.data[i]=Math.round(d.data[i]*(1-e[i]/255));
 g.putImageData(d,0,0);g.clearRect(55,135,29,21);return c;
}
function finish(base,donor,api){
 const editMask=mask(api),r=composite(base,donor,api,{maskOverride:editMask}),g=r.image.getContext('2d');
 const image=g.getImageData(0,0,225,225),eyes=pixels(mask(api,true),api);
 let sharpened=0;
 for(let i=0;i<image.data.length;i+=4){
  const w=eyes[i+3]/255;if(!w)continue;
  const rgb=[...image.data.subarray(i,i+3)],hi=Math.max(...rgb),lo=Math.min(...rgb);
  // Only neutral lid ink/fringe. No skin/hair repaint and no new curve geometry.
  if(hi-lo>30||hi>239)continue;
  const a=Math.max(0,Math.min(1,(255-hi)/185));
  const edge=Math.max(0,Math.min(1,(a-.5)*2.6+.5));
  const before=[...image.data.subarray(i,i+3)];
  // Local tonal adjustment only; no spatial resampling or newly drawn eyelid.
  for(let k=0;k<3;k++){
   const ink=[60,52,60][k],value=255*(1-edge)+ink*edge;
   image.data[i+k]=Math.round(before[k]*(1-w)+value*w);
  }
  if(before.some((v,k)=>v!==image.data[i+k]))sharpened++;
 }
 g.putImageData(image,0,0);return {...r,eyeMask:mask(api,true),sharpened};
}
module.exports={finish,mask,eyePolygons,polygon};

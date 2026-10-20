'use strict';
const {pixels,premultiply,sample,put}=require('./sampling.cjs');
// Manually inspected SOURCE225 face apertures, not eyelid paths. Hair and its
// contour are foreground; the old eye texture beneath them is replaced fully.
const apertures=[
 [[38.5,122],[43,116.5],[46,112.5],[48.3,108],[51,104.5],[52,108],[54,112],[58,115.5],[62,118],[66.5,120.5],[71,123],[69,130],[64,134.5],[52,136],[41,132]],
 [[77.5,125],[81,121],[85.3,117.8],[88,113.8],[90,110],[92,112.5],[96,116],[101,118],[104,120],[106,121],[105,140],[100,144],[86,144],[80,138]]
];
// The open eye's outer lash extends ONTO the pink lock at these source pixels.
// Restore it from adjacent original hair texture; do not import96px hair here.
const lashRepair=[[44,107],[45,107],[46,107],[43,108],[44,108],[45,108],[46,108],[43,109],[44,109],[45,109],[44,110]];
function mask(api){const c=api.createCanvas(225,225),g=c.getContext('2d');g.fillStyle='#fff';for(const p of apertures){g.beginPath();p.forEach(([x,y],i)=>i?g.lineTo(x,y):g.moveTo(x,y));g.closePath();g.fill();}return c;}
function transfer(master,open,expression,api){
 if(master.width!==225||master.height!==225||expression.width!==96||expression.height!==96)throw Error('Expected225 master and96 authored closed eyes');
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.drawImage(master,0,0);
 const d=g.getImageData(0,0,225,225),original=new Uint8ClampedArray(d.data),m=pixels(mask(api),api),src=premultiply(expression,api),v=new Float64Array(4),temp=new Uint8ClampedArray(4);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4,w=m[i+3]/255;if(!w)continue;
  sample(src,96,(x+.5)*96/225-.5,(y+.5)*96/225-.5,v);put(temp,0,v);
  for(let k=0;k<3;k++)d.data[i+k]=Math.round(d.data[i+k]*(1-w)+temp[k]*w);
 }
 for(const [x,y] of lashRepair){const i=(y*225+x)*4,from=(y*225+x-5)*4;d.data.set(original.subarray(from,from+4),i);}
 g.putImageData(d,0,0);return c;
}
module.exports={transfer,mask,apertures,lashRepair};

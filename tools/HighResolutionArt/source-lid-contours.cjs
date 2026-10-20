'use strict';
const {pixels}=require('./sampling.cjs');
// Trace measured 50% ink coverage in the authored raster. No hand-drawn eye path.
// One corner-cutting pass rounds the measured contour; no invented centreline.
const regions=[[16,50,27,55],[33,53,44,58]];
function contours(image,api){
 const src=pixels(image,api),result=[];
 for(const [x0,y0,x1,y1] of regions){
  const value=(x,y)=>{if(x<=x0||x>=x1||y<=y0||y>=y1||x>=42&&y>=57)return 0;const i=(y*96+x)*4,r=src[i],g=src[i+1],b=src[i+2];if(Math.max(r,g,b)-Math.min(r,g,b)>28)return 0;return Math.max(0,Math.min(1,(255-Math.max(r,g,b))/200));};
  const seg=[];
  for(let y=y0;y<y1;y++)for(let x=x0;x<x1;x++){
   const points=[[x+.5,y+.5],[x+1.5,y+.5],[x+1.5,y+1.5],[x+.5,y+1.5]],v=[value(x,y),value(x+1,y),value(x+1,y+1),value(x,y+1)],hits=[];
   for(let k=0;k<4;k++){const j=(k+1)%4;if((v[k]>=.5)!==(v[j]>=.5)){const t=(.5-v[k])/(v[j]-v[k]);hits.push([points[k][0]+t*(points[j][0]-points[k][0]),points[k][1]+t*(points[j][1]-points[k][1])]);}}
   if(hits.length===2)seg.push(hits);else if(hits.length===4){seg.push([hits[0],hits[1]],[hits[2],hits[3]]);}
  }
  const key=p=>p.map(v=>v.toFixed(5)).join(',');
  while(seg.length){const [a,b]=seg.pop(),line=[a,b];let n=0;while(key(line.at(-1))!==key(line[0])&&n++<500){const k=seg.findIndex(s=>key(s[0])===key(line.at(-1))||key(s[1])===key(line.at(-1)));if(k<0)break;const [u,v]=seg.splice(k,1)[0];line.push(key(u)===key(line.at(-1))?v:u);}if(line.length<4)continue;
   if(key(line[0])===key(line.at(-1)))line.pop();const smooth=[];
   for(let k=0;k<line.length;k++){const p=line[k],q=line[(k+1)%line.length];smooth.push([.75*p[0]+.25*q[0],.75*p[1]+.25*q[1]],[.25*p[0]+.75*q[0],.25*p[1]+.75*q[1]]);}result.push(smooth);
  }
 }return result;
}
function render(image,api){const paths=contours(image,api),c=api.createCanvas(225,225),g=c.getContext('2d');g.scale(225/96,225/96);g.fillStyle='#353d3d';
 for(const p of paths){g.beginPath();p.forEach(([x,y],i)=>i?g.lineTo(x,y):g.moveTo(x,y));g.closePath();g.fill();}return {image:c,paths};}
module.exports={render,contours,regions};

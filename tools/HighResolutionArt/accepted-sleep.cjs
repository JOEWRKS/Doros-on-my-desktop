'use strict';
// Articulated vector skin targets traced by the already accepted r23 still.
// No raster crossfade, no expression replacement, and no r21 final silhouette.
const {paths}=require('../PreviewLocomotion/reference-parts.cjs');
const {artwork}=require('./body.cjs');
const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
const smooth=x=>{x=Math.max(0,Math.min(1,x));return x*x*(3-2*x);};
const target={
 farFront:[['M',18,68],['C',18,70,12,72,11,74],['C',10,77,10.8,78,12,79],['C',15,82,24,82.8,27.8,79.6],['C',29.2,76,26.5,73,27.3,70],['C',25,66,20,65,18,68],['Z']],
 nearFront:[['M',27.3,70],['C',26.5,73,29.2,76,27.8,79.6],['C',32,80,36,77,36,73],['L',35,68],['Z']],
 haunch:[['M',59.8,79.1],['C',65,81,71,80.5,75.5,77],['C',80.5,73,82,65,78.7,58],['C',76.6,53.9,72,50.9,67,48.5],['C',61,42,42,47,27,58],['L',33,70],['Z']],
 chest:[['M',18,68],['C',18,64,19,61,27,58],['C',42,47,61,42,67,48.5],['L',64.2,72.2],['C',61.1,73.1,58.8,76,59.8,79.1],['C',57,78.6,54,80,50,81],['C',42,83,29.6,83,27.8,79.6],['C',29.2,76,26.5,73,27.3,70],['Z']],
 nearHind:[['M',64.2,72.2],['C',61.1,73.1,58.8,76,59.8,79.1],['C',65,81,71,80.5,75.5,77],['L',76,68],['Z']]
};
const lerp=(a,b,t)=>a.map((v,i)=>v+(b[i]-v)*t);
function cubics(path){let p,first;const out=[];for(const [op,...v]of path){
 if(op==='M'){p=v;first=v;}else if(op==='C'){out.push([p,v.slice(0,2),v.slice(2,4),v.slice(4)]);p=v.slice(4);}
 else {const q=op==='Z'?first:v;if(Math.hypot(...q.map((x,i)=>x-p[i]))>1e-10)out.push([p,lerp(p,q,1/3),lerp(p,q,2/3),q]);p=q;}}
 const signed=out.reduce((s,c)=>s+c[0][0]*c[3][1]-c[3][0]*c[0][1],0);return signed<0?out.reverse().map(c=>c.slice().reverse()):out;}
function splitLongest(curves){let index=0,best=-1;curves.forEach((c,i)=>{const d=c.slice(1).reduce((s,p,j)=>s+Math.hypot(...p.map((v,k)=>v-c[j][k])),0);if(d>best){best=d;index=i;}});
 const c=curves[index],a=lerp(c[0],c[1],.5),b=lerp(c[1],c[2],.5),d=lerp(c[2],c[3],.5),e=lerp(a,b,.5),f=lerp(b,d,.5),q=lerp(e,f,.5);curves.splice(index,1,[c[0],a,e,q],[q,f,d,c[3]]);}
function pair(a,b){a=cubics(a);b=cubics(b);while(a.length<b.length)splitLongest(a);while(b.length<a.length)splitLongest(b);
 let best=0,score=Infinity;for(let k=0;k<b.length;k++){let sum=0;for(let i=0;i<a.length;i++)for(let j=0;j<4;j++)for(let axis=0;axis<2;axis++)sum+=(a[i][j][axis]-b[(i+k)%b.length][j][axis])**2;if(sum<score){best=k;score=sum;}}
 return [a,b.map((_,i)=>b[(i+best)%b.length])];}
function boneMap(joints,root,tip,u){
 const mix=(a,b)=>a.map((v,i)=>v+(b[i]-v)*u),r=mix(joints[0],root),f=mix(joints[2],tip);
 const len=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1]),a=len(joints[0],joints[1]),b=len(joints[1],joints[2]),d=len(r,f);
 const along=(a*a-b*b+d*d)/(2*d),h=Math.sqrt(Math.max(0,a*a-along*along)),dx=(f[0]-r[0])/d,dy=(f[1]-r[1])/d;
 const k=[r[0]+dx*along+dy*h,r[1]+dy*along-dx*h];
 const angle=(a,b)=>Math.atan2(b[1]-a[1],b[0]-a[0]),turn=[angle(r,k)-angle(joints[0],joints[1]),angle(k,f)-angle(joints[1],joints[2])];
 return q=>{const weight=smooth((q[1]-joints[1][1]+2)/4),maps=[r,k].map((p,i)=>{const x=q[0]-joints[i][0],y=q[1]-joints[i][1],c=Math.cos(turn[i]),s=Math.sin(turn[i]);return [p[0]+x*c-y*s,p[1]+x*s+y*c];});return maps[0].map((v,i)=>v+(maps[1][i]-v)*weight);};
}
function create(rig,api,measurement){
 const rest=rig.pose(0),from={};for(const name of Object.keys(target))from[name]=paths(name,rest,{artwork});from.haunch=rootedHaunch(0);from.nearHind=connectedHind(q=>q);
 const pairs={};for(const name of Object.keys(target))pairs[name]=pair(from[name].fill,target[name]);
 const pen=require('../PreviewLocomotion/original-contour-source.cjs').measure(measurement,api).ink;
 function render(progress){
  const raw=Math.max(0,Math.min(1,progress)),t=smooth(raw),headY=8*t;
  if(raw===0)return {body:rig.render(rest,{includeHead:false,extraParts:{haunch:from.haunch,nearHind:from.nearHind}}),headY,effect:null};
  const extraParts={};
  for(const name of Object.keys(target)){
   // Forepads plant first; the rear folds and torso settles afterwards.
   const u=name==='farFront'?smooth(raw/.8):name==='nearFront'?smooth(raw/.92):smooth((raw-.08)/.92);
   const goals={farFront:[[22,69],[18,78]],nearFront:[[31,68],[30,79]],nearHind:[[67,65],[62,78]]};
   const limb=rig.rest.find(l=>l.name===name),map=limb?boneMap(limb.joints,...goals[name],u):q=>[q[0]+4*u,q[1]+5*u];
   // Constant-length leg axes lead the movement. Only the final folded skin
   // projection settles to the accepted r23 contour; no output images blend.
   const settle=u**3,[a,b]=pairs[name];
   const curves=a.map((c,i)=>c.map((p,k)=>map(p).map((v,j)=>v+(b[i][k][j]-v)*settle)));
   const fill=[['M',...curves[0][0]],...curves.map(c=>['C',...c.slice(1).flat()]),['Z']];
   extraParts[name]={name,fill,ink:[],rootInk:[]};
  }
  // Continuous local divider geometry. The moving foreground skin occludes
  // its proximal part; opacity is never animated to hide a late seam.
  const fold=[['M',64.2,72.2],['C',61.1,73.1,58.8,76,59.8,79.1]];
  const f=fold.map(([op,...q])=>[op,...q.map((v,i)=>v+(i%2?5:-1)*(1-t))]);
  // The near fold emerges from behind the settling torso. Clip the curve by
  // the moving occlusion edge, never switch a complete dark line on at entry.
  const curve=[f[0].slice(1),f[1].slice(1,3),f[1].slice(3,5),f[1].slice(5,7)],gate=90-20*t;
  const mix=(a,b,u)=>a.map((v,i)=>v+(b[i]-v)*u);
  function tail(u){const a=mix(curve[0],curve[1],u),b=mix(curve[1],curve[2],u),c=mix(curve[2],curve[3],u),d=mix(a,b,u),e=mix(b,c,u);return [mix(d,e,u),e,c,curve[3]];}
  let ink=[];
  if(gate<curve[3][1]){let lo=0,hi=1;for(let i=0;i<24;i++){const u=(lo+hi)/2;if(tail(u)[0][1]<gate)lo=u;else hi=u;}const c=tail(gate<=curve[0][1]?0:(lo+hi)/2);ink=[[['M',...c[0]],['C',...c.slice(1).flat()]]];}
  extraParts.chest.ink=ink;
  const body=rig.render(rest,{includeHead:false,extraParts,interiorParts:['chest'],interiorOccluders:{chest:[]},clipInteriorToSkin:true});
  const effect=api.createCanvas(225,225),g=effect.getContext('2d');g.scale(225/96,225/96);g.translate(0,-headY);
  // r23's short forepad divider remains in front of the white body skirt,
  // but the fixed head/face texture is never repainted.
  const py=74.25+4*(1-t),px=28+4*(1-t);
  g.beginPath();g.rect(26,66.25+headY,5,6.5);g.clip();g.beginPath();g.moveTo(px,py);
  g.bezierCurveTo(px+.4,76+4*(1-t),px+.7,78+3*(1-t),27.8+4*(1-t),79.6+2*(1-t));g.strokeStyle=`rgb(${pen.rgb.join(',')})`;g.lineWidth=pen.width;g.lineCap='round';g.stroke();
  return {body,headY,effect};
 }
 return {render};
}
module.exports={create,target};

'use strict';
const {paths}=require('../PreviewLocomotion/reference-parts.cjs');
const {artwork}=require('./body.cjs');
const {rootedHaunch,connectedHind}=require('./motion-polish.cjs');
const {premultiply,sample,over,surface}=require('./sampling.cjs');
const smooth=v=>{v=Math.max(0,Math.min(1,v));return v*v*(3-2*v);};
const mix=(a,b,t)=>a.map((v,i)=>v+(b[i]-v)*t);
function cubics(path){let p,first;const out=[];for(const [op,...v]of path){
 if(op==='M'){p=v;first=v;}else if(op==='C'){out.push([p,v.slice(0,2),v.slice(2,4),v.slice(4)]);p=v.slice(4);}
 else {const q=op==='Z'?first:v;if(Math.hypot(...q.map((x,i)=>x-p[i]))>1e-10)out.push([p,mix(p,q,1/3),mix(p,q,2/3),q]);p=q;}}
 return out.reduce((s,c)=>s+c[0][0]*c[3][1]-c[3][0]*c[0][1],0)<0?out.reverse().map(c=>c.slice().reverse()):out;}
function split(cs){let k=0,best=-1;cs.forEach((c,i)=>{const n=c.slice(1).reduce((s,p,j)=>s+Math.hypot(...p.map((v,a)=>v-c[j][a])),0);if(n>best){best=n;k=i;}});const c=cs[k],a=mix(c[0],c[1],.5),b=mix(c[1],c[2],.5),d=mix(c[2],c[3],.5),e=mix(a,b,.5),f=mix(b,d,.5),q=mix(e,f,.5);cs.splice(k,1,[c[0],a,e,q],[q,f,d,c[3]]);}
function morph(a,b){a=cubics(a);b=cubics(b);while(a.length<b.length)split(a);while(b.length<a.length)split(b);let best=0,score=Infinity;
 for(let k=0;k<b.length;k++){let s=0;for(let i=0;i<a.length;i++)for(let j=0;j<4;j++)for(let d=0;d<2;d++)s+=(a[i][j][d]-b[(i+k)%b.length][j][d])**2;if(s<score){score=s;best=k;}}
 const target=b.map((_,i)=>b[(i+best)%b.length]);return t=>{const c=a.map((p,i)=>p.map((q,j)=>mix(q,target[i][j],t)));return [['M',...c[0][0]],...c.map(p=>['C',...p.slice(1).flat()]),['Z']];};}
const transform=(path,dx,dy)=>path.map(([op,...v])=>[op,...v.map((p,i)=>p+(i%2?dy:dx))]);
function lowerInk(path,gate){const out=[];let prev=null,pen=false;for(const [op,...p]of path){
 if(op==='M'){prev=p;pen=false;continue;}if(!prev)continue;
 const a=prev,b=p;if(a[1]>=gate&&b[1]>=gate){if(!pen)out.push(['M',...a]);out.push(['L',...b]);pen=true;}
 else if((a[1]>=gate)!==(b[1]>=gate)){const q=mix(a,b,(gate-a[1])/(b[1]-a[1]));if(b[1]>=gate){out.push(['M',...q],['L',...b]);pen=true;}else{if(!pen)out.push(['M',...a]);out.push(['L',...q]);pen=false;}}
 else pen=false;prev=p;}return out;}
function create(model){
 const {s,heads}=model,rig=s.postures.open.rig,rest=rig.pose(0),base={};
 for(const name of ['farFront','nearFront','chest','haunch'])base[name]=paths(name,rest,{artwork});
 base.haunch=rootedHaunch(0);base.nearHind=connectedHind(p=>p);
 const torso=[['M',38,45],['C',44,40,55,41,58,46],['C',61,52,63,58,63.5,63],['C',63.8,66,64.125,69.25,64.25,72.4375],['L',48,74],['L',30,72],['C',25,66,26,53,38,45],['Z']];
 const chest=[['M',22,49],['C',26,43,42,42,49,48],['C',54,57,53,62,49,67],['C',42,71,34,70,29,65],['C',25,61,21,57,22,49],['Z']];
 const bodyPaths={chest:morph(base.chest.fill,chest),haunch:t=>base.haunch.fill.map(([op,...v],i)=>[op,...v.map((x,j)=>x+(torso[i][j+1]-x)*t)])};
 const bodyCache=new Map();
 function body(progress){const u=smooth(progress),key=u.toFixed(8);if(bodyCache.has(key))return bodyCache.get(key);
  if(u===0){const d=premultiply(rig.render(rest,{includeHead:false,extraParts:{haunch:base.haunch,nearHind:base.nearHind}}),s.api);bodyCache.set(key,d);return d;}
  const extraParts={};
  for(const name of ['chest','haunch'])extraParts[name]={fill:bodyPaths[name](u),ink:[],rootInk:[]};
  for(const [name,dx,dy]of [['farFront',12,-14],['nearFront',12,-19]]){
   const p=base[name];extraParts[name]={fill:transform(p.fill,dx*u,dy*u),ink:p.ink.map(p=>transform(name==='farFront'?lowerInk(p,76-25*u):p,dx*u,dy*u)),rootInk:[]};
  }
  extraParts.nearHind=connectedHind(([x,y])=>[x-10*u,y+7*u],[-10*u,7*u]);
  // Far rear paw starts wholly inside the torso and appears below it as it
  // descends. Reuse the accepted folded paw rather than adding a round toe.
  // Both hanging rear paws join the same skin envelope. Do not use the gait's
  // special farHind outline pass, which leaves a diagonal socket across the
  // vertical torso when the other rear paw moves in front of it.
  extraParts.rearFar=connectedHind(([x,y])=>[x-25*u,y-10+17*u],[-25*u,-10+17*u]);
  const image=rig.render(rest,{includeHead:false,extraParts,interiorParts:['farFront','nearFront'],interiorOccluders:{farFront:['nearFront'],nearFront:[]},clipInteriorToSkin:true});
  const result=premultiply(image,s.api);bodyCache.set(key,result);return result;
 }
 function safeAngle(progress,angle,padding=32){
  const u=smooth(progress),d=body(progress),points=[];
  for(const [pixels,dx,dy]of [[d,11.0236*u,-14.9681*u],[heads.open,0,0]]){
   let l=225,r=0,t=225,b=0;for(let y=0;y<225;y++)for(let x=0;x<225;x++)if(pixels[(y*225+x)*4+3]>0){l=Math.min(l,x);r=Math.max(r,x+1);t=Math.min(t,y);b=Math.max(b,y+1);}
   for(const x of [l,r])for(const y of [t,b])points.push([x*96/225-dx,y*96/225-dy]);
  }
  const fits=a=>[-a,a].every(a=>{const c=Math.cos(a),s=Math.sin(a);return points.every(([x,y])=>{const xx=43+(x-43)*c-(y-32)*s,yy=32+(x-43)*s+(y-32)*c;return xx>-padding+2&&xx<96+padding-2&&yy>-padding+2&&yy<96+padding-2;});});
  const request=Math.min(Math.abs(angle),88*Math.PI/180);if(fits(request))return Math.sign(angle)*request;
  let lo=0,hi=request;for(let n=0;n<14;n++){const a=(lo+hi)/2;if(fits(a))lo=a;else hi=a;}return Math.sign(angle)*lo;
 }
 function render(progress,{angle=0,padding=32,size=144,eye='open',compression=0}={}){
  if(![progress,angle,compression].every(Number.isFinite))throw Error('Finite grab motion required');
  angle=safeAngle(progress,angle,padding);
  const u=smooth(progress),bd=body(progress),a=new Float64Array(4),b=new Float64Array(4),co=Math.cos(angle),sn=Math.sin(angle);
  // Authored old-key registration: keep the source head at the captured point.
  // More canvas margin, never shrink the character or move its grab anchor.
  const dx=11.0236*u,dy=-14.9681*u,anchor=[43,32];
  return surface(s.api,size,(x,y)=>{
   x=48+(x-48)/(1+compression*5/9);y=87+(y-87)/(1-compression);
   const ox=x-anchor[0],oy=y-anchor[1];x=anchor[0]+ox*co+oy*sn;y=anchor[1]-ox*sn+oy*co;
   sample(bd,225,(x+dx)*225/96-.5,(y+dy)*225/96-.5,a);
   sample(heads[eye],225,x*225/96-.5,y*225/96-.5,b);return over(a,b);
  },padding);
 }
 return {render,body,safeAngle};
}
module.exports={create,smooth,morph};

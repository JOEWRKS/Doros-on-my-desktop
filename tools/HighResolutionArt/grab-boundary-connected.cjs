'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const traced=require('./grab-eight-traced.cjs');
const {smooth,interpolated}=require('./grab-eight-source.cjs');
const dir=path.join(root,'artifacts/grab-boundary-connected-20261017-r43');
const pad=32,native=375,factor=225/96;
function dense(rows){
 const p=[];let start;
 for(const [op,...v]of rows){if(op==='M'){start=v;p.push(v);}else if(op==='L'){p.push(v);start=v;}else if(op==='C'){
  for(let i=1;i<=64;i++){const t=i/64,u=1-t;p.push([0,1].map(k=>start[k]*u*u*u+3*v[k]*u*u*t+3*v[k+2]*u*t*t+v[k+4]*t*t*t));}start=v.slice(4);
 }}return p;
}
function resample(rows,count){const p=dense(rows),d=[0];for(let i=1;i<p.length;i++)d.push(d.at(-1)+Math.hypot(p[i][0]-p[i-1][0],p[i][1]-p[i-1][1]));let j=1;return Array.from({length:count},(_,i)=>{const at=i/(count-1)*d.at(-1);while(j<d.length-1&&d[j]<at)j++;const t=(at-d[j-1])/(d[j]-d[j-1]||1);return p[j-1].map((v,k)=>v+(p[j][k]-v)*t);});}
function anatomicalSamples(rows,anchors){
 const points=dense(rows),ends=[0];
 for(const a of anchors){let best=Infinity,index=ends.at(-1)+1;for(let j=index;j<points.length-1;j++){const d=Math.hypot(points[j][0]-a[0],points[j][1]-a[1]);if(d<best){best=d;index=j;}}ends.push(index);}
 ends.push(points.length-1);const out=[];
 for(let i=1;i<ends.length;i++){
  const p=points.slice(ends[i-1],ends[i]+1),rows=p.map((p,j)=>[j?'L':'M',...p]),samples=resample(rows,33);out.push(...(i===1?samples:samples.slice(1)));
 }return out;
}
function poly(points,close=false){const p=new api.Path2D();points.forEach((q,i)=>i?p.lineTo(...q):p.moveTo(...q));if(close){p.lineTo(43,43);p.lineTo(20,43);p.closePath();}return p;}
function renderBody(front,rear,fold,spur,pen){
 const skin=poly(front,true).op(poly(rear,true),api.PathOp.Union),c=api.createCanvas(native,native),g=c.getContext('2d');g.scale(factor,factor);g.translate(pad,pad);g.fillStyle='white';g.fill(skin);
 const ink=api.createCanvas(native,native),ig=ink.getContext('2d');ig.scale(factor,factor);ig.translate(pad,pad);ig.lineWidth=pen.width;ig.lineJoin=ig.lineCap='round';ig.strokeStyle='white';ig.stroke(skin);ig.globalCompositeOperation='lighten';ig.stroke(poly(front));
 if(Math.hypot(...fold[0].map((v,k)=>v-fold.at(-1)[k]))>.001)ig.stroke(poly(fold));
 if(Math.hypot(...spur[0].map((v,k)=>v-spur.at(-1)[k]))>.001)ig.stroke(poly(spur));
 ig.resetTransform();ig.globalCompositeOperation='source-in';ig.fillStyle=`rgb(${pen.rgb.join(',')})`;ig.fillRect(0,0,native,native);g.resetTransform();g.drawImage(ink,0,0);return c;
}
// Move small endpoint rasterization residuals with the same local geometry.
// The body itself is always an opaque vector union. No full-body opacity morph.
function transportedResidual(src,points,dest,x,y,out){
 let sum=0,dx=0,dy=0;
 for(let i=0;i<points.length;i++){const p=points[i],q=dest[i],d=(x-q[0])**2+(y-q[1])**2,w=1/(.4+d)**2;sum+=w;dx+=(p[0]-q[0])*w;dy+=(p[1]-q[1])*w;}
 const sx=(x+dx/sum+pad)*factor-.5,sy=(y+dy/sum+pad)*factor-.5;
 // Residuals are signed deltas, not colors: the normal RGBA sampler would
 // clamp their negative terms and darken/thicken every corrected outline.
 const ix=Math.floor(sx),iy=Math.floor(sy);out.fill(0);
 for(let yy=0;yy<2;yy++)for(let xx=0;xx<2;xx++){
  const x=ix+xx,y=iy+yy;if(x<0||x>=native||y<0||y>=native)continue;
  const w=(xx?sx-ix:1-sx+ix)*(yy?sy-iy:1-sy+iy),i=(y*native+x)*4;
  for(let c=0;c<4;c++)out[c]+=src[i+c]*w;
 }return out;
}
async function create(){
 const m=await traced.create(),raw=structuredClone(m.keys),common=await require('./current-motion.cjs').create(),rig=common.s.postures.open.rig;
 const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
 const idleSmall=rig.render(rig.pose(0),{includeHead:false,extraParts:{haunch:rootedHaunch(0),nearHind:connectedHind(q=>q)}});
 const idle=api.createCanvas(native,native);idle.getContext('2d').drawImage(idleSmall,75,75);
 const held=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/body-padded-375.png'));
 const pen=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-contour-still-20261017-r33/measurement.json'),'utf8')).pen;
 raw[0].front=[['M',16.5,57],['C',15.5,64,16.5,72,19,78],['C',21,82.5,25,83,26.5,79],['C',26.9,77.8,27,76.7,27.1,75.5],['C',29,75.5,30,75.5,33.1,76.5],['C',34.9,80,37.5,85.8,41.8,86.4],['C',45.4,87.1,48.6,85.4,48.7,81.5],['L',48.5,77.5],['C',52,77,56,75,58.5,73.5]];
 const hind=connectedHind(q=>q).fill;
 // Reverse the exact accepted near-hind outer curves, then the fixed rump.
 const reverse=[];let last=hind[0].slice(1),segments=[];for(const row of hind.slice(1,6)){segments.push([last,row]);last=row.slice(-2);}
 for(const [start,row]of segments.reverse())reverse.push(['C',row[3],row[4],row[1],row[2],...start]);
 raw[0].rear=[['M',38,73],['C',39,77,40,80,42,82],['C',47,82,51,77,58.5,73.5],['L',138.1*96/225,173.1*96/225],...reverse,['C',75.125,62.25,75.25,59,74.5,56],['C',73,52,70,48,65.5,45],['L',51,43]];
 const frontAnchors=[[[23,81.5],[27.1,75.5],[33.1,76.5],[42,86.4]],[[21,90],[26,83],[30.5,84],[39,94.5]],[[22,86.5],[30,78.2],[33.7,78.1],[41,90]],[[23,83],[29,75.2],[34.2,77.6],[39,83]],[[27,78.5],[34,69],[36,74.8],[43,78.5]],[[31,75.7],[37.5,66],[39.7,73.1],[46,76.5]],[[33,75.5],[39.5,67],[41.7,73.9],[46,76]],[[35,75.7],[42,65.9],[44,72],[49,76]]];
 const rearAnchors=[[[42,82],[58.5,73.5],[138.1*96/225,173.1*96/225],[153*96/225,197.1*96/225],[163.7*96/225,175.4*96/225],[74.25,65.4375]],[[43,94.6],[51.3,85.2],[55.1,85],[65,95.5],[70,88],[73.7,74]],[[45,94.8],[52.1,85.1],[55.5,85.5],[66,95.2],[71,88],[74.3,74]],[[43,95.2],[51.8,84.5],[56,84.8],[66,96],[73,87],[73.5,76]],[[44,95.3],[51.1,83.9],[55,84.8],[65,96],[73,86],[73.5,75]],[[42,95.4],[53.2,84.8],[55.5,85.2],[65,96],[73,85],[72.6,75]],[[42,96],[54,84.6],[56,85.5],[65,96],[72,87],[72,75]],[[42.4,96.3],[51,84.5],[54.8,84.2],[64.4,96.4],[71.8,90.4],[71.4,72.7]]];
 const keys=raw.map((k,n)=>{
  const tr=m.registration[n],fold=traced.folds[n]?resample(traced.mapRows(traced.folds[n],tr),32):Array.from({length:32},()=>n===0?[58.5,73.5]:k.rear[Math.min(4,k.rear.length-1)].slice(-2));
  const spur=n===3?resample(traced.mapRows([['M',34,75],['C',34,76.2,34.1,77,34.2,77.6]],tr),8):Array.from({length:8},()=>{const f=k.front[3].slice(-2);return f;});
  const convert=a=>n===0?a:a.map(p=>p.map((v,c)=>v*tr.factor+(c?tr.dy:tr.dx)));
  return {front:anatomicalSamples(k.front,convert(frontAnchors[n])),rear:anatomicalSamples(k.rear,convert(rearAnchors[n])),fold,spur};
 });
 const at=t=>Object.fromEntries(['front','rear','fold','spur'].map(name=>[name,interpolated(keys,name,t)]));
 const geometry=t=>{const p=at(t);return renderBody(p.front,p.rear,p.fold,p.spur,pen);};
 const endpoints=[idle,held],residuals=endpoints.map((im,i)=>{const a=premultiply(im,api),b=premultiply(geometry(i),api);return Float64Array.from(a,(v,k)=>v-b[k]);});
 const landmarks=p=>[...p.front.filter((_,i)=>i%8===0),...p.rear.filter((_,i)=>i%8===0)];
 const endpointPoints=[landmarks(keys[0]),landmarks(keys[7])],hp=premultiply(m.head,api),cache=new Map();
 function body(t){
  assert(Number.isFinite(t)&&t>=0&&t<=1);if(cache.has(t))return cache.get(t);
  const base=premultiply(geometry(t),api),w0=1-smooth(t*7),w1=smooth((t-6/7)*7),points=landmarks(at(t)),r=new Float64Array(4);
  if(w0||w1)for(let y=0;y<native;y++)for(let x=0;x<native;x++){
   const i=(y*native+x)*4,lx=(x+.5)/factor-pad,ly=(y+.5)/factor-pad;
   // Limit work to the body extent; source residuals elsewhere are zero.
   if(lx<10||lx>85||ly<35||ly>111)continue;
   for(const [k,w]of [[0,w0],[1,w1]])if(w){transportedResidual(residuals[k],endpointPoints[k],points,lx,ly,r);for(let c=0;c<4;c++)base[i+c]+=w*r[c];}
   base[i+3]=Math.max(0,Math.min(255,base[i+3]));for(let c=0;c<3;c++)base[i+c]=Math.max(0,Math.min(base[i+3],base[i+c]));
  }
  cache.set(t,base);return base;
 }
 function render(t,scale=1.5){const bd=body(t),a=new Float64Array(4),b=new Float64Array(4);return surface(api,96*scale,(x,y)=>{sample(bd,native,(x+pad)*factor-.5,(y+pad)*factor-.5,a);sample(hp,225,x*factor-.5,y*factor-.5,b);return over(a,b);},pad);}
 function reference(i,scale=1.5){const bd=premultiply(endpoints[i],api),a=new Float64Array(4),b=new Float64Array(4);return surface(api,96*scale,(x,y)=>{sample(bd,native,(x+pad)*factor-.5,(y+pad)*factor-.5,a);sample(hp,225,x*factor-.5,y*factor-.5,b);return over(a,b);},pad);}
 return {render,reference,geometry,keys,body,common,head:m.head,residuals};
}
module.exports={create,dir,resample};

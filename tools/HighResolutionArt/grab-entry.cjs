'use strict';
// Isolated source-key study. No product motion selectors are changed.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const {outline,forepaws}=require('./grab-contour-still.cjs');
const dir=path.join(root,'artifacts/grab-entry-20261017-r38');
const src=path.join(root,'artifacts/grab-contour-still-20261017-r33');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
// Observed stroke centers from the eight original100px source keys. These are
// input annotations, not independent verification truth. Each row keeps semantic
// forepaw/hip/foot identities; no nearest-curve matching or whole-body scaling.
const front=[
 [['M',12,76],['C',13,82,14,89,17,92],['C',18,94,20,94,21.5,93],['C',23,92.8,23.5,91,24,88],['C',30,88,32,99,39,99],['C',45,99,45,95,45,90],['C',49,89,53,88,56,85]],
 [['M',14,70],['C',14.5,79,14.5,87,18,89.5],['C',19,90.5,20.5,90.5,22,90],['C',24,89.5,25,87,26,83],['C',32,83,30,94,37,94],['C',42,95,45,95,45,90],['C',45,88,45,86,45,83]],
 [['M',15,66],['C',15.2,75,15.5,81,18,84],['C',19,86,20.5,87,22,86.5],['C',27,86.5,29,84,30,78],['C',34,78,31.5,88,40,90],['C',43,90,45,88,46,86],['C',47.5,83,48,81,48.5,78]],
 [['M',16,62],['C',16.2,70,16.5,76,18,79],['C',19,81.5,21,83,23,83],['C',27,83,29,81,29,75],['C',34,75,34,83,40,83],['C',43,83,46,80,47,78],['C',48,76,49,74,50,71]],
 [['M',18,59],['C',18.5,67,19,72,22,75],['C',23,77,25,78.5,27,78.5],['C',31,78.5,33,74,34,69],['C',35,74,36,78.5,43,78.5],['C',46,78.5,48,76,49,74],['C',51,70,52,68,53,65]],
 [['M',22,57],['C',22,65,23,72,27,74.5],['C',28,75.5,29.5,76,31,75.5],['C',35,75.5,37,71,37.5,66],['C',39,70,39,75,45,76],['C',49,77,51,74,53,71],['C',54,68,54.5,66,55.5,64]],
 [['M',25,59],['C',25.1,66,25,72,30,74.5],['C',31,75.5,32,76,33,75.5],['C',37,75.5,39,72,39.5,67],['C',41,72,41,75,46,76],['C',50,77,52,73,53,70],['C',54,68,54,67,54.5,66]],
 [['M',28,60],['C',28.7,65,28.9,71,31.4,73.5],['C',32.2,74.5,33.3,75.3,34.8,75.7],...forepaws.slice(1)]
];
function splitAt(p,index){const mid=(a,b)=>a.map((x,i)=>(x+b[i])/2),p0=p[index-1].slice(-2),v=p[index].slice(1),p1=v.slice(0,2),p2=v.slice(2,4),p3=v.slice(4),a=mid(p0,p1),b=mid(p1,p2),d=mid(p2,p3),e=mid(a,b),f=mid(b,d),q=mid(e,f);p.splice(index,1,['C',...a,...e,...q],['C',...f,...d,...p3]);}
splitAt(front[7],5);assert(front.every(p=>p.length===7));
const rear=[
 [['M',34,73],['C',35,80,38,85,43,87],['C',46,88,50,87,52,86],['C',54,85,55,85,56,85],['C',57,90,59,94,63,95.5],['C',68,97,69,92,67,87],['C',66,84,68,82,70,79],['C',74,74,73,65,66,61],['C',63,57,62,55,60,53]],
 [['M',37,74],['C',36,82,35,91,39,94],['C',43,96,47,94,49,92],['C',51,89,50.5,85.5,54,85.5],['C',55,85.5,58,95,64,95],['C',69,96,70,92,70,88],['C',70,84,72,81,73,77],['C',75,70,70,64,65,61],['C',63,58,62,56,60,54]],
 [['M',36,72],['C',37,81,39,91,43,94],['C',47,97,51,94,53,90],['C',54,88,54,85,55,85],['C',57,91,60,95,65,95],['C',70,96,71,93,71,88],['C',71,84,73,81,74,77],['C',77,68,69,61,65,59],['C',63,58,62,57,61,56]],
 [['M',32,70],['C',33,81,36,91,41,94.5],['C',47,98,51,94,53,90],['C',54,88,54,85,55,85],['C',57,92,61,96,66,96],['C',71,96,73,92,73,87],['C',74,83,74,78,73,73],['C',72,66,67,60,64,56],['C',63,54,62,52,61,50]],
 [['M',29,69],['C',30,78,36,91,42,94.5],['C',47,98,50,94,51,89],['C',52,86,51,84,53,84],['C',55,91,59,96,65,96],['C',71,96,73,91,73,86],['C',74,82,74,77,73,72],['C',71,66,66,60,63,57],['C',62,54,61,52,60,50]],
 [['M',30,70],['C',32,79,35,91,40,94.5],['C',46,98,49,93,51,89],['C',52,86,52,84,54,84],['C',57,90,59,96,65,96],['C',71,96,73,90,73,85],['C',74,81,73,76,72,72],['C',70,64,68,60,64,57],['C',63,54,62,52,61,50]],
 [['M',32,71],['C',33,81,35,92,39.5,95],['C',45,99,49,93,51,89],['C',52,86,53,84,55,84],['C',57,91,59,96,65,96],['C',71,96,72,92,72,87],['C',73,82,73,77,71.5,72],['C',70,65,68,60,66,55],['C',65,52,64,49,63,47]],
 null
];
const scalePath=(p,t)=>p.map(([op,...v])=>[op,...v.map((x,i)=>x*t.factor+(i%2?t.dy:t.dx))]);
function pathOf(p){const s=new api.Path2D();for(const [op,...v]of p){if(op==='M')s.moveTo(...v);else if(op==='C')s.bezierCurveTo(...v);else if(op==='L')s.lineTo(...v);else s.closePath();}return s;}
function pink(im){const d=pixels(im,api);let l=im.width,r=0,t=im.height,b=0;for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++){const i=(y*im.width+x)*4;if(d[i+3]>128&&d[i]>110&&d[i]-d[i+1]>22&&d[i]-d[i+2]>5){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}}return [l,t,r,b];}
// Smooth monotone timing per key, not a cross-fade between sprite images.
function interpolate(keys,part,k,t){
 const slope=(vals,i)=>{if(i===0||i===vals.length-1)return 0;const a=vals[i]-vals[i-1],b=vals[i+1]-vals[i];return a*b<=0?0:2*a*b/(a+b);};
 return keys[k][part].map(([op,...v],i)=>[op,...v.map((x,j)=>{
  const a=keys.map(q=>q[part][i][j+1]),u=t*t,w=u*t;
  return (2*w-3*u+1)*x+(w-2*u+t)*slope(a,k)+(-2*w+3*u)*a[k+1]+(w-u)*slope(a,k+1);
 })]);
}
async function create(){
 const head=await api.loadImage(path.join(src,'masked-head-225.png')),hp=premultiply(head,api),meta=JSON.parse(fs.readFileSync(path.join(src,'measurement.json'),'utf8'));
 const original=[],registration=[],keys=[];
 for(let n=0;n<8;n++){
  const f=path.join(root,`src/Dororong.App/Assets/user-body-drag/0${n+1}.png`),im=await api.loadImage(f),p=pink(im),h=meta.headPinkBounds;
  const factor=(h[2]-h[0]+1)/(p[2]-p[0]+1)*96/225,dx=(h[0]+h[2]+1)/2*96/225-(p[0]+p[2]+1)/2*factor,dy=h[1]*96/225-p[1]*factor;
  const tr={factor,dx,dy};original.push(im);registration.push(tr);
  let r=rear[n];
  if(n===7){
   // Exact accepted hind-foot controls, expanded with exact subdivisions to
   // nine curves; original source-key silhouette, not r29's long narrow legs.
   const a=outline.slice(3,-2);r=[['M',34.8,75.7],...a,['C',65.8,51.5,65,49,64,47]];
  }
  r=structuredClone(r);
  if(n>=1&&n<=4){
   const [x,y]=[[0,0],[50.5,85.5],[51.5,85.5],[51,85],[50.5,84]][n];
   const right=n===4?54:55;
   r[3]=['C',x+1,90,x,y+1,x,y];
   r.splice(4,0,['C',x+1,y,right-1,y,right,y]);
  }
  keys.push({front:scalePath(front[n],tr),rear:scalePath(r,tr)});
 }
 // Exact splits at the same anatomical categories in each input: left outer
 // descent and right inner descent. Never pair controls by segment length.
 for(let n=0;n<7;n++){
  splitAt(keys[n].rear,n>=1&&n<=4?5:4);splitAt(keys[n].rear,1);
  if(n===0||n>=5)splitAt(keys[n].rear,4);
 }
 splitAt(keys[7].rear,4);
 assert(keys.every(k=>k.rear.length===12));
 const bodyCache=new Map();
 function body(progress){
  assert(Number.isFinite(progress)&&progress>=0&&progress<=1);
  if(bodyCache.has(progress))return bodyCache.get(progress);
  const k=Math.min(6,Math.floor(progress*7)),t=progress*7-k,fp=interpolate(keys,'front',k,t),rp=interpolate(keys,'rear',k,t);
  const close=p=>p.concat([['L',43,45],['L',22,48],['Z']]);
  const f=pathOf(close(fp)),r=pathOf(close(rp)),union=f.op(r,api.PathOp.Union);
  const fill=api.createCanvas(375,375),g=fill.getContext('2d');g.scale(225/96,225/96);g.translate(32,32);g.fillStyle='white';g.fill(union);
  const ink=api.createCanvas(375,375),ig=ink.getContext('2d');ig.scale(225/96,225/96);ig.translate(32,32);ig.lineWidth=meta.pen.width;ig.lineCap=ig.lineJoin='round';ig.strokeStyle='white';ig.stroke(union);
  // One ink coverage mask: coincident foreground/outer curves must not darken.
  ig.globalCompositeOperation='lighten';ig.stroke(pathOf(fp));
  ig.resetTransform();ig.globalCompositeOperation='source-in';ig.fillStyle=`rgb(${meta.pen.rgb.join(',')})`;ig.fillRect(0,0,375,375);
  g.resetTransform();g.drawImage(ink,0,0);
  const result=premultiply(fill,api);bodyCache.set(progress,result);return result;
 }
 function render(progress,scale=1.5){const bd=body(progress),a=new Float64Array(4),b=new Float64Array(4);return surface(api,96*scale,(x,y)=>{sample(bd,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);},32);}
 return {render,body,original,registration,keys,hp};
}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create();
 const contract={sourceFiles:Object.fromEntries(['masked-head-225.png','body-padded-375.png'].map(f=>[f,sha(path.join(src,f))])),scope:'Isolated original-key body entry candidate; protected head unchanged',runtimeChanged:false,sourceArtworkChanged:false,endpointIdentity:'not yet established',style:'Existing partial-alpha high-resolution compositor; no palette conversion'};
 fs.writeFileSync(path.join(dir,'contract.json'),JSON.stringify(contract,null,2));
 for(let page=0;page<4;page++){
  const c=api.createCanvas(1000,650),g=c.getContext('2d');g.fillStyle='white';g.fillRect(0,0,1000,650);g.imageSmoothingEnabled=false;
  for(let row=0;row<2;row++){
   const n=page*2+row,tr=m.registration[n],im=m.render(n/7),y=row*325+42;
   fs.writeFileSync(path.join(dir,`key-${n}.png`),im.toBuffer('image/png'));
   g.fillStyle='#222';g.font='16px sans-serif';g.fillText(`Original 0${n+1} / head registered`,24,y-16);g.fillText(`Candidate key ${n} / fixed head`,520,y-16);
   const s=2.5;g.drawImage(m.original[n],90+(32+tr.dx)*s,y+(32+tr.dy)*s-125,100*tr.factor*s,100*tr.factor*s);
   g.drawImage(im,590,y-125,400,400);
  }fs.writeFileSync(path.join(dir,`key-comparison-${page}.png`),c.toBuffer('image/png'));
 }
 fs.writeFileSync(path.join(dir,'key-data.json'),JSON.stringify({registration:m.registration,keys:m.keys},null,2));
 console.log(dir);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={create,dir};

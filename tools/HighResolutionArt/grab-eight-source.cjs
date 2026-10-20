'use strict';
// Eight-source contour animation study. No image crossfade; no product selector.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const dir=path.join(root,'artifacts/grab-eight-source-20261017-r40');
const sourceDir=path.join(root,'src/Dororong.App/Assets/user-body-drag');
const lockedDir=path.join(root,'artifacts/grab-contour-still-20261017-r33');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const clamp=(v,a,b)=>Math.max(a,Math.min(b,v));
const smooth=t=>{t=clamp(t,0,1);return t*t*t*(10+t*(-15+6*t));};
function flatten(rows){
 const result=[];let last;
 for(const [op,...v]of rows){
  if(op==='M'){last=v;result.push(v);}else if(op==='C'){
   const a=last,b=v.slice(0,2),c=v.slice(2,4),d=v.slice(4);
   for(let i=1;i<=12;i++){const t=i/12,u=1-t;result.push([0,1].map(k=>a[k]*u*u*u+3*b[k]*u*u*t+3*c[k]*u*t*t+d[k]*t*t*t));}last=d;
  }else throw Error('Unexpected annotation command '+op);
 }return result;
}
function grayInk(data,x,y){
 const ix=Math.floor(x),iy=Math.floor(y);let s=0;
 for(let j=0;j<2;j++)for(let i=0;i<2;i++){
  const px=ix+i,py=iy+j;if(px<0||py<0||px>=100||py>=100)continue;
  const q=(py*100+px)*4,r=data[q],g=data[q+1],b=data[q+2],range=Math.max(r,g,b)-Math.min(r,g,b);
  // Body ink in these authored references is neutral gray. Reject pink hair.
  const value=range<=15?Math.max(0,(235-(r+g+b)/3)/180):0;
  s+=value*(i?x-ix:1-x+ix)*(j?y-iy:1-y+iy);
 }return s;
}
function fitInk(points,data){
 const offsets=[],hits=[];
 for(let i=0;i<points.length;i++){
  const p=points[i],a=points[Math.max(0,i-2)],b=points[Math.min(points.length-1,i+2)],len=Math.hypot(b[0]-a[0],b[1]-a[1]);
  const n=len?[-(b[1]-a[1])/len,(b[0]-a[0])/len]:[0,0];
  let best=-1,shift=0;
  for(let d=-3;d<=3;d+=.125){
   // Find the source stroke centre, with a distance prior to prevent jumping
   // from a hidden rear curve onto a neighbouring foreground paw.
   const ink=grayInk(data,p[0]+n[0]*d-.5,p[1]+n[1]*d-.5),score=ink-.055*d*d;
   if(score>best){best=score;shift=d;}
  }
  if(best<.12)shift=0;
  offsets.push(shift);hits.push(best>=.12);
 }
 let sm=offsets;
 for(let pass=0;pass<4;pass++)sm=sm.map((v,i)=>(sm[Math.max(0,i-1)]+2*v+sm[Math.min(sm.length-1,i+1)])/4);
 return {points:points.map((p,i)=>{
  const a=points[Math.max(0,i-2)],b=points[Math.min(points.length-1,i+2)],len=Math.hypot(b[0]-a[0],b[1]-a[1]);
  return len?[p[0]-(b[1]-a[1])/len*sm[i],p[1]+(b[0]-a[0])/len*sm[i]]:p;
 }),hits:hits.filter(Boolean).length,total:points.length};
}
function curve(points){
 const p=new api.Path2D();p.moveTo(...points[0]);
 for(let i=0;i<points.length-1;i++){
  const a=points[Math.max(0,i-1)],b=points[i],c=points[i+1],d=points[Math.min(points.length-1,i+2)];
  p.bezierCurveTo(b[0]+(c[0]-a[0])/6,b[1]+(c[1]-a[1])/6,c[0]-(d[0]-b[0])/6,c[1]-(d[1]-b[1])/6,...c);
 }return p;
}
function closed(points){const p=curve(points);p.lineTo(45,43);p.lineTo(22,43);p.closePath();return p;}
function interpolated(keys,name,t){
 const u=t*7,k=Math.min(6,Math.floor(u)),v=u-k;
 return keys[k][name].map((p,j)=>p.map((x,c)=>{
  const vals=keys.map(q=>q[name][j][c]);
  const slope=i=>{if(!i||i===7)return 0;const a=vals[i]-vals[i-1],b=vals[i+1]-vals[i];return a*b<=0?0:2*a*b/(a+b);};
  return (2*v*v*v-3*v*v+1)*x+(v*v*v-2*v*v+v)*slope(k)+(-2*v*v*v+3*v*v)*vals[k+1]+(v*v*v-v*v)*slope(k+1);
 }));
}
async function create(){
 // r38's saved manual annotations are input search corridors only. Its renderer
 // and failure verdict are not reused; output is re-registered against sources.
 const annotationFile=path.join(root,'artifacts/grab-entry-20261017-r38/key-data.json');
 const annotation=JSON.parse(fs.readFileSync(annotationFile,'utf8'));
 const head=await api.loadImage(path.join(lockedDir,'masked-head-225.png')),hp=premultiply(head,api);
 const meta=JSON.parse(fs.readFileSync(path.join(lockedDir,'measurement.json'),'utf8'));
 const keys=[],sources=[],fits=[],sourceHashes={};
 for(let n=0;n<8;n++){
  const file=path.join(sourceDir,`0${n+1}.png`),im=await api.loadImage(file),data=pixels(im,api),tr=annotation.registration[n],key={};sources.push(im);sourceHashes[`0${n+1}.png`]=sha(file);
  for(const name of ['front','rear']){
   const rows=annotation.keys[n][name].map(([op,...v])=>[op,...v.map((x,c)=>(x-(c%2?tr.dy:tr.dx))/tr.factor)]);
   if(name==='rear'&&n>=1&&n<=4){
    // Correct the old corridor's backwards inner-foot segment. Source02-05
    // show an ascending inner edge and short rounded saddle, not a hook that
    // travels right, then back left. These are source-image annotations.
    const [x,y]=[[0,0],[50.4,85.5],[52,85.8],[51.7,85.4],[51.5,84.6]][n];
    rows[3]=['C',rows[3][1],rows[3][2],x-3,94,x-1.7,90];
    rows[4]=['C',x-.9,88,x-1,86.2,x,y];
    rows[5]=['C',x+.8,y-.2,54.1,y,55,y+1.2];
    rows[6][1]=55.7;rows[6][2]=y+3;
   }
   const fit=fitInk(flatten(rows),data);key[name]=fit.points.map(p=>[p[0]*tr.factor+tr.dx,p[1]*tr.factor+tr.dy]);fits.push({key:n,part:name,hit:fit.hits,total:fit.total});
  }
  keys.push(key);
 }
 const cache=new Map();
 function body(t){
  assert(Number.isFinite(t)&&t>=0&&t<=1);if(cache.has(t))return cache.get(t);
  const front=interpolated(keys,'front',t),rear=interpolated(keys,'rear',t),skin=closed(front).op(closed(rear),api.PathOp.Union);
  const c=api.createCanvas(375,375),g=c.getContext('2d');g.scale(225/96,225/96);g.translate(32,32);g.fillStyle='white';g.fill(skin);
  const ink=api.createCanvas(375,375),ig=ink.getContext('2d');ig.scale(225/96,225/96);ig.translate(32,32);ig.lineWidth=meta.pen.width;ig.lineCap=ig.lineJoin='round';ig.strokeStyle='white';ig.stroke(skin);
  ig.globalCompositeOperation='lighten';ig.stroke(curve(front));ig.resetTransform();ig.globalCompositeOperation='source-in';ig.fillStyle=`rgb(${meta.pen.rgb.join(',')})`;ig.fillRect(0,0,375,375);
  g.resetTransform();g.drawImage(ink,0,0);const result=premultiply(c,api);cache.set(t,result);return result;
 }
 function render(t,scale=1.5){const bd=body(t),a=new Float64Array(4),b=new Float64Array(4);return surface(api,96*scale,(x,y)=>{sample(bd,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);},32);}
 return {render,body,keys,sources,registration:annotation.registration,fits,sourceHashes,annotationSha256:sha(annotationFile)};
}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create();
 const contract={scope:'Eight-source body/foot redraw and geometry-only interpolation preview',authority:'user-body-drag/01.png through08.png for body poses; r33 masked head locked',sources:m.sourceHashes,annotationSha256:m.annotationSha256,headSha256:sha(path.join(lockedDir,'masked-head-225.png')),sourceFilesEdited:false,runtimeChanged:false,criteria:['All eight source poses, not just endpoint morphing','Opaque rear body emerging from behind front paws','No ghost/double ink, foot notch hooks or extra lobes','Locked face/hair/tips','Source/previous/new direct comparison'],limitations:['Existing partial-alpha compositor; native palette gate not applicable','Manual annotation corridors and fitted points are not independent verification truth','Not product-ready until full review and boundary checks']};
 fs.writeFileSync(path.join(dir,'contract.json'),JSON.stringify(contract,null,2));
 for(let n=0;n<8;n++)fs.writeFileSync(path.join(dir,`key-${n}.png`),m.render(n/7).toBuffer('image/png'));
 for(let page=0;page<4;page++){
  const c=api.createCanvas(1000,800),g=c.getContext('2d');g.imageSmoothingEnabled=false;
  for(let row=0;row<2;row++){
   const n=page*2+row,tr=m.registration[n],im=await api.loadImage(path.join(dir,`key-${n}.png`)),y=row*400;
   g.fillStyle='white';g.fillRect(0,y,1000,400);
   const scale=3;g.drawImage(m.sources[n],20+(32+tr.dx)*scale,y-110+(32+tr.dy)*scale,100*tr.factor*scale,100*tr.factor*scale);g.drawImage(im,520,y-110,480,480);
   g.fillStyle='white';g.fillRect(0,y,1000,34);g.fillStyle='#222';g.font='16px sans-serif';g.fillText(`Original 0${n+1} / head-width registered`,20,y+24);g.fillText('Source-fitted body / locked head',520,y+24);
  }fs.writeFileSync(path.join(dir,`source-pairs-${page}.png`),c.toBuffer('image/png'));
 }
 fs.writeFileSync(path.join(dir,'source-fit.json'),JSON.stringify({fits:m.fits,keys:m.keys,registration:m.registration,scriptSha256:sha(__filename),status:'AWAITING_DIRECT_REVIEW'},null,2));
 console.log(dir);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={create,dir,curve,closed,interpolated,smooth};

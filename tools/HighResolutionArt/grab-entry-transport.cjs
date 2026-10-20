'use strict';
// Diagnostic ONLY. Fixed source endpoints + landmark-guided texture transport.
// The aligned two-source blend is a hypothesis to inspect, not an accepted rig.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const dir=path.join(root,'artifacts/grab-entry-transport-20261017-r39');
const source=path.join(root,'artifacts/grab-contour-still-20261017-r33');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
// Input annotations in shared logical coordinates; not visual-test ground truth.
// No landmark is placed on the face, hair, or ribbon.
const landmarks=[
 ['upper-left',[16,50],[16,50]],['upper-center',[43,45],[43,45]],
 ['upper-right',[65,45],[56,53]],['left-neck',[17,68],[21,68]],
 ['far-front-bottom',[23,81],[27.06,81.48]],
 ['front-notch',[28,75.5],[34.21,71.75]],
 ['near-front-bottom',[42,86.4],[40.06,81.68]],
 ['front-right',[48.5,77.5],[49.6,70.55]],
 ['belly',[55,75],[45.93,89.73]],
 ['hind-left',[59.5,78],[51,97]],['hind-bottom',[65.3,84.1],[56.45,102.04]],
 ['hind-right',[70.4,78.2],[63.1,96]],['rump',[74.25,65.44],[63.5,81]],
 ['back',[71,50],[59,62]],
];
const kernel=(x,y)=>{const r=x*x+y*y;return r<1e-20?0:r*Math.log(r);};
function solve(a,b){
 const m=a.map((r,i)=>[...r,...b[i]]),n=a.length,k=b[0].length;
 for(let j=0;j<n;j++){
  let p=j;for(let i=j+1;i<n;i++)if(Math.abs(m[i][j])>Math.abs(m[p][j]))p=i;
  assert(Math.abs(m[p][j])>1e-12,'singular landmark system');[m[j],m[p]]=[m[p],m[j]];
  const d=m[j][j];for(let c=j;c<n+k;c++)m[j][c]/=d;
  for(let i=0;i<n;i++)if(i!==j){const f=m[i][j];for(let c=j;c<n+k;c++)m[i][c]-=f*m[j][c];}
 }return m.map(r=>r.slice(n));
}
function mapFor(t,end){
 const p=landmarks.map(([,a,b])=>a.map((v,k)=>(v+(b[k]-v)*t)/100)),n=p.length;
 const a=Array.from({length:n+3},()=>Array(n+3).fill(0)),b=Array.from({length:n+3},()=>[0,0]);
 for(let i=0;i<n;i++){
  for(let j=0;j<n;j++)a[i][j]=kernel(p[i][0]-p[j][0],p[i][1]-p[j][1]);
  a[i][n]=a[n][i]=1;a[i][n+1]=a[n+1][i]=p[i][0];a[i][n+2]=a[n+2][i]=p[i][1];
  b[i]=landmarks[i][end+1].map((v,k)=>v/100-p[i][k]);
 }
 const w=solve(a,b);
 return (x,y,out)=>{const u=x/100,v=y/100;out[0]=x+100*(w[n][0]+w[n+1][0]*u+w[n+2][0]*v);out[1]=y+100*(w[n][1]+w[n+1][1]*u+w[n+2][1]*v);
  for(let i=0;i<n;i++){const k=100*kernel(u-p[i][0],v-p[i][1]);out[0]+=k*w[i][0];out[1]+=k*w[i][1];}return out;
 };
}
async function create(){
 const common=await require('./current-motion.cjs').create(),rig=common.s.postures.open.rig;
 const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
 const body0=rig.render(rig.pose(0),{includeHead:false,extraParts:{haunch:rootedHaunch(0),nearHind:connectedHind(q=>q)}});
 const body1=await api.loadImage(path.join(source,'body-padded-375.png'));
 const head0=common.s.rigs.open.parts.head,head1=await api.loadImage(path.join(source,'masked-head-225.png'));
 const bd=[premultiply(body0,api),premultiply(body1,api)],hd=[premultiply(head0,api),premultiply(head1,api)];
 function render(t,scale=1.5){
  assert(t>=0&&t<=1&&Number.isFinite(t));const maps=[mapFor(t,0),mapFor(t,1)];
  const a=new Float64Array(4),b=new Float64Array(4),h=new Float64Array(4),q=new Float64Array(4),xy=[0,0];
  // Fixed endpoints emerge from zero displacement and source weighting. There
  // is no t===0/1 rendered-image substitution to conceal a discontinuity.
  return surface(api,scale*96,(x,y)=>{
   maps[0](x,y,xy);sample(bd[0],225,xy[0]*225/96-.5,xy[1]*225/96-.5,a);
   maps[1](x,y,xy);sample(bd[1],375,(xy[0]+32)*225/96-.5,(xy[1]+32)*225/96-.5,b);
   for(let k=0;k<4;k++)a[k]=a[k]*(1-t)+b[k]*t;
   // Only the historical exterior-head cleanup differs between the locked
   // endpoints; inspect its exact difference mask, never move head texels.
   sample(hd[0],225,x*225/96-.5,y*225/96-.5,h);sample(hd[1],225,x*225/96-.5,y*225/96-.5,q);
   for(let k=0;k<4;k++)h[k]=h[k]*(1-t)+q[k]*t;
   return over(a,h);
  },32);
 }
 return {render,common,head0,head1,body0,body1};
}
function diff(a,b){const x=pixels(a,api),y=pixels(b,api);assert.equal(x.length,y.length);let changed=0,maxVisibleDelta=0;
 for(let i=0;i<x.length;i+=4){if(x.subarray(i,i+4).some((v,k)=>v!==y[i+k]))changed++;
  for(const bg of [0,255])for(let k=0;k<3;k++)maxVisibleDelta=Math.max(maxVisibleDelta,Math.abs(x[i+k]*x[i+3]/255+bg*(1-x[i+3]/255)-y[i+k]*y[i+3]/255-bg*(1-y[i+3]/255)));
 }return {changedPixels:changed,maxVisibleDelta};
}
function sheet(images,labels,file){
 const c=api.createCanvas(images.length*240,800),g=c.getContext('2d');g.imageSmoothingEnabled=false;
 for(let j=0;j<images.length;j++)for(const [k,bg]of ['white','#252a31'].entries()){
  const x=j*240,y=k*400;g.fillStyle=bg;g.fillRect(x,y,240,400);g.fillStyle=k?'white':'#222';g.font='14px sans-serif';g.fillText(labels[j],x+8,y+19);g.drawImage(images[j],x,y+20);
  g.drawImage(images[j],65,148,98,60,x+22,y+275,196,120);
 }fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));
}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create(),sourceHashes=Object.fromEntries(['masked-head-225.png','body-padded-375.png'].map(f=>[f,sha(path.join(source,f))]));
 const base=api.createCanvas(240,240);base.getContext('2d').drawImage(m.common.render('idle',0),24,24);
 const held=await api.loadImage(path.join(source,'candidate.png')),first=m.render(0),last=m.render(1);
 const endpoints={start:diff(first,base),end:diff(last,held)};
 assert.equal(endpoints.start.changedPixels,0);assert.equal(endpoints.end.changedPixels,0);
 const eps=[.000001,.0001,.001,.01],near=[];
 for(const e of eps)near.push({epsilon:e,start:diff(first,m.render(e)),end:diff(last,m.render(1-e))});
 const images=[];for(let i=0;i<=8;i++){const im=m.render(i/8);fs.writeFileSync(path.join(dir,`probe-${i}.png`),im.toBuffer('image/png'));images.push(await api.loadImage(path.join(dir,`probe-${i}.png`)));}
 sheet([base,first,m.render(.01)],['Current idle','Transport t=0','Transport t=.01'],'start.png');
 sheet([m.render(.99),last,held],['Transport t=.99','Transport t=1','Accepted held r33'],'end.png');
 for(let i=0;i<9;i+=3)sheet(images.slice(i,i+3),images.slice(i,i+3).map((_,j)=>`Diagnostic t=${(i+j)/8}`),`probes-${i}.png`);
 let headChanged=0;const bounds=[225,225,-1,-1],h0=pixels(m.head0,api),h1=pixels(m.head1,api);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;if(h0.subarray(i,i+4).some((v,k)=>v!==h1[i+k])){headChanged++;bounds[0]=Math.min(bounds[0],x);bounds[1]=Math.min(bounds[1],y);bounds[2]=Math.max(bounds[2],x);bounds[3]=Math.max(bounds[3],y);}}
 for(const [f,h]of Object.entries(sourceHashes))assert.equal(sha(path.join(source,f)),h);
 fs.writeFileSync(path.join(dir,'report.json'),JSON.stringify({status:'DIAGNOSTIC_NOT_VISUALLY_ACCEPTED',method:'Thin-plate anatomical landmark displacement plus aligned premultiplied source blend',sourceHashes,endpoints,near,headSourceDifference:{pixels:headChanged,bounds},landmarks,scriptSha256:sha(__filename),scope:'Nine isolated probes at 1.5x; not a dense sequence or product motion',runtimeChanged:false,limitations:['Topology change is not solved by landmark alignment. Inspect ghost contours and emerging far hind leg.','Head source texels never move. Source masks differ; not a claim of all output head pixels invariant.','Native palette/binary-alpha gate not run: existing partial-alpha compositor.']},null,2));
 console.log(JSON.stringify({dir,endpoints,near},null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={create,mapFor,landmarks,diff};

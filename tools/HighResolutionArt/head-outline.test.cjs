'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const {create}=require('./connected-families.cjs'),{pixels}=require('./sampling.cjs');
const ready=Promise.all([create(),create({outlineRecovery:true})]);
test('source heads, masters and body parts stay byte-identical',async()=>{
 const [a,b]=await ready;for(const eye of ['open','closed']){
  for(const key of ['head','body'])assert.deepEqual(pixels(a.s.rigs[eye].parts[key],a.s.api),pixels(b.s.rigs[eye].parts[key],b.s.api));
  assert.deepEqual(pixels(a.s.masters[eye],a.s.api),pixels(b.s.masters[eye],b.s.api));
 }
});
test('outline correction keeps alpha and contrast cap across composer paths',async()=>{
 const [a,b]=await ready;for(const args of [['track',0,{roll:-.2}],['run',.27,{}],['sit',.8,{}],['hunt',.6,{}],['jump',.4,{}],['eat',.8,{}],['happy',.5,{}],['sleep',1,{}]]){
  const x=pixels(a.render(...args),a.s.api),y=pixels(b.render(...args),b.s.api);let n=0;
  for(let i=0;i<x.length;i+=4){assert.equal(x[i+3],y[i+3],args[0]);for(let c=0;c<3;c++){assert.ok(y[i+c]<=x[i+c]);assert.ok(x[i+c]-y[i+c]<=26);if(x[i+c]!==y[i+c])n++;}}
  assert.ok(n>0,args[0]+' correction is connected');
 }
});
function chinWidth(d,w,scale,roll,sourceX){
 const co=Math.cos(roll),sn=Math.sin(roll),values=[];
 const l=(x,y)=>{x=Math.max(0,Math.min(w-2,x));y=Math.max(0,Math.min(w-2,y));const ix=Math.floor(x),iy=Math.floor(y),fx=x-ix,fy=y-iy;let sum=0;for(const [dx,dy,weight]of [[0,0,(1-fx)*(1-fy)],[1,0,fx*(1-fy)],[0,1,(1-fx)*fy],[1,1,fx*fy]]){const at=((iy+dy)*w+ix+dx)*4;sum+=weight*(d[at]+d[at+1]+d[at+2])/3;}return 255-sum;};
 for(let sy=145;sy<=162;sy+=.05){const hx=(sourceX+.5)*96/225-43,hy=(sy+.5)*96/225-66;values.push(l((43+hx*co-hy*sn+16)*scale-.5,(66+hx*sn+hy*co+16)*scale-.5));}
 const peak=Math.max(...values),center=values.indexOf(peak),half=peak/2;let left=center,right=center;while(left>0&&values[left]>half)left--;while(right<values.length-1&&values[right]>half)right++;
 return ((right-left)- (half-values[left])/(values[left+1]-values[left])-(half-values[right])/(values[right-1]-values[right]))*.05*96/225*scale;
}
test('chin half-contrast width does not meaningfully grow across saved rotation grid',async()=>{
 const [a]=await ready,{api,root}=a.s,dir=path.join(root,'artifacts/head-outline-recovery-20261017-r24'),manifest=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'))),{create:upright}=require('./upright.cjs');let max=0,at=null;
 for(const scale of manifest.scales)for(const eye of ['open','closed']){
  const rig=upright(a.s.masters[eye],api,{size:96*scale,padding:16,gaze:eye==='open',reachRun:true,fixedRunRoots:true,connected:true});
  for(const f of manifest.frames.filter(f=>f.scale===scale&&f.eye===eye)){
   const im=rig.render({roll:f.deg*Math.PI/180}),old=pixels(im,api),current=pixels(await api.loadImage(path.join(dir,f.name+'.png')),api);
   for(const sx of [62,66,70,74,78,82,86,90]){const delta=chinWidth(current,im.width,scale,f.deg*Math.PI/180,sx)-chinWidth(old,im.width,scale,f.deg*Math.PI/180,sx);if(delta>max){max=delta;at={frame:f.name,sx};}}
  }
 }
 console.log(JSON.stringify({maximumChinWidthIncreaseInOutputPixels:max,at}));assert.ok(max<.15,'No more than .15 output pixel FWHM change');
});

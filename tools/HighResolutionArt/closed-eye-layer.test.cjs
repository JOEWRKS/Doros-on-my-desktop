'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const {api,root}=require('./closed-eye-mask-review.cjs');
const {pixels}=require('./sampling.cjs'),{transfer,mask,lashRepair}=require('./closed-eye-layer.cjs');
const ready=(async()=>{
 const master=require('./canonical-fixes.cjs').render(await api.loadImage(root+'/tools/HighResolutionArt/input/canonical-225.png'),api);
 const authored=await api.loadImage(root+'/src/Dororong.App/Assets/dororong-closed-eyes.png');
 return {master,authored,result:transfer(master,null,authored,api)};
})();
test('closed-eye candidate changes nothing outside face apertures and named lash repair',async()=>{
 const {master,result}=await ready,a=pixels(master,api),b=pixels(result,api),m=pixels(mask(api),api),repair=new Set(lashRepair.map(p=>p.join(',')));
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4;assert.equal(b[i+3],a[i+3]);
  if(!m[i+3]&&!repair.has(`${x},${y}`))for(let k=0;k<4;k++)assert.equal(b[i+k],a[i+k],`${x},${y}`);
 }
});
test('foreground pink bangs and central strand ink are not replaced by96px hair',async()=>{
 const {master,result}=await ready,a=pixels(master,api),b=pixels(result,api),repair=new Set(lashRepair.map(p=>p.join(',')));
 let count=0;
 for(let y=90;y<125;y++)for(let x=30;x<108;x++){
  const i=(y*225+x)*4;
  if(a[i]>a[i+1]+35&&a[i]>a[i+2]+8&&!repair.has(`${x},${y}`)){count++;for(let k=0;k<4;k++)assert.equal(b[i+k],a[i+k],`pink hair ${x},${y}`);}
 }
 assert.ok(count>500);
 for(const [x,y] of [[65,106],[66,111],[69,117],[72,120],[74,121],[90,105],[93,110]]){
  const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`strand ${x},${y}`);
 }
});
test('former lash spur and dangling brow are cleared without adding new eyelid paths',async()=>{
 const {result}=await ready,d=pixels(result,api);
 for(const [x,y] of [[51,110],[52,111],[55,115],[81,124],[84,121],[92,117]]){
  const i=(y*225+x)*4;assert.ok(Math.min(...d.slice(i,i+3))>210,`residual ink ${x},${y}`);
 }
 for(const [x,y] of [[44,108],[45,109]]){const i=(y*225+x)*4;assert.ok(d[i]>180&&d[i]>d[i+1]+30,`hair under old lash ${x},${y}`);}
});
test('candidate keeps sampled authored closed-eye line interiors, not new curve geometry',async()=>{
 const {authored,result}=await ready;
 // Independent canvas resampling is not the oracle: directly evaluate source
 // texels using the established sampler at named points inside the two lids.
 const {premultiply,sample,put}=require('./sampling.cjs'),src=premultiply(authored,api),v=new Float64Array(4),expected=new Uint8ClampedArray(4),d=pixels(result,api);
 for(const [x,y] of [[47,124],[52,125],[56,125],[84,130],[91,132],[98,131]]){
  sample(src,96,(x+.5)*96/225-.5,(y+.5)*96/225-.5,v);put(expected,0,v);
  assert.deepEqual(d.slice((y*225+x)*4,(y*225+x)*4+4),expected,`${x},${y}`);
 }
});

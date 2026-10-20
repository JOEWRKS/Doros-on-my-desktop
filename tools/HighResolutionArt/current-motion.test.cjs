'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create,sequence}=require('./current-motion.cjs'),{pixels,sample}=require('./sampling.cjs');
const ready=create(),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
test('every current motion shares exactly the accepted r27 head layers',async()=>{
 const m=await ready;for(const eye of ['open','closed']){
  const source=await m.s.api.loadImage(path.join(m.s.root,`artifacts/pointed-hair-tip-20261017-r27/${eye}-head-225.png`));
  assert.deepEqual(pixels(m.s.rigs[eye].parts.head,m.s.api),pixels(source,m.s.api));
  assert.deepEqual(pixels(m.s.postures[eye].head,m.s.api),pixels(source,m.s.api));
 }
});
test('neutral motion endpoints do not switch to a different body or head',async()=>{
 const m=await ready,p=(k,t,o)=>pixels(m.render(k,t,o),m.s.api);
 for(const eye of ['open','closed']){const base=p('idle',0,{eye});for(const [k,t,o]of [['run',0,{amount:0}],['walk',.4,{amount:0}],['sit',0],['rise',1],['hunt',0],['hunt',2.6],['jump',.86],['track',0]])assert.deepEqual(p(k,t,{eye,...o}),base,k);}
 const closed=p('idle',0,{eye:'closed'});for(const [k,t]of [['sleep',0],['wake',1],['eat',1.7],['happy',0],['happy',2.16]])assert.deepEqual(p(k,t),closed,k);
 assert.deepEqual(p('body-click',1),p('idle',0),'click release');assert.deepEqual(p('body-click',0,{press:0}),p('idle',0),'press entry');
});
test('sleep face and pointed hair remain rigid; wake reverses the same path',async()=>{
 const m=await ready,empty=m.s.api.createCanvas(225,225);
 for(let n=0;n<=32;n++){
  const t=n/32,u=t*t*(3-2*t),frame=m.render('sleep',t),a=pixels(frame,m.s.api),b=pixels(m.compose(empty,'closed',{dy:8*u}),m.s.api);
  const h=new Float64Array(4);let checked=0;
  for(let y=0;y<192;y++)for(let x=0;x<192;x++){
   const sy=((y+.5)/1.5-16-8*u)*225/96;
   if(sy>153)continue;let opaque=true;
   for(const dy of [.25,.75])for(const dx of [.25,.75]){sample(m.heads.closed,225,((x+dx)/1.5-16)*225/96-.5,((y+dy)/1.5-16-8*u)*225/96-.5,h);if(h[3]<254.999)opaque=false;}
   if(opaque){const i=(y*192+x)*4;assert.deepEqual(a.slice(i,i+4),b.slice(i,i+4),'protected head '+n);checked++;}
  }assert.ok(checked>2500);
  assert.deepEqual(a,pixels(m.render('wake',1-t),m.s.api));
 }
});
test('approved r23 body silhouette is retained at the sleep endpoint',async()=>{
 const m=await ready,rig=require('./accepted-sleep.cjs').create(m.s.postures.closed.rig,m.s.api,m.s.measurement),actual=pixels(rig.render(1).body,m.s.api),expected=pixels(await m.s.api.loadImage(path.join(m.s.root,'artifacts/high-resolution-sleep-still-20261017-r23/body-225.png')),m.s.api);
 // Identical cubic boundary, split into limb contours: five edge samples
 // straddle alpha 128 after the union. Reject opaque contour displacement.
 let silhouette=0;for(let i=0;i<actual.length;i+=4)if((actual[i+3]>=128)!==(expected[i+3]>=128)){
  silhouette++;assert.ok(Math.abs(actual[i+3]-expected[i+3])<=40,'opaque contour displacement');
  assert.ok(actual[i+3]>90&&actual[i+3]<160&&expected[i+3]>90&&expected[i+3]<160,'not an antialias edge');
 }
 assert.ok(silhouette<=5,'r23 silhouette changed: '+silhouette);
 const end=pixels(m.render('sleep',1),m.s.api),near=pixels(m.render('sleep',1-1e-6),m.s.api);let strong=0;
 const discontinuity=(a,b)=>{let strong=0;for(let i=0;i<a.length;i+=4)for(let c=0;c<4;c++){const av=c===3?a[i+3]:a[i+c]*a[i+3]/255,bv=c===3?b[i+3]:b[i+c]*b[i+3]/255;if(Math.abs(av-bv)>24)strong++;}return strong;};
 strong=discontinuity(end,near);assert.ok(strong<30,'last frame discontinuity '+strong);
 const start=discontinuity(pixels(m.render('sleep',0),m.s.api),pixels(m.render('sleep',1e-6),m.s.api));assert.ok(start<30,'entry ink pop '+start);
});
test('final exported frames are hashed, padded and individually indexed for review',async()=>{
 const m=await ready,dir=path.join(m.s.root,'artifacts/accepted-motion-connection-20261017-r28'),manifest=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'))),index=JSON.parse(fs.readFileSync(path.join(dir,'review-index.json')));
 assert.equal(manifest.runtimeSelected,false);assert.deepEqual(manifest.scenarios.map(s=>s.name),sequence.map(s=>s.name));
 assert.deepEqual(index.flatMap(s=>s.frames),manifest.frames.map(f=>f.name));
 for(const [f,h]of Object.entries(manifest.sources))assert.equal(sha(fs.readFileSync(path.join(m.s.root,f))),h,f);
 for(const f of manifest.frames){const bytes=fs.readFileSync(path.join(dir,f.name+'.png'));assert.equal(sha(bytes),f.sha256);const im=await m.s.api.loadImage(bytes),p=pixels(im,m.s.api);assert.equal(im.width,192);for(let k=0;k<192;k++)for(const [x,y]of [[k,0],[k,191],[0,k],[191,k]])assert.equal(p[(y*192+x)*4+3],0,f.name+' clipped');}
});

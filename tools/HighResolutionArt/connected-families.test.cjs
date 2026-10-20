'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create}=require('./connected-families.cjs'),{pixels}=require('./sampling.cjs');
const {out}=require('./connected-families-review.cjs');
const ready=create(),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
test('neutral joins use identical pixels rather than switching bodies',async()=>{
 const m=await ready,p=(k,t,o)=>pixels(m.render(k,t,o),m.s.api);
 for(const eye of ['open','closed']){
  const base=p('idle',0,{eye});
  for(const [k,t,o]of [['run',0,{amount:0}],['walk',.4,{amount:0}],['sit',0],['rise',1],['hunt',0],['hunt',2.6],['jump',.86],['track',0]])assert.deepEqual(p(k,t,{eye,...o}),base,k);
 }
 const closed=p('idle',0,{eye:'closed'});
 for(const [k,t]of [['sleep',0],['wake',1],['eat',0],['eat',1.7],['happy',0],['happy',2.16]])assert.deepEqual(p(k,t),closed,k);
});
test('all48 accepted full-run frames remain pixel identical to r18',async()=>{
 const m=await ready;
 for(let n=0;n<48;n++)assert.deepEqual(pixels(m.render('run',n/48),m.s.api),pixels(await m.s.api.loadImage(path.join(m.s.root,`artifacts/high-resolution-motion-20261017-r18/run-open-full-${String(n).padStart(2,'0')}.png`)),m.s.api),'r18 phase '+n);
});
test('closed face source is unchanged and mouth motion stays in its local region',async()=>{
 const m=await ready,h=await m.s.api.loadImage(path.join(m.s.root,'artifacts/high-resolution-cheek-20261016-r15/head-layer-225.png'));
 assert.deepEqual(pixels(m.s.rigs.closed.parts.head,m.s.api),pixels(h,m.s.api));
 const rest=pixels(m.render('eat',0),m.s.api);let changes=0;
 for(const t of [.14,.24,.65,.9,1.2,1.7]){const d=pixels(m.render('eat',t),m.s.api);for(let i=0;i<d.length;i+=4){if(d.slice(i,i+4).some((v,j)=>v!==rest[i+j])){const x=i/4%192,y=Math.floor(i/4/192);assert.ok(x>=59&&x<=78&&y>=106&&y<=123,`mouth changed outside its region: ${x},${y}`);changes++;}}}
 assert.ok(changes>20);
});
test('walk/run blend endpoints use the same complete rig at every phase',async()=>{
 const m=await ready;
 for(let n=0;n<48;n++){
  const p=n/48,a=pixels(m.render('walk',p,{runMix:1}),m.s.api),b=pixels(m.render('run',p),m.s.api);
  assert.deepEqual(a,b,'full blend phase '+n);
  const almost=pixels(m.render('walk',p,{runMix:1-1e-8}),m.s.api);
  let total=0;for(let i=0;i<a.length;i++)total+=Math.abs(a[i]-almost[i]);
  assert.ok(total<50,'blend cannot swap a paw at its endpoint: '+n+' / '+total);
 }
});
test('shared attachment retains rigid distal paw controls in sit/hunt/sleep',async()=>{
 const m=await ready,distances=[];
 const original=[[163.7,175.4],[162.8,181],[164.9,183.3],[165.2,186.2],[165.7,191.5],[158.5,196.8],[153,197.1],[148.2,197.3],[141.2,188.5],[139.1,181.2],[137.8,177.2],[139.1,173.8],[138.1,173.1]].map(p=>p.map(v=>v*96/225));
 for(const eye of ['open','closed']){const rig=m.s.postures[eye].rig,render=rig.render;rig.render=(p,o)=>{const f=o?.extraParts?.nearHind?.fill;if(f){const controls=[f[1].slice(-2),...f.slice(2,6).flatMap(v=>[v.slice(1,3),v.slice(3,5),v.slice(5,7)])];for(let i=1;i<original.length;i++)distances.push(Math.abs(Math.hypot(...controls[i].map((v,j)=>v-controls[0][j]))-Math.hypot(...original[i].map((v,j)=>v-original[0][j]))));}return render(p,o);};}
 for(const kind of ['sit','hunt','sleep'])for(const t of [0,.25,.5,.75,1])m.render(kind,kind==='hunt'?t*2.6:t);
 assert.ok(distances.length>100);assert.ok(Math.max(...distances)<1e-10);
});
test('saved transition frames match source hashes and every frame has a review slot',async()=>{
 const m=await ready,manifest=JSON.parse(fs.readFileSync(path.join(out,'manifest.json'))),index=JSON.parse(fs.readFileSync(path.join(out,'review-index.json')));
 assert.equal(manifest.runtimeSelected,false);assert.equal(manifest.fps,60);assert.equal(manifest.scenarios.length,9);
 assert.deepEqual(index.flatMap(p=>p.frames),manifest.frames.map(f=>f.name));
 for(const [f,h]of Object.entries(manifest.sources))assert.equal(sha(fs.readFileSync(path.join(m.s.root,f))),h,f);
 for(const f of manifest.frames){const b=fs.readFileSync(path.join(out,f.name+'.png'));assert.equal(sha(b),f.sha256);const im=await m.s.api.loadImage(b);assert.equal(im.width,192);assert.equal(im.height,192);const d=pixels(im,m.s.api);for(let k=0;k<192;k++)for(const [x,y]of [[k,0],[k,191],[0,k],[191,k]])assert.equal(d[(y*192+x)*4+3],0,f.name+' padding');}
});

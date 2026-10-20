'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {setup,out}=require('./accepted-motion.cjs'),{pixels,premultiply}=require('./sampling.cjs'),{assemblyFor}=require('./parts.cjs');
const ready=setup(),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
test('accepted r15 face/body master and semantic head are exact before animation',async()=>{
 const s=await ready;assert.deepEqual(pixels(s.masters.closed,s.api),pixels(s.approvedMaster,s.api));
 const head=await s.api.loadImage(path.join(s.root,'artifacts/high-resolution-cheek-20261016-r15/head-layer-225.png'));
 assert.deepEqual(pixels(s.rigs.closed.parts.head,s.api),pixels(head,s.api));
 for(const eye of ['open','closed'])assert.strictEqual(assemblyFor(s.masters[eye]).head,s.rigs[eye].parts.head);
 assert.deepEqual(pixels(s.rigs.open.parts.body,s.api),pixels(s.rigs.closed.parts.body,s.api));
});
test('closed head ignores gaze displacement and source layers are immutable through motion',async()=>{
 const s=await ready,rig=s.rigs.closed,before=pixels(rig.parts.head,s.api);
 for(const roll of [-20,0,14].map(v=>v*Math.PI/180))assert.deepEqual(pixels(rig.render({roll,eyeX:.85,eyeY:.65}),s.api),pixels(rig.render({roll}),s.api));
 rig.render({phase:.4,amount:1,walking:false});s.postures.closed.render(1);
 assert.deepEqual(pixels(rig.parts.head,s.api),before);
});
test('neutral tracking/gait, sitting entry and happy endpoints share accepted frame',async()=>{
 const s=await ready,h=require('./happy.cjs');assert.deepEqual(h.times,[0,.06,.46,1.10,1.64,2.10,2.16]);assert.deepEqual(h.degrees,[0,0,-8,8,-4,0,0]);
 for(const eye of ['open','closed']){
  const r=s.rigs[eye],a=pixels(r.still(),s.api);
  for(const im of [r.render(),r.render({amount:0,phase:.7}),s.postures[eye].render(0)])assert.deepEqual(pixels(im,s.api),a);
 }
 const r=s.rigs.closed;for(const age of [0,2.16])assert.deepEqual(pixels(r.render({roll:h.roll(age)}),s.api),pixels(r.still(),s.api));
});
test('first nonzero motion does not swap the accepted body silhouette',async()=>{
 const s=await ready;for(const eye of ['open','closed']){const r=s.rigs[eye],a=premultiply(r.still(),s.api);
  for(const im of [r.render({roll:1e-6}),r.render({amount:1e-6,phase:.3}),s.postures[eye].render(1/64)]){const b=premultiply(im,s.api);let e=0;for(let i=0;i<a.length;i++)e+=Math.abs(a[i]-b[i]);assert.ok(e/a.length<.1,`${eye} rest discontinuity`);}
 }
});
test('604 saved frames decode, match hashes, source pins and transparent padding',async()=>{
 const s=await ready,m=JSON.parse(fs.readFileSync(path.join(out,'manifest.json'),'utf8'));assert.equal(m.frames.length,604);assert.equal(m.runtimeSelected,false);
 for(const [p,h] of Object.entries(m.sources))assert.equal(sha(fs.readFileSync(path.join(s.root,p))),h,`stale ${p}`);
 const index=JSON.parse(fs.readFileSync(path.join(out,'review-index.json'),'utf8')).filter(p=>!p.detail);
 assert.deepEqual(index.flatMap(p=>p.frames),m.frames.map(f=>f.name),'every frame needs numbered review coverage');
 for(const f of m.frames){const file=path.join(out,f.name+'.png');assert.equal(sha(fs.readFileSync(file)),f.sha256);const im=await s.api.loadImage(file);assert.equal(im.width,192);assert.equal(im.height,192);const d=pixels(im,s.api);
  for(let k=0;k<192;k++)for(const [x,y] of [[k,0],[k,191],[0,k],[191,k]])assert.equal(d[(y*192+x)*4+3],0,`clipped ${f.name}`);
  if(/^(walk|run)-/.test(f.name))for(const [lx,ly] of [[35,67],[55,67],[64,69]])assert.ok(d[(Math.floor(24+ly*1.5)*192+Math.floor(24+lx*1.5))*4+3]>=250,`socket ${f.name}`);
 }
 for(const eye of ['open','closed']){
  for(const [kind,count] of Object.entries({walk:32,run:48,track:25,breathe:33,sit:65,rise:65}))assert.equal(m.frames.filter(f=>f.name.startsWith(kind+'-'+eye+'-')).length,count);
  for(let n=0;n<=64;n++)assert.equal(m.frames.find(f=>f.name===`sit-${eye}-${String(n).padStart(2,'0')}`).sha256,m.frames.find(f=>f.name===`rise-${eye}-${String(64-n).padStart(2,'0')}`).sha256);
 }
});

'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {setup}=require('./accepted-motion.cjs'),{pixels}=require('./sampling.cjs');
const {rigidHindMap,rootedHind,rootedHaunch}=require('./motion-polish.cjs'),{out}=require('./rooted-run-review.cjs');
const ready=Promise.all([setup({polished:true,fixedRunRoots:true}),setup({polished:true})]);
test('folded distal contour uses original control points under rigid rotation',async()=>{
 const [s]=await ready,source=[[163.7,175.4],[162.8,181],[164.9,183.3],[165.2,186.2],[165.7,191.5],[158.5,196.8],[153,197.1],[148.2,197.3],[141.2,188.5],[139.1,181.2],[137.8,177.2],[139.1,173.8],[138.1,173.1]].map(p=>p.map(v=>v*96/225));
 for(const amount of [.01,.25,.5,.75,1])for(let n=0;n<96;n++){
  const map=rigidHindMap(2,n/96,amount,-1),p=rootedHind(2,n/96,amount,-1,s.api,s.postures.open.rig.pose(0));
  const controls=[p.fill[1].slice(-2),...p.fill.slice(2,6).flatMap(v=>[v.slice(1,3),v.slice(3,5),v.slice(5,7)])];
  assert.deepEqual(controls,source.map(map));
  for(let i=1;i<source.length;i++){const a=map(source[0]),b=map(source[i]);assert.ok(Math.abs(Math.hypot(a[0]-b[0],a[1]-b[1])-Math.hypot(source[0][0]-source[i][0],source[0][1]-source[i][1]))<1e-10);}
  const P=p.fill[0].slice(1),c=p.fill[1].slice(1);assert.ok(c[1]>=P[1]&&c[3]>=c[1]&&c[5]>=c[3],'connector must descend without an extra lobe');
 }
});
test('rump outer subdivision and connection tangent are fixed at every phase',async()=>{
 const [s]=await ready;
 for(let n=0;n<48;n++){
  const bob=-1.6*Math.sin(2*Math.PI*(n/48-.08))**2,p=rootedHind(2,n/48,1,bob,s.api,s.postures.open.rig.pose(0));
  assert.deepEqual(p.fill[0].slice(1),rootedHaunch(bob).fill[3].slice(-2));
  const originalCurve=[[74.5,56],[76,62],[74,69],[69.5,74.5]],half=[0,1].map(i=>(originalCurve[0][i]+3*originalCurve[1][i]+3*originalCurve[2][i]+originalCurve[3][i])/8+(i===1?bob:0));
  assert.deepEqual(p.fill[0].slice(1),half);
  const v=[p.fill[1][1]-p.fill[0][1],p.fill[1][2]-p.fill[0][2]];assert.ok(Math.abs(v[0]/v[1]-(-5.25/19.125))<1e-10);
 }
});
test('all168 saved frames preserve face pixels and source head; sit/walk unchanged',async()=>{
 const [s,old]=await ready,m=JSON.parse(fs.readFileSync(path.join(out,'manifest.json')));
 assert.equal(m.frames.length,168);
 for(const eye of ['open','closed'])assert.deepEqual(pixels(s.rigs[eye].parts.head,s.api),pixels(old.rigs[eye].parts.head,s.api));
 for(const f of m.frames){
  const im=await s.api.loadImage(path.join(out,f.name+'.png')),d=pixels(im,s.api),before=pixels(old.rigs[f.eye].render(f.parameters),s.api);
  // This rectangle contains the face/fringe/cheeks. The right-hand rump is
  // intentionally outside it; comparing whole upper rows confounds the two.
  for(let y=0;y<125;y++)assert.deepEqual(d.subarray(y*192*4,(y*192+125)*4),before.subarray(y*192*4,(y*192+125)*4),f.name+' face/hair row '+y);
 }
 for(const eye of ['open','closed']){
  for(let n=0;n<32;n++)assert.deepEqual(pixels(s.rigs[eye].render({amount:1,phase:n/32,walking:true}),s.api),pixels(old.rigs[eye].render({amount:1,phase:n/32,walking:true}),s.api));
  for(let n=0;n<=64;n++)assert.deepEqual(pixels(s.postures[eye].render(n/64),s.api),pixels(await s.api.loadImage(path.join(s.root,`artifacts/high-resolution-motion-20261017-r17/sit-${eye}-${String(n).padStart(2,'0')}.png`)),s.api));
 }
});
test('saved files, source hashes, review coverage and transparent borders agree',async()=>{
 const [s]=await ready,m=JSON.parse(fs.readFileSync(path.join(out,'manifest.json'))),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
 assert.equal(m.runtimeSelected,false);assert.equal(m.cycleSeconds,.56);
 for(const [f,h]of Object.entries(m.sources))assert.equal(sha(fs.readFileSync(path.join(s.root,f))),h,f);
 const index=JSON.parse(fs.readFileSync(path.join(out,'review-index.json')));assert.deepEqual(index.flatMap(p=>p.frames),m.frames.map(f=>f.name));
 for(const f of m.frames){const b=fs.readFileSync(path.join(out,f.name+'.png'));assert.equal(sha(b),f.sha256);const im=await s.api.loadImage(b);assert.equal(im.width,192);assert.equal(im.height,192);const d=pixels(im,s.api);for(let k=0;k<192;k++)for(const [x,y]of [[0,k],[191,k],[k,0],[k,191]])assert.equal(d[(y*192+x)*4+3],0);}
});

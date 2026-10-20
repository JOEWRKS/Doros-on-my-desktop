'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {setup}=require('./accepted-motion.cjs'),{pixels}=require('./sampling.cjs');
const {runMap,runAngle,roots,cycleSeconds,seatedHind}=require('./motion-polish.cjs'),{out}=require('./polished-motion-review.cjs');
const ready=setup({polished:true});
test('exposed run limbs rotate without changing segment length or width',()=>{
 for(let leg=0;leg<4;leg++)for(let n=0;n<=96;n++)for(const amount of [.25,.5,1]){
  const [x,y]=roots[leg],pts=[[x-3,y+1],[x+3,y+1],[x,y+11]],map=runMap(leg,n/96,amount,-1);
  for(let i=0;i<pts.length;i++)for(let j=i+1;j<pts.length;j++){const a=map(pts[i]),b=map(pts[j]);assert.ok(Math.abs(Math.hypot(a[0]-b[0],a[1]-b[1])-Math.hypot(pts[i][0]-pts[j][0],pts[i][1]-pts[j][1]))<1e-10);}
 }
});
test('run reaches forward, loops continuously and uses a .56second cadence',()=>{
 assert.equal(cycleSeconds,.56);assert.ok(cycleSeconds<1.6/2);
 for(let leg=0;leg<4;leg++){assert.ok(Math.abs(runAngle(leg,0)-runAngle(leg,1))<1e-10);let reach=false;
  for(let i=0;i<48;i++){const [x,y]=roots[leg],p=runMap(leg,i/48,1,0)([x,y+11]);if(p[0]<(x+(leg===3?-10:0))-7)reach=true;}assert.ok(reach);}
});
test('seated knee envelope no longer protrudes below the intended distal curve',async()=>{
 const s=await ready,p=s.postures.closed.rig.pose(1),part=seatedHind(p);
 for(const [op,x,y] of part.fill)if(op==='L'&&x>=54&&x<=60)assert.ok(y<=75+p.haunch[1]+.12*(x-(58+p.haunch[0]))+1e-6);
 const {paths}=require('../PreviewLocomotion/reference-parts.cjs'),{artwork}=require('./body.cjs');
 assert.deepEqual(seatedHind(s.postures.closed.rig.pose(0)),paths('nearHind',s.postures.closed.rig.pose(0),{artwork}));
});
test('approved face/head and neutral master remain byte identical',async()=>{
 const s=await ready;assert.deepEqual(pixels(s.masters.closed,s.api),pixels(s.approvedMaster,s.api));
 const h=await s.api.loadImage(path.join(s.root,'artifacts/high-resolution-cheek-20261016-r15/head-layer-225.png'));assert.deepEqual(pixels(s.rigs.closed.parts.head,s.api),pixels(h,s.api));
 for(const eye of ['open','closed']){const before=pixels(s.rigs[eye].parts.head,s.api);s.rigs[eye].render({phase:.4,amount:1,walking:false});s.postures[eye].render(1);assert.deepEqual(pixels(s.rigs[eye].parts.head,s.api),before);}
});
test('walking and tracking stay exactly equal to the saved r16 frames',async()=>{
 const s=await ready,dir=path.join(s.root,'artifacts/high-resolution-motion-20261016-r16');
 for(const eye of ['open','closed']){
  for(let n=0;n<32;n++){const im=await s.api.loadImage(path.join(dir,`walk-${eye}-${String(n).padStart(2,'0')}.png`));assert.deepEqual(pixels(s.rigs[eye].render({phase:n/32,amount:1,walking:true}),s.api),pixels(im,s.api));}
  for(const n of [0,12,24]){const roll=(-20+n/24*34)*Math.PI/180,p={roll,eyeX:eye==='open'?roll*2:0,eyeY:eye==='open'?-roll*1.5:0};assert.deepEqual(pixels(s.rigs[eye].render(p),s.api),pixels(await s.api.loadImage(path.join(dir,`track-${eye}-${String(n).padStart(2,'0')}.png`)),s.api));}
 }
});
test('all356 changed motion frames decode, match sources, remain unclipped and indexed',async()=>{
 const s=await ready,m=JSON.parse(fs.readFileSync(path.join(out,'manifest.json'),'utf8')),sha=b=>crypto.createHash('sha256').update(b).digest('hex');assert.equal(m.frames.length,356);assert.equal(m.runtimeSelected,false);
 for(const [p,h] of Object.entries(m.sources))assert.equal(sha(fs.readFileSync(path.join(s.root,p))),h,p);
 const index=JSON.parse(fs.readFileSync(path.join(out,'review-index.json'),'utf8')).filter(p=>!p.detail);assert.deepEqual(index.flatMap(p=>p.frames),m.frames.map(f=>f.name));
 for(const f of m.frames){const file=path.join(out,f.name+'.png');assert.equal(sha(fs.readFileSync(file)),f.sha256);const im=await s.api.loadImage(file);assert.equal(im.width,192);assert.equal(im.height,192);const d=pixels(im,s.api);for(let k=0;k<192;k++)for(const [x,y] of [[k,0],[k,191],[0,k],[191,k]])assert.equal(d[(y*192+x)*4+3],0,f.name);}
 for(const eye of ['open','closed'])for(let n=0;n<=64;n++)assert.equal(m.frames.find(f=>f.name===`sit-${eye}-${String(n).padStart(2,'0')}`).sha256,m.frames.find(f=>f.name===`rise-${eye}-${String(64-n).padStart(2,'0')}`).sha256);
});

'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path'),fs=require('node:fs');
const {setup}=require('./motion-proof.cjs'),{pixels}=require('./sampling.cjs'),{create,foot,round}=require('./upright.cjs'),{eyeRegions}=require('./source-expressions.cjs');
const ready=setup();
function borderClear(c,api){const d=pixels(c,api),w=c.width;for(let k=0;k<w;k++)for(const [x,y] of [[k,0],[k,w-1],[0,k],[w-1,k]])assert.equal(d[(y*w+x)*4+3],0,`clipped ${x},${y}`);}
test('expression migration keeps every source pixel outside eye regions',async()=>{
 const {api,originals}=await ready,a=pixels(originals.open,api);
 for(const name of ['closed','squint']){const b=pixels(originals[name],api);let changed=0;
  for(let y=0;y<225;y++)for(let x=0;x<225;x++){
   const i=(y*225+x)*4;if(![0,1,2,3].some(k=>a[i+k]!==b[i+k]))continue;changed++;
   const lx=(x+.5)*96/225,ly=(y+.5)*96/225;
   assert.ok(eyeRegions.some(([sx,sy,w,h])=>lx>=sx-1&&lx<=sx+w+1&&ly>=sy-1&&ly<=sy+h+1),`${name} escaped eye area at ${x},${y}`);
   assert.equal(a[i+3],b[i+3],'eye patch must not alter silhouette');
  }assert.ok(changed>100);
 }
});
test('shared rest assembly keeps opaque original face and ribbon texels',async()=>{
 const {api,masters,originals,postures}=await ready;
 for(const eye of ['open','closed']){const a=pixels(originals[eye],api),b=pixels(masters[eye],api),head=pixels(postures[eye].head,api);
  assert.equal(head[(120*225+161)*4+3],255,'repaired foreground ribbon must be head-owned');
  for(let y=0;y<225;y++)for(let x=0;x<225;x++){
   const i=(y*225+x)*4;if(head[i+3]!==255)continue;
   for(let k=0;k<4;k++)assert.equal(b[i+k],a[i+k],`${eye} face changed at ${x},${y}`);
  }
 }
});
test('rest, neutral tracking, zero gait and sit/rise endpoint share exact pixels',async()=>{
 const {api,masters,postures}=await ready;
 for(const eye of ['open','closed']){const r=create(masters[eye],api,{padding:16}),a=pixels(r.still(),api);
  assert.deepEqual(pixels(r.render(),api),a);
  assert.deepEqual(pixels(r.render({amount:0,phase:.72,walking:false}),api),a);
  assert.deepEqual(pixels(postures[eye].render(0),api),a);
 }
});
test('gait foot trajectories retain independent Windows formulas and midpoint rounding',()=>{
 const smooth=t=>t*t*(3-2*t);
 for(const walk of [false,true])for(let leg=0;leg<4;leg++)for(let n=0;n<128;n++){
  const p=n/128,q=(p+(walk?(leg%2===0?0:.5):[0,.09,.53,.60][leg]))%1,stance=walk?.6:.3,u=(q-stance)/(1-stance);
  const expected=q<stance?[walk?-3+10*q:-4.2+8.4*q/.3,0]:[(walk?3:4.2)-(walk?6:8.4)*smooth(u),-(walk?2.8:4)*Math.sin(Math.PI*u)**2];
  foot(leg,p,walk).forEach((v,i)=>assert.ok(Math.abs(v-expected[i])<1e-12));
 }
 assert.equal(round(-.5),0);assert.equal(round(-1.5),-2);assert.equal(round(2.5),2);
});
test('default96 sitting renderer still matches all130 existing product frames',async()=>{
 const {api,root,open}=await ready,ref=require('../PreviewLocomotion/reference-parts.cjs'),zlib=require('node:zlib');
 const bank=zlib.gunzipSync(fs.readFileSync(path.join(root,'src/Dororong.App/Assets/locomotion.pbgra.gz'))),stride=96*96*4;
 for(let eye=0;eye<2;eye++){
  const image=eye?await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png')):open,r=ref.create(image,api);
  for(let n=0;n<=64;n++){
   const d=pixels(r.render(r.pose(n/64)),api),actual=Buffer.alloc(stride),at=20+(eye*386+n)*stride;
   for(let i=0;i<stride;i+=4){const a=d[i+3];for(let k=0;k<3;k++)actual[i+k]=Math.round(d[i+2-k]*a/255);actual[i+3]=a;}
   assert.deepEqual(actual,bank.subarray(at,at+stride),`existing product changed ${eye}/${n}`);
  }
 }
});
test('tracking rotation has transparent padding at both extrema and no rump displacement',async()=>{
 const {api,masters}=await ready,r=create(masters.open,api,{padding:16}),a=pixels(r.still(),api);
 for(const roll of [-Math.PI/9,-.1,.1,Math.PI/9]){const c=r.render({roll,eyeX:.85,eyeY:.65}),b=pixels(c,api);borderClear(c,api);
  for(let y=24+Math.ceil(81*1.5);y<192;y++)for(let x=0;x<192;x++)for(let k=0;k<4;k++)assert.equal(b[(y*192+x)*4+k],a[(y*192+x)*4+k],`tracking changed planted feet at ${x},${y}`);
 }
});
test('body breathing preserves face and planted soles without whole-image scaling',async()=>{
 const {api,masters}=await ready,r=create(masters.open,api,{padding:16}),a=pixels(r.still(),api),b=pixels(r.render({breathing:1}),api);
 for(let y=0;y<192;y++)for(let x=0;x<192;x++){
  const ly=(y-24)/1.5;if(ly>=45&&ly<78)continue;
  const i=(y*192+x)*4;for(let k=0;k<4;k++)assert.equal(b[i+k],a[i+k],`breathing altered face/feet at ${x},${y}`);
 }
});
test('nonfinite motion and unsupported padding are rejected',async()=>{
 const {api,masters}=await ready,r=create(masters.open,api);
 assert.throws(()=>r.render({roll:NaN}));assert.throws(()=>r.render({phase:Infinity}));
 assert.throws(()=>create(masters.open,api,{padding:1}));assert.throws(()=>create(masters.open,api,{size:100}));
});
test('first motion samples have no second-body switch at the shared rest boundary',async()=>{
 const {api,masters,postures}=await ready,{premultiply}=require('./sampling.cjs'),r=create(masters.open,api,{padding:16}),a=premultiply(r.still(),api);
 for(const c of [r.render({roll:1e-6}),r.render({amount:1e-6,phase:.3}),postures.open.render(1/64)]){
  const b=premultiply(c,api);let error=0;for(let i=0;i<a.length;i++)error+=Math.abs(a[i]-b[i]);
  assert.ok(error/a.length<.1,'rest handoff must not substitute another body silhouette');
 }
});
test('exported review sequences re-decode, stay padded and reverse sit exactly',async()=>{
 const {api,root}=await ready,dir=path.join(root,'artifacts/high-resolution-motion-20261016-r5'),m=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'),'utf8'));
 assert.equal(m.runtimeSelected,false);assert.deepEqual(m.origin,[24,24]);
 for(const [file,sha] of Object.entries(m.sources))assert.equal(require('node:crypto').createHash('sha256').update(fs.readFileSync(path.join(__dirname,file))).digest('hex'),sha,`stale source ${file}`);
 for(const f of m.frames){const file=path.join(dir,f.name+'.png'),c=await api.loadImage(file);assert.equal(c.width,f.width);assert.equal(c.height,f.height);assert.equal(require('node:crypto').createHash('sha256').update(fs.readFileSync(file)).digest('hex'),f.sha256);if(c.width===192&&c.height===192)borderClear(c,api);}
 for(const eye of ['open','closed']){
  for(const kind of ['walk','run','track','breathe','sit','rise']){
   const list=m.frames.filter(f=>f.name.startsWith(kind+'-'+eye+'-'));
   assert.equal(list.length,{walk:32,run:48,track:25,breathe:33,sit:65,rise:65}[kind]);
   for(const f of list){assert.equal(f.width,192);assert.equal(f.height,192);}
  }
  for(let n=0;n<=64;n++)assert.equal(m.frames.find(f=>f.name===`sit-${eye}-${String(n).padStart(2,'0')}`).sha256,m.frames.find(f=>f.name===`rise-${eye}-${String(64-n).padStart(2,'0')}`).sha256);
 }
});
test('happy sway retains accepted angles, segment times and closed neutral endpoints',async()=>{
 const h=require('./happy.cjs'),{root}=await ready;
 assert.match(fs.readFileSync(path.join(root,'src/Dororong.Core/Behavior/SnackSession.cs'),'utf8'),/HappySeconds = 2\.16;/);
 h.times.forEach((t,i)=>assert.ok(Math.abs(h.roll(t)-h.degrees[i]*Math.PI/180)<1e-12));
 assert.equal(h.roll(-1),0);assert.equal(h.roll(Infinity),0);
 const m=JSON.parse(fs.readFileSync(path.join(root,'artifacts/high-resolution-motion-20261016-r5/manifest.json'),'utf8'));
 const neutral=m.frames.find(f=>f.name==='standing-closed').sha256;
 for(const n of [0,65])assert.equal(m.frames.find(f=>f.name===`happy-closed-${String(n).padStart(2,'0')}`).sha256,neutral);
});
test('all exported walk and gallop phases keep the interior torso sockets opaque',async()=>{
 const {api,root}=await ready,dir=path.join(root,'artifacts/high-resolution-motion-20261016-r5'),m=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'),'utf8'));
 for(const f of m.frames.filter(f=>/^(walk|run)-/.test(f.name))){
  const im=await api.loadImage(path.join(dir,f.name+'.png')),d=pixels(im,api),w=im.width;
  // Probe actual torso interiors, not all enclosed background between legs.
  // Near/far paws can overlap around a small negative-space pocket. That
  // exterior needs visual review, not automatic white filling by flood-fill.
  for(const [lx,ly] of [[35,67],[55,67],[64,69]]){
   const x=Math.floor(24+lx*1.5),y=Math.floor(24+ly*1.5);
   assert.ok(d[(y*w+x)*4+3]>=250,`${f.name} underfilled torso socket ${lx},${ly}`);
  }
 }
});

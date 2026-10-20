'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create,smooth}=require('./connected-families.cjs'),{pose}=require('./sleep-pose.cjs'),{pixels,sample}=require('./sampling.cjs'),{out}=require('./sleep-review.cjs');
const ready=create(),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
test('closed head source and rendered opaque face match r19 throughout sleep/wake',async()=>{
 const m=await ready,dir=path.join(m.s.root,'artifacts/high-resolution-connected-20261017-r19'),manifest=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json')));
 const expected=await m.s.api.loadImage(path.join(m.s.root,'artifacts/high-resolution-cheek-20261016-r15/head-layer-225.png'));
 assert.deepEqual(pixels(expected,m.s.api),pixels(m.s.rigs.closed.parts.head,m.s.api));
 for(const f of manifest.frames.filter(f=>f.scenario==='sleep-wake')){
  const [kind,phase]=f.parameters,p=kind==='wake'?1-phase:phase,old=pixels(await m.s.api.loadImage(path.join(dir,f.name+'.png')),m.s.api),current=pixels(m.render(kind,phase),m.s.api),head=new Float64Array(4);let count=0;
  for(let y=0;y<192;y++)for(let x=0;x<192;x++){
   let opaque=true;for(const dy of [.25,.75])for(const dx of [.25,.75]){sample(m.heads.closed,225,((x+dx)/1.5-16)*225/96-.5,((y+dy)/1.5-16-8*smooth(p))*225/96-.5,head);if(head[3]<254.999)opaque=false;}
   if(opaque){const i=(y*192+x)*4;assert.deepEqual(current.slice(i,i+4),old.slice(i,i+4),f.name+' face '+x+','+y);count++;}
  }assert.ok(count>3000);
 }
});
test('all96 open/closed running frames remain identical to accepted r18',async()=>{
 const m=await ready;for(const eye of ['open','closed'])for(let n=0;n<48;n++)assert.deepEqual(pixels(m.render('run',n/48,{eye}),m.s.api),pixels(await m.s.api.loadImage(path.join(m.s.root,`artifacts/high-resolution-motion-20261017-r18/run-${eye}-full-${String(n).padStart(2,'0')}.png`)),m.s.api),eye+' '+n);
});
test('sleep retains neutral joins and rounded resting contact, with visible pad/fold ink',async()=>{
 const m=await ready,base=pixels(m.render('idle',0,{eye:'closed'}),m.s.api);
 assert.deepEqual(pixels(m.render('sleep',0),m.s.api),base);assert.deepEqual(pixels(m.render('wake',1),m.s.api),base);
 for(let n=0;n<=64;n++){
  const p=pose(n/64);
  for(const part of Object.values(p.extraParts))for(const [, ...values]of part.fill)assert.ok(values.every(Number.isFinite));
  for(const map of Object.values(p.front)){const a=map([20,60]),b=map([25,81]);assert.ok(Math.abs(Math.hypot(b[0]-a[0],b[1]-a[1])-Math.hypot(5,21))<1e-10);}
 }
 // r21 intentionally changes the resting projection, not the run skeleton.
 // A rigid distal-control-distance assertion would contradict that correction.
 // Verify saved silhouette contact independently, not against pose control math.
 const d=pixels(m.render('sleep',1),m.s.api);
 const ink=(x0,y0,x1,y1)=>{let n=0;for(let y=Math.floor(24+y0*1.5);y<24+y1*1.5;y++)for(let x=Math.floor(24+x0*1.5);x<24+x1*1.5;x++){let i=(y*192+x)*4;if(d[i+3]>220&&d[i]<160&&d[i+1]<160&&d[i+2]<160)n++;}return n;};
 assert.ok(ink(27,79,35,86)>5,'front-pad separation');assert.ok(ink(60,78,66,84)>5,'folded hind contour');
 const floor=(a,b)=>{let max=-1;for(let x=Math.ceil(24+a*1.5);x<24+b*1.5;x++)for(let y=0;y<192;y++)if(d[(y*192+x)*4+3]>128)max=Math.max(max,y);return max;};
 assert.ok(floor(60,78)<=floor(48,60)+1,'rear paw may not hang below belly');
 for(let x=24+27*1.5;x<=24+33*1.5;x++)assert.ok(d[(Math.round(24+82*1.5)*192+Math.round(x))*4+3]>240,'front pads meet without a vertical slot');
});
test('every nonneutral pose uses fully opaque, skin-clipped interior strokes',()=>{
 const {render}=require('./sleep-pose.cjs'),rig={pose:()=>({t:0,support:0}),render:(p,opts)=>opts};
 for(let n=1;n<=64;n++){const opts=render(rig,n/64);assert.equal(opts.interiorOpacity??1,1);assert.equal(opts.clipInteriorToSkin,true);assert.deepEqual(opts.interiorParts,['farFront','nearFront','nearHind']);}
});
test('all130 saved sleep/wake frames decode, match hashes and appear in review',async()=>{
 const m=await ready,manifest=JSON.parse(fs.readFileSync(path.join(out,'manifest.json')));assert.equal(manifest.runtimeSelected,false);assert.equal(manifest.frames.length,130);
 assert.deepEqual(manifest.review.flatMap(s=>s.frames),manifest.frames.map(f=>f.name));
 for(const [f,h]of Object.entries(manifest.sources))assert.equal(sha(fs.readFileSync(path.join(m.s.root,f))),h,f);
 for(const f of manifest.frames){const b=fs.readFileSync(path.join(out,f.name+'.png'));assert.equal(sha(b),f.sha256);const im=await m.s.api.loadImage(b);assert.equal(im.width,192);assert.equal(im.height,192);const d=pixels(im,m.s.api);for(let k=0;k<192;k++)for(const [x,y]of [[k,0],[k,191],[0,k],[191,k]])assert.equal(d[(y*192+x)*4+3],0);}
 for(let i=0;i<=64;i++)assert.equal(manifest.frames[i].sha256,manifest.frames[129-i].sha256,'wake reverses exact authored path');
});

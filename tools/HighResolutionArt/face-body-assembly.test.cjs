'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {build,faceRegion,out}=require('./face-body-assembly-review.cjs');
const ready=build();
test('body pipeline reproduces the saved previous body/leg assembly exactly',async()=>{
 const r=await ready;assert.deepEqual(pixels(r.oldRegenerated,api),pixels(r.oldAssembly,api));
});
test('body, legs, seam and ribbon outside face are byte-identical to previous assembly',async()=>{
 const r=await ready,a=pixels(r.oldAssembly,api),b=pixels(r.image,api);let changed=0;
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;if(!faceRegion(x,y))assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));else if([0,1,2,3].some(k=>a[i+k]!==b[i+k]))changed++;}assert.ok(changed>1000);
});
test('assembled opaque face matches r13; protected bangs/cheeks match original directly',async()=>{
 const r=await ready,a=pixels(r.face,api),b=pixels(r.image,api),original=pixels(await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),api),lock=pixels(await api.loadImage(path.join(root,'artifacts/high-resolution-locked-eyes-20261016-r13/protected-mask.png')),api);
 for(let y=0;y<225;y++)for(let x=0;x<120;x++){const i=(y*225+x)*4;if(faceRegion(x,y)&&a[i+3]===255){assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`r13 ${x},${y}`);if(lock[i+3])assert.deepEqual(b.slice(i,i+4),original.slice(i,i+4),`original ${x},${y}`);}}
});
test('saved final PNG matches the checked assembly',async()=>{
 const r=await ready;assert.deepEqual(pixels(await api.loadImage(path.join(out,'candidate-225.png')),api),pixels(r.image,api));
});

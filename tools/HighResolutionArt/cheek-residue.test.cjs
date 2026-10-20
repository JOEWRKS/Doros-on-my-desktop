'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {build,points,keys,out}=require('./cheek-residue-review.cjs');
const ready=build();
test('37 reviewed residues are inherited original eye pixels, not new eyelid ink',async()=>{
 const r=await ready,a=pixels(r.before,api),original=pixels(await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),api),ink=pixels(await api.loadImage(path.join(root,'artifacts/high-resolution-locked-eyes-20261016-r13/eyes-only.png')),api);
 assert.equal(points.length,37);assert.equal(keys.size,37);
 for(const [x,y] of points){const i=(y*225+x)*4;assert.deepEqual(a.slice(i,i+4),original.slice(i,i+4),`${x},${y} source`);assert.equal(ink[i+3],0,`${x},${y} eyelid`);}
});
test('all pixels outside exact residue points and every alpha value remain unchanged',async()=>{
 const r=await ready,a=pixels(r.before,api),b=pixels(r.image,api);let changed=0;
 for(let i=0;i<a.length;i+=4){assert.equal(b[i+3],a[i+3]);if(!keys.has(i/4))assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));else if([0,1,2].some(k=>a[i+k]!==b[i+k]))changed++;}assert.equal(changed,37);
});
test('original bangs and mouth, cheek exterior, corrected body/legs remain exact',async()=>{
 const r=await ready,a=pixels(r.before,api),b=pixels(r.image,api);
 for(const [x0,y0,w,h] of [[0,0,225,122],[55,134,25,16],[20,128,20,25],[90,144,10,6],[0,150,225,75]])for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));}
});
test('head/source layers contain the same opaque repaired face as assembled image',async()=>{
 const r=await ready,b=pixels(r.image,api),s=pixels(r.source,api),h=pixels(r.head,api);
 for(const [x,y] of points){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),s.slice(i,i+4));assert.deepEqual(b.slice(i,i+4),h.slice(i,i+4));}
});
test('saved preview master and semantic layers match the checked build',async()=>{
 const r=await ready;for(const [file,key] of [['candidate-225','image'],['assembly-source-225','source'],['head-layer-225','head']])assert.deepEqual(pixels(await api.loadImage(path.join(out,file+'.png')),api),pixels(r[key],api));
});

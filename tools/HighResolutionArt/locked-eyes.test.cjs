'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {render,out}=require('./locked-eyes.cjs');
const ready=(async()=>{const base=await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),closed=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png'));return {base,closed,...render(base,closed)};})();
test('compare final directly with ORIGINAL: no protected or outside-eye changes',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api),l=pixels(r.lock,api),m=pixels(r.mask,api);let changes=0;
 for(let i=0;i<a.length;i+=4){assert.equal(a[i+3],b[i+3]);if(l[i+3]||!m[i+3])assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`${i/4%225},${Math.floor(i/4/225)}`);if([0,1,2].some(k=>a[i+k]!==b[i+k]))changes++;}assert.ok(changes>500&&changes<1100);
});
test('independent regressions: original central fringe, side-hair rim and cheek pixels',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api);
 for(const [x,y] of [[70,123],[71,123],[80,116],[82,116],[105,124],[105,125],[104,129],[103,134],[38,124],[96,142],[97,142],[98,142],[87,142],[100,142]]){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`${x},${y}`);}
 for(const [x0,y0,w,h] of [[55,135,25,14],[22,130,18,10],[90,144,10,6],[0,0,225,98],[110,0,115,225],[0,145,225,80]])for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));}
});
test('original cheek pigment outside the actual new eyelid ink remains exact',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api),ink=pixels(r.eyes,api);let checked=0;
 for(let y=124;y<145;y++)for(let x=22;x<108;x++){const i=(y*225+x)*4;if(a[i]>180&&a[i]>a[i+1]+12&&a[i]>a[i+2]+4&&!ink[i+3]){assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));checked++;}}assert.ok(checked>200);
});
test('saved candidate is reproducible from ORIGINAL inputs',async()=>{
 const r=await ready;assert.deepEqual(pixels(await api.loadImage(path.join(out,'candidate-225.png')),api),pixels(r.image,api));
});

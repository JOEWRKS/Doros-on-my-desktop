'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {render,out}=require('./original-closed-repair.cjs'),{contours}=require('./source-lid-contours.cjs');
const ready=(async()=>{const base=await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),closed=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png'));return {base,closed,...render(base,closed)};})();
test('only original eye apertures/lash spur change; alpha is exact',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api),m=pixels(r.mask,api);let changes=0;
 for(let i=0;i<a.length;i+=4){assert.equal(a[i+3],b[i+3]);if([0,1,2].some(k=>a[i+k]!==b[i+k])){changes++;assert.ok(m[i+3]);}}assert.ok(changes>500&&changes<1600);
});
test('original cheek exteriors, blush core, mouth, crown, side locks, ornament and body are exact',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api);
 for(const [x0,y0,w,h] of [[0,0,225,100],[0,100,36,125],[110,0,115,225],[55,135,25,14],[22,130,18,10],[90,144,10,6],[0,145,225,80]])
 for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`${x},${y}`);}
});
test('source-derived lid contours stay inside original eye support, not eyebrows',async()=>{
 const r=await ready,paths=contours(r.closed,api);assert.equal(paths.length,2);
 for(const p of paths)for(const [x,y] of p){assert.ok(x>=16&&x<=44);assert.ok(y>=51&&y<58);}
});
test('saved still reproduces from original inputs, not earlier candidates',async()=>{
 const r=await ready,saved=await api.loadImage(path.join(out,'candidate-225.png'));assert.deepEqual(pixels(saved,api),pixels(r.image,api));
});

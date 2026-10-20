'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root,out,basePath,donorPath}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs'),{composite}=require('./bangs-composite.cjs');
const ready=(async()=>{const base=await api.loadImage(path.join(root,basePath)),donor=await api.loadImage(path.join(root,donorPath));return {base,donor,...composite(base,donor,api)};})();
test('composite changes only its local mask and never changes alpha',async()=>{
 const {base,image,mask}=await ready,a=pixels(base,api),b=pixels(image,api),m=pixels(mask,api);let changes=0;
 for(let i=0;i<a.length;i+=4){assert.equal(a[i+3],b[i+3]);if([0,1,2].some(k=>a[i+k]!==b[i+k])){changes++;assert.ok(m[i+3]>0);const x=i/4%225,y=Math.floor(i/4/225);assert.ok(x>=29&&x<=115&&y>=64&&y<=126);}}
 assert.ok(changes>3000&&changes<4500);
});
test('both authored eyelids, mouth, body, ornament and outer side hair remain byte exact',async()=>{
 const {base,image}=await ready,a=pixels(base,api),b=pixels(image,api);
 // Independent anatomy rectangles, not merely reuse of the edit mask.
 for(const [x0,y0,w,h] of [[33,120,31,16],[78,128,31,17],[55,135,29,21],[0,150,225,75],[120,0,105,225],[0,0,29,225],[0,0,225,60]])
  for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`${x},${y}`);}
});
test('local patch is transparent everywhere outside its mask',async()=>{
 const {patch,mask}=await ready,p=pixels(patch,api),m=pixels(mask,api);for(let i=3;i<p.length;i+=4)if(!m[i])assert.equal(p[i],0);
});
test('saved final PNG equals the reproducible composite',async()=>{
 const {image}=await ready,saved=await api.loadImage(path.join(out,'closed-composite-225.png'));assert.deepEqual(pixels(saved,api),pixels(image,api));
});
test('unregistered dimensions are rejected rather than silently stretched',async()=>{
 const {base,donor}=await ready;assert.throws(()=>composite(api.createCanvas(96,96),donor,api));assert.throws(()=>composite(base,api.createCanvas(225,225),api));
});

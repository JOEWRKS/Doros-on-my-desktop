'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root,basePath,donorPath}=require('./bangs-composite-review.cjs');
const {finish}=require('./bangs-face-finish.cjs'),{pixels}=require('./sampling.cjs');
const {out}=require('./bangs-face-finish-review.cjs');
const ready=(async()=>{const base=await api.loadImage(path.join(root,basePath)),donor=await api.loadImage(path.join(root,donorPath));return {base,...finish(base,donor,api)};})();
test('r9 edits only hair and eye masks and preserves all alpha',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api),m=pixels(r.mask,api),e=pixels(r.eyeMask,api);let changed=0;
 for(let i=0;i<a.length;i+=4){assert.equal(a[i+3],b[i+3]);if([0,1,2].some(k=>a[i+k]!==b[i+k])){changed++;assert.ok(m[i+3]||e[i+3]);}}
 assert.ok(changed>3000);assert.ok(r.sharpened>0);
});
test('crown above seam, mouth, body, ribbon and outer hair remain exact',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api);
 for(const [x0,y0,w,h] of [[0,0,225,66],[55,135,29,21],[0,150,225,75],[120,0,105,225],[0,0,29,225]])
  for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`${x},${y}`);}
});
test('saved r9 is reproducible',async()=>{
 const r=await ready,saved=await api.loadImage(path.join(out,'closed-composite-225.png'));assert.deepEqual(pixels(saved,api),pixels(r.image,api));
});

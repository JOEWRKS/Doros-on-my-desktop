'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const folder='artifacts/high-resolution-eye-fringe-20261016-r10';
const ready=(async()=>{const read=async p=>pixels(await api.loadImage(path.join(root,p)),api);return {a:await read('artifacts/high-resolution-bangs-face-20261016-r9/closed-composite-225.png'),b:await read(folder+'/candidate-225.png'),m:await read(folder+'/mask-225.png')};})();
test('saved r10 preserves alpha and every pixel outside the local patch',async()=>{
 const {a,b,m}=await ready;let changed=0;for(let i=0;i<a.length;i+=4){assert.equal(a[i+3],b[i+3]);if([0,1,2].some(k=>a[i+k]!==b[i+k])){assert.ok(m[i+3]);changed++;}}assert.ok(changed>1000);
});
test('crown, outer silhouette, ribbon, mouth and body remain unchanged',async()=>{
 const {a,b}=await ready;
 for(const [x0,y0,w,h] of [[0,0,225,70],[0,0,25,225],[117,0,108,225],[55,141,29,16],[0,150,225,75]])
 for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4),`${x},${y}`);}
});

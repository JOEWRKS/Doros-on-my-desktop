'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {render,cleanFace,out}=require('./eye-only-composite.cjs');
const ready=(async()=>{const base=await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),closed=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png'));return {base,closed,...render(base,closed)};})();
test('eyelid compositing changes no pixel outside transparent ink',async()=>{
 const r=await ready,a=pixels(r.clean,api),b=pixels(r.image,api),e=pixels(r.eyes,api);let count=0;
 for(let i=0;i<a.length;i+=4){assert.equal(a[i+3],b[i+3]);if(!e[i+3])assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));else count++;}assert.ok(count>50&&count<300);
});
test('eye layer contains no opaque skin or rectangular background',async()=>{
 const r=await ready,e=pixels(r.eyes,api);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;if(!e[i+3])continue;assert.ok((x>=39&&x<=61&&y>=119&&y<=128)||(x>=79&&x<=104&&y>=128&&y<=135),`${x},${y}`);
  // Very low-alpha RGB has unpremultiplication rounding; constrain visible ink.
  if(e[i+3]>16)assert.ok(Math.max(...e.slice(i,i+3))<90);
 }
});
test('clean face depends only on original master, not closed-expression surroundings',async()=>{
 const r=await ready;assert.deepEqual(pixels(cleanFace(r.base).image,api),pixels(r.clean,api));
 const alt=api.createCanvas(96,96),g=alt.getContext('2d');g.drawImage(r.closed,0,0);g.fillStyle='#00ff00';g.fillRect(0,0,96,49);g.fillRect(0,59,96,37);
 assert.deepEqual(pixels(render(r.base,alt).image,api),pixels(r.image,api));
});
test('original mouth, cheeks, hair exterior and body are preserved; saved PNG reproduces',async()=>{
 const r=await ready,a=pixels(r.base,api),b=pixels(r.image,api),m=pixels(r.mask,api);
 for(let i=0;i<a.length;i+=4){assert.equal(a[i+3],b[i+3]);if(!m[i+3])assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));}
 for(const [x0,y0,w,h] of [[55,135,25,14],[22,130,18,10],[90,144,10,6],[0,0,225,100],[110,0,115,225],[0,145,225,80]])for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){const i=(y*225+x)*4;assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));}
 assert.deepEqual(pixels(await api.loadImage(path.join(out,'candidate-225.png')),api),b);
});

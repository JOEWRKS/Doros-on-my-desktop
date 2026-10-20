'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path'),fs=require('node:fs'),crypto=require('node:crypto');
const canvas=require(process.env.LOCOMOTION_CANVAS || path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {render,regions}=require('./canonical-fixes.cjs');
const file=path.join(__dirname,'input/canonical-225.png');
const pixels=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
async function source(){const img=await canvas.loadImage(file),c=canvas.createCanvas(225,225);c.getContext('2d').drawImage(img,0,0);return c;}
test('pinned accepted original master remains unchanged',()=>assert.equal(crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex'),'1d84aa57e2824add4450427d56b0d45eb729b5eedbe9f0fcc7a6fab9f766483c'));
test('reject low resolution input and unknown correction',()=>{
 assert.throws(()=>render(canvas.createCanvas(96,96),canvas),/225px/);
 assert.throws(()=>render(canvas.createCanvas(225,225),canvas,['unknown']),/Unknown/);
});
test('disabled corrections preserve exact source RGBA',async()=>{const s=await source();assert.deepEqual(pixels(render(s,canvas,[])),pixels(s));});
for(const name of Object.keys(regions))test(name+' changes only its assigned region',async()=>{
 const s=await source(),a=pixels(s),b=pixels(render(s,canvas,[name])),[rx,ry,w,h]=regions[name];let changed=0;
 for(let y=0;y<225;y++)for(let x=0;x<225;x++)for(let c=0;c<4;c++){
  const i=(y*225+x)*4+c;if(a[i]===b[i])continue;changed++;
  assert.ok(x>=rx&&x<rx+w&&y>=ry&&y<ry+h,`${name} escaped at ${x},${y}`);
 }
 assert.ok(changed>0);
});
test('combined corrections preserve face, hair, mouth and full canvas bounds',async()=>{
 const s=await source(),a=pixels(s),c=render(s,canvas),b=pixels(c);assert.equal(c.width,225);assert.equal(c.height,225);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  if(Object.values(regions).some(([rx,ry,w,h])=>x>=rx&&x<rx+w&&y>=ry&&y<ry+h))continue;
  const i=(y*225+x)*4;assert.deepEqual(b.subarray(i,i+4),a.subarray(i,i+4));
 }
});
test('PNG re-decode is pixel-identical and corners remain transparent',async()=>{
 const original=render(await source(),canvas),png=await canvas.loadImage(original.toBuffer('image/png'));
 const c=canvas.createCanvas(225,225);c.getContext('2d').drawImage(png,0,0);
 assert.deepEqual(pixels(c),pixels(original));
 for(const [x,y] of [[0,0],[224,0],[0,224],[224,224]])assert.equal(pixels(c)[(y*225+x)*4+3],0);
});

'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const {setup}=require('./motion-proof.cjs'),{transfer}=require('./source-expressions.cjs'),{pixels}=require('./sampling.cjs');
const ready=setup();
test('squint remains unchanged by closed-eye-only repair',async()=>{
 const {api,originals,open}=await ready;
 const dummy=api.createCanvas(96,96),g=dummy.getContext('2d');g.fillStyle='#00ff00';g.fillRect(0,0,96,96);
 assert.deepEqual(pixels(transfer(originals.open,open,dummy,api,{kind:'squint'}),api),pixels(originals.squint,api));
});
test('closed eyes use authored source texels, with no replacement lid curve',async()=>{
 const {api,root,originals,open}=await ready;
 const authored=await api.loadImage(require('node:path').join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png'));
 const expected=require('./expressions.cjs').transfer(originals.open,open,authored,api);
 assert.deepEqual(pixels(originals.closed,api),pixels(expected,api));
});
test('closed eyes remove purple iris pixels in both source225 eye interiors',async()=>{
 const {api,originals}=await ready,d=pixels(originals.closed,api);
 for(const [x0,y0,w,h] of [[44,119,20,11],[82,124,19,15]])for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){
  const i=(y*225+x)*4,[r,g,b]=d.subarray(i,i+3);
  assert.ok(!(b>g+12&&b>r+8),`residual iris ${x},${y}`);
 }
});
test('open eye source and non-eye mouth remain exact',async()=>{
 const {api,originals}=await ready,a=pixels(originals.open,api);
 for(const kind of ['closed','squint']){const b=pixels(originals[kind],api);
  for(let y=136;y<152;y++)for(let x=58;x<80;x++)for(let k=0;k<4;k++)assert.equal(b[(y*225+x)*4+k],a[(y*225+x)*4+k]);
 }
});

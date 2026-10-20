'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const api=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {render}=require('./canonical-fixes.cjs'),{split}=require('./parts.cjs'),{create}=require('../PreviewLocomotion/reference-parts.cjs');
const read=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
async function setup(){const master=render(await api.loadImage(path.join(__dirname,'input/canonical-225.png')),api),parts=split(master,api);
 const native=await api.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'));
 return {master,parts,rig:create(native,api,{outputScale:1.5,headOverride:parts.head})};}
test('head and body partition keeps source RGBA with single ownership',async()=>{
 const {master,parts}=await setup(),a=read(master),h=read(parts.head),b=read(parts.body);
 for(let i=0;i<a.length;i+=4){assert.ok(!(h[i+3]&&b[i+3]));for(let c=0;c<4;c++)assert.equal(h[i+c]+b[i+c],a[i+c]);}
});
test('all 65 high-resolution sit frames keep sockets opaque and fit canvas',async()=>{
 const {rig}=await setup();
 for(let n=0;n<=64;n++){
  const p=rig.pose(n/64),c=rig.render(p),d=read(c);assert.equal(c.width,144);assert.equal(c.height,144);
  for(const [x,y] of rig.attachmentProbes(p))assert.ok(d[(Math.floor(y*1.5)*144+Math.floor(x*1.5))*4+3]>=250,`socket gap at ${n}: ${x},${y}`);
  for(let k=0;k<144;k++)for(const [x,y] of [[k,0],[k,143],[0,k],[143,k]])assert.equal(d[(y*144+x)*4+3],0,`frame ${n} clips ${x},${y}`);
  // The initial failed scale implementation drew the ink layer a second time
  // at1.5x and detached it from the white body. Test occupied body edges, not
  // merely image dimensions or source hashes.
  for(let y=108;y<135;y++)for(let x=20;x<119;x++){
   const i=(y*144+x)*4;if(d[i+3]<200)continue;
   if([[1,0],[-1,0],[0,1],[0,-1]].some(([dx,dy])=>d[((y+dy)*144+x+dx)*4+3]<80))
     assert.ok((d[i]+d[i+1]+d[i+2])/3<240,`uninked body edge at ${n}: ${x},${y}`);
  }
 }
});

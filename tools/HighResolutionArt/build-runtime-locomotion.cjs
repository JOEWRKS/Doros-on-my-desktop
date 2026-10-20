'use strict';
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels,premultiply}=require('./sampling.cjs');
async function main(){
 const m=await require('./current-motion.cjs').create(),s=m.s;
 const fixed=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/masked-head-225.png'));
 const a=pixels(s.postures.open.head,api),b=pixels(s.postures.closed.head,api),target=pixels(fixed,api);
 const c=api.createCanvas(225,225),g=c.getContext('2d'),d=g.createImageData(225,225);d.data.set(b);
 let conflicts=0;
 for(let i=0;i<a.length;i+=4)if([0,1,2,3].some(k=>a[i+k]!==target[i+k])){
  if([0,1,2,3].some(k=>a[i+k]!==b[i+k])){conflicts++;continue;}
  d.data.set(target.subarray(i,i+4),i);
 }
 assert.equal(conflicts,0,'Fixed head boundary overlaps approved closed expression; do not overwrite it');g.putImageData(d,0,0);
 const frames=[{body:c,headX:0,headY:0}];
 for(let level=1;level<=8;level++)for(let phase=0;phase<32;phase++)frames.push(s.rigs.open.render({phase:phase/32,amount:level/8,walking:true,bodyOnly:true}));
 for(let frame=0;frame<=64;frame++)frames.push(s.postures.open.render(frame/64,{bodyOnly:true}));
 for(let level=1;level<=8;level++)for(let phase=0;phase<48;phase++)frames.push(s.rigs.open.render({phase:phase/48,amount:level/8,walking:false,runMix:1,bodyOnly:true}));
 const sleep=require('./accepted-sleep.cjs').create(s.postures.closed.rig,api,s.measurement);
 const sleeping=Array.from({length:65},(_,i)=>sleep.render(i/64));
 for(const f of sleeping)frames.push({body:f.body,headY:f.headY});
 // The approved forepad divider belongs ABOVE the head's white skirt.
 for(const f of sleeping)frames.push({body:f.effect||api.createCanvas(225,225)});
 frames.push({body:require('./perch-body.cjs').create(api,s.measurement)});
 const header=Buffer.alloc(8);header.write('DLM2');header.writeInt32LE(frames.length,4);const chunks=[header];
 for(const f of frames){const values=premultiply(f.body,api),raw=Buffer.alloc(values.length*4);
  for(let i=0;i<values.length;i+=4)for(let k=0;k<4;k++)raw.writeFloatLE(values[i+[2,1,0,3][k]],(i+k)*4);
  const data=zlib.gzipSync(raw),info=Buffer.alloc(20);info.writeDoubleLE(f.headX||0);info.writeDoubleLE(f.headY||0,8);info.writeInt32LE(data.length,16);chunks.push(info,data);
 }
 const file=path.join(root,'src/Dororong.App/Assets/high-resolution-locomotion.bank');fs.writeFileSync(file,Buffer.concat(chunks));console.log(file,frames.length);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

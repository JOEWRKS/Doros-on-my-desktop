'use strict';
// Derived body poses only. Original artwork and the approved grab bank are read-only.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{premultiply}=require('./sampling.cjs');
const hunt=require('../PreviewLocomotion/hunt-motion.js');
const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
async function main(){
 const m=await require('./grab-boundary-connected.cjs').create(),rig=m.common.s.postures.open.rig,chunks=[];
 const pen=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-contour-still-20261017-r33/measurement.json'))).pen;
 for(let i=0;i<=156;i++){
  const p=hunt.sample(i/60),shift=[p.sway,-1.6*p.amount+.22*p.sway];
  const rootPoint=hunt.map(64,73,p),tip=hunt.map(65,81,p),angle=Math.atan2(tip[1]-rootPoint[1],tip[0]-rootPoint[0])-Math.atan2(8,1),co=Math.cos(angle),si=Math.sin(angle);
  const map=([x,y])=>[tip[0]+(x-65)*co-(y-81)*si,tip[1]+(x-65)*si+(y-81)*co];
  const base=rig.render(rig.pose(0),{includeHead:false,mapOverride:()=> ([x,y])=>hunt.map(x,y,p),extraParts:{haunch:rootedHaunch(shift[1],shift[0]),nearHind:connectedHind(map,shift)}});
  const body=require('./hunt-forebody.cjs').apply(base,p,api,pen);
  const data=premultiply(body,api),bytes=Buffer.alloc(data.length*4);
  for(let j=0;j<data.length;j+=4)for(let c=0;c<4;c++)bytes.writeFloatLE(data[j+[2,1,0,3][c]],(j+c)*4);
  const compressed=zlib.gzipSync(bytes),n=Buffer.alloc(4);n.writeInt32LE(compressed.length);chunks.push(n,compressed);
 }
 const header=Buffer.alloc(8);header.write('DHF1');header.writeInt32LE(157,4);
 const file=path.join(root,'src/Dororong.App/Assets/high-resolution-hunt-body.bank');fs.writeFileSync(file,Buffer.concat([header,...chunks]));
 console.log(file);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

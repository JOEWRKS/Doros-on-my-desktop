'use strict';
const fs=require('node:fs'),path=require('node:path');
const api=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {render}=require('./canonical-fixes.cjs'),{split}=require('./parts.cjs');
const {create}=require('../PreviewLocomotion/reference-parts.cjs');
(async()=>{
 const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/scale-fixed-corrections-20261016/sitting-proof');fs.mkdirSync(out,{recursive:true});
 const source=await api.loadImage(path.join(__dirname,'input/canonical-225.png'));
 const master=render(source,api),parts=split(master,api);
 for(const [name,part] of Object.entries(parts))fs.writeFileSync(path.join(out,name+'-225.png'),part.toBuffer('image/png'));
 const native=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-canonical.png'));
 const rig=create(native,api,{outputScale:1.5,headOverride:parts.head});
 const sheet=api.createCanvas(720,192),ctx=sheet.getContext('2d');ctx.fillStyle='#e2e2e2';ctx.fillRect(0,0,720,192);
 for(let i=0;i<=64;i++){
  const frame=rig.render(rig.pose(i/64));fs.writeFileSync(path.join(out,`sit-${String(i).padStart(2,'0')}.png`),frame.toBuffer('image/png'));
  if(i%16===0){const col=i/16;ctx.drawImage(frame,col*144,0);ctx.fillStyle='#292329';ctx.font='14px sans-serif';ctx.fillText(`${Math.round(i/64*100)}%`,col*144+58,172);}
 }
 fs.writeFileSync(path.join(out,'sequence-contact-sheet.png'),sheet.toBuffer('image/png'));
 console.log('65 high-resolution sitting proof frames exported. Runtime unchanged; closed-eye and upright endpoint reconciliation pending.');
})();

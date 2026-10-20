'use strict';
const fs=require('node:fs'),path=require('node:path');
const api=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
(async()=>{
 const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/scale-fixed-corrections-20261016');
 const c=api.createCanvas(960,400),g=c.getContext('2d');g.fillStyle='#ddd';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
 for(const [i,name] of ['canonical','closed-eyes','blink-squint'].entries()){
  const im=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-'+name+'.png'));
  g.drawImage(im,10,37,40,29,i*320,30,320,232);g.fillStyle='#222';g.font='18px sans-serif';g.fillText(name,i*320+8,290);
 }
 fs.writeFileSync(path.join(out,'eye-source-inspection.png'),c.toBuffer('image/png'));
 const gait=await api.loadImage(path.join(root,'artifacts/high-resolution-motion-20261016/walk-open-23.png'));
 const detail=api.createCanvas(768,768),dg=detail.getContext('2d');dg.fillStyle='#252a31';dg.fillRect(0,0,768,768);dg.imageSmoothingEnabled=false;dg.drawImage(gait,0,0,768,768);
 fs.writeFileSync(path.join(out,'gait-seam-inspection.png'),detail.toBuffer('image/png'));
})();

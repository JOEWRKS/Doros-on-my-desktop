'use strict';
const fs=require('node:fs'),path=require('node:path');
const {setup}=require('./motion-proof.cjs'),{create,maxRaisedRoll}=require('./upright.cjs');
// Saved comparison: original authority / rejected candidate / repaired motion.
// Nearest zooms are inspection views, not replacement artwork.
(async()=>{
 const {root,api,masters,originals}=await setup(),out=path.join(root,'artifacts/high-resolution-motion-20261016-r3');fs.mkdirSync(out,{recursive:true});
 const old=path.join(root,'artifacts/high-resolution-motion-20261016');
 const r=create(masters.open,api,{padding:16}),h=create(masters.closed,api,{padding:16});
 const cases=[['Raised head','track-open-24',r.render({roll:maxRaisedRoll,eyeX:maxRaisedRoll*2,eyeY:-maxRaisedRoll*1.5})],
  ['Lowered head','track-open-00',r.render({roll:-Math.PI/9,eyeX:-Math.PI/9*2,eyeY:Math.PI/9*1.5})],
  ['Walking','walk-open-23',r.render({amount:1,phase:23/32})],
  ['Gallop','run-open-16',r.render({amount:1,phase:16/48,walking:false})],
  ['Happy sway','happy-closed-14',h.render({roll:-8*Math.PI/180})]];
 const c=api.createCanvas(960,660),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);g.font='14px sans-serif';
 for(let i=0;i<cases.length;i++){
  const [label,before,after]=cases[i];g.fillStyle='#e9e9eb';g.fillText(label,i*192+20,22);
  g.drawImage(await api.loadImage(path.join(old,before+'.png')),i*192,30);
  g.drawImage(after,i*192,246);
  g.imageSmoothingEnabled=false;g.drawImage(after,88,91,58,69,i*192+9,447,174,207);g.imageSmoothingEnabled=true;
  fs.writeFileSync(path.join(out,'fix-'+i+'.png'),after.toBuffer('image/png'));
 }
 fs.writeFileSync(path.join(out,'five-defects-before-after.png'),c.toBuffer('image/png'));
 const compare=api.createCanvas(768,480),cg=compare.getContext('2d');cg.fillStyle='#edeae4';cg.fillRect(0,0,768,480);
 const original=create(originals.open,api,{padding:16}).still(),now=r.still();
 for(const [i,item] of [original,now,cases[2][2],cases[3][2]].entries()){
  cg.drawImage(item,i*192,0);cg.imageSmoothingEnabled=false;cg.drawImage(item,105,119,32,40,i*192+24,216,128,160);cg.imageSmoothingEnabled=true;
 }
 cg.font='14px sans-serif';cg.fillStyle='#352e36';['Source-fixed','New rest','Walk 23/32','Gallop 16/48'].forEach((s,i)=>cg.fillText(s,i*192+20,418));
 fs.writeFileSync(path.join(out,'hind-source-comparison.png'),compare.toBuffer('image/png'));
 console.log(out);
})().catch(e=>{console.error(e);process.exitCode=1;});

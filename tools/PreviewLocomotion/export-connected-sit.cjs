'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {pose}=require('./connected-sit-rig.cjs'),{draw}=require('./connected-sit-blocking.cjs');
const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/forward-plop-20261001'));
fs.mkdirSync(out,{recursive:true});
const sample=canvas.createCanvas(300,252),sc=sample.getContext('2d');
const sheet=canvas.createCanvas(2100,504),cc=sheet.getContext('2d');
for(const bones of [false,true]){
  for(const [row,direction] of ['sit','rise'].entries())for(let n=0;n<=6;n++){
    draw(sc,pose(n/6,direction),{bones});cc.drawImage(sample,n*300,row*252);
  }
  fs.writeFileSync(path.join(out,bones?'joint-sheet.png':'contact-sheet.png'),sheet.toBuffer('image/png'));
}
const inputs=['connected-sit-rig.cjs','connected-sit-blocking.cjs','export-connected-sit.cjs'];
fs.writeFileSync(path.join(out,'provenance.json'),JSON.stringify({purpose:'forward-reaching hindlegs, late pelvic plop and canonical-proportioned thicker limbs; not character skin or product output',duration:12,fps:60,width:600,height:420,bonesInMovie:false,
  normalSpeed:[0,4],halfSpeed:[4,12],imageInputs:[],sheetOnly:process.argv.includes('--sheet-only'),
  inputs:Object.fromEntries(inputs.map(f=>[f,crypto.createHash('sha256').update(fs.readFileSync(path.join(__dirname,f))).digest('hex')]))},null,2));
if(!process.argv.includes('--sheet-only')){
  const dir=path.join(out,'frames');fs.mkdirSync(dir,{recursive:true});
  const frame=canvas.createCanvas(600,420),ctx=frame.getContext('2d');
  for(let n=0;n<720;n++){
    const seconds=n/60,s=seconds<4?seconds:(seconds-4)/2;
    const direction=s<2.5?'sit':'rise',time=direction==='sit'?(s-.6)/.9:(s-2.5)/.9;
    draw(ctx,pose(time,direction),{bones:false});fs.writeFileSync(path.join(dir,`${String(n).padStart(5,'0')}.png`),frame.toBuffer('image/png'));
  }
}
console.log(out);

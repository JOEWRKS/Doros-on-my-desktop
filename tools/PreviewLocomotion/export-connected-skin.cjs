'use strict';
// Local skin acceptance export only: never writes a product bank or source PNG.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {pose}=require('./connected-sit-rig.cjs'),{create}=require('./connected-sit-skin.cjs');
async function main(){
  const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/connected-skin-20261001'));
  fs.mkdirSync(out,{recursive:true});
  const source=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png');
  const original=await canvas.loadImage(source),skin=create(original,{createCanvas:canvas.createCanvas});
  const backgrounds=['#f3f0e9','#202630'];
  const sheet=canvas.createCanvas(1344,768),sc=sheet.getContext('2d');
  for(let row=0;row<4;row++){
    sc.fillStyle=backgrounds[Math.floor(row/2)];sc.fillRect(0,row*192,1344,192);
    for(let n=0;n<7;n++)sc.drawImage(skin.render(pose(n/6,row%2?'rise':'sit'),{scale:2}),n*192,row*192);
  }
  fs.writeFileSync(path.join(out,'skin-contact-sheet.png'),sheet.toBuffer('image/png'));
  const comparison=canvas.createCanvas(1152,384),cc=comparison.getContext('2d');
  cc.fillStyle=backgrounds[0];cc.fillRect(0,0,1152,384);cc.imageSmoothingEnabled=false;
  cc.drawImage(original,48,48,288,288);cc.drawImage(skin.render(pose(0),{scale:3}),432,48);cc.drawImage(skin.render(pose(1),{scale:3}),816,48);
  fs.writeFileSync(path.join(out,'source-standing-seated.png'),comparison.toBuffer('image/png'));
  for(const [name,t] of [['standing',0],['seated',1]])fs.writeFileSync(path.join(out,`${name}.png`),skin.render(pose(t),{scale:4}).toBuffer('image/png'));
  const inputs=['connected-sit-rig.cjs','connected-sit-skin.cjs','export-connected-skin.cjs'].map(f=>path.join(__dirname,f)).concat(source);
  fs.writeFileSync(path.join(out,'skin-provenance.json'),JSON.stringify({purpose:'approved rig skinned with rigid canonical head and joint-driven white body; local visual acceptance only',width:768,height:440,fps:60,duration:12,normalSpeed:[0,4],halfSpeed:[4,12],sheetOnly:process.argv.includes('--sheet-only'),
    inputs:Object.fromEntries(inputs.map(f=>[path.relative(path.join(__dirname,'../..'),f).replaceAll('\\','/'),crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex')]))},null,2));
  if(!process.argv.includes('--sheet-only')){
    const frames=path.join(out,'skin-frames');fs.mkdirSync(frames,{recursive:true});
    const frame=canvas.createCanvas(768,440),fc=frame.getContext('2d');
    for(let n=0;n<720;n++){
      const seconds=n/60,s=seconds<4?seconds:(seconds-4)/2,direction=s<2.5?'sit':'rise',time=direction==='sit'?(s-.6)/.9:(s-2.5)/.9;
      const p=pose(time,direction),native=skin.render(p),large=skin.render(p,{scale:3});
      for(let side=0;side<2;side++){
        fc.fillStyle=backgrounds[side];fc.fillRect(side*384,0,384,440);
        fc.drawImage(native,side*384+144,10);fc.drawImage(large,side*384+48,132);
      }
      fs.writeFileSync(path.join(frames,`${String(n).padStart(5,'0')}.png`),frame.toBuffer('image/png'));
    }
  }
  console.log(out);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

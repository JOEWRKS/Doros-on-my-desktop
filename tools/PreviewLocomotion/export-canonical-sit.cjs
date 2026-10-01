'use strict';
// Local visual review only. Reference seated artwork is shown in a comparison,
// NEVER supplied to the runtime renderer. Product bank and installed app stay put.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {create}=require('./canonical-sit.cjs');
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
  const args=process.argv.slice(2),positions=args.filter(a=>!a.startsWith('--'));
  if(positions.length>1||args.some(a=>a.startsWith('--')&&a!=='--sheet-only'))throw Error('Usage: export-canonical-sit.cjs [output-directory] [--sheet-only]');
  const out=path.resolve(positions[0]||path.join(__dirname,'../../artifacts/canonical-volume-20261001'));fs.mkdirSync(out,{recursive:true});
  const source=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'),original=await canvas.loadImage(source),skin=create(original,canvas);
  const backgrounds=['#f3f0e9','#202630'],write=(name,c)=>fs.writeFileSync(path.join(out,name),c.toBuffer('image/png'));
  const sheet=canvas.createCanvas(1344,768),sc=sheet.getContext('2d');
  for(let row=0;row<4;row++){
    sc.fillStyle=backgrounds[Math.floor(row/2)];sc.fillRect(0,row*192,1344,192);
    for(let n=0;n<7;n++)sc.drawImage(skin.render(n/6,{direction:row%2?'rise':'sit',scale:2}),n*192,row*192);
  }write('contact-sheet.png',sheet);
  const referencePath=path.join(__dirname,'assets/seated-final-10-2.png'),reference=canvas.createCanvas(96,96);reference.getContext('2d').drawImage(await canvas.loadImage(referencePath),3,-12);
  const compare=canvas.createCanvas(1152,768),cc=compare.getContext('2d');cc.imageSmoothingEnabled=false;
  for(let row=0;row<2;row++){
    cc.fillStyle=backgrounds[row];cc.fillRect(0,row*384,1152,384);
    cc.drawImage(original,48,row*384+48,288,288);cc.drawImage(skin.render(1,{scale:3}),432,row*384+48);cc.drawImage(reference,816,row*384+48,288,288);
  }write('standing-seated-reference.png',compare);
  for(const [name,t]of[['standing',0],['seated',1]])write(`${name}.png`,skin.render(t,{scale:4}));
  const bones=canvas.createCanvas(768,384),bc=bones.getContext('2d');
  for(let side=0;side<2;side++){
    bc.save();bc.translate(side*384,0);bc.drawImage(skin.render(side,{scale:4}),0,0);bc.scale(4,4);
    const p=skin.pose(side);bc.lineWidth=.6;
    for(const l of p.legs.filter(l=>l.visible)){bc.strokeStyle=l.front?'#149b83':'#9952b8';bc.beginPath();l.joints.forEach((q,n)=>n?bc.lineTo(...q):bc.moveTo(...q));bc.stroke();}
    bc.restore();
  }write('joint-endpoints.png',bones);
  const frames=path.join(out,'frames');
  if(!args.includes('--sheet-only')){
    fs.mkdirSync(frames,{recursive:true});const frame=canvas.createCanvas(768,440),fc=frame.getContext('2d');
    for(let n=0;n<720;n++){
      const seconds=n/60,s=seconds<4?seconds:(seconds-4)/2,direction=s<2.5?'sit':'rise',time=direction==='sit'?(s-.6)/.9:(s-2.5)/.9;
      const full=skin.render(time,{direction,scale:4}),native=canvas.createCanvas(96,96),large=canvas.createCanvas(288,288);
      native.getContext('2d').drawImage(full,0,0,96,96);large.getContext('2d').drawImage(full,0,0,288,288);
      for(let side=0;side<2;side++){fc.fillStyle=backgrounds[side];fc.fillRect(side*384,0,384,440);fc.drawImage(native,side*384+144,10);fc.drawImage(large,side*384+48,132);}write(path.join('frames',`${String(n).padStart(5,'0')}.png`),frame);
    }
  }
  fs.writeFileSync(path.join(out,'provenance.json'),JSON.stringify({purpose:'canonical paint on actual rigid bones; local sit/rise visual review, not product installation',sourceSha256:hash(source),runtimeRasterInputs:['canonical standing only'],referenceOnly:{file:path.relative(path.join(__dirname,'../..'),referencePath),sha256:hash(referencePath)},duration:12,fps:60,width:768,height:440,normalSpeed:[0,4],halfSpeed:[4,12],sitMs:900,riseMs:900,captionFree:true,sheetOnly:args.includes('--sheet-only'),inputs:Object.fromEntries(['canonical-bind.cjs','canonical-sit.cjs','export-canonical-sit.cjs'].map(f=>[f,hash(path.join(__dirname,f))]))},null,2));
  console.log(out);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

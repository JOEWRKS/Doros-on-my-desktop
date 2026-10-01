// Bake a kinematic sit/rise, not endpoint-image timing. The exact authored end
// poses remain immutable. This tool also emits a native-size review sequence.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const canvas=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
(async()=>{
  const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/seating-motion-20260930'));
  fs.mkdirSync(path.join(out,'review-frames'),{recursive:true});
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const f of ['rig.js','seating-motion.js','../GenerateLayeredPull/contour.js','sit-correspondence.js','authored-sit.js','render.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,f),'utf8'),scope);
  const assets=path.resolve(__dirname,'../../src/Dororong.App/Assets');
  const load=p=>canvas.loadImage(p);
  const standing=await load(path.join(assets,'dororong-canonical.png'));
  const seated=await load(path.join(__dirname,'assets/seated-final-10-2.png'));
  const renderer=scope.createLocomotionRenderer(standing,await load(path.join(assets,'dororong-closed-eyes.png')),await load(path.join(assets,'dororong-sleep.png')),seated);
  const bank=canvas.createCanvas(2048,4352),bankCtx=bank.getContext('2d');
  const sheet=canvas.createCanvas(1728,384),sc=sheet.getContext('2d');sc.fillStyle='#30343c';sc.fillRect(0,0,sheet.width,sheet.height);
  const nativeFrames=[],evidence=[];
  for(const direction of ['sit','rise'])for(let i=0;i<=64;i++){
    const t=i/64,rising=direction==='rise',p=scope.locomotion.pose({sit:rising?1-t:t});p.rising=rising;
    const frame=renderer.render(p),index=(rising?65:0)+i;
    bankCtx.drawImage(frame,(index%8)*256,Math.floor(index/8)*256);
    const native=canvas.createCanvas(96,96);native.getContext('2d').drawImage(frame,32,32,192,192,0,0,96,96);nativeFrames.push(native);
    evidence.push(scope.seatingMotion.pose(t,direction));
    if(i%8===0){const col=i/8;sc.imageSmoothingEnabled=false;sc.drawImage(native,col*192,rising?192:0,192,192);}
  }
  const bankPath=path.join(__dirname,'assets/seated-articulated-bank.png');
  fs.writeFileSync(bankPath,bank.toBuffer('image/png'));
  fs.writeFileSync(path.join(out,'kinematic-contact-sheet.png'),sheet.toBuffer('image/png'));
  fs.writeFileSync(path.join(out,'motion-evidence.json'),JSON.stringify(evidence,null,2));
  const beforePath=process.argv[3]?path.resolve(process.argv[3]):path.join(out,'before-bank.png');
  if(!fs.existsSync(beforePath)){
    console.log(`Baked 130 articulated frames. Comparison omitted: pass an existing baseline atlas as the second argument. ${bankPath}`);
    return;
  }
  const old=await load(beforePath);
  // First 4.5 s real-time, then the same action at 1/3 speed. Left=old,
  // right=new, no captions/cursor. Native 96px comparison plus 3x nearest view.
  const fps=60,duration=15;
  const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
  function sequence(s,newMotion){
    const sitTime=newMotion?.9:.65,riseTime=newMotion?.65:.65;
    if(s<.5)return {t:0,rising:false};
    if(s<.5+sitTime)return {t:(s-.5)/sitTime,rising:false};
    if(s<2.4)return {t:1,rising:false};
    if(s<2.4+riseTime)return {t:(s-2.4)/riseTime,rising:true};
    return {t:0,rising:false};
  }
  for(let n=0;n<fps*duration;n++){
    const seconds=n/fps,s=seconds<4.5?seconds:(seconds-4.5)/3;
    const frame=canvas.createCanvas(768,440),c=frame.getContext('2d');c.fillStyle='#f5f3f0';c.fillRect(0,0,768,440);
    for(let side=0;side<2;side++){
      const state=sequence(s,side===1),t=Math.max(0,Math.min(1,state.t));
      const sprite=canvas.createCanvas(96,96),d=sprite.getContext('2d');
      if(side===0){const idx=Math.round((state.rising?1-smooth(t):smooth(t))*64);d.drawImage(old,(idx%8)*256+32,Math.floor(idx/8)*256+32,192,192,0,0,96,96);}
      else d.drawImage(nativeFrames[(state.rising?65:0)+Math.round(t*64)],0,0);
      c.imageSmoothingEnabled=false;c.drawImage(sprite,side*384+144,12);
      c.drawImage(sprite,side*384+48,130,288,288);
    }
    fs.writeFileSync(path.join(out,'review-frames',`${String(n).padStart(5,'0')}.png`),frame.toBuffer('image/png'));
  }
  console.log(`Baked 130 articulated frames and ${fps*duration} review frames. ${bankPath}`);
})().catch(e=>{console.error(e);process.exitCode=1;});

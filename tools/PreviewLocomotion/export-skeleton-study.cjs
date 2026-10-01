// Local visual acceptance study. Does not overwrite any product bank or install.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),crypto=require('node:crypto');
const canvas=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
(async()=>{
  const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/skeleton-sit-20260930'));
  fs.mkdirSync(out,{recursive:true});
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const file of ['skeleton-sit-rig.js','skeleton-sit.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,file),'utf8'),scope);
  const source=path.resolve(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png');
  const renderer=scope.skeletonSit.create(await canvas.loadImage(source));
  const softSkin=process.argv.includes('--soft-skin');
  const render=(...args)=>softSkin?renderer.renderSoft(...args):renderer.render(...args);
  const sheet=canvas.createCanvas(1728,576),sc=sheet.getContext('2d');sc.fillStyle='#eeeae4';sc.fillRect(0,0,sheet.width,sheet.height);
  for(let n=0;n<=8;n++){
    const t=n/8;sc.drawImage(render(t,{scale:2}),n*192,0);
    sc.drawImage(render(t,{scale:2,debug:true}),n*192,192);
    sc.drawImage(render(t,{scale:2,direction:'rise'}),n*192,384);
  }
  fs.writeFileSync(path.join(out,'joint-contact-sheet.png'),sheet.toBuffer('image/png'));
  // Evaluation only: reference pixels are drawn in a separate comparison
  // panel. They are never passed to the skeleton renderer or its frame export.
  const referencePath=path.join(__dirname,'assets/seated-final-10-2.png');
  const reference=canvas.createCanvas(96,96);reference.getContext('2d').drawImage(await canvas.loadImage(referencePath),3,-12);
  const comparison=canvas.createCanvas(768,440),cc=comparison.getContext('2d');cc.fillStyle='#eeeae4';cc.fillRect(0,0,768,440);
  cc.drawImage(reference,144,10);cc.drawImage(render(1),528,10);
  cc.imageSmoothingEnabled=false;cc.drawImage(reference,48,132,288,288);cc.drawImage(render(1),432,132,288,288);
  fs.writeFileSync(path.join(out,'reference-comparison.png'),comparison.toBuffer('image/png'));
  const inputs=['skeleton-sit-rig.js','skeleton-sit.js','export-skeleton-study.cjs'].map(f=>path.join(__dirname,f)).concat(source);
  fs.writeFileSync(path.join(out,'provenance.json'),JSON.stringify({sourceOnly:'dororong-canonical.png',skin:softSkin?'joint-bound reconstructed body':'rigid canonical cutouts',referenceOnly:{file:'assets/seated-final-10-2.png',sha256:crypto.createHash('sha256').update(fs.readFileSync(referencePath)).digest('hex')},purpose:'visual acceptance; no product bank replacement',sheetOnly:process.argv.includes('--sheet-only'),duration:15,fps:60,inputs:Object.fromEntries(inputs.map(f=>[path.relative(path.resolve(__dirname,'../..'),f),crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex')]))},null,2));
  if(process.argv.includes('--sheet-only'))return;
  fs.mkdirSync(path.join(out,'frames'),{recursive:true});
  const frame=canvas.createCanvas(768,440),ctx=frame.getContext('2d');
  const samples=new Map();
  function sample(time,direction,scale,debug){
    const index=Math.round(time*64),key=[index,direction,scale,debug].join(':');
    if(!samples.has(key)){
      const copy=canvas.createCanvas(96*scale,96*scale);
      copy.getContext('2d').drawImage(render(index/64,{direction,scale,debug}),0,0);samples.set(key,copy);
    }return samples.get(key);
  }
  for(let n=0;n<900;n++){
    const seconds=n/60,s=seconds<4.5?seconds:(seconds-4.5)/3;
    const direction=s>=2.5?'rise':'sit',time=direction==='sit'?Math.max(0,Math.min(1,(s-.5)/.95)):Math.max(0,Math.min(1,(s-2.5)/.75));
    ctx.fillStyle='#eeeae4';ctx.fillRect(0,0,768,440);
    for(let side=0;side<2;side++){
      ctx.drawImage(sample(time,direction,1,side===1),side*384+144,10);
      ctx.drawImage(sample(time,direction,3,side===1),side*384+48,132);
    }
    fs.writeFileSync(path.join(out,'frames',`${String(n).padStart(5,'0')}.png`),frame.toBuffer('image/png'));
  }
  console.log(`Exported canonical-only joint study to ${out}`);
})().catch(e=>{console.error(e);process.exitCode=1;});

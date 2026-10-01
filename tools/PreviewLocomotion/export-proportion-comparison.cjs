'use strict';
// Diagnostic comparison only: source art is read unmodified, never used by the rig.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {pose}=require('./connected-sit-rig.cjs'),{draw}=require('./connected-sit-blocking.cjs');
const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/connected-proportions-20261001'));
async function main(){
  const oldRig=require(path.join(out,'before-source/connected-sit-rig.cjs'));
  const oldDraw=require(path.join(out,'before-source/connected-sit-blocking.cjs')).draw;
  const references=await Promise.all([
    '../../src/Dororong.App/Assets/dororong-canonical.png','assets/seated-final-10-2.png'
  ].map(p=>canvas.loadImage(path.join(__dirname,p))));
  const sheet=canvas.createCanvas(1800,840),ctx=sheet.getContext('2d');
  ctx.fillStyle='#f3f0e9';ctx.fillRect(0,0,1800,840);
  // Compare soles actually rendered by each version. Do not carry an offset
  // from an older projection into subsequent reviews that share a projection.
  const soleRow=(render,rig)=>{
    const c=canvas.createCanvas(600,420),x=c.getContext('2d');render(x,rig.pose(0),{bones:false});
    const d=x.getImageData(0,0,600,420).data;let bottom=-1;
    for(let y=0;y<420;y++)for(let px=0;px<600;px++){
      const i=(y*600+px)*4;if([57,165,223].every((v,j)=>Math.abs(v-d[i+j])<3))bottom=y;
    }
    if(bottom<0)throw Error('Visible near fore sole required for comparison alignment');return bottom;
  };
  const oldYOffset=soleRow(draw,{pose})-soleRow(oldDraw,oldRig);
  for(let row=0;row<2;row++){
    // Same source-pixel scale in every column. Stand is untouched; seated source
    // registration (+3,-12) retains its head/feet alignment with canonical.
    ctx.save();ctx.translate(70,row*420-65);ctx.scale(5,5);
    ctx.imageSmoothingEnabled=false;ctx.drawImage(references[row],row?3:0,row?-12:0);ctx.restore();
    const old=canvas.createCanvas(600,420),next=canvas.createCanvas(600,420);
    oldDraw(old.getContext('2d'),oldRig.pose(row),{bones:false});
    draw(next.getContext('2d'),pose(row),{bones:false});
    // Align the standing soles, retaining this same offset for the seated row.
    ctx.save();ctx.beginPath();ctx.rect(600,row*420,600,420);ctx.clip();
    ctx.drawImage(old,600,row*420+oldYOffset);ctx.restore();
    ctx.drawImage(next,1200,row*420);
  }
  fs.mkdirSync(out,{recursive:true});
  fs.writeFileSync(path.join(out,'reference-before-after.png'),sheet.toBuffer('image/png'));
  const sourceFiles=[path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'),
    path.join(__dirname,'assets/seated-final-10-2.png'),__filename,
    ...['connected-sit-rig.cjs','connected-sit-blocking.cjs'].flatMap(f=>[path.join(__dirname,f),path.join(out,'before-source',f)])];
  fs.writeFileSync(path.join(out,'comparison-provenance.json'),JSON.stringify({
    columns:['unmodified reference','previous blocking','reproportioned blocking'],rows:['stand','sit'],
    pixelsPerSourceUnit:5,seatedRegistration:[3,-12],oldProjectionYOffset:oldYOffset/5,
    referenceLandmarks:{mainHeadEnvelope:[7,25,65,68],bellyY:75,nearForeSoleY:86,farForeSoleY:81},
    note:'Hand-read mass landmarks exclude hair tips and flower; comparison of approximate proportions, not final skin likeness or inferred anatomy.',
    inputs:Object.fromEntries(sourceFiles.map(f=>[path.relative(path.join(__dirname,'../..'),f).replaceAll('\\','/'),
      crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex')]))
  },null,2));
  console.log(path.join(out,'reference-before-after.png'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

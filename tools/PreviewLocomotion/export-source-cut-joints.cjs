'use strict';
const fs=require('node:fs'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {cutArtwork}=require('./source-cut-artwork.cjs');
const poses=[{}, {head:-8,farFront:-12,nearFront:12,nearHind:-18}, {head:8,farFront:12,nearFront:-12,nearHind:18}];
async function main(){
  const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/source-cut-joints-20261001'));fs.mkdirSync(out,{recursive:true});
  const c=canvas.createCanvas(864,1152),ctx=c.getContext('2d');ctx.imageSmoothingEnabled=false;
  const comparison=canvas.createCanvas(576,576),cx=comparison.getContext('2d');cx.imageSmoothingEnabled=false;
  const parts=canvas.createCanvas(1152,576),pc=parts.getContext('2d');pc.fillStyle='#202630';pc.fillRect(0,0,parts.width,parts.height);pc.imageSmoothingEnabled=false;
  for(const [index,[name,file]] of [['standing','../../src/Dororong.App/Assets/dororong-canonical.png'],['seated','assets/seated-final-10-2.png']].entries()){
    const rig=cutArtwork(await canvas.loadImage(path.join(__dirname,file)),name,canvas);
    for(let row=0;row<2;row++){
      const y=(index*2+row)*288;ctx.fillStyle=row?'#202630':'#fff';ctx.fillRect(0,y,864,288);
      for(let col=0;col<poses.length;col++)ctx.drawImage(rig.renderTraced(poses[col]),col*288,y,288,288);
    }
    for(let col=0;col<2;col++){
      cx.fillStyle='#202630';cx.fillRect(col*288,index*288,288,288);
      cx.drawImage(col===1?rig.renderTraced(poses[1]):rig.renderArticulated(poses[1],{hiddenSkin:false}),col*288,index*288,288,288);
    }
    rig.skinLayers.forEach((skin,i)=>{pc.drawImage(skin.image,i*288,index*288,288,288);fs.writeFileSync(path.join(out,`${name}-${skin.name}-hidden.png`),skin.image.toBuffer('image/png'));});
    fs.writeFileSync(path.join(out,`${name}-posed.png`),rig.renderTraced(poses[1]).toBuffer('image/png'));
  }
  for(const [file,img] of [['angle-check.png',c],['before-after.png',comparison],['hidden-parts.png',parts]])fs.writeFileSync(path.join(out,file),img.toBuffer('image/png'));
  fs.writeFileSync(path.join(out,'scope.json'),JSON.stringify({poses,
    method:'Original cutout coverage, authored hidden skin, locally bound joint strips; body exterior traced from assembled coverage, source head texture retained',
    sampledChecks:{poses:2,combinationsPerPose:81,headDegrees:[-8,0,8],forelegDegrees:[-12,0,12],hindfootDegrees:[-18,0,18]},
    differences:'The traced opaque body is not RGBA-identical. Rest silhouette is bounded to two native pixels; source seated paw ink has partial-alpha gaps.',
    limits:'Small rigid joint rotations only. No sit transition, limb bend, root translation or installation validated.'},null,2));
  console.log(out);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

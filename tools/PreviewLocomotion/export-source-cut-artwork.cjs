'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {cutArtwork}=require('./source-cut-artwork.cjs');
const backgrounds=['#fff','#202630'];
const pixels=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
function sheet(images,scale){
  const cell=96*scale,c=canvas.createCanvas(images.length*cell,2*cell),ctx=c.getContext('2d');
  ctx.imageSmoothingEnabled=false;
  for(let row=0;row<2;row++){
    ctx.fillStyle=backgrounds[row];ctx.fillRect(0,row*cell,c.width,cell);
    images.forEach((img,col)=>ctx.drawImage(img,col*cell,row*cell,cell,cell));
  }
  return c;
}
async function exportArtwork(outDir){
  if(typeof outDir!=='string'||!outDir.trim())throw Error('Output directory required');
  const out=path.resolve(outDir);fs.mkdirSync(path.join(out,'layers'),{recursive:true});
  const manifest={},checks={},images=[],sourceHashes={},files=[];
  const write=(name,c)=>{fs.writeFileSync(path.join(out,name),c.toBuffer('image/png'));files.push(name);};
  for(const [pose,relative] of [
    ['standing','../../src/Dororong.App/Assets/dororong-canonical.png'],
    ['seated','assets/seated-final-10-2.png']
  ]){
    const file=path.join(__dirname,relative),cut=cutArtwork(await canvas.loadImage(file),pose,canvas),rendered=cut.render();
    let differingPixels=0;const a=pixels(cut.original),b=pixels(rendered);
    for(let i=0;i<a.length;i+=4)if(a.subarray(i,i+4).some((v,n)=>v!==b[i+n]))differingPixels++;
    if(differingPixels)throw Error(`${pose}: source reconstruction changed ${differingPixels} pixels`);
    checks[pose]={differingPixels};sourceHashes[pose]=hash(file);
    manifest[pose]={size:[96,96],source:relative,sourceOffset:cut.sourceOffset,layers:[]};
    for(const layer of cut.layers){
      const file=`layers/${pose}-${layer.name}.png`;write(file,layer.image);
      manifest[pose].layers.push({name:layer.name,file,offset:layer.offset,size:[layer.image.width,layer.image.height]});
    }
    write(`${pose}.png`,rendered);images.push(rendered);
  }
  write('artwork-native.png',sheet(images,1));write('artwork-3x.png',sheet(images,3));
  const parts=canvas.createCanvas(5*192,2*192),pc=parts.getContext('2d');pc.fillStyle='#202630';pc.fillRect(0,0,parts.width,parts.height);pc.imageSmoothingEnabled=false;
  for(const [row,pose] of ['standing','seated'].entries())for(const [col,layer] of manifest[pose].layers.entries()){
    const img=await canvas.loadImage(path.join(out,layer.file));
    pc.drawImage(img,col*192+layer.offset[0]*2,row*192+layer.offset[1]*2,img.width*2,img.height*2);
  }
  write('parts.png',parts);
  fs.writeFileSync(path.join(out,'parts.json'),JSON.stringify(manifest,null,2));files.push('parts.json');
  fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify({
    method:'Original standing and seated source pixels, disjoint cropped layers, integer alignment only',
    sourceHashes,checks,visualAcceptance:'not claimed',animationGenerated:false,
    limits:['Static artwork only; hidden joint surfaces are not supplied by a source crop',
      'Separate seated source is used with the current user authorization; no endpoint morph',
      'Application and installed animation bank are not replaced']
  },null,2));files.push('verification.json');
  return {outDir:out,files,checks};
}
if(require.main===module)exportArtwork(process.argv[2]||path.join(__dirname,'../../artifacts/source-cut-artwork-20261001')).then(r=>console.log(JSON.stringify(r))).catch(e=>{console.error(e);process.exitCode=1;});
module.exports={exportArtwork};

'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs');
const {angleAt}=require('./grab-tilt-transition.cjs');
const dir=path.join(root,'artifacts/grab-tilt-transition-20261017-r37');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const m=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'),'utf8'));
 assert.equal(sha(path.join(__dirname,'grab-tilt-transition.cjs')),m.scriptSha256);
 for(const [f,h]of Object.entries(m.sourceHashes))assert.equal(sha(path.join(root,'artifacts/grab-contour-still-20261017-r33',f)),h);
 assert.throws(()=>angleAt(NaN));assert.throws(()=>angleAt(-1));assert.throws(()=>angleAt(97));
 // Reuse the independent chunk/CRC decoder, not the APNG encoder.
 const code=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8');
 const fragment=code.slice(code.indexOf('const table='),code.indexOf('async function main()'));
 const decode=new Function('assert',fragment+';return decodeParts;')(assert);
 let count=0;
 for(const sc of m.contract.scales){
  const frames=m.records.filter(r=>r.scale===sc),decoded=decode(fs.readFileSync(path.join(dir,`preview-${sc.toFixed(2)}.png`)));
  assert.equal(frames.length,97);assert.equal(decoded.length,97);
  let baseline;
  for(let i=0;i<97;i++){
   const r=frames[i];assert.equal(sha(path.join(dir,r.file)),r.sha256);assert.equal(r.angle,angleAt(i));
   const im=await api.loadImage(path.join(dir,r.file)),d=pixels(im,api);
   if(!i)baseline=d;
   // Lower body/feet are outside the full moving-head envelope.
   for(let y=Math.ceil(122*sc);y<im.height;y++)for(let x=0;x<im.width;x++){
    const k=(y*im.width+x)*4;assert.deepEqual(d.subarray(k,k+4),baseline.subarray(k,k+4));
   }
   // Reconstruct presentation independently from saved sprite PNG.
   const c=api.createCanvas(480,384),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(const [j,bg]of ['white','#252a31'].entries()){
    const x=j*240;g.fillStyle=bg;g.fillRect(x,0,240,384);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(`${sc}x / f${i} / held-pose tilt`,x+8,22);
    g.drawImage(im,x+Math.floor((240-im.width)/2),30);
    const sx=Math.floor(40*sc),sy=Math.floor(82*sc),sw=Math.ceil(62*sc),sh=Math.ceil(30*sc);
    g.drawImage(im,sx,sy,sw,sh,x+20,280,sw*2,sh*2);
   }
   assert.deepEqual(pixels(await api.loadImage(decoded[i]),api),pixels(c,api));count++;
  }
 }
 fs.writeFileSync(path.join(dir,'encoding-verification.json'),JSON.stringify({status:'TECHNICAL_ONLY',framesDecoded:count,crcSequenceAndPixelIdentity:true,lowerBodyPixelIdentity:true,sourceHashesUnchanged:true,manifestSha256:sha(path.join(dir,'manifest.json'))},null,2));
 console.log(JSON.stringify({decodedFrames:count,sourceHashesUnchanged:true,lowerBodyPixelIdentity:true}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

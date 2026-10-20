'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,surface}=require('./sampling.cjs');
const {create,dir,interpolated}=require('./grab-eight-source.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const manifest=JSON.parse(fs.readFileSync(path.join(dir,'sequence.json'),'utf8')),contract=JSON.parse(fs.readFileSync(path.join(dir,'contract.json'),'utf8'));
 assert.equal(sha(path.join(__dirname,'grab-eight-source.cjs')),manifest.sourceSha256);
 assert.equal(sha(path.join(__dirname,'grab-eight-sequence.cjs')),manifest.exportSha256);
 for(const [name,h]of Object.entries(contract.sources))assert.equal(sha(path.join(root,'src/Dororong.App/Assets/user-body-drag',name)),h);
 const headFile=path.join(root,'artifacts/grab-contour-still-20261017-r33/masked-head-225.png');assert.equal(sha(headFile),contract.headSha256);
 const m=await create(),hp=premultiply(await api.loadImage(headFile),api);
 assert.throws(()=>m.render(-.1));assert.throws(()=>m.render(1.1));assert.throws(()=>m.render(NaN));
 // Interpolation must reproduce all eight actual fitted key coordinates.
 for(let k=0;k<8;k++)for(const part of ['front','rear']){
  const q=interpolated(m.keys,part,k/7);for(let i=0;i<q.length;i++)for(let c=0;c<2;c++)assert(Math.abs(q[i][c]-m.keys[k][part][i][c])<1e-10);
  if(k&&k<7){const a=interpolated(m.keys,part,k/7-1e-8),b=interpolated(m.keys,part,k/7+1e-8);for(let i=0;i<a.length;i++)for(let c=0;c<2;c++)assert(Math.abs(a[i][c]-b[i][c])<1e-4);}
 }
 const decoderSource=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8'),fragment=decoderSource.slice(decoderSource.indexOf('const table='),decoderSource.indexOf('async function main()'));
 const decode=new Function('assert',fragment+';return decodeParts;')(assert),protectedCounts=[],animations=[];
 let saved=0,decodedCount=0;
 for(const sc of manifest.scales){
  const p=new Float64Array(4),head=surface(api,96*sc,(x,y)=>sample(hp,225,x*225/96-.5,y*225/96-.5,p),32),hd=pixels(head,api),protectedPixels=[];
  for(let y=1;y<head.height-1;y++)for(let x=1;x<head.width-1;x++){
   let opaque=true;for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++)if(hd[((y+dy)*head.width+x+dx)*4+3]!==255)opaque=false;
   if(opaque)protectedPixels.push((y*head.width+x)*4);
  }
  assert(protectedPixels.length>0);protectedCounts.push({scale:sc,erodedOpaqueHeadPixels:protectedPixels.length});
  const images=[];
  for(const rec of manifest.records.filter(r=>r.scale===sc)){
   const f=path.join(dir,rec.file);assert.equal(sha(f),rec.sha256);const im=await api.loadImage(f),d=pixels(im,api);images.push(im);
   for(const at of protectedPixels)assert.deepEqual(d.subarray(at,at+4),hd.subarray(at,at+4),'opaque head interior must remain exact');
   if(rec.index%8===0&&sc===1.5)assert.deepEqual(d,pixels(await api.loadImage(path.join(dir,`key-${rec.index/8}.png`)),api));
   saved++;
  }
  const anim=manifest.animations.find(a=>a.file===`slow-preview-${sc.toFixed(2)}.png`),decoded=decode(fs.readFileSync(path.join(dir,anim.file)));assert.equal(decoded.length,anim.order.length);
  for(let f=0;f<decoded.length;f++){
   const i=anim.order[f],c=api.createCanvas(480,300),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(const [j,bg]of ['white','#252a31'].entries()){
    g.fillStyle=bg;g.fillRect(j*240,0,240,300);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(`${sc}x / slowed entry / f${i}`,j*240+7,22);g.drawImage(images[i],j*240+(240-images[i].width)/2,35);
   }assert.deepEqual(pixels(await api.loadImage(decoded[f]),api),pixels(c,api));decodedCount++;
  }
  animations.push({file:anim.file,sha256:sha(path.join(dir,anim.file)),decodedFrames:decoded.length});
 }
 const result={status:'TECHNICAL_CHECKS_PASSED_NOT_PRODUCT_ACCEPTANCE',savedFramesVerified:saved,decodedPresentationFrames:decodedCount,protectedCounts,animations,keyAndIntermediateGeometryContinuity:true,sourceHashesUnchanged:true,limits:['Opaque head interior is tested; alpha edge against the moving body is visually reviewed separately.','Prior idle/held output boundaries remain different as reported in sequence.json.','Encoding checks do not establish live perceived timing or product acceptance.']};
 fs.writeFileSync(path.join(dir,'tests.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

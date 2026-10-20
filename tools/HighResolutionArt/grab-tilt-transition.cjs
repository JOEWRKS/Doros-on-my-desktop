'use strict';
// Isolated held-pose tilt transition. NOT idle-to-grab or a product entry point.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const {apng}=require('./grab-tilt-audit.cjs');
const dir=path.join(root,'artifacts/grab-tilt-transition-20261017-r37');
const source=path.join(root,'artifacts/grab-contour-still-20261017-r33');
const previous=path.join(root,'artifacts/grab-left-occlusion-20261017-r35');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const ease=t=>t*t*t*(10+t*(-15+6*t));
function angleAt(frame){
 assert(Number.isFinite(frame)&&frame>=0&&frame<=96);
 if(frame<=12)return 0;
 if(frame<=36)return -16*ease((frame-12)/24);
 if(frame<=72)return -16+32*ease((frame-36)/36);
 return 16*(1-ease((frame-72)/24));
}
async function create(){
 const manifest=JSON.parse(fs.readFileSync(path.join(previous,'manifest.json'),'utf8'));
 const codeFile=path.join(__dirname,'grab-left-occlusion.cjs'),code=fs.readFileSync(codeFile,'utf8');
 assert.equal(sha(codeFile),manifest.codeSha256);
 const verifySources=()=>{for(const [f,h]of Object.entries(manifest.contract.sourceHashes))assert.equal(sha(path.join(source,f)),h);};
 verifySources();
 const hp=premultiply(await api.loadImage(path.join(source,'masked-head-225.png')),api),bp=premultiply(await api.loadImage(path.join(source,'body-padded-375.png')),api);
 const rotate=([x,y],p,d)=>{const a=d*Math.PI/180,c=Math.cos(a),s=Math.sin(a);return [p[0]+(x-p[0])*c-(y-p[1])*s,p[1]+(x-p[0])*s+(y-p[1])*c];};
 const fragment=code.slice(code.indexOf(' function render('),code.indexOf(' const save='));
 const render=new Function('api','hp','bp','sample','over','surface','rotate',fragment+';return render;')(api,hp,bp,sample,over,surface,rotate);
 return {render:(scale,frame)=>render(scale,angleAt(frame),true),verifySources,sourceHashes:manifest.contract.sourceHashes,compositorSha256:manifest.codeSha256};
}
async function main(){
 const m=await create();fs.mkdirSync(dir,{recursive:true});
 const scales=[1,1.25,1.5],records=[],sheets=[],animations=[];
 const contract={scope:'Held-neutral -> left tilt -> right tilt -> held-neutral only',
  idleToGrabConnected:false,runtimeChanged:false,artworkChanged:false,
  preserve:['r33 face/head/tips/body source files','r35 compositor, sampling and local occlusion'],
  fps:30,frames:97,scales,angleRange:[-16,16],
  criteria:['Neutral endpoints equal accepted r33 at 1.5x','No frame border clipping','No visible attachment gap or added tip defect in saved frames'],
  exclusions:['idle-to-grab body transition','release/landing','other interactions','runtime playback'],
  skillGate:'Native palette/binary-alpha conversion gate unsupported for this existing partial-alpha compositor; source hash, saved pixel and frame inspection checks used instead'};
 fs.writeFileSync(path.join(dir,'contract.json'),JSON.stringify(contract,null,2));
 assert.equal(angleAt(0),0);assert.equal(angleAt(12),0);assert.equal(angleAt(36),-16);assert.equal(angleAt(72),16);assert.equal(angleAt(96),0);
 // Analytic quintic segments have zero first/second derivatives at each join.
 for(const f of [12,36,72,96]){const e=1e-4,a=angleAt(f),b=angleAt(Math.max(0,f-e)),c=angleAt(Math.min(96,f+e));assert(Math.abs(a-b)<1e-8&&Math.abs(c-a)<1e-8);}
 for(const sc of scales){
  const imgs=[],packed=[];
  for(let i=0;i<=96;i++){
   const name=`frame-${sc.toFixed(2)}-${String(i).padStart(3,'0')}.png`,c=m.render(sc,i),file=path.join(dir,name);
   fs.writeFileSync(file,c.toBuffer('image/png'));const im=await api.loadImage(file),d=pixels(im,api);imgs.push(im);
   assert.deepEqual(d,pixels(c,api));let border=0;
   for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if((!x||!y||x===im.width-1||y===im.height-1)&&d[(y*im.width+x)*4+3])border++;
   assert.equal(border,0);records.push({file:name,scale:sc,frame:i,angle:angleAt(i),sha256:sha(file),borderAlpha:border});
  }
  assert.deepEqual(pixels(imgs[0],api),pixels(imgs[96],api));
  if(sc===1.5)assert.deepEqual(pixels(imgs[0],api),pixels(await api.loadImage(path.join(source,'candidate.png')),api));
  for(let start=0;start<97;start+=18){
   const n=Math.min(18,97-start),c=api.createCanvas(1200,Math.ceil(n/6)*430),g=c.getContext('2d');
   g.fillStyle='#ddd';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
   for(let j=0;j<n;j++){
    const i=start+j,x=j%6*200,y=Math.floor(j/6)*430,im=imgs[i];
    g.fillStyle='#222';g.font='13px sans-serif';g.fillText(`${sc}x f${i} ${angleAt(i).toFixed(3)}deg`,x+4,y+16);
    // Tight full-character crop at actual output size; no resampling.
    const ox=Math.floor(24*sc),oy=Math.floor(24*sc),w=Math.ceil(120*sc);
    for(const [k,bg]of ['white','#252a31'].entries()){
     g.fillStyle=bg;g.fillRect(x,y+24+k*200,198,198);
     g.drawImage(im,ox,oy,w,w,x+Math.floor((198-w)/2),y+25+k*200,w,w);
    }
   }
   const file=`sheet-${sc.toFixed(2)}-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push(file);
  }
  for(let i=0;i<97;i++){
   const c=api.createCanvas(480,384),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(const [j,bg]of ['white','#252a31'].entries()){
    const x=j*240;g.fillStyle=bg;g.fillRect(x,0,240,384);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(`${sc}x / f${i} / held-pose tilt`,x+8,22);
    g.drawImage(imgs[i],x+Math.floor((240-imgs[i].width)/2),30);
    const sx=Math.floor(40*sc),sy=Math.floor(82*sc),sw=Math.ceil(62*sc),sh=Math.ceil(30*sc);
    g.drawImage(imgs[i],sx,sy,sw,sh,x+20,280,sw*2,sh*2);
   }packed.push(c.toBuffer('image/png'));
  }
  const file=`preview-${sc.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,file),apng(packed));animations.push({file,frames:97,sha256:sha(path.join(dir,file))});
  console.log(`saved ${sc}x, 97 frames`);
 }
 m.verifySources();
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_CHECKS_ONLY_VISUAL_REVIEW_SEPARATE',contract,sourceHashes:m.sourceHashes,compositorSha256:m.compositorSha256,scriptSha256:sha(__filename),records,sheets,animations,sourceHashesUnchanged:true,neutralEndpointIdentity:true},null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={create,angleAt};

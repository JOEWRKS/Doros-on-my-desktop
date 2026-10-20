'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const out=path.resolve(process.argv[2]),before=path.join(root,'artifacts/face-boundaries-20261018-r66/final2');
async function pixels(file){const im=await api.loadImage(fs.readFileSync(file)),c=api.createCanvas(im.width,im.height),g=c.getContext('2d');g.drawImage(im,0,0);return {im,data:g.getImageData(0,0,im.width,im.height).data};}
async function main(){
 const comparisons=[];
 for(const scale of [1,1.1,1.25,1.35,1.5])for(const tag of ['neutral','protected-cheek--2','protected-cheek-0','protected-cheek-10','protected-cheek-20']){
  const name=`gaze-${scale.toFixed(2)}-${tag}.png`,a=await pixels(path.join(before,name)),b=await pixels(path.join(out,name));let count=0,outsideEye=0;
  for(let y=0;y<a.im.height;y++)for(let x=0;x<a.im.width;x++){
   const k=(y*a.im.width+x)*4;if(a.data.subarray(k,k+4).every((v,j)=>v===b.data[k+j]))continue;
   count++;const qx=(x+.5)/scale-80,qy=(y+.5)/scale-80;
   if(qx<16||qx>29||qy<48||qy>58.5)outsideEye++;
  }comparisons.push({name,count,outsideEye});
 }
 const proof={comparisons,outsideEye:comparisons.reduce((s,r)=>s+r.outsideEye,0),sourceFiles:[],nativeInteraction:'UNVERIFIED'};
 const lock=JSON.parse(fs.readFileSync(path.join(root,'artifacts/gaze-rim-20261018-r65/final/preservation.json')));
 for(const s of lock.sources){const hash=crypto.createHash('sha256').update(fs.readFileSync(path.join(root,s.file))).digest('hex');proof.sourceFiles.push({file:s.file,unchanged:hash===s.sha256,sha256:hash});}
 fs.writeFileSync(path.join(out,'r66-preservation.json'),JSON.stringify(proof,null,2));console.log(JSON.stringify(proof));
 const c=api.createCanvas(960,640),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;g.font='16px sans-serif';
 for(const [row,[dir,label]] of [[before,'Before r66: fixed eye outline'],[out,'Candidate r67: whole-eye movement']].entries()){
  g.fillStyle='#222';g.fillText(label,12,row*320+22);
  for(const [col,[file,title]] of [['gaze-1.50-d6-s4.png','Up'],['gaze-1.50-neutral.png','Neutral'],['gaze-1.50-d2-s4.png','Down']].entries()){
   const {im}=await pixels(path.join(dir,file));const x=col*320,y=row*320;
   g.fillText(title,x+12,y+48);g.drawImage(im,144,189,46,29,x+12,y+62,276,174);
   g.drawImage(im,120,155,144,135,x+100,y+240,86.4,81);
  }
 }fs.writeFileSync(path.join(out,'before-after.png'),c.toBuffer('image/png'));
 if(proof.outsideEye||proof.sourceFiles.some(s=>!s.unchanged))process.exitCode=1;
}
main().catch(e=>{console.error(e);process.exitCode=1});

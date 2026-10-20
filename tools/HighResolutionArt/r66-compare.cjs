'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const out=path.resolve(process.argv[2]);
async function pixels(file){const im=await api.loadImage(fs.readFileSync(file)),c=api.createCanvas(im.width,im.height),g=c.getContext('2d');g.drawImage(im,0,0);return {im,data:g.getImageData(0,0,im.width,im.height).data};}
async function main(){
 const before=path.join(root,'artifacts/gaze-rim-20261018-r65/final');
 const c=api.createCanvas(900,660),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;g.font='16px sans-serif';
 for(const [j,[file,label]] of [['gaze-1.50-d6-s4.png','Upward tracking: lower eye line'],['cheek-20-0.png','Cheek pull: mouth']].entries()){
  for(const [i,[dir,name]] of [[before,'Before r65'],[out,'Candidate r66']].entries()){
   const {im}=await pixels(path.join(dir,file));const x=i*450,y=j*330;
   g.fillStyle='#222';g.fillText(`${name} / ${label}`,x+10,y+20);
   g.drawImage(im,144,189,46,29,x+10,y+35,414,261);
  }
 }
 fs.writeFileSync(path.join(out,'before-after.png'),c.toBuffer('image/png'));
 const comparisons=[];
 for(const scale of [1,1.1,1.25,1.35,1.5])for(const tag of ['neutral','protected-cheek--2','protected-cheek-0','protected-cheek-10','protected-cheek-20']){
  const name=`gaze-${scale.toFixed(2)}-${tag}.png`,a=await pixels(path.join(before,name)),b=await pixels(path.join(out,name));let count=0,outsideMouth=0;
  for(let y=0;y<a.im.height;y++)for(let x=0;x<a.im.width;x++){
   let k=(y*a.im.width+x)*4;if(a.data.subarray(k,k+4).every((v,j)=>v===b.data[k+j]))continue;
   count++;const qx=(x+.5)/scale-80,qy=(y+.5)/scale-80;
   if(qx<24||qx>35||qy<56||qy>64)outsideMouth++;
  }
  comparisons.push({name,count,outsideMouth});
 }
 const proof={comparisons,outsideMouth:comparisons.reduce((s,r)=>s+r.outsideMouth,0),sourceFiles:[],nativeInteraction:'UNVERIFIED'};
 const lock=JSON.parse(fs.readFileSync(path.join(root,'artifacts/gaze-rim-20261018-r65/final/preservation.json')));
 for(const s of lock.sources){const hash=crypto.createHash('sha256').update(fs.readFileSync(path.join(root,s.file))).digest('hex');proof.sourceFiles.push({file:s.file,unchanged:hash===s.sha256,sha256:hash});}
 fs.writeFileSync(path.join(out,'r65-preservation.json'),JSON.stringify(proof,null,2));console.log(JSON.stringify(proof));
}
main().catch(e=>{console.error(e);process.exitCode=1});

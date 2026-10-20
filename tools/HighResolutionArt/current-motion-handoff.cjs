'use strict';
const fs=require('node:fs'),path=require('node:path');
const {create,sequence}=require('./current-motion.cjs');
const {root}=require('./bangs-composite-review.cjs');
async function main(){
 const dir=path.join(root,'artifacts/accepted-motion-connection-20261017-r28'),m=await create(),api=m.s.api;
 const manifest=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'))),cache=new Map(),video=path.join(dir,'handoff-video-frames');fs.mkdirSync(video,{recursive:true});
 for(let n=0;n<300;n++){
  const c=api.createCanvas(768,576),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);
  for(const [i,sc]of manifest.scenarios.entries()){
   const at=Math.min(sc.count-1,Math.floor((n/60%(sc.seconds+.25))*60)),f=manifest.frames[sc.start+at];
   if(!cache.has(f.name))cache.set(f.name,await api.loadImage(path.join(dir,f.name+'.png')));
   g.drawImage(cache.get(f.name),i%4*192,Math.floor(i/4)*192);
  }fs.writeFileSync(path.join(video,String(n).padStart(3,'0')+'.png'),c.toBuffer('image/png'));
 }
 const cases=[['track',0,{roll:-.349}],['track',0,{roll:.244}],['sleep',.55],['sleep',1],['happy',.54],['run',.4],['body-click',.4],['jump',.35,{lift:15,snackAge:.3}]];
 for(const scale of [1,1.25,1.5]){
  const model=await create({size:96*scale}),c=api.createCanvas(960,520),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,c.width,c.height);
  for(const [i,args]of cases.entries()){
   const im=model.render(...args),x=i%4*240,y=Math.floor(i/4)*260;g.drawImage(im,x+(240-im.width)/2,y+8);
   g.fillStyle='#302b33';g.font='13px sans-serif';g.fillText(args[0]+' '+scale+'x',x+15,y+242);
   fs.writeFileSync(path.join(dir,`scale-${scale}-${i}.png`),im.toBuffer('image/png'));
  }fs.writeFileSync(path.join(dir,`scale-review-${scale}.png`),c.toBuffer('image/png'));
 }
 console.log(sequence.length+' scenarios; 24 scale samples');
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={main};

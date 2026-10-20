'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {setup}=require('./accepted-motion.cjs'),{cycleSeconds}=require('./motion-polish.cjs');
const {api,root}=require('./bangs-composite-review.cjs');
const out=path.join(root,'artifacts/high-resolution-motion-20261017-r18');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 fs.mkdirSync(out,{recursive:true});const s=await setup({polished:true,fixedRunRoots:true});
 const frames=[];
 for(const eye of ['open','closed'])for(const amount of [1,.25,.5,.75]){
  const count=amount===1?48:12;
  for(let n=0;n<count;n++){
   const name=`run-${eye}-${amount===1?'full':Math.round(amount*100)}-${String(n).padStart(2,'0')}`,parameters={amount,phase:n/count,walking:false};
   const b=s.rigs[eye].render(parameters).toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),b);frames.push({name,eye,parameters,sha256:sha(b)});
  }
 }
 const index=[];
 for(let start=0;start<frames.length;start+=12){
  const batch=frames.slice(start,start+12),c=api.createCanvas(960,660),g=c.getContext('2d');g.fillStyle=start%24===0?'#eeeae4':'#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,f] of batch.entries()){
   const im=await api.loadImage(path.join(out,f.name+'.png')),x=i%4*240,y=Math.floor(i/4)*220;
   g.drawImage(im,40,92,104,74,x,y,208,148);g.drawImage(im,x+120,y+110,96,96);
   g.fillStyle=start%24===0?'#302b33':'#fff';g.font='12px sans-serif';g.fillText(f.name,x+4,y+207);
  }
  const file=`review-${String(start/12+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,file),c.toBuffer('image/png'));index.push({file,frames:batch.map(f=>f.name)});
 }
 const sample=api.createCanvas(768,384),g=sample.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,768,384);g.imageSmoothingEnabled=false;
 for(let i=0;i<4;i++){
  const n=[0,12,24,36][i],old=await api.loadImage(path.join(root,`artifacts/high-resolution-motion-20261017-r17/run-open-${String(n).padStart(2,'0')}.png`)),now=await api.loadImage(path.join(out,`run-open-full-${String(n).padStart(2,'0')}.png`));g.drawImage(old,i*192,0);g.drawImage(now,i*192,192);
 }fs.writeFileSync(path.join(out,'comparison.png'),sample.toBuffer('image/png'));
 const sources={};for(const f of ['rooted-run-review.cjs','motion-polish.cjs','accepted-motion.cjs','upright.cjs','body.cjs','sampling.cjs','../PreviewLocomotion/reference-parts.cjs']){const p=path.relative(root,path.join(__dirname,f));sources[p]=sha(fs.readFileSync(path.join(root,p)));}
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify({runtimeSelected:false,cycleSeconds,frames,sources},null,2));fs.writeFileSync(path.join(out,'review-index.json'),JSON.stringify(index,null,2));
 const dir=path.join(out,'video-frames');fs.mkdirSync(dir,{recursive:true});const cache=new Map();
 for(let n=0;n<240;n++){
  const f=Math.floor((n/60/cycleSeconds%1)*48),c=api.createCanvas(384,192),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,384,192);
  for(const [i,eye]of ['open','closed'].entries()){const name=`run-${eye}-full-${String(f).padStart(2,'0')}`;if(!cache.has(name))cache.set(name,await api.loadImage(path.join(out,name+'.png')));g.drawImage(cache.get(name),i*192,0);}fs.writeFileSync(path.join(dir,String(n).padStart(3,'0')+'.png'),c.toBuffer('image/png'));
 }
 console.log(`${frames.length} frames: ${out}`);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={out,main};

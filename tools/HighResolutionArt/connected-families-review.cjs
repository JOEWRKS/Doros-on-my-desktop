'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create}=require('./connected-families.cjs');
const {scenarios,draw}=require('./connected-scenarios.cjs');
const {root}=require('./bangs-composite-review.cjs');
const out=path.join(root,'artifacts/high-resolution-connected-20261017-r19');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main({outDir=out,options={},sequence=scenarios,full=false}={}){
 const out=outDir,m=await create(options);fs.mkdirSync(out,{recursive:true});
 const entries=[['idle',0],['walk',.4],['run',.4],['sit',.5],['sit',1],['hunt',1],['jump',.18],['eat',.2],['eat',.68],['happy',.46],['track',0,{roll:.2}],['track',0,{roll:-.3}],['sleep',0],['sleep',.25],['sleep',.5],['sleep',.75],['sleep',1],['wake',1]];
 const c=m.s.api.createCanvas(1152,648),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);
 for(const [i,[kind,t,options]]of entries.entries()){const im=m.render(kind,t,options);g.drawImage(im,i%6*192,Math.floor(i/6)*216);g.fillStyle='#fff';g.font='12px sans-serif';g.fillText(kind+' '+t,i%6*192+8,Math.floor(i/6)*216+204);}
 fs.writeFileSync(path.join(out,'sample.png'),c.toBuffer('image/png'));console.log(out);
 if(!full&&!process.argv.includes('--full'))return;
 const manifest={runtimeSelected:false,fps:60,sources:{},frames:[],scenarios:[]};
 for(const file of ['connected-families.cjs','connected-scenarios.cjs','connected-families-review.cjs','accepted-motion.cjs','motion-polish.cjs','upright.cjs','posture.cjs','parts.cjs','body.cjs','sampling.cjs','happy.cjs','pointed-tip.cjs','accepted-sleep.cjs','snack-effects.cjs','body-click.cjs','current-motion.cjs','../PreviewLocomotion/reference-parts.cjs','../PreviewLocomotion/hunt-motion.js']){
  const p=path.relative(root,path.join(__dirname,file));manifest.sources[p]=sha(fs.readFileSync(path.join(root,p)));
 }
 for(const sc of sequence){
  const last=Math.ceil(sc.seconds*60),start=manifest.frames.length;
  for(let n=0;n<=last;n++){
   const time=Math.min(n/60,sc.seconds),parameters=sc.at(time),name=`${sc.name}-${String(n).padStart(3,'0')}`;
   const b=draw(m,parameters).toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),b);manifest.frames.push({name,scenario:sc.name,time,parameters,sha256:sha(b)});
  }
  manifest.scenarios.push({name:sc.name,seconds:sc.seconds,start,count:last+1});console.log(sc.name+' '+(last+1));
 }
 const index=[];
 for(let start=0;start<manifest.frames.length;start+=24){
  const c=m.s.api.createCanvas(1152,864),g=c.getContext('2d'),batch=manifest.frames.slice(start,start+24),light=start%48===0;
  g.fillStyle=light?'#eeeae4':'#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,f]of batch.entries()){
   const im=await m.s.api.loadImage(path.join(out,f.name+'.png')),x=i%6*192,y=Math.floor(i/6)*216;
   g.drawImage(im,37,85,108,84,x,y,189,147);g.drawImage(im,x+100,y+116,80,80);g.fillStyle=light?'#302b33':'#fff';g.font='10px sans-serif';g.fillText(f.name,x+3,y+207);
  }
  const file=`review-${String(start/24+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,file),c.toBuffer('image/png'));index.push({file,frames:batch.map(f=>f.name)});
 }
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2));fs.writeFileSync(path.join(out,'review-index.json'),JSON.stringify(index,null,2));
 const video=path.join(out,'video-frames');fs.mkdirSync(video,{recursive:true});const cache=new Map();
 const selected=['idle-run-idle','walk-run-walk','sit-rise','hunt-recover','jump-land','eat-happy','sleep-wake','breathing'].map(name=>manifest.scenarios.find(s=>s.name===name));
 for(let n=0;n<300;n++){
  const c=m.s.api.createCanvas(768,384),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);
  for(const [i,sc]of selected.entries()){
   const at=Math.min(sc.count-1,Math.floor((n/60%(sc.seconds+.25))*60)),f=manifest.frames[sc.start+at];
   if(!cache.has(f.name))cache.set(f.name,await m.s.api.loadImage(path.join(out,f.name+'.png')));g.drawImage(cache.get(f.name),i%4*192,Math.floor(i/4)*192);
  }fs.writeFileSync(path.join(video,String(n).padStart(3,'0')+'.png'),c.toBuffer('image/png'));
 }
 console.log(manifest.frames.length+' transition frames');
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={main,out};

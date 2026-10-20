'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create,sequences}=require('./direct-motion.cjs');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){const m=await create(),{api,root}=m.common.s,dir=path.join(root,'artifacts/direct-motion-20261017-r29');fs.mkdirSync(dir,{recursive:true});
 const report={runtimeSelected:false,fps:60,frames:[],sheets:[],sources:{},sequences:[]};
 for(const f of ['direct-motion.cjs','direct-motion-review.cjs','grab-motion.cjs','landing-motion.cjs','current-motion.cjs','connected-families.cjs','pointed-tip.cjs','parts.cjs','body.cjs','sampling.cjs'])report.sources[f]=sha(fs.readFileSync(path.join(__dirname,f)));
 for(const sc of sequences){const start=report.frames.length,last=Math.ceil(sc.seconds*60);for(let n=0;n<=last;n++){
  const t=Math.min(n/60,sc.seconds),args=sc.at(t),im=m.render(...args),bytes=im.toBuffer('image/png'),name=sc.name+'-'+String(n).padStart(3,'0');
  fs.writeFileSync(path.join(dir,name+'.png'),bytes);report.frames.push({name,args,width:im.width,height:im.height,sha256:sha(bytes)});
 }report.sequences.push({name:sc.name,start,count:last+1,seconds:sc.seconds});console.log(sc.name);}
 for(let start=0;start<report.frames.length;start+=20){const c=api.createCanvas(1200,1080),g=c.getContext('2d'),frames=report.frames.slice(start,start+20),light=start%40===0;
  g.fillStyle=light?'#eeeae4':'#252a31';g.fillRect(0,0,c.width,c.height);
  for(const [i,f]of frames.entries()){g.drawImage(await api.loadImage(path.join(dir,f.name+'.png')),i%5*240,Math.floor(i/5)*270);g.fillStyle=light?'#302b33':'white';g.font='12px sans-serif';g.fillText(f.name,i%5*240+8,Math.floor(i/5)*270+257);}
  const file='review-'+String(start/20+1).padStart(2,'0')+'.png';fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));report.sheets.push({file,frames:frames.map(f=>f.name)});
 }
 const video=path.join(dir,'video-frames');fs.mkdirSync(video,{recursive:true});const cache=new Map();
 for(let n=0;n<150;n++){const c=api.createCanvas(720,240),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,720,240);
  for(const [i,sc]of report.sequences.entries()){const f=report.frames[sc.start+Math.min(sc.count-1,Math.floor((n/60%(sc.seconds+.4))*60))];if(!cache.has(f.name))cache.set(f.name,await api.loadImage(path.join(dir,f.name+'.png')));g.drawImage(cache.get(f.name),i*240,0);}fs.writeFileSync(path.join(video,String(n).padStart(3,'0')+'.png'),c.toBuffer('image/png'));
 }
 for(const scale of [1,1.25,1.5]){const c=api.createCanvas(1200,540),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,c.width,c.height);
  const cases=[['grab',0],['grab',.35],['grab',.7],['grab',1],['grab',1,{angle:1.53}],['grab',1,{angle:-1.53}],['hard-land',0],['hard-land',1,{squash:.574}],['hard-land',.4,{squash:-.16,lift:20}],['grab',0,{compression:.18}]];
  for(const [i,[k,t,o]]of cases.entries()){const im=m.render(k,t,{...o,size:96*scale});g.drawImage(im,i%5*240+(240-im.width)/2,Math.floor(i/5)*270);g.fillStyle='#302b33';g.font='12px sans-serif';g.fillText(k+' '+t+' '+scale+'x',i%5*240+8,Math.floor(i/5)*270+257);}fs.writeFileSync(path.join(dir,`scale-${scale}.png`),c.toBuffer('image/png'));
 }
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify(report,null,2));console.log(report.frames.length+' frames');
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={main};

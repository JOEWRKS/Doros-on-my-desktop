'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {setup}=require('./accepted-motion.cjs'),{cycleSeconds}=require('./motion-polish.cjs');
const {api,root}=require('./bangs-composite-review.cjs');
const out=path.join(root,'artifacts/high-resolution-motion-20261017-r17');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function sample(s){
 const c=api.createCanvas(1152,660),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
 const old=await api.loadImage(path.join(root,'artifacts/high-resolution-motion-20261016-r16/sit-closed-64.png'));
 for(const [i,im] of [old,s.postures.closed.render(1)].entries()){g.drawImage(im,70,125,78,44,i*384+20,30,312,176);g.fillStyle='#333';g.font='14px sans-serif';g.fillText(i?'Revised sit':'Previous sit',i*384+20,20);}
 for(let n=0;n<12;n++){const im=s.rigs.open.render({amount:1,phase:n/12,walking:false}),x=n%6*192,y=220+Math.floor(n/6)*220;g.drawImage(im,x,y);g.fillStyle='#333';g.fillText(`Run ${n}/12`,x+20,y+195);}
 fs.writeFileSync(path.join(out,'sample.png'),c.toBuffer('image/png'));
}
async function main(){
 fs.mkdirSync(out,{recursive:true});const s=await setup({polished:true});await sample(s);if(process.argv.includes('--sample'))return;
 const m={schema:1,runtimeSelected:false,cycleSeconds,previousPreviewCycleSeconds:1.6,sourceSize:225,outputSize:192,origin:[24,24],sources:{},frames:[]};
 for(const f of ['polished-motion-review.cjs','motion-polish.cjs','accepted-motion.cjs','posture.cjs','upright.cjs','body.cjs','sampling.cjs','parts.cjs','../PreviewLocomotion/reference-parts.cjs']){const p=path.relative(root,path.join(__dirname,f));m.sources[p]=sha(fs.readFileSync(path.join(root,p)));}
 for(const f of ['candidate-225.png','assembly-source-225.png','head-layer-225.png']){const p='artifacts/high-resolution-cheek-20261016-r15/'+f;m.sources[p]=sha(fs.readFileSync(path.join(root,p)));}
 function save(name,c,parameters){const b=c.toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),b);m.frames.push({name,parameters,sha256:sha(b)});}
 for(const eye of ['open','closed']){
  for(let n=0;n<48;n++)save(`run-${eye}-${String(n).padStart(2,'0')}`,s.rigs[eye].render({amount:1,phase:n/48,walking:false}),{amount:1,phase:n/48,walking:false});
  for(const kind of ['sit','rise'])for(let n=0;n<=64;n++){const t=kind==='sit'?n/64:1-n/64;save(`${kind}-${eye}-${String(n).padStart(2,'0')}`,s.postures[eye].render(t),{posture:t});}
  console.log(eye+' exported');
 }
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(m,null,2)+'\n');
 const index=[];
 for(let start=0;start<m.frames.length;start+=24){const batch=m.frames.slice(start,start+24),c=api.createCanvas(1152,864),g=c.getContext('2d'),light=start%48===0;g.fillStyle=light?'#eeeae4':'#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,f] of batch.entries()){const x=i%6*192,y=Math.floor(i/6)*216;g.drawImage(await api.loadImage(path.join(out,f.name+'.png')),x,y);g.fillStyle=light?'#333':'#fff';g.font='12px sans-serif';g.fillText(f.name,x+8,y+204);}
  const file=`review-${String(start/24+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,file),c.toBuffer('image/png'));index.push({file,frames:batch.map(f=>f.name)});
 }
 // Inspect every changed seated hind contour and gallop leg set at 2x.
 for(const kind of ['sit','run']){const list=m.frames.filter(f=>f.name.startsWith(kind+'-open-'));for(let start=0;start<list.length;start+=24){const batch=list.slice(start,start+24),c=api.createCanvas(1152,624),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,f] of batch.entries()){const x=i%6*192,y=Math.floor(i/6)*156;g.drawImage(await api.loadImage(path.join(out,f.name+'.png')),40,112,90,60,x,y,180,120);g.fillStyle='#333';g.font='12px sans-serif';g.fillText(f.name,x+8,y+143);}
  const file=`detail-${kind}-${start/24+1}.png`;fs.writeFileSync(path.join(out,file),c.toBuffer('image/png'));index.push({file,detail:true,frames:batch.map(f=>f.name)});
 }}
 fs.writeFileSync(path.join(out,'review-index.json'),JSON.stringify(index,null,2)+'\n');
 const dir=path.join(out,'video-frames');fs.mkdirSync(dir,{recursive:true});const cache=new Map(),load=async n=>{if(!cache.has(n))cache.set(n,await api.loadImage(path.join(out,n+'.png')));return cache.get(n);};
 for(let n=0;n<360;n++){const age=n/60,sit=Math.max(0,Math.min(1,age<2?(age-.4)/1.05:1-(age-2.6)/.65));const run=Math.floor((age/cycleSeconds%1)*48),items=[`sit-closed-${String(Math.round(sit*64)).padStart(2,'0')}`,`run-open-${String(run).padStart(2,'0')}`];
  const c=api.createCanvas(384,192),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,384,192);for(const [i,f] of items.entries())g.drawImage(await load(f),i*192,0);fs.writeFileSync(path.join(dir,String(n).padStart(3,'0')+'.png'),c.toBuffer('image/png'));
 }
 console.log(m.frames.length+' frames exported to '+out);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={main,out};

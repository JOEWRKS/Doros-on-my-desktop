'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs'),{diff}=require('./grab-entry-transport.cjs');
const {dir,duration,pose,create}=require('./grab-angled-release.cjs'),{presentation,lowest}=require('./grab-release-review.cjs'),{apng}=require('./grab-tilt-audit.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create(),prior=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-release-20261017-r45/manifest.json'))),sources={...prior.sources};
 for(const f of ['tools/HighResolutionArt/grab-angled-release.cjs','tools/HighResolutionArt/grab-angled-release-review.cjs','tools/HighResolutionArt/landing-motion.cjs'])sources[f]=sha(path.join(root,f));
 for(const [f,h]of Object.entries(prior.sources))assert.equal(sha(path.join(root,f)),h);
 const frames=[],sheets=[],animations=[],endpoints=[],geometry=[],near=[];
 for(const sc of [1,1.25,1.5])for(const angle of [-16,16])for(const hard of [false,true]){
  const id=`${hard?'hard':'normal'}-${angle<0?'left':'right'}-${sc.toFixed(2)}`,count=Math.ceil(duration(hard)*30),list=[],packed=[],floor=lowest(m.render(duration(hard),angle,hard,sc));
  for(let i=0;i<=count;i++){
   const t=Math.min(duration(hard),i/30),p=pose(t,angle,hard),im=m.render(t,angle,hard,sc),file=`${id}-${String(i).padStart(3,'0')}.png`;
   fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));const saved=await api.loadImage(path.join(dir,file));assert.deepEqual(pixels(saved,api),pixels(im,api));assert(lowest(saved)<=floor,'floor '+file);
   const d=pixels(saved,api);for(let n=0;n<im.width;n++)for(const [x,y]of [[n,0],[n,im.height-1],[0,n],[im.width-1,n]])assert.equal(d[(y*im.width+x)*4+3],0);
   if(t>=.08){const same=diff(m.render(t,angle,hard,sc,{positioned:false}),m.previous.previous.entry.render(p.progress,sc));assert.equal(same.changedPixels,0);geometry.push({id,index:i,...same});}
   frames.push({file,id,index:i,scale:sc,angle,hard,seconds:t,pose:p,sha256:sha(path.join(dir,file))});list.push(saved);packed.push(presentation(saved,sc,`${hard?'HARD':'NORMAL'} ${angle}deg / f${i}`,floor).toBuffer('image/png'));
  }
  const start=diff(list[0],m.previous.previous.tilt.render(sc,angle<0?36:72));assert.equal(start.changedPixels,0);endpoints.push({id,name:'tilted-held-start',...start});
  const end=diff(list.at(-1),m.previous.render(.44,sc));assert.equal(end.changedPixels,0);endpoints.push({id,name:'r45-settled-idle',...end});
  for(let from=0;from<list.length;from+=18){const n=Math.min(18,list.length-from),c=api.createCanvas(1200,Math.ceil(n/6)*510),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let j=0;j<n;j++){const i=from+j,x=j%6*200,y=Math.floor(j/6)*510,im=list[i],sx=Math.floor(24*sc),w=Math.ceil(112*sc);g.fillStyle='#ddd';g.fillRect(x,y,200,510);g.fillStyle='#222';g.font='12px sans-serif';g.fillText(`${sc} ${hard?'H':'N'} ${angle} f${i}`,x+4,y+16);
    for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+k*244,198,242);g.strokeStyle='#999';g.beginPath();g.moveTo(x+15,y+22+k*244+floor);g.lineTo(x+185,y+22+k*244+floor);g.stroke();g.drawImage(im,sx,0,w,im.height,x+Math.floor((198-w)/2),y+22+k*244,w,im.height);}}
   const file=`sheet-${id}-${from}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,id,from,to:from+n-1});}
  // Exact reviewed sprites, with holds only outside the connected transition.
  const movie=[...Array(12).fill(packed[0]),...packed,...Array(24).fill(packed.at(-1))],file=`preview-${id}.png`;fs.writeFileSync(path.join(dir,file),apng(movie));animations.push({file,id,scale:sc,frames:movie.length,transitionFrames:list.length,sha256:sha(path.join(dir,file))});
  console.log(id+' '+list.length);
 }
 for(const angle of [-16,-8,0,8,16])for(const hard of [false,true])for(const t of [.08,.26,.3,.34,...(hard?[1.4,1.9,2.1]:[.52])]){if(t>duration(hard))continue;near.push({angle,hard,t,before:diff(m.render(t-1e-6,angle,hard),m.render(t,angle,hard)),after:t===duration(hard)?null:diff(m.render(t,angle,hard),m.render(t+1e-6,angle,hard))});}
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_ONLY_AWAITING_DIRECT_REVIEW',sources,frames,sheets,animations,endpoints,geometry,near,scope:'Preview only. Angles -16..16, shape-preserving translation instead of old squash. All emitted 30fps transition frames saved.',runtimeChanged:false},null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

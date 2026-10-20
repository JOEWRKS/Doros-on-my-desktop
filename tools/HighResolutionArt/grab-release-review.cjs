'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {create,pose,dir,duration}=require('./grab-release.cjs'),{diff}=require('./grab-entry-transport.cjs'),{apng}=require('./grab-tilt-audit.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
function presentation(im,sc,label,floor){const c=api.createCanvas(480,300),g=c.getContext('2d');g.imageSmoothingEnabled=false;
 for(const [i,bg]of ['white','#252a31'].entries()){const x=i*240;g.fillStyle=bg;g.fillRect(x,0,240,300);g.fillStyle=i?'white':'#222';g.font='13px sans-serif';g.fillText(label,x+7,20);const ox=x+Math.floor((240-im.width)/2),oy=28;g.strokeStyle=i?'#a0a6b0':'#9c979f';g.beginPath();g.moveTo(x+24,oy+floor);g.lineTo(x+216,oy+floor);g.stroke();g.drawImage(im,ox,oy);}return c;}
function lowest(im){const d=pixels(im,api);let bottom=0;for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(d[(y*im.width+x)*4+3]>=128)bottom=Math.max(bottom,y+1);return bottom;}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create(),prior=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-idle-tilt-20261017-r44/manifest.json'))),sources={...prior.sources};
 for(const f of ['tools/HighResolutionArt/grab-release.cjs','tools/HighResolutionArt/grab-release-review.cjs','src/Dororong.App/Interaction/HeadLandingMotion.cs'])sources[f]=sha(path.join(root,f));
 for(const [f,h]of Object.entries(prior.sources))assert.equal(sha(path.join(root,f)),h);
 const frames=[],sheets=[],animations=[],endpoints=[],geometry=[],contact=[];
 for(const sc of [1,1.25,1.5]){
  const list=[],floor=lowest(m.render(duration,sc)),packed=[];
  for(let i=0;i<=88;i++){
   const t=i*.005,im=m.render(t,sc),file=`release-${sc.toFixed(2)}-${String(i).padStart(3,'0')}.png`;fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));const saved=await api.loadImage(path.join(dir,file));assert.deepEqual(pixels(saved,api),pixels(im,api));
   const p=pose(t),same=diff(m.render(t,sc,{positioned:false}),m.previous.entry.render(p.progress,sc));assert.equal(same.changedPixels,0);geometry.push({scale:sc,index:i,...same});
   const d=pixels(saved,api);for(let n=0;n<im.width;n++)for(const [x,y]of [[n,0],[n,im.height-1],[0,n],[im.width-1,n]])assert.equal(d[(y*im.width+x)*4+3],0);
   const bottom=lowest(saved);assert(bottom<=floor,'Below ground '+file);contact.push({scale:sc,index:i,bottom,floor});list.push(saved);frames.push({file,scale:sc,index:i,seconds:t,pose:p,sha256:sha(path.join(dir,file))});packed.push(presentation(saved,sc,`${sc}x / release f${i} / ${p.phase}`,floor).toBuffer('image/png'));
  }
  const start=diff(list[0],m.previous.render(152,sc));assert.equal(start.changedPixels,0);endpoints.push({scale:sc,name:'held-release',...start});
  const end=api.createCanvas(Math.ceil(160*sc),Math.ceil(160*sc));end.getContext('2d').drawImage(m.previous.idle(sc),0,36*sc);const finish=diff(list[88],end);assert.equal(finish.changedPixels,0);endpoints.push({scale:sc,name:'release-translated-idle',...finish});
  for(let start=0;start<89;start+=18){const n=Math.min(18,89-start),c=api.createCanvas(1200,Math.ceil(n/6)*510),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let j=0;j<n;j++){const i=start+j,x=j%6*200,y=Math.floor(j/6)*510,im=list[i],sx=Math.floor(24*sc),w=Math.ceil(112*sc),h=im.height;
    g.fillStyle='#ddd';g.fillRect(x,y,200,510);g.fillStyle='#222';g.font='13px sans-serif';g.fillText(`${sc}x f${i} ${pose(i*.005).phase}`,x+4,y+16);
    for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+k*244,198,242);g.strokeStyle='#999';g.beginPath();g.moveTo(x+15,y+22+k*244+floor);g.lineTo(x+185,y+22+k*244+floor);g.stroke();g.drawImage(im,sx,0,w,h,x+Math.floor((198-w)/2),y+22+k*244,w,h);}
   }const file=`sheet-${sc.toFixed(2)}-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,scale:sc,from:start,to:start+n-1});}
  const file=`release-slow-${sc.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,file),apng(packed));animations.push({file,scale:sc,kind:'slow',frames:89,sha256:sha(path.join(dir,file))});
  // Connected presentation: original reviewed idle/entry/tilt, then release slowed
  // for inspection. No visual reset between held and release. Loop resets after idle.
  const chain=[];for(let i=0;i<=180;i++){const t=i/30;chain.push(presentation(m.previous.at(t,sc),sc,`${sc}x / idle - grab - tilt`,floor).toBuffer('image/png'));}
  chain.push(...packed);for(let n=0;n<18;n++)chain.push(presentation(list[88],sc,`${sc}x / settled idle`,floor).toBuffer('image/png'));
  const combined=`connected-${sc.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,combined),apng(chain));animations.push({file:combined,scale:sc,kind:'connected',frames:chain.length,sha256:sha(path.join(dir,combined))});
  console.log(`saved ${sc}:89 dense release frames + connected sequence`);
 }
 const near=[];for(const t of [.18,.22,.26,.44])near.push({seconds:t,before:diff(m.render(t-1e-6),m.render(t)),after:t===.44?null:diff(m.render(t),m.render(t+1e-6))});
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_ONLY_AWAITING_DIRECT_REVIEW',sources,frames,sheets,animations,endpoints,geometry,contact,near,scope:'Neutral held release only; shape-preserving small rigid rebound, not legacy squash or extreme landing. Preview only.',runtimeUnchanged:true},null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={presentation,lowest};

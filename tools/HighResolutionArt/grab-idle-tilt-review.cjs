'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {create,dir}=require('./grab-idle-tilt.cjs'),{diff}=require('./grab-entry-transport.cjs'),{apng}=require('./grab-tilt-audit.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
function presentation(im,scale,label){const c=api.createCanvas(480,300),g=c.getContext('2d');g.imageSmoothingEnabled=false;
 for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(j*240,0,240,300);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(`${scale}x / ${label}`,j*240+8,22);g.drawImage(im,j*240+Math.floor((240-im.width)/2),35);}return c;}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create(),records=[],boundaries=[],sheets=[],animations=[];
 const old=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-boundary-connected-20261017-r43/review.json'),'utf8'));
 const sources={...old.sources};for(const f of ['tools/HighResolutionArt/grab-idle-tilt.cjs','tools/HighResolutionArt/grab-tilt-transition.cjs','tools/HighResolutionArt/grab-left-occlusion.cjs','tools/HighResolutionArt/current-motion.cjs','tools/HighResolutionArt/grab-idle-tilt-review.cjs'])sources[f]=sha(path.join(root,f));
 for(const [f,h]of Object.entries(old.sources))assert.equal(sha(path.join(root,f)),h);
 const savedByScale={};
 for(const sc of [1,1.25,1.5]){
  const images=[];
  for(let i=0;i<=152;i++){
   const im=m.render(i,sc),file=`frame-${sc.toFixed(2)}-${String(i).padStart(3,'0')}.png`;fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));
   const saved=await api.loadImage(path.join(dir,file)),d=pixels(saved,api);assert.deepEqual(d,pixels(im,api));
   const previous=i<=56?path.join(root,`artifacts/grab-boundary-connected-20261017-r43/frame-${sc.toFixed(2)}-${String(i).padStart(2,'0')}.png`):path.join(root,`artifacts/grab-tilt-transition-20261017-r37/frame-${sc.toFixed(2)}-${String(i-56).padStart(3,'0')}.png`);
   assert.deepEqual(d,pixels(await api.loadImage(previous),api),'Approved source sequence drift '+file);
   for(let n=0;n<im.width;n++)for(const [x,y]of [[n,0],[n,im.height-1],[0,n],[im.width-1,n]])assert.equal(d[(y*im.width+x)*4+3],0);
   images.push(saved);records.push({file,scale:sc,frame:i,sha256:sha(path.join(dir,file)),previousPixelIdentity:true});
  }savedByScale[sc]=images;
  for(const [name,a,b]of [['idle-entry',m.idle(sc),m.entry.render(0,sc)],['entry-tilt',m.entry.render(1,sc),m.tilt.render(sc,0)],['tilt-return',m.tilt.render(sc,0),m.render(152,sc)]]){const result=diff(a,b);assert.equal(result.changedPixels,0);boundaries.push({scale:sc,name,...result});}
  // Read each entry/tilt source file above. Only the NEW 1.5x chain and other-size
  // boundary sheets are claimed for direct review, not every unchanged scale anew.
  const batches=sc===1.5?Array.from({length:9},(_,n)=>Array.from({length:Math.min(18,153-n*18)},(_,i)=>n*18+i)):[[0,1,2,3,4,5,52,53,54,55,56,57,66,67,68,69,70,71]];
  for(let page=0;page<batches.length;page++){
   const ids=batches[page],c=api.createCanvas(1200,Math.ceil(ids.length/6)*390),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(const [n,i]of ids.entries()){
    const x=n%6*200,y=Math.floor(n/6)*390,im=images[i],o=Math.floor(24*sc),w=Math.ceil(120*sc);
    g.fillStyle='#dedbdf';g.fillRect(x,y,200,390);g.fillStyle='#222';g.font='13px sans-serif';g.fillText(`${sc}x f${i} ${i<=56?'entry':'tilt'}`,x+4,y+16);
    for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+j*184,198,182);g.drawImage(im,o,o,w,w,x+Math.floor((198-w)/2),y+23+j*184,w,w);}
   }const file=`sheet-${sc.toFixed(2)}-${page}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,scale:sc,frames:ids});
  }
  const packed=[],timed=[];
  for(let i=0;i<=192;i++){
   const seconds=i/30,im=m.at(seconds,sc),file=`timed-${sc.toFixed(2)}-${String(i).padStart(3,'0')}.png`;fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));timed.push({file,seconds,sha256:sha(path.join(dir,file))});packed.push(presentation(im,sc,`idle - entry - tilt / ${seconds.toFixed(2)}s`).toBuffer('image/png'));
  }
  const file=`preview-${sc.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,file),apng(packed));animations.push({file,scale:sc,frames:193,sha256:sha(path.join(dir,file)),timed});
  console.log(`saved ${sc}x:153 source-progress samples +193 timed frames`);
 }
 // Explicit before/after idle and both sides of the seams, actual size and crop.
 const c=api.createCanvas(1200,600),g=c.getContext('2d'),oldIdle=api.createCanvas(240,240);oldIdle.getContext('2d').drawImage(m.entry.common.render('idle',0),24,24);
 const imgs=[oldIdle,m.idle(),m.entry.render(0),m.entry.render(1),m.tilt.render(1.5,0)];
 for(const [i,im]of imgs.entries())for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(i*240,j*300,240,300);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(['Old idle','Matched idle','Entry start','Entry end','Tilt start'][i],i*240+8,j*300+24);g.drawImage(im,i*240,j*300+25);}fs.writeFileSync(path.join(dir,'boundary-comparison.png'),c.toBuffer('image/png'));
 const near=[];for(const t of [.4,2.8,3.2,4,5.2,6]){near.push({seconds:t,before:diff(m.at(t-1e-6),m.at(t)),after:diff(m.at(t),m.at(t+1e-6))});}
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_CHECKS_ONLY_VISUAL_REVIEW_SEPARATE',sources,records,boundaries,sheets,animations,near,oldIdleDifference:diff(oldIdle,m.idle()),scope:'Open neutral idle -> eight-source entry -> accepted held head tilt only. Preview selection; historical shared renderer and runtime unchanged.',limits:['Other common-motion head masks not migrated','Native-palette/binary-alpha gate not applicable to approved smooth compositor','Timed review traversal is not production timing','Loop reset is not release/landing']},null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={presentation};

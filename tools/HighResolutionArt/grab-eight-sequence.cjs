'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs');
const {create,dir}=require('./grab-eight-source.cjs');
const {apng}=require('./grab-tilt-audit.cjs');
const {diff}=require('./grab-entry-transport.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const m=await create(),scales=[1,1.25,1.5],records=[],sheets=[],animations=[];
 fs.mkdirSync(dir,{recursive:true});
 for(const scale of scales){
  const images=[];let first;
  for(let i=0;i<=56;i++){
   const frame=m.render(i/56,scale),f=`frame-${scale.toFixed(2)}-${String(i).padStart(2,'0')}.png`;
   fs.writeFileSync(path.join(dir,f),frame.toBuffer('image/png'));const im=await api.loadImage(path.join(dir,f)),d=pixels(im,api);images.push(im);if(!i)first=d;
   assert.deepEqual(d,pixels(frame,api));let border=0,centralFace=0;
   for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++){
    const at=(y*im.width+x)*4;if((!x||!y||x===im.width-1||y===im.height-1)&&d[at+3])border++;
    const lx=(x+.5)/scale-32,ly=(y+.5)/scale-32;
    if(lx>=22&&lx<42&&ly>=42&&ly<60&&d.subarray(at,at+4).some((v,k)=>v!==first[at+k]))centralFace++;
   }assert.equal(border,0);assert.equal(centralFace,0);
   records.push({file:f,scale,index:i,progress:i/56,sha256:sha(path.join(dir,f)),border,centralFace});
  }
  for(let start=0;start<57;start+=18){
   const n=Math.min(18,57-start),c=api.createCanvas(1200,Math.ceil(n/6)*400),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let j=0;j<n;j++){
    const i=start+j,x=j%6*200,y=Math.floor(j/6)*400,im=images[i],offset=24*scale,width=112*scale;
    for(const [k,bg]of ['white','#252a31'].entries()){
     g.fillStyle=bg;g.fillRect(x,y+k*200,200,200);g.fillStyle=k?'white':'#222';g.font='13px sans-serif';g.fillText(`${scale}x / f${i} / ${(i/56).toFixed(3)}`,x+5,y+k*200+17);
     g.drawImage(im,offset,offset,width,width,x+(200-width)/2,y+k*200+25,width,width);
    }
   }
   const file=`sheet-${scale.toFixed(2)}-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push(file);
  }
  // Slow inspection entry only: reverse playback is NOT a release animation.
  const order=[...Array(15).fill(0),...Array.from({length:57},(_,i)=>i),...Array(30).fill(56)],frames=[];
  for(const i of order){
   const c=api.createCanvas(480,300),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(const [j,bg]of ['white','#252a31'].entries()){
    g.fillStyle=bg;g.fillRect(j*240,0,240,300);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(`${scale}x / slowed entry / f${i}`,j*240+7,22);g.drawImage(images[i],j*240+(240-images[i].width)/2,35);
   }frames.push(c.toBuffer('image/png'));
  }
  const file=`slow-preview-${scale.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,file),apng(frames));animations.push({file,frames:frames.length,fps:30,entrySeconds:56/30,order});
  console.log(`saved ${scale}x /57 frames`);
 }
 // Report boundaries honestly; these are new eight-source body keys, not the
 // previous whole-body transport's exact endpoint guarantee.
 const common=await require('./current-motion.cjs').create(),idle=api.createCanvas(240,240);idle.getContext('2d').drawImage(common.render('idle',0),24,24);
 const held=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/candidate.png'));
 const endpoints={currentIdle:diff(idle,m.render(0)),acceptedHeld:diff(held,m.render(1))};
 const c=api.createCanvas(960,550),g=c.getContext('2d');g.imageSmoothingEnabled=false;
 for(const [i,im]of [idle,m.render(0),held,m.render(1)].entries())for(const [j,bg]of ['white','#252a31'].entries()){
  g.fillStyle=bg;g.fillRect(i*240,j*275,240,275);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(['Current idle','New source01 key','Accepted held','New source08 key'][i],i*240+8,j*275+20);g.drawImage(im,i*240,j*275+25);
 }fs.writeFileSync(path.join(dir,'boundary-comparison.png'),c.toBuffer('image/png'));
 fs.writeFileSync(path.join(dir,'sequence.json'),JSON.stringify({status:'SAVED_CHECKS_ONLY_VISUAL_REVIEW_SEPARATE',scales,framesPerScale:57,records,sheets,animations,endpoints,entryMs:140,runtimeChanged:false,sourceSha256:sha(path.join(__dirname,'grab-eight-source.cjs')),exportSha256:sha(__filename),limitations:['Current idle and accepted held boundaries are reported, not assumed identical.','No real-time observation asserted by this exporter.','Geometry preview only; no release, landing, tilt or product integration.']},null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

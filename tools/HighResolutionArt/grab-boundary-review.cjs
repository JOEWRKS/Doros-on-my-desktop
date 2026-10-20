'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels,premultiply,sample,surface}=require('./sampling.cjs');
const {create,dir}=require('./grab-boundary-connected.cjs'),{diff}=require('./grab-entry-transport.cjs'),{apng}=require('./grab-tilt-audit.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create(),traced=await require('./grab-eight-traced.cjs').create(),records=[],sheets=[],comparisons=[],endpoints=[],near=[];
 const files=['tools/HighResolutionArt/grab-eight-traced.cjs','tools/HighResolutionArt/grab-boundary-connected.cjs',...Array.from({length:8},(_,i)=>`src/Dororong.App/Assets/user-body-drag/0${i+1}.png`),'artifacts/grab-contour-still-20261017-r33/masked-head-225.png','artifacts/grab-contour-still-20261017-r33/body-padded-375.png'];
 const sources=Object.fromEntries(files.map(f=>[f,sha(path.join(root,f))]));
 const hp=premultiply(m.head,api),animations=[];
 for(const scale of [1,1.25,1.5]){
  const list=[],p=new Float64Array(4),h=surface(api,96*scale,(x,y)=>sample(hp,225,x*225/96-.5,y*225/96-.5,p),32),hd=pixels(h,api),protect=[];
  for(let y=1;y<h.height-1;y++)for(let x=1;x<h.width-1;x++){
   let yes=true;for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++)if(hd[((y+dy)*h.width+x+dx)*4+3]!==255)yes=false;
   if(yes)protect.push((y*h.width+x)*4);
  }
  for(let i=0;i<=56;i++){
   const im=m.render(i/56,scale),file=`frame-${scale.toFixed(2)}-${String(i).padStart(2,'0')}.png`;fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));
   const saved=await api.loadImage(path.join(dir,file)),d=pixels(saved,api);assert.deepEqual(d,pixels(im,api));
   for(const at of protect)assert.deepEqual(d.subarray(at,at+4),hd.subarray(at,at+4));
   let border=0;for(let n=0;n<im.width;n++)for(const [x,y]of [[n,0],[n,im.height-1],[0,n],[im.width-1,n]])if(d[(y*im.width+x)*4+3])border++;assert.equal(border,0);
   list.push(saved);records.push({file,scale,index:i,sha256:sha(path.join(dir,file)),protectedPixels:protect.length,border});
  }
  for(let start=0;start<57;start+=15){
   const n=Math.min(15,57-start),c=api.createCanvas(1200,Math.ceil(n/5)*470),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let j=0;j<n;j++)for(const [b,bg]of ['white','#252a31'].entries()){
    const x=j%5*240,y=Math.floor(j/5)*470+b*235,im=list[start+j];g.fillStyle=bg;g.fillRect(x,y,240,235);g.fillStyle=b?'white':'#222';g.font='14px sans-serif';g.fillText(`${scale}x / f${start+j}`,x+8,y+18);g.drawImage(im,x+(240-im.width)/2,y-10);
   }
   const file=`sheet-${scale.toFixed(2)}-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push(file);
  }
  for(const i of [0,1]){const d=diff(m.render(i,scale),m.reference(i,scale));assert.equal(d.changedPixels,0);endpoints.push({scale,endpoint:i,...d});}
  const order=[...Array(12).fill(0),...Array.from({length:57},(_,i)=>i),...Array(24).fill(56)],frames=[];
  for(const i of order){const c=api.createCanvas(480,300),g=c.getContext('2d');for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(j*240,0,240,300);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(`${scale}x / slow inspection / f${i}`,j*240+7,22);g.drawImage(list[i],j*240+(240-list[i].width)/2,35);}frames.push(c.toBuffer('image/png'));}
  const file=`slow-preview-${scale.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,file),apng(frames));animations.push({file,sha256:sha(path.join(dir,file)),frames:order.length,order,seconds:56/30});
  console.log(`saved ${scale}x, 57 frames; endpoints exact against locked-head references`);
 }
 for(let n=0;n<8;n++){
  const im=await api.loadImage(path.join(dir,`frame-1.50-${String(n*8).padStart(2,'0')}.png`)),tr=traced.registration[n],c=api.createCanvas(960,740),g=c.getContext('2d');g.fillStyle='white';g.fillRect(0,0,960,740);g.imageSmoothingEnabled=false;
  g.drawImage(traced.sources[n],20+(32+tr.dx)*4,-65+(32+tr.dy)*4,400*tr.factor,400*tr.factor);g.drawImage(im,500,-65,640,640);
  g.fillStyle='#252a31';g.fillRect(0,480,960,260);g.drawImage(im,510,475);g.fillStyle='white';g.font='18px sans-serif';g.fillText('Actual 1.5x saved pixels (right)',20,510);
  g.fillStyle='white';g.fillRect(0,0,960,40);g.fillStyle='#222';g.fillText(`Original 0${n+1} / ${n===0?'existing idle boundary':n===7?'accepted held boundary':'corrected body + fixed head'}`,20,28);
  const file=`pair-${n+1}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));comparisons.push(file);
 }
 const c=api.createCanvas(960,550),g=c.getContext('2d'),actualIdle=api.createCanvas(240,240);actualIdle.getContext('2d').drawImage(m.common.render('idle',0),24,24);
 const accepted=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/candidate.png'));
 for(const [i,im]of [actualIdle,m.render(0),accepted,m.render(1)].entries())for(const [j,bg]of ['white','#252a31'].entries()){
  g.fillStyle=bg;g.fillRect(i*240,j*275,240,275);g.fillStyle=j?'white':'#222';g.font='14px sans-serif';g.fillText(['Existing idle','Connected start','Accepted held','Connected end'][i],i*240+8,j*275+20);g.drawImage(im,i*240,j*275+25);
 }fs.writeFileSync(path.join(dir,'boundary-comparison.png'),c.toBuffer('image/png'));
 for(const t of [.000001,.0001,.001,.01,.99,.999,.9999,.999999])near.push({t,...diff(m.render(t),m.reference(t<.5?0:1))});
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 const report={status:'TECHNICAL_CHECKS_ONLY_AWAITING_VISUAL_REVIEW',records,sheets,comparisons,animations,endpoints,near,sources,originalCompositeDifference:{idle:diff(actualIdle,m.render(0)),held:diff(accepted,m.render(1))},scope:'review renderer only; product and installer unchanged',method:'Source-traced geometric paths; corrected endpoint body paths plus locally transported signed raster residual, decaying before02 and after07. No head warp.',limits:['Endpoint references use fixed r33 head throughout. Existing idle used an earlier exterior head mask; compare separately.','The native-palette/binary-alpha workflow is not applicable to the existing smooth partial-alpha compositor.','No app interaction or real-time playback observation claimed by this exporter.']};
 fs.writeFileSync(path.join(dir,'review.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({dir,endpoints,originalCompositeDifference:report.originalCompositeDifference,near}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

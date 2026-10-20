'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
const out=path.join(root,'artifacts/gaze-isolation-20261018-r60');
async function main(){
 fs.mkdirSync(out,{recursive:true});
 const im=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/masked-head-225.png'));
 const c=api.createCanvas(800,640),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,800,640);
 g.imageSmoothingEnabled=false;g.drawImage(im,12*225/96,40*225/96,38*225/96,28*225/96,40,40,760,560);
 g.font='14px sans-serif';
 for(let x=12;x<=50;x+=2){g.fillStyle='#333';g.fillText(String(x),40+(x-12)*20,25);g.strokeStyle='#00999950';g.beginPath();g.moveTo(40+(x-12)*20,40);g.lineTo(40+(x-12)*20,600);g.stroke();}
 for(let y=40;y<=68;y+=2){g.fillStyle='#333';g.fillText(String(y),4,40+(y-40)*20);g.strokeStyle='#00999950';g.beginPath();g.moveTo(40,40+(y-40)*20);g.lineTo(800,40+(y-40)*20);g.stroke();}
 fs.writeFileSync(path.join(out,'source-grid.png'),c.toBuffer('image/png'));
 if(!fs.existsSync(path.join(out,'after')))return;
 const before=path.join(out,'before'),after=path.join(out,'after');
 const read=async f=>{const im=await api.loadImage(f),c=api.createCanvas(im.width,im.height),g=c.getContext('2d');g.drawImage(im,0,0);return {im,p:g.getImageData(0,0,im.width,im.height).data};};
 const checks=[];
 for(const name of fs.readdirSync(before)){
  if(!fs.existsSync(path.join(after,name)))continue;
  const a=await read(path.join(before,name)),b=await read(path.join(after,name));let changed=0;
  for(let i=0;i<a.p.length;i+=4)if([0,1,2,3].some(k=>a.p[i+k]!==b.p[i+k]))changed++;
  checks.push({name,changed,protected:/neutral|protected-cheek/.test(name)});
 }
 fs.writeFileSync(path.join(out,'pixel-comparison.json'),JSON.stringify(checks,null,2));
 const locality=[];
 for(const scale of [1,1.1,1.25,1.35,1.5]){
  const s=scale.toFixed(2),base=await read(path.join(after,`gaze-${s}-neutral.png`));
  for(let d=0;d<8;d++)for(let step=0;step<=8;step++){
   const f=`gaze-${s}-d${d}-s${step}.png`,changed=await read(path.join(after,f));let outside=0,pink=0,total=0;
   for(let y=0;y<base.im.height;y++)for(let x=0;x<base.im.width;x++){
    const i=(y*base.im.width+x)*4;if(![0,1,2,3].some(k=>base.p[i+k]!==changed.p[i+k]))continue;total++;
    const qx=(x+.5)/scale-80,qy=(y+.5)/scale-80;
    if(!((qx>=17.5&&qx<=29&&qy>=47.5&&qy<=56.5)||(qx>=35&&qx<=43.5&&qy>=51.5&&qy<=60.5)))outside++;
    if(base.p[i]>base.p[i+2]+12&&base.p[i+2]>base.p[i+1]+12&&base.p[i+3]>=128)pink++;
   }
   locality.push({file:f,total,outside,pink});
  }
 }
 fs.writeFileSync(path.join(out,'gaze-locality.json'),JSON.stringify(locality,null,2));
 for(const scale of [1,1.1,1.25,1.35,1.5]){
  const s=scale.toFixed(2),canvas=api.createCanvas(1080,864),ctx=canvas.getContext('2d');
  ctx.fillStyle='#eeeae5';ctx.fillRect(0,0,1080,864);ctx.imageSmoothingEnabled=false;ctx.font='12px sans-serif';ctx.fillStyle='#222';
  for(let d=0;d<8;d++)for(let step=0;step<=8;step++){
   const file=path.join(after,`gaze-${s}-d${d}-s${step}.png`);if(!fs.existsSync(file))continue;
   const im=await api.loadImage(file),x=step*120,y=d*108;
   ctx.fillText(`${d}/${step} x${s}`,x+3,y+14);
   // Same fixed registration, nearest enlargement for inspection only.
   ctx.drawImage(im,92*scale,120*scale,38*scale,28*scale,x+3,y+20,114,84);
  }
  fs.writeFileSync(path.join(out,`eyes-${s}.png`),canvas.toBuffer('image/png'));
 }
 for(const scale of [1,1.25,1.5]){
  const s=scale.toFixed(2),meta=path.join(after,`loop-${s}.json`);if(!fs.existsSync(meta))continue;
  const rows=JSON.parse(fs.readFileSync(meta,'utf8'));
  for(let side=0;side<2;side++){
   const canvas=api.createCanvas(1200,720),ctx=canvas.getContext('2d');ctx.fillStyle='#eeeae5';ctx.fillRect(0,0,1200,720);ctx.imageSmoothingEnabled=false;ctx.font='12px sans-serif';
   for(const [i,row] of rows.filter(r=>r.side===side).entries()){
    const im=await api.loadImage(path.join(after,row.file)),x=i%8*150,y=Math.floor(i/8)*120;
    ctx.fillStyle='#222';ctx.fillText(`${i} ${row.entering?'in':'out'} f${row.frame}`,x+2,y+14);
    // Presenter images use an extra transformed window margin. Crop by actual
    // opaque bounds, not assumed source coordinates, and keep aspect ratio.
    const {p}=await read(path.join(after,row.file));let l=im.width,t=im.height,r=0,b=0;
    for(let yy=0;yy<im.height;yy++)for(let xx=0;xx<im.width;xx++)if(p[(yy*im.width+xx)*4+3]>=128){l=Math.min(l,xx);r=Math.max(r,xx);t=Math.min(t,yy);b=Math.max(b,yy);}
    const w=r-l+5,h=b-t+5,k=Math.min(140/w,100/h);ctx.drawImage(im,l-2,t-2,w,h,x+(150-w*k)/2,y+18,w*k,h*k);
   }
   fs.writeFileSync(path.join(out,`loop-sheet-${s}-${side}.png`),canvas.toBuffer('image/png'));
  }
 }
 // Matched before / after directional extremes at 1.5x.
 const pair=api.createCanvas(900,480),pctx=pair.getContext('2d');pctx.fillStyle='#eeeae5';pctx.fillRect(0,0,900,480);pctx.imageSmoothingEnabled=false;pctx.font='16px sans-serif';
 for(let d=0;d<4;d++)for(let k=0;k<2;k++){
  const im=await api.loadImage(path.join(k?after:before,`gaze-1.50-d${d*2}-s4.png`));pctx.fillStyle='#222';pctx.fillText(`${k?'After':'Before'} direction ${d*2}`,k*450+10,d*120+20);
  pctx.drawImage(im,138,180,57,42,k*450+15,d*120+30,114,84);
  pctx.drawImage(im,138,180,57,42,k*450+155,d*120+30,114,84);
 }
 fs.writeFileSync(path.join(out,'before-after.png'),pair.toBuffer('image/png'));
 const strip=api.createCanvas(780,290),sg=strip.getContext('2d');sg.fillStyle='#eeeae5';sg.fillRect(0,0,780,290);sg.imageSmoothingEnabled=false;sg.font='16px sans-serif';
 for(const [i,[folder,file,label]] of [[after,'gaze-1.50-neutral.png','Neutral'],[before,'gaze-1.50-d0-s4.png','Before: gaze right'],[after,'gaze-1.50-d0-s4.png','After: gaze right']].entries()){
  const im=await api.loadImage(path.join(folder,file));sg.fillStyle='#222';sg.fillText(label,i*260+12,25);
  sg.drawImage(im,138,180,57,42,i*260+16,45,228,168);
  sg.drawImage(im,120,155,144,135,i*260+75,220,72,67.5);
 }
 fs.writeFileSync(path.join(out,'comparison.png'),strip.toBuffer('image/png'));
 console.log(JSON.stringify({comparisons:checks.length,protectedChanged:checks.filter(r=>r.protected&&r.changed),sheets:fs.readdirSync(out).filter(x=>/eyes-|loop-sheet/.test(x))}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
const folder=process.argv[2]||'trial',out=path.isAbsolute(folder)?folder:path.join(root,'artifacts/gaze-rigid-20261018-r61',folder);
async function main(){
 for(const scale of [1,1.1,1.25,1.35,1.5]){
  const s=scale.toFixed(2);if(!fs.existsSync(path.join(out,`gaze-${s}-d7-s8.png`)))continue;
  const c=api.createCanvas(1080,864),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;g.font='12px sans-serif';
  for(let d=0;d<8;d++)for(let step=0;step<=8;step++){
   const im=await api.loadImage(path.join(out,`gaze-${s}-d${d}-s${step}.png`)),x=step*120,y=d*108;g.fillStyle='#222';g.fillText(`${d}/${step} x${s}`,x+3,y+14);
   g.drawImage(im,92*scale,120*scale,38*scale,28*scale,x+3,y+20,114,84);
  }fs.writeFileSync(path.join(out,`eyes-${s}.png`),c.toBuffer('image/png'));
 }
 const s=1.5,c=api.createCanvas(1000,650),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;g.font='14px sans-serif';
 for(let d=0;d<8;d++){
  const im=await api.loadImage(path.join(out,`gaze-1.50-d${d}-s4.png`)),x=d%4*250,y=Math.floor(d/4)*320;
  g.fillStyle='#222';g.fillText(`direction ${d}`,x+8,y+20);g.drawImage(im,138,180,57,42,x+10,y+35,228,168);
  g.drawImage(im,120,155,144,135,x+55,y+215,144,135);
 }fs.writeFileSync(path.join(out,'extremes.png'),c.toBuffer('image/png'));
 const comparison=api.createCanvas(780,230),gc=comparison.getContext('2d');gc.fillStyle='#eeeae5';gc.fillRect(0,0,780,230);gc.imageSmoothingEnabled=false;gc.font='16px sans-serif';
 for(const [i,[file,label]] of [['gaze-1.50-d4-s4.png','Left'],['gaze-1.50-neutral.png','Neutral'],['gaze-1.50-d0-s4.png','Right']].entries()){
  const im=await api.loadImage(path.join(out,file));gc.fillStyle='#222';gc.fillText(label,i*260+12,25);gc.drawImage(im,138,180,57,42,i*260+16,45,228,168);
 }fs.writeFileSync(path.join(out,'left-neutral-right.png'),comparison.toBuffer('image/png'));
 for(const scale of [1,1.25,1.5])for(let side=0;side<2;side++){
  const meta=path.join(out,`loop-${scale.toFixed(2)}.json`);if(!fs.existsSync(meta))continue;
  const rows=JSON.parse(fs.readFileSync(meta,'utf8')).filter(r=>r.side===side),sheet=api.createCanvas(1200,720),ctx=sheet.getContext('2d');ctx.fillStyle='#eeeae5';ctx.fillRect(0,0,1200,720);ctx.imageSmoothingEnabled=false;ctx.font='12px sans-serif';
  for(const [i,row] of rows.entries()){
   const im=await api.loadImage(path.join(out,row.file)),raw=api.createCanvas(im.width,im.height),rg=raw.getContext('2d');rg.drawImage(im,0,0);const p=rg.getImageData(0,0,im.width,im.height).data;
   let l=im.width,t=im.height,r=0,b=0;for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(p[(y*im.width+x)*4+3]>=128){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}
   const x=i%8*150,y=Math.floor(i/8)*120,w=r-l+5,h=b-t+5,k=Math.min(140/w,100/h);ctx.fillStyle='#222';ctx.fillText(`${i} ${row.entering?'in':'out'}`,x+2,y+14);ctx.drawImage(im,l-2,t-2,w,h,x+(150-w*k)/2,y+18,w*k,h*k);
  }fs.writeFileSync(path.join(out,`loop-sheet-${scale.toFixed(2)}-${side}.png`),sheet.toBuffer('image/png'));
 }
 const crypto=require('node:crypto'),hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
 const protectedFiles=fs.readdirSync(out).filter(n=>/gaze-.*(neutral|protected-cheek).*png$/.test(n));
 const protection=protectedFiles.map(n=>({file:n,same:hash(path.join(out,n))===hash(path.join(root,'artifacts/gaze-isolation-20261018-r60/before',n))}));
 fs.writeFileSync(path.join(out,'protection.json'),JSON.stringify(protection,null,2));console.log(protection.filter(p=>!p.same));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

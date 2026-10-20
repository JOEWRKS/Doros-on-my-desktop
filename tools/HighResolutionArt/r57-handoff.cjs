'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const base=path.join(root,'artifacts/contour-feedback-20261018-r57');
 const pairs=[
  ['Body ink / 1.5x','before/face-1.50-000-Right-idle.png','final-face/face-1.50-000-Right-idle.png',[80,100,96,80]],
  ['Flutter paw boundary','../runtime-performance-20261018-r56/feedback/review-1.50-flutter-00.png','final-readiness/review-1.50-flutter-00.png',[90,126,70,60]],
  ['Cheek pull','before/face-1.50-021-Right-pull.png','final-face/face-1.50-021-Right-pull.png',[75,110,64,42]],
  ['Cheek rebound','before/face-1.50-025-Right-release.png','final-face/face-1.50-025-Right-release.png',[75,110,64,42]]
 ];
 const c=api.createCanvas(640,940),g=c.getContext('2d');g.fillStyle='#f5f4f2';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;g.fillStyle='#222';g.font='18px sans-serif';g.fillText('Before',18,25);g.fillText('Candidate / not native-input verified',330,25);
 for(let i=0;i<pairs.length;i++){
  const [label,before,after,crop]=pairs[i],y=45+i*220;g.fillStyle='#222';g.font='15px sans-serif';g.fillText(label,18,y);
  for(let side=0;side<2;side++){
   const im=await api.loadImage(path.resolve(base,side?after:before));const [x0,y0,w,h]=crop.map(v=>v*1.5);const zoom=i===0?1:2;
   g.drawImage(im,x0,y0,w,h,side*320+18,y+10,w*zoom,h*zoom);
  }
 }
 fs.writeFileSync(path.join(base,'comparison.png'),c.toBuffer('image/png'));
 const dir=path.join(base,'final-states'),files=fs.readdirSync(dir),selected=[];
 for(const phase of ['idle','sleep','run','jump','perch','left-paw','right-paw']){
  const group=files.filter(f=>f.startsWith('state-1.50-')&&f.endsWith(`-${phase}.png`)).sort();
  for(const i of [...new Set([0,Math.floor(group.length/2),group.length-1])])if(group[i])selected.push(group[i]);
 }
 for(let start=0;start<selected.length;start+=8){
  const list=selected.slice(start,start+8),sheet=api.createCanvas(960,Math.ceil(list.length/4)*260),ctx=sheet.getContext('2d');ctx.fillStyle='#292d34';ctx.fillRect(0,0,sheet.width,sheet.height);ctx.imageSmoothingEnabled=false;
  for(let i=0;i<list.length;i++){
   const im=await api.loadImage(path.join(dir,list[i])),x=i%4*240,y=Math.floor(i/4)*260,src=api.createCanvas(im.width,im.height);src.getContext('2d').drawImage(im,0,0);const d=src.getContext('2d').getImageData(0,0,im.width,im.height).data;let l=im.width,r=0,t=im.height,b=0;
   for(let yy=0;yy<im.height;yy++)for(let xx=0;xx<im.width;xx++)if(d[(yy*im.width+xx)*4+3]){l=Math.min(l,xx);r=Math.max(r,xx);t=Math.min(t,yy);b=Math.max(b,yy);}
   ctx.fillStyle='#fff';ctx.font='12px sans-serif';ctx.fillText(list[i],x+2,y+15);ctx.drawImage(im,l,t,r-l+1,b-t+1,x+20,y+40,r-l+1,b-t+1);
  }
  fs.writeFileSync(path.join(base,`state-bounds-samples-${start}.png`),sheet.toBuffer('image/png'));
 }
 fs.writeFileSync(path.join(base,'state-sample-index.json'),JSON.stringify(selected,null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

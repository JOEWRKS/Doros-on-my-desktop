'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const base=path.join(root,'artifacts/runtime-performance-20261018-r56'),dir=path.join(base,'feedback');
 const files=fs.readdirSync(dir).filter(f=>f.endsWith('.png')).sort(),index=[];
 for(const scale of [1,1.25,1.5])for(const phase of ['hover','flutter','attach']){
  const list=files.filter(f=>f.startsWith(`review-${scale.toFixed(2)}-${phase}`));
  for(let start=0;start<list.length;start+=16){
   const group=list.slice(start,start+16),c=api.createCanvas(960,Math.ceil(group.length/4)*230),g=c.getContext('2d');
   g.fillStyle='#292d34';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
   for(let i=0;i<group.length;i++){
    const x=i%4*240,y=Math.floor(i/4)*230;g.fillStyle='white';g.font='12px sans-serif';g.fillText(group[i],x+3,y+15);
    const im=await api.loadImage(path.join(dir,group[i]));g.drawImage(im,65*scale,65*scale,130*scale,130*scale,x+5,y+22,130*scale,130*scale);
   }
   const file=`review-${scale}-${phase}-${start}.png`;fs.writeFileSync(path.join(base,file),c.toBuffer('image/png'));index.push({file,frames:group});
  }
 }
 fs.writeFileSync(path.join(base,'feedback-index.json'),JSON.stringify(index,null,2));console.log(index.map(x=>x.file).join('\n'));
 const body=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/body-padded-375.png'));
 const c=api.createCanvas(750,750),g=c.getContext('2d');g.fillStyle='#292d34';g.fillRect(0,0,750,750);g.imageSmoothingEnabled=false;g.drawImage(body,0,0,750,750);
 g.font='13px sans-serif';for(let u=15;u<=100;u+=5){const q=(u+32)*225/96*2;g.strokeStyle='#8885';g.beginPath();g.moveTo(q,0);g.lineTo(q,750);g.moveTo(0,q);g.lineTo(750,q);g.stroke();g.fillStyle='#ff9000';g.fillText(String(u),q,355);g.fillText(String(u),210,q);}
 fs.writeFileSync(path.join(base,'body-grid.png'),c.toBuffer('image/png'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

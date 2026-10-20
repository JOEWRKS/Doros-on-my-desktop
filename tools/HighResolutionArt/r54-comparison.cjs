'use strict';
// Read-only presentation of saved renderer outputs; no source-art mutation.
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const dir=path.resolve(root,process.argv[2]||'artifacts/runtime-face-20261018-r54/v2');
 const rows=JSON.parse(fs.readFileSync(path.join(dir,'pairs.json')));
 for(const kind of ['hunt','cheek']){
  const chosen=rows.filter(r=>r.kind===kind&&r.scale===1.5);
  for(let start=0;start<chosen.length;start+=8){
   const batch=chosen.slice(start,start+8),c=api.createCanvas(960,Math.ceil(batch.length/4)*300),g=c.getContext('2d');
   g.fillStyle='#292d34';g.fillRect(0,0,c.width,c.height);g.font='12px sans-serif';
   for(let i=0;i<batch.length;i++){
    const row=batch[i],x=i%4*240,y=Math.floor(i/4)*300;
    g.fillStyle='white';g.fillText(row.name.replace('1.50-',''),x+6,y+18);g.fillText('product / candidate (same logical size)',x+6,y+34);
    const old=await api.loadImage(path.join(dir,row.name+'-old.png')),now=await api.loadImage(path.join(dir,row.name+'-new.png'));
    g.imageSmoothingEnabled=false;
    g.drawImage(old,x+42,y+38,144,144);
    g.drawImage(now,120,120,144,144,x+42,y+158,144,144);
   }
   fs.writeFileSync(path.join(dir,`${kind}-sheet-${start}.png`),c.toBuffer('image/png'));
  }
 }
 const source=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/masked-head-225.png'));
 const c=api.createCanvas(768,576),g=c.getContext('2d');g.fillStyle='#eee';g.fillRect(0,0,768,576);g.imageSmoothingEnabled=false;
 g.drawImage(source,0,80,128,96,0,0,768,576);
 g.strokeStyle='#00a0a088';g.fillStyle='#005050';g.font='12px sans-serif';
 for(let x=8;x<=48;x+=4){let dx=x*225/96*6;g.beginPath();g.moveTo(dx,0);g.lineTo(dx,576);g.stroke();g.fillText('x'+x,dx+2,15);}
 for(let y=36;y<=72;y+=4){let dy=(y*225/96-80)*6;g.beginPath();g.moveTo(0,dy);g.lineTo(768,dy);g.stroke();g.fillText('y'+y,2,dy-2);}
 fs.writeFileSync(path.join(dir,'source-coordinates.png'),c.toBuffer('image/png'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

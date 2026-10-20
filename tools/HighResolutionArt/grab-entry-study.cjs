'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const dir=path.join(root,'artifacts/grab-entry-20261017-r38');fs.mkdirSync(dir,{recursive:true});
 for(let page=0;page<4;page++){
  const c=api.createCanvas(920,500),g=c.getContext('2d');g.fillStyle='white';g.fillRect(0,0,920,500);g.imageSmoothingEnabled=false;
  for(let col=0;col<2;col++){
   const n=page*2+col+1,x=col*460+40,im=await api.loadImage(path.join(root,`src/Dororong.App/Assets/user-body-drag/0${n}.png`));
   g.drawImage(im,x,50,400,400);g.font='14px sans-serif';g.fillStyle='#333';g.fillText(`Original ${n}`,x,25);
   for(let k=0;k<=100;k+=5){g.strokeStyle='rgba(0,100,220,.19)';g.beginPath();g.moveTo(x+k*4,50);g.lineTo(x+k*4,450);g.moveTo(x,50+k*4);g.lineTo(x+400,50+k*4);g.stroke();g.fillText(k,x+k*4,42);g.fillText(k,x-25,54+k*4);}
  }fs.writeFileSync(path.join(dir,`source-study-${page}.png`),c.toBuffer('image/png'));
 }
}
main().catch(e=>{console.error(e);process.exitCode=1;});

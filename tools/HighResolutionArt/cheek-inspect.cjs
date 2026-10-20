'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
(async()=>{
 const names=['tools/HighResolutionArt/input/canonical-225.png','artifacts/high-resolution-face-body-20261016-r14/candidate-225.png'];
 const c=api.createCanvas(1200,660),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,1200,660);
 for(let j=0;j<2;j++){
  const im=await api.loadImage(path.join(root,names[j]));
  for(let k=0;k<2;k++){
   const [sx,sy,w,h]=k?[80,132,25,17]:[39,122,30,17];const ox=j*600+40,oy=k*320+35;
   g.imageSmoothingEnabled=false;g.drawImage(im,sx,sy,w,h,ox,oy,w*17,h*17);
   g.fillStyle='#333';g.font='10px sans-serif';
   for(let x=0;x<w;x++)g.fillText(sx+x,ox+x*17,oy-5);
   for(let y=0;y<h;y++)g.fillText(sy+y,ox-25,oy+y*17+12);
  }
 }
 const out=path.join(root,'artifacts/high-resolution-cheek-20261016-r15');fs.mkdirSync(out,{recursive:true});fs.writeFileSync(path.join(out,'source-coordinate-inspection.png'),c.toBuffer('image/png'));
})();

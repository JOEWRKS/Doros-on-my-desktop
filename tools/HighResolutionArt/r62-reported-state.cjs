'use strict';
const fs=require('node:fs'),path=require('node:path');const {api,root}=require('./bangs-composite-review.cjs');
async function main(){const out=process.argv[2]||path.join(root,'artifacts/reported-state-20261018-r62'),c=api.createCanvas(900,540),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,900,540);g.imageSmoothingEnabled=false;g.font='14px sans-serif';const report=[];
 for(const [row,pull] of [-2,10,20].entries()){
  let base;
  for(const [col,gaze] of [0,-.85,.85].entries()){
   const im=await api.loadImage(path.join(out,`cheek-${pull}-${gaze}.png`)),raw=api.createCanvas(im.width,im.height),rg=raw.getContext('2d');rg.drawImage(im,0,0);const data=rg.getImageData(0,0,im.width,im.height).data;
   if(gaze===0)base=data;
   let count=0,l=im.width,t=im.height,r=0,b=0;
   for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++){const i=(y*im.width+x)*4;if([0,1,2,3].some(k=>data[i+k]!==base[i+k])){count++;l=Math.min(l,x);t=Math.min(t,y);r=Math.max(r,x);b=Math.max(b,y);}}
   report.push({pull,gaze,count,logicalBounds:count?[l/1.5-80,t/1.5-80,(r+1)/1.5-80,(b+1)/1.5-80]:null});
   const x=col*300,y=row*180;g.fillStyle='#222';g.fillText(`Pull ${pull} / Gaze ${gaze}`,x+8,y+20);g.drawImage(im,105,186,87,36,x+8,y+35,261,108);
  }
 }
 fs.writeFileSync(path.join(out,'cheek-comparison.png'),c.toBuffer('image/png'));fs.writeFileSync(path.join(out,'difference-bounds.json'),JSON.stringify(report,null,2));console.log(report);
}main().catch(e=>{console.error(e);process.exitCode=1;});

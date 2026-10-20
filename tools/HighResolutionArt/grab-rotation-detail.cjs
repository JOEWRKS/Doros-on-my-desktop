'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const dir=path.join(root,'artifacts/grab-rotation-review-20261017-r34');
 const c=api.createCanvas(1050,720),g=c.getContext('2d');g.imageSmoothingEnabled=false;
 for(let row=0;row<2;row++)for(const [col,deg]of [-16,0,16].entries()){
  const x=col*350,y=row*360;g.fillStyle=row?'#252a31':'white';g.fillRect(x,y,350,360);g.fillStyle=row?'white':'#222';g.font='17px sans-serif';g.fillText(`Head-only diagnostic: ${deg}deg / 1.5x`,x+10,y+24);
  const f=`head-probe-1.50-${deg>=0?'p':'m'}${Math.abs(deg)}.png`,im=await api.loadImage(path.join(dir,f));
  g.drawImage(im,64,124,88,55,x+10,y+50,330,206.25);
  // Left junction: fixed output region, enlarged without smoothing.
  g.drawImage(im,66,139,25,24,x+10,y+264,87.5,84);
  g.fillText('Left junction',x+110,y+290);
 }
 fs.writeFileSync(path.join(dir,'head-junction-detail.png'),c.toBuffer('image/png'));
 const summary=api.createCanvas(960,360),s=summary.getContext('2d');s.fillStyle='#252a31';s.fillRect(0,0,960,360);s.imageSmoothingEnabled=false;
 for(const [i,[sc,deg]]of [[1,-88],[1.2,-20],[1.4,20],[1.5,88]].entries()){
  const im=await api.loadImage(path.join(dir,`whole-${sc.toFixed(2)}-${deg>=0?'p':'m'}${Math.abs(deg)}.png`));s.drawImage(im,i*240+(240-im.width)/2,40);s.fillStyle='white';s.font='18px sans-serif';s.fillText(`Whole ${sc}x / ${deg}deg`,i*240+18,30);
 }fs.writeFileSync(path.join(dir,'whole-rotation-examples.png'),summary.toBuffer('image/png'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

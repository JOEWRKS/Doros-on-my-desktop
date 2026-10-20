'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
const out=path.resolve(process.argv[2]||path.join(root,'artifacts/gaze-cheek-20261018-r63/final'));
async function main(){
 const before=path.join(root,'artifacts/gaze-rigid-20261018-r61/final');
 const c=api.createCanvas(900,460),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,900,460);g.imageSmoothingEnabled=false;g.font='16px sans-serif';
 for(const [i,[file,label,dir]] of [
  ['gaze-1.50-neutral.png','Original neutral',before],['gaze-1.50-d2-s4.png','Before: down',before],['gaze-1.50-d2-s4.png','Candidate: down',out]
 ].entries()){
  const im=await api.loadImage(fs.readFileSync(path.join(dir,file)));g.fillStyle='#222';g.fillText(label,i*300+10,25);
  g.drawImage(im,138,180,57,42,i*300+10,40,285,210);g.drawImage(im,120,155,144,135,i*300+75,275,144,135);
 }
 fs.writeFileSync(path.join(out,'down-before-after.png'),c.toBuffer('image/png'));
 // Review every exported interaction frame, split by facing, with nearest
 // enlargement. These are saved product renders, not native screen captures.
 for(const scale of [1,1.1,1.25,1.35,1.5]){
  const json=path.join(out,`face-${scale.toFixed(2)}.json`);if(!fs.existsSync(json))continue;
  const rows=JSON.parse(fs.readFileSync(json));
  for(const side of ['Right','Left']){
   const list=rows.filter(r=>r.facing===side),canvas=api.createCanvas(1200,Math.ceil(list.length/6)*150),ctx=canvas.getContext('2d');
   ctx.fillStyle='#eeeae5';ctx.fillRect(0,0,canvas.width,canvas.height);ctx.imageSmoothingEnabled=false;ctx.font='12px sans-serif';
   for(const [i,row] of list.entries()){
    const im=await api.loadImage(fs.readFileSync(path.join(out,row.file)));const x=i%6*200,y=Math.floor(i/6)*150;
    ctx.fillStyle='#222';ctx.fillText(`${i} ${row.phase} ${scale}x`,x+4,y+15);
    ctx.drawImage(im,76*scale,116*scale,100*scale,64*scale,x,y+20,200,128);
   }
   fs.writeFileSync(path.join(out,`interaction-${scale.toFixed(2)}-${side}.png`),canvas.toBuffer('image/png'));
  }
 }
}
main().catch(e=>{console.error(e);process.exitCode=1});

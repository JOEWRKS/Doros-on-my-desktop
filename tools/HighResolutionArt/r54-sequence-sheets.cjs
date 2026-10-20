'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const base=path.join(root,'artifacts/runtime-face-20261018-r54');const index=[];
 const huntDir=path.join(base,'sequence'),hunts=JSON.parse(fs.readFileSync(path.join(huntDir,'pairs.json'))).filter(r=>r.kind==='hunt');
 for(const scale of [...new Set(hunts.map(r=>r.scale))]){
  const list=hunts.filter(r=>r.scale===scale);
  for(let start=0;start<list.length;start+=20){
   const group=list.slice(start,start+20),c=api.createCanvas(1000,Math.ceil(group.length/5)*184),g=c.getContext('2d');
   g.fillStyle='#292d34';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
   for(let i=0;i<group.length;i++){
    const r=group[i],x=i%5*200,y=Math.floor(i/5)*184,old=await api.loadImage(path.join(huntDir,r.name+'-old.png')),now=await api.loadImage(path.join(huntDir,r.name+'-new.png'));
    g.fillStyle='white';g.font='12px sans-serif';g.fillText(`s${scale} frame ${r.frame}: old / new`,x+4,y+14);
    // Whole candidate above, lower contour comparison below. Both are saved
    // renderer pixels, integer enlargement only for the 1x detail.
    g.drawImage(now,80*scale,80*scale,96*scale,96*scale,x+40,y+18,96*scale,96*scale);
    g.drawImage(old,8,64,76,26,x+4,y+154,76,26);
    g.drawImage(now,88*scale,144*scale,76*scale,26*scale,x+102,y+154,76,26);
   }
   const filename=`hunt-sequence-${scale}-${start}.png`;fs.writeFileSync(path.join(base,filename),c.toBuffer('image/png'));index.push({filename,frames:group.map(r=>r.name)});
  }
 }
 const motionDir=path.join(base,'locomotion');
 if(fs.existsSync(motionDir))for(const file of fs.readdirSync(motionDir).filter(f=>f.endsWith('.json'))){
  const list=JSON.parse(fs.readFileSync(path.join(motionDir,file)));
  for(let start=0;start<list.length;start+=20){
   const group=list.slice(start,start+20),c=api.createCanvas(1000,Math.ceil(group.length/5)*180),g=c.getContext('2d');g.fillStyle='#292d34';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
   for(let i=0;i<group.length;i++){const r=group[i],x=i%5*200,y=Math.floor(i/5)*180;g.fillStyle='white';g.font='12px sans-serif';g.fillText(r.filename,x+2,y+14);
    const im=await api.loadImage(path.join(motionDir,r.filename));g.drawImage(im,80*r.scale,80*r.scale,96*r.scale,96*r.scale,x+30,y+20,96*r.scale,96*r.scale);
   }
   const filename=`${file.slice(0,-5)}-sheet-${start}.png`;fs.writeFileSync(path.join(base,filename),c.toBuffer('image/png'));index.push({filename,frames:group.map(r=>r.filename)});
  }
 }
 fs.writeFileSync(path.join(base,'sheet-index.json'),JSON.stringify(index,null,2));console.log(index.map(r=>r.filename).join('\n'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

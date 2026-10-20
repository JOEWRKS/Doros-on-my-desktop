'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const base=path.join(root,'artifacts/runtime-completion-20261018-r55'),dir=path.join(base,process.argv[2]||'states');
 const files=fs.readdirSync(dir).filter(x=>x.endsWith('.png')).sort(),index=[];
 for(const scale of [1,1.25,1.5]){
  const list=files.filter(f=>f.startsWith(`state-${scale.toFixed(2)}-`));
  for(let start=0;start<list.length;start+=24){
   const group=list.slice(start,start+24),c=api.createCanvas(1080,Math.ceil(group.length/6)*180),g=c.getContext('2d');g.fillStyle='#292d34';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
   for(let i=0;i<group.length;i++){
    const x=i%6*180,y=Math.floor(i/6)*180;g.fillStyle='white';g.font='11px sans-serif';g.fillText(group[i],x+3,y+13);
    const im=await api.loadImage(path.join(dir,group[i]));g.drawImage(im,70*scale,70*scale,116*scale,116*scale,x+3,y+17,116*scale,116*scale);
   }
   const file=`${process.argv[2]||'states'}-${scale}-${start}.png`;fs.writeFileSync(path.join(base,file),c.toBuffer('image/png'));index.push({file,frames:group});
  }
 }
 const old=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-edge-perch-09.png'));
 const c=api.createCanvas(800,420),g=c.getContext('2d');g.fillStyle='#292d34';g.fillRect(0,0,800,420);g.imageSmoothingEnabled=false;g.drawImage(old,0,0,400,400);
 g.fillStyle='white';g.fillRect(400,0,400,420);g.drawImage(old,400,0,400,400);
 fs.writeFileSync(path.join(base,'perch-reference.png'),c.toBuffer('image/png'));
 fs.writeFileSync(path.join(base,'sheet-index.json'),JSON.stringify(index,null,2));console.log(index.map(x=>x.file).join('\n'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

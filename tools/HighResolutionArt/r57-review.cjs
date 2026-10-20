'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const base=path.join(root,'artifacts/contour-feedback-20261018-r57'),variant=process.argv[2]||'flutter-v1',dir=path.join(base,variant),index=[];
 for(const scale of [1,1.25,1.5])for(const phase of ['flutter','attach']){
  const files=fs.readdirSync(dir).filter(f=>f.startsWith(`review-${scale.toFixed(2)}-${phase}`)).sort();
  for(let start=0;start<files.length;start+=8){
   const list=files.slice(start,start+8),c=api.createCanvas(960,Math.ceil(list.length/4)*380),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let i=0;i<list.length;i++){
    const x=i%4*240,y=Math.floor(i/4)*380,im=await api.loadImage(path.join(dir,list[i]));
    g.fillStyle='#fff';g.fillRect(x,y,240,190);g.fillStyle='#292d34';g.fillRect(x,y+190,240,190);
    g.fillStyle='#555';g.font='12px sans-serif';g.fillText(list[i],x+3,y+14);
    for(const offset of [20,205])g.drawImage(im,92*scale,134*scale,60*scale,55*scale,x+8,y+offset,120*scale,110*scale);
   }
   const name=`${variant}-${scale}-${phase}-${start}.png`;fs.writeFileSync(path.join(base,name),c.toBuffer('image/png'));index.push({name,files:list});
  }
 }
 fs.writeFileSync(path.join(base,`${variant}-index.json`),JSON.stringify(index,null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

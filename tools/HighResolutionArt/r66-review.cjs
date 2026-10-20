'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
const out=path.resolve(process.argv[2]||path.join(root,'artifacts/face-boundaries-20261018-r66'));
async function main(){
 fs.mkdirSync(out,{recursive:true});
 const c=api.createCanvas(1050,620),g=c.getContext('2d');g.fillStyle='#eeeae5';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;g.font='16px sans-serif';
 const src=await api.loadImage(fs.readFileSync(path.join(root,'artifacts/grab-contour-still-20261017-r33/masked-head-225.png')));
 g.drawImage(src,35,108,74,45,0,30,592,360);
 const reference=path.join(root,'artifacts/gaze-rim-20261018-r65/final');
 for(const [i,name] of ['gaze-1.50-d6-s4.png','gaze-1.50-d2-s4.png','cheek-20-0.png'].entries()){
  const im=await api.loadImage(fs.readFileSync(path.join(reference,name)));g.fillStyle='#222';g.fillText(name,620,i*200+20);g.drawImage(im,144,189,46,29,620,i*200+25,368,174);
 }
 g.fillStyle='#222';g.fillText('Immutable source / 8x pixels',5,20);
 // Annotated logical source pixel values for source-region inspection only.
 const raw=api.createCanvas(225,225),r=raw.getContext('2d');r.drawImage(src,0,0);const data=r.getImageData(0,0,225,225).data;
 for(let y=133;y<=148;y++){
  let line=`y${y}(${(y*96/225).toFixed(2)}): `;
  for(let x=57;x<=84;x++){const p=(y*225+x)*4;line+=data[p+1]<110?'#':data[p+1]<190?'+':'.';}
  console.log(line);
 }
 fs.writeFileSync(path.join(out,'source-diagnosis.png'),c.toBuffer('image/png'));
}
main().catch(e=>{console.error(e);process.exitCode=1});

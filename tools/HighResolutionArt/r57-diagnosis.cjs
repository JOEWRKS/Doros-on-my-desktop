'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const base=path.join(root,'artifacts/contour-feedback-20261018-r57'),dir=path.join(base,process.argv[2]||'before');
 const source=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-canonical.png'));
 const original=api.createCanvas(96,96);original.getContext('2d').drawImage(source,0,0);
 function measure(c,scale,pad){const d=c.getContext('2d').getImageData(0,0,c.width,c.height).data,rows=[];const contrast=1-(68+52+56)/765;
  for(let y=Math.ceil((60+pad)*scale);y<Math.floor((70+pad)*scale);y++){let width=0;for(let x=Math.floor((70+pad)*scale);x<Math.ceil((80+pad)*scale);x++){const i=(y*c.width+x)*4;width+=(1-(d[i]+d[i+1]+d[i+2])/765)*d[i+3]/255/contrast;}rows.push(width/scale);}rows.sort((a,b)=>a-b);return {medianLogical:rows[Math.floor(rows.length/2)],rows};}
 const widths={source:measure(original,1,0)};
 for(const scale of [1,1.25,1.5]){
  const name=fs.readdirSync(dir).find(f=>f.startsWith(`face-${scale.toFixed(2)}-000`));const image=await api.loadImage(path.join(dir,name));
  const c=api.createCanvas(image.width,image.height);c.getContext('2d').drawImage(image,0,0);widths[scale]=measure(c,scale,80);
 }
 fs.writeFileSync(path.join(base,'widths.json'),JSON.stringify(widths,null,2));console.log(JSON.stringify(widths));
 const scale=Number(process.argv[3]||1.5),facing=process.argv[4]||'Right';
 const files=fs.readdirSync(dir).filter(f=>f.startsWith(`face-${scale.toFixed(2)}-`)&&f.includes(`-${facing}-`)&&(/pull|release/.test(f))).sort();
 const c=api.createCanvas(960,Math.ceil(files.length/4)*240),g=c.getContext('2d');g.fillStyle='#fff';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
 for(let i=0;i<files.length;i++){const x=i%4*240,y=Math.floor(i/4)*240;const im=await api.loadImage(path.join(dir,files[i]));g.fillStyle='#222';g.font='11px sans-serif';g.fillText(files[i],x+2,y+14);g.drawImage(im,Math.floor((facing==='Left'?117:75)*scale),Math.floor(110*scale),Math.ceil(64*scale),Math.ceil(42*scale),x+4,y+25,Math.ceil(64*scale)*2,Math.ceil(42*scale)*2);g.fillStyle='#292d34';g.fillRect(x,y+158,240,82);g.drawImage(im,75*scale,75*scale,90*scale,110*scale,x+12,y+160,67.5,82.5);}
 fs.writeFileSync(path.join(base,`${process.argv[2]||'before'}-cheek${process.argv[3]?`-${scale}-${facing}${facing==='Left'?'-corrected-crop':''}`:''}.png`),c.toBuffer('image/png'));
 const full=await api.loadImage(path.join(dir,fs.readdirSync(dir).find(f=>f.startsWith('face-1.00-000'))));
 const compare=api.createCanvas(640,380),a=compare.getContext('2d');a.fillStyle='white';a.fillRect(0,0,640,380);a.imageSmoothingEnabled=false;a.drawImage(source,0,0,96,96,16,50,288,288);a.drawImage(full,80,80,96,96,336,50,288,288);a.fillStyle='#222';a.font='18px sans-serif';a.fillText('Previous canonical (1x, shown 3x)',10,26);a.fillText('Current raster (1x, shown 3x)',334,26);fs.writeFileSync(path.join(base,'body-compare.png'),compare.toBuffer('image/png'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

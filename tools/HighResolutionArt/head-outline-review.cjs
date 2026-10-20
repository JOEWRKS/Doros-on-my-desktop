'use strict';
// Read-only artwork diagnostic; no source-art or runtime edits.
const fs=require('node:fs'),path=require('node:path');
const {create}=require('./connected-families.cjs');
async function main(){
 const m=await create(),{api,root}=m.s;
 const files=['tools/HighResolutionArt/input/canonical-225.png','artifacts/high-resolution-cheek-20261016-r15/head-layer-225.png','artifacts/high-resolution-cheek-20261016-r15/assembly-source-225.png','artifacts/continuous-scale-review-20261017/pose-3-scale-1.5.png'];
 const sheet=api.createCanvas(960,640),g=sheet.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,960,640);g.imageSmoothingEnabled=false;
 for(let i=0;i<files.length;i++){const im=await api.loadImage(path.join(root,files[i]));g.drawImage(im,i*240,30,240,240);g.fillStyle='#252a31';g.font='14px sans-serif';g.fillText(['Original','Saved head','Assembly source','Happy / 1.5x'][i],i*240+8,20);if(i<3)g.drawImage(im,25,135,85,40,i*240,300,238,112);else g.drawImage(im,39,109,58,31,i*240,300,232,124);}
 const out=path.join(root,'artifacts/head-outline-review-20261017');fs.mkdirSync(out,{recursive:true});fs.writeFileSync(path.join(out,'comparison.png'),sheet.toBuffer('image/png'));
 const detail=api.createCanvas(1000,480),dc=detail.getContext('2d');dc.fillStyle='#eeeae4';dc.fillRect(0,0,1000,480);dc.imageSmoothingEnabled=false;
 for(let i=0;i<6;i++){const scale=(1+i*.1).toFixed(1),im=await api.loadImage(path.join(root,`artifacts/continuous-scale-review-20261017/pose-3-scale-${scale}.png`));const sx=Number(scale);dc.drawImage(im,25*sx,69*sx,43*sx,24*sx,i%3*330,60+Math.floor(i/3)*220,301,168);dc.font='17px sans-serif';dc.fillStyle='#252a31';dc.fillText(scale+'x',i%3*330+5,35+Math.floor(i/3)*220);}
 fs.writeFileSync(path.join(out,'chin-scales.png'),detail.toBuffer('image/png'));console.log(out);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

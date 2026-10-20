'use strict';
const fs=require('node:fs'),path=require('node:path');
const {create}=require('./current-motion.cjs');
async function main(){const m=await create(),gmodel=require('./grab-motion.cjs').create(m),{api,root}=m.s,dir=path.join(root,'artifacts/direct-motion-20261017-r29');fs.mkdirSync(dir,{recursive:true});
 const c=api.createCanvas(1200,720),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,c.width,c.height);
 for(let n=0;n<10;n++){const t=n/9,im=gmodel.render(t);fs.writeFileSync(path.join(dir,`grab-study-${n}.png`),im.toBuffer('image/png'));const x=n%5*240,y=Math.floor(n/5)*300;g.drawImage(im,x,y);g.fillStyle='#302b33';g.font='14px sans-serif';g.fillText(t.toFixed(2),x+12,y+276);}
 const ref=await api.loadImage(path.join(root,'src/Dororong.App/Assets/user-body-drag/08.png'));g.imageSmoothingEnabled=false;g.drawImage(ref,1000,600,100,100);g.fillText('Authored hold reference',800,672);fs.writeFileSync(path.join(dir,'grab-study.png'),c.toBuffer('image/png'));console.log(dir);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={main};

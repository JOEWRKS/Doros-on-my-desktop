'use strict';
const fs=require('node:fs'),path=require('node:path');
const {create}=require('./connected-families.cjs');
async function main(){
 const m=await create({pointedHairTip:true,acceptedSleep:true}),{api,root}=m.s;
 const out=path.join(root,'artifacts/accepted-motion-connection-20261017-r28');fs.mkdirSync(out,{recursive:true});
 const c=api.createCanvas(1152,800),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);
 for(let i=0;i<=11;i++){const t=i/11,im=m.render('sleep',t),x=i%6*192,y=Math.floor(i/6)*250;g.drawImage(im,x,y);g.fillStyle='#fff';g.font='14px sans-serif';g.fillText(t.toFixed(2),x+10,y+210);}
 const reference=await api.loadImage(path.join(root,'artifacts/high-resolution-sleep-still-20261017-r23/candidate.png'));
 g.drawImage(reference,300,550);g.drawImage(m.render('sleep',1),600,550);
 fs.writeFileSync(path.join(out,'sleep-study.png'),c.toBuffer('image/png'));console.log(out);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={main};

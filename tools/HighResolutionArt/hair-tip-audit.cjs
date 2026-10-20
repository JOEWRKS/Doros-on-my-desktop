'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{split}=require('./parts.cjs'),{pixels}=require('./sampling.cjs');
async function main(){
 const original=await api.loadImage(path.join(__dirname,'input/canonical-225.png'));
 const assembly=await api.loadImage(path.join(root,'artifacts/high-resolution-cheek-20261016-r15/assembly-source-225.png'));
 const head=split(assembly,api).head;
 const c=api.createCanvas(840,410),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,840,410);g.imageSmoothingEnabled=false;
 for(const [i,im]of [original,assembly,head].entries()){g.fillStyle='#302b33';g.font='17px sans-serif';g.fillText(['Original225','Assembly225','Split head225'][i],280*i+12,24);g.drawImage(im,96,140,35,38,280*i,50,280,304);}
 const a=pixels(assembly,api),b=pixels(head,api),lost=[];
 // Exclude the body stroke at y175+, which correctly belongs outside the head.
 for(let y=140;y<173;y++)for(let x=96;x<122;x++){const at=(y*225+x)*4;if(a[at+3]>100&&Math.max(...a.slice(at,at+3))<180&&b[at+3]===0)lost.push({x,y,rgba:Array.from(a.slice(at,at+4))});}
 const out=path.join(root,'artifacts/hair-tip-audit-20261017');fs.mkdirSync(out,{recursive:true});fs.writeFileSync(path.join(out,'comparison.png'),c.toBuffer('image/png'));fs.writeFileSync(path.join(out,'lost-ink.json'),JSON.stringify(lost,null,2));console.log(JSON.stringify({lostInkPixels:lost.length,examples:lost.slice(0,8),out}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

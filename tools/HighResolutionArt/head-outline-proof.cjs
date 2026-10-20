'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {setup}=require('./accepted-motion.cjs'),{create:upright}=require('./upright.cjs'),{pixels}=require('./sampling.cjs'),{region}=require('./head-outline.cjs');
const outName='artifacts/head-outline-recovery-20261017-r24';
async function main(){
 const s=await setup({polished:true,fixedRunRoots:true,connected:true}),{api,root}=s,out=path.join(root,outName);fs.mkdirSync(out,{recursive:true});
 const scales=process.argv.includes('--trial')?[1,1.25,1.5]:[1,1.1,1.17,1.2,1.3,1.333,1.4,1.5],angles=process.argv.includes('--trial')?[-8,0,8]:Array.from({length:18},(_,i)=>-20+i*2),frames=[],sheets=[];
 const summary=api.createCanvas(900,scales.length*210),cg=summary.getContext('2d');cg.fillStyle='#eeeae4';cg.fillRect(0,0,summary.width,summary.height);cg.imageSmoothingEnabled=false;
 for(const [sc,scale]of scales.entries())for(const eye of ['open','closed']){
  const opts={size:96*scale,padding:16,gaze:eye==='open',reachRun:true,fixedRunRoots:true,connected:true};
  const oldRig=upright(s.masters[eye],api,opts),newRig=upright(s.masters[eye],api,{...opts,outlineRecovery:true});
  const sheet=api.createCanvas(1200,Math.ceil(angles.length/6)*360),g=sheet.getContext('2d');g.fillStyle=eye==='open'?'#eeeae4':'#252a31';g.fillRect(0,0,sheet.width,sheet.height);g.imageSmoothingEnabled=false;
  for(const [index,deg]of angles.entries()){
   const roll=deg*Math.PI/180,old=oldRig.render({roll}),current=newRig.render({roll}),a=pixels(old,api),b=pixels(current,api),co=Math.cos(roll),sn=Math.sin(roll);let differences=0,maxDelta=0;
   for(let y=0;y<current.height;y++)for(let x=0;x<current.width;x++){
    const at=(y*current.width+x)*4;assert.equal(a[at+3],b[at+3],'alpha/outline footprint');
    if(a.slice(at,at+3).some((v,c)=>v!==b[at+c])){
     const dx=(x+.5)/scale-16-43,dy=(y+.5)/scale-16-66,hx=43+dx*co+dy*sn,hy=66-dx*sn+dy*co;
     assert.ok(region(hx*225/96-.5,hy*225/96-.5),'changed outside lower-head outline');
     for(let c=0;c<3;c++){assert.ok(b[at+c]<=a[at+c]);maxDelta=Math.max(maxDelta,a[at+c]-b[at+c]);}differences++;
    }
   }
   assert.ok(maxDelta<=26);const name=`${eye}-${scale.toFixed(3)}-${String(index).padStart(2,'0')}`;fs.writeFileSync(path.join(out,name+'.png'),current.toBuffer('image/png'));
   const cellX=index%6*200,cellY=Math.floor(index/6)*360;
   g.drawImage(current,cellX+Math.floor((200-current.width)/2),cellY+20);
   // Fixed world crop contains the lower head across the full rotation range.
   const crop=[Math.floor(23*scale),Math.floor(64*scale),Math.ceil(48*scale),Math.ceil(31*scale)];
   g.drawImage(current,...crop,cellX+4,cellY+210,192,192*crop[3]/crop[2]);
   g.fillStyle=eye==='open'?'#302b33':'#fff';g.font='13px sans-serif';g.fillText(`${eye} ${scale.toFixed(3)}x ${deg}deg`,cellX+6,cellY+15);
   frames.push({name,eye,scale,deg,differences,maxDelta,alphaIdentical:true,protectedPixelsIdentical:true});
   if(eye==='closed'&&deg===0){
    for(const [j,im]of [old,current].entries()){cg.drawImage(im,j*260+Math.floor((240-im.width)/2),sc*210+20);cg.fillStyle='#302b33';cg.font='16px sans-serif';cg.fillText(`${scale}x ${j?'recovered':'before'}`,j*260+12,sc*210+18);}
    cg.drawImage(old,...crop,520,sc*210+30,180,116);cg.drawImage(current,...crop,710,sc*210+30,180,116);
   }
  }
  const file=`sheet-${eye}-${scale.toFixed(3)}.png`;fs.writeFileSync(path.join(out,file),sheet.toBuffer('image/png'));sheets.push(file);
 }
 fs.writeFileSync(path.join(out,'comparison.png'),summary.toBuffer('image/png'));
 const manifest={runtimeSelected:false,scales,angles,frames,sheets};fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2));console.log(JSON.stringify({out,frames:frames.length,maxDelta:Math.max(...frames.map(f=>f.maxDelta)),changedRange:[Math.min(...frames.map(f=>f.differences)),Math.max(...frames.map(f=>f.differences))]}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

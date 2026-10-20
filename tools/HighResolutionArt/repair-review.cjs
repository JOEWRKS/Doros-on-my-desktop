'use strict';
const fs=require('node:fs'),path=require('node:path');
const api=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/high-resolution-motion-20261016-r4');
async function sources(){
 fs.mkdirSync(out,{recursive:true});
 const files=['tools/HighResolutionArt/input/canonical-225.png','src/Dororong.App/Assets/dororong-closed-eyes.png','src/Dororong.App/Assets/dororong-blink-squint.png'];
 const c=api.createCanvas(960,400),g=c.getContext('2d');g.fillStyle='#eeeae3';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
 for(let i=0;i<files.length;i++){const im=await api.loadImage(path.join(root,files[i])),s=im.width/96;g.drawImage(im,14*s,43*s,34*s,22*s,i*320,40,306,198);g.fillStyle='#000';g.font='16px sans-serif';g.fillText(['Source225','Authored closed96','Authored squint96'][i],i*320+8,25);}
 const im=await api.loadImage(path.join(root,files[0]));
 const grid=api.createCanvas(800,500),q=grid.getContext('2d');q.imageSmoothingEnabled=false;q.drawImage(im,30,95,80,50,0,0,800,500);
 q.font='12px sans-serif';for(let x=30;x<=110;x+=5){q.strokeStyle='#268a9470';q.beginPath();q.moveTo((x-30)*10,0);q.lineTo((x-30)*10,500);q.stroke();q.fillStyle='#006070';q.fillText(x,(x-30)*10+2,14);}
 for(let y=95;y<=145;y+=5){q.strokeStyle='#268a9470';q.beginPath();q.moveTo(0,(y-95)*10);q.lineTo(800,(y-95)*10);q.stroke();q.fillText(y,2,(y-95)*10+14);}
 fs.writeFileSync(path.join(out,'eye-grid.png'),grid.toBuffer('image/png'));
 fs.writeFileSync(path.join(out,'eye-authority.png'),c.toBuffer('image/png'));
}
async function pages(){
 const manifest=JSON.parse(fs.readFileSync(path.join(out,'manifest.json'),'utf8'));
 const list=manifest.frames.filter(f=>f.width===192&&f.height===192),index=[];
 for(let start=0;start<list.length;start+=24){
  const batch=list.slice(start,start+24),c=api.createCanvas(1152,864),g=c.getContext('2d');
  g.fillStyle=start%48===0?'#eeeae3':'#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(let i=0;i<batch.length;i++){const im=await api.loadImage(path.join(out,batch[i].name+'.png')),x=i%6*192,y=Math.floor(i/6)*216;g.drawImage(im,x,y);g.fillStyle=start%48===0?'#29242d':'#fff';g.font='12px sans-serif';g.fillText(batch[i].name,x+12,y+202);}
  const name=`review-page-${String(start/24+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,name),c.toBuffer('image/png'));index.push({file:name,frames:batch.map(f=>({name:f.name,sha256:f.sha256}))});
 }
 for(const walking of [true,false]){
  const frames=list.filter(f=>f.name.startsWith((walking?'walk':'run')+'-open-')),c=api.createCanvas(960,Math.ceil(frames.length/8)*120),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(let i=0;i<frames.length;i++){const im=await api.loadImage(path.join(out,frames[i].name+'.png'));g.drawImage(im,87,123,51,38,i%8*120,Math.floor(i/8)*120,102,76);g.fillStyle='#fff';g.font='12px sans-serif';g.fillText(String(i),i%8*120+5,Math.floor(i/8)*120+100);}
  fs.writeFileSync(path.join(out,walking?'review-walk-legs.png':'review-run-legs.png'),c.toBuffer('image/png'));
 }
 fs.writeFileSync(path.join(out,'review-index.json'),JSON.stringify(index,null,2)+'\n');console.log(`${list.length} motion/still files on ${index.length} review pages`);
 const compare=api.createCanvas(768,648),g=compare.getContext('2d');g.fillStyle='#ece9e4';g.fillRect(0,0,768,648);g.imageSmoothingEnabled=false;
 const previous=path.join(root,'artifacts/high-resolution-motion-20261016-r3');
 for(const [i,name] of ['standing-open','standing-closed','standing-squint','run-open-16'].entries()){
  for(const [row,dir] of [previous,out].entries())g.drawImage(await api.loadImage(path.join(dir,name+'.png')),i*192,row*216);
  g.fillStyle='#352e36';g.font='13px sans-serif';g.fillText(name,i*192+16,199);g.fillText('r4 '+name,i*192+16,415);
 }
 for(const [i,dir] of [previous,out].entries()){
  const im=await api.loadImage(path.join(dir,'run-open-16.png'));g.drawImage(im,87,123,51,38,i*384+50,446,255,190);
 }
 fs.writeFileSync(path.join(out,'repair-comparison.png'),compare.toBuffer('image/png'));
}
if(require.main===module)(process.argv.includes('--pages')?pages():sources()).catch(e=>{console.error(e);process.exitCode=1;});
module.exports={api,root,out};

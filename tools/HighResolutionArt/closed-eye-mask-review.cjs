'use strict';
const fs=require('node:fs'),path=require('node:path');
const api=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/high-resolution-closed-eye-20261016-r6');
async function inspect(){
 fs.mkdirSync(out,{recursive:true});
 const source=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png'));
 const c=api.createCanvas(800,560),g=c.getContext('2d');g.imageSmoothingEnabled=false;g.drawImage(source,14,43,40,28,0,0,800,560);
 g.font='11px sans-serif';for(let x=14;x<=54;x++){g.strokeStyle='#00aabb66';g.beginPath();g.moveTo((x-14)*20,0);g.lineTo((x-14)*20,560);g.stroke();g.fillStyle='#004452';g.fillText(x,(x-14)*20+2,12);}
 for(let y=43;y<=71;y++){g.strokeStyle='#00aabb66';g.beginPath();g.moveTo(0,(y-43)*20);g.lineTo(800,(y-43)*20);g.stroke();g.fillText(y,0,(y-43)*20+12);}
 fs.writeFileSync(path.join(out,'authored-grid.png'),c.toBuffer('image/png'));
 const {render:correct}=require('./canonical-fixes.cjs'),{transfer,mask}=require('./closed-eye-layer.cjs');
 const original=correct(await api.loadImage(path.join(__dirname,'input/canonical-225.png')),api),closed=transfer(original,null,source,api);
 fs.writeFileSync(path.join(out,'candidate-closed-225.png'),closed.toBuffer('image/png'));
 const r5=await api.loadImage(path.join(root,'artifacts/high-resolution-closed-eye-20261016-r5/master-closed-225.png'));
 const sheet=api.createCanvas(1080,560),q=sheet.getContext('2d');q.fillStyle='#ece9e4';q.fillRect(0,0,1080,560);
 for(const [i,[im,title]] of [[original,'Original225'],[r5,'Rejected r5'],[closed,'Separated face aperture']].entries()){
  q.fillStyle='#302b33';q.font='16px sans-serif';q.fillText(title,i*360+12,22);q.imageSmoothingEnabled=true;q.imageSmoothingQuality='high';q.drawImage(im,i*360+90,38,144,144);
  q.imageSmoothingEnabled=false;q.drawImage(im,30,98,80,55,i*360+20,200,320,220);
 }
 fs.writeFileSync(path.join(out,'mask-comparison.png'),sheet.toBuffer('image/png'));
 const mc=api.createCanvas(225,225),mg=mc.getContext('2d');mg.drawImage(original,0,0);mg.globalAlpha=.35;mg.drawImage(mask(api),0,0);
 fs.writeFileSync(path.join(out,'mask-overlay.png'),mc.toBuffer('image/png'));
 const zoom=api.createCanvas(600,440),z=zoom.getContext('2d');z.imageSmoothingEnabled=false;z.drawImage(closed,38,101,30,22,0,0,600,440);z.font='10px sans-serif';
 for(let x=38;x<68;x++){z.strokeStyle='#00aabb66';z.beginPath();z.moveTo((x-38)*20,0);z.lineTo((x-38)*20,440);z.stroke();z.fillStyle='#004452';z.fillText(x,(x-38)*20+2,11);}
 for(let y=101;y<123;y++){z.strokeStyle='#00aabb66';z.beginPath();z.moveTo(0,(y-101)*20);z.lineTo(600,(y-101)*20);z.stroke();z.fillText(y,0,(y-101)*20+11);}
 fs.writeFileSync(path.join(out,'candidate-left-grid.png'),zoom.toBuffer('image/png'));
 const nativeOpen=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-canonical.png'));
 const assembled=require('./posture.cjs').create(closed,nativeOpen,api).master;
 fs.writeFileSync(path.join(out,'candidate-assembled-225.png'),assembled.toBuffer('image/png'));
 const final=api.createCanvas(864,420),f=final.getContext('2d');f.fillStyle='#edeae3';f.fillRect(0,0,864,420);
 for(const [i,[im,title]] of [[source,'Original closed eyes'],[r5,'Rejected r5'],[assembled,'Eye layer / original bangs']].entries()){
  const s=im.width/96;f.fillStyle='#302b33';f.font='14px sans-serif';f.fillText(title,i*288+12,24);
  f.imageSmoothingEnabled=true;f.imageSmoothingQuality='high';f.drawImage(im,i*288+72,40,144,144);
  f.imageSmoothingEnabled=false;f.drawImage(im,14*s,43*s,34*s,22*s,i*288+8,200,272,176);
 }
 fs.writeFileSync(path.join(out,'closed-face-comparison.png'),final.toBuffer('image/png'));
}
if(require.main===module)inspect().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={api,root,out};

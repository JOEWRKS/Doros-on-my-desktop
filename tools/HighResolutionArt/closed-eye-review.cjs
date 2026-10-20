'use strict';
// Local, source-pixel-only correction proof. No eyelid curves are authored here.
const fs=require('node:fs'),path=require('node:path');
const {setup}=require('./motion-proof.cjs');
const {transfer}=require('./expressions.cjs');
const {create:posture}=require('./posture.cjs');
const {create}=require('./upright.cjs');
async function main(){
 const {api,root,originals,open}=await setup();
 const out=path.join(root,'artifacts/high-resolution-closed-eye-20261016-r5');fs.mkdirSync(out,{recursive:true});
 const authored=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png'));
 const corrected=transfer(originals.open,open,authored,api);
 const master=posture(corrected,open,api).master,rig=create(master,api,{padding:16});
 const save=(name,c)=>fs.writeFileSync(path.join(out,name+'.png'),c.toBuffer('image/png'));
 save('master-closed-225',master);save('standing-closed',rig.still());
 const rejected=await api.loadImage(path.join(root,'artifacts/high-resolution-motion-20261016-r4/master-closed-225.png'));
 const canvas=api.createCanvas(864,420),g=canvas.getContext('2d');g.fillStyle='#edeae3';g.fillRect(0,0,864,420);
 const items=[[authored,'Original closed eyes'],[rejected,'Rejected r4'],[master,'Source pixels / smooth sampling']];
 for(let i=0;i<items.length;i++){
  const [img,label]=items[i],x=i*288,s=img.width/96;
  g.fillStyle='#302b33';g.font='14px sans-serif';g.fillText(label,x+12,24);
  g.imageSmoothingEnabled=true;g.imageSmoothingQuality='high';g.drawImage(img,x+72,40,144,144);
  // Same logical crop and magnification for all columns; nearest reveals saved pixels.
  g.imageSmoothingEnabled=false;g.drawImage(img,14*s,43*s,34*s,22*s,x+8,200,272,176);
 }
 save('closed-eye-comparison',canvas);
 const sheet=api.createCanvas(960,432),sg=sheet.getContext('2d');sg.fillStyle='#232b31';sg.fillRect(0,0,960,432);
 const angles=[-20,-10,0,8,14];
 for(let i=0;i<angles.length;i++)for(let row=0;row<2;row++){
  const c=rig.render({roll:angles[i]*Math.PI/180,amount:row,phase:.33,walking:true});
  save(`closed-${row?'walking':'tracking'}-${i}`,c);sg.drawImage(c,i*192,row*216);
 }
 save('closed-eye-motion-review',sheet);
 console.log(out);
}
async function pages(){
 const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/high-resolution-closed-eye-20261016-r5');
 const dir=path.join(root,'artifacts/high-resolution-motion-20261016-r5');
 const api=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
 const manifest=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'),'utf8'));
 const list=manifest.frames.filter(f=>f.name.includes('-closed')&&f.width===192),index=[];
 for(let start=0;start<list.length;start+=24){
  const batch=list.slice(start,start+24),c=api.createCanvas(1152,864),g=c.getContext('2d');
  const dark=start%48!==0;g.fillStyle=dark?'#232b31':'#edeae3';g.fillRect(0,0,c.width,c.height);
  for(let i=0;i<batch.length;i++){
   const im=await api.loadImage(path.join(dir,batch[i].name+'.png')),x=i%6*192,y=Math.floor(i/6)*216;
   g.drawImage(im,x,y);g.fillStyle=dark?'white':'#302b33';g.font='12px sans-serif';g.fillText(batch[i].name,x+12,y+201);
  }
  const name=`frames-${String(start/24+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,name),c.toBuffer('image/png'));index.push({file:name,frames:batch});
 }
 const old=JSON.parse(fs.readFileSync(path.join(root,'artifacts/high-resolution-motion-20261016-r4/manifest.json'),'utf8'));
 const unchanged=manifest.frames.filter(f=>/-open|squint/.test(f.name));
 for(const f of unchanged)if(old.frames.find(q=>q.name===f.name)?.sha256!==f.sha256)throw Error('Non-closed regression: '+f.name);
 fs.writeFileSync(path.join(out,'review-index.json'),JSON.stringify({closedFrameCount:list.length,unchangedOpenSquintFiles:unchanged.length,pages:index},null,2)+'\n');
 console.log(`${list.length} closed frames on ${index.length} pages; ${unchanged.length} open/squint files identical to r4.`);
}
if(require.main===module)(process.argv.includes('--frames')?pages():main()).catch(e=>{console.error(e);process.exitCode=1;});

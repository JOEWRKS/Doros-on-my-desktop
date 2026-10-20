'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {create}=require('./connected-families.cjs');
const out=path.join(root,'artifacts/high-resolution-sleep-20261017-r21');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 fs.mkdirSync(out,{recursive:true});const m=await create();
 const original=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-sleep.png'));
 const old=await api.loadImage(path.join(root,'artifacts/high-resolution-sleep-20261017-r20/sleep-final.png'));
 const current=m.render('sleep',1),c=api.createCanvas(1008,550),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,c.width,c.height);
 for(const [i,im]of [original,old,current].entries()){
  g.imageSmoothingEnabled=false;
  if(i===0)g.drawImage(im,0,0,96,96,i*336,40,336,336);else g.drawImage(im,24,24,144,144,i*336,40,336,336);
  g.fillStyle='#302b33';g.font='16px sans-serif';g.fillText(['Original sleep','Previous r20','Current r21'][i],i*336+12,24);
  g.imageSmoothingEnabled=true;
  if(i===0)g.drawImage(im,i*336+96,380,144,144);else g.drawImage(im,24,24,144,144,i*336+96,380,144,144);
 }
 fs.writeFileSync(path.join(out,'comparison.png'),c.toBuffer('image/png'));
 fs.writeFileSync(path.join(out,'sleep-final.png'),current.toBuffer('image/png'));
 if(process.argv.includes('--full')){
  const manifest={runtimeSelected:false,sources:{},frames:[],review:[],video:{fps:60,entrySeconds:.36,wakeSeconds:.18}};
  for(const f of ['tools/HighResolutionArt/sleep-pose.cjs','tools/HighResolutionArt/sleep-review.cjs','tools/HighResolutionArt/connected-families.cjs','tools/HighResolutionArt/motion-polish.cjs','tools/PreviewLocomotion/reference-parts.cjs','src/Dororong.App/Assets/dororong-sleep.png','artifacts/high-resolution-cheek-20261016-r15/head-layer-225.png'])manifest.sources[f]=sha(fs.readFileSync(path.join(root,f)));
  const images=[];
  for(const kind of ['sleep','wake'])for(let n=0;n<=64;n++){
   const im=m.render(kind,n/64),name=`${kind}-${String(n).padStart(3,'0')}`,b=im.toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),b);images.push(im);manifest.frames.push({name,kind,phase:n/64,sha256:sha(b)});
  }
  for(let start=0;start<images.length;start+=16){
   const sheet=api.createCanvas(1152,1248),ctx=sheet.getContext('2d'),light=start%32===0;ctx.fillStyle=light?'#eeeae4':'#252a31';ctx.fillRect(0,0,sheet.width,sheet.height);ctx.imageSmoothingEnabled=false;
   const names=[];
   for(let n=start;n<Math.min(images.length,start+16);n++){const i=n-start,x=i%4*288,y=Math.floor(i/4)*312;ctx.drawImage(images[n],x,y,288,288);ctx.fillStyle=light?'#302b33':'#fff';ctx.font='14px sans-serif';ctx.fillText(manifest.frames[n].name,x+12,y+302);names.push(manifest.frames[n].name);}
   const name=`review-${String(start/16+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,name),sheet.toBuffer('image/png'));manifest.review.push({name,frames:names});
  }
  const dir=path.join(out,'video-frames');fs.mkdirSync(dir,{recursive:true});
  for(let n=0;n<300;n++){
   const age=(n/60)%2.5,p=age<.4?0:age<.76?(age-.4)/.36:age<1.8?1:age<1.98?1-(age-1.8)/.18:0;
   const im=images[Math.round(Math.max(0,Math.min(1,p))*64)],v=api.createCanvas(384,384),ctx=v.getContext('2d');ctx.fillStyle='#252a31';ctx.fillRect(0,0,384,384);ctx.drawImage(im,96,96);fs.writeFileSync(path.join(dir,String(n).padStart(3,'0')+'.png'),v.toBuffer('image/png'));
  }
  fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2));console.log(manifest.frames.length+' frames');
 }
 console.log(out);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={out,main};

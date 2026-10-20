'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const api=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {composite}=require('./bangs-composite.cjs'),{pixels}=require('./sampling.cjs');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'artifacts/high-resolution-bangs-composite-20261016-r8');
const basePath='artifacts/high-resolution-closed-eye-20261016-r6/candidate-assembled-225.png';
const donorPath='artifacts/high-resolution-bangs-redraw-20261016-r7/bangs-redraw-preview.png';
async function main(){
 fs.mkdirSync(out,{recursive:true});
 const base=await api.loadImage(path.join(root,basePath)),donor=await api.loadImage(path.join(root,donorPath));
 const {image,registered,mask,patch}=composite(base,donor,api);
 const save=(name,c)=>fs.writeFileSync(path.join(out,name+'.png'),c.toBuffer('image/png'));
 save('closed-composite-225',image);save('patch-mask-225',mask);save('registered-donor-225',registered);
 save('bangs-face-patch-225',patch);
 const c=api.createCanvas(864,460),g=c.getContext('2d');g.fillStyle='#edeae3';g.fillRect(0,0,c.width,c.height);
 for(const [i,[im,label]] of [[base,'Existing character'],[registered,'Redraw donor (not whole replacement)'],[image,'Local composite']].entries()){
  g.fillStyle='#302b33';g.font='12px sans-serif';g.fillText(label,i*288+8,24);
  g.imageSmoothingEnabled=true;g.imageSmoothingQuality='high';g.drawImage(im,i*288+72,40,144,144);
  g.imageSmoothingEnabled=false;g.drawImage(im,30,65,84,88,i*288+18,185,252,264);
 }
 save('comparison',c);
 const pair=api.createCanvas(576,460),p=pair.getContext('2d');p.fillStyle='#edeae3';p.fillRect(0,0,576,460);
 for(const [i,[im,label]] of [[base,'Before'],[image,'After: local patch only']].entries()){
  p.fillStyle='#302b33';p.font='14px sans-serif';p.fillText(label,i*288+12,24);
  p.imageSmoothingEnabled=true;p.imageSmoothingQuality='high';p.drawImage(im,i*288+72,40,144,144);
  p.imageSmoothingEnabled=false;p.drawImage(im,30,65,84,88,i*288+18,185,252,264);
 }save('before-after',pair);
 const a=pixels(base,api),b=pixels(image,api),m=pixels(mask,api);let changed=0,outside=0,alpha=0;const bounds=[225,225,-1,-1];
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;if(a[i+3]!==b[i+3])alpha++;
  if([0,1,2,3].some(k=>a[i+k]!==b[i+k])){changed++;if(!m[i+3])outside++;bounds[0]=Math.min(bounds[0],x);bounds[1]=Math.min(bounds[1],y);bounds[2]=Math.max(bounds[2],x);bounds[3]=Math.max(bounds[3],y);}
 }
 const sha=p=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex');
 const report={base:basePath,baseSha256:sha(basePath),donor:donorPath,donorSha256:sha(donorPath),changedPixels:changed,changedBounds:bounds,outsideMaskChanges:outside,alphaChanges:alpha,runtimeSelected:false};
 fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={api,root,out,basePath,donorPath};

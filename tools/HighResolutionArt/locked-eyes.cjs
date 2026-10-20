'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {protectedAt,isLashSpur}=require('./source-anatomy-lock.cjs');
const {cleanFace}=require('./eye-only-composite.cjs');
const out=path.join(root,'artifacts/high-resolution-locked-eyes-20261016-r13');
const eyeShapes=[[[40,117],[43,111],[48,109],[55,110],[62,115],[67,120],[69,125],[67,130],[61,133],[51,134],[44,131],[40,126]],[[77,124],[82,116],[88,113],[96,116],[103,120],[107,125],[106,132],[101,137],[97,141],[88,141],[82,137],[78,132]]];
function render(base,closed){
 const original=pixels(base,api),clean=pixels(cleanFace(base).image,api),inkLayer=require('./source-lid-contours.cjs').render(closed,api).image,ink=pixels(inkLayer,api);
 const scope=api.createCanvas(225,225),sg=scope.getContext('2d');sg.fillStyle='white';for(const p of eyeShapes){sg.beginPath();p.forEach(([x,y],i)=>i?sg.lineTo(x,y):sg.moveTo(x,y));sg.closePath();sg.fill();}const mask=pixels(scope,api);
 const c=api.createCanvas(225,225),g=c.getContext('2d'),d=g.createImageData(225,225);d.data.set(original);
 const lock=api.createCanvas(225,225),lg=lock.getContext('2d'),ld=lg.createImageData(225,225);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4;
  const cheekSkin=y>=124&&original[i]>180&&original[i]>original[i+1]+12&&original[i]>original[i+2]+4;
  const protect=protectedAt(x,y)||(cheekSkin&&!ink[i+3]);if(protect){ld.data.set([0,180,220,255],i);mask[i+3]=0;continue;}
  if(isLashSpur(x,y))mask[i+3]=255;
  if(!mask[i+3])continue;
  for(let k=0;k<3;k++)d.data[i+k]=cheekSkin?original[i+k]:clean[i+k];
  const w=ink[i+3]/255;for(let k=0;k<3;k++)d.data[i+k]=Math.round(d.data[i+k]*(1-w)+ink[i+k]*w);
 }
 g.putImageData(d,0,0);lg.putImageData(ld,0,0);const md=sg.createImageData(225,225);md.data.set(mask);sg.putImageData(md,0,0);return {image:c,lock,mask:scope,eyes:inkLayer};
}
async function main(){fs.mkdirSync(out,{recursive:true});const base=await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),closed=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png')),r=render(base,closed);const save=(n,c)=>fs.writeFileSync(path.join(out,n+'.png'),c.toBuffer('image/png'));save('candidate-225',r.image);save('protected-mask',r.lock);save('eye-edit-mask',r.mask);save('eyes-only',r.eyes);
 const a=pixels(base,api),b=pixels(r.image,api),m=pixels(r.mask,api),l=pixels(r.lock,api);let changed=0,protectedChanges=0,outside=0,alpha=0;
 const dc=api.createCanvas(225,225),dg=dc.getContext('2d');dg.drawImage(base,0,0);const dd=dg.getImageData(0,0,225,225);
 for(let i=0;i<a.length;i+=4){if(a[i+3]!==b[i+3])alpha++;if([0,1,2,3].some(k=>a[i+k]!==b[i+k])){changed++;if(l[i+3])protectedChanges++;if(!m[i+3])outside++;dd.data.set([255,0,190,a[i+3]],i);}}dg.putImageData(dd,0,0);save('difference',dc);
 const c=api.createCanvas(936,540),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,936,540);for(const [i,im] of [base,r.image,dc].entries()){g.fillStyle='#302b33';g.font='16px sans-serif';g.fillText(['Original225','Locked hair / cheeks','Original difference'][i],i*312+12,25);g.imageSmoothingEnabled=true;g.imageSmoothingQuality='high';g.drawImage(im,i*312+84,35,144,144);g.imageSmoothingEnabled=false;g.drawImage(im,25,90,85,75,i*312,195,312,275);}save('comparison',c);
 const normal=api.createCanvas(432,230),ng=normal.getContext('2d');ng.fillStyle='#eeeae4';ng.fillRect(0,0,216,230);ng.fillStyle='#222a30';ng.fillRect(216,0,216,230);ng.imageSmoothingQuality='high';ng.drawImage(r.image,36,35,144,144);ng.drawImage(r.image,252,35,144,144);save('candidate-light-dark',normal);
 const sha=p=>require('node:crypto').createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex');
 const report={comparisonBase:'tools/HighResolutionArt/input/canonical-225.png',baseSha256:sha('tools/HighResolutionArt/input/canonical-225.png'),closedSha256:sha('src/Dororong.App/Assets/dororong-closed-eyes.png'),changedPixels:changed,protectedChanges,outsideEyeMaskChanges:outside,alphaChanges:alpha,runtimeSelected:false};fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(report);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={render,out};

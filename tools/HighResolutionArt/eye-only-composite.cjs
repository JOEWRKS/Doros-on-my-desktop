'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {apertures}=require('./original-closed-repair.cjs');
const out=path.join(root,'artifacts/high-resolution-eye-only-20261016-r12');
const skinAnchors=[[51,107],[52,108],[89,111],[92,113],[73,131],[75,136],[78,131],[70,133],[74,145],[66,149],[33,132],[35,137],[41,138],[45,140],[94,147]];
function cleanFace(master){
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.drawImage(master,0,0);const d=g.getImageData(0,0,225,225),a=d.data.slice();
 const mc=api.createCanvas(225,225),mg=mc.getContext('2d');mg.fillStyle='white';for(const p of apertures){mg.beginPath();p.forEach(([x,y],i)=>i?mg.lineTo(x,y):mg.moveTo(x,y));mg.closePath();mg.fill();}mg.clearRect(55,135,25,14);
 const m=pixels(mc,api),anchors=skinAnchors.map(([x,y])=>({x,y,rgb:[...a.subarray((y*225+x)*4,(y*225+x)*4+3)]}));
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4,w=m[i+3]/255;if(!w)continue;
  if(a[i]>a[i+1]+22&&a[i]>a[i+2]+6&&a[i]>150)continue;
  const rgb=[0,0,0];let total=0;for(const p of anchors){const weight=1/Math.pow(1+(x-p.x)**2+(y-p.y)**2,1.5);total+=weight;for(let k=0;k<3;k++)rgb[k]+=p.rgb[k]*weight;}
  for(let k=0;k<3;k++)d.data[i+k]=Math.round(a[i+k]*(1-w)+rgb[k]/total*w);
 }
 for(const [y,x0,x1] of [[107,43,45],[108,41,44],[109,41,43],[110,41,42]])for(let x=x0;x<=x1;x++){const i=(y*225+x)*4;if(a[i+1]>100)continue;const from=(y*225+x-6)*4;for(let k=0;k<3;k++)d.data[i+k]=a[from+k];m[i+3]=255;}
 g.putImageData(d,0,0);const md=mg.createImageData(225,225);md.data.set(m);mg.putImageData(md,0,0);return {image:c,mask:mc};
}
function render(master,closed){
 const clean=cleanFace(master),eyes=require('./source-lid-contours.cjs').render(closed,api).image;
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.drawImage(clean.image,0,0);const d=g.getImageData(0,0,225,225),ink=pixels(eyes,api),m=pixels(clean.mask,api);
 // Only RGB/alpha from the transparent eyelid layer is allowed into the face.
 // No closed-expression skin, hair, blush, white fill or post-composite eraser.
 for(let i=0;i<d.data.length;i+=4){const w=ink[i+3]/255*m[i+3]/255;for(let k=0;k<3;k++)d.data[i+k]=Math.round(d.data[i+k]*(1-w)+ink[i+k]*w);}
 g.putImageData(d,0,0);return {image:c,clean:clean.image,mask:clean.mask,eyes};
}
async function main(){
 fs.mkdirSync(out,{recursive:true});const base=await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),closed=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png')),r=render(base,closed);
 const save=(n,c)=>fs.writeFileSync(path.join(out,n+'.png'),c.toBuffer('image/png'));save('candidate-225',r.image);save('clean-face-225',r.clean);save('eyes-only-225',r.eyes);save('edit-mask-225',r.mask);
 const c=api.createCanvas(936,540),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,936,540);
 for(const [i,im] of [base,closed,r.image].entries()){g.fillStyle='#302b33';g.font='16px sans-serif';g.fillText(['Original hair / cheeks','Original closed expression','Eyes-only composite'][i],i*312+12,25);g.imageSmoothingEnabled=true;g.imageSmoothingQuality='high';g.drawImage(im,i*312+84,35,144,144);const s=im.width/96;g.imageSmoothingEnabled=false;g.drawImage(im,9*s,23*s,52*s,55*s,i*312,195,312,330);}save('comparison',c);
 const layers=api.createCanvas(864,350),lg=layers.getContext('2d');lg.fillStyle='#eeeae4';lg.fillRect(0,0,864,350);
 for(const [i,im] of [r.clean,r.eyes,r.image].entries()){lg.fillStyle='#302b33';lg.font='14px sans-serif';lg.fillText(['Original-source clean face','Transparent eyelids ONLY','Composite'][i],i*288+10,24);for(let y=40;y<328;y+=16)for(let x=i*288;x<(i+1)*288;x+=16){lg.fillStyle=((x-i*288)/16+(y-40)/16)%2?'#ddd':'#f8f8f8';lg.fillRect(x,y,16,16);}lg.imageSmoothingEnabled=false;lg.drawImage(im,30,100,80,55,i*288,64,288,198);}save('layers',layers);
 const a=pixels(base,api),b=pixels(r.image,api),m=pixels(r.mask,api),cl=pixels(r.clean,api),ink=pixels(r.eyes,api);let changed=0,outside=0,alpha=0,nonEyeCompositeChanges=0;
 for(let i=0;i<a.length;i+=4){if(a[i+3]!==b[i+3])alpha++;if([0,1,2].some(k=>a[i+k]!==b[i+k])){changed++;if(!m[i+3])outside++;}if(!ink[i+3]&&[0,1,2,3].some(k=>cl[i+k]!==b[i+k]))nonEyeCompositeChanges++;}
 const report={changedPixels:changed,outsideMaskChanges:outside,alphaChanges:alpha,nonEyeCompositeChanges,closedSourceBackgroundCopied:false,generatedDonorUsed:false,runtimeSelected:false};fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(report);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={render,cleanFace,out};

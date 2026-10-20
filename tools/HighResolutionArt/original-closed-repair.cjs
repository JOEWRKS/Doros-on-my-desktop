'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,put}=require('./sampling.cjs');
const out=path.join(root,'artifacts/high-resolution-original-face-20261016-r11');
const apertures=[
 [[38,123],[42,117],[47,113],[49,108],[51,104],[52,108],[54,112],[58,116],[63,120],[69,123],[72,123],[70,130],[63,134],[51,135],[42,132]],
 [[76,126],[79,121],[83,117],[87,113],[90,108],[94,113],[99,116],[104,119],[108,120],[106,137],[99,143],[87,143],[80,138]]
];
function render(master,closed){
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.drawImage(master,0,0);const d=g.getImageData(0,0,225,225),original=d.data.slice();
 const mc=api.createCanvas(225,225),mg=mc.getContext('2d');mg.fillStyle='white';
 for(const poly of apertures){mg.beginPath();poly.forEach(([x,y],i)=>i?mg.lineTo(x,y):mg.moveTo(x,y));mg.closePath();mg.fill();}
 mg.clearRect(55,135,25,14); // Mouth and its immediate surrounding skin stay original.
 const m=pixels(mc,api),src=premultiply(closed,api),v=new Float64Array(4),temp=new Uint8ClampedArray(4);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4,w=m[i+3]/255;if(!w)continue;
  sample(src,96,(x+.5)*96/225-.5,(y+.5)*96/225-.5,v);put(temp,0,v);
  // Preserve existing pink cheek/hair pixels, including their original shading.
  if(original[i]>original[i+1]+22&&original[i]>original[i+2]+6&&original[i]>150)continue;
  for(let k=0;k<3;k++)d.data[i+k]=Math.round(original[i+k]*(1-w)+temp[k]*w);
 }
 // Original open-eye lash spur lies on pink hair, not on the face aperture.
 // Restore only this small spur from the original neighbouring pink texture.
 for(const [y,x0,x1] of [[107,43,45],[108,41,44],[109,41,43],[110,41,42]])for(let x=x0;x<=x1;x++){
  const i=(y*225+x)*4;if(original[i+1]>100)continue;
  const from=(y*225+x-6)*4;for(let k=0;k<3;k++)d.data[i+k]=original[from+k];
  m[i+3]=255;
 }
 const lids=require('./source-lid-contours.cjs').render(closed,api),ink=pixels(lids.image,api);
 // Replace only the original lid ink's neutral pixels, not pink cheek texture.
 for(const [x0,y0,x1,y1] of [[37,118,64,132],[77,126,106,138]])for(let y=y0;y<y1;y++)for(let x=x0;x<x1;x++){
  const i=(y*225+x)*4;if(!m[i+3])continue;const rgb=[...d.data.subarray(i,i+3)];
  if((Math.max(...rgb)-Math.min(...rgb)<35||rgb[1]>=rgb[0]-5&&rgb[2]>=rgb[0]-5)&&Math.min(...rgb)<240)for(let k=0;k<3;k++)d.data[i+k]=255;
  const w=ink[i+3]/255;for(let k=0;k<3;k++)d.data[i+k]=Math.round(d.data[i+k]*(1-w)+ink[i+k]*w);
 }
 g.putImageData(d,0,0);const md=mg.createImageData(225,225);md.data.set(m);mg.putImageData(md,0,0);return {image:c,mask:mc};
}
async function main(){
 fs.mkdirSync(out,{recursive:true});const original=await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),closed=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-closed-eyes.png'));
 const r=render(original,closed),save=(name,c)=>fs.writeFileSync(path.join(out,name+'.png'),c.toBuffer('image/png'));save('candidate-225',r.image);save('edit-mask',r.mask);
 const c=api.createCanvas(936,540),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,936,540);
 for(const [i,im] of [original,closed,r.image].entries()) {g.fillStyle='#302b33';g.font='16px sans-serif';g.fillText(['Original hair / cheeks','Original closed expression','Original-based candidate'][i],i*312+12,25);g.imageSmoothingEnabled=true;g.imageSmoothingQuality='high';g.drawImage(im,i*312+84,35,144,144);const s=im.width/96;g.imageSmoothingEnabled=false;g.drawImage(im,9*s,23*s,52*s,55*s,i*312,195,312,330);}save('comparison',c);
 const view=api.createCanvas(432,230),vg=view.getContext('2d');vg.fillStyle='#eeeae4';vg.fillRect(0,0,216,230);vg.fillStyle='#222a30';vg.fillRect(216,0,216,230);vg.imageSmoothingQuality='high';vg.drawImage(r.image,36,35,144,144);vg.drawImage(r.image,252,35,144,144);save('candidate-light-dark',view);
 const a=pixels(original,api),b=pixels(r.image,api),m=pixels(r.mask,api);let changed=0,outside=0,alpha=0;
 for(let i=0;i<a.length;i+=4){if(a[i+3]!==b[i+3])alpha++;if([0,1,2].some(k=>a[i+k]!==b[i+k])){changed++;if(!m[i+3])outside++;}}
 const sha=p=>require('node:crypto').createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex');
 const report={changedPixels:changed,outsideMaskChanges:outside,alphaChanges:alpha,base:'tools/HighResolutionArt/input/canonical-225.png',baseSha256:sha('tools/HighResolutionArt/input/canonical-225.png'),closedSource:'src/Dororong.App/Assets/dororong-closed-eyes.png',closedSha256:sha('src/Dororong.App/Assets/dororong-closed-eyes.png'),generatedDonorsUsed:false,runtimeSelected:false};fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(report);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={render,out,apertures};

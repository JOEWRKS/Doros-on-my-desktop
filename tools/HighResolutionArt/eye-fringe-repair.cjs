'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const out=path.join(root,'artifacts/high-resolution-eye-fringe-20261016-r10');
async function main(){
 const base=await api.loadImage(path.join(root,'artifacts/high-resolution-bangs-face-20261016-r9/closed-composite-225.png'));
 const donor=await api.loadImage(path.join(out,'generated-donor.png'));
 const registered=api.createCanvas(225,225),rg=registered.getContext('2d');rg.imageSmoothingQuality='high';
 // Register the two fringe/temple junctions before extracting the local patches.
 rg.setTransform(75/73,0.03,0,1,35-34*75/73,2);rg.drawImage(donor,0,0,225,225);
 const mask=api.createCanvas(225,225),g=mask.getContext('2d');g.fillStyle='#fff';
 // One continuous fringe/upper-face patch: never cut across individual hair tips.
 g.beginPath();g.moveTo(51,77);g.bezierCurveTo(61,70,80,72,94,81);g.bezierCurveTo(110,94,116,113,116,133);
 g.lineTo(107,140);g.lineTo(85,140);g.lineTo(80,135);g.lineTo(62,134);g.lineTo(32,134);g.lineTo(26,128);
 g.bezierCurveTo(28,112,38,90,51,77);g.closePath();g.fill();
 const a=pixels(base,api),b=pixels(registered,api),m=pixels(mask,api),c=api.createCanvas(225,225),ctx=c.getContext('2d'),d=ctx.createImageData(225,225);d.data.set(a);
 // Blend only the pink interior seam. Leave fringe tips and eye ink unblurred.
 const originalMask=m.slice();
 for(let y=0;y<107;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;let distance=3;
  if(!m[i+3])continue;
  for(let dy=-2;dy<=2;dy++)for(let dx=-2;dx<=2;dx++)if(x+dx>=0&&x+dx<225&&y+dy>=0&&y+dy<225&&!originalMask[((y+dy)*225+x+dx)*4+3])distance=Math.min(distance,Math.hypot(dx,dy));
  m[i+3]=Math.round(m[i+3]*Math.min(1,distance/3));
 }
 const md=g.createImageData(225,225);md.data.set(m);g.putImageData(md,0,0);
 let changed=0,outside=0,alpha=0;
 for(let i=0;i<a.length;i+=4){const w=m[i+3]/255;if(w&&b[i+3]>240)for(let k=0;k<3;k++)d.data[i+k]=Math.round(a[i+k]*(1-w)+b[i+k]*w);
 if([0,1,2].some(k=>a[i+k]!==d.data[i+k])){changed++;if(!m[i+3])outside++;}if(a[i+3]!==d.data[i+3])alpha++;}
 ctx.putImageData(d,0,0);
 const save=(n,v)=>fs.writeFileSync(path.join(out,n+'.png'),v.toBuffer('image/png'));save('candidate-225',c);save('mask-225',mask);save('registered-donor-225',registered);
 const sheet=api.createCanvas(576,550),s=sheet.getContext('2d');s.fillStyle='#edeae3';s.fillRect(0,0,576,550);
 for(const [i,im] of [base,c].entries()) {s.fillStyle='#302b33';s.font='14px sans-serif';s.fillText(i?'Local correction':'Rejected r9',i*288+12,24);s.imageSmoothingEnabled=true;s.imageSmoothingQuality='high';s.drawImage(im,i*288+72,35,144,144);s.imageSmoothingEnabled=false;s.drawImage(im,25,50,96,120,i*288,184,288,360);}
 save('comparison',sheet);
 const normal=api.createCanvas(432,250),n=normal.getContext('2d');n.fillStyle='#edeae3';n.fillRect(0,0,216,250);n.fillStyle='#222a30';n.fillRect(216,0,216,250);n.imageSmoothingQuality='high';n.drawImage(c,36,40,144,144);n.drawImage(c,252,40,144,144);save('candidate-light-dark',normal);
 const report={changedPixels:changed,outsideMaskChanges:outside,alphaChanges:alpha,runtimeSelected:false};fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(report);
 if(outside||alpha)throw Error('Local invariance failed');
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});

'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {create}=require('./posture.cjs');
const out=path.join(root,'artifacts/high-resolution-cheek-20261016-r15');
const previous=path.join(root,'artifacts/high-resolution-face-body-20261016-r14');
// Verified against the open-eye source and coordinate inspection. These are
// residual iris/lower-lid pixels, not a rectangular cheek or fringe replacement.
const rows=[
 [126,43],[128,44,47,48,49,51],[129,45,49,50,51],[130,46],[131,48],
 [137,88,90,91,100],[138,85,89,90,91,92,93,94,95,99,100],
 [139,86],[140,88,96,97],[141,90,91,92,93,94,95,96]
];
const points=rows.flatMap(([y,...xs])=>xs.map(x=>[x,y]));
const keys=new Set(points.map(([x,y])=>y*225+x));
function repair(input){
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.drawImage(input,0,0);
 const d=g.getImageData(0,0,225,225),a=d.data,work=Float64Array.from(a);
 // Discrete local skin fill; unmasked pixels are immutable boundary samples.
 // No blur/filter/resampling is applied to eyes, skin outside these points or art.
 for(let n=0;n<160;n++)for(const [x,y] of points){
  const i=(y*225+x)*4;
  for(let k=0;k<3;k++)work[i+k]=(work[i-4+k]+work[i+4+k]+work[i-900+k]+work[i+900+k])/4;
 }
 for(const [x,y] of points){const i=(y*225+x)*4;for(let k=0;k<3;k++)a[i+k]=Math.round(work[i+k]);}
 g.putImageData(d,0,0);return c;
}
async function build(){
 const oldSource=await api.loadImage(path.join(previous,'assembly-source-225.png'));
 const before=await api.loadImage(path.join(previous,'candidate-225.png'));
 const measurement=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-canonical.png'));
 const source=repair(oldSource),assembled=create(source,measurement,api);
 return {before,oldSource,source,image:assembled.master,head:assembled.head};
}
async function main(){
 fs.mkdirSync(out,{recursive:true});const r=await build();
 const save=(n,c)=>fs.writeFileSync(path.join(out,n+'.png'),c.toBuffer('image/png'));
 save('candidate-225',r.image);save('assembly-source-225',r.source);save('head-layer-225',r.head);
 const a=pixels(r.before,api),b=pixels(r.image,api);let changed=0,outside=0,alpha=0;
 for(let i=0;i<a.length;i+=4){if(a[i+3]!==b[i+3])alpha++;if([0,1,2,3].some(k=>a[i+k]!==b[i+k])){changed++;if(!keys.has(i/4))outside++;}}
 const preview=api.createCanvas(432,230),pg=preview.getContext('2d');pg.fillStyle='#eeeae4';pg.fillRect(0,0,216,230);pg.fillStyle='#222a30';pg.fillRect(216,0,216,230);pg.imageSmoothingQuality='high';pg.drawImage(r.image,36,35,144,144);pg.drawImage(r.image,252,35,144,144);save('preview',preview);
 const comparison=api.createCanvas(960,430),g=comparison.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,960,430);
 for(const [j,im] of [r.before,r.image].entries()){g.fillStyle='#302b33';g.font='16px sans-serif';g.fillText(j?'After — cheek residue only':'Before',j*480+12,24);g.imageSmoothingEnabled=false;g.drawImage(im,25,95,85,65,j*480+12,40,425,325);}save('comparison',comparison);
 const report={changedPixels:changed,maskPixels:points.length,outsideMaskChanges:outside,alphaChanges:alpha,points,runtimeSelected:false,previousSha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(previous,'candidate-225.png'))).digest('hex'),candidateSha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(out,'candidate-225.png'))).digest('hex')};
 fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify({...report,points:undefined},null,2));if(outside||alpha)throw Error('Cheek-only preservation failed');
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={build,repair,points,keys,out};

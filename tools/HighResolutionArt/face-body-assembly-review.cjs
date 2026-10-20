'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {split,headPolygon}=require('./parts.cjs'),{create}=require('./posture.cjs');
const out=path.join(root,'artifacts/high-resolution-face-body-20261016-r14');
const paths={face:'artifacts/high-resolution-locked-eyes-20261016-r13/candidate-225.png',oldSource:'artifacts/high-resolution-closed-eye-20261016-r6/candidate-closed-225.png',oldAssembly:'artifacts/high-resolution-closed-eye-20261016-r6/candidate-assembled-225.png',measurement:'src/Dororong.App/Assets/dororong-canonical.png'};
function inside(p,x,y){let hit=false;for(let i=0,j=p.length-1;i<p.length;j=i++){const a=p[i],b=p[j];if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])hit=!hit;}return hit;}
function faceRegion(x,y){return x<120&&inside(headPolygon,x+.5,y+.5);}
async function build(){
 const images=Object.fromEntries(await Promise.all(Object.entries(paths).map(async([k,p])=>[k,await api.loadImage(path.join(root,p))])));
 const source=api.createCanvas(225,225),g=source.getContext('2d'),d=g.createImageData(225,225),old=pixels(images.oldSource,api),face=pixels(images.face,api);d.data.set(old);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++)if(faceRegion(x,y)){const i=(y*225+x)*4;d.data.set(face.subarray(i,i+4),i);}g.putImageData(d,0,0);
 const assembled=create(source,images.measurement,api),oldRegenerated=create(images.oldSource,images.measurement,api);
 return {...images,source,image:assembled.master,head:assembled.head,oldRegenerated:oldRegenerated.master};
}
async function main(){
 fs.mkdirSync(out,{recursive:true});const r=await build(),save=(n,c)=>fs.writeFileSync(path.join(out,n+'.png'),c.toBuffer('image/png'));save('candidate-225',r.image);save('assembly-source-225',r.source);save('head-layer-225',r.head);
 const a=pixels(r.oldAssembly,api),b=pixels(r.image,api),f=pixels(r.face,api),h=pixels(r.head,api),reg=pixels(r.oldRegenerated,api);let outsideFace=0,faceMismatch=0,bodyPipelineDrift=0,changed=0;
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;if([0,1,2,3].some(k=>a[i+k]!==reg[i+k]))bodyPipelineDrift++;
  if([0,1,2,3].some(k=>a[i+k]!==b[i+k])){changed++;if(!faceRegion(x,y))outsideFace++;}
  // All facial ink/skin is opaque; transparent head edges are verified on the layer.
  if(faceRegion(x,y)&&f[i+3]===255&&[0,1,2,3].some(k=>b[i+k]!==f[i+k]))faceMismatch++;
 }
 const sheet=api.createCanvas(936,570),sg=sheet.getContext('2d');sg.fillStyle='#eeeae4';sg.fillRect(0,0,936,570);
 for(const [j,im] of [r.oldAssembly,r.face,r.image].entries()){sg.fillStyle='#302b33';sg.font='15px sans-serif';sg.fillText(['Previous edited body / legs','Current face on old source body','Combined'][j],j*312+10,25);sg.imageSmoothingEnabled=true;sg.imageSmoothingQuality='high';sg.drawImage(im,j*312+84,38,144,144);sg.imageSmoothingEnabled=false;sg.drawImage(im,20,90,160,118,j*312,205,312,230);}save('comparison',sheet);
 const preview=api.createCanvas(432,230),pg=preview.getContext('2d');pg.fillStyle='#eeeae4';pg.fillRect(0,0,216,230);pg.fillStyle='#222a30';pg.fillRect(216,0,216,230);pg.imageSmoothingQuality='high';pg.drawImage(r.image,36,35,144,144);pg.drawImage(r.image,252,35,144,144);save('combined-light-dark',preview);
 const detail=api.createCanvas(960,720),dg=detail.getContext('2d');dg.fillStyle='#eeeae4';dg.fillRect(0,0,960,720);const crops=[[25,95,85,65],[30,145,145,60]];
 for(let row=0;row<2;row++)for(let col=0;col<2;col++){const im=row===0?(col?r.image:r.face):(col?r.image:r.oldAssembly);dg.fillStyle='#302b33';dg.font='16px sans-serif';dg.fillText(row===0?(col?'Combined face':'r13 face reference'):(col?'Combined body / seam':'Previous edited body / seam'),col*480+12,row*360+24);dg.imageSmoothingEnabled=false;const [x,y,w,hh]=crops[row];dg.drawImage(im,x,y,w,hh,col*480+12,row*360+40,456,row?189:300);}save('inspection',detail);
 const hashes=Object.fromEntries(Object.entries(paths).map(([k,p])=>[k,{path:p,sha256:crypto.createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex')}]));
 const report={sources:hashes,changedVsPreviousAssembly:changed,outsideFaceChanges:outsideFace,opaqueFaceMismatchVsR13:faceMismatch,previousBodyPipelineDrift:bodyPipelineDrift,runtimeSelected:false};fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');console.log(JSON.stringify(report,null,2));
 if(outsideFace||faceMismatch||bodyPipelineDrift)throw Error('Assembly preservation failed');
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={build,faceRegion,out};

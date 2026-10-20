'use strict';
// Diagnostic: discrepancies are reported, never treated as accepted tolerances.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs');
const {create,dir}=require('./grab-entry.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const m=await create(),common=await require('./current-motion.cjs').create();
 const baseline=api.createCanvas(240,240);baseline.getContext('2d').drawImage(common.render('idle',0),24,24);
 const held=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/candidate.png'));
 const pairs=[[baseline,m.render(0),'Current idle','Entry candidate'],[held,m.render(1),'Accepted held r33','Entry end candidate']];
 const report={status:'NOT_READY_ENDPOINT_MISMATCH',runtimeChanged:false,sourceArtworkChanged:false,checks:[],limits:['Pixel deltas are diagnostic, not automatic visual rejection scores.','No continuous frames inspected or runtime connection attempted.']};
 for(const [n,p]of pairs.entries()){
  const [a,b]=p,ad=pixels(a,api),bd=pixels(b,api);let changed=0,bodyChanged=0,max=0,headChanged=0;const box=[240,240,-1,-1];
  for(let y=0;y<240;y++)for(let x=0;x<240;x++){
   const i=(y*240+x)*4;if(!ad.subarray(i,i+4).some((v,k)=>v!==bd[i+k]))continue;
   changed++;if(y>=150)bodyChanged++;if(x>=78&&x<113&&y>=108&&y<141)headChanged++;
   box[0]=Math.min(box[0],x);box[1]=Math.min(box[1],y);box[2]=Math.max(box[2],x);box[3]=Math.max(box[3],y);
   for(const bg of [0,255])for(let k=0;k<3;k++)max=Math.max(max,Math.abs(ad[i+k]*ad[i+3]/255+bg*(1-ad[i+3]/255)-bd[i+k]*bd[i+3]/255-bg*(1-bd[i+3]/255)));
  }
  assert.equal(headChanged,0,'fixed central face patch must not change');
  report.checks.push({name:p[2],changedPixels:changed,changedPixelsAtOrBelow150:bodyChanged,changedCentralFacePatchPixels:headChanged,changeBounds:box,maxCompositeDelta:max});
  const c=api.createCanvas(960,600),g=c.getContext('2d');g.imageSmoothingEnabled=false;
  for(const [j,im]of [a,b,a,b].entries()){
   const x=j*240,dark=j>=2;g.fillStyle=dark?'#252a31':'white';g.fillRect(x,0,240,600);g.fillStyle=dark?'white':'#222';g.font='15px sans-serif';g.fillText(p[j%2+2],x+8,22);g.drawImage(im,x,30);
   g.drawImage(im,60,140,108,74,x+12,310,216,148);
  }
  const f=`endpoint-${n}.png`;fs.writeFileSync(path.join(dir,f),c.toBuffer('image/png'));
  for(const [j,im]of [a,b].entries())if(j===0){const b=api.createCanvas(im.width,im.height);b.getContext('2d').drawImage(im,0,0);fs.writeFileSync(path.join(dir,`endpoint-baseline-${n}.png`),b.toBuffer('image/png'));}
 }
 const sources=['masked-head-225.png','body-padded-375.png'],contract=JSON.parse(fs.readFileSync(path.join(dir,'contract.json'),'utf8'));
 for(const f of sources)assert.equal(sha(path.join(root,'artifacts/grab-contour-still-20261017-r33',f)),contract.sourceFiles[f]);
 report.unchangedSourceHashes=contract.sourceFiles;report.scriptHash=sha(path.join(__dirname,'grab-entry.cjs'));
 fs.writeFileSync(path.join(dir,'endpoint-check.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

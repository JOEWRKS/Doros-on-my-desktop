'use strict';
// Saved-output checks, not a replacement for white/dark visual inspection.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs');
async function main(){
 const dir=path.join(root,'artifacts/grab-contour-still-20261017-r32'),before=path.join(root,'artifacts/grab-contour-still-20261017-r31');
 const read=async(d,f)=>pixels(await api.loadImage(path.join(d,f)),api),head=await read(dir,'masked-head-225.png'),old=await read(before,'locked-head-225.png');
 const body=await read(dir,'body-padded-375.png'),oldBody=await read(before,'body-padded-375.png'),out=await read(dir,'candidate.png'),oldOut=await read(before,'candidate.png');
 const {sourceToLogical:{factor,dx,dy}}=JSON.parse(fs.readFileSync(path.join(dir,'measurement.json')));
 let alphaRemovals=0,changedNonWhite=0,changedSourceFace=0,changedBodyOutsideJoin=0,changedBody=0,changedOutputFace=0,changedLowerBody=0;
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4;if(![0,1,2,3].some(k=>head[i+k]!==old[i+k]))continue;
  const white=Math.min(...old.slice(i,i+3))>=210&&Math.max(...old.slice(i,i+3))-Math.min(...old.slice(i,i+3))<=12;
  if(!white)changedNonWhite++;
  assert.equal(head[i+3],0,'only alpha removal allowed');alphaRemovals++;
  if(x>=35&&x<=110&&y>=100&&y<=154)changedSourceFace++;
 }
 for(let y=0;y<375;y++)for(let x=0;x<375;x++){
  const i=(y*375+x)*4;if(![0,1,2,3].some(k=>body[i+k]!==oldBody[i+k]))continue;changedBody++;
  const sx=((x+.5)*96/225-32-dx)/factor,sy=((y+.5)*96/225-32-dy)/factor;
  if(!(sx>=61&&sx<=70&&sy>=45&&sy<=59))changedBodyOutsideJoin++;
 }
 for(let y=0;y<240;y++)for(let x=0;x<240;x++){
  const i=(y*240+x)*4;if(![0,1,2,3].some(k=>out[i+k]!==oldOut[i+k]))continue;
  const hx=((x+.5)/1.5-32)*225/96,hy=((y+.5)/1.5-32)*225/96;
  if(hx>=38&&hx<=107&&hy>=103&&hy<=151)changedOutputFace++;
  // Approved hair tip reaches below source y64; body-only rows start at y72.
  const sy=((y+.5)/1.5-32-dy)/factor;if(sy>=72)changedLowerBody++;
 }
 assert.equal(changedNonWhite,0);assert.equal(changedSourceFace,0);assert.equal(changedBodyOutsideJoin,0);assert.equal(changedOutputFace,0);assert.equal(changedLowerBody,0);
 const report={status:'TECHNICAL_CHECKS_ONLY_USER_ACCEPTANCE_PENDING',transitionConnected:false,runtimeSelected:false,alphaRemovals,changedNonWhite,changedSourceFace,sourceFaceRectangle:[35,100,110,154],changedBody,changedBodyOutsideJoin,changedOutputFace,outputFaceInHeadCoordinates:[38,103,107,151],changedLowerBody,limits:['Head exterior white alpha intentionally differs; whole head RGBA identity no longer claimed.','Nonwhite source ink is unchanged; interpolated edge output can differ when exterior alpha is removed.','Same pen settings; no stroke dilation or thickness adjustment.','Only one still at 1.5x; other rotations, scales, transitions and runtime untested.'],files:{}};
 for(const f of ['candidate.png','masked-head-225.png','locked-head-225.png','body-padded-375.png','white-dark-comparison.png','junction-inspection.png','left-junction-layers.png','measurement.json'])report.files[f]=crypto.createHash('sha256').update(fs.readFileSync(path.join(dir,f))).digest('hex');
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

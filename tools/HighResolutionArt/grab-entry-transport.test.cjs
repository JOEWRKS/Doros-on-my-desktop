'use strict';
// Saved-output checks, deliberately not a visual-acceptance gate.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs');
const dir=path.join(root,'artifacts/grab-entry-transport-20261017-r39');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const report=JSON.parse(fs.readFileSync(path.join(dir,'report.json'),'utf8'));
 assert.equal(report.scriptSha256,sha(path.join(__dirname,'grab-entry-transport.cjs')));
 for(const [f,h]of Object.entries(report.sourceHashes))assert.equal(sha(path.join(root,'artifacts/grab-contour-still-20261017-r33',f)),h);
 const common=await require('./current-motion.cjs').create(),base=api.createCanvas(240,240);base.getContext('2d').drawImage(common.render('idle',0),24,24);
 const first=await api.loadImage(path.join(dir,'probe-0.png')),last=await api.loadImage(path.join(dir,'probe-8.png'));
 assert.deepEqual(pixels(first,api),pixels(base,api));
 assert.deepEqual(pixels(last,api),pixels(await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/candidate.png')),api));
 const ref=pixels(first,api),records=[];
 for(let n=0;n<=8;n++){
  const file=`probe-${n}.png`,im=await api.loadImage(path.join(dir,file)),d=pixels(im,api);assert.equal(im.width,240);assert.equal(im.height,240);
  let border=0,centralFaceChanges=0;
  for(let y=0;y<240;y++)for(let x=0;x<240;x++){
   const i=(y*240+x)*4;if((x===0||y===0||x===239||y===239)&&d[i+3])border++;
   if(x>=80&&x<112&&y>=110&&y<140&&d.subarray(i,i+4).some((v,k)=>v!==ref[i+k]))centralFaceChanges++;
  }
  assert.equal(border,0);assert.equal(centralFaceChanges,0);
  records.push({file,sha256:sha(path.join(dir,file)),borderAlpha:border,centralFaceChanges});
 }
 const rig=common.s.postures.open.rig,{connectedHind,rootedHaunch}=require('./motion-polish.cjs');
 const bodies=[rig.render(rig.pose(0),{includeHead:false,extraParts:{haunch:rootedHaunch(0),nearHind:connectedHind(q=>q)}}),await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r33/body-padded-375.png'))];
 for(let n=0;n<2;n++){
  const c=api.createCanvas(bodies[n].width,bodies[n].height),g=c.getContext('2d');
  for(const name of ['front','rear'])g.drawImage(await api.loadImage(path.join(dir,`ownership-${n}-${name}.png`)),0,0);
  assert.deepEqual(pixels(c,api),pixels(bodies[n],api));
 }
 const result={status:'TECHNICAL_CHECKS_PASSED_VISUAL_CANDIDATE_REJECTED',exactSavedEndpoints:true,centralFaceRect:[80,110,112,140],records,limits:['Central-face rectangle is not a whole-head invariance proof.','Nine probe images are not dense temporal coverage.','No APNG, playback, other scale, release/landing or runtime verification.','Partition reconstruction is not a tested moving-limb compositor.']};
 fs.writeFileSync(path.join(dir,'saved-tests.json'),JSON.stringify(result,null,2));console.log(JSON.stringify({status:result.status,probes:records.length}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

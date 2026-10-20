'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {create,state,recovery,dir}=require('./landing-recovery-r49.cjs'),{panel}=require('./legacy-parity-review.cjs'),{apng}=require('./grab-tilt-audit.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const model=await create(),priorDir=path.join(root,'artifacts/legacy-parity-20261017-r48'),prior=JSON.parse(fs.readFileSync(path.join(priorDir,'manifest.json'))),sources={...prior.sources};
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 for(const f of ['tools/HighResolutionArt/landing-recovery-r49.cjs','tools/HighResolutionArt/r49-review.cjs','tools/HighResolutionArt/RecoveryTimingProbe/Program.cs','src/Dororong.App/Controls/SuppliedBodyDragFrames.cs','src/Dororong.App/Controls/ExtremeLandingPresentation.cs'])sources[f]=sha(path.join(root,f));
 const actual=JSON.parse(fs.readFileSync(path.join(dir,'legacy/timing.json')));let timingChecks=0;
 for(const r of actual.rows){const s=recovery(r.ms/1000,r.start);if(r.actualPosition!==null)assert(Math.abs(s.progress-r.actualPosition)<1e-9);else {assert.equal(s.progress,0);assert(s.bridge>0);}timingChecks++;}
 const records=[],animations=[],comparisons=[],ims=new Map();let headChecks=0;
 async function save(file,im,meta){fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));const loaded=await api.loadImage(path.join(dir,file));assert.deepEqual(pixels(loaded,api),pixels(im,api));const p=pixels(loaded,api);for(let n=0;n<im.width;n++)for(const [x,y]of [[n,0],[n,im.height-1],[0,n],[im.width-1,n]])assert.equal(p[(y*im.width+x)*4+3],0);ims.set(file,loaded);const r={file,...meta,sha256:sha(path.join(dir,file))};records.push(r);return r;}
 const hardTimes=[0,10,20,30,35,40,50,60,65,80,100,1100,...Array.from({length:18},(_,i)=>1120+i*40)];
 for(const scale of [1,1.25,1.5]){
  const sk=scale.toFixed(2);
  for(const ms of hardTimes){const im=model.hard(ms/1000,scale),file=`hard-${sk}-${ms}.png`;await save(file,im,{kind:'hard',scale,ms});
   const before=model.prior.hard(ms/1000,scale),a=pixels(before,api),b=pixels(im,api),p=require('./landing-motion.cjs').extreme(ms/1000),foot=model.prior.contact(p.amount);
   const protectedBottom=(80-p.lift+foot.sole+(73-foot.sole)*(1-p.squash))*scale;
   for(let y=0;y<Math.floor(protectedBottom);y++)for(let x=0;x<im.width;x++)for(let k=0;k<4;k++)assert.equal(b[(y*im.width+x)*4+k],a[(y*im.width+x)*4+k],'head sampling changed');headChecks++;
   const old=path.join(priorDir,`legacy/legacy-${ms}-${sk}.png`);if(fs.existsSync(old))comparisons.push({kind:'hard',scale,ms,current:file,old:path.relative(dir,old)});
  }
  for(const angle of [0,-65,65]){const id=`release-${sk}-${angle}`,sequence=[];for(let ms=0;ms<=440;ms+=20){const s=state(ms/1000,angle,[37.5,28.25]),file=`${id}-${ms}.png`,r=await save(file,model.render(s,scale),{kind:'release',id,scale,ms,angle,state:s,label:`${id} ${ms}ms body=${s.progress.toFixed(3)}`});sequence.push(file);
    if(ms===0)assert.deepEqual(pixels(ims.get(file),api),pixels(model.prior.render({...s,progress:1},scale),api));
    if(ms>=180)assert.deepEqual(pixels(ims.get(file),api),pixels(model.prior.render(s,scale),api));
    if(angle===0&&[0,20,60,100,140,160,180,260].includes(ms))comparisons.push({kind:'release',scale,ms,current:file,old:`legacy/presenter-${ms}-${sk}.png`});
   }
   const file=`preview-${id}.png`,packed=sequence.map(f=>panel(ims.get(f),records.find(r=>r.file===f).label).toBuffer('image/png'));fs.writeFileSync(path.join(dir,file),apng([...packed,...Array(15).fill(packed.at(-1))]));animations.push({id,file,sequence,frames:packed.length+15,sha256:sha(path.join(dir,file)),timing:'20ms source frames at30fps =0.6x speed;15-frame tail hold'});
  }
  // Partial release uses the same actual bank span+bridge timing, not full span.
  for(let ms=0;ms<=180;ms+=20){const s=state(ms/1000,0,[37.5,28.25],false,.4);await save(`partial-${sk}-${ms}.png`,model.render(s,scale),{kind:'partial',scale,ms,state:s});}
  // Exact timing landmarks, including start/end of the identity canonical bridge.
  for(const ms of [22.5,45,67.5,90,112.5,135,157.5,158,170]){const s=state(ms/1000,0,[37.5,28.25]);await save(`keytime-${sk}-${ms}.png`,model.render(s,scale),{kind:'keytime',scale,ms,state:s});}
  console.log('saved scale '+sk);
 }
 const unique=[...new Map(records.map(r=>[r.sha256,r])).values()],sheets=[];
 for(const scale of [1,1.25,1.5]){const list=unique.filter(r=>r.scale===scale),box=[Math.floor(44*scale),Math.floor(59*scale),Math.ceil(158*scale),Math.ceil(150*scale)];
  for(const r of list){const im=ims.get(r.file),p=pixels(im,api);for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(p[(y*im.width+x)*4+3])assert(x>=box[0]&&x<box[0]+box[2]&&y>=box[1]&&y<box[1]+box[3],'sheet crop');}
  for(let at=0;at<list.length;at+=12){const rows=list.slice(at,at+12),c=api.createCanvas(1200,Math.ceil(rows.length/4)*510),g=c.getContext('2d');g.imageSmoothingEnabled=false;for(let j=0;j<rows.length;j++){const r=rows[j],x=j%4*300,y=Math.floor(j/4)*510;g.fillStyle='#ddd';g.fillRect(x,y,300,510);g.fillStyle='#222';g.font='11px sans-serif';g.fillText(r.file,x+3,y+15);for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+k*244,298,242);g.drawImage(ims.get(r.file),...box,x+(298-box[2])/2,y+22+k*244,box[2],box[3]);}}const file=`review-${scale.toFixed(2)}-${at}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,records:rows.map(r=>r.file)});}
 }
 // Paired actual renderer samples at the same logical scale, no post-resize.
 const comparisonSheets=[];
 for(const scale of [1,1.25,1.5]){const list=comparisons.filter(r=>r.scale===scale);for(let at=0;at<list.length;at+=6){const rows=list.slice(at,at+6),c=api.createCanvas(1080,Math.ceil(rows.length/2)*490),g=c.getContext('2d');g.imageSmoothingEnabled=false;const box=[Math.floor(55*scale),Math.floor(65*scale),Math.ceil(150*scale),Math.ceil(140*scale)];for(let j=0;j<rows.length;j++){const r=rows[j],x=j%2*540,y=Math.floor(j/2)*490,old=await api.loadImage(path.join(dir,r.old));r.oldSha256=sha(path.join(dir,r.old));g.fillStyle='#ddd';g.fillRect(x,y,540,490);g.fillStyle='#222';g.font='12px sans-serif';g.fillText(`${r.kind} ${r.ms}ms ${scale}x | OLD left / NEW right`,x+3,y+16);for(const [k,bg]of ['white','#252a31'].entries())for(const [i,im]of [old,ims.get(r.current)].entries()){g.fillStyle=bg;g.fillRect(x+i*270,y+24+k*232,268,230);g.drawImage(im,...box,x+i*270+(268-box[2])/2,y+24+k*232,box[2],box[3]);}}const file=`compare-${scale.toFixed(2)}-${at}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));comparisonSheets.push({file,rows});}}
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_ONLY_DIRECT_REVIEW_PENDING',sources,timingChecks,headChecks,records,unique:unique.length,animations,comparisons,comparisonSheets,sheets,originalArtChanged:false,runtimeIntegrated:false},null,2));console.log(JSON.stringify({frames:records.length,unique:unique.length,sheets:sheets.length,comparisonSheets:comparisonSheets.length,timingChecks,headChecks}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

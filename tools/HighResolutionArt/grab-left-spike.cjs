'use strict';
// User-authorized removal of the left dangling ink only. Isolated still.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
async function main(){
 const before=path.join(root,'artifacts/grab-contour-still-20261017-r32'),dir=path.join(root,'artifacts/grab-contour-still-20261017-r33');fs.mkdirSync(dir,{recursive:true});
 const head=await api.loadImage(path.join(before,'masked-head-225.png'));
 const grid=api.createCanvas(700,560),g=grid.getContext('2d');g.fillStyle='white';g.fillRect(0,0,700,560);g.imageSmoothingEnabled=false;g.drawImage(head,28,145,28,27,70,55,504,486);
 g.font='12px sans-serif';g.strokeStyle='#8ca5af';g.fillStyle='#222';
 for(let i=0;i<=28;i++){g.beginPath();g.moveTo(70+i*18,55);g.lineTo(70+i*18,541);g.stroke();g.fillText(28+i,70+i*18,42);}
 for(let i=0;i<=27;i++){g.beginPath();g.moveTo(70,55+i*18);g.lineTo(574,55+i*18);g.stroke();g.fillText(145+i,35,55+i*18);}
 fs.writeFileSync(path.join(dir,'source-grid.png'),grid.toBuffer('image/png'));
 // The annotated source grid shows the normal diagonal ending on row163;
 // rows164..167 contain the downward dark stalk, not the pink hair tip.
 const old=pixels(head,api),fixed=api.createCanvas(225,225),fg=fixed.getContext('2d'),data=fg.createImageData(225,225);data.data.set(old);
 const removed=[];
 for(let y=164;y<=169;y++)for(let x=36;x<=41;x++){
  const i=(y*225+x)*4;if(!old[i+3])continue;
  removed.push({x,y,rgba:[...old.slice(i,i+4)]});data.data[i+3]=0;
 }
 fg.putImageData(data,0,0);
 const body=await api.loadImage(path.join(before,'body-padded-375.png')),bp=premultiply(body,api),hp=premultiply(fixed,api),a=new Float64Array(4),b=new Float64Array(4);
 const candidate=surface(api,144,(x,y)=>{sample(bp,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);},32);
 const save=(name,canvas)=>fs.writeFileSync(path.join(dir,name),canvas.toBuffer('image/png'));
 save('masked-head-225.png',fixed);save('candidate.png',candidate);
 for(const f of ['locked-head-225.png','body-padded-375.png'])fs.copyFileSync(path.join(before,f),path.join(dir,f));
 const meta=JSON.parse(fs.readFileSync(path.join(before,'measurement.json')));
 fs.writeFileSync(path.join(dir,'measurement.json'),JSON.stringify({...meta,revision:'r33',removedSpikeHeadBounds:[36,164,41,169]},null,2));
 const prev=await api.loadImage(path.join(before,'candidate.png'));
 const sheet=api.createCanvas(960,580),sg=sheet.getContext('2d');
 for(let col=0;col<4;col++){
  const dark=col>=2,img=col%2?candidate:prev,left=col*240;
  sg.fillStyle=dark?'#252a31':'white';sg.fillRect(left,0,240,580);sg.fillStyle=dark?'white':'#222';sg.font='18px sans-serif';sg.fillText(col%2?'After':'Before',left+80,30);
  sg.drawImage(img,left,25);sg.imageSmoothingEnabled=false;sg.drawImage(img,66,139,24,32,left+24,290,192,256);
 }save('white-dark-comparison.png',sheet);
 const dark=api.createCanvas(480,260),dg=dark.getContext('2d');dg.fillStyle='#252a31';dg.fillRect(0,0,480,260);dg.drawImage(candidate,120,0);save('actual-size-dark.png',dark);
 // Read saved files, not the in-memory intended result.
 const saved=pixels(await api.loadImage(path.join(dir,'masked-head-225.png')),api),out=pixels(await api.loadImage(path.join(dir,'candidate.png')),api),oldOut=pixels(prev,api);
 let changed=0,outside=0,changedOutput=0,changedOutputOutside=0;
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4;if(![0,1,2,3].some(k=>saved[i+k]!==old[i+k]))continue;changed++;
  if(x<36||x>41||y<164||y>169)outside++;
 }
 for(let y=0;y<240;y++)for(let x=0;x<240;x++){
  const i=(y*240+x)*4;if(![0,1,2,3].some(k=>out[i+k]!==oldOut[i+k]))continue;changedOutput++;
  if(x<69||x>77||y<151||y>158)changedOutputOutside++;
 }
 assert.equal(outside,0);assert.equal(changedOutputOutside,0);assert.ok(removed.length>0);
 const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
 assert.equal(hash(path.join(dir,'body-padded-375.png')),hash(path.join(before,'body-padded-375.png')));
 const report={status:'STILL_CANDIDATE_USER_ACCEPTANCE_PENDING',transitionConnected:false,runtimeSelected:false,removed,changedHeadPixels:changed,changedHeadOutsidePatch:outside,changedOutputPixels:changedOutput,changedOutputOutsidePatch:changedOutputOutside,outputPatch:[69,151,77,158],bodyFileExactlyUnchanged:true,limits:['Local dark stalk removal explicitly authorized; not whole-head identity.','Normal ink and all other source pixels outside the patch preserved.','Only this 1.5x still inspected; transitions, rotation and other scales not tested.'],files:{}};
 for(const f of ['masked-head-225.png','candidate.png','body-padded-375.png','white-dark-comparison.png','actual-size-dark.png','source-grid.png'])report.files[f]=hash(path.join(dir,f));
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({changed,changedOutput,outside,changedOutputOutside,removed}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

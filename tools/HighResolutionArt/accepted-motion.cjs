'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {faceRegion}=require('./face-body-assembly-review.cjs');
const {create:posture}=require('./posture.cjs'),{create:upright}=require('./upright.cjs');
const happy=require('./happy.cjs');
const approved='artifacts/high-resolution-cheek-20261016-r15';
const out=path.join(root,'artifacts/high-resolution-motion-20261016-r16');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const sourceFiles=['accepted-motion.cjs','posture.cjs','upright.cjs','sampling.cjs','parts.cjs','body.cjs','happy.cjs','../PreviewLocomotion/reference-parts.cjs'];
async function setup({polished=false,fixedRunRoots=false,connected=false,outlineRecovery=false,restoreHairTip=false,pointedHairTip=false,size=144}={}){
 restoreHairTip=restoreHairTip||pointedHairTip;
 const closed=await api.loadImage(path.join(root,approved,'assembly-source-225.png'));
 const approvedMaster=await api.loadImage(path.join(root,approved,'candidate-225.png'));
 const original=await api.loadImage(path.join(__dirname,'input/canonical-225.png'));
 const measurement=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-canonical.png'));
 const open=api.createCanvas(225,225),g=open.getContext('2d');g.drawImage(closed,0,0);
 const d=g.getImageData(0,0,225,225),o=pixels(original,api);
 // Same accepted body/ribbon source. Only restore the original open face.
 for(let y=0;y<225;y++)for(let x=0;x<225;x++)if(faceRegion(x,y)){const i=(y*225+x)*4;d.data.set(o.subarray(i,i+4),i);}g.putImageData(d,0,0);
 const originals={open,closed},postures={},masters={},rigs={};
 for(const [eye,source] of Object.entries(originals)){
  postures[eye]=posture(source,measurement,api,{smoothSit:polished,connected,outlineRecovery,restoreHairTip,pointedHairTip,size});masters[eye]=postures[eye].master;
  // Closed expression is a rigid head texture, never the legacy eye patch or gaze warp.
  rigs[eye]=upright(masters[eye],api,{padding:16,gaze:eye==='open',reachRun:polished,fixedRunRoots,connected,outlineRecovery,size});
 }
 if(!restoreHairTip)assert.deepEqual(pixels(masters.closed,api),pixels(approvedMaster,api),'Accepted r15 master drift');
 else {
  const a=pixels(masters.closed,api),b=pixels(approvedMaster,api),{inside,hairTipBoundary}=require('./parts.cjs');
  for(let y=0;y<225;y++)for(let x=0;x<225;x++)if(!inside(hairTipBoundary,x+.5,y+.5)&&!(pointedHairTip&&require('./pointed-tip.cjs').contains(x,y))){
   const i=(y*225+x)*4;assert.deepEqual(a.slice(i,i+4),b.slice(i,i+4),'Outside approved hair-tip repair');
  }
 }
 return {api,root,originals,masters,postures,rigs,approvedMaster,measurement};
}
async function pages(manifest){
 const index=[];
 for(let start=0;start<manifest.frames.length;start+=24){
  const batch=manifest.frames.slice(start,start+24),c=api.createCanvas(1152,864),g=c.getContext('2d');
  const light=(start/24)%2===0;g.fillStyle=light?'#eeeae4':'#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,f] of batch.entries()){const x=i%6*192,y=Math.floor(i/6)*216;g.drawImage(await api.loadImage(path.join(out,f.name+'.png')),x,y);g.fillStyle=light?'#302b33':'#fff';g.font='12px sans-serif';g.fillText(f.name,x+8,y+204);}
  const file=`review-${String(start/24+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,file),c.toBuffer('image/png'));index.push({file,frames:batch.map(f=>f.name)});
 }
 // Every closed face at 2x output scale: no sampling frame omitted.
 const closed=manifest.frames.filter(f=>f.name.includes('closed'));
 for(let start=0;start<closed.length;start+=24){
  const batch=closed.slice(start,start+24),c=api.createCanvas(1152,864),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,f] of batch.entries()){const x=i%6*192,y=Math.floor(i/6)*216;g.drawImage(await api.loadImage(path.join(out,f.name+'.png')),45,75,90,90,x,y,180,180);g.fillStyle='#302b33';g.font='12px sans-serif';g.fillText(f.name,x+4,y+202);}
  const file=`faces-${String(start/24+1).padStart(2,'0')}.png`;fs.writeFileSync(path.join(out,file),c.toBuffer('image/png'));index.push({file,detail:'closed face x2',frames:batch.map(f=>f.name)});
 }
 for(const kind of ['walk','run']){
  const list=manifest.frames.filter(f=>f.name.startsWith(kind+'-open-')),c=api.createCanvas(960,Math.ceil(list.length/8)*120),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,f] of list.entries()){const x=i%8*120,y=Math.floor(i/8)*120;g.drawImage(await api.loadImage(path.join(out,f.name+'.png')),87,123,51,38,x,y,102,76);g.fillStyle='#fff';g.font='12px sans-serif';g.fillText(f.name,x+2,y+98);}
  const file=`legs-${kind}.png`;fs.writeFileSync(path.join(out,file),c.toBuffer('image/png'));index.push({file,detail:'hind legs x2',frames:list.map(f=>f.name)});
 }
 fs.writeFileSync(path.join(out,'review-index.json'),JSON.stringify(index,null,2)+'\n');
}
async function video(){
 const cache=new Map(),load=async n=>{if(!cache.has(n))cache.set(n,await api.loadImage(path.join(out,n+'.png')));return cache.get(n);};
 const dir=path.join(out,'video-frames');fs.mkdirSync(dir,{recursive:true});const f=(k,n)=>k+'-'+String(n).padStart(2,'0');
 for(let n=0;n<180;n++){
  const age=n/30,sit=Math.max(0,Math.min(1,age<2?(age-.4)/1.05:1-(age-2.6)/.65));
  const items=[f('track-closed',Math.round(12+12*Math.sin(age*Math.PI/2))),f('walk-open',n%32),f('run-open',n%48),f('sit-closed',Math.round(sit*64)),f('breathe-closed',Math.round((1-Math.cos(age*Math.PI))/2*32)),f('happy-closed',Math.min(65,Math.round(age%3/2.16*65)))];
  const c=api.createCanvas(576,384),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,name] of items.entries())g.drawImage(await load(name),i%3*192,Math.floor(i/3)*192);
  fs.writeFileSync(path.join(dir,String(n).padStart(3,'0')+'.png'),c.toBuffer('image/png'));
 }
}
async function main(){
 const s=await setup();fs.mkdirSync(out,{recursive:true});
 const manifest={schema:1,status:'r15 still accepted; r16 motion review pending',runtimeSelected:false,logicalSize:96,sourceSize:225,contentSize:144,outputSize:192,origin:[24,24],sources:{},frames:[],pending:['Motion visual acceptance','Squint and remaining hunt/jump/sleep/grab/perch/cheek/eating migration','Windows/Mac runtime integration']};
 for(const f of [...sourceFiles.map(f=>path.relative(root,path.join(__dirname,f))),...['candidate-225.png','assembly-source-225.png','head-layer-225.png'].map(f=>approved+'/'+f)])manifest.sources[f]=sha(fs.readFileSync(path.join(root,f)));
 function save(name,c,parameters){const b=c.toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),b);manifest.frames.push({name,width:c.width,height:c.height,sha256:sha(b),parameters});}
 for(const eye of ['open','closed']){
  const rig=s.rigs[eye];fs.writeFileSync(path.join(out,'master-'+eye+'-225.png'),s.masters[eye].toBuffer('image/png'));
  save('standing-'+eye,rig.still(),{});
  for(const walking of [true,false])for(let i=0,count=walking?32:48;i<count;i++){const p={amount:1,phase:i/count,walking};save(`${walking?'walk':'run'}-${eye}-${String(i).padStart(2,'0')}`,rig.render(p),p);}
  console.log(eye+' gaits exported');
  for(let i=0;i<=24;i++){const roll=(-20+i/24*34)*Math.PI/180,p={roll,eyeX:eye==='open'?roll*2:0,eyeY:eye==='open'?-roll*1.5:0};save(`track-${eye}-${String(i).padStart(2,'0')}`,rig.render(p),p);}
  for(let i=0;i<=32;i++)save(`breathe-${eye}-${String(i).padStart(2,'0')}`,rig.render({breathing:i/32}),{breathing:i/32});
  for(const kind of ['sit','rise'])for(let i=0;i<=64;i++){const t=kind==='sit'?i/64:1-i/64;save(`${kind}-${eye}-${String(i).padStart(2,'0')}`,s.postures[eye].render(t),{posture:t});}
  console.log(eye+' tracking/breathing/sit/rise exported');
 }
 for(let i=0;i<=65;i++){const p={roll:happy.roll(i/65*happy.times.at(-1))};save(`happy-closed-${String(i).padStart(2,'0')}`,s.rigs.closed.render(p),p);}
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');await pages(manifest);await video();console.log(manifest.frames.length+' frames and review sheets exported to '+out);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={setup,main,out};

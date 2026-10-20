'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {setup}=require('./accepted-motion.cjs'),{pixels}=require('./sampling.cjs'),{inside,hairTipBoundary}=require('./parts.cjs'),{create:upright}=require('./upright.cjs');
const outName='artifacts/hair-tip-boundary-20261017-r25',sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 const opts={polished:true,fixedRunRoots:true,connected:true},old=await setup(opts),current=await setup({...opts,restoreHairTip:true}),{api,root}=old,out=path.join(root,outName);fs.mkdirSync(out,{recursive:true});
 const original=await api.loadImage(path.join(__dirname,'input/canonical-225.png')),orig=pixels(original,api),report={runtimeSelected:false,outlineRecovery:false,sourceChecks:[],frames:[],sheets:[]};
 for(const eye of ['open','closed']){
  const a=pixels(old.rigs[eye].parts.head,api),b=pixels(current.rigs[eye].parts.head,api),src=pixels(current.originals[eye],api);let restoredDark=0,changed=0;
  for(let y=0;y<225;y++)for(let x=0;x<225;x++){
   const at=(y*225+x)*4,inRepair=inside(hairTipBoundary,x+.5,y+.5);
   if(inRepair){assert.deepEqual(b.slice(at,at+4),src.slice(at,at+4),'exact source RGBA in repair');assert.deepEqual(b.slice(at,at+4),orig.slice(at,at+4),'exact canonical RGBA in repair');}
   else assert.deepEqual(b.slice(at,at+4),a.slice(at,at+4),'protected head pixels');
   if(a.slice(at,at+4).some((v,k)=>v!==b[at+k])){changed++;if(src[at+3]>100&&Math.max(...src.slice(at,at+3))<180)restoredDark++;}
   // Independent rectangle excludes the body stroke. All original inner-tip
   // ink, not just the preselected missing27 pixels, must survive the split.
   if(x>=96&&x<122&&y>=140&&y<173&&src[at+3]>100&&Math.max(...src.slice(at,at+3))<180)assert.deepEqual(b.slice(at,at+4),src.slice(at,at+4),'all source tip ink retained');
  }
  assert.equal(restoredDark,27);
  assert.deepEqual(pixels(old.rigs[eye].parts.body,api),pixels(current.rigs[eye].parts.body,api),'procedural body unchanged');
  const bodyPath=path.join(out,`${eye}-head-225.png`);fs.writeFileSync(bodyPath,current.rigs[eye].parts.head.toBuffer('image/png'));
  report.sourceChecks.push({eye,restoredDark,changed,protectedHeadIdentical:true,bodyIdentical:true,restoredPixelsEqualCanonical:true});
 }
 const sourceSheet=api.createCanvas(1120,410),sg=sourceSheet.getContext('2d');sg.fillStyle='#eeeae4';sg.fillRect(0,0,1120,410);sg.imageSmoothingEnabled=false;
 for(const [i,im]of [original,old.rigs.closed.parts.head,current.rigs.closed.parts.head,current.masters.closed].entries()){
  sg.fillStyle='#302b33';sg.font='16px sans-serif';sg.fillText(['Original225','Old cut','Repaired cut','Reassembled'][i],i*280+10,26);sg.drawImage(im,96,140,35,38,i*280,50,280,304);
 }fs.writeFileSync(path.join(out,'source-comparison.png'),sourceSheet.toBuffer('image/png'));
 if(process.argv.includes('--source-only')){console.log(JSON.stringify({out,sourceChecks:report.sourceChecks}));return;}
 const scales=[1,1.1,1.17,1.2,1.3,1.333,1.4,1.5],angles=Array.from({length:18},(_,i)=>-20+i*2);report.scales=scales;report.angles=angles;
 const summary=api.createCanvas(900,630),sum=summary.getContext('2d');sum.fillStyle='#eeeae4';sum.fillRect(0,0,900,630);sum.imageSmoothingEnabled=false;
 for(const scale of scales)for(const eye of ['open','closed']){
  const rigOpts={size:96*scale,padding:16,gaze:eye==='open',reachRun:true,fixedRunRoots:true,connected:true};
  const before=upright(old.masters[eye],api,rigOpts),after=upright(current.masters[eye],api,rigOpts);
  const sheet=api.createCanvas(1200,1080),g=sheet.getContext('2d');g.fillStyle=eye==='open'?'#eeeae4':'#252a31';g.fillRect(0,0,1200,1080);g.imageSmoothingEnabled=false;
  for(const [n,deg]of angles.entries()){
   const roll=deg*Math.PI/180,co=Math.cos(roll),si=Math.sin(roll),a=before.render({roll}),b=after.render({roll}),ad=pixels(a,api),bd=pixels(b,api);let changed=0;
   for(let y=0;y<b.height;y++)for(let x=0;x<b.width;x++){
    const at=(y*b.width+x)*4;if(!ad.slice(at,at+4).some((v,k)=>v!==bd[at+k]))continue;changed++;
    const dx=(x+.5)/scale-16-43,dy=(y+.5)/scale-16-66,sx=(43+dx*co+dy*si)*225/96-.5,sy=(66-dx*si+dy*co)*225/96-.5;
    // Original cubic support plus half a physical output pixel; only the
    // moved cut can differ. Face/body outside this local support stay exact.
    const support=2+225/(96*scale)*.5;
    assert.ok(sx>=96-support&&sx<=109+support&&sy>=149-support&&sy<=171+support,'output changed outside restored tip support');
   }
   const name=`${eye}-${scale.toFixed(3)}-${String(n).padStart(2,'0')}`,png=b.toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),png);
   const x=n%6*200,y=Math.floor(n/6)*360;g.drawImage(b,x+Math.floor((200-b.width)/2),y+20);
   // Follow the original inner-tip center so each detail includes the restored
   // edge even at the two rotation extremes. No aspect-ratio deformation.
   const hx=104*96/225-43,hy=159*96/225-66,cx=(43+hx*co-hy*si+16)*scale,cy=(66+hx*si+hy*co+16)*scale;
   const crop=[Math.floor(cx-10*scale),Math.floor(cy-11*scale),Math.ceil(20*scale),Math.ceil(22*scale)];
   g.drawImage(b,...crop,x+46,y+210,108,108*crop[3]/crop[2]);g.fillStyle=eye==='open'?'#302b33':'#fff';g.font='12px sans-serif';g.fillText(`${eye} ${scale}x ${deg}deg`,x+5,y+16);
   report.frames.push({name,eye,scale,deg,changed,protectedOutputIdentical:true,sha256:sha(png)});
   const row=[1,1.2,1.5].indexOf(scale);if(eye==='closed'&&deg===0&&row>=0){for(const [i,im]of [a,b].entries()){sum.drawImage(im,i*250+Math.floor((240-im.width)/2),row*210+25);sum.fillStyle='#302b33';sum.font='16px sans-serif';sum.fillText(`${scale}x ${i?'repaired cut':'old cut'}`,i*250+10,row*210+18);sum.drawImage(im,...crop,510+i*190,row*210+30,150,150*crop[3]/crop[2]);}}
  }
  const sheetName=`sheet-${eye}-${scale.toFixed(3)}.png`;fs.writeFileSync(path.join(out,sheetName),sheet.toBuffer('image/png'));report.sheets.push(sheetName);
 }
 fs.writeFileSync(path.join(out,'comparison-short.png'),summary.toBuffer('image/png'));report.sources={};
 for(const file of ['parts.cjs','posture.cjs','upright.cjs','sampling.cjs','accepted-motion.cjs','connected-families.cjs','hair-tip-proof.cjs'])report.sources['tools/HighResolutionArt/'+file]=sha(fs.readFileSync(path.join(__dirname,file)));
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({out,frames:report.frames.length,sourceChecks:report.sourceChecks}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

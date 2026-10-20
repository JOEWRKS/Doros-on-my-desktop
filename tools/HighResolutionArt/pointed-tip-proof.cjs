'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {setup}=require('./accepted-motion.cjs'),{pixels}=require('./sampling.cjs'),{create:upright}=require('./upright.cjs');
const {contains}=require('./pointed-tip.cjs');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 const opts={polished:true,fixedRunRoots:true,connected:true,restoreHairTip:true};
 const old=await setup(opts),now=await setup({...opts,pointedHairTip:true}),{api,root}=now;
 const out=path.join(root,'artifacts/pointed-hair-tip-20261017-r27');fs.mkdirSync(out,{recursive:true});
 const report={runtimeSelected:false,sourceChecks:[],frames:[],sheets:[]};
 const canonical=pixels(await api.loadImage(path.join(__dirname,'input/canonical-225.png')),api);
 for(const eye of ['open','closed']){
  const a=pixels(old.rigs[eye].parts.head,api),b=pixels(now.rigs[eye].parts.head,api);let changed=0;
  for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;
   if(!contains(x,y))assert.deepEqual(a.slice(i,i+4),b.slice(i,i+4),'protected source head');
   if(a.slice(i,i+4).some((v,k)=>v!==b[i+k]))changed++;
  }
  assert.ok(changed>0);assert.deepEqual(pixels(old.rigs[eye].parts.body,api),pixels(now.rigs[eye].parts.body,api));
  const widths=[];
  function run(d,y){let end=114;const dark=x=>{const i=(y*225+x)*4;return d[i+3]>200&&Math.max(...d.slice(i,i+3))<64;};
   while(end>=97&&!dark(end))end--;let start=end;while(start>=97&&dark(start))start--;return {start:start+1,end,width:end-start};}
  for(let y=159;y<=165;y++){const source=run(canonical,y),candidate=run(b,y);assert.ok(candidate.width>=1&&candidate.width<=3);widths.push({y,source,candidate});}
  const dark=new Set();for(let y=159;y<=168;y++)for(let x=97;x<=114;x++){const i=(y*225+x)*4;if(b[i+3]>200&&Math.max(...b.slice(i,i+3))<64)dark.add(y*225+x);}
  const visited=new Set(),queue=[160*225+110];
  for(let i=0;i<queue.length;i++){const at=queue[i];if(visited.has(at)||!dark.has(at))continue;visited.add(at);for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++)queue.push(at+dy*225+dx);}
  assert.ok([...visited].some(i=>Math.floor(i/225)===167&&i%225<=99),'dark junction reaches pointed endpoint');
  report.sourceChecks.push({eye,changed,protectedHeadExact:true,bodyExact:true,outerCoreRowWidths:widths,darkJunctionConnected:true});
  fs.writeFileSync(path.join(out,eye+'-head-225.png'),now.rigs[eye].parts.head.toBuffer('image/png'));
 }
 const priorDir=path.join(root,'artifacts/pointed-hair-tip-20261017-r26');
 const priorHead=await api.loadImage(path.join(priorDir,'open-head-225.png'));
 const priorFrame=await api.loadImage(path.join(priorDir,'open-1.5-10.png'));
 const summary=api.createCanvas(900,520),g=summary.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,900,520);g.imageSmoothingEnabled=false;
 for(const [n,[label,frame,head]]of [
  ['Original',old.rigs.open.still(),old.masters.open],
  ['Previous',priorFrame,priorHead],
  ['Corrected',now.rigs.open.still(),now.masters.open]].entries()){
  g.fillStyle='#302b33';g.font='18px sans-serif';g.fillText(label,n*300+22,27);
  g.drawImage(frame,n*300+54,40);
  g.fillStyle='#fff';g.fillRect(n*300+30,245,240,240);
  g.drawImage(head,94,150,24,24,n*300+30,245,240,240);
 }fs.writeFileSync(path.join(out,'comparison.png'),summary.toBuffer('image/png'));
 if(process.argv.includes('--source-only')){console.log(JSON.stringify({out,checks:report.sourceChecks}));return;}
 for(const scale of [1,1.1,1.17,1.2,1.3,1.333,1.4,1.5]){
  const sheet=api.createCanvas(1200,1440),g=sheet.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,1200,1440);g.imageSmoothingEnabled=false;
  for(const [e,eye]of ['open','closed'].entries()){
   const ro={size:96*scale,padding:16,gaze:eye==='open',reachRun:true,fixedRunRoots:true,connected:true},before=upright(old.masters[eye],api,ro),after=upright(now.masters[eye],api,ro);
   for(let k=0;k<18;k++){
    const deg=-20+2*k,roll=deg*Math.PI/180,c=Math.cos(roll),s=Math.sin(roll),a=before.render({roll}),b=after.render({roll}),ad=pixels(a,api),bd=pixels(b,api);let changed=0;
    for(let y=0;y<b.height;y++)for(let x=0;x<b.width;x++){const i=(y*b.width+x)*4;
     if(!ad.slice(i,i+4).some((v,j)=>v!==bd[i+j]))continue;changed++;
     const dx=(x+.5)/scale-16-43,dy=(y+.5)/scale-16-66,sx=(43+dx*c+dy*s)*225/96-.5,sy=(66-dx*s+dy*c)*225/96-.5,r=2+225/96/scale*.71;
     assert.ok(sx>=97-r&&sx<=111+r&&sy>=161-r&&sy<=170+r,'protected rotated output');
    }
    const name=`${eye}-${scale}-${k}`,png=b.toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),png);
    report.frames.push({name,eye,scale,deg,changed,protectedOutputExact:true,sha256:sha(png)});
    const n=e*18+k,x=n%6*200,y=Math.floor(n/6)*240;
    g.fillStyle='#fff';g.font='12px sans-serif';g.fillText(`${eye} ${scale}x ${deg}deg`,x+5,y+15);g.drawImage(b,x+(200-b.width)/2,y+15);
    const hx=103*96/225-43,hy=165*96/225-66,cx=(43+hx*c-hy*s+16)*scale,cy=(66+hx*s+hy*c+16)*scale;
    const w=Math.ceil(9*scale);g.drawImage(b,Math.floor(cx-w/2),Math.floor(cy-w/2),w,w,x+75,y+185,50,50);
   }
  }
  const file=`sheet-${scale}.png`;fs.writeFileSync(path.join(out,file),sheet.toBuffer('image/png'));report.sheets.push(file);console.log(file);
 }
 report.sources={};for(const f of ['pointed-tip.cjs','pointed-tip-proof.cjs','posture.cjs','accepted-motion.cjs','connected-families.cjs','parts.cjs','sampling.cjs','upright.cjs'])report.sources[f]=sha(fs.readFileSync(path.join(__dirname,f)));
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({frames:report.frames.length,checks:report.sourceChecks}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

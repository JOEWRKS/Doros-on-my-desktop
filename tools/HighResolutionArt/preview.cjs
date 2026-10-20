'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const api=require(process.env.LOCOMOTION_CANVAS || path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const fixes=require('./canonical-fixes.cjs');
(async()=>{
 const repo=path.resolve(__dirname,'../..');
 const input=path.join(__dirname,'input/canonical-225.png');
 const out=path.join(repo,'artifacts/scale-fixed-corrections-20261016');
 fs.mkdirSync(out,{recursive:true});
 const bytes=fs.readFileSync(input),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
 if(sha(bytes)!=='1d84aa57e2824add4450427d56b0d45eb729b5eedbe9f0fcc7a6fab9f766483c')throw Error('Unexpected input');
 const source=await api.loadImage(input),before=api.createCanvas(225,225);before.getContext('2d').drawImage(source,0,0);
 const after=fixes.render(source,api),a=before.getContext('2d').getImageData(0,0,225,225).data,b=after.getContext('2d').getImageData(0,0,225,225).data;
 const changes=[];let outside=0;
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4;if(a.subarray(i,i+4).some((v,j)=>v!==b[i+j])){
   changes.push([x,y]);if(!Object.values(fixes.regions).some(([rx,ry,w,h])=>x>=rx&&x<rx+w&&y>=ry&&y<ry+h))outside++;
  }
 }
 if(outside)throw Error('Changes escaped correction regions');
 const save=(name,c)=>fs.writeFileSync(path.join(out,name+'.png'),c.toBuffer('image/png'));
 save('canonical-fixed-225',after);
 for(const [name,c] of [['before',before],['after',after]]) {
  const native=api.createCanvas(144,144),ctx=native.getContext('2d');ctx.imageSmoothingQuality='high';ctx.drawImage(c,0,0,144,144);save(name+'-144',native);
 }
 const comparison=api.createCanvas(900,600),ctx=comparison.getContext('2d');
 ctx.fillStyle='#e2e2e2';ctx.fillRect(0,0,900,600);
 ctx.imageSmoothingEnabled=false;
 ctx.drawImage(before,100,95,85,110,35,35,340,440);ctx.drawImage(after,100,95,85,110,485,35,340,440);
 ctx.fillStyle='#292329';ctx.font='20px sans-serif';ctx.fillText('Before',35,520);ctx.fillText('After',485,520);save('details-before-after',comparison);
 const report={input,inputSha256:sha(bytes),regions:fixes.regions,changedPixels:changes.length,outsideChanges:outside,changes,masterSha256:sha(after.toBuffer('image/png')),productionChanged:false};
 fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify(report,null,2)+'\n');
 console.log(JSON.stringify({...report,changes:undefined}));
})();

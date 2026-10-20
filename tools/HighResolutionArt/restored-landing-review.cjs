'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs'),{diff}=require('./grab-entry-transport.cjs');
const {create,pose,dir}=require('./restored-landing.cjs'),{apng}=require('./grab-tilt-audit.cjs'),{presentation}=require('./grab-release-review.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const curves=JSON.parse(fs.readFileSync(path.join(dir,'runtime-curves.json'))),m=await create();let comparisons=0;
 for(const r of curves.ordinary){const p=pose(r.ms/1000,{strength:r.strength,distance:r.distance});assert(Math.abs(p.squash-r.compression)<1e-12);assert(Math.abs(p.y-r.y)<1e-12);comparisons++;}
 for(const r of curves.strong){const p=pose(.22+r.ms/1000,{hard:true});assert(Math.abs(p.squash-r.squash)<1e-12);assert(Math.abs(p.spread-r.spread)<1e-12);assert(Math.abs(p.lift-r.lift)<1e-12);comparisons++;}
 const inherited=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-release-20261017-r45/manifest.json'))).sources,sources={...inherited};for(const [f,h]of Object.entries(inherited))assert.equal(sha(path.join(root,f)),h);
 for(const f of ['tools/HighResolutionArt/restored-landing.cjs','tools/HighResolutionArt/restored-landing-review.cjs','tools/HighResolutionArt/landing-motion.cjs','src/Dororong.Core/Platforms/PlatformMotion.cs','src/Dororong.App/Controls/PlatformContactPresentation.cs','tools/HighResolutionArt/RuntimeMotionProbe/Program.cs','tools/HighResolutionArt/RuntimeMotionProbe/RuntimeMotionProbe.csproj'])sources[f]=sha(path.join(root,f));
 const scenarios=[{name:'normal',options:{},end:.44},{name:'partial',options:{strength:.4,distance:18},end:.44},{name:'strong',options:{hard:true},end:2.02}],frames=[],animations=[],unique=new Map(),endpoints=[];
 for(const scale of [1,1.25,1.5])for(const scene of scenarios){const list=[],packed=[],id=`${scene.name}-${scale.toFixed(2)}`;
  for(let i=0;i<=Math.round(scene.end/.02);i++){
   const t=Math.min(scene.end,i*.02),im=m.render(t,scene.options,scale),file=`${id}-${String(i).padStart(3,'0')}.png`;fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));const saved=await api.loadImage(path.join(dir,file));assert.deepEqual(pixels(saved,api),pixels(im,api));
   const data=pixels(saved,api);for(let n=0;n<im.width;n++)for(const [x,y]of [[n,0],[n,im.height-1],[0,n],[im.width-1,n]])assert.equal(data[(y*im.width+x)*4+3],0,'clipped '+file);
   const r={id,file,scale,index:i,seconds:t,options:scene.options,sha256:sha(path.join(dir,file)),pose:pose(t,scene.options)};frames.push(r);list.push(saved);if(!unique.has(r.sha256))unique.set(r.sha256,{...r,aliases:[]});unique.get(r.sha256).aliases.push(file);
   packed.push(presentation(saved,scale,`${scene.name} ${scale}x / ${Math.round(t*1000)}ms`,(32+87+(scene.options.distance??36))*scale).toBuffer('image/png'));
  }
  const initial=diff(list[0],m.prior.previous.entry.render(scene.options.strength??1,scale));assert.equal(initial.changedPixels,0);endpoints.push({id,start:initial});
  // Final shape equals the independently rendered ordinary idle at the release floor.
  const end=api.createCanvas(list[0].width,list[0].height);end.getContext('2d').drawImage(m.prior.previous.idle(scale),0,(scene.options.distance??36)*scale);
  // Fractional18*1.25 translation has different Canvas resampling: don't assert that surrogate.
  if(Number.isInteger((scene.options.distance??36)*scale)){const d=diff(list.at(-1),end);assert.equal(d.changedPixels,0);endpoints.at(-1).finish=d;}
  const movie=[...Array(12).fill(packed[0]),...packed,...Array(24).fill(packed.at(-1))],file=`preview-${id}.png`;fs.writeFileSync(path.join(dir,file),apng(movie));animations.push({file,id,frames:movie.length,transitionFrames:packed.length,sha256:sha(path.join(dir,file))});console.log(id+' '+list.length);
 }
 const sheets=[];for(const scale of [1,1.25,1.5]){const rows=[...unique.values()].filter(r=>r.scale===scale);for(let from=0;from<rows.length;from+=18){const items=rows.slice(from,from+18),c=api.createCanvas(1560,Math.ceil(items.length/6)*510),g=c.getContext('2d');g.imageSmoothingEnabled=false;
  for(let j=0;j<items.length;j++){const r=items[j],im=await api.loadImage(path.join(dir,r.file)),x=j%6*260,y=Math.floor(j/6)*510;g.fillStyle='#ddd';g.fillRect(x,y,260,510);g.fillStyle='#222';g.font='12px sans-serif';g.fillText(`${r.id} ${Math.round(r.seconds*1000)}ms`,x+2,y+16);
   for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+k*244,258,242);assert(im.width<=258&&im.height<=242);g.drawImage(im,x+Math.floor((258-im.width)/2),y+22+k*244);}
  }const file=`unique-${scale.toFixed(2)}-${from}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,records:items});}}
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_ONLY_VISUAL_REVIEW_PENDING',sources,runtimeCurveFileSha256:sha(path.join(dir,'runtime-curves.json')),comparisons,frames,animations,sheets,endpoints,unique:unique.size,scope:'Neutral full/partial release and extreme contact restore; no grab inertia/anchor integration'},null,2));
 console.log(JSON.stringify({comparisons,frames:frames.length,unique:unique.size,sheets:sheets.map(s=>s.file)}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

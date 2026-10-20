'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs'),{create,state,transform,Swing,dir}=require('./legacy-parity.cjs'),{apng}=require('./grab-tilt-audit.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
function panel(im,label){const c=api.createCanvas(800,430),g=c.getContext('2d');g.imageSmoothingEnabled=false;for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(k*400,0,400,430);g.fillStyle=k?'white':'#222';g.font='13px sans-serif';g.fillText(label,k*400+5,18);g.drawImage(im,k*400+Math.floor((400-im.width)/2),30);}return c;}
async function main(){const m=await create(),probe=JSON.parse(fs.readFileSync(path.join(dir,'legacy/probe.json'))),prior=JSON.parse(fs.readFileSync(path.join(root,'artifacts/restored-landing-20261017-r47/manifest.json'))),sources={...prior.sources};
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 for(const f of ['tools/HighResolutionArt/legacy-parity.cjs','tools/HighResolutionArt/legacy-parity-review.cjs','tools/HighResolutionArt/LegacyParityProbe/Program.cs','src/Dororong.App/Interaction/HeadSwingSession.cs','src/Dororong.App/Controls/HeadPullAnchoring.cs','src/Dororong.App/Controls/ExtremeLandingPresentation.cs'])sources[f]=sha(path.join(root,f));
 let curveChecks=0;for(const trace of probe.swings){const sim=new Swing();sim.step(0,400);for(const f of trace.frames){assert(Math.abs(sim.step(f.dt,f.x)-f.angle)<1e-10);curveChecks++;}for(const f of trace.release){assert(Math.abs(state(f.ms/1000,trace.frames.at(-1).angle,[48,48]).angle-f.angle)<1e-10);curveChecks++;}}
 const frames=[],animations=[],comparisons=[],anchors=[],unique=new Map();
 async function save(file,im,meta){fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));const saved=await api.loadImage(path.join(dir,file));assert.deepEqual(pixels(saved,api),pixels(im,api));const d=pixels(saved,api);for(let n=0;n<im.width;n++)for(const [x,y]of [[n,0],[n,im.height-1],[0,n],[im.width-1,n]])assert.equal(d[(y*im.width+x)*4+3],0,'viewport clipping '+file);
  const r={file,...meta,sha256:sha(path.join(dir,file))};frames.push(r);if(!unique.has(r.sha256))unique.set(r.sha256,{...r,aliases:[]});unique.get(r.sha256).aliases.push(file);return saved;}
 for(const scale of [1,1.25,1.5]){
  for(const row of probe.landing){const file=`hard-${scale.toFixed(2)}-${row.ms}.png`,im=await save(file,m.hard(row.ms/1000,scale),{scale,kind:'hard',ms:row.ms});
   const legacyFile=`legacy/legacy-${row.ms}-${scale.toFixed(2)}.png`,old=await api.loadImage(path.join(dir,legacyFile));assert.equal(old.width,im.width);
   const c=api.createCanvas(800,860),g=c.getContext('2d');g.drawImage(panel(old,`OLD WPF ${scale}x ${row.ms}ms`),0,0);g.drawImage(panel(im,`NEW ${scale}x ${row.ms}ms`),0,430);const comparison=`compare-${scale.toFixed(2)}-${row.ms}.png`;fs.writeFileSync(path.join(dir,comparison),c.toBuffer('image/png'));comparisons.push({file:comparison,legacyFile,current:file,scale,ms:row.ms});
  }
  for(const [index,g]of [[25.25,37.5],[37.5,28.25],[50.25,48.5]].entries())for(const mirror of [false,true])for(const angle of [-88,-40,0,40,88]){
   assert(m.opaqueAnchor(g),'transparent anchor');const s={progress:1,angle,anchor:g,mirror,remaining:1},t=transform(s),actual=t.forward(...g);assert(Math.hypot(actual[0]-t.target[0],actual[1]-t.target[1])<1e-10);
   const file=`anchor-${scale.toFixed(2)}-${index}-${mirror?'L':'R'}-${angle}.png`;await save(file,m.render(s,scale),{scale,kind:'anchor',index,mirror,angle,anchor:g});anchors.push({file,error:Math.hypot(actual[0]-t.target[0],actual[1]-t.target[1])});
  }
  for(const trace of probe.swings){const g=trace.direction===-1?[25.25,37.5]:[50.25,48.5],mirror=trace.direction===1,id=`swing-${scale.toFixed(2)}-${trace.direction}`,packed=[],sequence=[];
   for(const f of trace.frames.filter(f=>f.i%2===0)){const s={progress:1,angle:f.angle,anchor:g,mirror,remaining:1},file=`${id}-hold-${f.i}.png`,label=`${id} hold ${f.ms}ms ${f.angle.toFixed(1)}deg`;const im=await save(file,m.render(s,scale),{scale,kind:'hold',id,ms:f.ms,label,state:s});packed.push(panel(im,label).toBuffer('image/png'));sequence.push(file);}
   for(const f of trace.release.filter(f=>f.ms%20===0)){const s=state(f.ms/1000,trace.frames.at(-1).angle,g,mirror),file=`${id}-release-${f.ms}.png`,label=`${id} release ${f.ms}ms`;const im=await save(file,m.render(s,scale),{scale,kind:'release',id,ms:f.ms,label,state:s});packed.push(panel(im,label).toBuffer('image/png'));sequence.push(file);}
   const file=`preview-${id}.png`;fs.writeFileSync(path.join(dir,file),apng([...packed,...Array(24).fill(packed.at(-1))]));animations.push({file,id,sequence,frames:packed.length+24,sha256:sha(path.join(dir,file)),timing:'hold32ms samples at30fps; release20ms samples at30fps; not normal-speed whole sequence'});
  }console.log('saved '+scale);
 }
 const sheets=[];for(const scale of [1,1.25,1.5]){const records=[...unique.values()].filter(r=>r.scale===scale);for(let start=0;start<records.length;start+=12){const rows=records.slice(start,start+12),c=api.createCanvas(1600,Math.ceil(rows.length/4)*830),g=c.getContext('2d');for(let j=0;j<rows.length;j++){const r=rows[j],im=await api.loadImage(path.join(dir,r.file)),x=j%4*400,y=Math.floor(j/4)*830;g.fillStyle='#ddd';g.fillRect(x,y,400,830);g.fillStyle='#222';g.font='11px sans-serif';g.fillText(r.file,x+3,y+16);for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+k*404,398,402);g.drawImage(im,x+Math.floor((398-im.width)/2),y+22+k*404);}}
   const file=`sheet-${scale.toFixed(2)}-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,records:rows});}}
 for(const [f,h]of Object.entries(sources))assert.equal(sha(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_ONLY_AWAITING_DIRECT_REVIEW',sources,curveChecks,probeSha256:sha(path.join(dir,'legacy/probe.json')),frames,animations,comparisons,anchors,sheets,unique:unique.size,contact:m.contact(1),runtimeIntegrated:false},null,2));console.log(JSON.stringify({curveChecks,frames:frames.length,unique:unique.size,sheets:sheets.length}));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});module.exports={panel};

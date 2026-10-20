'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const {create,fallTrace,shortDrop,Lag,field,dir}=require('./height-lag-r50.cjs');
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const xy=p=>[p.X,p.Y],close=(a,b)=>assert(Math.abs(a-b)<1e-8,`${a} != ${b}`);
async function main(){const probe=JSON.parse(fs.readFileSync(path.join(dir,'legacy/probe.json'))),previous=JSON.parse(fs.readFileSync(path.join(root,'artifacts/landing-recovery-20261018-r49/manifest.json'))),sources={...previous.sources};for(const [f,h]of Object.entries(sources))assert.equal(hash(path.join(root,f)),h);
 for(const f of ['tools/HighResolutionArt/height-lag-r50.cjs','tools/HighResolutionArt/r50-review.cjs','tools/HighResolutionArt/HeightLagProbe/Program.cs','src/Dororong.Core/Platforms/PlatformMotion.cs','src/Dororong.App/Controls/HeadSurroundingPresentation.cs','src/Dororong.App/Controls/SurroundingPullRenderer.cs','src/Dororong.App/Interaction/SurroundingPullMotion.cs'])sources[f]=hash(path.join(root,f));
 let physics=0,lagChecks=0,fieldChecks=0;for(const f of probe.falls){const rows=fallTrace(f.height);assert.equal(rows.length,f.rows.length);for(let i=0;i<rows.length;i++){const a=rows[i],b=f.rows[i];assert.equal(a.phase,b.phase);assert.equal(a.extreme,b.extreme);for(const k of ['y','squash','spread','age'])close(a[k],b[k]);physics++;}}
 for(const r of probe.shortDrops){const s=shortDrop(r.ms,r.distance);close(s.y,r.y);close(s.squash,r.squash);}
 const lag=new Lag();for(const r of probe.lag){if(r.ms>=780){lag.near=[0,0];lag.far=[0,0];}else lag.step(xy(r.pointer),r.ms?20:0,r.settling);for(let i=0;i<2;i++){close(lag.near[i],xy(r.near)[i]);close(lag.far[i],xy(r.far)[i]);}lagChecks++;for(const q of r.fieldChecks){const d=field(xy(q.p),xy(r.center),xy(r.grab),xy(r.localNear),xy(r.localFar));for(let i=0;i<2;i++)close(d[i],xy(q.d)[i]);fieldChecks++;}}
 const model=await create(),records=[],sheets=[];
 for(const scale of [1,1.25,1.5]){const sk=scale.toFixed(2),items=[];
  for(const f of probe.falls)for(const r of f.rows.filter(r=>r.selected)){const id=`fall-${f.height}-${r.ms}`,im=model.platform(r,scale);items.push({id,im,kind:'fall',height:f.height,...r});}
  for(const r of probe.lag.filter(r=>r.selected))items.push({id:`lag-${r.ms}`,kind:'lag',...r,im:model.lag(r,scale)});
  for(const item of items){const {im,...meta}=item,file=`${item.id}-${sk}.png`,old=`legacy/${file}`;fs.writeFileSync(path.join(dir,file),im.toBuffer('image/png'));const saved=await api.loadImage(path.join(dir,file));assert.deepEqual(pixels(im,api),pixels(saved,api));const p=pixels(saved,api);for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(p[(y*im.width+x)*4+3])assert(x>0&&y>0&&x<im.width-1&&y<im.height-1,'clipped');records.push({file,old,scale,...meta,sha256:hash(path.join(dir,file)),oldSha256:hash(path.join(dir,old))});}
  // All selected transition frames: old/new same size on both backgrounds.
  const list=[...new Map(records.filter(r=>r.scale===scale).map(r=>[r.sha256+r.oldSha256,r])).values()];for(let start=0;start<list.length;start+=6){const rows=list.slice(start,start+6),c=api.createCanvas(1120,Math.ceil(rows.length/2)*470),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let j=0;j<rows.length;j++){
    const r=rows[j],x=j%2*560,y=Math.floor(j/2)*470,images=await Promise.all([r.old,r.file].map(f=>api.loadImage(path.join(dir,f))));
    let left=Infinity,top=Infinity,right=-1,bottom=-1;
    for(const im of images){const px=pixels(im,api);for(let yy=0;yy<im.height;yy++)for(let xx=0;xx<im.width;xx++)if(px[(yy*im.width+xx)*4+3]){left=Math.min(left,xx);top=Math.min(top,yy);right=Math.max(right,xx);bottom=Math.max(bottom,yy);}}
    left-=2;top-=2;const w=right-left+3,h=bottom-top+3;assert(w<=278&&h<=222,'review cell too small');
    g.fillStyle='#ddd';g.fillRect(x,y,560,470);g.fillStyle='#222';g.font='12px sans-serif';g.fillText(`${r.id} ${scale}x OLD | NEW`,x+4,y+15);
    for(const [k,bg]of ['white','#252a31'].entries())for(const [n,im]of images.entries()){g.fillStyle=bg;g.fillRect(x+n*280,y+22+k*224,278,222);g.drawImage(im,left,top,w,h,x+n*280+(278-w)/2,y+22+k*224+(222-h)/2,w,h);}
   }
   const file=`compare-${sk}-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,records:rows.map(r=>r.file)});
  }console.log('Saved scale '+scale+' frames '+items.length);
 }
 for(const [f,h]of Object.entries(sources))assert.equal(hash(path.join(root,f)),h);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_ONLY_VISUAL_PENDING',sources,physics,lagChecks,fieldChecks,shortDropChecks:probe.shortDrops.length,records,sheets,probeSha256:hash(path.join(dir,'legacy/probe.json')),originalArtChanged:false,runtimeIntegrated:false,camera:'following host; height travel recorded separately'},null,2));console.log(JSON.stringify({physics,lagChecks,fieldChecks,frames:records.length,sheets:sheets.length}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

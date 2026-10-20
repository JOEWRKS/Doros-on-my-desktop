'use strict';
// Raster ownership diagnosis, NOT animation-ready layers. A hard partition is
// deliberately used to preserve every source texel and expose missing underlay.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs');
const {create,diff}=require('./grab-entry-transport.cjs');
const {outline,forepaws}=require('./grab-contour-still.cjs');
const dir=path.join(root,'artifacts/grab-entry-transport-20261017-r39');
function pathOf(rows){const p=new api.Path2D();for(const [op,...v]of rows){if(op==='M')p.moveTo(...v);else if(op==='C')p.bezierCurveTo(...v);else if(op==='L')p.lineTo(...v);else p.closePath();}return p;}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create();
 const meta=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-contour-still-20261017-r33/measurement.json'),'utf8'));
 const tr=meta.sourceToLogical;
 const front=pathOf([...outline.slice(0,3),...forepaws.slice(1),['L',60,35],['L',20,35],['Z']]);
 const check=api.createCanvas(1,1).getContext('2d'),report=[];
 for(const [n,im]of [m.body0,m.body1].entries()){
  const a=pixels(im,api),w=im.width,h=im.height,c=api.createCanvas(w,h),g=c.getContext('2d');
  const layers=[api.createCanvas(w,h),api.createCanvas(w,h)],ds=layers.map(l=>l.getContext('2d').createImageData(w,h));
  let frontTexels=0,rearTexels=0;
  for(let y=0;y<h;y++)for(let x=0;x<w;x++){
   const lx=(x+.5)*96/225-(n?32:0),ly=(y+.5)*96/225-(n?32:0);
   // Idle partition runs through uninked chest/rump skin. Held partition uses
   // the historical anatomical forepaw curve, without repainting its edge.
   const isFront=n?check.isPointInPath(front,(lx-tr.dx)/tr.factor,(ly-tr.dy)/tr.factor):lx<56;
   const i=(y*w+x)*4;ds[isFront?0:1].data.set(a.subarray(i,i+4),i);
   if(a[i+3])isFront?frontTexels++:rearTexels++;
  }
  layers.forEach((l,i)=>l.getContext('2d').putImageData(ds[i],0,0));
  for(const l of layers)g.drawImage(l,0,0);
  assert.equal(diff(im,c).changedPixels,0,'unmoved partition must exactly reconstruct source');
  for(let j=0;j<2;j++)fs.writeFileSync(path.join(dir,`ownership-${n}-${j?'rear':'front'}.png`),layers[j].toBuffer('image/png'));
  // Equal-size enlarged crops show the new internal cut, not only exterior.
  const sheet=api.createCanvas(1200,620),sg=sheet.getContext('2d');sg.imageSmoothingEnabled=false;
  for(const [j,part]of [im,...layers,c].entries())for(const [k,bg]of ['white','#252a31'].entries()){
   const x=j*300,y=k*310;sg.fillStyle=bg;sg.fillRect(x,y,300,310);sg.fillStyle=k?'white':'#222';sg.font='14px sans-serif';sg.fillText(['Unchanged body','Front ownership only','Rear ownership only','Recombined: exact'][j],x+7,y+20);
   const ox=(n?32:0)*225/96,oy=ox;sg.drawImage(part,ox+12*225/96,oy+42*225/96,70*225/96,64*225/96,x+10,y+35,280,256);
  }
  fs.writeFileSync(path.join(dir,`ownership-${n}.png`),sheet.toBuffer('image/png'));
  report.push({endpoint:n,frontTexels,rearTexels,recombinedChangedPixels:diff(im,c).changedPixels});
 }
 fs.writeFileSync(path.join(dir,'ownership-report.json'),JSON.stringify({status:'DIAGNOSTIC_PARTITION_ONLY',checks:report,limitations:['No hidden skin has been fabricated. Moving these cut layers apart exposes their internal cut.','Exact unmoved reconstruction does not prove animated overlap or antialiasing continuity.','No product file or source artwork edited.']},null,2));
 console.log(JSON.stringify(report));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

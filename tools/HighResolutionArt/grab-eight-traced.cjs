'use strict';
// Source-authored body curves, not r40's normal-search/smoothing result.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {premultiply,sample,over,surface}=require('./sampling.cjs');
const dir=path.join(root,'artifacts/grab-eight-traced-20261017-r41');
const lock=path.join(root,'artifacts/grab-contour-still-20261017-r33');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
// Source100 coordinates. Each row is directly inspectable against the original
// PNG. Hidden closure curves are never stroked. Short saddles are not rounded
// into deep notches by a nearest-ink search.
const fronts=[null,
 [['M',14,70],['C',14.5,80,14.7,87,18,89.5],['C',20,91,23,90.5,24.5,87.5],['C',25.5,85.6,25.6,84,26,83],['C',27.5,83.1,29,83.8,30.5,84],['C',31.5,90,33,94,38,94.5],['C',42,94.6,45,94,45,89],['C',45,87,45,85,45,83.5]],
 [['M',15,66],['C',15.2,75,15.5,81,18,84],['C',20,87,22.5,87,25,85.4],['C',28,83.6,29.5,81.3,30,78.2],['C',31.3,78,32.4,78,33.7,78.1],['C',34,84,36,88.5,40,89.7],['C',43.5,91,46,86.5,47,83.5],['C',47.7,81.4,48,79.6,48.5,78]],
 [['M',16,62],['C',16.3,70,16.4,76,18.5,79.5],['C',20,82.5,23,83.6,25.6,82.6],['C',28.5,81.5,28.7,78.4,29,75.2],['C',31,76,32.6,77,34.2,77.6],['C',34.4,81.6,36.3,83.5,39,83],['C',42.3,82.9,44.9,79.5,46.1,76.5],['C',47,74.4,47.1,72.7,47.3,71]],
 [['M',18,59],['C',18.6,67,19,72,22,75],['C',24,77.5,27.3,79.2,30,78],['C',32.9,76.7,33.7,73.5,34,69],['C',34.4,71.3,34.8,73.3,36,74.8],['C',37.7,77.2,41.6,79.2,44.2,78.5],['C',47.1,77.6,49.2,73.7,50.5,70.4],['C',51.4,68.3,52.3,66.7,53,65]],
 [['M',22,57],['C',22.2,65,23,72,27,74.5],['C',29,76.4,32,76.2,34,74.5],['C',36,72.8,37.3,69.5,37.5,66],['C',37.9,69,38.4,71.3,39.7,73.1],['C',41.8,76,45.3,77.2,48.3,75.9],['C',51.1,74.6,52.7,71.6,53.8,68.2],['C',54.3,66.7,54.8,65.3,55.5,64]],
 [['M',25,59],['C',25.2,66,25.2,72,29,74.3],['C',31,76.2,34,75.8,36,74.2],['C',38,72.6,39.3,69.9,39.5,67],['C',39.8,69.7,40.3,72.2,41.7,73.9],['C',43.6,76.3,47,77.2,49.3,75.6],['C',51.7,74,52.8,71.7,53.6,69],['C',54,67.7,54.3,66.7,54.5,66]],null];
const rears=[null,
 [['M',37,74],['C',36,82,35,91,39,94],['C',42,95,46,95,48.4,92.8],['C',50.6,90.5,50.7,87.1,51.3,85.2],['C',52.4,85.2,54,85,55.1,85],['C',55.2,89.3,59.4,95,64,95.5],['C',69,96.5,70,92,70,88],['C',70,84,72.3,81,73.2,77],['C',75,70,70,64,65,61],['C',63,58,62,56,60,54]],
 [['M',38.5,85],['C',39,89,40,92.8,43,94],['C',47,96,50.5,93.6,51.3,90],['C',52,88.5,51.9,86.5,52.1,85.1],['C',53.3,85.1,54.4,85.3,55.5,85.5],['C',55.7,90.9,60,94.5,65,95],['C',70,96,71,92.5,71,88],['C',71,84,73.1,81,74,77],['C',76.7,68,69,61,65,59],['C',63,58,62,57,61,56]],
 [['M',32.1,78.5],['C',33,85,36,92,41,94.6],['C',45.8,97.1,49.7,93.2,50.9,89.7],['C',51.5,88.4,51.7,86.4,51.8,84.5],['C',53.5,84.5,54.7,84.6,56,84.8],['C',56.5,90.5,61,95.4,66,96],['C',71,96,73,92,73,87],['C',74,83,74,78,73,73],['C',72,66,67,60,64,56],['C',63,54,62,52,61,50]],
 [['M',31.5,77],['C',33,83,36,91,42,94.5],['C',46.5,97,50,93.9,51.2,89.5],['C',51.8,87.8,51.6,85.5,51.1,83.9],['C',52.6,84,54,84.3,55,84.8],['C',55.8,90.1,59.7,95.6,65,96],['C',71,96,73,91,73,86],['C',74,82,74,77,73,72],['C',71,66,66,60,63,57],['C',62,54,61,52,60,50]],
 [['M',33,75.5],['C',35,81,35.5,91,40,94.5],['C',45,97.8,49.2,93,51.5,88.6],['C',52,87,52.6,85.3,53.2,84.8],['C',54,84.6,54.9,84.7,55.5,85.2],['C',56.8,90,60.5,95.9,65,96],['C',71,96,73,90,73,85],['C',74,81,73,76,72,72],['C',70,64,68,60,64,57],['C',63,54,62,52,61,50]],
 [['M',33,75.4],['C',34.3,81,35,92,39.5,95],['C',45,98.4,49,93,51.2,88.8],['C',52.1,87,53,85.1,54,84.6],['C',54.8,84.2,55.6,84.5,56,85.5],['C',57.1,90.9,60,96,65,96],['C',71,96,72,92,72,87],['C',73,82,73,77,71.5,72],['C',70,65,68,60,66,55],['C',65,52,64,49,63,47]],null];
const folds=[null,[['M',45,84],['C',48,84.2,52,84.4,56.3,84.3]],[['M',47,82.1],['C',49,84.2,52,85.2,55.8,85.5]],[['M',45.3,82.3],['C',48,84.4,51.2,84.5,55.9,84.8]],[['M',48.4,83.4],['C',50.5,83.8,53.2,84.1,55,84.8]],null,null,null];
function pathOf(rows){const p=new api.Path2D();for(const [op,...v] of rows){if(op==='M')p.moveTo(...v);else if(op==='C')p.bezierCurveTo(...v);else if(op==='L')p.lineTo(...v);else p.closePath();}return p;}
const mapRows=(rows,tr)=>rows.map(([op,...v])=>[op,...v.map((v,k)=>v*tr.factor+(k%2?tr.dy:tr.dx))]);
async function create(){
 const old=JSON.parse(fs.readFileSync(path.join(root,'artifacts/grab-entry-20261017-r38/key-data.json'),'utf8'));
 const meta=JSON.parse(fs.readFileSync(path.join(lock,'measurement.json'),'utf8'));
 const head=await api.loadImage(path.join(lock,'masked-head-225.png')),hp=premultiply(head,api),sources=[];
 const keys=old.keys.map((key,n)=>fronts[n]?{front:mapRows(fronts[n],old.registration[n]),rear:mapRows(rears[n],old.registration[n])}:key);
 for(let n=0;n<8;n++)sources.push(await api.loadImage(path.join(root,`src/Dororong.App/Assets/user-body-drag/0${n+1}.png`)));
 function body(n){
  const {front,rear}=keys[n],close=p=>pathOf([...p,['L',43,43],['L',20,43],['Z']]);
  const skin=close(front).op(close(rear),api.PathOp.Union),c=api.createCanvas(375,375),g=c.getContext('2d');
  g.scale(225/96,225/96);g.translate(32,32);g.fillStyle='white';g.fill(skin);
  const ink=api.createCanvas(375,375),ig=ink.getContext('2d');ig.scale(225/96,225/96);ig.translate(32,32);ig.lineWidth=meta.pen.width;ig.lineCap=ig.lineJoin='round';ig.strokeStyle='white';ig.stroke(skin);ig.globalCompositeOperation='lighten';
  ig.stroke(pathOf(front));
  // The original has a short fold above the rear paw in02–05. A union-only
  // outline loses this internal line and falsely rounds the whole junction.
  if(folds[n])ig.stroke(pathOf(mapRows(folds[n],old.registration[n])));
  if(n===3)ig.stroke(pathOf(mapRows([['M',34,75],['C',34,76.2,34.1,77,34.2,77.6]],old.registration[n])));
  ig.resetTransform();ig.globalCompositeOperation='source-in';ig.fillStyle=`rgb(${meta.pen.rgb.join(',')})`;ig.fillRect(0,0,375,375);g.resetTransform();g.drawImage(ink,0,0);return c;
 }
 function render(n,scale=1.5){const bd=premultiply(body(n),api),a=new Float64Array(4),b=new Float64Array(4);return surface(api,96*scale,(x,y)=>{sample(bd,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);},32);}
 return {render,body,keys,sources,registration:old.registration,head};
}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const m=await create(),images=[];
 for(let n=0;n<8;n++){const file=path.join(dir,`key-${n}.png`);fs.writeFileSync(file,m.render(n).toBuffer('image/png'));images.push(await api.loadImage(file));}
 for(let n=0;n<8;n++){
  const c=api.createCanvas(960,740),g=c.getContext('2d'),tr=m.registration[n];g.fillStyle='white';g.fillRect(0,0,960,740);g.imageSmoothingEnabled=false;g.font='18px sans-serif';g.fillStyle='#222';g.fillText(`Original 0${n+1} (left) / traced body + locked head (right)`,20,28);
  // Identical registration and view transform for the original and candidate.
  const s=4;g.drawImage(m.sources[n],20+(32+tr.dx)*s,-65+(32+tr.dy)*s,100*tr.factor*s,100*tr.factor*s);g.drawImage(images[n],500,-65,640,640);
  g.fillStyle='#252a31';g.fillRect(0,480,960,260);g.drawImage(images[n],510,475);g.fillStyle='white';g.fillText('Actual 1.5x saved pixels (right)',20,510);
  g.fillStyle='white';g.fillRect(0,0,960,40);g.fillStyle='#222';g.fillText(`Original 0${n+1} (left) / traced body + locked head (right)`,20,28);
  fs.writeFileSync(path.join(dir,`pair-${n+1}.png`),c.toBuffer('image/png'));
  const study=api.createCanvas(800,800),sg=study.getContext('2d');sg.imageSmoothingEnabled=false;sg.drawImage(m.sources[n],0,0,800,800);sg.scale(8,8);sg.strokeStyle='#00aaff';sg.lineWidth=.16;
  for(const p of [m.keys[n].front,m.keys[n].rear])sg.stroke(pathOf(p.map(([op,...v])=>[op,...v.map((v,k)=>(v-(k%2?tr.dy:tr.dx))/tr.factor)])));
  fs.writeFileSync(path.join(dir,`overlay-${n+1}.png`),study.toBuffer('image/png'));
 }
 fs.writeFileSync(path.join(dir,'keys.json'),JSON.stringify({keys:m.keys,registration:m.registration,sourceHashes:Object.fromEntries(m.sources.map((_,n)=>[`0${n+1}`,sha(path.join(root,`src/Dororong.App/Assets/user-body-drag/0${n+1}.png`))])),headSha:sha(path.join(lock,'masked-head-225.png')),codeSha:sha(__filename),state:'AWAITING_DIRECT_REVIEW',connected:false},null,2));
 console.log(dir);
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={create,dir,pathOf,fronts,rears,folds,mapRows};

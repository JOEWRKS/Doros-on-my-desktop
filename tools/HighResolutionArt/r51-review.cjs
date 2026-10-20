'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs');
const dir=path.join(root,'artifacts/runtime-grab-20261018-r51'),hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){const bank=JSON.parse(fs.readFileSync(path.join(dir,'bank-manifest.json')));for(const [p,h]of Object.entries(bank.sources))assert.equal(hash(path.join(root,p)),h,p);assert.equal(hash(bank.file),bank.sha256);
 const sheets=[],records=[];let identical=0;
 for(const s of ['1.00','1.25','1.50']){
  const run=JSON.parse(fs.readFileSync(path.join(dir,`review/runtime-${s}.json`)));
  for(const r of run.rows){const file='review/'+r.file,h=hash(path.join(dir,file));const previous=path.join(dir,'parallel',r.file);if(fs.existsSync(previous)){assert.equal(hash(previous),h,'Optimization changed pixels: '+r.file);identical++;}records.push({...r,file,scale:run.scale,sha256:h});}
  const unique=[...new Map(records.filter(r=>r.scale===run.scale).map(r=>[r.sha256,r])).values()];
  for(let start=0;start<unique.length;start+=8){const rows=unique.slice(start,start+8),canvas=api.createCanvas(960,Math.ceil(rows.length/4)*440),g=canvas.getContext('2d');g.imageSmoothingEnabled=false;
   for(let i=0;i<rows.length;i++){const r=rows[i],im=await api.loadImage(path.join(dir,r.file)),d=pixels(im,api);let l=im.width,t=im.height,right=-1,b=-1;
    for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(d[(y*im.width+x)*4+3]){l=Math.min(l,x);t=Math.min(t,y);right=Math.max(right,x);b=Math.max(b,y);assert(x>0&&y>0&&x<im.width-1&&y<im.height-1,'canvas clip');}
    const w=right-l+5,h=b-t+5;assert(w<238&&h<210);const x=i%4*240,y=Math.floor(i/4)*440;g.fillStyle='#dedbd6';g.fillRect(x,y,240,440);g.fillStyle='#222';g.font='12px sans-serif';g.fillText(r.file.split('/').at(-1).replace('runtime-',''),x+4,y+15);
    for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+20+j*210,238,208);g.drawImage(im,l-2,t-2,w,h,x+(238-w)/2,y+20+j*210+(208-h)/2,w,h);}
   }const file=`sheet-${s}-${start}.png`;fs.writeFileSync(path.join(dir,file),canvas.toBuffer('image/png'));sheets.push({file,records:rows.map(r=>r.file)});}
 }
 const code=['src/Dororong.App/Controls/HighResolutionGrabRaster.cs','src/Dororong.App/Controls/DororongPresenter.HighResolutionGrab.cs','src/Dororong.App/Controls/DororongPresenter.xaml.cs','src/Dororong.App/Controls/HeadSurroundingPresentation.cs','src/Dororong.App/MainWindow.xaml.cs','tests/Dororong.App.Tests/Runtime/HighResolutionGrabTests.cs','tests/Dororong.App.Tests/Runtime/PetLoopPlatformTests.cs'];
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_ONLY_VISUAL_PENDING',records,sheets,optimizationIdentical:identical,sources:bank.sources,code:Object.fromEntries(code.map(p=>[p,hash(path.join(root,p))])),bank:bank.sha256},null,2));
 console.log(JSON.stringify({frames:records.length,sheets:sheets.length,unique:sheets.reduce((n,s)=>n+s.records.length,0),optimizationIdentical:identical}));
}main().catch(e=>{console.error(e);process.exitCode=1;});

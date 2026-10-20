'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const dir=path.join(root,'artifacts/runtime-face-20261018-r53'),source=process.argv[2]||'v2';
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const bank=JSON.parse(fs.readFileSync(path.join(root,'artifacts/runtime-grab-20261018-r51/bank-manifest.json')));
 for(const [f,h]of Object.entries(bank.sources))assert.equal(hash(path.join(root,f)),h,f);
 assert.equal(hash(bank.file),bank.sha256);
 const prior=source==='v3'?JSON.parse(fs.readFileSync(path.join(dir,'manifest-v2.json'))):null;
 const reviewed=new Set(prior?.records.map(r=>r.sha256)||[]);
 const sheets=[],records=[];
 for(const scale of ['1.00','1.10','1.25','1.35','1.50']){
  const rows=JSON.parse(fs.readFileSync(path.join(dir,source,`face-${scale}.json`))).map(r=>({...r,file:source+'/'+r.file,sha256:hash(path.join(dir,source,r.file))}));records.push(...rows);
  const unique=[...new Map(rows.filter(r=>!reviewed.has(r.sha256)).map(r=>[r.sha256,r])).values()];
  for(let start=0;start<unique.length;start+=12){
   const batch=unique.slice(start,start+12),canvas=api.createCanvas(960,Math.ceil(batch.length/4)*340),g=canvas.getContext('2d');g.imageSmoothingEnabled=false;
   for(const [i,r]of batch.entries()){
    const im=await api.loadImage(path.join(dir,r.file)),c=api.createCanvas(im.width,im.height),ctx=c.getContext('2d');ctx.drawImage(im,0,0);const d=ctx.getImageData(0,0,im.width,im.height).data;
    let l=im.width,t=im.height,rr=-1,b=-1;for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(d[(y*im.width+x)*4+3]){l=Math.min(l,x);t=Math.min(t,y);rr=Math.max(rr,x);b=Math.max(b,y);}
    const w=rr-l+5,h=b-t+5,x=i%4*240,y=Math.floor(i/4)*340;assert(w<238&&h<157);
    g.fillStyle='#ddd';g.fillRect(x,y,240,340);g.fillStyle='#222';g.font='11px sans-serif';g.fillText(path.basename(r.file),x+2,y+14);
    for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+20+j*160,238,158);g.drawImage(im,l-2,t-2,w,h,x+(238-w)/2,y+20+j*160+(158-h)/2,w,h);}
   }
   const file=`sheet-${source}-${scale}-${start}.png`;fs.writeFileSync(path.join(dir,file),canvas.toBuffer('image/png'));sheets.push({file,records:batch.map(r=>r.file)});
  }
 }
 fs.writeFileSync(path.join(dir,`manifest-${source}.json`),JSON.stringify({status:'TECHNICAL_ONLY_VISUAL_PENDING',prior:prior?'manifest-v2.json':null,priorIdenticalSlots:records.filter(r=>reviewed.has(r.sha256)).length,originals:bank.sources,grabBank:bank.sha256,bodyBank:hash(path.join(root,'src/Dororong.App/Assets/high-resolution-hunt-body.bank')),records,sheets},null,2));
 console.log(JSON.stringify({slots:records.length,unique:sheets.reduce((n,s)=>n+s.records.length,0),sheets:sheets.map(s=>s.file)}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

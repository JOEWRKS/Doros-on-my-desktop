'use strict';
const fs=require('fs'),path=require('path'),crypto=require('crypto'),assert=require('assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const dir=path.join(root,'artifacts/runtime-grab-20261018-r52');
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){
 const original=JSON.parse(fs.readFileSync(path.join(root,'artifacts/runtime-grab-20261018-r51/bank-manifest.json')));
 for(const [f,h]of Object.entries(original.sources))assert.equal(hash(path.join(root,f)),h,f);
 assert.equal(hash(original.file),original.sha256);
 const records=[],sheets=[];const final='final-v4';const host='host-final-v4';
 for(const scale of ['1.00','1.10','1.25','1.35','1.50']){
  const rows=JSON.parse(fs.readFileSync(path.join(dir,`${host}/host-${scale}.json`)));
  for(const row of rows){assert.equal(hash(path.join(dir,host,row.file)),hash(path.join(dir,'host-v3',row.file)),row.file);records.push({...row,file:host+'/'+row.file,sha256:hash(path.join(dir,host,row.file))});}
  const unique=[...new Map(records.filter(r=>r.scale===Number(scale)).map(r=>[r.sha256,r])).values()];
  for(let start=0;start<unique.length;start+=8){
   const batch=unique.slice(start,start+8),canvas=api.createCanvas(1040,Math.ceil(batch.length/4)*480),g=canvas.getContext('2d');g.imageSmoothingEnabled=false;
   for(let i=0;i<batch.length;i++){
    const r=batch[i],im=await api.loadImage(path.join(dir,r.file)),c=api.createCanvas(im.width,im.height),ctx=c.getContext('2d');ctx.drawImage(im,0,0);const d=ctx.getImageData(0,0,im.width,im.height).data;
    let l=im.width,t=im.height,right=-1,bottom=-1;
    for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(d[(y*im.width+x)*4+3]){l=Math.min(l,x);t=Math.min(t,y);right=Math.max(right,x);bottom=Math.max(bottom,y);assert(x>0&&y>0&&x<im.width-1&&y<im.height-1);}
    const w=right-l+5,h=bottom-t+5;assert(w<=258&&h<=225);
    const x=i%4*260,y=Math.floor(i/4)*480;g.fillStyle='#dedbd6';g.fillRect(x,y,260,480);g.fillStyle='#222';g.font='11px sans-serif';g.fillText(path.basename(r.file),x+3,y+14);
    for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+20+j*230,258,228);g.drawImage(im,l-2,t-2,w,h,x+(258-w)/2,y+20+j*230+(228-h)/2,w,h);}
   }
   const file=`host-sheet-${scale}-${start}.png`;fs.writeFileSync(path.join(dir,file),canvas.toBuffer('image/png'));sheets.push({file,records:batch.map(r=>r.file)});
  }
 }
 for(const scale of ['1.00','1.10','1.25','1.35','1.50']){
  const source=path.join(dir,`${final}/runtime-${scale}.json`);if(!fs.existsSync(source))continue;
  const run=JSON.parse(fs.readFileSync(source));
  const changed=run.rows.filter(r=>{const old=path.join(root,'artifacts/runtime-grab-20261018-r51/fifo',r.file);return !fs.existsSync(old)||hash(old)!==hash(path.join(dir,final,r.file));});
  const unique=[...new Map(changed.map(r=>[hash(path.join(dir,final,r.file)),r])).values()];
  for(const r of run.rows)records.push({...r,scale:run.scale,file:final+'/'+r.file,sha256:hash(path.join(dir,final,r.file))});
  for(let start=0;start<unique.length;start+=8){
   const batch=unique.slice(start,start+8),canvas=api.createCanvas(1040,Math.ceil(batch.length/4)*480),g=canvas.getContext('2d');g.imageSmoothingEnabled=false;
   for(let i=0;i<batch.length;i++){
    const r=batch[i],im=await api.loadImage(path.join(dir,final,r.file)),c=api.createCanvas(im.width,im.height),ctx=c.getContext('2d');ctx.drawImage(im,0,0);const d=ctx.getImageData(0,0,im.width,im.height).data;
    let l=im.width,t=im.height,right=-1,bottom=-1;for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(d[(y*im.width+x)*4+3]){l=Math.min(l,x);t=Math.min(t,y);right=Math.max(right,x);bottom=Math.max(bottom,y);assert(x>0&&y>0&&x<im.width-1&&y<im.height-1);}
    const w=right-l+5,h=bottom-t+5,x=i%4*260,y=Math.floor(i/4)*480;assert(w<258&&h<225);g.fillStyle='#dedbd6';g.fillRect(x,y,260,480);g.fillStyle='#222';g.font='11px sans-serif';g.fillText(r.file,x+3,y+14);
    for(const [j,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+20+j*230,258,228);g.drawImage(im,l-2,t-2,w,h,x+(258-w)/2,y+20+j*230+(228-h)/2,w,h);}
   }
   const file=`loop-sheet-${scale}-${start}.png`;fs.writeFileSync(path.join(dir,file),canvas.toBuffer('image/png'));sheets.push({file,records:batch.map(r=>final+'/'+r.file)});
  }
 }
 const same=[],changed=[];for(const file of fs.readdirSync(path.join(dir,final)).filter(f=>f.endsWith('.png'))){const old=path.join(root,'artifacts/runtime-grab-20261018-r51/fifo',file);if(fs.existsSync(old)){if(hash(old)===hash(path.join(dir,final,file)))same.push(file);else changed.push(file);}}
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'VISUAL_PENDING',sourceHashes:original.sources,bank:original.sha256,records,sheets,identicalR51Frames:same.length,changedR51Frames:changed},null,2));
 const code=['src/Dororong.App/PetLoop.cs','src/Dororong.App/Controls/DororongPresenter.xaml.cs','src/Dororong.App/Controls/DororongPresenter.HighResolutionGrab.cs','src/Dororong.App/Controls/HighResolutionGrabRaster.cs','src/Dororong.App/MainWindow.xaml.cs','tests/Dororong.App.Tests/Runtime/HighResolutionGrabTests.cs','tests/Dororong.App.Tests/Controls/HighResolutionHostTests.cs'];
 const tests=['tests/all-app.trx','tests/final-targeted-v4.trx','tests/final-all-scales-v4.trx'];
 fs.writeFileSync(path.join(dir,'receipt.json'),JSON.stringify({code:Object.fromEntries(code.map(f=>[f,hash(path.join(root,f))])),tests:Object.fromEntries(tests.map(f=>[f,hash(path.join(dir,f))])),binary:Object.fromEntries(['Dororong.App.exe','Dororong.App.dll','Dororong.Core.dll'].map(f=>[f,hash(path.join(dir,'app-final',f))])),visualLedger:'docs/verification/2026-10-18-r52-runtime-grab.md',manifest:hash(path.join(dir,'manifest.json'))},null,2));
 fs.writeFileSync(path.join(dir,'index.html'),`<!doctype html><html lang="ko"><meta charset="utf-8"><title>r52 잡기·놓기 검수</title><style>body{font:16px/1.6 system-ui;max-width:1080px;margin:32px auto;background:#242930;color:#eee;padding:16px}img{max-width:100%;height:auto}a{color:#afd4ff}summary{cursor:pointer;padding:12px}button,select,input{font:inherit}#stage{height:440px;overflow:auto;background:white}#frame{max-width:none}label{margin-right:16px}</style><h1>잡기·놓기·동적 발판 — r52</h1><p>저장 표본 검수 페이지. 실제 마우스 조작·실시간 성능 통과를 의미하지 않음. 원화 미수정 / 설치본 미교체.</p><p>1 / 1.1 / 1.25 / 1.35 / 1.5배, 양방향 기울임·재잡기·낙하·일반/강한 착지. 아래 재생은 저장 표본 순서이며 실제 시간 간격이 아님.</p><label>크기 <select id="scale">${['1.00','1.10','1.25','1.35','1.50'].map(s=>`<option>${s}</option>`).join('')}</select></label><button id="play">표본 넘기기</button><button id="bg">배경 전환</button><input id="seek" type="range" min="0" max="95" value="0"><p id="name"></p><div id="stage"><img id="frame" alt="저장 런타임 프레임"></div><h2>직접 검수 대상 시트</h2>${sheets.map(s=>`<details><summary>${s.file}</summary><img loading="lazy" src="${s.file}" alt="${s.file}"></details>`).join('')}<script>const records=${JSON.stringify(records.filter(r=>r.file.startsWith(final+'/')))};let timer;const scale=document.querySelector('#scale'),seek=document.querySelector('#seek'),frame=document.querySelector('#frame');function show(){const rows=records.filter(r=>r.scale===Number(scale.value));const r=rows[Number(seek.value)];frame.src=r.file;document.querySelector('#name').textContent=r.file+' / '+r.phase;}scale.onchange=show;seek.oninput=show;document.querySelector('#play').onclick=()=>{if(timer){clearInterval(timer);timer=null;}else timer=setInterval(()=>{seek.value=(Number(seek.value)+1)%96;show();},150)};let dark=false;document.querySelector('#bg').onclick=()=>{dark=!dark;document.querySelector('#stage').style.background=dark?'#242930':'white';};show();</script></html>`);
 console.log(JSON.stringify({frames:records.length,distinct:sheets.reduce((n,s)=>n+s.records.length,0),sheets:sheets.map(s=>s.file),identicalR51Frames:same.length}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

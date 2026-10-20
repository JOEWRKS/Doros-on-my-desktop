'use strict';
const fs=require('node:fs'),path=require('node:path');
const {api}=require('./bangs-composite-review.cjs'),{dir}=require('./grab-boundary-connected.cjs');
async function main(){
 const m=await require('./grab-eight-traced.cjs').create(),c=api.createCanvas(1280,1360),g=c.getContext('2d');g.imageSmoothingEnabled=false;
 for(let n=0;n<8;n++){
  const x=n%2*640,y=Math.floor(n/2)*340,tr=m.registration[n],im=await api.loadImage(path.join(dir,`frame-1.50-${String(n*8).padStart(2,'0')}.png`));
  g.fillStyle=n%2?'#f4f1ed':'white';g.fillRect(x,y,640,340);
  g.drawImage(m.sources[n],x+(32+tr.dx)*2,y-10+(32+tr.dy)*2,200*tr.factor,200*tr.factor);
  g.drawImage(im,x+320,y-10,320,320);g.fillStyle='#222';g.font='18px sans-serif';g.fillText(`0${n+1}  Original`,x+16,y+26);g.fillText(n===0?'Existing idle body':n===7?'Accepted held body':'Corrected body',x+336,y+26);
  g.strokeStyle='#ccc';g.beginPath();g.moveTo(x,y+339);g.lineTo(x+640,y+339);g.stroke();
 }
 fs.writeFileSync(path.join(dir,'eight-comparison.png'),c.toBuffer('image/png'));
 const html=`<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>도로롱 · 원본 8장 대조 r43</title>
 <style>body{margin:0;background:#ece9e3;color:#29252b;font:16px/1.6 system-ui,sans-serif}main{max-width:1120px;margin:auto;padding:28px}h1{font-size:24px}button,select,input{font:inherit}button,select{padding:7px 12px}section{background:white;padding:20px;margin:20px 0;border:1px solid #d4cfd1;border-radius:12px}.notice{border-left:4px solid #a1526b;padding-left:14px}.pairs{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:12px}.pairs img{width:100%;height:auto}.player{display:flex;flex-wrap:wrap;gap:16px}.player img{max-width:100%;height:auto}.scrub{background:linear-gradient(90deg,white 50%,#252a31 50%);width:480px;height:300px;display:flex;align-items:center;justify-content:space-around}.scrub img{max-width:none;image-rendering:auto}input{width:320px}a{color:#843650}@media(max-width:700px){.pairs{grid-template-columns:1fr}}</style>
 <main><h1>원본 8장 ↔ 수정본 · r43</h1><p>각 쌍의 왼쪽은 원본, 오른쪽은 수정본. 머리 폭으로 크기를 맞췄음. 얼굴·머리 끝은 승인된 r33 파츠 그대로이며, 원본의 저해상도 얼굴을 다시 붙인 것이 아님.</p>
 <p class="notice">미리보기임. 실제 앱·설치본 미적용. 02~07 몸·발을 수정하고 양끝 몸을 기존 평상시·수락 잡기에 맞춤. 구형 평상시 머리와 승인된 머리의 바깥 마스크 차이는 남아 있으며, 이를 원래대로 되돌리지 않았음.</p>
 <section><h2>전환 확인</h2><p>저장된 57개 프레임으로 재생함. 느린 검수용 속도이며 마지막→처음은 반복 재생 초기화, 놓기 모션이 아님.</p><label>크기 <select id="size"><option>1.00</option><option>1.25</option><option selected>1.50</option></select></label><div class="player"><img id="movie" src="slow-preview-1.50.png" width="480" height="300" alt="흰색·어두운 배경의 느린 전환"><div><div class="scrub"><img id="light" src="frame-1.50-00.png" alt="선택 프레임 흰 배경"><img id="dark" src="frame-1.50-00.png" alt="선택 프레임 어두운 배경"></div><input id="frame" type="range" min="0" max="56" value="0"><output id="number">0 / 56</output></div></div></section>
 <section><h2>8장 각각 대조</h2><p>이미지를 누르면 확대 비교를 열 수 있음. 01은 기존 평상시 몸, 08은 이미 확인한 잡기 몸으로 경계를 맞춘 상태.</p><div class="pairs">${Array.from({length:8},(_,n)=>`<a href="pair-${n+1}.png"><img src="pair-${n+1}.png" alt="원본 0${n+1}과 수정본 나란히"></a>`).join('')}</div></section>
 <section><h2>시작·끝 별도 비교</h2><img src="boundary-comparison.png" style="width:100%" alt="기존 평상시·전환 시작·수락 잡기·전환 끝"><p>동일한 승인 머리로 합성하면 시작·끝의 몸 출력은 일치함. 기존 평상시 전체 이미지와는 머리 파츠 차이 때문에 352픽셀이 다름. 잡기 끝은 기존 수락 이미지와 일치함.</p><p>직접 관찰 범위: 8개 원본 비교 + 1·1.25·1.5배의 57개 저장 표본 각각, 흰색·어두운 배경. 자동 검사: 파일/보호 영역/양끝/애니메이션 인코딩. 실시간 앱 조작·다른 각도·사용자 시각 수락은 별도임.</p></section></main>
 <script>const size=document.querySelector('#size'),frame=document.querySelector('#frame');function draw(){let n=String(frame.value).padStart(2,'0');for(const id of ['light','dark']){const im=document.getElementById(id);im.src='frame-'+size.value+'-'+n+'.png';im.width=160*Number(size.value);im.height=160*Number(size.value);}document.querySelector('#number').textContent=frame.value+' / 56';}size.onchange=()=>{document.querySelector('#movie').src='slow-preview-'+size.value+'.png';draw()};frame.oninput=draw;draw();</script></html>`;
 fs.writeFileSync(path.join(dir,'index.html'),html);console.log(dir);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

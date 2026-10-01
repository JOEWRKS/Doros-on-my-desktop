'use strict';
const {test}=require('node:test'),assert=require('node:assert/strict'),path=require('node:path'),fs=require('node:fs');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const pixels=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
const at=(c,x,y)=>[...c.getContext('2d').getImageData(Math.floor(x),Math.floor(y),1,1).data];
async function setup(){
  assert.ok(fs.existsSync(path.join(__dirname,'original-contour-render.cjs')),'endpoint renderer is missing');
  const s=await canvas.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'));
  const m=require('./original-contour-source.cjs').measure(s,canvas);
  return {s,m,r:require('./original-contour-render.cjs').createRenderer(s,m,canvas),getPose:require('./original-contour-poses.cjs').getPose,buildParts:require('./original-contour-art.cjs').buildParts};
}
function boundary(c,mask){
  const ctx=canvas.createCanvas(96,96).getContext('2d');ctx.beginPath();
  for(const cmd of mask){if(cmd.op==='M')ctx.moveTo(...cmd.points[0]);else if(cmd.op==='L')ctx.lineTo(...cmd.points[0]);else ctx.closePath();}
  const d=pixels(c),a=(x,y)=>x<0||y<0||x>=96||y>=96?0:d[(y*96+x)*4+3],result=[];
  for(let y=0;y<96;y++)for(let x=0;x<96;x++)if(a(x,y)>=128&&!ctx.isPointInPath(x+.5,y+.5)&&[[1,0],[-1,0],[0,1],[0,-1]].some(([dx,dy])=>a(x+dx,y+dy)<128))result.push([x+.5,y+.5]);
  return result;
}
const distance=(a,b)=>Math.max(...a.map(p=>Math.min(...b.map(q=>Math.hypot(p[0]-q[0],p[1]-q[1])))));
test('opaque sockets stay opaque in both endpoint poses',async()=>{
  const {m,r,getPose,buildParts}=await setup();
  for(const name of ['standing','seated']){
    const pose=getPose(name,m),image=r.render(pose);
    for(const part of buildParts(m,pose))for(const q of part.coverage)assert.ok(at(image,...q)[3]>=254,`${name} ${part.name} socket ${q} exposes background`);
    const y=name==='standing'?75:77,x=name==='standing'?38:32;
    for(let n=0;n<5;n++)assert.ok(at(image,x+n,y).slice(0,3).every(v=>v>240),`${name} hidden root ink leaked onto skin`);
    for(const bg of ['#ffffff','#202630']){
      const out=canvas.createCanvas(96,96),ctx=out.getContext('2d');ctx.fillStyle=bg;ctx.fillRect(0,0,96,96);ctx.drawImage(image,0,0);
      for(let n=0;n<5;n++)assert.ok(at(out,x+n,y).slice(0,3).every(v=>v>240));
    }
  }
});
test('depth is explicit and foreground skin hides rear ink',async()=>{
  const {m,r,getPose,buildParts}=await setup(),p=getPose('standing',m),parts=buildParts(m,p);
  assert.deepEqual(pixels(r.renderParts(parts,p)),pixels(r.renderParts([...parts].reverse(),p)));
  const injected=structuredClone(parts),far=injected.find(l=>l.name==='farFront');
  far.ink.push([{op:'M',points:[[37,80]]},{op:'L',points:[[44,80]]}]);
  assert.deepEqual(at(r.renderParts(injected,p),40,80),[255,255,255,255]);
});
test('pose changes artwork, not head size or body scale',async()=>{
  const {m,r,getPose}=await setup(),a=getPose('standing',m),b=structuredClone(a);
  b.legs.find(l=>l.name==='nearHind').joints[1][0]-=2;
  assert.notDeepEqual(pixels(r.render(a)),pixels(r.render(b)));
  assert.deepEqual(pixels(r.renderHead(a)),pixels(r.renderHead(b)));
  const actual=boundary(r.render(a),m.headMask),error=Math.max(distance(actual,m.bodyBoundary),distance(m.bodyBoundary,actual));
  assert.ok(error<=1,`standing contour distance ${error}px exceeds 1 native pixel`);
  for(const leg of getPose('seated',m).legs.filter(l=>l.name!=='nearHind')){
    const [root,knee,tip]=leg.joints,v=[tip[0]-root[0],tip[1]-root[1]];
    assert.ok(Math.abs((knee[0]-root[0])*v[1]-(knee[1]-root[1])*v[0])/Math.hypot(...v)<=.25);
    assert.ok(tip[1]>root[1]);
  }
});
test('invalid coordinates and scales are rejected',async()=>{
  const {m,r,getPose}=await setup(),p=getPose('standing',m),bad=structuredClone(p);bad.pelvis[0]=NaN;
  assert.throws(()=>r.render(bad),/finite|coordinate/i);
  for(const s of [0,-1,Infinity])assert.throws(()=>r.render(p,s),/scale/i);
  const duplicate=structuredClone(p);duplicate.legs[0].name=duplicate.legs[1].name;
  assert.throws(()=>r.render(duplicate),/leg|anatomy/i);
});
test('seated rump has continuous exterior ink without a detached inner curl',async()=>{
  const {m,r,getPose}=await setup(),image=r.render(getPose('seated',m)),points=boundary(image,m.headMask),missing=[];
  for(const [cx,cy] of points.filter(([x,y])=>x>50&&y>78)){
    const x=Math.floor(cx),y=Math.floor(cy);let ink=false;
    for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){
      const v=at(image,x+dx,y+dy);ink||=v[3]>100&&v.slice(0,3).some(n=>n<210);
    }
    if(!ink)missing.push([x,y]);
  }
  assert.deepEqual(missing,[],'rump fill boundary has no exterior outline');
});
test('upper thigh ink is occluded without punching a skin hole',async()=>{
  const {m,r,getPose}=await setup(),v=at(r.render(getPose('seated',m)),53,75);
  assert.ok(v.slice(0,3).every(n=>n>240),'upper thigh ink should be occluded by opaque rump tissue');assert.equal(v[3],255);
});
test('forward distal hindfoot contour remains visible over seated rump skin',async()=>{
  const {m,r,getPose}=await setup(),image=r.render(getPose('seated',m));
  for(const q of [[50,77],[49,78]]){
    const v=at(image,...q);assert.ok(v[3]>=254&&v[0]<210,`distal hindfoot contour ${q} is buried by rump`);
    for(const bg of ['#ffffff','#202630']){
      const c=canvas.createCanvas(96,96),ctx=c.getContext('2d');ctx.fillStyle=bg;ctx.fillRect(0,0,96,96);ctx.drawImage(image,0,0);
      assert.ok(at(c,...q)[0]<210);
    }
  }
});

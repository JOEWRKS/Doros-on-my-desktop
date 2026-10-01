'use strict';
const {test}=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {pose}=require('./connected-sit-rig.cjs');
const file=path.join(__dirname,'connected-sit-skin.cjs');
const source=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png');
async function setup(){
  assert.ok(fs.existsSync(file),'Approved connected rig needs its own skin renderer');
  const original=await canvas.loadImage(source);
  return {original,skin:require(file).create(original,{createCanvas:canvas.createCanvas})};
}
const pixels=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
const pixel=(c,x,y)=>[...c.getContext('2d').getImageData(Math.round(x),Math.round(y),1,1).data];
test('standing face and ribbon keep opaque canonical pixels without an endpoint image',async()=>{
  const {original,skin}=await setup(),c=canvas.createCanvas(96,96);c.getContext('2d').drawImage(original,0,0);
  const expected=pixels(c),out=skin.render(pose(0)),actual=pixels(out);
  assert.equal(out.width,96);assert.equal(out.height,96);
  for(let y=24;y<62;y++)for(let x=8;x<76;x++){
    const i=(y*96+x)*4;
    if(expected[i+3]===255&&expected[i]>expected[i+1]+25)
      assert.deepEqual([...actual.slice(i,i+4)],[...expected.slice(i,i+4)],`source head changed at ${x},${y}`);
  }
  for(const [x,y] of [[67,40],[65,45],[63,54],[62,60]]){
    const i=(y*96+x)*4;assert.deepEqual([...actual.slice(i,i+4)],[...expected.slice(i,i+4)],'ribbon detail changed');
  }
});
test('skin consumes the supplied joints, not a separate timeline or static endpoint',async()=>{
  const {skin}=await setup(),p=pose(.5),original=JSON.stringify(p);
  const before=skin.render(p,{scale:2}),changed=structuredClone(p);
  const leg=changed.legs.find(l=>l.name==='nearHind');
  leg.joints=leg.joints.map(q=>[q[0]+5,q[1],q[2]]);
  const after=skin.render(changed,{scale:2});
  assert.ok(!Buffer.from(pixels(before)).equals(Buffer.from(pixels(after))),'renderer ignores actual joints');
  assert.equal(JSON.stringify(p),original,'renderer mutated approved rig');
});
test('near hind knee and extended paw stay opaque white over the torso',async()=>{
  const {skin}=await setup();
  for(const direction of ['sit','rise'])for(let n=0;n<=20;n++){
    const p=pose(n/20,direction),out=skin.render(p,{scale:2});
    for(const j of p.legs.find(l=>l.name==='nearHind').joints.slice(1)){
      const rgba=pixel(out,(j[0]+.7*j[2])*2,(83.5-j[1]+.27*j[2])*2);
      assert.ok(rgba[0]>220&&rgba[1]>220&&rgba[2]>220&&rgba[3]>245,`near leg lost at ${direction}:${n}`);
    }
  }
});
test('near hind ink stays in front of white torso, while far ink stays behind',async()=>{
  const {skin}=await setup(),p=pose(1),back=structuredClone(p);
  back.legs.find(l=>l.name==='nearHind').near=false;
  const frontPixel=pixel(skin.render(p,{scale:4}),62.3*4,71.5*4);
  const backPixel=pixel(skin.render(back,{scale:4}),62.3*4,71.5*4);
  assert.ok(frontPixel[0]<140&&frontPixel[3]===255,'near-hind edge erased by white torso');
  assert.ok(backPixel[0]>240&&backPixel[3]===255,'far-hind edge painted over torso');
});
test('near foreleg occludes hindleg interior ink independent of input order',async()=>{
  const {skin}=await setup(),p=pose(1),withoutForeInk=structuredClone(p);
  withoutForeInk.legs.find(l=>l.name==='nearFront').near=false;
  const hidden=pixel(skin.render(p,{scale:4}),47.3*4,79.5*4);
  const exposed=pixel(skin.render(withoutForeInk,{scale:4}),47.3*4,79.5*4);
  assert.ok(hidden[0]>240,'front skin did not cover hind ink');
  assert.ok(exposed[0]<180,'fixture does not overlap an actual hind edge');
  p.legs.reverse();assert.deepEqual(pixel(skin.render(p,{scale:4}),47.3*4,79.5*4),hidden);
});
test('canonical head remains on top when a near limb crosses its pixels',async()=>{
  const {skin}=await setup(),p=pose(0),baseline=skin.render(p,{scale:4});
  p.legs.find(l=>l.name==='nearHind').joints=[[20,50,9],[30,50,9],[40,50,9]];
  const covered=skin.render(p,{scale:4});
  assert.deepEqual(pixel(covered,32*4,34*4),pixel(baseline,32*4,34*4),'limb painted over opaque head artwork');
});
test('skin has transparent exterior and no sole paint below the approved floor',async()=>{
  const {skin}=await setup();
  for(const t of [0,.4,.55,.65,.72,.8,1]){
    const out=skin.render(pose(t),{scale:4}),d=pixels(out);
    assert.equal(pixel(out,0,0)[3],0);
    for(let y=346;y<384;y++)for(let x=0;x<384;x++)
      assert.ok(d[(y*384+x)*4+3]<8,`paint below near floor at ${t}:${x},${y}`);
  }
});
test('head retains its area through the connected plop without diagnostic ink',async()=>{
  const {skin}=await setup();let min=Infinity,max=0;
  for(const t of [0,.2,.4,.55,.65,.72,.8,1]){
    const d=pixels(skin.render(pose(t),{scale:3}));let pink=0;
    for(let i=0;i<d.length;i+=4)if(d[i+3]>220&&d[i]>d[i+1]+25&&d[i+2]>d[i+1]+8)pink++;
    min=Math.min(min,pink);max=Math.max(max,pink);
  }
  assert.ok(min>2000,'original head missing');assert.ok((max-min)/max<.05,'head stretched or dissolved');
});
test('render rejects unsupported output scales instead of silently resampling',async()=>{
  const {skin}=await setup();
  for(const scale of [0,-1,1.5,NaN])assert.throws(()=>skin.render(pose(0),{scale}),/scale/i);
});
test('body and limb union has no cut-out notches at skin attachment roots',async()=>{
  const {skin}=await setup();
  for(const t of [0,.4,.55,.72,1]){
    const p=pose(t),withoutHead=structuredClone(p);withoutHead.head.center=[500,500,0];
    const out=pixels(skin.render(withoutHead,{scale:4}));
    const c=canvas.createCanvas(384,384),ctx=c.getContext('2d');ctx.scale(4,4);ctx.fillStyle=ctx.strokeStyle='#fff';ctx.lineCap=ctx.lineJoin='round';
    const point=q=>[q[0]+.7*q[2],83.5-q[1]+.27*q[2]];
    const line=(points,width)=>{ctx.beginPath();points.forEach((q,i)=>i?ctx.lineTo(...point(q)):ctx.moveTo(...point(q)));ctx.lineWidth=width;ctx.stroke();};
    for(const m of [p.chest,p.pelvis]){ctx.beginPath();ctx.ellipse(...point(m.center),...m.radii,0,0,Math.PI*2);ctx.fill();}
    line([p.chest.center,p.pelvis.center],17);line([p.neck.base,p.neck.tip],9);
    for(const l of p.legs)line(l.joints,l.radii[0]*2);
    const mask=pixels(c);let holes=0;
    for(let i=3;i<mask.length;i+=4)if(mask[i]>240&&out[i]<16)holes++;
    assert.equal(holes,0,`attachment mask cut ${holes} solid skin pixels at ${t}`);
  }
});

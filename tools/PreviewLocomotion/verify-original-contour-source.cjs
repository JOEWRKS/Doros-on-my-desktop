'use strict';
const {test}=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const sourcePath=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png');
async function setup(){
  const file=path.join(__dirname,'original-contour-source.cjs');
  assert.ok(fs.existsSync(file),'original contour source module is missing');
  const source=await canvas.loadImage(sourcePath),api=require(file),m=api.measure(source,canvas);
  return {source,m,head:api.extractHead(source,m,canvas),api};
}
const rgba=c=>c.getContext('2d').getImageData(0,0,96,96).data;
const alpha=(c,x,y)=>c.getContext('2d').getImageData(x,y,1,1).data[3];
test('source dimensions and original head are preserved',async()=>{
  const {source,m,head}=await setup();
  assert.deepEqual([source.width,source.height],[96,96]);
  assert.deepEqual([head.width,head.height],[96,96]);assert.ok(m.headLandmarks.length>=3);
  const original=canvas.createCanvas(96,96);original.getContext('2d').drawImage(source,0,0);
  const expected=rgba(original),actual=rgba(head);let visible=0;
  for(let i=0;i<actual.length;i+=4)if(actual[i+3]){
    assert.deepEqual([...actual.slice(i,i+4)],[...expected.slice(i,i+4)],`changed original head texel ${i/4}`);visible++;
  }
  assert.ok(visible>1800,'original head must not be replaced by a small fragment');
  assert.equal(alpha(head,16,69),0);assert.ok(alpha(head,35,65)>128);
  for(const [x,y] of [[28,46],[42,47],[59,34],[11,52],[37,61]])assert.equal(alpha(head,x,y),alpha(original,x,y));
});
test('measurement distinguishes bone tips from sole contacts',async()=>{
  const {m}=await setup();
  assert.deepEqual(m.standing.legs.map(l=>l.name).sort(),['farFront','nearFront','nearHind']);
  for(const leg of m.standing.legs){
    assert.equal(leg.joints.length,3);assert.notDeepEqual(leg.sole,leg.joints[2]);
    assert.ok(m.legWidths[leg.name]>0);
  }
  assert.ok(m.ink.width>0&&m.ink.width<3);assert.equal(m.ink.rgb.length,3);
  assert.ok(m.bodyBoundary.length>40);
});
test('invalid source size and malformed anatomy are rejected',async()=>{
  const {api,m}=await setup();
  assert.throws(()=>api.measure(canvas.createCanvas(100,100),canvas),/96/);
  const bad=structuredClone(m);bad.standing.legs[0].joints[0][0]=Infinity;
  assert.throws(()=>api.validateMeasurements(bad),/finite|coordinate/);
});
test('outline thickness is measured from source instead of a fixed style',async()=>{
  const {source,m,api}=await setup(),thick=canvas.createCanvas(96,96),ctx=thick.getContext('2d');
  ctx.drawImage(source,0,0);ctx.strokeStyle='rgb(69,53,57)';ctx.lineWidth=2.5;
  ctx.beginPath();ctx.moveTo(76,52);ctx.lineTo(76,71);ctx.stroke();
  assert.ok(api.measure(thick,canvas).ink.width>m.ink.width+.5,'extra real ink did not affect measured outline width');
});
test('left hair fringe remains with original head rather than body',async()=>{
  const {source,head}=await setup(),c=canvas.createCanvas(96,96);c.getContext('2d').drawImage(source,0,0);
  assert.ok(alpha(c,14,68)>=128);
  assert.equal(alpha(head,14,68),alpha(c,14,68));
});

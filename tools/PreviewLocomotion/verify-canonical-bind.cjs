'use strict';
const {test}=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const source=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png');
const file=path.join(__dirname,'canonical-bind.cjs');
const pixels=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
async function setup(){
  assert.ok(fs.existsSync(file),'Original-shaped drawing must exist before joint animation');
  const original=await canvas.loadImage(source),reference=canvas.createCanvas(96,96);
  reference.getContext('2d').drawImage(original,0,0);
  return {reference,bind:require(file).create(original,canvas)};
}
// These fail if a generic body replaces source paint, a cut loses coverage,
// hidden legs are duplicated, or joint transforms are merely decorative.
test('rest drawing reconstructs every decoded source RGBA byte',async()=>{
  const {reference,bind}=await setup();
  assert.deepEqual(pixels(bind.render()),pixels(reference));
});
test('every visible source pixel belongs to exactly one nonempty anatomical layer',async()=>{
  const {reference,bind}=await setup(),expected=pixels(reference),counts={};
  for(const layer of bind.layers){
    const rgba=pixels(layer.canvas);counts[layer.name]=0;
    for(let i=0;i<rgba.length;i+=4)if(rgba[i+3]){
      counts[layer.name]++;
      assert.deepEqual([...rgba.slice(i,i+4)],[...expected.slice(i,i+4)]);
      assert.equal(bind.ownerAt(i/4%96,Math.floor(i/4/96)),layer.name);
    }
  }
  assert.ok(counts.head>0&&counts.torso>0&&counts.nearFront>0&&counts.farFront>0&&counts.nearHind>0);
  const owners=new Uint8Array(96*96);
  for(const layer of bind.layers){const rgba=pixels(layer.canvas);for(let i=0;i<owners.length;i++)owners[i]+=Number(rgba[i*4+3]>0);}
  for(let i=0;i<owners.length;i++)assert.equal(owners[i],Number(expected[i*4+3]>0),`paint ownership at ${i%96},${Math.floor(i/96)}`);
});
test('each visible leg has only root, bend and tip; no independent ankle axis',async()=>{
  const {bind}=await setup();
  for(const leg of bind.rig.legs){
    assert.equal(leg.joints.length,3);
    if(leg.visible)for(const [x,y] of leg.joints)assert.ok(pixels(bind.render())[(Math.floor(y)*96+Math.floor(x))*4+3]>128,'joint outside original skin');
    assert.equal(leg.visible,leg.name!=='farHind','occluded hindleg is not invented');
  }
  for(const leg of bind.rig.legs.filter(l=>l.front)){
    const [a,b,c]=leg.joints;
    assert.ok(Math.abs((b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0]))<1e-8,'foreleg must be straight');
  }
});
test('body, near forefoot and hindfoot keep canonical outline and proportions',async()=>{
  const {bind}=await setup();
  assert.equal(bind.ownerAt(42,82),'nearFront');
  assert.equal(bind.ownerAt(23,78),'farFront');
  assert.equal(bind.ownerAt(65,78),'nearHind');
  assert.equal(bind.ownerAt(69,57),'torso');
  assert.equal(bind.ownerAt(66,41),'head');
});
test('the front-body corner below the hair belongs to body paint, not the moving head',async()=>{
  const {bind}=await setup(),head=bind.renderLayer('head');
  assert.equal(head.getContext('2d').getImageData(16,69,1,1).data[3],0,'front-body outline rides the head');
  assert.equal(bind.ownerAt(16,69),'farFront');
  assert.ok(head.getContext('2d').getImageData(35,65,1,1).data[3]>128,'canonical jaw line was removed');
});
test('haunch and belly tissue do not fold with the narrow leg shafts',async()=>{
  const {bind}=await setup();
  for(const point of [[71,64],[70,66],[30,71]])assert.equal(bind.ownerAt(...point),'torso',`body tissue bound to a leg at ${point}`);
  for(const [point,name] of [[[42,82],'nearFront'],[[23,78],'farFront'],[[65,78],'nearHind']])assert.equal(bind.ownerAt(...point),name);
});
test('actual root rotation moves isolated original leg paint, without moving the head',async()=>{
  const {bind}=await setup(),before=pixels(bind.renderLayer('nearFront'));
  const after=pixels(bind.renderLayer('nearFront',{bend:0,rootAngle:.12}));
  assert.notDeepEqual(before,after,'joint transform ignored');
  assert.deepEqual(pixels(bind.renderLayer('head')),pixels(bind.layers.find(l=>l.name==='head').canvas));
});
test('knee bends consume the second segment and hold the root attachment fixed',async()=>{
  const {bind}=await setup(),leg=bind.rig.legs.find(l=>l.name==='nearHind');
  const a=bind.transformPoint(leg.name,leg.joints[0],{bend:.2});
  const b=bind.transformPoint(leg.name,leg.joints[2],{bend:.2});
  assert.deepEqual(a,leg.joints[0]);assert.ok(Math.hypot(b[0]-leg.joints[2][0],b[1]-leg.joints[2][1])>1);
});
test('bend-only rendering moves actual original hindleg pixels without a root rotation',async()=>{
  const {bind}=await setup(),before=pixels(bind.renderLayer('nearHind'));
  const after=pixels(bind.renderLayer('nearHind',{rootAngle:0,bend:.22}));
  let changed=0;for(let i=0;i<before.length;i+=4)if(before.slice(i,i+4).some((v,n)=>v!==after[i+n]))changed++;
  assert.ok(changed>20,`rendered knee bend ignored: only ${changed} pixels changed`);
});
test('scaled rest is the same source drawing and export cannot mutate its bind',async()=>{
  const {reference,bind}=await setup(),before=pixels(bind.render()).slice();
  const expected=canvas.createCanvas(384,384),ctx=expected.getContext('2d');ctx.imageSmoothingEnabled=false;ctx.drawImage(reference,0,0,384,384);
  assert.deepEqual(pixels(bind.render({scale:4})),pixels(expected));
  bind.renderLayer('nearHind',{bend:.1,rootAngle:.1});
  assert.deepEqual(pixels(bind.render()),before);
  assert.throws(()=>bind.render({scale:0}),/scale/i);
});

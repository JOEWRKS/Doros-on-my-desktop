'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const {setup}=require('./motion-proof.cjs'),{create,maxRaisedRoll}=require('./upright.cjs');
const {pixels}=require('./sampling.cjs'),{split}=require('./parts.cjs');
const ready=setup();
test('gaze displacement is continuous across the old hard mouth rectangle',()=>{
 const {gazeWeight}=require('./upright.cjs');
 for(const x of [25,26,33,34])for(let y=54;y<=64;y+=.125)
  assert.ok(Math.abs(gazeWeight(x-1e-5,y)-gazeWeight(x+1e-5,y))<.0001,`x discontinuity ${x},${y}`);
 for(const y of [57,58,62])for(let x=23;x<=36;x+=.125)
  assert.ok(Math.abs(gazeWeight(x,y-1e-5)-gazeWeight(x,y+1e-5))<.0001,`y discontinuity ${x},${y}`);
 for(const [x,y] of [[27,59],[30,60],[32,59]])assert.equal(gazeWeight(x,y),0,'mouth must remain fixed');
});
test('rear paw ink is hidden only under foreground skin; exposed paw remains',async()=>{
 const {api,postures}=await ready,r=postures.open.rig;
 for(let n=0;n<48;n++){
  const phase=n/48,feet=[0,1,2].map(i=>require('./upright.cjs').foot(i,phase,false));
  const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
  const options={includeHead:false,mapOverride:name=>{
   const leg=['farFront','nearFront','nearHind'].indexOf(name);
   return ([x,y])=>{const w=smooth((y-59)/(leg===0?21:25));return leg<0?[x,y]:[x+feet[leg][0]*w,y+feet[leg][1]*w];};
  }};
  const far=require('./upright.cjs').foot(3,phase,false);
  options.extraParts={farHind:require('../PreviewLocomotion/reference-parts.cjs').paths('nearHind',r.pose(0),{
   artwork:require('./body.cjs').artwork,mapOverride:()=> ([x,y])=>{const w=smooth((y-59)/25);return [x-10+far[0]*w,y-2+far[1]*w];}
  })};
  const d=pixels(r.render(r.pose(0),options),api);
  const frontOptions={...options,extraParts:{}};
  const front=pixels(r.render(r.pose(0),frontOptions),api),skin=pixels(r.render(r.pose(0),{...frontOptions,includeInk:false}),api);
  let exposed=0;
  for(let y=155;y<202;y++)for(let x=120;x<176;x++){
   // Use uninked skin, not the output alpha: a stroke can fill a tiny exterior
   // gap with opaque ink. Such a pixel is not inside the actual skin geometry.
   let interior=true;for(let dy=-3;dy<=3;dy++)for(let dx=-3;dx<=3;dx++)if(skin[((y+dy)*225+x+dx)*4+3]<254)interior=false;
   const i=(y*225+x)*4;
   if(interior)for(let k=0;k<4;k++)assert.equal(d[i+k],front[i+k],`hidden paw ink ${n}: ${x},${y}`);
   if(front[i+3]<10&&d[i+3]>200)exposed++;
  }
  assert.ok(exposed>0,`rear paw entirely lost at ${n}`);
 }
});
test('head retains its original semantic layer, not a recut flattened body',async()=>{
 const {api,masters,originals,postures}=await ready;
 for(const eye of ['open','closed','squint']){
  const r=create(masters[eye],api),original=split(originals[eye],api);
  assert.deepEqual(pixels(r.parts.head,api),pixels(original.head,api));
  assert.strictEqual(r.parts.head,postures[eye].head);
  const h=pixels(r.parts.head,api);
  for(const [x,y] of [[161,120],[149,140]])assert.ok(h[(y*225+x)*4+3]>200,'ribbon lost from foreground');
  assert.equal(h[(118*225+169)*4+3],0,'shoulder must not move with head');
 }
});
test('headless body has no stationary ribbon or coloured hair remnants',async()=>{
 const {api,masters}=await ready,b=pixels(create(masters.open,api).parts.body,api);
 for(let i=0;i<b.length;i+=4)if(b[i+3]>200)
  assert.ok(Math.max(...b.slice(i,i+3))-Math.min(...b.slice(i,i+3))<40,'head detail duplicated in body');
});
test('lowered tracking keeps the exposed rear shoulder filled',async()=>{
 const {api,masters}=await ready,r=create(masters.open,api,{padding:16});
 for(const deg of [-20,-16,-12,-8,-4,0]){
  const d=pixels(r.render({roll:deg*Math.PI/180}),api);
  for(const [x,y] of [[67,52],[69,56],[70,61],[67,65]]){
   const i=(Math.floor(24+y*1.5)*192+Math.floor(24+x*1.5))*4;
   assert.ok(d[i+3]>=250,`rear shoulder notch at ${deg}: ${x},${y}`);
  }
 }
});
test('raised extreme stops at14 degrees while accepted8-degree sway remains distinct',async()=>{
 const {api,masters}=await ready,r=create(masters.open,api,{padding:16});
 assert.equal(maxRaisedRoll,14*Math.PI/180);
 assert.deepEqual(pixels(r.render({roll:Math.PI/9}),api),pixels(r.render({roll:maxRaisedRoll}),api));
 assert.notDeepEqual(pixels(r.render({roll:8*Math.PI/180}),api),pixels(r.render({roll:maxRaisedRoll}),api));
});
test('rest hind paw cross-sections follow the corrected original225 silhouette',async()=>{
 const {api,originals,masters}=await ready,a=pixels(originals.open,api),b=pixels(masters.open,api);
 const section=(d,y)=>{const xs=[];for(let x=135;x<172;x++)if(d[(y*225+x)*4+3]>=128)xs.push(x);return [xs[0],xs.at(-1)];};
 for(let y=182;y<=196;y++){
  const old=section(a,y),now=section(b,y);
  for(let i=0;i<2;i++)assert.ok(Math.abs(old[i]-now[i])<=2,`hind contour drift ${y}: ${old} / ${now}`);
 }
});

'use strict';
const {test}=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const source=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png');
const file=path.join(__dirname,'canonical-sit.cjs');
const pixels=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
const d=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1]);
async function setup(){assert.ok(fs.existsSync(file),'Original-bound sitting renderer required');const original=await canvas.loadImage(source);return {original,skin:require(file).create(original,canvas)};}
// Catch rigid-paint scaling, independent contour morphs, sliding feet, late
// leg activation, pose-input ignorance and broken attachment/depth coverage.
test('standing reconstruction retains every canonical source pixel',async()=>{
  const {original,skin}=await setup(),ref=canvas.createCanvas(96,96);ref.getContext('2d').drawImage(original,0,0);
  assert.deepEqual(pixels(skin.render(0)),pixels(ref));
});
test('completed chest and haunch artwork remain rigid, independent of leg folding',async()=>{
  const {skin}=await setup();
  assert.equal(typeof skin.renderBody,'function','missing independent complete body artwork');
  const rest=skin.pose(0),image=skin.renderBody(rest,{scale:4}),rgba=pixels(image);
  const area=a=>{let sum=0;for(let i=3;i<a.length;i+=4)sum+=a[i]/255;return sum;},expected=area(rgba);
  for(const t of [.35,.5,.7,1]){
    const p=skin.pose(t),changed=structuredClone(p);
    changed.legs.find(l=>l.name==='nearHind').joints.forEach(q=>q[0]-=10);
    assert.deepEqual(pixels(skin.renderBody(changed,{scale:4})),pixels(skin.renderBody(p,{scale:4})),'folding a leg also folded haunch artwork');
    for(const part of ['chest','haunch']){
      const original=area(pixels(skin.renderBody(rest,{scale:4,part})));
      assert.ok(Math.abs(area(pixels(skin.renderBody(p,{scale:4,part})))-original)<original*.015,`${part} artwork volume changed`);
    }
    assert.ok(Math.abs(area(pixels(skin.renderBody(p,{scale:4})))-expected)<expected*.08,'body volume changed beyond interior overlap');
  }
  for(const [x,y]of[[71,64],[70,66],[30,71]])assert.ok(rgba[(y*4*384+x*4)*4+3]>240,'original body tissue missing from completed plate');
});
test('all two-bone chains and spine retain their original bound lengths',async()=>{
  const {skin}=await setup(),rest=skin.pose(0);
  for(const direction of ['sit','rise'])for(let n=0;n<=120;n++){
    const p=skin.pose(n/120,direction);
    assert.ok(Math.abs(d(p.chest,p.pelvis)-d(rest.chest,rest.pelvis))<1e-8);
    for(const l of p.legs){const r=rest.legs.find(r=>r.name===l.name);assert.equal(l.joints.length,3);for(let j=0;j<2;j++)assert.ok(Math.abs(d(l.joints[j],l.joints[j+1])-d(r.joints[j],r.joints[j+1]))<1e-8,`${l.name} segment stretched`);}
  }
});
test('front supports remain straight with planted original tips',async()=>{
  const {skin}=await setup(),rest=skin.pose(0);
  for(const direction of ['sit','rise'])for(let n=0;n<=100;n++)for(const l of skin.pose(n/100,direction).legs.filter(l=>l.front)){
    const [a,b,c]=l.joints;assert.ok(Math.abs((b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0]))<1e-8);
    assert.deepEqual(c,rest.legs.find(r=>r.name===l.name).joints[2]);
  }
});
test('hindpaw lifts and reaches before the late pelvic plop',async()=>{
  const {skin}=await setup(),a=skin.pose(0),b=skin.pose(.5),c=skin.pose(.72),leg=p=>p.legs.find(l=>l.name==='nearHind');
  assert.ok(leg(b).joints[2][0]<leg(a).joints[2][0]-6);
  assert.ok(leg(b).joints[2][1]<leg(a).joints[2][1]-1);
  assert.ok(b.pelvis[1]<a.pelvis[1]+3,'pelvis drops before the reaching action');
  assert.ok(c.pelvis[1]>a.pelvis[1]+7);
  assert.ok(leg(c).joints[2][0]<c.pelvis[0]-7,'hindfoot did not reach ahead of haunch');
  assert.deepEqual(leg(c).joints[2],leg(skin.pose(1)).joints[2]);
});
test('tip translation happens off the ground, with one small settle',async()=>{
  const {skin}=await setup();
  for(const direction of ['sit','rise'])for(let n=1;n<=100;n++){
    const a=skin.pose((n-1)/100,direction).legs.find(l=>l.name==='nearHind'),b=skin.pose(n/100,direction).legs.find(l=>l.name==='nearHind');
    if(Math.abs(a.joints[2][0]-b.joints[2][0])>.001)assert.ok(Math.max(a.lift,b.lift)>0);
  }
  assert.ok(skin.pose(.8).pelvis[1]<skin.pose(.72).pelvis[1]);
  assert.ok(Math.abs(skin.pose(.8).pelvis[1]-skin.pose(1).pelvis[1])<1);
});
test('independent rise meets the seated/standing endpoint with continuous poses',async()=>{
  const {skin}=await setup();
  for(const [a,b] of [[skin.pose(1),skin.pose(0,'rise')],[skin.pose(0),skin.pose(1,'rise')]]){
    assert.ok(d(a.chest,b.chest)<1e-9&&d(a.pelvis,b.pelvis)<1e-9);
    for(const l of a.legs)for(let j=0;j<3;j++)assert.ok(d(l.joints[j],b.legs.find(r=>r.name===l.name).joints[j])<1e-8);
  }
  for(let n=1;n<=300;n++){const a=skin.pose((n-1)/300),b=skin.pose(n/300);for(const l of a.legs)for(let j=0;j<3;j++)assert.ok(d(l.joints[j],b.legs.find(r=>r.name===l.name).joints[j])<.8);}
});
test('bone paint transforms remain rigid and their endpoints consume actual joints',async()=>{
  const {skin}=await setup();
  for(let n=0;n<=20;n++){
    const p=skin.pose(n/20);
    for(const part of skin.transforms(p)){
      const [a,b,c,d,e,f]=part.matrix;assert.ok(Math.abs(a*d-b*c-1)<1e-8,'paint was scaled');
      if(part.from)for(let j=0;j<2;j++)assert.ok(Math.hypot(a*part.from[j][0]+c*part.from[j][1]+e-part.to[j][0],b*part.from[j][0]+d*part.from[j][1]+f-part.to[j][1])<1e-8);
    }
  }
});
test('rendered head retains source pink area without body melting or extra hidden-leg paint',async()=>{
  const {skin}=await setup();const count=c=>{let n=0;const a=pixels(c);for(let i=0;i<a.length;i+=4)if(a[i+3]>240&&a[i]>a[i+1]+25)n++;return n;};const rest=count(skin.render(0,{scale:2}));
  for(let n=0;n<=10;n++)assert.ok(Math.abs(count(skin.render(n/10,{scale:2}))-rest)<rest*.06);
  assert.equal(skin.bind.layers.some(l=>l.name==='farHind'),false);
});
test('moving a supplied hind joint actually changes the rendered original limb',async()=>{
  const {skin}=await setup(),p=skin.pose(.7),copy=structuredClone(p),before=pixels(skin.renderPose(p,{scale:2}));
  const l=copy.legs.find(l=>l.name==='nearHind');l.joints=l.joints.map(q=>[q[0]+3,q[1]]);
  assert.notDeepEqual(pixels(skin.renderPose(copy,{scale:2})),before);
  assert.deepEqual(p,skin.pose(.7));
});
test('opaque connecting skin covers joint roots and painted limb centers through both motions',async()=>{
  const {skin}=await setup();
  for(const direction of ['sit','rise'])for(let n=0;n<=20;n++){
    const p=skin.pose(n/20,direction),c=skin.renderPose(p,{scale:4}),ctx=c.getContext('2d');
    for(const l of p.legs.filter(l=>l.visible))for(const q of l.joints){const a=ctx.getImageData(Math.round(q[0]*4),Math.round(q[1]*4),1,1).data;assert.ok(a[3]>240,`skin seam at ${l.name}:${n}:${q}`);}
  }
});
test('render validates scale and motion inputs instead of generating invalid frames',async()=>{
  const {skin}=await setup();assert.throws(()=>skin.pose(NaN),/finite/i);assert.throws(()=>skin.pose(.5,'walk'),/direction/i);assert.throws(()=>skin.render(.5,{scale:0}),/scale/i);
});
test('articulated white body has no trapped transparent cut holes across sit and rise',async()=>{
  const {skin}=await setup(),failures=[];
  for(const direction of ['sit','rise'])for(let n=0;n<=40;n++){
    const t=n/40,c=skin.render(t,{direction,scale:4}),a=pixels(c),w=c.width,seen=new Uint8Array(w*w),queue=[0];seen[0]=1;
    for(let n=0;n<queue.length;n++){const i=queue[n],x=i%w,y=Math.floor(i/w);for(const [dx,dy]of[[1,0],[-1,0],[0,1],[0,-1]]){const xx=x+dx,yy=y+dy,j=yy*w+xx;if(xx>=0&&xx<w&&yy>=0&&yy<w&&!seen[j]&&a[j*4+3]<128){seen[j]=1;queue.push(j);}}}
    let count=0,first=null;
    for(let y=70*4;y<86*4;y++)for(let x=17*4;x<74*4;x++)if(!seen[y*w+x]&&a[(y*w+x)*4+3]<128){count++;first??=[x/4,y/4];}
    if(count)failures.push({direction,t,count,first});
  }
  assert.equal(failures.length,0,`incomplete sockets in ${failures.length} sampled poses: ${JSON.stringify(failures.slice(0,12))}`);
});
test('overlapping front-root cuts do not leave a tiny outlined notch inside the belly',async()=>{
  const {skin}=await setup(),c=skin.render(1,{scale:4});
  const p=c.getContext('2d').getImageData(110,298,1,1).data;
  assert.ok(p[3]>245&&Math.min(...p.slice(0,3))>230,'old between-leg notch became a gray belly spot');
});
test('near front opaque paint hides a hind crease crossing its foreground interior',async()=>{
  const {skin}=await setup(),p=skin.pose(1),l=p.legs.find(l=>l.name==='nearHind');
  l.joints=[[46,65],[47,73],[47,81]];
  const pixel=skin.renderPose(p,{scale:4}).getContext('2d').getImageData(159,311,1,1).data;
  assert.ok(pixel[3]>245&&Math.min(...pixel.slice(0,3))>230,'hind crease shows through front paw');
  const reversed=structuredClone(p);reversed.legs.reverse();
  assert.deepEqual(pixels(skin.renderPose(reversed)),pixels(skin.renderPose(p)),'array order changes depth');
});
test('rendered paint approaches rest continuously, without an entry or exit outline swap',async()=>{
  const {skin}=await setup(),rest=pixels(skin.render(0));
  for(const c of [skin.render(.0001),skin.render(.919,{direction:'rise'})]){
    const a=pixels(c);let total=0;for(let i=0;i<a.length;i++)total+=Math.abs(a[i]-rest[i]);
    assert.ok(total<200,`near-rest outline swap: RGBA difference ${total}`);
  }
});
test('head-only motion does not recolor or reshape the resting body',async()=>{
  const {skin}=await setup(),p=skin.pose(0),before=pixels(skin.renderPose(p));p.head.angle=.0001;
  const after=pixels(skin.renderPose(p));
  assert.ok(Buffer.from(after.slice(74*96*4)).equals(Buffer.from(before.slice(74*96*4))),'head tracking triggers body reconstruction');
});
test('rigid whole-character translation retains original paint rather than rebuilding body style',async()=>{
  const {skin,original}=await setup(),p=skin.pose(0);p.chest[0]+=3;p.pelvis[0]+=3;p.head.pivot[0]+=3;
  for(const l of p.legs)for(const q of l.joints)q[0]+=3;
  const expected=canvas.createCanvas(96,96);expected.getContext('2d').drawImage(original,3,0);
  assert.ok(Buffer.from(pixels(skin.renderPose(p))).equals(Buffer.from(pixels(expected))),'translated source paint changed');
});

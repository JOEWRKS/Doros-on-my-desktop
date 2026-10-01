const {test}=require('node:test');
const assert=require('node:assert/strict');
const path=require('node:path'),fs=require('node:fs');
// Absence is an assertion failure, not an import failure: the old morph has no
// articulated contact/weight-transfer contract at all.
const file=path.join(__dirname,'seating-motion.js');
const motion=fs.existsSync(file)?require(file):null;
const available=()=>assert.ok(motion,'An articulated sit/rise motion model is required');
const dist=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1]);
test('forepaws remain planted after the initial placement, through settling',()=>{
  available();
  for(const phase of ['sit','rise']){
    const anchor=motion.pose(phase==='sit'?.36:0,phase).frontFeet;
    const last=phase==='sit'?1:.68;
    for(let t=phase==='sit'?.36:0;t<=last;t+=.01)
      motion.pose(t,phase).frontFeet.forEach((p,i)=>assert.ok(dist(p,anchor[i])<1e-8,`${phase} foot ${i} slid at ${t}`));
  }
});
test('hindpaw relocation occurs above the ground, never as a grounded slide',()=>{
  available();
  for(const phase of ['sit','rise'])for(let i=1;i<=200;i++){
    const a=motion.pose((i-1)/200,phase),b=motion.pose(i/200,phase);
    for(let foot=0;foot<2;foot++)if(Math.abs(a.hindFeet[foot][0]-b.hindFeet[foot][0])>.0001)
      assert.ok(Math.max(a.hindLift[foot],b.hindLift[foot])>0,`${phase} hind foot ${foot} slid`);
  }
});
test('weight shifts before the pelvis drops and the head settles after contact',()=>{
  available();
  const ready=motion.pose(.13),down=motion.pose(.58),contact=motion.pose(.8),late=motion.pose(.9),end=motion.pose(1);
  assert.ok(ready.shoulder[0]<-1,'no forward weight transfer');
  assert.ok(ready.fold<.06,'pelvis dropped before support preparation');
  assert.ok(down.fold>.45,'no pelvic descent');
  assert.equal(contact.fold,1,'rump should have reached its support');
  assert.ok(dist(contact.head,late.head)>.05,'head and torso stop at the same instant');
  assert.equal(dist(end.head,[0,0]),0);
  assert.equal(dist(end.shoulder,[0,0]),0);
});
test('rendered face shifts as a rigid part during preparation, preserving both endpoints',async()=>{
  available();
  const vm=require('node:vm');
  const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const name of ['rig.js','seating-motion.js','../GenerateLayeredPull/contour.js','sit-correspondence.js','authored-sit.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,name),'utf8'),scope);
  const standing=await canvas.loadImage(path.resolve(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'));
  const seated=await canvas.loadImage(path.join(__dirname,'assets/seated-final-10-2.png'));
  const renderer=scope.authoredSit.create(standing,seated);
  const pixels=t=>Buffer.from(renderer.render(t).getContext('2d').getImageData(0,0,96,96).data);
  function pinkCenter(data){let sx=0,sy=0,n=0;for(let y=0;y<60;y++)for(let x=0;x<96;x++){
    const i=(y*96+x)*4;if(data[i+3]>200&&data[i]>data[i+1]+25&&data[i+2]>data[i+1]+10){sx+=x;sy+=y;n++;}
  }return [sx/n,sy/n];}
  const a=pinkCenter(pixels(0)),b=pinkCenter(pixels(.28));
  assert.ok(b[0]<a[0]-1,'renderer ignored head weight shift');
  const expected=canvas.createCanvas(96,96);expected.getContext('2d').drawImage(seated,3,-12);
  assert.ok(pixels(1).equals(Buffer.from(expected.getContext('2d').getImageData(0,0,96,96).data)));
  for(const direction of ['sit','rise'])for(let n=0;n<=64;n++){
    const data=renderer.render(n/64,1,direction).getContext('2d').getImageData(0,0,96,96).data;
    // A missing body patch or clipped pose must not silently pass the rig tests.
    let area=0;for(let i=3;i<data.length;i+=4)area+=data[i]/255;
    assert.ok(area>2400&&area<4200,`invalid silhouette area ${direction} ${n}: ${area}`);
  }
  for(const boundary of [4.2,5.1,6.8,7.45]){
    const a=scope.locomotion.scenario(boundary-1e-6),b=scope.locomotion.scenario(boundary+1e-6);
    assert.ok(Math.abs(a.position-b.position)<.001,'preview teleported at sit/rise transition');
    assert.ok(Math.abs(a.sit-b.sit)<.001,'preview pose jumped at sit/rise transition');
  }
});
test('hind joints solve fixed-length two-link legs, rise is not reversed sitting',()=>{
  available();
  for(const phase of ['sit','rise'])for(let i=0;i<=100;i++){
    const p=motion.pose(i/100,phase);
    for(const leg of p.hindLegs){
      assert.ok(Math.abs(dist(leg.hip,leg.knee)-leg.upper)<1e-8);
      assert.ok(Math.abs(dist(leg.knee,leg.paw)-leg.lower)<1e-8);
    }
  }
  assert.ok(dist(motion.pose(.3,'rise').head,motion.pose(.7,'sit').head)>.1);
});
test('pose is continuous, finite, bounded and rejects invalid time or direction',()=>{
  available();
  assert.throws(()=>motion.pose(NaN),/finite/i);
  assert.throws(()=>motion.pose(.5,'jump'),/direction/i);
  for(const direction of ['sit','rise']){
    let before;
    for(let i=0;i<=1000;i++){
      const p=motion.pose(i/1000,direction);
      const values=[...p.head,...p.shoulder,...p.pelvis,...p.frontFeet.flat(),...p.hindFeet.flat()];
      assert.ok(values.every(Number.isFinite));
      if(before)assert.ok(Math.max(...values.map((v,k)=>Math.abs(v-before[k])))<.25,'pose jumped');
      before=values;
    }
  }
});
test('articulated texture triangles do not fold or collapse during sit/rise',()=>{
  available();const vm=require('node:vm'),scope={};vm.createContext(scope);
  vm.runInContext(fs.readFileSync(file,'utf8'),scope);
  // Inspect the renderer's actual topology in the test VM, not a copied mesh.
  vm.runInContext(fs.readFileSync(path.join(__dirname,'sit-correspondence.js'),'utf8')
    .replace('root.sitCorrespondence={createMap,landmarks};','root.sitCorrespondence={createMap,landmarks,triangles,pairs};'),scope);
  const {landmarks,triangles,pairs}=scope.sitCorrespondence;
  const cross=(a,b,c)=>(b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0]);
  for(const direction of ['sit','rise'])for(let n=0;n<=500;n++){
    const q=landmarks(n/500,direction);
    for(const tri of triangles)assert.ok(cross(...tri.map(i=>pairs[i][0]))*cross(...tri.map(i=>q[i]))>1e-6,`folded ${direction} mesh at ${n}/500 triangle ${tri}`);
  }
});

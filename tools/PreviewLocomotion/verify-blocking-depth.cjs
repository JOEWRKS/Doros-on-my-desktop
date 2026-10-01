const {test}=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const {draw}=require('./connected-sit-blocking.cjs');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const rgb={head:[228,147,181],chest:[62,195,190],pelvis:[234,162,89],nearFront:[57,165,223],nearHind:[153,102,207]};
const makeLeg=(front,near,joints)=>({name:`${near?'near':'far'}${front?'Front':'Hind'}`,front,near,joints,radii:[3,3]});
const fixture=legs=>({chest:{center:[50,25,0],radii:[10,10]},pelvis:{center:[75,25,0],radii:[10,10]},
  head:{center:[50,55,0],radii:[8,8],angle:0},neck:{base:[50,25,0],tip:[50,55,0]},legs});
test('default rendering keeps x-ray joint overlays opt-in',()=>{
  const c=canvas.createCanvas(600,420),ctx=c.getContext('2d'),p=fixture([]);
  draw(ctx,p);const normal=Buffer.from(ctx.getImageData(0,0,600,420).data);
  draw(ctx,p,{bones:false});assert.ok(normal.equals(Buffer.from(ctx.getImageData(0,0,600,420).data)),'default exposes x-ray bones');
  draw(ctx,p,{bones:true});assert.ok(!normal.equals(Buffer.from(ctx.getImageData(0,0,600,420).data)),'explicit joint diagnostics missing');
});
function at(pose,x,y){
  const c=canvas.createCanvas(600,420),ctx=c.getContext('2d');draw(ctx,pose,{bones:false});
  return [...ctx.getImageData(Math.round(70+x*5),Math.round(-65+y*5),1,1).data].slice(0,3);
}
test('near distal foreleg covers the torso while its own shoulder stays buried',()=>{
  const p=fixture([makeLeg(true,true,[[50,25,9],[60,25,9],[65,25,9]])]);
  assert.deepEqual(at(p,66.3,60.93),rgb.nearFront,'near leg incorrectly buried behind unrelated torso');
  assert.deepEqual(at(p,56.3,60.93),rgb.chest,'shoulder attachment painted over its parent');
});
test('only a short root join is hidden, not the near limb inside its parent silhouette',()=>{
  for(const [front,rootX,middleX,tipX,sampleX,parentColor] of [
    [true,50,60,65,59.5,rgb.chest],[false,75,65,60,78.1,rgb.pelvis]
  ]){
    const p=fixture([makeLeg(front,true,[[rootX,25,9],[middleX,25,9],[tipX,25,9]])]);
    assert.deepEqual(at(p,rootX+6.3,60.93),parentColor,'attachment center must stay covered');
    assert.deepEqual(at(p,sampleX,60.93),front?rgb.nearFront:rgb.nearHind,'parent silhouette erased visible near limb');
    (front?p.chest:p.pelvis).radii=[25,20];
    assert.deepEqual(at(p,sampleX,60.93),front?rgb.nearFront:rgb.nearHind,'larger parent expanded the root cover');
  }
});
test('near hind knee remains visible while folding inside the actual haunch',()=>{
  const {pose}=require('./connected-sit-rig.cjs');
  for(const direction of ['sit','rise'])for(let n=0;n<=24;n++){
    const p=pose(n/24,direction),k=p.legs.find(l=>l.near&&!l.front).joints[1];
    assert.deepEqual(at(p,k[0]+.7*k[2],83.5-k[1]+.27*k[2]),rgb.nearHind,`near knee erased at ${direction}:${n}`);
  }
});
test('far limbs stay behind the torso even where their screen paths cross it',()=>{
  const p=fixture([makeLeg(false,false,[[80,25,-9],[80,20,-9],[80,15,-9]])]);
  assert.deepEqual(at(p,73.7,56.07),rgb.pelvis);
});
test('near front outranks near hind and far legs independently of input array order',()=>{
  const limbs=[makeLeg(true,true,[[40,10,9],[50,10,9],[60,10,9]]),
    makeLeg(false,true,[[40,10,9],[50,10,9],[60,10,9]]),
    makeLeg(true,false,[[52.6,5.14,-9],[62.6,5.14,-9],[72.6,5.14,-9]])];
  for(const legs of [limbs,limbs.toReversed()])assert.deepEqual(at(fixture(legs),56.3,75.93),rgb.nearFront);
});
test('head stays in front of near limbs and neck without diagnostic x-ray bones',()=>{
  const p=fixture([makeLeg(true,true,[[43.7,57.43,9],[43.7,52.43,9],[43.7,47.43,9]])]);
  assert.deepEqual(at(p,50,28.5),rgb.head);
});

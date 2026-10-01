const {test}=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const rigPath=path.join(__dirname,'skeleton-sit-rig.js');
const rig=fs.existsSync(rigPath)?require(rigPath):null;
const available=()=>assert.ok(rig,'Canonical-only skeletal rig is required');
const distance=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1]);
test('every skin-bearing limb segment keeps its length throughout sit and rise',()=>{
  available();
  for(const direction of ['sit','rise'])for(let n=0;n<=400;n++){
    const pose=rig.pose(n/400,direction);
    assert.equal(pose.legs.length,4);
    pose.legs.forEach(leg=>leg.lengths.forEach((length,i)=>
      assert.ok(Math.abs(distance(leg.joints[i],leg.joints[i+1])-length)<1e-8,`${leg.name} stretched at ${n}`)));
  }
});
test('skin transforms rotate and translate without scaling, and follow the solved joints',()=>{
  available();
  for(const direction of ['sit','rise'])for(let n=0;n<=100;n++){
    const pose=rig.pose(n/100,direction);
    for(const part of pose.parts){
      const [a,b,c,d,e,f]=part.matrix;
      assert.ok(Math.abs(a*d-b*c-1)<1e-10,'skin changed area');
      assert.ok(Math.abs(a*a+b*b-1)<1e-10,'skin stretched');
      if(part.from){
        const map=p=>[a*p[0]+c*p[1]+e,b*p[0]+d*p[1]+f];
        assert.ok(distance(map(part.from[0]),part.to[0])<1e-8);
        assert.ok(distance(map(part.from[1]),part.to[1])<1e-8);
      }
    }
  }
});
test('front feet stay planted during descent and all foot relocation occurs while lifted',()=>{
  available();const seated=rig.pose(1);
  for(const direction of ['sit','rise'])for(let n=1;n<=400;n++){
    const a=rig.pose((n-1)/400,direction),b=rig.pose(n/400,direction);
    if(direction==='sit'&&n/400>=.36||direction==='rise'&&n/400<=.68)
      for(let i=0;i<2;i++)assert.ok(distance(b.legs[i].joints.at(-1),seated.legs[i].joints.at(-1))<1e-8);
    for(let i=0;i<4;i++)if(Math.abs(a.legs[i].joints.at(-1)[0]-b.legs[i].joints.at(-1)[0])>.0001)
      assert.ok(Math.max(a.legs[i].lift,b.legs[i].lift)>0,'grounded hindpaw slide');
  }
});
test('seated feet approach the authored reference instead of remaining in the standing stance',()=>{
  available();const p=rig.pose(1);
  // Hand measured in the reference registered at(+3,-12), not renderer values.
  for(const [index,goal] of [[0,[26,86]],[1,[40,86]]])
    assert.ok(distance(p.legs[index].joints.at(-1),goal)<1.5,'front paw does not reach the reference seat');
  assert.ok(p.legs[2].joints[0][0]<61,'pelvis remained too far right');
});
test('paths meet continuously at standing and seated poses, with independent rising',()=>{
  available();
  for(const [a,b] of [[rig.pose(0),rig.pose(1,'rise')],[rig.pose(1),rig.pose(0,'rise')]])
    a.parts.forEach((part,i)=>part.matrix.forEach((x,j)=>assert.ok(Math.abs(x-b.parts[i].matrix[j])<1e-8)));
  assert.notDeepEqual(rig.pose(.35,'sit').parts,rig.pose(.65,'rise').parts);
  assert.throws(()=>rig.pose(NaN),/finite/i);assert.throws(()=>rig.pose(.5,'fly'),/direction/i);
});
test('renderer needs only canonical art, preserves the resting silhouette/head, and carries bones into the image',async()=>{
  available();
  const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const name of ['skeleton-sit-rig.js','skeleton-sit.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,name),'utf8'),scope);
  const source=await canvas.loadImage(path.resolve(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'));
  const renderer=scope.skeletonSit.create(source);
  const pixels=t=>Buffer.from(renderer.render(t).getContext('2d').getImageData(0,0,96,96).data);
  const c=canvas.createCanvas(96,96);c.getContext('2d').drawImage(source,0,0);
  const original=c.getContext('2d').getImageData(0,0,96,96).data,rest=pixels(0);
  let intersection=0,union=0;
  for(let i=0;i<rest.length;i+=4){
    if(rest[i+3]>128||original[i+3]>128)union++;
    if(rest[i+3]>128&&original[i+3]>128)intersection++;
    // Body joint cuts are re-outlined, but opaque hair/face texels stay literal.
    if(i/4<96*66&&i/4%96<65&&original[i+3]===255&&original[i]>original[i+1]+20)
      assert.deepEqual([...rest.subarray(i,i+4)],[...original.subarray(i,i+4)]);
  }
  assert.ok(intersection/union>.97,'standing silhouette drifted');
  const lowerInk=data=>{
    let ink=0;for(let y=73;y<88;y++)for(let x=15;x<80;x++){
      const i=(y*96+x)*4;ink+=(255-data[i])*data[i+3]/255;
    }return ink;
  };
  assert.ok(lowerInk(rest)>=lowerInk(original)*.9,'joint seam repair washed out the original body outline');
  assert.ok(lowerInk(rest)<=lowerInk(original)*1.3,'body outline became substantially heavier than the source');
  assert.ok(!pixels(0).equals(pixels(.5)),'bones did not move the actual skin');
  const area=data=>{let a=0;for(let i=3;i<data.length;i+=4)a+=data[i]/255;return a;};
  const restArea=area(pixels(0));
  for(let n=0;n<=32;n++){
    const a=area(pixels(n/32));assert.ok(a>restArea*.8&&a<restArea*1.2,`missing/exploded body at ${n}`);
  }
  // Reference is read by the TEST ONLY. The renderer never receives this image.
  const reference=await canvas.loadImage(path.join(__dirname,'assets/seated-final-10-2.png'));
  const rc=canvas.createCanvas(96,96);rc.getContext('2d').drawImage(reference,3,-12);
  const target=rc.getContext('2d').getImageData(0,0,96,96).data,end=pixels(1);
  let common=0,either=0;
  for(let y=70;y<90;y++)for(let x=15;x<81;x++){
    const i=(y*96+x)*4,a=end[i+3]>128,b=target[i+3]>128;if(a||b)either++;if(a&&b)common++;
  }
  assert.ok(common/either>.78,`seated lower-body silhouette diverges from reference: ${common/either}`);
  for(const direction of ['sit','rise'])for(const endpoint of [0,1]){
    const data=t=>Buffer.from(renderer.render(t,{direction,scale:2}).getContext('2d').getImageData(0,0,192,192).data);
    const a=data(endpoint),b=data(endpoint===0?1e-9:1-1e-9);
    let max=0;for(let i=0;i<a.length;i+=4)for(let c=0;c<4;c++){
      const x=c===3?a[i+3]:a[i+c]*a[i+3]/255,y=c===3?b[i+3]:b[i+c]*b[i+3]/255;
      max=Math.max(max,Math.abs(x-y));
    }assert.ok(max<=2,`${direction} ${endpoint}: endpoint raster popped by ${max}`);
  }
});

test('soft-skin endpoint exposes long forelegs and a smooth reference-like haunch',async()=>{
  const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const name of ['skeleton-sit-rig.js','skeleton-sit.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,name),'utf8'),scope);
  const renderer=scope.skeletonSit.create(await canvas.loadImage(path.resolve(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png')));
  const endpoint=renderer.renderSeatedEndpoint();
  const data=endpoint.getContext('2d').getImageData(0,0,96,96).data;
  const alpha=(x,y)=>data[(y*96+x)*4+3];
  // Registered reference has alpha0 at (33,82); (32,79) is partial edge ink,
  // not a transparent gap, so it must not be used as an empty-pixel assertion.
  assert.ok(alpha(33,82)<100,'chest still fills the central foreleg gap');
  assert.ok(alpha(68,80)>200,'haunch has a cutout notch instead of a rounded side');
  const reference=canvas.createCanvas(96,96);
  reference.getContext('2d').drawImage(await canvas.loadImage(path.join(__dirname,'assets/seated-final-10-2.png')),3,-12);
  const ref=reference.getContext('2d').getImageData(0,0,96,96).data;
  let common=0,either=0;
  for(let y=68;y<89;y++)for(let x=16;x<74;x++){
    const a=alpha(x,y)>128,b=ref[(y*96+x)*4+3]>128;
    if(a||b)either++;if(a&&b)common++;
  }
  assert.ok(common/either>.9,`endpoint lower silhouette IoU: ${common/either}`);
  assert.equal(typeof renderer.renderSeatedEndpoint,'function','endpoint study must remain separate from the unapproved transition');
  for(const scale of [1,2,3,4]){
    const c=renderer.renderSeatedEndpoint({scale});assert.equal(c.width,96*scale);
  }
  assert.throws(()=>renderer.renderSeatedEndpoint({scale:0}),/scale/i);
});

test('joint-skinned transition reaches the accepted endpoint without a pose swap or disappearing limb',async()=>{
  const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const name of ['skeleton-sit-rig.js','skeleton-sit.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,name),'utf8'),scope);
  const source=await canvas.loadImage(path.resolve(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'));
  const renderer=scope.skeletonSit.create(source);
  assert.equal(typeof renderer.renderSoft,'function','continuous joint-skinned renderer is missing');
  const pixels=(t,direction='sit')=>Buffer.from(renderer.renderSoft(t,{direction}).getContext('2d').getImageData(0,0,96,96).data);
  // Immutable test-only snapshot approved before transition implementation.
  // Comparing two methods backed by the same renderer would miss endpoint drift.
  const snapshot=canvas.createCanvas(96,96);
  snapshot.getContext('2d').drawImage(await canvas.loadImage(path.join(__dirname,'assets/seated-skin-approved-test.png')),0,0);
  const accepted=snapshot.getContext('2d').getImageData(0,0,96,96).data,end=pixels(1);
  let endpointDelta=0;
  for(let i=0;i<end.length;i+=4)for(let j=0;j<4;j++){
    const a=j===3?accepted[i+3]:accepted[i+j]*accepted[i+3]/255;
    const b=j===3?end[i+3]:end[i+j]*end[i+3]/255;
    endpointDelta=Math.max(endpointDelta,Math.abs(a-b));
  }
  assert.ok(endpointDelta<=2,`accepted seated skin changed: ${endpointDelta}`);
  assert.deepEqual(pixels(0),pixels(1,'rise'));
  assert.deepEqual(pixels(1),pixels(0,'rise'));
  const alphaArea=data=>{let a=0;for(let i=3;i<data.length;i+=4)a+=data[i]/255;return a;};
  const baseArea=alphaArea(pixels(0));
  const originalCanvas=canvas.createCanvas(96,96);originalCanvas.getContext('2d').drawImage(source,0,0);
  const original=originalCanvas.getContext('2d').getImageData(0,0,96,96).data,rest=pixels(0);
  let restCommon=0,restUnion=0,lowerCommon=0,lowerUnion=0;
  for(let i=3;i<rest.length;i+=4){
    const a=rest[i]>128,b=original[i]>128;if(a||b)restUnion++;if(a&&b)restCommon++;
    if(i>=96*68*4){if(a||b)lowerUnion++;if(a&&b)lowerCommon++;}
  }
  assert.ok(restCommon/restUnion>.97,`standing silhouette drifted: ${restCommon/restUnion}`);
  assert.ok(lowerCommon/lowerUnion>.94,`standing lower-body silhouette drifted: ${lowerCommon/lowerUnion}`);
  for(const direction of ['sit','rise']){
    let previous=pixels(0,direction),total=0;
    for(let n=1;n<=64;n++){
      const current=pixels(n/64,direction),area=alphaArea(current);
      assert.ok(area>baseArea*.8&&area<baseArea*1.2,'body volume melted or exploded');
      let delta=0;for(let i=3;i<current.length;i+=4)delta+=Math.abs(current[i]-previous[i])/255;
      assert.ok(delta<90,`one-frame silhouette jump: ${direction} ${n} ${delta}`);
      total+=delta;previous=current;
    }
    assert.ok(total>150,'static endpoint substituted for moving skin');
    for(const endpoint of [0,1])assert.deepEqual(pixels(endpoint,direction),pixels(endpoint?1-1e-9:1e-9,direction),'endpoint swap');
  }
  assert.throws(()=>renderer.renderSoft(NaN),/finite/i);
  assert.throws(()=>renderer.renderSoft(.5,{direction:'fly'}),/direction/i);
});

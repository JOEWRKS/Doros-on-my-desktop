const {test}=require('node:test');
const assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path');
const file=path.join(__dirname,'connected-sit-rig.cjs');
const rig=fs.existsSync(file)?require(file):null;
const available=()=>assert.ok(rig,'Connected mass-and-limb rig is required');
const distance=(a,b)=>Math.hypot(...a.map((v,i)=>v-b[i]));
const angle=j=>{
  const a=j[0].map((v,i)=>v-j[1][i]),b=j[2].map((v,i)=>v-j[1][i]);
  return Math.acos(Math.max(-1,Math.min(1,a.reduce((s,v,i)=>s+v*b[i],0)/(Math.hypot(...a)*Math.hypot(...b)))));
};

test('forelegs remain straight supports throughout sitting and rising, not folded elbows',()=>{
  for(const direction of ['sit','rise'])for(let n=0;n<=1000;n++){
    const p=rig.pose(n/1000,direction);
    for(const l of p.legs.filter(l=>l.front)){
      assert.ok(angle(l.joints)>178*Math.PI/180,'visible foreleg is folded');
      const [root,,foot]=l.joints;
      assert.ok(Math.abs(root[0]-foot[0])/(root[1]-foot[1])<.4,'support leans too far from vertical');
      assert.ok(Math.abs(distance(root,foot)-l.lengths.reduce((a,b)=>a+b))<1e-8,'body does not respect straight support length');
    }
  }
});

test('canonical proportions keep the head broad, belly low and limbs short',()=>{
  available();const p=rig.pose(0),width=p.head.radii[0]*2;
  // Hand-read canonical main head/face mass x7..65/y25..68; the long side-lock
  // reaches y74 but is NOT a full-width chin. Belly y75, near fore sole y86.
  assert.ok(width/(p.head.radii[1]*2)>=1.3,'head is too narrow relative to canonical');
  assert.ok(width/(p.head.radii[1]*2)<=1.4,'head is too flat');
  for(const l of p.legs)
    assert.ok(l.lengths.reduce((a,b)=>a+b)/width<=(l.front?.23:.26),'leg is too long relative to the head');
  assert.ok(p.chest.center[1]-p.chest.radii[1]<=9,'too much leg exposed below the belly');
  assert.ok(p.pelvis.center[1]-p.pelvis.radii[1]<=9,'haunch sits too high above the floor');
});

test('rendered standing landmarks match the canonical head, rump and forefoot envelope',()=>{
  const {draw}=require('./connected-sit-blocking.cjs');
  const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
  const c=canvas.createCanvas(600,420),ctx=c.getContext('2d');draw(ctx,rig.pose(0),{bones:false});
  const d=ctx.getImageData(0,0,600,420).data;
  const bounds=rgb=>{const b=[600,420,-1,-1];for(let y=0;y<420;y++)for(let x=0;x<600;x++){
    const i=(y*600+x)*4;if(rgb.every((v,j)=>Math.abs(v-d[i+j])<3)){
      b[0]=Math.min(b[0],x);b[1]=Math.min(b[1],y);b[2]=Math.max(b[2],x);b[3]=Math.max(b[3],y);
    }}return [(b[0]-70)/5,(b[1]+65)/5,(b[2]-70)/5,(b[3]+65)/5];};
  // Independently read source-space envelopes; small tolerance for abstract
  // ellipses versus the original fringe, pads and ink, not a pixel-likeness claim.
  const head=bounds([228,147,181]);
  [7,25,65,68].forEach((v,i)=>assert.ok(Math.abs(head[i]-v)<2,`head landmark ${i}: ${head[i]}`));
  const rump=bounds([234,162,89]);assert.ok(rump[2]>=72&&rump[2]<=76,'rump width differs from canonical');
  const front=bounds([57,165,223]);assert.ok(front[3]>=84&&front[3]<=87,'front sole height differs from canonical');
  assert.ok(front[2]>=44&&front[2]<=49,'front paw extends beyond canonical right edge plus1px tolerance');
  // Canonical alpha at y82 spans x36..48:13px near forefoot. Compare a
  // cross-section, not the whole projected/tilted limb's bounding box.
  let visible=0;
  const row=Math.round(-65+82*5);
  for(let x=0;x<600;x++)if([57,165,223].every((v,j)=>Math.abs(d[(row*600+x)*4+j]-v)<3))visible++;
  assert.ok(visible/5>=11&&visible/5<=14,'rendered forefoot is too thin relative to canonical13px section');
});

test('every shoulder and hip stays attached to its parent mass, with fixed segment lengths',()=>{
  available();const rest=rig.pose(0);
  for(const direction of ['sit','rise'])for(let n=0;n<=240;n++){
    const p=rig.pose(n/240,direction);
    assert.equal(p.legs.length,4);
    for(const l of p.legs){
      assert.equal(l.joints.length,3,'terminal ankle/toe axis must be removed');
      assert.equal(l.lengths.length,2,'cartoon leg must have only two articulated segments');
      const parent=l.front?p.chest.center:p.pelvis.center;
      assert.deepEqual(l.attachment,rest.legs.find(x=>x.name===l.name).attachment,'attachment slid within parent');
      assert.ok(distance(l.joints[0],parent.map((v,i)=>v+l.attachment[i]))<1e-9,'limb detached from torso');
      const mass=l.front?p.chest:p.pelvis;
      assert.ok((l.attachment[0]/mass.radii[0])**2+(l.attachment[1]/mass.radii[1])**2<1,'root lies outside its parent mass');
      for(let j=0;j<2;j++)assert.ok(Math.abs(distance(l.joints[j],l.joints[j+1])-l.lengths[j])<1e-9,'limb stretched');
    }
    assert.ok(Math.abs(distance(p.chest.center,p.pelvis.center)-24)<1e-9,'spine stretched');
    assert.ok(Math.abs(distance(p.neck.base,p.neck.tip)-distance(rest.neck.base,rest.neck.tip))<1e-9,'neck detached/stretched');
    assert.deepEqual(p.head.radii,rest.head.radii);
    assert.deepEqual(p.chest.radii,rest.chest.radii);
    assert.deepEqual(p.pelvis.radii,rest.pelvis.radii);
  }
});

test('front soles remain planted and hind feet only relocate off the floor',()=>{
  available();const rest=rig.pose(0);
  for(const direction of ['sit','rise'])for(let n=1;n<=300;n++){
    const p=rig.pose(n/300,direction),prev=rig.pose((n-1)/300,direction);
    for(let i=0;i<4;i++){
      const l=p.legs[i],foot=l.joints.at(-1),old=prev.legs[i].joints.at(-1);
      assert.ok(foot[1]>=l.radii.at(-1)-1e-9,'rounded sole penetrated floor');
      for(let segment=0;segment<l.lengths.length;segment++)
        assert.ok(Math.min(l.joints[segment][1],l.joints[segment+1][1])-l.radii[segment]>=-1e-9,'capsule penetrated floor');
      if(l.front)assert.ok(distance(foot,rest.legs[i].joints.at(-1))<1e-9,'supporting forefoot slid');
      else if(Math.abs(foot[0]-old[0])>1e-6)assert.ok(Math.max(l.lift,prev.legs[i].lift)>0,'grounded hindfoot slide');
    }
  }
});

test('hindfeet reach forward of the haunch before the plop and stay there',()=>{
  available();const start=rig.pose(0),end=rig.pose(1);
  for(let i=2;i<4;i++){
    const foot=end.legs[i].joints.at(-1);
    assert.ok(foot[0]<end.pelvis.center[0]-10,'hindfoot still tucked under the rump');
    assert.ok(start.legs[i].joints.at(-1)[0]-foot[0]>11,'forward reach is not visible');
    assert.ok(angle(end.legs[i].joints)>125*Math.PI/180,'seated hindleg is still tightly folded');
    assert.ok(distance(rig.pose(.72).legs[i].joints.at(-1),foot)<1e-8,'foot still moves after the plop');
  }
});

test('plop holds preparation then drops quickly and rebounds only once',()=>{
  const start=rig.pose(0),prepare=rig.pose(.4),late=rig.pose(.54),land=rig.pose(.72),bounce=rig.pose(.8),end=rig.pose(1);
  assert.ok(start.pelvis.center[1]-prepare.pelvis.center[1]<.4,'no held preparation');
  assert.ok(late.pelvis.center[1]-land.pelvis.center[1]>5,'descent lacks a quick final drop');
  const floor=land.pelvis.radii[1];
  assert.ok(Math.abs(land.pelvis.center[1]-floor)<1e-8,'settle must begin after ground contact');
  assert.ok(bounce.pelvis.center[1]>floor+.2&&bounce.pelvis.center[1]<floor+.8,'settle is absent or too large');
  assert.ok(Math.abs(end.pelvis.center[1]-floor)<1e-8);
  let peaks=0;
  for(let n=73;n<99;n++)if(rig.pose(n/100).pelvis.center[1]>rig.pose((n-1)/100).pelvis.center[1]&&rig.pose(n/100).pelvis.center[1]>=rig.pose((n+1)/100).pelvis.center[1])peaks++;
  assert.equal(peaks,1,'landing needs exactly one settling rebound');
});

test('visible limb width matches the chunky canonical paws without longer bones',()=>{
  const p=rig.pose(0);
  // Source-space near fore about13px at y82, hind10..11px at y78..80.
  // Rounded proxy diameters are approximate, not anatomical bone lengths.
  for(const leg of p.legs){
    const farFront=leg.front&&!leg.near;
    assert.ok(leg.radii[0]*2>=(farFront?8:10)&&leg.radii[0]*2<=(farFront?10:leg.front?14:12),'limb too thin or inflated relative to source');
    assert.ok(leg.lengths.reduce((a,b)=>a+b)<=14,'thickness must not lengthen limbs');
  }
});

test('weight shift precedes pelvic descent while straight forelegs tilt as supports',()=>{
  available();const rest=rig.pose(0),prepare=rig.pose(.18),end=rig.pose(1);
  assert.ok(prepare.chest.center[0]<rest.chest.center[0]-1.5,'no forward support shift');
  assert.ok(prepare.pelvis.center[1]>rest.pelvis.center[1]-.5,'pelvis drops before support');
  assert.ok(end.pelvis.center[1]<rest.pelvis.center[1]-7,'no pelvic descent');
  assert.ok(end.pelvis.center[0]<rest.pelvis.center[0]-1.5,'rump did not tuck forward');
  for(let i=0;i<2;i++){
    const [root,,foot]=prepare.legs[i].joints;
    assert.ok(Math.abs(root[0]-foot[0])>1.5,'straight support did not lean with chest');
  }
  assert.ok(distance(rest.head.center,prepare.head.center)>1,'head stayed pinned to screen');
  assert.ok(end.pelvis.center[1]-end.pelvis.radii[1]>=-.001,'pelvis intersects ground');
});

test('sit and independent rise share endpoints without discontinuities or invalid poses',()=>{
  available();
  const points=p=>[p.chest.center,p.pelvis.center,p.head.center,...p.legs.flatMap(l=>l.joints)];
  for(const [a,b] of [[rig.pose(0),rig.pose(1,'rise')],[rig.pose(1),rig.pose(0,'rise')]])
    points(a).forEach((v,i)=>assert.ok(distance(v,points(b)[i])<1e-9));
  assert.notDeepEqual(points(rig.pose(.35)),points(rig.pose(.65,'rise')));
  for(const direction of ['sit','rise'])for(let n=1;n<=600;n++){
    const prev=points(rig.pose((n-1)/600,direction));
    points(rig.pose(n/600,direction)).forEach((v,i)=>assert.ok(distance(v,prev[i])<.65,'joint path popped'));
  }
  assert.throws(()=>rig.pose(NaN),/finite/i);assert.throws(()=>rig.pose(.5,'fly'),/direction/i);
});

test('blocking renderer draws moving colored masses without any sprite input',()=>{
  available();const rendererPath=path.join(__dirname,'connected-sit-blocking.cjs');
  assert.ok(fs.existsSync(rendererPath),'Colored blocking renderer is required');
  const {draw}=require(rendererPath);
  const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
  const c=canvas.createCanvas(600,420),ctx=c.getContext('2d');
  const frame=t=>{draw(ctx,rig.pose(t));return Buffer.from(ctx.getImageData(0,0,600,420).data);};
  const start=frame(0),middle=frame(.5),end=frame(1);
  assert.ok(!start.equals(middle)&&!middle.equals(end),'static proxy substituted for articulation');
  const pink=data=>{let n=0;for(let i=0;i<data.length;i+=4)if(data[i]>175&&data[i+1]<180&&data[i+2]>145)n++;return n;};
  assert.ok(pink(start)>10000&&pink(end)>10000,'head mass missing');
  assert.ok(Math.abs(pink(start)-pink(end))/pink(start)<.03,'head changed size');
  // At600x420 the near-front floor is screen y364.65, tip center x281.5.
  // Insetting decorative ink must keep it above that physical floor (AA aside).
  for(let y=366;y<=368;y++)for(let x=279;x<=284;x++)
    assert.ok(start[(y*600+x)*4]>150,'paw outline extends below the contact plane');
});

'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
test('body ink matches source optical width instead of accumulating duplicate contour strokes',async()=>{
  const {create}=require('./reference-parts.cjs'),{measure}=require('./original-contour-source.cjs');
  const source=await canvas.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png')),r=create(source,canvas);
  const expected=measure(source,canvas).ink.width;
  // Same exposed rump, still untranslated during support placement/reach.
  // Restoring additive contour overpainting must make this comparison fail.
  for(const t of [0,.2,.5]){
    const actual=measure(r.render(r.pose(t)),canvas).ink.width;
    assert.ok(Math.abs(actual-expected)<.15,`optical body outline at t=${t}: source ${expected}, render ${actual}`);
  }
});
test('every playback frame has ink at the exposed body perimeter, including newly uncovered sockets',async()=>{
  const {create,trace}=require('./reference-parts.cjs'),r=create(await canvas.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png')),canvas);
  for(let frame=0;frame<240;frame++){
    const p=r.pose(Math.max(0,Math.min(1,(frame/60-.55)/1.05))),skin=canvas.createCanvas(96,96),ctx=skin.getContext('2d');
    for(const name of r.depth){trace(ctx,r.paths(name,p).fill);ctx.fill();}
    ctx.drawImage(r.head,...p.head);
    const a=ctx.getImageData(0,0,96,96).data,d=r.render(p).getContext('2d').getImageData(0,0,96,96).data;
    for(let y=70;y<91;y++)for(let x=14;x<78;x++){
      if(a[(y*96+x)*4+3]<128)continue;
      if(![[1,0],[-1,0],[0,1],[0,-1]].some(([dx,dy])=>a[((y+dy)*96+x+dx)*4+3]<128))continue;
      let ink=false;
      for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){
        if(Math.abs(dx)+Math.abs(dy)>1)continue;
        const i=((y+dy)*96+x+dx)*4;
        if(d[i+3]>=32&&Math.max(d[i],d[i+1],d[i+2])<180)ink=true;
      }
      assert.ok(ink,`missing perimeter ink: video frame ${frame}, pixel ${x},${y}`);
    }
  }
});
test('reference parts retain limb lengths, planted front feet and opaque attachments through the sit',async()=>{
  const file=path.join(__dirname,'reference-parts.cjs');
  assert.ok(require('node:fs').existsSync(file),'anatomical part renderer must exist');
  const {create}=require(file),r=create(await canvas.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png')),canvas);
  const end=r.pose(1),lengths=r.pose(0).legs.map(l=>l.joints.slice(1).map((p,i)=>Math.hypot(p[0]-l.joints[i][0],p[1]-l.joints[i][1])));
  for(let i=0;i<=120;i++){
    const p=r.pose(i/120),d=r.render(p).getContext('2d').getImageData(0,0,96,96).data;
    p.legs.forEach((l,n)=>l.joints.slice(1).forEach((q,j)=>assert.ok(Math.abs(Math.hypot(q[0]-l.joints[j][0],q[1]-l.joints[j][1])-lengths[n][j])<1e-8,'bones must not stretch')));
    for(const l of p.legs){
      const [x,y]=l.joints[0];assert.ok(d[(Math.floor(y)*96+Math.floor(x))*4+3]>=250,`opaque ${l.name} socket at ${i}`);
      if(i>=36&&l.name!=='nearHind')assert.deepEqual(l.joints[2],end.legs.find(e=>e.name===l.name).joints[2],'front supports stay planted during hip drop');
    }
    for(const q of r.attachmentProbes(p))assert.ok(d[(Math.floor(q[1])*96+Math.floor(q[0]))*4+3]>=250,`skin gap at frame ${i}: ${q}`);
    // Interior partial transparency is not accepted as coverage. Exterior AA
    // is flood-connected to the background; an enclosed underfilled seam isn't.
    const outside=new Uint8Array(96*96),queue=[0];outside[0]=1;
    for(let j=0;j<queue.length;j++){
      const n=queue[j],x=n%96,y=Math.floor(n/96);
      // Exterior antialias pixels may connect diagonally around a curved paw.
      // Eight-neighbour background connectivity avoids calling those boundary
      // samples enclosed holes; the opacity requirement remains >=250.
      for(const [dx,dy] of [[1,0],[-1,0],[0,1],[0,-1],[1,1],[1,-1],[-1,1],[-1,-1]]){
        const nx=x+dx,ny=y+dy,q=ny*96+nx;
        if(nx<0||ny<0||nx>=96||ny>=96||outside[q]||d[q*4+3]>=250)continue;
        outside[q]=1;queue.push(q);
      }
    }
    for(let y=70;y<90;y++)for(let x=18;x<74;x++)assert.ok(d[(y*96+x)*4+3]>=250||outside[y*96+x],`underfilled internal skin at ${i}: ${x},${y}`);
    for(const name of ['chest','haunch']){
      const original=r.paths(name,r.pose(0)).fill,moved=r.paths(name,p).fill;
      moved.forEach((c,n)=>c.slice(1).forEach((v,k)=>assert.ok(Math.abs(v-original[n][k+1]-p[name][k%2])<1e-10,'torso and rump shapes must move rigidly')));
    }
  }
  assert.equal(r.pose(.5).haunch[1],0,'haunch must stay up while hindfoot is reaching');
  assert.ok(r.pose(1).haunch[1]>10,'rump must reach the reference seated height');
});
test('hidden parts are closed artwork and individually cover their own joints',async()=>{
  const {create,paint}=require('./reference-parts.cjs'),r=create(await canvas.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png')),canvas);
  for(const t of [0,.3,.5,.75,1])for(const leg of r.pose(t).legs){
    const part=r.paths(leg.name,r.pose(t)),c=canvas.createCanvas(96,96);paint(c.getContext('2d'),part);
    assert.equal(part.fill.at(-1)[0],'Z');
    for(const p of leg.joints.slice(0,2))assert.ok(c.getContext('2d').getImageData(Math.floor(p[0]),Math.floor(p[1]),1,1).data[3]>=250,'joint skin must exist before composition');
  }
});

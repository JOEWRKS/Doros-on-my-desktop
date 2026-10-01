const {test}=require('node:test'),assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),size=96*96*4;
const bank=zlib.gunzipSync(fs.readFileSync(path.join(root,'src/Dororong.App/Assets/locomotion.pbgra.gz')));
const frame=(data,n)=>data.subarray(20+n*size,20+(n+1)*size);
const hash=data=>crypto.createHash('sha256').update(data).digest('hex');
test('v2 bank has complete open/closed sit, walk and continuous reverse-rise frames',()=>{
  assert.equal(bank.toString('ascii',0,4),'LOCO');
  assert.deepEqual([4,8,12,16].map(i=>bank.readUInt32LE(i)),[2,96,96,772]);
  assert.equal(bank.length,20+772*size);
  for(const eye of [0,386]){
    assert.ok(frame(bank,eye+64).equals(frame(bank,eye+321)),'rise starts away from seated endpoint');
    assert.ok(frame(bank,eye).equals(frame(bank,eye+385)),'rise does not end at standing endpoint');
    for(let n=0;n<=64;n++)assert.ok(frame(bank,eye+n).equals(frame(bank,eye+385-n)),'rise must follow the accepted part geometry in reverse');
  }
  for(let i=20;i<bank.length;i+=4)for(let c=0;c<3;c++)assert.ok(bank[i+c]<=bank[i+3],'broken premultiplied alpha');
});

test('all baked sit frames equal the accepted single-ink reference renderer',async()=>{
  const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
  const {create}=require('./reference-parts.cjs');
  for(const [eye,file] of ['dororong-canonical.png','dororong-closed-eyes.png'].entries()){
    const r=create(await canvas.loadImage(path.join(root,'src/Dororong.App/Assets',file)),canvas);
    for(let n=0;n<=64;n++){
      const rgba=r.render(r.pose(n/64)).getContext('2d').getImageData(0,0,96,96).data,expected=Buffer.alloc(size);
      for(let i=0;i<size;i+=4){const a=rgba[i+3];expected[i]=Math.round(rgba[i+2]*a/255);expected[i+1]=Math.round(rgba[i+1]*a/255);expected[i+2]=Math.round(rgba[i]*a/255);expected[i+3]=a;}
      assert.ok(frame(bank,eye*386+n).equals(expected),`accepted renderer differs at eye=${eye}, sit=${n}`);
    }
  }
});
test('source manifest and every baked frame agree with the checked-in bank',()=>{
  const manifest=JSON.parse(fs.readFileSync(path.join(__dirname,'product-manifest.json'),'utf8'));
  assert.equal(hash(bank),manifest.rawSha256);
  for(const [file,sha]of Object.entries(manifest.sources))assert.equal(hash(fs.readFileSync(path.join(__dirname,file))),sha,`stale source ${file}`);
  for(const row of manifest.frames)assert.equal(hash(frame(bank,row.index)),row.sha256,`frame ${row.index}`);
});
// Optional immutable pre-change capture; never overwritten by the exporter.
if(process.env.SEATING_BEFORE_BANK)test('all 512 existing walking frames remain byte-identical',()=>{
  const before=zlib.gunzipSync(fs.readFileSync(process.env.SEATING_BEFORE_BANK));
  const beforeEye=before.readUInt32LE(16)/2;assert.ok([321,386].includes(beforeEye));
  for(let eye=0;eye<2;eye++)for(let n=65;n<321;n++)
    assert.ok(frame(bank,eye*386+n).equals(frame(before,eye*beforeEye+n)),`unrelated walking pose ${eye}/${n} changed`);
});

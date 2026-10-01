// Static acceptance only. No transition frames or product assets are exported.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),crypto=require('node:crypto');
const canvas=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
(async()=>{
  const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/seated-skin-endpoint-20260930'));
  fs.mkdirSync(out,{recursive:true});
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const file of ['skeleton-sit-rig.js','skeleton-sit.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,file),'utf8'),scope);
  const source=path.resolve(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png');
  const renderer=scope.skeletonSit.create(await canvas.loadImage(source));
  const referencePath=path.join(__dirname,'assets/seated-final-10-2.png');
  const reference=canvas.createCanvas(96,96);reference.getContext('2d').drawImage(await canvas.loadImage(referencePath),3,-12);
  const candidate=renderer.renderSeatedEndpoint();
  fs.writeFileSync(path.join(out,'seated-endpoint.png'),candidate.toBuffer('image/png'));
  const comparison=canvas.createCanvas(768,880),cc=comparison.getContext('2d');
  for(const [row,color] of ['#eeeae4','#1c2530'].entries()){
    const y=row*440;cc.fillStyle=color;cc.fillRect(0,y,768,440);
    cc.imageSmoothingEnabled=true;cc.drawImage(reference,144,y+10);cc.drawImage(candidate,528,y+10);
    cc.imageSmoothingEnabled=false;cc.drawImage(reference,48,y+132,288,288);cc.drawImage(candidate,432,y+132,288,288);
  }
  fs.writeFileSync(path.join(out,'reference-comparison.png'),comparison.toBuffer('image/png'));
  const joint=canvas.createCanvas(384,384);joint.getContext('2d').drawImage(renderer.renderSeatedEndpoint({scale:4,debug:true}),0,0);
  fs.writeFileSync(path.join(out,'joint-overlay.png'),joint.toBuffer('image/png'));
  const inputs=['skeleton-sit-rig.js','skeleton-sit.js','export-seated-endpoint.cjs'].map(f=>path.join(__dirname,f)).concat(source);
  const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
  fs.writeFileSync(path.join(out,'provenance.json'),JSON.stringify({purpose:'static endpoint acceptance only; no transition or app integration',body:'joint-anchored reconstructed white skin',texture:'canonical head only',referenceOnly:{file:referencePath,sha256:hash(referencePath)},inputs:Object.fromEntries(inputs.map(f=>[f,hash(f)]))},null,2));
  console.log(out);
})().catch(e=>{console.error(e);process.exitCode=1;});

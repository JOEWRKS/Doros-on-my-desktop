const {test}=require('node:test'),assert=require('node:assert/strict');
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const canvas=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
test('actual 2x export sampler has no one-frame endpoint outline pop in either direction',async()=>{
  const scope={document:{createElement:()=>canvas.createCanvas(96,96)}};vm.createContext(scope);
  for(const file of ['rig.js','seating-motion.js','../GenerateLayeredPull/contour.js','sit-correspondence.js','authored-sit.js'])vm.runInContext(fs.readFileSync(path.join(__dirname,file),'utf8'),scope);
  const renderer=scope.authoredSit.create(
    await canvas.loadImage(path.resolve(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png')),
    await canvas.loadImage(path.join(__dirname,'assets/seated-final-10-2.png')));
  function pixels(t,dir){return Uint8Array.from(renderer.render(t,2,dir).getContext('2d').getImageData(0,0,192,192).data);}
  for(const dir of ['sit','rise'])for(const [a,b]of [[0,1e-8],[1-1e-8,1]]){
    const p=pixels(a,dir),q=pixels(b,dir);let maximum=0;
    for(let i=0;i<p.length;i+=4){maximum=Math.max(maximum,Math.abs(p[i+3]-q[i+3]));for(let c=0;c<3;c++)maximum=Math.max(maximum,Math.abs(p[i+c]*p[i+3]-q[i+c]*q[i+3])/255);}
    assert.ok(maximum<=2,`${dir} endpoint ${a}: pixel jump ${maximum}`);
  }
});

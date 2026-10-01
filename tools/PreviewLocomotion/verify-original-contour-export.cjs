'use strict';
const {test}=require('node:test'),assert=require('node:assert/strict'),path=require('node:path'),fs=require('node:fs'),os=require('node:os'),cp=require('node:child_process');
const canvasPath=path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'),canvas=require(canvasPath);
function exporter(){const file=path.join(__dirname,'export-original-contour.cjs');assert.ok(fs.existsSync(file),'static evidence exporter is missing');return require(file);}
test('static export contains native and enlarged comparison evidence',async()=>{
  const {exportStatic}=exporter(),out=fs.mkdtempSync(path.join(os.tmpdir(),'original-contour-export-')),result=await exportStatic(out);
  for(const f of ['standing.png','seated.png','comparison-native.png','comparison-3x.png','parts.png','joint-overlay.png','verification.json'])assert.ok(result.files.includes(f),f);
  for(const f of ['standing.png','seated.png']){const i=await canvas.loadImage(path.join(out,f));assert.deepEqual([i.width,i.height],[96,96]);}
  const v=JSON.parse(fs.readFileSync(path.join(out,'verification.json'),'utf8'));
  assert.equal(v.animationGenerated,false);assert.equal(v.visualAcceptance,'pending');
  assert.deepEqual(v.backgrounds,['#ffffff','#202630']);assert.ok(v.checks.standingContourDistance<=1);
  assert.equal(v.sourceSha256,'699348d1973709f228d843341ac5312aa7f449d57b9bfc76576256231e259a78');
});
test('runtime is independent of seated reference',()=>{
  // Name the break: adding runtime I/O for the seated PNG must fail in a
  // canonical-only fixture, not merely a source-string grep.
  const out=fs.mkdtempSync(path.join(os.tmpdir(),'original-contour-runtime-'));
  for(const file of ['original-contour-data.json','original-contour-source.cjs','original-contour-art.cjs','original-contour-poses.cjs','original-contour-render.cjs']){
    assert.ok(fs.existsSync(path.join(__dirname,file)),`runtime module ${file} is missing`);fs.copyFileSync(path.join(__dirname,file),path.join(out,file));
  }
  fs.copyFileSync(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'),path.join(out,'canonical.png'));
  const script=`const c=require(${JSON.stringify(canvasPath)});c.loadImage('./canonical.png').then(s=>{const m=require('./original-contour-source.cjs').measure(s,c),r=require('./original-contour-render.cjs').createRenderer(s,m,c);for(const n of ['standing','seated']){const p=require('./original-contour-poses.cjs').getPose(n,m),d=r.render(p).getContext('2d').getImageData(0,0,96,96).data;if(!d.some(Boolean))throw Error('empty runtime artwork');}console.log('canonical-only runtime passed');}).catch(e=>{console.error(e);process.exitCode=1;});`;
  const r=cp.spawnSync(process.execPath,['-e',script],{cwd:out,encoding:'utf8'});assert.equal(r.status,0,r.stderr);assert.match(r.stdout,/canonical-only runtime passed/);
});
test('export CLI rejects unknown flags before producing files',()=>{
  exporter();const r=cp.spawnSync(process.execPath,[path.join(__dirname,'export-original-contour.cjs'),'--video'],{encoding:'utf8'});
  assert.notEqual(r.status,0);assert.match(r.stderr,/argument|static|flag/i);
});

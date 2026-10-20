'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create}=require('./connected-families.cjs'),{pixels}=require('./sampling.cjs'),{inside,hairTipBoundary}=require('./parts.cjs');
const ready=Promise.all([create(),create({restoreHairTip:true})]);
test('restored head pixels match original225 without changing protected head or procedural body',async()=>{
 const [old,current]=await ready,{api,root}=old.s,original=pixels(await api.loadImage(path.join(root,'tools/HighResolutionArt/input/canonical-225.png')),api);
 for(const eye of ['open','closed']){
  const a=pixels(old.s.rigs[eye].parts.head,api),b=pixels(current.s.rigs[eye].parts.head,api);let changed=0,dark=0;
  for(let y=0;y<225;y++)for(let x=0;x<225;x++){
   const i=(y*225+x)*4;if(inside(hairTipBoundary,x+.5,y+.5))assert.deepEqual(b.slice(i,i+4),original.slice(i,i+4));else assert.deepEqual(b.slice(i,i+4),a.slice(i,i+4));
   if(a.slice(i,i+4).some((v,k)=>v!==b[i+k])){changed++;if(b[i+3]>100&&Math.max(...b.slice(i,i+3))<180)dark++;}
  }
  assert.equal(changed,94);assert.equal(dark,27);assert.deepEqual(pixels(old.s.rigs[eye].parts.body,api),pixels(current.s.rigs[eye].parts.body,api));
  // The nearby body contour must not be pulled into the head to fix its tip.
  for(let y=175;y<=177;y++)for(let x=125;x<=130;x++)assert.equal(b[(y*225+x)*4+3],0);
 }
});
test('restored head is used by upright, posture and common composition paths',async()=>{
 const [old,current]=await ready;for(const args of [['track',0,{roll:-.2}],['run',.27,{}],['sit',.8,{}],['hunt',.6,{}],['jump',.4,{}],['eat',.8,{}],['happy',.5,{}],['sleep',1,{}]]){
  assert.notDeepEqual(pixels(current.render(...args),current.s.api),pixels(old.render(...args),old.s.api),args[0]);
 }
});
test('288 saved rotation frames and16 sheets have complete size/eye/angle coverage and valid hashes',async()=>{
 const [old]=await ready,{api,root}=old.s,dir=path.join(root,'artifacts/hair-tip-boundary-20261017-r25'),manifest=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'))),sha=b=>crypto.createHash('sha256').update(b).digest('hex');
 assert.equal(manifest.frames.length,288);assert.equal(manifest.sheets.length,16);assert.equal(manifest.outlineRecovery,false);
 assert.equal(new Set(manifest.frames.map(f=>`${f.eye}/${f.scale}/${f.deg}`)).size,288);
 for(const [file,hash]of Object.entries(manifest.sources))assert.equal(sha(fs.readFileSync(path.join(root,file))),hash,file);
 for(const f of manifest.frames){const raw=fs.readFileSync(path.join(dir,f.name+'.png'));assert.equal(sha(raw),f.sha256);const im=await api.loadImage(raw);assert.equal(im.width,Math.ceil(128*f.scale));assert.equal(im.height,im.width);assert.equal(f.protectedOutputIdentical,true);}
 for(const scale of manifest.scales)for(const eye of ['open','closed'])assert.deepEqual(manifest.frames.filter(f=>f.scale===scale&&f.eye===eye).map(f=>f.deg),manifest.angles);
 for(const sheet of manifest.sheets)assert.ok(fs.existsSync(path.join(dir,sheet)));
});

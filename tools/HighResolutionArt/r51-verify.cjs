'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const root=path.resolve(__dirname,'../..'),dir=path.join(root,'artifacts/runtime-grab-20261018-r51');
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const m=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'))),bank=JSON.parse(fs.readFileSync(path.join(dir,'bank-manifest.json')));
for(const [p,h]of Object.entries(m.sources))assert.equal(hash(path.join(root,p)),h,p);
assert.equal(hash(bank.file),bank.sha256);
const seen=new Set(m.sheets.flatMap(s=>s.records)),hashes=new Set(m.records.filter(r=>seen.has(r.file)).map(r=>r.scale+':'+r.sha256));
for(const r of m.records){assert.equal(hash(path.join(dir,r.file)),r.sha256);assert.equal(hash(path.join(dir,'fifo',path.basename(r.file))),r.sha256);assert(hashes.has(r.scale+':'+r.sha256));}
assert.equal(m.records.length,288);assert.equal(hashes.size,151);
const verification={originalSourcesUnchanged:true,bankUnchanged:true,reviewedFrameSlots:288,distinctReviewed:151,sheets:21,optimizedPixelsIdentical:true,
 code:Object.fromEntries(Object.keys(m.code).map(p=>[p,hash(path.join(root,p))])),
 performance:['1.00','1.25','1.50'].map(s=>{const r=JSON.parse(fs.readFileSync(path.join(dir,`fifo/runtime-${s}.json`)));return{scale:r.scale,meanTickMs:r.meanMs,maxTickMs:r.maxMs,holdTickMs:r.timings.slice(148,192).reduce((a,b)=>a+b)/44};}),
 status:'PARTIAL_RUNTIME_INTEGRATION_NOT_RELEASE_READY',visualEvidence:'visual-review.md'};
fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify(verification,null,2));console.log(JSON.stringify(verification));

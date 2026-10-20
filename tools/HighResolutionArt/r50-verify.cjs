'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const root=path.resolve(__dirname,'../..'),dir=path.join(root,'artifacts/height-lag-20261018-r50');
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const m=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json')));
for(const [f,h]of Object.entries(m.sources))assert.equal(hash(path.join(root,f)),h,f);
assert.equal(hash(path.join(dir,'legacy/probe.json')),m.probeSha256);
const covered=new Set(),records=new Map(m.records.map(r=>[r.file,r]));
for(const sheet of m.sheets){assert(fs.statSync(path.join(dir,sheet.file)).size>0);for(const file of sheet.records){const r=records.get(file);assert(r);covered.add(r.scale+':'+r.sha256+r.oldSha256);}}
for(const r of m.records){assert.equal(hash(path.join(dir,r.file)),r.sha256,r.file);assert.equal(hash(path.join(dir,r.old)),r.oldSha256,r.old);assert(covered.has(r.scale+':'+r.sha256+r.oldSha256));}
assert.equal(m.records.length,414);assert.equal(covered.size,198);assert.equal(m.sheets.length,33);
console.log(JSON.stringify({sourceHashes:Object.keys(m.sources).length,savedPairs:m.records.length,distinctPairsCovered:covered.size,sheets:m.sheets.length,physics:m.physics,lag:m.lagChecks,field:m.fieldChecks,visualStatus:'See manual visual-review.md; hashes are not visual approval'}));

'use strict';
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const {root,api}=require('./bangs-composite-review.cjs'),{premultiply,sample,surface}=require('./sampling.cjs');
async function main(){
 const prior=JSON.parse(fs.readFileSync(path.join(root,'artifacts/height-lag-20261018-r50/manifest.json'))),sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
 // Only immutable artwork and preview authoring inputs; app code will change.
 const sources=Object.fromEntries(Object.entries(prior.sources).filter(([p])=>!p.endsWith('.cs')));
 for(const [p,h]of Object.entries(sources))assert.equal(sha(path.join(root,p)),h,p);
 const m=await require('./grab-boundary-connected.cjs').create(),chunks=[],steps=224;
 function add(data){const b=Buffer.alloc(data.length*4);for(let i=0;i<data.length;i+=4)for(let c=0;c<4;c++)b.writeFloatLE(data[i+([2,1,0,3][c])],(i+c)*4);const z=zlib.gzipSync(b),n=Buffer.alloc(4);n.writeInt32LE(z.length);chunks.push(n,z);}
 add(premultiply(m.head,api));const a=new Float64Array(4);add(premultiply(surface(api,96,(x,y)=>sample(m.body(0),375,(x+32)*225/96-.5,(y+32)*225/96-.5,a),32),api));
 for(let i=0;i<=steps;i++){add(m.body(i/steps));if(i%32===0)console.log('body '+i+'/'+steps);}
 const h=Buffer.alloc(8);h.write('DHR1');h.writeInt32LE(steps,4);
 const file=path.join(root,'src/Dororong.App/Assets/high-resolution-grab.bank');fs.writeFileSync(file,Buffer.concat([h,...chunks]));
 const out=path.join(root,'artifacts/runtime-grab-20261018-r51');fs.mkdirSync(out,{recursive:true});
 fs.writeFileSync(path.join(out,'bank-manifest.json'),JSON.stringify({sources,steps,file,sha256:sha(file),generator:sha(__filename),note:'Immutable approved body samples; nearest 1/224 geometry. No full-pose opacity crossfade.'},null,2));
 console.log(file);
}main().catch(e=>{console.error(e);process.exitCode=1;});

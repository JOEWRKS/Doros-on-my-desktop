'use strict';
// Presentation/decoder verification only; reads existing r36 reviewed frames.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels}=require('./sampling.cjs');
const {apng}=require('./grab-tilt-audit.cjs');
const dir=path.join(root,'artifacts/grab-tilt-audit-20261017-r36');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const table=Array.from({length:256},(_,n)=>{for(let k=0;k<8;k++)n=n&1?0xedb88320^(n>>>1):n>>>1;return n>>>0;});
function crc(data){let c=0xffffffff;for(const b of data)c=table[(c^b)&255]^(c>>>8);return (c^0xffffffff)>>>0;}
function chunk(type,data){const b=Buffer.alloc(data.length+12);b.writeUInt32BE(data.length);b.write(type,4);data.copy(b,8);b.writeUInt32BE(crc(b.subarray(4,-4)),b.length-4);return b;}
function decodeParts(b){let header,parts=[],out=[],expected=0;for(let p=8;p<b.length;){const len=b.readUInt32BE(p),type=b.toString('ascii',p+4,p+8),data=b.subarray(p+8,p+8+len);assert.equal(crc(b.subarray(p+4,p+8+len)),b.readUInt32BE(p+8+len));
 if(type==='IHDR')header=data;
 if(type==='fcTL'){if(parts.length){out.push(parts);parts=[];}assert.equal(data.readUInt32BE(0),expected++);assert.equal(data.readUInt16BE(20),1);assert.equal(data.readUInt16BE(22),30);}
 if(type==='IDAT')parts.push(data);
 if(type==='fdAT'){assert.equal(data.readUInt32BE(0),expected++);parts.push(data.subarray(4));}
 p+=len+12;}out.push(parts);return out.map(ps=>Buffer.concat([b.subarray(0,8),chunk('IHDR',header),...ps.map(d=>chunk('IDAT',d)),chunk('IEND',Buffer.alloc(0))]));}
async function main(){const m=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'),'utf8')),verified=[];
 for(const sc of m.scales){const rec=m.records.filter(f=>f.scale===sc),imgs=[];for(const r of rec){assert.equal(sha(path.join(dir,r.file)),r.sha256);imgs.push(await api.loadImage(path.join(dir,r.file)));}
 const order=m.sequences.find(s=>s.file===`sweep-${sc.toFixed(2)}.png`).order,packed=[];
 for(const i of order){const c=api.createCanvas(480,384),g=c.getContext('2d');g.imageSmoothingEnabled=false;
  for(const dark of [false,true]){const x=dark?240:0;g.fillStyle=dark?'#252a31':'white';g.fillRect(x,0,240,384);g.fillStyle=dark?'white':'#222';g.font='14px sans-serif';g.fillText(`${sc}x | ${m.angles[i]}deg | f${i}`,x+8,22);g.drawImage(imgs[i],x+Math.floor((240-imgs[i].width)/2),30);
   const sx=Math.floor(40*sc),sy=Math.floor(82*sc),sw=Math.ceil(62*sc),sh=Math.ceil(30*sc);g.drawImage(imgs[i],sx,sy,sw,sh,x+20,280,sw*2,sh*2);
  }packed.push(c.toBuffer('image/png'));}
 const f=`playback-${sc.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,f),apng(packed));
 // Independently decode every APNG frame and compare full presentation pixels.
 const decoded=decodeParts(fs.readFileSync(path.join(dir,f)));assert.equal(decoded.length,128);
 for(let i=0;i<decoded.length;i++)assert.deepEqual(pixels(await api.loadImage(decoded[i]),api),pixels(await api.loadImage(packed[i]),api));
 verified.push({file:f,sha256:sha(path.join(dir,f)),decodedFrames:128,decodedPixelIdentity:true,integerPlacement:true,fps:30,order});
 console.log(`verified ${f}`);
 }
 fs.writeFileSync(path.join(dir,'playback-verification.json'),JSON.stringify({status:'ENCODING_VERIFIED_NOT_REALTIME_VISUAL_ACCEPTANCE',verified,sourceManifestSha256:sha(path.join(dir,'manifest.json'))},null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

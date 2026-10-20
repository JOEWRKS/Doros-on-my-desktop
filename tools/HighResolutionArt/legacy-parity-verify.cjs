'use strict';
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs'),{pixels}=require('./sampling.cjs'),{dir}=require('./legacy-parity.cjs'),{panel}=require('./legacy-parity-review.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
async function main(){const m=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json')));for(const [f,h]of Object.entries(m.sources))assert.equal(sha(path.join(root,f)),h);
 const code=fs.readFileSync(path.join(__dirname,'grab-tilt-playback.cjs'),'utf8'),decode=new Function('assert',code.slice(code.indexOf('const table='),code.indexOf('async function main()'))+';return decodeParts;')(assert),images=new Map(),unique=new Map(),boundaries=[];
 for(const r of m.frames){assert.equal(sha(path.join(dir,r.file)),r.sha256);images.set(r.file,await api.loadImage(path.join(dir,r.file)));if(!unique.has(r.sha256))unique.set(r.sha256,r);}
 for(const a of m.animations){assert.equal(sha(path.join(dir,a.file)),a.sha256);const parts=decode(fs.readFileSync(path.join(dir,a.file)));assert.equal(parts.length,a.frames);for(let i=0;i<parts.length;i++){const file=a.sequence[Math.min(i,a.sequence.length-1)],r=m.frames.find(r=>r.file===file);assert.deepEqual(pixels(await api.loadImage(parts[i]),api),pixels(panel(images.get(file),r.label),api));}
  const hold=m.frames.find(r=>r.id===a.id&&r.kind==='hold'&&r.ms===1024),release=m.frames.find(r=>r.id===a.id&&r.kind==='release'&&r.ms===0);assert.equal(hold.sha256,release.sha256);boundaries.push({id:a.id,releaseStartIdentity:true});}
 const sheets=[];for(const scale of [1,1.25,1.5]){const rows=[...unique.values()].filter(r=>r.scale===scale);let b=[Infinity,Infinity,-1,-1];for(const r of rows){const im=images.get(r.file),d=pixels(im,api);for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++)if(d[(y*im.width+x)*4+3])b=[Math.min(b[0],x),Math.min(b[1],y),Math.max(b[2],x+1),Math.max(b[3],y+1)];}
  const box=[b[0]-4,b[1]-4,b[2]-b[0]+8,b[3]-b[1]+8];assert(box[2]<=290&&box[3]<=238);
  for(let start=0;start<rows.length;start+=12){const records=rows.slice(start,start+12),c=api.createCanvas(1200,Math.ceil(records.length/4)*510),g=c.getContext('2d');g.imageSmoothingEnabled=false;
   for(let j=0;j<records.length;j++){const r=records[j],im=images.get(r.file),x=j%4*300,y=Math.floor(j/4)*510;g.fillStyle='#ddd';g.fillRect(x,y,300,510);g.fillStyle='#222';g.font='11px sans-serif';g.fillText(r.file,x+2,y+15);for(const [k,bg]of ['white','#252a31'].entries()){g.fillStyle=bg;g.fillRect(x,y+22+k*244,298,242);g.drawImage(im,...box,x+Math.floor((298-box[2])/2),y+22+k*244,box[2],box[3]);}}
   const file=`review-${scale.toFixed(2)}-${start}.png`;fs.writeFileSync(path.join(dir,file),c.toBuffer('image/png'));sheets.push({file,box,records});}
 }
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify({status:'TECHNICAL_ONLY_DIRECT_REVIEW_PENDING',manifestSha256:sha(path.join(dir,'manifest.json')),sourceHashesUnchanged:true,decoded:m.animations.length,boundaries,sheets,verifierSha256:sha(__filename)},null,2));console.log(JSON.stringify({decoded:m.animations.length,releaseBoundaries:boundaries.length,sheets:sheets.map(s=>s.file)}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

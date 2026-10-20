'use strict';
// Read-only audit of the exact r35 compositor. Does not select runtime motions.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const dir=path.join(root,'artifacts/grab-tilt-audit-20261017-r36');
const src=path.join(root,'artifacts/grab-contour-still-20261017-r33');
const previous=path.join(root,'artifacts/grab-left-occlusion-20261017-r35');
const codeFile=path.join(__dirname,'grab-left-occlusion.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const scales=[1,1.1,1.2,1.25,1.3,1.4,1.5],angles=Array.from({length:65},(_,i)=>-16+i*.5);
const zeroAngles=[-.25,-.1,-.01,-.000001,0,.000001,.01,.1,.25];
const rotate=([x,y],p,deg)=>{const a=deg*Math.PI/180,c=Math.cos(a),s=Math.sin(a);return [p[0]+(x-p[0])*c-(y-p[1])*s,p[1]+(x-p[0])*s+(y-p[1])*c];};
const fname=(sc,i)=>`frame-${sc.toFixed(2)}-${String(i).padStart(3,'0')}.png`;
// APNG wraps unchanged lossless canvas PNG IDAT chunks. No GIF palette conversion.
const crcTable=Array.from({length:256},(_,n)=>{for(let k=0;k<8;k++)n=n&1?0xedb88320^(n>>>1):n>>>1;return n>>>0;});
function chunk(type,data){const t=Buffer.from(type),len=Buffer.alloc(4),check=Buffer.alloc(4);len.writeUInt32BE(data.length);let c=0xffffffff;for(const b of Buffer.concat([t,data]))c=crcTable[(c^b)&255]^(c>>>8);check.writeUInt32BE((c^0xffffffff)>>>0);return Buffer.concat([len,t,data,check]);}
function apng(frames){const parts=[frames[0].subarray(0,8)];let seq=0;
 const read=b=>{const out=[];for(let p=8;p<b.length;){const n=b.readUInt32BE(p),type=b.toString('ascii',p+4,p+8);out.push({type,data:b.subarray(p+8,p+8+n)});p+=n+12;}return out;};
 const header=read(frames[0]).find(c=>c.type==='IHDR').data;parts.push(chunk('IHDR',header));const act=Buffer.alloc(8);act.writeUInt32BE(frames.length);parts.push(chunk('acTL',act));
 for(const [i,png]of frames.entries()){const fc=Buffer.alloc(26);fc.writeUInt32BE(seq++,0);header.copy(fc,4,0,8);fc.writeUInt16BE(1,20);fc.writeUInt16BE(30,22);fc[24]=0;fc[25]=0;parts.push(chunk('fcTL',fc));
  for(const p of read(png).filter(c=>c.type==='IDAT'))if(!i)parts.push(chunk('IDAT',p.data));else{const s=Buffer.alloc(4);s.writeUInt32BE(seq++);parts.push(chunk('fdAT',Buffer.concat([s,p.data])));}}
 parts.push(chunk('IEND',Buffer.alloc(0)));return Buffer.concat(parts);
}
function delta(a,b,w,sc){let count=0,max=0,total=0;const bounds=[Infinity,Infinity,-1,-1];
 for(let y=Math.floor(85*sc);y<Math.ceil(110*sc);y++)for(let x=Math.floor(44*sc);x<Math.ceil(65*sc);x++){
  const at=(y*w+x)*4;let m=0;for(const bg of [0,255])for(let c=0;c<3;c++)m=Math.max(m,Math.abs(a[at+c]*a[at+3]/255+bg*(1-a[at+3]/255)-b[at+c]*b[at+3]/255-bg*(1-b[at+3]/255)));
  if(m>.001){count++;total+=m;max=Math.max(max,m);bounds[0]=Math.min(bounds[0],x);bounds[1]=Math.min(bounds[1],y);bounds[2]=Math.max(bounds[2],x);bounds[3]=Math.max(bounds[3],y);}
 }return {changedPixels:count,maxCompositeDelta:max,totalCompositeDelta:total,bounds:count?bounds:null};}
async function main(){
 fs.mkdirSync(dir,{recursive:true});const old=JSON.parse(fs.readFileSync(path.join(previous,'manifest.json'),'utf8'));
 const code=fs.readFileSync(codeFile,'utf8');assert.equal(sha(codeFile),old.codeSha256);
 for(const [f,hash]of Object.entries(old.contract.sourceHashes))assert.equal(sha(path.join(src,f)),hash);
 const hp=premultiply(await api.loadImage(path.join(src,'masked-head-225.png')),api),bp=premultiply(await api.loadImage(path.join(src,'body-padded-375.png')),api);
 // Extract the exact unchanged function, rather than maintain a second renderer.
 const fragment=code.slice(code.indexOf(' function render('),code.indexOf(' const save='));assert(fragment.includes('function render('));
 const render=new Function('api','hp','bp','sample','over','surface','rotate',fragment+';return render;')(api,hp,bp,sample,over,surface,rotate);
 const save=(f,c)=>fs.writeFileSync(path.join(dir,f),c.toBuffer('image/png'));
 const records=[],sheets=[],zero=[],sequences=[];
 function crop(g,img,x,y,sc,dark){g.fillStyle=dark?'#252a31':'white';g.fillRect(x,y,198,94);g.imageSmoothingEnabled=false;
  const sx=Math.floor(40*sc),sy=Math.floor(82*sc),sw=Math.ceil(62*sc),sh=Math.ceil(30*sc);
  g.drawImage(img,sx,sy,sw,sh,x+4,y+2,sw*2,sh*2);
 }
 for(const sc of scales){
  const imgs=[],data=[],steps=[];
  for(const [i,deg]of angles.entries()){
   const f=fname(sc,i),im=render(sc,deg,true);save(f,im);const read=await api.loadImage(path.join(dir,f)),d=pixels(read,api);imgs.push(read);data.push(d);
   const base=render(sc,deg,false),bd=pixels(base,api);let outside=0,border=0;
   for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++){const at=(y*im.width+x)*4;
    if((!x||!y||x===im.width-1||y===im.height-1)&&d[at+3])border++;
    if(d.slice(at,at+4).some((v,k)=>v!==bd[at+k])&&(x+1<50*sc||x>=61*sc||y+1<89*sc||y>=98.5*sc))outside++;}
   assert.equal(outside,0);assert.equal(border,0);
   const match=old.frames.find(p=>p.scale===sc&&p.angle===deg);if(match)assert.deepEqual(d,pixels(await api.loadImage(path.join(previous,match.file)),api));
   records.push({file:f,scale:sc,angle:deg,sha256:sha(path.join(dir,f)),outsideAllowed:outside,borderAlpha:border});
   if(i)steps.push({from:angles[i-1],to:deg,...delta(data[i-1],d,im.width,sc)});
  }
  for(let start=0;start<65;start+=24){const c=api.createCanvas(1200,Math.ceil(Math.min(24,65-start)/3)*120),g=c.getContext('2d');g.fillStyle='#d8d8dc';g.fillRect(0,0,c.width,c.height);
   for(let n=0;n<Math.min(24,65-start);n++){const i=start+n,x=n%3*400,y=Math.floor(n/3)*120;g.fillStyle='#111';g.font='14px sans-serif';g.fillText(`${sc}x | f${i} | ${angles[i]}deg`,x+6,y+18);crop(g,imgs[i],x,y+24,sc,false);crop(g,imgs[i],x+200,y+24,sc,true);}
   const f=`sheet-${sc.toFixed(2)}-${start}.png`;save(f,c);sheets.push(f);
  }
  const zi=[];for(const [i,deg]of zeroAngles.entries()){const c=render(sc,deg,true),f=`zero-${sc.toFixed(2)}-${i}.png`;save(f,c);zi.push(await api.loadImage(path.join(dir,f)));}
  const c=api.createCanvas(1200,360),g=c.getContext('2d');g.fillStyle='#d8d8dc';g.fillRect(0,0,c.width,c.height);
  for(let i=0;i<zi.length;i++){const x=i%3*400,y=Math.floor(i/3)*120;g.fillStyle='#111';g.font='14px sans-serif';g.fillText(`${sc}x | ${zeroAngles[i]}deg`,x+6,y+18);crop(g,zi[i],x,y+24,sc,false);crop(g,zi[i],x+200,y+24,sc,true);}
  const zf=`zero-sheet-${sc.toFixed(2)}.png`;save(zf,c);sheets.push(zf);
  zero.push({scale:sc,negativeLimit:delta(pixels(zi[3],api),pixels(zi[4],api),zi[4].width,sc),positiveLimit:delta(pixels(zi[4],api),pixels(zi[5],api),zi[4].width,sc),steps});
  // One continuous 30fps forward/back sweep; no product transition is connected.
  const order=[...Array(65).keys(),...Array.from({length:63},(_,i)=>63-i)],pngs=[];
  for(const i of order){const c=api.createCanvas(480,360),g=c.getContext('2d');for(const dark of [false,true]){const x=dark?240:0;g.fillStyle=dark?'#252a31':'white';g.fillRect(x,0,240,360);g.fillStyle=dark?'white':'#222';g.font='14px sans-serif';g.fillText(`${sc}x | ${angles[i]}deg | frame ${i}`,x+8,22);g.drawImage(imgs[i],x+(240-imgs[i].width)/2,30);crop(g,imgs[i],x+20,275,sc,dark);}pngs.push(c.toBuffer('image/png'));}
  const anim=`sweep-${sc.toFixed(2)}.png`;fs.writeFileSync(path.join(dir,anim),apng(pngs));sequences.push({file:anim,frames:128,fps:30,order,sha256:sha(path.join(dir,anim))});
  console.log(JSON.stringify({scale:sc,positiveLimit:zero.at(-1).positiveLimit,frames:65}));
 }
 assert.equal(sha(codeFile),old.codeSha256);for(const [f,hash]of Object.entries(old.contract.sourceHashes))assert.equal(sha(path.join(src,f)),hash);
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'GENERATED_NOT_VISUALLY_REVIEWED',sourceHashes:old.contract.sourceHashes,compositorSha256:old.codeSha256,scales,angles,zeroAngles,records,sheets,zero,sequences,transitionConnected:false,runtimeChanged:false},null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={apng};

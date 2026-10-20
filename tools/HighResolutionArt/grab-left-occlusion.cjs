'use strict';
// Isolated still compositor study. Source artwork and runtime are never written.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const source=path.join(root,'artifacts/grab-contour-still-20261017-r33');
const prior=path.join(root,'artifacts/grab-rotation-review-20261017-r34');
const dir=path.join(root,'artifacts/grab-left-occlusion-20261017-r35');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const scales=[1,1.1,1.2,1.25,1.3,1.4,1.5],angles=[-16,-12,-8,-4,0,4,8,12,16];
const rotate=([x,y],p,deg)=>{const a=deg*Math.PI/180,c=Math.cos(a),s=Math.sin(a);return [p[0]+(x-p[0])*c-(y-p[1])*s,p[1]+(x-p[0])*s+(y-p[1])*c];};
const name=(sc,d)=>`head-${sc.toFixed(2)}-${d<0?'m':'p'}${Math.abs(d)}.png`;
async function main(){
 fs.mkdirSync(dir,{recursive:true});
 const replay=process.argv.includes('--verify-replay')?JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'),'utf8')):null;
 const files=['masked-head-225.png','body-padded-375.png','candidate.png'];
 const hashes=Object.fromEntries(files.map(f=>[f,sha(path.join(source,f))]));
 const h=await api.loadImage(path.join(source,files[0])),b=await api.loadImage(path.join(source,files[1]));
 const hp=premultiply(h,api),bp=premultiply(b,api);
 // Contract fixed before rendering: only left attachment rectangle may differ.
 // Existing body's first nearly vertical edge at logical y=66.5 supplies both
 // skin and ink. It forms a short occlusion plate behind the unchanged head.
 // No new color/outline is authored. The plate owns this region (no doubled AA).
 const contract={sourceHashes:hashes,allowedLogicalRect:[18,57,29,66.5],headPivot:[43,66],scales,angles,
  reference:'r33 source parts; r34 same-angle unoccluded compositor',
  preserve:['source head/face/hair RGBA','source body RGBA','all output outside left attachment','zero/negative angles','whole-pose rotations'],
  criteria:['Hide exposed inward notch without a white rectangle or hole','Keep pointed hair tips and same face/body elsewhere'],
  runtimeSelected:false,transitionConnected:false,
  sampling:'Existing premultiplied cubic source lookup and 2x2 pixel coverage, unchanged',
  skillGate:'Palette/binary-alpha conversion gate not applicable to this partial-alpha compositor; equivalent saved PNG/locality/hash tests below'};
 fs.writeFileSync(path.join(dir,'contract.json'),JSON.stringify(contract,null,2));
 function render(sc,deg,occlude,whole=false){
  const a=new Float64Array(4),z=new Float64Array(4);
  return surface(api,96*sc,(x,y)=>{
   const q=whole?rotate([x,y],[43,32],-deg):[x,y];
   let sy=q[1];
   if(occlude&&!whole&&deg>0&&q[0]>=18&&q[0]<29&&q[1]>=57&&q[1]<66.5)sy=66.5;
   sample(bp,375,(q[0]+32)*225/96-.5,(sy+32)*225/96-.5,a);
   const v=whole?q:rotate(q,[43,66],-deg);
   sample(hp,225,v[0]*225/96-.5,v[1]*225/96-.5,z);
   return over(a,z);
  },32);
 }
 const save=(f,c)=>fs.writeFileSync(path.join(dir,f),c.toBuffer('image/png'));
 const frames=[];
 for(const sc of scales)for(const deg of angles){
  const before=render(sc,deg,false),after=render(sc,deg,true),f=name(sc,deg);
  save(f,after);save('before-'+f,before);
  const im=await api.loadImage(path.join(dir,f)),d=pixels(im,api),old=pixels(await api.loadImage(path.join(dir,'before-'+f)),api);
  let changed=0,outside=0,border=0;const box=[Infinity,Infinity,-Infinity,-Infinity];
  for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++){
   const i=(y*im.width+x)*4;
   if((x===0||y===0||x===im.width-1||y===im.height-1)&&d[i+3])border++;
   if(d.slice(i,i+4).some((v,k)=>v!==old[i+k])){
    changed++;box[0]=Math.min(box[0],x);box[1]=Math.min(box[1],y);box[2]=Math.max(box[2],x);box[3]=Math.max(box[3],y);
    if(x+1<50*sc||x>=61*sc||y+1<89*sc||y>=98.5*sc)outside++;
   }
  }
  assert.equal(outside,0);assert.equal(border,0);if(deg<=0)assert.equal(changed,0);
  const oldFile=path.join(prior,`head-probe-${sc.toFixed(2)}-${deg<0?'m':'p'}${Math.abs(deg)}.png`);
  if(fs.existsSync(oldFile))assert.deepEqual(old,pixels(await api.loadImage(oldFile),api),'baseline must reproduce r34');
  if(sc===1.5&&deg===0)assert.deepEqual(d,pixels(await api.loadImage(path.join(source,'candidate.png')),api));
  frames.push({file:f,scale:sc,angle:deg,sha256:sha(path.join(dir,f)),changedPixels:changed,bounds:changed?box:null,outsideAllowed:outside,borderAlpha:border});
 }
 const whole=[];
 for(const sc of [1,1.1,1.2,1.3,1.4,1.5])for(const deg of [-88,-45,-20,-8,0,8,20,45,88]){
  const f=`whole-${sc.toFixed(2)}-${deg<0?'m':'p'}${Math.abs(deg)}.png`,im=render(sc,deg,true,true);
  assert.deepEqual(pixels(im,api),pixels(await api.loadImage(path.join(prior,f)),api));whole.push({file:f,unchanged:true});
 }
 const sheets=[];
 for(const sc of scales)for(const dark of [false,true]){
  const c=api.createCanvas(960,1080),g=c.getContext('2d');g.fillStyle=dark?'#252a31':'white';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,deg]of angles.entries()){
   const x=i%3*320,y=Math.floor(i/3)*360,im=await api.loadImage(path.join(dir,name(sc,deg)));
   g.fillStyle=dark?'white':'#222';g.font='16px sans-serif';g.fillText(`${sc}x / ${deg}deg`,x+12,y+22);g.drawImage(im,x+(320-im.width)/2,y+28);
   const sx=Math.floor(47*sc),sy=Math.floor(86*sc),sw=Math.ceil(20*sc),sh=Math.ceil(20*sc);
   g.drawImage(im,sx,sy,sw,sh,x+18,y+275,sw*2,sh*2);g.fillText('Left join / 2x',x+110,y+310);
  }
  const f=`sheet-${sc.toFixed(2)}-${dark?'dark':'white'}.png`;save(f,c);sheets.push(f);
 }
 // Saved before/after, same-size output and integer enlarged attachment crops.
 for(const dark of [false,true]){
  const c=api.createCanvas(900,640),g=c.getContext('2d');g.fillStyle=dark?'#252a31':'white';g.fillRect(0,0,900,640);g.imageSmoothingEnabled=false;
  for(const [i,sc]of [1,1.25,1.5].entries())for(const [j,prefix]of ['before-',''].entries()){
   const x=i*300+j*150,im=await api.loadImage(path.join(dir,prefix+name(sc,16)));
   g.fillStyle=dark?'white':'#222';g.font='14px sans-serif';
   if(j===0)g.fillText(`${sc}x / +16deg`,i*300+12,24);
   // Keep full image at actual size; use paired rows to avoid overlap at 1.5x.
   const yy=j?280:40;g.fillText(j?'After':'Before',i*300+12,yy+18);g.drawImage(im,i*300+(300-im.width)/2,yy);
   const sx=Math.floor(47*sc),sy=Math.floor(89*sc),sw=Math.ceil(17*sc),sh=Math.ceil(12*sc);
   g.fillText(j?'After / 3x crop':'Before / 3x crop',x+10,540);g.drawImage(im,sx,sy,sw,sh,x+10,550,sw*3,sh*3);
  }save(`comparison-${dark?'dark':'white'}.png`,c);
 }
 for(const f of files)assert.equal(sha(path.join(source,f)),hashes[f]);
 if(replay)for(const f of frames)assert.equal(f.sha256,replay.frames.find(p=>p.file===f.file).sha256,'reviewed frame changed during replay');
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify({status:'TECHNICAL_CHECKS_ONLY_SEE_VISUAL_REVIEW',codeSha256:sha(__filename),contract,frames,sheets,wholeUnchanged:whole,sourceHashesUnchanged:true,replayMatchesReviewedFrames:!!replay},null,2));
 console.log(JSON.stringify({dir,frames:frames.length,sheets:sheets.length,wholeUnchanged:whole.length,changed:frames.filter(f=>f.changedPixels).length}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

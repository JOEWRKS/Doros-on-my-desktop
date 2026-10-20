'use strict';
// Read-only artwork inspection. No transition renderer or runtime integration.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const pivot=[43,32],headPivot=[43,66],padding=32;
const rotate=([x,y],p,deg)=>{const a=deg*Math.PI/180,c=Math.cos(a),s=Math.sin(a);return [p[0]+(x-p[0])*c-(y-p[1])*s,p[1]+(x-p[0])*s+(y-p[1])*c];};
async function main(){
 const source=path.join(root,'artifacts/grab-contour-still-20261017-r33'),dir=path.join(root,'artifacts/grab-rotation-review-20261017-r34');fs.mkdirSync(dir,{recursive:true});
 const sourceFiles=['masked-head-225.png','body-padded-375.png','candidate.png'],inputHashes=Object.fromEntries(sourceFiles.map(f=>[f,sha(path.join(source,f))]));
 const h=await api.loadImage(path.join(source,sourceFiles[0])),body=await api.loadImage(path.join(source,sourceFiles[1])),hp=premultiply(h,api),bp=premultiply(body,api);
 const frames=[],sheets=[];
 const modes=[{mode:'whole',scales:[1,1.1,1.2,1.3,1.4,1.5],angles:[-88,-45,-20,-8,0,8,20,45,88]},
  {mode:'head-probe',scales:[1,1.25,1.5],angles:[-16,-8,0,8,16]}];
 for(const config of modes)for(const scale of config.scales){
  const imgs=[];
  for(const deg of config.angles){
   const a=new Float64Array(4),b=new Float64Array(4);
   const img=surface(api,96*scale,(x,y)=>{
    let q=[x,y];if(config.mode==='whole')q=rotate(q,pivot,-deg);
    sample(bp,375,(q[0]+32)*225/96-.5,(q[1]+32)*225/96-.5,a);
    const v=config.mode==='head-probe'?rotate(q,headPivot,-deg):q;
    sample(hp,225,v[0]*225/96-.5,v[1]*225/96-.5,b);return over(a,b);
   },padding);
   const name=`${config.mode}-${scale.toFixed(2)}-${deg>=0?'p':'m'}${Math.abs(deg)}.png`,file=path.join(dir,name);fs.writeFileSync(file,img.toBuffer('image/png'));
   const saved=await api.loadImage(file),d=pixels(saved,api);let border=0;
   for(let n=0;n<saved.width;n++)for(const [x,y]of [[n,0],[n,saved.height-1],[0,n],[saved.width-1,n]])if(d[(y*saved.width+x)*4+3]>0)border++;
   let zeroMatchesR33=null;if(scale===1.5&&deg===0){const original=await api.loadImage(path.join(source,'candidate.png'));assert.deepEqual(d,pixels(original,api),'zero angle must match r33');zeroMatchesR33=true;}
   frames.push({name,mode:config.mode,scale,deg,width:saved.width,borderAlphaPixels:border,zeroMatchesR33,sha256:sha(file)});imgs.push(saved);
  }
  for(const dark of [false,true]){
   const rows=Math.ceil(imgs.length/3),sheet=api.createCanvas(960,rows*480),g=sheet.getContext('2d');g.fillStyle=dark?'#252a31':'white';g.fillRect(0,0,960,sheet.height);g.imageSmoothingEnabled=false;
   for(let i=0;i<imgs.length;i++){
    const img=imgs[i],deg=config.angles[i],cx=i%3*320,cy=Math.floor(i/3)*480;g.fillStyle=dark?'white':'#222';g.font='16px sans-serif';g.fillText(`${config.mode} ${scale}x ${deg}deg`,cx+10,cy+22);g.drawImage(img,cx+(320-img.width)/2,cy+30);
    // Crop box derived by transforming an inspection region, not the output.
    const pts=[[12,47],[76,47],[12,76],[76,76]].map(p=>config.mode==='whole'?rotate(p,pivot,deg):p);
    const x=Math.floor((Math.min(...pts.map(p=>p[0]))+padding)*scale)-2,y=Math.floor((Math.min(...pts.map(p=>p[1]))+padding)*scale)-2;
    const w=Math.ceil((Math.max(...pts.map(p=>p[0]))+padding)*scale)+2-x,ht=Math.ceil((Math.max(...pts.map(p=>p[1]))+padding)*scale)+2-y;
    g.drawImage(img,x,y,w,ht,cx+(320-w*2)/2,cy+280,w*2,ht*2);
   }
   const name=`sheet-${config.mode}-${scale.toFixed(2)}-${dark?'dark':'white'}.png`;fs.writeFileSync(path.join(dir,name),sheet.toBuffer('image/png'));sheets.push(name);
  }
 }
 for(const f of sourceFiles)assert.equal(sha(path.join(source,f)),inputHashes[f],'source changed');
 const manifest={status:'GENERATED_NOT_YET_VISUALLY_REVIEWED',runtimeSelected:false,transitionConnected:false,source:'r33',inputHashes,pivot,headPivot,padding,modes,frames,sheets,clippedFrames:frames.filter(f=>f.borderAlphaPixels>0).map(f=>f.name),limitations:['Discrete static samples only, not every scale/angle.','Whole pose rotation follows existing grab pivot without safeAngle clamping.','Head-probe is a diagnostic relative rotation at the common-head pivot, not an accepted hanging motion.','PNG source artwork preserved; no runtime/compositor/platform guarantee.']};
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify(manifest,null,2));console.log(JSON.stringify({dir,frames:frames.length,sheets:sheets.length,clippedFrames:manifest.clippedFrames}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

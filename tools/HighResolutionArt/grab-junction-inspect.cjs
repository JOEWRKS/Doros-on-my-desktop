'use strict';
// Diagnostic views only; never modifies any source image.
const fs=require('node:fs'),path=require('node:path');
const {api,root}=require('./bangs-composite-review.cjs');
const {premultiply,sample,over,surface,pixels}=require('./sampling.cjs');
async function main(){
 const version=process.argv[2]||'r30',dir=path.join(root,`artifacts/grab-contour-still-20261017-${version}`);
 const ref=await api.loadImage(path.join(root,'src/Dororong.App/Assets/user-body-drag/08.png'));
 const cand=await api.loadImage(path.join(dir,'candidate.png'));
 const {sourceToLogical:{factor,dx,dy}}=JSON.parse(fs.readFileSync(path.join(dir,'measurement.json')));
 const regions=[['Left hair / shoulder',22,54,16,23],['Right hair / back',58,48,19,24],['Ribbon / back',65,39,17,25],['Hind feet',44,80,19,18]];
 const c=api.createCanvas(960,1100),g=c.getContext('2d');g.fillStyle='#e8e5df';g.fillRect(0,0,c.width,c.height);
 g.font='17px sans-serif';g.fillStyle='#222';g.fillText('Original 08',55,25);g.fillText(version+' white',365,25);g.fillText(version+' dark',685,25);
 for(let n=0;n<regions.length;n++){
  const [name,x,y,w,h]=regions[n],top=60+n*260,z=9;g.fillStyle='#222';g.fillText(name,25,top-10);
  for(let col=0;col<3;col++){
   const left=35+col*315;g.fillStyle=col===2?'#252a31':'white';g.fillRect(left,top,w*z,h*z);g.imageSmoothingEnabled=false;
   if(col===0)g.drawImage(ref,x,y,w,h,left,top,w*z,h*z);
   else g.drawImage(cand,(32+dx+x*factor)*1.5,(32+dy+y*factor)*1.5,w*factor*1.5,h*factor*1.5,left,top,w*z,h*z);
  }
 }
 fs.writeFileSync(path.join(dir,'junction-inspection.png'),c.toBuffer('image/png'));
 const masked=fs.existsSync(path.join(dir,'masked-head-225.png'));
 const head=await api.loadImage(path.join(dir,masked?'masked-head-225.png':'locked-head-225.png')),body=await api.loadImage(path.join(dir,'body-padded-375.png'));
 const hp=premultiply(head,api),bp=premultiply(body,api),s=new Float64Array(4),b=new Float64Array(4);
 const headOut=surface(api,144,(x,y)=>sample(hp,225,x*225/96-.5,y*225/96-.5,s),32);
 const bodyOut=surface(api,144,(x,y)=>sample(bp,375,(x+32)*225/96-.5,(y+32)*225/96-.5,b),32);
 const sheet=api.createCanvas(900,370),sg=sheet.getContext('2d');sg.fillStyle='#252a31';sg.fillRect(0,0,900,370);
 for(const [i,im,label]of [[0,headOut,masked?'White-masked head alone':'Unchanged head alone'],[1,bodyOut,'Body alone'],[2,cand,'Composite']]){
  sg.fillStyle='white';sg.font='18px sans-serif';sg.fillText(label,i*300+20,25);sg.imageSmoothingEnabled=false;
  const x=22,y=52,w=14,h=20;sg.drawImage(im,(32+dx+x*factor)*1.5,(32+dy+y*factor)*1.5,w*factor*1.5,h*factor*1.5,i*300+20,55,w*14,h*14);
 }
 fs.writeFileSync(path.join(dir,'left-junction-layers.png'),sheet.toBuffer('image/png'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

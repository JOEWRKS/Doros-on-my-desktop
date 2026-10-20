'use strict';
// Isolated original-key08 contour study. No transitions or runtime selection.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create}=require('./current-motion.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
// Observed visible stroke centers on original key08, source100 coordinates.
// The forepaw boundary is one connected valley, not two independent U shapes.
const outline=[['M',28,60],['C',28.7,65,28.9,71,31.4,73.5],
 ['C',32.2,74.5,33.3,75.3,34.8,75.7],
 ['C',34.4,80.2,34.7,86.2,35.8,90],
 ['C',36.8,93.8,39,96.3,42.4,96.3],
 ['C',46.5,96.3,47.8,93.1,49.1,89.7],
 ['C',49.2,85.4,50.1,83.9,53.8,84],
 ['C',56.2,84,57.5,87.3,58.5,90.8],
 ['C',59.6,94.6,61.1,96.4,64.4,96.4],
 ['C',69.2,96.4,71,94,71.8,90.4],
 ['C',72.5,84.3,72,77.8,71.4,72.7],
 ['C',70.6,66.5,68.9,60,66.6,54.1],
 ['C',57,47,35,48,28,60],['Z']];
const forepaws=[['M',34.8,75.7],['C',39.6,77,40.9,71.5,42,65.9],
 ['C',43.5,69.9,43.2,74.4,47.9,75.9],
 ['C',53.2,78.4,56.2,70.4,57.5,64.7]];
function trace(g,p){g.beginPath();for(const [op,...v]of p){if(op==='M')g.moveTo(...v);else if(op==='C')g.bezierCurveTo(...v);else if(op==='L')g.lineTo(...v);else g.closePath();}}
function pink(im,api){const d=pixels(im,api);let l=im.width,r=0,t=im.height,b=0;for(let y=0;y<im.height;y++)for(let x=0;x<im.width;x++){const i=(y*im.width+x)*4;if(d[i+3]>128&&d[i]>110&&d[i]-d[i+1]>22&&d[i]-d[i+2]>5){l=Math.min(l,x);r=Math.max(r,x);t=Math.min(t,y);b=Math.max(b,y);}}return [l,t,r,b];}
async function main(){
 const m=await create(),{api,root}=m.s,dir=path.join(root,'artifacts/grab-contour-still-20261017-r31');fs.mkdirSync(dir,{recursive:true});
 const original=await api.loadImage(path.join(root,'src/Dororong.App/Assets/user-body-drag/08.png'));
 const study=api.createCanvas(800,800),g=study.getContext('2d');g.fillStyle='white';g.fillRect(0,0,800,800);g.imageSmoothingEnabled=false;g.drawImage(original,40,40,700,700);
 g.font='12px sans-serif';for(let n=0;n<=100;n+=5){g.strokeStyle='rgba(0,100,200,.16)';g.beginPath();g.moveTo(40+7*n,40);g.lineTo(40+7*n,740);g.moveTo(40,40+7*n);g.lineTo(740,40+7*n);g.stroke();g.fillStyle='#225577';g.fillText(n,40+7*n,28);g.fillText(n,12,44+7*n);}
 fs.writeFileSync(path.join(dir,'original-coordinate-study.png'),study.toBuffer('image/png'));
 const head=m.s.rigs.open.parts.head,sp=pink(original,api),hp=pink(head,api),factor=(hp[2]-hp[0]+1)/(sp[2]-sp[0]+1)*96/225;
 const dx=(hp[0]+hp[2]+1)/2*96/225-(sp[0]+sp[2]+1)/2*factor,dy=hp[1]*96/225-sp[1]*factor;
 const pen=require('../PreviewLocomotion/original-contour-source.cjs').measure(m.s.measurement,api).ink;
 const body=api.createCanvas(375,375),bg=body.getContext('2d');
 bg.scale(225/96,225/96);bg.translate(32+dx,32+dy);bg.scale(factor,factor);
 bg.fillStyle='white';bg.strokeStyle=`rgb(${pen.rgb.join(',')})`;bg.lineWidth=pen.width/factor;bg.lineCap=bg.lineJoin='round';trace(bg,outline);bg.fill();bg.stroke();trace(bg,forepaws);bg.stroke();
 // Hard integer pixel ownership, not an antialiased clip/clear boundary.
 // Keep r30 body RGBA outside the edit; no seam blending or painted rectangle.
 bg.resetTransform();
 const prior=await api.loadImage(path.join(root,'artifacts/grab-contour-still-20261017-r30/body-padded-375.png'));
 const previous=pixels(prior,api),next=bg.getImageData(0,0,375,375);
 for(let y=0;y<375;y++)for(let x=0;x<375;x++){
  const sx=((x+.5)*96/225-32-dx)/factor,sy=((y+.5)*96/225-32-dy)/factor;
  if(sx>=46&&sx<=60&&sy>=81&&sy<=94)continue;
  const i=(y*375+x)*4;for(let k=0;k<4;k++)next.data[i+k]=previous[i+k];
 }
 bg.putImageData(next,0,0);
 const bd=premultiply(body,api),a=new Float64Array(4),b=new Float64Array(4);
 const candidate=surface(api,144,(x,y)=>{sample(bd,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(m.heads.open,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);},32);
 const save=(name,c)=>fs.writeFileSync(path.join(dir,name),c.toBuffer('image/png'));
 save('candidate.png',candidate);save('body-padded-375.png',body);save('locked-head-225.png',head);
 const sheet=api.createCanvas(900,700),sg=sheet.getContext('2d');sg.fillStyle='white';sg.fillRect(0,0,900,700);
 function drawPair(y,zoom){
  sg.imageSmoothingEnabled=false;
  // Same source-to-logical transform and uniform display scale for both sides.
  const scale=1.5*zoom,center=(hp[0]+hp[2]+1)/2*96/225,top=hp[1]*96/225;
  sg.drawImage(original,225+(dx-center)*scale,y+(dy-top)*scale,100*factor*scale,100*factor*scale);
  sg.drawImage(candidate,675+(-32-center)*scale,y+(-32-top)*scale,240*zoom,240*zoom);
 }
 drawPair(58,1);drawPair(310,2.5);sg.fillStyle='#302b33';sg.font='18px sans-serif';sg.fillText('Original grab 08',145,28);sg.fillText('Body contour candidate / locked head',505,28);sg.font='14px sans-serif';sg.fillText('Same head width; lower pair is 2.5x inspection only',270,282);save('comparison.png',sheet);
 const d=api.createCanvas(480,260),dg=d.getContext('2d');dg.fillStyle='#252a31';dg.fillRect(0,0,480,260);dg.drawImage(candidate,120,0);save('actual-size-dark.png',d);
 fs.writeFileSync(path.join(dir,'measurement.json'),JSON.stringify({status:'CANDIDATE_NOT_ACCEPTED',transitionConnected:false,runtimeSelected:false,sourcePinkBounds:sp,headPinkBounds:hp,sourceToLogical:{factor,dx,dy},pen,sourceSha:sha(fs.readFileSync(path.join(root,'src/Dororong.App/Assets/user-body-drag/08.png')))},null,2));
 console.log(JSON.stringify({dir,sourceToLogical:{factor,dx,dy},pen}));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={outline,forepaws,trace};

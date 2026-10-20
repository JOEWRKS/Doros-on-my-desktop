'use strict';
// Isolated still-only exterior-white mask. Never selected by the motion renderer.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample,over,surface}=require('./sampling.cjs');
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 const beforeDir=path.join(root,'artifacts/grab-contour-still-20261017-r31');
 const dir=path.join(root,'artifacts/grab-contour-still-20261017-r32');fs.mkdirSync(dir,{recursive:true});
 const load=f=>api.loadImage(f),head=await load(path.join(beforeDir,'locked-head-225.png'));
 const original=pixels(head,api),c=api.createCanvas(225,225),g=c.getContext('2d'),im=g.createImageData(225,225);im.data.set(original);
 const neutralWhite=i=>original[i+3]>0&&Math.min(...original.slice(i,i+3))>=210&&Math.max(...original.slice(i,i+3))-Math.min(...original.slice(i,i+3))<=12;
 // Exterior-connected white only. Dark/colored ink is a flood barrier.
 const seen=new Uint8Array(225*225),queue=[];
 function visit(x,y){if(x<0||y<0||x>=225||y>=225)return;const p=y*225+x,i=p*4;if(seen[p]||!(original[i+3]===0||neutralWhite(i)))return;seen[p]=1;queue.push(p);}
 for(let n=0;n<225;n++){visit(n,0);visit(n,224);visit(0,n);visit(224,n);}
 for(let n=0;n<queue.length;n++){const p=queue[n],x=p%225,y=Math.floor(p/225);visit(x-1,y);visit(x+1,y);visit(x,y-1);visit(x,y+1);}
 const changes=[];
 // Below the last dark/colored pixel in each column: face and ribbon interiors
 // above their lower ink boundary cannot be changed even if flood-connected.
 for(let x=28;x<=173;x++){
  let lastInk=-1;for(let y=100;y<190;y++){const i=(y*225+x)*4;if(original[i+3]>=32&&!neutralWhite(i))lastInk=y;}
  for(let y=Math.max(120,lastInk+1);y<190;y++){
   // Fixed conservative face rectangle. Even connected white at this boundary
   // is retained; only the exterior of this protected rectangle is eligible.
   if(x>=35&&x<=110&&y>=100&&y<=154)continue;
   const i=(y*225+x)*4;if(!seen[y*225+x]||!neutralWhite(i))continue;
   changes.push({x,y,rgba:[...original.slice(i,i+4)]});im.data[i+3]=0;
  }
 }
 g.putImageData(im,0,0);
 const maskView=api.createCanvas(900,560),mg=maskView.getContext('2d');mg.fillStyle='#252a31';mg.fillRect(0,0,900,560);mg.imageSmoothingEnabled=false;
 mg.drawImage(head,0,40,450,450);mg.drawImage(head,450,40,450,450);mg.fillStyle='#ff3030';for(const p of changes)mg.fillRect(450+p.x*2,40+p.y*2,2,2);
 mg.strokeStyle='#00ddee';mg.strokeRect(450+35*2,40+100*2,(110-35)*2,(154-100)*2);mg.fillStyle='white';mg.font='16px sans-serif';mg.fillText('Original head',20,25);mg.fillText('Red: removed white / cyan: face check region',470,25);
 fs.writeFileSync(path.join(dir,'mask-scope.png'),maskView.toBuffer('image/png'));
 const oldBody=await load(path.join(beforeDir,'body-padded-375.png'));
 const meta=JSON.parse(fs.readFileSync(path.join(beforeDir,'measurement.json'))),{factor,dx,dy}=meta.sourceToLogical;
 const {outline,forepaws,trace}=require('./grab-contour-still.cjs'),joined=outline.slice(0,-2).concat([['C',65.8,51.5,65,49,64,47],['C',54,43,34,45,28,60],['Z']]);
 const body=api.createCanvas(375,375),bg=body.getContext('2d');bg.scale(225/96,225/96);bg.translate(32+dx,32+dy);bg.scale(factor,factor);
 bg.fillStyle='white';bg.strokeStyle=`rgb(${meta.pen.rgb.join(',')})`;bg.lineWidth=meta.pen.width/factor;bg.lineCap=bg.lineJoin='round';trace(bg,joined);bg.fill();bg.stroke();trace(bg,forepaws);bg.stroke();bg.resetTransform();
 const bodyPixels=bg.getImageData(0,0,375,375),oldBodyPixels=pixels(oldBody,api);
 for(let y=0;y<375;y++)for(let x=0;x<375;x++){
  const sx=((x+.5)*96/225-32-dx)/factor,sy=((y+.5)*96/225-32-dy)/factor;
  if(sx>=61&&sx<=70&&sy>=45&&sy<=59)continue;
  const i=(y*375+x)*4;for(let k=0;k<4;k++)bodyPixels.data[i+k]=oldBodyPixels[i+k];
 }bg.putImageData(bodyPixels,0,0);
 const bp=premultiply(body,api),hp=premultiply(c,api),a=new Float64Array(4),b=new Float64Array(4);
 const candidate=surface(api,144,(x,y)=>{sample(bp,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);},32);
 const save=(f,im)=>fs.writeFileSync(path.join(dir,f),im.toBuffer('image/png'));
 save('candidate.png',candidate);save('masked-head-225.png',c);
 fs.copyFileSync(path.join(beforeDir,'locked-head-225.png'),path.join(dir,'locked-head-225.png'));
 save('body-padded-375.png',body);
 fs.writeFileSync(path.join(dir,'measurement.json'),JSON.stringify({...meta,revision:'r32',headMask:'masked-head-225.png',bodyJoinSourceBounds:[61,45,70,59]},null,2));
 const sheet=api.createCanvas(960,640),sg=sheet.getContext('2d');
 const prev=await load(path.join(beforeDir,'candidate.png'));
 for(const [col,bg]of [[0,'white'],[1,'#252a31']]){sg.fillStyle=bg;sg.fillRect(col*480,0,480,640);sg.fillStyle=col?'white':'#222';sg.font='18px sans-serif';sg.fillText('Before',col*480+65,30);sg.fillText('Exterior-white mask',col*480+250,30);sg.drawImage(prev,col*480+5,20);sg.drawImage(candidate,col*480+235,20);sg.imageSmoothingEnabled=false;sg.drawImage(candidate,60,125,90,80,col*480+35,280,405,360);}
 save('white-dark-comparison.png',sheet);
 const dark=api.createCanvas(480,260),dg=dark.getContext('2d');dg.fillStyle='#252a31';dg.fillRect(0,0,480,260);dg.drawImage(candidate,120,0);save('actual-size-dark.png',dark);
 const check=pixels(await load(path.join(dir,'masked-head-225.png')),api);let changedInk=0;for(let i=0;i<check.length;i+=4){if(!neutralWhite(i)&&[0,1,2,3].some(k=>check[i+k]!==original[i+k]))changedInk++;}
 assert.equal(changedInk,0,'non-white head pixels changed');
 const report={status:'CANDIDATE_REQUIRES_VISUAL_REVIEW',transitionConnected:false,runtimeSelected:false,removedWhitePixels:changes.length,changedNonWhiteHeadPixels:changedInk,changes,files:{}};
 for(const f of ['candidate.png','masked-head-225.png','body-padded-375.png','white-dark-comparison.png'])report.files[f]=hash(fs.readFileSync(path.join(dir,f)));
 fs.writeFileSync(path.join(dir,'mask-check.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({dir,removedWhitePixels:changes.length,changedInk}));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

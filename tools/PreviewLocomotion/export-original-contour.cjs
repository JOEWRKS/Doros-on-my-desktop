'use strict';
// Static review artifacts only. Never writes source PNGs, product banks or app.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {measure,extractHead}=require('./original-contour-source.cjs');
const {buildParts,renderPart}=require('./original-contour-art.cjs');
const {getPose}=require('./original-contour-poses.cjs');
const {createRenderer}=require('./original-contour-render.cjs');
const backgrounds=['#ffffff','#202630'];
const hash=file=>crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');
const pixels=c=>c.getContext('2d').getImageData(0,0,c.width,c.height).data;
function contourDistance(image,m){
  const d=pixels(image),mask=canvas.createCanvas(96,96).getContext('2d'),points=[];
  mask.beginPath();for(const c of m.headMask){if(c.op==='M')mask.moveTo(...c.points[0]);else if(c.op==='L')mask.lineTo(...c.points[0]);else mask.closePath();}
  const a=(x,y)=>x<0||y<0||x>=96||y>=96?0:d[(y*96+x)*4+3];
  for(let y=0;y<96;y++)for(let x=0;x<96;x++)if(a(x,y)>=128&&!mask.isPointInPath(x+.5,y+.5)&&[[1,0],[-1,0],[0,1],[0,-1]].some(([dx,dy])=>a(x+dx,y+dy)<128))points.push([x+.5,y+.5]);
  const directed=(a,b)=>a.length&&b.length?Math.max(...a.map(p=>Math.min(...b.map(q=>Math.hypot(p[0]-q[0],p[1]-q[1]))))):Infinity;
  return Math.max(directed(points,m.bodyBoundary),directed(m.bodyBoundary,points));
}
function comparisons(images,scale){
  const cell=96*scale,rowHeight=cell+22,c=canvas.createCanvas(cell*4,rowHeight*2),ctx=c.getContext('2d');
  for(let row=0;row<2;row++)for(let col=0;col<4;col++){
    const x=col*cell,y=row*rowHeight;ctx.fillStyle=backgrounds[row];ctx.fillRect(x,y,cell,rowHeight);
    ctx.imageSmoothingEnabled=false;ctx.drawImage(images[col],x,y,cell,cell);
    ctx.fillStyle=row?'#e8e8ec':'#49454a';ctx.font='11px Arial';ctx.textAlign='center';ctx.fillText(['Stand: source','Stand: new','Sit: reference','Sit: new'][col],x+cell/2,y+cell+15);
  }return c;
}
function jointSheet(renderer,m){
  const c=canvas.createCanvas(576,288),ctx=c.getContext('2d');ctx.fillStyle='#202630';ctx.fillRect(0,0,c.width,c.height);ctx.imageSmoothingEnabled=false;
  for(const [col,name] of ['standing','seated'].entries()){
    const p=getPose(name,m);ctx.drawImage(renderer.render(p,3),col*288,0);ctx.save();ctx.translate(col*288,0);ctx.scale(3,3);ctx.lineWidth=.6;
    for(const l of p.legs){
      ctx.strokeStyle=l.name==='nearHind'?'#cb8eee':l.name==='nearFront'?'#31ca9b':'#55aeda';ctx.fillStyle=ctx.strokeStyle;
      ctx.beginPath();l.joints.forEach((q,n)=>n?ctx.lineTo(...q):ctx.moveTo(...q));ctx.stroke();
      for(const q of l.joints){ctx.beginPath();ctx.arc(...q,.75,0,Math.PI*2);ctx.fill();}
      ctx.strokeStyle='#f4bd5a';ctx.beginPath();ctx.moveTo(l.sole[0]-1,l.sole[1]);ctx.lineTo(l.sole[0]+1,l.sole[1]);ctx.stroke();
    }ctx.restore();
  }return c;
}
async function exportStatic(outDir){
  if(typeof outDir!=='string'||!outDir.trim())throw Error('output directory required');outDir=path.resolve(outDir);
  const sourcePath=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'),referencePath=path.join(__dirname,'assets/seated-final-10-2.png');
  const source=await canvas.loadImage(sourcePath),m=measure(source,canvas),r=createRenderer(source,m,canvas);
  const standing=getPose('standing',m),seated=getPose('seated',m),a=r.render(standing),b=r.render(seated);
  const original=canvas.createCanvas(96,96);original.getContext('2d').drawImage(source,0,0);
  const reference=await canvas.loadImage(referencePath),aligned=canvas.createCanvas(96,96),rc=aligned.getContext('2d'),alignment=m.metadata.referenceAlignment;
  rc.translate(...alignment.translation);rc.rotate(alignment.rotation);rc.drawImage(reference,0,0); // rigid, no scaling
  const parts=canvas.createCanvas(1152,576),pc=parts.getContext('2d');pc.fillStyle='#202630';pc.fillRect(0,0,parts.width,parts.height);pc.imageSmoothingEnabled=false;
  let n=0;for(const pose of [standing,seated])for(const part of buildParts(m,pose)){
    const x=n%4*288,y=Math.floor(n/4)*288;pc.drawImage(renderPart(part,m,canvas),x,y,288,288);n++;
  }
  const headA=pixels(extractHead(source,m,canvas)),headB=pixels(r.renderHead(standing));let headDifferences=0;
  for(let i=0;i<headA.length;i+=4)if(headA.subarray(i,i+4).some((v,n)=>v!==headB[i+n]))headDifferences++;
  const coverage=[];for(const pose of [standing,seated]){const image=r.render(pose).getContext('2d');for(const part of buildParts(m,pose))for(const q of part.coverage)coverage.push(image.getImageData(Math.floor(q[0]),Math.floor(q[1]),1,1).data[3]);}
  const verification={purpose:'original-contour complete artwork; static endpoint review, not animation or product integration',animationGenerated:false,visualAcceptance:'pending',backgrounds,
    sourceSha256:hash(sourcePath),referenceSha256:hash(referencePath),nativeSize:[96,96],enlargement:3,referenceAlignment:alignment,
    checks:{standingContourDistance:contourDistance(a,m),headDifferingTexels:headDifferences,minimumSocketAlpha:Math.min(...coverage)},
    measuredStyle:{ink:m.ink,shaftWidths:m.legWidths},poses:{standing,seated},
    method:'closed original-derived curves; complete hidden skin; two anatomical segments; explicit depth; hidden rear-root ink clipping only',
    limits:['Technical checks are not user visual acceptance','Seated reference is comparison-only','No interpolation, animation, bank replacement or installation'],
    toolHashes:Object.fromEntries(['original-contour-source.cjs','original-contour-data.json','original-contour-art.cjs','original-contour-poses.cjs','original-contour-render.cjs','export-original-contour.cjs'].map(f=>[f,hash(path.join(__dirname,f))]))};
  if(!Number.isFinite(verification.checks.standingContourDistance))throw Error('empty standing body contour');
  fs.mkdirSync(outDir,{recursive:true});const files=[];
  const write=(name,c)=>{fs.writeFileSync(path.join(outDir,name),c.toBuffer('image/png'));files.push(name);};
  write('standing.png',a);write('seated.png',b);write('comparison-native.png',comparisons([original,a,aligned,b],1));write('comparison-3x.png',comparisons([original,a,aligned,b],3));
  write('parts.png',parts);write('joint-overlay.png',jointSheet(r,m));
  fs.writeFileSync(path.join(outDir,'verification.json'),JSON.stringify(verification,null,2));files.push('verification.json');return {outDir,files};
}
if(require.main===module){
  const args=process.argv.slice(2),legacy=args[0]==='--legacy-contour';
  if(legacy)args.shift();
  if(args.length>1||args.some(a=>a.startsWith('--'))){console.error('static export accepts only an output-directory argument; no video flags');process.exitCode=1;}
  else {
    // The traced endpoint was rejected. Normal exports now use the user's
    // explicitly requested original-image cutouts; retain the old study opt-in.
    const exportCurrent=legacy?exportStatic:require('./export-source-cut-artwork.cjs').exportArtwork;
    exportCurrent(args[0]||path.join(__dirname,legacy?'../../artifacts/original-contour-static-20261001':'../../artifacts/source-cut-artwork-20261001')).then(r=>console.log(JSON.stringify(r))).catch(e=>{console.error(e);process.exitCode=1;});
  }
}
module.exports={exportStatic};

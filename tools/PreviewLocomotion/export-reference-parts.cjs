'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {create,paths}=require('./reference-parts.cjs');
const hash=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
const stages=[0,.3,.5,1];
const svgPath=p=>p.map(c=>c[0]+c.slice(1).map(n=>Number(n.toFixed(4))).join(' ')).join(' ');
function sheet(images,scale=3){
  const size=96*scale,c=canvas.createCanvas(images.length*size,2*size),ctx=c.getContext('2d');ctx.imageSmoothingEnabled=false;
  for(let row=0;row<2;row++){
    ctx.fillStyle=row?'#202630':'#fff';ctx.fillRect(0,row*size,c.width,size);
    images.forEach((im,col)=>ctx.drawImage(im,col*size,row*size,size,size));
  }
  return c;
}
async function main(){
  const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/reference-parts-20261001'));
  fs.mkdirSync(path.join(out,'parts'),{recursive:true});
  const sourcePath=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'),referencePath=path.join(__dirname,'assets/seated-final-10-2.png');
  const source=await canvas.loadImage(sourcePath),r=create(source,canvas),reference=canvas.createCanvas(96,96);
  reference.getContext('2d').drawImage(await canvas.loadImage(referencePath),3,-12);
  const original=canvas.createCanvas(96,96);original.getContext('2d').drawImage(source,0,0);
  const frames=stages.map(t=>r.render(r.pose(t)));
  const write=(f,c)=>fs.writeFileSync(path.join(out,f),c.toBuffer('image/png'));
  write('four-poses.png',sheet(frames));write('reference-comparison.png',sheet([original,frames[0],reference,frames[3]]));
  write('standing.png',frames[0]);write('seated.png',frames[3]);write('layer-order.png',sheet(stages.map(t=>r.render(r.pose(t),{colors:true}))));
  write('mid-drop.png',sheet([r.render(r.pose(76/120)),r.render(r.pose(76/120),{colors:true})],6));
  const separate=[];
  for(const name of r.depth){
    const p=paths(name,r.pose(0)),c=canvas.createCanvas(96,96);require('./reference-parts.cjs').paint(c.getContext('2d'),p);separate.push(c);
    write(`parts/${name}.png`,c);
    fs.writeFileSync(path.join(out,`parts/${name}.svg`),`<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 96 96"><path d="${svgPath(p.fill)}" fill="white"/>${[...p.rootInk,...p.ink].map(ink=>`<path d="${svgPath(ink)}" fill="none" stroke="#443438" stroke-width="1.25" stroke-linecap="round" stroke-linejoin="round"/>`).join('')}</svg>`);
  }
  write('complete-parts.png',sheet(separate,2));write('parts/head.png',r.head);
  // Overlay uses colour only for checking source alignment, never for animation.
  const overlays=[];
  for(const [a,b] of [[original,frames[0]],[reference,frames[3]]]){
    const c=canvas.createCanvas(96,96),ctx=c.getContext('2d');ctx.globalAlpha=.5;ctx.drawImage(a,0,0);ctx.drawImage(b,0,0);overlays.push(c);
  }
  write('source-overlay.png',sheet(overlays));
  const bones=canvas.createCanvas(1152,288),bc=bones.getContext('2d');bc.fillStyle='#202630';bc.fillRect(0,0,bones.width,bones.height);bc.imageSmoothingEnabled=false;
  for(let i=0;i<4;i++){
    const p=r.pose(stages[i]);bc.drawImage(frames[i],i*288,0,288,288);bc.save();bc.translate(i*288,0);bc.scale(3,3);bc.lineWidth=.55;
    for(const [index,l] of p.legs.entries()){
      bc.strokeStyle=['#53a8f0','#28be95','#d294f1'][index];bc.fillStyle=bc.strokeStyle;bc.beginPath();l.joints.forEach((q,n)=>n?bc.lineTo(...q):bc.moveTo(...q));bc.stroke();
      for(const q of l.joints){bc.beginPath();bc.arc(...q,.7,0,Math.PI*2);bc.fill();}
    }
    bc.restore();
  }
  write('joints.png',bones);
  if(process.argv.includes('--frames')){
    fs.mkdirSync(path.join(out,'frames'),{recursive:true});
    for(let i=0;i<240;i++){
      const seconds=i/60,t=Math.max(0,Math.min(1,(seconds-.55)/1.05)),f=r.render(r.pose(t));
      const c=canvas.createCanvas(576,288),ctx=c.getContext('2d');ctx.imageSmoothingEnabled=false;
      for(let col=0;col<2;col++){ctx.fillStyle=col?'#202630':'#fff';ctx.fillRect(col*288,0,288,288);ctx.drawImage(f,col*288,0,288,288);}
      write(`frames/${String(i).padStart(4,'0')}.png`,c);
    }
  }
  fs.writeFileSync(path.join(out,'rig.json'),JSON.stringify({sourceHashes:{standing:hash(sourcePath),seated:hash(referencePath)},depth:[...r.depth,'head'],rest:r.rest,poses:stages.map(r.pose),method:'Independent closed source-aligned artwork; rigid chest/haunch; two-segment legs; no composite retrace',visualAcceptance:'not claimed'},null,2));
  console.log(out);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

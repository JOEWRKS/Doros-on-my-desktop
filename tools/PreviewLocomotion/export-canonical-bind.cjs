'use strict';
// Static review only. No source art, animation bank or installed app is written.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {create}=require('./canonical-bind.cjs');
const hash=file=>crypto.createHash('sha256').update(fs.readFileSync(file)).digest('hex');
async function main(){
  const out=path.resolve(process.argv[2]||path.join(__dirname,'../../artifacts/canonical-bind-20261001'));
  if(process.argv.slice(2).some(arg=>arg.startsWith('--')))throw Error('Only an output-directory argument is supported');
  fs.mkdirSync(out,{recursive:true});
  const source=path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'),original=await canvas.loadImage(source);
  const bind=create(original,canvas),rest=bind.render(),sourceCanvas=canvas.createCanvas(96,96);sourceCanvas.getContext('2d').drawImage(original,0,0);
  const a=sourceCanvas.getContext('2d').getImageData(0,0,96,96).data,b=rest.getContext('2d').getImageData(0,0,96,96).data;
  let differingPixels=0,maximumChannelDelta=0;
  for(let i=0;i<a.length;i+=4){let differs=false;for(let n=0;n<4;n++){const d=Math.abs(a[i+n]-b[i+n]);differs||=d>0;maximumChannelDelta=Math.max(maximumChannelDelta,d);}differingPixels+=Number(differs);}
  if(differingPixels)throw Error(`Rest drawing changed ${differingPixels} source pixels`);
  const bones=canvas.createCanvas(384,384),bc=bones.getContext('2d');bc.drawImage(bind.render({scale:4}),0,0);bc.scale(4,4);
  const colors={farFront:'#358bb2',nearFront:'#12987c',nearHind:'#9851ad',farHind:'#9851ad'};
  function chain(points,color,dashed=false){
    bc.save();bc.lineWidth=.65;bc.strokeStyle=color;bc.fillStyle=color;bc.setLineDash(dashed?[1.2,1]:[]);
    bc.beginPath();points.forEach((p,n)=>n?bc.lineTo(...p):bc.moveTo(...p));bc.stroke();bc.setLineDash([]);
    for(const p of points){bc.beginPath();bc.arc(...p,.85,0,Math.PI*2);bc.fill();}bc.restore();
  }
  chain([bind.rig.torso.chest,bind.rig.torso.pelvis],'#c89020');
  chain([bind.rig.torso.chest,bind.rig.head.pivot],'#d46072');
  // The occluded hindleg is recorded in rig.json, not painted as a fourth paw.
  for(const l of bind.rig.legs.filter(l=>l.visible))chain(l.joints,colors[l.name],!l.front||l.name==='farFront');
  const sheet=canvas.createCanvas(1152,848),ctx=sheet.getContext('2d');
  for(let row=0;row<2;row++)for(let col=0;col<3;col++){
    const x=col*384,y=row*424;ctx.fillStyle=row?'#202630':'#f3f0e9';ctx.fillRect(x,y,384,424);
    ctx.imageSmoothingEnabled=false;ctx.drawImage([sourceCanvas,rest,bones][col],x,y,384,384);
    ctx.font='16px Arial';ctx.fillStyle=row?'#e5e8eb':'#45434a';ctx.textAlign='center';
    ctx.fillText(['Original','Original-bound artwork','Joint placement'][col],x+192,y+406);
  }
  const ownership=canvas.createCanvas(384,384),oc=ownership.getContext('2d'),palette={head:'#dfa0b8',torso:'#f2b360',nearFront:'#41c9a4',farFront:'#69a7c3',nearHind:'#a875c4'};
  for(let y=0;y<96;y++)for(let x=0;x<96;x++){const owner=bind.ownerAt(x,y);if(owner){oc.fillStyle=palette[owner];oc.globalAlpha=a[(y*96+x)*4+3]/255;oc.fillRect(x*4,y*4,4,4);}}
  const write=(name,c)=>fs.writeFileSync(path.join(out,name),c.toBuffer('image/png'));
  write('comparison.png',sheet);write('standing.png',bind.render({scale:4}));write('joint-overlay.png',bones);write('paint-ownership.png',ownership);
  for(const l of bind.layers)write(`layer-${l.name}.png`,bind.renderLayer(l.name));
  fs.writeFileSync(path.join(out,'rig.json'),JSON.stringify(bind.rig,null,2));
  fs.writeFileSync(path.join(out,'verification.json'),JSON.stringify({purpose:'original-first rest artwork and internal joint binding; static review, NOT final sitting animation',sourceSha256:hash(source),differingPixels,maximumChannelDelta,canvas:[96,96],visiblePaintLayers:bind.layers.map(l=>l.name),occludedFarHind:'no duplicated source paint',limits:['No hidden under-head/torso paint invented','Animated attachment seams and sitting endpoint remain to be built','Installed app and product banks unchanged'],inputHashes:Object.fromEntries(['canonical-bind.cjs','export-canonical-bind.cjs'].map(name=>[name,hash(path.join(__dirname,name))]))},null,2));
  console.log(JSON.stringify({out,differingPixels,maximumChannelDelta}));
}
main().catch(error=>{console.error(error);process.exitCode=1;});

'use strict';
// Every distinct animation frame, in playback order. Holds duplicate the ends.
const fs=require('node:fs'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {create}=require('./reference-parts.cjs');
async function main(){
  const label=process.argv[2]||'after',out=path.join(__dirname,'../../artifacts/reference-parts-20261001',`outline-audit-${label}`);
  fs.mkdirSync(out,{recursive:true});
  const r=create(await canvas.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png')),canvas);
  for(const dark of [false,true])for(let page=0;page<4;page++){
    const c=canvas.createCanvas(1024,832),ctx=c.getContext('2d');ctx.imageSmoothingEnabled=false;
    ctx.fillStyle=dark?'#202630':'#fff';ctx.fillRect(0,0,c.width,c.height);
    for(let j=0;j<16;j++){
      const n=page*16+j,p=r.pose(n/63),f=r.render(p),x=j%4*256,y=Math.floor(j/4)*208;
      ctx.drawImage(f,13,44,64,47,x,y+20,256,188);
      ctx.fillStyle=dark?'#ccc':'#555';ctx.font='13px sans-serif';ctx.fillText(`video ${n+33} / t=${(n/63).toFixed(3)}`,x+5,y+15);
      ctx.strokeStyle='#ddd';ctx.strokeRect(x,y,256,208);
    }
    fs.writeFileSync(path.join(out,`${dark?'dark-':''}page-${page+1}.png`),c.toBuffer('image/png'));
  }
  if(label==='after'){
    const c=canvas.createCanvas(768,452),ctx=c.getContext('2d');ctx.fillStyle='#fff';ctx.fillRect(0,0,c.width,c.height);
    for(const [row,version] of ['before','after'].entries()){
      ctx.fillStyle='#444';ctx.font='14px sans-serif';ctx.fillText(version,8,row*226+15);
      for(const [col,n] of [13,25,43].entries()){
        const f=path.join(out,'..',`outline-audit-${version}`,`page-${Math.floor(n/16)+1}.png`);
        if(!fs.existsSync(f))continue;
        const im=await canvas.loadImage(f),j=n%16;
        ctx.drawImage(im,j%4*256,Math.floor(j/4)*208,256,208,col*256,row*226+18,256,208);
      }
    }
    fs.writeFileSync(path.join(out,'../outline-before-after.png'),c.toBuffer('image/png'));
  }
  console.log(out);
}
main().catch(e=>{console.error(e);process.exitCode=1;});

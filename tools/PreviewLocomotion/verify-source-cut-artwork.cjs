'use strict';
const test=require('node:test'),assert=require('node:assert/strict');
const fs=require('node:fs'),os=require('node:os'),path=require('node:path');
const {execFileSync}=require('node:child_process');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const rgba=c=>c.getContext('2d').getImageData(0,0,96,96).data;
test('default artwork export preserves both original drawings and reconstructs them from cropped layers',async()=>{
  const out=fs.mkdtempSync(path.join(os.tmpdir(),'dororong-source-art-'));
  execFileSync(process.execPath,[path.join(__dirname,'export-original-contour.cjs'),out]);
  for(const [pose,file,offset] of [
    ['standing','../../src/Dororong.App/Assets/dororong-canonical.png',[0,0]],
    ['seated','assets/seated-final-10-2.png',[3,-12]]
  ]){
    const expected=canvas.createCanvas(96,96),actual=canvas.createCanvas(96,96);
    expected.getContext('2d').drawImage(await canvas.loadImage(path.join(__dirname,file)),...offset);
    actual.getContext('2d').drawImage(await canvas.loadImage(path.join(out,pose+'.png')),0,0);
    let changed=0;const a=rgba(expected),b=rgba(actual);
    for(let i=0;i<a.length;i+=4)if(a.subarray(i,i+4).some((v,n)=>v!==b[i+n]))changed++;
    assert.equal(changed,0,`${pose}: source silhouette, ink and colour must not be redrawn`);
    const manifest=JSON.parse(fs.readFileSync(path.join(out,'parts.json'),'utf8'))[pose];
    const assembled=canvas.createCanvas(96,96),ctx=assembled.getContext('2d');
    assert.equal(manifest.layers.length,5);
    const owners=new Uint8Array(96*96);
    for(const layer of manifest.layers){
      const image=await canvas.loadImage(path.join(out,layer.file));
      ctx.drawImage(image,...layer.offset);
      const c=canvas.createCanvas(image.width,image.height);c.getContext('2d').drawImage(image,0,0);
      const d=c.getContext('2d').getImageData(0,0,c.width,c.height).data;
      for(let y=0;y<c.height;y++)for(let x=0;x<c.width;x++)if(d[(y*c.width+x)*4+3])owners[(y+layer.offset[1])*96+x+layer.offset[0]]++;
    }
    assert.deepEqual(rgba(assembled),a,`${pose}: cut PNG files must reassemble exactly`);
    for(let i=0;i<owners.length;i++)assert.equal(owners[i],a[i*4+3]?1:0,'no missing or duplicated visible texel');
  }
});

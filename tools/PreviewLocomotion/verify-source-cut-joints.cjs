'use strict';
const test=require('node:test'),assert=require('node:assert/strict'),path=require('node:path');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {cutArtwork}=require('./source-cut-artwork.cjs');
const pixels=c=>c.getContext('2d').getImageData(0,0,96,96).data;
function outside(d){
  const visited=new Uint8Array(96*96),queue=[0];visited[0]=1;
  for(let i=0;i<queue.length;i++){
    const p=queue[i],x=p%96,y=Math.floor(p/96);
    for(const [dx,dy] of [[1,0],[-1,0],[0,1],[0,-1]]){
      const nx=x+dx,ny=y+dy,q=ny*96+nx;
      if(nx<0||ny<0||nx>=96||ny>=96||visited[q]||d[q*4+3]>=128)continue;
      visited[q]=1;queue.push(q);
    }
  }
  return visited;
}
function outline(d){
  const exterior=outside(d),result=[];
  for(let y=1;y<95;y++)for(let x=1;x<95;x++){
    const p=y*96+x;
    if(d[p*4+3]>=128&&[p-1,p+1,p-96,p+96].some(n=>exterior[n]))result.push([x,y]);
  }
  return result;
}
const samples=[];
for(const head of [-8,0,8])for(const farFront of [-12,0,12])for(const nearFront of [-12,0,12])for(const nearHind of [-18,0,18])samples.push({head,farFront,nearFront,nearHind});
for(const [pose,file] of [['standing','../../src/Dororong.App/Assets/dororong-canonical.png'],['seated','assets/seated-final-10-2.png']]){
  test(`${pose}: complete joint skin preserves rest drawing and covers attachment axes across supported angles`,async()=>{
    const cut=cutArtwork(await canvas.loadImage(path.join(__dirname,file)),pose,canvas);
    assert.equal(typeof cut.renderArticulated,'function','moving artwork needs hidden attachment skin, not bare cropped parts');
    assert.deepEqual(pixels(cut.render()),pixels(cut.original),'exposed source artwork must remain unchanged');
    // Hidden white skin legitimately backs the source's semi-transparent ink.
    // Compare the visible result over white, rather than demanding its original
    // partial alpha even where the source had transparent internal ink strokes.
    const matte=c=>{const o=canvas.createCanvas(96,96),ctx=o.getContext('2d');ctx.fillStyle='#fff';ctx.fillRect(0,0,96,96);ctx.drawImage(c,0,0);return pixels(o);};
    const original=matte(cut.original),rest=matte(cut.renderArticulated());
    assert.ok(rest.every((v,i)=>Math.abs(v-original[i])<=1),'hidden skin must not change the rest drawing on white');
    const before=outline(pixels(cut.original)),after=outline(pixels(cut.renderTraced()));
    const distance=(a,b)=>Math.max(...a.map(p=>Math.min(...b.map(q=>Math.hypot(p[0]-q[0],p[1]-q[1])))));
    const contourDistance=Math.max(distance(before,after),distance(after,before));
    const worst=before.map(p=>({p,d:Math.min(...after.map(q=>Math.hypot(p[0]-q[0],p[1]-q[1])))})).sort((a,b)=>b.d-a.d).slice(0,4);
    // The seated raster contains partial-alpha gaps in its paw ink (notably
    // x26..31/y80..87). Continuous opaque skin bridges those subpixel gaps;
    // keep that measured deviation bounded rather than claiming RGBA identity.
    assert.ok(contourDistance<=2,`traced rest silhouette exceeds two source pixels: ${contourDistance} ${JSON.stringify(worst)}`);
    const owners=new Array(96*96),source=pixels(cut.original),seams=[];
    for(const layer of cut.layers){
      const d=layer.image.getContext('2d').getImageData(0,0,layer.image.width,layer.image.height).data;
      for(let y=0;y<layer.image.height;y++)for(let x=0;x<layer.image.width;x++)if(d[(y*layer.image.width+x)*4+3])owners[(y+layer.offset[1])*96+x+layer.offset[0]]=layer.name;
    }
    for(let y=58;y<95;y++)for(let x=0;x<95;x++)for(const [dx,dy] of [[1,0],[0,1]]){
      const a=y*96+x,b=(y+dy)*96+x+dx;
      if(source[a*4+3]>=250&&source[b*4+3]>=250&&owners[a]!==owners[b])seams.push([[x+.5,y+.5,owners[a]],[x+dx+.5,y+dy+.5,owners[b]]]);
    }
    for(const angles of samples){
      const d=pixels(cut.renderTraced(angles));
      const exterior=outside(d);
      for(let y=68;y<90;y++)for(let x=16;x<67;x++)assert.ok(d[(y*96+x)*4+3]>0||exterior[y*96+x],`trapped transparent body hole @${x},${y}`);
      for(const p of Object.values(cut.pivots))for(let dy=-1;dy<=1;dy++)for(let dx=-1;dx<=1;dx++){
        const x=Math.round(p[0])+dx,y=Math.round(p[1])+dy;
        assert.ok(d[(y*96+x)*4+3]>=250,`open socket ${pose} ${JSON.stringify(angles)} @${x},${y}`);
      }
      const move=([x,y,name])=>{const p=cut.pivots[name]||[0,0],a=(angles[name]||0)*Math.PI/180,c=Math.cos(a),s=Math.sin(a);return [p[0]+(x-p[0])*c-(y-p[1])*s,p[1]+(x-p[0])*s+(y-p[1])*c];};
      for(const seam of seams){
        const [a,b]=seam.map(move);
        for(const t of [.25,.5,.75]){
          const x=Math.floor(a[0]+(b[0]-a[0])*t),y=Math.floor(a[1]+(b[1]-a[1])*t);
          // A rotated cut endpoint can land on an antialiased exterior pixel;
          // require coverage here, while the interior socket test above stays opaque.
          assert.ok(d[(y*96+x)*4+3]>0,`open cut seam ${pose} ${JSON.stringify(angles)} @${x},${y} ${JSON.stringify(seam)}`);
        }
      }
    }
    assert.throws(()=>cut.renderArticulated({head:20}),/range/,'unsupported angles must not silently render broken skin');
    assert.throws(()=>cut.renderArticulated({nearFront:NaN}),/finite/);
  });
}
module.exports={samples};

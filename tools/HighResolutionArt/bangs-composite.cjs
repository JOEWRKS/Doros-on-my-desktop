'use strict';
const {pixels}=require('./sampling.cjs');
// User-approved deterministic extraction of r7, not a new generation.
// Source225 coordinates. Outer bob, crown silhouette, ornament and eyes excluded.
const polygon=[[44,84],[52,74],[62,64],[84,64],[96,71],[108,89],[115,108],
 [116,124],[103,127],[81,127],[76,127],[65,127],[56,123],[44,120],[29,119],[34,108],[39,96]];
const protectedRects=[[33,120,31,16],[78,128,31,17],[55,135,29,21]];
function makeMask(api){
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.fillStyle='#fff';g.beginPath();
 polygon.forEach(([x,y],i)=>i?g.lineTo(x,y):g.moveTo(x,y));g.closePath();g.fill();
 for(const r of protectedRects)g.clearRect(...r);return c;
}
function composite(base,donor,api,{maskOverride}={}){
 if(base.width!==225||base.height!==225||donor.width!==1254||donor.height!==1254)throw Error('Unexpected registered source dimensions');
 const out=api.createCanvas(225,225),g=out.getContext('2d');g.drawImage(base,0,0);
 const registered=api.createCanvas(225,225),rg=registered.getContext('2d');
 // Match the donor's left fringe foot39 to existing35, and right foot109 to109.
 const sx=74/70,dx=109-109*sx;
 rg.imageSmoothingEnabled=true;rg.imageSmoothingQuality='high';rg.drawImage(donor,dx,0,225*sx,225);
 const editMask=maskOverride||makeMask(api);
 const d=g.getImageData(0,0,225,225),source=pixels(registered,api),mask=pixels(editMask,api);
 const patch=api.createCanvas(225,225),pg=patch.getContext('2d'),pd=pg.createImageData(225,225);
 for(let i=0;i<d.data.length;i+=4){
  let w=mask[i+3]/255;if(!w)continue;
  const x=(i/4)%225,y=Math.floor(i/4/225);
  // Blend only inside the pink-hair seam. Keep forehead cleanup fully opaque.
  if(y<114){let distance=3;for(let dy=-2;dy<=2;dy++)for(let dx=-2;dx<=2;dx++){
   if(x+dx>=0&&x+dx<225&&y+dy>=0&&y+dy<225&&!mask[((y+dy)*225+x+dx)*4+3])distance=Math.min(distance,Math.hypot(dx,dy));
  }w*=Math.min(1,distance/2);}
  if(source[i+3]<230)throw Error('Donor patch has insufficient coverage');
  // Keep existing opacity; semi-transparent generated RGB must not dim the base.
  for(let k=0;k<3;k++){d.data[i+k]=Math.round(d.data[i+k]*(1-w)+source[i+k]*w);pd.data[i+k]=source[i+k];}
  pd.data[i+3]=Math.round(w*255);
 }
 g.putImageData(d,0,0);pg.putImageData(pd,0,0);return {image:out,registered,mask:editMask,patch};
}
module.exports={composite,makeMask,polygon,protectedRects};

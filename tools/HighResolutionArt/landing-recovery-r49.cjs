'use strict';
const path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs');
const {premultiply,sample,over,put,surface}=require('./sampling.cjs');
const old=require('./legacy-parity.cjs'),motion=require('./landing-motion.cjs');
const dir=path.join(root,'artifacts/landing-recovery-20261018-r49');
// SuppliedBodyDragFrames traverses 7 authored intervals and a final bridge,
// at linear release progress. The accepted high-res key01 already IS canonical;
// its final bridge is therefore identity, not a newly drawn extra pose.
function recovery(t,start=1){assert(Number.isFinite(t)&&t>=0&&Number.isFinite(start)&&start>=0&&start<=1);const p=Math.min(1,t/.18),remaining=start*7-p*(start*7+1);return {progress:Math.max(0,remaining/7),bridge:Math.max(0,-remaining),release:p};}
function state(t,angle,anchor,mirror=false,start=1){return {...old.state(t,angle,anchor,mirror),...recovery(t,start)};}
async function create(){
 const prior=await old.create(),entry=prior.entry,hp=premultiply(entry.head,api),spreadCache=new Map();
 function linear(data,size,x,y,out){const ix=Math.floor(x),iy=Math.floor(y),fx=x-ix,fy=y-iy;out.fill(0);for(let yy=0;yy<2;yy++)for(let xx=0;xx<2;xx++){const px=ix+xx,py=iy+yy;if(px<0||py<0||px>=size||py>=size)continue;const weight=(xx?fx:1-fx)*(yy?fy:1-fy),i=(py*size+px)*4;for(let k=0;k<4;k++)out[k]+=data[i+k]*weight;}return out;}
 const scratch=new Float64Array(4),nativeBody=premultiply(surface(api,96,(x,y)=>sample(entry.body(0),375,(x+32)*225/96-.5,(y+32)*225/96-.5,scratch),32),api);
 function legacySpread(amount){if(spreadCache.has(amount))return spreadCache.get(amount);const d=new Float64Array(160*160*4),p=new Float64Array(4);for(let y=0;y<160;y++)for(let x=0;x<160;x++){const ly=y-32,bx=motion.inverseX(x-32,ly,amount);linear(nativeBody,160,bx+32,y,p);d.set(p,(y*160+x)*4);}spreadCache.set(amount,d);return d;}
 function render(s,scale=1.5,coverage=8){
  if(!s.hard||!(s.spread>0))return prior.render(s,scale);
  const body=entry.body(s.progress),a=new Float64Array(4),b=new Float64Array(4),tr=old.transform({...s,angle:s.angle??0}),foot=prior.contact(s.spread),sx=1+s.squash*.5,sy=1-s.squash;
  const uncompress=(x,y)=>[foot.center+(x-foot.center)/sx,(s.y??0)+foot.sole+(y-(s.y??0)-foot.sole)/sy];
  const legacyBody=legacySpread(s.spread),low=new Float64Array(4),smooth=require('./grab-motion.cjs').smooth;
  function paint(x,y){[x,y]=tr.inverse(...uncompress(x,y));if(x< -32||x>128||y<0||y>116){a.fill(0);return a;}const bx=motion.inverseX(x,y,s.spread);sample(body,375,(bx+32)*225/96-.5,(y+32)*225/96-.5,a);
   // Match the existing row-warp + reconstruction footprint only in the
   // compressed paws. Gradually enter/leave it with spread; no frame switch,
   // added ink, source edits, or filtering of the head (bottom71.68 < gate74).
   const mix=smooth((y-74)/2)*smooth(s.spread);if(mix){linear(legacyBody,160,x+32-.5,y+32-.5,low);for(let k=0;k<4;k++)a[k]+=(low[k]-a[k])*mix;}
   sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);}
  // Integrate the compressed lower-body footprint instead of adding outline
  // pixels or blurring the output. Only pixels wholly below the head are refined.
  // Thus face/hair/tip sampling is byte-identical to r48 at the same pose.
  return surface(api,96*scale,paint,80,(data,w,h)=>{
   const sum=new Float64Array(4);
   for(let y=0;y<h;y++){
    const top=tr.inverse(...uncompress(48,y/scale-80))[1];if(top<74||top>112)continue;
    for(let x=0;x<w;x++){
     const lx=(x+.5)/scale-80;if(lx< -25||lx>130)continue;
     sum.fill(0);for(let yy=0;yy<coverage;yy++)for(let xx=0;xx<coverage;xx++){
      const p=paint((x+(xx+.5)/coverage)/scale-80,(y+(yy+.5)/coverage)/scale-80);for(let k=0;k<4;k++)sum[k]+=p[k]/(coverage*coverage);
     }put(data,(y*w+x)*4,sum);
    }
   }
  });
 }
 function hard(t,scale=1.5,coverage=8){const p=motion.extreme(t);return render({progress:0,angle:0,remaining:0,hard:true,squash:p.squash,spread:p.amount,y:-p.lift},scale,coverage);}
 return {render,hard,prior};
}
module.exports={create,state,recovery,dir};

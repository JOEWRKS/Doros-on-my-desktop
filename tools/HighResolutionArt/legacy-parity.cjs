'use strict';
const path=require('node:path'),assert=require('node:assert/strict');
const {api,root}=require('./bangs-composite-review.cjs'),{premultiply,sample,over,surface}=require('./sampling.cjs');
const {smooth}=require('./grab-motion.cjs'),legacy=require('./landing-motion.cjs');
const dir=path.join(root,'artifacts/legacy-parity-20261017-r48');
// Literal port of HeadSwingSession's bounded240Hz spring. Pointer units are
// screen pixels, not distance from the pet; no invented angle animation.
class Swing {
 constructor(){this.x=null;this.filtered=0;this.angle=0;this.velocity=0;}
 step(ms,x){assert(Number.isFinite(ms)&&Number.isFinite(x));if(this.x===null){this.x=x;return 0;}const seconds=ms/1000;if(seconds<=0||seconds>.25){this.x=x;this.filtered=0;if(seconds>.25)this.angle=this.velocity=0;return this.angle;}
  const speed=Math.max(-4000,Math.min(4000,(x-this.x)/seconds));this.x=x;const steps=Math.ceil(seconds*240),dt=seconds/steps,filter=1-Math.exp(-dt/.06);
  for(let i=0;i<steps;i++){this.filtered+=(speed-this.filtered)*filter;const target=Math.abs(this.filtered)<8?0:Math.max(-88,Math.min(88,this.filtered*.08));this.velocity+=(121*(target-this.angle)-2*.27*11*this.velocity)*dt;this.angle+=this.velocity*dt;if(Math.abs(this.angle)>88){this.angle=Math.max(-88,Math.min(88,this.angle));if(this.angle*this.velocity>0)this.velocity=0;}}
  if(Math.abs(this.filtered)<8&&Math.abs(this.angle)<.025&&Math.abs(this.velocity)<.25)this.angle=this.velocity=0;return this.angle;
 }
}
function state(t,angle,anchor,mirror=false){assert(t>=0&&t<=.44&&Math.abs(angle)<=88);const remaining=1-smooth(t/.18);return {progress:remaining,angle:angle*remaining,remaining,anchor,mirror,y:36*Math.min(1,t/.22)**2,squash:legacy.compression(t*1000)};}
function transform(s){
 const a=s.angle*Math.PI/180,c=Math.cos(a),n=Math.sin(a),f=s.mirror?-1:1,anchor=s.anchor??[48,48],remaining=s.remaining??1;
 const base=(x,y)=>[48+(x-48)*f*c-(y-48)*n,48+(x-48)*f*n+(y-48)*c];
 const g=base(...anchor),target=[48+(anchor[0]-48)*f,anchor[1]],dx=(target[0]-g[0])*remaining,dy=(target[1]-g[1])*remaining;
 return {forward:(x,y)=>{const p=base(x,y);return [p[0]+dx,p[1]+dy+(s.y??0)];},inverse:(x,y)=>{const xx=x-dx-48,yy=y-(s.y??0)-dy-48;return [48+f*(xx*c+yy*n),48-xx*n+yy*c];},target};
}
async function create(){
 const entry=await require('./grab-boundary-connected.cjs').create(),hp=premultiply(entry.head,api),body0=entry.body(0),contactCache=new Map();
 // Use occupied source cell corners and the lower2-DIP silhouette band, as
 // PlatformContactPresentation does, rather than an arbitrary centerX48.
 function contact(spread=0){if(contactCache.has(spread))return contactCache.get(spread);const factor=225/96;let bottom=-Infinity;const cells=[];
  for(let y=0;y<375;y++)for(let x=0;x<375;x++)if(body0[(y*375+x)*4+3]>=128){const x0=x/factor-32,y0=y/factor-32,x1=(x+1)/factor-32,y1=(y+1)/factor-32;cells.push([x0,y0,x1,y1]);bottom=Math.max(bottom,y1);}
  let left=Infinity,right=-Infinity;const map=(x,y)=>x+24*spread*smooth((y-66)/17)*(2*smooth((x-46)/14)-1);
  for(const [x0,y0,x1,y1]of cells)if(y1>bottom-2){for(const y of [Math.max(y0,bottom-2),y1])for(const x of [x0,x1]){const q=map(x,y);left=Math.min(left,q);right=Math.max(right,q);}}
  const r={left,right,sole:bottom,center:(left+right)/2};contactCache.set(spread,r);return r;
 }
 function render(s,scale=1.5){assert(scale>=1&&scale<=1.5&&s.progress>=0&&s.progress<=1&&Math.abs(s.angle??0)<=88);const body=entry.body(s.progress),a=new Float64Array(4),b=new Float64Array(4),tr=transform({...s,angle:s.angle??0});const compression=s.squash??0,spread=s.spread??0,foot=contact(spread),pivot=s.hard?[foot.center,foot.sole]:[48,87],sx=1+compression*(s.hard?.5:5/9),sy=1-compression;
  return surface(api,96*scale,(x,y)=>{
   // Impact begins after angle recovery. Keep the actual sole fixed.
   x=pivot[0]+(x-pivot[0])/sx;y=(s.y??0)+pivot[1]+(y-(s.y??0)-pivot[1])/sy;
   [x,y]=tr.inverse(x,y);if(x< -32||x>128||y<0||y>116){a.fill(0);return a;}const bx=legacy.inverseX(x,y,spread);
   sample(body,375,(bx+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);
  },80);
 }
 const hard=(t,scale=1.5)=>{const p=legacy.extreme(t);return render({progress:0,angle:0,remaining:0,hard:true,squash:p.squash,spread:p.amount,y:-p.lift},scale);};
 function opaqueAnchor(g){const x=Math.floor(g[0]*225/96),y=Math.floor(g[1]*225/96);return hp[(y*225+x)*4+3]>=128;}
 return {render,hard,contact,entry,opaqueAnchor};
}
module.exports={create,state,transform,Swing,dir};

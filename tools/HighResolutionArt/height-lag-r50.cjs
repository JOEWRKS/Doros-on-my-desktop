'use strict';
const path=require('node:path'),{api,root}=require('./bangs-composite-review.cjs');
const {surface,sample,premultiply,over}=require('./sampling.cjs'),{smooth}=require('./grab-motion.cjs');
const r49=require('./landing-recovery-r49.cjs'),r48=require('./legacy-parity.cjs');
const dir=path.join(root,'artifacts/height-lag-20261018-r50');
const clamp=(x,a,b)=>Math.max(a,Math.min(b,x));
// Literal free-fall step and impact curves from PlatformMotion, for a stationary floor.
function fallTrace(height,screenHeight=1000,dt=20){let y=0,v=0,hit=null;const rows=[];
 for(let ms=0;ms<=4000;ms+=dt){let age=-1,squash=0,spread=0,extreme=false,phase='Falling';
  if(hit===null){let remaining=ms===0?0:Math.min(.25,dt/1000);while(remaining>0){const h=Math.min(.008,remaining),nv=Math.min(1200,v+1800*h),ny=y+(v+nv)*.5*h;if(ny>=height){y=height;hit=ms;v=0;break;}y=ny;v=nv;remaining-=h;}}
  if(hit!==null){age=ms-hit;const t=age/1000;extreme=height+1e-7>=screenHeight*.8;phase=age>=(extreme?1800:450)?'Supported':'Landing';y=height;
   if(phase==='Landing'){if(extreme){const p=require('./landing-motion.cjs').extreme(t);squash=p.squash;spread=p.amount;y-=p.lift;}
    else{const strength=clamp(height/300,.12,1);squash=strength*(t<.08?.30*smooth(t/.08):t<.14?.30-.40*smooth((t-.08)/.06):t<.32?-.10*(1-smooth((t-.14)/.18)):t<.38?.08*smooth((t-.32)/.06):.08*(1-smooth((t-.38)/.07)));if(t>.08&&t<.32)y-=18*strength*Math.sin(Math.PI*(t-.08)/.24)**2;}}
   else extreme=false;
  }rows.push({ms,y,age,phase,squash,spread,extreme});if(phase==='Supported')break;
 }return rows;
}
function shortDrop(ms,distance,strength=1){return{y:distance*clamp(ms/220,0,1)**2,squash:require('./landing-motion.cjs').compression(ms,strength,distance)};}
class Lag{
 constructor(){this.near=[0,0];this.far=[0,0];this.prev=null;this.released=false;this.elapsed=0;}
 step(pointer,dt,settling){if(settling&&!this.released){this.released=true;this.n0=[...this.near];this.f0=[...this.far];dt=0;}
  if(this.released){this.elapsed=Math.min(200,this.elapsed+dt);const k=1-smooth(this.elapsed/200);this.near=this.n0.map(x=>x*k);this.far=this.f0.map(x=>x*k);}
  else{let target=this.prev&&dt>0&&dt<=250?pointer.map((x,i)=>(x-this.prev[i])*75/dt):[0,0];const len=Math.hypot(...target);if(len<.6)target=[0,0];else if(len>14)target=target.map(x=>x*14/len);const a=Math.exp(-dt/75),b=Math.exp(-dt/120),old=[...this.near];this.near=target.map((x,i)=>x+(old[i]-x)*a);this.far=target.map((x,i)=>x+(this.far[i]-x)*b+(old[i]-x)*75/(75-120)*(a-b));}
  this.prev=[...pointer];return this;
 }
}
const bump=(p,a,b,c,d)=>smooth((p-a)/(b-a))*(1-smooth((p-c)/(d-c)));
function field(p,center,grab,near,far){const [x,y]=p,[cx,cy]=center,pin=smooth((Math.hypot(x-grab[0],y-grab[1])-3)/9),sole=1-smooth((y-79)/5);if(!pin||!sole)return[0,0];const face=bump(x,cx-31,cx-23,cx+6,cx+13)*bump(y,cy+3,cy+8,cy+24,cy+31),facePin=smooth((Math.hypot(cx-8-grab[0],cy+16-grab[1])-3)/9),hair=Math.exp(-(((x-grab[0])/25)**2)-((y-grab[1])/24)**2),lock=1-smooth((x-(cx+12))/7)*(1-smooth((y-(cy+15))/14)),gate=1-smooth((y-(cy+25))/10),h=(hair*pin*(1-face)+.6*face*facePin*pin)*gate*lock,body=smooth((y-(cy+24))/12)*bump(x,12,30,72,85)*sole;return near.map((n,i)=>-.12*n*h*sole-.055*far[i]*body*pin);}
async function create(){const model=await r49.create(),entry=model.prior.entry,hp=premultiply(entry.head,api);let sx=0,sy=0,n=0;for(let y=0;y<225;y++)for(let x=0;x<225;x++){const i=(y*225+x)*4;if(hp[i+3]>=128&&hp[i]>hp[i+2]+12&&hp[i+2]>hp[i+1]+12){sx+=x;sy+=y;n++;}}const center=[sx/n*96/225,sy/n*96/225];
 function lag(r,scale){const t=Math.max(0,r.ms-600),s=r.settling?r49.state(Math.min(.44,t/1000),r.lastAngle,[37.5,28.25]):{progress:1,angle:r.displayed,anchor:[37.5,28.25],remaining:1};s.angle=r.displayed;s.y=0;s.squash=0;
  s.squash=0;if(r.ms>=780)return model.render({progress:0,angle:0,remaining:0,hard:true,spread:r.platform?.LegSpread??0,squash:r.platform?.Squash??0,y:0},scale);
  const tr=r48.transform(s),body=entry.body(s.progress),a=new Float64Array(4),b=new Float64Array(4),near=[r.localNear.X,r.localNear.Y],far=[r.localFar.X,r.localFar.Y],grab=[37.5,28.25];
  return surface(api,96*scale,(x,y)=>{x=48+(x-48)/(1+s.squash*5/9);y=87+(y-87)/(1-s.squash);[x,y]=tr.inverse(x,y);const q=[x,y];let uv=q;for(let k=0;k<8;k++){const d=field(uv,center,grab,near,far),next=[q[0]-d[0],q[1]-d[1]];if(Math.hypot(next[0]-uv[0],next[1]-uv[1])<1e-8){uv=next;break;}uv=next;}[x,y]=uv;if(x< -32||x>128||y<0||y>116){a.fill(0);return a;}sample(body,375,(x+32)*225/96-.5,(y+32)*225/96-.5,a);sample(hp,225,x*225/96-.5,y*225/96-.5,b);return over(a,b);},80);
 }
 const platform=(r,scale)=>model.render({progress:0,angle:0,remaining:0,hard:true,spread:r.spread,squash:r.squash,y:0},scale);
 return{lag,platform,center,model};
}
module.exports={create,fallTrace,shortDrop,Lag,field,dir};

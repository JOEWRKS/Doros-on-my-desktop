'use strict';
const {setup}=require('./accepted-motion.cjs');
const hunt=require('../PreviewLocomotion/hunt-motion.js');
const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
const {premultiply,sample,over,surface}=require('./sampling.cjs');
const happy=require('./happy.cjs');
const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
function jumpTime(age){
 if(age<.18)return .25+.45*smooth(age/.18);
 if(age<.66)return 2.05+.55*smooth(((age-.18)/.48)/.65);
 return 2.6;
}
async function create({outlineRecovery=false,restoreHairTip=false,pointedHairTip=false,acceptedSleep=false,snackEffects=false,size=144}={}){
 const s=await setup({polished:true,fixedRunRoots:true,connected:true,outlineRecovery,restoreHairTip,pointedHairTip,size});
 const heads=Object.fromEntries(Object.keys(s.rigs).map(eye=>[eye,premultiply(s.rigs[eye].parts.head,s.api)]));
 const sleepRig=acceptedSleep?require('./accepted-sleep.cjs').create(s.postures.closed.rig,s.api,s.measurement):null;
 const jewel=snackEffects?await s.api.loadImage(require('node:path').join(s.root,'src/Dororong.App/Assets/snack-jewel.png')):null;
 function huntFrame(age,eye='open',lift=0,effect=null){
  const p=hunt.sample(age);
  if(p.amount===0&&lift===0)return effect?s.rigs[eye].render({effect}):s.rigs[eye].still();
  const rig=s.postures[eye].rig,shift=[p.sway,-1.6*p.amount+.22*p.sway];
  // Keep the source folded paw rigid. The stationary contact uses the existing
  // hunt's hip/foot shift, without warping its socket or reusing low-res ink.
  const root=hunt.map(64,73,p),tip=hunt.map(65,81,p),angle=Math.atan2(tip[1]-root[1],tip[0]-root[0])-Math.atan2(8,1),co=Math.cos(angle),si=Math.sin(angle);
  const map=([x,y])=>[tip[0]+(x-65)*co-(y-81)*si,tip[1]+(x-65)*si+(y-81)*co];
  const extraParts={haunch:rootedHaunch(shift[1],shift[0]),nearHind:connectedHind(map,shift)};
  const body=rig.render(rig.pose(0),{includeHead:false,mapOverride:()=> ([x,y])=>hunt.map(x,y,p),extraParts});
  return compose(body,eye,{roll:p.headRoll,pivot:[43,48],dy:p.headBob,lift,effect});
 }
 function compose(body,eye,{roll=0,pivot=[43,66],dx=0,dy=0,lift=0,chew=0,effect=null,bodyClick=null}={}){
  const bd=premultiply(body,s.api),h=heads[eye],co=Math.cos(roll),si=Math.sin(roll),a=new Float64Array(4),b=new Float64Array(4),c=new Float64Array(4);
  const eff=effect?premultiply(effect,s.api):null;
  return surface(s.api,size,(x,y)=>{
   if(bodyClick){const [sx,sy,ty]=bodyClick;x=48+(x-48)/sx;y=88+(y-88-ty)/sy;}
   y+=lift;sample(bd,225,x*225/96-.5,y*225/96-.5,a);
   const ox=x-dx-pivot[0],oy=y-dy-pivot[1];let hx=pivot[0]+ox*co+oy*si,hy=pivot[1]-ox*si+oy*co;
   const ex=hx,ey=hy;
   if(chew&&hx>=24&&hx<=34&&hy>=56&&hy<=64){
    const w=smooth((hx-24)/2)*smooth((34-hx)/2)*smooth((hy-56)/2)*smooth((64-hy)/2);
    hx=29+(hx-29)/(1+.18*chew*w);hy=60+(hy-60)/(1-.08*chew*w)+.95*chew*w;
   }
   sample(h,225,hx*225/96-.5,hy*225/96-.5,b);over(a,b);
   if(eff){sample(eff,225,ex*225/96-.5,ey*225/96-.5,c);over(a,c);}return a;
  },16,outlineRecovery?require('./head-outline.cjs').create(h,(x,y)=>{
   const ox=x-dx-pivot[0],oy=y+lift-dy-pivot[1];return [pivot[0]+ox*co+oy*si,pivot[1]-ox*si+oy*co];
  }):null);
 }
 function eat(age){
  const rig=s.postures.closed.rig,p=rig.pose(0),extraParts={haunch:rootedHaunch(0),nearHind:connectedHind(q=>q)};
  const body=rig.render(p,{includeHead:false,extraParts});
  const beat=age>.48&&age<1.55?Math.sin((age-.48)/1.07*3*Math.PI)**2:0;
  const effect=s.api.createCanvas(225,225),g=effect.getContext('2d');g.scale(225/96,225/96);
  const open=smooth(age/.14)*(1-smooth((age-.30)/.18));
  if(open>.01){
   g.beginPath();g.moveTo(27,60);g.quadraticCurveTo(29,60.6,31.2,60);g.bezierCurveTo(31,60+open*4.2,27.2,60+open*4.2,27,60);
   g.fillStyle='#6d3948';g.fill();g.strokeStyle='#50343d';g.lineWidth=.85;g.lineJoin=g.lineCap='round';g.stroke();
   g.beginPath();g.ellipse(29.2,60+open*2,1.1,open*.75,0,0,Math.PI*2);g.fillStyle='#eb91aa';g.fill();
  }
  if(snackEffects)require('./snack-effects.cjs').draw(g,age,{eating:true,jewel});
  return compose(body,'closed',{chew:beat,effect});
 }
 function sleepFrame(progress){
  if(sleepRig){const p=sleepRig.render(progress);return compose(p.body,'closed',{dy:p.headY,effect:p.effect});}
  const sleep=require('./sleep-pose.cjs');
  return compose(sleep.render(s.postures.closed.rig,progress),'closed',{dy:sleep.pose(progress).headY});
 }
 function render(kind,t,options={}){
  const eye=options.eye||'open';
  const effect=snackEffects&&options.snackAge!==undefined?require('./snack-effects.cjs').layer(s.api,options.snackAge):null;
  if(kind==='idle')return s.rigs[eye].render({breathing:options.breathing||0});
  if(kind==='track')return s.rigs[eye].render({roll:options.roll||0,eyeX:options.eyeX||0,eyeY:options.eyeY||0});
  if(kind==='walk'||kind==='run')return s.rigs[eye].render({phase:t,amount:options.amount??1,walking:kind==='walk',runMix:options.runMix??(kind==='run'?1:0),effect});
  if(kind==='sit'||kind==='rise')return s.postures[eye].render(kind==='rise'?1-t:t);
  if(kind==='hunt')return huntFrame(t,eye);
  if(kind==='jump')return huntFrame(jumpTime(t),eye,options.lift||0,effect);
  if(kind==='eat')return eat(t);
  if(kind==='sleep'||kind==='wake')return sleepFrame(kind==='wake'?1-t:t);
  if(kind==='happy')return s.rigs.closed.render({roll:happy.roll(t)});
  if(kind==='body-click'){
   const rig=s.postures.open.rig,body=rig.render(rig.pose(0),{includeHead:false,extraParts:{haunch:rootedHaunch(0),nearHind:connectedHind(q=>q)}});
   return compose(body,'open',{bodyClick:options.press!==undefined?[1+.012*options.press,1-.025*options.press,0]:require('./body-click.cjs').pose(t)});
  }
  throw Error('Unmigrated motion: '+kind);
 }
 return {s,render,compose,huntFrame,heads};
}
module.exports={create,jumpTime,smooth};

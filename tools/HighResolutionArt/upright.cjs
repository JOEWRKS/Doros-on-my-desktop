'use strict';
const {split,assemblyFor}=require('./parts.cjs');
const {paths}=require('../PreviewLocomotion/reference-parts.cjs');
const {premultiply,sample,over,surface}=require('./sampling.cjs');
const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
const bump=(v,a,b,c,d)=>smooth((v-a)/(b-a))*smooth((d-v)/(d-c));
function gazeWeight(x,y){
 let m=0;for(const e of [[15,17,27,30,43,46,57,60],[32,34,43,46,46,49,60,63]])m=Math.max(m,bump(x,...e.slice(0,4))*bump(y,...e.slice(4)));
 // A smooth protected mouth, not an abrupt rectangular zero in the eye field.
 return m*(1-bump(x,24,26,33,34)*bump(y,56,58,62,63));
}
const offsets=[0,.09,.53,.60];
const maxRaisedRoll=14*Math.PI/180;
function foot(leg,phase,walking){
 const q=((phase+(walking?(leg%2===0?0:.5):offsets[leg]))%1+1)%1;
 if(walking){const u=(q-.6)/.4;return q<.6?[-3+10*q,0]:[3-6*smooth(u),-2.8*Math.sin(Math.PI*u)**2];}
 const u=(q-.3)/.7;return q<.3?[-4.2+8.4*q/.3,0]:[4.2-8.4*smooth(u),-4*Math.sin(Math.PI*u)**2];
}
// Match .NET midpoint-to-even: integer body/head bob must not change gait.
const round=v=>{const a=Math.floor(v),d=v-a;return d===.5?(a%2===0?a:a+1):Math.round(v);};
function create(master,api,{size=144,padding=0,gaze=true,reachRun=false,fixedRunRoots=false,connected=false,outlineRecovery=false}={}){
 if(!Number.isFinite(size)||size<96||size>288)throw Error('Unsupported review size');
 if(!Number.isInteger(padding)||padding<0||padding>24)throw Error('Integral logical padding required');
 const assembly=assemblyFor(master),parts=assembly?{head:assembly.head,body:assembly.rig.render(assembly.rig.pose(0),{includeHead:false})}:split(master,api);
 const source=premultiply(master,api),head=premultiply(parts.head,api);
 const read=(data,x,y,p)=>sample(data,225,x*225/96-.5,y*225/96-.5,p);
 function still(){if(connected)return render();const p=new Float64Array(4);return surface(api,size,(x,y)=>read(source,x,y,p),padding);}
 function render({phase=0,amount=0,walking=true,roll=0,eyeX=0,eyeY=0,breathing=0,runMix=walking?0:1,effect=null,bodyOnly=false}={}){
  if(![phase,amount,roll,eyeX,eyeY,breathing].every(Number.isFinite))throw Error('Finite motion values required');
  amount=Math.max(0,Math.min(1,amount));breathing=Math.max(0,Math.min(1,breathing));
  // Keep the accepted +/-8 degree happy sway. Limit only the over-raised
  // tracking extreme: the20 degree candidate exposed an unnaturally long neck.
  roll=Math.max(-Math.PI/9,Math.min(maxRaisedRoll,roll));eyeX=Math.max(-.85,Math.min(.85,eyeX));eyeY=Math.max(-.65,Math.min(.65,eyeY));
  if(!gaze){eyeX=0;eyeY=0;}
  if(!connected&&amount===0&&roll===0&&eyeX===0&&eyeY===0&&breathing===0)return still();
  const revisedRun=reachRun&&!walking;
  if(!Number.isFinite(runMix))throw Error('Finite gait blend required');runMix=Math.max(0,Math.min(1,runMix));
  const walkBob=-.55*amount*(1-Math.cos(phase*4*Math.PI)),runBob=-1.6*amount*Math.sin(2*Math.PI*(phase-.08))**2;
  const bob=connected?walkBob*(1-runMix)+runBob*runMix:walking?round(walkBob):(revisedRun?runBob:round(-2.4*amount*Math.sin(2*Math.PI*(phase-.08))**2));
  const feet=[0,1,2].map(i=>foot(i,phase,walking)),far=foot(3,phase,walking),co=Math.cos(roll),si=Math.sin(roll);
  // Fully closed body parts remain behind the original head. Never cut the
  // assembled body again, and never restrict exposed neck skin to an old head
  // mask: that was the triangular hole when looking down.
  const h=(x,y)=>[43+(x-43)*co-(y-66)*si,66+(x-43)*si+(y-66)*co];
  const front=h(16,68),chest=[17.2,73],extraParts={};
  if(roll>0){const edge=[['M',...front],['C',front[0]-1,front[1]+(chest[1]-front[1])*.35,chest[0]-.7,chest[1]-1.8,...chest]];
   extraParts.neck={fill:[...edge,['L',46,75],['L',...h(47,65)],['Z']],ink:[edge],rootInk:[]};}
  // Move whole closed leg contours from the hip to the toe, sharing the same
  // mapping for skin and ink. No short ankle warp or cropped duplicate paw.
  const legNames=['farFront','nearFront','nearHind'];
  const mapOverride=name=>{
   const leg=legNames.indexOf(name);
   if(connected&&leg>=0){
    const run=require('./motion-polish.cjs').runMap(leg,phase,amount,bob),walk=foot(leg,phase,true);
    return ([x,y])=>{const w=smooth((y-59)/(leg===0?21:25)),a=[x+walk[0]*amount*w,y+bob+(walk[1]*amount-bob)*w],b=run([x,y]);return a.map((v,i)=>v+(b[i]-v)*runMix);};
   }
   if(revisedRun&&leg>=0)return require('./motion-polish.cjs').runMap(leg,phase,amount,bob);
   return ([x,y])=>{
    if(leg<0)return [x,y+bob];
    const w=smooth((y-59)/(leg===0?21:25));
    return [x+feet[leg][0]*amount*w,y+bob+(feet[leg][1]*amount-bob)*w];
   };
  };
  if(amount>0&&assembly){
   // The far hind leg starts hidden inside the torso and emerges continuously.
   // It is an entire closed folded-triangle part, never a rectangular crop.
   const farMap=()=> revisedRun?require('./motion-polish.cjs').runMap(3,phase,amount,bob):([x,y])=>{
    const w=smooth((y-59)/25);
    // Keep the same paw geometry and phase; expose its distal skin instead of
    // compressing its visible part into a one-pixel outlined sliver.
    return [x-10+far[0]*amount*w,y-9+7*amount+bob+(far[1]*amount-bob)*w];
   };
   extraParts.farHind=paths('nearHind',assembly.rig.pose(0),{artwork:assembly.artwork,mapOverride:farMap});
  }
  if(revisedRun&&fixedRunRoots&&amount>0&&assembly){
   const {rootedHind,rootedHaunch}=require('./motion-polish.cjs');
   extraParts.haunch=rootedHaunch(bob);
   extraParts.nearHind=rootedHind(2,phase,amount,bob,api,assembly.rig.pose(0));
   extraParts.farHind=rootedHind(3,phase,amount,bob,api,assembly.rig.pose(0));
  }
  if(connected&&assembly){
   const {connectedHind,rootedHaunch,runAngle}=require('./motion-polish.cjs'),walk=foot(2,phase,true);
   const angle=-Math.atan2(walk[0]*amount,11)*(1-runMix)+runAngle(2,phase)*amount*runMix,co=Math.cos(angle),si=Math.sin(angle);
   const map=([x,y])=>[64+(x-64)*co-(y-73)*si,73+(x-64)*si+(y-73)*co+bob+walk[1]*amount*(1-runMix)];
   extraParts.nearHind=connectedHind(map,[0,bob]);extraParts.haunch=rootedHaunch(bob);
   if(amount>0){
    const farWalk=foot(3,phase,true),a=-Math.atan2(farWalk[0]*amount,11)*(1-runMix)+runAngle(3,phase)*amount*runMix,c=Math.cos(a),sn=Math.sin(a);
    const map=([x,y])=>[64+(x-64)*c-(y-73)*sn-10,73+(x-64)*sn+(y-73)*c+bob+(-9+7*amount)+farWalk[1]*amount*(1-runMix)];
    extraParts.farHind=require('./motion-polish.cjs').clippedHind(map,bob,api,assembly.rig.pose(0));
   }
  }
  const bodyCanvas=assembly?assembly.rig.render(assembly.rig.pose(0),{includeHead:false,mapOverride,extraParts}):parts.body;
  if(bodyOnly)return {body:bodyCanvas,headY:bob};
  const bd2=premultiply(bodyCanvas,api);
  const effectPixels=effect?premultiply(effect,api):null;
  const p=new Float64Array(4),q=new Float64Array(4),r=new Float64Array(4);
  return surface(api,size,(x,y)=>{
   p.fill(0);
   let sx=x,sy=y;
   if(breathing&&amount===0&&sx>=50&&sx<84&&sy>=49&&sy<74){const w=smooth((sx-50)/13)*Math.sin(Math.PI*(sy-49)/25)**2*breathing;sx-=.4*w;sy+=.22*w;}
   read(bd2,sx,sy,q);over(p,q);
   const dx=x-43,dy=y-bob-66;let hx=43+dx*co+dy*si,hy=66-dx*si+dy*co;
   const m=gaze?gazeWeight(hx,hy):0;hx-=eyeX*m;hy-=eyeY*m;
   read(head,hx,hy,r);over(p,r);if(effectPixels){read(effectPixels,hx,hy,r);over(p,r);}return p;
  },padding,outlineRecovery?require('./head-outline.cjs').create(head,(x,y)=>{
   const dx=x-43,dy=y-bob-66;return [43+dx*co+dy*si,66-dx*si+dy*co];
  }):null);
 }
 return {still,render,parts,foot};
}
module.exports={create,foot,round,maxRaisedRoll,gazeWeight};

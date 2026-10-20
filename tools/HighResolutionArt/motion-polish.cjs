'use strict';
const {paths}=require('../PreviewLocomotion/reference-parts.cjs');
const {artwork}=require('./body.cjs');
const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
const cycleSeconds=.56;
// Visible limb hinges sit inside the body overlap. Rotating at the old high
// skeleton roots hid the entire exposed paw inside the torso on recovery.
const roots=[[22,70],[40,73],[64,73],[64,73]],offsets=[0,.09,.53,.60];
function runAngle(leg,phase){
 const q=((phase+offsets[leg])%1+1)%1;
 // Positive rotation reaches toward the face (left). Whole limbs rotate;
 // no vertical ankle pull, bone scaling or tip-only shortening.
 return (q<.42?50-68*smooth(q/.42):-18+68*smooth((q-.42)/.58))*Math.PI/180;
}
function runMap(leg,phase,amount,bob){
 const [rx,ry]=roots[leg],angle=runAngle(leg,phase)*amount,co=Math.cos(angle),si=Math.sin(angle);
 return ([x,y])=>{
  const w=smooth((y-(ry-12))/12),dx=(x-rx)*co-(y-ry)*si-(x-rx),dy=(x-rx)*si+(y-ry)*co-(y-ry);
  // Fully rigid below the hinge; only hidden socket skin blends to the torso.
  return [x+w*dx+(leg===3?-10:0),y+w*dy+bob+(leg===3?-9+7*amount:0)];
 };
}
function seatedHind(p){
 const part=paths('nearHind',p,{artwork}),weight=smooth((p.t-.50)/.41);
 if(!weight)return part;
 // The two-bone blend folds the outer knee below the seated paw/haunch floor.
 // Smooth only that distal vector envelope; never erase/paint output pixels.
 const map=([x,y])=>{
  const cap=75+p.haunch[1]+.12*(x-(58+p.haunch[0]));
  const excess=Math.log1p(Math.exp(Math.min(100,(y-cap)*3)))/3;
  return [x,y-weight*excess];
 };
 const convert=path=>path.map(([op,...v])=>[op,...v.flatMap((_,i)=>i%2?[]:map(v.slice(i,i+2)))]);
 return {...part,fill:convert(part.fill),ink:part.ink.map(convert),rootInk:part.rootInk.map(convert)};
}
function rigidHindMap(leg,phase,amount,bob){
 const [rx,ry]=roots[leg],a=runAngle(leg,phase)*amount,co=Math.cos(a),si=Math.sin(a);
 return ([x,y])=>[rx+(x-rx)*co-(y-ry)*si+(leg===3?-10:0),ry+(x-rx)*si+(y-ry)*co+bob+(leg===3?-9+7*amount:0)];
}
function rootedHind(leg,phase,amount,bob,api,pose){
 const map=rigidHindMap(leg,phase,amount,bob);
 if(leg===2)return connectedHind(map,[0,bob]);
 return clippedHind(map,bob,api,pose);
}
function clippedHind(map,bob,api,pose){
 const part=paths('nearHind',pose,{artwork,mapOverride:()=>map});
 const pathOf=fill=>{const p=new api.Path2D();for(const [op,...q]of fill){if(op==='M')p.moveTo(...q);else if(op==='L')p.lineTo(...q);else if(op==='C')p.bezierCurveTo(...q);else p.closePath();}return p;};
 // Keep the entire folded distal contour rigid. Only the proximal socket is
 // clipped to the fixed torso; it can never contribute an exterior bump.
 let torso=null;for(const name of ['chest','haunch']){
  const p=pathOf(paths(name,pose,{artwork,mapOverride:()=> ([x,y])=>[x,y+bob]}).fill);
  torso=torso?torso.op(p,api.PathOp.Union):p;
 }
 const distal=new api.Path2D();[[-100,73],[200,73],[200,200],[-100,200]].map(map).forEach((p,i)=>i?distal.lineTo(...p):distal.moveTo(...p));distal.closePath();
 part.shape=pathOf(part.fill).op(torso.op(distal,api.PathOp.Union),api.PathOp.Intersect);
 return part;
}
function connectedHind(map,shift=[0,0]){
  const part={name:'nearHind',ink:[],rootInk:[]};
  // The rump ends at an exact subdivision of its original outer curve. Join
  // from that fixed tangent to the rigid source paw, rather than unioning two
  // exposed rounded sockets (which leaves a second lobe beside the paw).
  const P=[74.25+shift[0],65.4375+shift[1]],source=(x,y)=>map([x*96/225,y*96/225]);
  const Q=source(163.7,175.4),next=source(162.8,181),d=Math.hypot(Q[0]-P[0],Q[1]-P[1])/3;
  const len=Math.hypot(next[0]-Q[0],next[1]-Q[1]);
  const tangentLength=Math.hypot(-5.25,19.125);
  part.fill=[['M',...P],['C',P[0]-5.25/tangentLength*d,P[1]+19.125/tangentLength*d,Q[0]-(next[0]-Q[0])/len*d,Q[1]-(next[1]-Q[1])/len*d,...Q],
   ['C',...source(162.8,181),...source(164.9,183.3),...source(165.2,186.2)],
   ['C',...source(165.7,191.5),...source(158.5,196.8),...source(153,197.1)],
   ['C',...source(148.2,197.3),...source(141.2,188.5),...source(139.1,181.2)],
   ['C',...source(137.8,177.2),...source(139.1,173.8),...source(138.1,173.1)],
   ['L',55+shift[0],65+shift[1]],['Z']];
  part.ink=[[['M',...Q],...part.fill.slice(2,6)]];
 return part;
}
function rootedHaunch(bob,dx=0){
 const part={name:'haunch',fill:[['M',51,51+bob],['C',56,43+bob,62,42+bob,65.5,45+bob],
  ['C',70,48+bob,73,52+bob,74.5,56+bob],
  ['C',75.25,59+bob,75.125,62.25+bob,74.25,65.4375+bob],
  ['L',58,70+bob],['L',51,70+bob],['C',45,70+bob,43,63+bob,51,51+bob],['Z']],ink:[],rootInk:[]};
 if(dx)part.fill=part.fill.map(([op,...v])=>[op,...v.map((q,i)=>q+(i%2?0:dx))]);return part;
}
module.exports={cycleSeconds,runAngle,runMap,seatedHind,roots,rigidHindMap,rootedHind,rootedHaunch,connectedHind,clippedHind};

'use strict';
const {connectedHind,rootedHaunch}=require('./motion-polish.cjs');
const {paths}=require('../PreviewLocomotion/reference-parts.cjs');
const {artwork}=require('./body.cjs');
const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
const rotate=(root,target,angle)=>([x,y])=>[target[0]+(x-root[0])*Math.cos(angle)-(y-root[1])*Math.sin(angle),target[1]+(x-root[0])*Math.sin(angle)+(y-root[1])*Math.cos(angle)];
function pose(progress){
 const t=smooth(progress),shift=[4*t,4*t];
 const rear=rotate([65,84],[65-3*t,84],1.2*t);
 const nearHind=connectedHind(rear,shift),q=(x,y)=>rear([x*96/225,y*96/225]);
 // The inner edge belongs to the folded paw, not the rump silhouette.
 // Preserve the source curve and reveal it as the paw tucks in front.
 const curve=[[139.1,181.2],[141.2,188.5],[148.2,197.3],[153,197.1]].map(p=>q(...p)),u=.3;
 const mix=(a,b)=>a.map((v,i)=>v+(b[i]-v)*u),a=mix(curve[0],curve[1]),b=mix(curve[1],curve[2]),c=mix(curve[2],curve[3]),d=mix(a,b),e=mix(b,c),f=mix(d,e);
 nearHind.ink=[[['M',...f],['C',...e,...c,...curve[3]],['L',curve[3][0],100]]];
 // Resting projection, not a dangling standing toe: settle only the bottom
 // control envelope onto the reference's shared belly/paw contact line.
 const settle=path=>path.map(([op,...v])=>[op,...v.map((q,i)=>i%2?q-t*Math.max(0,q-(84.3-.035*Math.max(0,v[i-1]-65)**2)):q)]);
 nearHind.fill=settle(nearHind.fill);
 nearHind.ink=nearHind.ink.map(path=>[...settle(path.slice(0,-1)),path.at(-1)]);
 // The original has two pads under the chin, not long sideways forearms.
 // Lower the shoulders behind the head and retain the rounded distal pads.
 const front={farFront:rotate([23,81],[23-4*t,81+2*t],1.3*t),nearFront:rotate([41.8,86.4],[41.8-10.8*t,86.4-6.4*t],Math.PI/2*t)};
 // Sleep lowers the belly onto the floor. Reusing the standing rump's high
 // underside left an empty wedge between the pads and the folded hind leg.
 const from=[['M',51,51],['C',56,43,62,42,65.5,45],['C',70,48,73,52,74.5,56],['C',75.25,59,75.125,62.25,74.25,65.4375],['C',68.833333333333,66.958333333333,63.416666666667,68.479166666667,58,70],['C',55.666666666667,70,53.333333333333,70,51,70],['C',45,70,43,63,51,51],['Z']];
 const to=[['M',51,59],['C',56,51,62,50,65.5,53],['C',70,56,77,58,79,64],['C',81,69,80,77,75,81],['C',70,84.3,65,84.3,58,84.3],['C',54,84.3,51,84,48,83],['C',38,77,43,68,51,59],['Z']];
 const haunch=t===0?rootedHaunch(0):{name:'haunch',fill:from.map((v,i)=>v.map((q,j)=>j?q+(to[i][j]-q)*t:q)),ink:[],rootInk:[]};
 return {t,headY:8*t,extraParts:{haunch,nearHind},front,mapOverride:name=>front[name]||(([x,y])=>[x,y+t])};
}
function render(rig,progress){
 const p=pose(progress);
 if(p.t===0)return rig.render(rig.pose(0),{includeHead:false,extraParts:{haunch:rootedHaunch(0),nearHind:connectedHind(q=>q)}});
 const art=structuredClone(artwork);
 // Fixed source edge, uncovered by geometry rather than faded or grown.
 const end=['C',48.5,72,48,67,44.8,66.4];
 art.nearFront.ink[0].push(end);
 art.nearFront.fill[5]=end;
 art.nearFront.fill[6]=['C',42,64,36,61,33,57];
 const nearFront=paths('nearFront',rig.pose(0),{artwork:art,mapOverride:()=>p.front.nearFront});
 // Source sleep shows a small rounded pad, not the oblique standing shin.
 // Keep the shoulder rotation, then articulate its resting silhouette. Fill
 // and ink share the exact same curves at every pose (no painted seam).
 const rounded=[['M',22,66],['C',15,71,12,76,12,81],['C',12,85,15,86,20,86],['C',25,86,30,85,31,81],['C',32,76,30,70,25,67],['C',24,65,23,65,22,66],['Z']];
 const fill=art.farFront.fill.map(([op,...v],i)=>[op,...v.flatMap((_,j)=>j%2?[]:p.front.farFront(v.slice(j,j+2))).map((q,j)=>q+(rounded[i][j+1]-q)*p.t)]);
 const farFront={name:'farFront',fill,ink:[fill.slice(0,4)],rootInk:[]};
 return rig.render(rig.pose(0),{includeHead:false,extraParts:{...p.extraParts,nearFront,farFront},mapOverride:p.mapOverride,interiorParts:['farFront','nearFront','nearHind'],clipInteriorToSkin:true,interiorOccluders:{nearHind:['nearFront'],farFront:['chest','nearFront'],nearFront:['chest']}});
}
module.exports={pose,render};

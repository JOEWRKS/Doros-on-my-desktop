'use strict';
// Local blocking rig only. World Y is up; Z separates near/far limbs.
// Main mass proportions are calibrated to canonical image landmarks. Hidden
// bone lengths are an artistic rig choice, not anatomy measured from pixels.
// No skin or endpoint artwork is read at runtime.
const clamp=x=>Math.max(0,Math.min(1,x));
const smooth=x=>{x=clamp(x);return x*x*x*(10+x*(-15+6*x));};
const span=(t,a,b)=>smooth((t-a)/(b-a));
const pulse=(t,a,m,b)=>t<m?span(t,a,m):1-span(t,m,b);
// Widen the paw without raising the chest/head: reduce the hidden support by
// the same amount as the new sole radius. Total standing shoulder height stays.
const foreSupportLength=7.5,foreSoleRadius=6.5,shoulderDrop=4,spineLength=24;

function solve(root,foot,upper,lower,bend){
  const dx=foot[0]-root[0],dy=foot[1]-root[1],d=Math.hypot(dx,dy);
  if(Math.abs(root[2]-foot[2])>1e-9||d>=upper+lower||d<=Math.abs(upper-lower))
    throw Error('Unreachable connected limb');
  const along=(upper*upper-lower*lower+d*d)/(2*d);
  const across=Math.sqrt(Math.max(0,upper*upper-along*along))*bend;
  return [root[0]+dx/d*along-dy/d*across,root[1]+dy/d*along+dx/d*across,root[2]];
}

function pose(time,direction='sit'){
  if(!Number.isFinite(time))throw Error('Time must be finite');
  if(!['sit','rise'].includes(direction))throw Error('Invalid direction');
  const t=clamp(time),rise=direction==='rise';
  // Hold the haunch while the hindfeet start reaching, then let it drop in a
  // short late beat. Rising gathers the feet first instead of reversing a fall.
  const fold=rise?1-span(t,.2,.74):span(t,.34,.72)**2;
  const support=rise?pulse(t,0,.18,.82):pulse(t,0,.2,.76);
  // Front legs are planted straight supports, not two-bone IK elbows. Solve
  // chest height FROM the support circle so a body path cannot force folding.
  const chestX=37-(rise?.7:2.1)*support-2*fold,lean=37-chestX;
  const chestY=foreSoleRadius+shoulderDrop+Math.sqrt(foreSupportLength**2-lean**2);
  // The small settle belongs to the pelvis. Raising the whole torso while
  // keeping straight legs and planted soles would require stretching the legs.
  const settle=rise?0:.65*pulse(t,.72,.8,.96);
  const chest={center:[chestX,chestY,0],radii:[14,10]};
  const hipY=23-8*fold+settle,hipRise=hipY-chestY;
  const pelvis={center:[chestX+Math.sqrt(spineLength**2-hipRise**2),hipY,0],radii:[14,15]};
  const headFollow=rise?pulse(t,.025,.23,.92):pulse(t,.03,.25,.85);
  const pitch=.07*headFollow+(rise?0:.035*pulse(t,.8,.9,1));
  const neckVector=[-1,Math.sqrt(19*19-1)];
  const neck={base:chest.center.slice(),tip:[
    chest.center[0]+neckVector[0]*Math.cos(pitch)-neckVector[1]*Math.sin(pitch),
    chest.center[1]+neckVector[0]*Math.sin(pitch)+neckVector[1]*Math.cos(pitch),0]};
  const head={center:neck.tip.slice(),radii:[29,21.5],angle:pitch};
  const legs=[];
  for(const front of [true,false])for(const near of [false,true]){
    const parent=front?chest:pelvis,z=near?9:-9;
    // Fixed attachments INSIDE the masses, not their top/center. The original
    // has tiny exposed paws; treating the whole haunch as a visible thigh made
    // the previous colored rig look long-legged even after shortening bones.
    // At rest both forelegs align vertically with their separate planted paws.
    // Canonical far forepaw is narrower (~8..9px versus near13px). Lower its
    // hidden shoulder by the sole-radius difference so both supports keep the
    // same length/lean and both narrower/wider soles still touch the floor.
    const farForeInset=front&&!near?2:0;
    const attachment=[front?(near?-1:-5):0,front?-shoulderDrop-farForeInset:-6,z];
    const root=parent.center.map((v,i)=>v+attachment[i]);
    const u=front?0:rise?1-span(t,near?.08:.1,near?.56:.58):span(t,near?.3:.32,near?.68:.7);
    const lift=front?0:3.2*Math.sin(Math.PI*u)**2;
    const radius=front?foreSoleRadius-farForeInset:5.5;
    // No ankle, toe axis, foot rotation or third segment: the round endpoint is
    // the foot. Both fore- and hindlegs have just an upper and lower segment.
    const foot=[front?(near?36:32):60-13.5*u,radius+lift,z],lengths=front?[foreSupportLength/2,foreSupportLength/2]:[7,7];
    const middle=front?root.map((v,i)=>(v+foot[i])/2):solve(root,foot,lengths[0],lengths[1],-1);
    legs.push({name:`${near?'near':'far'}${front?'Front':'Hind'}`,front,near,
      attachment,joints:[root,middle,foot],lengths,lift,radii:[radius,radius]});
  }
  return {time:t,direction,fold,chest,pelvis,neck,head,legs};
}
module.exports={pose};

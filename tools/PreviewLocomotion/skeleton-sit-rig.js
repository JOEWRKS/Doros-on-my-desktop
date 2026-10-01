(function(root){
  'use strict';
  const clamp=x=>Math.max(0,Math.min(1,x));
  const smooth=x=>{x=clamp(x);return x*x*x*(10+x*(-15+6*x));};
  const span=(t,a,b)=>smooth((t-a)/(b-a));
  const pulse=(t,a,m,b)=>t<m?span(t,a,m):1-span(t,m,b);
  const distance=(a,b)=>Math.hypot(b[0]-a[0],b[1]-a[1]);
  const transform=(p,m)=>[m[0]*p[0]+m[2]*p[1]+m[4],m[1]*p[0]+m[3]*p[1]+m[5]];
  function rotation(pivot,angle,offset=[0,0]){
    const c=Math.cos(angle),s=Math.sin(angle);
    return [c,s,-s,c,pivot[0]+offset[0]-c*pivot[0]+s*pivot[1],pivot[1]+offset[1]-s*pivot[0]-c*pivot[1]];
  }
  function boneMatrix(a,b,c,d){
    const angle=Math.atan2(d[1]-c[1],d[0]-c[0])-Math.atan2(b[1]-a[1],b[0]-a[0]);
    return rotation(a,angle,[c[0]-a[0],c[1]-a[1]]);
  }
  const definitions=[
    {name:'farFront',joints:[[22,68],[21,73],[22,77],[23,80]]},
    {name:'nearFront',joints:[[39,68],[37,75],[41,81],[43,85]]},
    {name:'nearHind',joints:[[66,66],[61,71],[68,77],[66,82]]},
    {name:'farHind',joints:[[63,64],[58,69],[65,75],[63,80]]}
  ];
  for(const d of definitions){
    d.lengths=d.joints.slice(1).map((p,i)=>distance(d.joints[i],p));
    const [a,b,c]=d.joints;
    d.bend=Math.sign((c[0]-a[0])*(b[1]-a[1])-(c[1]-a[1])*(b[0]-a[0]));
  }
  function solve(root,wrist,lengths,bend){
    const [upper,lower]=lengths,dx=wrist[0]-root[0],dy=wrist[1]-root[1],d=Math.hypot(dx,dy);
    if(d>=upper+lower||d<=Math.abs(upper-lower))throw Error('Unreachable skeletal limb');
    const along=(upper*upper-lower*lower+d*d)/(2*d),across=Math.sqrt(Math.max(0,upper*upper-along*along))*bend;
    return [root[0]+dx/d*along-dy/d*across,root[1]+dy/d*along+dx/d*across];
  }
  function pose(time,direction='sit'){
    if(!Number.isFinite(time))throw Error('Time must be finite');
    if(!['sit','rise'].includes(direction))throw Error('Invalid direction');
    const t=clamp(time),rise=direction==='rise';
    const fold=rise?1-span(t,.12,.79):span(t,.25,.81);
    const support=rise?pulse(t,0,.2,.86):pulse(t,0,.22,.88);
    const settle=rise?0:pulse(t,.78,.87,1);
    const trunk=rotation([39,66],.42*fold,[-5*fold-.8*support,.35*support+.18*settle]);
    const head=rotation([40,57],.018*support-.012*settle,[-1.4*support,.55*support+.65*settle]);
    const parts=[{name:'trunk',matrix:trunk},{name:'head',matrix:head}];
    const legs=definitions.map((def,index)=>{
      const hind=index>=2,far=index===0||index===3;
      const u=hind?(rise?1-span(t,.42+(far?.045:0),.86+(far?.04:0)):span(t,.19+(far?.07:0),.58+(far?.07:0)))
        :(rise?1-span(t,.7+(far?0:.025),.95+(far?0:.025)):span(t,.02+(far?0:.06),.27+(far?0:.06)));
      const lift=(hind?2.4:1.5)*Math.sin(Math.PI*u)**2;
      const restPaw=def.joints[3];
      const target=hind?[restPaw[0]-6,restPaw[1]+2]:(far?[26,86]:[40,86]);
      const paw=[restPaw[0]+(target[0]-restPaw[0])*u,restPaw[1]+(target[1]-restPaw[1])*u-lift];
      const restAngle=Math.atan2(restPaw[1]-def.joints[2][1],restPaw[0]-def.joints[2][0]);
      const ankleAngle=restAngle+(hind?.85*fold:(Math.PI/2-restAngle)*u);
      const wrist=[paw[0]-Math.cos(ankleAngle)*def.lengths[2],paw[1]-Math.sin(ankleAngle)*def.lengths[2]];
      const base=hind?transform(def.joints[0],trunk):[def.joints[0][0]+(far?3:-1)*u-.8*support,def.joints[0][1]+(far?6:0)*u+.35*support];
      const knee=hind?solve(base,wrist,def.lengths,def.bend):null;
      // Forelegs make a small placement step as one supported chain; do not
      // over-bend short elbow cuts just to reach the reference foot location.
      const placement=rotation(restPaw,(far?-.17:.17)*u+(far?-.025:.03)*support,[paw[0]-restPaw[0],paw[1]-restPaw[1]]);
      const joints=hind?[base,knee,wrist,paw]:def.joints.map(p=>transform(p,placement));
      for(let i=0;i<3;i++){
        const from=[def.joints[i],def.joints[i+1]],to=[joints[i],joints[i+1]];
        parts.push({name:`${def.name}${i}`,matrix:boneMatrix(...from,...to),from,to});
      }
      return {name:def.name,joints,lengths:def.lengths.slice(),lift};
    });
    return {time:t,direction,fold,parts,legs};
  }
  const api={pose,definitions,transform,boneMatrix};
  if(typeof module!=='undefined')module.exports=api;else root.skeletonSitRig=api;
})(globalThis);

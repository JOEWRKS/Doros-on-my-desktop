(function(root){
  const clamp=x=>Math.max(0,Math.min(1,x));
  const smooth=x=>{x=clamp(x);return x*x*x*(10+x*(-15+6*x));};
  const span=(t,a,b)=>smooth((t-a)/(b-a));
  const pulse=(t,a,peak,b)=>t<peak?span(t,a,peak):1-span(t,peak,b);
  const mix=(a,b,t)=>a.map((v,i)=>v+(b[i]-v)*t);
  const frontStand=[[23,81],[43,86.5]],frontSit=[[26,87],[41,87]];
  const rearStand=[[64,83],[70,81]],rearSit=[[59,87],[70,85]];
  // Two rigid projected bone lengths. The solver determines the knee from
  // the pelvis and supported paw, rather than interpolating a knee pixel.
  function leg(hip,paw){
    const upper=13,lower=10,dx=paw[0]-hip[0],dy=paw[1]-hip[1],d=Math.hypot(dx,dy);
    if(d<=Math.abs(upper-lower)||d>=upper+lower)throw Error('Unreachable seating paw');
    const along=(upper*upper-lower*lower+d*d)/(2*d),across=Math.sqrt(upper*upper-along*along);
    return {hip,paw,knee:[hip[0]+dx/d*along-dy/d*across,hip[1]+dy/d*along+dx/d*across],upper,lower};
  }
  function step(a,b,t,start,end,lift){
    const u=span(t,start,end),height=lift*Math.sin(Math.PI*u)**2;
    const p=mix(a,b,u);p[1]-=height;return {p,height,u};
  }
  function pose(time,direction='sit'){
    if(!Number.isFinite(time))throw Error('Seating time must be finite');
    if(!['sit','rise'].includes(direction))throw Error('Invalid seating direction');
    const t=clamp(time),rising=direction==='rise';
    const fold=rising?1-span(t,.16,.72):span(t,.24,.78);
    const settle=rising?0:.6*pulse(t,.73,.82,1);
    const shoulder=rising?[-2.5*pulse(t,0,.23,.8),-.7*pulse(t,.12,.42,.92)]
      :[-2.2*pulse(t,0,.18,.76),.6*pulse(t,.04,.24,.75)];
    const head=rising?[-2.7*pulse(t,0,.26,.91),-1.5*pulse(t,.04,.4,1)]
      :[-2.1*pulse(t,.035,.28,.9),1.3*pulse(t,.1,.38,.76)+1.1*pulse(t,.7,.87,1)];
    const pelvis=[2*fold-2.1*Math.sin(Math.PI*fold),7*fold+settle];
    const fronts=frontStand.map((a,i)=>rising
      ?step(frontSit[i],a,t,.7+i*.04,.92+i*.04,1.2)
      :step(a,frontSit[i],t,.02+i*.1,.2+i*.12,1.6));
    const rears=rearStand.map((a,i)=>rising
      ?step(rearSit[i],a,t,.38+i*.07,.72+i*.09,2.1)
      :step(a,rearSit[i],t,.19+i*.08,.58+i*.08,2.6));
    const hips=[[65+pelvis[0],69+pelvis[1]],[61+pelvis[0],67+pelvis[1]]];
    return {time:t,direction,fold,head,shoulder,pelvis,settle,
      frontFeet:fronts.map(s=>s.p),frontLift:fronts.map(s=>s.height),frontBlend:fronts.map(s=>rising?1-s.u:s.u),
      hindFeet:rears.map(s=>s.p),hindLift:rears.map(s=>s.height),
      hindLegs:hips.map((hip,i)=>leg(hip,rears[i].p))};
  }
  const api={pose};if(typeof module!=='undefined')module.exports=api;else root.seatingMotion=api;
})(globalThis);

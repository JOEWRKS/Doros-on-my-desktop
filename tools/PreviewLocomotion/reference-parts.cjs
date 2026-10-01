'use strict';
// Source-aligned, independently closed artwork. No raster ownership cuts,
// image morph, inferred rendered silhouette, seam strips or global repaint.
const {measure,extractHead}=require('./original-contour-source.cjs');
const M=(...p)=>['M',...p],C=(...p)=>['C',...p],L=(...p)=>['L',...p],Z=['Z'];
const art={
  chest:{
    fill:[M(18,59),C(18,48,36,47,51,50),C(57,53,64,59,64,68),C(62,73,53,77,48.5,77.5),C(42,79,36,78,32.5,76.5),C(30,75.5,29,75.5,26.5,75.5),C(20,73,16,68,18,59),Z],
    rootInk:[[M(18,59),C(16,68,20,73,26.5,75.5)]],
    ink:[[M(26.5,75.5),C(29,75.5,30,75.5,32.5,76.5)],[M(48.5,77.5),C(52,77,56,75,58.5,73.5)]]
  },
  haunch:{
    fill:[M(51,51),C(56,43,62,42,65.5,45),C(70,48,73,52,74.5,56),C(76,62,74,69,69.5,74.5),C(65,77,61,76,58.5,73.5),C(45,74,43,63,51,51),Z],
    ink:[[M(65.5,45),C(70,48,73,52,74.5,56),C(76,62,74,69,69.5,74.5),C(65,77,61,76,58.5,73.5)]]
  },
  farFront:{
    fill:[M(16.5,57),C(15.5,64,16.5,72,19,78),C(21,82.5,25,83,26.5,79),C(27.5,76,27,72,28,66),C(28,60,27,56,23,55),C(20,54,17,55,16.5,57),Z],
    ink:[[M(16.5,57),C(15.5,64,16.5,72,19,78),C(21,82.5,25,83,26.5,79),C(27.5,76,27,72,28,66)]]
  },
  nearFront:{
    fill:[M(33,57),C(32,63,32,70,33.1,76.5),C(34.9,80,37.5,85.8,41.8,86.4),C(45.4,87.1,48.6,85.4,48.7,81.5),L(48.5,77.5),C(48.5,70,48,63,46,58),C(43,54,37,54,33,57),Z],
    ink:[[M(33.1,76.5),C(34.9,80,37.5,85.8,41.8,86.4),C(45.4,87.1,48.6,85.4,48.7,81.5),L(48.5,77.5)]]
  },
  nearHind:{
    fill:[M(59,59),C(59,55,68,55,70,59),C(71,64,70,70,69.5,74.5),C(69,77,71.5,78,70.7,81.1),C(69.8,83.9,66,85.2,63,83.2),C(60.5,81,59.5,77,59.3,73.5),C(58.5,67,58.5,62,59,59),Z],
    rootInk:[[M(59.3,73.5),C(59.43,75.775,59.898,78.26125,60.8962375,80.3271125)],
      [M(70,59),C(71,64,70,70,69.5,74.5)]],
    // Proximal skin is complete but uninked where it meets the rump. Only the
    // actual visible paw contour follows the distal segment into the sit.
    ink:[[M(60.8962375,80.3271125),C(61.43375,81.4395,62.125,82.43,63,83.2),C(66,85.2,69.8,83.9,70.7,81.1),C(71.5,78,69,77,69.5,74.5)]]
  }
};
const rest=[
  {name:'farFront',joints:[[22,60],[23,70],[24,79]]},
  {name:'nearFront',joints:[[40,60],[41,72],[42,84]]},
  {name:'nearHind',joints:[[64,59],[65,71],[65,81]]}
];
const clamp=v=>Math.max(0,Math.min(1,v));
const smooth=v=>{v=clamp(v);return v*v*v*(10+v*(-15+6*v));};
const span=(t,a,b)=>smooth((t-a)/(b-a));
const mix=(a,b,t)=>a+(b-a)*t;
function rotate(p,a,b,r){const x=p[0]-a[0],y=p[1]-a[1],co=Math.cos(r),s=Math.sin(r);return [b[0]+x*co-y*s,b[1]+x*s+y*co];}
function angle(a,b){return Math.atan2(b[1]-a[1],b[0]-a[0]);}
function lengths(l){return l.joints.slice(1).map((p,i)=>Math.hypot(p[0]-l.joints[i][0],p[1]-l.joints[i][1]));}
function solve(root,tip,a,b){
  const dx=tip[0]-root[0],dy=tip[1]-root[1],d=Math.hypot(dx,dy);
  if(d<1e-6||d>a+b+1e-7||d<Math.abs(a-b)-1e-7)throw Error('Unreachable anatomical foot target');
  const along=(a*a-b*b+d*d)/(2*d),h=Math.sqrt(Math.max(0,a*a-along*along));
  return [root,[root[0]+dx/d*along+dy/d*h,root[1]+dy/d*along-dx/d*h],tip];
}
function pose(t){
  if(!Number.isFinite(t))throw Error('Finite sit time required');t=clamp(t);
  const support=span(t,0,.3),reach=span(t,.18,.5),drop=span(t,.52,.91);
  const settle=t>.91?Math.sin(Math.PI*span(t,.91,1))*.45:0;
  const haunch=[-3*drop,11.5*drop+settle],chest=[0,support];
  const legs=rest.map((l,n)=>{
    const [a,b]=lengths(l);
    if(n===2){
      const root=[64+haunch[0],59+haunch[1]],tip=[65-10*reach-6*drop,81-4*reach+4*drop-Math.sin(Math.PI*drop)];
      return {name:l.name,joints:solve(root,tip,a,b)};
    }
    const target=n===0?[26,65]:[38,61],root=l.joints[0].map((v,i)=>mix(v,target[i],support));
    root[1]-=Math.sin(Math.PI*support)*1.3;
    const upper=mix(angle(l.joints[0],l.joints[1]),Math.PI/2,support),lower=mix(angle(l.joints[1],l.joints[2]),Math.PI/2,support);
    const knee=[root[0]+Math.cos(upper)*a,root[1]+Math.sin(upper)*a],tip=[knee[0]+Math.cos(lower)*b,knee[1]+Math.sin(lower)*b];
    return {name:l.name,joints:[root,knee,tip]};
  });
  return {t,support,haunch,chest,head:[0,Math.sin(Math.PI*drop)*.65],legs};
}
function mapPart(name,p){
  if(name==='chest'||name==='haunch')return q=>[q[0]+p[name][0],q[1]+p[name][1]];
  const a=rest.find(l=>l.name===name).joints,b=p.legs.find(l=>l.name===name).joints;
  const upper=angle(b[0],b[1])-angle(a[0],a[1]),lower=angle(b[1],b[2])-angle(a[1],a[2]);
  const dx=a[1][0]-a[0][0],dy=a[1][1]-a[0][1],length=Math.hypot(dx,dy);
  return q=>{
    const distance=((q[0]-a[0][0])*dx+(q[1]-a[0][1])*dy)/length,w=smooth((distance-length+2)/4);
    const u=rotate(q,a[0],b[0],upper),l=rotate(q,a[1],b[1],lower);
    return [mix(u[0],l[0],w),mix(u[1],l[1],w)];
  };
}
function paths(name,p){
  const map=mapPart(name,p);
  // The seated reference exposes the far paw more front-on and the near paw
  // more upright. These small, explicitly authored contour corrections belong
  // to those paws; they never scale a bone, torso or whole character image.
  const correct=q=>{
    if(name==='farFront'&&q[0]>=25&&q[1]>=76)return [q[0]+2.5*p.support,q[1]];
    if(name==='nearFront'&&q[0]<40&&q[1]>=80)return [q[0]+2*p.support,q[1]];
    return q;
  };
  const convert=path=>{
    const result=[];let current;
    for(const [op,...values] of path){
      const q=[];for(let i=0;i<values.length;i+=2)q.push(correct([values[i],values[i+1]]));
      if(op==='C'&&name!=='chest'&&name!=='haunch'){
        // Bind actual points on a curve, not its off-curve control handles:
        // handles can lie on different bones and otherwise bend ink and fill
        // along different trajectories at the same joint.
        for(let i=1;i<=32;i++){
          const t=i/32,u=1-t,point=[0,1].map(n=>u*u*u*current[n]+3*u*u*t*q[0][n]+3*u*t*t*q[1][n]+t*t*t*q[2][n]);
          result.push(L(...map(point)));
        }
      }else result.push([op,...q.flatMap(map)]);
      if(q.length)current=q.at(-1);
    }
    return result;
  };
  return {name,fill:convert(art[name].fill),ink:art[name].ink.map(convert),rootInk:(art[name].rootInk||[]).map(convert)};
}
function trace(ctx,path,begin=true){
  if(begin)ctx.beginPath();for(const [op,...q] of path){if(op==='M')ctx.moveTo(...q);else if(op==='C')ctx.bezierCurveTo(...q);else if(op==='L')ctx.lineTo(...q);else ctx.closePath();}
}
function paint(ctx,part,color='#fff'){
  trace(ctx,part.fill);ctx.fillStyle=color;ctx.fill();
  ctx.strokeStyle='#443438';ctx.lineWidth=1.25;ctx.lineCap='round';ctx.lineJoin='round';
  for(const ink of [...(part.rootInk||[]),...part.ink]){trace(ctx,ink);ctx.stroke();}
}
function addSkin(ctx,path){
  const points=[];let current;
  for(const [op,...q] of path){
    if(op==='M'||op==='L'){current=q;points.push(q);}
    else if(op==='C'){
      for(let i=1;i<=32;i++){
        const t=i/32,u=1-t;points.push([0,1].map(n=>u*u*u*current[n]+3*u*u*t*q[n]+3*u*t*t*q[n+2]+t*t*t*q[n+4]));
      }
      current=q.slice(4);
    }
  }
  const area=points.reduce((sum,p,i)=>{const q=points[(i+1)%points.length];return sum+p[0]*q[1]-q[0]*p[1];},0);
  if(area<0)points.reverse();
  points.forEach((p,i)=>i?ctx.lineTo(...p):ctx.moveTo(...p));ctx.closePath();
}
const depth=['farFront','chest','haunch','nearHind','nearFront'];
function create(source,canvas){
  const m=measure(source,canvas),head=extractHead(source,m,canvas);
  function render(p,{colors=false}={}){
    const c=canvas.createCanvas(96,96),ctx=c.getContext('2d');
    const parts=Object.fromEntries(depth.map(name=>[name,paths(name,p)]));
    const palette={farFront:'#619abd',chest:'#66c6b4',haunch:'#e6ab57',nearHind:'#a981c7',nearFront:'#58b7d8'};
    // Rasterize the SAME named vector skin faces in one nonzero-winding fill.
    // Adjacent antialiased edges must not source-over into a translucent joint.
    // No additional geometry, raster contour inference or post-render filling.
    ctx.beginPath();for(const name of depth)addSkin(ctx,parts[name].fill);ctx.fillStyle='#fff';ctx.fill();
    if(colors)for(const name of depth){trace(ctx,parts[name].fill);ctx.fillStyle=palette[name];ctx.fill();}
    // All body ink shares one coverage mask. Taking maximum coverage, rather
    // than source-over, makes coincident interior/exterior paths idempotent:
    // the edge has the original single-stroke AA, not a darker second coat.
    const layer=canvas.createCanvas(96,96),lc=layer.getContext('2d'),coverage=new Uint8ClampedArray(96*96);
    function strokeInk(ink,occluders){
      lc.clearRect(0,0,96,96);lc.save();
      for(const other of occluders){lc.beginPath();lc.rect(0,0,96,96);trace(lc,parts[other].fill,false);lc.clip('evenodd');}
      trace(lc,ink);lc.strokeStyle='#fff';lc.lineWidth=m.ink.width;lc.lineCap='round';lc.lineJoin='round';lc.stroke();lc.restore();
      const pixels=lc.getImageData(0,0,96,96).data;
      for(let i=0;i<coverage.length;i++)coverage[i]=Math.max(coverage[i],pixels[i*4+3]);
    }
    for(const [index,name] of depth.entries()){
      const foreground=depth.slice(index+1),part=parts[name];
      for(const ink of part.ink)strokeInk(ink,foreground);
      const socketCover=name==='chest'?'farFront':'haunch';
      for(const ink of part.rootInk)strokeInk(ink,[...new Set([...foreground,socketCover])]);
    }
    // A socket boundary can become the exterior as neighbouring parts move.
    // Stroke the actual closed part curve wherever NO other skin covers it.
    // Do this after layered fills, so a foreground white face cannot erase the
    // exposed edge. Sequential complement clips form the complement of the
    // UNION (one even-odd clip would wrongly reveal doubly covered seams).
    // This never traces/replaces the composite silhouette or changes artwork.
    for(const name of depth)strokeInk(parts[name].fill,depth.filter(other=>other!==name));
    const image=lc.createImageData(96,96);
    for(let i=0;i<coverage.length;i++){image.data.set(m.ink.rgb,i*4);image.data[i*4+3]=coverage[i];}
    lc.putImageData(image,0,0);ctx.drawImage(layer,0,0);
    ctx.imageSmoothingEnabled=false;ctx.drawImage(head,...p.head);return c;
  }
  function attachmentProbes(p){
    return [[35,64+p.chest[1]],[53+p.haunch[0],62+p.haunch[1]],...p.legs.map(l=>l.joints[1])];
  }
  return {pose,render,paths,art,rest,depth,head,attachmentProbes,canvas};
}
module.exports={create,pose,paths,art,rest,trace,paint};

'use strict';
// Deliberately NOT character artwork: rigid colored masses make the body action
// inspectable before any skinning. Near/far feet share a world-space floor.
const colors={head:'#e493b5',chest:'#3ec3be',pelvis:'#eaa259',spine:'#80ba91',
  farFront:'#216a91',nearFront:'#39a5df',farHind:'#654388',nearHind:'#9966cf'};
// Canonical fore soles lie near y81 (far) / y86 (near). Match that depth
// separation instead of lengthening the far leg to fake a single screen floor.
const project=p=>[p[0]+.7*p[2],83.5-p[1]+.27*p[2]];
function draw(ctx,pose,{bones=false}={}){
  const {width,height}=ctx.canvas,scale=Math.min(width/120,height/84);
  ctx.resetTransform();ctx.fillStyle='#f3f0e9';ctx.fillRect(0,0,width,height);
  ctx.translate(width/2-46*scale,height/2-55*scale);ctx.scale(scale,scale);
  const line=(a,b,color,w)=>{ctx.beginPath();ctx.moveTo(...a);ctx.lineTo(...b);ctx.strokeStyle=color;ctx.lineWidth=w;ctx.lineCap='round';ctx.stroke();};
  const capsule=(a,b,color,r)=>{a=project(a);b=project(b);line(a,b,'#34404c',r*2);line(a,b,color,r*2-.65);};
  function mass(center,radii,color,angle=0){
    const p=project(center);ctx.save();ctx.translate(...p);ctx.rotate(-angle);
    ctx.beginPath();ctx.ellipse(0,0,...radii,0,0,Math.PI*2);ctx.fillStyle=color;ctx.fill();
    ctx.beginPath();ctx.ellipse(0,0,radii[0]-.325,radii[1]-.325,0,0,Math.PI*2);
    ctx.strokeStyle='#34404c';ctx.lineWidth=.65;ctx.stroke();ctx.restore();
  }
  // Two parallel contact rails show the near/far ground plane without pretending
  // perspective-offset feet should land on the same screen-space Y coordinate.
  for(const z of [-9,9]){
    line(project([7,0,z]),project([90,0,z]),'#d5d2c9',.35);
    for(const x of [32,36,54,60]){
      const p=project([x,0,z]);line([p[0]-1,p[1]],[p[0]+1,p[1]],'#a5a699',.5);
    }
  }
  const leg=(l,localRootJoin=false)=>{
    // A single rounded contour avoids separate tube outlines crossing at the
    // knee. The centerline still passes through the actual solved middle joint.
    const points=l.joints.map(project);
    if(localRootJoin){
      // A tiny curved socket covers the round root cap and at most15% of the
      // first segment (capped at1.2 units). It follows the root/tangent, never
      // the parent silhouette. The full knee, lower leg and paw stay in front.
      const [root,middle]=points,dx=middle[0]-root[0],dy=middle[1]-root[1],length=Math.hypot(dx,dy);
      const inset=Math.min(1.2,length*.15);
      const ux=length?dx/length:0,uy=length?dy/length:1,w=l.radii[0]+.5;
      const local=(across,along)=>[root[0]-uy*across+ux*along,root[1]+ux*across+uy*along];
      ctx.save();ctx.beginPath();
      const transform=ctx.getTransform();ctx.resetTransform();ctx.rect(0,0,width,height);ctx.setTransform(transform);
      ctx.moveTo(...local(-w,-w));ctx.lineTo(...local(w,-w));ctx.lineTo(...local(w,0));
      ctx.quadraticCurveTo(...local(0,2*inset),...local(-w,0));ctx.closePath();ctx.clip('evenodd');
    }
    for(const [color,width] of [['#34404c',l.radii[0]*2],[colors[l.name],l.radii[0]*2-.65]]){
      ctx.beginPath();points.forEach((p,i)=>i?ctx.lineTo(...p):ctx.moveTo(...p));
      ctx.strokeStyle=color;ctx.lineWidth=width;ctx.lineJoin=ctx.lineCap='round';ctx.stroke();
    }
    if(localRootJoin)ctx.restore();
  };
  // Explicit camera-depth passes, independent of caller array order:
  // far hind/front -> torso -> near hind/front -> head. Near limbs keep their
  // foreground ownership even over their own parent; only the root join is cut.
  const backToFront=(a,b)=>Number(a.front)-Number(b.front);
  pose.legs.filter(l=>!l.near).sort(backToFront).forEach(l=>leg(l));
  capsule(pose.chest.center,pose.pelvis.center,colors.spine,8.5);
  mass(pose.pelvis.center,pose.pelvis.radii,colors.pelvis);
  mass(pose.chest.center,pose.chest.radii,colors.chest);
  capsule(pose.neck.base,pose.neck.tip,colors.chest,4.5);
  for(const l of pose.legs.filter(l=>l.near).sort(backToFront)){
    leg(l,true);
  }
  mass(pose.head.center,pose.head.radii,colors.head,pose.head.angle);
  // A constant nose marker indicates facing/pitch, not eyes or character skin.
  const hc=project(pose.head.center);ctx.save();ctx.translate(...hc);ctx.rotate(-pose.head.angle);
  ctx.beginPath();ctx.arc(-25,3,1.25,0,Math.PI*2);ctx.fillStyle='#77415e';ctx.fill();ctx.restore();
  if(bones){
    const bone=(a,b)=>line(project(a),project(b),'#34404c',.4);
    bone(pose.chest.center,pose.pelvis.center);bone(pose.neck.base,pose.neck.tip);
    for(const l of pose.legs){
      for(let i=0;i<l.joints.length-1;i++)bone(l.joints[i],l.joints[i+1]);
      for(const p of l.joints){
        ctx.beginPath();ctx.arc(...project(p),.85,0,Math.PI*2);ctx.fillStyle='#fff';ctx.fill();ctx.strokeStyle=colors[l.name];ctx.lineWidth=.4;ctx.stroke();
      }
    }
    for(const p of [pose.chest.center,pose.pelvis.center,pose.head.center]){
      ctx.beginPath();ctx.arc(...project(p),1,0,Math.PI*2);ctx.fillStyle='#34404c';ctx.fill();
    }
  }
  ctx.resetTransform();
}
module.exports={draw};

'use strict';
// Skin for the approved connected pose, never a second animation timeline.
// The only raster input is the unchanged canonical standing head/hair/ribbon.
const {pose:restPose}=require('./connected-sit-rig.cjs');
const project=p=>[p[0]+.7*p[2],83.5-p[1]+.27*p[2]];
const headBoundary=[[0,0],[96,0],[96,48],[69,48],[69,51],[67,55],[65,59],[63,63],[60,62],[58,58],[56,62],[52,66],[49,70],[46,72],[43,72],[42,68],[23,68],[20,70],[17,70],[0,70]];
function inside(poly,x,y){let yes=false;for(let i=0,j=poly.length-1;i<poly.length;j=i++){
  const a=poly[i],b=poly[j];if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])yes=!yes;
}return yes;}
function create(standing,{createCanvas}){
  if(!standing||standing.width!==96||standing.height!==96)throw Error('Canonical 96x96 standing image required');
  const head=createCanvas(96,96),h=head.getContext('2d');h.drawImage(standing,0,0);
  const pixels=h.getImageData(0,0,96,96);
  for(let y=0;y<96;y++)for(let x=0;x<96;x++)if(!inside(headBoundary,x+.5,y+.5))pixels.data.fill(0,(y*96+x)*4,(y*96+x+1)*4);
  h.putImageData(pixels,0,0);
  const bind=project(restPose(0).head.center),ink='#625c60',outlineWidth=1.05;
  const body=createCanvas(384,384),mask=createCanvas(384,384),inner=createCanvas(384,384),detail=createCanvas(384,384);
  const bc=body.getContext('2d'),mc=mask.getContext('2d'),ic=inner.getContext('2d');
  const clear=c=>{c.resetTransform();c.globalCompositeOperation='source-over';c.clearRect(0,0,384,384);};
  function mass(ctx,m){const p=project(m.center);ctx.beginPath();ctx.ellipse(...p,...m.radii,0,0,Math.PI*2);ctx.fill();}
  function stroke(ctx,points,width,color){ctx.beginPath();points.forEach((p,i)=>i?ctx.lineTo(...p):ctx.moveTo(...p));ctx.lineWidth=width;ctx.strokeStyle=color;ctx.lineJoin=ctx.lineCap='round';ctx.stroke();}
  function silhouette(p){
    clear(mc);mc.scale(4,4);mc.fillStyle='#fff';
    stroke(mc,[project(p.chest.center),project(p.pelvis.center)],17,'#fff');
    mass(mc,p.chest);mass(mc,p.pelvis);
    stroke(mc,[project(p.neck.base),project(p.neck.tip)],9,'#fff');
    for(const l of p.legs)stroke(mc,l.joints.map(project),l.radii[0]*2,'#fff');
    // Outline the whole body/limb union, never individual chest/hip ellipses.
    // Root occlusion is an INK decision, not a hole cut in opaque white skin.
    // Erosion is inset, so neither haunch nor paw ink crosses the ground plane.
    clear(ic);ic.drawImage(mask,0,0);ic.globalCompositeOperation='destination-in';
    for(let n=0;n<16;n++){
      const angle=n*Math.PI/8;ic.drawImage(mask,Math.cos(angle)*outlineWidth*4,Math.sin(angle)*outlineWidth*4);
    }
    mc.resetTransform();mc.globalCompositeOperation='source-in';mc.fillStyle=ink;mc.fillRect(0,0,384,384);mc.globalCompositeOperation='source-over';
    bc.save();bc.resetTransform();bc.drawImage(mask,0,0);bc.drawImage(inner,0,0);bc.restore();
  }
  function limb(ctx,l){
    const points=l.joints.map(project);
    ctx.save();
    if(l.near){
      const [root,middle]=points,dx=middle[0]-root[0],dy=middle[1]-root[1],len=Math.hypot(dx,dy);
      const ux=len?dx/len:0,uy=len?dy/len:1,w=l.radii[0]+.5,inset=Math.min(1.2,len*.15);
      const local=(across,along)=>[root[0]-uy*across+ux*along,root[1]+ux*across+uy*along];
      ctx.beginPath();ctx.rect(0,0,96,96);ctx.moveTo(...local(-w,-w));ctx.lineTo(...local(w,-w));ctx.lineTo(...local(w,0));
      ctx.quadraticCurveTo(...local(0,2*inset),...local(-w,0));ctx.closePath();ctx.clip('evenodd');
    }
    stroke(ctx,points,l.radii[0]*2,ink);stroke(ctx,points,l.radii[0]*2-2*outlineWidth,'#fff');
    ctx.restore();
  }
  function render(p,{scale=1}={}){
    if(![1,2,3,4].includes(scale))throw Error('Render scale must be1,2,3 or4');
    clear(bc);bc.scale(4,4);
    const order=(a,b)=>Number(a.front)-Number(b.front);
    silhouette(p);
    // Far limbs belong behind the body. Near limb interior edges own the
    // foreground (hind then front), without redrawing/doubling the outer ink.
    const dc=detail.getContext('2d');clear(dc);dc.scale(4,4);
    p.legs.filter(l=>l.near).sort(order).forEach(l=>limb(dc,l));
    dc.resetTransform();dc.globalCompositeOperation='destination-in';dc.drawImage(inner,0,0);
    bc.save();bc.resetTransform();bc.drawImage(detail,0,0);bc.restore();
    const out=createCanvas(96*scale,96*scale),ctx=out.getContext('2d');ctx.scale(scale,scale);
    ctx.drawImage(body,0,0,96,96);
    // One rigid transform from the immutable original; never transform a prior
    // frame. No image-to-image dissolve or animated head scaling is possible.
    const target=project(p.head.center);ctx.translate(...target);ctx.rotate(-p.head.angle);ctx.translate(-bind[0],-bind[1]);
    ctx.drawImage(head,0,0);
    return out;
  }
  return {render};
}
module.exports={create};

'use strict';
// Artwork first: immutable canonical paint is partitioned without tracing it
// into substitute ellipses/capsules. Joints bind INSIDE this drawing. This is
// a rest-artwork study, not a finished sitting animation or product bank.
const headBoundary=[[0,0],[96,0],[96,48],[69,48],[69,51],[67,55],[65,59],[63,63],[60,62],[58,58],[56,62],[52,66],[49,70],[46,72],[43,72],[42,66],[23,66],[21,68],[19,69],[15,68],[0,68]];
function inside(poly,x,y){let yes=false;for(let i=0,j=poly.length-1;i<poly.length;j=i++){
  const a=poly[i],b=poly[j];if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])yes=!yes;
}return yes;}
function definition(){
  // Image coordinates; root/bend/tip are internal landmarks, not extra paws.
  // The absent far-hind drawing stays occluded: no cloned paw or new outline.
  return {
    head:{pivot:[36,60]},torso:{chest:[34,66],pelvis:[62,62]},
    legs:[
      {name:'farFront',front:true,visible:true,joints:[[22.5,68.5],[23,73.5],[23.5,78.5]]},
      {name:'farHind',front:false,visible:false,joints:[[58,62],[58,69],[58,76]]},
      {name:'nearHind',front:false,visible:true,joints:[[64,62],[65,70],[65,78]]},
      {name:'nearFront',front:true,visible:true,joints:[[40.5,68],[41.5,75],[42.5,82]]}
    ],depth:['farHind','farFront','torso','nearHind','nearFront','head']
  };
}
function create(standing,{createCanvas}){
  if(!standing||standing.width!==96||standing.height!==96)throw Error('Canonical 96x96 standing image required');
  const original=createCanvas(96,96);original.getContext('2d').drawImage(standing,0,0);
  const rgba=original.getContext('2d').getImageData(0,0,96,96).data.slice(),rig=definition();
  const names=rig.depth.filter(name=>name!=='farHind'),owners=new Int8Array(96*96).fill(-1);
  const layers=names.map(name=>{const canvas=createCanvas(96,96);return {name,canvas,image:canvas.getContext('2d').createImageData(96,96)};});
  const byName=new Map(layers.map(l=>[l.name,l]));
  function classify(x,y){
    if(inside(headBoundary,x+.5,y+.5))return 'head';
    // The belly and broad outer haunch stay on the body. Only the narrower
    // shafts below their emergence belong to a limb; a rectangular quarter
    // of the rump is NOT an upper hindleg.
    // Front legs taper from wide shoulders into the original small paws.
    // Keep their exterior root ink on the limb, not on a rotating torso cut.
    if(x<29&&y>=68)return 'farFront';
    if(x>=30&&x<51&&(y>=74||(x>=34&&y>=70)))return 'nearFront';
    if(x>=58&&y>=75)return 'nearHind';
    return 'torso';
  }
  for(let y=0;y<96;y++)for(let x=0;x<96;x++){
    const i=y*96+x;if(!rgba[i*4+3])continue;
    const name=classify(x,y);owners[i]=names.indexOf(name);
    byName.get(name).image.data.set(rgba.subarray(i*4,i*4+4),i*4);
  }
  for(const l of layers)l.canvas.getContext('2d').putImageData(l.image,0,0);
  const ownerAt=(x,y)=>names[owners[Math.floor(y)*96+Math.floor(x)]]||null;
  function transformPoint(name,point,{rootAngle=0,bend=0}={}){
    if(!Number.isFinite(rootAngle)||!Number.isFinite(bend))throw Error('Finite joint angles required');
    const l=rig.legs.find(l=>l.name===name);if(!l)throw Error('Visible leg required');
    if(!l.visible)throw Error('Occluded leg has no paint');
    const [root,knee]=l.joints,dx=knee[0]-root[0],dy=knee[1]-root[1],length=Math.hypot(dx,dy);
    const distance=((point[0]-root[0])*dx+(point[1]-root[1])*dy)/length;
    // Bounded skin hinge around the bend, never a terminal foot transform.
    const weight=Math.max(0,Math.min(1,(distance-length+1.5)/3));
    const kx=point[0]-knee[0],ky=point[1]-knee[1];
    const bx=knee[0]+kx*Math.cos(bend)-ky*Math.sin(bend),by=knee[1]+kx*Math.sin(bend)+ky*Math.cos(bend);
    const px=point[0]+(bx-point[0])*weight-root[0],py=point[1]+(by-point[1])*weight-root[1];
    return [root[0]+px*Math.cos(rootAngle)-py*Math.sin(rootAngle),root[1]+px*Math.sin(rootAngle)+py*Math.cos(rootAngle)];
  }
  function render({scale=1}={}){
    if(!Number.isInteger(scale)||scale<1||scale>8)throw Error('Integer scale 1..8 required');
    // Reassemble the actual separated paint, not a rest-frame shortcut. Each
    // texel is owned once; no alpha accumulation, outline rebuild or body fill.
    const assembled=createCanvas(96,96),ac=assembled.getContext('2d'),image=ac.createImageData(96,96);
    for(const l of layers)for(let i=0;i<96*96;i++)if(owners[i]===names.indexOf(l.name))image.data.set(l.image.data.subarray(i*4,i*4+4),i*4);
    ac.putImageData(image,0,0);if(scale===1)return assembled;
    const out=createCanvas(96*scale,96*scale),ctx=out.getContext('2d');ctx.imageSmoothingEnabled=false;ctx.drawImage(assembled,0,0,out.width,out.height);return out;
  }
  function renderLayer(name,{rootAngle=0,bend=0}={}){
    const l=byName.get(name);if(!l)throw Error('Paint layer required');
    const out=createCanvas(96,96),ctx=out.getContext('2d');
    if(!rootAngle&&!bend){ctx.putImageData(l.image,0,0);return out;}
    // Isolated articulation diagnostic: deform original texel quads through
    // the bound hinge. A complete animated seam/occlusion pass comes later.
    for(let y=0;y<96;y++)for(let x=0;x<96;x++){
      const i=(y*96+x)*4,a=l.image.data[i+3];if(!a)continue;
      const p=[[x,y],[x+1,y],[x+1,y+1],[x,y+1]].map(q=>transformPoint(name,q,{rootAngle,bend}));
      ctx.fillStyle=`rgba(${l.image.data[i]},${l.image.data[i+1]},${l.image.data[i+2]},${a/255})`;
      ctx.beginPath();p.forEach((q,n)=>n?ctx.lineTo(...q):ctx.moveTo(...q));ctx.closePath();ctx.fill();
    }
    return out;
  }
  return {rig,layers,ownerAt,render,renderLayer,transformPoint};
}
module.exports={create};

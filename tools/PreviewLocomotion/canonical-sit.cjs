'use strict';
// INCOMPLETE LOCAL ARTWORK STUDY: socket coverage and seated appearance remain
// open. Do not install or present this renderer as a finished correction.
// Original-bound, two-bone sitting. No seated raster, endpoint contour morph,
// geometric substitute body or independent ankle transform is accepted here.
const {create:createBind}=require('./canonical-bind.cjs');
const clamp=x=>Math.max(0,Math.min(1,x));
const smooth=x=>{x=clamp(x);return x*x*x*(10+x*(-15+6*x));};
const span=(t,a,b)=>smooth((t-a)/(b-a));
const pulse=(t,a,m,b)=>t<m?span(t,a,m):1-span(t,m,b);
const distance=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1]);
const angle=(a,b)=>Math.atan2(b[1]-a[1],b[0]-a[0]);
function matrix(from,to,rotation=0){const c=Math.cos(rotation),s=Math.sin(rotation);return [c,s,-s,c,to[0]-c*from[0]+s*from[1],to[1]-s*from[0]-c*from[1]];}
const apply=(m,p)=>[m[0]*p[0]+m[2]*p[1]+m[4],m[1]*p[0]+m[3]*p[1]+m[5]];
const bone=(a,b,c,d)=>matrix(a,c,angle(c,d)-angle(a,b));
function solve(root,tip,upper,lower){
  const dx=tip[0]-root[0],dy=tip[1]-root[1],d=Math.hypot(dx,dy);
  if(d>upper+lower+1e-9||d<Math.abs(upper-lower)-1e-9)throw Error('Unreachable original-bound hindleg');
  const along=(upper*upper-lower*lower+d*d)/(2*d),across=Math.sqrt(Math.max(0,upper*upper-along*along));
  return [root[0]+dx/d*along+dy/d*across,root[1]+dy/d*along-dx/d*across];
}
function create(standing,{createCanvas}){
  const bind=createBind(standing,{createCanvas}),rest=bind.rig;
  const spineLength=distance(rest.torso.chest,rest.torso.pelvis);
  const sourceLegs=new Map(rest.legs.map(l=>[l.name,l]));
  function pose(time,direction='sit'){
    if(!Number.isFinite(time))throw Error('Time must be finite');
    if(!['sit','rise'].includes(direction))throw Error('Invalid direction');
    const t=clamp(time),rise=direction==='rise';
    const fold=rise?1-span(t,.2,.74):span(t,.34,.72)**2;
    const support=rise?.7*pulse(t,0,.18,.82):2*pulse(t,0,.2,.76);
    const dx=-support-2*fold,near=sourceLegs.get('nearFront'),tip=near.joints[2];
    const foreLength=distance(near.joints[0],tip),rootX=near.joints[0][0]+dx;
    const rootY=tip[1]-Math.sqrt(foreLength**2-(tip[0]-rootX)**2);
    const chest=[rest.torso.chest[0]+dx,rootY-(near.joints[0][1]-rest.torso.chest[1])];
    const settle=rise?0:.65*pulse(t,.72,.8,.96);
    // The original near-straight hindleg needs a small hip preload when the
    // chest leans; do not silently stretch it to reach a planted paw.
    const hipY=rest.torso.pelvis[1]+8*fold+.25*support-settle;
    const pelvis=[chest[0]+Math.sqrt(spineLength**2-(hipY-chest[1])**2),hipY];
    const torsoMatrix=bone(rest.torso.chest,rest.torso.pelvis,chest,pelvis),legs=[];
    for(const l of rest.legs){
      if(!l.visible){legs.push({...l,joints:l.joints.map(q=>apply(torsoMatrix,q)),lift:0});continue;}
      const lengths=[distance(l.joints[0],l.joints[1]),distance(l.joints[1],l.joints[2])];
      if(l.front){
        const foot=l.joints[2].slice(),x=l.joints[0][0]+dx,length=lengths[0]+lengths[1];
        const root=[x,foot[1]-Math.sqrt(length**2-(foot[0]-x)**2)],ratio=lengths[0]/length;
        legs.push({...l,joints:[root,[root[0]+(foot[0]-root[0])*ratio,root[1]+(foot[1]-root[1])*ratio],foot],lift:0});
      }else{
        const u=rise?1-span(t,.08,.56):span(t,.3,.68),lift=3.2*Math.sin(Math.PI*u)**2;
        const root=apply(torsoMatrix,l.joints[0]),foot=[l.joints[2][0]-13.5*u,l.joints[2][1]-lift];
        legs.push({...l,joints:[root,solve(root,foot,...lengths),foot],lift});
      }
    }
    const pitch=rise?-.04*pulse(t,.025,.23,.92):-.04*pulse(t,.03,.25,.85)-.015*pulse(t,.8,.9,1);
    const neck=rest.head.pivot.map((v,i)=>v-rest.torso.chest[i]);
    const headPivot=[chest[0]+neck[0]*Math.cos(pitch)-neck[1]*Math.sin(pitch),chest[1]+neck[0]*Math.sin(pitch)+neck[1]*Math.cos(pitch)];
    return {time:t,direction,fold,chest,pelvis,head:{pivot:headPivot,angle:pitch},legs};
  }
  // Split only the original near-hind paint at its knee. This adds no drawing
  // and no terminal segment. Every rest texel still has one owner.
  const paint=[];
  for(const layer of bind.layers){
    if(layer.name!=='nearHind'){paint.push({name:layer.name,canvas:layer.canvas,data:layer.image.data});continue;}
    for(const part of ['upper','lower']){
      const canvas=createCanvas(96,96),ctx=canvas.getContext('2d'),image=ctx.createImageData(96,96);
      for(let y=0;y<96;y++)for(let x=0;x<96;x++)if((y<74)===(part==='upper')){const i=(y*96+x)*4;image.data.set(layer.image.data.subarray(i,i+4),i);}
      ctx.putImageData(image,0,0);paint.push({name:`nearHind.${part}`,canvas,data:image.data});
    }
  }
  const paintMap=new Map(paint.map(p=>[p.name,p]));
  const rgba=createCanvas(96,96);rgba.getContext('2d').drawImage(standing,0,0);
  const source=rgba.getContext('2d').getImageData(0,0,96,96).data;
  // Complete only OCCLUDED body tissue inside the original standing drawing.
  // Visible torso texels remain immutable. This is a fixed artwork plate,
  // never a union/closing operation on a changing animation silhouette.
  const body=createCanvas(96,96),bc=body.getContext('2d'),bi=bc.createImageData(96,96);
  bi.data.set(paintMap.get('torso').data);
  const bodyCore=[[19,64],[23,58],[33,55],[51,50],[64,47],[70,48],
    [74,52],[76,59],[73,69],[70,75],[66,78],[60,76],[51,75],[43,74],
    [35,73],[31,75],[26,74],[21,69]];
  const inside=(poly,x,y)=>{let yes=false;for(let i=0,j=poly.length-1;i<poly.length;j=i++){
    const a=poly[i],b=poly[j];if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])yes=!yes;
  }return yes;};
  for(let y=0;y<96;y++)for(let x=0;x<96;x++){
    const i=(y*96+x)*4;
    // Only fully opaque original cover may conceal new underpaint at rest.
    // Keep antialiased exterior pixels single-owned, preventing white halos.
    if(!bi.data[i+3]&&source[i+3]===255&&inside(bodyCore,x+.5,y+.5))bi.data.set(['head','nearHind'].includes(bind.ownerAt(x,y))?[255,255,255,255]:source.subarray(i,i+4),i);
  }bc.putImageData(bi,0,0);
  // Close only the hidden bottom of the haunch in artwork space. The moving
  // paw covers this short curve at rest; the original outer rump is unchanged.
  const closure=createCanvas(96,96),cc=closure.getContext('2d');
  cc.fillStyle='#fff';cc.beginPath();cc.moveTo(60,68);cc.lineTo(68,68);cc.lineTo(68,74);
  cc.bezierCurveTo(70,78,65,80,60,75);cc.closePath();cc.fill();
  cc.strokeStyle='#645458';cc.lineWidth=1.1;cc.lineCap='round';cc.beginPath();
  cc.moveTo(68,74);cc.bezierCurveTo(70,78,65,80,60,75);cc.stroke();
  const ci=cc.getImageData(0,0,96,96);
  for(let i=0;i<ci.data.length;i+=4)if(source[i+3]!==255||bind.ownerAt(i/4%96,Math.floor(i/4/96))!=='nearHind')ci.data.fill(0,i,i+4);
  cc.putImageData(ci,0,0);bc.drawImage(closure,0,0);
  // The shoulder stays upright over the planted forelegs; rotating that entire
  // front silhouette with the pelvis would pull its socket away from the paw.
  // Two fixed original-art plates overlap only in opaque interior body paint.
  const bodyParts=[];
  const complete=bc.getImageData(0,0,96,96).data;
  for(const name of ['chest','haunch']){
    const c=createCanvas(96,96),cx=c.getContext('2d'),image=cx.createImageData(96,96);
    for(let y=0;y<96;y++)for(let x=0;x<96;x++){
      const i=(y*96+x)*4;
      if(name==='chest'?x<52:(x>=52||(x>=48&&complete[i+3]===255)))image.data.set(complete.subarray(i,i+4),i);
    }cx.putImageData(image,0,0);bodyParts.push({name,canvas:c});
  }
  // Occluded limb shafts are complete fixed artwork, not stretched seams.
  // Source-colored opaque underpaint preserves exact standing RGBA; head ink
  // is not copied onto legs. Only existing fully opaque cover is duplicated.
  for(const [name,a,b,width]of[
    ['farFront',[22.5,68.5],[23,74],8],['nearFront',[40.5,68],[41.5,76],13],
    ['nearHind.upper',[64,62],[65,70],9],['nearHind.lower',[65,70],[65,78],10]
  ]){
    const layer=paintMap.get(name),c=createCanvas(96,96),cx=c.getContext('2d');
    cx.lineWidth=width;cx.strokeStyle='#fff';cx.lineCap='round';cx.beginPath();cx.moveTo(...a);cx.lineTo(...b);cx.stroke();
    if(name==='nearFront'){
      // Original shoulder tapers from the broad upper white drawing into the
      // narrow paw. Its hidden left socket is not a13px vertical rectangle.
      cx.fillStyle='#fff';cx.beginPath();[[30,66],[48,66],[49,75],[48,79],[35,79],[31,76],[26,74],[28,70]].forEach((q,n)=>n?cx.lineTo(...q):cx.moveTo(...q));cx.closePath();cx.fill();
    }
    const under=cx.getImageData(0,0,96,96);
    for(let y=0;y<96;y++)for(let x=0;x<96;x++){
      const i=(y*96+x)*4;
      if(source[i+3]!==255||!under.data[i+3]){under.data.fill(0,i,i+4);continue;}
      if(bind.ownerAt(x,y)!=='head')under.data.set(source.subarray(i,i+3),i);
    }
    cx.putImageData(under,0,0);cx.drawImage(layer.canvas,0,0);
    layer.canvas=c;layer.data=cx.getImageData(0,0,96,96).data;
  }
  const rootCuts=[];
  for(let y=68;y<76;y++)for(let x=26;x<51;x++)for(const [dx,dy]of[[1,0],[0,1]]){
    const a=bind.ownerAt(x,y),b=bind.ownerAt(x+dx,y+dy),i=(y*96+x)*4,j=((y+dy)*96+x+dx)*4;
    if(a===b||![a,b].includes('torso')||![a,b].some(n=>['farFront','nearFront'].includes(n)))continue;
    if(source[i+3]!==255||source[j+3]!==255||Math.min(...source.slice(i,i+3),...source.slice(j,j+3))<230)continue;
    rootCuts.push({a,b,edge:dx?[[x+1,y],[x+1,y+1]]:[[x,y+1],[x+1,y+1]]});
  }
  function transforms(p){
    const result=[{name:'torso',matrix:bone(rest.torso.chest,rest.torso.pelvis,p.chest,p.pelvis),from:[rest.torso.chest,rest.torso.pelvis],to:[p.chest,p.pelvis]},
      {name:'head',matrix:matrix(rest.head.pivot,p.head.pivot,p.head.angle)}];
    for(const l of p.legs.filter(l=>l.visible)){
      const from=sourceLegs.get(l.name).joints;
      if(l.front)result.push({name:l.name,matrix:bone(from[0],from[2],l.joints[0],l.joints[2]),from:[from[0],from[2]],to:[l.joints[0],l.joints[2]]});
      else for(let j=0;j<2;j++)result.push({name:`${l.name}.${j?'lower':'upper'}`,matrix:bone(from[j],from[j+1],l.joints[j],l.joints[j+1]),from:[from[j],from[j+1]],to:[l.joints[j],l.joints[j+1]]});
    }return result;
  }
  function bodyMatrix(p,name){return name==='chest'?matrix(rest.torso.chest,p.chest):bone(rest.torso.chest,rest.torso.pelvis,p.chest,p.pelvis);}
  function renderBody(p,{scale=1,part=null}={}){
    if(!Number.isInteger(scale)||scale<1||scale>4)throw Error('Integer scale 1..4 required');
    const out=createCanvas(96*scale,96*scale),ctx=out.getContext('2d');ctx.scale(scale,scale);
    ctx.imageSmoothingEnabled=false;
    for(const layer of [...bodyParts].reverse())if(!part||layer.name===part){ctx.save();ctx.transform(...bodyMatrix(p,layer.name));ctx.drawImage(layer.canvas,0,0);ctx.restore();}return out;
  }
  function renderPose(p,{scale=1}={}){
    if(!Number.isInteger(scale)||scale<1||scale>4)throw Error('Integer scale 1..4 required');
    const matrices=new Map(transforms(p).map(part=>[part.name,part.matrix]));
    const out=createCanvas(384,384),ctx=out.getContext('2d');ctx.scale(4,4);ctx.imageSmoothingEnabled=false;
    const draw=(name,image=paintMap.get(name).canvas)=>{ctx.save();ctx.transform(...matrices.get(name));ctx.drawImage(image,0,0);ctx.restore();};
    // Tiny white interior root underlaps only. A hard2.5px limit prevents this
    // from inventing broad wedges or treating the entire rump as a seam.
    ctx.fillStyle='#fff';ctx.beginPath();
    for(const cut of rootCuts){
      const m=name=>name==='torso'?bodyMatrix(p,'chest'):matrices.get(name),a=cut.edge.map(q=>apply(m(cut.a),q)),b=cut.edge.map(q=>apply(m(cut.b),q));
      const gap=Math.max(distance(a[0],b[0]),distance(a[1],b[1]));if(gap<1e-8||gap>2.5)continue;
      [a[0],a[1],b[1],b[0]].forEach((q,n)=>n?ctx.lineTo(...q):ctx.moveTo(...q));ctx.closePath();
    }ctx.fill();
    // Far limb is behind the complete body. Near limbs own the foreground.
    // No broad seam wedges, global silhouette closing, erosion or recoloring.
    draw('farFront');
    for(const layer of [...bodyParts].reverse()){ctx.save();ctx.transform(...bodyMatrix(p,layer.name));ctx.drawImage(layer.canvas,0,0);ctx.restore();}
    for(const name of ['nearHind.upper','nearHind.lower','nearFront'])draw(name);
    draw('head');
    if(scale===4)return out;
    const target=createCanvas(96*scale,96*scale);target.getContext('2d').drawImage(out,0,0,target.width,target.height);return target;
  }
  const render=(time,{direction='sit',scale=1}={})=>renderPose(pose(time,direction),{scale});
  return {bind,pose,transforms,render,renderPose,renderBody};
}
module.exports={create};

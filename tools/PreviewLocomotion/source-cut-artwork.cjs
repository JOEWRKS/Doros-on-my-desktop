'use strict';
// Original exposed paint plus independently authored hidden attachment skin.
// Bounded rigid joint diagnostics only, not a standing-to-sitting transition.
const headPolygon=require('./original-contour-data.json').headPolygon;
const depth=['farFront','torso','nearHind','nearFront','head'];
const angleLimits={head:8,farFront:12,nearFront:12,nearHind:18};
const attachments={
  standing:{
    pivots:{head:[36,60],farFront:[23,70],nearFront:[41,73],nearHind:[64,75]},
    skin:{
      torso:[[20,62],[56,54],[65,57],[68,65],[66,74],[61,73],[54,73],[48,75],[33,74],[25,74],[20,68]],
      farFront:[[20,60],[26,60],[26,76],[20,76]],
      nearFront:[[35,60],[46,60],[46,76],[35,76]],
      nearHind:[[61,58],[68,58],[68,78],[61,78]]
    }
  },
  seated:{
    pivots:{head:[36,60],farFront:[24,72],nearFront:[36,72],nearHind:[58,81]},
    skin:{
      torso:[[20,62],[56,54],[62,58],[66,68],[66,81],[52,84],[44,77],[30,74],[22,72]],
      farFront:[[21,61],[27,61],[27,79],[21,79]],
      nearFront:[[32,60],[42,60],[42,79],[32,79]],
      nearHind:[[51,72],[65,72],[65,85],[51,85]]
    }
  }
};
function inside(poly,x,y){
  let hit=false;
  for(let i=0,j=poly.length-1;i<poly.length;j=i++){
    const a=poly[i],b=poly[j];
    if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])hit=!hit;
  }
  return hit;
}
function ownerAt(pose,x,y){
  if(inside(headPolygon,x+.5,y+.5))return 'head';
  if(pose==='standing'){
    if(x<29&&y>=69)return 'farFront';
    if(x>=30&&x<51&&(y>=74||(x>=34&&y>=70)))return 'nearFront';
    if(x>=58&&y>=75)return 'nearHind';
  }else{
    if(x<29&&y>=69)return 'farFront';
    if(x>=29&&x<46&&y>=68)return 'nearFront';
    if(x>=46&&(y>=82||(x<57&&y>=77)))return 'nearHind';
  }
  return 'torso';
}
function cutArtwork(source,pose,{createCanvas}){
  if(!['standing','seated'].includes(pose))throw Error('standing or seated source required');
  const size=pose==='standing'?96:100;
  if(source.width!==size||source.height!==size)throw Error(`Expected ${size}x${size} ${pose} source`);
  const offset=pose==='standing'?[0,0]:[3,-12];
  const original=createCanvas(96,96),ctx=original.getContext('2d');
  // Integer translation only: retain the source head/body ratio and source AA.
  ctx.drawImage(source,...offset);
  const rgba=ctx.getImageData(0,0,96,96).data;
  const layers=depth.map(name=>{
    const coords=[];
    for(let y=0;y<96;y++)for(let x=0;x<96;x++)if(rgba[(y*96+x)*4+3]&&ownerAt(pose,x,y)===name)coords.push([x,y]);
    if(!coords.length)throw Error(`Missing source part: ${name}`);
    const x0=Math.min(...coords.map(p=>p[0])),y0=Math.min(...coords.map(p=>p[1]));
    const width=Math.max(...coords.map(p=>p[0]))-x0+1,height=Math.max(...coords.map(p=>p[1]))-y0+1;
    const image=createCanvas(width,height),c=image.getContext('2d'),pixels=c.createImageData(width,height);
    for(const [x,y] of coords)pixels.data.set(rgba.subarray((y*96+x)*4,(y*96+x)*4+4),((y-y0)*width+x-x0)*4);
    c.putImageData(pixels,0,0);
    return {name,offset:[x0,y0],image};
  });
  function render(){
    const out=createCanvas(96,96),c=out.getContext('2d');
    for(const layer of layers)c.drawImage(layer.image,...layer.offset);
    return out;
  }
  const config=attachments[pose];
  // Bind a small skin strip to each side of every actual opaque source cut.
  // Its two rims follow their own rigid part, so the joint surface remains
  // continuous instead of hoping that two independently rotated cutouts overlap.
  const seams=[];
  for(let y=58;y<95;y++)for(let x=0;x<95;x++)for(const [dx,dy] of [[1,0],[0,1]]){
    const a=y*96+x,b=(y+dy)*96+x+dx,first=ownerAt(pose,x,y),second=ownerAt(pose,x+dx,y+dy);
    if(rgba[a*4+3]<250||rgba[b*4+3]<250||first===second)continue;
    seams.push({first,second,normal:[dx,dy],edge:dx?[[x+1,y],[x+1,y+1]]:[[x,y+1],[x+1,y+1]]});
  }
  const skinLayers=Object.entries(config.skin).map(([name,polygon])=>{
    const image=createCanvas(96,96),c=image.getContext('2d');
    c.fillStyle='#fff';c.beginPath();polygon.forEach((p,i)=>i?c.lineTo(...p):c.moveTo(...p));c.closePath();c.fill();
    return {name,image};
  });
  function renderArticulated(angles={},options={}){
    for(const [name,value] of Object.entries(angles)){
      if(!(name in angleLimits))throw Error('Unknown joint');
      if(!Number.isFinite(value))throw Error('Joint angle must be finite');
      if(Math.abs(value)>angleLimits[name])throw Error(`${name} outside verified angle range`);
    }
    const out=createCanvas(96,96),c=out.getContext('2d');c.imageSmoothingEnabled=false;
    const paint=(name,image,offset)=>{
      c.save();const p=config.pivots[name]||[0,0];
      c.translate(...p);c.rotate((angles[name]||0)*Math.PI/180);c.translate(-p[0],-p[1]);c.drawImage(image,...offset);c.restore();
    };
    // Hidden skin has no ink and sits behind the exposed source paint. It is
    // authored before posing; no post-render dilation, hole filling or alpha mask.
    if(options.hiddenSkin!==false)for(const l of skinLayers)paint(l.name,l.image,[0,0]);
    if(options.hiddenSkin!==false){
      const move=(p,name)=>{
        const root=config.pivots[name]||[0,0],a=(angles[name]||0)*Math.PI/180,s=Math.sin(a),co=Math.cos(a),x=p[0]-root[0],y=p[1]-root[1];
        return [root[0]+x*co-y*s,root[1]+x*s+y*co];
      };
      c.fillStyle='#fff';c.beginPath();
      for(const seam of seams){
        const n=seam.normal,first=seam.edge.map(p=>move([p[0]-.75*n[0],p[1]-.75*n[1]],seam.first));
        const second=seam.edge.map(p=>move([p[0]+.75*n[0],p[1]+.75*n[1]],seam.second));
        c.moveTo(...first[0]);c.lineTo(...first[1]);c.lineTo(...second[1]);c.lineTo(...second[0]);c.closePath();
      }
      c.fill();
    }
    for(const l of layers.filter(l=>l.name!=='head'))paint(l.name,l.image,l.offset);
    if(options.traced){
      const skinOutline=require('./source-skin-contour.cjs').paintSkin(c);
      if(pose==='seated'){
        // The source's one visible thigh crease, kept separate from the
        // exterior silhouette. A cut edge is never inked as a second crease.
        c.save();c.beginPath();skinOutline.forEach((p,i)=>i?c.lineTo(...p):c.moveTo(...p));c.closePath();c.clip();
        const p=config.pivots.nearHind;
        c.translate(...p);c.rotate((angles.nearHind||0)*Math.PI/180);c.translate(-p[0],-p[1]);
        c.strokeStyle='rgb(68,52,56)';c.lineWidth=1.15;c.lineCap='round';c.lineJoin='round';
        c.beginPath();c.moveTo(44.5,85.2);c.bezierCurveTo(45.3,81.7,46.4,78.1,47.6,76.6);c.lineTo(51,76);c.stroke();c.restore();
      }
    }
    const head=layers.find(l=>l.name==='head');paint(head.name,head.image,head.offset);
    return out;
  }
  const renderTraced=(angles={})=>renderArticulated(angles,{traced:true});
  return {original,layers,render,renderArticulated,renderTraced,skinLayers,pivots:structuredClone(config.pivots),angleLimits:{...angleLimits},sourceOffset:offset};
}
module.exports={cutArtwork};

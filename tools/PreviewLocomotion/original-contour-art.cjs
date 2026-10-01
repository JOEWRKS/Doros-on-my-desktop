'use strict';
const data=require('./original-contour-data.json');
function rotate(point,origin,target,angle){
  const x=point[0]-origin[0],y=point[1]-origin[1],c=Math.cos(angle),s=Math.sin(angle);
  return [target[0]+x*c-y*s,target[1]+x*s+y*c];
}
function angle(a,b){return Math.atan2(b[1]-a[1],b[0]-a[0]);}
function mapper(name,m,pose){
  if(name==='torso')return p=>{
    const anchor=p[2]||'chest',a=m.standing[anchor],b=pose[anchor];
    return [p[0]+b[0]-a[0],p[1]+b[1]-a[1]];
  };
  const a=m.standing.legs.find(l=>l.name===name).joints,b=pose.legs.find(l=>l.name===name).joints;
  const upper=angle(b[0],b[1])-angle(a[0],a[1]),lower=angle(b[1],b[2])-angle(a[1],a[2]);
  const dx=a[1][0]-a[0][0],dy=a[1][1]-a[0][1],length=Math.hypot(dx,dy);
  return p=>{
    const distance=((p[0]-a[0][0])*dx+(p[1]-a[0][1])*dy)/length;
    const w=Math.max(0,Math.min(1,(distance-length+2)/4));
    const u=rotate(p,a[0],b[0],upper),l=rotate(p,a[1],b[1],lower);
    return [u[0]+(l[0]-u[0])*w,u[1]+(l[1]-u[1])*w];
  };
}
/** Complete closed artwork, driven by two anatomical segments, not source alpha. */
function buildParts(m,pose){
  return Object.entries(data.artwork).map(([name,art])=>{
    const map=mapper(name,m,pose),path=commands=>commands.map(c=>({op:c.op,points:c.points.map(map)}));
    return {name,fill:path(art.fill),ink:art.ink.map(path),rootInk:(art.rootInk||[]).map(path),root:name==='torso'?pose.chest.slice():pose.legs.find(l=>l.name===name).joints[0].slice(),coverage:art.coverage.map(map)};
  });
}
function trace(ctx,commands,begin=true){
  if(begin)ctx.beginPath();for(const c of commands){const p=c.points.flat();
    if(c.op==='M')ctx.moveTo(...p);else if(c.op==='L')ctx.lineTo(...p);else if(c.op==='C')ctx.bezierCurveTo(...p);else if(c.op==='Z')ctx.closePath();else throw Error('unknown path command');
  }
}
function paintPart(ctx,part,m){
  trace(ctx,part.fill);ctx.fillStyle='#fff';ctx.fill();
  paintInk(ctx,part,m);
}
function paintInk(ctx,part,m){
  ctx.strokeStyle=`rgb(${m.ink.rgb.join(',')})`;ctx.lineWidth=m.ink.width;ctx.lineCap='round';ctx.lineJoin='round';
  for(const ink of [...(part.rootInk||[]),...part.ink]){trace(ctx,ink);ctx.stroke();}
}
function renderPart(part,m,{createCanvas}){
  const c=createCanvas(96,96);paintPart(c.getContext('2d'),part,m);return c;
}
module.exports={buildParts,renderPart,paintPart,paintInk,trace};

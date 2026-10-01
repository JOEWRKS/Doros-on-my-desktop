'use strict';
// Trace the actual assembled skin coverage. This does not displace vertices,
// scale body parts, close exterior notches, or guess an ellipse for the rump.
function contour(image){
  const {width:w,height:h,data}=image;
  const alpha=(x,y)=>x<0||y<0||x>=w||y>=h?0:data[(y*w+x)*4+3];
  const segments=[];
  for(let y=-1;y<h;y++)for(let x=-1;x<w;x++){
    const points=[[x+.5,y+.5],[x+1.5,y+.5],[x+1.5,y+1.5],[x+.5,y+1.5]];
    const values=[alpha(x,y),alpha(x+1,y),alpha(x+1,y+1),alpha(x,y+1)],cross=[];
    for(let e=0;e<4;e++){
      const next=(e+1)%4,a=values[e],b=values[next];
      if((a>=128)===(b>=128))continue;
      const t=(128-a)/(b-a);cross.push({edge:e,p:[points[e][0]+(points[next][0]-points[e][0])*t,points[e][1]+(points[next][1]-points[e][1])*t]});
    }
    if(cross.length===2)segments.push(cross.map(p=>p.p));
    else if(cross.length===4){
      const pairs=values[0]>=128?[[0,3],[1,2]]:[[0,1],[2,3]];
      for(const [a,b] of pairs)segments.push([cross[a].p,cross[b].p]);
    }
  }
  const key=p=>p.map(v=>v.toFixed(6)).join(','),adj=new Map(),used=new Set(),loops=[];
  segments.forEach((segment,i)=>segment.forEach(p=>{const k=key(p);if(!adj.has(k))adj.set(k,[]);adj.get(k).push(i);}));
  segments.forEach((segment,index)=>{
    if(used.has(index))return;
    used.add(index);const loop=segment.slice();let end=key(loop.at(-1));
    while(end!==key(loop[0])){
      const next=(adj.get(end)||[]).find(i=>!used.has(i));
      if(next===undefined)throw Error('Unclosed skin contour');
      used.add(next);const pair=segments[next],p=key(pair[0])===end?pair[1]:pair[0];loop.push(p);end=key(p);
    }
    loops.push(loop);
  });
  const area=loop=>Math.abs(loop.reduce((a,p,i)=>{const q=loop[(i+1)%loop.length];return a+p[0]*q[1]-q[0]*p[1];},0)/2);
  loops.sort((a,b)=>area(b)-area(a));
  if(!loops.length)throw Error('Empty skin');
  return {outline:loops[0],loops,area:area(loops[0])};
}
function trace(ctx,loop){ctx.beginPath();loop.forEach((p,i)=>i?ctx.lineTo(...p):ctx.moveTo(...p));ctx.closePath();}
function paintSkin(ctx){
  const outline=contour(ctx.getImageData(0,0,96,96)).outline;
  ctx.clearRect(0,0,96,96);trace(ctx,outline);ctx.fillStyle='#fff';ctx.fill();
  // Source alpha gives the outside edge of the ink. Place the measured stroke
  // inside that same edge, rather than growing the whole body by half a stroke.
  ctx.save();trace(ctx,outline);ctx.clip();ctx.lineJoin='round';ctx.lineCap='round';
  ctx.strokeStyle='rgb(68,52,56)';ctx.lineWidth=2.4;ctx.stroke();ctx.restore();
  return outline;
}
module.exports={contour,paintSkin};

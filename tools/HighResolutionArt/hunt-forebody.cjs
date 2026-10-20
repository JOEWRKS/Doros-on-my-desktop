'use strict';
// The product's approved gaze-compose.js foreleg replacement, evaluated at
// original-part resolution instead of enlarging its completed 96px output.
const hunt=require('../PreviewLocomotion/hunt-motion.js');
const smooth=v=>{v=Math.max(0,Math.min(1,v));return v*v*(3-2*v);};
function apply(body,pose,api,pen){
 if(pose.amount===0)return body;
 const size=body.width,k=size/96,region=api.createCanvas(size,size),mask=region.getContext('2d');mask.scale(k,k);
 mask.fillStyle='white';mask.fillRect(0,65,46,31);const edge=mask.createLinearGradient(46,0,52,0);edge.addColorStop(0,'white');edge.addColorStop(1,'transparent');mask.fillStyle=edge;mask.fillRect(46,65,6,31);
 const layer=api.createCanvas(size,size),g=layer.getContext('2d');g.scale(k,k);
 const point=(source,target)=>hunt.map(...source,pose).map((v,i)=>v+(target[i]-v)*pose.amount);
 const move=(a,b)=>g.moveTo(...point(a,b));
 const curve=(a,b,c,d,e,f)=>g.bezierCurveTo(...point(a,b),...point(c,d),...point(e,f));
 const outline=()=>{
  move([17,68],[23,68]);
  curve([17,73],[18,71],[18,78],[8,72],[21,80],[9,78]);
  curve([23,82],[9.5,82],[28,80],[17,82],[28,75],[24,76]);
  curve([30,75],[26,74],[33,78],[28,76],[35,78],[34,76]);
  curve([36,81],[31,78],[37,85],[25,79],[40,86],[27,83.5]);
  curve([43,88],[28.5,87],[47,86],[35,87],[48,81],[40,81]);
  curve([48,78],[42,79],[50,78],[47,77],[53,76],[53,76]);
 };
 g.beginPath();outline();g.lineTo(54,60);g.lineTo(14,60);g.closePath();g.fillStyle='white';g.fill();
 g.beginPath();outline();g.strokeStyle=`rgb(${pen.rgb.join(',')})`;g.lineWidth=pen.width;g.lineJoin=g.lineCap='round';g.stroke();
 g.resetTransform();g.globalCompositeOperation='destination-in';g.drawImage(region,0,0);
 const result=api.createCanvas(size,size),r=result.getContext('2d');r.drawImage(body,0,0);r.globalAlpha=smooth(pose.amount/.2);
 r.globalCompositeOperation='destination-out';r.drawImage(region,0,0);r.globalCompositeOperation='lighter';r.drawImage(layer,0,0);
 return result;
}
module.exports={apply};

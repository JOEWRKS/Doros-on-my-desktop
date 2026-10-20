// EXPERIMENT ONLY: rejected iris/eyelid remnants; never selected by setup.
'use strict';
const {pixels,premultiply,sample,put}=require('./sampling.cjs');
// The user-authored 96px expressions remain the shape authority. Transfer only
// their changed eye texels, never their old hair, mouth, body or entire head.
// This retains a low-resolution expression detail; it is not invented detail.
const eyeRegions=[[16,45,14,12],[32,48,15,12]];
function hull(points){
 const cross=(a,b,c)=>(b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0]);
 const sorted=points.sort((a,b)=>a[0]-b[0]||a[1]-b[1]),half=p=>{const s=[];for(const q of p){while(s.length>1&&cross(s.at(-2),s.at(-1),q)<=0)s.pop();s.push(q);}return s;};
 return half(sorted).slice(0,-1).concat(half([...sorted].reverse()).slice(0,-1));
}
function transfer(master,open,expression,api,{kind='closed'}={}){
 if(!['closed','squint'].includes(kind))throw Error('Unknown eye expression');
 if(master.width!==225||master.height!==225||open.width!==96||open.height!==96||expression.width!==96||expression.height!==96)throw Error('Expected 225px master and 96px authored expressions');
 const base=pixels(open,api),changed=pixels(expression,api),paths=[];
 for(const [x0,y0,w,h] of eyeRegions){const points=[];
  for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){
   const i=(y*96+x)*4;if([0,1,2,3].some(k=>base[i+k]!==changed[i+k]))
    for(const dy of [-.35,1.35])for(const dx of [-.35,1.35])points.push([x+dx,y+dy]);
  }if(points.length)paths.push(hull(points));
 }
 const out=api.createCanvas(225,225),g=out.getContext('2d');g.drawImage(master,0,0);
 const image=g.getImageData(0,0,225,225),originalPixels=pixels(master,api);
 const maskCanvas=api.createCanvas(225,225),mg=maskCanvas.getContext('2d');mg.scale(225/96,225/96);
 mg.fillStyle='#fff';for(const p of paths){mg.beginPath();p.forEach((q,i)=>i?mg.lineTo(...q):mg.moveTo(...q));mg.closePath();mg.fill();}
 const mask=pixels(maskCanvas,api);
 // The96px expression supplies the REGION and expression reference only.
 // Do not enlarge its raster eyelid/iris texels into a225px source layer.
 // Clear the eye interior while retaining source hair/cheek pink in the patch.
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const at=(y*225+x)*4,w=mask[at+3]/255;if(!w)continue;
  const [r,gg,b]=originalPixels.subarray(at,at+3);
  if(r>180&&r>gg+25&&r>b+10)continue;
  for(let k=0;k<3;k++)image.data[at+k]=Math.round(originalPixels[at+k]*(1-w)+255*w);
 }
 g.putImageData(image,0,0);
 const detail=api.createCanvas(225,225),dg=detail.getContext('2d');dg.scale(225/96,225/96);
 dg.strokeStyle='#50464f';dg.lineWidth=1.1;dg.lineCap=dg.lineJoin='round';
 if(kind==='closed'){
  dg.beginPath();dg.moveTo(17.1,50.2);dg.bezierCurveTo(19.0,53.8,22.0,54.4,25.4,53.7);dg.stroke();
  dg.beginPath();dg.moveTo(34.4,55.4);dg.bezierCurveTo(37.3,57.2,41.0,57.5,44.0,55.2);dg.stroke();
 }else{
  // Half blink keeps the original225 iris colour/texture. Compress only the
  // eye interior vertically under the authored lid, not the whole face.
  for(const [x,y,w,h] of [[17,47,11,10],[34,50,11,10]]){
   dg.save();dg.beginPath();dg.ellipse(x+w/2,y+h*.72,w*.48,h*.29,0,0,Math.PI*2);dg.clip();
   dg.drawImage(master,x*225/96,y*225/96,w*225/96,h*225/96,x,y+h*.43,w,h*.57);dg.restore();
  }
  dg.beginPath();dg.moveTo(17.7,50.8);dg.bezierCurveTo(20.1,50.6,23.3,52.3,26.8,53.4);dg.stroke();
  dg.beginPath();dg.moveTo(34.5,53.8);dg.bezierCurveTo(37.0,53.7,41.4,55.5,44.3,56.2);dg.stroke();
 }
 // Restrict the authored source-scale details to the same local eye support.
 const ink=pixels(detail,api),dst=g.getImageData(0,0,225,225);
 for(let i=0;i<ink.length;i+=4){const a=ink[i+3]/255*mask[i+3]/255;if(!a)continue;
  for(let k=0;k<3;k++)dst.data[i+k]=Math.round(dst.data[i+k]*(1-a)+ink[i+k]*a);
 }
 g.putImageData(dst,0,0);return out;
}
module.exports={transfer,eyeRegions};

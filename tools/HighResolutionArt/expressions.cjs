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
function transfer(master,open,expression,api){
 if(master.width!==225||master.height!==225||open.width!==96||open.height!==96||expression.width!==96||expression.height!==96)throw Error('Expected 225px master and 96px authored expressions');
 const base=pixels(open,api),changed=pixels(expression,api),paths=[];
 for(const [x0,y0,w,h] of eyeRegions){const points=[];
  for(let y=y0;y<y0+h;y++)for(let x=x0;x<x0+w;x++){
   const i=(y*96+x)*4;if([0,1,2,3].some(k=>base[i+k]!==changed[i+k]))
    for(const dy of [-.35,1.35])for(const dx of [-.35,1.35])points.push([x+dx,y+dy]);
  }if(points.length)paths.push(hull(points));
 }
 const out=api.createCanvas(225,225),g=out.getContext('2d');g.drawImage(master,0,0);
 const image=g.getImageData(0,0,225,225),src=premultiply(expression,api),p=new Float64Array(4);
 const maskCanvas=api.createCanvas(225,225),mg=maskCanvas.getContext('2d');mg.scale(225/96,225/96);
 mg.fillStyle='#fff';for(const p of paths){mg.beginPath();p.forEach((q,i)=>i?mg.lineTo(...q):mg.moveTo(...q));mg.closePath();mg.fill();}
 const mask=pixels(maskCanvas,api);
 // Known detail limit: this retains the authored96px expression. Source225
 // reconstruction attempts are isolated in expression-detail-candidate.cjs.
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const sx=(x+.5)*96/225-.5,sy=(y+.5)*96/225-.5;
  const at=(y*225+x)*4,w=mask[at+3]/255;if(!w)continue;
  sample(src,96,sx,sy,p);const original=[...image.data.subarray(at,at+4)];put(image.data,at,p);
  for(let k=0;k<4;k++)image.data[at+k]=Math.round(original[k]*(1-w)+image.data[at+k]*w);
 }
 g.putImageData(image,0,0);return out;
}
module.exports={transfer,eyeRegions};

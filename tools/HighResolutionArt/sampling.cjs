'use strict';
// One bounded cubic source lookup. Work in premultiplied RGBA to avoid halos.
function pixels(image,api){const c=api.createCanvas(image.width,image.height),g=c.getContext('2d');g.drawImage(image,0,0);return g.getImageData(0,0,c.width,c.height).data;}
function premultiply(image,api){const d=pixels(image,api),out=new Float64Array(d.length);for(let i=0;i<d.length;i+=4){const a=d[i+3]/255;for(let k=0;k<3;k++)out[i+k]=d[i+k]*a;out[i+3]=d[i+3];}return out;}
function weights(t){const t2=t*t,t3=t2*t;return [-.5*t+t2-.5*t3,1-2.5*t2+1.5*t3,.5*t+2*t2-1.5*t3,-.5*t2+.5*t3];}
function sample(data,size,x,y,out){
 const ix=Math.floor(x),iy=Math.floor(y),wx=weights(x-ix),wy=weights(y-iy);
 for(let c=3;c>=0;c--){let v=0,lo=255,hi=0;
  for(let yy=0;yy<4;yy++)for(let xx=0;xx<4;xx++){
   const sx=ix+xx-1,sy=iy+yy-1,a=sx>=0&&sx<size&&sy>=0&&sy<size?data[(sy*size+sx)*4+c]:0;
   v+=a*wx[xx]*wy[yy];if(xx===1||xx===2)if(yy===1||yy===2){lo=Math.min(lo,a);hi=Math.max(hi,a);}
  }
  out[c]=Math.max(0,Math.min(c===3?255:out[3],Math.max(lo,Math.min(hi,v))));
 }return out;
}
function over(bottom,top){const a=1-top[3]/255;for(let k=0;k<4;k++)bottom[k]=top[k]+bottom[k]*a;return bottom;}
function put(data,at,p){const a=p[3];data[at+3]=Math.round(a);for(let k=0;k<3;k++)data[at+k]=a?Math.round(p[k]*255/a):0;}
function surface(api,size,paint,padding=0,refine=null){
 const extent=Math.ceil(size+2*padding*size/96);
 const c=api.createCanvas(extent,extent),g=c.getContext('2d'),d=g.createImageData(extent,extent),sum=new Float64Array(4);
 // Integrate each output pixel directly from the original source. This is
 // coverage sampling, not blurring or resizing an already rendered frame.
 for(let y=0;y<extent;y++)for(let x=0;x<extent;x++){
  sum.fill(0);for(const dy of [.25,.75])for(const dx of [.25,.75]){
   const p=paint((x+dx)*96/size-padding,(y+dy)*96/size-padding);for(let k=0;k<4;k++)sum[k]+=p[k]/4;
  }put(d.data,(y*extent+x)*4,sum);
 }if(refine)refine(d.data,extent,extent,size/96,padding);g.putImageData(d,0,0);return c;
}
module.exports={pixels,premultiply,sample,over,put,surface};

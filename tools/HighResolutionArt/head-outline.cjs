'use strict';
// Output-only, source-conditioned contrast recovery. No new stroke, dilation,
// source texture edit or alpha change. Coordinates below are in source225.
const {sample}=require('./sampling.cjs');
const clamp=(x,a,b)=>Math.max(a,Math.min(b,x));
function region(x,y){
 // Lower head contour only. Eyes, mouth, cheeks and upper bangs are excluded.
 return (x>=40&&x<=108&&y>=148&&y<=158)||
  (x>=100&&x<=121&&y>=153&&y<=171)||
  (x>=28&&x<=50&&y>=153&&y<=168);
}
function create(head,toHead){
 const p=new Float64Array(4);
 return function refine(data,width,height,scale,padding){
  const original=new Uint8ClampedArray(data);
  for(let y=1;y<height-1;y++)for(let x=1;x<width-1;x++){
   const [hx,hy]=toHead((x+.5)/scale-padding,(y+.5)/scale-padding),sx=hx*225/96-.5,sy=hy*225/96-.5;
   if(!region(sx,sy))continue;
   sample(head,225,sx,sy,p);
   // Limit recovery to existing neutral ink, not pink hair or white skin.
   const alpha=p[3]/255;if(alpha<.95)continue;
   const rgb=[p[0]/alpha,p[1]/alpha,p[2]/alpha],hi=Math.max(...rgb),lo=Math.min(...rgb);
   const weight=clamp((190-hi)/65,0,1)*clamp((48-(hi-lo))/20,0,1);
   if(!weight)continue;
   const at=(y*width+x)*4;
   if(original[at+3]!==255)continue;
   const l=k=>(original[k]+original[k+1]+original[k+2])/3;
   const center=l(at),neighbors=(l(at-4)+l(at+4)+l(at-width*4)+l(at+width*4))/4;
   // Recover line core contrast; never extend the ink's outer grey fringe.
   const delta=clamp((center-neighbors)*.55,-26,0)*weight;
   for(let c=0;c<3;c++)data[at+c]=clamp(original[at+c]+delta,0,255);
  }
 };
}
module.exports={create,region};

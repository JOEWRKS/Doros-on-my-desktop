'use strict';
// Source225 boundary, traced against the hair tips and the complete white
// ribbon. The old96 polygon cut through the ribbon and included shoulder skin.
const headPolygon=[[0,0],[225,0],[225,109],[164,109],[165,115],
 [162,123],[157,133],[151,144],[147,146],[143,139],[139,129],
 [137,129],[134,139],[126,151],[118,160],[111,167],[104,170],
 [101,168],[104,159],[107,151],[98,156],[83,159],[62,159],
 [48,156],[50,164],[44,165],[40,163],[39,167],[31,162],[0,164]];
// r25: move only the inner side-hair cut away from its original ink/AA pixels.
// Union with the historical mask, never redraw or erode the source contour.
const hairTipBoundary=[[106,149],[109,150],[107,160],[105,171],
 [99,170],[96,167],[96,164],[99,159],[101,155],[102,151]];
// Keep semantic parts across assembly. Re-cutting the flattened composite
// moves exposed shoulder pixels with the head and strands a second ribbon.
const assemblies=new WeakMap();
function remember(master,assembly){assemblies.set(master,assembly);}
function assemblyFor(master){return assemblies.get(master);}
function inside(p,x,y){let hit=false;for(let i=0,j=p.length-1;i<p.length;j=i++){
 const a=p[i],b=p[j];if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])hit=!hit;
}return hit;}
function split(master,api,{restoreHairTip=false}={}){
 if(master.width!==225||master.height!==225)throw Error('225px corrected master required');
 const source=api.createCanvas(225,225),sc=source.getContext('2d');sc.drawImage(master,0,0);
 const pixels=sc.getImageData(0,0,225,225),head=api.createCanvas(225,225),body=api.createCanvas(225,225);
 const h=head.getContext('2d').createImageData(225,225),b=body.getContext('2d').createImageData(225,225);
 for(let y=0;y<225;y++)for(let x=0;x<225;x++){
  const i=(y*225+x)*4,target=(inside(headPolygon,x+.5,y+.5)||(restoreHairTip&&inside(hairTipBoundary,x+.5,y+.5)))?h:b;
  target.data.set(pixels.data.subarray(i,i+4),i);
 }
 head.getContext('2d').putImageData(h,0,0);body.getContext('2d').putImageData(b,0,0);
 return {head,body};
}
module.exports={split,remember,assemblyFor,headPolygon,hairTipBoundary,inside};

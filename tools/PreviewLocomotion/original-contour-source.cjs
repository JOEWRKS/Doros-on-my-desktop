'use strict';
// Read-only source measurement. No seated image or historical rig is loaded.
const data=require('./original-contour-data.json');
function inside(polygon,x,y){
  let hit=false;for(let i=0,j=polygon.length-1;i<polygon.length;j=i++){
    const a=polygon[i],b=polygon[j];
    if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])hit=!hit;
  }return hit;
}
function point(p){if(!Array.isArray(p)||p.length!==2||!p.every(Number.isFinite))throw Error('finite coordinate pair required');}
function validateMeasurements(m){
  if(!m||!Array.isArray(m.headLandmarks)||m.headLandmarks.length<3)throw Error('head landmarks required');
  m.headLandmarks.forEach(point);m.bodyBoundary.forEach(point);
  const pose=m.standing;point(pose.head.pivot);point(pose.chest);point(pose.pelvis);
  if(!Number.isFinite(pose.head.angle))throw Error('finite head angle required');
  if(pose.legs.length!==3||new Set(pose.legs.map(l=>l.name)).size!==3)throw Error('three distinct visible legs required');
  for(const l of pose.legs){if(l.joints.length!==3)throw Error('root bend tip only');l.joints.forEach(point);point(l.sole);if(!(m.legWidths[l.name]>0))throw Error('positive measured shaft width required');}
  if(!(m.ink.width>0)||m.ink.rgb.length!==3||!m.ink.rgb.every(Number.isFinite))throw Error('measured ink required');
}
function sourcePixels(source,{createCanvas}){
  if(!source||source.width!==96||source.height!==96)throw Error('96x96 canonical image required');
  const c=createCanvas(96,96);c.getContext('2d').drawImage(source,0,0);return c.getContext('2d').getImageData(0,0,96,96);
}
function measure(source,canvasApi){
  const rgba=sourcePixels(source,canvasApi).data,boundary=[],inkSamples=[],legWidths={};
  const a=(x,y)=>x<0||y<0||x>=96||y>=96?0:rgba[(y*96+x)*4+3];
  for(let y=0;y<96;y++)for(let x=0;x<96;x++){
    const i=(y*96+x)*4;if(inside(data.headPolygon,x+.5,y+.5))continue;
    if(a(x,y)>=128&&[[1,0],[-1,0],[0,1],[0,-1]].some(([dx,dy])=>a(x+dx,y+dy)<128))boundary.push([x+.5,y+.5]);
    if(a(x,y)>200&&rgba[i]<160&&rgba[i+1]<160&&rgba[i+2]<160)inkSamples.push([...rgba.slice(i,i+3)]);
  }
  for(const [name,s] of Object.entries(data.widthStations)){
    const occupied=[];for(let x=s.minX;x<=s.maxX;x++)if(a(x,s.y)>=128)occupied.push(x);
    legWidths[name]=occupied.length?occupied.at(-1)-occupied[0]+1:0;
  }
  if(!inkSamples.length)throw Error('source body outline has no measurable ink');
  const median=n=>inkSamples.map(v=>v[n]).sort((a,b)=>a-b)[Math.floor(inkSamples.length/2)];
  const rgb=[median(0),median(1),median(2)],contrast=1-rgb.reduce((a,b)=>a+b,0)/765,widths=[];
  // Integrate optical ink coverage across the exposed, almost vertical rump
  // edge. This includes fractional AA rather than counting only black texels.
  for(let y=52;y<72;y++){
    let w=0;for(let x=70;x<80;x++){const i=(y*96+x)*4;w+=(1-(rgba[i]+rgba[i+1]+rgba[i+2])/765)*(rgba[i+3]/255)/contrast;}
    if(w>0)widths.push(w);
  }
  widths.sort((a,b)=>a-b);const width=widths[Math.floor(widths.length/2)];
  const m={headMask:data.headPolygon.map((p,i)=>({op:i?'L':'M',points:[p.slice()]})).concat({op:'Z',points:[]}),
    headLandmarks:structuredClone(data.headLandmarks),bodyBoundary:boundary,ink:{rgb,width},
    legWidths,standing:structuredClone(data.standing),widthStations:structuredClone(data.widthStations),metadata:structuredClone(data.metadata)};
  validateMeasurements(m);return m;
}
function extractHead(source,m,canvasApi){
  validateMeasurements(m);const original=sourcePixels(source,canvasApi),out=canvasApi.createCanvas(96,96),ctx=out.getContext('2d'),image=ctx.createImageData(96,96);
  const polygon=m.headMask.filter(c=>c.op!=='Z').map(c=>c.points[0]);
  for(let y=0;y<96;y++)for(let x=0;x<96;x++)if(inside(polygon,x+.5,y+.5)){
    const i=(y*96+x)*4;image.data.set(original.data.subarray(i,i+4),i);
  }
  ctx.putImageData(image,0,0);return out;
}
module.exports={measure,extractHead,validateMeasurements};

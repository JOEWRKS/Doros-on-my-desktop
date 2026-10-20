'use strict';
// Diagnostic only. Capture native-source paint functions without changing the
// production renderer; integrate into integer raster extents at arbitrary scale.
const fs=require('node:fs'),path=require('node:path');
const sampling=require('./sampling.cjs');
const originalSurface=sampling.surface;
let captured;
sampling.surface=function(api,size,paint,padding=0){captured={api,paint,padding};return originalSurface(api,size,paint,padding);};
const {create}=require('./connected-families.cjs');
function output(source,scale){
 const {api,paint,padding}=source,size=96*scale,extent=Math.ceil((96+2*padding)*scale);
 const c=api.createCanvas(extent,extent),g=c.getContext('2d'),d=g.createImageData(extent,extent),sum=new Float64Array(4);
 for(let y=0;y<extent;y++)for(let x=0;x<extent;x++){
  sum.fill(0);for(const dy of [.25,.75])for(const dx of [.25,.75]){
   const p=paint((x+dx)/scale-padding,(y+dy)/scale-padding);for(let k=0;k<4;k++)sum[k]+=p[k]/4;
  }sampling.put(d.data,(y*extent+x)*4,sum);
 }
 g.putImageData(d,0,0);return c;
}
async function main(){
 const m=await create(),{api,root}=m.s,out=path.join(root,'artifacts/continuous-scale-review-20261017');fs.mkdirSync(out,{recursive:true});
 const scales=[1,1.1,1.2,1.3,1.4,1.5],rows=[];
 for(const [name,kind,t,options]of [['Idle','idle',0,{}],['Tracking','track',0,{roll:-.20}],['Run / phase .27','run',.27,{}],['Closed / happy','happy',.5,{}]]){
  m.render(kind,t,options);const src=captured;rows.push({name,src,images:scales.map(s=>output(src,s))});
 }
 const {outline,ink}=require('./sleep-reference-still.cjs'),rig=m.s.postures.closed.rig;
 const parts=Object.fromEntries(['farFront','nearFront','nearHind','haunch'].map(name=>[name,{name,fill:[],ink:[],rootInk:[]}]));
 parts.chest={name:'chest',fill:outline,ink:ink.slice(1),rootInk:[]};
 const body=rig.render(rig.pose(0),{includeHead:false,extraParts:parts,interiorParts:['chest'],interiorOccluders:{chest:[]},clipInteriorToSkin:true});
 const effect=api.createCanvas(225,225),eg=effect.getContext('2d');eg.scale(225/96,225/96);eg.translate(0,-8);
 const {ink:pen}=require('../PreviewLocomotion/original-contour-source.cjs').measure(m.s.measurement,api);
 eg.beginPath();eg.rect(26,74.25,5,6.5);eg.clip();eg.beginPath();eg.moveTo(28,74.25);eg.bezierCurveTo(28.4,76,28.7,78,27.8,79.6);eg.strokeStyle=`rgb(${pen.rgb.join(',')})`;eg.lineWidth=pen.width;eg.lineCap='round';eg.stroke();
 const accepted=m.compose(body,'closed',{dy:8,effect}),src=captured;
 const previous=await api.loadImage(path.join(root,'artifacts/high-resolution-sleep-still-20261017-r23/candidate.png'));
 if(!Buffer.from(sampling.pixels(accepted,api)).equals(Buffer.from(sampling.pixels(previous,api))))throw Error('Accepted sleep reconstruction mismatch');
 rows.push({name:'Accepted sleep still',src,images:scales.map(s=>output(src,s))});
 const sheet=api.createCanvas(1200,1120),g=sheet.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,sheet.width,sheet.height);g.font='16px sans-serif';g.fillStyle='#f7f2f0';
 for(let i=0;i<scales.length;i++)g.fillText(scales[i].toFixed(1)+'x / 100% DPI',i*200+20,25);
 for(let r=0;r<rows.length;r++){g.fillStyle='#b8cbd7';g.fillText(rows[r].name,12,60+r*210);for(let col=0;col<6;col++){
  const im=rows[r].images[col];g.drawImage(im,col*200+Math.floor((200-im.width)/2),70+r*210);fs.writeFileSync(path.join(out,`pose-${r}-scale-${scales[col].toFixed(1)}.png`),im.toBuffer('image/png'));
 }}fs.writeFileSync(path.join(out,'comparison.png'),sheet.toBuffer('image/png'));
 // Awkward fractional values expose raster stride/padding mistakes. Not an
 // animation sweep, nor proof that every pose or live DPI combination is safe.
 for(const s of [1.01,1.17,1.333,1.499]){const im=output(rows[0].src,s);fs.writeFileSync(path.join(out,`fractional-${s}.png`),im.toBuffer('image/png'));}
 const dpi=api.createCanvas(840,440),dg=dpi.getContext('2d');dg.fillStyle='#eeeae4';dg.fillRect(0,0,840,440);dg.font='17px sans-serif';dg.fillStyle='#302b33';
 for(const [i,d]of [1,1.5,2].entries()){const im=output(rows[0].src,1.5*d);dg.fillText(`1.5x / ${d*100}% DPI / ${144*d}px`,i*280+12,28);dg.drawImage(im,i*280+(280-im.width)/2,50);}
 fs.writeFileSync(path.join(out,'dpi-physical-pixels.png'),dpi.toBuffer('image/png'));
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify({diagnosticOnly:true,runtimeChanged:false,sourceSize:225,logicalSize:96,scales,poses:rows.map(r=>r.name),fractionalSamples:[1.01,1.17,1.333,1.499],acceptedSleepExactMatch:true,limitations:['selected static phases only','no live slider, all-frame sweep or runtime DPI/hit-test verification','200% DPI at 1.5x requires288px beyond225px source']},null,2));
 console.log(JSON.stringify({out,staticSamples:37,acceptedSleepExactMatch:true}));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});

'use strict';
// Isolated posture study. NOT selected by connected-families or either runtime.
// Coordinates are observed from dororong-sleep.png at its original96 canvas,
// independently of the rejected standing-rig-derived sleep endpoint.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {create}=require('./connected-families.cjs');
const {pixels,sample}=require('./sampling.cjs');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
const outline=[['M',18,68],['C',18,70,12,72,11,74],['C',10,77,10.8,78,12,79],
 ['C',15,82,24,82.8,27.8,79.6],
 ['C',29.6,83,42,83,50,81],
 ['C',54,80,57,78.6,59.8,79.1],
 ['C',65,81,71,80.5,75.5,77],
 ['C',80.5,73,82,65,78.7,58],
 ['C',76.6,53.9,72,50.9,67,48.5],
 ['C',61,42,42,47,27,58],['C',19,61,18,64,18,68],['Z']];
const ink=[
 [['M',27.3,70],['C',26.5,73,29.2,76,27.8,79.6]],
 [['M',64.2,72.2],['C',61.1,73.1,58.8,76,59.8,79.1]]
];
async function main(){
 const m=await create(),{api,root}=m.s,out=path.join(root,'artifacts/high-resolution-sleep-still-20261017-r23');fs.mkdirSync(out,{recursive:true});
 const empty=name=>({name,fill:[],ink:[],rootInk:[]});
 const rig=m.s.postures.closed.rig,parts=Object.fromEntries(['farFront','nearFront','nearHind','haunch'].map(n=>[n,empty(n)]));
 parts.chest={name:'chest',fill:outline,ink:ink.slice(1),rootInk:[]};
 const body=rig.render(rig.pose(0),{includeHead:false,extraParts:parts,interiorParts:['chest'],interiorOccluders:{chest:[]},clipInteriorToSkin:true});
 // The locked head texture contains a white BODY skirt below its chin.
 // Render only the forepad divider over that skirt, never over the face.
 const effect=api.createCanvas(225,225),eg=effect.getContext('2d');eg.scale(225/96,225/96);eg.translate(0,-8);
 const {ink:pen}=require('../PreviewLocomotion/original-contour-source.cjs').measure(m.s.measurement,api);
 eg.beginPath();eg.rect(26,74.25,5,6.5);eg.clip();eg.beginPath();eg.moveTo(28,74.25);eg.bezierCurveTo(28.4,76,28.7,78,27.8,79.6);eg.strokeStyle=`rgb(${pen.rgb.join(',')})`;eg.lineWidth=pen.width;eg.lineCap='round';eg.stroke();
 const current=m.compose(body,'closed',{dy:8,effect});
 const original=await api.loadImage(path.join(root,'src/Dororong.App/Assets/dororong-sleep.png'));
 const previous=await api.loadImage(path.join(root,'artifacts/high-resolution-sleep-still-20261017-r22/candidate.png'));
 const save=(name,c)=>fs.writeFileSync(path.join(out,name),c.toBuffer('image/png'));
 save('candidate.png',current);save('body-225.png',body);
 const sheet=api.createCanvas(768,600),g=sheet.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,768,600);
 for(const [i,im]of [original,current].entries()){
  g.imageSmoothingEnabled=false;
  if(i===0)g.drawImage(im,0,0,96,96,i*384,20,384,384);else g.drawImage(im,24,24,144,144,i*384,20,384,384);
  g.fillStyle='#302b33';g.font='17px sans-serif';g.fillText(i?'Source-contour still / locked face':'Original sleep',i*384+20,25);
  g.imageSmoothingEnabled=true;
  if(i===0)g.drawImage(im,i*384+120,405,144,144);else g.drawImage(im,24,24,144,144,i*384+120,405,144,144);
 }save('comparison.png',sheet);
 const detail=api.createCanvas(900,330),dc=detail.getContext('2d');dc.fillStyle='#eeeae4';dc.fillRect(0,0,900,330);dc.imageSmoothingEnabled=false;
 dc.drawImage(original,8,66,49,20,0,80,441,180);dc.drawImage(current,36,123,73.5,30,450,80,441,180);
 dc.fillStyle='#302b33';dc.font='20px sans-serif';dc.fillText('Original forepaws',20,30);dc.fillText('Corrected divider / locked face',470,30);save('forepaws-comparison.png',detail);
 const dark=api.createCanvas(384,256),dg=dark.getContext('2d');dg.fillStyle='#252a31';dg.fillRect(0,0,384,256);dg.drawImage(current,96,20);save('actual-size-dark.png',dark);
 // Compare protected output head against the actual previously delivered PNG.
 const a=pixels(current,api),b=pixels(previous,api),h=new Float64Array(4);let headPixels=0,headDifferences=0,bodySkirtDifferences=0,totalDifferences=0;
 for(let y=0;y<192;y++)for(let x=0;x<192;x++){
  let opaque=true;for(const dy of [.25,.75])for(const dx of [.25,.75]){sample(m.heads.closed,225,((x+dx)/1.5-16)*225/96-.5,((y+dy)/1.5-16-8)*225/96-.5,h);if(h[3]<254.999)opaque=false;}
  const k=(y*192+x)*4,changed=a.slice(k,k+4).some((v,j)=>v!==b[k+j]);
  // Includes filter support, entirely below the unchanged chin ink. The body
  // skirt inside the old head mask is NOT reclassified as face content.
  const padRegion=x>=62&&x<=72&&y>=135&&y<=146;
  if(changed){totalDifferences++;if(!padRegion)throw Error('Changed outside forepad divider: '+x+','+y);}
  if(opaque){if(padRegion){if(changed){if(Math.min(...b.slice(k,k+3))<190)throw Error('Forepad crossed dark chin/face ink');bodySkirtDifferences++;}}else{headPixels++;if(changed)headDifferences++;}}
 }
 if(headDifferences)throw Error('Protected head pixels changed: '+headDifferences);
 // Independent source/output lower silhouettes, at alpha128. The low-res
 // source's AA implies subpixel uncertainty; report errors, do not call this
 // metric proof of shape acceptance.
 const src=pixels(original,api),deltas=[];
 for(let x=12;x<=76;x++){
  let sy=-1,cy=-1;for(let y=60;y<96;y++)if(src[(y*96+x)*4+3]>=128)sy=y+.5;
  for(let y=24;y<192;y++)if(a[(y*192+Math.round(24+(x+.5)*1.5-.5))*4+3]>=128)cy=(y+.5-24)/1.5;
  if(sy>=0&&cy>=0)deltas.push({x,sourceBottom:sy,candidateBottom:cy,delta:cy-sy});
 }
 const report={runtimeSelected:false,transitionConnected:false,headPixels,headDifferences,bodySkirtDifferences,totalDifferences,allowedForepadPixelRegion:[62,135,72,146],sourceBottomMeanAbsError:deltas.reduce((s,p)=>s+Math.abs(p.delta),0)/deltas.length,sourceBottomMaxAbsError:Math.max(...deltas.map(p=>Math.abs(p.delta))),bottomSamples:deltas,sources:{},files:{}};
 for(const file of ['tools/HighResolutionArt/sleep-reference-still.cjs','tools/HighResolutionArt/sleep-pose.cjs','tools/HighResolutionArt/connected-families.cjs','tools/HighResolutionArt/motion-polish.cjs','src/Dororong.App/Assets/dororong-sleep.png','artifacts/high-resolution-cheek-20261016-r15/head-layer-225.png'])report.sources[file]=sha(fs.readFileSync(path.join(root,file)));
 for(const name of ['candidate.png','body-225.png','comparison.png','forepaws-comparison.png','actual-size-dark.png'])report.files[name]=sha(fs.readFileSync(path.join(out,name)));
 fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(report,null,2));
 console.log(JSON.stringify({out,headPixels,headDifferences,bodySkirtDifferences,totalDifferences,meanBottomError:report.sourceBottomMeanAbsError,maxBottomError:report.sourceBottomMaxAbsError}));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={main,outline,ink};

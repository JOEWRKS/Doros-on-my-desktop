'use strict';
// Evidence about this saved still only. No automatic visual approval.
const fs=require('node:fs'),path=require('node:path'),assert=require('node:assert/strict'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
const {pixels,premultiply,sample}=require('./sampling.cjs');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 const dir=path.join(root,'artifacts/grab-contour-still-20261017-r31'),load=async f=>api.loadImage(path.join(root,f));
 const candidate=await load('artifacts/grab-contour-still-20261017-r31/candidate.png'),d=pixels(candidate,api);
 const head=await load('artifacts/grab-contour-still-20261017-r31/locked-head-225.png'),hd=pixels(head,api),hp=premultiply(head,api);
 const approved=await load('artifacts/pointed-hair-tip-20261017-r27/open-head-225.png');assert.deepEqual(hd,pixels(approved,api),'approved head layer changed');
 const prior=await load('artifacts/direct-motion-20261017-r29/grab-study-9.png'),pd=pixels(prior,api);
 let fullyOpaqueHeadPixels=0,changedOpaqueHeadPixels=0;const s=new Float64Array(4);
 for(let y=0;y<240;y++)for(let x=0;x<240;x++){
  let opaque=true;for(const dy of [.25,.75])for(const dx of [.25,.75]){sample(hp,225,((x+dx)/1.5-32)*225/96-.5,((y+dy)/1.5-32)*225/96-.5,s);if(s[3]<254.999)opaque=false;}
  const k=(y*240+x)*4;if(opaque){fullyOpaqueHeadPixels++;if([0,1,2,3].some(c=>d[k+c]!==pd[k+c]))changedOpaqueHeadPixels++;}
 }assert.equal(changedOpaqueHeadPixels,0,'protected opaque head output changed');
 let clipped=0;for(let k=0;k<240;k++)for(const [x,y]of [[k,0],[k,239],[0,k],[239,k]])if(d[(y*240+x)*4+3])clipped++;assert.equal(clipped,0);
 // Independent cross-sections from saved original/candidate PNGs. No vector
 // control points are read as expected values. Rows avoid the overlaid head.
 const ref=await load('src/Dororong.App/Assets/user-body-drag/08.png'),rd=pixels(ref,api),meta=JSON.parse(fs.readFileSync(path.join(dir,'measurement.json'))),{factor,dx,dy}=meta.sourceToLogical;
 const before=await load('artifacts/grab-contour-still-20261017-r30/candidate.png'),beforeData=pixels(before,api);
 const changeRegions=[{name:'inner hind feet',box:[45,80,61,95]}];
 let changedPixels=0,changedOutsideRegions=0;const changeBounds=[240,240,-1,-1],outside=[];
 for(let y=0;y<240;y++)for(let x=0;x<240;x++){
  const i=(y*240+x)*4;if(![0,1,2,3].some(k=>d[i+k]!==beforeData[i+k]))continue;
  changedPixels++;changeBounds[0]=Math.min(changeBounds[0],x);changeBounds[1]=Math.min(changeBounds[1],y);changeBounds[2]=Math.max(changeBounds[2],x);changeBounds[3]=Math.max(changeBounds[3],y);
  const sx=((x+.5)/1.5-32-dx)/factor,sy=((y+.5)/1.5-32-dy)/factor;
  if(!changeRegions.some(({box:[l,t,r,b]})=>sx>=l&&sx<=r&&sy>=t&&sy<=b)){changedOutsideRegions++;outside.push({x,y,sx,sy,before:[...beforeData.slice(i,i+4)],after:[...d.slice(i,i+4)]});}
 }
 if(outside.length)console.error(JSON.stringify(outside.slice(0,12)));
 assert.equal(changedOutsideRegions,0,'changes outside inner hind feet');
 function darkOriginal(x,y){const i=(y*100+x)*4;return Math.max(0,(210-(rd[i]+rd[i+1]+rd[i+2])/3)/210);}
 function darkCandidate(x,y){const xx=Math.round((32+dx+(x+.5)*factor)*1.5-.5),yy=Math.round((32+dy+(y+.5)*factor)*1.5-.5),i=(yy*240+xx)*4;return Math.max(0,(210-(d[i]+d[i+1]+d[i+2])/3)/210)*d[i+3]/255;}
 const stations=[];for(const [name,rows,x0,x1]of [['left side',[78,80,82,85,88],32,38],['right side',[65,70,75,80,85,88],67,75],['left forepaw outer',[65,68,71],26,33],['forepaw center',[67,69,71],39,45],['right forepaw',[66,68,70,72],52,60]])for(const y of rows){
  const center=fn=>{let w=0,s=0;for(let x=x0;x<=x1;x++){const z=fn(x,y);w+=z;s+=(x+.5)*z;}return w?s/w:null;},a=center(darkOriginal),b=center(darkCandidate);stations.push({name,y,sourceCenter:a,candidateCenter:b,delta:a===null||b===null?null:b-a});
 }
 const bottoms=[];for(let x=38;x<=69;x++){
  const center=fn=>{let last=-1;for(let y=81;y<=98;y++)if(fn(x,y)>.06)last=y;if(last<0)return null;let w=0,s=0;for(let y=Math.max(81,last-2);y<=last;y++){const v=fn(x,y);w+=v;s+=(y+.5)*v;}return w?s/w:null;};const a=center(darkOriginal),b=center(darkCandidate);bottoms.push({x,sourceCenter:a,candidateCenter:b,delta:a===null||b===null?null:b-a});
 }
 const outlineDeltas=stations.filter(s=>s.delta!==null).map(s=>Math.abs(s.delta)),bottomDeltas=bottoms.filter(s=>s.delta!==null).map(s=>Math.abs(s.delta));
 const report={status:'STILL_CANDIDATE_NOT_VISUALLY_ACCEPTED',transitionConnected:false,runtimeSelected:false,headLayerExactlyMatchesApprovedR27:true,fullyOpaqueHeadPixels,changedOpaqueHeadPixels,clippedBorderPixels:clipped,outlineStations:stations,stationMeanAbsError:outlineDeltas.reduce((a,b)=>a+b,0)/outlineDeltas.length,stationMaxAbsError:Math.max(...outlineDeltas),bottomStations:bottoms,bottomMeanAbsError:bottomDeltas.reduce((a,b)=>a+b,0)/bottomDeltas.length,bottomMaxAbsError:Math.max(...bottomDeltas),limits:['Color-bound head registration is approximate.','Cross-sections use threshold210, fixed row windows and nearest sample mapping; not whole-contour or subpixel equivalence.','Opaque-head output equality excludes semitransparent composite boundaries; source head layer including alpha is exactly preserved.','No transition, rotation, other size, runtime or subjective acceptance tested.'],files:{}};
 report.changeScope={baseline:'r30/candidate.png',changedPixels,changedOutsideRegions,changeBounds,sourceCoordinateRegions:changeRegions};
 for(const f of ['candidate.png','locked-head-225.png','body-padded-375.png','comparison.png','actual-size-dark.png','measurement.json'])report.files[f]=sha(fs.readFileSync(path.join(dir,f)));
 report.sources={};for(const f of ['tools/HighResolutionArt/grab-contour-still.cjs','tools/HighResolutionArt/grab-contour-still-check.cjs','src/Dororong.App/Assets/user-body-drag/08.png','artifacts/pointed-hair-tip-20261017-r27/open-head-225.png'])report.sources[f]=sha(fs.readFileSync(path.join(root,f)));
 fs.writeFileSync(path.join(dir,'verification.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});

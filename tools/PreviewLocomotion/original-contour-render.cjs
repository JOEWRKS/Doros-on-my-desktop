'use strict';
const {extractHead,validateMeasurements}=require('./original-contour-source.cjs');
const {buildParts,paintPart,paintInk,trace}=require('./original-contour-art.cjs');
const {validatePose}=require('./original-contour-poses.cjs');
const depth=['farFront','torso','nearHind','nearFront'];
function createRenderer(source,m,canvasApi){
  validateMeasurements(m);const head=extractHead(source,m,canvasApi),origin=m.standing.head.pivot;
  function renderHead(pose){
    validatePose(pose);const c=canvasApi.createCanvas(96,96),ctx=c.getContext('2d');ctx.imageSmoothingEnabled=false;
    ctx.translate(...pose.head.pivot);ctx.rotate(pose.head.angle);ctx.drawImage(head,-origin[0],-origin[1]);return c;
  }
  function renderParts(parts,pose,scale=1){
    validatePose(pose);if(scale!==1&&scale!==3)throw Error('scale must be 1 or 3');
    if(!Array.isArray(parts)||parts.length!==4||new Set(parts.map(p=>p.name)).size!==4||parts.some(p=>!depth.includes(p.name)))throw Error('four named anatomical parts required');
    const c=canvasApi.createCanvas(96,96),ctx=c.getContext('2d');
    for(const name of depth){
      const part=parts.find(p=>p.name===name);
      if(name!=='nearHind'){paintPart(ctx,part,m);continue;}
      // Complete foreground skin remains opaque. Only rear root ink is
      // occluded by the torso's closed skin path; no alpha repair/fill pass.
      trace(ctx,part.fill);ctx.fillStyle='#fff';ctx.fill();ctx.save();
      ctx.beginPath();ctx.rect(0,0,96,96);trace(ctx,parts.find(p=>p.name==='torso').fill,false);
      ctx.clip('evenodd');paintInk(ctx,{ink:part.rootInk||[]},m);ctx.restore();
      // Distal foot contour is a visible anatomical crease, even where its
      // skin overlaps the rump. Do not bury it with the hidden shoulder ink.
      paintInk(ctx,{ink:part.ink},m);
    }
    ctx.drawImage(renderHead(pose),0,0);
    if(scale===1)return c;
    const enlarged=canvasApi.createCanvas(96*scale,96*scale),e=enlarged.getContext('2d');e.imageSmoothingEnabled=false;e.drawImage(c,0,0,enlarged.width,enlarged.height);return enlarged;
  }
  function render(pose,scale=1){validatePose(pose);return renderParts(buildParts(m,pose),pose,scale);}
  return {render,renderHead,renderParts};
}
module.exports={createRenderer};

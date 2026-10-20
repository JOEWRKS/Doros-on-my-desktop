'use strict';
// Source-coordinate geometry corrections authorized 2026-10-16.
// This is not a face redraw, global smoothing pass or a new body silhouette.
// Coordinates refer to the retained 225px original master, never a 96px upscale.
const regions = {
  hindLeg: [133, 169, 35, 31],
  ribbonJoin: [157, 109, 10, 16],
};
function hindContour(ctx) {
  ctx.moveTo(133,175.2);
  ctx.bezierCurveTo(135.4,174.1,137.1,172.5,138.1,173.1);
  ctx.bezierCurveTo(139.1,173.8,137.8,177.2,139.1,181.2);
  ctx.bezierCurveTo(141.2,188.5,148.2,197.3,153,197.1);
  ctx.bezierCurveTo(158.5,196.8,165.7,191.5,165.2,186.2);
  ctx.bezierCurveTo(164.9,183.3,162.8,181,163.7,175.4);
  ctx.bezierCurveTo(164,172.9,165.2,170.8,166.5,169);
}
function ribbonContour(ctx) {
  ctx.moveTo(159.4,109.4);
  ctx.bezierCurveTo(160.4,112,163.1,114.4,161.2,118.1);
  ctx.bezierCurveTo(160.2,120.3,158.8,122.5,157.5,124.4);
}
function render(source, api, enabled = Object.keys(regions)) {
  if(source.width!==225 || source.height!==225) throw Error('225px master required');
  const c=api.createCanvas(225,225),ctx=c.getContext('2d');
  ctx.drawImage(source,0,0);
  for(const name of enabled) {
    const r=regions[name];if(!r)throw Error('Unknown correction');
    ctx.save();ctx.beginPath();ctx.rect(...r);ctx.clip();
    if(name==='hindLeg') {
      ctx.clearRect(...r);
      ctx.beginPath();hindContour(ctx);ctx.lineTo(168,165);ctx.lineTo(130,165);ctx.closePath();
      ctx.fillStyle='#ffffff';ctx.fill();
      ctx.beginPath();hindContour(ctx);ctx.strokeStyle='#1a020a';ctx.lineWidth=2.1;
      ctx.lineJoin='round';ctx.lineCap='round';ctx.stroke();
    } else {
      // Ribbon is foreground. Its small white backing covers the body-side
      // junction; only the existing ribbon edge continues over that backing.
      ctx.beginPath();ribbonContour(ctx);ctx.lineTo(153.7,121);ctx.lineTo(156.4,110);ctx.closePath();
      ctx.fillStyle='#ffffff';ctx.fill();
      ctx.beginPath();ribbonContour(ctx);ctx.strokeStyle='#241920';ctx.lineWidth=1.6;
      ctx.lineJoin='round';ctx.lineCap='round';ctx.stroke();
    }
    ctx.restore();
  }
  return c;
}
module.exports={render,regions};

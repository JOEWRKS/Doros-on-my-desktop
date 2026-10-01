'use strict';
const {test}=require('node:test'),assert=require('node:assert/strict'),path=require('node:path'),fs=require('node:fs');
const canvas=require(path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
async function setup(){
  const file=path.join(__dirname,'original-contour-art.cjs');assert.ok(fs.existsSync(file),'complete original-contour art module is missing');
  const s=await canvas.loadImage(path.join(__dirname,'../../src/Dororong.App/Assets/dororong-canonical.png'));
  return {m:require('./original-contour-source.cjs').measure(s,canvas),api:require(file)};
}
function fillCanvas(part){
  const c=canvas.createCanvas(96,96),ctx=c.getContext('2d');ctx.beginPath();
  for(const cmd of part.fill){const p=cmd.points.flat();if(cmd.op==='M')ctx.moveTo(...p);else if(cmd.op==='L')ctx.lineTo(...p);else if(cmd.op==='C')ctx.bezierCurveTo(...p);else ctx.closePath();}
  ctx.fillStyle='#fff';ctx.fill();return c;
}
const alpha=(c,x,y)=>c.getContext('2d').getImageData(Math.floor(x),Math.floor(y),1,1).data[3];
test('hidden roots and upper thigh are complete filled artwork',async()=>{
  const {m,api}=await setup(),parts=api.buildParts(m,m.standing);
  assert.deepEqual(parts.map(p=>p.name).sort(),['farFront','nearFront','nearHind','torso']);
  for(const p of parts){
    assert.equal(p.fill[0].op,'M');assert.equal(p.fill.at(-1).op,'Z');assert.ok(p.coverage.length>=2);
    const fill=fillCanvas(p);for(const q of [p.root,...p.coverage])assert.ok(alpha(fill,...q)>250,`${p.name} has no hidden skin at ${q}`);
    assert.ok(api.renderPart(p,m,canvas).getContext('2d').getImageData(0,0,96,96).data.some(Boolean));
  }
  const thigh=fillCanvas(parts.find(p=>p.name==='nearHind'));let area=0;
  for(let y=61;y<71;y++)for(let x=60;x<70;x++)area+=Number(alpha(thigh,x,y)>128);
  assert.ok(area>=60,`upper thigh is empty or a narrow seam: ${area}`);
});
test('standing widths and outward contours follow measurements',async()=>{
  const {m,api}=await setup(),parts=api.buildParts(m,m.standing);
  for(const name of ['farFront','nearFront','nearHind']){
    const p=parts.find(p=>p.name===name),c=api.renderPart(p,m,canvas),s=m.widthStations[name],xs=[];
    for(let x=0;x<96;x++)if(alpha(c,x,s.y)>=128)xs.push(x);
    const width=xs.at(-1)-xs[0]+1;
    assert.ok(Math.abs(width-m.legWidths[name])<=1,`${name}: width ${width}, source ${m.legWidths[name]}`);
  }
});
test('real bend input changes complete hindleg drawing',async()=>{
  const {m,api}=await setup(),pose=structuredClone(m.standing);
  pose.legs.find(l=>l.name==='nearHind').joints[1][0]-=2;
  const part=p=>api.buildParts(m,p).find(l=>l.name==='nearHind');
  assert.notDeepEqual(part(m.standing).fill,part(pose).fill);
  assert.deepEqual(api.buildParts(m,pose).find(l=>l.name==='nearFront'),api.buildParts(m,m.standing).find(l=>l.name==='nearFront'));
});

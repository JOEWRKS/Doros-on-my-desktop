'use strict';
const {pixels}=require('./sampling.cjs');
const {transfer:transferAuthored}=require('./expressions.cjs');
// Closed eyes use the actual approved96 raster, not a newly authored lid curve.
// The previous r4 squint path remains unchanged by this closed-eye-only repair.
const eyeRegions=[[16,45,14,12],[32,48,15,12]];
const regions=[
 [['M',41,118],['C',45,115,48,111.8,51,112.5],['C',57,115,62,120,68.8,121.3],['C',70,129,63,134,55,134],['C',44,134,39,129,41,118],['Z']],
 [['M',80,124],['C',84,115,96,117,102,125],['C',108,133,102,143,92,143],['C',81,143,76,135,80,124],['Z']]
];
function trace(g,p){g.beginPath();for(const [op,...v] of p){if(op==='M')g.moveTo(...v);else if(op==='C')g.bezierCurveTo(...v);else g.closePath();}}
function transfer(master,open,expression,api,{kind='closed'}={}){
 if(master.width!==225||open.width!==96||expression.width!==96)throw Error('Expected source225 and authored96 references');
 if(!['closed','squint'].includes(kind))throw Error('Unknown expression');
 if(kind==='closed')return transferAuthored(master,open,expression,api);
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.imageSmoothingEnabled=false;g.drawImage(master,0,0);
 const src=pixels(master,api);
 const color=(x,y)=>`rgb(${[...src.subarray((y*225+x)*4,(y*225+x)*4+3)].join(',')})`;
 for(let n=0;n<regions.length;n++){
  trace(g,regions[n]);g.save();g.clip();
  const skin=g.createLinearGradient(39,0,67,0);skin.addColorStop(0,color(39,131));skin.addColorStop(1,color(70,132));
  g.fillStyle=n===0?skin:color(77,135);g.fillRect(35,110,75,35);g.restore();
 }
 if(kind==='squint'){
  // A lowered lid covers original225 iris detail rather than squashing it.
  for(const opening of [
   [['M',43.5,119.5],['C',49,119,56,122,64,125],['C',61,133,48,134,43,127],['Z']],
   [['M',81.5,127],['C',88,126,96,129,102,131],['C',103,138,98,142,92,142],['C',84,142,79,136,81.5,127],['Z']]
  ]){
   g.save();trace(g,opening);g.clip();g.drawImage(master,0,0);g.restore();
  }
 }
 g.strokeStyle='#50464f';g.lineWidth=2.5;g.lineCap=g.lineJoin='round';
 g.beginPath();g.moveTo(43.5,119.5);g.bezierCurveTo(49,119,56,122,64,125);g.stroke();
 g.beginPath();g.moveTo(81.5,127);g.bezierCurveTo(88,126,96,129,102,131);g.stroke();
 // Protect the original mouth and immediately adjacent skin after all eye
 // painting; the right eye mask's AA must not alter even this small fringe.
 const image=g.getImageData(0,0,225,225);
 for(let y=136;y<152;y++)for(let x=58;x<80;x++){const at=(y*225+x)*4;image.data.set(src.subarray(at,at+4),at);}
 g.putImageData(image,0,0);return c;
}
module.exports={transfer,eyeRegions,regions};

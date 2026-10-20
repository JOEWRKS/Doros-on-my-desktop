'use strict';
// User-authorized contour edit, distinct from r25's source-pixel restoration.
// r27 matches the original near-black ink and carries its width through the
// root junction. The pointed endpoint stays at (97.8,168.2).
const region=[97,161,111,170];
const contains=(x,y)=>x>=region[0]&&x<region[2]&&y>=region[1]&&y<region[3];
function apply(head,api){
 const g=head.getContext('2d'),before=g.getImageData(0,0,225,225);
 const patch=api.createCanvas(225,225),p=patch.getContext('2d');
 p.fillStyle='#ffffff';p.fillRect(...[97,161,14,9]);
 p.fillStyle='#0c0002';p.beginPath();p.moveTo(100.2,160);
 p.quadraticCurveTo(100.1,163.1,97.8,168.2);
 p.quadraticCurveTo(105.8,166.8,112.2,160);p.closePath();p.fill();
 p.fillStyle='#ecabc1';p.beginPath();p.moveTo(103,160);
 p.quadraticCurveTo(102,162.9,99.7,166.1);
 p.quadraticCurveTo(104.8,164.9,109.7,160);p.closePath();p.fill();
 const edited=p.getImageData(0,0,225,225);
 for(let y=161;y<170;y++)for(let x=97;x<111;x++){
  const i=(y*225+x)*4;
  // Never turn empty cutout pixels into a rectangular white head extension.
  if(before.data[i+3]===0&&Math.min(...edited.data.subarray(i,i+3))===255)continue;
  before.data.set(edited.data.subarray(i,i+4),i);
 }
 g.putImageData(before,0,0);return head;
}
module.exports={apply,region,contains};

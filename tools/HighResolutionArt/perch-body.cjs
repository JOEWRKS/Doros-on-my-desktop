'use strict';
// Registered image09's two front paws (x-8,y+6), reconstructed at 225px.
// The source head remains a separate, immutable layer. No low-res face crop.
function create(api,measurement){
 const pen=require('../PreviewLocomotion/original-contour-source.cjs').measure(measurement,api).ink;
 const c=api.createCanvas(225,225),g=c.getContext('2d');g.scale(225/96,225/96);
 g.beginPath();g.moveTo(18.5,60);g.lineTo(51,48);g.lineTo(62,57);g.lineTo(65,70);
 g.lineTo(53,70);g.bezierCurveTo(50,74,49.5,83.5,43,83.5);
 g.bezierCurveTo(37,83.5,35.5,79,34.5,73);
 g.bezierCurveTo(33,79,31,83.5,26,83.5);g.bezierCurveTo(20.5,83.5,19,76,18.5,68);g.closePath();
 g.fillStyle='white';g.fill();
 g.beginPath();g.moveTo(18.5,68);g.bezierCurveTo(19,76,20.5,83.5,26,83.5);
 g.bezierCurveTo(31,83.5,33,79,34.5,73);g.bezierCurveTo(35.5,79,37,83.5,43,83.5);
 g.bezierCurveTo(49.5,83.5,50,74,53,70);
 // The upper/right body silhouette was filled but never outlined. Keep the
 // same registered body path and pen; extend the existing outline to the back.
 g.lineTo(65,70);g.lineTo(62,57);g.lineTo(51,48);
 g.strokeStyle=`rgb(${pen.rgb.join(',')})`;g.lineWidth=pen.width;g.lineCap='round';g.lineJoin='round';g.stroke();return c;
}
module.exports={create};

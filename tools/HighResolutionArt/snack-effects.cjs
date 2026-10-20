'use strict';
// Same existing Windows effect geometry; only the source output canvas grows.
const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
function draw(g,age,{eating=false,jewel=null}={}){
 if(eating){if(age<.34&&jewel){const swallow=smooth((age-.12)/.22),size=10*(1-swallow)+1;g.save();g.globalAlpha=1-swallow;g.drawImage(jewel,21+8*swallow-size/2,58-size/2,size,size);g.restore();}return;}
 const y=67+4*(.5+.5*Math.sin(age*4));
 g.beginPath();g.moveTo(30.8,61.5);g.bezierCurveTo(32,65,30,67,31,y);g.strokeStyle='#6cb9de';g.lineWidth=1.3;g.lineCap='round';g.stroke();
 g.beginPath();g.ellipse(31,y+1,1.3,2,0,0,Math.PI*2);g.fillStyle='#aee2f2';g.fill();g.strokeStyle='#4682b4';g.lineWidth=.45;g.stroke();
 g.beginPath();g.moveTo(30.8,y);g.lineTo(30.8,y+1.5);g.strokeStyle='#fff';g.stroke();
}
function layer(api,age){const c=api.createCanvas(225,225),g=c.getContext('2d');g.scale(225/96,225/96);draw(g,age);return c;}
module.exports={draw,layer};

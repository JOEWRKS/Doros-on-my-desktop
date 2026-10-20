'use strict';
const fs=require('node:fs'),path=require('node:path');
const {setup}=require('./accepted-motion.cjs');
(async()=>{const s=await setup(),r=s.postures.closed,dir=path.join(s.root,'artifacts/high-resolution-motion-20261017-r17');fs.mkdirSync(dir,{recursive:true});
const c=s.api.createCanvas(1000,560),g=c.getContext('2d');g.fillStyle='#eeeae4';g.fillRect(0,0,1000,560);g.imageSmoothingEnabled=false;
for(let i=0;i<2;i++){const im=r.rig.render(r.rig.pose(1),{colors:!!i,includeHead:false});g.drawImage(im,75,155,110,60,i*500,40,495,270);}
g.font='12px sans-serif';g.fillStyle='#333';for(let x=32;x<=79;x+=2)g.fillText(x,(x*225/96-75)*4.5,330);
fs.writeFileSync(path.join(dir,'sit-diagnosis.png'),c.toBuffer('image/png'));
for(const t of [.5,.75,1]){const p=r.rig.pose(t),part=require('../PreviewLocomotion/reference-parts.cjs').paths('nearHind',p,{artwork:require('./body.cjs').artwork});let pts=part.fill.filter(v=>v.length===3).map(v=>v.slice(1));console.log(t,p.haunch,p.legs[2].joints,'bottom',pts.sort((a,b)=>b[1]-a[1]).slice(0,8));}
})();

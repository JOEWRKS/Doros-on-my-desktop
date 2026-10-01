'use strict';
const data=require('./original-contour-data.json');
const names=['farFront','nearFront','nearHind'];
function point(p){if(!Array.isArray(p)||p.length!==2||!p.every(Number.isFinite))throw Error('finite coordinate pair required');}
function validatePose(pose){
  if(!pose||!pose.head)throw Error('anatomy pose required');
  point(pose.head.pivot);point(pose.chest);point(pose.pelvis);
  if(!Number.isFinite(pose.head.angle))throw Error('finite head angle required');
  if(!Array.isArray(pose.legs)||pose.legs.length!==3||new Set(pose.legs.map(l=>l.name)).size!==3||pose.legs.some(l=>!names.includes(l.name)))throw Error('three distinct visible legs required');
  for(const l of pose.legs){
    if(!Array.isArray(l.joints)||l.joints.length!==3)throw Error('root bend tip anatomy required');
    l.joints.forEach(point);point(l.sole);
    for(let i=0;i<2;i++)if(Math.hypot(l.joints[i+1][0]-l.joints[i][0],l.joints[i+1][1]-l.joints[i][1])<.01)throw Error('nonzero leg segment required');
  }
}
function getPose(name,m){
  if(name!=='standing'&&name!=='seated')throw Error('standing or seated pose required');
  const p=structuredClone(name==='standing'?m.standing:data.seated);validatePose(p);return p;
}
module.exports={getPose,validatePose};

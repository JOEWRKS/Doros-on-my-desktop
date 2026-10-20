'use strict';
const base=require('./current-motion.cjs'),grabFactory=require('./grab-motion.cjs'),landingFactory=require('./landing-motion.cjs');
async function create(){const common=await base.create(),grab=grabFactory.create(common),landing=landingFactory.create(common,grab);
 return {common,grab,landing,render(kind,t,options={}){if(kind==='grab')return grab.render(t,options);if(kind==='hard-land')return landing.render(t,options);return common.render(kind,t,options);}};
}
const sequences=[
 {name:'grab-release',seconds:1.78,at:t=>{
  if(t<.14)return ['grab',t/.14];
  if(t<1.34)return ['grab',1,{angle:.65*Math.sin((t-.14)/1.2*Math.PI*2)*Math.sin((t-.14)/1.2*Math.PI)}];
  if(t<1.52)return ['grab',1-(t-1.34)/.18];
  return ['grab',0,{compression:landingFactory.compression((t-1.34)*1000)}];
 }},
 {name:'hard-landing',seconds:1.8,at:t=>{const p=landingFactory.extreme(t);return ['hard-land',p.amount,p];}},
 {name:'swing-extremes',seconds:.6,at:t=>['grab',1,{angle:(t/.6*176-88)*Math.PI/180}]}
];
module.exports={create,sequences};

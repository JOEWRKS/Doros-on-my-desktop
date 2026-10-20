'use strict';
const {smooth}=require('./connected-families.cjs');
const pulse=t=>Math.sin(Math.PI*Math.max(0,Math.min(1,t)))**2;
const scenarios=[
 {name:'idle-run-idle',seconds:.9,at:t=>['run',t/.56,{amount:pulse(t/.9)}]},
 {name:'walk-run-walk',seconds:1.2,at:t=>['walk',t/.56,{amount:1,runMix:pulse(t/1.2)}]},
 {name:'track-walk-idle',seconds:.8,at:t=>({kind:'combined',phase:t,amount:t<.4?smooth(t/.4):1-smooth((t-.4)/.4),roll:-.3*(1-smooth(t/.35))})},
 {name:'sit-rise',seconds:1.7,at:t=>t<=1.05?['sit',t/1.05]:['rise',Math.min(1,(t-1.05)/.65)]},
 {name:'hunt-recover',seconds:2.6,at:t=>['hunt',t]},
 {name:'jump-land',seconds:.86,at:t=>{const u=(t-.18)/.48,lift=u<=0||u>=1?0:20*4*u*(1-u)*smooth(u/.12)*smooth((1-u)/.12);return ['jump',t,{lift}];}},
 {name:'eat-happy',seconds:4.18,at:t=>t<=1.7?['eat',t]:t<2.02?['idle',0,{eye:'closed'}]:['happy',Math.min(2.16,t-2.02)]},
 {name:'sleep-wake',seconds:.74,at:t=>t<=.36?['sleep',t/.36]:t<.56?['sleep',1]:['wake',Math.min(1,(t-.56)/.18)]},
 {name:'breathing',seconds:.8,at:t=>['idle',0,{breathing:pulse(t/.8)}]}
];
function draw(model,parameters){return Array.isArray(parameters)?model.render(...parameters):model.s.rigs.open.render({...parameters,walking:true});}
module.exports={scenarios,draw};

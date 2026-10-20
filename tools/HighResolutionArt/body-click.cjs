'use strict';
// Exact accepted Windows phase/scale/lift curve; art is sampled from225 once.
const smooth=t=>{t=Math.max(0,Math.min(1,t));return t*t*(3-2*t);};
const mix=(a,b,t)=>a.map((v,i)=>v+(b[i]-v)*t);
function pose(progress){
 const t=Math.max(0,Math.min(1,progress)),rest=[1,1,0],press=[1.012,.975,0],deep=[1.024,.960,0];
 if(t<=0)return press;if(t>=1)return rest;
 if(t<.08)return mix(press,deep,smooth(t/.08));
 if(t<.16)return mix(deep,rest,smooth((t-.08)/.08));
 if(t<.46)return [1,1,-12*(1-(1-(t-.16)/.30)**3)];
 if(t<.66)return [1,1,-12];
 if(t<.84)return [1,1,-12*(1-smooth((t-.66)/.18))];
 if(t<.91)return mix(rest,[1.025,.965,0],smooth((t-.84)/.07));
 return mix([1.025,.965,0],rest,smooth((t-.91)/.09));
}
module.exports={pose};

'use strict';
// Kept in sync with SnackRenderer/DororongPresenter.Snack.HappyRoll.
const times=[0,.06,.46,1.10,1.64,2.10,2.16],degrees=[0,0,-8,8,-4,0,0];
function roll(age){
 if(!Number.isFinite(age)||age<=0||age>=times.at(-1))return 0;
 for(let i=1;i<times.length;i++)if(age<=times[i]){const t=(age-times[i-1])/(times[i]-times[i-1]),u=t*t*(3-2*t);return (degrees[i-1]+(degrees[i]-degrees[i-1])*u)*Math.PI/180;}
 return 0;
}
module.exports={roll,times,degrees};

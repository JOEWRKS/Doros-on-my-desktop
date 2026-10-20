'use strict';
// One explicitly current preview entry. Historical renderers retain opt-outs.
const path=require('node:path');
const {create:families,smooth}=require('./connected-families.cjs');
const {scenarios}=require('./connected-scenarios.cjs');
const options={pointedHairTip:true,acceptedSleep:true,snackEffects:true,outlineRecovery:false};
const sequence=[...scenarios,
 {name:'body-click',seconds:.68,at:t=>t<.08?['body-click',0,{press:smooth(t/.08)}]:['body-click',Math.min(1,(t-.08)/.5)]},
 {name:'snack-chase',seconds:.8,at:t=>['run',t/.56,{amount:Math.sin(Math.PI*t/.8)**2,snackAge:t}]}
];
const create=({size=144}={})=>families({...options,size});
async function main(){const {root}=require('./bangs-composite-review.cjs');return require('./connected-families-review.cjs').main({outDir:path.join(root,'artifacts/accepted-motion-connection-20261017-r28'),options,sequence,full:process.argv.includes('--full')});}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={create,main,options,sequence};

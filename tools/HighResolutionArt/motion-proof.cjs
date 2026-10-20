'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const api=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
const {render:correct}=require('./canonical-fixes.cjs'),{transfer}=require('./source-expressions.cjs');
const {create}=require('./upright.cjs'),{create:posture}=require('./posture.cjs');
const happy=require('./happy.cjs');
async function setup(){
 const root=path.resolve(__dirname,'../..'),assets=path.join(root,'src/Dororong.App/Assets');
 const load=n=>api.loadImage(path.join(assets,n));
 const master=correct(await api.loadImage(path.join(__dirname,'input/canonical-225.png')),api),open=await load('dororong-canonical.png');
 const masters={open,closed:await load('dororong-closed-eyes.png'),squint:await load('dororong-blink-squint.png')};
 for(const k of ['closed','squint'])masters[k]=transfer(master,open,masters[k],api,{kind:k});masters.open=master;
 const originals={...masters},postures={};
 for(const eye of Object.keys(masters)){postures[eye]=posture(masters[eye],open,api);masters[eye]=postures[eye].master;}
 return {api,root,open,masters,originals,postures};
}
async function main(){
 const {root,masters,originals,postures}=await setup(),out=path.join(root,'artifacts/high-resolution-motion-20261016-r5');fs.mkdirSync(out,{recursive:true});
 const manifest={schema:1,logicalSize:96,sourceSize:225,contentSize:144,outputSize:192,origin:[24,24],runtimeSelected:false,bodyAuthority:'Shared source-aligned rig with source225 folded-triangle hind contour; semantic original head retained separately',trackingDegrees:[-20,14],expressionDetail:'Authored 96px eye-only patches; no new detail claimed',frames:[],pending:['User review of five reported defects','Hunt/jump/sleep/grab/perch/cheek/eating','Windows/Mac runtime selection and user acceptance']};
 manifest.status='Candidate r5: source-pixel closed eyes; user visual acceptance pending';
 manifest.expressionDetail='Closed: authored96 eye texels with bounded cubic sampling, no new curves. Squint: unchanged r4 source225 composition, not user accepted.';
 manifest.sources=Object.fromEntries(['canonical-fixes.cjs','parts.cjs','body.cjs','source-expressions.cjs','expressions.cjs','sampling.cjs','upright.cjs','posture.cjs','happy.cjs','../PreviewLocomotion/reference-parts.cjs','input/canonical-225.png'].map(file=>[file,crypto.createHash('sha256').update(fs.readFileSync(path.join(__dirname,file))).digest('hex')]));
 const sheet=api.createCanvas(1152,1296),g=sheet.getContext('2d');g.fillStyle='#ece9e4';g.fillRect(0,0,1152,1296);
 function save(name,c){const b=c.toBuffer('image/png');fs.writeFileSync(path.join(out,name+'.png'),b);manifest.frames.push({name,width:c.width,height:c.height,sha256:crypto.createHash('sha256').update(b).digest('hex')});}
 function tile(c,col,row,label){g.drawImage(c,col*192,row*216);g.font='13px sans-serif';g.fillStyle='#352e36';g.fillText(label,col*192+24,row*216+200);}
 tile(create(originals.open,api,{padding:16}).still(),5,0,'Original-fixed reference');
 for(const [eye,master] of Object.entries(masters)){
  save('master-'+eye+'-225',master);const rig=create(master,api,{padding:16});save('standing-'+eye,rig.still());
  if(eye==='squint'){tile(rig.still(),1,0,'Squint');continue;}
  tile(rig.still(),eye==='open'?0:2,0,eye);
  for(const walking of [true,false])for(let i=0;i<(walking?32:48);i++){
   const count=walking?32:48,c=rig.render({amount:1,phase:i/count,walking});save(`${walking?'walk':'run'}-${eye}-${String(i).padStart(2,'0')}`,c);
   const step=walking?6:8;if(eye==='open'&&i%step===0)tile(c,i/step,walking?2:3,`${walking?'Walk':'Run'} ${i}/${count}`);
  }
  for(let i=0;i<=24;i++){
   const roll=(-20+i/24*34)*Math.PI/180,c=rig.render({roll,eyeX:roll*2,eyeY:-roll*1.5});save(`track-${eye}-${String(i).padStart(2,'0')}`,c);
   if(eye==='open'&&i%5===0)tile(c,i/5,1,`Look ${Math.round(roll*180/Math.PI)}°`);
  }
  for(let i=0;i<=32;i++)save(`breathe-${eye}-${String(i).padStart(2,'0')}`,rig.render({breathing:i/32}));
  const sr=postures[eye];
  for(let i=0;i<=64;i++){
   const c=sr.render(i/64);save(`sit-${eye}-${String(i).padStart(2,'0')}`,c);
   if(i%16===0)tile(c,i/16,eye==='open'?4:5,`Sit ${i}/64`);
  }
  // Rise is the same authored part path in reverse, not a second pose source.
  for(let i=0;i<=64;i++)save(`rise-${eye}-${String(i).padStart(2,'0')}`,sr.render(1-i/64));
  tile(sr.render(0),eye==='open'?3:4,0,'Sit entry '+eye);
  if(eye==='closed')for(let i=0;i<=65;i++)save(`happy-closed-${String(i).padStart(2,'0')}`,rig.render({roll:happy.roll(i/65*happy.times.at(-1))}));
 }
 save('motion-contact-sheet',sheet);fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
 console.log(`${manifest.frames.length} artifacts exported to ${out}. Not runtime-selected; see pending boundaries in manifest.`);
}
// Current CLI follows the accepted r15 source. Keep the old setup/export only
// for explicitly requested historical regression work, never as the default proof.
if(require.main===module)(process.argv.includes('--legacy-r5')?main():process.argv.includes('--r16')?require('./accepted-motion.cjs').main():process.argv.includes('--r17')?require('./polished-motion-review.cjs').main():process.argv.includes('--r18')?require('./rooted-run-review.cjs').main():process.argv.includes('--r19')?require('./connected-families-review.cjs').main():require('./sleep-review.cjs').main()).catch(e=>{console.error(e);process.exitCode=1;});
module.exports={setup};

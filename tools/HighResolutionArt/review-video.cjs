'use strict';
const fs=require('node:fs'),path=require('node:path');
const api=require(process.env.LOCOMOTION_CANVAS||path.join(process.env.USERPROFILE,'.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas'));
(async()=>{
 const root=path.resolve(__dirname,'../../artifacts/high-resolution-motion-20261016-r4'),out=path.join(root,'video-frames');fs.mkdirSync(out,{recursive:true});
 const cache=new Map(),image=async name=>{if(!cache.has(name))cache.set(name,await api.loadImage(path.join(root,name+'.png')));return cache.get(name);};
 const f=(kind,n)=>kind+'-'+String(n).padStart(2,'0');
 for(let n=0;n<120;n++){
  const age=n/30,sit=age<.4?0:age<1.45?(age-.4)/1.05:age<2?1:Math.max(0,1-(age-2)/.65);
  const items=[['Blink',n%90>40&&n%90<47?'standing-closed':n%90===40||n%90===47?'standing-squint':'standing-open'],
   ['Head tracking',f('track-open',Math.round(12+12*Math.sin(age*Math.PI/2)))],
   ['Walking',f('walk-open',n%32)],['Sit / rise',f('sit-open',Math.round(sit*64))],
   ['Snack gallop',f('run-open',n%48)],['Happy sway',f('happy-closed',Math.min(65,Math.round((age%3)/2.16*65)))]];
  const c=api.createCanvas(576,432),g=c.getContext('2d');g.fillStyle='#252a31';g.fillRect(0,0,c.width,c.height);g.imageSmoothingEnabled=false;
  for(const [i,[label,name]] of items.entries()){const x=i%3*192,y=Math.floor(i/3)*216;g.drawImage(await image(name),x,y);g.fillStyle='#e9e9eb';g.font='13px sans-serif';g.fillText(label,x+24,y+200);}
  fs.writeFileSync(path.join(out,String(n).padStart(3,'0')+'.png'),c.toBuffer('image/png'));
 }
 console.log(out);
})().catch(e=>{console.error(e);process.exitCode=1;});

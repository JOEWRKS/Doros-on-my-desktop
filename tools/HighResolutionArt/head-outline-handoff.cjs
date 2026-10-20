'use strict';
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {api,root}=require('./bangs-composite-review.cjs');
async function main(){
 const dir=path.join(root,'artifacts/head-outline-recovery-20261017-r24'),im=await api.loadImage(path.join(dir,'comparison.png')),c=api.createCanvas(900,630),g=c.getContext('2d');
 for(const [i,row]of [0,3,7].entries())g.drawImage(im,0,row*210,900,210,0,i*210,900,210);
 fs.writeFileSync(path.join(dir,'comparison-short.png'),c.toBuffer('image/png'));
 const manifest=JSON.parse(fs.readFileSync(path.join(dir,'manifest.json'))),sha=f=>crypto.createHash('sha256').update(fs.readFileSync(f)).digest('hex');
 manifest.sources={};for(const file of ['head-outline.cjs','sampling.cjs','upright.cjs','posture.cjs','accepted-motion.cjs','connected-families.cjs','head-outline-proof.cjs','head-outline.test.cjs'])manifest.sources['tools/HighResolutionArt/'+file]=sha(path.join(__dirname,file));
 manifest.visualReview={reviewedSheets:manifest.sheets,frames:manifest.frames.length,scope:'all numbered sheet frames at native size and magnified lower-head crop; not live runtime',observations:'continuous chin edge in sampled angles; no visible added body/face blur or thick outline; pixel-grid contrast still varies'};
 for(const f of manifest.frames)f.sha256=sha(path.join(dir,f.name+'.png'));
 fs.writeFileSync(path.join(dir,'manifest.json'),JSON.stringify(manifest,null,2));
 console.log(path.join(dir,'comparison-short.png'));
}
main().catch(e=>{console.error(e);process.exitCode=1;});

(function(root){
  'use strict';
  // One immutable standing texture. No seated/sleep artwork or image tween is
  // accepted here. The legacy study uses rigid parts; the isolated seated
  // endpoint additionally studies a soft, joint-anchored white body envelope.
  const rig=root.skeletonSitRig;
  const headBoundary=[[0,0],[96,0],[96,48],[69,48],[69,51],[67,55],[65,59],[63,63],[60,62],[58,58],[56,62],[52,66],[49,70],[46,72],[43,72],[42,68],[23,68],[20,70],[17,70],[0,70]];
  function inside(poly,x,y){let yes=false;for(let i=0,j=poly.length-1;i<poly.length;j=i++){
    const a=poly[i],b=poly[j];if((a[1]>y)!==(b[1]>y)&&x<(b[0]-a[0])*(y-a[1])/(b[1]-a[1])+a[0])yes=!yes;
  }return yes;}
  const canvas=(w=96,h=96)=>{const c=document.createElement('canvas');c.width=w;c.height=h;return c;};
  function create(standing){
    if(!standing||standing.width!==96||standing.height!==96)throw Error('Canonical 96x96 standing image required');
    const original=canvas(),oc=original.getContext('2d');oc.drawImage(standing,0,0);
    const source=oc.getImageData(0,0,96,96),layers=new Map();
    for(const p of rig.pose(0).parts){const c=canvas();layers.set(p.name,{canvas:c,data:c.getContext('2d').createImageData(96,96)});}
    function owner(x,y){
      if(inside(headBoundary,x+.5,y+.5))return 'head';
      if(x<31&&y>=69)return 'farFront'+(y<73?0:y<77?1:2);
      if(x>=31&&x<51&&y>=72)return 'nearFront'+(y<76?0:y<81?1:2);
      // The haunch belongs to the upper hind limb, not the torso plate. Keeping
      // it on the trunk leaves a standing rump above otherwise folded bones.
      if(x>=53&&y>=60&&y<72)return 'nearHind0';
      if(x>=57&&y>=68)return 'nearHind'+(y<72?0:y<77?1:2);
      return 'trunk';
    }
    for(let y=0;y<96;y++)for(let x=0;x<96;x++){
      const i=(y*96+x)*4;layers.get(owner(x,y)).data.data.set(source.data.subarray(i,i+4),i);
    }
    for(const layer of layers.values())layer.canvas.getContext('2d').putImageData(layer.data,0,0);
    const foreEdge=[];
    for(let y=77;y<=86;y++){
      let x=50;while(x>32&&source.data[(y*96+x)*4+3]<128)x--;
      if(x>32)foreEdge.push([x+1,y+.5]);
    }
    const outputs=new Map(),body=canvas(384,384),bc=body.getContext('2d');
    const ink=[80,52,61];
    // Re-outline the union of rigid white cutouts: the edges of a cut are not
    // anatomical ink. This has one pose, no target contour and no interpolation.
    function outlineBody(){
      const pixels=bc.getImageData(0,0,384,384);
      // Close only subpixel cut seams. This does not invent a destination shape
      // or shorten a limb; it seals the antialiased raster partition boundaries.
      let alpha=Uint8Array.from({length:384*384},(_,i)=>pixels.data[i*4+3]);
      for(const maximum of [true,false])for(const vertical of [false,true]){
        const next=new Uint8Array(alpha.length);
        for(let y=0;y<384;y++)for(let x=0;x<384;x++){
          let value=maximum?0:255;
          for(let k=-3;k<=3;k++){
            const xx=x+(vertical?0:k),yy=y+(vertical?k:0),a=xx<0||yy<0||xx>=384||yy>=384?0:alpha[yy*384+xx];
            if(maximum?a>value:a<value)value=a;
          }next[y*384+x]=value;
        }alpha=next;
      }
      // The support plane clips soft haunch paint, not bones or source images.
      // Re-outline after contact so the seated base has a continuous floor edge.
      for(let i=0;i<alpha.length;i++)pixels.data[i*4+3]=i>=350*384?0:alpha[i];
      const src=pixels.data.slice();
      // Source body ink is about1.4 native pixels wide. A1px reconstruction
      // loses roughly30% of its contrast after downsampling at actual size.
      const offsets=[];for(let y=-6;y<=6;y++)for(let x=-6;x<=6;x++)if(x*x+y*y<=5.6*5.6)offsets.push([x,y]);
      for(let y=0;y<384;y++)for(let x=0;x<384;x++){
        const i=(y*384+x)*4,a=src[i+3];if(!a)continue;
        let interior=a;for(const [dx,dy] of offsets){
          const xx=x+dx,yy=y+dy,near=xx<0||yy<0||xx>=384||yy>=384?0:src[(yy*384+xx)*4+3];if(near<interior)interior=near;
        }
        for(let c=0;c<3;c++)pixels.data[i+c]=ink[c]+(255-ink[c])*interior/a;
      }
      bc.resetTransform();bc.putImageData(pixels,0,0);
    }
    function render(time,{direction='sit',scale=1,debug=false}={}){
      if(![1,2,3,4].includes(scale))throw Error('Render scale must be 1,2,3 or4');
      const pose=rig.pose(time,direction),key=scale;
      if(!outputs.has(key))outputs.set(key,canvas(96*scale,96*scale));
      const out=outputs.get(key),ctx=out.getContext('2d');ctx.resetTransform();ctx.clearRect(0,0,out.width,out.height);ctx.scale(scale,scale);
      {
        const byName=new Map(pose.parts.map(p=>[p.name,p]));
        bc.resetTransform();bc.clearRect(0,0,384,384);bc.scale(4,4);
        bc.fillStyle='#fff';
        // Missing paint under the head is a bounded neck attachment, carried
        // by its two parents. It is never sampled from another pose.
        const hp=p=>rig.transform(p,byName.get('head').matrix),tp=p=>rig.transform(p,byName.get('trunk').matrix);
        const neck=[hp([21,65]),hp([42,64]),hp([59,56]),hp([68.5,47.5])];
        bc.beginPath();neck.forEach((p,i)=>i?bc.lineTo(...p):bc.moveTo(...p));
        bc.bezierCurveTo(...hp([72,50]),...tp([73,51]),...tp([72,61]));
        for(const p of [tp([63,72]),tp([44,76]),hp([22,72])])bc.lineTo(...p);
        bc.closePath();bc.fill();
        // Small underlapping white joint pads only cover cut interiors. The
        // canonical outer ink stays on each rigid texture segment.
        for(const leg of pose.legs){
          bc.fillStyle='#fff';
          bc.strokeStyle='#fff';bc.lineWidth=leg.name.includes('Hind')?6.5:5;bc.lineCap=bc.lineJoin='round';
          bc.beginPath();leg.joints.slice(0,3).forEach((p,j)=>j?bc.lineTo(...p):bc.moveTo(...p));bc.stroke();
          for(let j=0;j<3;j++){
            const p=leg.joints[j],haunch=leg.name.includes('Hind')&&j===1;
            const radius=j===0?4.3:haunch?3+3*pose.fold**3:3;
            bc.beginPath();bc.arc(p[0],p[1],radius,0,Math.PI*2);bc.fill();
          }
        }
        function draw(name){
          const part=byName.get(name);bc.save();bc.transform(...part.matrix);bc.drawImage(layers.get(name).canvas,0,0);bc.restore();
        }
        // Distant hind foot borrows the SAME standing hind leg, behind the torso.
        for(let j=0;j<3;j++){
          const from=rig.definitions[2].joints,to=pose.legs[3].joints;
          bc.save();bc.transform(...rig.boneMatrix(from[j],from[j+1],to[j],to[j+1]));bc.drawImage(layers.get('nearHind'+j).canvas,0,0);bc.restore();
        }
        for(let j=2;j>=0;j--)draw('farFront'+j);
        draw('trunk');
        for(let j=0;j<3;j++)draw('nearHind'+j);
        for(let j=0;j<3;j++)draw('nearFront'+j);
        outlineBody();
        // Preserve the near foreleg's ownership in front of the folded haunch.
        // A union outline alone erases this boundary and looks like a third paw.
        const coverage=bc.getImageData(0,0,384,384).data;
        bc.save();bc.scale(4,4);bc.globalCompositeOperation='source-atop';bc.strokeStyle='#50343d';bc.lineWidth=1;bc.lineCap='round';
        for(let i=1;i<foreEdge.length;i++){
          const sourceA=foreEdge[i-1],sourceB=foreEdge[i];
          const ma=byName.get('nearFront'+(sourceA[1]<81?1:2)).matrix,mb=byName.get('nearFront'+(sourceB[1]<81?1:2)).matrix;
          const a=rig.transform(sourceA,ma),b=rig.transform(sourceB,mb);
          const x=Math.round(((a[0]+b[0])/2+ma[0]*1.8)*4),y=Math.round(((a[1]+b[1])/2+ma[1]*1.8)*4);
          const alpha=x<0||y<0||x>=384||y>=384?0:coverage[(y*384+x)*4+3]/255;
          if(alpha){bc.globalAlpha=alpha;bc.beginPath();bc.moveTo(...a);bc.lineTo(...b);bc.stroke();}
        }bc.restore();
        // As the shin folds against the thigh its upper edge becomes an
        // interior crease. Carry it with that bone, never with a target image.
        const leg=pose.legs[2],a=leg.joints[1],b=leg.joints[2],length=leg.lengths[1];
        const tangent=[(b[0]-a[0])/length,(b[1]-a[1])/length],normal=[tangent[1],-tangent[0]];
        const visible=pose.fold*pose.fold;
        if(visible>0){
          const start=[a[0]+tangent[0]*1.2+normal[0]*1.1,a[1]+tangent[1]*1.2+normal[1]*1.1];
          bc.save();bc.scale(4,4);bc.globalCompositeOperation='source-atop';bc.strokeStyle='#50343d';bc.lineWidth=.8;bc.lineCap='round';
          bc.beginPath();bc.moveTo(...start);bc.lineTo(start[0]+tangent[0]*4.5*visible,start[1]+tangent[1]*4.5*visible);bc.stroke();bc.restore();
        }
        ctx.drawImage(body,0,0,96,96);
        ctx.save();ctx.transform(...byName.get('head').matrix);ctx.drawImage(layers.get('head').canvas,0,0);ctx.restore();
      }
      if(debug){
        ctx.lineWidth=.65;
        for(const [i,leg] of pose.legs.entries()){
          ctx.strokeStyle=ctx.fillStyle=['#009eb0','#005bda','#ef663c','#8f55c4'][i];ctx.beginPath();
          leg.joints.forEach((p,j)=>j?ctx.lineTo(...p):ctx.moveTo(...p));ctx.stroke();
          for(const p of leg.joints){ctx.beginPath();ctx.arc(...p,1,0,Math.PI*2);ctx.fill();}
        }
      }
      return out;
    }
    // Skin authored around the user-accepted seated rig is inverse-bound once.
    // Every control point follows its actual bone(s), never a time interpolation
    // between two outlines. White skin is reconstructed, not seated-image pixels.
    const seatedBind=new Map(rig.pose(1).parts.map(p=>[p.name,p.matrix]));
    function renderSoft(time,{direction='sit',scale=1,debug=false}={}){
      if(![1,2,3,4].includes(scale))throw Error('Render scale must be 1,2,3 or4');
      const pose=rig.pose(time,direction),byName=new Map(pose.parts.map(p=>[p.name,p.matrix]));
      const out=canvas(96*scale,96*scale),ctx=out.getContext('2d');
      const skin=canvas(384,384),s=skin.getContext('2d');s.scale(4,4);
      const bind=rig.pose(1),far=bind.legs[0].joints.at(-1),near=bind.legs[1].joints.at(-1);
      const hip=bind.legs[2].joints[0],knee=bind.legs[2].joints[1];
      function carry(name,p){
        const m=seatedBind.get(name),dx=p[0]-m[4],dy=p[1]-m[5];
        return rig.transform([m[0]*dx+m[1]*dy,m[2]*dx+m[3]*dy],byName.get(name));
      }
      const mix=(a,b,weight=.5)=>[a[0]*weight+b[0]*(1-weight),a[1]*weight+b[1]*(1-weight)];
      // Small pose-space tissue corrections are restricted to the armpit and
      // paw pads. They respond to foreleg rotation, not timeline/endpoint blends.
      const relaxed=name=>Math.max(0,Math.min(1,1-Math.abs(Math.atan2(byName.get(name)[1],byName.get(name)[0]))/.17));
      const farRelax=relaxed('farFront2'),nearRelax=relaxed('nearFront2');
      const trunkMatrix=byName.get('trunk');
      const haunchRelax=Math.max(0,Math.min(1,1-Math.atan2(trunkMatrix[1],trunkMatrix[0])/.42));
      const fp=(x,y,dx=0,dy=0)=>{const p=carry('farFront2',[far[0]+x,far[1]+y]);return [p[0]+dx*farRelax,p[1]+dy*farRelax];};
      const np=(x,y,dx=0,dy=0)=>{const p=carry('nearFront2',[near[0]+x,near[1]+y]);return [p[0]+dx*nearRelax,p[1]+dy*nearRelax];};
      const hp=(x,y,dx=0)=>{const p=[hip[0]+x,hip[1]+y],q=mix(carry('trunk',p),carry('nearHind0',p));return [q[0]+dx*haunchRelax,q[1]];};
      const kp=(x,y)=>{const p=[knee[0]+x,knee[1]+y];return mix(carry('trunk',p),carry('nearHind1',p));};
      const head=p=>carry('head',p);
      // The original distal hind foot tucks underneath the thigh as the pelvis
      // descends. Occlusion is geometric; no opacity fading switches the paw off.
      for(const name of ['nearHind1','nearHind2']){
        const segment=Number(name.at(-1)),joints=rig.definitions[2].joints,a=joints[segment],b=joints[segment+1];
        const angle=Math.atan2(b[1]-a[1],b[0]-a[0]);
        // A tucked paw turns edge-on beneath the haunch. Compress transverse
        // soft tissue, not length along the fixed bone; keep opacity untouched.
        s.save();s.transform(...byName.get(name));s.translate(...a);s.rotate(angle);
        s.scale(1,1-.65*pose.fold);s.rotate(-angle);s.translate(-a[0],-a[1]);
        s.drawImage(layers.get(name).canvas,0,0);s.restore();
      }
      s.fillStyle='#fff';s.strokeStyle='#625c60';s.lineWidth=1.25;s.lineCap=s.lineJoin='round';
      // Front skin has its own inner edges: the torso cannot fill the crotch.
      // Contact surfaces meet the same y=87.5 plane as the skeletal study.
      s.beginPath();s.moveTo(...fp(-7.5,-22));
      s.bezierCurveTo(...fp(-8,-13),...fp(-6.5,-3),...fp(-3,0));
      s.bezierCurveTo(...fp(1,3),...fp(4,0,-1),...fp(4.8,-4,-1.4));
      s.bezierCurveTo(...fp(6,-6,-3,3),...fp(6.4,-8.5,-3,3),...fp(6.8,-10,-3,3));
      s.bezierCurveTo(...np(-6,-5,-1),...np(-5,0),...np(-2,1,1.25,-.1));
      s.bezierCurveTo(...np(1,2,1.5,.25),...np(4,0,1.5,.5),...np(5,-4));
      // The folded thigh is one soft volume around hip/knee, not three cutouts
      // whose separated corners become false notches or extra toes.
      const bellyAttachment=[near[0]+4.8,near[1]+1];
      s.bezierCurveTo(...mix(carry('nearFront2',bellyAttachment),carry('nearHind0',bellyAttachment),.55),...kp(-7.5,4),...kp(-3,3.6));
      s.bezierCurveTo(...kp(4,4),...hp(10,10,-4.5),...hp(11.8,1,-3));
      s.bezierCurveTo(...hp(13,-7,-2.5),...hp(7,-13),...hp(5.2,-17));
      s.bezierCurveTo(...head([62.8,55]),...head([66.8,51]),...head([67,47.5]));
      s.lineTo(...head([57,52]));s.lineTo(...head([38,61]));s.closePath();s.fill();s.stroke();
      // Near foreleg occludes the thigh. A short knee crease joins the boundary
      // instead of creating a floating isolated line on the haunch.
      s.beginPath();s.moveTo(...np(5,-4));
      s.bezierCurveTo(...np(5.7,-7),...np(6.5,-9),...np(7,-12));s.stroke();
      s.beginPath();s.moveTo(...np(6.4,-8.5));
      s.quadraticCurveTo(...np(9,-8.5),...np(11.5,-10));s.stroke();
      ctx.scale(scale,scale);ctx.drawImage(skin,0,0,96,96);
      ctx.save();ctx.transform(...byName.get('head'));ctx.drawImage(layers.get('head').canvas,0,0);ctx.restore();
      if(debug){
        ctx.lineWidth=.65;
        pose.legs.forEach((leg,i)=>{
          ctx.strokeStyle=ctx.fillStyle=['#009eb0','#005bda','#ef663c','#8f55c4'][i];
          ctx.beginPath();leg.joints.forEach((p,j)=>j?ctx.lineTo(...p):ctx.moveTo(...p));ctx.stroke();
          for(const p of leg.joints){ctx.beginPath();ctx.arc(...p,1,0,Math.PI*2);ctx.fill();}
        });
      }
      return out;
    }
    function renderSeatedEndpoint(options={}){return renderSoft(1,{...options,direction:'sit'});}
    return {render,renderSeatedEndpoint,renderSoft};
  }
  root.skeletonSit={create};
})(globalThis);

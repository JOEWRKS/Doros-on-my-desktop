'use strict';
// Source225 landmarks. This mask owns foreground hair/face rim independently of
// either eraser or eyelid layer. Every protected RGBA pixel is copied from source.
const fringe=[[36,122],[38,118],[40,117],[42,115],[44,114],[46,109],[48,107],[50,103],[52,100],[53,99],[54,103],[55,106],[56,111],[58,114],[60,116],[62,118],[64,119],[66,120],[68,122],[70,123],[72,123],[74,122],[76,120],[78,118],[80,119],[82,118],[84,116],[86,112],[88,104],[89,98],[90,105],[92,108],[94,112],[96,113],[98,115],[100,117],[102,118],[104,119],[106,119],[108,120]];
const rightRim=[[120,107],[123,105],[128,104],[133,103],[138,101],[143,99],[145,98]];
const lashSpur=[[107,43,45],[108,41,44],[109,41,43],[110,41,42]];
function isLashSpur(x,y){return lashSpur.some(([yy,x0,x1])=>y===yy&&x>=x0&&x<=x1);}
function interp(points,x){if(x<=points[0][0])return points[0][1];for(let j=1;j<points.length;j++)if(x<=points[j][0]){const a=points[j-1],b=points[j],t=(x-a[0])/(b[0]-a[0]);return a[1]*(1-t)+b[1]*t;}return points.at(-1)[1];}
function protectedAt(x,y){
 if(isLashSpur(x,y))return false;
 if(x<36||x>108||y<100||y>141)return true;
 if(y<=interp(fringe,x))return true;
 if(y>=120&&x>=interp(rightRim,y))return true;
 if(x<74&&y>=134)return true;
 if(x>=74&&y>=141)return true;
 if(x>=55&&x<80&&y>=135)return true;
 return false;
}
module.exports={protectedAt,fringe,rightRim,isLashSpur};

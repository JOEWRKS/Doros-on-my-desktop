namespace Dororong.App.Controls;

// Prepared once, at the source's native density. The immutable artwork stays
// untouched; only the closed expression's fringe/lid ink is normalized.
internal static class HighResolutionClosedInk
{
    private const int Size=225;
    internal static float[] Prepare(float[] source){
        var ink=new bool[Size*Size];
        for(var i=0;i<ink.Length;i++)ink[i]=source[i*4+3]>200&&Math.Max(source[i*4],Math.Max(source[i*4+1],source[i*4+2]))<155;
        var skeleton=(bool[])ink.Clone();
        var remove=new List<int>();
        bool changed;
        do{
            changed=false;
            for(var pass=0;pass<2;pass++){
                remove.Clear();
                for(var y=1;y<Size-1;y++)for(var x=1;x<Size-1;x++){
                    var i=y*Size+x;if(!skeleton[i])continue;
                    var p2=skeleton[i-Size];var p3=skeleton[i-Size+1];var p4=skeleton[i+1];var p5=skeleton[i+Size+1];
                    var p6=skeleton[i+Size];var p7=skeleton[i+Size-1];var p8=skeleton[i-1];var p9=skeleton[i-Size-1];
                    var n=(p2?1:0)+(p3?1:0)+(p4?1:0)+(p5?1:0)+(p6?1:0)+(p7?1:0)+(p8?1:0)+(p9?1:0);
                    var transitions=(!p2&&p3?1:0)+(!p3&&p4?1:0)+(!p4&&p5?1:0)+(!p5&&p6?1:0)+
                        (!p6&&p7?1:0)+(!p7&&p8?1:0)+(!p8&&p9?1:0)+(!p9&&p2?1:0);
                    if(n<2||n>6||transitions!=1)continue;
                    if(pass==0?p2&&p4&&p6||p4&&p6&&p8:p2&&p4&&p8||p2&&p6&&p8)continue;
                    remove.Add(i);
                }
                foreach(var i in remove)skeleton[i]=false;
                changed|=remove.Count>0;
            }
        }while(changed);
        var points=new (double X,double Y)[ink.Length];
        for(var y=1;y<Size-1;y++)for(var x=1;x<Size-1;x++){
            var i=y*Size+x;if(!skeleton[i])continue;
            double sx=x*2,sy=y*2;var count=2;
            for(var dy=-1;dy<=1;dy++)for(var dx=-1;dx<=1;dx++){
                if((dx!=0||dy!=0)&&skeleton[i+dy*Size+dx]){sx+=x+dx;sy+=y+dy;count++;}
            }
            points[i]=(sx/count,sy/count);
        }
        var result=(float[])source.Clone();
        for(var y=76;y<=136;y++)for(var x=29;x<=115;x++){
            var lid=(x>=35&&x<=64&&y>=120&&y<=130)||(x>=78&&x<=107&&y>=126&&y<=136);
            var fringe=!lid&&y<=126&&x>=33&&x<=114;
            if(!lid&&!fringe)continue;
            var i=y*Size+x;var at=i*4;
            if(source[at+3]<250)continue;
            // Pink/white colour fields are not line pixels. Limit correction
            // to the existing ink and its native antialias collar.
            var hi=Math.Max(source[at],Math.Max(source[at+1],source[at+2]));
            if(hi>=240)continue;
            if(source[at+2]>180&&source[at+2]-source[at+1]>30)continue;
            var nearest=double.PositiveInfinity;
            for(var dy=-5;dy<=5;dy++)for(var dx=-5;dx<=5;dx++){
                var j=(y+dy)*Size+x+dx;if(!skeleton[j])continue;
                var a=points[j];
                for(var ey=-1;ey<=1;ey++)for(var ex=-1;ex<=1;ex++){
                    if((ex==0&&ey==0)||!skeleton[j+ey*Size+ex])continue;
                    var b=points[j+ey*Size+ex];var vx=b.X-a.X;var vy=b.Y-a.Y;
                    var den=vx*vx+vy*vy;
                    var t=den==0?0:Math.Clamp(((x-a.X)*vx+(y-a.Y)*vy)/den,0,1);
                    var px=x-a.X-t*vx;var py=y-a.Y-t*vy;
                    nearest=Math.Min(nearest,Math.Sqrt(px*px+py*py));
                }
            }
            if(nearest>5)continue;
            var coverage=Math.Clamp(1.5-nearest,0,1); // 2 native-pixel ink + AA.
            var donor=-1;var distance=int.MaxValue;
            for(var dy=-8;dy<=8;dy++)for(var dx=-8;dx<=8;dx++){
                var j=((y+dy)*Size+x+dx)*4;var d=dx*dx+dy*dy;
                if(d>=distance||source[j+3]<250)continue;
                var min=Math.Min(source[j],Math.Min(source[j+1],source[j+2]));
                if(min<175||Math.Max(source[j],Math.Max(source[j+1],source[j+2]))<235)continue;
                donor=j;distance=d;
            }
            if(donor<0)continue;
            for(var c=0;c<3;c++){
                var color=c==1?35d:45d;
                result[at+c]=(float)(source[donor+c]*(1-coverage)+color*coverage);
            }
        }
        return result;
    }
}

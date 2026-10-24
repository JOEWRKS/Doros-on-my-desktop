namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    private float[]? angryHead;
    private bool angryHeadFurious;

    // One bounded source, shared by every reaction frame and scale. Never edit
    // the accepted open/closed artwork or cache a source for each animation age.
    private float[] AngryHead(bool furious)
    {
        if (angryHead is not null && angryHeadFurious == furious) return angryHead;
        var source = head;
        var result = angryHead ?? new float[source.Length];
        HighResolutionBodyInk.Invalidate(result);
        Array.Copy(source,result,source.Length);
        angryHeadFurious = furious;
        const int size = 225;
        // User reference: open purple eyes, brows descending toward the nose,
        // and red comic veins. The third reaction additionally pinches the
        // lower lids; the original iris colour/highlights are kept above them.
        Line(17.6,46,26.7,50.4,1.4,35,27,35);
        // The authored far upper lid already supplies the reference's brow.
        // An extra diagonal above it read as two eyebrows; keep that one line.
        if (!furious) Vein(47,37,1);
        else
        {
            Vein(29.5,35,0.85);
            Vein(44.5,42,0.9);
            // The enlarged reference has a visibly pinched near lower eyelid,
            // not two round eyes with extra marks underneath. Lift only its
            // lower band into a cheek crease; do not move the brow/hair opening.
            ReadOnlySpan<(double X,double Y)> nearLid =
                [(17.3,53.65),(19,53.8),(21,54.35),(23,55.55),(24.7,56.8),(26,55.7),(27.5,54.1)];
            LowerLid(nearLid,58.3);
            // The user circled the rising inner connector for removal. End
            // the lower-lid stroke before that circle; do not redraw its tail.
            Curve(17.3,53.65,19.9,53.65,22.2,54.5,23.9,55.9,1.12);
            // Keep the far eye's original outline: its lower-right corner
            // adjoins the hair. A rectangular skin fill would erase that seam.
            // Only the old omega mouth footprint is replaced. Use the clean
            // cheek donor: same-row x83 can touch iris ink and leave a grey bar.
            for(var y=135;y<=145;y++) for(var x=59;x<=81;x++)
            {
                var at=(y*size+x)*4;
                if(source[at+3]<250)continue;
                var donor=(147*size+83)*4;
                for(var c=0;c<3;c++)result[at+c]=source[donor+c];
            }
            // Nose bridge: two compressed arcs.
            // Paired short glabellar folds, separated from the lower eyelid.
            // Trace the latest annotated blue guides in the 432px face crop:
            // crop origin(14 2/3,42 2/3), 12px/DIP, annotation top inset6px.
            // Both prior bridge strokes are replaced, not layered underneath.
            Curve(28.5,52.08,28.2,53.05,27.85,53.9,27.25,54.42,.75);
            Curve(32.08,52.75,31.85,53.75,32.6,54.7,33.42,55.42,.75);
            Curve(27.4,54.7,28.5,53.7,30.1,53.7,31.3,54.7,.65);
            Curve(27.8,55.8,28.8,55.1,30.1,55.1,31.3,55.7,.62);
            // Two continuous, inward-curving muzzle folds replace the tiny
            // disconnected hatches. Keep a clear central skin channel and a
            // gap above the mouth; each curve is antialiased only once.
            Curve(27.5,56.85,27.45,57.3,27.9,57.65,28.5,57.8,.55);
            Curve(32.65,56.85,32.5,57.3,32.05,57.65,31.45,57.8,.55);
            // Curved, clenched omega, not the previous thin zigzag. The two
            // lobes meet at a high central pinch; corners and chin lines taper.
            Curve(25.7,58.8,25.4,61.8,28.1,62.1,29.9,58.9,1.12);
            Curve(29.9,58.9,30.1,61.8,33.6,61.8,33.7,59.8,1.12);
            Curve(33.7,59.8,33.8,59.3,33.5,58.9,33.1,58.8,.85);
            Curve(24.4,58.8,23.8,59,24.1,60.1,24.4,60.3,.65);
            Curve(26.7,61.1,26.7,61.6,26.8,62.2,26.7,62.7,.55);
            Curve(29.1,61.4,29.2,62,29.1,62.7,29.2,63.1,.55);
            Curve(32.6,61.1,32.5,61.6,32.6,62,32.5,62.5,.55);
            // Cheek folds echo each lower eyelid rather than unrelated hatches.
            Curve(18.5,55.8,20.8,55.8,22.2,56.9,23.2,58.2,.7);
            Curve(19,57.2,20.5,57.3,21.6,58,22.1,58.8,.48);
            Curve(36.5,61.2,37.3,60.8,38.5,60.8,39.3,61,.5);
        }
        return angryHead=result;

        void LowerLid(ReadOnlySpan<(double X,double Y)> points,double bottom)
        {
            var factor=size/96d;
            for(var x=(int)Math.Ceiling(points[0].X*factor);x<=Math.Floor(points[^1].X*factor);x++){
                var q=x/factor;var index=1;while(index<points.Length-1&&points[index].X<q)index++;
                var a=points[index-1];var b=points[index];var top=a.Y+(b.Y-a.Y)*(q-a.X)/(b.X-a.X);
                for(var y=(int)Math.Floor(top*factor);y<=Math.Floor(bottom*factor);y++){
                    var at=(y*size+x)*4;if(source[at+3]<250)continue;
                    var coverage=Math.Clamp(y-top*factor+.5,0,1);
                    // Immutable white cheek donor, outside eyes and mouth.
                    var donor=(147*size+83)*4;
                    for(var c=0;c<3;c++)result[at+c]=(float)(result[at+c]*(1-coverage)+source[donor+c]*coverage);
                }
            }
        }
        void Curve(double ax,double ay,double bx,double by,double cx,double cy,double dx,double dy,double width)
        {
            // Rasterise the whole path once. Alpha-blending 24 short lines
            // repeatedly darkened AA pixels and made tiny curves look swollen.
            Span<(double X,double Y)> points=stackalloc (double,double)[25];
            var factor=size/96d;
            for(var i=0;i<=24;i++){
                var t=i/24d;var s=1-t;
                points[i]=((s*s*s*ax+3*s*s*t*bx+3*s*t*t*cx+t*t*t*dx)*factor,
                    (s*s*s*ay+3*s*s*t*by+3*s*t*t*cy+t*t*t*dy)*factor);
            }
            var radius=width*factor/2;
            var left=Math.Max(0,(int)Math.Floor(Math.Min(Math.Min(ax,bx),Math.Min(cx,dx))*factor-radius-1));
            var right=Math.Min(size-1,(int)Math.Ceiling(Math.Max(Math.Max(ax,bx),Math.Max(cx,dx))*factor+radius+1));
            var top=Math.Max(0,(int)Math.Floor(Math.Min(Math.Min(ay,by),Math.Min(cy,dy))*factor-radius-1));
            var bottom=Math.Min(size-1,(int)Math.Ceiling(Math.Max(Math.Max(ay,by),Math.Max(cy,dy))*factor+radius+1));
            for(var y=top;y<=bottom;y++)for(var x=left;x<=right;x++){
                var distance=double.PositiveInfinity;
                for(var i=1;i<points.Length;i++){
                    var a=points[i-1];var b=points[i];var vx=b.X-a.X;var vy=b.Y-a.Y;
                    var den=vx*vx+vy*vy;var t=den==0?0:Math.Clamp(((x-a.X)*vx+(y-a.Y)*vy)/den,0,1);
                    var qx=x-a.X-t*vx;var qy=y-a.Y-t*vy;distance=Math.Min(distance,qx*qx+qy*qy);
                }
                var coverage=Math.Clamp(radius+.5-Math.Sqrt(distance),0,1);
                var at=(y*size+x)*4;if(source[at+3]<250)continue;
                result[at]=(float)(result[at]*(1-coverage)+35*coverage);
                result[at+1]=(float)(result[at+1]*(1-coverage)+27*coverage);
                result[at+2]=(float)(result[at+2]*(1-coverage)+35*coverage);
            }
        }
        void Vein(double cx,double cy,double scale)
        {
            // Four curved, separated inward hooks, not a plus or a square.
            Hook(-4,-1,-1,-1,-1,-5);Hook(1,-5,1,-1,4,-1);
            Hook(-4,1,-1,1,-1,5);Hook(1,5,1,1,4,1);
            void Hook(double ax,double ay,double bx,double by,double cx1,double cy1)
            {
                var px=ax;var py=ay;
                for(var i=1;i<=12;i++){
                    var t=i/12d;var s=1-t;
                    var x=s*s*ax+2*s*t*bx+t*t*cx1;var y=s*s*ay+2*s*t*by+t*t*cy1;
                    Line(cx+px*scale,cy+py*scale,cx+x*scale,cy+y*scale,1.05,42,31,205);
                    px=x;py=y;
                }
            }
        }
        void Line(double ax,double ay,double bx,double by,double width,double blue,double green,double red)
        {
            var factor=size/96d;ax*=factor;ay*=factor;bx*=factor;by*=factor;
            var radius=width*factor/2;var vx=bx-ax;var vy=by-ay;var den=vx*vx+vy*vy;
            for(var y=Math.Max(0,(int)Math.Floor(Math.Min(ay,by)-radius-1));y<=Math.Min(size-1,(int)Math.Ceiling(Math.Max(ay,by)+radius+1));y++)
            for(var x=Math.Max(0,(int)Math.Floor(Math.Min(ax,bx)-radius-1));x<=Math.Min(size-1,(int)Math.Ceiling(Math.Max(ax,bx)+radius+1));x++)
            {
                var t=Math.Clamp(((x-ax)*vx+(y-ay)*vy)/den,0,1);
                var dx=x-ax-t*vx;var dy=y-ay-t*vy;
                var coverage=Math.Clamp(radius+.5-Math.Sqrt(dx*dx+dy*dy),0,1);
                var at=(y*size+x)*4;if(source[at+3]<250)continue;
                result[at]=(float)(result[at]*(1-coverage)+blue*coverage);
                result[at+1]=(float)(result[at+1]*(1-coverage)+green*coverage);
                result[at+2]=(float)(result[at+2]*(1-coverage)+red*coverage);
            }
        }
    }
}

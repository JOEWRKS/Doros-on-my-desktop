using System.Windows;

namespace Dororong.App.Controls;

internal sealed partial class HighResolutionGrabRaster
{
    // The authored seated chest bows 2.3 DIP outside the foreleg's straight
    // edge. Transport that small collar inward; retain its original ink and
    // colour, the paw tip, belly, head, and every standing/non-seated source.
    internal static double SeatedContourSource(double x,double y,int frame){
        if(frame<257||frame>321||y<=58||y>=73||x>=30)return x;
        var shift=2.3*Smooth((frame-257)/64d)*Smooth((y-58)/4)*(1-Smooth((y-67)/6));
        if(shift==0)return x;
        var source=x-shift;
        for(var i=0;i<6;i++){
            var t=(source-22)/8;var weight=1-Smooth(t);
            var derivative=t<=0||t>=1?0:-6*t*(1-t)/8;
            source-=(source+shift*weight-x)/(1+shift*derivative);
        }
        return source;
    }
    // Shake has an exact inverse; tapping no longer distorts source texels.
    internal readonly struct SeatedWarp(HighResolutionFacePose face)
    {
        internal bool Active=>face.SeatedShake!=0;
        internal Point Map(Point p)=>new(p.X+face.SeatedShake*(1-Smooth((p.Y-63)/15)),p.Y);
        internal Point Source(double x,double y)=>new(x-face.SeatedShake*(1-Smooth((y-63)/15)),y);
    }

    private SeatedBodyDistance? seatedBodyDistance;
    internal readonly struct SeatedPaw(HighResolutionFacePose face,double scale)
    {
        internal bool Active=>face.SeatedPawAmount>0;
        internal Point Tip {
            get {
                var amount=face.SeatedPawAmount;
                // A newly authored rounded paw rotates out from the shoulder;
                // the two taps change its reach, not the source image width.
                var angle=Math.PI/2+(Math.PI-face.SeatedPawAngle-Math.PI/2)*amount;
                var length=7+(10+2*face.SeatedPawPulse)*amount;
                return new(25+Math.Cos(angle)*length,73+Math.Sin(angle)*length);
            }
        }
        private double ArmDistance(double x,double y){
            var end=Tip;var dx=end.X-25;var dy=end.Y-73;
            var length=Math.Sqrt(dx*dx+dy*dy);
            var nx=dy/length;var ny=-dx/length;
            // Bow the entire limb, not just its end cap. A smooth width
            // profile gives both sides a rounded sweep instead of parallel
            // straight edges. Root/tip and pointer-directed reach stay fixed.
            var bend=3.1*Smooth(face.SeatedPawAmount);
            var t=Math.Clamp(((x-25)*dx+(y-73)*dy)/(length*length),0,1);
            for(var i=0;i<5;i++){
                var bow=4*bend*t*(1-t);
                var qx=25+t*dx+nx*bow-x;var qy=73+t*dy+ny*bow-y;
                var vx=dx+nx*4*bend*(1-2*t);var vy=dy+ny*4*bend*(1-2*t);
                var denominator=vx*vx+vy*vy-8*bend*(qx*nx+qy*ny);
                if(denominator<=1e-8)break;
                t=Math.Clamp(t-(qx*vx+qy*vy)/denominator,0,1);
            }
            var offset=4*bend*t*(1-t);
            var radius=6-1.2*Smooth(t)+.6*Math.Sin(Math.PI*t)*Smooth(face.SeatedPawAmount);
            return Math.Sqrt(Math.Pow(x-25-t*dx-nx*offset,2)+Math.Pow(y-73-t*dy-ny*offset,2))-radius;
        }
        internal void Composite(SeatedBodyDistance field,double x,double y,Span<double> pixel){
            if(!Active||x>=39||x<=-4||y<=47||y>=92)return;
            var amount=face.SeatedPawAmount;
            var original=field.Distance(x,y);
            // Remove the original hanging foreleg, retaining the diagonal
            // chest/belly boundary. Unioning the new arm removes a root cap.
            var cut=Math.Min(y-71,(31+.65*(y-80)-x)/1.193686);
            var torso=Math.Max(original,cut);
            var arm=ArmDistance(x,y);
            var junction=Math.Max(1.5-Math.Abs(torso-arm),0)/1.5;
            var authored=Math.Min(torso,arm)-junction*junction*.375;
            var distance=original+(authored-original)*Smooth(amount/.35);
            var alpha=Math.Clamp(.5-distance,0,1)*255;
            var width=1.295415959252971/Math.Max(1,scale);
            var ink=Smooth((distance+width+.25)/.5);
            var boundary=Smooth((39-x)/3)*Smooth((y-47)/3)*Smooth((92-y)/3);
            // Retain the original interior line only during the short entry
            // morph; it is absent once this authored limb replaces the old one.
            var oldInk=pixel[3]>250?Math.Clamp((255-pixel[0])/199,0,1)*(1-Smooth(amount/.35)):0;
            ink=Math.Max(ink,oldInk);
            pixel[0]+=(alpha*(1-ink+ink*56/255)-pixel[0])*boundary;
            pixel[1]+=(alpha*(1-ink+ink*52/255)-pixel[1])*boundary;
            pixel[2]+=(alpha*(1-ink+ink*68/255)-pixel[2])*boundary;
            pixel[3]+=(alpha-pixel[3])*boundary;
        }
    }
    // Fixed ~156 KiB alpha-distance tile, shared across directions/angles/ages.
    // No per-cursor or per-frame bitmap cache.
    internal sealed class SeatedBodyDistance
    {
        private const int W=177,H=225;
        private readonly float[] values=new float[W*H];
        internal SeatedBodyDistance(float[] body){
            var edges=new List<Point>();
            double Alpha(double x,double y){
                Span<double> p=stackalloc double[4];
                Sample(body,225,SeatedContourSource(x,y,321)*225/96-.5,y*225/96-.5,p);
                return p[3]/255;
            }
            for(var row=0;row<224;row++)for(var col=0;col<224;col++){
                var a=body[(row*225+col)*4+3]/255d;
                var b=body[(row*225+col+1)*4+3]/255d;
                var c=body[((row+1)*225+col)*4+3]/255d;
                if((a>=.5)!=(b>=.5))edges.Add(new((col+.5+(.5-a)/(b-a))*96/225,(row+.5)*96/225));
                if((a>=.5)!=(c>=.5))edges.Add(new((col+.5)*96/225,(row+.5+(.5-a)/(c-a))*96/225));
            }
            for(var row=0;row<H;row++)for(var col=0;col<W;col++){
                var x=-4+col*.25;var y=42+row*.25;
                var sx=SeatedContourSource(x,y,321);var squared=double.MaxValue;
                foreach(var p in edges){var dx=sx-p.X;var dy=y-p.Y;squared=Math.Min(squared,dx*dx+dy*dy);}
                values[row*W+col]=(float)(Math.Sqrt(squared)*(Alpha(x,y)>=.5?-1:1));
            }
        }
        internal double Distance(double x,double y){
            var u=(x+4)*4;var v=(y-42)*4;
            if(u<0||v<0||u>=W-1||v>=H-1)return 100;
            var ix=(int)u;var iy=(int)v;var fx=u-ix;var fy=v-iy;var i=iy*W+ix;
            return (values[i]*(1-fx)+values[i+1]*fx)*(1-fy)+(values[i+W]*(1-fx)+values[i+W+1]*fx)*fy;
        }
    }
}
